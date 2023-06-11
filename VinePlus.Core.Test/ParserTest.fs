module Tests.Parser

open System
open System.Text.RegularExpressions
open Comicvine.Core
open Comicvine.Core.Parsers
open Xunit




module BlogTests = 

  let usersWithBlog: obj [] list = [
      [| "sc" |]
      [| "owie" |]
      [| "cbishop" |]
      [| "darthjhawk" |]
      [| "static_shock" |]
      [| "death4bunnies" |]
  ]
  let usersWithoutBlog: obj [] list = [
      [| "abc" |]
      [| "temsbumbum" |]
    ]


  module BlogFullParser =
    let withBlog = usersWithBlog
    let withoutBlog = usersWithoutBlog
    
    [<Theory>]
    [<MemberData(nameof withBlog)>]
    let ``users that have blogs should return an amount of blogs`` (username: string) =
      task {
        let! j = BlogParser.ParseMultiple $"/profile/{username}/blog"
        Assert.NotEmpty j
      }

    [<Theory>]
    [<MemberData(nameof withoutBlog)>]
    let ``users that don't have blogs should not return any blogs`` (username: string) =
      task {
        let! j = BlogParser.ParseMultiple $"/profile/{username}/blog"
        Assert.Empty j
      }
      
    [<Theory>]
    [<MemberData(nameof withBlog)>]
    let ``created dates should be unique`` (username: string) =
      task {
        let! j = BlogParser.ParseMultiple $"/profile/{username}/blog"
        let k = j |> Seq.distinctBy (fun x -> x.Created)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

    [<Theory>]
    [<MemberData(nameof withoutBlog)>]
    let ``blog id should be unique`` (username: string) =
      task {
        let! j = BlogParser.ParseMultiple $"/profile/{username}/blog"
        let k = j |> Seq.distinctBy (fun x -> x.Id)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

    [<Theory>]
    [<MemberData(nameof withBlog)>]
    let ``thread ids that are not null should be unique`` (username: string) =
      task {
        let! a = BlogParser.ParseMultiple $"/profile/{username}/blog"
        let j = a |> Seq.filter (fun x -> x.ThreadId.HasValue)
        let k = j |> Seq.distinctBy (fun x -> x.ThreadId.Value)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }


  module BlogEndParer =
    [<Fact>]
    let ``number of pages returned should not be more than amount known`` () =
      task {
        let! node = Net.getNodeFromPath $"/profile/owie/blog"
        let no = BlogParser.ParseEnd node
        Assert.True(no >= 4)
        let! node = Net.getNodeFromPath $"/profile/death4bunnies/blog"
        let no = BlogParser.ParseEnd node
        Assert.True(no >= 2)
        let! node = Net.getNodeFromPath $"/profile/sc/blog"
        let no = BlogParser.ParseEnd node
        Assert.True(no >= 2)
        let! node = Net.getNodeFromPath $"/profile/cbishop/blog"
        let no = BlogParser.ParseEnd node
        Assert.True(no >= 88)
        let! node = Net.getNodeFromPath $"/profile/temsbumbum/blog"
        let no = BlogParser.ParseEnd node
        Assert.Equal(no, 1)
      }

  module BlogParser =
    let withBlog = usersWithBlog
    let withoutBlog = usersWithoutBlog
    
    [<Fact>]
    let ``should return empty sequence when user has no blogs`` () =
      task {
        let! node = Net.getNodeFromPath $"/profile/temsbumbum/blog"
        let j = BlogParser.ParseSingle node
        Assert.Empty j
      }

    [<Theory>]
    [<MemberData(nameof withBlog)>]
    let ``valid blog should not be empty sequence`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/blog"
        let j = BlogParser.ParseSingle node
        Assert.NotEmpty j
      }

    [<Theory>]
    [<MemberData(nameof withBlog)>]
    let ``created dates should be unique`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/blog"
        let j = BlogParser.ParseSingle node
        let k = j |> Seq.distinctBy (fun x -> x.Created)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

    [<Theory>]
    [<MemberData(nameof withBlog)>]
    let ``blog id should be unique`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/blog"
        let j = BlogParser.ParseSingle node
        let k = j |> Seq.distinctBy (fun x -> x.Id)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

    [<Theory>]
    [<MemberData(nameof withBlog)>]
    let ``threadids that are not null should be unique`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/blog"
        let j =
          BlogParser.ParseSingle node
          |> Seq.filter (fun x -> x.ThreadId.HasValue)
        let k = j |> Seq.distinctBy (fun x -> x.ThreadId.Value)
        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

module ThreadTests =
  module ThreadParser =
    [<Fact>]
    let ``should parse the correct number of threads`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node
        Assert.NotEmpty j
        Assert.Equal(50, j |> Seq.length)
      }

    [<Fact>]
    let ``number of views should not be the same`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.True(
          j
          |> Seq.distinctBy (fun x -> x.TotalView)
          |> Seq.length > 1
        )
      }

    [<Fact>]
    let ``there shouldn't be threads with unknown "ThreadType"`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.Empty(
          j
          |> Seq.filter (fun x -> x.Type = ThreadType.Unknown)
        )
      }

    [<Fact>]
    let ``number of posts should not be the same`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.True(
          j
          |> Seq.distinctBy (fun x -> x.TotalPosts)
          |> Seq.length > 1
        )
      }

    [<Fact>]
    let ``"LastPostNo" should not be the same`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.True(
          j
          |> Seq.distinctBy (fun x -> x.LastPostNo)
          |> Seq.length > 1
        )
      }

    [<Fact>]
    let ``"LastPostPage" should not be the same`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.True(
          j
          |> Seq.distinctBy (fun x -> x.LastPostPage)
          |> Seq.length > 1
        )
      }

    [<Fact>]
    let ``"Created" should not be the same`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.True(
          j
          |> Seq.distinctBy (fun x -> x.Created)
          |> Seq.length > 1
        )
      }

    [<Fact>]
    let ``"Creator.Link" should be valid`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.All(
          j,
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
    let ``thread ids should be unique`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node
        let k = j |> Seq.distinctBy (fun x -> x.Id)

        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

    [<Fact>]
    let ``should be at least two distinct blog type on frontpage`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"

        let j =
          ThreadParser.ParseSingle node
          |> Seq.distinctBy (fun x -> x.Type)

        Assert.True(j |> Seq.length |> (<) 1)
      }

    [<Fact>]
    let ``should be at least 5 (picked randomly) pinned thread on frontpage`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"

        let j =
          ThreadParser.ParseSingle node
          |> Seq.filter (fun x -> x.IsPinned)

        Assert.True(j |> Seq.length |> (<) 5)
      }

    [<Fact>]
    let ``should be at least one locked thread on frontpage`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"

        Assert.NotEmpty(
          ThreadParser.ParseSingle node
          |> Seq.filter (fun x -> x.IsLocked)
        )
      }

    [<Fact>]
    let ``should be at least one thread that is both locked and pinned on frontpage`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"

        Assert.NotEmpty(
          ThreadParser.ParseSingle node
          |> Seq.filter (fun x -> x.IsLocked && x.IsPinned)
        )
      }

    [<Fact>]
    let ``question/answered thread name always starts with "Q: "`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.All(
          j,
          fun x ->
            if x.Thread.Text.StartsWith("Q: ") then
              Assert.Contains(
                x.Type,
                [ ThreadType.Question
                  ThreadType.Answered ]
              )
        )
      }

    let extractBoard (link: string) =
      let s = link.Split("forums/").[1]

      if s.Length > 1 then
        s.Split("/")[0]
      else
        link.Split("/").[1]


    [<Fact>]
    let ``"Board.Link" should be valid`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node
        Assert.All(j, (fun x -> Assert.Null(Record.Exception(fun () -> x.Board.Link |> extractBoard |> ignore))))
      }

    [<Fact>]
    let ``"Board.Text" should satisfy invariant with "Board.Link"`` () =
      task {
        let convertNameRegex (name: string) =
          let replace (v: string) (pattern: string) (str: string) = Regex.Replace(str, pattern, v)

          name.ToLower()
          |> replace "" "&#\d+;"
          |> replace "" "&[a-z]+;"
          |> replace "" "[^- 0-9a-zA-Z]+"
          |> replace "-" " +"
          |> replace "-" "-+"

        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.All(
          j,
          fun x ->
            let name, link = x.Board.Text, x.Board.Link

            let fullName, prefix =
              link |> extractBoard, name |> convertNameRegex

            Assert.Equal(fullName[.. (prefix.Length - 1)], prefix)
        )
      }

    [<Fact>]
    let ``valid thread should not give empty sequence`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        Assert.NotEmpty(ThreadParser.ParseSingle node)
      }

    [<Fact>]
    let ``"Thread.Link" should be valid`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node

        Assert.All(
          j,
          fun k ->
            Assert.StartsWith("/", k.Thread.Link)
            Assert.EndsWith("/", k.Thread.Link)
        )
      }

    [<Fact>]
    let ``"Thread.Text" should be valid`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let j = ThreadParser.ParseSingle node
        Assert.All(j, (fun k -> Assert.True(k.Thread.Text.Length > 1)))
      }

  module ThreadEndParser =
    [<Fact>]
    let ``parsing no of threads works correctly`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/"
        let no = ThreadParser.ParseEnd node
        Assert.True(no > 16640)
      }

