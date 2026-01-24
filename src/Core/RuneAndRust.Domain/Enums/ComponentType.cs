// ------------------------------------------------------------------------------
// <copyright file="ComponentType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the type classifications for salvaged crafting components.
// Components are acquired through critical successes on bypass checks
// and used to craft improvised tools.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the type classifications for salvaged crafting components.
/// Components are acquired through critical successes on bypass checks
/// and used to craft improvised tools.
/// </summary>
/// <remarks>
/// <para>
/// Component types determine compatibility with recipes:
/// <list type="bullet">
///   <item><description>Metal: Scrap metal, clips, springs from locks and structures</description></item>
///   <item><description>Wire: Wire, cables, filaments from electronics and traps</description></item>
///   <item><description>Circuit: Circuit boards, capacitors, chips from terminals</description></item>
///   <item><description>Misc: Miscellaneous parts like handles and casings</description></item>
/// </list>
/// </para>
/// <para>
/// Some components of one type may substitute for another (e.g., Metal can
/// substitute for Misc in some recipes), representing the flexible nature
/// of improvised crafting.
/// </para>
/// </remarks>
public enum ComponentType
{
    /// <summary>
    /// Metal scraps, clips, springs, and structural components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common sources: Locks, structures, containers.
    /// Used in: Shim Picks, Bypass Clamps.
    /// Rarity: Common (Scrap Metal), Common (Metal Clips).
    /// </para>
    /// </remarks>
    [Description("Metal scraps, clips, and springs")]
    Metal = 0,

    /// <summary>
    /// Wire, cables, and conductive filaments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common sources: Terminals, traps, electronics.
    /// Used in: Wire Probe, Glitch Trigger.
    /// Rarity: Common (Wire), Common (Copper Wire).
    /// </para>
    /// </remarks>
    [Description("Wire, cables, and filaments")]
    Wire = 1,

    /// <summary>
    /// Circuit boards, capacitors, and electronic chips.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common sources: Terminals, advanced devices.
    /// Used in: Glitch Trigger.
    /// Rarity: Uncommon (Circuit Fragment), Rare (Capacitor).
    /// </para>
    /// </remarks>
    [Description("Circuit boards, capacitors, and chips")]
    Circuit = 2,

    /// <summary>
    /// Miscellaneous parts including handles, casings, and other non-specific components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common sources: Tools, devices, containers.
    /// Used in: Wire Probe.
    /// Rarity: Common (Handle).
    /// </para>
    /// </remarks>
    [Description("Miscellaneous parts and casings")]
    Misc = 3
}
