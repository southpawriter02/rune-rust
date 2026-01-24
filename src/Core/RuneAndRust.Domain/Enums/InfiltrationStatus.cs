// ------------------------------------------------------------------------------
// <copyright file="InfiltrationStatus.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Overall status of a terminal infiltration attempt.
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Overall status of a terminal infiltration attempt.
/// </summary>
public enum InfiltrationStatus
{
    /// <summary>
    /// Infiltration is actively in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Temporarily locked out from Layer 1 failure.
    /// </summary>
    /// <remarks>
    /// Can retry after 1 minute at DC +2.
    /// </remarks>
    TemporaryLockout,

    /// <summary>
    /// Alert was triggered by Layer 2 failure.
    /// </summary>
    /// <remarks>
    /// ICE may activate, security may respond.
    /// </remarks>
    AlertTriggered,

    /// <summary>
    /// Infiltration completed (success or partial).
    /// </summary>
    Completed,

    /// <summary>
    /// Terminal permanently locked out due to fumble.
    /// </summary>
    LockedOut,

    /// <summary>
    /// Disconnected from terminal (voluntary or forced).
    /// </summary>
    Disconnected
}
