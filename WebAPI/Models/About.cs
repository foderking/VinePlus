namespace WebAPI.Models;

public class About
{
    /// <summary>
    /// The date the user joined
    /// </summary>
    public DateTime DateJoined { get; set; }
    /// <summary>
    /// The users alignment - Good, Neutral or Evil?
    /// </summary>
    public Alignment Alignment { get; set; }
    /// <summary>
    /// How many points the user has
    /// </summary>
    public int Points { get; set; }
    /// <summary>
    /// A summary text (optional)
    /// </summary>
    public string? Summary { get; set; }
}

public enum Alignment
{
    Good, Neutral, Evil
}