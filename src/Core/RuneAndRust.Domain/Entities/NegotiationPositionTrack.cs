// ------------------------------------------------------------------------------
// <copyright file="NegotiationPositionTrack.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Entity that tracks the state of a multi-round negotiation including PC/NPC
// positions on a 0-8 scale, round history, and current negotiation status.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks the complete state of a multi-round negotiation including both party
/// positions, round history, and current status.
/// </summary>
/// <remarks>
/// <para>
/// The negotiation position track represents the core state machine for the
/// negotiation system. It manages:
/// </para>
/// <list type="bullet">
///   <item><description>PC and NPC positions on the 0-8 bargaining scale</description></item>
///   <item><description>Round counting and time-out detection</description></item>
///   <item><description>Status transitions (Opening → Bargaining → Crisis/Finalization → Deal/Collapse)</description></item>
///   <item><description>Consecutive failure tracking for crisis detection</description></item>
///   <item><description>Active concession bonus management</description></item>
///   <item><description>Complete round history for result generation</description></item>
/// </list>
/// <para>
/// Position Scale (0-8):
/// </para>
/// <list type="bullet">
///   <item><description>0: Maximum Demand (most favorable to that party)</description></item>
///   <item><description>1-2: Strong position</description></item>
///   <item><description>3-4: Favorable position</description></item>
///   <item><description>5: Compromise (neutral)</description></item>
///   <item><description>6-7: Unfavorable position</description></item>
///   <item><description>8: Walk Away (causes negotiation collapse)</description></item>
/// </list>
/// <para>
/// A deal is reached when the gap between PC and NPC positions reaches 0
/// (positions overlap). The gap also determines negotiation phase:
/// </para>
/// <list type="bullet">
///   <item><description>Gap 0: DealReached</description></item>
///   <item><description>Gap ≤ 1: Finalization</description></item>
///   <item><description>Gap 2-4: Bargaining</description></item>
///   <item><description>Gap ≥ 5 or 2+ consecutive failures: CrisisManagement</description></item>
/// </list>
/// </remarks>
public sealed class NegotiationPositionTrack
{
    /// <summary>
    /// Internal list of negotiation round history.
    /// </summary>
    private readonly List<NegotiationRound> _history = new();

    /// <summary>
    /// Gets the unique identifier for this negotiation session.
    /// </summary>
    /// <remarks>
    /// Used to track and retrieve negotiation state across rounds.
    /// </remarks>
    public required string NegotiationId { get; init; }

    /// <summary>
    /// Gets the player character's current position on the 0-8 scale.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lower values are more favorable to the PC:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>0: Maximum Demand (PC wants everything)</description></item>
    ///   <item><description>1-2: Strong (PC in strong bargaining position)</description></item>
    ///   <item><description>3-4: Favorable (PC in favorable position)</description></item>
    ///   <item><description>5: Compromise (neutral ground)</description></item>
    ///   <item><description>6-7: Unfavorable (PC making concessions)</description></item>
    ///   <item><description>8: Walk Away (negotiation collapses)</description></item>
    /// </list>
    /// </remarks>
    public int PcPosition { get; private set; }

    /// <summary>
    /// Gets the NPC's current position on the 0-8 scale.
    /// </summary>
    /// <remarks>
    /// <para>
    /// NPCs typically start at position 6 (Unfavorable) and move toward
    /// Compromise (5) when the player succeeds in checks.
    /// </para>
    /// <para>
    /// If NPC reaches position 8, the negotiation collapses as the NPC
    /// refuses to continue talking.
    /// </para>
    /// </remarks>
    public int NpcPosition { get; private set; }

    /// <summary>
    /// Gets the calculated gap between PC and NPC positions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Gap = |NpcPosition - PcPosition|
    /// </para>
    /// <para>
    /// The gap determines:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Whether a deal can be reached (gap = 0)</description></item>
    ///   <item><description>The current negotiation phase</description></item>
    ///   <item><description>How close the parties are to agreement</description></item>
    /// </list>
    /// </remarks>
    public int Gap => Math.Abs(NpcPosition - PcPosition);

