// ------------------------------------------------------------------------------
// <copyright file="PersuasionRequest.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Categorizes persuasion requests by complexity, determining base DC.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes persuasion requests by complexity, determining base DC.
/// </summary>
/// <remarks>
/// <para>
/// Request complexity is determined by the personal cost or risk to the NPC.
/// Higher complexity requests require more successes and typically need
/// higher disposition levels to have reasonable chances of success.
/// </para>
/// <para>
/// The five tiers map to difficulty classes:
/// <list type="bullet">
///   <item><description>Trivial: DC 8 (1-2 successes)</description></item>
///   <item><description>Simple: DC 12 (2 successes)</description></item>
///   <item><description>Moderate: DC 16 (3 successes)</description></item>
///   <item><description>Major: DC 20 (4 successes)</description></item>
///   <item><description>Extreme: DC 24 (4+ successes)</description></item>
/// </list>
/// </para>
/// <para>
/// Some requests may be impossible regardless of roll if the NPC's disposition
/// is too low or if the request contradicts the NPC's core values.
/// </para>
/// </remarks>
public enum PersuasionRequest
{
    /// <summary>
    /// Minimal cost or effort for the NPC. Almost always granted.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 8 (1-2 successes needed)</para>
    /// <para>Examples: "Tell me the time", "Point me to the tavern", "Hold this for a moment"</para>
    /// <para>Even Unfriendly NPCs may comply with trivial requests.</para>
    /// </remarks>
    Trivial = 0,

    /// <summary>
    /// Small favor or minor inconvenience to the NPC.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 12 (2 successes needed)</para>
    /// <para>Examples: "Share your rations", "Give me a small discount", "Introduce me to your friend"</para>
    /// <para>Typically requires Neutral or better disposition.</para>
    /// </remarks>
    Simple = 1,

    /// <summary>
    /// Meaningful assistance with some personal cost to the NPC.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 16 (3 successes needed)</para>
    /// <para>Examples: "Help me fight", "Lend me your weapon", "Share sensitive information"</para>
    /// <para>Typically requires NeutralPositive or better disposition.</para>
    /// </remarks>
    Moderate = 2,

    /// <summary>
    /// Significant personal risk or potential faction conflict for the NPC.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 20 (4 successes needed)</para>
    /// <para>Examples: "Attack faction enemies", "Give me your life savings", "Reveal faction secrets"</para>
    /// <para>Typically requires Friendly or better disposition.</para>
    /// </remarks>
    Major = 3,

    /// <summary>
    /// Life-altering decision, ultimate betrayal, or sacrifice.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 24 (4+ successes needed)</para>
    /// <para>Examples: "Betray your clan leader", "Sacrifice yourself", "Kill your closest ally"</para>
    /// <para>Typically requires Ally disposition; many NPCs will refuse outright.</para>
    /// <para>Some requests may be flagged as impossible by the game regardless of roll.</para>
    /// </remarks>
    Extreme = 4
}
