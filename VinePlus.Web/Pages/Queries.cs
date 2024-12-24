using Comicvine.Core;
using VinePlus.Database;
using Microsoft.EntityFrameworkCore;
using VinePlus.Web.Pages;

namespace VinePlus.Web.Pages;

public static class Queries
{
    public static ThreadHeading getThreadTitle(ComicvineContext context, int thread_id) {
        var thread = context
            .Threads
            .Where(thread => thread.Id == thread_id)
            .Select(thread => new { title = thread.Thread.Text, link = thread.Thread.Link })
            .FirstOrDefault();
        return thread switch
        {
            null => new ThreadHeading("", ""),
            { } t => new ThreadHeading(t.title, t.link)
        };
    }
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

    public static int getPostsMaxPage(ComicvineContext context, int thread_id) {
        int count = context
            .Posts
            .Count(p => p.ThreadId == thread_id);
        return count / Util.PostsPerPage + 1;
    }
        
    public static int getThreadsMaxPage(ComicvineContext context) {
        return context.Threads.Count() / Util.ThreadPerPage + 1;
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
                new
                {
                    x.Key.Id, 
                    x.Key.Text, 
                    Count = x.Count()
                    
                }
            )
            .OrderByDescending(x => x.Count)
            .Skip(Util.PostsPerPage * (page-1))
            .Take(Util.PostsPerPage)
            .Select(e => new ThreadsSummary(e.Id, e.Text, e.Count));
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
            .Take(Util.ThreadPerPage)
            .ToList();
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

    public static IEnumerable<ProfilePostView> getPostViews(ComicvineContext context) {
        return context
            .Posts
            .GroupBy(post => post.Creator.Text)
            .OrderByDescending(group => group.Count())
            .Take(10)
            .Select(x => new ProfilePostView(x.Key, x.Count()));
    }

}