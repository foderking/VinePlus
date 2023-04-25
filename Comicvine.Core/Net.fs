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
        let getStreamWithParam (query: Dictionary<string,string>) (path: string) = task {
            use querystring = new FormUrlEncodedContent(query)
            let! q = querystring.ReadAsStringAsync()
            // printfn "%A" q.Result
            
            let client = new HttpClient()
            client.BaseAddress <- Uri("https://comicvine.gamespot.com")
            return! client.GetStreamAsync($"{path}?{q}");
            // return! client.GetStreamAsync("/forums/battles-7/kotor-sith-lords-vs-cw-sith-lords-1631389/?page=2");
        }
            
        let getStreamWithParamCt (ct: CancellationToken) (query: Dictionary<string,string>) (path: string) = task {
            use querystring = new FormUrlEncodedContent(query)
            let! q = querystring.ReadAsStringAsync(ct)
            // printfn "%A" q.Result
            
            let client = new HttpClient()
            client.BaseAddress <- Uri("https://comicvine.gamespot.com")
            return! client.GetStreamAsync($"{path}?{q}");
            // return! client.GetStreamAsync("/forums/battles-7/kotor-sith-lords-vs-cw-sith-lords-1631389/?page=2");
        }
        
        let getStream path= getStreamWithParam (Dictionary<string, string>()) path
        
        let getStreamByPage (page: int) path =
            getStreamWithParam (Dictionary[KeyValuePair("page", page |> string)]) path 
        
        let getStreamByPageCt (ct: CancellationToken) (page: int) path =
            getStreamWithParamCt ct (Dictionary[KeyValuePair("page", page |> string)]) path 
        let getRootNode(htmlStream: Stream) =
            let rootNode = HtmlDocument()
            rootNode.Load(htmlStream)
            rootNode.DocumentNode
            
        // let unwrapTaskSeq t =
        //     let _folder state curr =
        //       Seq.append state [curr]
        //     t
        //     |> TaskSeq.map (fun x -> x |> TaskSeq.ofSeq)
        //     |> TaskSeq.concat 
        //     |> TaskSeq.fold _folder (Seq.ofList [])
      
        let map (mapper: 'T -> 'U) (task_: Task<'T>) = task {
            let! t = task_
            return mapper t
        }
 
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
        
