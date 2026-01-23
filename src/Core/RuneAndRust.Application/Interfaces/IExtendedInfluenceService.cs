// ------------------------------------------------------------------------------
// <copyright file="IExtendedInfluenceService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for the Extended Influence System, enabling players to
// gradually change NPC beliefs over multiple interactions.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for the Extended Influence System.
/// </summary>
/// <remarks>
/// <para>
/// The Extended Influence Service orchestrates attempts to change NPC beliefs
/// over multiple interactions. Unlike immediate social checks, extended influence:
/// </para>
/// <list type="bullet">
///   <item><description>Persists across game sessions</description></item>
///   <item><description>Accumulates progress toward a conviction threshold</description></item>
///   <item><description>Tracks resistance that increases on failed attempts</description></item>
///   <item><description>Supports stalling and resumption based on external events</description></item>
/// </list>
/// <para>
/// Conviction levels determine difficulty:
/// </para>
/// <list type="bullet">
///   <item><description>WeakOpinion: DC 10, 5 pool required, no resistance buildup</description></item>
///   <item><description>ModerateBelief: DC 12, 10 pool required, no resistance buildup</description></item>
///   <item><description>StrongConviction: DC 14, 15 pool required, +1 resistance per 2 failures</description></item>
///   <item><description>CoreBelief: DC 16, 20 pool required, +1 resistance per failure</description></item>
///   <item><description>Fanatical: DC 18, 25 pool required, +2 resistance per failure</description></item>
/// </list>
/// <para>
/// The effective DC is: baseDC + currentResistance. Maximum resistance is 6.
/// </para>
/// </remarks>
public interface IExtendedInfluenceService
{
    /// <summary>
    /// Attempts to influence an NPC's belief.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="beliefId">The belief ID to influence.</param>
    /// <param name="characterRhetoric">The character's Rhetoric skill level.</param>
    /// <param name="characterAttribute">The character's relevant attribute (typically WILL).</param>
    /// <param name="bonusDice">Any bonus dice from items, abilities, or circumstances.</param>
    /// <param name="dcModifier">Additional DC modifiers from circumstances.</param>
    /// <returns>An <see cref="InfluenceAttemptResult"/> describing the outcome.</returns>
    /// <remarks>
    /// <para>
    /// Influence attempt flow:
    /// </para>
    /// <list type="number">
    ///   <item><description>Get or create influence tracking for this character/target/belief</description></item>
    ///   <item><description>Calculate effective DC (baseDC + resistance)</description></item>
    ///   <item><description>Roll rhetoric check: attribute + skill + bonuses vs DC</description></item>
    ///   <item><description>On success: add net successes to influence pool</description></item>
    ///   <item><description>On failure: increment resistance based on conviction level</description></item>
    ///   <item><description>Check for conviction threshold or failure conditions</description></item>
    /// </list>
    /// <para>
    /// Possible outcomes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Success with pool gain (status remains Active)</description></item>
    ///   <item><description>Success with threshold reached (status becomes Successful)</description></item>
    ///   <item><description>Failure with resistance increase (status remains Active)</description></item>
    ///   <item><description>Failure with max resistance reached (status becomes Failed)</description></item>
    ///   <item><description>External condition triggered (status becomes Stalled)</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when characterId, targetId, or beliefId is null or empty.
    /// </exception>
    InfluenceAttemptResult AttemptInfluence(
        string characterId,
        string targetId,
        string beliefId,
        int characterRhetoric,
        int characterAttribute,
        int bonusDice = 0,
        int dcModifier = 0);

