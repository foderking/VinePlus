﻿using Comicvine.Core;
using Comicvine.Database;
using Microsoft.EntityFrameworkCore;
using VinePlus.Web.Pages;

namespace VinePlus.Web.Pages;

public static class Queries
{
    public static IEnumerable<PostView> getAllPosts(ComicvineContext context, int thread_id) {
        return context
            .Posts
            .Where(p => p.ThreadId == thread_id)
            .OrderBy(p => p.PostNo)
            .Select(post =>
                new PostView(
                    post.PostNo,
                    post.Creator,
                    post.IsDeleted,
                    post.IsEdited,
                    post.Created,
                    post.Content
                )
            );
    }

    public static IEnumerable<PostView> getAllPosts(ComicvineContext context, int thread_id, int page) {
        return context
            .Posts
            .Where(p => p.ThreadId == thread_id)
            .OrderBy(p => p.PostNo)
            .Select(post =>
                new PostView(
                    post.PostNo,
                    post.Creator,
                    post.IsDeleted,
                    post.IsEdited,
                    post.Created,
                    post.Content
                )
            )
            .Skip(Util.PostsPerPage * (page - 1))
            .Take(Util.PostsPerPage);
    }

    public static IEnumerable<ThreadView> getUsersThreads(ComicvineContext context, string user, int page=1) {
        return context
            .Threads
            .Where(thread => thread.Creator.Text == user)
            .OrderByDescending(thread => thread.TotalPosts)
            .ThenBy(thread => thread.TotalView )
             .Select(thread => 
                 new ThreadView(
                     thread.TotalView,
                     thread.TotalPosts,
                     thread.Creator.Text,
                     thread.Creator.Link,
                     thread.Board.Text,
                     thread.Thread.Text,
                     thread.Thread.Link,
                     thread.IsPinned,
                     thread.IsLocked,
                     thread.LastPostNo,
                     thread.LastPostPage,
                     thread.Id
                 )
             )           
            .Skip(Util.ThreadPerPage * (page-1))
            .Take(Util.ThreadPerPage);
    }

    public static IEnumerable<PostSummary> getUserPostSummary(ComicvineContext context, string user, int page=1) {
        return context
            .Posts
            .Where(posts => posts.Creator.Text == user)
            .OrderByDescending(each => each.Created)
            .Join(
                context.Threads,
                post => post.ThreadId,
                thread => thread.Id,
                (post, thread) => 
                    new PostSummary(
                        thread.Id,
                        thread.Thread.Text,
                        post.PostNo,
                        post.Creator,
                        post.IsDeleted,
                        post.IsEdited,
                        post.Created,
                        post.Content
                    )
            )
            .Skip(Util.PostsPerPage * (page-1))
            .Take(Util.PostsPerPage);
    }

    public static IEnumerable<ThreadsSummary> getThreadsPosted(ComicvineContext context, string user, int page = 1) {
        return context
            .Posts
            .Where(posts => posts.Creator.Text == user)
            .Join(
                context.Threads,
                post => post.ThreadId,
                thread => thread.Id,
                (post, thread) => new { Thread = thread, Post = post }
            )
            .GroupBy(x => new { x.Thread.Id, x.Thread.Thread.Text })
            .Select(x => 
                new ThreadsSummary(
                    x.Key.Id, 
                    x.Key.Text, 
                    x.Count()
                )
            );
        //.OrderByDescending(x => x.no_posts)
        //.Skip(PostsPerPage * (page-1))
        //.Take(PostsPerPage);
    }

