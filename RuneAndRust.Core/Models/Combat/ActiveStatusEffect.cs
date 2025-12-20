using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Represents an active status effect on a combatant during combat.
/// Tracks effect type, duration, stacks, and the source that applied it.
/// Combat-volatile: not persisted to database, cleared when combat ends.
/// </summary>
public class ActiveStatusEffect
{
    /// <summary>
    /// Unique identifier for this effect instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The type of status effect.
    /// </summary>
    public StatusEffectType Type { get; set; }

    /// <summary>
    /// Current number of stacks for stackable effects.
    /// Defaults to 1. Maximum varies by effect type.
    /// </summary>
    public int Stacks { get; set; } = 1;

    /// <summary>
    /// Remaining duration in turns. Effect is removed when this reaches 0.
    /// </summary>
    public int DurationRemaining { get; set; }

    /// <summary>
    /// The combatant ID that applied this effect. Used for tracking and attribution.
    /// </summary>
    public Guid SourceId { get; set; }

    /// <summary>
    /// Gets the maximum number of stacks allowed for a given effect type.
    /// </summary>
    /// <param name="type">The status effect type.</param>
    /// <returns>Maximum stack count (1 for non-stackable effects, up to 5 for stackable).</returns>
    public static int GetMaxStacks(StatusEffectType type) => type switch
    {
        StatusEffectType.Bleeding => 5,
        StatusEffectType.Poisoned => 5,
        StatusEffectType.Fortified => 5,
        _ => 1
    };

    /// <summary>
    /// Determines whether an effect type can stack (Intensify) or only refresh duration.
    /// </summary>
    /// <param name="type">The status effect type.</param>
    /// <returns>True if the effect stacks; false if it only refreshes duration.</returns>
    public static bool CanStack(StatusEffectType type) => type switch
    {
        StatusEffectType.Bleeding => true,
        StatusEffectType.Poisoned => true,
        StatusEffectType.Fortified => true,
        _ => false
    };

    /// <summary>
    /// Determines whether an effect type is a debuff (harmful to the target).
    /// </summary>
    /// <param name="type">The status effect type.</param>
    /// <returns>True if the effect is a debuff; false if it's a buff.</returns>
    public static bool IsDebuff(StatusEffectType type) => (int)type < 100;

    /// <summary>
    /// Determines whether an effect type is a damage-over-time effect.
    /// </summary>
    /// <param name="type">The status effect type.</param>
    /// <returns>True if the effect deals periodic damage.</returns>
    public static bool IsDamageOverTime(StatusEffectType type) => type switch
    {
        StatusEffectType.Bleeding => true,
        StatusEffectType.Poisoned => true,
        _ => false
    };
}