module PostTests = 
  module PostEndParser =
    [<Fact>]
    let ``parsing no of pages in thread works correctly`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/site-rules-and-faqs-669033/"
        Assert.Equal(PostParser.ParseEnd node, 1)
        let! node = Net.getNodeFromPath "/forums/bug-reporting-2/important-new-bug-reporting-procedure-2052355/"
        Assert.Equal(PostParser.ParseEnd node, 6)
        let! node = Net.getNodeFromPath "/forums/battles-7/mcu-thor-iw-vs-lord-boros-opm-1946556/"
        Assert.Equal(PostParser.ParseEnd node, 7)
      }

  module PostParser =
    [<Fact>]
    let ``post without date parses correctly`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/battles-7/cw-ras-al-ghul-and-damien-darhk-vs-hawkman-and-haw-1849534/"

        let j =
          PostParser.ParseSingle node
          |> Seq.tryFind (fun x -> x.PostNo = 1)

        Assert.True(j.IsSome)

        let date =
          j |> Option.map (fun x -> x.Created) |> Option.get

        Assert.Equal(DateTime.MinValue, date)
      }

    [<Fact>]
    let ``valid comments should not be empty`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = PostParser.ParseSingle node
        Assert.NotEmpty j
      }

    [<Fact>]
    let ``should correctly show edited posts exists`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"

        let j =
          PostParser.ParseSingle node
          |> Seq.filter (fun e -> e.IsEdited)

        Assert.False(j |> Seq.isEmpty)
      }

    [<Fact>]
    let ``date created for each post should be unique`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = PostParser.ParseSingle node

        let k =
          j |> Seq.distinctBy (fun e -> e.Created)

        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

    [<Fact>]
    let ``"ThreadId" value should be correct`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = PostParser.ParseSingle node
        Assert.All(j, (fun k -> Assert.Equal(2300312, k.ThreadId)))
      }

    [<Fact>]
    let ``content of each post should not be empty`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = PostParser.ParseSingle node
        Assert.All(j, (fun k -> Assert.True(k.Content.Length > 0)))
      }

    [<Fact>]
    let ``creator details should not be empty`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = PostParser.ParseSingle node

        Assert.All(
          j,
          fun k ->
            Assert.True(k.Creator.Link.Length > 0)
            Assert.True(k.Creator.Text.Length > 0)
        )
      }

    [<Fact>]
    let ``comment ids should be distinct`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = PostParser.ParseSingle node
        let k = j |> Seq.distinctBy (fun e -> e.Id)

        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

    [<Fact>]
    let ``"PostNo" should be distinct`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = PostParser.ParseSingle node

        let k =
          j |> Seq.distinctBy (fun e -> e.PostNo)

        Assert.Equal(j |> Seq.length, k |> Seq.length)
      }

    [<Fact>]
    let ``comments and op are mutually exclusive`` () =
      task {
        let! node = Net.getNodeFromPath "/forums/gen-discussion-1/new-mcu-captain-marvel-statement-2300312/"
        let j = PostParser.ParseSingle node

        Assert.All(
          j,
          fun k ->
            if k.IsComment then
              Assert.StartsWith("Cx", k.Id)
              Assert.True(k.PostNo > 0)
            else
              Assert.StartsWith("Tx", k.Id)
              Assert.Equal(0, k.PostNo)
        )
      }

    [<Theory>]
    [<InlineData(1, 0, "/forums/battles-7/tai-lung-vs-alex-madagascar-2281381/")>]
    [<InlineData(1, 0, "/forums/battles-7/thanos-vs-superman-3155/")>]
    [<InlineData(2, 4, "/forums/battles-7/thanos-vs-superman-3155/")>]
    [<InlineData(1, 1, "/forums/battles-7/cyttorak-vs-living-tribunal-528457/")>]
    [<InlineData(2, 0, "/forums/battles-7/cyttorak-vs-living-tribunal-528457/")>]
    let ``the correct amount of moderator comments should be parsed`` (page: int, expected: int, path: string) =
      task {
        let! node = Net.getNodeFromPage path page

        let j =
          PostParser.ParseSingle node
          |> Seq.filter (fun x -> x.IsModComment)
          |> Seq.length

        Assert.Equal(expected, j)
      }

  module PostFullParser =
    [<Fact>]
    let ``parsing full posts return the correct number of comments`` () =
      task {
        let! j = PostParser.ParseMultiple "/forums/battles-7/tai-lung-vs-alex-madagascar-2281381/"
        Assert.Equal(12, j |> Seq.length)
        let! j =PostParser.ParseMultiple "/forums/gen-discussion-1/your-magical-racer-and-of-course-will-be-talking-1776966/"
        Assert.Equal(1, j |> Seq.length)
        let! j =PostParser.ParseMultiple "/forums/battles-7/thanos-vs-superman-3155/"
        Assert.Equal(1759, j |> Seq.length)
        let! j = PostParser.ParseMultiple "/forums/battles-7/cyttorak-vs-living-tribunal-528457/"
        Assert.Equal(54, j |> Seq.length)
      }

