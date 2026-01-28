using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single dropped item with quantity and optional quality tier information.
/// </summary>
/// <remarks>
/// <para>
/// DroppedItem is an immutable value object representing loot from defeated enemies
/// or containers. Items can optionally have quality tier information (v0.16.0d) which
/// affects their display name and stat bonuses.
/// </para>
/// <para>
/// For tiered equipment, use the <see cref="CreateWeapon"/> or <see cref="CreateArmor"/>
/// factory methods which enforce tier and bonus validation.
/// </para>
/// </remarks>
public readonly record struct DroppedItem
{
    /// <summary>
    /// Gets the unique identifier of the dropped item.
    /// </summary>
    public string ItemId { get; init; }

    /// <summary>
    /// Gets the display name of the item (base name without tier prefix).
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the quantity dropped.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Gets the quality tier of the item.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="QualityTier.Scavenged"/> for non-tiered items.
    /// </remarks>
    public QualityTier QualityTier { get; init; }

    /// <summary>
    /// Gets the attribute bonus value, if any.
    /// </summary>
    /// <remarks>
    /// Attribute bonuses are available for Tier 2+ items (ClanForged and above).
    /// The bonus value is rolled based on the tier's attribute bonus range.
    /// </remarks>
    public int? AttributeBonus { get; init; }

    /// <summary>
    /// Gets the attribute receiving the bonus.
    /// </summary>
    /// <remarks>
    /// Common attributes include: "Might", "Finesse", "Vigor", "Will", "Wits", "Presence".
    /// </remarks>
    public string? BonusAttribute { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the formatted display name including tier prefix and attribute bonus.
    /// </summary>
    /// <remarks>
    /// Format: "[TierName] ItemName +Bonus ATTR"
    /// Example: "[ClanForged] Iron Sword +2 MIG"
    /// </remarks>
    public string FormattedName
    {
        get
        {
            var tierPrefix = $"[{GetTierDisplayName(QualityTier)}]";
            var bonusSuffix = HasAttributeBonus
                ? $" +{AttributeBonus} {GetAttributeAbbreviation(BonusAttribute!)}"
                : string.Empty;
            return $"{tierPrefix} {Name}{bonusSuffix}";
        }
    }

    /// <summary>
    /// Gets whether this item has an attribute bonus.
    /// </summary>
    public bool HasAttributeBonus => AttributeBonus.HasValue && 
                                     AttributeBonus.Value > 0 && 
                                     !string.IsNullOrWhiteSpace(BonusAttribute);

    /// <summary>
    /// Gets whether this item is legendary (Myth-Forged tier).
    /// </summary>
    public bool IsLegendary => QualityTier == QualityTier.MythForged;

    /// <summary>
    /// Gets the numeric tier value (0-4) for sorting and comparison.
    /// </summary>
    public int TierValue => (int)QualityTier;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a basic dropped item without tier information.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <param name="name">The display name of the item.</param>
    /// <param name="quantity">The quantity dropped (default: 1).</param>
    /// <returns>A new DroppedItem instance.</returns>
    public static DroppedItem Create(string itemId, string name, int quantity = 1)
    {
        return new DroppedItem
        {
            ItemId = itemId,
            Name = name,
            Quantity = Math.Max(1, quantity),
            QualityTier = QualityTier.Scavenged,
            AttributeBonus = null,
            BonusAttribute = null
        };
    }

    /// <summary>
    /// Creates a tiered weapon item with optional attribute bonus.
    /// </summary>
    /// <param name="itemId">The unique identifier of the weapon.</param>
    /// <param name="name">The display name of the weapon.</param>
    /// <param name="tier">The quality tier of the weapon.</param>
    /// <param name="attributeBonus">Optional attribute bonus value.</param>
    /// <param name="bonusAttribute">Optional attribute name for the bonus.</param>
    /// <returns>A new DroppedItem representing the weapon.</returns>
    /// <remarks>
    /// Weapons typically receive Might or Finesse bonuses at higher tiers.
    /// </remarks>
    public static DroppedItem CreateWeapon(
        string itemId,
        string name,
        QualityTier tier,
        int? attributeBonus = null,
        string? bonusAttribute = null)
    {
        return new DroppedItem
        {
            ItemId = itemId,
            Name = name,
            Quantity = 1,
            QualityTier = tier,
            AttributeBonus = attributeBonus,
            BonusAttribute = bonusAttribute
        };
    }

    /// <summary>
    /// Creates a tiered armor item with optional attribute bonus.
    /// </summary>
    /// <param name="itemId">The unique identifier of the armor.</param>
    /// <param name="name">The display name of the armor.</param>
    /// <param name="tier">The quality tier of the armor.</param>
    /// <param name="attributeBonus">Optional attribute bonus value.</param>
    /// <param name="bonusAttribute">Optional attribute name for the bonus.</param>
    /// <returns>A new DroppedItem representing the armor.</returns>
    /// <remarks>
    /// Armor typically receives Vigor, Will, or Presence bonuses at higher tiers.
    /// </remarks>
    public static DroppedItem CreateArmor(
        string itemId,
        string name,
        QualityTier tier,
        int? attributeBonus = null,
        string? bonusAttribute = null)
    {
        return new DroppedItem
        {
            ItemId = itemId,
            Name = name,
            Quantity = 1,
            QualityTier = tier,
            AttributeBonus = attributeBonus,
            BonusAttribute = bonusAttribute
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a display-friendly name for the quality tier.
    /// </summary>
    private static string GetTierDisplayName(QualityTier tier) => tier switch
    {
        QualityTier.JuryRigged => "Jury-Rigged",
        QualityTier.Scavenged => "Scavenged",
        QualityTier.ClanForged => "Clan-Forged",
        QualityTier.Optimized => "Optimized",
        QualityTier.MythForged => "Myth-Forged",
        _ => tier.ToString()
    };

    /// <summary>
    /// Gets a 3-character abbreviation for an attribute name.
    /// </summary>
    private static string GetAttributeAbbreviation(string attribute) => attribute.ToUpperInvariant() switch
    {
        "MIGHT" => "MIG",
        "FINESSE" => "FIN",
        "VIGOR" => "VIG",
        "WILL" => "WIL",
        "WITS" => "WIT",
        "PRESENCE" => "PRE",
        _ => attribute.Length >= 3 
            ? attribute[..3].ToUpperInvariant() 
            : attribute.ToUpperInvariant()
    };

    /// <summary>
    /// Returns a string representation for logging.
    /// </summary>
    public override string ToString() => 
        Quantity > 1 
            ? $"{Quantity}x {FormattedName}" 
            : FormattedName;
}
