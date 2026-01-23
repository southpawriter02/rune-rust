// ------------------------------------------------------------------------------
// <copyright file="InfluenceAttemptResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Records the outcome of a single extended influence attempt including
// the rhetoric check result, pool changes, and resistance updates.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Records the outcome of a single extended influence attempt including
/// the rhetoric check result, pool changes, and resistance updates.
/// </summary>
/// <remarks>
/// <para>
/// Each influence attempt represents a player's effort to shift an NPC's belief
/// through dialogue and rhetoric. The result tracks:
/// <list type="bullet">
///   <item><description>Whether the rhetoric check succeeded or failed</description></item>
///   <item><description>Net successes achieved (determines pool gain on success)</description></item>
///   <item><description>Changes to the influence pool and resistance modifier</description></item>
///   <item><description>Whether the conviction threshold was reached</description></item>
/// </list>
/// </para>
/// <para>
/// Pool mechanics:
/// <list type="bullet">
///   <item><description>On success: Pool increases by net successes</description></item>
///   <item><description>On failure: Resistance may increase based on conviction level</description></item>
///   <item><description>When pool reaches threshold: Belief changes, status becomes Successful</description></item>
/// </list>
/// </para>
/// </remarks>
public readonly record struct InfluenceAttemptResult
{
    /// <summary>
    /// Gets a value indicating whether the rhetoric check succeeded.
    /// </summary>
    /// <remarks>
    /// Success means the check met or exceeded the effective DC.
    /// The effective DC is base DC + current resistance modifier.
    /// </remarks>
    public required bool SkillCheckSucceeded { get; init; }

    /// <summary>
    /// Gets the net successes achieved on the rhetoric check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Net successes = successes rolled - successes required (DC).
    /// On success, this value is added to the influence pool.
    /// On failure, this will be 0 or negative.
    /// </para>
    /// <para>
    /// Example: Roll 5 successes vs DC 3 = 2 net successes added to pool.
    /// </para>
    /// </remarks>
    public required int NetSuccesses { get; init; }

    /// <summary>
    /// Gets the change to the influence pool from this attempt.
    /// </summary>
    /// <remarks>
    /// On successful check: equals NetSuccesses (positive value).
    /// On failed check: always 0 (failures don't reduce the pool).
    /// </remarks>
    public required int PoolChange { get; init; }

    /// <summary>
    /// Gets the total influence pool after this attempt.
    /// </summary>
    /// <remarks>
    /// The pool accumulates across multiple interactions until
    /// it reaches the conviction threshold.
    /// </remarks>
    public required int NewPoolTotal { get; init; }

    /// <summary>
    /// Gets the pool threshold required to change the belief.
    /// </summary>
    /// <remarks>
    /// Determined by the target's conviction level:
    /// WeakOpinion: 5, ModerateBelief: 10, StrongConviction: 15,
    /// CoreBelief: 20, Fanatical: 25.
    /// </remarks>
    public required int ThresholdRequired { get; init; }

    /// <summary>
    /// Gets the change to the resistance modifier from this attempt.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On failed check: increases based on conviction level:
    /// <list type="bullet">
    ///   <item><description>WeakOpinion/ModerateBelief: no resistance increase</description></item>
    ///   <item><description>StrongConviction: +1 per 2 failures</description></item>
    ///   <item><description>CoreBelief: +1 per failure</description></item>
    ///   <item><description>Fanatical: +2 per failure</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// On successful check: always 0 (successes don't reduce resistance).
    /// </para>
    /// </remarks>
    public required int ResistanceChange { get; init; }

    /// <summary>
    /// Gets the total resistance modifier after this attempt.
    /// </summary>
    /// <remarks>
    /// The resistance modifier is added to the base DC for future attempts.
    /// Maximum resistance is 6 (configurable in global settings).
    /// </remarks>
    public required int NewResistanceTotal { get; init; }

    /// <summary>
    /// Gets a value indicating whether the conviction threshold was reached.
    /// </summary>
    /// <remarks>
    /// When true, the NPC's belief has been successfully changed and the
    /// influence status transitions to Successful.
    /// </remarks>
    public required bool IsConvictionChanged { get; init; }

    /// <summary>
    /// Gets the status of the influence tracking after this attempt.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Possible outcomes:
    /// <list type="bullet">
    ///   <item><description>Active: Attempt completed, can continue</description></item>
    ///   <item><description>Successful: Threshold reached, belief changed</description></item>
    ///   <item><description>Failed: Max resistance reached or failure condition</description></item>
    ///   <item><description>Stalled: External event required to continue</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public required InfluenceStatus Status { get; init; }

    /// <summary>
    /// Gets the conviction level of the target belief.
    /// </summary>
    /// <remarks>
    /// Determines base DC, threshold, and resistance mechanics.
    /// </remarks>
    public required ConvictionLevel TargetConviction { get; init; }

    /// <summary>
    /// Gets the interaction number (1-based) for this attempt.
    /// </summary>
    /// <remarks>
    /// Tracks how many influence attempts have been made toward this belief.
    /// </remarks>
    public required int InteractionNumber { get; init; }

    /// <summary>
    /// Gets the base DC before resistance modifier was applied.
    /// </summary>
    public required int BaseDc { get; init; }

    /// <summary>
    /// Gets the effective DC (base DC + resistance modifier) used for this check.
    /// </summary>
    public required int EffectiveDc { get; init; }

    /// <summary>
    /// Gets the number of dice rolled for the rhetoric check.
    /// </summary>
    public required int DiceRolled { get; init; }

    /// <summary>
    /// Gets the total successes achieved on the roll.
    /// </summary>
    public required int SuccessesRolled { get; init; }

    /// <summary>
    /// Gets the stall reason if status is Stalled, otherwise null.
    /// </summary>
    public string? StallReason { get; init; }

    /// <summary>
    /// Gets the resume condition if status is Stalled, otherwise null.
    /// </summary>
    public string? ResumeCondition { get; init; }

    /// <summary>
    /// Gets a narrative description of the attempt outcome.
    /// </summary>
    public required string NarrativeDescription { get; init; }

    /// <summary>
    /// Gets the timestamp when this attempt occurred.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the progress ratio toward the conviction threshold (0.0 to 1.0+).
    /// </summary>
    /// <remarks>
    /// Can exceed 1.0 if pool surpasses threshold (overflow successes).
    /// </remarks>
    public decimal ProgressRatio =>
        ThresholdRequired > 0 ? (decimal)NewPoolTotal / ThresholdRequired : 0m;

    /// <summary>
    /// Gets the progress percentage toward the conviction threshold (0-100+).
    /// </summary>
    public int ProgressPercentage => (int)(ProgressRatio * 100);

    /// <summary>
    /// Gets the remaining pool points needed to reach the threshold.
    /// </summary>
    /// <remarks>
    /// Returns 0 if threshold is already reached or exceeded.
    /// </remarks>
    public int RemainingPool =>
        Math.Max(0, ThresholdRequired - NewPoolTotal);

    /// <summary>
    /// Gets a value indicating whether this attempt was a breakthrough.
    /// </summary>
    /// <remarks>
    /// A breakthrough occurs when a single attempt gains 3+ net successes,
    /// representing a particularly compelling argument.
    /// </remarks>
    public bool IsBreakthrough => SkillCheckSucceeded && NetSuccesses >= 3;

    /// <summary>
    /// Gets a value indicating whether this was the final successful attempt.
    /// </summary>
    public bool IsCompletedSuccess => IsConvictionChanged && Status.IsSuccess();

    /// <summary>
    /// Gets a value indicating whether this attempt caused the influence to fail.
    /// </summary>
    public bool CausedFailure => Status.IsFailure();

    /// <summary>
    /// Gets a value indicating whether this attempt caused a stall.
    /// </summary>
    public bool CausedStall => Status == InfluenceStatus.Stalled;

    /// <summary>
    /// Gets a value indicating whether resistance increased on this attempt.
    /// </summary>
    public bool ResistanceIncreased => ResistanceChange > 0;

    /// <summary>
    /// Gets the effective DC for the next attempt (if influence continues).
    /// </summary>
    /// <remarks>
    /// Returns the new effective DC accounting for updated resistance.
    /// Returns 0 if influence has reached a terminal state.
    /// </remarks>
    public int NextEffectiveDc =>
        Status.CanContinue() ? BaseDc + NewResistanceTotal : 0;

    /// <summary>
    /// Gets a formatted summary of this attempt for display.
    /// </summary>
    /// <returns>A compact summary string describing the attempt outcome.</returns>
    public string GetSummary()
    {
        var outcomeText = SkillCheckSucceeded
            ? $"Success (+{PoolChange} pool)"
            : "Failure";

        if (IsBreakthrough)
        {
            outcomeText = $"BREAKTHROUGH (+{PoolChange} pool)";
        }

        if (IsConvictionChanged)
        {
            outcomeText += " - BELIEF CHANGED!";
        }

        return $"Attempt {InteractionNumber}: {outcomeText}. " +
               $"Pool: {NewPoolTotal}/{ThresholdRequired} ({ProgressPercentage}%)";
    }

    /// <summary>
    /// Gets a detailed summary including all mechanical details.
    /// </summary>
    /// <returns>A detailed multi-line summary of the attempt.</returns>
    public string GetDetailedSummary()
    {
        var lines = new List<string>
        {
            $"Attempt {InteractionNumber}: {TargetConviction.GetDisplayName()} belief",
            $"  Roll: {SuccessesRolled} vs DC {EffectiveDc} (base {BaseDc} + {NewResistanceTotal - ResistanceChange} resistance)",
            $"  Result: {(SkillCheckSucceeded ? "SUCCESS" : "FAILURE")}" +
                (IsBreakthrough ? " (Breakthrough!)" : string.Empty),
            $"  Net Successes: {NetSuccesses}",
            $"  Pool: {NewPoolTotal - PoolChange} → {NewPoolTotal} / {ThresholdRequired} ({ProgressPercentage}%)"
        };

        if (ResistanceIncreased)
        {
            lines.Add($"  Resistance: +{ResistanceChange} → {NewResistanceTotal}");
        }

        if (IsConvictionChanged)
        {
            lines.Add("  *** CONVICTION THRESHOLD REACHED - BELIEF CHANGED ***");
        }

        if (CausedStall && !string.IsNullOrEmpty(StallReason))
        {
            lines.Add($"  ⏸ STALLED: {StallReason}");
            if (!string.IsNullOrEmpty(ResumeCondition))
            {
                lines.Add($"     Resume: {ResumeCondition}");
            }
        }

        if (CausedFailure)
        {
            lines.Add("  ✗ INFLUENCE FAILED - Max resistance reached");
        }

        lines.Add($"  Status: {Status.GetDisplayName()}");

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a short display string for compact UI elements.
    /// </summary>
    /// <returns>A very compact representation of the attempt.</returns>
    public string ToShortDisplay()
    {
        var indicator = Status.GetStatusIndicator();
        var progressBar = GetProgressBar(5);
        return $"#{InteractionNumber} {(SkillCheckSucceeded ? "✓" : "✗")} {progressBar} {indicator}";
    }

    /// <summary>
    /// Gets a text-based progress bar.
    /// </summary>
    /// <param name="width">The width in characters.</param>
    /// <returns>A progress bar string like "[███░░]".</returns>
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
    /// Creates a result for a successful influence attempt.
    /// </summary>
    /// <param name="netSuccesses">Net successes achieved.</param>
    /// <param name="previousPool">Pool total before this attempt.</param>
    /// <param name="threshold">Pool threshold required.</param>
    /// <param name="resistance">Current resistance modifier.</param>
    /// <param name="conviction">Target conviction level.</param>
    /// <param name="interactionNumber">The interaction number.</param>
    /// <param name="diceRolled">Number of dice rolled.</param>
    /// <param name="successesRolled">Successes achieved on roll.</param>
    /// <param name="narrative">Narrative description.</param>
    /// <returns>A new <see cref="InfluenceAttemptResult"/> for the success.</returns>
    public static InfluenceAttemptResult Success(
        int netSuccesses,
        int previousPool,
        int threshold,
        int resistance,
        ConvictionLevel conviction,
        int interactionNumber,
        int diceRolled,
        int successesRolled,
        string narrative)
    {
        var newPool = previousPool + netSuccesses;
        var thresholdReached = newPool >= threshold;
        var baseDc = conviction.GetBaseDc();

        return new InfluenceAttemptResult
        {
            SkillCheckSucceeded = true,
            NetSuccesses = netSuccesses,
            PoolChange = netSuccesses,
            NewPoolTotal = newPool,
            ThresholdRequired = threshold,
            ResistanceChange = 0, // Success never increases resistance
            NewResistanceTotal = resistance,
            IsConvictionChanged = thresholdReached,
            Status = thresholdReached ? InfluenceStatus.Successful : InfluenceStatus.Active,
            TargetConviction = conviction,
            InteractionNumber = interactionNumber,
            BaseDc = baseDc,
            EffectiveDc = baseDc + resistance,
            DiceRolled = diceRolled,
            SuccessesRolled = successesRolled,
            NarrativeDescription = narrative,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a result for a failed influence attempt.
    /// </summary>
    /// <param name="netSuccesses">Net successes (negative or zero).</param>
    /// <param name="currentPool">Current pool total (unchanged by failure).</param>
    /// <param name="threshold">Pool threshold required.</param>
    /// <param name="previousResistance">Resistance before this attempt.</param>
    /// <param name="resistanceIncrease">How much resistance increased.</param>
    /// <param name="conviction">Target conviction level.</param>
    /// <param name="interactionNumber">The interaction number.</param>
    /// <param name="diceRolled">Number of dice rolled.</param>
    /// <param name="successesRolled">Successes achieved on roll.</param>
    /// <param name="narrative">Narrative description.</param>
    /// <param name="maxResistance">Maximum allowed resistance (default 6).</param>
    /// <returns>A new <see cref="InfluenceAttemptResult"/> for the failure.</returns>
    public static InfluenceAttemptResult Failure(
        int netSuccesses,
        int currentPool,
        int threshold,
        int previousResistance,
        int resistanceIncrease,
        ConvictionLevel conviction,
        int interactionNumber,
        int diceRolled,
        int successesRolled,
        string narrative,
        int maxResistance = 6)
    {
        var newResistance = previousResistance + resistanceIncrease;
        var baseDc = conviction.GetBaseDc();

        // Check if max resistance reached
        var isFailed = newResistance >= maxResistance;
        var status = isFailed ? InfluenceStatus.Failed : InfluenceStatus.Active;

        return new InfluenceAttemptResult
        {
            SkillCheckSucceeded = false,
            NetSuccesses = netSuccesses,
            PoolChange = 0, // Failure doesn't change pool
            NewPoolTotal = currentPool,
            ThresholdRequired = threshold,
            ResistanceChange = resistanceIncrease,
            NewResistanceTotal = Math.Min(newResistance, maxResistance),
            IsConvictionChanged = false,
            Status = status,
            TargetConviction = conviction,
            InteractionNumber = interactionNumber,
            BaseDc = baseDc,
            EffectiveDc = baseDc + previousResistance,
            DiceRolled = diceRolled,
            SuccessesRolled = successesRolled,
            NarrativeDescription = narrative,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a result for an attempt that causes a stall.
    /// </summary>
    /// <param name="currentPool">Current pool total.</param>
    /// <param name="threshold">Pool threshold required.</param>
    /// <param name="resistance">Current resistance modifier.</param>
    /// <param name="conviction">Target conviction level.</param>
    /// <param name="interactionNumber">The interaction number.</param>
    /// <param name="stallReason">Why the influence stalled.</param>
    /// <param name="resumeCondition">What's needed to resume.</param>
    /// <param name="narrative">Narrative description.</param>
    /// <returns>A new <see cref="InfluenceAttemptResult"/> for the stall.</returns>
    public static InfluenceAttemptResult Stalled(
        int currentPool,
        int threshold,
        int resistance,
        ConvictionLevel conviction,
        int interactionNumber,
        string stallReason,
        string resumeCondition,
        string narrative)
    {
        var baseDc = conviction.GetBaseDc();

        return new InfluenceAttemptResult
        {
            SkillCheckSucceeded = false,
            NetSuccesses = 0,
            PoolChange = 0,
            NewPoolTotal = currentPool,
            ThresholdRequired = threshold,
            ResistanceChange = 0,
            NewResistanceTotal = resistance,
            IsConvictionChanged = false,
            Status = InfluenceStatus.Stalled,
            TargetConviction = conviction,
            InteractionNumber = interactionNumber,
            BaseDc = baseDc,
            EffectiveDc = baseDc + resistance,
            DiceRolled = 0,
            SuccessesRolled = 0,
            StallReason = stallReason,
            ResumeCondition = resumeCondition,
            NarrativeDescription = narrative,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}
