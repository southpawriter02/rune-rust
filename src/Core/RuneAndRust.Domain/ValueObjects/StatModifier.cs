using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a modification to a combat stat.
/// </summary>
/// <remarks>
/// <para>StatModifier is an immutable value object that defines how a stat is changed.</para>
/// <para>Modifiers from multiple effects are aggregated by StatCalculator.</para>
/// </remarks>
/// <param name="StatId">The stat being modified (e.g., "attack", "defense", "speed").</param>
/// <param name="ModifierType">How the modification is applied (Flat, Percentage, Override).</param>
/// <param name="Value">The modification value. For Percentage, use decimal (0.3 = +30%).</param>
public readonly record struct StatModifier(
    string StatId,
    StatModifierType ModifierType,
    float Value)
{
    /// <summary>
    /// Applies this modifier to a base value.
    /// </summary>
    /// <param name="baseValue">The base stat value before modification.</param>
    /// <returns>The modified value.</returns>
    public int Apply(int baseValue)
    {
        return ModifierType switch
        {
            StatModifierType.Flat => baseValue + (int)Value,
            StatModifierType.Percentage => (int)(baseValue * (1 + Value)),
            StatModifierType.Override => (int)Value,
            _ => baseValue
        };
    }

    /// <summary>
    /// Creates a flat modifier that adds/subtracts a fixed value.
    /// </summary>
    /// <param name="statId">The stat to modify.</param>
    /// <param name="value">The flat value to add (negative to subtract).</param>
    /// <returns>A flat stat modifier.</returns>
    public static StatModifier Flat(string statId, int value) =>
        new(statId, StatModifierType.Flat, value);

    /// <summary>
    /// Creates a percentage modifier that multiplies the stat.
    /// </summary>
    /// <param name="statId">The stat to modify.</param>
    /// <param name="percentage">The percentage as decimal (0.3 = +30%, -0.5 = -50%).</param>
    /// <returns>A percentage stat modifier.</returns>
    public static StatModifier Percentage(string statId, float percentage) =>
        new(statId, StatModifierType.Percentage, percentage);

    /// <summary>
    /// Creates an override modifier that sets the stat to a specific value.
    /// </summary>
    /// <param name="statId">The stat to modify.</param>
    /// <param name="value">The override value.</param>
    /// <returns>An override stat modifier.</returns>
    public static StatModifier Override(string statId, int value) =>
        new(statId, StatModifierType.Override, value);

    /// <summary>
    /// Returns a display string for this modifier.
    /// </summary>
    public override string ToString()
    {
        return ModifierType switch
        {
            StatModifierType.Flat when Value >= 0 => $"{StatId} +{Value:F0}",
            StatModifierType.Flat => $"{StatId} {Value:F0}",
            StatModifierType.Percentage when Value >= 0 => $"{StatId} +{Value * 100:F0}%",
            StatModifierType.Percentage => $"{StatId} {Value * 100:F0}%",
            StatModifierType.Override => $"{StatId} = {Value:F0}",
            _ => $"{StatId}: {Value}"
        };
    }
}
