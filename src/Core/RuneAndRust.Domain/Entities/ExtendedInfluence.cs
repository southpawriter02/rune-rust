// ------------------------------------------------------------------------------
// <copyright file="ExtendedInfluence.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Tracks the state of an extended influence attempt to change an NPC's belief.
// This is an aggregate root entity for the Extended Influence System.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks the state of an extended influence attempt to change an NPC's belief.
/// </summary>
/// <remarks>
/// <para>
/// An ExtendedInfluence represents an ongoing or completed effort by a player
/// to change a specific belief held by a specific NPC. Unlike immediate social
/// checks, extended influence persists across multiple game sessions and tracks
/// cumulative progress toward a conviction threshold.
/// </para>
/// <para>
/// Key mechanics:
/// <list type="bullet">
///   <item><description>Influence Pool: Accumulates net successes from rhetoric checks</description></item>
///   <item><description>Conviction Threshold: Pool points needed to change the belief</description></item>
///   <item><description>Resistance Modifier: Increases effective DC after failures</description></item>
///   <item><description>Status Tracking: Active, Successful, Failed, or Stalled</description></item>
/// </list>
/// </para>
/// <para>
/// This entity serves as the aggregate root for extended influence tracking,
/// ensuring all state changes are coordinated and consistent.
/// </para>
/// </remarks>
public sealed class ExtendedInfluence
{
    /// <summary>
    /// Maximum resistance modifier allowed (configurable default).
    /// </summary>
    private const int DefaultMaxResistance = 6;

    /// <summary>
    /// Resistance reduction when resuming from stalled state.
    /// </summary>
    private const int ResumeResistanceReduction = 2;

    /// <summary>
    /// History of all influence attempts made.
    /// </summary>
    private readonly List<InfluenceAttemptResult> _history = new();

    /// <summary>
    /// Tracks accumulated fractional resistance for StrongConviction level.
    /// </summary>
    private decimal _fractionalResistance;

    /// <summary>
    /// Gets the unique identifier for this influence tracking instance.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the ID of the player character attempting influence.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// Gets the ID of the NPC being influenced.
    /// </summary>
    public required string TargetId { get; init; }

    /// <summary>
    /// Gets the display name of the target NPC.
    /// </summary>
    /// <remarks>
    /// Stored for display purposes and logging, avoiding additional lookups.
    /// </remarks>
    public required string TargetName { get; init; }

    /// <summary>
    /// Gets the ID of the specific belief being targeted.
    /// </summary>
    /// <remarks>
    /// An NPC may have multiple beliefs with different conviction levels.
    /// This identifies which specific belief this influence targets.
    /// </remarks>
    public required string BeliefId { get; init; }

    /// <summary>
    /// Gets a description of the belief being targeted.
    /// </summary>
    /// <remarks>
    /// Example: "The Combine is necessary for survival."
    /// </remarks>
    public required string BeliefDescription { get; init; }

    /// <summary>
    /// Gets the conviction level of the target belief.
    /// </summary>
    /// <remarks>
    /// Determines base DC, pool threshold, and resistance mechanics.
    /// </remarks>
    public required ConvictionLevel TargetConviction { get; init; }

    /// <summary>
    /// Gets the current influence pool total.
    /// </summary>
    /// <remarks>
    /// Accumulates from successful rhetoric checks. When this reaches
    /// the conviction threshold, the belief is changed.
    /// </remarks>
    public int InfluencePool { get; private set; }

    /// <summary>
    /// Gets the current resistance modifier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Added to base DC to determine effective DC for rhetoric checks.
    /// Increases on failed attempts based on conviction level:
    /// <list type="bullet">
    ///   <item><description>WeakOpinion/ModerateBelief: never increases</description></item>
    ///   <item><description>StrongConviction: +1 per 2 failures</description></item>
    ///   <item><description>CoreBelief: +1 per failure</description></item>
    ///   <item><description>Fanatical: +2 per failure</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Maximum value is 6 (configurable). Reaching max resistance while
    /// pool is below 50% of threshold causes permanent failure.
    /// </para>
    /// </remarks>
    public int ResistanceModifier { get; private set; }

