using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Represents a specialized player class derived from an archetype.
/// </summary>
/// <remarks>
/// Classes define specific stat modifiers, growth rates, resource types,
/// and starting abilities. They are loaded from configuration and should
/// not be instantiated directly.
/// </remarks>
public class ClassDefinition
{
    /// <summary>
    /// Gets the unique identifier for this class.
    /// </summary>
    /// <example>"shieldmaiden", "galdr-caster", "shadow-walker"</example>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of this class.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed description of this class.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the ID of the archetype this class belongs to.
    /// </summary>
    public string ArchetypeId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the stat modifiers applied when selecting this class.
    /// </summary>
    /// <remarks>
    /// These modifiers are added to the player's base stats.
    /// Can be positive or negative.
    /// </remarks>
    public StatModifiers StatModifiers { get; init; }

    /// <summary>
    /// Gets the stat growth rates per level for this class.
    /// </summary>
    /// <remarks>
    /// These values determine how much each stat increases on level up.
    /// Used in v0.0.8 (Experience & Leveling).
    /// </remarks>
    public StatModifiers GrowthRates { get; init; }

    /// <summary>
    /// Gets the requirements to select this class.
    /// </summary>
    /// <remarks>
    /// Null or empty requirements mean any character can select this class.
    /// </remarks>
    public ClassRequirements? Requirements { get; init; }

    /// <summary>
    /// Gets the ID of the primary resource type for this class.
    /// </summary>
    /// <remarks>
    /// References a ResourceTypeDefinition from v0.0.4b.
    /// Example: "mana", "rage", "energy", "faith", "focus"
    /// </remarks>
    public string PrimaryResourceId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the IDs of abilities this class starts with.
    /// </summary>
    /// <remarks>
    /// References AbilityDefinitions from v0.0.4c.
    /// </remarks>
    public IReadOnlyList<string> StartingAbilityIds { get; init; } = [];

    /// <summary>
    /// Display sort order within archetype.
    /// </summary>
    public int SortOrder { get; init; } = 0;

    /// <summary>
    /// Creates a new class definition with validation.
    /// </summary>
    public static ClassDefinition Create(
        string id,
        string name,
        string description,
        string archetypeId,
        StatModifiers statModifiers,
        StatModifiers growthRates,
        string primaryResourceId,
        IEnumerable<string>? startingAbilityIds = null,
        ClassRequirements? requirements = null,
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryResourceId);

        return new ClassDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Description = description,
            ArchetypeId = archetypeId.ToLowerInvariant(),
            StatModifiers = statModifiers,
            GrowthRates = growthRates,
            PrimaryResourceId = primaryResourceId.ToLowerInvariant(),
            StartingAbilityIds = startingAbilityIds?.ToList() ?? [],
            Requirements = requirements,
            SortOrder = sortOrder
        };
    }
}
