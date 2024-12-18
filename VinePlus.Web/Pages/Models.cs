using Comicvine.Core;

namespace ComicVine.API.Pages;

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