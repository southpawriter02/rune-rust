using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models.Crafting;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Provides repair and salvage operations for equipment items.
/// Repair uses WITS-based dice rolls to determine success and quality.
/// Salvage destroys equipment to extract Scrap materials.
/// </summary>
public interface IBodgingService
{
    /// <summary>
    /// Attempts to repair an equipment item using Scrap and a WITS roll.
    /// DC = 8 + (damage / 5). Consumes Scrap regardless of outcome.
    /// Fumble (net negative) reduces MaxDurability by 10.
    /// </summary>
    /// <param name="character">The character performing the repair.</param>
    /// <param name="itemId">The unique identifier of the equipment to repair.</param>
    /// <returns>A RepairResult containing the outcome and durability changes.</returns>
    Task<RepairResult> RepairItemAsync(Character character, Guid itemId);

    /// <summary>
    /// Destroys an equipment item to extract Scrap materials.
    /// Yield = (Weight / 100) * (QualityModifier + 1)
    /// </summary>
    /// <param name="character">The character performing the salvage.</param>
    /// <param name="itemId">The unique identifier of the equipment to salvage.</param>
    /// <returns>A SalvageResult containing the Scrap yield.</returns>
    Task<SalvageResult> SalvageItemAsync(Character character, Guid itemId);

    /// <summary>
    /// Calculates the Scrap cost for repairing an equipment item.
    /// Cost = Ceiling(damage / 5), minimum 1.
    /// </summary>
    /// <param name="equipment">The equipment to calculate repair cost for.</param>
    /// <returns>The number of Scrap required for repair.</returns>
    int CalculateRepairCost(Equipment equipment);

    /// <summary>
    /// Calculates the Scrap yield from salvaging an equipment item.
    /// Yield = (Weight / 100) * (QualityModifier + 1)
    /// </summary>
    /// <param name="equipment">The equipment to calculate salvage yield for.</param>
    /// <returns>The amount of Scrap obtained from salvaging.</returns>
    int CalculateSalvageYield(Equipment equipment);

    /// <summary>
    /// Checks if the character has enough Scrap to repair the equipment.
    /// </summary>
    /// <param name="character">The character whose inventory to check.</param>
    /// <param name="equipment">The equipment to check repair requirements for.</param>
    /// <returns>True if the character has sufficient Scrap, false otherwise.</returns>
    bool CanRepair(Character character, Equipment equipment);
}
