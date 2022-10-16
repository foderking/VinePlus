namespace WebAPI.Models;

public class FollowingPage
{
    public int? PageNo { get; set; }
    public int? TotalPages { get; set; }
    public Following[]? Followings { get; set; }
}
public class Following: LinkPreview
{
    /// <summary>
    /// The type of entity being followed.
    /// Could be a comicvine user, a fictional character etc
    /// </summary>
    public string? Type { get; set; }
    /// <summary>
    /// A link to the mini avatar
    /// </summary>
    public string? MiniAvatarUrl { get; set; }

}