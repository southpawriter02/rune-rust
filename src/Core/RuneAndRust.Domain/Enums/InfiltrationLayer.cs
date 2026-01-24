// ------------------------------------------------------------------------------
// <copyright file="InfiltrationLayer.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Layers of terminal infiltration representing progressive security barriers.
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Layers of terminal infiltration representing progressive security barriers.
/// </summary>
/// <remarks>
/// <para>
/// Terminal hacking proceeds through three sequential layers, each with
/// distinct challenges and consequences:
/// <list type="bullet">
///   <item><description>Layer 1 (Access): Bypass initial security and establish connection</description></item>
///   <item><description>Layer 2 (Authentication): Verify identity or spoof credentials</description></item>
///   <item><description>Layer 3 (Navigation): Locate and access desired data</description></item>
/// </list>
/// </para>
/// <para>
/// Failure at any layer has escalating consequences, from temporary lockouts
/// to permanent terminal disabling. Success at each layer grants progressively
/// greater access.
/// </para>
/// </remarks>
public enum InfiltrationLayer
{
    /// <summary>
    /// Initial connection and security bypass.
    /// </summary>
    /// <remarks>
    /// The first barrier—establishing a connection to the terminal's
    /// internal systems. Requires bypassing firewall equivalents and
    /// connection protocols.
    /// DC: Based on terminal type.
    /// Time: 1-5 rounds based on encryption level.
    /// Failure: 1 minute lockout, retry at DC +2.
    /// </remarks>
    Layer1_Access = 1,

    /// <summary>
    /// Identity verification bypass.
    /// </summary>
    /// <remarks>
    /// The second barrier—convincing the system you are authorized.
    /// May involve password cracking, biometric spoofing, or
    /// credential forgery.
    /// DC: Layer 1 DC + security modifier (0 to +6).
    /// Failure: Alert triggers, ICE may activate.
    /// </remarks>
    Layer2_Authentication = 2,

    /// <summary>
    /// Data location and access.
    /// </summary>
    /// <remarks>
    /// The final barrier—finding the specific data you need within
    /// the system's architecture. More sensitive data requires
    /// deeper navigation.
    /// DC: Based on data type (10-22).
    /// Failure: Partial access only.
    /// </remarks>
    Layer3_Navigation = 3
}
