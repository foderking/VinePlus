namespace WebAPI.Models;

public class BlogPage
{
    public int? PageNo { get; set; }
    public int? TotalPages { get; set; }
    public Blog[]? Blogs { get; set; }
}

public class Blog
{
    /// <summary>
    /// Title of the blog post 
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// Link to the blog post
    /// </summary>
    public string? Link { get; set; }
    /// <summary>
    /// The date the blog post was created
    /// </summary>
    public DateTime? DateCreated { get; set; }
    /// <summary>
    /// number of comments on the blog post
    /// </summary>
    public int? Comments { get; set; }
}