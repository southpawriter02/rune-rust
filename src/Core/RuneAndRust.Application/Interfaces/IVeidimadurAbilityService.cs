using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Veiðimaðr (Hunter) specialization ability execution.
/// Handles Tier 1 (Foundation) ability logic including quarry marking,
/// perception bonuses, and track investigation.
/// </summary>
/// <remarks>
/// <para>The Veiðimaðr is a Coherent path Skirmisher with zero Corruption risk.
/// All ability methods follow the guard-clause chain pattern:</para>
/// <list type="number">
/// <item>Null check on player parameter</item>
/// <item>Specialization validation (must be Veiðimaðr)</item>
/// <item>Ability unlock check</item>
/// <item>AP cost validation</item>
/// <item>No Corruption evaluation (Coherent path)</item>
/// <item>Execute ability logic</item>
/// </list>
/// <para>Methods return null when prerequisites are not met (wrong specialization,
/// ability not unlocked, insufficient AP). Non-null results indicate successful execution.</para>
/// <para>Introduced in v0.20.7a. Tier 2 abilities will be added in v0.20.7b,
/// Tier 3 and Capstone in v0.20.7c.</para>
/// </remarks>
public interface IVeidimadurAbilityService
{
    // ===== Tier 1 Abilities (v0.20.7a) =====

    /// <summary>
    /// Executes the Mark Quarry ability: designates a visible enemy as quarry,
    /// granting +2 to all attack rolls against that target.
    /// </summary>
    /// <param name="player">The Veiðimaðr player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target to mark.</param>
    /// <param name="targetName">Display name of the target.</param>
    /// <param name="encounterId">Optional encounter ID if marking during combat.</param>
    /// <returns>
    /// A <see cref="MarkQuarryResult"/> containing the created mark and replacement info,
    /// or null if prerequisites are not met.
    /// </returns>
    MarkQuarryResult? ExecuteMarkQuarry(
        Player player,
        Guid targetId,
        string targetName,
        Guid? encounterId = null);

    /// <summary>
    /// Executes the Read the Signs investigation ability: examines creature tracks or remains
    /// to reveal information about creatures in the area.
    /// </summary>
    /// <param name="player">The Veiðimaðr player executing the ability.</param>
    /// <param name="locationDescription">Description of the area being investigated.</param>
    /// <param name="trackQuality">Freshness/quality of the tracks (determines DC).</param>
    /// <param name="creatureType">Creature type data to reveal on success. Null if unknown.</param>
    /// <param name="creatureCount">Creature count data to reveal on success. Null if unknown.</param>
    /// <param name="timePassedEstimate">Time estimate data to reveal on success. Null if unknown.</param>
    /// <param name="directionOfTravel">Direction data to reveal on success. Null if unknown.</param>
    /// <param name="creatureCondition">Condition data to reveal on success. Null if unknown.</param>
    /// <returns>
    /// A <see cref="ReadTheSignsResult"/> containing the investigation findings,
    /// or null if prerequisites are not met.
    /// </returns>
    ReadTheSignsResult? ExecuteReadTheSigns(
        Player player,
        string locationDescription,
        CreatureTrackType trackQuality,
        string? creatureType = null,
        int? creatureCount = null,
        string? timePassedEstimate = null,
        string? directionOfTravel = null,
        string? creatureCondition = null);

    /// <summary>
    /// Gets the Keen Senses passive bonus value for the player.
    /// Returns +1 if Keen Senses is unlocked, 0 otherwise.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>1 if Keen Senses is unlocked, 0 otherwise.</returns>
    /// <remarks>
    /// Keen Senses is a passive ability — no AP cost, no execution.
    /// The +1 bonus applies to all Perception and Investigation checks.
    /// Called inline by other abilities (e.g., Read the Signs) and the perception system.
    /// </remarks>
    int GetKeenSensesBonus(Player player);

    // ===== Utility Methods =====

    /// <summary>
    /// Gets a dictionary indicating whether each Veiðimaðr ability is currently usable
    /// (sufficient AP, resources, and prerequisites met).
    /// </summary>
    /// <param name="player">The player to check readiness for.</param>
    /// <returns>A dictionary mapping each unlocked ability ID to its readiness state.</returns>
    Dictionary<VeidimadurAbilityId, bool> GetAbilityReadiness(Player player);

    /// <summary>
    /// Checks if the player meets the PP requirement to unlock Tier 2 abilities (8 PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has at least 8 PP invested in Veiðimaðr abilities.</returns>
    bool CanUnlockTier2(Player player);

    /// <summary>
    /// Checks if the player meets the PP requirement to unlock Tier 3 abilities (16 PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has at least 16 PP invested in Veiðimaðr abilities.</returns>
    bool CanUnlockTier3(Player player);

    /// <summary>
    /// Checks if the player meets the PP requirement to unlock the Capstone ability (24 PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has at least 24 PP invested in Veiðimaðr abilities.</returns>
    bool CanUnlockCapstone(Player player);

    /// <summary>
    /// Gets the total Progression Points invested in Veiðimaðr abilities.
    /// Delegates to <see cref="Player.GetVeidimadurPPInvested"/>.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>Total PP invested.</returns>
    int GetPPInvested(Player player);
}
