using System.ComponentModel.DataAnnotations;

namespace ComicVine.API.Database;

public class ComicvineUser
{
    public ComicvineUser(string name, string galleryId, string link) {
        Name = name;
        GalleryId = galleryId;
        Link = link;
    }

    [Key]
    public string Name { get; set; }
    public string GalleryId { get; set; }
    public string Link { get; set; }
}
