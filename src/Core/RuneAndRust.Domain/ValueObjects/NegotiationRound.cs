// ------------------------------------------------------------------------------
// <copyright file="NegotiationRound.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Records the outcome of a single negotiation round including tactic used,
// check result, position movement, and any costs incurred.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Records the complete outcome of a single negotiation round.
/// </summary>
/// <remarks>
/// <para>
/// Each negotiation round involves the player selecting a tactic (Persuade, Deceive,
/// Pressure, or Concede), making a skill check (if applicable), and seeing the
/// resulting position movement on the negotiation track.
/// </para>
/// <para>
/// Position Movement Rules:
/// </para>
/// <list type="bullet">
///   <item><description>Success: Opponent moves 1 step toward Compromise</description></item>
///   <item><description>Critical Success: Opponent moves 2 steps toward Compromise</description></item>
///   <item><description>Failure: Self moves 1 step toward opponent</description></item>
///   <item><description>Critical Failure: Self moves 2 steps toward opponent (may trigger crisis/collapse)</description></item>
///   <item><description>Concede: Self moves 1 step toward opponent (gains +2d10 and DC reduction for next check)</description></item>
/// </list>
/// <para>
/// Rounds are tracked in the <see cref="Entities.NegotiationPositionTrack"/> history
/// to provide a complete record of the negotiation.
/// </para>
/// </remarks>
public sealed record NegotiationRound
{
    /// <summary>
    /// Gets the round number (1-based).
    /// </summary>
    /// <remarks>
    /// Round numbers start at 1 and increment each round. The maximum number of
    /// rounds depends on the <see cref="RequestComplexity"/> of the negotiation.
    /// </remarks>
    public required int RoundNumber { get; init; }

    /// <summary>
    /// Gets the tactic used this round.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Available tactics:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Persuade: Uses Rhetoric (Persuasion) subsystem</description></item>
    ///   <item><description>Deceive: Uses Rhetoric (Deception) subsystem</description></item>
    ///   <item><description>Pressure: Uses Rhetoric (Intimidation) subsystem</description></item>
    ///   <item><description>Concede: No check, voluntary position movement</description></item>
    /// </list>
    /// </remarks>
    public required NegotiationTactic TacticUsed { get; init; }

    /// <summary>
    /// Gets the outcome of the skill check for this round.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is null for the Concede tactic, which does not require a skill check.
    /// For other tactics, it reflects the outcome of the underlying skill system check.
    /// </para>
    /// <para>
    /// Possible outcomes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>CriticalFailure: Fumble with severe consequences</description></item>
    ///   <item><description>Failure: Check did not meet DC</description></item>
    ///   <item><description>MarginalSuccess: Barely met DC</description></item>
    ///   <item><description>FullSuccess: Clearly exceeded DC</description></item>
    ///   <item><description>ExceptionalSuccess: Outstanding performance</description></item>
    ///   <item><description>CriticalSuccess: Masterful achievement</description></item>
    /// </list>
    /// </remarks>
    public SkillOutcome? CheckResult { get; init; }

    /// <summary>
    /// Gets the description of position movement this round.
    /// </summary>
    /// <remarks>
    /// Describes which position moved, in which direction, and by how many steps.
    /// Example: "NPC moved 1 step toward Compromise" or "PC moved 2 steps toward Walk Away".
    /// </remarks>
    public required string PositionMovement { get; init; }

    /// <summary>
    /// Gets the PC position after this round (0-8 scale).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Position scale for PC:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>0: Maximum Demand</description></item>
    ///   <item><description>1-2: Strong</description></item>
    ///   <item><description>3-4: Favorable</description></item>
    ///   <item><description>5: Compromise</description></item>
    ///   <item><description>6: Unfavorable</description></item>
    ///   <item><description>7: Unfavorable+</description></item>
    ///   <item><description>8: Walk Away (causes collapse)</description></item>
    /// </list>
    /// </remarks>
    public required int PcPositionAfter { get; init; }

    /// <summary>
    /// Gets the NPC position after this round (0-8 scale).
    /// </summary>
    /// <remarks>
    /// The NPC position uses the same 0-8 scale as the PC position.
    /// A deal is reached when PC and NPC positions meet or overlap.
    /// </remarks>
    public required int NpcPositionAfter { get; init; }

    /// <summary>
    /// Gets the gap between positions after this round.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Gap = |NpcPosition - PcPosition|
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Gap 0: Deal can be reached</description></item>
    ///   <item><description>Gap 1: Finalization phase - close to agreement</description></item>
    ///   <item><description>Gap 2-4: Bargaining phase - normal negotiation</description></item>
    ///   <item><description>Gap 5+: Crisis Management phase - at risk of collapse</description></item>
    /// </list>
    /// </remarks>
    public required int GapAfter { get; init; }

    /// <summary>
    /// Gets the concession made this round, if any.
    /// </summary>
    /// <remarks>
    /// Only populated when the Concede tactic is used. The concession provides
    /// +2d10 bonus dice and a DC reduction on the next check.
    /// </remarks>
    public Concession? ConcessionMade { get; init; }

