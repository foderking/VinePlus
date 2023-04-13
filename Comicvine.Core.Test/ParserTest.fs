module Tests.Parser

open System
open System.Net.Http
open Comicvine.Core
open Xunit

let makeRequest (path: string) =
    task {
        let client = new HttpClient()
        client.BaseAddress <- Uri("https://comicvine.gamespot.com")
        return! client.GetStringAsync(path)
    }
    
// let tryTask f x =
//     fun _ -> f x |> printfn "%A"
   
let getNodeFromPath path = task {
    let! stream = Net.getStream path
    return Net.getRootNode stream
}

// let inline testShouldNotThrow parser path = task {
//     let! node = getNodeFromPath path
//     
//     let ex = Record.Exception(fun () ->
//         parser node
//         |> ignore
//     )
//     Assert.Null(ex)
// }
//
// module Helpers =
//     [<Fact>]
//     let ``function raising exception should fail "testShouldNotThrow" helper`` () =
//         let f _ =
//             raise (Exception("bla"))
//         // let ex = Record.Exception(
//         //     testShouldNotThrow f ""
//         // )
//         Assert.ThrowsAsync<Xunit.Sdk.NullException>(
//             fun () ->
//                 testShouldNotThrow f ""
//         )
//     

module BlogParser =
    // [<Fact>]
    // let ``parsing a valid blog should not fail`` () =
    //     testShouldNotThrow Parsers.ParseBlog "/profile/saboyaba/blog"
        
    [<Fact>]
    let ``valid blog should not be empty sequence``() = task {
        let! node = getNodeFromPath "/profile/saboyaba/blog"
        Assert.NotEmpty (Parsers.ParseBlog node)
    }
        

module PostParser =
    [<Fact>]
    let ``parsing no of pages in thread works correctly``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/site-rules-and-faqs-669033/"
        Assert.Equal(Parsers.parsePostEnd node, 1)
        let! node = getNodeFromPath "/forums/bug-reporting-2/important-new-bug-reporting-procedure-2052355/"
        Assert.Equal(Parsers.parsePostEnd node, 6)
        let! node = getNodeFromPath "/forums/battles-7/mcu-thor-iw-vs-lord-boros-opm-1946556/"
        Assert.Equal(Parsers.parsePostEnd node, 7)
    }

    [<Fact>]
    let ``valid comments should not be empty``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
        Assert.NotEmpty j
    }
    
    [<Fact>]
    let ``should correctly show edited posts exists``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
                |> Seq.map (fun e ->
                    match e with
                    | Parsers.ThreadPost.OP(n) -> n.IsEdited
                    | Parsers.ThreadPost.Comment(n) -> n.IsEdited
                )
        Assert.False(j |> Seq.isEmpty )
    }
    
    [<Fact>]
    let ``date created for each post should be unique``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
        let k = j
                |> Seq.map (fun e ->
                    match e with
                    | Parsers.ThreadPost.OP(n) -> n.Created
                    | Parsers.ThreadPost.Comment(n) -> n.Created
                )
                |> Seq.distinct
                
        Assert.True((j |> Seq.length) = (k |> Seq.length ))
    }   
    
    [<Fact>]
    let ``"ThreadId" value should be correct and the same``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
        let k = j
                |> Seq.map (fun e ->
                    match e with
                    | Parsers.ThreadPost.OP(n) -> n.ThreadId
                    | Parsers.ThreadPost.Comment(n) -> n.ThreadId
                )
                |> Seq.filter (fun x -> x = 2300312)
                
        Assert.True((j |> Seq.length) = (k |> Seq.length ))
    }   
    
    [<Fact>]
    let ``content of each post should not be empty``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
        let k = j
                |> Seq.map (fun e ->
                    match e with
                    | Parsers.ThreadPost.OP(n) -> n.Content
                    | Parsers.ThreadPost.Comment(n) -> n.Content
                )
                |> Seq.filter (fun x -> x.Length > 0)
                
        Assert.True((j |> Seq.length) = (k |> Seq.length ))
    }
    
    [<Fact>]
    let ``creator details should not be empty``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
        let k = j
                |> Seq.map (fun e ->
                    match e with
                    | Parsers.ThreadPost.OP(n) -> n.Creator
                    | Parsers.ThreadPost.Comment(n) -> n.Creator
                )
                |> Seq.filter (fun x -> x.Link.Length > 0 && x.Text.Length > 0)
                
        Assert.True((j |> Seq.length) = (k |> Seq.length ))
    }
    
    [<Fact>]
    let ``comment ids should be distinct``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
                |> Seq.map (fun e ->
                    match e with
                    | Parsers.ThreadPost.OP _ -> None
                    | Parsers.ThreadPost.Comment(n) -> Some(n.Id)
                )
                |> Seq.filter Option.isSome
                |> Seq.map Option.get
        let k = j
                |> Seq.distinct
                
        Assert.True((j |> Seq.length) = (k |> Seq.length ))
    }
    
    [<Fact>]
    let ``"PostNo" should be distinct``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
                |> Seq.map (fun e ->
                    match e with
                    | Parsers.ThreadPost.OP _ -> None
                    | Parsers.ThreadPost.Comment(n) -> Some(n.PostNo)
                )
                |> Seq.filter Option.isSome
                |> Seq.map Option.get
        let k = j
                |> Seq.distinct
                
        Assert.True((j |> Seq.length) = (k |> Seq.length ))
    }
    