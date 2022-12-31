using System.ComponentModel.DataAnnotations;

namespace WebAPI.Database;

public class ComicvineUser
{
    [Key]
    public string GalleryId { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
}
