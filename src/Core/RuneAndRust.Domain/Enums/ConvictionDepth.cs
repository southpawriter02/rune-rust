// ------------------------------------------------------------------------------
// <copyright file="ConvictionDepth.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Indicates how deeply an NPC was convinced by a persuasion attempt.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Indicates how deeply an NPC was convinced by a persuasion attempt.
/// </summary>
/// <remarks>
/// <para>
/// Conviction depth affects how willingly the NPC complies, whether they
/// volunteer additional help, and how long the persuasion effect lasts.
/// </para>
/// <para>
/// Deeper conviction may unlock additional dialogue options and make
/// future requests with the same NPC easier.
/// </para>
/// </remarks>
public enum ConvictionDepth
{
    /// <summary>
    /// NPC was not convinced at all. Request denied.
    /// </summary>
    None = 0,

    /// <summary>
    /// Barely convinced. NPC agrees with reservations.
    /// </summary>
    /// <remarks>
    /// Achieved on marginal success. May impose conditions or partial compliance.
    /// </remarks>
    Shallow = 1,

    /// <summary>
    /// Willingly agrees. Standard compliance with the request.
    /// </summary>
    /// <remarks>
    /// Achieved on full success. NPC complies without reservation.
    /// </remarks>
    Moderate = 2,

    /// <summary>
    /// Genuinely convinced. Enthusiastic compliance.
    /// </summary>
    /// <remarks>
    /// Achieved on exceptional success. May unlock related dialogue options.
    /// </remarks>
    Strong = 3,

    /// <summary>
    /// Deeply won over. May volunteer additional help.
    /// </summary>
    /// <remarks>
    /// Achieved on critical success. NPC becomes a strong supporter.
    /// Multiple new dialogue options unlock. Future requests are easier (DC -2).
    /// </remarks>
    Deep = 4
}
