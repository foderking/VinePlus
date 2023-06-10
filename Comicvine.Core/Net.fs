namespace Comicvine.Core

open System
open System.Collections.Generic
open System.Net.Http
open System.Threading
open FSharp.Control
open HtmlAgilityPack

module Net =
  let getStringFromUrl(query: Dictionary<string, string>)(path: string) = task {
      use querystring = new FormUrlEncodedContent(query)
      let! q = querystring.ReadAsStringAsync()
      let client = new HttpClient()
      client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      return! client.GetStringAsync($"{path}?{q}")
    }
  
  // let getReaderWithHandler(query: Dictionary<string, string>)(handler: HttpClientHandler)(path: string) = task {
  //     use querystring = new FormUrlEncodedContent(query)
  //     let! q = querystring.ReadAsStringAsync()
  //     let client = new HttpClient(handler, true)
  //     client.BaseAddress <- Uri("https://comicvine.gamespot.com")
  //     return! client.GetStringAsync($"{path}?{q}")
  //   }
    
      // use querystring = new FormUrlEncodedContent(query)
      // let! q = querystring.ReadAsStringAsync()
      // let client = new HttpClient()
      // client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      // return! client.GetStreamAsync($"{path}?{q}")
    // }
  let getUrlWithCancellation (ct: CancellationToken) (query: Dictionary<string, string>) (path: string) = task {
      use querystring = new FormUrlEncodedContent(query)
      let! q = querystring.ReadAsStringAsync(ct)
      let client = new HttpClient()
      client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      return! client.GetStringAsync($"{path}?{q}")
    // return! client.GetStreamAsync("/forums/battles-7/kotor-sith-lords-vs-cw-sith-lords-1631389/?page=2");
  }



  let createRootNode (html: string) =
    let rootNode = HtmlDocument()
    rootNode.LoadHtml(html)
    rootNode.DocumentNode
    
  let getNodeFromPage(path: string)(page: int) = 
    getStringFromUrl (Dictionary[KeyValuePair("page", page |> string)]) path
    |> Task.map createRootNode
    
  let getNodeFromPath(path: string) = 
    getStringFromUrl (Dictionary<string,string>()) path
    |> Task.map createRootNode

  // let map (mapper: 'T -> 'U) (task_: Task<'T>) = task {
  //     let! t = task_
  //     return mapper t
  // }
  // let mapSeq (mapper: 'T -> 'U) (s: Task<'T> seq)  =
  //     s
  //     |> Seq.map (map mapper)