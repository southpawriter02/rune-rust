namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a reusable room template loaded from JSON data.
/// Templates define the structure for procedurally generated rooms with variable substitution.
/// </summary>
public class RoomTemplate
{
    /// <summary>
    /// Gets or sets the unique identifier for this template (database primary key).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the template identifier (e.g., "reactor_core", "collapsed_entry_hall").
    /// Must be unique across all templates.
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the biome identifier this template belongs to (e.g., "the_roots").
    /// </summary>
    public string BiomeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the room size classification (Small, Medium, Large).
    /// Affects spawn rules (OnlyInLargeRooms, etc.).
    /// </summary>
    public string Size { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the architectural archetype (Corridor, Chamber, BossArena, EntryHall, SecretRoom).
    /// Used for layout generation and spawn rule filtering.
    /// </summary>
    public string Archetype { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of name templates with {Adjective} tokens for substitution.
    /// Example: ["The {Adjective} Reactor Core", "The {Adjective} Power Nexus"]
    /// </summary>
    public List<string> NameTemplates { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of adjectives for {Adjective} token substitution.
    /// Example: ["Pulsing", "Unstable", "Critical"]
    /// </summary>
    public List<string> Adjectives { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of description templates with {Adjective} and {Detail} tokens.
    /// Example: ["A {Adjective} reactor core dominates this space. {Detail}."]
    /// </summary>
    public List<string> DescriptionTemplates { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of detail sentences for {Detail} token substitution.
    /// Example: ["The reactor core pulses with blinding runic light"]
    /// </summary>
    public List<string> Details { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of valid archetype connections for layout generation.
    /// Example: ["Corridor", "Chamber"]
    /// </summary>
    public List<string> ValidConnections { get; set; } = new();

    /// <summary>
    /// Gets or sets the metadata tags for filtering and categorization.
    /// Example: ["Boss", "Combat", "Hazard", "Power"]
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum number of connection points (exits) this room requires.
    /// </summary>
    public int MinConnectionPoints { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of connection points (exits) this room can have.
    /// </summary>
    public int MaxConnectionPoints { get; set; }

    /// <summary>
    /// Gets or sets the difficulty rating (Easy, Medium, Hard).
    /// Maps to DangerLevel enum during room instantiation.
    /// </summary>
    public string Difficulty { get; set; } = string.Empty;
}