    /// <summary>
    /// Gets the current round number (1-based).
    /// </summary>
    /// <remarks>
    /// Round number starts at 0 before any rounds are played, then increments
    /// to 1 after the first round is recorded.
    /// </remarks>
    public int RoundNumber { get; private set; }

    /// <summary>
    /// Gets the number of rounds remaining before negotiation times out.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If rounds remaining reaches 0 without a deal, the negotiation collapses.
    /// </para>
    /// <para>
    /// Initial round count depends on <see cref="RequestComplexity"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>FairTrade: 3 rounds</description></item>
    ///   <item><description>SlightAdvantage: 4 rounds</description></item>
    ///   <item><description>NoticeableAdvantage: 5 rounds</description></item>
    ///   <item><description>MajorAdvantage: 6 rounds</description></item>
    ///   <item><description>OneSidedDeal: 7 rounds</description></item>
    /// </list>
    /// </remarks>
    public int RoundsRemaining { get; private set; }

    /// <summary>
    /// Gets the current negotiation phase/status.
    /// </summary>
    /// <remarks>
    /// Status progression:
    /// <list type="bullet">
    ///   <item><description>Opening: Initial phase before any rounds</description></item>
    ///   <item><description>Bargaining: Main negotiation phase</description></item>
    ///   <item><description>CrisisManagement: At risk of collapse (gap ≥ 5 or 2+ failures)</description></item>
    ///   <item><description>Finalization: Close to agreement (gap ≤ 1)</description></item>
    ///   <item><description>DealReached: Terminal success (gap = 0)</description></item>
    ///   <item><description>Collapsed: Terminal failure</description></item>
    /// </list>
    /// </remarks>
    public NegotiationStatus Status { get; private set; }

    /// <summary>
    /// Gets the complexity of what the player is asking for.
    /// </summary>
    /// <remarks>
    /// Complexity determines base DC, initial gap, and default rounds.
    /// </remarks>
    public required RequestComplexity RequestComplexity { get; init; }

    /// <summary>
    /// Gets the identifier of the NPC being negotiated with.
    /// </summary>
    public required string NpcId { get; init; }

    /// <summary>
    /// Gets the count of consecutive check failures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used for crisis detection. When consecutive failures reach 2 or more,
    /// the negotiation enters CrisisManagement phase.
    /// </para>
    /// <para>
    /// Reset to 0 on any successful check.
    /// </para>
    /// </remarks>
    public int ConsecutiveFailures { get; private set; }

    /// <summary>
    /// Gets read-only access to the complete round history.
    /// </summary>
    /// <remarks>
    /// Contains all rounds played in this negotiation, in order.
    /// Used for generating the final negotiation result.
    /// </remarks>
    public IReadOnlyList<NegotiationRound> History => _history.AsReadOnly();

    /// <summary>
    /// Gets the active concession bonus from the previous round, if any.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a player uses the Concede tactic, they gain a concession bonus
    /// that applies to their next check. The bonus provides:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>+2d10 bonus dice</description></item>
    ///   <item><description>DC reduction (-2 to -6 depending on concession type)</description></item>
    /// </list>
    /// <para>
    /// The concession is consumed after being applied to a check.
    /// </para>
    /// </remarks>
    public Concession? ActiveConcession { get; private set; }

