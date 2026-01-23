// ------------------------------------------------------------------------------
// <copyright file="ConcessionTypeExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the ConcessionType enum providing DC reductions,
// bonus dice, display names, and descriptive information.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="ConcessionType"/> enum providing
/// DC reductions, bonus dice, display names, and descriptive utilities.
/// </summary>
public static class ConcessionTypeExtensions
{
    /// <summary>
    /// Gets the DC reduction granted by this concession type.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>DC reduction value (negative, reduces check difficulty).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown concession type is provided.
    /// </exception>
    /// <remarks>
    /// DC reductions scale with the sacrifice involved:
    /// <list type="bullet">
    ///   <item><description>OfferItem: -2 DC (item consumed)</description></item>
    ///   <item><description>PromiseFavor: -2 DC (creates debt)</description></item>
    ///   <item><description>TradeInformation: -4 DC (reveals secrets)</description></item>
    ///   <item><description>TakeRisk: -4 DC (accepts personal danger)</description></item>
    ///   <item><description>StakeReputation: -6 DC (faction rep on the line)</description></item>
    /// </list>
    /// </remarks>
    public static int GetDcReduction(this ConcessionType type)
    {
        return type switch
        {
            ConcessionType.OfferItem => 2,
            ConcessionType.PromiseFavor => 2,
            ConcessionType.TradeInformation => 4,
            ConcessionType.TakeRisk => 4,
            ConcessionType.StakeReputation => 6,
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }

    /// <summary>
    /// Gets the bonus dice granted by this concession type.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>Number of bonus d10 dice added to the next check.</returns>
    /// <remarks>
    /// All concession types grant +2d10 bonus dice on the next check,
    /// regardless of the DC reduction amount.
    /// </remarks>
    public static int GetBonusDice(this ConcessionType type)
    {
        // All concession types grant the same bonus dice
        return type switch
        {
            ConcessionType.OfferItem => 2,
            ConcessionType.PromiseFavor => 2,
            ConcessionType.TradeInformation => 2,
            ConcessionType.TakeRisk => 2,
            ConcessionType.StakeReputation => 2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>Human-readable concession type name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown concession type is provided.
    /// </exception>
    public static string GetDisplayName(this ConcessionType type)
    {
        return type switch
        {
            ConcessionType.OfferItem => "Offer Item",
            ConcessionType.PromiseFavor => "Promise Favor",
            ConcessionType.TradeInformation => "Trade Information",
            ConcessionType.TakeRisk => "Take Risk",
            ConcessionType.StakeReputation => "Stake Reputation",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }

    /// <summary>
    /// Gets a description of what this concession type involves.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>Descriptive text explaining the concession.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown concession type is provided.
    /// </exception>
    public static string GetDescription(this ConcessionType type)
    {
        return type switch
        {
            ConcessionType.OfferItem =>
                "Offer a valuable item as part of the deal. The item is consumed and transferred to the NPC.",
            ConcessionType.PromiseFavor =>
                "Promise a future favor to the NPC. Creates a debt that must be repaid later.",
            ConcessionType.TradeInformation =>
                "Trade valuable information to the NPC. Information cannot be unshared once revealed.",
            ConcessionType.TakeRisk =>
                "Take on personal risk as part of the agreement. Shows commitment by accepting vulnerability.",
            ConcessionType.StakeReputation =>
                "Put faction reputation on the line. The most powerful concession, but failure causes severe reputation damage.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }

    /// <summary>
    /// Gets a short description suitable for tooltips or compact displays.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>Brief description of the concession.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown concession type is provided.
    /// </exception>
    public static string GetShortDescription(this ConcessionType type)
    {
        return type switch
        {
            ConcessionType.OfferItem => "Give an item of value",
            ConcessionType.PromiseFavor => "Owe them a favor",
            ConcessionType.TradeInformation => "Reveal a secret",
            ConcessionType.TakeRisk => "Accept personal danger",
            ConcessionType.StakeReputation => "Risk faction standing",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }

    /// <summary>
    /// Gets the cost or consequence of making this concession.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>Description of what the player loses or risks.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown concession type is provided.
    /// </exception>
    public static string GetCost(this ConcessionType type)
    {
        return type switch
        {
            ConcessionType.OfferItem => "Item is consumed and transferred",
            ConcessionType.PromiseFavor => "Creates a debt that must be repaid",
            ConcessionType.TradeInformation => "Information is now known to the NPC",
            ConcessionType.TakeRisk => "You accept personal danger or disadvantage",
            ConcessionType.StakeReputation => "Failure damages faction reputation significantly",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }

    /// <summary>
    /// Gets example concessions of this type for display.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>Array of example concession descriptions.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown concession type is provided.
    /// </exception>
    public static IReadOnlyList<string> GetExamples(this ConcessionType type)
    {
        return type switch
        {
            ConcessionType.OfferItem => new[]
            {
                "\"I'll include this Dvergr compass as a sign of good faith.\"",
                "\"Take these medical supplies as a gesture of goodwill.\"",
                "\"This salvage toolkit should sweeten the deal.\""
            },
            ConcessionType.PromiseFavor => new[]
            {
                "\"I'll owe you a favor - no questions asked.\"",
                "\"When you need something done, call on me.\"",
                "\"Consider me in your debt.\""
            },
            ConcessionType.TradeInformation => new[]
            {
                "\"I'll tell you where the Combine keeps their supply cache.\"",
                "\"The guild master has a weakness you should know about...\"",
                "\"I know who really controls the shipping routes.\""
            },
            ConcessionType.TakeRisk => new[]
            {
                "\"I'll go first into the dangerous section.\"",
                "\"I'll accept liability if this goes wrong.\"",
                "\"Put my life on the line to prove I'm serious.\""
            },
            ConcessionType.StakeReputation => new[]
            {
                "\"My clan's honor guarantees this agreement.\"",
                "\"The Scavenger Guild stands behind my word.\"",
                "\"My faction vouches for this deal.\""
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }

    /// <summary>
    /// Gets a formatted summary of the concession's mechanical benefits.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>Formatted string showing DC reduction and bonus dice.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown concession type is provided.
    /// </exception>
    public static string GetMechanicalSummary(this ConcessionType type)
    {
        var dcReduction = type.GetDcReduction();
        var bonusDice = type.GetBonusDice();
        return $"-{dcReduction} DC, +{bonusDice}d10";
    }

    /// <summary>
    /// Determines if this concession type consumes an item.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>True if an item is consumed; otherwise, false.</returns>
    public static bool ConsumesItem(this ConcessionType type)
    {
        return type == ConcessionType.OfferItem;
    }

    /// <summary>
    /// Determines if this concession type creates a debt obligation.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>True if a debt is created; otherwise, false.</returns>
    public static bool CreatesDebt(this ConcessionType type)
    {
        return type == ConcessionType.PromiseFavor;
    }

    /// <summary>
    /// Determines if this concession type reveals information.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>True if information is revealed; otherwise, false.</returns>
    public static bool RevealsInformation(this ConcessionType type)
    {
        return type == ConcessionType.TradeInformation;
    }

    /// <summary>
    /// Determines if this concession type risks faction reputation.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>True if reputation is at stake; otherwise, false.</returns>
    public static bool RisksReputation(this ConcessionType type)
    {
        return type == ConcessionType.StakeReputation;
    }

    /// <summary>
    /// Gets the risk level associated with this concession type.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>Risk level from "Low" to "Very High".</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown concession type is provided.
    /// </exception>
    /// <remarks>
    /// Risk levels correspond to the potential negative consequences:
    /// <list type="bullet">
    ///   <item><description>Low: OfferItem (only lose the item)</description></item>
    ///   <item><description>Medium: PromiseFavor (future obligation)</description></item>
    ///   <item><description>High: TradeInformation, TakeRisk (lasting consequences)</description></item>
    ///   <item><description>Very High: StakeReputation (faction-wide impact)</description></item>
    /// </list>
    /// </remarks>
    public static string GetRiskLevel(this ConcessionType type)
    {
        return type switch
        {
            ConcessionType.OfferItem => "Low",
            ConcessionType.PromiseFavor => "Medium",
            ConcessionType.TradeInformation => "High",
            ConcessionType.TakeRisk => "High",
            ConcessionType.StakeReputation => "Very High",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }

    /// <summary>
    /// Determines if this concession requires additional context data.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <returns>True if additional data is required; otherwise, false.</returns>
    /// <remarks>
    /// Some concession types require specific data:
    /// <list type="bullet">
    ///   <item><description>OfferItem: Requires item ID</description></item>
    ///   <item><description>StakeReputation: Requires faction ID</description></item>
    /// </list>
    /// </remarks>
    public static bool RequiresContextData(this ConcessionType type)
    {
        return type switch
        {
            ConcessionType.OfferItem => true,       // Requires item ID
            ConcessionType.PromiseFavor => false,
            ConcessionType.TradeInformation => false,
            ConcessionType.TakeRisk => false,
            ConcessionType.StakeReputation => true, // Requires faction ID
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown concession type")
        };
    }
}
