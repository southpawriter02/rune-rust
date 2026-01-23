// ------------------------------------------------------------------------------
// <copyright file="NegotiationContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Value object containing all context needed for a negotiation round,
// including the base social context, position track, selected tactic,
// and any active concession bonuses.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Contains all context needed for executing a negotiation round.
/// </summary>
/// <remarks>
/// <para>
/// The negotiation context aggregates information from multiple sources:
/// </para>
/// <list type="bullet">
///   <item><description>Base social context (disposition, modifiers, etc.)</description></item>
///   <item><description>Request complexity (determines base DC and initial gap)</description></item>
///   <item><description>Current position track state</description></item>
///   <item><description>Selected tactic for this round</description></item>
///   <item><description>Active concession bonus from previous round</description></item>
///   <item><description>NPC flexibility factor</description></item>
/// </list>
/// <para>
/// This value object is used by the NegotiationService to execute rounds
/// and determine outcomes.
/// </para>
/// </remarks>
public sealed record NegotiationContext
{
    /// <summary>
    /// Gets the base social context containing disposition and modifiers.
    /// </summary>
    /// <remarks>
    /// The social context provides:
    /// <list type="bullet">
    ///   <item><description>Target disposition and dice modifiers</description></item>
    ///   <item><description>Equipment modifiers (evidence, documents)</description></item>
    ///   <item><description>Situational modifiers (time pressure, etc.)</description></item>
    ///   <item><description>Applied status effects</description></item>
    /// </list>
    /// </remarks>
    public required SocialContext BaseContext { get; init; }

    /// <summary>
    /// Gets the complexity of what the player is asking for.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Request complexity determines:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base DC for all checks (10-26)</description></item>
    ///   <item><description>Initial gap between positions (2-6)</description></item>
    ///   <item><description>Default number of rounds (3-7)</description></item>
    /// </list>
    /// </remarks>
    public required RequestComplexity RequestComplexity { get; init; }

    /// <summary>
    /// Gets the current state of the negotiation position track.
    /// </summary>
    /// <remarks>
    /// Contains PC/NPC positions, round history, and current status.
    /// </remarks>
    public required NegotiationPositionTrack PositionTrack { get; init; }

    /// <summary>
    /// Gets the tactic selected for this round.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Available tactics:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Persuade: Uses Rhetoric (Persuasion) - no side effects</description></item>
    ///   <item><description>Deceive: Uses Rhetoric (Deception) - incurs Psychic Stress</description></item>
    ///   <item><description>Pressure: Uses Rhetoric (Intimidation) - costs reputation</description></item>
    ///   <item><description>Concede: No check - voluntary position movement, gains bonus</description></item>
    /// </list>
    /// </remarks>
    public required NegotiationTactic SelectedTactic { get; init; }

    /// <summary>
    /// Gets the active concession bonus from a previous Concede action, if any.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When active, provides:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>+2d10 bonus dice on the next check</description></item>
    ///   <item><description>DC reduction based on concession type (-2 to -6)</description></item>
    /// </list>
    /// <para>
    /// The bonus is consumed after being applied to a check.
    /// </para>
    /// </remarks>
    public Concession? ActiveConcession { get; init; }

    /// <summary>
    /// Gets the NPC's flexibility factor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Range 1-3 where higher values indicate more flexibility:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>1: Inflexible - stubborn, hard to move</description></item>
    ///   <item><description>2: Average - normal negotiation</description></item>
    ///   <item><description>3: Flexible - willing to compromise</description></item>
    /// </list>
    /// <para>
    /// Affects how much the NPC position moves on successful checks.
    /// </para>
    /// </remarks>
    public required int NpcFlexibility { get; init; }

    /// <summary>
    /// Gets the concession being offered this round, if using the Concede tactic.
    /// </summary>
    /// <remarks>
    /// Only populated when <see cref="SelectedTactic"/> is <see cref="NegotiationTactic.Concede"/>.
    /// </remarks>
    public Concession? ConcessionOffer { get; init; }

    /// <summary>
    /// Gets the current round number (from position track).
    /// </summary>
    public int CurrentRound => PositionTrack.RoundNumber + 1;

    /// <summary>
    /// Gets the rounds remaining (from position track).
    /// </summary>
    public int RoundsRemaining => PositionTrack.RoundsRemaining;

    /// <summary>
    /// Gets the current negotiation status (from position track).
    /// </summary>
    public NegotiationStatus CurrentStatus => PositionTrack.Status;

    /// <summary>
    /// Gets the current gap between positions (from position track).
    /// </summary>
    public int CurrentGap => PositionTrack.Gap;

    /// <summary>
    /// Gets the base DC for this negotiation, determined by request complexity.
    /// </summary>
    /// <returns>The base DC value (10, 14, 18, 22, or 26).</returns>
    public int GetBaseDc()
    {
        return RequestComplexity.GetBaseDc();
    }

