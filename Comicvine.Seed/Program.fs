

open System.IO
open System.Linq
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
let postKey = "vine:post"
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
 
let writeThreadCsv(db: IDatabase)(file: string) = task {
  let json(x: Link) =
    sprintf "\"{\"\"Text\"\":\"\"%s\"\"}\""
  let entries =
    db.HashScan(threadKey)
    |> Seq.map (fun x -> JsonSerializer.Deserialize<Thread>(x.Value.ToString()))
    |> Seq.map (fun x ->
      $"{x.Id},{x.Thread}"
    )
  printfn "deserialized entries"
  do! File.WriteAllLinesAsync(file, [||])
  printfn "written entry to file"
} 
/// Gets the forum pages to be used by the current batch to parse the next set of threads
let threadEnumerator: PollCreator<int> =
  fun batchCount currentBatch ->
    seq{batchCount*currentBatch..batchCount*(currentBatch+1)-1}
    
/// Gets all the threads in a particular page
let threadUser: PollUser<int, Thread seq> =
  fun page ->
    Common.ParseSingle ThreadParser.ParseSingle page "/forums/"

/// Stores the next set of threads in redis
let threadConsumer: PollConsumer<Thread seq> =
  fun redis threads -> 
    threads
    |> Seq.map (fun thread ->
      task {
        let serialized = JsonSerializer.Serialize(thread)
        let! _ = redis.HashSetAsync(threadKey, thread.Id, serialized)
        ()
      } :> Task
    )
    |> Task.WhenAll
  
  
let getPostEntries(db: IDatabase)=
    db.HashScan(threadKey)
    |> Seq.map (fun x -> JsonSerializer.Deserialize<Thread>(x.Value))
    |> Seq.collect (fun x -> 
      [1..x.LastPostPage] 
      |> Seq.map (fun y -> x.Thread.Link, y) 
    )

let getFileEntries(name: string) =
  File.ReadLines(name).ToArray()
  |> Seq.map(fun x -> x.Split(',')) 
  |> Seq.map(fun x -> x[0],x[1] |> int)
  
  
// posts
let postEnumeratorRedis(db: IDatabase): PollCreator<string * int> =
  fun batchCount currentBatch ->
    getPostEntries db
    |> Seq.skip (batchCount*currentBatch)
    |> Seq.truncate batchCount
    
let postEnumerator(entries: (string * int) seq): PollCreator<string * int> =
  fun batchCount currentBatch ->
    entries
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
  
  
let rec Work
  (redis: IDatabase)(enumerator: PollCreator<'T>)(producer: PollUser<'T,'U>)
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
        do! (consumer redis item |> Async.ofUnitTask)
        
      printfn "finished batch %d" currentBatch
      return! Work redis enumerator producer consumer batchCount (currentBatch+1) lastBatch
      
    with
    | ex ->
      printfn "exception at batch %d occured: %A" currentBatch ex.Message
      do! Async.Sleep 1000
      return! Work redis enumerator producer consumer batchCount currentBatch lastBatch
  }

let Seed() = task{
  let redis = ConnectionMultiplexer.Connect("localhost").GetDatabase()
  let threadBatch = 6
  let postBatch = 6
  
  let! threadCount =
    Net.getNodeFromPage "/forums/" 1
    |> Task.map ThreadParser.ParseEnd
    |> Task.map (fun n -> n / threadBatch  - 1)
    
  printfn "[+] starting threads, count: %d" threadCount
  do! Work redis threadEnumerator threadUser threadConsumer threadBatch 0 threadCount
  
  printfn "[+] writing threads to json"
  do! writeFile redis threadKey "threads.json"
  
  let entries = (getPostEntries redis).ToArray()
  let noPosts =
    entries
    |> Seq.length
    |> fun n -> n / postBatch + 1
    
  printfn "[+] starting posts, count: %d" noPosts
  do! Work redis (postEnumerator entries) (postUser redis) postConsumer postBatch 0 noPosts
  
  printfn "[+] writing posts to json"
  do! writeFile redis postKey "posts.json"
}

[<EntryPoint>]
let main _ =
  Seed() |> Task.WaitAll
  0