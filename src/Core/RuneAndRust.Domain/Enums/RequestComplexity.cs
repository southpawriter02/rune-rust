// ------------------------------------------------------------------------------
// <copyright file="RequestComplexity.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Categorizes what the player is asking for in a negotiation.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes what the player is asking for in a negotiation,
/// determining the base DC and initial position gap.
/// </summary>
/// <remarks>
/// <para>
/// The complexity of the player's request determines:
/// </para>
/// <list type="bullet">
///   <item><description>Base DC for all checks during the negotiation</description></item>
///   <item><description>Initial gap between PC and NPC positions</description></item>
///   <item><description>Default number of rounds allowed</description></item>
/// </list>
/// <para>
/// More ambitious requests have higher DCs and wider starting gaps,
/// requiring more successful checks or concessions to reach agreement.
/// </para>
/// </remarks>
public enum RequestComplexity
{
    /// <summary>
    /// Equal exchange where both parties benefit equally.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC 10. Starting gap: 2. Default rounds: 3.
    /// </para>
    /// <para>
    /// The simplest negotiations where both parties gain roughly equally.
    /// Most likely to succeed with minimal effort.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I'll trade my supplies for your map."
    /// "Even split of the salvage rights."
    /// </example>
    FairTrade = 0,

    /// <summary>
    /// Player seeks a minor edge in the deal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC 14. Starting gap: 3. Default rounds: 4.
    /// </para>
    /// <para>
    /// The player wants something slightly better than an equal exchange.
    /// Achievable with good negotiating skills.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I'll take 60% of the profits."
    /// "You throw in a small bonus for my trouble."
    /// </example>
    SlightAdvantage = 1,

    /// <summary>
    /// Player seeks a clear benefit over the NPC.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC 18. Starting gap: 4. Default rounds: 5.
    /// </para>
    /// <para>
    /// A noticeably favorable deal for the player. Requires skilled
    /// negotiation and possibly concessions to achieve.
    /// </para>
    /// </remarks>
    /// <example>
    /// "Exclusive access to the northern trade route."
    /// "Your support in the upcoming council vote."
    /// </example>
    NoticeableAdvantage = 2,

    /// <summary>
    /// Player seeks a significant gain at NPC's expense.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC 22. Starting gap: 5. Default rounds: 6.
    /// </para>
    /// <para>
    /// A heavily favorable deal that puts the NPC at a clear disadvantage.
    /// Requires excellent negotiating skills, leverage, or significant
    /// concessions.
    /// </para>
    /// </remarks>
    /// <example>
    /// "Full control of the guild's contracts."
    /// "Your oath of non-interference in my territory."
    /// </example>
    MajorAdvantage = 3,

    /// <summary>
    /// Player wants everything in their favor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC 26. Starting gap: 6. Default rounds: 7.
    /// </para>
    /// <para>
    /// The player is asking for a deal that overwhelmingly favors them.
    /// Extremely difficult to achieve through negotiation alone - requires
    /// exceptional leverage, multiple concessions, or very favorable
    /// circumstances.
    /// </para>
    /// </remarks>
    /// <example>
    /// "Unconditional surrender of all assets."
    /// "Complete control with no strings attached."
    /// </example>
    OneSidedDeal = 4
}