    /// <summary>
    /// Gets the maximum resistance modifier for this influence.
    /// </summary>
    public int MaxResistance { get; init; } = DefaultMaxResistance;

    /// <summary>
    /// Gets the total number of influence interactions attempted.
    /// </summary>
    public int InteractionCount { get; private set; }

    /// <summary>
    /// Gets the number of successful interactions.
    /// </summary>
    public int SuccessfulInteractions { get; private set; }

    /// <summary>
    /// Gets the current status of this influence tracking.
    /// </summary>
    public InfluenceStatus Status { get; private set; }

    /// <summary>
    /// Gets the timestamp when this influence tracking was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp of the last influence attempt.
    /// </summary>
    public DateTimeOffset? LastInteractionAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the conviction was changed (success).
    /// </summary>
    public DateTimeOffset? ConvictionChangedAt { get; private set; }

    /// <summary>
    /// Gets the reason why influence is stalled, if applicable.
    /// </summary>
    public string? StallReason { get; private set; }

    /// <summary>
    /// Gets the condition required to resume influence, if stalled.
    /// </summary>
    public string? ResumeCondition { get; private set; }

    /// <summary>
    /// Gets read-only access to the history of influence attempts.
    /// </summary>
    public IReadOnlyList<InfluenceAttemptResult> History => _history.AsReadOnly();

    /// <summary>
    /// Gets the pool threshold required to change this belief.
    /// </summary>
    /// <returns>The threshold based on conviction level.</returns>
    public int GetThreshold() => TargetConviction.GetPoolThreshold();

    /// <summary>
    /// Gets the base DC for rhetoric checks against this belief.
    /// </summary>
    /// <returns>The base DC based on conviction level.</returns>
    public int GetBaseDc() => TargetConviction.GetBaseDc();

    /// <summary>
    /// Gets the effective DC for rhetoric checks (base + resistance).
    /// </summary>
    /// <returns>The effective DC including accumulated resistance.</returns>
    public int GetEffectiveDc() => GetBaseDc() + ResistanceModifier;

    /// <summary>
    /// Gets whether the pool has reached the conviction threshold.
    /// </summary>
    /// <returns>True if threshold is reached.</returns>
    public bool IsThresholdReached() => InfluencePool >= GetThreshold();

    /// <summary>
    /// Gets the progress ratio toward the threshold (0.0 to 1.0+).
    /// </summary>
    public decimal ProgressRatio =>
        GetThreshold() > 0 ? (decimal)InfluencePool / GetThreshold() : 0m;

    /// <summary>
    /// Gets the progress percentage toward the threshold.
    /// </summary>
    public int ProgressPercentage => (int)(ProgressRatio * 100);

    /// <summary>
    /// Gets the remaining pool points needed to reach threshold.
    /// </summary>
    public int RemainingPool => Math.Max(0, GetThreshold() - InfluencePool);

