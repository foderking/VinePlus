open System.Globalization
open System.IO
open System.Net.Http
open System.Text
open System.Text.Json
open System.Threading.Tasks
open Comicvine.Core
open Comicvine.Core.Parsers
open FSharp.Control
open StackExchange.Redis

/// A function that returns the next batch of dependencies
type PollCreator<'T> =
  int -> int -> seq<'T>
  
/// A function that takes the dependency and makes a request to comicvine for the required data
type PollUser<'T,'U> =
  'T -> Task<'U>
  
/// A single page of a particular thread on comicvine
type ThreadPage = string * int

/// A function that stores required data in redis
type PollConsumer<'U> =
  IDatabase -> 'U -> Task


/// Key to the redis hash containing all threads parsed from comicvine so far
let threadKey      = "vine:thread"
/// Key to the redis hash containing all post parsed from comicvine so far
let postKey        = "vine:post"
/// Key to the redis hashset containing links to threads that gave server internal errors (just for research)
let serverErrorKey = "vine:500"


/// returns function that gets the next batch of forum pages
let threadEnumerator: PollCreator<int> =
  fun batchCount currentBatch ->
    seq{batchCount*currentBatch..batchCount*(currentBatch+1)-1}
    
/// returns function that parses all the threads in a particular page
let threadUser: PollUser<int, seq<Thread>> =
  fun page ->
    Common.ParseSingle ThreadParser.ParseSingle page "/forums/"

/// returns function that stores the threads in redis
let threadConsumer: PollConsumer<seq<Thread>> =
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
  
  
/// Return an array containing every page of every thread parsed from redis
let getPostEntries(db: IDatabase): ThreadPage[] =
    db.HashScan(threadKey)
    |> Seq.map (fun x ->
      JsonSerializer.Deserialize<Thread>(x.Value)
    )
    |> Seq.collect (fun x -> seq {
      for y in 1 .. x.LastPostPage do
        yield (x.Thread.Link, y)
    })
    |> Array.ofSeq

// let getFileEntries(name: string) =
//   File.ReadLines(name).ToArray()
//   |> Seq.map(fun x -> x.Split(',')) 
//   |> Seq.map(fun x -> x[0],x[1] |> int)
    
    
/// returns function that gets the next batch of thread pages to parse
let postEnumerator(entries: ThreadPage seq): PollCreator<ThreadPage> =
  fun batchCount currentBatch ->
    entries
    |> Seq.skip (batchCount*currentBatch)
    |> Seq.filter (fun (x,_) -> x <> "") // filter therads without links
    |> Seq.truncate batchCount

/// returns function that parses all the posts in a particular threads page
let postUser(db: IDatabase): PollUser<string * int, Post seq option> =
  fun (path, page) -> task {
    try
      let! res = Common.ParseSingle PostParser.ParseSingle page path
      return Some res
    with
    | :? HttpRequestException as ex
      when ex.Message = "Response status code does not indicate success: 404 (Not Found)." ->
        // if there is a 404, ignore it, thread was probably deleted
        printfn "- deleted thread %s" path
        return None
    | :? HttpRequestException as ex
      when ex.Message = "Response status code does not indicate success: 500 (Internal Server Error)." ->
        // if there was a 500 error, log it to redis and ignore
        printfn "- error thread %s" path
        let! _ = db.SetAddAsync(serverErrorKey, path) |> Async.ofTask
        return None
  }

/// returns a function that stores the next batch of posts in redis
let postConsumer: PollConsumer<Post seq option> =
  fun db parsedPosts -> task {
    return
      parsedPosts
      |> Option.map (fun allPosts ->
        allPosts
        |> Seq.map (fun post ->
          task {
            let serialized = JsonSerializer.Serialize(post)
            let! _ = db.HashSetAsync(postKey, post.Id, serialized)
            ()
          }
          :> Task
        )
        |> Array.ofSeq
        |> Task.WaitAll
      )
  }
 
let writeFile(db: IDatabase)(key: string)(file: string) = task {
  let entries =
    db.HashScan(key)
    |> Seq.map (fun x -> JsonSerializer.Deserialize(x.Value.ToString()))
  printfn "deserialized entries"
  use stream = File.Create(file)
  let! _ = JsonSerializer.SerializeAsync(stream, entries)
  printfn "written entry to file"
}
 
let escape(str: string) =
  let mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"))
  if mustQuote then
    let sb = StringBuilder();
    sb.Append("\"") |> ignore
    for nextChar in str do
      sb.Append(nextChar) |> ignore
      if (nextChar = '"') then
        sb.Append("\"") |> ignore
    sb.Append("\"") |> ignore
    sb.ToString()
  else
    str
    
/// Writes threads stored in redis to csv file
let writeThreadCsv(db: IDatabase)(file: string) = task {
  let json(x: Link) =
    sprintf "\"{\"\"Text\"\":\"\"%s\"\",\"\"Link\"\":\"\"%s\"\"}\"" (escape x.Text) (escape x.Link)
    
  let entries =
    db.HashScan(threadKey)
    |> Seq.map (fun x -> JsonSerializer.Deserialize<Thread>(x.Value.ToString()))
    |> Seq.map (fun x ->
      // .Id,.Thread,.Board,.IsPinned,.IsLocked,.IsDeleted,.Type,.LastPostNo,.LastPostPage,.Created,.TotalPosts,.TotalView,.Creator
      sprintf
        "%d,%s,%s,%b,%b,%b,%d,%d,%d,%s,%d,%d,%s"
        x.Id
        (json x.Thread)
        (json x.Board)
        x.IsPinned
        x.IsLocked
        x.IsDeleted
        (int x.Type)
        x.LastPostNo
        x.LastPostPage
        (x.Created.ToString("s", CultureInfo.InvariantCulture))
        x.TotalPosts
        x.TotalView
        (json x.Creator)
    )
  printfn "deserialized entries"
  do! File.WriteAllLinesAsync(file, entries)
  printfn "written entry to file"
}

