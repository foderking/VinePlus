﻿using Comicvine.Core;
using Comicvine.Database;

namespace VinePlus.Web.Pages.Profile;

public class Posts : Navigator<PostSummary>
{
    public string UserName = "";
    private ComicvineContext _context;

    public Posts(ComicvineContext ctx) {
        _context = ctx;
    }
    
    public void OnGet(string user, int p) {
        UserName = user;
        Entities = Queries.getUserPostSummary(_context, user, p);
        NavRecord = new(p, 1000, user);
    }

    public string LinkToPost(Parsers.Post post, Parsers.Thread thread) {
        return $"/archives/thread/{thread.Id}#{post.PostNo}";
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/posts/{user}/{page}";
    }
}