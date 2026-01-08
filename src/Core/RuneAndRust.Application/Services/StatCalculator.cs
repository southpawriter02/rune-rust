using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Calculator for computing effective stat values with modifiers.
/// </summary>
/// <remarks>
/// <para>StatCalculator aggregates modifiers from all active effects.</para>
/// <para>Application order: Flat → Percentage → Override (if present).</para>
/// </remarks>
public class StatCalculator
{
    /// <summary>
    /// Calculates the effective value of a stat after all modifiers.
    /// </summary>
    /// <param name="target">The entity with active effects.</param>
    /// <param name="statId">The stat to calculate (e.g., "attack", "defense").</param>
    /// <param name="baseValue">The base stat value before modifiers.</param>
    /// <returns>The effective stat value (minimum 0).</returns>
    public int CalculateStat(IEffectTarget target, string statId, int baseValue)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(statId);

        var normalizedStatId = statId.ToLowerInvariant();

        // Gather all modifiers for this stat
        var modifiers = target.ActiveEffects
            .SelectMany(e => e.GetEffectiveStatModifiers())
            .Where(m => m.StatId.Equals(normalizedStatId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (modifiers.Count == 0)
            return baseValue;

        // Check for override first (takes precedence)
        var overrideModifier = modifiers
            .FirstOrDefault(m => m.ModifierType == StatModifierType.Override);

        if (overrideModifier.ModifierType == StatModifierType.Override)
        {
            return Math.Max(0, (int)overrideModifier.Value);
        }

        // Apply flat modifiers
        var flatSum = modifiers
            .Where(m => m.ModifierType == StatModifierType.Flat)
            .Sum(m => m.Value);

        var afterFlat = baseValue + (int)flatSum;

        // Apply percentage modifiers (multiplicative)
        var percentageMultiplier = 1.0f;
        foreach (var mod in modifiers.Where(m => m.ModifierType == StatModifierType.Percentage))
        {
            percentageMultiplier *= (1 + mod.Value);
        }

        var result = (int)(afterFlat * percentageMultiplier);
        return Math.Max(0, result);
    }

    /// <summary>
    /// Calculates multiple stats at once.
    /// </summary>
    /// <param name="target">The entity with active effects.</param>
    /// <param name="baseStats">Dictionary of stat IDs to base values.</param>
    /// <returns>Dictionary of stat IDs to effective values.</returns>
    public Dictionary<string, int> CalculateStats(
        IEffectTarget target,
        IReadOnlyDictionary<string, int> baseStats)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(baseStats);

        return baseStats.ToDictionary(
            kvp => kvp.Key,
            kvp => CalculateStat(target, kvp.Key, kvp.Value));
    }

    /// <summary>
    /// Gets all stat modifiers active on a target.
    /// </summary>
    /// <param name="target">The entity to check.</param>
    /// <returns>All active stat modifiers grouped by stat ID.</returns>
    public ILookup<string, StatModifier> GetAllModifiers(IEffectTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        return target.ActiveEffects
            .SelectMany(e => e.GetEffectiveStatModifiers())
            .ToLookup(m => m.StatId.ToLowerInvariant());
    }

    /// <summary>
    /// Checks if a target has any modifiers for a specific stat.
    /// </summary>
    /// <param name="target">The entity to check.</param>
    /// <param name="statId">The stat to check.</param>
    /// <returns>True if any modifiers exist for the stat.</returns>
    public bool HasModifiersForStat(IEffectTarget target, string statId)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(statId);

        var normalizedStatId = statId.ToLowerInvariant();

        return target.ActiveEffects
            .SelectMany(e => e.GetEffectiveStatModifiers())
            .Any(m => m.StatId.Equals(normalizedStatId, StringComparison.OrdinalIgnoreCase));
    }
}
