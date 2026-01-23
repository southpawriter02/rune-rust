// ------------------------------------------------------------------------------
// <copyright file="IntimidationApproach.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the two approaches to intimidation: Physical (MIGHT) and Mental (WILL).
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the approach used for intimidation, determining
/// which attribute (MIGHT or WILL) is used for the check.
/// Players choose their approach based on character strengths
/// and narrative preference.
/// </summary>
/// <remarks>
/// <para>
/// Unlike other Rhetoric subsystems that use a fixed attribute,
/// intimidation allows the player to choose between two approaches:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Physical (MIGHT)</b>: Uses physical presence, size, and
///       the implied threat of violence. Effective for characters
///       with high MIGHT attribute.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Mental (WILL)</b>: Uses psychological pressure, reputation,
///       and force of personality. Effective for characters with
///       high WILL attribute.
///     </description>
///   </item>
/// </list>
/// <para>
/// Both approaches use the Rhetoric skill in addition to the chosen attribute.
/// The dice pool is: [Chosen Attribute] + Rhetoric + Modifiers.
/// </para>
/// </remarks>
public enum IntimidationApproach
{
    /// <summary>
    /// Physical intimidation using the MIGHT attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Involves overt physical threat and displays of strength:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Looming over the target</description></item>
    ///   <item><description>Drawing or displaying weapons</description></item>
    ///   <item><description>Flexing muscles, cracking knuckles</description></item>
    ///   <item><description>Physical blocking of escape routes</description></item>
    ///   <item><description>Grabbing collar or slamming table</description></item>
    /// </list>
    /// <para>
    /// Best suited for characters with high MIGHT who prefer direct,
    /// unsubtle approaches to coercion.
    /// </para>
    /// </remarks>
    Physical = 0,

    /// <summary>
    /// Mental intimidation using the WILL attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Involves psychological pressure and implied consequences:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Cold, unflinching stare</description></item>
    ///   <item><description>Calm threats with implied doom</description></item>
    ///   <item><description>Referencing reputation or past deeds</description></item>
    ///   <item><description>Quiet certainty of violence to come</description></item>
    ///   <item><description>Mentioning what happened to the last one</description></item>
    /// </list>
    /// <para>
    /// Best suited for characters with high WILL who prefer subtle,
    /// psychological approaches to coercion.
    /// </para>
    /// </remarks>
    Mental = 1
}
