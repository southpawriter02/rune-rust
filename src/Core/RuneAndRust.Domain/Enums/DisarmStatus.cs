// ------------------------------------------------------------------------------
// <copyright file="DisarmStatus.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Tracks the overall status of a trap disarmament attempt.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks the overall status of a trap disarmament attempt.
/// </summary>
/// <remarks>
/// <para>
/// Status progression typically follows:
/// <list type="number">
///   <item><description>Undetected → Detected (successful detection)</description></item>
///   <item><description>Detected → Analyzed (after analysis) or DisarmInProgress (skip analysis)</description></item>
///   <item><description>Analyzed/DisarmInProgress → Disarmed (success) or Triggered/Destroyed (failure/fumble)</description></item>
/// </list>
/// </para>
/// <para>
/// Terminal states:
/// <list type="bullet">
///   <item><description>Disarmed: Trap successfully neutralized, may have salvage</description></item>
///   <item><description>Triggered: Trap activated (detection failure or fumble)</description></item>
///   <item><description>Destroyed: Trap destroyed during fumble, no salvage possible</description></item>
/// </list>
/// </para>
/// </remarks>
public enum DisarmStatus
{
    /// <summary>
    /// Trap has not yet been detected.
    /// </summary>
    /// <remarks>
    /// Initial state before detection attempt.
    /// </remarks>
    Undetected = 0,

    /// <summary>
    /// Trap has been detected but not yet analyzed or disarmed.
    /// </summary>
    /// <remarks>
    /// Character has found the trap and may proceed to Analysis or Disarmament.
    /// </remarks>
    Detected = 1,

    /// <summary>
    /// Trap has been analyzed, revealing information.
    /// </summary>
    /// <remarks>
    /// Analysis complete—revealed information stored in state.
    /// Character may now attempt disarmament with any hints gained.
    /// </remarks>
    Analyzed = 2,

    /// <summary>
    /// Disarmament attempt is in progress.
    /// </summary>
    /// <remarks>
    /// Active disarmament attempt (between detection and resolution).
    /// May have failed attempts recorded (DC escalation).
    /// </remarks>
    DisarmInProgress = 3,

    /// <summary>
    /// Trap has been successfully disarmed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal success state. Trap is neutralized.
    /// Critical success may have yielded salvage components.
    /// </para>
    /// </remarks>
    Disarmed = 4,

    /// <summary>
    /// Trap was triggered (detection failure).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal failure state from failed detection.
    /// Trap effects have been applied to the character.
    /// Trap is no longer a threat (already triggered).
    /// </para>
    /// </remarks>
    Triggered = 5,

    /// <summary>
    /// Trap was destroyed during a fumbled disarmament.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal failure state from fumbled disarmament ([Forced Execution]).
    /// Trap effects have been applied to the disarmer.
    /// Trap is destroyed—no salvage possible.
    /// </para>
    /// </remarks>
    Destroyed = 6
}
