using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for status effect icon display.
/// </summary>
/// <remarks>
/// <para>Contains all information needed to render an effect icon in the combat UI.</para>
/// <para>Maps from <see cref="Domain.Entities.ActiveStatusEffect"/> to presentation data.</para>
/// </remarks>
public record StatusEffectDisplayDto
{
    /// <summary>
    /// Effect definition ID (e.g., "poison", "regeneration").
    /// </summary>
    public required string EffectId { get; init; }

    /// <summary>
    /// Display name for the effect (e.g., "Poisoned", "Regenerating").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Effect category determining buff/debuff classification and color.
    /// </summary>
    public required EffectCategory Category { get; init; }

    /// <summary>
    /// Primary effect type for icon selection.
    /// </summary>
    public required StatusEffectType EffectType { get; init; }

    /// <summary>
    /// Remaining duration in turns (null if permanent or triggered).
    /// </summary>
    public int? RemainingDuration { get; init; }

    /// <summary>
    /// Duration type for formatting (Turns, Permanent, Triggered, ResourceBased).
    /// </summary>
    public required DurationType DurationType { get; init; }

    /// <summary>
    /// Current stack count (1 = single application).
    /// </summary>
    public int CurrentStacks { get; init; } = 1;

    /// <summary>
    /// Maximum allowed stacks for this effect.
    /// </summary>
    public int MaxStacks { get; init; } = 1;

    /// <summary>
    /// Damage type for DoT effects (e.g., "fire", "poison").
    /// </summary>
    /// <remarks>
    /// Used to select the appropriate damage type icon (F, P, I, etc.).
    /// </remarks>
    public string? DamageType { get; init; }

    /// <summary>
    /// Custom icon identifier from the effect definition.
    /// </summary>
    /// <remarks>
    /// If specified, this overrides the default icon mapping.
    /// </remarks>
    public string? IconId { get; init; }

    /// <summary>
    /// Name of the entity that applied this effect.
    /// </summary>
    public string? SourceName { get; init; }

    /// <summary>
    /// Whether this effect is at maximum stacks.
    /// </summary>
    public bool IsAtMaxStacks => CurrentStacks >= MaxStacks && MaxStacks > 1;
}
