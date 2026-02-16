using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Bone-Setter specialization ability execution.
/// Handles Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) ability logic including healing, diagnosis,
/// crafting, tactical triage, revival, aura protection, and miraculous restoration.
/// </summary>
/// <remarks>
/// <para>The Bone-Setter is the first dedicated healing specialization and the first
/// Coherent (no Corruption) path in the v0.20.x series. All abilities operate without
/// Corruption risk evaluation, unlike Berserkr abilities.</para>
/// <para>Tier 1 abilities (0 PP required):</para>
/// <list type="bullet">
/// <item>Field Dressing (Active): 2d6 + quality bonus healing, costs 2 AP + 1 supply</item>
/// <item>Diagnose (Active): Reveals target HP, wound severity, status effects, costs 1 AP</item>
/// <item>Steady Hands (Passive): +2 to all healing rolls when unlocked</item>
/// </list>
/// <para>Tier 2 abilities (4 PP each, 8 PP invested required):</para>
/// <list type="bullet">
/// <item>Emergency Surgery (Active): 4d6 + quality + recovery bonus healing, costs 3 AP + 1 supply</item>
/// <item>Antidote Craft (Active): Creates Antidote from Herbs + crafting materials, costs 2 AP</item>
/// <item>Triage (Passive): +50% healing bonus to most wounded ally within 5-space radius</item>
/// </list>
/// <para>Tier 3 abilities (5 PP each, 16 PP invested required):</para>
/// <list type="bullet">
/// <item>Resuscitate (Active): Revives unconscious target to 1 HP, costs 4 AP + 2 supplies</item>
/// <item>Preventive Care (Passive): Aura granting +1 poison/disease saves within 5 spaces</item>
/// </list>
/// <para>Capstone ability (6 PP, 24 PP invested required):</para>
/// <list type="bullet">
/// <item>Miracle Worker (Active): Full HP restore + clear all conditions, costs 5 AP, once per long rest</item>
/// </list>
/// </remarks>
public interface IBoneSetterAbilityService
{
    // ===== Tier 1 Abilities (v0.20.6a) =====

    /// <summary>
    /// Executes the Field Dressing active healing ability on a target.
    /// </summary>
    /// <param name="player">The Bone-Setter player performing the healing.</param>
    /// <param name="targetId">Unique identifier of the target being healed.</param>
    /// <param name="targetName">Display name of the target being healed.</param>
    /// <param name="targetCurrentHp">Target's current HP before healing.</param>
    /// <param name="targetMaxHp">Target's maximum HP (healing is capped at this value).</param>
    /// <returns>
    /// A <see cref="FieldDressingResult"/> containing the full healing breakdown
    /// if the ability was successfully executed; null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Bone-Setter specialization, Field Dressing unlocked, 2+ AP,
    /// at least 1 Medical Supply available.</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP, supplies)</item>
    /// <item>Spend 2 AP and 1 Medical Supply (lowest quality first)</item>
    /// <item>Roll healing (2d6)</item>
    /// <item>Calculate quality bonus from consumed supply (Quality - 1)</item>
    /// <item>Add Steady Hands bonus (+2) if passive is unlocked</item>
    /// <item>Apply healing capped at target's max HP</item>
    /// <item>Return complete result with healing breakdown</item>
    /// </list>
    /// <para>No Corruption risk — Field Dressing follows the Coherent path.</para>
    /// </remarks>
    FieldDressingResult? ExecuteFieldDressing(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp,
        int targetMaxHp);

    /// <summary>
    /// Executes the Diagnose active ability to analyze a target's health status.
    /// </summary>
    /// <param name="player">The Bone-Setter player performing the diagnosis.</param>
    /// <param name="targetId">Unique identifier of the target being diagnosed.</param>
    /// <param name="targetName">Display name of the target being diagnosed.</param>
    /// <param name="targetCurrentHp">Target's current HP at time of diagnosis.</param>
    /// <param name="targetMaxHp">Target's maximum HP.</param>
    /// <param name="statusEffects">Collection of active status effects on the target.</param>
    /// <param name="vulnerabilities">Collection of known vulnerabilities.</param>
    /// <param name="resistances">Collection of known resistances.</param>
    /// <returns>
    /// A <see cref="DiagnoseResult"/> containing the complete diagnostic report
    /// if the ability was successfully executed; null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Bone-Setter specialization, Diagnose unlocked, 1+ AP.</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP)</item>
    /// <item>Spend 1 AP (no supply cost)</item>
    /// <item>Classify wound severity from HP percentage</item>
    /// <item>Build diagnostic report with all target information</item>
    /// <item>Return complete result</item>
    /// </list>
    /// <para>No Corruption risk — Diagnose follows the Coherent path.</para>
    /// </remarks>
    DiagnoseResult? ExecuteDiagnose(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp,
        int targetMaxHp,
        IEnumerable<string> statusEffects,
        IEnumerable<string> vulnerabilities,
        IEnumerable<string> resistances);

