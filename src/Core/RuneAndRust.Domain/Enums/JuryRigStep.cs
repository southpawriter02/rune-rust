// ------------------------------------------------------------------------------
// <copyright file="JuryRigStep.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The steps in the jury-rigging trial-and-error procedure.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The steps in the jury-rigging trial-and-error procedure.
/// </summary>
/// <remarks>
/// <para>
/// The jury-rigging procedure follows a five-step flow:
/// <list type="number">
///   <item><description>Observe (optional): Study the mechanism visually (WITS DC 10)</description></item>
///   <item><description>Probe (automatic): Try obvious buttons and levers</description></item>
///   <item><description>Pattern (optional): Recognize mechanism type (WITS DC 12)</description></item>
///   <item><description>MethodSelection: Choose a bypass approach</description></item>
///   <item><description>Experiment: Attempt the bypass (System Bypass check)</description></item>
///   <item><description>Iterate (automatic): Learn from failure (DC -1 next attempt)</description></item>
/// </list>
/// </para>
/// <para>
/// Optional steps can be skipped, but provide bonuses when successful.
/// The procedure loops from Iterate back to MethodSelection until success,
/// permanent lockout, or mechanism destruction.
/// </para>
/// </remarks>
public enum JuryRigStep
{
    /// <summary>
    /// Study the mechanism visually to identify type and inputs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Check Type: WITS.
    /// DC: 10.
    /// Optional: Can skip to proceed blind.
    /// Success: Learn mechanism type, reveal potential methods.
    /// </para>
    /// </remarks>
    [Description("Study the mechanism visually (WITS DC 10)")]
    Observe = 0,

    /// <summary>
    /// Try obvious buttons and levers to establish baseline behavior.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Check Type: Automatic (no roll required).
    /// Outcome: Machine reacts (may beep, light up, or do nothing).
    /// Purpose: Establish baseline behavior before experimentation.
    /// </para>
    /// </remarks>
    [Description("Try obvious controls (automatic)")]
    Probe = 1,

    /// <summary>
    /// Attempt to recognize the mechanism type from past experience.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Check Type: WITS.
    /// DC: 12.
    /// Optional: Can skip to proceed without familiarity bonus.
    /// Success + Familiar: +2d10 bonus dice on experiment.
    /// Success + Unfamiliar: Mark mechanism type as "seen" for future encounters.
    /// </para>
    /// </remarks>
    [Description("Recognize mechanism type (WITS DC 12)")]
    Pattern = 2,

    /// <summary>
    /// Select a bypass method for the experiment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// No check required—player choice.
    /// Available methods depend on context (familiarity, glitched state).
    /// Each method has different DC modifiers and risks.
    /// </para>
    /// </remarks>
    [Description("Select a bypass method")]
    MethodSelection = 3,

    /// <summary>
    /// Attempt the bypass with the selected method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Check Type: System Bypass vs. Modified DC.
    /// DC Modified by: Method modifier, iteration learning.
    /// Dice Pool Modified by: Tools, familiarity bonus.
    /// Outcomes: Critical Success, Success, Failure (complication), Fumble (destruction).
    /// </para>
    /// </remarks>
    [Description("Attempt bypass with selected method")]
    Experiment = 4,

    /// <summary>
    /// Learn from failure and prepare for next attempt.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Automatic (no roll required).
    /// Effect: DC reduced by 1 for next attempt on same mechanism.
    /// Allows: Change bypass method for next attempt.
    /// Returns to: MethodSelection step.
    /// </para>
    /// </remarks>
    [Description("Learn from failure (DC -1 next attempt)")]
    Iterate = 5
}