    /// <summary>
    /// Gets the timestamp when this negotiation was initiated.
    /// </summary>
    public DateTime InitiatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when this negotiation was completed, if applicable.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Initializes the position track with starting positions and round count.
    /// </summary>
    /// <param name="pcStartPosition">The PC's starting position (0-8).</param>
    /// <param name="npcStartPosition">The NPC's starting position (0-8).</param>
    /// <param name="totalRounds">The maximum number of rounds allowed.</param>
    /// <remarks>
    /// <para>
    /// This method must be called before any rounds can be played.
    /// Starting positions are determined by <see cref="RequestComplexity"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>FairTrade: PC at 4, NPC at 6 (gap 2)</description></item>
    ///   <item><description>SlightAdvantage: PC at 3, NPC at 6 (gap 3)</description></item>
    ///   <item><description>NoticeableAdvantage: PC at 2, NPC at 6 (gap 4)</description></item>
    ///   <item><description>MajorAdvantage: PC at 1, NPC at 6 (gap 5)</description></item>
    ///   <item><description>OneSidedDeal: PC at 0, NPC at 6 (gap 6)</description></item>
    /// </list>
    /// </remarks>
    public void Initialize(int pcStartPosition, int npcStartPosition, int totalRounds)
    {
        // Validate and clamp positions to valid range
        PcPosition = Math.Clamp(pcStartPosition, 0, 8);
        NpcPosition = Math.Clamp(npcStartPosition, 0, 8);

        // Set round tracking
        RoundsRemaining = Math.Max(1, totalRounds);
        RoundNumber = 0;

        // Initial status
        Status = NegotiationStatus.Opening;
        ConsecutiveFailures = 0;

        // Clear any previous state
        ActiveConcession = null;
        _history.Clear();

        // Track timing
        InitiatedAt = DateTime.UtcNow;
        CompletedAt = null;
    }

    /// <summary>
    /// Moves the PC position toward the opponent (used on failure).
    /// </summary>
    /// <param name="steps">The number of steps to move (positive value).</param>
    /// <remarks>
    /// <para>
    /// On failure, the PC's position moves toward the NPC's position,
    /// representing a weakening bargaining position.
    /// </para>
    /// <para>
    /// Movement direction is determined automatically based on relative positions.
    /// If PC position reaches 8, the negotiation collapses.
    /// </para>
    /// </remarks>
    public void MovePcPosition(int steps)
    {
        if (steps <= 0)
        {
            return;
        }

        // Determine direction: PC moves toward NPC position
        int direction = NpcPosition > PcPosition ? 1 : -1;

        // Apply movement with bounds checking
        PcPosition = Math.Clamp(PcPosition + (steps * direction), 0, 8);

        // Check if we've hit the walk-away threshold
        CheckForCollapse();
    }

    /// <summary>
    /// Moves the NPC position toward compromise (used on success).
    /// </summary>
    /// <param name="steps">The number of steps to move (positive value).</param>
    /// <remarks>
    /// <para>
    /// On success, the NPC's position moves toward the PC's position,
    /// representing the NPC becoming more amenable to the player's terms.
    /// </para>
    /// <para>
    /// Movement direction is determined automatically. NPCs generally move
    /// toward the PC's position (lowering the gap).
    /// </para>
    /// </remarks>
    public void MoveNpcPosition(int steps)
    {
        if (steps <= 0)
        {
            return;
        }

        // Determine direction: NPC moves toward PC position
        int direction;
        if (NpcPosition > PcPosition)
        {
            direction = -1; // NPC moves down (toward PC)
        }
        else if (NpcPosition < PcPosition)
        {
            direction = 1; // NPC moves up (toward PC)
        }
        else
        {
            direction = 0; // Already at same position
        }

        // Apply movement with bounds checking
        NpcPosition = Math.Clamp(NpcPosition + (steps * direction), 0, 8);
    }

    /// <summary>
    /// Records a completed round and updates the negotiation state.
    /// </summary>
    /// <param name="round">The round data to record.</param>
    /// <remarks>
    /// <para>
    /// This method performs several state updates:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Adds the round to history</description></item>
    ///   <item><description>Increments round number</description></item>
    ///   <item><description>Decrements rounds remaining</description></item>
    ///   <item><description>Updates consecutive failure count</description></item>
    ///   <item><description>Consumes active concession if used</description></item>
    ///   <item><description>Updates negotiation status</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when round is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when negotiation is already in a terminal state.</exception>
    public void RecordRound(NegotiationRound round)
    {
        ArgumentNullException.ThrowIfNull(round);

        if (Status.IsTerminal())
        {
            throw new InvalidOperationException(
                $"Cannot record rounds in a negotiation that has ended. Status: {Status}");
        }

        // Add to history
        _history.Add(round);

        // Update round counts
        RoundNumber++;
        RoundsRemaining--;

        // Track consecutive failures for crisis detection
        if (round.CheckResult == SkillOutcome.CriticalFailure ||
            round.CheckResult == SkillOutcome.Failure)
        {
            ConsecutiveFailures++;
        }
        else
        {
            // Reset on any non-failure (including concessions)
            ConsecutiveFailures = 0;
        }

        // Consume active concession if a check was made (not on Concede tactic)
        if (ActiveConcession != null && round.TacticUsed != NegotiationTactic.Concede)
        {
            ActiveConcession = ActiveConcession.MarkAsConsumed();
            ActiveConcession = null;
        }

        // Update positions from the round
        PcPosition = round.PcPositionAfter;
        NpcPosition = round.NpcPositionAfter;

        // Update status based on new state
        UpdateStatus();
    }

