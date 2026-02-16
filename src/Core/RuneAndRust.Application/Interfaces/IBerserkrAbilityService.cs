using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Berserkr specialization ability execution.
/// Handles Tier 1 (Foundation) ability logic including Fury Strike,
/// Blood Scent detection, and Pain is Fuel passive processing.
/// </summary>
/// <remarks>
/// <para>Tier 1 abilities (0 PP required):</para>
/// <list type="bullet">
/// <item>Fury Strike (Active): weapon + 3d6 damage, costs 2 AP + 20 Rage, nat 20 = +1d6</item>
/// <item>Blood Scent (Passive): +10 Rage on enemy bloodied, +1 Attack vs bloodied targets</item>
/// <item>Pain is Fuel (Passive): +5 Rage per damage instance received</item>
/// </list>
/// <para>This interface will be extended in v0.20.5b with Tier 2 ability methods
/// following the same pattern as <see cref="IRunasmidrAbilityService"/>.</para>
/// </remarks>
public interface IBerserkrAbilityService
{
    /// <summary>
    /// Executes the Fury Strike active ability against a target.
    /// </summary>
    /// <param name="player">The Berserkr player executing the strike.</param>
    /// <param name="targetId">Unique identifier of the target being attacked.</param>
    /// <returns>
    /// A <see cref="FuryStrikeResult"/> containing the full damage breakdown and Corruption status
    /// if the strike was successfully executed; null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Berserkr specialization, Fury Strike unlocked, 2+ AP, 20+ Rage.</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP, Rage)</item>
    /// <item>Evaluate Corruption risk BEFORE spending resources</item>
    /// <item>Spend 2 AP and 20 Rage</item>
    /// <item>Roll attack (d20) and damage (weapon + 3d6, +1d6 on crit)</item>
    /// <item>Return complete result with Corruption status</item>
    /// </list>
    /// </remarks>
    FuryStrikeResult? UseFuryStrike(Player player, Guid targetId);

    /// <summary>
    /// Checks and processes the Blood Scent passive when an enemy takes damage.
    /// Detects the transition to bloodied state and grants +10 Rage if applicable.
    /// </summary>
    /// <param name="player">The Berserkr player with Blood Scent.</param>
    /// <param name="targetId">Unique identifier of the damaged target.</param>
    /// <param name="targetName">Display name of the damaged target.</param>
    /// <param name="previousHp">Target's HP before the damage was applied.</param>
    /// <param name="currentHp">Target's HP after the damage was applied.</param>
    /// <param name="maxHp">Target's maximum HP.</param>
    /// <returns>
    /// A <see cref="BloodiedState"/> snapshot if the target just became bloodied;
    /// null if Blood Scent is not unlocked or the target was already bloodied.
    /// </returns>
    BloodiedState? CheckBloodScent(
        Player player,
        Guid targetId,
        string targetName,
        int previousHp,
        int currentHp,
        int maxHp);

    /// <summary>
    /// Processes the Pain is Fuel passive when the Berserkr takes damage.
    /// Grants +5 Rage per damage instance.
    /// </summary>
    /// <param name="player">The Berserkr player that took damage.</param>
    /// <param name="damageTaken">The amount of damage taken. Must be positive to trigger gain.</param>
    /// <returns>The amount of Rage gained (0 if Pain is Fuel is not unlocked or damage is 0).</returns>
    int CheckPainIsFuel(Player player, int damageTaken);

    /// <summary>
    /// Gets a readiness summary for all Berserkr abilities for the specified player.
    /// Used for UI display to show which abilities are available.
    /// </summary>
    /// <param name="player">The Berserkr player to check.</param>
    /// <returns>
    /// A dictionary mapping each unlocked <see cref="BerserkrAbilityId"/> to a boolean
    /// indicating whether the ability can currently be used (sufficient AP, Rage, etc.).
    /// </returns>
    Dictionary<BerserkrAbilityId, bool> GetAbilityReadiness(Player player);

