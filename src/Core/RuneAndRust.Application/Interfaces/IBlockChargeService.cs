// ═══════════════════════════════════════════════════════════════════════════════
// IBlockChargeService.cs
// Interface for authoritative Block Charge resource management.
// Version: 0.20.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages Block Charge resource operations for the Skjaldmær specialization.
/// </summary>
/// <remarks>
/// <para>
/// This service is the authoritative manager of Block Charge state. All charge
/// spending and restoration operations should go through this service to ensure
/// proper validation, logging, and state consistency.
/// </para>
/// </remarks>
public interface IBlockChargeService
{
    /// <summary>
    /// Attempts to spend charges from the resource.
    /// </summary>
    /// <param name="resource">Current charge state.</param>
    /// <param name="amount">Number of charges to spend.</param>
    /// <param name="characterId">Character ID for logging.</param>
    /// <param name="characterName">Character name for logging.</param>
    /// <returns>
    /// A tuple of (success, updatedResource). If insufficient charges,
    /// returns (false, original resource unchanged).
    /// </returns>
    (bool Success, BlockChargeResource Resource) SpendCharges(
        BlockChargeResource resource,
        int amount,
        Guid characterId,
        string characterName);

    /// <summary>
    /// Restores a specified number of charges.
    /// </summary>
    /// <param name="resource">Current charge state.</param>
    /// <param name="amount">Number of charges to restore.</param>
    /// <param name="characterId">Character ID for logging.</param>
    /// <param name="characterName">Character name for logging.</param>
    /// <returns>Updated resource with charges restored (capped at max).</returns>
    BlockChargeResource RestoreCharges(
        BlockChargeResource resource,
        int amount,
        Guid characterId,
        string characterName);

    /// <summary>
    /// Fully restores charges to maximum. Typically called after rest.
    /// </summary>
    /// <param name="resource">Current charge state.</param>
    /// <param name="characterId">Character ID for logging.</param>
    /// <param name="characterName">Character name for logging.</param>
    /// <returns>Resource at full capacity.</returns>
    BlockChargeResource RestoreAllCharges(
        BlockChargeResource resource,
        Guid characterId,
        string characterName);

    /// <summary>
    /// Checks if the resource has sufficient charges without modifying state.
    /// </summary>
    /// <param name="resource">Current charge state.</param>
    /// <param name="amount">Number of charges required.</param>
    /// <returns>True if charges are sufficient.</returns>
    bool CanSpend(BlockChargeResource resource, int amount);

    /// <summary>
    /// Calculates the Bulwark passive HP bonus for the given resource.
    /// </summary>
    /// <param name="resource">Current charge state.</param>
    /// <returns>HP bonus based on current charges.</returns>
    int CalculateBulwarkBonus(BlockChargeResource resource);
}