    /// <summary>
    /// Gets the effective DC after applying all modifiers.
    /// </summary>
    /// <returns>
    /// The final DC after applying social context modifiers and concession reductions.
    /// Minimum value is 4.
    /// </returns>
    /// <remarks>
    /// <para>
    /// DC calculation:
    /// </para>
    /// <list type="number">
    ///   <item><description>Start with base DC from request complexity</description></item>
    ///   <item><description>Add/subtract social context DC modifiers</description></item>
    ///   <item><description>Subtract active concession DC reduction (if any)</description></item>
    ///   <item><description>Clamp to minimum of 4</description></item>
    /// </list>
    /// </remarks>
    public int GetEffectiveDc()
    {
        var baseDc = GetBaseDc();

        // Apply social context modifiers
        baseDc += BaseContext.TotalDcModifier;

        // Apply concession reduction
        if (ActiveConcession != null)
        {
            baseDc -= ActiveConcession.DcReduction;
        }

        // Minimum DC of 4
        return Math.Max(baseDc, 4);
    }

    /// <summary>
    /// Gets the bonus dice from an active concession.
    /// </summary>
    /// <returns>The number of bonus d10 dice (typically 2), or 0 if no concession is active.</returns>
    public int GetConcessionBonusDice()
    {
        return ActiveConcession?.BonusDice ?? 0;
    }

    /// <summary>
    /// Gets the total dice modifier for this round's check.
    /// </summary>
    /// <returns>The total dice modifier combining all sources.</returns>
    /// <remarks>
    /// <para>
    /// Dice modifiers include:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Disposition modifier from social context</description></item>
    ///   <item><description>Social modifiers from social context</description></item>
    ///   <item><description>Equipment modifiers from social context</description></item>
    ///   <item><description>Situational modifiers from social context</description></item>
    ///   <item><description>Concession bonus dice (if active)</description></item>
    /// </list>
    /// </remarks>
    public int GetTotalDiceModifier()
    {
        return BaseContext.TotalDiceModifier + GetConcessionBonusDice();
    }