module ImageParser =
  let usersWithImages: obj [] list =
    [ [| "owie" |]
      [| "static_shock" |]
      [| "cbishop" |]
      [| "death4bunnies" |]
      [| "sc" |]
      [| "darthjhawk" |] ]

  let usersWithImageTags: obj [] list =
    [ [| "owie" |]
      [| "darthjhawk" |]
      [| "cbishop" |]
      [| "sc" |] ]

  [<Theory>]
  [<MemberData(nameof usersWithImages)>]
  let ``gallery and object id should match a particular format`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}/images"
      let j = ImageParser.ParseSingle node
      Assert.Matches("\d+-\d+", j.ObjectId)
      Assert.Matches("\d+-\d+", j.GalleryId)
    }

  [<Theory>]
  [<MemberData(nameof usersWithImages)>]
  let ``every user has the "All Images" tag`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}/images"
      let j = ImageParser.ParseSingle node

      Assert.True(
        j.Tags
        |> Seq.exists (fun x -> x.Text = "All Images")
      )
    }

  [<Theory>]
  [<MemberData(nameof usersWithImageTags)>]
  let ``tags link should be in the correct format`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}/images"
      let j = ImageParser.ParseSingle node
      Assert.All(j.Tags, (fun k -> Assert.True(k.Link.StartsWith("?tag="))))
    }

  [<Theory>]
  [<MemberData(nameof usersWithImages)>]
  let ``the amount of images should be more than one`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}/images"
      let j = ImageParser.ParseSingle node
      Assert.True(j.TotalImages > 1)
    }

