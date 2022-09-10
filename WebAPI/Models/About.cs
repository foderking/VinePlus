using System.ComponentModel;

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

    public bool Equals(About other) {
        Console.WriteLine(other);
        return Alignment.Equals(other.Alignment)
               && Points == other.Points
               && (Summary != null && Summary.Equals(other.Summary) || Summary == null && other.Summary == null)
               && DateJoined.Equals(other.DateJoined);
    }
}

public enum Alignment
{
    None,
    Good,
    Neutral,
    Evil,
}