    /// <summary>
    /// Gets the position movement steps for a given outcome.
    /// </summary>
    /// <param name="outcome">The skill check outcome.</param>
    /// <returns>The number of steps to move.</returns>
    /// <remarks>
    /// <para>
    /// Movement rules:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>CriticalSuccess: 2 steps</description></item>
    ///   <item><description>ExceptionalSuccess: 2 steps</description></item>
    ///   <item><description>FullSuccess: 1 step</description></item>
    ///   <item><description>MarginalSuccess: 1 step</description></item>
    ///   <item><description>Failure: 1 step</description></item>
    ///   <item><description>CriticalFailure: 2 steps</description></item>
    /// </list>
    /// </remarks>
    public static int GetPositionMovementSteps(SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalSuccess => 2,
            SkillOutcome.ExceptionalSuccess => 2,
            SkillOutcome.FullSuccess => 1,
            SkillOutcome.MarginalSuccess => 1,
            SkillOutcome.Failure => 1,
            SkillOutcome.CriticalFailure => 2,
            _ => 1
        };
    }

    /// <summary>
    /// Determines if this context is valid for executing a round.
    /// </summary>
    /// <returns>True if the context is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        // Cannot execute rounds in terminal states
        if (CurrentStatus.IsTerminal())
        {
            return false;
        }

        // Concede tactic requires a concession offer
        if (SelectedTactic == NegotiationTactic.Concede && ConcessionOffer == null)
        {
            return false;
        }

        // Flexibility must be in valid range
        if (NpcFlexibility < 1 || NpcFlexibility > 3)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a validation message if the context is invalid.
    /// </summary>
    /// <returns>A message describing why the context is invalid, or null if valid.</returns>
    public string? GetValidationError()
    {
        if (CurrentStatus.IsTerminal())
        {
            return $"Cannot execute rounds: negotiation has ended with status {CurrentStatus}";
        }

        if (SelectedTactic == NegotiationTactic.Concede && ConcessionOffer == null)
        {
            return "Concede tactic requires a concession offer";
        }

        if (NpcFlexibility < 1 || NpcFlexibility > 3)
        {
            return $"NPC flexibility must be between 1 and 3, got {NpcFlexibility}";
        }

        return null;
    }

    /// <summary>
    /// Determines if the selected tactic is risky in the current state.
    /// </summary>
    /// <returns>True if the tactic has elevated risk; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// Risk assessment:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Pressure during CrisisManagement: Very risky (fumble collapses)</description></item>
    ///   <item><description>Deceive during CrisisManagement: Risky (fumble collapses + stress)</description></item>
    ///   <item><description>Any tactic when gap ≥ 5: Elevated risk</description></item>
    /// </list>
    /// </remarks>
    public bool IsTacticRisky()
    {
        var inCrisis = CurrentStatus == NegotiationStatus.CrisisManagement;

        return SelectedTactic switch
        {
            NegotiationTactic.Pressure => inCrisis || CurrentGap >= 5,
            NegotiationTactic.Deceive => inCrisis,
            NegotiationTactic.Persuade => inCrisis && CurrentGap >= 6,
            NegotiationTactic.Concede => false, // Concessions are never risky
            _ => false
        };
    }

    /// <summary>
    /// Gets a warning message about the current tactic choice, if applicable.
    /// </summary>
    /// <returns>A warning message, or null if no warning is needed.</returns>
    public string? GetTacticWarning()
    {
        var inCrisis = CurrentStatus == NegotiationStatus.CrisisManagement;

        if (inCrisis)
        {
            return SelectedTactic switch
            {
                NegotiationTactic.Pressure =>
                    "Warning: A fumble during Crisis Management will immediately collapse the negotiation!",
                NegotiationTactic.Deceive =>
                    "Warning: A fumble during Crisis Management will collapse the negotiation and expose your deception!",
                NegotiationTactic.Persuade when CurrentGap >= 6 =>
                    "Warning: The gap is very wide. Consider using a concession to improve your odds.",
                _ => null
            };
        }

        return SelectedTactic switch
        {
            NegotiationTactic.Pressure when CurrentGap >= 5 =>
                "Caution: Using Pressure with a wide gap may trigger Crisis Management.",
            NegotiationTactic.Deceive =>
                $"Note: Deception will cost {BaseContext.PotentialSuccessStressCost}-{BaseContext.PotentialFailureStressCost} Psychic Stress.",
            _ => null
        };
    }

    /// <summary>
    /// Builds a detailed breakdown of all modifiers affecting this round.
    /// </summary>
    /// <returns>A multi-line string showing all modifiers.</returns>
    public string ToModifierBreakdown()
    {
        var lines = new List<string>
        {
            $"Negotiation Round {CurrentRound}: {SelectedTactic.GetDisplayName()}",
            $"Request: {RequestComplexity.GetDisplayName()} (Base DC {GetBaseDc()})",
            $"Status: {CurrentStatus.GetDisplayName()}, Gap: {CurrentGap}",
            ""
        };

        // Add social context modifiers
        if (BaseContext.DispositionDiceModifier != 0)
        {
            var sign = BaseContext.DispositionDiceModifier > 0 ? "+" : "";
            lines.Add($"  Disposition ({BaseContext.DispositionCategory}): {sign}{BaseContext.DispositionDiceModifier}d10");
        }

        if (BaseContext.SocialModifiers.Count > 0)
        {
            lines.Add("  Social Modifiers:");
            foreach (var mod in BaseContext.SocialModifiers)
            {
                lines.Add($"    {mod.ToShortDescription()}");
            }
        }

        if (BaseContext.EquipmentModifiers.Count > 0)
        {
            lines.Add("  Equipment Modifiers:");
            foreach (var mod in BaseContext.EquipmentModifiers)
            {
                lines.Add($"    {mod.ToShortDescription()}");
            }
        }

        if (BaseContext.SituationalModifiers.Count > 0)
        {
            lines.Add("  Situational Modifiers:");
            foreach (var mod in BaseContext.SituationalModifiers)
            {
                lines.Add($"    {mod.ToShortDescription()}");
            }
        }

        // Add concession bonus
        if (ActiveConcession != null)
        {
            lines.Add($"  Active Concession: +{ActiveConcession.BonusDice}d10, -{ActiveConcession.DcReduction} DC");
        }

        lines.Add("");
        lines.Add($"Effective DC: {GetEffectiveDc()}");
        lines.Add($"Total Dice Modifier: {GetTotalDiceModifier():+0;-0}d10");

        var warning = GetTacticWarning();
        if (warning != null)
        {
            lines.Add("");
            lines.Add(warning);
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a short summary of this context for logging.
    /// </summary>
    /// <returns>A compact single-line summary.</returns>
    public override string ToString()
    {
        var concessionInfo = ActiveConcession != null
            ? $" [Concession: -{ActiveConcession.DcReduction}DC]"
            : "";

        return $"Round {CurrentRound}: {SelectedTactic} vs DC {GetEffectiveDc()}, " +
               $"Gap {CurrentGap}, Status {CurrentStatus}{concessionInfo}";
    }

    /// <summary>
    /// Creates a minimal negotiation context for testing purposes.
    /// </summary>
    /// <param name="npcId">The NPC ID.</param>
    /// <param name="complexity">The request complexity.</param>
    /// <param name="tactic">The selected tactic.</param>
    /// <returns>A minimal negotiation context.</returns>
    public static NegotiationContext CreateMinimal(
        string npcId,
        RequestComplexity complexity = RequestComplexity.FairTrade,
        NegotiationTactic tactic = NegotiationTactic.Persuade)
    {
        var positionTrack = new NegotiationPositionTrack
        {
            NegotiationId = Guid.NewGuid().ToString(),
            NpcId = npcId,
            RequestComplexity = complexity
        };

        positionTrack.Initialize(
            complexity.GetDefaultPcStartPosition(),
            complexity.GetDefaultNpcStartPosition(),
            complexity.GetDefaultRounds());

        return new NegotiationContext
        {
            BaseContext = SocialContext.CreateMinimal(npcId, SocialInteractionType.Negotiation),
            RequestComplexity = complexity,
            PositionTrack = positionTrack,
            SelectedTactic = tactic,
            NpcFlexibility = 2,
            ActiveConcession = null,
            ConcessionOffer = null
        };
    }
}
