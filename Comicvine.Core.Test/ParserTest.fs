module Tests.Parser

open System
open System.Net.Http
open System.Text.RegularExpressions
open Comicvine.Core
open Xunit

let convertNameRegex (name: string) =
    let reg (v: string) (pattern: string) (str: string) =
        Regex.Replace(str, pattern, v)
    name.ToLower()
    |> reg "" "&#\d+;"
    |> reg "" "&[a-z]+;"
    |> reg ""  "[^- 0-9a-zA-Z]+"
    |> reg "-" " +"
    |> reg "-" "-+"
 

let makeRequest (path: string) =
    task {
        let client = new HttpClient()
        client.BaseAddress <- Uri("https://comicvine.gamespot.com")
        return! client.GetStringAsync(path)
    }
    
let getNodeFromPath path = task {
    let! stream = Net.getStream path
    return Net.getRootNode stream
}
let usersWithBlog: obj[] list =
    [
        [|"owie"|]
        [|"death4bunnies"|]
        [|"sc"|]
        [|"cbishop"|]
    ]

module BlogEndParer =
    [<Fact>]
    let ``number of pages returned should not be more than amount known``() = task {
        let! node = getNodeFromPath $"/profile/owie/blog"
        let no = Parsers.parseBlogEnd node
        Assert.True(no >= 4)
        let! node = getNodeFromPath $"/profile/death4bunnies/blog"
        let no = Parsers.parseBlogEnd node
        Assert.True(no >= 2)
        let! node = getNodeFromPath $"/profile/sc/blog"
        let no = Parsers.parseBlogEnd node
        Assert.True(no >= 2)
        let! node = getNodeFromPath $"/profile/cbishop/blog"
        let no = Parsers.parseBlogEnd node
        Assert.True(no >= 88)
    }
    
module BlogParser =
    [<Theory>]
    [<MemberData(nameof(usersWithBlog))>]
    let ``valid blog should not be empty sequence``(username: string) = task {
        let! node = getNodeFromPath $"/profile/{username}/blog"
        let j = Parsers.parseBlog node
        Assert.NotEmpty j
    }
    
    [<Theory>]
    [<MemberData(nameof(usersWithBlog))>]
    let ``created dates should be unique``(username: string) = task {
        let! node = getNodeFromPath $"/profile/{username}/blog"
        let j = Parsers.parseBlog node
        let k = j |> Seq.distinctBy (fun x -> x.Created)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
    }
    
    [<Theory>]
    [<MemberData(nameof(usersWithBlog))>]
    let ``blog id should be unique``(username: string) = task {
        let! node = getNodeFromPath $"/profile/{username}/blog"
        let j = Parsers.parseBlog node
        let k = j |> Seq.distinctBy (fun x -> x.Id)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
    }
    
    [<Theory>]
    [<MemberData(nameof(usersWithBlog))>]
    let ``threadids that are not null should be unique``(username: string) = task {
        let! node = getNodeFromPath $"/profile/{username}/blog"
        let j = Parsers.parseBlog node |> Seq.filter (fun x -> x.ThreadId.HasValue)
        let k = j |> Seq.distinctBy (fun x -> x.ThreadId.Value)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
    }
    
       
        
        // Regex.Replace(name.ToLower(), , "")
          
        
    [<Theory>]
    [<MemberData(nameof(usersWithBlog))>]
    let ``"Blog.Text" should satisfy invariant with "Blog.Link"``(username: string) = task {
        let! node = getNodeFromPath $"/profile/{username}/blog"
        let j = Parsers.parseBlog node
        j
        |> Seq.iter (
            fun x ->
                let name, link = x.Blog.Text, x.Blog.Link
                let pref, full = link |> (fun x -> x.Split("/").[4]), name |> convertNameRegex
                Assert.Equal(full[..(pref.Length-1)], pref)
        )
    }   
        
