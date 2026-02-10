// ═══════════════════════════════════════════════════════════════════════════════
// IAbilitySlotService.cs
// Interface for managing the tiered ability slot structure for character
// specializations in the Aethelgard system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages the tiered ability slot structure for character specializations.
/// </summary>
/// <remarks>
/// <para>
/// The ability slot system uses a fixed 4-tier layout per specialization:
/// </para>
/// <list type="bullet">
///   <item><description><b>Tier 1:</b> 3 slots — Free (0 PP each), unlocked immediately</description></item>
///   <item><description><b>Tier 2:</b> 3 slots — 4 PP each, requires 8 PP invested</description></item>
///   <item><description><b>Tier 3:</b> 2 slots — 5 PP each, requires 16 PP invested</description></item>
///   <item><description><b>Capstone:</b> 1 slot — 6 PP, requires 24 PP invested</description></item>
/// </list>
/// <para>
/// Slots are initialized when a specialization is selected and tier
/// unlocking is tracked via PP investment thresholds.
/// </para>
/// </remarks>
/// <seealso cref="AbilitySlotPreparation"/>
/// <seealso cref="SpecializationId"/>
public interface IAbilitySlotService
{
    /// <summary>
    /// Initializes the ability slot structure for a character's specialization.
    /// Creates slots for all four tiers with standard counts and costs.
    /// </summary>
    /// <param name="characterId">The character to initialize slots for.</param>
    /// <param name="specialization">The specialization to create slots for.</param>
    /// <returns>
    /// The initialized <see cref="AbilitySlotPreparation"/> with the standard
    /// 3+3+2+1 slot configuration.
    /// </returns>
    AbilitySlotPreparation InitializeAbilitySlots(Guid characterId, SpecializationId specialization);

    /// <summary>
    /// Retrieves the current ability slot configuration for a character's
    /// specialization.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <param name="specialization">The specialization to query.</param>
    /// <returns>
    /// The <see cref="AbilitySlotPreparation"/> if slots have been initialized;
    /// <c>null</c> otherwise.
    /// </returns>
    AbilitySlotPreparation? GetAbilitySlots(Guid characterId, SpecializationId specialization);

    /// <summary>
    /// Checks whether a specific tier is unlocked for a character's specialization
    /// based on their current PP investment.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="specialization">The specialization to check.</param>
    /// <param name="tier">The tier number (1-4) to check.</param>
    /// <returns>
    /// <c>true</c> if the tier is unlocked; <c>false</c> otherwise.
    /// Tier 1 is always unlocked.
    /// </returns>
    bool IsTierUnlocked(Guid characterId, SpecializationId specialization, int tier);

    /// <summary>
    /// Returns the PP cost to unlock the next locked tier for a character's
    /// specialization.
    /// </summary>
    /// <param name="characterId">The character to calculate for.</param>
    /// <param name="specialization">The specialization to calculate for.</param>
    /// <returns>
    /// The PP cost of the next tier. 0 if all tiers are unlocked.
    /// </returns>
    int GetNextTierUnlockCost(Guid characterId, SpecializationId specialization);
}
