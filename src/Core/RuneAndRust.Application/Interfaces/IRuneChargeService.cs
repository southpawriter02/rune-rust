using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Rune Charge resource management for the Rúnasmiðr specialization.
/// </summary>
/// <remarks>
/// <para>Rune Charges fuel inscription and ward abilities:</para>
/// <list type="bullet">
/// <item>Inscribe Rune: 1 charge</item>
/// <item>Runestone Ward: 1 charge</item>
/// <item>Empowered Inscription: 2 charges (Tier 2)</item>
/// <item>Runic Trap: 2 charges (Tier 2)</item>
/// <item>Living Runes: 3 charges (Tier 3)</item>
/// <item>Word of Unmaking: 4 charges (Capstone)</item>
/// </list>
/// <para>Charges are generated through crafting and restore fully on rest.</para>
/// <para>Follows the same interface pattern as <see cref="IBlockChargeService"/>
/// for consistency across specialization resource systems.</para>
/// </remarks>
public interface IRuneChargeService
{
    /// <summary>
    /// Attempts to spend the specified number of Rune Charges.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <param name="amount">Number of charges to spend.</param>
    /// <returns>True if charges were successfully spent; false if insufficient.</returns>
    bool SpendCharges(Player player, int amount);

    /// <summary>
    /// Generates Rune Charges from a crafting action.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <param name="isComplexCraft">True for complex crafts (2 charges); false for standard (1).</param>
    /// <returns>The number of charges actually generated.</returns>
    int GenerateFromCraft(Player player, bool isComplexCraft);

    /// <summary>
    /// Restores the specified number of Rune Charges.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <param name="amount">Number of charges to restore.</param>
    void RestoreCharges(Player player, int amount);

    /// <summary>
    /// Restores all Rune Charges to maximum.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    void RestoreAllCharges(Player player);

    /// <summary>
    /// Checks if player can spend the specified number of charges.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <param name="amount">Number of charges to check.</param>
    /// <returns>True if sufficient charges are available.</returns>
    bool CanSpend(Player player, int amount);

    /// <summary>
    /// Gets the current number of Rune Charges.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <returns>Current charges, or 0 if player has no RuneChargeResource.</returns>
    int GetCurrentValue(Player player);

    /// <summary>
    /// Gets the maximum Rune Charges for this player.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <returns>Max charges, or 0 if player has no RuneChargeResource.</returns>
    int GetMaxValue(Player player);
}
