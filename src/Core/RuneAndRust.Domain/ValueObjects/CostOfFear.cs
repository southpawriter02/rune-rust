// ------------------------------------------------------------------------------
// <copyright file="CostOfFear.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Tracks the mandatory reputation cost of intimidation attempts.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the mandatory reputation cost of using intimidation.
/// </summary>
/// <remarks>
/// <para>
/// Unlike other social interactions, intimidation ALWAYS costs faction
/// reputation regardless of outcome. This reflects that using fear as a
/// tool damages relationships even when successful.
/// </para>
/// <para>
/// Reputation costs by outcome:
/// </para>
/// <list type="bullet">
///   <item><description>Critical Success: -3 (efficient intimidation minimizes damage)</description></item>
///   <item><description>Success: -5 (standard fear-based cost, NPC tells others)</description></item>
///   <item><description>Failure: -10 (failed threat looks weak and desperate)</description></item>
///   <item><description>Fumble: -10 (plus combat initiation)</description></item>
/// </list>
/// <para>
/// This mechanic creates meaningful decisions: intimidation provides
/// immediate compliance but at the cost of long-term relationships.
/// </para>
/// </remarks>
/// <param name="Outcome">The outcome of the intimidation attempt.</param>
/// <param name="FactionId">The faction ID affected by the intimidation.</param>
public readonly record struct CostOfFear(SkillOutcome Outcome, string FactionId)
{
    /// <summary>
    /// Reputation cost for critical success (-3).
    /// </summary>
    /// <remarks>
    /// Efficient display of dominance minimizes lasting damage.
    /// </remarks>
    public const int CriticalSuccessReputationCost = -3;

    /// <summary>
    /// Reputation cost for standard success outcomes (-5).
    /// </summary>
    /// <remarks>
    /// Word of threatening behavior spreads through the faction.
    /// </remarks>
    public const int SuccessReputationCost = -5;

    /// <summary>
    /// Reputation cost for failure or fumble (-10).
    /// </summary>
    /// <remarks>
    /// Failed threats make the player look weak and desperate.
    /// For fumbles, this cost is applied before combat begins.
    /// </remarks>
    public const int FailureReputationCost = -10;

    /// <summary>
    /// Gets the reputation cost for this intimidation outcome.
    /// </summary>
    /// <remarks>
    /// Always negative: Critical Success -3, Success -5, Failure/Fumble -10.
    /// </remarks>
    public int ReputationCost => GetCostForOutcome(Outcome);

    /// <summary>
    /// Gets whether this outcome triggers the maximum reputation penalty.
    /// </summary>
    public bool HasMaximumPenalty => Outcome is SkillOutcome.Failure or SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets whether this outcome was successful (any success tier).
    /// </summary>
    public bool WasSuccessful => Outcome is SkillOutcome.CriticalSuccess
        or SkillOutcome.ExceptionalSuccess
        or SkillOutcome.FullSuccess
        or SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets whether this outcome was a fumble (CriticalFailure).
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets the absolute value of the reputation cost.
    /// </summary>
    /// <remarks>
    /// Useful for display purposes where you want to show "5" instead of "-5".
    /// </remarks>
    public int AbsoluteCost => Math.Abs(ReputationCost);

    /// <summary>
    /// Gets a narrative description of the reputation impact.
    /// </summary>
    /// <returns>A flavor text string for display.</returns>
    public string GetNarrativeDescription()
    {
        return Outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                $"Your efficient display of dominance minimizes lasting damage. (Reputation {ReputationCost})",
            SkillOutcome.ExceptionalSuccess or SkillOutcome.FullSuccess =>
                $"Word of your threatening behavior spreads. (Reputation {ReputationCost})",
            SkillOutcome.MarginalSuccess =>
                $"Your barely successful threat still damages relationships. (Reputation {ReputationCost})",
            SkillOutcome.Failure =>
                $"Your failed threat makes you look weak and desperate. (Reputation {ReputationCost})",
            SkillOutcome.CriticalFailure =>
                $"Your catastrophic attempt at intimidation is the talk of the settlement. (Reputation {ReputationCost})",
            _ => $"Using fear as a tool damages relationships. (Reputation {ReputationCost})"
        };
    }

    /// <summary>
    /// Gets a narrative description including the faction name.
    /// </summary>
    /// <param name="factionName">The display name of the affected faction.</param>
    /// <returns>A flavor text string with faction name for display.</returns>
    public string GetNarrativeDescription(string factionName)
    {
        return Outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                $"Your efficient display of dominance minimizes lasting damage. ({factionName} Reputation {ReputationCost})",
            SkillOutcome.ExceptionalSuccess or SkillOutcome.FullSuccess =>
                $"Word of your threatening behavior spreads. ({factionName} Reputation {ReputationCost})",
            SkillOutcome.MarginalSuccess =>
                $"Your barely successful threat still damages relationships. ({factionName} Reputation {ReputationCost})",
            SkillOutcome.Failure =>
                $"Your failed threat makes you look weak and desperate. ({factionName} Reputation {ReputationCost})",
            SkillOutcome.CriticalFailure =>
                $"Your catastrophic attempt at intimidation is the talk of the settlement. ({factionName} Reputation {ReputationCost})",
            _ => $"Using fear as a tool damages relationships. ({factionName} Reputation {ReputationCost})"
        };
    }

    /// <summary>
    /// Gets the reputation cost for a given outcome.
    /// </summary>
    /// <param name="outcome">The intimidation outcome.</param>
    /// <returns>The reputation cost (always negative).</returns>
    public static int GetCostForOutcome(SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalSuccess => CriticalSuccessReputationCost,
            SkillOutcome.ExceptionalSuccess => SuccessReputationCost,
            SkillOutcome.FullSuccess => SuccessReputationCost,
            SkillOutcome.MarginalSuccess => SuccessReputationCost,
            SkillOutcome.Failure => FailureReputationCost,
            SkillOutcome.CriticalFailure => FailureReputationCost,
            _ => SuccessReputationCost
        };
    }

    /// <summary>
    /// Creates a CostOfFear for a successful intimidation.
    /// </summary>
    /// <param name="factionId">The affected faction ID.</param>
    /// <param name="outcome">The success outcome (defaults to FullSuccess).</param>
    /// <returns>A CostOfFear with -3 or -5 reputation cost.</returns>
    public static CostOfFear ForSuccess(string factionId, SkillOutcome outcome = SkillOutcome.FullSuccess)
    {
        return new CostOfFear(outcome, factionId);
    }

    /// <summary>
    /// Creates a CostOfFear for a critical success.
    /// </summary>
    /// <param name="factionId">The affected faction ID.</param>
    /// <returns>A CostOfFear with -3 reputation cost.</returns>
    public static CostOfFear ForCriticalSuccess(string factionId)
    {
        return new CostOfFear(SkillOutcome.CriticalSuccess, factionId);
    }

    /// <summary>
    /// Creates a CostOfFear for a failed intimidation.
    /// </summary>
    /// <param name="factionId">The affected faction ID.</param>
    /// <returns>A CostOfFear with -10 reputation cost.</returns>
    public static CostOfFear ForFailure(string factionId)
    {
        return new CostOfFear(SkillOutcome.Failure, factionId);
    }

    /// <summary>
    /// Creates a CostOfFear for a fumbled intimidation with [Challenge Accepted].
    /// </summary>
    /// <param name="factionId">The affected faction ID.</param>
    /// <returns>A CostOfFear with -10 reputation cost.</returns>
    /// <remarks>
    /// Note: The fumble also triggers immediate combat with [Furious] buff
    /// and +5 Psychic Stress, which are handled separately from reputation cost.
    /// </remarks>
    public static CostOfFear ForFumble(string factionId)
    {
        return new CostOfFear(SkillOutcome.CriticalFailure, factionId);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Cost of Fear: {ReputationCost} ({Outcome})";
}