module FollowTests =
  module FollowRelationship =
    let usersWithFollows: obj [] list =
      [ [| "owie" |]
        [| "static_shock" |]
        [| "cbishop" |]
        [| "death4bunnies" |]
        [| "sc" |]
        [| "darthjhawk" |] ]

    [<Theory>]
    [<MemberData(nameof usersWithFollows)>]
    let ``parsing users followers should not return empty sequence`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/follower"
        let j = FollowerParser.ParseSingle node
        Assert.NotEmpty(j)
      }

    [<Theory>]
    [<MemberData(nameof usersWithFollows)>]
    let ``parsed followers should be valid`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/follower"
        let j = FollowerParser.ParseSingle node

        Assert.All(
          j,
          (fun x ->
            Assert.NotEmpty(x.Avatar)
            Assert.NotEmpty(x.Follower.Link))
        )
      }

    [<Fact>]
    let ``parsing users without followers should return empty sequence`` () =
      task {
        let! node = Net.getNodeFromPath $"/profile/temsbumbum/follower"
        let j = FollowerParser.ParseSingle node
        Assert.Empty(j)
      }

    [<Theory>]
    [<MemberData(nameof usersWithFollows)>]
    let ``parsing users followings should not return empty sequence`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/following"
        let j = FollowingParser.ParseSingle node
        Assert.NotEmpty(j)
      }

    [<Theory>]
    [<MemberData(nameof usersWithFollows)>]
    let ``parsed followings should be valid`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/following"
        let j = FollowingParser.ParseSingle node

        Assert.All(
          j,
          (fun x ->
            Assert.NotEmpty(x.Avatar)
            Assert.NotEmpty(x.Following.Link)
            Assert.NotEmpty(x.Type))
        )
      }

    [<Fact>]
    let ``parsing users without followings should return empty sequence`` () =
      task {
        let! node = Net.getNodeFromPath $"/profile/temsbumbum/following"
        let j = FollowingParser.ParseSingle node
        Assert.Empty(j)
      }

  module FollowRelationshipEndParser =
    let usersWithManyFollows: obj [] list =
      [ [| "owie" |]
        [| "static_shock" |]
        [| "cbishop" |]
        [| "sc" |] ]

    [<Fact>]
    let ``parsing users without followings should only have one page of followings`` () =
      task {
        let! node = Net.getNodeFromPath $"/profile/temsbumbum/following"
        Assert.Equal(1, Common.parseFollowRelationshipEnd node)
      }

    [<Fact>]
    let ``parsing users without followers should only have one page of followers`` () =
      task {
        let! node = Net.getNodeFromPath $"/profile/temsbumbum/follower"
        Assert.Equal(1, Common.parseFollowRelationshipEnd node)
      }

    [<Theory>]
    [<MemberData(nameof usersWithManyFollows)>]
    let ``no of pages for users with many followers should more than one`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/follower"
        Assert.True(Common.parseFollowRelationshipEnd node > 1)
      }

    [<Theory>]
    [<MemberData(nameof usersWithManyFollows)>]
    let ``no of pages for users with many followings should more than one`` (username: string) =
      task {
        let! node = Net.getNodeFromPath $"/profile/{username}/following"

        let no =
          Common.parseFollowRelationshipEnd node

        Assert.True(no > 1)
      }

  module FollowRelationshipFullParser =
    let sampleUsers: obj [] list =
      [ [| "owie" |]
        [| "static_shock" |]
        [| "cbishop" |]
        [| "death4bunnies" |]
        [| "sc" |]
        [| "darthjhawk" |]
        [| "temsbumbum" |] ]

    [<Theory>]
    [<MemberData(nameof sampleUsers)>]
    let ``no of followers parsed from profile should be consistent`` (username: string) =
      task {
        let! profileNode = Net.getNodeFromPath $"/profile/{username}"

        let profile =
          ProfileParser.ParseSingle profileNode

        let! follower =
          Common.ParseMultiple FollowerParser.ParseSingle FollowerParser.ParseEnd $"/profile/{username}/follower"

        Assert.Equal(profile.Followers, follower |> Seq.length)
      }

    [<Theory>]
    [<MemberData(nameof sampleUsers)>]
    let ``no of followings parsed from profile should be consistent`` (username: string) =
      task {
        let! profileNode = Net.getNodeFromPath $"/profile/{username}"

        let profile =
          ProfileParser.ParseSingle profileNode

        let! following =
          Common.ParseMultiple FollowingParser.ParseSingle FollowingParser.ParseEnd $"/profile/{username}/following"

        Assert.Equal(profile.Following, following |> Seq.length)
      }

module WikiParser =
  let usersWithWiki: obj [] list =
    [ [| "owie" |]; [| "cbishop" |] ]


module ProfileParsers =
  let usersWithoutAboutSummary: obj [] list =
    [ [| "estrelladeleon" |]
      [| "temsbumbum" |] ]

  let sampleUsers: obj [] list =
    [ [| "owie" |]
      [| "static_shock" |]
      [| "cbishop" |]
      [| "death4bunnies" |]
      [| "sc" |]
      [| "darthjhawk" |]
      [| "temsbumbum" |] ]

  let usersWithCover: obj [] list =
    [ [| "static_shock" |]
      [| "cbishop" |]
      [| "death4bunnies" |]
      [| "sc" |]
      [| "darthjhawk" |] ]

  let usersWithImage: obj [] list =
    [ [| "owie" |]
      [| "static_shock" |]
      [| "cbishop" |]
      [| "death4bunnies" |]
      [| "sc" |]
      [| "darthjhawk" |] ]

  let usersWithBlog: obj [] list =
    [ [| "owie" |]
      [| "death4bunnies" |]
      [| "sc" |]
      [| "cbishop" |]
      [| "static_shock" |] ]

  let usersWithNoCover: obj [] list =
    [ [| "abc" |] ]

  [<Theory>]
  [<MemberData(nameof sampleUsers)>]
  let ``background images should have correct format`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}"
      let j = ProfileParser.ParseSingle node
      Assert.True(j.BackgroundImage.StartsWith(@"https://comicvine.gamespot.com/a/"))
    }

  [<Theory>]
  [<MemberData(nameof sampleUsers)>]
  let ``avatar should have correct format`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}"
      let j = ProfileParser.ParseSingle node
      Assert.True(j.Avatar.StartsWith(@"https://comicvine.gamespot.com/a/"))
    }

  [<Theory>]
  [<MemberData(nameof usersWithCover)>]
  let ``cover should have correct format`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}"
      let j = ProfileParser.ParseSingle node
      Assert.True(j.CoverImage.StartsWith(@"https://comicvine.gamespot.com/a/"))
    }

  [<Theory>]
  [<MemberData(nameof sampleUsers)>]
  let ``description should contain something`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}"
      let j = ProfileParser.ParseSingle node
      Assert.True(j.Description.Length > 0)
    }

  [<Theory>]
  [<MemberData(nameof sampleUsers)>]
  let ``username should contain something`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}"
      let j = ProfileParser.ParseSingle node
      Assert.True(j.UserName.Length > 0)
    }

  [<Theory>]
  [<MemberData(nameof usersWithImage)>]
  let ``users with images should have the corresponding flag set`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}"
      let j = ProfileParser.ParseSingle node
      Assert.True(j.HasImages)
    }

  [<Theory>]
  [<MemberData(nameof usersWithBlog)>]
  let ``users with blogs should have the corresponding flag set`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}"
      let j = ProfileParser.ParseSingle node
      Assert.True(j.HasBlogs)
    }

  [<Theory>]
  [<MemberData(nameof usersWithoutAboutSummary)>]
  let ``users without about summary should have empty field`` (username: string) =
    task {
      let! node = Net.getNodeFromPath $"/profile/{username}"
      let j = ProfileParser.ParseSingle node
      Assert.True(j.About.Summary.Length = 0)
    }