    /// <summary>
    /// Attempts to influence an NPC's belief asynchronously.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="beliefId">The belief ID to influence.</param>
    /// <param name="characterRhetoric">The character's Rhetoric skill level.</param>
    /// <param name="characterAttribute">The character's relevant attribute.</param>
    /// <param name="bonusDice">Any bonus dice.</param>
    /// <param name="dcModifier">Additional DC modifiers.</param>
    /// <returns>A task that resolves to an <see cref="InfluenceAttemptResult"/>.</returns>
    /// <remarks>
    /// Async overload that allows for fetching NPC data, belief information,
    /// and updating game state after the attempt.
    /// </remarks>
    Task<InfluenceAttemptResult> AttemptInfluenceAsync(
        string characterId,
        string targetId,
        string beliefId,
        int characterRhetoric,
        int characterAttribute,
        int bonusDice = 0,
        int dcModifier = 0);

    /// <summary>
    /// Gets the influence progress for a specific tracking by ID.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the influence tracking.</param>
    /// <returns>The <see cref="ExtendedInfluence"/> if found, otherwise null.</returns>
    ExtendedInfluence? GetInfluenceProgress(Guid influenceId);

    /// <summary>
    /// Gets the influence progress for a character's attempt on a specific belief.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="beliefId">The belief ID.</param>
    /// <returns>The <see cref="ExtendedInfluence"/> if found, otherwise null.</returns>
    /// <remarks>
    /// This is the natural key lookup: one character can only have one
    /// active influence attempt per belief per NPC.
    /// </remarks>
    ExtendedInfluence? GetInfluenceProgress(
        string characterId,
        string targetId,
        string beliefId);

    /// <summary>
    /// Gets all influence progress between a character and target.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <returns>All influence trackings between the character and target.</returns>
    /// <remarks>
    /// An NPC may have multiple beliefs that a character is working to change.
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetInfluenceProgressByTarget(
        string characterId,
        string targetId);

    /// <summary>
    /// Gets all active (non-terminal) influences for a character.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <returns>All active and stalled influences for the character.</returns>
    /// <remarks>
    /// Returns influences that can still progress (Active status) or
    /// that are waiting for external events (Stalled status).
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetActiveInfluences(string characterId);

    /// <summary>
    /// Gets all influences targeting a specific NPC.
    /// </summary>
    /// <param name="targetId">The target NPC ID.</param>
    /// <returns>All influences targeting the NPC (from all characters).</returns>
    /// <remarks>
    /// Useful for determining how many players are trying to influence
    /// a particular NPC or for game master oversight.
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetInfluencesOnTarget(string targetId);

    /// <summary>
    /// Gets all successful influences for a character.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <returns>All influences where the character changed an NPC's belief.</returns>
    /// <remarks>
    /// Useful for tracking achievements or reviewing past successes.
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetSuccessfulInfluences(string characterId);

    /// <summary>
    /// Gets all stalled influences for a character.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <returns>All stalled influences awaiting external events.</returns>
    /// <remarks>
    /// Stalled influences require the player to take action outside of
    /// normal dialogue to resume progress (quests, evidence, time).
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetStalledInfluences(string characterId);

    /// <summary>
    /// Resumes a stalled influence attempt.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the stalled influence.</param>
    /// <param name="resistanceReduction">
    /// How much to reduce resistance (default 2). Represents the NPC's
    /// renewed openness after the stall condition is resolved.
    /// </param>
    /// <returns>True if the influence was resumed; false if not found or not stalled.</returns>
    /// <remarks>
    /// <para>
    /// Resumption occurs when the stall condition is met:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Time passed: 24+ game hours since stall</description></item>
    ///   <item><description>Quest completed: Player proved themselves</description></item>
    ///   <item><description>Evidence found: Counter-evidence obtained</description></item>
    ///   <item><description>Mood changed: NPC emotional state improved</description></item>
    /// </list>
    /// <para>
    /// Resistance is reduced by the specified amount (default 2) when resumed,
    /// representing the NPC's renewed openness to discussion.
    /// </para>
    /// </remarks>
    bool ResumeInfluence(Guid influenceId, int resistanceReduction = 2);

