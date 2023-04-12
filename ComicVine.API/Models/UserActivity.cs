namespace ComicVine.API.Models;

public class UserActivity
{
    public ActivityType Type { get; set; }
    public string? ActivityTime { get; set; }
    
    public CommentActivity? CommentActivity { get; set; }
    public ImageActivity? ImageActivity { get; set; }
    public FollowActivity? FollowActivity { get; set; }
}

public enum ActivityType
{
    Comment, Image, Follow
}

public class FollowActivity
{
    public LinkPreview? User { get; set; }
}
    

public class ImageActivity
{
    public string? ImageUrl {get; set; }
}

public class CommentActivity
{
    public string? Content { get; set; }
    public LinkPreview? Topic { get; set; }
    public LinkPreview? Forum { get; set; }
}