    /// <summary>
    /// Sets a concession bonus to be applied on the next check.
    /// </summary>
    /// <param name="concession">The concession providing the bonus.</param>
    /// <remarks>
    /// <para>
    /// The concession bonus is applied to the next check and then consumed.
    /// Setting a new concession while one is active will replace it.
    /// </para>
    /// <para>
    /// Concession benefits:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>+2d10 bonus dice on next check</description></item>
    ///   <item><description>DC reduction based on concession type</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when concession is null.</exception>
    public void SetConcessionBonus(Concession concession)
    {
        ArgumentNullException.ThrowIfNull(concession);
        ActiveConcession = concession;
    }

    /// <summary>
    /// Clears the active concession bonus without consuming it.
    /// </summary>
    /// <remarks>
    /// Use this when a concession bonus should be discarded without being applied.
    /// </remarks>
    public void ClearConcessionBonus()
    {
        ActiveConcession = null;
    }

    /// <summary>
    /// Forces the negotiation to collapse immediately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this method when external events cause the negotiation to end,
    /// such as:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>A fumble during CrisisManagement phase</description></item>
    ///   <item><description>Combat initiation from failed intimidation</description></item>
    ///   <item><description>External interruption of the negotiation</description></item>
    /// </list>
    /// </remarks>
    public void ForceCollapse()
    {
        Status = NegotiationStatus.Collapsed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the negotiation as successfully completed.
    /// </summary>
    /// <remarks>
    /// Called when a deal is reached (gap = 0). Sets the completion timestamp.
    /// </remarks>
    public void MarkComplete()
    {
        if (Status == NegotiationStatus.DealReached || Status == NegotiationStatus.Collapsed)
        {
            CompletedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets a human-readable assessment of the current gap.
    /// </summary>
    /// <returns>A description of how far apart the positions are.</returns>
    public string GetGapAssessment()
    {
        return Gap switch
        {
            0 => "Positions overlap - deal possible",
            1 => "Very close - finalization imminent",
            <= 2 => "Close - quick resolution possible",
            <= 4 => "Moderate - standard negotiation",
            <= 6 => "Far apart - will require effort",
            _ => "Very far - significant concessions needed"
        };
    }

    /// <summary>
    /// Gets the position name for a given position value.
    /// </summary>
    /// <param name="position">The position value (0-8).</param>
    /// <returns>The human-readable position name.</returns>
    public static string GetPositionName(int position)
    {
        return position switch
        {
            0 => "Maximum Demand",
            1 => "Strong+",
            2 => "Strong",
            3 => "Favorable+",
            4 => "Favorable",
            5 => "Compromise",
            6 => "Unfavorable",
            7 => "Unfavorable+",
            8 => "Walk Away",
            _ => "Invalid"
        };
    }

    /// <summary>
    /// Gets a formatted display of the current position state.
    /// </summary>
    /// <returns>A string showing PC position, NPC position, and gap.</returns>
    public string GetPositionDisplay()
    {
        return $"PC: {GetPositionName(PcPosition)} ({PcPosition}) | " +
               $"NPC: {GetPositionName(NpcPosition)} ({NpcPosition}) | " +
               $"Gap: {Gap}";
    }

    /// <summary>
    /// Gets a summary of the negotiation state for display.
    /// </summary>
    /// <returns>A multi-line summary of the current state.</returns>
    public string GetStatusSummary()
    {
        var lines = new List<string>
        {
            $"Negotiation: {NegotiationId}",
            $"Status: {Status.GetDisplayName()}",
            $"Round: {RoundNumber} ({RoundsRemaining} remaining)",
            GetPositionDisplay(),
            $"Assessment: {GetGapAssessment()}"
        };

        if (ConsecutiveFailures > 0)
        {
            lines.Add($"Warning: {ConsecutiveFailures} consecutive failure(s)");
        }

        if (ActiveConcession != null)
        {
            lines.Add($"Active Bonus: +{ActiveConcession.BonusDice}d10, -{ActiveConcession.DcReduction} DC");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Determines if it's currently risky to use the Pressure (Intimidation) tactic.
    /// </summary>
    /// <returns>True if using Pressure could easily cause collapse; otherwise, false.</returns>
    /// <remarks>
    /// Pressure is risky when in CrisisManagement (fumble causes collapse) or
    /// when the gap is already large.
    /// </remarks>
    public bool IsPressureRisky()
    {
        return Status == NegotiationStatus.CrisisManagement || Gap >= 5;
    }

    /// <summary>
    /// Determines if it's currently risky to use the Deceive tactic.
    /// </summary>
    /// <returns>True if using Deceive could easily cause collapse; otherwise, false.</returns>
    /// <remarks>
    /// Deceive is risky during CrisisManagement (fumble causes collapse) and
    /// also incurs Psychic Stress regardless of outcome.
    /// </remarks>
    public bool IsDeceiveRisky()
    {
        return Status == NegotiationStatus.CrisisManagement;
    }

    /// <summary>
    /// Gets the total accumulated stress cost from all rounds.
    /// </summary>
    /// <returns>The sum of all stress costs.</returns>
    public int GetTotalStressCost()
    {
        return _history.Sum(r => r.StressCost);
    }

    /// <summary>
    /// Gets the total accumulated reputation cost from all rounds.
    /// </summary>
    /// <returns>The sum of all reputation costs.</returns>
    public int GetTotalReputationCost()
    {
        return _history.Sum(r => r.ReputationCost);
    }

    /// <summary>
    /// Gets the total disposition change from all rounds.
    /// </summary>
    /// <returns>The sum of all disposition changes.</returns>
    public int GetTotalDispositionChange()
    {
        return _history.Sum(r => r.DispositionChange);
    }

    /// <summary>
    /// Updates the negotiation status based on current state.
    /// </summary>
    private void UpdateStatus()
    {
        // Don't change terminal states
        if (Status.IsTerminal())
        {
            return;
        }

        // Check for deal reached (highest priority)
        if (Gap == 0)
        {
            Status = NegotiationStatus.DealReached;
            CompletedAt = DateTime.UtcNow;
            return;
        }

        // Check for finalization (close to deal)
        if (Gap <= 1)
        {
            Status = NegotiationStatus.Finalization;
            return;
        }

        // Check for crisis (gap too wide or too many failures)
        if (Gap >= 5 || ConsecutiveFailures >= 2)
        {
            Status = NegotiationStatus.CrisisManagement;
            return;
        }

        // Check for timeout (no rounds left)
        if (RoundsRemaining <= 0)
        {
            Status = NegotiationStatus.Collapsed;
            CompletedAt = DateTime.UtcNow;
            return;
        }

        // Transition from Opening to Bargaining after first round
        if (RoundNumber > 0 && Status == NegotiationStatus.Opening)
        {
            Status = NegotiationStatus.Bargaining;
        }
    }

    /// <summary>
    /// Checks if either position has reached the Walk Away threshold.
    /// </summary>
    private void CheckForCollapse()
    {
        if (PcPosition >= 8 || NpcPosition >= 8)
        {
            Status = NegotiationStatus.Collapsed;
            CompletedAt = DateTime.UtcNow;
        }
    }
}
