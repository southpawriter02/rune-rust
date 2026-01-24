// ------------------------------------------------------------------------------
// <copyright file="ImprovisedToolType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the types of improvised tools that can be crafted from salvaged components.
// Each tool type aids a specific bypass operation, reflecting Aethelgard's
// cargo-cult approach to Old World technology.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of improvised tools that can be crafted from salvaged components.
/// Each tool type aids a specific bypass operation, reflecting Aethelgard's
/// cargo-cult approach to Old World technology.
/// </summary>
/// <remarks>
/// <para>
/// Improvised tools are crafted from salvaged components and provide bonuses
/// to specific bypass operations:
/// <list type="bullet">
///   <item><description>Standard tools provide +1d10 bonus and have 3 uses</description></item>
///   <item><description>Quality tools (from critical crafting) provide +2d10 bonus and have 5 uses</description></item>
///   <item><description>Some tools have special effects beyond dice bonuses</description></item>
/// </list>
/// </para>
/// <para>
/// These tools represent the practical ingenuity of Aethelgard's survivors,
/// who fashion crude but functional equipment from the remains of
/// incomprehensible Old World machinery.
/// </para>
/// </remarks>
public enum ImprovisedToolType
{
    /// <summary>
    /// Thin metal shims that slip between lock pins for manipulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bonus: +1d10 (standard) or +2d10 (quality) to lockpicking attempts.
    /// Crafted from: 3× Scrap Metal.
    /// Craft DC: 10 (WITS or FINESSE).
    /// </para>
    /// </remarks>
    [Description("Metal shims for manipulating lock pins")]
    ShimPicks = 0,

    /// <summary>
    /// A wrapped probe of copper wire for manipulating terminal circuits.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bonus: +1d10 (standard) or +2d10 (quality) to terminal hacking attempts.
    /// Crafted from: 2× Copper Wire, 1× Handle.
    /// Craft DC: 12 (WITS or FINESSE).
    /// </para>
    /// </remarks>
    [Description("Copper wire probe for terminal circuit manipulation")]
    WireProbe = 1,

    /// <summary>
    /// A crude device that induces glitched states in Old World machinery.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bonus: +1d10 (standard) or +2d10 (quality) to glitch exploitation attempts.
    /// Special Effect: Can force a mechanism into [Glitched] state.
    /// Crafted from: 1× Capacitor, 4× Wire.
    /// Craft DC: 14 (WITS).
    /// </para>
    /// </remarks>
    [Description("Device that forces mechanisms into glitched state")]
    GlitchTrigger = 2,

    /// <summary>
    /// Spring-loaded metal clips that bypass terminal access layers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bonus: +1d10 (standard) or +2d10 (quality) to terminal hacking attempts.
    /// Special Effect: Skips Layer 1 (Access) of terminal infiltration.
    /// Crafted from: 4× Metal Clips.
    /// Craft DC: 12 (FINESSE).
    /// </para>
    /// </remarks>
    [Description("Clips that bypass outer terminal access layer")]
    BypassClamps = 3
}
