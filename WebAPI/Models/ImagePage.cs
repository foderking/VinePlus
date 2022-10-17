namespace WebAPI.Models;

public class ImagePage
{
    /// <summary>
    /// Page number, starting from 0
    /// </summary>
    public int? PageIndex { get; set; }
    /// <summary>
    /// Total numbr of pages
    /// </summary>
    public int? TotalPages { get; set; }
    /// <summary>
    /// All the images in the page
    /// </summary>
    public object[]? Images { get; set; }
}

public class Image
{
    
}