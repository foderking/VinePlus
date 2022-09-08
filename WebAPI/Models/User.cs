namespace WebAPI.Models;

/// <summary>
/// A class representing a comicvine user
/// </summary>
public class User
{
    /// <summary>
    /// The name of the user
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// The Url of the users profile picture
    /// </summary>
    public string? AvatarUrl { get; set; }
    /// <summary>
    /// The title on a users profile
    /// </summary>
    public string? ProfileTitle { get; set; }
    /// <summary>
    /// Number of post a user has made on the forum
    /// </summary>
    public int ForumPosts { get; set; }
    /// <summary>
    /// Number of wiki points a user has
    /// </summary>
    public int WikiPoints { get; set; }
    /// <summary>
    /// The people the user is currently following
    /// </summary>
    public Follow? Following { get; set; }
    /// <summary>
    /// The people following the current user
    /// </summary>
    public Follow? Followers { get; set; }
    
    public string? CoverPicture { get; set; }
    public string[]? LatestImages { get; set; }
    public About? AboutMe { get; set; }
    public UserActivity[]? Activities { get; set; }
    public ListPreview[]? TopRatedLists { get; set; }
}