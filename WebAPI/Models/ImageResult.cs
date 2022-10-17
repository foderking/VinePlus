namespace WebAPI.Models;

public class ImageResult
{    
    public object[]? images { get; set; }
    public string? start { get; set; }
    public int count { get; set; }
    public int total { get; set; }
}