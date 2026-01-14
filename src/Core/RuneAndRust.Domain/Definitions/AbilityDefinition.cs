using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Represents a data-driven ability definition loaded from configuration.
/// </summary>
/// <remarks>
/// AbilityDefinition is immutable and represents the template for abilities.
/// Actual ability state (cooldowns, usage count) is tracked by PlayerAbility.
/// Abilities are loaded from config/abilities.json.
/// </remarks>
public class AbilityDefinition
{
    /// <summary>
    /// Gets the unique string identifier for this ability.
    /// </summary>
    /// <example>"shield-bash", "flame-bolt", "healing-word"</example>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name shown to players.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed description of the ability.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the class IDs that can use this ability.
    /// </summary>
    public IReadOnlyList<string> ClassIds { get; init; } = [];

    /// <summary>
    /// Gets the resource cost to use this ability.
    /// </summary>
    public AbilityCost Cost { get; init; }

    /// <summary>
    /// Gets the number of turns between uses (0 = no cooldown).
    /// </summary>
    public int Cooldown { get; init; }

    /// <summary>
    /// Gets the effects applied when the ability is used.
    /// </summary>
    public IReadOnlyList<AbilityEffect> Effects { get; init; } = [];

    /// <summary>
    /// Gets the valid target type for this ability.
    /// </summary>
    public AbilityTargetType TargetType { get; init; }

    // ===== Area Effect Properties (v0.5.3c) =====

    /// <summary>
    /// Gets the area effect configuration (null for single-target abilities).
    /// </summary>
    public AreaEffect? AreaEffect { get; init; }

    /// <summary>
    /// Gets whether this ability has an area effect.
    /// </summary>
    public bool IsAreaEffect => AreaEffect.HasValue && AreaEffect.Value.Shape != Enums.AreaEffectShape.None;

    // ===== Range Properties (v0.5.1a) =====

    /// <summary>
    /// Gets the range of this ability.
    /// </summary>
    /// <remarks>
    /// Default is 1 for melee abilities. Ranged abilities can have configurable range.
    /// </remarks>
    public int Range { get; init; } = 1;

    /// <summary>
    /// Gets the range type of this ability.
    /// </summary>
    public RangeType RangeType { get; init; } = RangeType.Melee;

    /// <summary>
    /// Gets the effective range based on range type.
    /// </summary>
    /// <returns>1 for Melee, 2 for Reach, or Range for Ranged.</returns>
    public int GetEffectiveRange() => RangeType switch
    {
        RangeType.Melee => 1,
        RangeType.Reach => 2,
        RangeType.Ranged => Range,
        _ => 1
    };

    /// <summary>
    /// Checks if a target at the given distance is in range.
    /// </summary>
    /// <param name="distance">The distance to the target.</param>
    /// <returns>True if the target is in range.</returns>
    public bool IsInRange(int distance) => RangeType switch
    {
        RangeType.Melee => distance == 1,
        RangeType.Reach => distance >= 1 && distance <= 2,
        RangeType.Ranged => distance >= 1 && distance <= Range,
        _ => distance == 1
    };

    // ===== Extended Range Properties (v0.5.1b) =====

    /// <summary>
    /// Gets the minimum range for this ability (0 = no minimum).
    /// </summary>
    public int MinRange { get; init; }

    /// <summary>
    /// Gets the optimal range (no penalty zone). Null = Range.
    /// </summary>
    public int? OptimalRange { get; init; }

    /// <summary>
    /// Gets the accuracy penalty per cell beyond optimal (0 = no penalty).
    /// </summary>
    public int RangePenalty { get; init; }

    /// <summary>
    /// Gets the effective optimal range.
    /// </summary>
    /// <returns>OptimalRange if set, otherwise Range (most spells have no penalty).</returns>
    public int GetOptimalRange() => OptimalRange ?? Range;

    /// <summary>
    /// Gets the accuracy penalty at a given distance.
    /// </summary>
    /// <param name="distance">The distance to the target.</param>
    /// <returns>0 if no penalty or in optimal range, penalty per cell beyond optimal otherwise.</returns>
    public int GetPenaltyAtDistance(int distance)
    {
        // No penalty if RangePenalty is 0
        if (RangePenalty == 0) return 0;

        // Min range / out of range check
        if (distance < MinRange) return int.MaxValue;
        if (distance > Range) return int.MaxValue;

        var optimal = GetOptimalRange();
        if (distance <= optimal) return 0;

        return (distance - optimal) * RangePenalty;
    }

    /// <summary>
    /// Gets the level required to unlock this ability (default 1).
    /// </summary>
    public int UnlockLevel { get; init; } = 1;

    /// <summary>
    /// Gets tags for categorization and filtering.
    /// </summary>
    /// <example>["damage", "melee", "stun"], ["heal", "support", "holy"]</example>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets whether this ability is available to a specific class.
    /// </summary>
    /// <param name="classId">The class ID to check.</param>
    /// <returns>True if the class can use this ability.</returns>
    public bool IsAvailableToClass(string classId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(classId);
        return ClassIds.Contains(classId.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets whether this ability has a resource cost.
    /// </summary>
    public bool HasCost => Cost.HasCost;

    /// <summary>
    /// Gets whether this ability has a cooldown.
    /// </summary>
    public bool HasCooldown => Cooldown > 0;

    /// <summary>
    /// Gets whether this ability is instant (no cooldown).
    /// </summary>
    public bool IsInstant => Cooldown == 0;

    /// <summary>
    /// Gets whether this ability has the specified tag.
    /// </summary>
    /// <param name="tag">The tag to check.</param>
    /// <returns>True if the ability has this tag.</returns>
    public bool HasTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return Tags.Contains(tag.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a new ability definition with validation.
    /// </summary>
    /// <param name="id">Unique identifier for the ability.</param>
    /// <param name="name">Display name.</param>
    /// <param name="description">Detailed description.</param>
    /// <param name="classIds">Classes that can use this ability.</param>
    /// <param name="cost">Resource cost.</param>
    /// <param name="cooldown">Turns between uses (0 = no cooldown).</param>
    /// <param name="effects">Effects applied when used.</param>
    /// <param name="targetType">Valid target type.</param>
    /// <param name="unlockLevel">Level required to unlock (default 1).</param>
    /// <param name="tags">Optional categorization tags.</param>
    /// <returns>A new AbilityDefinition instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when cooldown is negative or unlockLevel is less than 1.</exception>
    public static AbilityDefinition Create(
        string id,
        string name,
        string description,
        IEnumerable<string> classIds,
        AbilityCost cost,
        int cooldown,
        IEnumerable<AbilityEffect> effects,
        AbilityTargetType targetType,
        int unlockLevel = 1,
        IEnumerable<string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(cooldown);
        ArgumentOutOfRangeException.ThrowIfLessThan(unlockLevel, 1);

        return new AbilityDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Description = description,
            ClassIds = classIds.Select(c => c.ToLowerInvariant()).ToList(),
            Cost = cost,
            Cooldown = cooldown,
            Effects = effects.ToList(),
            TargetType = targetType,
            UnlockLevel = unlockLevel,
            Tags = tags?.Select(t => t.ToLowerInvariant()).ToList() ?? []
        };
    }
}
