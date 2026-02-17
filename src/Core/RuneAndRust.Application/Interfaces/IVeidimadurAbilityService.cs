using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Veiðimaðr (Hunter) specialization ability execution.
/// Handles Tier 1 (Foundation) and Tier 2 (Discipline) ability logic including quarry marking,
/// perception bonuses, track investigation, cover bypass, traps, and stance management.
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
/// <para>Introduced in v0.20.7a (Tier 1). Tier 2 abilities added in v0.20.7b.
/// Tier 3 and Capstone will be added in v0.20.7c.</para>
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

    // ===== Tier 2 Abilities (v0.20.7b) =====

    /// <summary>
    /// Evaluates the Hunter's Eye passive ability for a ranged attack against a covered target.
    /// Hunter's Eye ignores <see cref="CoverType.Partial"/> cover but cannot bypass
    /// <see cref="CoverType.Full"/> cover.
    /// </summary>
    /// <param name="player">The Veiðimaðr player making the attack.</param>
    /// <param name="targetId">Unique identifier of the target being attacked.</param>
    /// <param name="targetName">Display name of the target.</param>
    /// <param name="targetCover">The cover type the target currently has.</param>
    /// <param name="distance">Distance to the target in spaces.</param>
    /// <returns>
    /// A <see cref="HuntersEyeResult"/> indicating whether cover was ignored and any bonus gained,
    /// or null if prerequisites are not met.
    /// </returns>
    /// <remarks>
    /// Hunter's Eye is a passive ability — no AP cost. The method evaluates
    /// whether cover should be bypassed for a specific attack context.
    /// </remarks>
    HuntersEyeResult? ExecuteHuntersEye(
        Player player,
        Guid targetId,
        string targetName,
        CoverType targetCover,
        int distance);

    /// <summary>
    /// Executes the Trap Mastery ability in placement mode: places a hunting trap at the
    /// specified location. Costs 2 AP. Maximum 2 armed traps at once.
    /// </summary>
    /// <param name="player">The Veiðimaðr player placing the trap.</param>
    /// <param name="x">X coordinate for trap placement.</param>
    /// <param name="y">Y coordinate for trap placement.</param>
    /// <param name="trapType">The type of trap to place.</param>
    /// <returns>
    /// A <see cref="TrapMasteryResult"/> indicating success or failure,
    /// or null if prerequisites are not met (wrong spec, ability not unlocked, insufficient AP).
    /// </returns>
    TrapMasteryResult? ExecutePlaceTrap(
        Player player,
        int x,
        int y,
        TrapType trapType);

    /// <summary>
    /// Executes the Trap Mastery ability in detection mode: scans the area for nearby traps.
    /// Costs 2 AP. Perception check: 1d20 + Trap Mastery bonus (+3) + Keen Senses (+1 if unlocked) vs DC 13.
    /// </summary>
    /// <param name="player">The Veiðimaðr player detecting traps.</param>
    /// <param name="centerX">X coordinate of the detection center.</param>
    /// <param name="centerY">Y coordinate of the detection center.</param>
    /// <returns>
    /// A <see cref="TrapMasteryResult"/> indicating how many traps were detected,
    /// or null if prerequisites are not met.
    /// </returns>
    TrapMasteryResult? ExecuteDetectTraps(
        Player player,
        int centerX,
        int centerY);

    /// <summary>
    /// Activates the Predator's Patience stance. Costs 1 AP.
    /// While active, the hunter gains +3 to all attack rolls as long as they do not move.
    /// </summary>
    /// <param name="player">The Veiðimaðr player activating the stance.</param>
    /// <returns>
    /// The activated <see cref="PredatorsPatienceState"/>,
    /// or null if prerequisites are not met.
    /// </returns>
    PredatorsPatienceState? ActivatePredatorsPatience(Player player);

    /// <summary>
    /// Deactivates the Predator's Patience stance. No AP cost to exit.
    /// </summary>
    /// <param name="player">The Veiðimaðr player deactivating the stance.</param>
    /// <returns>True if the stance was successfully deactivated; false if not active or prerequisites not met.</returns>
    bool DeactivatePredatorsPatience(Player player);

    /// <summary>
    /// Gets the current hit bonus from Predator's Patience.
    /// Returns +3 if the stance is active and the hunter has not moved, 0 otherwise.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>The current hit bonus from the stance (0 or +3).</returns>
    int GetPredatorsPatienceBonus(Player player);

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
