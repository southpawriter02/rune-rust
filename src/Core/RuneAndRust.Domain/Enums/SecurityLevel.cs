// ------------------------------------------------------------------------------
// <copyright file="SecurityLevel.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Security levels for terminal authentication (Layer 2).
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Security levels for terminal authentication (Layer 2).
/// </summary>
/// <remarks>
/// <para>
/// Security level determines the DC modifier applied to Layer 2 (Authentication)
/// checks. Higher security levels represent more sophisticated authentication
/// systems from the World Before.
/// </para>
/// </remarks>
public enum SecurityLevel
{
    /// <summary>
    /// Simple password authentication.
    /// </summary>
    /// <remarks>DC modifier: +0</remarks>
    PasswordOnly = 0,

    /// <summary>
    /// Biometric verification (fingerprint, retinal).
    /// </summary>
    /// <remarks>DC modifier: +2</remarks>
    Biometric = 2,

    /// <summary>
    /// Multi-factor authentication.
    /// </summary>
    /// <remarks>DC modifier: +4</remarks>
    MultiFactor = 4,

    /// <summary>
    /// Ancient Jötun authentication protocols.
    /// </summary>
    /// <remarks>DC modifier: +6</remarks>
    JotunLocked = 6
}