    // ===== Tier 2 Abilities (v0.20.6b) =====

    /// <summary>
    /// Executes the Emergency Surgery active healing ability on a target.
    /// High-impact healing (4d6) with recovery condition bonus for critical targets.
    /// </summary>
    /// <param name="player">The Bone-Setter player performing the surgery.</param>
    /// <param name="targetId">Unique identifier of the target being healed.</param>
    /// <param name="targetName">Display name of the target being healed.</param>
    /// <param name="targetCurrentHp">Target's current HP before healing.</param>
    /// <param name="targetMaxHp">Target's maximum HP (healing is capped at this value).</param>
    /// <param name="targetCondition">Target's current recovery condition for bonus calculation.</param>
    /// <returns>
    /// An <see cref="EmergencySurgeryResult"/> containing the full healing breakdown
    /// including recovery bonus if applicable; null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Bone-Setter specialization, Emergency Surgery unlocked, 3+ AP,
    /// at least 1 Medical Supply available.</para>
    /// <para>Recovery bonus by condition:</para>
    /// <list type="bullet">
    /// <item>Active: +0 (no bonus)</item>
    /// <item>Incapacitated: +1</item>
    /// <item>Recovering: +3</item>
    /// <item>Dying: +4 (maximum bonus)</item>
    /// </list>
    /// <para>Uses highest-quality supply available (opposite of Field Dressing which uses lowest).</para>
    /// <para>No Corruption risk — Emergency Surgery follows the Coherent path.</para>
    /// </remarks>
    EmergencySurgeryResult? ExecuteEmergencySurgery(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp,
        int targetMaxHp,
        RecoveryCondition targetCondition);

    /// <summary>
    /// Executes the Antidote Craft active ability to create an Antidote from materials.
    /// Always succeeds if prerequisites are met (100% success rate, no DC check).
    /// </summary>
    /// <param name="player">The Bone-Setter player performing the crafting.</param>
    /// <param name="availableMaterials">Array of crafting materials available to the player.</param>
    /// <returns>
    /// An <see cref="AntidoteCraftResult"/> containing the crafting outcome;
    /// null if ability prerequisites were not met (wrong spec, ability not unlocked, insufficient AP).
    /// Returns a failure result (not null) if materials are insufficient.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Bone-Setter specialization, Antidote Craft unlocked, 2+ AP,
    /// at least 1 Herbs supply, 2+ Plant Fiber, 1+ Mineral Powder.</para>
    /// <para>Output quality: Min(Herbs Quality + material bonus, 5).</para>
    /// <para>Material bonus: +1 if all crafting materials are Quality 3+, otherwise +0.</para>
    /// <para>No Corruption risk — Antidote Craft follows the Coherent path.</para>
    /// </remarks>
    AntidoteCraftResult? ExecuteAntidoteCraft(
        Player player,
        CraftingMaterial[] availableMaterials);

    /// <summary>
    /// Evaluates the Triage passive ability to determine bonus healing for the most wounded ally.
    /// Called by other healing abilities to check if Triage bonus should be applied.
    /// </summary>
    /// <param name="player">The Bone-Setter player with Triage passive.</param>
    /// <param name="alliesInRadius">Array of allies within the 5-space Triage radius.</param>
    /// <param name="baseHealing">The base healing amount from the executing ability.</param>
    /// <returns>
    /// A <see cref="TriageResult"/> with the most wounded target and bonus calculation;
    /// null if Triage is not unlocked, player is not a Bone-Setter, or no allies are in radius.
    /// </returns>
    /// <remarks>
    /// <para>Triage is a passive ability — always active when unlocked, no AP or supply cost.</para>
    /// <para>Bonus: +50% healing (×1.5 multiplier, rounded down) to the most wounded ally
    /// (lowest HP percentage) within 5-space radius.</para>
    /// </remarks>
    TriageResult? EvaluateTriage(
        Player player,
        TriageTarget[] alliesInRadius,
        int baseHealing);

    // ===== Tier 3 Abilities (v0.20.6c) =====

    /// <summary>
    /// Executes the Resuscitate active ability to revive an unconscious target to 1 HP.
    /// Emergency revival that brings a downed ally back to consciousness at minimal health.
    /// </summary>
    /// <param name="player">The Bone-Setter player performing the resuscitation.</param>
    /// <param name="targetId">Unique identifier of the unconscious target.</param>
    /// <param name="targetName">Display name of the unconscious target.</param>
    /// <param name="targetCurrentHp">Target's current HP (must be 0 for unconscious).</param>
    /// <returns>
    /// A <see cref="ResuscitateResult"/> containing the revival outcome
    /// if the ability was successfully executed; null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Bone-Setter specialization, Resuscitate unlocked, 4+ AP,
    /// at least 2 Medical Supplies available, target must be at 0 HP (unconscious).</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP, target HP == 0, supplies ≥ 2)</item>
    /// <item>Spend 4 AP and 2 Medical Supplies (lowest quality first, sequential)</item>
    /// <item>Set target HP to 1 (barely conscious)</item>
    /// <item>Return result with revival details</item>
    /// </list>
    /// <para>No Corruption risk — Resuscitate follows the Coherent path.</para>
    /// <para>No cooldown — can be used unlimited times per combat if supplies allow.</para>
    /// </remarks>
    ResuscitateResult? ExecuteResuscitate(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp);