    public static IEnumerable<ThreadView> getArchivedThreads(ComicvineContext context, int page, SortForumBy sort_by = SortForumBy.DateCreated) {
        return sort_by switch
        {
            SortForumBy.DateCreated => 
                context
                    .Threads
                    .OrderByDescending(thread => thread.Id)
                    .Select(thread => 
                        new ThreadView(
                            thread.TotalView,
                            thread.TotalPosts,
                            thread.Creator.Text,
                            thread.Creator.Link,
                            thread.Board.Text,
                            thread.Thread.Text,
                            thread.Thread.Link,
                            thread.IsPinned,
                            thread.IsLocked,
                            thread.LastPostNo,
                            thread.LastPostPage,
                            thread.Id
                        )
                    )
                    .Skip(Util.ThreadPerPage * (page - 1))
                    .Take(Util.ThreadPerPage),
            SortForumBy.NoViews => 
                context
                    .Threads
                    .OrderByDescending(thread => thread.TotalView)
                    .Select(thread => 
                        new ThreadView(
                            thread.TotalView,
                            thread.TotalPosts,
                            thread.Creator.Text,
                            thread.Creator.Link,
                            thread.Board.Text,
                            thread.Thread.Text,
                            thread.Thread.Link,
                            thread.IsPinned,
                            thread.IsLocked,
                            thread.LastPostNo,
                            thread.LastPostPage,
                            thread.Id
                        )
                    )
                    .Skip(Util.ThreadPerPage * (page - 1))
                    .Take(Util.ThreadPerPage),
            SortForumBy.NoPosts => 
                context
                    .Threads
                    .OrderByDescending(thread => thread.TotalPosts)
                    .Select(thread => 
                        new ThreadView(
                            thread.TotalView,
                            thread.TotalPosts,
                            thread.Creator.Text,
                            thread.Creator.Link,
                            thread.Board.Text,
                            thread.Thread.Text,
                            thread.Thread.Link,
                            thread.IsPinned,
                            thread.IsLocked,
                            thread.LastPostNo,
                            thread.LastPostPage,
                            thread.Id
                        )
                    )
                    .Skip(Util.ThreadPerPage * (page - 1))
                    .Take(Util.ThreadPerPage),
            _ => throw new ArgumentOutOfRangeException(nameof(sort_by), sort_by, null)
        };
    }

    public static IEnumerable<ThreadView> searchThreads(ComicvineContext context, string query, int page) {
        return context
            .Threads
            .Where(thread => 
                EF.Functions
                    .ToTsVector(thread.Thread.Text)
                    .Matches(EF.Functions.WebSearchToTsQuery(query))
            )
            .OrderByDescending(thread => thread.TotalPosts)
            .Select(thread => 
                new ThreadView(
                    thread.TotalView,
                    thread.TotalPosts,
                    thread.Creator.Text,
                    thread.Creator.Link,
                    thread.Board.Text,
                    thread.Thread.Text,
                    thread.Thread.Link,
                    thread.IsPinned,
                    thread.IsLocked,
                    thread.LastPostNo,
                    thread.LastPostPage,
                    thread.Id
                )
            )
            .Skip(Util.ThreadPerPage * (page - 1))
            .Take(Util.ThreadPerPage);
    }

    public static IEnumerable<ThreadView> searchThreadsFromUser(ComicvineContext context, string query, string creator, int page) {
        return context
            .Threads
            .Where(thread => 
                EF.Functions
                    .ToTsVector(thread.Thread.Text)
                    .Matches(EF.Functions.WebSearchToTsQuery(query))
                && thread.Creator.Link.EndsWith(creator + "/")
            )
            .OrderByDescending(thread => thread.TotalPosts)
            .Select(thread => 
                new ThreadView(
                    thread.TotalView,
                    thread.TotalPosts,
                    thread.Creator.Text,
                    thread.Creator.Link,
                    thread.Board.Text,
                    thread.Thread.Text,
                    thread.Thread.Link,
                    thread.IsPinned,
                    thread.IsLocked,
                    thread.LastPostNo,
                    thread.LastPostPage,
                    thread.Id
                )
            )               
            .Skip(Util.ThreadPerPage * (page - 1))
            .Take(Util.ThreadPerPage);
    }

    public static IEnumerable<PostSummary> searchUserPosts(ComicvineContext context, string query, string user, int page) {
        return context
            .Posts
            .Where(post => 
                post.Creator.Text == user 
                && 
                EF.Functions.Like(post.Content, $"%{query}%")
            )
            .OrderByDescending(x => x.Created)
            .Join(
                context.Threads,
                post => post.ThreadId,
                thread => thread.Id,
                (post, thread) => 
                    new PostSummary(
                        thread.Id,
                        thread.Thread.Text,
                        post.PostNo,
                        post.Creator,
                        post.IsDeleted,
                        post.IsEdited,
                        post.Created,
                        post.Content
                    )
            )
            .Skip(Util.ThreadPerPage * (page - 1))
            .Take(Util.ThreadPerPage);
    }

    public static IEnumerable<Parsers.Post> getUserPosts(ComicvineContext context, string user, int page=1) {
        return context
                .Posts
                .Where(posts => posts.Creator.Text == user)
                // .OrderByDescending(each => each.Id)
                .Skip(Util.PostsPerPage * (page-1))
                .Take(Util.PostsPerPage)
            ;
    }

    public static int getThreadsMaxPage(ComicvineContext context) {
        return context.Threads.Count() / Util.ThreadPerPage + 1;
    }
}