using WebAPI.Database;

namespace WebAPI.Models;

public class ForumThread
{
    /// <summary>
    /// The title of the thread
    /// </summary>
    public string? ThreadTitle { get; set; }
    /// <summary>
    /// Link to the thread
    /// </summary>
    public string? ThreadLink { get; set; }
    
    /// <summary>
    /// The board the thread was posted to
    /// </summary>
    public string? BoardName { get; set; }
    /// <summary>
    /// Link to the board the thread was posted to
    /// </summary>
    public string? BoardLink { get; set; }
    
    public bool IsLocked { get; set; }
    public bool IsPinned { get; set; }
    // public bool IsAnswered { get; set; }
    
    public string? ThreadType { get; set; }
    
    /// <summary>
    /// Serial number of the most recent post on the thread
    /// </summary>
    public int MostRecentPostNo { get; set; }
    public string? MostRecentPostLink { get; set; }
    
    /// <summary>
    /// The date the thread was created
    /// </summary>
    public DateTime DateCreated { get; set; }
    
    /// <summary>
    /// The number of post to the thread
    /// </summary>
    public int NoPost { get; set; }
    
    public int NoViews { get; set; }
    
    public string? CreatorLink { get; set; }
    /////////////////////////
    public ComicvineUser? Creator { get; set; }
    public string? CreatorName { get; set; }
}

// public enum ThreadType
// {
//     Normal,
//     Poll,
//     Question,
//     Blog
// }