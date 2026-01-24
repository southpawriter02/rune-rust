// ------------------------------------------------------------------------------
// <copyright file="GlitchCyclePhase.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the phases of a glitch cycle in corrupted Old World technology.
// Glitched mechanisms cycle through these phases, with the Permissive phase
// providing an exploit window for skilled characters.
// Part of v0.15.4f Glitch Exploitation System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The phases of a glitch cycle in corrupted Old World technology.
/// </summary>
/// <remarks>
/// <para>
/// Glitched mechanisms cycle through four phases in sequence:
/// Stable → Unstable → Permissive → Lockdown → (repeat).
/// </para>
/// <para>
/// Phase effects on DC:
/// <list type="bullet">
///   <item><description>Stable: +0 DC (normal [Glitched] behavior)</description></item>
///   <item><description>Unstable: +0 DC (erratic but no modifier)</description></item>
///   <item><description>Permissive: -4 DC (exploit window!)</description></item>
///   <item><description>Lockdown: +2 DC (system compensates)</description></item>
/// </list>
/// </para>
/// <para>
/// Characters who successfully observe the pattern (WITS DC 14) can time
/// their actions to coincide with the Permissive phase for maximum benefit.
/// Those who fail observation must roll on the Chaos table for a random modifier.
/// </para>
/// </remarks>
public enum GlitchCyclePhase
{
    /// <summary>
    /// Mechanism operates with its usual erratic [Glitched] hum.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: +0.
    /// Visual: Steady but flickering lights, occasional sparks.
    /// Audio: Low electrical hum, intermittent clicks.
    /// Duration: 1d4+1 rounds (2-5 rounds).
    /// </para>
    /// </remarks>
    [Description("The mechanism operates with its usual erratic hum")]
    Stable = 0,

    /// <summary>
    /// Erratic behavior increases as the glitch cycle intensifies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: +0.
    /// Visual: Rapid light flickering, visible sparks, display glitches.
    /// Audio: Crackling electricity, high-pitched whine, distorted sounds.
    /// Duration: 1d4+1 rounds (2-5 rounds).
    /// Precedes: The Permissive phase exploit window.
    /// </para>
    /// </remarks>
    [Description("Lights flicker rapidly and strange sounds emanate from within")]
    Unstable = 1,

    /// <summary>
    /// Security momentarily lapses—the exploit window for -4 DC bonus.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: -4 (EXPLOIT WINDOW).
    /// Visual: Lights dim suddenly, access panels loosen, security indicators off.
    /// Audio: Sudden silence, soft clicks, whirring down.
    /// Duration: 1d4 rounds (1-4 rounds, shorter than other phases).
    /// Action: Strike now for maximum advantage!
    /// </para>
    /// </remarks>
    [Description("The mechanism's defenses momentarily lapse!")]
    Permissive = 2,

    /// <summary>
    /// System compensates for the glitch, becoming temporarily more resistant.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: +2 (harder).
    /// Visual: Red warning lights, panels seal tight, security active.
    /// Audio: Alarm tone, heavy clicking locks, power surge hum.
    /// Duration: 1d4+1 rounds (2-5 rounds).
    /// Strategy: Wait for cycle to return to Stable or Permissive.
    /// </para>
    /// </remarks>
    [Description("The system compensates, becoming more resistant")]
    Lockdown = 3
}
