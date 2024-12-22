using System.Text.Json.Serialization;
using Comicvine.Core;

namespace VinePlus.Web.Pages;

public static class Util
{
    public static int ThreadPerPage = 50;
    public static int PostsPerPage = 50;
}

public enum SortForumBy
{
    DateCreated, // sorts by id
    NoViews,
    NoPosts
}

public static class MainHighlight
{
    public const string Vine = "vine";
    public const string Archives = "archives";
    public const string Search = "search";
    public const string Stats = "stats";
}

public static class Keys
{
    public const string Highlight = "highlight";
    public const string Headings = "headings";
    public const string Title = "title";
    public const string ProfileName = "profile-name";
    public const string ProfileHasBlog = "profile-blog";
    public const string ProfileHasImage = "profile-image";
}

public static class ProfileHighlight
{
    public const string Main = "profile";
    public const string Images = "images";
    public const string Blogs = "blogs";
    public const string Threads = "threads";
    public const string Posts = "posts";
}

public record Nav(int CurrentPage, int LastPage, string DelegateParam);

public record ThreadHeading(string thread_title, string thread_link);

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

public record ImageData(
    [property: JsonPropertyName("dateCreated")]
    string date_created,
    [property: JsonPropertyName("gallery")]
    string gallery,
    [property: JsonPropertyName("original")]
    string original
);

public record ImageResponse(
    [property: JsonPropertyName("images")] IEnumerable<ImageData> images
);