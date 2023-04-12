module Tests.Parser

open System
open System.Net.Http
open System.Threading.Tasks
open Comicvine.Core
open Xunit

let makeRequest (path: string) =
    task {
        let client = new HttpClient()
        client.BaseAddress <- Uri("https://comicvine.gamespot.com")
        return! client.GetStringAsync(path)
    }
    
let tryTask f x =
    fun _ -> f x |> printfn "%A"
   
let getNodeFromPath path = task {
    let! stream = Main.getStream path
    return Main.getRootNode stream
}

let testShouldNotThrow parser path = task {
    let! node = getNodeFromPath path
    // let z = parser node
    let ex = Record.Exception(tryTask parser node)
    Assert.Null(ex)
}

module BlogParser =
    [<Fact>]
    let ``parsing a valid blog should not fail`` () =
        testShouldNotThrow Parsers.ParseBlog "/profile/saboyaba/blog"
        
    [<Fact>]
    let ``valid blog should not be empty sequence``() = task {
        let! node = getNodeFromPath "/profile/saboyaba/blog"
        Assert.NotEmpty (Parsers.ParseBlog node)
    }
        

module CommentParser =
    [<Fact>]
    let ``parsing valid thread with comments should not fail``() =
        testShouldNotThrow (Parsers.ParseComments 2300312) "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"

    [<Fact>]
    let ``valid comments should not be empty``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.ParseComments 2300312 node
        Assert.NotEmpty (j)
    }