    /// <summary>
    /// Gets the stress cost incurred this round.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Stress is primarily incurred when using the Deceive tactic, which adds
    /// Psychic Stress regardless of outcome (due to the Liar's Burden).
    /// </para>
    /// <para>
    /// Stress values:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Deceive Success: +3 Psychic Stress</description></item>
    ///   <item><description>Deceive Failure: +6 Psychic Stress</description></item>
    ///   <item><description>Other tactics: 0 (typically)</description></item>
    /// </list>
    /// </remarks>
    public int StressCost { get; init; }

    /// <summary>
    /// Gets the reputation cost incurred this round.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reputation is primarily affected when using the Pressure (Intimidation) tactic,
    /// which always costs reputation due to the Cost of Fear.
    /// </para>
    /// <para>
    /// Reputation values (all negative):
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Pressure Critical Success: -3 reputation</description></item>
    ///   <item><description>Pressure Success: -5 reputation</description></item>
    ///   <item><description>Pressure Failure: -10 reputation</description></item>
    /// </list>
    /// </remarks>
    public int ReputationCost { get; init; }

    /// <summary>
    /// Gets the disposition change for this round.
    /// </summary>
    /// <remarks>
    /// Disposition may change based on the tactic used and the outcome.
    /// Pressure (Intimidation) typically causes negative disposition changes.
    /// </remarks>
    public int DispositionChange { get; init; }

    /// <summary>
    /// Gets a narrative description of what happened this round.
    /// </summary>
    /// <remarks>
    /// This provides flavor text suitable for display to the player,
    /// describing the events of the round in narrative terms.
    /// </remarks>
    public required string NarrativeDescription { get; init; }

    /// <summary>
    /// Gets the DC that was used for the check this round.
    /// </summary>
    /// <remarks>
    /// This is the effective DC after applying any concession reductions.
    /// Null for Concede tactic which has no check.
    /// </remarks>
    public int? EffectiveDcUsed { get; init; }

    /// <summary>
    /// Gets the dice pool size used for this round's check.
    /// </summary>
    /// <remarks>
    /// This includes base dice plus any bonuses from concessions.
    /// Null for Concede tactic which has no check.
    /// </remarks>
    public int? DicePoolUsed { get; init; }

    /// <summary>
    /// Gets a value indicating whether this round resulted in a fumble.
    /// </summary>
    /// <remarks>
    /// A fumble (CriticalFailure) during negotiation can have severe consequences,
    /// potentially collapsing the negotiation entirely if in Crisis Management phase.
    /// </remarks>
    public bool IsFumble => CheckResult == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets a value indicating whether this round was successful.
    /// </summary>
    /// <remarks>
    /// Success includes MarginalSuccess, FullSuccess, ExceptionalSuccess, and CriticalSuccess.
    /// Also true for Concede tactic which always "succeeds" (intentional position movement).
    /// </remarks>
    public bool IsSuccess => TacticUsed == NegotiationTactic.Concede ||
        (CheckResult.HasValue && CheckResult.Value >= SkillOutcome.MarginalSuccess);

    /// <summary>
    /// Gets a value indicating whether this round was a failure.
    /// </summary>
    /// <remarks>
    /// Failure includes CriticalFailure and Failure outcomes.
    /// Never true for Concede tactic.
    /// </remarks>
    public bool IsFailure => CheckResult.HasValue &&
        CheckResult.Value <= SkillOutcome.Failure;

    /// <summary>
    /// Gets a value indicating whether a concession was made this round.
    /// </summary>
    public bool HasConcession => ConcessionMade != null;

    /// <summary>
    /// Gets a value indicating whether this round incurred stress.
    /// </summary>
    public bool IncurredStress => StressCost > 0;

    /// <summary>
    /// Gets a value indicating whether this round incurred reputation cost.
    /// </summary>
    public bool IncurredReputationCost => ReputationCost != 0;

    /// <summary>
    /// Gets a formatted summary of this round for display.
    /// </summary>
    /// <returns>A compact summary string describing the round outcome.</returns>
    /// <remarks>
    /// Format: "Round {N}: {Tactic} - {Outcome}. {PositionMovement}"
    /// </remarks>
    public string GetSummary()
    {
        var outcome = TacticUsed == NegotiationTactic.Concede
            ? "Concession"
            : (CheckResult?.ToString() ?? "N/A");

        return $"Round {RoundNumber}: {TacticUsed.GetDisplayName()} - {outcome}. {PositionMovement}";
    }

