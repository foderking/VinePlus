namespace WebAPI.Models;

public class UserActivity
{
    public ActivityType Type { get; set; }
    public DateTime ActivityTime { get; set; }
    
    public MessageActivity? MessageActivity { get; set; }
    public ImageActivity? ImageActivity { get; set; }
    
}

public enum ActivityType
{
    Message, Image
}

public class ImageActivity
{
    public string? ImageUrl {get; set; }
}

public class MessageActivity
{
    public string? MessageText { get; set; }
    public LinkPreview? TargetTopic { get; set; }
    public LinkPreview? TargetForum { get; set; }
}