// ------------------------------------------------------------------------------
// <copyright file="LieComplexity.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Categorizes lies by plausibility, determining base DC for deception attempts.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes lies by plausibility, determining base DC for deception attempts.
/// </summary>
/// <remarks>
/// <para>
/// Lie complexity is determined by how believable the falsehood is given the
/// context and the NPC's knowledge. Higher complexity lies are more difficult
/// to sell and more dangerous when exposed.
/// </para>
/// <para>
/// Unlike persuasion, deception uses an opposed roll system:
/// Player (WILL + Rhetoric) vs. NPC (WITS). The DC modifies the effective
/// threshold for determining success.
/// </para>
/// <para>
/// The four tiers map to difficulty classes:
/// <list type="bullet">
///   <item><description>WhiteLie: DC 10 (minor falsehoods)</description></item>
///   <item><description>Plausible: DC 14 (believable given context)</description></item>
///   <item><description>Unlikely: DC 18 (stretches credibility)</description></item>
///   <item><description>Outrageous: DC 22 (nearly unbelievable)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum LieComplexity
{
    /// <summary>
    /// Minor falsehoods that are easily believed and low stakes.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 10</para>
    /// <para>Examples: "I was here the whole time", "I didn't see anything"</para>
    /// <para>Even slightly suspicious NPCs may let these pass.</para>
    /// <para>Detection reaction: Mild annoyance, slight suspicion.</para>
    /// </remarks>
    WhiteLie = 0,

    /// <summary>
    /// Believable lies that could reasonably be true given context.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 14</para>
    /// <para>Examples: "I'm with the Combine", "The captain sent me"</para>
    /// <para>Requires consistent delivery and may benefit from supporting evidence.</para>
    /// <para>Detection reaction: Suspicion, possible refusal to interact.</para>
    /// </remarks>
    Plausible = 1,

    /// <summary>
    /// Lies that stretch credibility and typically require supporting details.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 18</para>
    /// <para>Examples: "The Warden sent me", "I'm a high-ranking official"</para>
    /// <para>Usually requires forged documents or exceptional circumstances to succeed.</para>
    /// <para>Detection reaction: Alarm, possible hostility, may report to authorities.</para>
    /// </remarks>
    Unlikely = 2,

    /// <summary>
    /// Nearly unbelievable claims requiring extreme audacity.
    /// </summary>
    /// <remarks>
    /// <para>Base DC: 22</para>
    /// <para>Examples: "I am the Warden", "I single-handedly defeated the warband"</para>
    /// <para>Rarely succeeds without [Trusting] NPC status or masterwork cover story.</para>
    /// <para>Exposure is almost always catastrophic with immediate hostility.</para>
    /// </remarks>
    Outrageous = 3
}