/// Writes post stored in redis to csv file
let writePostCsv(db: IDatabase)(file: string) = task {
  let json(x: Link) =
    sprintf "\"{\"\"Text\"\":\"\"%s\"\",\"\"Link\"\":\"\"%s\"\"}\"" (escape x.Text) (escape x.Link)
    
  let entries =
    db.HashScan(postKey)
    |> Seq.map (fun x -> JsonSerializer.Deserialize<Post>(x.Value.ToString()))
    |> Seq.map (fun x ->
      // .Id,.IsComment,.IsDeleted,.IsModComment,.PostNo,.Creator,.IsEdited,.Created,.Content,.ThreadId
      sprintf
        "%s,%b,%b,%b,%d,%s,%b,%s,%s,%d"
        x.Id
        x.IsComment
        x.IsDeleted
        x.IsModComment
        x.PostNo
        (json x.Creator)
        x.IsEdited
        (x.Created.ToString("s", CultureInfo.InvariantCulture))
        (escape x.Content)
        x.ThreadId
    )
  printfn "deserialized entries"
  do! File.WriteAllLinesAsync(file, entries)
  printfn "written entry to file"
}
  
let rec Work
  (redis: IDatabase)(enumerator: PollCreator<'T>)(producer: PollUser<'T,'U>)(waitTime: int)
  (consumer: PollConsumer<'U>)(batchCount: int)(currentBatch: int)(lastBatch: int) =
  async {
    // ends polling when the last batch is reached
    if currentBatch = lastBatch then
      return ()
    else
    try
      // parse current batch
      let! batched =
        enumerator batchCount currentBatch
        |> Seq.map producer
        |> Task.WhenAll
        |> Async.AwaitTask
      // store current batch in redis
      for item in batched do
        do! (consumer redis item |> Async.ofUnitTask)
      // go to next batch
      printfn "finished batch %d" currentBatch
      do! Async.Sleep waitTime
      return! Work redis enumerator producer waitTime consumer batchCount (currentBatch+1) lastBatch
      
    with
    | ex ->
      // when an error occurs, log and wait for some time before retrying
      printfn "exception at batch %d occured: %A" currentBatch ex.Message
      do! Async.Sleep 1000
      return! Work redis enumerator producer waitTime consumer batchCount currentBatch lastBatch
  }

let Seed() = task{
  let redis = ConnectionMultiplexer.Connect("localhost").GetDatabase()
  let threadBatch = 6
  let postBatch   = 6
  let threadTime = 0 // for some reason, there is no reason to wait after parsing threads
  let postTime = 1000 // waits 1s after parsing a single batch of posts to reduce 429s
  // number of batches for threads
  let! threadCount =
    Net.getNodeFromPage "/forums/" 1
    |> Task.map ThreadParser.ParseEnd
    |> Task.map (fun n -> n / threadBatch  - 1)
  // parse all threads and store in redis
  printfn "[+] starting threads, count: %d" threadCount
  do! Work redis threadEnumerator threadUser threadTime threadConsumer threadBatch 0 threadCount
  // write parsed threads to csv file
  printfn "[+] writing threads to csv"
  // do! writeThreadCsv redis "threads_full.csv"
  do! writeFile redis threadKey "threads_full.json"
  // get number of batches for posts
  let entries = getPostEntries redis
  let noPosts =
    entries
    |> Seq.length
    |> fun n -> n / postBatch + 1
  // config for sets
  // You have to store in redis set by set,
  // because doing it all at once will consume an insane amount of memory..
  // ..I'm guessing at least 25 GB to parse the full 22 million post available
  let totalSets = 6
  let setSize = noPosts / totalSets
  printfn "[+] starting posts, count: %d" noPosts
  
  for set = 0 to (totalSets-1) do
    // get range of batches for next set
    let startS = setSize * set
    let endS   = setSize * (set+1) - 1
    // parse next set
    printfn "[+] set %d from batch %d to %d" set startS endS
    do! Work redis (postEnumerator entries) (postUser redis) postTime postConsumer postBatch startS (endS+1)
    // write files in set
    printfn "[+] writing posts_%d to json" set
    // do! writePostCsv redis $"posts_{set}.csv"
    do! writeFile redis postKey $"posts_{set}.json"
    // empties key (next set starts on an empty key to minimize memory used)
    let! _ = redis.KeyDeleteAsync(postKey)
    ()
  // last set of batches
  let lastStart = totalSets*setSize
  printfn "[+] set %d from batch %d to %d" totalSets lastStart noPosts
  do! Work redis (postEnumerator entries) (postUser redis) postTime postConsumer postBatch lastStart noPosts
  // write to csv file
  printfn "[+] writing posts_%d to json" totalSets
  // do! writePostCsv redis $"posts_{totalSets}.csv"
  do! writeFile redis postKey $"posts_{totalSets}.json"
}

[<EntryPoint>]
let main _ =
  Seed() |> Task.WaitAll
  0