    /// <summary>
    /// Evaluates the Preventive Care passive aura to identify allies receiving protection.
    /// Pure evaluation with no AP or supply cost — always active when unlocked.
    /// </summary>
    /// <param name="player">The Bone-Setter player providing the aura.</param>
    /// <param name="allyIdsInRadius">Array of ally identifiers within the 5-space aura radius.</param>
    /// <returns>
    /// A <see cref="PreventiveCareAura"/> describing the aura's current effect and affected allies;
    /// null if the ability is not unlocked, player is not a Bone-Setter, or no allies are in radius.
    /// </returns>
    /// <remarks>
    /// <para>Preventive Care is a passive ability — always active when unlocked, no AP or supply cost.</para>
    /// <para>All allies within 5 spaces receive +1 to saving throws against poison and disease.</para>
    /// <para>No Corruption risk — Preventive Care follows the Coherent path.</para>
    /// </remarks>
    PreventiveCareAura? EvaluatePreventiveCare(
        Player player,
        Guid[] allyIdsInRadius);

    // ===== Capstone Ability (v0.20.6c) =====

    /// <summary>
    /// Executes the Miracle Worker capstone ability for full HP restoration and condition clearing.
    /// The pinnacle of the Bone-Setter's medical art — a once-per-long-rest miracle.
    /// </summary>
    /// <param name="player">The Bone-Setter player performing the miracle.</param>
    /// <param name="targetId">Unique identifier of the target being healed.</param>
    /// <param name="targetName">Display name of the target being healed.</param>
    /// <param name="targetCurrentHp">Target's current HP before miracle.</param>
    /// <param name="targetMaxHp">Target's maximum HP (restored to this value).</param>
    /// <param name="activeConditions">Collection of active negative condition names on the target.</param>
    /// <returns>
    /// A <see cref="MiracleWorkerResult"/> containing the full restoration outcome
    /// if the ability was successfully executed; null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Bone-Setter specialization, Miracle Worker unlocked, 5+ AP,
    /// not already used this rest cycle (<see cref="Domain.Entities.Player.HasUsedMiracleWorkerThisRestCycle"/>).</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, cooldown, AP)</item>
    /// <item>Spend 5 AP (no supply cost)</item>
    /// <item>Set rest cycle cooldown flag</item>
    /// <item>Restore target to full HP and clear all negative conditions</item>
    /// <item>Return result with complete miracle breakdown</item>
    /// </list>
    /// <para>No Corruption risk — Miracle Worker follows the Coherent path.</para>
    /// <para>No supply cost — the miracle transcends material requirements.</para>
    /// <para>Cooldown resets on long rest via
    /// <see cref="Domain.Entities.Player.ResetMiracleWorkerCooldown"/>.</para>
    /// </remarks>
    MiracleWorkerResult? ExecuteMiracleWorker(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp,
        int targetMaxHp,
        IEnumerable<string> activeConditions);

    // ===== Utility Methods =====

    /// <summary>
    /// Gets a readiness summary for all Bone-Setter abilities for the specified player.
    /// Used for UI display to show which abilities are available.
    /// </summary>
    /// <param name="player">The Bone-Setter player to check.</param>
    /// <returns>
    /// A dictionary mapping each unlocked <see cref="BoneSetterAbilityId"/> to a boolean
    /// indicating whether the ability can currently be used (sufficient AP, supplies, etc.).
    /// </returns>
    Dictionary<BoneSetterAbilityId, bool> GetAbilityReadiness(Player player);

    /// <summary>
    /// Checks if the player meets Tier 2 unlock requirements (8+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 8 or more PP invested in the Bone-Setter tree.</returns>
    bool CanUnlockTier2(Player player);

    /// <summary>
    /// Checks if the player meets Tier 3 unlock requirements (16+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 16 or more PP invested in the Bone-Setter tree.</returns>
    bool CanUnlockTier3(Player player);

    /// <summary>
    /// Checks if the player meets Capstone unlock requirements (24+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 24 or more PP invested in the Bone-Setter tree.</returns>
    bool CanUnlockCapstone(Player player);

    /// <summary>
    /// Gets total Progression Points invested in the Bone-Setter ability tree.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>Total PP invested.</returns>
    int GetPPInvested(Player player);
}
