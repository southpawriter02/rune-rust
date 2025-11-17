namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.1: Represents a game world
/// Corresponds to database Worlds table
/// </summary>
public class World
{
    public int WorldId { get; set; }
    public string WorldName { get; set; } = string.Empty;
    public string WorldDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