    /// <summary>
    /// Gets a detailed summary including all costs and changes.
    /// </summary>
    /// <returns>A detailed multi-line summary of the round.</returns>
    public string GetDetailedSummary()
    {
        var lines = new List<string>
        {
            $"Round {RoundNumber}: {TacticUsed.GetDisplayName()}",
            $"  Outcome: {(CheckResult?.ToString() ?? "N/A")}",
            $"  Position: {PositionMovement}",
            $"  Gap: {GapAfter}"
        };

        if (EffectiveDcUsed.HasValue)
        {
            lines.Add($"  DC: {EffectiveDcUsed.Value}");
        }

        if (DicePoolUsed.HasValue)
        {
            lines.Add($"  Dice Pool: {DicePoolUsed.Value}d10");
        }

        if (HasConcession)
        {
            lines.Add($"  Concession: {ConcessionMade!.Type.GetDisplayName()}");
        }

        if (IncurredStress)
        {
            lines.Add($"  Stress: +{StressCost}");
        }

        if (IncurredReputationCost)
        {
            lines.Add($"  Reputation: {ReputationCost:+0;-0}");
        }

        if (DispositionChange != 0)
        {
            lines.Add($"  Disposition: {DispositionChange:+0;-0}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a short display string for compact UI elements.
    /// </summary>
    /// <returns>A very compact representation of the round.</returns>
    public string ToShortDisplay()
    {
        var indicator = IsSuccess ? "✓" : (IsFumble ? "✗✗" : "✗");
        return $"R{RoundNumber}: {TacticUsed.GetShortName()} {indicator} Gap:{GapAfter}";
    }

    /// <summary>
    /// Creates a round record for a successful check.
    /// </summary>
    /// <param name="roundNumber">The round number (1-based).</param>
    /// <param name="tactic">The tactic used.</param>
    /// <param name="outcome">The check outcome.</param>
    /// <param name="pcPosition">PC position after this round.</param>
    /// <param name="npcPosition">NPC position after this round.</param>
    /// <param name="positionMovement">Description of position movement.</param>
    /// <param name="narrative">Narrative description.</param>
    /// <param name="effectiveDc">The DC used for the check.</param>
    /// <param name="dicePool">The dice pool size used.</param>
    /// <returns>A new <see cref="NegotiationRound"/> recording the outcome.</returns>
    public static NegotiationRound ForCheck(
        int roundNumber,
        NegotiationTactic tactic,
        SkillOutcome outcome,
        int pcPosition,
        int npcPosition,
        string positionMovement,
        string narrative,
        int effectiveDc,
        int dicePool)
    {
        return new NegotiationRound
        {
            RoundNumber = roundNumber,
            TacticUsed = tactic,
            CheckResult = outcome,
            PcPositionAfter = pcPosition,
            NpcPositionAfter = npcPosition,
            GapAfter = Math.Abs(npcPosition - pcPosition),
            PositionMovement = positionMovement,
            NarrativeDescription = narrative,
            EffectiveDcUsed = effectiveDc,
            DicePoolUsed = dicePool,
            StressCost = 0,
            ReputationCost = 0,
            DispositionChange = 0
        };
    }

    /// <summary>
    /// Creates a round record for a concession.
    /// </summary>
    /// <param name="roundNumber">The round number (1-based).</param>
    /// <param name="concession">The concession made.</param>
    /// <param name="pcPosition">PC position after this round.</param>
    /// <param name="npcPosition">NPC position after this round.</param>
    /// <param name="narrative">Narrative description.</param>
    /// <returns>A new <see cref="NegotiationRound"/> recording the concession.</returns>
    public static NegotiationRound ForConcession(
        int roundNumber,
        Concession concession,
        int pcPosition,
        int npcPosition,
        string narrative)
    {
        return new NegotiationRound
        {
            RoundNumber = roundNumber,
            TacticUsed = NegotiationTactic.Concede,
            CheckResult = null,
            PcPositionAfter = pcPosition,
            NpcPositionAfter = npcPosition,
            GapAfter = Math.Abs(npcPosition - pcPosition),
            PositionMovement = "PC moved 1 step toward compromise",
            ConcessionMade = concession.WithRound(roundNumber),
            NarrativeDescription = narrative,
            StressCost = 0,
            ReputationCost = 0,
            DispositionChange = 0
        };
    }

    /// <summary>
    /// Creates a new round with stress cost applied.
    /// </summary>
    /// <param name="stress">The stress cost to apply.</param>
    /// <returns>A new <see cref="NegotiationRound"/> with the stress cost.</returns>
    public NegotiationRound WithStressCost(int stress)
    {
        return this with { StressCost = stress };
    }

    /// <summary>
    /// Creates a new round with reputation cost applied.
    /// </summary>
    /// <param name="reputation">The reputation cost to apply.</param>
    /// <returns>A new <see cref="NegotiationRound"/> with the reputation cost.</returns>
    public NegotiationRound WithReputationCost(int reputation)
    {
        return this with { ReputationCost = reputation };
    }

    /// <summary>
    /// Creates a new round with disposition change applied.
    /// </summary>
    /// <param name="disposition">The disposition change to apply.</param>
    /// <returns>A new <see cref="NegotiationRound"/> with the disposition change.</returns>
    public NegotiationRound WithDispositionChange(int disposition)
    {
        return this with { DispositionChange = disposition };
    }
}
