// ------------------------------------------------------------------------------
// <copyright file="CraftingAttribute.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines which character attribute is used for crafting checks.
// Different tools require different approaches: some need understanding (WITS),
// others need precision (FINESSE), and some can use either.
// Part of v0.15.4g Improvised Tool Crafting System implementation.
// </summary>
// ------------------------------------------------------------------------------

using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines which character attribute is used for crafting checks.
/// Different tools require different approaches: some need understanding (WITS),
/// others need precision (FINESSE), and some can use either.
/// </summary>
/// <remarks>
/// <para>
/// Crafting attribute requirements by tool:
/// <list type="bullet">
///   <item><description>Shim Picks: WITS or FINESSE (player's choice of higher)</description></item>
///   <item><description>Wire Probe: WITS or FINESSE (player's choice of higher)</description></item>
///   <item><description>Glitch Trigger: WITS (understanding circuit patterns)</description></item>
///   <item><description>Bypass Clamps: FINESSE (precise metalwork)</description></item>
/// </list>
/// </para>
/// <para>
/// The choice between attributes reflects whether the crafting process
/// requires more theoretical understanding or practical dexterity.
/// </para>
/// </remarks>
public enum CraftingAttribute
{
    /// <summary>
    /// Use WITS for crafting checks requiring understanding of complex mechanisms.
    /// </summary>
    /// <remarks>
    /// <para>
    /// WITS-only crafting represents tools that require understanding how
    /// Old World technology works, even if that understanding is incomplete.
    /// </para>
    /// </remarks>
    [Description("Use WITS attribute")]
    Wits = 0,

    /// <summary>
    /// Use FINESSE for crafting checks requiring precise manipulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// FINESSE-only crafting represents tools that require precise physical
    /// manipulation and delicate handiwork.
    /// </para>
    /// </remarks>
    [Description("Use FINESSE attribute")]
    Finesse = 1,

    /// <summary>
    /// Use the higher of WITS or FINESSE for crafting checks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// WitsOrFinesse crafting allows the character to approach the task
    /// either through understanding or through skill, whichever they excel at.
    /// </para>
    /// </remarks>
    [Description("Use higher of WITS or FINESSE")]
    WitsOrFinesse = 2
}
