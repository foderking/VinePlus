

open System.IO
open System.Net.Http
open System.Text.Json
open System.Threading.Tasks
open Comicvine.Core
open Comicvine.Core.Parsers
open FSharp.Control
open StackExchange.Redis

type PollCreator<'T> =
  int -> int -> 'T seq
type PollUser<'T,'U> =
  'T -> 'U Task
type PollConsumer<'U> =
  IDatabase -> 'U -> Task

let threadKey = "vine:thread"
let postKey = "vine:pot"
let serverErrorKey = "vine:500"


let writeFile(db: IDatabase)(key: string)(file: string) = task {
  let entries =
    db.HashScan(key)
    |> Seq.map (fun x -> JsonSerializer.Deserialize(x.Value.ToString()))
  printfn "deserialized entries"
  use stream = File.Create(file)
  let! _ = JsonSerializer.SerializeAsync(stream, entries)
  printfn "written entry to file"
}
  
// threads
let threadEnumerator: PollCreator<int> =
  fun batchCount currentBatch ->
    seq{batchCount*currentBatch..batchCount*(currentBatch+1)-1}

let threadUser: PollUser<int, Thread seq> =
   fun page ->
     try
       Common.ParseSingle ThreadParser.ParseSingle page "/forums/"
     with
     | :? HttpRequestException as ex
      when ex.Message <> "Response status code does not indicate success: 404 (Not Found)." ->
       printfn "exception occured %A..." ex
       Common.ParseSingle ThreadParser.ParseSingle page "/forums/"
     | :? HttpRequestException as ex ->
       task{return Seq.empty}

let threadConsumer: PollConsumer<Thread seq> =
  let consume (db: IDatabase)(thread: Thread) =
    task {
      let serialized = JsonSerializer.Serialize(thread)
      let! _ = db.HashSetAsync(threadKey, thread.Id, serialized)
      ()
    } :> Task
  
  fun db item -> task {
    item
    |> Seq.map (consume db)
    |> Array.ofSeq
    |> Task.WaitAll
  }
  
let getPostEntries(db: IDatabase)=
    db.HashScan(threadKey)
    |> Seq.map (fun x -> JsonSerializer.Deserialize<Thread>(x.Value))
    |> Seq.collect (fun x -> 
      [1..x.LastPostPage] 
      |> Seq.map (fun y -> x.Thread.Link, y) 
      |> Seq.filter (fun (a,_) -> a <> "")
    )
// posts
let postEnumerator(db: IDatabase): PollCreator<string * int> =
  fun batchCount currentBatch ->
    getPostEntries db
    |> Seq.skip (batchCount*currentBatch)
    |> Seq.truncate batchCount

let postUser(db: IDatabase): PollUser<string * int, Post seq option> =
  fun (path, page) -> task {
    try
      let! res = Common.ParseSingle PostParser.ParseSingle page path
      return Some res
    with
    | :? HttpRequestException as ex
      when ex.Message = "Response status code does not indicate success: 404 (Not Found)." ->
        printfn "- deleted thread %s" path
        return None
    | :? HttpRequestException as ex
      when ex.Message = "Response status code does not indicate success: 500 (Internal Server Error)." ->
        printfn "- error thread %s" path
        let! _ = db.SetAddAsync(serverErrorKey, path) |> Async.ofTask
        return None
  }

let postConsumer: PollConsumer<Post seq option> =
  let consumePost(db: IDatabase)(post: Post) =
    task {
      let serialized = JsonSerializer.Serialize(post)
      let! _ = db.HashSetAsync(postKey, post.Id, serialized)
      ()
    }
    :> Task
    
  fun db posts -> task {
    match posts with
    | None -> ()
    | Some p ->
      p
      |> Seq.map (consumePost db)
      |> Array.ofSeq
      |> Task.WaitAll
  }
  
  
let rec doWork
  (db: IDatabase)(enumerator: PollCreator<'T>)(producer: PollUser<'T,'U>)
  (consumer: PollConsumer<'U>)(batchCount: int)(currentBatch: int)(lastBatch: int) =
  async {
    if currentBatch = lastBatch then
      return ()
    else
    try
      let! batched =
        enumerator batchCount currentBatch
        |> Seq.map producer
        |> Task.WhenAll
        |> Async.AwaitTask
        
      for item in batched do
        do! (consumer db item |> Async.ofUnitTask)
        
      printfn "finished batch %d" currentBatch
      return! doWork db enumerator producer consumer batchCount (currentBatch+1) lastBatch
      
    with
    | ex ->
      printfn "exception at batch %d occured: %A" currentBatch ex.Message
      do! Async.Sleep 1000
  }

let Seed() = task{
  let db = ConnectionMultiplexer.Connect("localhost").GetDatabase()
  let threadBatch = 6
  let postBatch = 4
  
  let! noThreads =
    Net.getNodeFromPage "/forums/" 1
    |> Task.map ThreadParser.ParseEnd
    |> Task.map (fun n -> n / threadBatch  - 1)
  printfn "[+] starting threads, count: %d" noThreads
  do! doWork db threadEnumerator threadUser threadConsumer threadBatch 0 noThreads
  printfn "[+] writing threads to json"
  do! writeFile db threadKey "threads.json"
  
  let noPosts =
    getPostEntries db
    |> Seq.length
    |> fun n -> n / postBatch + 1
  printfn "[+] starting posts, count: %d" noPosts
  do! doWork db (postEnumerator db) (postUser db) postConsumer postBatch 0 noPosts
  printfn "[+] writing posts to json"
  do! writeFile db postKey "posts.json"
}

[<EntryPoint>]
let main _ =
  Seed() |> Task.WaitAll
  0