namespace WebAPI.Models;

public class ForumPage
{
    public int? PageNo { get; set; }
    public int? TotalPages { get; set; }
    public ForumThread[]? ForumThreads { get; set; }
}