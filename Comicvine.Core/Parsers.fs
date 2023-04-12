
namespace Comicvine.Core
open System
open HtmlAgilityPack
open Microsoft.FSharp.Core

module Parsers =
    type CommentContent = string
    type OPContent = string
    
    type Alignment =
        | None    = 1
        | Good    = 2
        | Neutral = 3
        | Evil    = 4
    type ThreadType =
        | Normal = 1
        | Poll   = 2
        | Blog   = 3
        
    type Link =
        { Text: string; Link: string }
    type Blog =
        { Blog: Link; Created: DateTime; Comments: int }
    type About =
        { DateJoined: DateTime;  Alignment: Alignment; Points: int; Summary: string }
    type FollowRelationship =
        { Name: string; Link: string; AvatarUrl: string }
    type ThreadComments =
        { Id: int; PostNo: int; Creator: Link; IsEdited: bool
          Created: DateTime; Content: CommentContent; ThreadId: int }
    type ThreadOP =
        { Creator: Link; IsEdited: bool; Created: DateTime; Content: OPContent; ThreadId: int }
    type ThreadPost =
        | Comment of ThreadComments
        | OP      of ThreadOP
    type ThreadOverview =
        { Id: int; Thread: Link; Board: Link; IsPinned: bool; IsLocked: bool;
          Type: ThreadType; LastPostNo: int; LastPostPage: int; Created: DateTime;
          TotalPosts: int; TotalView: int; Creator: Link; Comments: seq<ThreadComments> }
    type Activity =
        | Comment of {| Content: string; Topic: Link; Forum: Link |}
        | Image   of {| Url: string |}
        | Follow  of {| User: Link |}
    type Profile =
        { UserName: string; Avatar: string; Description: string; Posts: int; WikiPoints: int
          Following: Link; Followers: Link; Cover: string; Background: string; About: About; Activities: Activity }
    type Image = obj
    
    let _innerT (node: HtmlNode) = node.InnerText
    let _trim (s: string) = s.Trim()
    let _split (s: string) = s.Split()
    
    let _getAttribute (def: string) (attrib: string) (node: HtmlNode) =
        node.GetAttributeValue(attrib, def)
       
    let _getAttrib =
        _getAttribute ""
    
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
        
    let _getFirstChildElement predicate name =
        _getChildElements predicate name >> Seq.head
    
    let _getFirstChildElem =
        _getFirstChildElement (fun _ -> true)
        
    let _idPredicate id (node: HtmlNode) =
        _getAttrib "id" node = id
       
    let _classPredicate cls (node: HtmlNode) =
        node.HasClass cls
 
    let getWrapperNode node =
        _getFirstChildElem "html" node
        |> _getFirstChildElem "body"
        |> _getFirstChildElement (_idPredicate "site-main") "div"
        |> _getFirstChildElement (_idPredicate "wrapper"  ) "div"
        
    
        
    
    
    type Page<'T> =
        { PageNo: int; TotalPages: int; Data: 'T }
    
    type IParsable<'T> =
        abstract member ParseData: HtmlNode -> 'T
        
        
    let ParseBlog (rootNode: HtmlNode) =
        let parse (node: HtmlNode) =
            let aNode = node
                        |> _getFirstChildElement (_classPredicate "news-hdr") "section"
                        |> _getFirstChildElem "h1"
                        |> _getFirstChildElem "a"
            let hNode = node
                        |> _getFirstChildElement (_classPredicate "news-hdr") "section"
                        |> _getFirstChildElem "h4"
            {
                Blog =  { Text = aNode.InnerText.Trim(); Link = _getAttrib "href" aNode }
                Created  = hNode
                           |> _getFirstChildElem "time"
                           |> _getAttrib "datetime"
                           |> DateTime.Parse
                Comments = hNode
                           |> _getFirstChildElem "a"
                           |> _innerT
                           |> _trim
                           |> _split
                           |> Seq.head
                           |> int
            }
            
        getWrapperNode rootNode
        |> _getFirstChildElement (_idPredicate "site") "div"
        |> _getFirstChildElement (_idPredicate "default-content") "div"
        |> _getFirstChildElement (_classPredicate "primary-content") "div"
        |> _getChildElements (_classPredicate "profile-blog") "article"
        |> Seq.map parse
   
    let ParseComments threadId (rootNode: HtmlNode) =
        let getForumBlockNode wrapperNode =
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
        
        match rootNode |> getWrapperNode |> getForumBlockNode with
              | Some(node) -> node
              | None -> raise (Exception("Unknown thread type"))
        |> _getFirstChildElement (_classPredicate "js-forum-block") "div"
        |> _getFirstChildElement (_classPredicate "forum-messages") "section"
        |> _getChildElements (_classPredicate "js-message") "div"
        |> Seq.map
                ( fun node ->
                    let messageNode = node
                                     |> _getFirstChildElement (_classPredicate "message-wrap") "div"
                                     |> _getFirstChildElement (_classPredicate "message-inner") "div"
                                     
                    if messageNode
                             |> _getFirstChildElement (_classPredicate "message-title") "div"
                             |> _getChildElements (fun n -> n.Attributes.Contains("name")) "a"
                             |> Seq.exists (fun _ -> true)
                    then
                        {
                            Id = messageNode
                                 |> _getFirstChildElement (_classPredicate "message-title") "div"
                                 |> _getFirstChildElement (fun n -> n.Attributes.Contains("name")) "a"
                                 |> _getAttrib "name"
                                 |> (fun x -> x.Split("-")[x.Length - 1])
                                 |> int
                            PostNo = messageNode
                                     |> _getFirstChildElement (_classPredicate "message-title") "div"
                                     |> _getFirstChildElement (fun n -> n.Attributes.Contains("name")) "a"
                                     |> (fun x -> x.InnerText.Trim()[1..])
                                     |> int
                            IsEdited = messageNode
                                     |> _getFirstChildElement (_classPredicate "message-title") "div"
                                     |> (fun x -> x.InnerText.Contains("Edited By"))
                        
                            Creator = {
                                Link = messageNode
                                        |> _getFirstChildElement (_classPredicate "message-title") "div"
                                        |> _getFirstChildElement (_classPredicate "message-user") "a"
                                        |> _getAttrib "data-user-profile"
                                Text = messageNode
                                        |> _getFirstChildElement (_classPredicate "message-title") "div"
                                        |> _getFirstChildElement (_classPredicate "message-user") "a"
                                        |> (fun x -> x.InnerText.Trim())
                            }
                            Created = messageNode
                                      |> _getFirstChildElement (_classPredicate "message-options") "div"
                                      |> _getFirstChildElement (_classPredicate "date") "div"
                                      |> _getAttrib "datetime"
                                      |> DateTime.Parse
                            Content = messageNode
                                      |> _getFirstChildElement (_classPredicate "message-title") "div"
                                      |> _getFirstChildElement (_classPredicate "message-content") "article"
                                      |> (fun x -> x.InnerHtml)
                            ThreadId = threadId
                        }
                        |> ThreadPost.Comment
                    else
                        // {
                        //     IsEdited = messageNode
                        //              |> _getFirstChildElement (_classPredicate "message-title") "div"
                        //              |> (fun x -> x.InnerText.Contains("Edited By"))
                        //
                        //     Creator = {
                        //         Link = messageNode
                        //                 |> _getFirstChildElement (_classPredicate "message-title") "div"
                        //                 |> _getFirstChildElement (_classPredicate "message-user") "a"
                        //                 |> _getAttrib "data-user-profile"
                        //         Text = messageNode
                        //                 |> _getFirstChildElement (_classPredicate "message-title") "div"
                        //                 |> _getFirstChildElement (_classPredicate "message-user") "a"
                        //                 |> (fun x -> x.InnerText.Trim())
                        //     }
                        //     Created = messageNode
                        //               |> _getFirstChildElement (_classPredicate "message-options") "div"
                        //               |> _getFirstChildElement (_classPredicate "date") "div"
                        //               |> _getAttrib "datetime"
                        //               |> DateTime.Parse
                        //     Content = messageNode
                        //               |> _getFirstChildElement (_classPredicate "message-title") "div"
                        //               |> _getFirstChildElement (_classPredicate "message-content") "article"
                        //               |> (fun x -> x.InnerHtml)
                        //     ThreadId = threadId
                        // }
                        
                        { ThreadId = threadId; Content = ""; Created = DateTime.Now; IsEdited = true; Creator = {Text =""; Link=""} }
                        |> ThreadPost.OP
                )