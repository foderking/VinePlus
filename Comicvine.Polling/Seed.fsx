#r "bin/Release/net6.0/Comicvine.Core.dll"
#r "bin/Release/net6.0/HtmlAgilityPack.dll"
#r "bin/Release/net6.0/HtmlAgilityPack.dll"
#r "bin/Release/net6.0/StackExchange.Redis.dll"

open System.Threading.Tasks
open Comicvine.Core
open Microsoft.FSharp.Control
open StackExchange.Redis

module Poll =
  let threadParser = Parsers.ThreadParser()
  let postParser = Parsers.PostParser()
  
  let redis = ConnectionMultiplexer.Connect("localhost")
  let db = redis.GetDatabase
  
  let getThreads page = async {
    let! stream = Net.getStreamByPage page "/forums/" |> Async.AwaitTask
    return
      Net.getRootNode stream
      |> threadParser.ParseSingle
  }
  
  let consumeThread (db: IDatabase) thread = async {
    db.Ha
  }
  
  let getPosts path = async {
    return! postParser.ParseAll(path)|> Async.AwaitTask
  }

  
  let poller = async {
    let! stream = Net.getStreamByPage 1 "/forums/" |> Async.AwaitTask
    let pages = 
      Net.getRootNode stream
      |> threadParser._parseEnd
    let! batchedThreads =
      seq{1..pages}
      |> Seq.map getThreads
      |> Async.Parallel
    ()
      
  }