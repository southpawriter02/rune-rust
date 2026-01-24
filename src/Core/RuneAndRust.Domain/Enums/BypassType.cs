// ------------------------------------------------------------------------------
// <copyright file="BypassType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the types of bypass operations that improvised tools can assist with.
// Each bypass type corresponds to a specific v0.15.4x system bypass subsystem.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of bypass operations that improvised tools can assist with.
/// Each bypass type corresponds to a specific v0.15.4x system bypass subsystem.
/// </summary>
/// <remarks>
/// <para>
/// Bypass types map to the system bypass skill expansion phases:
/// <list type="bullet">
///   <item><description>Lockpicking: v0.15.4a Lockpicking System</description></item>
///   <item><description>TerminalHacking: v0.15.4b Terminal Hacking System</description></item>
///   <item><description>TrapDisarmament: v0.15.4d Trap Disarmament System</description></item>
///   <item><description>GlitchExploitation: v0.15.4f Glitch Exploitation System</description></item>
/// </list>
/// </para>
/// <para>
/// Improvised tools provide bonuses only when their type matches the bypass operation
/// being attempted. Using the wrong tool type provides no benefit.
/// </para>
/// </remarks>
public enum BypassType
{
    /// <summary>
    /// No specific bypass type. Used when a tool doesn't apply.
    /// </summary>
    [Description("No bypass operation")]
    None = 0,

    /// <summary>
    /// Lock manipulation using pins, tumblers, and mechanical skill.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Corresponds to v0.15.4a Lockpicking System.
    /// Assisted by: Shim Picks.
    /// </para>
    /// </remarks>
    [Description("Lock manipulation and picking")]
    Lockpicking = 1,

    /// <summary>
    /// Terminal infiltration through layered security systems.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Corresponds to v0.15.4b Terminal Hacking System.
    /// Assisted by: Wire Probe, Bypass Clamps.
    /// </para>
    /// </remarks>
    [Description("Terminal infiltration and hacking")]
    TerminalHacking = 2,

    /// <summary>
    /// Trap neutralization through careful disarmament.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Corresponds to v0.15.4d Trap Disarmament System.
    /// Note: No specific improvised tools currently assist trap disarmament.
    /// </para>
    /// </remarks>
    [Description("Trap disarmament and neutralization")]
    TrapDisarmament = 3,

    /// <summary>
    /// Exploitation of corrupted Old World technology glitches.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Corresponds to v0.15.4f Glitch Exploitation System.
    /// Assisted by: Glitch Trigger.
    /// </para>
    /// </remarks>
    [Description("Glitched technology exploitation")]
    GlitchExploitation = 4
}
