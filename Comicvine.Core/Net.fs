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
  type readerFunction<'T> = string -> HttpClient -> 'T
  module Reader =
    let stream(path: string)(client: HttpClient) =
      client.GetStreamAsync(path)
    let string(path: string)(client: HttpClient) =
      client.GetStringAsync(path)
    
  let getReaderWithParamGeneric (returnFunc: 'T readerFunction)(query: Dictionary<string, string>) (path: string) = task {
      use querystring = new FormUrlEncodedContent(query)
      let! q = querystring.ReadAsStringAsync()
      let client = new HttpClient()
      client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      return!
        client
        |> returnFunc $"{path}?{q}"
    }
    // return! client.GetStreamAsync("/forums/battles-7/kotor-sith-lords-vs-cw-sith-lords-1631389/?page=2");
  let getStreamWithParam (**(query: Dictionary<string, string>) (path: string)**) =
    getReaderWithParamGeneric Reader.stream
      // use querystring = new FormUrlEncodedContent(query)
      // let! q = querystring.ReadAsStringAsync()
      // let client = new HttpClient()
      // client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      // return! client.GetStreamAsync($"{path}?{q}")
    // return! client.GetStreamAsync("/forums/battles-7/kotor-sith-lords-vs-cw-sith-lords-1631389/?page=2");
    // }
  let getStringWithParam(query: Dictionary<string,string>) =
    getReaderWithParamGeneric Reader.string query

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
    
  let getStringByPage (page: int) path =
    getStreamWithParam (Dictionary[KeyValuePair("page", page |> string)]) path

  let getStreamByPageCt (ct: CancellationToken) (page: int) path =
    getStreamWithParamCt ct (Dictionary[KeyValuePair("page", page |> string)]) path

  let getRootNode (htmlStream: Stream) =
    let rootNode = HtmlDocument()
    rootNode.Load(htmlStream)
    rootNode.DocumentNode
    
  let getRoot (html: string) =
    let rootNode = HtmlDocument()
    rootNode.LoadHtml(html)
    rootNode.DocumentNode
    
  let getNodeFromPage(path: string)(page: int) = 
    getReaderWithParamGeneric Reader.string (Dictionary[KeyValuePair("page", page |> string)]) path
    |> Task.map getRoot
    
  let getNode(path: string) = 
    getReaderWithParamGeneric Reader.string (Dictionary<string,string>()) path
    |> Task.map getRoot

  let map (mapper: 'T -> 'U) (task_: Task<'T>) = task {
      let! t = task_
      return mapper t
  }
  let mapSeq (mapper: 'T -> 'U) (s: Task<'T> seq)  =
      s
      |> Seq.map (map mapper)