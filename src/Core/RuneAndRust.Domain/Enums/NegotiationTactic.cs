// ------------------------------------------------------------------------------
// <copyright file="NegotiationTactic.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the tactical approaches available during negotiation rounds.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The four tactical approaches available during each negotiation round.
/// </summary>
/// <remarks>
/// <para>
/// Each tactic uses a different underlying social system and has different
/// costs, risks, and side effects. Players choose their tactic based on
/// the situation, their character's strengths, and acceptable risk level.
/// </para>
/// <para>
/// Tactic selection is a strategic choice that affects not just the current
/// round, but potentially the entire negotiation. Aggressive tactics
/// (Deceive, Pressure) may achieve faster results but carry higher risks
/// and costs.
/// </para>
/// </remarks>
public enum NegotiationTactic
{
    /// <summary>
    /// Honest appeal using the Persuasion system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses WILL + Rhetoric for the skill check. No side effects on success.
    /// This is the safest tactic with predictable outcomes.
    /// </para>
    /// <para>
    /// Fumble triggers [Trust Shattered], which locks persuasion attempts
    /// with the target NPC.
    /// </para>
    /// <para>
    /// Best used when: NPC is friendly, gap is small, or player wants to
    /// maintain relationship.
    /// </para>
    /// </remarks>
    /// <example>
    /// "Consider our mutual interests in this matter."
    /// "I believe this arrangement benefits us both."
    /// </example>
    Persuade = 0,

    /// <summary>
    /// Misleading claims using the Deception system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses WILL + Rhetoric vs NPC's WITS (opposed roll). Incurs Liar's Burden
    /// stress cost regardless of outcome (+1 on success, +3 on failure).
    /// </para>
    /// <para>
    /// Fumble triggers [Lie Exposed] which immediately collapses the negotiation
    /// and damages the relationship.
    /// </para>
    /// <para>
    /// Best used when: Need an edge, NPC has low WITS, or truth is disadvantageous.
    /// Risky but can swing negotiations in player's favor.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I have other offers at twice this price."
    /// "My clan has already agreed to support your rival..."
    /// </example>
    Deceive = 1,

    /// <summary>
    /// Intimidating pressure using the Intimidation system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses MIGHT or WILL + Rhetoric (player choice). Always costs faction
    /// reputation regardless of outcome (Cost of Fear: -3 to -10 depending
    /// on result).
    /// </para>
    /// <para>
    /// Fumble triggers [Challenge Accepted] which collapses the negotiation
    /// and initiates combat with the NPC gaining [Furious] buff.
    /// </para>
    /// <para>
    /// Best used when: Player is stronger, NPC is intimidatable, or reputation
    /// cost is acceptable. Fast but damaging to relationships.
    /// </para>
    /// </remarks>
    /// <example>
    /// "You don't want to see what happens if we can't reach an agreement."
    /// "My reputation precedes me. Ask around."
    /// </example>
    Pressure = 2,

    /// <summary>
    /// Voluntary concession - give ground to gain advantage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// No check required. Automatically moves the player's position 1 step
    /// toward the opponent. Grants +2d10 bonus dice and DC reduction on
    /// the next check.
    /// </para>
    /// <para>
    /// Concession types and their DC reductions:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Offer item of value: -2 DC</description></item>
    ///   <item><description>Promise future favor: -2 DC</description></item>
    ///   <item><description>Trade information: -4 DC</description></item>
    ///   <item><description>Take personal risk: -4 DC</description></item>
    ///   <item><description>Stake faction reputation: -6 DC</description></item>
    /// </list>
    /// <para>
    /// Best used when: Gap is large, success probability is low, or need to
    /// set up a decisive follow-up move.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I'll include the Dvergr compass as a gesture of good faith."
    /// "My word carries the weight of my clan."
    /// </example>
    Concede = 3
}
