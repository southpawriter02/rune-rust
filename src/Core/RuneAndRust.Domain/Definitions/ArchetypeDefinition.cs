using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Represents a base class archetype from which specialized classes derive.
/// </summary>
/// <remarks>
/// Archetypes define the fundamental playstyle category (Warrior, Mystic, etc.)
/// and serve as a grouping mechanism for related classes. They are loaded
/// from configuration and should not be instantiated directly.
/// </remarks>
public class ArchetypeDefinition
{
    /// <summary>
    /// Gets the unique identifier for this archetype.
    /// </summary>
    /// <example>"warrior", "mystic", "skirmisher", "adept"</example>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of this archetype.
    /// </summary>
    /// <example>"Warrior", "Mystic"</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed description of this archetype.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets a brief summary of the archetype's playstyle.
    /// </summary>
    /// <example>"Masters of martial combat who thrive on the front lines."</example>
    public string PlaystyleSummary { get; init; } = string.Empty;

    /// <summary>
    /// Gets the general stat tendency for classes in this archetype.
    /// </summary>
    public StatTendency StatTendency { get; init; } = StatTendency.Balanced;

    /// <summary>
    /// Display sort order.
    /// </summary>
    public int SortOrder { get; init; } = 0;

    /// <summary>
    /// Creates a new archetype definition with validation.
    /// </summary>
    public static ArchetypeDefinition Create(
        string id,
        string name,
        string description,
        string playstyleSummary,
        StatTendency statTendency,
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new ArchetypeDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Description = description,
            PlaystyleSummary = playstyleSummary,
            StatTendency = statTendency,
            SortOrder = sortOrder
        };
    }
}
