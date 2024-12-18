﻿using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Search.Posts;

public class Results(ComicvineContext context) : Navigator<Util.PostWithThread>
{
    public void OnGet(bool searchPost, string query, string creator, int p) {
        string s_query = $"searchPost={searchPost}&query={query}" + (creator==null ? "" : "&creator=takenstew22");
        NavRecord = new(p, int.MaxValue, s_query);
        Entities = Util.Search.searchUserPosts(context, query, creator, p);
    }

    public override Func<string, int, string> PageDelegate() {
        return (s_query, page) => $"/search/posts/results?{s_query}&p={page}";
    }
}