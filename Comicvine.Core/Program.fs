namespace Comicvine.Core

open System
open System.Collections.Generic
open System.IO
open System.Net.Http
open HtmlAgilityPack

    module Main = 
        let getStreamWithParam (query: Dictionary<string,string>) (path: string) = task {
            use querystring = new FormUrlEncodedContent(query)
            let q = querystring.ReadAsStringAsync()
            
            let client = new HttpClient()
            client.BaseAddress <- Uri("https://comicvine.gamespot.com")
            return! client.GetStreamAsync($"{path}?{q}");
        }
        
        let getStream = getStreamWithParam (Dictionary<string, string>())
        
        
        let getRootNode(htmlStream: Stream) =
            let rootNode = HtmlDocument()
            rootNode.Load(htmlStream)
            rootNode.DocumentNode
