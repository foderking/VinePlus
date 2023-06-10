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
  
  let getUrlWithCancellation (ct: CancellationToken) (query: Dictionary<string, string>) (path: string) = task {
      use querystring = new FormUrlEncodedContent(query)
      let! q = querystring.ReadAsStringAsync(ct)
      let client = new HttpClient()
      client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      return! client.GetStringAsync($"{path}?{q}")
  }

  /// creates an html node from an html string
  let createRootNode (html: string) =
    let rootNode = HtmlDocument()
    rootNode.LoadHtml(html)
    rootNode.DocumentNode
    
  /// convert a specific paginated page into an html node
  let getNodeFromPage(path: string)(page: int) = 
    getStringFromUrl (Dictionary[KeyValuePair("page", page |> string)]) path
    |> Task.map createRootNode
    
  /// converts the page at a path to an html node
  let getNodeFromPath(path: string) = 
    getStringFromUrl (Dictionary<string,string>()) path
    |> Task.map createRootNode