namespace WebAPI.Models;

public class ForumPost
{
    /// <summary>
    /// Unique Id representing a post on a thread
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The position of the post in the thread
    /// </summary>
    public int PostNo { get; set; }
    
    /// <summary>
    /// Name the first user the post is replying to
    /// </summary>
    public string? FirstMention { get; set; }
    
    /// <summary>
    /// Creator of the post
    /// </summary>
    public string CreatorName { get; set; }
    /// <summary>
    /// Link to the profile of the creator of the post
    /// </summary>
    public string CreatorLink { get; set; }
    /// <summary>
    /// Indicates whether the post has been edited
    /// </summary>
    public bool IsEdited { get; set; }
    /// <summary>
    /// Date the post was made
    /// </summary>
    public DateTime DateCreated { get; set; }
    /// <summary>
    /// Content of the post
    /// </summary>
    public string Content { get; set; }
    
    public ForumThread Thread { get; set; }
    public int ThreadId { get; set; }
    
}