module ThreadParser =
    [<Fact>]
    let ``should parse the correct number of threads``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        Assert.NotEmpty j
        Assert.Equal( 50, j |> Seq.length)
    }
    
    [<Fact>]
    let ``number of views should not be the same``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        Assert.True( j |> Seq.distinctBy (fun x -> x.TotalView) |> Seq.length > 1)
    }
     
    [<Fact>]
    let ``number of posts should not be the same``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        Assert.True( j |> Seq.distinctBy (fun x -> x.TotalPosts) |> Seq.length > 1)
    }
    
    [<Fact>]
    let ``"LastPostNo" should not be the same``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        Assert.True( j |> Seq.distinctBy (fun x -> x.LastPostNo) |> Seq.length > 1)
    }
    
    [<Fact>]
    let ``"LastPostPage" should not be the same``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        Assert.True( j |> Seq.distinctBy (fun x -> x.LastPostPage) |> Seq.length > 1)
    }
    
    [<Fact>]
    let ``"Created" should not be the same``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        Assert.True( j |> Seq.distinctBy (fun x -> x.Created) |> Seq.length > 1)
    }   
     
    [<Fact>]
    let ``"Creator.Link" should be valid``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        j
        |> Seq.iter (
            fun x ->
                let link = x.Creator.Link
                let splitLink = link.Split("/")
                Assert.Equal(4, splitLink.Length)
                Assert.Equal("", splitLink[0])
                Assert.Equal("", splitLink[3])
                Assert.Equal("profile", splitLink[1])
        )
    }
    
    [<Fact>]
    let ``"Creator.Text" should satisfy invariant with "Creator.Link"``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        j
        |> Seq.iter (
            fun x ->
                let name, link = x.Creator.Text, x.Creator.Link
                let convertNameFormat (name: string) =
                    name
                        .ToLower()
                        .Replace(".", "_")
                        .Replace(" ", "_")
                        
                Assert.Equal(
                    link.Split("/").[2],
                    name |> convertNameFormat
                )
        )
    }
    
    [<Fact>]
    let ``thread ids should be unique``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        let k = j |> Seq.distinctBy (fun x -> x.Id )
            
        Assert.Equal(j |> Seq.length, k |> Seq.length)
    }
    
    [<Fact>]
    let ``should be at least two distinct blog type on frontpage``() = task {
        let! node = getNodeFromPath "/forums/"
        let j =
            Parsers.parseThread node
            |> Seq.distinctBy (fun x -> x.Type )
        Assert.True(
            j
            |> Seq.length
            |> (<) 1
        )
    }
    [<Fact>]
    let ``should be at least 5 (picked randomly) pinned thread on frontpage``() = task {
        let! node = getNodeFromPath "/forums/"
        let j =
            Parsers.parseThread node
            |> Seq.filter (fun x -> x.IsPinned )
        Assert.True(
            j
            |> Seq.length
            |> (<) 5
        )
    }
    
    [<Fact>]
    let ``should be at least one locked thread on frontpage``() = task {
        let! node = getNodeFromPath "/forums/"
        Assert.NotEmpty(
            Parsers.parseThread node
            |> Seq.filter (fun x -> x.IsLocked )
        )
    }
    
    [<Fact>]
    let ``should be at least one thread that is both locked and pinned on frontpage``() = task {
        let! node = getNodeFromPath "/forums/"
        Assert.NotEmpty(
            Parsers.parseThread node
            |> Seq.filter (fun x -> x.IsLocked && x.IsPinned)
        )
    }
    
    [<Fact>]
    let ``question/answered thread name always starts with "Q: "``() = task {
        let! node = getNodeFromPath "/forums/"
        Parsers.parseThread node
        |> Seq.iter(
            fun x ->
                if x.Thread.Text.StartsWith("Q: ") then
                    Assert.True(x.Type = Parsers.ThreadType.Question || x.Type = Parsers.ThreadType.Answered)
        )
    }
    let extractBoard (link: string) =
        let s = link.Split("forums/").[1]
        if  s.Length > 1 then
            s.Split("/")[0]
        else
            link.Split("/").[1]
        
        
    [<Fact>]
    let ``"Board.Link" should be valid``() = task {
        let! node = getNodeFromPath "/forums/"
        Parsers.parseThread node
        |> Seq.iter (
            fun x ->
                let link = x.Board.Link
                Assert.Null(
                    Record.Exception( fun () -> 
                        link
                        |> extractBoard
                        |> ignore
                    )
                )
        )
    }
    
    [<Fact>]
    let ``"Board.Text" should satisfy invariant with "Board.Link"``() = task {
        let! node = getNodeFromPath "/forums/"
        let j = Parsers.parseThread node
        j
        |> Seq.iter (
            fun x ->
                let name, link = x.Board.Text, x.Board.Link
                // let convertNameFormat (name: string) =
                //     name
                //         .ToLower()
                //         .Replace(".", "")
                //         .Replace("&amp; ", "")
                //         .Replace("/", "")
                //         .Replace(" ", "-")
                        
                let fullName, prefix = link |> extractBoard, name |> convertNameRegex
                // Assert.True(fullName.StartsWith(prefix))
                Assert.Equal(fullName[..(prefix.Length-1)], prefix)
        )
    }   
    
    [<Fact>]
    let ``valid thread should not give empty sequence``() = task {
        let! node = getNodeFromPath "/forums/"
        Assert.NotEmpty(Parsers.parseThread node)
    }
    
    [<Fact>]
    let ``"Thread.Link" should be valid``() = task {
        let! node = getNodeFromPath "/forums/"
        Parsers.parseThread node
        |> Seq.iter (
            fun x ->
                Assert.True(x.Thread.Link.StartsWith("/"))
                Assert.True(x.Thread.Link.EndsWith("/"))
        )
    }
    
    [<Fact>]
    let ``"Thread.Text" should be valid``() = task {
        let! node = getNodeFromPath "/forums/"
        Parsers.parseThread node
        |> Seq.iter (
            fun x ->
                Assert.True(x.Thread.Text.Length > 1)
        )
    }
    
 module ThreadEndParser =
    [<Fact>]
    let ``parsing no of threads works correctly``() = task {
        let! node = getNodeFromPath "/forums/"
        let no = Parsers.parsePageEnd node
        Assert.True(no > 16640)
    }
   
