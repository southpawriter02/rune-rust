// ------------------------------------------------------------------------------
// <copyright file="ChaosEffect.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the possible effects when rolling on the Chaos table during
// glitch exploitation without first identifying the pattern.
// Part of v0.15.4f Glitch Exploitation System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The effect categories for chaos rolls when exploiting a glitch
/// without first identifying the pattern.
/// </summary>
/// <remarks>
/// <para>
/// When a character attempts glitch exploitation without successfully
/// observing the pattern (WITS DC 14), they must roll d6 on the Chaos table:
/// <list type="bullet">
///   <item><description>1-2 (33%): Against - Glitch resists (+4 DC)</description></item>
///   <item><description>3-4 (33%): Neutral - No effect (+0 DC)</description></item>
///   <item><description>5-6 (33%): Helps - Glitch aids (-2 DC)</description></item>
/// </list>
/// </para>
/// <para>
/// The chaos roll represents the unpredictable nature of corrupted technology.
/// Unlike identified patterns where the character can time their actions,
/// chaos rolls are a gamble that may help or hinder the bypass attempt.
/// </para>
/// </remarks>
public enum ChaosEffect
{
    /// <summary>
    /// The glitch actively resists the character's interference.
    /// </summary>
    /// <remarks>
    /// <para>
    /// d6 Roll: 1-2 (33% chance).
    /// DC Modifier: +4 (harder).
    /// The glitch pulses angrily as you approach, its erratic behavior
    /// intensifying as if the machine itself rejects your interference.
    /// Security protocols flare to life in unexpected ways.
    /// </para>
    /// </remarks>
    [Description("The glitch actively resists your interference (+4 DC)")]
    Against = 0,

    /// <summary>
    /// The glitch has no particular effect on the attempt.
    /// </summary>
    /// <remarks>
    /// <para>
    /// d6 Roll: 3-4 (33% chance).
    /// DC Modifier: +0 (normal difficulty).
    /// The machine's corrupted patterns continue their chaotic dance,
    /// neither helping nor hindering your attempt. You proceed against
    /// the normal [Glitched] resistance.
    /// </para>
    /// </remarks>
    [Description("The glitch has no particular effect (+0 DC)")]
    Neutral = 1,

    /// <summary>
    /// The glitch coincidentally aids the character's attempt.
    /// </summary>
    /// <remarks>
    /// <para>
    /// d6 Roll: 5-6 (33% chance).
    /// DC Modifier: -2 (easier).
    /// Fortune favors the bold! The glitch cycles into a momentarily
    /// permissive state just as you act. The machine's own malfunction
    /// becomes your ally.
    /// </para>
    /// </remarks>
    [Description("The glitch coincidentally aids your attempt (-2 DC)")]
    Helps = 2
}
