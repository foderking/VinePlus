#r "bin/Release/net6.0/Comicvine.Core.dll"
#r "bin/Release/net6.0/HtmlAgilityPack.dll"
#r "bin/Release/net6.0/HtmlAgilityPack.dll"
#r "bin/Release/net6.0/StackExchange.Redis.dll"
#r "bin/Release/net6.0/FSharp.Control.TaskSeq.dll"

open System
open System.IO
open System.Net
open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Comicvine.Core
open Comicvine.Core.Parsers
open FSharp.Control
open Microsoft.FSharp.Control
open StackExchange.Redis

module Poll =
  let threadParser = Parsers.ThreadParser()
  let postParser = Parsers.PostParser()

  let redis = ConnectionMultiplexer.Connect("localhost")
  let db = redis.GetDatabase()

  let threadKey = "vine:thread"
  let postKey = "vine:post"
  let vineKey = "vine:full"

     
  let rec consumer (task: Async<unit>) (stop: bool) = async {
    if stop then
      ()
    else
      try
        do! task
        // printfn "consumer"
        return! (consumer task true)
      with
      | :? HttpRequestException as ex ->
        if ex.StatusCode.Value = HttpStatusCode.GatewayTimeout then
          printfn "timeout"
          do! Async.Sleep 500
          return! (consumer task false)
        elif ex.StatusCode.Value = HttpStatusCode.TooManyRequests then
          printfn "slowing down"
          do! Async.Sleep 1000
          return! (consumer task false)
        printfn "xxxx"
        return! (consumer task true)
      | _ ->
        return! (consumer task true)
  }
  
  let consumePost(db: IDatabase)(post: Post) =
    task {
      
      let serialized = JsonSerializer.Serialize(post)
      let! _ = db.HashSetAsync(postKey, post.Id, serialized)
      ()
    }
  
  let doPost(db: IDatabase)(page: int) =
    task {
       let! t = Common.ParseSingle threadParser page "/forums/"
       for thread in t do
         for p = 1 to thread.LastPostPage do
           let! post = Common.ParseSingle postParser p thread.Thread.Link
           for pp in post do
             do! consumePost db pp
         printfn "done thread %d" thread.Id
    } :> Task
    
  let doThread(db: IDatabase)(page: int) =
    task {
      let getThreads(page: int) =
       Common.ParseSingle threadParser page "/forums/"
     
      let consumeThread(db: IDatabase)(thread: Parsers.Thread) = task {
        let serialized = JsonSerializer.Serialize(thread)
        let! _ = db.HashSetAsync(threadKey, thread.Id, serialized)
        // printfn "written thread %d" thread.Id
        ()
      }
      
      let! t = getThreads page
      
      for each in t do
        let tsk = consumeThread db each |> Async.AwaitTask
        do! (consumer tsk false)
    }
    :> Task

   
  let pollThread(batch: int)(doPoll: IDatabase -> int -> Task)(initialBatch: int) = task {
     let! stream = Net.getStreamByPage 1 "/forums/"
     let pages = 
       Net.getRootNode stream
       |> threadParser.ParseEnd
       
     for p = initialBatch to (pages/batch) do
         do!
           seq{batch*p..batch*(p+1)-1}
           |> Seq.map (doPoll db)
           |> Task.WhenAll
         printfn "finished batch %d" p
  }

       
  
  let rec iterate batchCount currentBatch lastBatch doPoll = async {
    if currentBatch > lastBatch then
      ()
    else
         try
           do!
             seq{batchCount*currentBatch..batchCount*(currentBatch+1)-1}
             |> Seq.map (doPoll db)
             |> Task.WhenAll
             |> Async.AwaitTask
           printfn "finished batch %d" currentBatch
           return! iterate batchCount (currentBatch+1) lastBatch doPoll
         with
         | ex ->
           printfn "exception at batch %d occured: %A" currentBatch ex.Message
           do! Async.Sleep 1000
           return! iterate batchCount (currentBatch) lastBatch doPoll
           
      
  }
  
  let pollPages(batch: int)(initialBatch: int)(doPoll: IDatabase -> int -> Task) = task {
     let! stream = Net.getStreamByPage 1 "/forums/"
     let pages = 
       Net.getRootNode stream
       |> threadParser.ParseEnd
     do! iterate batch initialBatch (pages/batch) doPoll
  }
  
  let writeThreads(db: IDatabase) = task {
    printfn "got threads"
    let t =
      db.HashScan(threadKey)
      |> Seq.map (fun x -> JsonSerializer.Deserialize<Thread>(x.Value.ToString()))
    printfn "serialized threads"
    let stream = File.Create("threads.json")
    do! JsonSerializer.SerializeAsync<Parsers.Thread seq>(stream, t)
    do! stream.DisposeAsync()
  }
  // pollThread 6 doThread 958 |> Task.WaitAll;;
  // pollThread 6 doThread 0 |> Task.WaitAll;;
  
  // pollTasks 6 1074 doThread |> Task.WaitAll;;
  pollPages 6 0 doPost |> Task.WaitAll;;
  // writeThreads db |> Task.WaitAll;;