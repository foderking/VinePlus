namespace WebAPI.Models;

public class FollowersPage
{
    public int? PageNo { get; set; }
    public int? TotalPages { get; set; }
    public Followers[]? Followers { get; set; }
}

public class Followers: LinkPreview
{
    /// <summary>
    /// A link to the mini avatar
    /// </summary>
    public string? MiniAvatarUrl { get; set; }
}