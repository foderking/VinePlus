namespace Comicvine.Core

open System
open System.Collections.Generic
open System.IO
open System.Net.Http
open FSharp.Control
open HtmlAgilityPack

    module Net = 
        let getStreamWithParam (query: Dictionary<string,string>) (path: string) = task {
            use querystring = new FormUrlEncodedContent(query)
            let! q = querystring.ReadAsStringAsync()
            // printfn "%A" q.Result
            
            let client = new HttpClient()
            client.BaseAddress <- Uri("https://comicvine.gamespot.com")
            return! client.GetStreamAsync($"{path}?{q}");
            // return! client.GetStreamAsync("/forums/battles-7/kotor-sith-lords-vs-cw-sith-lords-1631389/?page=2");
        }
        
        let getStream = getStreamWithParam (Dictionary<string, string>())
        
        let getStreamByPage (page: int) path =
            getStreamWithParam (Dictionary[KeyValuePair("page", page |> string)]) path 
        
        let getRootNode(htmlStream: Stream) =
            let rootNode = HtmlDocument()
            rootNode.Load(htmlStream)
            rootNode.DocumentNode

        // let parseAllGeneric parseStream parserData parseEnd (path: string) (page: int) = taskSeq {
        //     let! stream = parseStream page path 
        //     let node = stream |> getRootNode
        //     let last = parseEnd node
        //     yield parserData node
        //     
        //     for page in 2..last do
        //         let! stream = parseStream page path 
        //         yield stream
        //             |> getRootNode
        //             |> parserData
        // }
        
