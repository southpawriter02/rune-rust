// ------------------------------------------------------------------------------
// <copyright file="IDeceptionService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for handling deception attempts.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling deception attempts.
/// </summary>
/// <remarks>
/// <para>
/// The deception service manages the complete lifecycle of a deception
/// attempt, from building context to resolving the opposed roll and
/// applying all consequences including the Liar's Burden stress cost.
/// </para>
/// <para>
/// Deception uses an opposed roll system: Player (WILL + Rhetoric) vs.
/// NPC (WITS), with various modifiers applied to both sides.
/// </para>
/// </remarks>
public interface IDeceptionService
{
    /// <summary>
    /// Attempts to deceive an NPC.
    /// </summary>
    /// <param name="characterId">The ID of the character attempting deception.</param>
    /// <param name="targetNpcId">The ID of the NPC being deceived.</param>
    /// <param name="lieComplexity">The complexity of the lie being told.</param>
    /// <param name="coverStoryQuality">The quality of any prepared cover story.</param>
    /// <param name="hasForgedDocuments">Whether the character has forged documents.</param>
    /// <param name="forgedDocumentQuality">Quality of forged documents (0=none, 1=basic, 2=high).</param>
    /// <param name="lieContainsTruth">Whether the lie incorporates some genuine truth.</param>
    /// <returns>The result of the deception attempt.</returns>
    Task<DeceptionResult> AttemptDeceptionAsync(
        string characterId,
        string targetNpcId,
        LieComplexity lieComplexity,
        CoverStoryQuality coverStoryQuality = CoverStoryQuality.None,
        bool hasForgedDocuments = false,
        int forgedDocumentQuality = 0,
        bool lieContainsTruth = false);

    /// <summary>
    /// Builds the complete deception context for a potential attempt.
    /// </summary>
    /// <remarks>
    /// Use this to preview the difficulty and modifiers before committing
    /// to a deception attempt.
    /// </remarks>
    /// <param name="characterId">The ID of the character.</param>
    /// <param name="targetNpcId">The ID of the target NPC.</param>
    /// <param name="lieComplexity">The complexity of the intended lie.</param>
    /// <param name="coverStoryQuality">The quality of any prepared cover story.</param>
    /// <param name="hasForgedDocuments">Whether the character has forged documents.</param>
    /// <param name="forgedDocumentQuality">Quality of forged documents.</param>
    /// <param name="lieContainsTruth">Whether the lie incorporates truth.</param>
    /// <returns>The complete deception context with all modifiers calculated.</returns>
    Task<DeceptionContext> BuildContextAsync(
        string characterId,
        string targetNpcId,
        LieComplexity lieComplexity,
        CoverStoryQuality coverStoryQuality = CoverStoryQuality.None,
        bool hasForgedDocuments = false,
        int forgedDocumentQuality = 0,
        bool lieContainsTruth = false);

    /// <summary>
    /// Checks if the player has the [Untrustworthy] flag with a specific NPC.
    /// </summary>
    /// <param name="characterId">The ID of the character.</param>
    /// <param name="npcId">The ID of the NPC.</param>
    /// <returns>True if the character has the [Untrustworthy] flag with this NPC.</returns>
    Task<bool> HasUntrustworthyFlagAsync(string characterId, string npcId);

    /// <summary>
    /// Attempts to remove the [Untrustworthy] flag with a specific NPC.
    /// </summary>
    /// <remarks>
    /// Removal requires specific conditions such as completing a beneficial quest,
    /// saving the NPC, significant time passage, or third-party intercession.
    /// </remarks>
    /// <param name="characterId">The ID of the character.</param>
    /// <param name="npcId">The ID of the NPC.</param>
    /// <param name="removalReason">The reason for removal attempt.</param>
    /// <returns>True if the flag was successfully removed.</returns>
    Task<bool> TryRemoveUntrustworthyFlagAsync(
        string characterId,
        string npcId,
        UntrustworthyRemovalReason removalReason);

    /// <summary>
    /// Calculates the Liar's Burden stress cost for an outcome.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <param name="isFumble">Whether the roll was a fumble.</param>
    /// <returns>The Liar's Burden with calculated stress costs.</returns>
    LiarsBurden CalculateLiarsBurden(SkillOutcome outcome, bool isFumble);

    /// <summary>
    /// Marks an NPC as having been fooled by the player.
    /// </summary>
    /// <param name="characterId">The character who deceived the NPC.</param>
    /// <param name="npcId">The NPC who was fooled.</param>
    Task MarkNpcAsFooledAsync(string characterId, string npcId);

    /// <summary>
    /// Checks if an NPC was previously deceived by the player.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="npcId">The NPC to check.</param>
    /// <returns>True if the NPC was previously fooled by this character.</returns>
    Task<bool> WasNpcPreviouslyFooledAsync(string characterId, string npcId);
}

/// <summary>
/// Reasons for removing the [Untrustworthy] flag.
/// </summary>
public enum UntrustworthyRemovalReason
{
    /// <summary>
    /// Completed a quest that benefits the NPC.
    /// </summary>
    BeneficialQuest,

    /// <summary>
    /// Saved the NPC's life or property.
    /// </summary>
    RescuedNpc,

    /// <summary>
    /// Significant time has passed (2+ in-game weeks).
    /// </summary>
    TimePassage,

    /// <summary>
    /// A trusted third party vouched for the character.
    /// </summary>
    ThirdPartyIntercession
}
