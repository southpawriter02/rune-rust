namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a player background (profession or lineage) loaded from configuration.
/// </summary>
public class BackgroundDefinition
{
    /// <summary>
    /// Unique identifier for this background.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name shown to players.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Category of background (Profession, Lineage, etc.).
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Brief description of the background.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Extended lore and narrative.
    /// </summary>
    public string Lore { get; init; } = string.Empty;

    /// <summary>
    /// Attribute bonuses from this background.
    /// </summary>
    public IReadOnlyDictionary<string, int> AttributeBonuses { get; init; } =
        new Dictionary<string, int>();

    /// <summary>
    /// Starting ability granted by this background.
    /// </summary>
    public string? StarterAbilityId { get; init; }

    /// <summary>
    /// Display name of the starter ability.
    /// </summary>
    public string? StarterAbilityName { get; init; }

    /// <summary>
    /// Description of the starter ability.
    /// </summary>
    public string? StarterAbilityDescription { get; init; }

    /// <summary>
    /// Item IDs that the player starts with.
    /// </summary>
    public IReadOnlyList<string> StartingItems { get; init; } = [];

    /// <summary>
    /// Whether this background is available for selection.
    /// </summary>
    public bool IsPlayable { get; init; } = true;

    /// <summary>
    /// Display order within category.
    /// </summary>
    public int SortOrder { get; init; } = 100;
}
