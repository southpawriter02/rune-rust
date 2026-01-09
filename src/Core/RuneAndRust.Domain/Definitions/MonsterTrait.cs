using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a special trait that grants monsters unique abilities or behaviors.
/// </summary>
/// <remarks>
/// Traits add tactical variety to combat encounters. A monster may have
/// multiple traits that combine to create unique challenges.
/// </remarks>
public class MonsterTrait
{
    /// <summary>
    /// Gets the unique identifier for this trait.
    /// </summary>
    /// <example>"regenerating", "flying", "venomous"</example>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of this trait.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description of this trait shown to players.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the mechanical effect type of this trait.
    /// </summary>
    public TraitEffect Effect { get; init; } = TraitEffect.None;

    /// <summary>
    /// Gets the numeric value associated with the effect.
    /// </summary>
    /// <remarks>
    /// Meaning depends on Effect type:
    /// - Regeneration: HP healed per turn
    /// - Flying: Defense/dodge bonus
    /// - Venomous: Poison damage per turn
    /// - Armored: Flat damage reduction (added to defense)
    /// - Berserker: Damage bonus percentage at low HP
    /// </remarks>
    public int EffectValue { get; init; } = 0;

    /// <summary>
    /// Gets the HP threshold for conditional traits (like Berserker).
    /// </summary>
    /// <remarks>
    /// Expressed as percentage of max HP (e.g., 30 = activates below 30% HP).
    /// Null for traits that don't have thresholds.
    /// </remarks>
    public int? TriggerThreshold { get; init; }

    /// <summary>
    /// Gets the categorical tags for this trait.
    /// </summary>
    /// <remarks>
    /// Used for filtering and categorization (e.g., "passive", "combat", "defensive").
    /// </remarks>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the color used for displaying this trait.
    /// </summary>
    /// <remarks>
    /// Should be a valid Spectre.Console color name.
    /// </remarks>
    public string Color { get; init; } = "white";

    /// <summary>
    /// Gets the display sort order.
    /// </summary>
    public int SortOrder { get; init; } = 0;

    /// <summary>
    /// Private parameterless constructor for JSON deserialization.
    /// </summary>
    private MonsterTrait()
    {
    }

    /// <summary>
    /// Creates a validated MonsterTrait.
    /// </summary>
    /// <param name="id">The unique identifier (will be normalized to lowercase).</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The player-visible description.</param>
    /// <param name="effect">The mechanical effect type.</param>
    /// <param name="effectValue">The numeric value for the effect.</param>
    /// <param name="triggerThreshold">Optional HP threshold percentage.</param>
    /// <param name="tags">Optional categorization tags.</param>
    /// <param name="color">Spectre.Console color name.</param>
    /// <param name="sortOrder">Display sort order.</param>
    /// <returns>A new validated MonsterTrait.</returns>
    /// <exception cref="ArgumentException">When id or name is null/empty.</exception>
    public static MonsterTrait Create(
        string id,
        string name,
        string description,
        TraitEffect effect,
        int effectValue = 0,
        int? triggerThreshold = null,
        IEnumerable<string>? tags = null,
        string color = "white",
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new MonsterTrait
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            Effect = effect,
            EffectValue = effectValue,
            TriggerThreshold = triggerThreshold,
            Tags = tags?.Select(t => t.ToLowerInvariant()).ToList() ?? [],
            Color = string.IsNullOrWhiteSpace(color) ? "white" : color,
            SortOrder = sortOrder
        };
    }

    /// <summary>
    /// Checks if this trait has the specified tag.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    /// <returns>True if the trait has the tag; otherwise, false.</returns>
    public bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return false;
        return Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }
}
