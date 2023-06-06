
namespace Comicvine.Core
open System
open System.Collections.Generic
open System.Threading.Tasks
open FSharp.Control
open HtmlAgilityPack
open Microsoft.FSharp.Core

module Parsers =
  // Threads and Posts
  type Alignment =
    | Unknown = 1
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
    
  [<CLIMutable>]
  type Post =
    {
      Id: string; IsComment: bool; IsDeleted: bool; IsModComment: bool; PostNo: int;
      Creator: Link; IsEdited: bool; Created: DateTime; Content: string; ThreadId: int
    }
    
  [<CLIMutable>]
  type Thread =
    {
      Id: int; Thread: Link; Board: Link; IsPinned: bool; IsLocked: bool; IsDeleted: bool
      Type: ThreadType; LastPostNo: int; LastPostPage: int; Created: DateTime;
      TotalPosts: int; TotalView: int; Creator: Link; Posts: Post ICollection
    }
  
  // Profile section
  type ActivityType =
    | Post   = 1
    | Image  = 2
    | Follow = 3
    | List   = 4
    
  type Activity =
    { Type: ActivityType; Date: string; }
  
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
     
  type Info =
    {
      NewThreads: int
      NewPosts: int
      DeletedPosts: int
    }
    
  /// function that can parse a page
  type 'T IParseSingle = HtmlNode -> 'T
  /// function that gets the last page no
  type IParseEnd = HtmlNode -> int
  
  // general purpose helpers
  module Helpers =
    let replace (oldS: string) (newS: string) (s: string) =
      s.Replace(oldS, newS)
      
    let splitDefault (s: string) = s.Split()
    
    let split (sep: string) (s: string) = s.Split(sep)
    
    let innerTrim (node: HtmlNode) = node.InnerText.Trim()
  
    let getAttribute (default_: string) (attrib: string) (node: HtmlNode) =
      node.GetAttributeValue(attrib, default_)
 
    let getAttrib =
      getAttribute ""
  
  module Nodes =
    type NodePredicate = HtmlNode -> bool
    // functions for selecting html nodes
    let getChildrenIfAny (name: string) (predicate: NodePredicate) (node: HtmlNode) =
      let elems =
        node.Elements name
        |> Seq.filter predicate
      if (elems |> Seq.length) < 1 then
        None
      else
        Some(elems)
  
    let getChildren (name: string) (predicate: NodePredicate) (node: HtmlNode) =
      node.Elements name
      |> Seq.filter predicate
      
    let getFirstChild (name: string) (predicate: NodePredicate) (node: HtmlNode) =
      getChildren name predicate node
      |> Seq.head
      
    let getFirstChildIfAny (name: string) (predicate: NodePredicate) (node: HtmlNode) =
      getChildrenIfAny name predicate node
      |> Option.map Seq.head
      
  module Predicates =
    let identity =
      (fun _ -> true)
  
    let idAttrib id node =
      Helpers.getAttrib "id" node = id
  
    let classAttrib class_ (node: HtmlNode) =
      node.HasClass class_
 
    let attribute attrib value node =
      Helpers.getAttrib attrib node = value
  
    let hasAttrib (attrib: string) (node: HtmlNode) =
      node.Attributes.Contains(attrib)
  
  // commonly used node functions
  module Common =
    let getWrapperNode node =
      Nodes.getFirstChild "html" Predicates.identity node
      |> Nodes.getFirstChild "body" Predicates.identity 
      |> Nodes.getFirstChild "div" (Predicates.idAttrib "site-main")
      |> Nodes.getFirstChild "div" (Predicates.idAttrib "wrapper"  )
    
    let getForumBlockNode node =
      let wrapperNode = node |> getWrapperNode
      if Option.isSome (Nodes.getFirstChildIfAny "div" (Predicates.idAttrib "forum-content") wrapperNode) then
        wrapperNode
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "forum-content")
        |> Nodes.getFirstChild "div" (Predicates.classAttrib "three-column--span-two")
        |> Some
      elif Option.isSome (Nodes.getFirstChildIfAny "div" (Predicates.classAttrib "js-toc-generate") wrapperNode) then
        wrapperNode
        |> Nodes.getFirstChild "div" (Predicates.classAttrib "js-toc-generate")
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "site")
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "forum-content")
        |> Nodes.getFirstChild "div" (Predicates.classAttrib "primary-content")
        |> Some
      else
        None
    
    let getThreadTitle node =
      node
      |> getWrapperNode
      |> Nodes.getFirstChild "section" (Predicates.classAttrib "forum-above-grid")
      |> Nodes.getFirstChild "h1" (Predicates.classAttrib "header-border")
      |> Helpers.innerTrim
    
    let parsePageEnd rootNode =
      match rootNode
        |> getForumBlockNode
        |> Option.get
        |> Nodes.getFirstChild      "div"(Predicates.classAttrib "forum-bar")
        |> Nodes.getFirstChildIfAny "ul" (Predicates.classAttrib "paginate" )
      with
      | None    -> 1
      | Some(x) ->
        x
        |> Nodes.getChildren "li" (fun node ->
          node.GetAttributeValue("class","") = "paginate__item on" ||
          node.GetAttributeValue("class","") = "paginate__item" ) //(Predicates.attribute "class" "paginate__item")
        |> Seq.map Helpers.innerTrim
        |> Seq.last
        |> int
    
    let parseBlogEnd (rootNode: HtmlNode) =
      match
        rootNode
        |> getWrapperNode
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "site")
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "default-content")
        |> Nodes.getFirstChild "div" (Predicates.classAttrib "primary-content")
        |> Nodes.getFirstChildIfAny "ul" (Predicates.classAttrib "paginate")
      with
      | None -> 1
      | Some(n) ->
        n
        |> Nodes.getChildren "li" (Predicates.attribute "class" "paginate__item")
        |> Seq.last
        |> Helpers.innerTrim
        |> int
  
    let parseFollowRelationshipEnd rootNode =
      match
        rootNode
        |> getWrapperNode
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "site")
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "default-content")
        |> Nodes.getFirstChild "div" (Predicates.classAttrib "primary-content")
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "js-sort-filter-results")
        |> Nodes.getFirstChild "div" (Predicates.classAttrib "navigation")
        |> Nodes.getFirstChildIfAny "ul" Predicates.identity
      with
      | None -> 1
      | Some x ->
        x
        |> Nodes.getChildren "li" (Predicates.attribute "class" "paginate__item")
        |> Seq.last
        |> Helpers.innerTrim
        |> int

    let parseFollowRelationship parseData rootNode =
      match
        rootNode
        |> getWrapperNode
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "site")
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "default-content")
        |> Nodes.getFirstChild "div" (Predicates.classAttrib "primary-content")
        |> Nodes.getFirstChild "div" (Predicates.idAttrib "js-sort-filter-results")
        |> Nodes.getFirstChild "table" Predicates.identity
        |> Nodes.getFirstChild "tbody" Predicates.identity
        |> Nodes.getChildrenIfAny "tr" Predicates.identity
      with
        | None -> Seq.empty
        | Some n ->
          n |> Seq.map parseData
    
    let ParseDefault(parseSingle: 'T IParseSingle)(path: string) =
      Net.getNode path
      |> Task.map parseSingle
      
    let ParseSingle(parser: 'T IParseSingle)(page: int)(path: string) =
      Net.getNodeFromPage path page
      |> Task.map parser
      
    let ParseMultiple(parseSingle)(parseEnd)(path: string) =
      let parseAll parseSingle parseEnd (path: string) (page: int) = task {
        let! node = Net.getNodeFromPage path page
        let lastPage = parseEnd node
        
        let! batchedReqs =
          seq {page..lastPage}
          |> Seq.map (Net.getNodeFromPage path)
          |> Seq.map (Task.map parseSingle)
          |> Task.WhenAll
        return
          batchedReqs
          |> Seq.concat 
      }
      
      parseAll parseSingle parseEnd path 1
      
  module ThreadParser =
    let ParseEnd: IParseEnd =
      fun n ->
        Common.parsePageEnd n
    
    let ParseSingle: Thread seq IParseSingle =
      fun rootNode ->
        let (|ThreadType|) (s: string) =
          match s with
          | "Poll"     -> ThreadType.Poll
          | "Blog"     -> ThreadType.Blog
          | "Question" -> ThreadType.Question
          | "Answered" -> ThreadType.Answered
          | _  -> ThreadType.Unknown
 
        rootNode
        |> Common.getForumBlockNode
        |> Option.get
        |> Nodes.getFirstChild "div" (Predicates.classAttrib "table-forums")
        |> Nodes.getChildren   "div" (Predicates.classAttrib "flexbox-align-stretch")
        |> Seq.map (fun flexNode ->
          let views = 
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small views hide-mobile")
            |> Helpers.innerTrim
            |> Helpers.replace "," ""
            |> int
          let posts = 
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "js-posts inner-space-small views")
            |> Helpers.innerTrim
            |> Helpers.replace "," ""
            |> int
          let lastPostNo =
            flexNode
            |> Nodes.getFirstChild "div"  (Predicates.attribute   "class" "inner-space-small last-post hide-mobile")
            |> Nodes.getFirstChild "span" (Predicates.classAttrib "info")
            |> Nodes.getFirstChild "a"    (Predicates.classAttrib "last")
            |> Helpers.getAttrib "href"
            |> Helpers.split "#"
            |> Seq.item 1
            |> Helpers.split "-"
            |> Seq.last
            |> int
          let lastPostPage =
            flexNode
            |> Nodes.getFirstChild "div"  (Predicates.attribute "class" "inner-space-small last-post hide-mobile")
            |> Nodes.getFirstChild "span" (Predicates.classAttrib "info")
            |> Nodes.getFirstChild "a"    (Predicates.classAttrib "last")
            |> Helpers.getAttrib "href"
            |> Helpers.split "#"
            |> Seq.item 0
            |> Helpers.split "="
            |> Seq.item 1
            |> int
          let created =
            flexNode
            |>  Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small author hide-laptop")
            |> Nodes.getFirstChild "span" (Predicates.classAttrib "info")
            |> Helpers.innerTrim
            |> DateTime.Parse
          let creatorName = 
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small author hide-laptop")
            |> Nodes.getFirstChild "div" Predicates.identity
            |> Nodes.getFirstChild "a"   Predicates.identity
            |> Helpers.innerTrim
          let creatorLink = 
            flexNode
            |> Nodes.getFirstChild "div"(Predicates.attribute "class" "inner-space-small author hide-laptop")
            |> Nodes.getFirstChild "div" Predicates.identity
            |> Nodes.getFirstChild "a"   Predicates.identity
            |> Helpers.getAttrib "href"
          let boardName =
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small forum-topic")
            |> Nodes.getFirstChild "a"   (Predicates.classAttrib "board")
            |> Helpers.innerTrim
          let boardLink =
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small forum-topic")
            |> Nodes.getFirstChild "a"   (Predicates.classAttrib "board")
            |> Helpers.getAttrib "href"
          let threadName =
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small forum-topic")
            |> Nodes.getFirstChild "div"  Predicates.identity
            |> Nodes.getFirstChild "a"   (Predicates.classAttrib "topic-name")
            |> Helpers.innerTrim
          let threadLink =
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small forum-topic")
            |> Nodes.getFirstChild "div" Predicates.identity
            |> Nodes.getFirstChild "a"  (Predicates.classAttrib "topic-name")
            |> Helpers.getAttrib "href"
          let threadType =
            match 
              flexNode
              |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small forum-topic")
              |> Nodes.getFirstChild "div" Predicates.identity
              |> Nodes.getFirstChildIfAny "span" (Predicates.classAttrib "type")
            with
            | None -> ThreadType.Normal
            | Some(node) ->
              match (Helpers.innerTrim node) with
                ThreadType x -> x
          let id =
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "js-posts")
            |> Nodes.getFirstChild "meta"(Predicates.classAttrib "js-post-render-topic")
            |> Helpers.getAttrib "data-post-render-value"
            |> int
          let isLocked = 
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small forum-topic")
            |> Nodes.getFirstChild "div"  Predicates.identity
            |> Nodes.getFirstChildIfAny "img" (Predicates.attribute "src" "https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-lock-16x16.png")
            |> Option.isSome
          let isPinned = 
            flexNode
            |> Nodes.getFirstChild "div" (Predicates.attribute "class" "inner-space-small forum-topic")
            |> Nodes.getFirstChild "div"  Predicates.identity
            |> Nodes.getFirstChildIfAny "img" (Predicates.attribute "src" "https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-pin-16x16.png")
            |> Option.isSome
          {
            Thread = { Text = threadName; Link = threadLink }; Board = { Text = boardName; Link = boardLink } ;
            Id = id; IsPinned = isPinned; IsLocked = isLocked; Type = threadType; LastPostNo = lastPostNo;
            LastPostPage = lastPostPage; Created = created; TotalPosts = posts; TotalView = views; IsDeleted = false;
            Creator = { Text = creatorName; Link = creatorLink }; Posts = [||]
          }
        )
        
    let ParseMultiple path =
      Common.ParseMultiple ParseSingle ParseEnd path
    
    let ParsePage page path =
      Common.ParseSingle ParseSingle page path
  
  module PostParser =
    let ParseEnd: IParseEnd =
      fun n ->
        Common.parsePageEnd n
        
    let ParseSingle: Post seq IParseSingle =
      fun rootNode ->
        let node =
          rootNode
          |> Common.getForumBlockNode
          |> Option.get
          |> Nodes.getFirstChild "div"     (Predicates.classAttrib "js-forum-block")
          |> Nodes.getFirstChild "section" (Predicates.classAttrib "forum-messages")
        let threadId =
          node
          |> Nodes.getFirstChild "meta" (Predicates.attribute "data-post-render-param" "ForumBundle.topicId")
          |> Helpers.getAttrib "data-post-render-value"
          |> int
          
        node
        |> Nodes.getChildren "div" (Predicates.classAttrib "js-message")
        |> Seq.map(fun node ->
          let messageNode =
            node
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-wrap")
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-inner")
          let edited =
            messageNode
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-title")
            |> (fun x -> x.InnerText.Contains("Edited By"))
          let created =
            match
              messageNode
              |> Nodes.getFirstChildIfAny "div" (Predicates.classAttrib "message-options")
            with
            | None -> DateTime.MinValue
            | Some mOpt ->
              mOpt
              |> Nodes.getFirstChild "time" (Predicates.classAttrib "date")
              |> Helpers.getAttrib "datetime"
              |> DateTime.Parse
          let content =
            messageNode
            |> Nodes.getFirstChild "article" (Predicates.classAttrib "message-content")
            |> (fun x -> x.InnerHtml)
          let creator =
            {
            Link =
              messageNode
              |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-title")
              |> Nodes.getFirstChild "a"   (Predicates.classAttrib "message-user")
              |> Helpers.getAttrib "data-user-profile"
            Text =
              messageNode
              |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-title")
              |> Nodes.getFirstChild "a"   (Predicates.classAttrib "message-user")
              |> Helpers.innerTrim
            }
          let isComment = 
            messageNode
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-title")
            |> Nodes.getChildren "a" (Predicates.hasAttrib "name")
            |> Seq.exists Predicates.identity
          let modComment =
              messageNode
              |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-title")
              |> Nodes.getFirstChildIfAny "span" (Predicates.classAttrib "role-mod")
              |> Option.isSome
          
          {
            IsDeleted = false; IsComment = isComment; IsEdited = edited; Creator = creator
            Created = created; Content = content; ThreadId = threadId
            Id =
              if not isComment then
                $"Tx{threadId}"
              else
                messageNode
                |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-title")
                |> Nodes.getFirstChild "a" (Predicates.hasAttrib "name")
                |> Helpers.getAttrib "name"
                |> Helpers.split "-"
                |> Seq.last
                |> int
                |> (fun id -> $"Cx{id}")
            IsModComment = modComment;
            PostNo =
              if isComment then
                messageNode
                |> Nodes.getFirstChild "div" (Predicates.classAttrib "message-title")
                |> Nodes.getFirstChild "a" (Predicates.hasAttrib "name")
                |> Helpers.innerTrim
                |> (fun x -> x[1..])
                |> int
              else
                0
          }                                   
        )
        
    let ParseMultiple path =
      Common.ParseMultiple ParseSingle ParseEnd path
      
    let ParsePage page path =
      Common.ParseSingle ParseSingle page path

  module BlogParser =
    let ParseEnd: IParseEnd =
      fun n ->
        Common.parseBlogEnd n
        
    let ParseSingle: Blog seq IParseSingle =
      fun rootNode ->
        let parse (node: HtmlNode) =
          let aNode =
            node
            |> Nodes.getFirstChild "section" (Predicates.classAttrib "news-hdr")
            |> Nodes.getFirstChild "h1" Predicates.identity
            |> Nodes.getFirstChild "a"  Predicates.identity
          let hNode =
            node
            |> Nodes.getFirstChild "section" (Predicates.classAttrib "news-hdr")
            |> Nodes.getFirstChild "h4" Predicates.identity
          {
            Blog =  { Text = Helpers.innerTrim aNode; Link = Helpers.getAttrib "href" aNode }
            Created  =
              hNode
              |> Nodes.getFirstChild "time" Predicates.identity
              |> Helpers.getAttrib "datetime"
              |> DateTime.Parse
            Comments =
              hNode
              |> Nodes.getFirstChild "a" Predicates.identity
              |> Helpers.innerTrim
              |> Helpers.splitDefault
              |> Seq.head
              |> int
            Id =
              aNode
              |> Helpers.getAttrib "href"
              |> Helpers.split "/"
              |> Seq.filter (fun x -> x.Length > 0)
              |> Seq.last
              |> int
            ThreadId =
              match
                hNode
                |> Nodes.getFirstChild "a" Predicates.identity
                |> Helpers.getAttrib "href"
                |> Helpers.split "-"
                |> Seq.last
                |> Int32.TryParse
              with
              | true,  x -> Nullable x
              | _ -> Nullable()
          }
        
        Common.getWrapperNode rootNode
        |> Nodes.getFirstChild "div"   (Predicates.idAttrib "site")
        |> Nodes.getFirstChild "div"   (Predicates.idAttrib "default-content")
        |> Nodes.getFirstChild "div"   (Predicates.classAttrib "primary-content")
        |> Nodes.getChildren "article" (Predicates.classAttrib "profile-blog")
        |> Seq.map parse
        
    let ParseMultiple path =
      Common.ParseMultiple ParseSingle ParseEnd path
      
    let ParsePage page path =
      Common.ParseSingle ParseSingle page path
      
  module ImageParser =
    let ParseSingle: Image IParseSingle =
      fun rootNode ->
        let xNode =
          rootNode
          |> Common.getWrapperNode
          |> Nodes.getFirstChild "div" (Predicates.idAttrib "site")
          |> Nodes.getFirstChild "div" (Predicates.idAttrib "gallery-content")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "primary-content")
          |> Nodes.getFirstChild "header" (Predicates.classAttrib "gallery-header")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "isotope-image")
          |> Nodes.getFirstChild "ul"  (Predicates.classAttrib "gallery-tags")
        let dataNode =
          xNode
          |> Nodes.getFirstChild "li" (Predicates.classAttrib "gallery-tags__item")
          |> Nodes.getFirstChild "a"  (Predicates.idAttrib "galleryMarker")

        {
          GalleryId = Helpers.getAttrib "data-gallery-id" dataNode
          ObjectId  = Helpers.getAttrib "data-object-id" dataNode
          Tags =
            xNode
            |> Nodes.getChildren "li" (Predicates.classAttrib "gallery-tags__item")
            |> Seq.map (fun x ->
              {
                Link = 
                  x
                  |> Nodes.getFirstChild "a" Predicates.identity
                  |> Helpers.getAttrib "href"
                Text = 
                  x
                  |> Nodes.getFirstChild "a" Predicates.identity
                  |> Helpers.innerTrim
              }
            )
          TotalImages =
            rootNode
            |> Common.getWrapperNode
            |> Nodes.getFirstChild "nav" (Predicates.classAttrib "sub-nav")
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "container")
            |> Nodes.getFirstChild "ul" Predicates.identity
            |> Nodes.getFirstChild "li" (fun x -> x.InnerText.Trim().StartsWith("Images") )
            |> Helpers.innerTrim
            |> (fun x -> x.Split("(").[1].Trim(')') )
            |> int
        }
        
    let ParseDefault path =
      Common.ParseDefault ParseSingle path
        
 
  module FollowerParser =
    let ParseEnd: IParseEnd =
        fun n ->
          Common.parseFollowRelationshipEnd n
          
    let ParseSingle: Follower seq IParseSingle =
      fun rootNode ->
        let parser tr =
          {
            Follower =
              {
              Text =
                tr
                |>  Nodes.getFirstChild "td" Predicates.identity
                |>  Nodes.getFirstChild "a"  Predicates.identity
                |> Helpers.innerTrim
              Link = 
                tr
                |>  Nodes.getFirstChild "td" Predicates.identity
                |>  Nodes.getFirstChild "a"  Predicates.identity
                |> Helpers.getAttrib "href"
              }
            Avatar =
              tr
              |>  Nodes.getFirstChild "td"  Predicates.identity
              |>  Nodes.getFirstChild "a"   Predicates.identity
              |>  Nodes.getFirstChild "img" Predicates.identity
              |> Helpers.getAttrib "src"
          }
          
        Common.parseFollowRelationship parser rootNode
    let ParseMultiple path =
      Common.ParseMultiple ParseSingle ParseEnd path
      
    let ParsePage page path =
      Common.ParseSingle ParseSingle page path
      
  module FollowingParser =
    let ParseEnd: IParseEnd =
      fun n ->
        Common.parseFollowRelationshipEnd n
    
    let ParseSingle: Following seq IParseSingle =
      fun rootNode ->
        let parser (tr: HtmlNode) =
          {
            Following =
              {
                Text =
                  tr
                  |>  Nodes.getFirstChild "td" Predicates.identity
                  |>  Nodes.getFirstChild "a"  Predicates.identity
                  |> Helpers.innerTrim
                Link = 
                  tr
                  |>  Nodes.getFirstChild "td" Predicates.identity
                  |>  Nodes.getFirstChild "a"  Predicates.identity
                  |> Helpers.getAttrib "href"
              }
            Avatar =
              tr
              |>  Nodes.getFirstChild "td"  Predicates.identity
              |>  Nodes.getFirstChild "a"   Predicates.identity
              |>  Nodes.getFirstChild "img" Predicates.identity
              |> Helpers.getAttrib "src"
            Type = 
              tr
              |>  Nodes.getChildren "td" Predicates.identity
              |>  Seq.last
              |>  Helpers.innerTrim
            }
          
        Common.parseFollowRelationship parser rootNode
        
    let ParseMultiple path =
      Common.ParseMultiple ParseSingle ParseEnd path
      
    let ParsePage page path =
      Common.ParseSingle ParseSingle page path
      
  module ProfileParser =
    let ParseSingle: Profile IParseSingle =
      fun rootNode ->
        let mainNode =
          rootNode
          |> Common.getWrapperNode
          |> Nodes.getFirstChild "div" (Predicates.idAttrib "site")
          |> Nodes.getFirstChild "div" (Predicates.idAttrib "default-content")
        let headerNode =
          rootNode
          |> Common.getWrapperNode
          |> Nodes.getFirstChild "div" (Predicates.idAttrib "js-kubrick-lead")
        let background =
          headerNode
          |> Helpers.getAttrib "style"
          |> Helpers.split "background-image: url("
          |> Seq.item 1
          |> (fun x -> x.TrimEnd(')'))
        let avatar =
          headerNode
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "profile-header")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "container")
          |> Nodes.getFirstChild "section" (Predicates.classAttrib "profile-avatar")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "avatar")
          |> Nodes.getFirstChild "img" Predicates.identity
          |> Helpers.getAttrib "src"
        let description =
          headerNode
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "profile-header")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "container")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "profile-title-hold")
          |> Nodes.getFirstChild "section" (Predicates.classAttrib "profile-title")
          |> Nodes.getFirstChild "h4" (Predicates.classAttrib "js-status-message")
          |> Helpers.innerTrim
        let username =
          headerNode
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "profile-header")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "container")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "profile-title-hold")
          |> Nodes.getFirstChild "section" (Predicates.classAttrib "profile-title")
          |> Nodes.getFirstChild "h1" Predicates.identity
          |> Helpers.innerTrim
        let stats =
          headerNode
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "profile-header")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "container")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "profile-title-hold")
          |> Nodes.getFirstChild "section" (Predicates.classAttrib "profile-follow")
          |> Nodes.getFirstChild "table" Predicates.identity
          |> Nodes.getFirstChild "tr" Predicates.identity
          |> Nodes.getChildren "td" Predicates.identity
          |> Seq.map (fun x -> x |> Helpers.innerTrim |> int)
        let navItems =
          rootNode
          |> Common.getWrapperNode
          |> Nodes.getFirstChild "nav" (Predicates.classAttrib "sub-nav")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "container")
          |> Nodes.getFirstChild "ul" Predicates.identity
          |> Nodes.getChildren "li" Predicates.identity
          |> Seq.map Helpers.innerTrim
        let navPredicate (prefix: string) (nav: string) =
          nav.StartsWith(prefix)
        
        let cover =
          match
            mainNode
            |> Nodes.getFirstChild "aside" (Predicates.classAttrib "secondary-content")
            |> Nodes.getFirstChildIfAny "div" (Predicates.classAttrib "profile-image")
          with
          | None -> ""
          | Some n ->
            n
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "img")
            |> Nodes.getFirstChild "a" (Predicates.classAttrib "imgflare")
            |> Nodes.getFirstChild "img" Predicates.identity
            |> Helpers.getAttrib "src"
          
        let (|About|) (asidePodNode: HtmlNode) =
          let (|Alignment|) (s: string) =
            match s with
            | "Neutral" -> Alignment.Neutral
            | "Good"    -> Alignment.Good
            | "Evil"    -> Alignment.Evil
            | _         -> Alignment.Unknown
          
          let description =
            match
              asidePodNode
              |> Nodes.getFirstChildIfAny "div" (Predicates.classAttrib "about-me")
            with
            | None -> ""
            | Some n ->
              n
              |> (fun x -> x.InnerHtml.Trim())
          let stats =
            asidePodNode
            |> Nodes.getFirstChild "ul" Predicates.identity
            |> Nodes.getChildren "li" Predicates.identity
            |> Seq.map Helpers.innerTrim
            |> Seq.map (Helpers.split ":")
            |> Seq.map (Seq.item 1)
            |> Seq.map (fun x -> x.Trim())
            
          {
            DateJoined =
              stats
              |> Seq.item 0
              |> DateTime.Parse
            Alignment =
              match stats |> Seq.item 1
              with
              | Alignment ali -> ali
            Points =
              stats
              |> Seq.item 2
              |> Helpers.split " Points"
              |> Seq.item 0
              |> int
            Summary = description
          }
          
        let (|Activity|)(activityItem: HtmlNode) =
          let node =
            activityItem
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "media")
            |> Nodes.getFirstChild "div" (Predicates.classAttrib "media-body")
            |> Nodes.getFirstChild "span"(Predicates.classAttrib "activity-message")
          let date =
            node
            |> Nodes.getFirstChild "time" (Predicates.classAttrib "activity-time")
            |> Helpers.innerTrim
          node
          |> Nodes.getFirstChild "i" (Predicates.identity)
          |> Nodes.getFirstChild "svg" (Predicates.identity)
          |> Helpers.getAttrib "class"
          |> (fun cls ->
            match cls with
            | "symbol symbol-picture" ->
              { Type = ActivityType.Image; Date = date }
            | "symbol symbol-comments-alt" ->
              { Type = ActivityType.Post; Date = date }
            | "symbol symbol-smile" ->
              { Type = ActivityType.Follow; Date = date }
            | "symbol symbol-star" ->
              { Type = ActivityType.List; Date = date }
            | x ->
              failwith x
          )
          
        let about =
          match
            mainNode
            |> Nodes.getFirstChild "aside" (Predicates.classAttrib "secondary-content")
            |> Nodes.getFirstChild "div"   (Predicates.classAttrib "aside-pod")
          with
          | About n -> n
          
        let activities =
          mainNode
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "primary-content")
          |> Nodes.getFirstChild "div" (Predicates.classAttrib "tab-content")
          |> Nodes.getFirstChild "div" (Predicates.idAttrib "js-user-main-feed")
          |> Nodes.getFirstChild "ul"  (Predicates.classAttrib "activity-list")
          |> Nodes.getChildrenIfAny "li" (Predicates.classAttrib "activity-list__item")
          |> Option.map (Seq.map (fun node ->
            match node with
            | Activity act -> act
          ))
          |> Option.defaultValue Seq.empty
          
        {
          UserName = username; Avatar = avatar; Description = description; Posts = stats |> Seq.item 0; WikiPoints = stats |> Seq.item 1
          Following = stats |> Seq.item 2; Followers = stats |> Seq.item 3; CoverImage = cover; BackgroundImage = background; 
          About = about; Activities = activities; HasBlogs = navItems |> Seq.exists (navPredicate "Images"); HasImages = navItems |> Seq.exists (navPredicate "Images");
        }
        
    let ParseDefault path =
      Common.ParseDefault ParseSingle path