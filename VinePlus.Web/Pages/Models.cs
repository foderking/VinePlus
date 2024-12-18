using Comicvine.Core;

namespace VinePlus.Web.Pages;

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