using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for effect tooltip content.
/// </summary>
/// <remarks>
/// <para>Contains detailed information for rendering a tooltip when an effect is selected.</para>
/// <para>Includes description, duration, stacks, damage/healing, stat modifiers, and source.</para>
/// </remarks>
public record EffectTooltipDto
{
    /// <summary>
    /// Effect display name shown as the tooltip header.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Full description text for the effect.
    /// </summary>
    /// <remarks>
    /// This text is word-wrapped to fit within the tooltip width.
    /// </remarks>
    public required string Description { get; init; }

    /// <summary>
    /// Effect category for color coding the tooltip.
    /// </summary>
    public required EffectCategory Category { get; init; }

    /// <summary>
    /// Remaining duration in turns (null if permanent or triggered).
    /// </summary>
    public int? RemainingDuration { get; init; }

    /// <summary>
    /// Duration type for formatting the duration line.
    /// </summary>
    public required DurationType DurationType { get; init; }

    /// <summary>
    /// Current stack count.
    /// </summary>
    public int CurrentStacks { get; init; } = 1;

    /// <summary>
    /// Maximum allowed stacks.
    /// </summary>
    public int MaxStacks { get; init; } = 1;

    /// <summary>
    /// Damage dealt per turn for DoT effects.
    /// </summary>
    public int? DamagePerTurn { get; init; }

    /// <summary>
    /// Damage type for DoT effects (e.g., "fire", "poison").
    /// </summary>
    public string? DamageType { get; init; }

    /// <summary>
    /// Healing received per turn for HoT effects.
    /// </summary>
    public int? HealingPerTurn { get; init; }

    /// <summary>
    /// List of stat modifiers applied by this effect.
    /// </summary>
    public IReadOnlyList<StatModifier>? StatModifiers { get; init; }

    /// <summary>
    /// Name of the entity that applied this effect.
    /// </summary>
    public string? SourceName { get; init; }

    /// <summary>
    /// Whether this effect has stacking behavior (MaxStacks > 1).
    /// </summary>
    public bool IsStackable => MaxStacks > 1;

    /// <summary>
    /// Whether this effect is at maximum stacks.
    /// </summary>
    public bool IsAtMaxStacks => CurrentStacks >= MaxStacks && MaxStacks > 1;

    /// <summary>
    /// Whether this effect deals damage over time.
    /// </summary>
    public bool HasDamageOverTime => DamagePerTurn.HasValue && DamagePerTurn > 0;

    /// <summary>
    /// Whether this effect heals over time.
    /// </summary>
    public bool HasHealingOverTime => HealingPerTurn.HasValue && HealingPerTurn > 0;

    /// <summary>
    /// Whether this effect modifies stats.
    /// </summary>
    public bool HasStatModifiers => StatModifiers?.Count > 0;
}
