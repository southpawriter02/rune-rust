using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Bone-Setter specialization ability execution.
/// Handles Tier 1 (Foundation) and Tier 2 (Discipline) ability logic including
/// healing, diagnosis, crafting, and tactical triage evaluation.
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
