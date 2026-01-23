// ------------------------------------------------------------------------------
// <copyright file="NegotiationStatus.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the phases of a multi-round negotiation.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current phase of a multi-round negotiation.
/// </summary>
/// <remarks>
/// <para>
/// Negotiations progress through phases based on position gap and round outcomes.
/// The system tracks both PC and NPC positions on a 0-8 scale, with deal reached
/// when positions overlap or both reach Compromise (5).
/// </para>
/// <para>
/// Phase transitions are triggered by:
/// </para>
/// <list type="bullet">
///   <item><description>Opening → Bargaining: After first round</description></item>
///   <item><description>Bargaining → CrisisManagement: Gap ≥ 5 or 2+ consecutive failures</description></item>
///   <item><description>Any → Finalization: Gap ≤ 1</description></item>
///   <item><description>Any → DealReached: Gap = 0 (positions overlap)</description></item>
///   <item><description>Any → Collapsed: Position reaches 8 or fumble during Crisis</description></item>
/// </list>
/// </remarks>
public enum NegotiationStatus
{
    /// <summary>
    /// Initial phase where positions are established and stakes clarified.
    /// </summary>
    /// <remarks>
    /// <para>
    /// During the Opening phase, both parties assess each other and establish
    /// their initial negotiating positions. The gap between positions determines
    /// the difficulty of reaching a deal.
    /// </para>
    /// <para>
    /// Transitions to Bargaining after the first round.
    /// </para>
    /// </remarks>
    Opening = 0,

    /// <summary>
    /// Main negotiation phase where tactics are used and positions shift.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The bulk of negotiation happens in this phase. Each round, players choose
    /// a tactic (Persuade, Deceive, Pressure, or Concede) and make skill checks.
    /// Success moves the opponent's position toward compromise; failure moves
    /// the player's position toward the opponent.
    /// </para>
    /// <para>
    /// This phase continues until positions overlap, crisis occurs, or rounds
    /// are exhausted.
    /// </para>
    /// </remarks>
    Bargaining = 1,

    /// <summary>
    /// Negotiation is at risk of collapse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered when the gap widens to 5+ or after 2+ consecutive failures.
    /// The negotiation is on the verge of breaking down and requires careful
    /// tactical choices to recover.
    /// </para>
    /// <para>
    /// A fumble during CrisisManagement causes immediate collapse.
    /// Recovery is possible through successful checks or strategic concessions.
    /// </para>
    /// </remarks>
    CrisisManagement = 2,

    /// <summary>
    /// Positions are overlapping or very close (gap ≤ 1).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Both parties are close to agreement. Final terms are being negotiated.
    /// One more successful check typically seals the deal.
    /// </para>
    /// <para>
    /// This phase indicates imminent success unless a failure pushes
    /// positions apart again.
    /// </para>
    /// </remarks>
    Finalization = 3,

    /// <summary>
    /// Negotiation succeeded. Deal has been reached and terms finalized.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state. The final deal terms favor whoever has the lower
    /// position number. Both parties have agreed to the terms and the
    /// negotiation is complete.
    /// </para>
    /// <para>
    /// Successful negotiations typically result in unlocked dialogue options,
    /// positive disposition changes, and achievement of the player's goals.
    /// </para>
    /// </remarks>
    DealReached = 4,

    /// <summary>
    /// Negotiation failed completely. No deal is possible.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state with negative consequences. This occurs when:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>A party reaches Walk Away (position 8)</description></item>
    ///   <item><description>A fumble occurs during CrisisManagement</description></item>
    ///   <item><description>All rounds are exhausted without agreement</description></item>
    /// </list>
    /// <para>
    /// Collapsed negotiations damage relationships and may lock future
    /// negotiation attempts with the NPC.
    /// </para>
    /// </remarks>
    Collapsed = 5
}