module PostEndParser =
    [<Fact>]
    let ``parsing no of pages in thread works correctly``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/site-rules-and-faqs-669033/"
        Assert.Equal(Parsers.parsePageEnd node, 1)
        let! node = getNodeFromPath "/forums/bug-reporting-2/important-new-bug-reporting-procedure-2052355/"
        Assert.Equal(Parsers.parsePageEnd node, 6)
        let! node = getNodeFromPath "/forums/battles-7/mcu-thor-iw-vs-lord-boros-opm-1946556/"
        Assert.Equal(Parsers.parsePageEnd node, 7)
    }

module PostParser =
    [<Fact>]
    let ``valid comments should not be empty``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = Parsers.parsePosts 2300312 node
        Assert.NotEmpty j
    }
    
    [<Fact>]
    let ``should correctly show edited posts exists``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j =
            Parsers.parsePosts 2300312 node
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
        let k =
            j
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
        let k =
            j
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
        let k =
            j
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
        let k =
            j
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
        let j =
            Parsers.parsePosts 2300312 node
            |> Seq.map (fun e ->
                match e with
                | Parsers.ThreadPost.OP _ -> None
                | Parsers.ThreadPost.Comment(n) -> Some(n.Id)
            )
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
        let k =
            j
            |> Seq.distinct
                
        Assert.True((j |> Seq.length) = (k |> Seq.length ))
    }
    
    [<Fact>]
    let ``"PostNo" should be distinct``() = task {
        let! node = getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j =
            Parsers.parsePosts 2300312 node
            |> Seq.map (
                fun e ->
                    match e with
                    | Parsers.ThreadPost.OP _ -> None
                    | Parsers.ThreadPost.Comment(n) -> Some(n.PostNo)
            )
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
        let k =
            j
            |> Seq.distinct
                
        Assert.True((j |> Seq.length) = (k |> Seq.length ))
    }
    
module PostFullParser =
    [<Fact>]
    let ``parsing full posts return the correct number of comments``() = task {
        let! j = Parsers.ParsePostsFull "/forums/battles-7/tai-lung-vs-alex-madagascar-2281381/"
        Assert.Equal(12, j |> Seq.length)
        let! j = Parsers.ParsePostsFull "/forums/gen-discussion-1/your-magical-racer-and-of-course-will-be-talking-1776966/"
        Assert.Equal(1, j |> Seq.length)
        let! j = Parsers.ParsePostsFull "/forums/battles-7/thanos-vs-superman-3155/"
        Assert.Equal(1759, j |> Seq.length)
        let! j = Parsers.ParsePostsFull "/forums/battles-7/cyttorak-vs-living-tribunal-528457/"
        Assert.Equal(54, j |> Seq.length)
    }
