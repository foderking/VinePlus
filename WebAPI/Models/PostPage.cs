﻿namespace WebAPI.Models;

public class PostPage
{
    public int? PageNo { get; set; }
    public int? TotalPages { get; set; }
    public ForumPost[]? ForumThreads { get; set; }
}