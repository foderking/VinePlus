using Comicvine.Core;

namespace VinePlus.Web.Pages;

public record ThreadsSummary(int thread_id, string thread_text, int no_posts);

public record PostSummary(
    int thread_id,
    string thread_text,
    int post_no,
    Parsers.Link post_creator,
    bool post_is_deleted,
    bool post_is_edited,
    DateTime post_created,
    string post_content
);

public record PostView(
    int post_no,
    Parsers.Link post_creator,
    bool post_is_deleted,
    bool post_is_edited,
    DateTime post_created,
    string post_content
)
{
    public static PostView create(Parsers.Post post) {
        return new PostView(
            post.PostNo,
            post.Creator,
            post.IsDeleted,
            post.IsEdited,
            post.Created,
            post.Content
        );
    }
};

public record ThreadView(
    int total_views,
    int total_posts,
    string creator,
    string creator_link,
    string board,
    string thread_name,
    string thread_link,
    bool is_pinned,
    bool is_locked,
    int last_post_no,
    int last_post_page,
    int thread_id
)
{
    public static ThreadView create(Parsers.Thread thread) {
        return new ThreadView(
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
        );
    }
};