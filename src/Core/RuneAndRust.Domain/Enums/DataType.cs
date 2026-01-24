// ------------------------------------------------------------------------------
// <copyright file="DataType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Types of data that can be accessed on terminals (Layer 3).
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of data that can be accessed on terminals (Layer 3).
/// </summary>
/// <remarks>
/// <para>
/// Data type determines the DC for Layer 3 (Navigation) checks.
/// More sensitive data requires higher skill to locate and access.
/// </para>
/// <para>
/// ArchivedHidden data additionally requires AdminLevel access to locate.
/// </para>
/// </remarks>
public enum DataType
{
    /// <summary>
    /// Public records and openly available information.
    /// </summary>
    /// <remarks>DC: 10</remarks>
    PublicRecords = 10,

    /// <summary>
    /// Internal documents not meant for public access.
    /// </summary>
    /// <remarks>DC: 14</remarks>
    InternalDocuments = 14,

    /// <summary>
    /// Classified information with restricted access.
    /// </summary>
    /// <remarks>DC: 18</remarks>
    Classified = 18,

    /// <summary>
    /// Archived or hidden files requiring deep access.
    /// </summary>
    /// <remarks>DC: 22. Requires AdminLevel access to locate.</remarks>
    ArchivedHidden = 22
}
