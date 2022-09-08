namespace WebAPI.Models;

public class Follow
{
    public int Number { get; set; }
    
    public string? Link { get; set; }

    public bool Equals(Follow other) {
        return Equals(other.Link, Link) && other.Number == Number;
    }
}