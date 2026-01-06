namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a player attribute loaded from configuration.
/// </summary>
public class AttributeDefinition
{
    /// <summary>
    /// Unique identifier (e.g., "might", "fortitude").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name shown to players.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Three-letter abbreviation (e.g., "MIG", "FOR").
    /// </summary>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// Description of what this attribute affects.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// List of primary effects this attribute influences.
    /// </summary>
    public IReadOnlyList<string> PrimaryEffects { get; init; } = [];

    /// <summary>
    /// Display order in attribute lists.
    /// </summary>
    public int SortOrder { get; init; } = 100;

    /// <summary>
    /// Associated color for UI display (hex code).
    /// </summary>
    public string Color { get; init; } = "#FFFFFF";
}
