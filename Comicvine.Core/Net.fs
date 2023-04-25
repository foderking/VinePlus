namespace Comicvine.Core

open System
open System.Collections.Generic
open System.IO
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open FSharp.Control
open HtmlAgilityPack

module Net =
  let getStreamWithParam (query: Dictionary<string, string>) (path: string) = task {
      use querystring = new FormUrlEncodedContent(query)
      let! q = querystring.ReadAsStringAsync()
      let client = new HttpClient()
      client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      return! client.GetStreamAsync($"{path}?{q}")
    // return! client.GetStreamAsync("/forums/battles-7/kotor-sith-lords-vs-cw-sith-lords-1631389/?page=2");
    }

  let getStreamWithParamCt (ct: CancellationToken) (query: Dictionary<string, string>) (path: string) = task {
      use querystring = new FormUrlEncodedContent(query)
      let! q = querystring.ReadAsStringAsync(ct)
      let client = new HttpClient()
      client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      return! client.GetStreamAsync($"{path}?{q}")
    // return! client.GetStreamAsync("/forums/battles-7/kotor-sith-lords-vs-cw-sith-lords-1631389/?page=2");
  }

  let getStream path =
    getStreamWithParam (Dictionary<string, string>()) path

  let getStreamByPage (page: int) path =
    getStreamWithParam (Dictionary[KeyValuePair("page", page |> string)]) path

  let getStreamByPageCt (ct: CancellationToken) (page: int) path =
    getStreamWithParamCt ct (Dictionary[KeyValuePair("page", page |> string)]) path

  let getRootNode (htmlStream: Stream) =
    let rootNode = HtmlDocument()
    rootNode.Load(htmlStream)
    rootNode.DocumentNode

  let map (mapper: 'T -> 'U) (task_: Task<'T>) = task {
      let! t = task_
      return mapper t
  }