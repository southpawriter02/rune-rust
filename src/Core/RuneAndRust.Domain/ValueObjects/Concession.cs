// ------------------------------------------------------------------------------
// <copyright file="Concession.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents a voluntary concession made during negotiation, providing
// DC reduction and bonus dice for subsequent checks.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents a voluntary concession made during negotiation.
/// </summary>
/// <remarks>
/// <para>
/// Concessions are voluntary sacrifices made by the player to gain advantage
/// in subsequent negotiation checks. All concessions provide bonus dice and
/// a DC reduction on the next check, with the magnitude varying by type.
/// </para>
/// <para>
/// The trade-off for making concessions:
/// </para>
/// <list type="bullet">
///   <item><description>Immediate benefit: +2d10 bonus dice and DC reduction on next check</description></item>
///   <item><description>Position cost: Player position moves 1 step toward opponent (via Concede tactic)</description></item>
///   <item><description>Resource cost: Items consumed, debts created, secrets revealed, etc.</description></item>
/// </list>
/// <para>
/// Concession benefits are applied to the next check only and do not stack.
/// If multiple concessions are made before a check, only the most recent applies.
/// </para>
/// </remarks>
public sealed record Concession
{
    /// <summary>
    /// Gets the type of concession being offered.
    /// </summary>
    /// <remarks>
    /// The type determines the DC reduction amount and the nature of the cost.
    /// </remarks>
    public required ConcessionType Type { get; init; }

    /// <summary>
    /// Gets the description of what was conceded.
    /// </summary>
    /// <remarks>
    /// This is a player-facing description that explains what they're giving up.
    /// </remarks>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the DC reduction provided for the next check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC reductions by type:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>OfferItem: -2 DC</description></item>
    ///   <item><description>PromiseFavor: -2 DC</description></item>
    ///   <item><description>TradeInformation: -4 DC</description></item>
    ///   <item><description>TakeRisk: -4 DC</description></item>
    ///   <item><description>StakeReputation: -6 DC</description></item>
    /// </list>
    /// </remarks>
    public required int DcReduction { get; init; }

    /// <summary>
    /// Gets the bonus dice provided for the next check.
    /// </summary>
    /// <remarks>
    /// All concession types grant +2d10 bonus dice, regardless of DC reduction.
    /// </remarks>
    public int BonusDice { get; init; } = 2;

    /// <summary>
    /// Gets the cost or consequence description of this concession.
    /// </summary>
    /// <remarks>
    /// This describes what the player loses or risks by making this concession.
    /// </remarks>
    public required string Cost { get; init; }

    /// <summary>
    /// Gets the item ID consumed, if offering an item.
    /// </summary>
    /// <remarks>
    /// Only populated for <see cref="ConcessionType.OfferItem"/> concessions.
    /// The item will be removed from the player's inventory when the concession is made.
    /// </remarks>
    public string? ConsumedItemId { get; init; }

    /// <summary>
    /// Gets the debt or obligation created, if promising a favor.
    /// </summary>
    /// <remarks>
    /// Only populated for <see cref="ConcessionType.PromiseFavor"/> concessions.
    /// The NPC may call in this favor at a later time.
    /// </remarks>
    public string? DebtCreated { get; init; }

    /// <summary>
    /// Gets the information revealed, if trading information.
    /// </summary>
    /// <remarks>
    /// Only populated for <see cref="ConcessionType.TradeInformation"/> concessions.
    /// Once revealed, information cannot be taken back.
    /// </remarks>
    public string? InformationRevealed { get; init; }

    /// <summary>
    /// Gets the risk description, if taking on personal risk.
    /// </summary>
    /// <remarks>
    /// Only populated for <see cref="ConcessionType.TakeRisk"/> concessions.
    /// Describes the danger or disadvantage the player accepts.
    /// </remarks>
    public string? RiskAccepted { get; init; }

    /// <summary>
    /// Gets the faction ID whose reputation is staked, if applicable.
    /// </summary>
    /// <remarks>
    /// Only populated for <see cref="ConcessionType.StakeReputation"/> concessions.
    /// Failure will damage reputation with this faction.
    /// </remarks>
    public string? StakedFactionId { get; init; }

    /// <summary>
    /// Gets the round number in which this concession was made.
    /// </summary>
    /// <remarks>
    /// Used for tracking when concessions were made during the negotiation.
    /// </remarks>
    public int? RoundMade { get; init; }

    /// <summary>
    /// Gets a value indicating whether this concession has been consumed.
    /// </summary>
    /// <remarks>
    /// A concession is consumed when its bonus is applied to a check.
    /// Consumed concessions cannot provide bonuses again.
    /// </remarks>
    public bool IsConsumed { get; init; }

