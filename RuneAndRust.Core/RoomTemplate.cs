using RuneAndRust.Core.Descriptors;

namespace RuneAndRust.Core;

/// <summary>
/// Represents the relative size of a room (v0.10)
/// Affects description flavor, not game mechanics
/// </summary>
public enum RoomSize
{
    Small,      // Cramped, claustrophobic
    Medium,     // Standard room size
    Large       // Vast, echoing spaces
}

/// <summary>
/// Represents the difficulty tier for encounter balancing (v0.10)
/// Used for template selection based on dungeon depth
/// </summary>
public enum RoomDifficulty
{
    Easy,       // Early game, simple layouts
    Medium,     // Mid-game challenge
    Hard        // Late game, complex encounters
}

/// <summary>
/// Blueprint for procedurally generating room instances (v0.10)
/// Contains variation points for names, descriptions, and connections
/// </summary>
public class RoomTemplate
{
    // Identity
    public string TemplateId { get; set; } = string.Empty;
    public string Biome { get; set; } = "the_roots"; // Default biome
    public RoomSize Size { get; set; } = RoomSize.Medium;
    public RoomArchetype Archetype { get; set; } = RoomArchetype.Chamber;

    // Name Generation
    public List<string> NameTemplates { get; set; } = new();
    public List<string> Adjectives { get; set; } = new();

    // Description Generation
    public List<string> DescriptionTemplates { get; set; } = new();
    public List<string> Details { get; set; } = new();

    // Connection Rules
    public int MinConnectionPoints { get; set; } = 1;
    public int MaxConnectionPoints { get; set; } = 4;
    public List<RoomArchetype> ValidConnections { get; set; } = new();

    // Metadata
    public RoomDifficulty Difficulty { get; set; } = RoomDifficulty.Easy;
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Validates that the template has sufficient variation data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(TemplateId)) return false;
        if (NameTemplates.Count == 0) return false;
        if (DescriptionTemplates.Count == 0) return false;
        if (Adjectives.Count == 0) return false;
        if (Details.Count == 0) return false;
        if (ValidConnections.Count == 0) return false;

        return true;
    }

    /// <summary>
    /// Checks if this template can connect to a given archetype
    /// </summary>
    public bool CanConnectTo(RoomArchetype targetArchetype)
    {
        return ValidConnections.Contains(targetArchetype);
    }

    /// <summary>
    /// Gets the number of available connection slots for this template
    /// </summary>
    public int GetAvailableConnectionSlots()
    {
        return MaxConnectionPoints - MinConnectionPoints;
    }
}
