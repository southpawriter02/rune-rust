// ------------------------------------------------------------------------------
// <copyright file="IIntimidationService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for handling intimidation attempts.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling intimidation attempts against NPCs.
/// </summary>
/// <remarks>
/// <para>
/// The intimidation service manages the complete lifecycle of an intimidation
/// attempt, from building context to performing the skill check and
/// applying all consequences including the mandatory Cost of Fear.
/// </para>
/// <para>
/// Key features:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>Dual attribute choice: MIGHT (Physical) or WILL (Mental)</description>
///   </item>
///   <item>
///     <description>Mandatory reputation cost regardless of outcome</description>
///   </item>
///   <item>
///     <description>[Challenge Accepted] fumble consequence with combat</description>
///   </item>
///   <item>
///     <description>[Furious] NPC buff on fumble (+2d10 damage)</description>
///   </item>
/// </list>
/// </remarks>
public interface IIntimidationService
{
    /// <summary>
    /// Attempts to intimidate an NPC.
    /// </summary>
    /// <param name="characterId">The ID of the character attempting intimidation.</param>
    /// <param name="targetNpcId">The ID of the NPC being intimidated.</param>
    /// <param name="approach">The intimidation approach (Physical/MIGHT or Mental/WILL).</param>
    /// <returns>The result of the intimidation attempt.</returns>
    /// <remarks>
    /// <para>
    /// This is the primary method for performing intimidation attempts.
    /// It builds the context, performs the skill check, applies reputation
    /// costs, and handles fumble consequences.
    /// </para>
    /// <para>
    /// WARNING: Intimidation ALWAYS costs faction reputation regardless of
    /// outcome. Critical Success: -3, Success: -5, Failure/Fumble: -10.
    /// </para>
    /// </remarks>
    Task<IntimidationResult> AttemptIntimidationAsync(
        string characterId,
        string targetNpcId,
        IntimidationApproach approach);

    /// <summary>
    /// Performs an intimidation check with a pre-built context.
    /// </summary>
    /// <param name="context">The intimidation context containing all modifiers.</param>
    /// <returns>The complete result of the intimidation attempt.</returns>
    /// <remarks>
    /// Use this overload when you have already built the context and want
    /// to perform the actual check. This is useful for previewing the
    /// context before committing to the attempt.
    /// </remarks>
    Task<IntimidationResult> IntimidateAsync(IntimidationContext context);

    /// <summary>
    /// Builds the complete intimidation context for a potential attempt.
    /// </summary>
    /// <remarks>
    /// Use this to preview the difficulty and modifiers before committing
    /// to an intimidation attempt.
    /// </remarks>
    /// <param name="characterId">The ID of the character.</param>
    /// <param name="targetNpcId">The ID of the target NPC.</param>
    /// <param name="approach">The intended intimidation approach.</param>
    /// <returns>The complete intimidation context with all modifiers calculated.</returns>
    Task<IntimidationContext> BuildContextAsync(
        string characterId,
        string targetNpcId,
        IntimidationApproach approach);

    /// <summary>
    /// Classifies an NPC into an IntimidationTarget tier based on their stats.
    /// </summary>
    /// <param name="npcId">The ID of the NPC to classify.</param>
    /// <returns>The appropriate IntimidationTarget tier.</returns>
    /// <remarks>
    /// Classification is based on NPC attributes, combat experience, and
    /// position. Higher-tier targets are harder to intimidate.
    /// </remarks>
    Task<IntimidationTarget> ClassifyTargetAsync(string npcId);

    /// <summary>
    /// Calculates the relative strength between player and NPC.
    /// </summary>
    /// <param name="characterId">The ID of the player character.</param>
    /// <param name="npcId">The ID of the NPC to compare against.</param>
    /// <returns>The RelativeStrength assessment.</returns>
    /// <remarks>
    /// Level comparison: Player 2+ above NPC = PlayerStronger (+1d10),
    /// Player 2+ below NPC = PlayerWeaker (NPC +1d10),
    /// Within ±1 level = Equal (no modifier).
    /// </remarks>
    Task<RelativeStrength> CalculateRelativeStrengthAsync(string characterId, string npcId);

    /// <summary>
    /// Checks if the player has [Honored] or [Feared] reputation with the target's faction.
    /// </summary>
    /// <param name="characterId">The ID of the character.</param>
    /// <param name="factionId">The faction to check reputation with.</param>
    /// <returns>True if the player has reputation that affects intimidation (+1d10).</returns>
    Task<bool> HasReputationBonusAsync(string characterId, string factionId);

    /// <summary>
    /// Checks if the player is currently wielding an [Artifact] item.
    /// </summary>
    /// <param name="characterId">The ID of the character.</param>
    /// <returns>True if wielding an artifact (+1d10 bonus).</returns>
    Task<bool> IsWieldingArtifactAsync(string characterId);

    /// <summary>
    /// Checks if an intimidating ally is present in the current encounter.
    /// </summary>
    /// <param name="characterId">The ID of the character.</param>
    /// <returns>True if an intimidating ally is present (+1d10 bonus).</returns>
    /// <remarks>
    /// Intimidating allies include Kriger companions, summoned creatures,
    /// or other visibly dangerous party members.
    /// </remarks>
    Task<bool> HasIntimidatingAllyAsync(string characterId);

    /// <summary>
    /// Checks if the target NPC has backup (allies) nearby.
    /// </summary>
    /// <param name="npcId">The ID of the target NPC.</param>
    /// <returns>True if the NPC has backup (NPC +1d10 resistance).</returns>
    Task<bool> TargetHasBackupAsync(string npcId);

    /// <summary>
    /// Gets the faction ID for the target NPC.
    /// </summary>
    /// <param name="npcId">The ID of the target NPC.</param>
    /// <returns>The NPC's faction ID, or null if no faction.</returns>
    Task<string?> GetTargetFactionAsync(string npcId);

    /// <summary>
    /// Calculates the Cost of Fear for a given outcome.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <param name="factionId">The affected faction ID.</param>
    /// <returns>The CostOfFear with calculated reputation cost.</returns>
    CostOfFear CalculateCostOfFear(SkillOutcome outcome, string factionId);

    /// <summary>
    /// Applies the [Furious] status effect to an NPC after fumble.
    /// </summary>
    /// <param name="npcId">The ID of the NPC to receive [Furious].</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// [Furious] grants +2d10 to all damage rolls for the entire combat.
    /// Cannot be removed or dispelled.
    /// </remarks>
    Task ApplyFuriousEffectAsync(string npcId);

    /// <summary>
    /// Initiates combat with the specified NPC after fumble.
    /// </summary>
    /// <param name="characterId">The ID of the player character.</param>
    /// <param name="npcId">The ID of the hostile NPC.</param>
    /// <param name="npcInitiativeBonus">Initiative bonus for the NPC.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// On [Challenge Accepted], the NPC gains +2 initiative and will
    /// not accept surrender.
    /// </remarks>
    Task InitiateChallengeAcceptedCombatAsync(
        string characterId,
        string npcId,
        int npcInitiativeBonus);
}
