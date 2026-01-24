// ------------------------------------------------------------------------------
// <copyright file="BypassAbilityType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the types of specialization abilities available in the System Bypass
// skill system, categorizing how and when abilities activate.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of specialization abilities in the System Bypass skill system.
/// </summary>
/// <remarks>
/// <para>
/// Ability types determine when and how abilities activate:
/// <list type="bullet">
///   <item><description><see cref="Passive"/>: Always active when conditions are met, no action required</description></item>
///   <item><description><see cref="Triggered"/>: Automatically activate on specific outcomes</description></item>
///   <item><description><see cref="UniqueAction"/>: Grant new actions not available to other characters</description></item>
/// </list>
/// </para>
/// <para>
/// This pattern follows the Specialization Bonus Pattern established in v0.15.4i
/// and can be reused across other skill expansions.
/// </para>
/// </remarks>
public enum BypassAbilityType
{
    /// <summary>
    /// Always active when conditions are met. No action required.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Passive abilities provide constant bonuses that apply automatically
    /// whenever their trigger conditions are satisfied.
    /// </para>
    /// <para>
    /// <b>Examples:</b>
    /// <list type="bullet">
    ///   <item><description>[Sixth Sense] - Auto-detect traps within 10 feet (Ruin-Stalker)</description></item>
    ///   <item><description>[Pattern Recognition] - Reduce [Glitched] penalty (Jötun-Reader)</description></item>
    ///   <item><description>[Fast Pick] - Reduce bypass time (Gantry-Runner)</description></item>
    ///   <item><description>[Bypass Under Fire] - Negate combat penalties (Gantry-Runner)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Always active when conditions met")]
    Passive = 0,

    /// <summary>
    /// Activates automatically on specific outcomes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered abilities fire automatically when their trigger condition
    /// is met, providing additional effects on top of the normal outcome.
    /// </para>
    /// <para>
    /// <b>Examples:</b>
    /// <list type="bullet">
    ///   <item><description>[Deep Access] - On terminal hack success, gain Admin-Level (Jötun-Reader)</description></item>
    ///   <item><description>[Master Craftsman] - On craft attempt, unlock masterwork recipes (Scrap-Tinker)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Activates on specific outcomes")]
    Triggered = 1,

    /// <summary>
    /// Grants new actions not available to other characters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unique actions are special commands that only characters with specific
    /// specializations can perform. They require explicit activation.
    /// </para>
    /// <para>
    /// <b>Examples:</b>
    /// <list type="bullet">
    ///   <item><description>[Relock] - Re-lock a bypassed lock (Scrap-Tinker, WITS DC 12)</description></item>
    ///   <item><description>[Trap Artist] - Re-arm a disabled trap (Ruin-Stalker, WITS DC 14)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Grants new actions")]
    UniqueAction = 2
}
