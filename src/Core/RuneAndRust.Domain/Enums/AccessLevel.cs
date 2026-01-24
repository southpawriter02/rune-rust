// ------------------------------------------------------------------------------
// <copyright file="AccessLevel.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Levels of access achieved during terminal infiltration.
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Levels of access achieved during terminal infiltration.
/// </summary>
/// <remarks>
/// <para>
/// Access levels represent the depth of control a hacker has achieved
/// over the target terminal. Higher levels grant access to more sensitive
/// functions and data.
/// </para>
/// <para>
/// Access progression:
/// <list type="bullet">
///   <item><description>None → UserLevel: Achieved by passing Layer 2</description></item>
///   <item><description>UserLevel → AdminLevel: Achieved by critical success on Layer 2, or Jötun-Reader ability</description></item>
///   <item><description>Any → Lockout: Caused by fumble at any layer</description></item>
/// </list>
/// </para>
/// </remarks>
public enum AccessLevel
{
    /// <summary>
    /// No access to the terminal.
    /// </summary>
    /// <remarks>
    /// Initial state before infiltration begins, or after being
    /// disconnected by ICE or other security measures.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Basic user-level access.
    /// </summary>
    /// <remarks>
    /// Standard access level achieved by completing Layer 2 normally.
    /// Grants access to:
    /// <list type="bullet">
    ///   <item><description>Public records and basic functions</description></item>
    ///   <item><description>Internal documents (with Layer 3 check)</description></item>
    ///   <item><description>Standard user operations</description></item>
    /// </list>
    /// Does NOT grant access to:
    /// <list type="bullet">
    ///   <item><description>Hidden files or archived data</description></item>
    ///   <item><description>Administrative functions</description></item>
    ///   <item><description>Security logs or system controls</description></item>
    /// </list>
    /// </remarks>
    UserLevel = 1,

    /// <summary>
    /// Full administrative access.
    /// </summary>
    /// <remarks>
    /// Elevated access achieved by critical success on Layer 2, or by
    /// certain specialization abilities (Jötun-Reader [Deep Access]).
    /// Grants access to:
    /// <list type="bullet">
    ///   <item><description>All user-level data and functions</description></item>
    ///   <item><description>Hidden files and archived data</description></item>
    ///   <item><description>Security logs and audit trails</description></item>
    ///   <item><description>System configuration and controls</description></item>
    /// </list>
    /// </remarks>
    AdminLevel = 2,

    /// <summary>
    /// Terminal permanently locked out.
    /// </summary>
    /// <remarks>
    /// Terminal state caused by fumble at any layer. The terminal
    /// is disabled and cannot be used again by anyone.
    /// Additionally triggers a security alert broadcast.
    /// </remarks>
    Lockout = -1
}
