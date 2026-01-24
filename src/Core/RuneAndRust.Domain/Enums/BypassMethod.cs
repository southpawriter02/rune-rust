// ------------------------------------------------------------------------------
// <copyright file="BypassMethod.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the available methods for jury-rigging Old World technology.
// Each method represents a different cargo-cult approach to making
// incomprehensible machines do what the user wants.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the available methods for jury-rigging Old World technology.
/// Each method represents a different cargo-cult approach to making
/// incomprehensible machines do what the user wants.
/// </summary>
/// <remarks>
/// <para>
/// Methods have varying DC modifiers and associated risks:
/// <list type="bullet">
///   <item><description>Negative modifiers make bypass easier but introduce specific hazards</description></item>
///   <item><description>Positive modifiers make bypass harder but offer reliability</description></item>
///   <item><description>Zero modifiers are neutral approaches with situational effects</description></item>
/// </list>
/// </para>
/// <para>
/// The jury-rigging system represents Aethelgard's cargo-cult relationship with
/// Old World technology: inhabitants know certain actions produce results, but
/// rarely understand why. Methods range from percussive maintenance (hitting it)
/// to glitch exploitation (using the machine's own corruption against it).
/// </para>
/// </remarks>
public enum BypassMethod
{
    /// <summary>
    /// Hit the machine until it works. The classic approach.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: +0.
    /// Risk: May permanently break the mechanism on fumble.
    /// Best used: No tools available, mechanical devices.
    /// </para>
    /// </remarks>
    [Description("Strike the mechanism forcefully to jar components into alignment")]
    PercussiveMaintenance = 0,

    /// <summary>
    /// Rewire internal connections to bypass security or reroute power.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: -2 (easier).
    /// Risk: Electrocution (FINESSE save DC 12 or 2d10 lightning damage).
    /// Best used: Have proper tools and accept the shock risk.
    /// </para>
    /// </remarks>
    [Description("Manipulate internal wiring to bypass security circuits")]
    WireManipulation = 1,

    /// <summary>
    /// Exploit the machine's corrupted or malfunctioning state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: -4 (much easier).
    /// Risk: Unpredictable—chaos roll determines actual modifier.
    /// Requirement: Only effective on [Glitched] mechanisms.
    /// Best used: Mechanism is glitched, player feeling lucky.
    /// </para>
    /// </remarks>
    [Description("Exploit the mechanism's corrupted behavior patterns")]
    GlitchExploitation = 2,

    /// <summary>
    /// Apply a sequence learned from previous encounters with similar machines.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: -2 (easier).
    /// Requirement: Character must have successfully bypassed this mechanism type before.
    /// Best used: Familiar mechanism type encountered previously.
    /// </para>
    /// </remarks>
    [Description("Apply a previously memorized activation sequence")]
    MemorizedSequence = 3,

    /// <summary>
    /// Forcefully disassemble the mechanism to access internal components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: +2 (harder).
    /// Consequence: Mechanism is destroyed on success (cannot be reused).
    /// Benefit: Guaranteed component salvage on success.
    /// Best used: Don't need the mechanism intact, want salvage.
    /// </para>
    /// </remarks>
    [Description("Tear apart the mechanism to force access")]
    BruteDisassembly = 4,

    /// <summary>
    /// Cut power and restore it, hoping to reset the machine to a default state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier: +0.
    /// Effect: Resets all progress (clears lockouts, resets alert levels).
    /// Risk: May lose partial progress achieved through other methods.
    /// Best used: Need to clear lockouts or reset a stuck state.
    /// </para>
    /// </remarks>
    [Description("Cycle power to reset the mechanism to factory state")]
    PowerCycling = 5
}
