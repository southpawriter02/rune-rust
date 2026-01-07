using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single effect that an ability applies when used.
/// </summary>
/// <remarks>
/// AbilityEffect is an immutable value object that defines what happens
/// when an ability is executed. Effects can be instant (damage, heal) or
/// over time (DoT, HoT, buffs, debuffs).
/// </remarks>
public readonly record struct AbilityEffect
{
    /// <summary>
    /// Gets the type of effect.
    /// </summary>
    public AbilityEffectType EffectType { get; init; }

    /// <summary>
    /// Gets the base value of the effect (damage amount, heal amount, etc.).
    /// </summary>
    public int Value { get; init; }

    /// <summary>
    /// Gets the duration in turns (0 = instant effect).
    /// </summary>
    public int Duration { get; init; }

    /// <summary>
    /// Gets the status effect identifier (e.g., "poison", "stun", "burning").
    /// </summary>
    public string? StatusEffect { get; init; }

    /// <summary>
    /// Gets stat modifiers for buff/debuff effects.
    /// </summary>
    public StatModifiers? StatModifier { get; init; }

    /// <summary>
    /// Gets the success chance (0.0 to 1.0, default 1.0 = always succeeds).
    /// </summary>
    public float Chance { get; init; }

    /// <summary>
    /// Gets the stat that scales this effect's value (e.g., "attack", "will").
    /// </summary>
    public string? ScalingStat { get; init; }

    /// <summary>
    /// Gets the multiplier for stat scaling.
    /// </summary>
    public float ScalingMultiplier { get; init; }

    /// <summary>
    /// Gets a description text override for this effect.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets whether this effect is instant (no duration).
    /// </summary>
    public bool IsInstant => Duration == 0;

    /// <summary>
    /// Gets whether this effect has stat scaling.
    /// </summary>
    public bool HasScaling => !string.IsNullOrEmpty(ScalingStat) && ScalingMultiplier != 0;

    /// <summary>
    /// Gets whether this effect has a chance to fail.
    /// </summary>
    public bool HasChance => Chance < 1.0f;

    /// <summary>
    /// Creates a damage effect.
    /// </summary>
    /// <param name="value">Base damage amount.</param>
    /// <param name="scalingStat">Optional stat that scales damage.</param>
    /// <param name="scalingMultiplier">Multiplier for stat scaling.</param>
    /// <returns>A new AbilityEffect configured for damage.</returns>
    public static AbilityEffect Damage(int value, string? scalingStat = null, float scalingMultiplier = 0f)
    {
        return new AbilityEffect
        {
            EffectType = AbilityEffectType.Damage,
            Value = value,
            Duration = 0,
            Chance = 1.0f,
            ScalingStat = scalingStat?.ToLowerInvariant(),
            ScalingMultiplier = scalingMultiplier
        };
    }

    /// <summary>
    /// Creates a healing effect.
    /// </summary>
    /// <param name="value">Base heal amount.</param>
    /// <param name="scalingStat">Optional stat that scales healing.</param>
    /// <param name="scalingMultiplier">Multiplier for stat scaling.</param>
    /// <returns>A new AbilityEffect configured for healing.</returns>
    public static AbilityEffect Heal(int value, string? scalingStat = null, float scalingMultiplier = 0f)
    {
        return new AbilityEffect
        {
            EffectType = AbilityEffectType.Heal,
            Value = value,
            Duration = 0,
            Chance = 1.0f,
            ScalingStat = scalingStat?.ToLowerInvariant(),
            ScalingMultiplier = scalingMultiplier
        };
    }

    /// <summary>
    /// Creates a damage-over-time effect.
    /// </summary>
    /// <param name="valuePerTurn">Damage dealt each turn.</param>
    /// <param name="duration">Number of turns the effect lasts.</param>
    /// <param name="statusEffect">The status effect name (e.g., "poison", "burning").</param>
    /// <returns>A new AbilityEffect configured for damage over time.</returns>
    public static AbilityEffect DamageOverTime(int valuePerTurn, int duration, string statusEffect)
    {
        return new AbilityEffect
        {
            EffectType = AbilityEffectType.DamageOverTime,
            Value = valuePerTurn,
            Duration = duration,
            StatusEffect = statusEffect.ToLowerInvariant(),
            Chance = 1.0f
        };
    }

    /// <summary>
    /// Creates a heal-over-time effect.
    /// </summary>
    /// <param name="valuePerTurn">Health restored each turn.</param>
    /// <param name="duration">Number of turns the effect lasts.</param>
    /// <returns>A new AbilityEffect configured for healing over time.</returns>
    public static AbilityEffect HealOverTime(int valuePerTurn, int duration)
    {
        return new AbilityEffect
        {
            EffectType = AbilityEffectType.HealOverTime,
            Value = valuePerTurn,
            Duration = duration,
            Chance = 1.0f
        };
    }

    /// <summary>
    /// Creates a buff effect with stat modifiers.
    /// </summary>
    /// <param name="modifiers">The stat modifiers to apply.</param>
    /// <param name="duration">Number of turns the buff lasts.</param>
    /// <param name="description">Optional description override.</param>
    /// <returns>A new AbilityEffect configured as a buff.</returns>
    public static AbilityEffect Buff(StatModifiers modifiers, int duration, string? description = null)
    {
        return new AbilityEffect
        {
            EffectType = AbilityEffectType.Buff,
            Value = 0,
            Duration = duration,
            StatModifier = modifiers,
            Chance = 1.0f,
            Description = description
        };
    }

    /// <summary>
    /// Creates a debuff effect with stat modifiers.
    /// </summary>
    /// <param name="modifiers">The stat modifiers to apply (typically negative).</param>
    /// <param name="duration">Number of turns the debuff lasts.</param>
    /// <param name="chance">Chance to apply (0.0 to 1.0).</param>
    /// <returns>A new AbilityEffect configured as a debuff.</returns>
    public static AbilityEffect Debuff(StatModifiers modifiers, int duration, float chance = 1.0f)
    {
        return new AbilityEffect
        {
            EffectType = AbilityEffectType.Debuff,
            Value = 0,
            Duration = duration,
            StatModifier = modifiers,
            Chance = chance
        };
    }

    /// <summary>
    /// Creates a shield/absorb effect.
    /// </summary>
    /// <param name="absorbAmount">Amount of damage the shield can absorb.</param>
    /// <param name="duration">Number of turns the shield lasts.</param>
    /// <returns>A new AbilityEffect configured as a shield.</returns>
    public static AbilityEffect Shield(int absorbAmount, int duration)
    {
        return new AbilityEffect
        {
            EffectType = AbilityEffectType.Shield,
            Value = absorbAmount,
            Duration = duration,
            Chance = 1.0f
        };
    }

    /// <summary>
    /// Creates a status effect with a chance to apply.
    /// </summary>
    /// <param name="statusEffect">The status effect name (e.g., "stun", "slow").</param>
    /// <param name="duration">Number of turns the status lasts.</param>
    /// <param name="chance">Chance to apply (0.0 to 1.0).</param>
    /// <returns>A new AbilityEffect configured as a status effect.</returns>
    public static AbilityEffect Status(string statusEffect, int duration, float chance = 1.0f)
    {
        return new AbilityEffect
        {
            EffectType = AbilityEffectType.Debuff,
            Value = 0,
            Duration = duration,
            StatusEffect = statusEffect.ToLowerInvariant(),
            Chance = chance
        };
    }
}
