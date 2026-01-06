namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a playable race loaded from configuration.
/// </summary>
/// <remarks>
/// Race definitions are data-driven, allowing new races to be added
/// without code changes. Each race has stat modifiers and special traits.
/// </remarks>
public class RaceDefinition
{
    /// <summary>
    /// Unique identifier for this race.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name shown to players.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Narrative description of the race.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Lore and background information.
    /// </summary>
    public string Lore { get; init; } = string.Empty;

    /// <summary>
    /// Attribute modifiers applied when selecting this race.
    /// Keys are attribute IDs, values are modifier amounts.
    /// </summary>
    public IReadOnlyDictionary<string, int> AttributeModifiers { get; init; } =
        new Dictionary<string, int>();

    /// <summary>
    /// Special racial trait identifier.
    /// </summary>
    public string? TraitId { get; init; }

    /// <summary>
    /// Display name of the special trait.
    /// </summary>
    public string? TraitName { get; init; }

    /// <summary>
    /// Description of what the trait does.
    /// </summary>
    public string? TraitDescription { get; init; }

    /// <summary>
    /// Whether this race is available for player selection.
    /// </summary>
    public bool IsPlayable { get; init; } = true;

    /// <summary>
    /// Display order in selection lists (lower = earlier).
    /// </summary>
    public int SortOrder { get; init; } = 100;
}
