using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Block Charge resource management for the Skjaldmær specialization.
/// </summary>
/// <remarks>
/// Block Charges fuel defensive reactions:
/// <list type="bullet">
/// <item>Intercept: 1 charge</item>
/// <item>Guardian's Sacrifice: 2 charges</item>
/// </list>
/// Charges restore fully on any rest (short or long).
/// </remarks>
public interface IBlockChargeService
{
    /// <summary>
    /// Attempts to spend the specified number of Block Charges.
    /// </summary>
    /// <param name="player">The Skjaldmær player.</param>
    /// <param name="amount">Number of charges to spend.</param>
    /// <returns>True if charges were successfully spent; false if insufficient.</returns>
    bool SpendCharges(Player player, int amount);

    /// <summary>
    /// Restores the specified number of Block Charges.
    /// </summary>
    /// <param name="player">The Skjaldmær player.</param>
    /// <param name="amount">Number of charges to restore.</param>
    void RestoreCharges(Player player, int amount);

    /// <summary>
    /// Restores all Block Charges to maximum.
    /// </summary>
    /// <param name="player">The Skjaldmær player.</param>
    void RestoreAllCharges(Player player);

    /// <summary>
    /// Checks if player can spend the specified number of charges.
    /// </summary>
    /// <param name="player">The Skjaldmær player.</param>
    /// <param name="amount">Number of charges to check.</param>
    /// <returns>True if sufficient charges are available.</returns>
    bool CanSpend(Player player, int amount);

    /// <summary>
    /// Gets the current number of Block Charges.
    /// </summary>
    /// <param name="player">The Skjaldmær player.</param>
    /// <returns>Current charges, or 0 if player has no BlockChargeResource.</returns>
    int GetCurrentValue(Player player);

    /// <summary>
    /// Gets the maximum Block Charges for this player.
    /// </summary>
    /// <param name="player">The Skjaldmær player.</param>
    /// <returns>Max charges, or 0 if player has no BlockChargeResource.</returns>
    int GetMaxValue(Player player);
}
