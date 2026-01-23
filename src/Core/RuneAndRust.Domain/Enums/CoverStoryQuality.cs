// ------------------------------------------------------------------------------
// <copyright file="CoverStoryQuality.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Evaluates the quality of a prepared cover story for deception attempts.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Evaluates the quality of a prepared cover story for deception attempts.
/// </summary>
/// <remarks>
/// <para>
/// Cover story quality is determined by how much preparation the player has
/// invested in creating a believable alternate identity or explanation.
/// Higher quality cover stories reduce the DC of deception attempts.
/// </para>
/// <para>
/// Cover stories are typically established through roleplay, research,
/// or specific actions before the deception attempt.
/// </para>
/// </remarks>
public enum CoverStoryQuality
{
    /// <summary>
    /// No prepared cover story; improvised lie.
    /// </summary>
    /// <remarks>
    /// <para>DC Modifier: +0</para>
    /// <para>The player is making up the lie on the spot.</para>
    /// </remarks>
    None = 0,

    /// <summary>
    /// Basic cover story with few details.
    /// </summary>
    /// <remarks>
    /// <para>DC Modifier: -1</para>
    /// <para>Player has a general story but lacks specific details.</para>
    /// <para>Example: "I'm a merchant" but can't name specific goods or routes.</para>
    /// </remarks>
    Basic = 1,

    /// <summary>
    /// Good cover story with consistent details.
    /// </summary>
    /// <remarks>
    /// <para>DC Modifier: -2</para>
    /// <para>Player has memorized names, places, and a coherent backstory.</para>
    /// <para>Example: "I'm a merchant from Ironhold, I trade in salvage parts."</para>
    /// </remarks>
    Good = 2,

    /// <summary>
    /// Excellent cover story with researched, verifiable elements.
    /// </summary>
    /// <remarks>
    /// <para>DC Modifier: -3</para>
    /// <para>Player has done research and includes verifiable details.</para>
    /// <para>Example: Knows the merchant guild master's name, recent trade routes,
    /// and can discuss current market conditions accurately.</para>
    /// </remarks>
    Excellent = 3,

    /// <summary>
    /// Masterwork cover story with supporting proof.
    /// </summary>
    /// <remarks>
    /// <para>DC Modifier: -4</para>
    /// <para>Player has an airtight cover story with physical evidence.</para>
    /// <para>Typically requires forged documents, planted witnesses, or
    /// other elaborate preparation.</para>
    /// <para>Example: Has forged guild credentials, knows secret handshakes,
    /// has a reference from a real guild member (bribed or coerced).</para>
    /// </remarks>
    Masterwork = 4
}
