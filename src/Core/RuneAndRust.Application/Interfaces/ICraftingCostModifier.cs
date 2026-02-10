using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for modifying crafting costs and time.
/// </summary>
/// <remarks>
/// <para>Used by crafting systems to apply passive bonuses such as
/// Dvergr Techniques (20% material and time reduction).</para>
/// <para>Implementations should check whether the player qualifies for
/// the modifier before applying it. If the player does not qualify,
/// the original cost/time should be returned unchanged.</para>
/// </remarks>
public interface ICraftingCostModifier
{
    /// <summary>
    /// Applies cost reduction to crafting material costs.
    /// </summary>
    /// <param name="player">The player performing the craft.</param>
    /// <param name="baseCost">The original material cost before modifiers.</param>
    /// <returns>The modified cost (reduced if applicable), never less than 1.</returns>
    int ModifyMaterialCost(Player player, int baseCost);

    /// <summary>
    /// Applies time reduction to crafting duration.
    /// </summary>
    /// <param name="player">The player performing the craft.</param>
    /// <param name="baseTimeMinutes">The original crafting time in minutes.</param>
    /// <returns>The modified time in minutes (reduced if applicable), never less than 1.</returns>
    int ModifyCraftingTime(Player player, int baseTimeMinutes);

    /// <summary>
    /// Gets the percentage of cost reduction this modifier provides.
    /// </summary>
    /// <returns>Decimal percentage (e.g., 0.20 = 20%).</returns>
    decimal GetCostReductionPercentage();
}
