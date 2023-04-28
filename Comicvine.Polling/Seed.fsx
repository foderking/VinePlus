#r "bin/Release/net6.0/Comicvine.Core.dll"
#r "bin/Release/net6.0/HtmlAgilityPack.dll"
#r "bin/Release/net6.0/HtmlAgilityPack.dll"
#r "bin/Release/net6.0/StackExchange.Redis.dll"

open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Comicvine.Core
open Comicvine.Core.Parsers
open Microsoft.FSharp.Control
open StackExchange.Redis

// module Poll =
//   let threadParser = Parsers.ThreadParser()
//   let postParser = Parsers.PostParser()
//   
//   let redis = ConnectionMultiplexer.Connect("localhost")
//   let db = redis.GetDatabase()
//
//   let consumeThread thread = task {
//     let! posts = postParser.ParseAll thread.Thread.Link 
//     let t = { thread with Posts = posts }
//     let serialized = JsonSerializer.Serialize(t)
//     let! _ = db.ListRightPushAsync("vine:thread", serialized)
//     printfn "written to redis"
//   }
//   
//   let getThreads (db: IDatabase) page = task {
//     let! stream =
//       Net.getStreamByPage page "/forums/"
//     let! _ =
//       Net.getRootNode stream
//       |> threadParser.ParseSingle
//       |> Seq.map consumeThread
//       |> Task.WhenAll
//     return ()
//   }
//   
//   
//   let poller() = task {
//     let! stream = Net.getStreamByPage 1 "/forums/"
//     let pages = 
//       Net.getRootNode stream
//       |> threadParser._parseEnd
//     let! _ =
//       seq{1..pages}
//       |> Seq.map (getThreads db)
//       |> Task.WhenAll
//       
//     ()
//       
//   }
//   
//   poller() |> Task.WaitAll;;
printfn "hello";;