    /// <summary>
    /// Resumes a stalled influence attempt asynchronously.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the stalled influence.</param>
    /// <param name="resistanceReduction">How much to reduce resistance.</param>
    /// <returns>A task that resolves to true if resumed, false otherwise.</returns>
    Task<bool> ResumeInfluenceAsync(Guid influenceId, int resistanceReduction = 2);

    /// <summary>
    /// Stalls an active influence, requiring external events to continue.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the influence.</param>
    /// <param name="stallReason">Why the influence is stalling.</param>
    /// <param name="resumeCondition">What must happen to resume.</param>
    /// <returns>True if the influence was stalled; false if not found or not active.</returns>
    /// <remarks>
    /// <para>
    /// Common stall conditions:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>"I need time to think." (Wait 24+ hours)</description></item>
    ///   <item><description>"Prove yourself first." (Complete quest)</description></item>
    ///   <item><description>"Show me the records." (Find evidence)</description></item>
    ///   <item><description>"I can't discuss this now." (Improve NPC mood)</description></item>
    /// </list>
    /// </remarks>
    bool StallInfluence(Guid influenceId, string stallReason, string resumeCondition);

    /// <summary>
    /// Marks an influence as permanently failed.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the influence.</param>
    /// <param name="reason">Optional reason for the failure.</param>
    /// <returns>True if the influence was failed; false if not found or already terminal.</returns>
    /// <remarks>
    /// <para>
    /// Failure conditions:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Maximum resistance reached with insufficient pool</description></item>
    ///   <item><description>Related quest failed</description></item>
    ///   <item><description>NPC becomes permanently unavailable</description></item>
    ///   <item><description>Critical fumble with unrecoverable consequence</description></item>
    /// </list>
    /// </remarks>
    bool FailInfluence(Guid influenceId, string? reason = null);

    /// <summary>
    /// Checks if a conviction threshold is reached for an influence.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the influence.</param>
    /// <returns>True if the threshold is reached (or exceeded).</returns>
    bool CheckConvictionThreshold(Guid influenceId);

    /// <summary>
    /// Initializes a new influence tracking for a character and belief.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="targetName">The target NPC's display name.</param>
    /// <param name="beliefId">The belief ID to influence.</param>
    /// <param name="beliefDescription">Description of the belief.</param>
    /// <param name="conviction">The conviction level of the belief.</param>
    /// <returns>The newly created <see cref="ExtendedInfluence"/>.</returns>
    /// <remarks>
    /// <para>
    /// Creates a new tracking record with initial values:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>InfluencePool: 0</description></item>
    ///   <item><description>ResistanceModifier: 0</description></item>
    ///   <item><description>InteractionCount: 0</description></item>
    ///   <item><description>Status: Active</description></item>
    /// </list>
    /// <para>
    /// If a tracking already exists for this character/target/belief combination,
    /// the existing tracking is returned instead of creating a duplicate.
    /// </para>
    /// </remarks>
    ExtendedInfluence InitializeInfluence(
        string characterId,
        string targetId,
        string targetName,
        string beliefId,
        string beliefDescription,
        ConvictionLevel conviction);

    /// <summary>
    /// Gets or creates an influence tracking for a character and belief.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="targetName">The target NPC's display name.</param>
    /// <param name="beliefId">The belief ID to influence.</param>
    /// <param name="beliefDescription">Description of the belief.</param>
    /// <param name="conviction">The conviction level of the belief.</param>
    /// <returns>The existing or newly created <see cref="ExtendedInfluence"/>.</returns>
    /// <remarks>
    /// Convenience method that returns existing tracking if found,
    /// or creates a new one if not.
    /// </remarks>
    ExtendedInfluence GetOrCreateInfluence(
        string characterId,
        string targetId,
        string targetName,
        string beliefId,
        string beliefDescription,
        ConvictionLevel conviction);