    /// <summary>
    /// Checks if the player meets Tier 2 unlock requirements (8+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 8 or more PP invested in the Berserkr tree.</returns>
    bool CanUnlockTier2(Player player);

    /// <summary>
    /// Checks if the player meets Tier 3 unlock requirements (16+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 16 or more PP invested in the Berserkr tree.</returns>
    bool CanUnlockTier3(Player player);

    /// <summary>
    /// Checks if the player meets Capstone unlock requirements (24+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 24 or more PP invested in the Berserkr tree.</returns>
    bool CanUnlockCapstone(Player player);

    /// <summary>
    /// Gets total Progression Points invested in the Berserkr ability tree.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>Total PP invested.</returns>
    int GetPPInvested(Player player);

    // ===== Tier 2 Abilities (v0.20.5b) =====

    /// <summary>
    /// Toggles the Reckless Assault stance. If the player is not currently in the stance,
    /// enters it (costing 1 AP). If already in the stance, exits it (free action).
    /// </summary>
    /// <param name="player">The Berserkr player toggling the stance.</param>
    /// <returns>
    /// A <see cref="RecklessAssaultState"/> representing the new stance state if entering;
    /// null if exiting the stance or if prerequisites are not met.
    /// </returns>
    /// <remarks>
    /// <para>While active: +4 Attack (scales +1 per 20 Rage), -2 Defense.</para>
    /// <para>Per-turn Corruption: +1 each turn at 80+ Rage.</para>
    /// <para>Prerequisites: Berserkr, Reckless Assault unlocked, Tier 2 (8 PP), 1 AP to enter.</para>
    /// </remarks>
    RecklessAssaultState? ExecuteRecklessAssault(Player player);

    /// <summary>
    /// Activates the Unstoppable ability, granting immunity to all movement penalties
    /// for 2 turns.
    /// </summary>
    /// <param name="player">The Berserkr player activating the ability.</param>
    /// <returns>
    /// An <see cref="UnstoppableEffect"/> representing the active effect if successful;
    /// null if prerequisites are not met or effect is already active.
    /// </returns>
    /// <remarks>
    /// <para>Cost: 1 AP, 15 Rage.</para>
    /// <para>Corruption: +1 if activated at 80+ Rage.</para>
    /// <para>Prerequisites: Berserkr, Unstoppable unlocked, Tier 2 (8 PP).</para>
    /// </remarks>
    UnstoppableEffect? ExecuteUnstoppable(Player player);

    /// <summary>
    /// Executes Intimidating Presence, forcing Will saves on all eligible enemies
    /// within range.
    /// </summary>
    /// <param name="player">The Berserkr player using the ability.</param>
    /// <param name="targets">
    /// List of targets with their save information:
    /// (targetId, targetName, willSaveRoll, isCoherent, isMindless, hasFearImmunity).
    /// </param>
    /// <returns>
    /// A list of <see cref="IntimidationEffect"/> results for each target processed;
    /// empty if prerequisites are not met or no valid targets.
    /// </returns>
    /// <remarks>
    /// <para>Cost: 2 AP, 10 Rage.</para>
    /// <para>Save DC: 12 + (Rage / 20).</para>
    /// <para>On fail: -2 Attack for 3 turns. On success: 24h immunity.</para>
    /// <para>Corruption: +1 per Coherent-aligned target in range.</para>
    /// <para>Prerequisites: Berserkr, Intimidating Presence unlocked, Tier 2 (8 PP).</para>
    /// </remarks>
    List<IntimidationEffect> ExecuteIntimidatingPresence(
        Player player,
        List<(Guid targetId, string targetName, int willSaveRoll, bool isCoherent, bool isMindless, bool hasFearImmunity)> targets);

    /// <summary>
    /// Gets the current Reckless Assault stance state for a player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>The active stance state, or null if not in the stance.</returns>
    RecklessAssaultState? GetRecklessAssaultState(Player player);

    /// <summary>
    /// Gets the current Unstoppable effect for a player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>The active Unstoppable effect, or null if not active.</returns>
    UnstoppableEffect? GetUnstoppableEffect(Player player);
}
