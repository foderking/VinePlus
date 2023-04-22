
namespace Comicvine.Core
open System
// open System.Collections.Generic
open System.Threading.Tasks
open FSharp.Control
open HtmlAgilityPack
open Microsoft.FSharp.Core

module Parsers =
    // type CommentContent = string
    // type OPContent = string
    
    type Alignment =
        | None    = 1
        | Good    = 2
        | Neutral = 3
        | Evil    = 4
    type ThreadType =
        | Normal   = 1
        | Poll     = 2
        | Blog     = 3
        | Question = 4
        | Answered = 5
        | Unknown  = 6
        
    type Link =
        { Text: string; Link: string }
        
    // Threads and Posts section
    // type CommentInfo = { Id: int; PostNo: int }
    type Post =
        {
            IsComment: bool; IsDeleted: bool; CommentId: Nullable<int>; PostNo: int
            Creator: Link; IsEdited: bool; Created: DateTime; Content: string; ThreadId: int
        }
    type Thread =
        {
            Id: int; Thread: Link; Board: Link; IsPinned: bool; IsLocked: bool; IsDeleted: bool
            Type: ThreadType; LastPostNo: int; LastPostPage: int; Created: DateTime;
            TotalPosts: int; TotalView: int; Creator: Link; Comments: List<int>
        }
        
    // Profile section
    type ActivityType =
        | Comment = 1
        | Image   = 2
        | Follow  = 3
    type Activity =
        { Type: ActivityType;  }
    //     | Comment of {| Content: string; Topic: Link; Forum: Link |}
    //     | Image   of {| Url: string |}
    //     | Follow  of {| User: Link |}
    type About =
        { DateJoined: DateTime;  Alignment: Alignment; Points: int; Summary: string }
        
    type Profile =
        {
            UserName: string; Avatar: string; Description: string; Posts: int; WikiPoints: int
            Following: int; Followers: int; CoverImage: string; BackgroundImage: string; About: About;
            Activities: seq<Activity>; HasBlogs: bool; HasImages: bool; // forums, wiki are always there
        }
        
    // Extra profile info
    type Blog =
        { Blog: Link; Created: DateTime; Comments: int; Id: int; ThreadId: Nullable<int> }
    type Image =
        { ObjectId: string; GalleryId: string; TotalImages: int; Tags: seq<Link> }
    type Following =
        { Following: Link; Avatar: string; Type: string }
    type Follower =
        { Follower: Link; Avatar: string }
        
        
        
    // helpers
    let _innerT (node: HtmlNode) = node.InnerText
    let _split (s: string) = s.Split()
    let _innerTrim (node: HtmlNode) = node.InnerText.Trim()
    
    let _getAttribute (def: string) (attrib: string) (node: HtmlNode) =
        node.GetAttributeValue(attrib, def)
       
    let _getAttrib =
        _getAttribute ""
        
    // functions for selecting html nodes
    let _getChildElementsIfAny predicate name (node: HtmlNode) =
        let elems = node.Elements name |> Seq.filter predicate
        if (elems |> Seq.length) < 1 then
            None
        else
            Some(elems)
    
    let _getFirstChildIfAny predicate name (node: HtmlNode) =
        let elems = _getChildElementsIfAny predicate name node
        match elems with
        | Some(arr) -> Some(arr |> Seq.head)
        | None -> None
        
    let _getChildElements predicate name (node: HtmlNode) =
        node.Elements name
        |> Seq.filter predicate
        
    let _getChildElems =
        _getChildElements (fun _ -> true)
        
    let _getFirstChildElement predicate name =
        _getChildElements predicate name >> Seq.head
    
    let _getFirstChildElem =
        _getFirstChildElement (fun _ -> true)
        
    // predicates for filtering html nodes
    let _idPredicate id (node: HtmlNode) =
        _getAttrib "id" node = id
       
    let _classPredicate cls (node: HtmlNode) =
        node.HasClass cls
 
    let _attribPredicate attrib value node =
        (node |> _getAttrib attrib) = value
    
    
    
          
    let unwrapTaskSeq t =
        let _folder state curr =
            Seq.append state [curr]
        t
        |> TaskSeq.map (fun x -> x |> TaskSeq.ofSeq)
        |> TaskSeq.concat 
        |> TaskSeq.fold _folder (Seq.ofList [])
   
    
   
    type ISingle<'T> =
        interface
            abstract member ParseSingle: HtmlNode -> 'T
        end
        
    type IMultiple<'T> =
        interface
            inherit ISingle<seq<'T>>
            abstract _parseEnd: HtmlNode -> int
            abstract ParseAll: string -> Task<seq<'T>>
        end
        
    let yieldMultipleData (path: string) (page: int) (parser: IMultiple<'T>) = taskSeq {
        let! stream = Net.getStreamByPage page path 
        let node = stream |> Net.getRootNode
        let lastPage = parser._parseEnd node
        yield parser.ParseSingle node
        
        for p in [2..lastPage] do
            let! s = Net.getStreamByPage p path 
            yield s
                |> Net.getRootNode
                |> parser.ParseSingle
    }
    
    let parseMultipleGeneric (path: string) (page: int) (parser: IMultiple<'T>) =
        yieldMultipleData path page parser
        |> unwrapTaskSeq
        
        
        
        
        
   
    // let parseAllData parseData parseEnd (path: string) page = taskSeq {
    //     let! stream = Net.getStreamByPage page path 
    //     let node = stream |> Net.getRootNode
    //     let last = parseEnd node
    //     yield parseData node
    //     
    //     for p in [2..last] do
    //         let! s = Net.getStreamByPage p path 
    //         yield s
    //             |> Net.getRootNode
    //             |> parseData
    // }

    let _unwrapSome message opt =
        match opt with
        | Some x -> x
        | None ->  raise (Exception(message))
        
        
        
        
        
        
        
    // commonly used nodes
    let _getWrapperNode node =
        _getFirstChildElem "html" node
        |> _getFirstChildElem "body"
        |> _getFirstChildElement (_idPredicate "site-main") "div"
        |> _getFirstChildElement (_idPredicate "wrapper"  ) "div"
        
    let _getForumBlockNode wrapperNode =
        if Option.isSome (_getFirstChildIfAny (_idPredicate "forum-content") "div" wrapperNode) then
            wrapperNode
            |> _getFirstChildElement (_idPredicate "forum-content") "div"
            |> _getFirstChildElement (_classPredicate "three-column--span-two") "div"
            |> Some
        elif Option.isSome (_getFirstChildIfAny (_classPredicate "js-toc-generate") "div" wrapperNode) then
            wrapperNode
            |> _getFirstChildElement (_classPredicate "js-toc-generate") "div"
            |> _getFirstChildElement (_idPredicate "site") "div"
            |> _getFirstChildElement (_idPredicate "forum-content") "div"
            |> _getFirstChildElement (_classPredicate "primary-content") "div"
            |> Some
        else
            None
            
    let parsePageEnd rootNode =
        match rootNode
            |> _getWrapperNode
            |> _getForumBlockNode
            |> _unwrapSome "Unknown Thread type"
            |> _getFirstChildElement (_classPredicate "forum-bar") "div"
            |> _getFirstChildIfAny (_classPredicate "paginate") "ul" with
        | Some(x) ->
            x
            |> _getChildElements (_attribPredicate "class" "paginate__item") "li"
            |> Seq.map (fun a -> a.InnerText.Trim())
            |> Seq.last
            |> int
        | None -> 1
        
        
    let parseBlogEnd (rootNode: HtmlNode) =
        match
            rootNode
            |> _getWrapperNode
            |> _getFirstChildElement (_idPredicate "site") "div"
            |> _getFirstChildElement (_idPredicate "default-content") "div"
            |> _getFirstChildElement (_classPredicate "primary-content") "div"
            |> _getFirstChildIfAny (_classPredicate "paginate") "ul"
        with
        | Some(n) ->
            n
            |> _getChildElements (_attribPredicate "class" "paginate__item") "li"
            |> Seq.last
            |> _innerTrim
            |> int
        | None -> 1
        
   
    // let parseAllBlogs path page =
    //     parseAllData parseBlog parseBlogEnd path page
        
    // let ParseBlogsFull path =
    //     parseAllBlogs path 1
    //     |> unwrapTaskSeq
    
    type ThreadParser() =
        member this._parseEnd(path) =
            (this :> IMultiple<Thread>)._parseEnd(path)
        member this.ParseAll(path) =
            (this :> IMultiple<Thread>).ParseAll(path)
        member this.ParseSingle(node) =
            (this :> IMultiple<Thread>).ParseSingle(node)
            
        interface IMultiple<Thread> with
            member this.ParseAll(path) =
                this
                |> parseMultipleGeneric path 1
                
            member this._parseEnd(rootNode) =
                parsePageEnd rootNode
                
            member this.ParseSingle(rootNode) =
                rootNode
                |> _getWrapperNode
                |> _getForumBlockNode
                |> _unwrapSome "Unknown Thread type"
                |> _getFirstChildElement (_classPredicate "table-forums") "div"
                |> _getChildElements (_classPredicate "flexbox-align-stretch") "div"
                |> Seq.map (
                    fun flexNode ->
                        let views = 
                            flexNode
                            |> _getFirstChildElement (_attribPredicate "class" "inner-space-small views hide-mobile") "div"
                            |> (fun x -> x.InnerText.Trim().Replace(",", ""))
                            |> int
                        let posts = 
                            flexNode
                            |> _getFirstChildElement (_attribPredicate "class" "js-posts inner-space-small views") "div"
                            |> (fun x -> x.InnerText.Trim().Replace(",", ""))
                            |> int
                        let lastPostNo =
                            flexNode
                            |> _getFirstChildElement (_attribPredicate "class" "inner-space-small last-post hide-mobile") "div"
                            |> _getFirstChildElement (_classPredicate "info") "span"
                            |> _getFirstChildElement (_classPredicate "last") "a"
                            |> _getAttrib "href"
                            |> (fun x -> x.Split("#").[1].Split("-") |> Seq.last)
                            |> int
                        let lastPostPage =
                            flexNode
                            |> _getFirstChildElement (_attribPredicate "class" "inner-space-small last-post hide-mobile") "div"
                            |> _getFirstChildElement (_classPredicate "info") "span"
                            |> _getFirstChildElement (_classPredicate "last") "a"
                            |> _getAttrib "href"
                            |> (fun x -> x.Split("#").[0].Split("=")[1])
                            |> int
                        let created =
                            flexNode
                            |>  _getFirstChildElement (_attribPredicate "class" "inner-space-small author hide-laptop") "div"
                            |> _getFirstChildElement (_classPredicate "info") "span"
                            |> _innerTrim
                            |> DateTime.Parse
                        let creatorName = 
                            flexNode
                            |>  _getFirstChildElement (_attribPredicate "class" "inner-space-small author hide-laptop") "div"
                            |> _getFirstChildElem "div"
                            |> _getFirstChildElem "a"
                            |> _innerTrim
                        let creatorLink = 
                            flexNode
                            |>  _getFirstChildElement (_attribPredicate "class" "inner-space-small author hide-laptop") "div"
                            |> _getFirstChildElem "div"
                            |> _getFirstChildElem "a"
                            |> _getAttrib "href"
                        let boardName =
                            flexNode
                            |> _getFirstChildElement (_attribPredicate "class" "inner-space-small forum-topic") "div"
                            |> _getFirstChildElement (_classPredicate "board") "a"
                            |> _innerTrim
                        let boardLink =
                            flexNode
                            |> _getFirstChildElement (_attribPredicate "class" "inner-space-small forum-topic") "div"
                            |> _getFirstChildElement (_classPredicate "board") "a"
                            |> _getAttrib "href"
                        let threadName =
                            flexNode
                            |> _getFirstChildElement (_attribPredicate "class" "inner-space-small forum-topic") "div"
                            |> _getFirstChildElem "div"
                            
                            |> _getFirstChildElement (_classPredicate "topic-name") "a"
                            |> _innerTrim
                        let threadLink =
                           flexNode
                           |> _getFirstChildElement (_attribPredicate "class" "inner-space-small forum-topic") "div"
                           |> _getFirstChildElem "div"
                           
                           |> _getFirstChildElement (_classPredicate "topic-name") "a"
                           |> _getAttrib "href"
                           
                        let (|ThreadType|) (s: string) =
                            match s with
                            | "Poll"     -> ThreadType.Poll
                            | "Blog"     -> ThreadType.Blog
                            | "Question" -> ThreadType.Question
                            | "Answered" -> ThreadType.Answered
                            | _  -> ThreadType.Unknown
                            
                        let threadType =
                           match 
                               flexNode
                               |> _getFirstChildElement (_attribPredicate "class" "inner-space-small forum-topic") "div"
                               |> _getFirstChildElem "div"
                               |> _getFirstChildIfAny (_classPredicate "type") "span"
                           with
                           | Some(node) ->
                               match (_innerTrim node) with ThreadType x -> x
                           | None -> ThreadType.Normal
                        
                        let id =
                            flexNode
                            |> _getFirstChildElement (_classPredicate "js-posts") "div"
                            |> _getFirstChildElement (_classPredicate "js-post-render-topic") "meta"
                            |> _getAttrib "data-post-render-value"
                            |> int
                        let isLocked = 
                               flexNode
                               |> _getFirstChildElement (_attribPredicate "class" "inner-space-small forum-topic") "div"
                               |> _getFirstChildElem "div"
                               |> _getFirstChildIfAny (_attribPredicate "src" "https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-lock-16x16.png") "img"
                               |> Option.isSome
                        let isPinned = 
                               flexNode
                               |> _getFirstChildElement (_attribPredicate "class" "inner-space-small forum-topic") "div"
                               |> _getFirstChildElem "div"
                               |> _getFirstChildIfAny (_attribPredicate "src" "https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-pin-16x16.png") "img"
                               |> Option.isSome
                        {
                            Thread = { Text = threadName; Link = threadLink }; Board = { Text = boardName; Link = boardLink } ;
                            Id = id; IsPinned = isPinned; IsLocked = isLocked; Type = threadType; LastPostNo = lastPostNo;
                            LastPostPage = lastPostPage; Created = created; TotalPosts = posts; TotalView = views; IsDeleted = false;
                            Creator = { Text = creatorName; Link = creatorLink }; Comments = List.Empty }
                        )
                
       
    // let parsePosts (rootNode: HtmlNode) =
    //     let node =
    //         rootNode
    //         |> _getWrapperNode
    //         |> _getForumBlockNode
    //         |> _unwrapSome "Unknown Thread type"
    //         |> _getFirstChildElement (_classPredicate "js-forum-block") "div"
    //         |> _getFirstChildElement (_classPredicate "forum-messages") "section"
    //         
    //     let threadId =
    //         node
    //         |> _getFirstChildElement (_attribPredicate "data-post-render-param" "ForumBundle.topicId") "meta"
    //         |> _getAttrib "data-post-render-value"
    //         |> int
    //     node
    //     |> _getChildElements (_classPredicate "js-message") "div"
    //     |> Seq.map(
    //         fun node ->
    //             let messageNode =
    //                  node
    //                  |> _getFirstChildElement (_classPredicate "message-wrap") "div"
    //                  |> _getFirstChildElement (_classPredicate "message-inner") "div"
    //                              
    //             let edited =
    //                 messageNode
    //                 |> _getFirstChildElement (_classPredicate "message-title") "div"
    //                 |> (fun x -> x.InnerText.Contains("Edited By"))
    //             let created =
    //                 messageNode
    //                 |> _getFirstChildElement (_classPredicate "message-options") "div"
    //                 |> _getFirstChildElement (_classPredicate "date") "time"
    //                 |> _getAttrib "datetime"
    //                 |> DateTime.Parse
    //             let content =
    //                 messageNode
    //                 |> _getFirstChildElement (_classPredicate "message-content") "article"
    //                 |> (fun x -> x.InnerHtml)
    //             let creator =
    //                 {
    //                     Link =
    //                         messageNode
    //                         |> _getFirstChildElement (_classPredicate "message-title") "div"
    //                         |> _getFirstChildElement (_classPredicate "message-user") "a"
    //                         |> _getAttrib "data-user-profile"
    //                     Text =
    //                         messageNode
    //                         |> _getFirstChildElement (_classPredicate "message-title") "div"
    //                         |> _getFirstChildElement (_classPredicate "message-user") "a"
    //                         |> (fun x -> x.InnerText.Trim())
    //                 }
    //             
    //             if messageNode
    //                 |> _getFirstChildElement (_classPredicate "message-title") "div"
    //                 |> _getChildElements (fun n -> n.Attributes.Contains("name")) "a"
    //                 |> Seq.exists (fun _ -> true)
    //             then
    //                 {
    //                     ThreadId = threadId; Content = content; Created = created; IsEdited = edited; Creator = creator
    //                     Id =
    //                         messageNode
    //                         |> _getFirstChildElement (_classPredicate "message-title") "div"
    //                         |> _getFirstChildElement (fun n -> n.Attributes.Contains("name")) "a"
    //                         |> _getAttrib "name"
    //                         |> (fun x -> x.Split("-"))
    //                         |> Seq.last
    //                         |> int;
    //                     PostNo =
    //                         messageNode
    //                         |> _getFirstChildElement (_classPredicate "message-title") "div"
    //                         |> _getFirstChildElement (fun n -> n.Attributes.Contains("name")) "a"
    //                         |> (fun x -> x.InnerText.Trim()[1..])
    //                         |> int  
    //                 }
    //                 |> ThreadPost.Comment
    //             else
    //                 { ThreadId = threadId; Content = content; Created = created; IsEdited = edited; Creator = creator }
    //                 |> ThreadPost.OP
    //     )
        
        
        
    type PostParser() =
        member this._parseEnd(path) =
            (this :> IMultiple<Post>)._parseEnd(path)
        member this.ParseAll(path) =
            (this :> IMultiple<Post>).ParseAll(path)
        member this.ParseSingle(node) =
            (this :> IMultiple<Post>).ParseSingle(node)
            
        interface IMultiple<Post> with
            member this.ParseAll(path) =
                this
                |> parseMultipleGeneric path 1
                
            member this._parseEnd(rootNode) =
                parsePageEnd rootNode
                
            member this.ParseSingle(rootNode) =
                let node =
                    rootNode
                    |> _getWrapperNode
                    |> _getForumBlockNode
                    |> _unwrapSome "Unknown Thread type"
                    |> _getFirstChildElement (_classPredicate "js-forum-block") "div"
                    |> _getFirstChildElement (_classPredicate "forum-messages") "section"
                    
                let threadId =
                    node
                    |> _getFirstChildElement (_attribPredicate "data-post-render-param" "ForumBundle.topicId") "meta"
                    |> _getAttrib "data-post-render-value"
                    |> int
                node
                |> _getChildElements (_classPredicate "js-message") "div"
                |> Seq.map(
                    fun node ->
                        let messageNode =
                             node
                             |> _getFirstChildElement (_classPredicate "message-wrap") "div"
                             |> _getFirstChildElement (_classPredicate "message-inner") "div"
                                         
                        let edited =
                            messageNode
                            |> _getFirstChildElement (_classPredicate "message-title") "div"
                            |> (fun x -> x.InnerText.Contains("Edited By"))
                        let created =
                            messageNode
                            |> _getFirstChildElement (_classPredicate "message-options") "div"
                            |> _getFirstChildElement (_classPredicate "date") "time"
                            |> _getAttrib "datetime"
                            |> DateTime.Parse
                        let content =
                            messageNode
                            |> _getFirstChildElement (_classPredicate "message-content") "article"
                            |> (fun x -> x.InnerHtml)
                        let creator =
                            {
                                Link =
                                    messageNode
                                    |> _getFirstChildElement (_classPredicate "message-title") "div"
                                    |> _getFirstChildElement (_classPredicate "message-user") "a"
                                    |> _getAttrib "data-user-profile"
                                Text =
                                    messageNode
                                    |> _getFirstChildElement (_classPredicate "message-title") "div"
                                    |> _getFirstChildElement (_classPredicate "message-user") "a"
                                    |> (fun x -> x.InnerText.Trim())
                            }
                        let isComment = 
                            messageNode
                            |> _getFirstChildElement (_classPredicate "message-title") "div"
                            |> _getChildElements (fun n -> n.Attributes.Contains("name")) "a"
                            |> Seq.exists (fun _ -> true)
                        
                        {
                            IsDeleted = false; IsComment = isComment; IsEdited = edited; Creator = creator
                            Created = created; Content = content; ThreadId = threadId
                            CommentId =
                                if not isComment then
                                    Nullable()
                                else
                                    messageNode
                                    |> _getFirstChildElement (_classPredicate "message-title") "div"
                                    |> _getFirstChildElement (fun n -> n.Attributes.Contains("name")) "a"
                                    |> _getAttrib "name"
                                    |> (fun x -> x.Split("-"))
                                    |> Seq.last
                                    |> int
                                    |> Nullable
                            PostNo =
                                if not isComment then
                                    0
                                else
                                    messageNode
                                    |> _getFirstChildElement (_classPredicate "message-title") "div"
                                    |> _getFirstChildElement (fun n -> n.Attributes.Contains("name")) "a"
                                    |> (fun x -> x.InnerText.Trim()[1..])
                                    |> int
                        }                                   
                                    
                        
                )
                
                // parsePosts rootNode
                // |> Seq.map (
                //     fun node ->
                //         match node with
                //         | ThreadPost.Comment(n) ->
                //             { IsComment = true ; Comment = n; OP = Unchecked.defaultof<_> ; IsDeleted = false}
                //         | ThreadPost.OP(n) ->
                //             { IsComment = false; Comment = Unchecked.defaultof<_> ; OP = n; IsDeleted = false}
                // )
    // let parseAllPosts (path: string) page = taskSeq {
    //     let! stream = Net.getStreamByPage page path 
    //     let node = stream |> Net.getRootNode
    //     let last = parsePageEnd node
    //     yield ParsePosts node
    //     
    //     for p in [2..last] do
    //         printfn "%A" p
    //         let! s= Net.getStreamByPage p path 
    //         yield s
    //             |> Net.getRootNode
    //             |> ParsePosts
    // }
    //
    // let ParsePostsFull path = 
    //     parseAllPosts path 1
    //     |> unwrapTaskSeq
    //     
        
        
         
    type BlogParser() =
        member this._parseEnd(path) =
            (this :> IMultiple<Blog>)._parseEnd(path)
        member this.ParseAll(path) =
            (this :> IMultiple<Blog>).ParseAll(path)
        member this.ParseSingle(node) =
            (this :> IMultiple<Blog>).ParseSingle(node)
            
        interface IMultiple<Blog> with
            member this.ParseAll(path) =
                this
                |> parseMultipleGeneric path 1
                
            member this._parseEnd(rootNode) =
                parseBlogEnd rootNode
                
            member this.ParseSingle(rootNode) = 
                let parse (node: HtmlNode) =
                    let aNode =
                        node
                        |> _getFirstChildElement (_classPredicate "news-hdr") "section"
                        |> _getFirstChildElem "h1"
                        |> _getFirstChildElem "a"
                    let hNode =
                        node
                        |> _getFirstChildElement (_classPredicate "news-hdr") "section"
                        |> _getFirstChildElem "h4"
                    {
                        Blog =  { Text = aNode.InnerText.Trim(); Link = _getAttrib "href" aNode }
                        Created  =
                            hNode
                            |> _getFirstChildElem "time"
                            |> _getAttrib "datetime"
                            |> DateTime.Parse
                        Comments =
                            hNode
                            |> _getFirstChildElem "a"
                            |> _innerTrim
                            |> _split
                            |> Seq.head
                            |> int
                        Id =
                            aNode
                            |> _getAttrib "href"
                            |> (fun x -> x.Split("/"))
                            |> Seq.filter (fun x -> x.Length > 0)
                            |> Seq.last
                            |> int
                            
                        ThreadId =
                            match
                                hNode
                                |> _getFirstChildElem "a"
                                |> _getAttrib "href"
                                |> (fun x -> x.Split("-"))
                                |> Seq.last
                                |> Int32.TryParse
                            with
                            | true,  x -> Nullable x
                            | false, _ -> Nullable()
                    }
                    
                _getWrapperNode rootNode
                |> _getFirstChildElement (_idPredicate "site") "div"
                |> _getFirstChildElement (_idPredicate "default-content") "div"
                |> _getFirstChildElement (_classPredicate "primary-content") "div"
                |> _getChildElements (_classPredicate "profile-blog") "article"
                |> Seq.map parse
        
        
    type ImageParser() =
        member this.ParseSingle(node) =
            (this :> ISingle<Image>).ParseSingle(node)
            
        interface ISingle<Image> with
            member this.ParseSingle(rootNode) = 
                let xNode =
                    rootNode
                    |> _getWrapperNode
                    |> _getFirstChildElement (_idPredicate "site") "div"
                    |> _getFirstChildElement (_idPredicate "gallery-content") "div"
                    |> _getFirstChildElement (_classPredicate "primary-content") "div"
                    
                    |> _getFirstChildElement (_classPredicate "gallery-header") "header"
                    |> _getFirstChildElement (_classPredicate "isotope-image") "div"
                    |> _getFirstChildElement (_classPredicate "gallery-tags") "ul"
                let dataNode =
                    xNode
                    |> _getFirstChildElement (_classPredicate "gallery-tags__item") "li"
                    |> _getFirstChildElement (_idPredicate "galleryMarker") "a"

                {
                    GalleryId = _getAttrib "data-gallery-id" dataNode
                    ObjectId  = _getAttrib "data-object-id" dataNode
                    Tags =
                        xNode
                        |> _getChildElements (_classPredicate "gallery-tags__item") "li"
                        |> Seq.map (fun x ->
                            {
                                Link = 
                                    x
                                    |> _getFirstChildElem "a"
                                    |> _getAttrib "href"
                                Text = 
                                    x
                                    |> _getFirstChildElem "a"
                                    |> _innerTrim
                            }
                        )
                    TotalImages =
                        rootNode
                        |> _getWrapperNode
                        |> _getFirstChildElement (_classPredicate "sub-nav") "nav"
                        |> _getFirstChildElement (_classPredicate "container") "div"
                        |> _getFirstChildElem "ul"
                        |> _getFirstChildElement (fun x -> x.InnerText.Trim().StartsWith("Images") ) "li"
                        |> _innerTrim
                        |> (fun x -> x.Split("(").[1].Trim(')') )
                        |> int
                }

 
    let parseFollowRelationshipEnd rootNode =
        match
            rootNode
            |> _getWrapperNode
            |> _getFirstChildElement (_idPredicate "site") "div"
            |> _getFirstChildElement (_idPredicate "default-content") "div"
            |> _getFirstChildElement (_classPredicate "primary-content") "div"
            |> _getFirstChildElement (_idPredicate "js-sort-filter-results") "div"
            |> _getFirstChildElement (_classPredicate "navigation") "div"
            |> _getFirstChildIfAny (fun _ -> true) "ul"
        with
        | None -> 1
        | Some x ->
            x
            |> _getChildElements (_attribPredicate "class" "paginate__item") "li"
            |> Seq.last
            |> _innerTrim
            |> int
    

    let parseFollowRelationship parseData rootNode =
        match
            rootNode
            |> _getWrapperNode
            |> _getFirstChildElement (_idPredicate "site") "div"
            |> _getFirstChildElement (_idPredicate "default-content") "div"
            |> _getFirstChildElement (_classPredicate "primary-content") "div"
            |> _getFirstChildElement (_idPredicate "js-sort-filter-results") "div"
            |> _getFirstChildElem "table"
            |> _getFirstChildElem "tbody"
            |> _getChildElementsIfAny (fun _ -> true) "tr"
        with
        | Some n ->
            n
            |> Seq.map parseData
        | None -> []
        
    
    type FollowerParser() =
        member this._parseEnd(path) =
            (this :> IMultiple<Follower>)._parseEnd(path)
        member this.ParseAll(path) =
            (this :> IMultiple<Follower>).ParseAll(path)
        member this.ParseSingle(node) =
            (this :> IMultiple<Follower>).ParseSingle(node)
            
        interface IMultiple<Follower> with
            member this.ParseAll(path) =
                this
                |> parseMultipleGeneric path 1
                
            member this._parseEnd(rootNode) =
                parseFollowRelationshipEnd rootNode
                
            member this.ParseSingle(rootNode) =
                let parser tr =
                    {
                        Follower =
                            {
                                Text =
                                    tr
                                    |>  _getFirstChildElem "td"
                                    |>  _getFirstChildElem "a"
                                    |> _innerTrim
                                Link = 
                                    tr
                                    |>  _getFirstChildElem "td"
                                    |>  _getFirstChildElem "a"
                                    |> _getAttrib "href"
                            }
                        Avatar =
                            tr
                            |>  _getFirstChildElem "td"
                            |>  _getFirstChildElem "a"
                            |>  _getFirstChildElem "img"
                            |> _getAttrib "src"
                    }
                parseFollowRelationship parser rootNode
        
    
    type FollowingParser() =
        member this._parseEnd(path) =
            (this :> IMultiple<Following>)._parseEnd(path)
        member this.ParseAll(path) =
            (this :> IMultiple<Following>).ParseAll(path)
        member this.ParseSingle(node) =
            (this :> IMultiple<Following>).ParseSingle(node)
            
        interface IMultiple<Following> with
            member this.ParseAll(path) =
                this
                |> parseMultipleGeneric path 1
                
            member this._parseEnd(rootNode) =
                parseFollowRelationshipEnd rootNode
                
            member this.ParseSingle(rootNode) =
                let parser (tr: HtmlNode) =
                    {
                        Following =
                            {
                                Text =
                                    tr
                                    |>  _getFirstChildElem "td"
                                    |>  _getFirstChildElem "a"
                                    |> _innerTrim
                                Link = 
                                    tr
                                    |>  _getFirstChildElem "td"
                                    |>  _getFirstChildElem "a"
                                    |> _getAttrib "href"
                            }
                        Avatar =
                            tr
                            |>  _getFirstChildElem "td"
                            |>  _getFirstChildElem "a"
                            |>  _getFirstChildElem "img"
                            |> _getAttrib "src"
                            
                        Type = 
                            tr
                            |>  _getChildElems "td"
                            |> Seq.last
                            |>  _innerTrim
                    }
                    
                parseFollowRelationship parser rootNode
                   
    
    type ProfileParser() =
        member this.ParseSingle(rootNode) =
            (this :> ISingle<Profile>).ParseSingle(rootNode)
            
        interface ISingle<Profile> with
            member this.ParseSingle(rootNode) =
                let mainNode =
                    rootNode
                    |> _getWrapperNode
                    |> _getFirstChildElement (_idPredicate "site") "div"
                    |> _getFirstChildElement (_idPredicate "default-content") "div"
                    
                let headerNode =
                    rootNode
                    |> _getWrapperNode
                    |> _getFirstChildElement (_idPredicate "js-kubrick-lead") "div"
                
                let background =
                    headerNode
                    |> _getAttrib "style"
                    |> (fun x -> x.Split("background-image: url(")[1])
                    |> (fun x -> x.TrimEnd(')'))
                    
                let avatar =
                    headerNode
                    |> _getFirstChildElement (_classPredicate "profile-header") "div"
                    |> _getFirstChildElement (_classPredicate "container") "div"
                    |> _getFirstChildElement (_classPredicate "profile-avatar") "section"
                    |> _getFirstChildElement (_classPredicate "avatar") "div"
                    |> _getFirstChildElem "img"
                    |> _getAttrib "src"
                    
                let description =
                    headerNode
                    |> _getFirstChildElement (_classPredicate "profile-header") "div"
                    |> _getFirstChildElement (_classPredicate "container") "div"
                    |> _getFirstChildElement (_classPredicate "profile-title-hold") "div"
                    |> _getFirstChildElement (_classPredicate "profile-title") "section"
                    |> _getFirstChildElement (_classPredicate "js-status-message") "h4"
                    |> _innerTrim
                    
                let username =
                    headerNode
                    |> _getFirstChildElement (_classPredicate "profile-header") "div"
                    |> _getFirstChildElement (_classPredicate "container") "div"
                    |> _getFirstChildElement (_classPredicate "profile-title-hold") "div"
                    |> _getFirstChildElement (_classPredicate "profile-title") "section"
                    |> _getFirstChildElem "h1"
                    |> _innerTrim
                    
                let stats =
                    headerNode
                    |> _getFirstChildElement (_classPredicate "profile-header") "div"
                    |> _getFirstChildElement (_classPredicate "container") "div"
                    |> _getFirstChildElement (_classPredicate "profile-title-hold") "div"
                    |> _getFirstChildElement (_classPredicate "profile-follow") "section"
                    |> _getFirstChildElem "table"
                    |> _getFirstChildElem "tr"
                    |> _getChildElems "td"
                    |> Seq.map (fun x -> x |> _innerTrim |> int)
                    
                let navItems =
                    rootNode
                    |> _getWrapperNode
                    |> _getFirstChildElement (_classPredicate "sub-nav") "nav"
                    |> _getFirstChildElement (_classPredicate "container") "div"
                    |> _getFirstChildElem "ul"
                    |> _getChildElems "li"
                    |> Seq.map _innerTrim
                    
                let navPredicate (prefix: string) (nav: string) =
                    nav.StartsWith(prefix)
                    
                let cover =
                    match
                        mainNode
                        |> _getFirstChildElement (_classPredicate "secondary-content") "aside"
                        |> _getFirstChildIfAny (_classPredicate "profile-image") "div"
                    with
                    | None -> ""
                    | Some n ->
                        n
                        |> _getFirstChildElement (_classPredicate "img") "div"
                        |> _getFirstChildElement (_classPredicate "imgflare") "a"
                        |> _getFirstChildElem "img"
                        |> _getAttrib "src"
                
                let (|About|) (asidePodNode: HtmlNode) =
                    let (|Alignment|) (s: string) =
                        match s with
                        | "Neutral" -> Alignment.Neutral
                        | "Good"    -> Alignment.Good
                        | "Evil"    -> Alignment.Evil
                        | _         -> Alignment.None
                            
                    let description =
                        match
                            asidePodNode
                            |> _getFirstChildIfAny (_classPredicate "about-me") "div"
                        with
                        | None -> ""
                        | Some n ->
                            n
                            |> (fun x -> x.InnerHtml.Trim())
                    let stats =
                        asidePodNode
                        |> _getFirstChildElem "ul"
                        |> _getChildElems "li"
                        |> Seq.map _innerTrim
                        |> Seq.map (fun x -> x.Split(":")[1])
                        |> Seq.map (fun x -> x.Trim())
                        
                    {
                        DateJoined = stats |> Seq.item 0 |> DateTime.Parse
                        Alignment = match stats |> Seq.item 1 with | Alignment ali -> ali
                        Points = stats |> Seq.item 2 |> (fun x -> x.Split(" Points")[0])|> int
                        Summary = description
                    }
                
                let about =
                    match
                        mainNode
                        |> _getFirstChildElement (_classPredicate "secondary-content") "aside"
                        |> _getFirstChildElement (_classPredicate "aside-pod") "div"
                        // |> _getFirstChildElement
                        //     (fun x -> x |> _getFirstChildIfAny (_classPredicate "about-me") "div" |> Option.isSome )
                        //     "div"
                    with
                    | About n -> n
                    
                {
                    UserName = username; Avatar = avatar; Description = description; Posts = stats |> Seq.item 0; WikiPoints = stats |> Seq.item 1
                    Following = stats |> Seq.item 2; Followers = stats |> Seq.item 3; CoverImage = cover; BackgroundImage = background; 
                    About = about; Activities = [] |> Seq.ofList; HasBlogs = navItems |> Seq.exists (navPredicate "Images"); HasImages = navItems |> Seq.exists (navPredicate "Images");
                }
             
                
                    
        
        
            
        
        