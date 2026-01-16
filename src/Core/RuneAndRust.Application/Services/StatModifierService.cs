namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for applying and calculating stat modifiers from status effects.
/// </summary>
public class StatModifierService : IStatModifierService
{
    private readonly ILogger<StatModifierService> _logger;

    /// <summary>
    /// Creates a new stat modifier service.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public StatModifierService(ILogger<StatModifierService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public int CalculateModifiedStat(int baseValue, IEnumerable<StatModifier> modifiers, int stacks = 1)
    {
        float flatTotal = 0;
        float percentageTotal = 0;
        int? overrideValue = null;

        foreach (var modifier in modifiers)
        {
            switch (modifier.ModifierType)
            {
                case StatModifierType.Flat:
                    flatTotal += modifier.Value * stacks;
                    break;
                case StatModifierType.Percentage:
                    percentageTotal += modifier.Value * stacks;
                    break;
                case StatModifierType.Override:
                    overrideValue = (int)modifier.Value;
                    break;
            }
        }

        // Override takes precedence
        if (overrideValue.HasValue)
        {
            _logger.LogDebug("Stat override to {Value}", overrideValue.Value);
            return overrideValue.Value;
        }

        // Apply flat modifiers first, then percentage
        var modified = baseValue + (int)flatTotal;
        if (Math.Abs(percentageTotal) > 0.001f)
        {
            modified = (int)(modified * (1 + percentageTotal));
        }

        _logger.LogDebug("Stat modified: {Base} → {Modified} (flat: {Flat}, %: {Percent})",
            baseValue, modified, flatTotal, percentageTotal);

        return Math.Max(0, modified);
    }

    /// <inheritdoc />
    public int GetTotalFlatModifier(IEnumerable<StatModifier> modifiers, string statId, int stacks = 1)
    {
        return (int)modifiers
            .Where(m => m.StatId == statId && m.ModifierType == StatModifierType.Flat)
            .Sum(m => m.Value * stacks);
    }

    /// <inheritdoc />
    public float GetTotalPercentageModifier(IEnumerable<StatModifier> modifiers, string statId, int stacks = 1)
    {
        return modifiers
            .Where(m => m.StatId == statId && m.ModifierType == StatModifierType.Percentage)
            .Sum(m => m.Value * stacks);
    }
}
