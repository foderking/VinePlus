using Comicvine.Core;
using VinePlus.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages.Search.Posts;

public class Results(ComicvineContext context) : Pagination<PostSummary>
{
    public void OnGet(bool searchPost, string query, string creator, int p) {
        string s_query = $"searchPost={searchPost}&query={query}" + (creator==null ? "" : "&creator=takenstew22");
        NavRecord = new(p, int.MaxValue, s_query);
        Entities = Queries.searchUserPosts(context, query, creator, p)
            .Select(entity => 
                entity with { post_content = entity.post_content.Replace(query, $"<span class=\"search-highlight\">{query}</span>")}
            );
            
    }

    public override Func<string, int, string> PageDelegate() {
        return (s_query, page) => $"/search/posts/results?{s_query}&p={page}";
    }
}