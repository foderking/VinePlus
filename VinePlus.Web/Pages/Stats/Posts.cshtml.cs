﻿using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages.Stats;

public class Posts(ComicvineContext context): Navigator<ThreadsSummary>
{
    public void OnGet(string user, int p) {
        Entities = Queries.getThreadsPosted(context, user, p);
        NavRecord = new(p, 1000, user);
    }
    
    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/threads/{user}/{page}";
    }
}