namespace ComicVine.API.Models;

public class PostPage
{
    public int PageNo { get; set; }
    public int TotalPages { get; set; }
    public ForumPost[] ForumPosts { get; set; } = Array.Empty<ForumPost>();
}