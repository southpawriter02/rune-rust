// ------------------------------------------------------------------------------
// <copyright file="IPersuasionService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for handling persuasion attempts.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Domain.Entities;

/// <summary>
/// Service interface for handling persuasion attempts.
/// </summary>
/// <remarks>
/// <para>
/// The persuasion service orchestrates the complete persuasion flow:
/// building context, evaluating argument alignment, performing skill checks,
/// and applying outcomes including disposition and reputation changes.
/// </para>
/// </remarks>
public interface IPersuasionService
{
    /// <summary>
    /// Attempts to persuade an NPC to grant a request.
    /// </summary>
    /// <param name="characterId">The character making the persuasion attempt.</param>
    /// <param name="targetId">The NPC being persuaded.</param>
    /// <param name="requestType">The complexity of the request.</param>
    /// <param name="argumentThemes">Themes present in the player's argument.</param>
    /// <param name="evidenceItemId">Optional evidence item to support the argument.</param>
    /// <returns>The result of the persuasion attempt.</returns>
    Task<PersuasionResult> AttemptPersuasionAsync(
        string characterId,
        string targetId,
        PersuasionRequest requestType,
        IReadOnlyList<string> argumentThemes,
        string? evidenceItemId = null);

    /// <summary>
    /// Builds a persuasion context without performing the check.
    /// </summary>
    /// <param name="characterId">The character making the persuasion attempt.</param>
    /// <param name="targetId">The NPC being persuaded.</param>
    /// <param name="requestType">The complexity of the request.</param>
    /// <param name="argumentThemes">Themes present in the player's argument.</param>
    /// <param name="evidenceItemId">Optional evidence item.</param>
    /// <returns>The persuasion context with all modifiers calculated.</returns>
    /// <remarks>
    /// Useful for previewing the difficulty and modifiers before committing to the attempt.
    /// </remarks>
    Task<PersuasionContext> BuildContextAsync(
        string characterId,
        string targetId,
        PersuasionRequest requestType,
        IReadOnlyList<string> argumentThemes,
        string? evidenceItemId = null);

    /// <summary>
    /// Evaluates argument alignment against NPC values.
    /// </summary>
    /// <param name="targetId">The NPC to evaluate against.</param>
    /// <param name="argumentThemes">Themes present in the player's argument.</param>
    /// <returns>The argument alignment evaluation.</returns>
    Task<ArgumentAlignment> EvaluateArgumentAlignmentAsync(
        string targetId,
        IReadOnlyList<string> argumentThemes);

    /// <summary>
    /// Checks if persuasion is currently blocked with an NPC.
    /// </summary>
    /// <param name="characterId">The character attempting persuasion.</param>
    /// <param name="targetId">The NPC to check.</param>
    /// <returns>True if [Trust Shattered] or similar consequence is active.</returns>
    Task<bool> IsPersuasionBlockedAsync(string characterId, string targetId);

    /// <summary>
    /// Gets the current [Trust Shattered] consequence if active.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="targetId">The NPC to check.</param>
    /// <returns>The active fumble consequence, or null if none.</returns>
    Task<FumbleConsequence?> GetTrustShatteredConsequenceAsync(string characterId, string targetId);

    /// <summary>
    /// Attempts to recover from [Trust Shattered] through a specific action.
    /// </summary>
    /// <param name="characterId">The character attempting recovery.</param>
    /// <param name="targetId">The NPC to recover trust with.</param>
    /// <param name="recoveryAction">The action taken to recover (e.g., quest completion).</param>
    /// <returns>True if trust was successfully recovered.</returns>
    Task<bool> AttemptTrustRecoveryAsync(
        string characterId,
        string targetId,
        string recoveryAction);

    /// <summary>
    /// Gets the number of previous failed persuasion attempts this conversation.
    /// </summary>
    /// <param name="characterId">The character making attempts.</param>
    /// <param name="targetId">The NPC being persuaded.</param>
    /// <returns>The number of previous failed attempts.</returns>
    Task<int> GetPreviousAttemptsAsync(string characterId, string targetId);

    /// <summary>
    /// Records a persuasion attempt for tracking purposes.
    /// </summary>
    /// <param name="characterId">The character making the attempt.</param>
    /// <param name="targetId">The NPC being persuaded.</param>
    /// <param name="result">The result of the attempt.</param>
    Task RecordAttemptAsync(string characterId, string targetId, PersuasionResult result);

    /// <summary>
    /// Resets the conversation state with an NPC, clearing attempt counts.
    /// </summary>
    /// <param name="characterId">The character to reset.</param>
    /// <param name="targetId">The NPC to reset conversation with.</param>
    /// <remarks>
    /// Called when a new conversation begins or after significant time passes.
    /// </remarks>
    Task ResetConversationStateAsync(string characterId, string targetId);
}
