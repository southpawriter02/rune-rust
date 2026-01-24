// ------------------------------------------------------------------------------
// <copyright file="BypassPassiveBonusType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the types of passive bonuses that specialization abilities provide
// in the System Bypass skill system.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of passive bonuses provided by specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// Passive bonus types are a subset of effect types that specifically
/// apply as constant modifiers when conditions are met:
/// <list type="bullet">
///   <item><description><see cref="TrapDetection"/>: Bonus to detecting traps</description></item>
///   <item><description><see cref="GlitchResistance"/>: Reduction to glitch penalties</description></item>
///   <item><description><see cref="SpeedBonus"/>: Reduction to time required</description></item>
///   <item><description><see cref="PressureImmunity"/>: Immunity to danger penalties</description></item>
/// </list>
/// </para>
/// </remarks>
public enum BypassPassiveBonusType
{
    /// <summary>
    /// No passive bonus.
    /// </summary>
    [Description("No bonus")]
    None = 0,

    /// <summary>
    /// Bonus to trap detection.
    /// </summary>
    /// <remarks>
    /// Provides automatic detection of traps within a certain radius,
    /// or bonus dice to trap detection checks.
    /// <para>
    /// <b>Abilities:</b> [Sixth Sense] (10 ft auto-detection radius)
    /// </para>
    /// </remarks>
    [Description("Trap detection")]
    TrapDetection = 1,

    /// <summary>
    /// Resistance to [Glitched] penalties.
    /// </summary>
    /// <remarks>
    /// Reduces the DC penalty from [Glitched] technology by a fixed amount.
    /// <para>
    /// <b>Abilities:</b> [Pattern Recognition] (reduce penalty by 2)
    /// </para>
    /// </remarks>
    [Description("Glitch resistance")]
    GlitchResistance = 2,

    /// <summary>
    /// Bonus to bypass speed.
    /// </summary>
    /// <remarks>
    /// Reduces the number of rounds required to complete bypass attempts.
    /// <para>
    /// <b>Abilities:</b> [Fast Pick] (reduce by 1 round)
    /// </para>
    /// </remarks>
    [Description("Speed bonus")]
    SpeedBonus = 3,

    /// <summary>
    /// Immunity to danger/pressure penalties.
    /// </summary>
    /// <remarks>
    /// Negates penalties that would apply when attempting bypass while
    /// in combat, under threat, or being observed.
    /// <para>
    /// <b>Abilities:</b> [Bypass Under Fire] (ignore combat penalties)
    /// </para>
    /// </remarks>
    [Description("Pressure immunity")]
    PressureImmunity = 4
}
