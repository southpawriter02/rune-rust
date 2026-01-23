// ------------------------------------------------------------------------------
// <copyright file="ConcessionType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the types of voluntary concessions that can be made during negotiation.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of voluntary concessions that can be made during negotiation.
/// </summary>
/// <remarks>
/// <para>
/// Concessions are voluntary sacrifices made to gain advantage in subsequent
/// negotiation checks. All concessions grant +2d10 bonus dice on the next
/// check, plus a DC reduction that varies by concession type.
/// </para>
/// <para>
/// DC reduction values:
/// </para>
/// <list type="bullet">
///   <item><description>OfferItem: -2 DC (item consumed)</description></item>
///   <item><description>PromiseFavor: -2 DC (creates debt)</description></item>
///   <item><description>TradeInformation: -4 DC (reveals secrets)</description></item>
///   <item><description>TakeRisk: -4 DC (accepts personal danger)</description></item>
///   <item><description>StakeReputation: -6 DC (faction rep on the line)</description></item>
/// </list>
/// </remarks>
public enum ConcessionType
{
    /// <summary>
    /// Offering a valuable item as part of the deal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC reduction: -2. The item is consumed and transferred to the NPC.
    /// </para>
    /// <para>
    /// Most straightforward concession - clear value exchange. Best used
    /// when the player has items they can spare.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I'll include this Dvergr compass as a sign of good faith."
    /// "Take these medical supplies as a gesture of goodwill."
    /// </example>
    OfferItem = 0,

    /// <summary>
    /// Promising a future favor to the NPC.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC reduction: -2. Creates a debt that must be repaid later.
    /// </para>
    /// <para>
    /// Less immediate cost than offering an item, but creates a future
    /// obligation. The NPC may call in the favor at an inconvenient time.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I'll owe you a favor - no questions asked."
    /// "When you need something done, call on me."
    /// </example>
    PromiseFavor = 1,

    /// <summary>
    /// Trading valuable information to the NPC.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC reduction: -4. The information is now known to the NPC.
    /// </para>
    /// <para>
    /// Powerful concession but information cannot be unshared. May have
    /// consequences if the NPC uses the information against the player's
    /// interests later.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I'll tell you where the Combine keeps their supply cache."
    /// "The guild master has a secret weakness..."
    /// </example>
    TradeInformation = 2,

    /// <summary>
    /// Taking on personal risk as part of the agreement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC reduction: -4. The player accepts danger or disadvantage.
    /// </para>
    /// <para>
    /// Shows commitment and builds trust by accepting vulnerability.
    /// The risk may or may not materialize, but it signals serious intent.
    /// </para>
    /// </remarks>
    /// <example>
    /// "I'll go first into the dangerous section."
    /// "I'll accept liability if this goes wrong."
    /// </example>
    TakeRisk = 3,

    /// <summary>
    /// Putting faction reputation on the line.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC reduction: -6. Failure will significantly damage faction reputation.
    /// </para>
    /// <para>
    /// The most powerful concession type. By staking reputation, the player
    /// demonstrates ultimate commitment. However, if the deal falls through
    /// or the player fails to uphold their end, reputation damage is severe.
    /// </para>
    /// </remarks>
    /// <example>
    /// "My clan's honor guarantees this agreement."
    /// "The Scavenger Guild stands behind my word."
    /// </example>
    StakeReputation = 4
}