    /// <summary>
    /// Creates a concession for offering an item of value.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item being offered.</param>
    /// <param name="itemDescription">A human-readable description of the item.</param>
    /// <returns>A new <see cref="Concession"/> for offering an item.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> or <paramref name="itemDescription"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// OfferItem concessions provide -2 DC and +2d10 bonus dice.
    /// </para>
    /// <para>
    /// The item will be consumed (removed from inventory) when the concession is made.
    /// Choose items carefully, as this is a permanent loss.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var concession = Concession.OfferItem("item_123", "Dvergr compass");
    /// // Type: OfferItem
    /// // DcReduction: 2
    /// // BonusDice: 2
    /// // ConsumedItemId: "item_123"
    /// </code>
    /// </example>
    public static Concession OfferItem(string itemId, string itemDescription)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID is required.", nameof(itemId));
        }

        if (string.IsNullOrWhiteSpace(itemDescription))
        {
            throw new ArgumentException("Item description is required.", nameof(itemDescription));
        }

        return new Concession
        {
            Type = ConcessionType.OfferItem,
            Description = $"Offering {itemDescription} as part of the deal",
            DcReduction = ConcessionType.OfferItem.GetDcReduction(),
            BonusDice = ConcessionType.OfferItem.GetBonusDice(),
            Cost = $"Item '{itemDescription}' will be consumed",
            ConsumedItemId = itemId,
            IsConsumed = false
        };
    }

    /// <summary>
    /// Creates a concession for promising a future favor.
    /// </summary>
    /// <param name="favorDescription">A description of the promised favor.</param>
    /// <returns>A new <see cref="Concession"/> for promising a favor.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="favorDescription"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// PromiseFavor concessions provide -2 DC and +2d10 bonus dice.
    /// </para>
    /// <para>
    /// This creates a debt that must be repaid later. The NPC may call in
    /// the favor at an inconvenient time, potentially creating complications.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var concession = Concession.PromiseFavor("A future favor - no questions asked");
    /// // Type: PromiseFavor
    /// // DcReduction: 2
    /// // BonusDice: 2
    /// // DebtCreated: "A future favor - no questions asked"
    /// </code>
    /// </example>
    public static Concession PromiseFavor(string favorDescription)
    {
        if (string.IsNullOrWhiteSpace(favorDescription))
        {
            throw new ArgumentException("Favor description is required.", nameof(favorDescription));
        }

        return new Concession
        {
            Type = ConcessionType.PromiseFavor,
            Description = $"Promising: {favorDescription}",
            DcReduction = ConcessionType.PromiseFavor.GetDcReduction(),
            BonusDice = ConcessionType.PromiseFavor.GetBonusDice(),
            Cost = "Creates a debt that must be repaid",
            DebtCreated = favorDescription,
            IsConsumed = false
        };
    }

    /// <summary>
    /// Creates a concession for trading valuable information.
    /// </summary>
    /// <param name="informationDescription">A description of the information being revealed.</param>
    /// <returns>A new <see cref="Concession"/> for trading information.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="informationDescription"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// TradeInformation concessions provide -4 DC and +2d10 bonus dice.
    /// </para>
    /// <para>
    /// Information cannot be unshared once revealed. The NPC may use this
    /// information against the player's interests in the future.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var concession = Concession.TradeInformation("The patrol routes of the Combine guards");
    /// // Type: TradeInformation
    /// // DcReduction: 4
    /// // BonusDice: 2
    /// // InformationRevealed: "The patrol routes of the Combine guards"
    /// </code>
    /// </example>
    public static Concession TradeInformation(string informationDescription)
    {
        if (string.IsNullOrWhiteSpace(informationDescription))
        {
            throw new ArgumentException("Information description is required.", nameof(informationDescription));
        }

        return new Concession
        {
            Type = ConcessionType.TradeInformation,
            Description = $"Revealing: {informationDescription}",
            DcReduction = ConcessionType.TradeInformation.GetDcReduction(),
            BonusDice = ConcessionType.TradeInformation.GetBonusDice(),
            Cost = "Information is now known to the NPC",
            InformationRevealed = informationDescription,
            IsConsumed = false
        };
    }

    /// <summary>
    /// Creates a concession for taking on personal risk.
    /// </summary>
    /// <param name="riskDescription">A description of the risk being accepted.</param>
    /// <returns>A new <see cref="Concession"/> for taking risk.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="riskDescription"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// TakeRisk concessions provide -4 DC and +2d10 bonus dice.
    /// </para>
    /// <para>
    /// By accepting vulnerability, the player demonstrates serious intent.
    /// The risk may or may not materialize, but it signals commitment.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var concession = Concession.TakeRisk("I'll go first into the dangerous section");
    /// // Type: TakeRisk
    /// // DcReduction: 4
    /// // BonusDice: 2
    /// // RiskAccepted: "I'll go first into the dangerous section"
    /// </code>
    /// </example>
    public static Concession TakeRisk(string riskDescription)
    {
        if (string.IsNullOrWhiteSpace(riskDescription))
        {
            throw new ArgumentException("Risk description is required.", nameof(riskDescription));
        }

        return new Concession
        {
            Type = ConcessionType.TakeRisk,
            Description = $"Accepting risk: {riskDescription}",
            DcReduction = ConcessionType.TakeRisk.GetDcReduction(),
            BonusDice = ConcessionType.TakeRisk.GetBonusDice(),
            Cost = riskDescription,
            RiskAccepted = riskDescription,
            IsConsumed = false
        };
    }

    /// <summary>
    /// Creates a concession for putting faction reputation on the line.
    /// </summary>
    /// <param name="factionId">The unique identifier of the faction whose reputation is staked.</param>
    /// <param name="stakeDescription">A description of how the reputation is being staked.</param>
    /// <returns>A new <see cref="Concession"/> for staking reputation.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="factionId"/> or <paramref name="stakeDescription"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// StakeReputation concessions provide -6 DC and +2d10 bonus dice.
    /// This is the most powerful concession type.
    /// </para>
    /// <para>
    /// By staking reputation, the player demonstrates ultimate commitment.
    /// However, if the deal falls through or the player fails to uphold
    /// their end, reputation damage with the faction is severe.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var concession = Concession.StakeReputation("faction_scavenger_guild", "The Guild stands behind my word");
    /// // Type: StakeReputation
    /// // DcReduction: 6
    /// // BonusDice: 2
    /// // StakedFactionId: "faction_scavenger_guild"
    /// </code>
    /// </example>
    public static Concession StakeReputation(string factionId, string stakeDescription)
    {
        if (string.IsNullOrWhiteSpace(factionId))
        {
            throw new ArgumentException("Faction ID is required.", nameof(factionId));
        }

        if (string.IsNullOrWhiteSpace(stakeDescription))
        {
            throw new ArgumentException("Stake description is required.", nameof(stakeDescription));
        }

        return new Concession
        {
            Type = ConcessionType.StakeReputation,
            Description = $"Staking reputation: {stakeDescription}",
            DcReduction = ConcessionType.StakeReputation.GetDcReduction(),
            BonusDice = ConcessionType.StakeReputation.GetBonusDice(),
            Cost = "Failure will damage faction reputation significantly",
            StakedFactionId = factionId,
            IsConsumed = false
        };
    }

    /// <summary>
    /// Creates a consumed version of this concession.
    /// </summary>
    /// <returns>A new <see cref="Concession"/> with <see cref="IsConsumed"/> set to true.</returns>
    /// <remarks>
    /// Call this method after applying the concession's bonus to a check.
    /// The consumed concession cannot provide bonuses again.
    /// </remarks>
    public Concession MarkAsConsumed()
    {
        return this with { IsConsumed = true };
    }

    /// <summary>
    /// Creates a version of this concession with the round number set.
    /// </summary>
    /// <param name="roundNumber">The round in which the concession was made.</param>
    /// <returns>A new <see cref="Concession"/> with the round number set.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="roundNumber"/> is less than 1.
    /// </exception>
    public Concession WithRound(int roundNumber)
    {
        if (roundNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roundNumber),
                roundNumber,
                "Round number must be at least 1.");
        }

        return this with { RoundMade = roundNumber };
    }

    /// <summary>
    /// Gets a formatted summary of this concession for display.
    /// </summary>
    /// <returns>A human-readable summary of the concession.</returns>
    /// <remarks>
    /// The summary includes the concession type, description, and mechanical benefits.
    /// </remarks>
    public string ToSummary()
    {
        var status = IsConsumed ? " [Used]" : "";
        var round = RoundMade.HasValue ? $" (Round {RoundMade})" : "";
        return $"[{Type.GetDisplayName()}]{status}{round}: {Description} " +
               $"(Next check: -{DcReduction} DC, +{BonusDice}d10)";
    }

    /// <summary>
    /// Gets a short display string for compact UI elements.
    /// </summary>
    /// <returns>A compact representation of the concession.</returns>
    public string ToShortDisplay()
    {
        return $"{Type.GetDisplayName()}: -{DcReduction} DC, +{BonusDice}d10";
    }
}
