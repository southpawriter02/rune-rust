// ═══════════════════════════════════════════════════════════════════════════════
// IRuneChargeService.cs
// Interface defining the contract for managing Rune Charges, the special
// resource of the Rúnasmiðr specialization.
// Version: 0.20.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines the contract for managing Rune Charges for the Rúnasmiðr specialization.
/// </summary>
/// <remarks>
/// <para>
/// The Rune Charge service orchestrates resource spending, generation, and
/// restoration with structured logging for all operations. It wraps the
/// immutable <see cref="RuneChargeResource"/> value object, providing
/// validation, logging, and error handling.
/// </para>
/// <para>
/// Key operations:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>SpendCharges:</b> Deducts charges when using abilities. Logs warnings
///     on insufficient charges.
///   </description></item>
///   <item><description>
///     <b>GenerateFromCraft:</b> Adds charges after crafting actions (1 standard,
///     2 complex). Logs generation events.
///   </description></item>
///   <item><description>
///     <b>RestoreCharges / RestoreAllCharges:</b> Partial or full restoration
///     (e.g., short rest = 1 charge, long rest = full). Logs restoration events.
///   </description></item>
///   <item><description>
///     <b>CanSpend:</b> Read-only check for ability cost validation without
///     side effects or logging.
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="RuneChargeResource"/>
public interface IRuneChargeService
{
    /// <summary>
    /// Spends the specified number of Rune Charges for an ability activation.
    /// </summary>
    /// <param name="resource">The current Rune Charge resource state.</param>
    /// <param name="amount">Number of charges to spend. Must be positive.</param>
    /// <param name="characterId">ID of the character spending charges.</param>
    /// <param name="characterName">Display name of the character (for logging).</param>
    /// <returns>
    /// A tuple of (success, updatedResource). If insufficient charges,
    /// returns (false, original resource) and logs a warning.
    /// </returns>
    (bool Success, RuneChargeResource Resource) SpendCharges(
        RuneChargeResource resource,
        int amount,
        Guid characterId,
        string characterName);

    /// <summary>
    /// Generates Rune Charges from a crafting action.
    /// </summary>
    /// <param name="resource">The current Rune Charge resource state.</param>
    /// <param name="isComplexCraft">Whether the craft is complex (2 charges) or standard (1 charge).</param>
    /// <param name="characterId">ID of the character generating charges.</param>
    /// <param name="characterName">Display name of the character (for logging).</param>
    /// <returns>The updated resource with generated charges.</returns>
    RuneChargeResource GenerateFromCraft(
        RuneChargeResource resource,
        bool isComplexCraft,
        Guid characterId,
        string characterName);

    /// <summary>
    /// Restores the specified number of Rune Charges (e.g., from short rest).
    /// </summary>
    /// <param name="resource">The current Rune Charge resource state.</param>
    /// <param name="amount">Number of charges to restore. Must be positive.</param>
    /// <param name="characterId">ID of the character restoring charges.</param>
    /// <param name="characterName">Display name of the character (for logging).</param>
    /// <returns>The updated resource with restored charges.</returns>
    RuneChargeResource RestoreCharges(
        RuneChargeResource resource,
        int amount,
        Guid characterId,
        string characterName);

    /// <summary>
    /// Fully restores Rune Charges to maximum capacity (e.g., on long rest).
    /// </summary>
    /// <param name="resource">The current Rune Charge resource state.</param>
    /// <param name="characterId">ID of the character restoring charges.</param>
    /// <param name="characterName">Display name of the character (for logging).</param>
    /// <returns>The updated resource at full capacity.</returns>
    RuneChargeResource RestoreAllCharges(
        RuneChargeResource resource,
        Guid characterId,
        string characterName);

    /// <summary>
    /// Checks whether the specified number of charges can be spent.
    /// Read-only operation — does not modify state or produce log output.
    /// </summary>
    /// <param name="resource">The current Rune Charge resource state.</param>
    /// <param name="amount">Number of charges to check. Must be positive.</param>
    /// <returns><c>true</c> if sufficient charges are available.</returns>
    bool CanSpend(RuneChargeResource resource, int amount);
}
