// ------------------------------------------------------------------------------
// <copyright file="ToolQuality.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Quality levels for lockpicking and bypass tools with associated dice modifiers.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Quality levels for lockpicking and bypass tools.
/// </summary>
/// <remarks>
/// <para>
/// Tool quality directly affects the dice pool for lockpicking attempts:
/// <list type="bullet">
///   <item><description>BareHands: -2d10 (only viable for DC &lt; 10)</description></item>
///   <item><description>Improvised: +0 (bent wire, hairpin)</description></item>
///   <item><description>Proper: +1d10 (Tinker's Toolkit, quality picks)</description></item>
///   <item><description>Masterwork: +2d10 (Dvergr-crafted tools)</description></item>
/// </list>
/// </para>
/// <para>
/// Locks with DC 10+ (SimpleLock and above) require at least Improvised tools.
/// DC 14+ (StandardLock and above) effectively require Proper tools due to the
/// increased difficulty.
/// </para>
/// </remarks>
public enum ToolQuality
{
    /// <summary>
    /// No tools—attempting with fingers only.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modifier: -2d10.
    /// Only viable for ImprovisedLatch (DC 6) with sufficient skill.
    /// Cannot attempt locks with DC 10+ (SimpleLock and above).
    /// </para>
    /// </remarks>
    BareHands = 0,

    /// <summary>
    /// Improvised tools such as bent wire, hairpins, or scrap metal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modifier: +0.
    /// Sufficient for basic attempts but provides no bonus.
    /// Can break on fumble, requiring replacement.
    /// May be crafted from scrap metal components.
    /// </para>
    /// </remarks>
    Improvised = 1,

    /// <summary>
    /// Proper lockpicking tools from a [Tinker's Toolkit].
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modifier: +1d10.
    /// Standard equipment for System Bypass specialists.
    /// Required for most serious lockpicking attempts.
    /// Purchased from merchants or found in ruins.
    /// </para>
    /// </remarks>
    Proper = 2,

    /// <summary>
    /// Masterwork tools of exceptional quality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modifier: +2d10.
    /// Rare and valuable. May be crafted by Scrap-Tinkers with
    /// the [Master Craftsman] ability and rare components.
    /// </para>
    /// </remarks>
    Masterwork = 3
}