    /// <summary>
    /// Creates a new extended influence tracking instance.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="targetName">The target NPC name.</param>
    /// <param name="beliefId">The belief ID.</param>
    /// <param name="beliefDescription">Description of the belief.</param>
    /// <param name="conviction">The conviction level.</param>
    /// <param name="maxResistance">Maximum resistance (optional, default 6).</param>
    /// <returns>A new ExtendedInfluence ready for tracking.</returns>
    public static ExtendedInfluence Create(
        string characterId,
        string targetId,
        string targetName,
        string beliefId,
        string beliefDescription,
        ConvictionLevel conviction,
        int maxResistance = DefaultMaxResistance)
    {
        return new ExtendedInfluence
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            TargetId = targetId,
            TargetName = targetName,
            BeliefId = beliefId,
            BeliefDescription = beliefDescription,
            TargetConviction = conviction,
            MaxResistance = maxResistance,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = InfluenceStatus.Active
        };
    }

    /// <summary>
    /// Adds points to the influence pool from a successful check.
    /// </summary>
    /// <param name="netSuccesses">Net successes to add to the pool.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if influence is not in an active state.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Called when a rhetoric check succeeds. The net successes are added
    /// to the pool. If the pool reaches or exceeds the threshold, the status
    /// changes to Successful.
    /// </para>
    /// </remarks>
    public void AddToPool(int netSuccesses)
    {
        if (!Status.CanContinue())
        {
            throw new InvalidOperationException(
                $"Cannot add to pool when influence status is {Status}.");
        }

        if (netSuccesses <= 0)
        {
            return; // Nothing to add
        }

        InfluencePool += netSuccesses;
        InteractionCount++;
        SuccessfulInteractions++;
        LastInteractionAt = DateTimeOffset.UtcNow;

        // Check for threshold reached
        if (IsThresholdReached())
        {
            Status = InfluenceStatus.Successful;
            ConvictionChangedAt = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// Increments the resistance modifier after a failed check.
    /// </summary>
    /// <returns>The actual resistance increase applied.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if influence is not in an active state.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Called when a rhetoric check fails. The resistance increase depends
    /// on the conviction level. If max resistance is reached while pool
    /// is below 50% of threshold, the influence permanently fails.
    /// </para>
    /// </remarks>
    public int IncrementResistance()
    {
        if (!Status.CanContinue())
        {
            throw new InvalidOperationException(
                $"Cannot increment resistance when influence status is {Status}.");
        }

        InteractionCount++;
        LastInteractionAt = DateTimeOffset.UtcNow;

        // Get resistance rate for this conviction level
        var resistanceRate = TargetConviction.GetResistancePerFailure();

        if (resistanceRate == 0)
        {
            return 0; // No resistance accumulation for this conviction level
        }

        // Add to fractional resistance tracking
        _fractionalResistance += resistanceRate;

        // Calculate integer resistance to add
        var integerIncrease = (int)_fractionalResistance;
        _fractionalResistance -= integerIncrease;

        if (integerIncrease > 0)
        {
            ResistanceModifier = Math.Min(ResistanceModifier + integerIncrease, MaxResistance);
        }

        // Check for permanent failure
        if (ResistanceModifier >= MaxResistance && ProgressRatio < 0.5m)
        {
            Status = InfluenceStatus.Failed;
        }

        return integerIncrease;
    }

    /// <summary>
    /// Records an influence attempt result to the history.
    /// </summary>
    /// <param name="result">The attempt result to record.</param>
    public void RecordAttempt(InfluenceAttemptResult result)
    {
        _history.Add(result);
    }

    /// <summary>
    /// Stalls the influence, requiring external events to continue.
    /// </summary>
    /// <param name="reason">Why the influence is stalled.</param>
    /// <param name="resumeCondition">What's needed to resume.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if influence is already in a terminal state.
    /// </exception>
    public void Stall(string reason, string resumeCondition)
    {
        if (Status.IsTerminal())
        {
            throw new InvalidOperationException(
                $"Cannot stall when influence status is {Status}.");
        }

        Status = InfluenceStatus.Stalled;
        StallReason = reason;
        ResumeCondition = resumeCondition;
        LastInteractionAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Resumes the influence from a stalled state.
    /// </summary>
    /// <param name="resistanceReduction">
    /// How much to reduce resistance (default 2).
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if influence is not in stalled state.
    /// </exception>
    /// <remarks>
    /// When resumed, resistance is reduced to reflect the NPC's renewed
    /// openness after the stall condition is resolved.
    /// </remarks>
    public void Resume(int resistanceReduction = ResumeResistanceReduction)
    {
        if (Status != InfluenceStatus.Stalled)
        {
            throw new InvalidOperationException(
                $"Cannot resume when influence status is {Status}. Must be Stalled.");
        }

        Status = InfluenceStatus.Active;
        ResistanceModifier = Math.Max(0, ResistanceModifier - resistanceReduction);
        StallReason = null;
        ResumeCondition = null;
        LastInteractionAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the influence as permanently failed.
    /// </summary>
    /// <param name="reason">Optional reason for the failure.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if influence is already in a terminal state.
    /// </exception>
    public void Fail(string? reason = null)
    {
        if (Status.IsTerminal())
        {
            throw new InvalidOperationException(
                $"Cannot fail when influence status is {Status}.");
        }

        Status = InfluenceStatus.Failed;
        if (!string.IsNullOrEmpty(reason))
        {
            StallReason = reason; // Repurpose as failure reason
        }

        LastInteractionAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets a summary of the current influence state.
    /// </summary>
    /// <returns>A multi-line summary string.</returns>
    public string GetStateSummary()
    {
        var lines = new List<string>
        {
            $"Influence: {TargetName} - \"{BeliefDescription}\"",
            $"Status: {Status.GetDisplayName()} {Status.GetStatusIndicator()}",
            $"Conviction: {TargetConviction.GetDisplayName()}",
            $"Progress: {InfluencePool}/{GetThreshold()} ({ProgressPercentage}%)",
            $"Effective DC: {GetEffectiveDc()} (base {GetBaseDc()} + {ResistanceModifier} resistance)",
            $"Interactions: {SuccessfulInteractions}/{InteractionCount} successful"
        };

        if (Status == InfluenceStatus.Stalled && !string.IsNullOrEmpty(StallReason))
        {
            lines.Add($"Stalled: {StallReason}");
            if (!string.IsNullOrEmpty(ResumeCondition))
            {
                lines.Add($"Resume: {ResumeCondition}");
            }
        }

        if (Status == InfluenceStatus.Successful && ConvictionChangedAt.HasValue)
        {
            lines.Add($"Belief changed: {ConvictionChangedAt.Value:g}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a short progress display string.
    /// </summary>
    /// <returns>A compact progress string.</returns>
    public string ToProgressDisplay()
    {
        var indicator = Status.GetStatusIndicator();
        var progressBar = GetProgressBar(10);
        return $"{indicator} {TargetName}: {progressBar} {ProgressPercentage}%";
    }

    /// <summary>
    /// Gets a text-based progress bar.
    /// </summary>
    /// <param name="width">Width in characters.</param>
    /// <returns>A progress bar string.</returns>
    private string GetProgressBar(int width)
    {
        var filled = (int)(ProgressRatio * width);
        if (filled > width)
        {
            filled = width;
        }

        var empty = width - filled;
        return $"[{new string('█', filled)}{new string('░', empty)}]";
    }

    /// <summary>
    /// Determines if the influence can mathematically succeed.
    /// </summary>
    /// <param name="maxAttempts">Maximum attempts allowed (optional).</param>
    /// <param name="avgNetSuccesses">Average net successes per attempt (default 2).</param>
    /// <returns>True if success is still mathematically possible.</returns>
    /// <remarks>
    /// Returns false if resistance has grown too high relative to remaining
    /// attempts, making success effectively impossible.
    /// </remarks>
    public bool CanSucceed(int? maxAttempts = null, int avgNetSuccesses = 2)
    {
        if (Status.IsTerminal())
        {
            return Status.IsSuccess();
        }

        if (Status == InfluenceStatus.Stalled)
        {
            return true; // Could succeed if resumed
        }

        // If no max attempts, always possible
        if (!maxAttempts.HasValue)
        {
            return true;
        }

        var attemptsRemaining = maxAttempts.Value - InteractionCount;
        var poolRemaining = RemainingPool;

        return attemptsRemaining * avgNetSuccesses >= poolRemaining;
    }

    /// <summary>
    /// Gets the estimated number of successful attempts remaining.
    /// </summary>
    /// <param name="avgNetSuccesses">Average net successes per success (default 2).</param>
    /// <returns>Estimated attempts needed, or 0 if already complete.</returns>
    public int GetEstimatedAttemptsRemaining(int avgNetSuccesses = 2)
    {
        if (Status.IsTerminal())
        {
            return 0;
        }

        if (avgNetSuccesses <= 0)
        {
            avgNetSuccesses = 1;
        }

        return (int)Math.Ceiling((double)RemainingPool / avgNetSuccesses);
    }
}
