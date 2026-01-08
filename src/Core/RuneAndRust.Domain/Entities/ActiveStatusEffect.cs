using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an active status effect on an entity.
/// </summary>
/// <remarks>
/// <para>ActiveStatusEffect tracks the runtime state of an applied effect.</para>
/// <para>Each instance is unique even if multiple targets have the same effect type.</para>
/// </remarks>
public class ActiveStatusEffect
{
    /// <summary>Unique identifier for this instance.</summary>
    public Guid Id { get; private set; }

    /// <summary>The effect definition.</summary>
    public StatusEffectDefinition Definition { get; private set; } = null!;

    /// <summary>The entity that applied this effect.</summary>
    public Guid? SourceId { get; private set; }

    /// <summary>Name of the source (for display).</summary>
    public string? SourceName { get; private set; }

    /// <summary>When this effect was applied.</summary>
    public DateTime AppliedAt { get; private set; }

    /// <summary>Remaining duration in turns (null if permanent/triggered).</summary>
    public int? RemainingDuration { get; private set; }

    /// <summary>Current stack count.</summary>
    public int Stacks { get; private set; } = 1;

    /// <summary>Remaining resource pool (for resource-based effects like Shield).</summary>
    public int? RemainingResource { get; private set; }

    /// <summary>Whether this effect has expired.</summary>
    public bool IsExpired =>
        (Definition.DurationType == DurationType.Turns && RemainingDuration <= 0) ||
        (Definition.DurationType == DurationType.ResourceBased && RemainingResource <= 0);

    /// <summary>Whether this effect is still active.</summary>
    public bool IsActive => !IsExpired;

    /// <summary>Private constructor for controlled creation.</summary>
    private ActiveStatusEffect() { }

    /// <summary>
    /// Creates an active status effect instance.
    /// </summary>
    public static ActiveStatusEffect Create(
        StatusEffectDefinition definition,
        Guid? sourceId = null,
        string? sourceName = null)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var effect = new ActiveStatusEffect
        {
            Id = Guid.NewGuid(),
            Definition = definition,
            SourceId = sourceId,
            SourceName = sourceName,
            AppliedAt = DateTime.UtcNow,
            Stacks = 1
        };

        // Set initial duration based on type
        effect.RemainingDuration = definition.DurationType == DurationType.Turns
            ? definition.BaseDuration
            : null;

        effect.RemainingResource = definition.DurationType == DurationType.ResourceBased
            ? definition.ResourcePool
            : null;

        return effect;
    }

    /// <summary>
    /// Decrements the duration by one turn.
    /// </summary>
    /// <returns>True if the effect is still active.</returns>
    public bool TickDuration()
    {
        if (RemainingDuration.HasValue && RemainingDuration > 0)
        {
            RemainingDuration--;
        }
        return IsActive;
    }

    /// <summary>
    /// Refreshes the duration to maximum.
    /// </summary>
    public void RefreshDuration()
    {
        if (Definition.DurationType == DurationType.Turns)
        {
            RemainingDuration = Definition.BaseDuration;
        }
    }

    /// <summary>
    /// Adds stacks to this effect.
    /// </summary>
    /// <returns>True if stacks were added, false if at max.</returns>
    public bool AddStacks(int count = 1)
    {
        if (Stacks >= Definition.MaxStacks)
            return false;

        Stacks = Math.Min(Stacks + count, Definition.MaxStacks);
        return true;
    }

    /// <summary>
    /// Consumes resource from this effect.
    /// </summary>
    /// <param name="amount">Amount to consume.</param>
    /// <returns>Amount actually consumed.</returns>
    public int ConsumeResource(int amount)
    {
        if (!RemainingResource.HasValue || RemainingResource <= 0)
            return 0;

        var consumed = Math.Min(amount, RemainingResource.Value);
        RemainingResource -= consumed;
        return consumed;
    }

    /// <summary>
    /// Calculates the effective damage per turn (considering stacks).
    /// </summary>
    public int CalculateDamagePerTurn()
    {
        if (!Definition.DamagePerTurn.HasValue)
            return 0;

        return Definition.DamagePerTurn.Value * Stacks;
    }

    /// <summary>
    /// Calculates the effective healing per turn (considering stacks).
    /// </summary>
    public int CalculateHealingPerTurn()
    {
        if (!Definition.HealingPerTurn.HasValue)
            return 0;

        return Definition.HealingPerTurn.Value * Stacks;
    }

    /// <summary>
    /// Gets all stat modifiers (scaled by stacks if applicable).
    /// </summary>
    public IEnumerable<StatModifier> GetEffectiveStatModifiers()
    {
        foreach (var mod in Definition.StatModifiers)
        {
            // Flat modifiers scale with stacks
            if (mod.ModifierType == StatModifierType.Flat)
            {
                yield return mod with { Value = mod.Value * Stacks };
            }
            // Percentage modifiers don't stack (would be too powerful)
            else
            {
                yield return mod;
            }
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var duration = RemainingDuration.HasValue
            ? $" ({RemainingDuration} turns)"
            : Definition.DurationType == DurationType.Permanent
                ? " (permanent)"
                : "";
        var stacks = Stacks > 1 ? $" x{Stacks}" : "";
        return $"{Definition.Name}{stacks}{duration}";
    }
}
