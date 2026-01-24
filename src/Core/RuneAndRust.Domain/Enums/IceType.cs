// ------------------------------------------------------------------------------
// <copyright file="IceType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the three categories of ICE (Intrusion Countermeasures Electronics)
// that protect secured terminals in Aethelgard.
// Part of v0.15.4c ICE Countermeasures implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the three categories of ICE (Intrusion Countermeasures Electronics)
/// that protect secured terminals in Aethelgard.
/// </summary>
/// <remarks>
/// <para>
/// ICE types represent increasingly hostile automated defense systems that
/// characters encounter during terminal hacking attempts:
/// </para>
/// <list type="bullet">
///   <item><description>Passive ICE observes and reports intruder locations.</description></item>
///   <item><description>Active ICE directly interferes with intrusion attempts.</description></item>
///   <item><description>Lethal ICE attacks the intruder's mind through neural interfaces.</description></item>
/// </list>
/// <para>
/// Higher-security terminals may have multiple ICE types active simultaneously.
/// For example, Military Servers have both Active and Lethal ICE.
/// </para>
/// <para>
/// <b>ICE by Terminal Type:</b>
/// <list type="bullet">
///   <item><description>CivilianDataPort: No ICE</description></item>
///   <item><description>CorporateMainframe: Passive (Rating 12)</description></item>
///   <item><description>SecurityHub: Active (Rating 16)</description></item>
///   <item><description>MilitaryServer: Active + Lethal (Rating 20)</description></item>
///   <item><description>JotunArchive: Lethal (Rating 24)</description></item>
///   <item><description>GlitchedManifold: Unpredictable</description></item>
/// </list>
/// </para>
/// </remarks>
public enum IceType
{
    /// <summary>
    /// Trace ICE that attempts to reveal the hacker's physical location.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Passive ICE detects intrusion attempts and broadcasts the hacker's
    /// physical location to security systems without directly interfering
    /// with the hacking attempt.
    /// </para>
    /// <para>
    /// <b>Resolution:</b> Contested System Bypass vs. ICE Rating DC.
    /// </para>
    /// <para>
    /// <b>On Character Win:</b> Trace evaded, no consequences.
    /// </para>
    /// <para>
    /// <b>On ICE Win:</b> Location broadcast to security systems, Alert +2.
    /// </para>
    /// </remarks>
    Passive = 0,

    /// <summary>
    /// Attack ICE that attempts to force disconnect the intruder.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Active ICE aggressively attacks the hacker's connection, attempting
    /// to sever their link to the terminal and lock them out temporarily.
    /// </para>
    /// <para>
    /// <b>Resolution:</b> Contested System Bypass vs. ICE Rating DC.
    /// </para>
    /// <para>
    /// <b>On Character Win:</b> ICE disabled, +1d10 bonus on next layer check.
    /// </para>
    /// <para>
    /// <b>On ICE Win:</b> Connection severed, terminal locked out for 1 minute, Alert +1.
    /// </para>
    /// </remarks>
    Active = 1,

    /// <summary>
    /// Neural ICE that attacks the intruder's mind through their interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lethal ICE bypasses digital defenses entirely, striking directly at
    /// the hacker's mind through their neural interface. This ancient
    /// technology from the World Before can cause severe harm or death.
    /// </para>
    /// <para>
    /// <b>Resolution:</b> WILL save DC 16 (not a contested check).
    /// </para>
    /// <para>
    /// <b>On Save Success:</b> Auto-disconnect, 1d6 stress, 1 minute lockout.
    /// </para>
    /// <para>
    /// <b>On Save Failure:</b> 3d10 psychic damage, 2d6 stress, permanent lockout.
    /// </para>
    /// </remarks>
    Lethal = 2
}
