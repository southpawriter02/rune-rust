namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for applying and removing stat modifiers from status effects.
/// </summary>
public interface IStatModifierService
{
    /// <summary>
    /// Calculates the modified value of a stat after applying modifiers.
    /// </summary>
    /// <param name="baseValue">The base stat value.</param>
    /// <param name="modifiers">The modifiers to apply.</param>
    /// <param name="stacks">The stack count for scaling.</param>
    /// <returns>The modified stat value.</returns>
    int CalculateModifiedStat(int baseValue, IEnumerable<StatModifier> modifiers, int stacks = 1);

    /// <summary>
    /// Gets the total flat modifier for a stat from a list of modifiers.
    /// </summary>
    /// <param name="modifiers">The modifiers to sum.</param>
    /// <param name="statId">The stat to get modifiers for.</param>
    /// <param name="stacks">The stack count for scaling.</param>
    /// <returns>Total flat modifier value.</returns>
    int GetTotalFlatModifier(IEnumerable<StatModifier> modifiers, string statId, int stacks = 1);

    /// <summary>
    /// Gets the total percentage modifier for a stat from a list of modifiers.
    /// </summary>
    /// <param name="modifiers">The modifiers to sum.</param>
    /// <param name="statId">The stat to get modifiers for.</param>
    /// <param name="stacks">The stack count for scaling.</param>
    /// <returns>Total percentage modifier value.</returns>
    float GetTotalPercentageModifier(IEnumerable<StatModifier> modifiers, string statId, int stacks = 1);
}
