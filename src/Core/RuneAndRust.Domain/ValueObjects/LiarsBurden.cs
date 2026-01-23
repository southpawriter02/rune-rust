// ------------------------------------------------------------------------------
// <copyright file="LiarsBurden.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Tracks the psychic stress cost of deception attempts.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks the psychic stress cost of deception attempts.
/// </summary>
/// <remarks>
/// <para>
/// The Liar's Burden represents the psychological toll of maintaining
/// falsehoods. Unlike persuasion, deception always incurs a stress cost
/// regardless of outcome, though the cost varies by result:
/// </para>
/// <list type="bullet">
///   <item><description>Success: +1 Psychic Stress</description></item>
///   <item><description>Failure: +3 Psychic Stress</description></item>
///   <item><description>Fumble: +8 Psychic Stress (plus +5 from [Lie Exposed])</description></item>
/// </list>
/// <para>
/// This mechanic discourages over-reliance on deception and creates
/// interesting resource management decisions for players.
/// </para>
/// </remarks>
/// <param name="Outcome">The outcome of the deception attempt.</param>
/// <param name="IsFumble">Whether the roll was a fumble (0 successes + ≥1 botch).</param>
public readonly record struct LiarsBurden(SkillOutcome Outcome, bool IsFumble)
{
    /// <summary>
    /// Stress cost for successful deception.
    /// </summary>
    public const int SuccessStressCost = 1;

    /// <summary>
    /// Stress cost for failed deception.
    /// </summary>
    public const int FailureStressCost = 3;

    /// <summary>
    /// Base stress cost for fumbled deception.
    /// </summary>
    public const int FumbleBaseStressCost = 8;

    /// <summary>
    /// Additional stress from [Lie Exposed] consequence.
    /// </summary>
    public const int LieExposedAdditionalStress = 5;

    /// <summary>
    /// Gets the base stress cost from the Liar's Burden.
    /// </summary>
    public int BaseStressCost => Outcome switch
    {
        SkillOutcome.CriticalSuccess => SuccessStressCost,
        SkillOutcome.ExceptionalSuccess => SuccessStressCost,
        SkillOutcome.FullSuccess => SuccessStressCost,
        SkillOutcome.MarginalSuccess => SuccessStressCost,
        SkillOutcome.Failure => FailureStressCost,
        SkillOutcome.CriticalFailure => FumbleBaseStressCost,
        _ => FailureStressCost
    };

    /// <summary>
    /// Gets the additional stress from [Lie Exposed] if applicable.
    /// </summary>
    public int LieExposedStress => IsFumble ? LieExposedAdditionalStress : 0;

    /// <summary>
    /// Gets the total stress cost.
    /// </summary>
    /// <remarks>
    /// Total stress: Base + LieExposed additional (fumble only).
    /// Success: 1, Failure: 3, Fumble: 8 + 5 = 13.
    /// </remarks>
    public int TotalStressCost => BaseStressCost + LieExposedStress;

    /// <summary>
    /// Gets whether this burden includes [Lie Exposed] consequences.
    /// </summary>
    public bool HasLieExposedConsequence => IsFumble;

    /// <summary>
    /// Gets a narrative description of the stress effect.
    /// </summary>
    /// <returns>A flavor text string for display.</returns>
    public string GetNarrativeDescription()
    {
        if (IsFumble)
        {
            return $"The shame of exposure burns in your mind. (+{TotalStressCost} Psychic Stress)";
        }

        return Outcome switch
        {
            SkillOutcome.CriticalSuccess or
            SkillOutcome.ExceptionalSuccess or
            SkillOutcome.FullSuccess =>
                $"The weight of deception settles in your mind. (+{TotalStressCost} Psychic Stress)",
            SkillOutcome.MarginalSuccess =>
                $"You got away with it, but the close call rattles you. (+{TotalStressCost} Psychic Stress)",
            SkillOutcome.Failure =>
                $"Your failed lie haunts you. Did they see through you completely? (+{TotalStressCost} Psychic Stress)",
            _ => $"The lie takes its toll. (+{TotalStressCost} Psychic Stress)"
        };
    }

    /// <summary>
    /// Creates a LiarsBurden for a successful deception.
    /// </summary>
    /// <param name="outcome">The success outcome (defaults to FullSuccess).</param>
    /// <returns>A LiarsBurden with +1 stress cost.</returns>
    public static LiarsBurden ForSuccess(SkillOutcome outcome = SkillOutcome.FullSuccess)
    {
        return new LiarsBurden(outcome, false);
    }

    /// <summary>
    /// Creates a LiarsBurden for a failed deception.
    /// </summary>
    /// <returns>A LiarsBurden with +3 stress cost.</returns>
    public static LiarsBurden ForFailure()
    {
        return new LiarsBurden(SkillOutcome.Failure, false);
    }

    /// <summary>
    /// Creates a LiarsBurden for a fumbled deception with [Lie Exposed].
    /// </summary>
    /// <returns>A LiarsBurden with +13 stress cost (8 base + 5 exposure).</returns>
    public static LiarsBurden ForFumble()
    {
        return new LiarsBurden(SkillOutcome.CriticalFailure, true);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Liar's Burden: +{TotalStressCost} Stress ({Outcome})";
}