    /// <summary>
    /// Gets tactical advice for the current influence state.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the influence.</param>
    /// <returns>A list of tactical suggestions, or empty if not found.</returns>
    /// <remarks>
    /// <para>
    /// Provides context-aware advice such as:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Progress assessment and remaining effort estimate</description></item>
    ///   <item><description>Warnings about rising resistance</description></item>
    ///   <item><description>Suggestions for when to pause vs. continue</description></item>
    ///   <item><description>Recommended approaches based on conviction level</description></item>
    /// </list>
    /// </remarks>
    IReadOnlyList<string> GetTacticalAdvice(Guid influenceId);

    /// <summary>
    /// Gets a formatted summary of an influence's current state.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the influence.</param>
    /// <returns>A formatted summary string, or null if not found.</returns>
    string? GetInfluenceSummary(Guid influenceId);

    /// <summary>
    /// Calculates the effective DC for an influence attempt.
    /// </summary>
    /// <param name="conviction">The conviction level.</param>
    /// <param name="currentResistance">The current resistance modifier.</param>
    /// <returns>The effective DC (base DC + resistance).</returns>
    int CalculateEffectiveDc(ConvictionLevel conviction, int currentResistance);

    /// <summary>
    /// Calculates the resistance increase for a failed attempt.
    /// </summary>
    /// <param name="conviction">The conviction level.</param>
    /// <param name="accumulatedFractional">
    /// Current fractional resistance accumulation (for StrongConviction).
    /// </param>
    /// <returns>
    /// A tuple of (integerIncrease, newFractionalValue) for updating state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Resistance calculation varies by conviction:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>WeakOpinion/ModerateBelief: (0, 0) - no accumulation</description></item>
    ///   <item><description>StrongConviction: Fractional (0.5 per failure)</description></item>
    ///   <item><description>CoreBelief: (1, 0) - 1 per failure</description></item>
    ///   <item><description>Fanatical: (2, 0) - 2 per failure</description></item>
    /// </list>
    /// </remarks>
    (int integerIncrease, decimal newFractional) CalculateResistanceIncrease(
        ConvictionLevel conviction,
        decimal accumulatedFractional);

    /// <summary>
    /// Removes an influence tracking record.
    /// </summary>
    /// <param name="influenceId">The unique identifier of the influence.</param>
    /// <returns>True if removed; false if not found.</returns>
    /// <remarks>
    /// Generally should only be used for administrative cleanup.
    /// Normal completion should leave records for history tracking.
    /// </remarks>
    bool RemoveInfluence(Guid influenceId);

    /// <summary>
    /// Gets statistics about a character's influence history.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <returns>Statistics about the character's influence attempts.</returns>
    /// <remarks>
    /// <para>
    /// Statistics include:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Total influence attempts started</description></item>
    ///   <item><description>Successful belief changes</description></item>
    ///   <item><description>Failed attempts</description></item>
    ///   <item><description>Currently active attempts</description></item>
    ///   <item><description>Average interactions to success</description></item>
    /// </list>
    /// </remarks>
    InfluenceStatistics GetStatistics(string characterId);
}

/// <summary>
/// Statistics about a character's influence history.
/// </summary>
/// <param name="TotalStarted">Total influence attempts started.</param>
/// <param name="TotalSuccessful">Successfully changed beliefs.</param>
/// <param name="TotalFailed">Permanently failed attempts.</param>
/// <param name="CurrentlyActive">Currently active (non-terminal) attempts.</param>
/// <param name="CurrentlyStalled">Currently stalled attempts.</param>
/// <param name="AverageInteractionsToSuccess">
/// Average number of interactions needed to change a belief.
/// </param>
/// <param name="TotalInteractions">Total rhetoric checks made.</param>
public readonly record struct InfluenceStatistics(
    int TotalStarted,
    int TotalSuccessful,
    int TotalFailed,
    int CurrentlyActive,
    int CurrentlyStalled,
    decimal AverageInteractionsToSuccess,
    int TotalInteractions);
