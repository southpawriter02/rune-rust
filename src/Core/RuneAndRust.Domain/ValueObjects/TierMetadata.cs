namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Contains metadata for a quality tier including display properties and configuration.
/// </summary>
/// <remarks>
/// <para>
/// TierMetadata is loaded from configuration (quality-tiers.json) rather than
/// hardcoded, allowing customization of tier names, colors, and descriptions
/// without code changes.
/// </para>
/// <para>
/// This value object is immutable after creation. Use the factory method
/// <see cref="Create"/> to construct new instances with validation.
/// </para>
/// </remarks>
/// <param name="Tier">The quality tier this metadata describes.</param>
/// <param name="DisplayName">The singular display name (e.g., "Clan-Forged").</param>
/// <param name="DisplayNamePlural">The plural display name (e.g., "Clan-Forged items").</param>
/// <param name="Description">Flavor text describing items of this tier.</param>
/// <param name="ColorDefinition">Color specification for UI rendering.</param>
/// <param name="IconGlyph">Single character icon for terminal display.</param>
/// <param name="DropWeightMultiplier">Relative drop rate multiplier (1.0 = baseline).</param>
public readonly record struct TierMetadata(
    QualityTier Tier,
    string DisplayName,
    string DisplayNamePlural,
    string Description,
    TierColorDefinition ColorDefinition,
    char IconGlyph,
    decimal DropWeightMultiplier)
{
    /// <summary>
    /// Gets the tier as an integer value.
    /// </summary>
    public int TierValue => (int)Tier;

    /// <summary>
    /// Gets whether this is the lowest tier (Jury-Rigged).
    /// </summary>
    public bool IsLowestTier => Tier == QualityTier.JuryRigged;

    /// <summary>
    /// Gets whether this is the highest tier (Myth-Forged).
    /// </summary>
    public bool IsHighestTier => Tier == QualityTier.MythForged;

    /// <summary>
    /// Gets whether this is a legendary tier (Myth-Forged).
    /// </summary>
    /// <remarks>
    /// Legendary items have special rules: unique per run, special effects, etc.
    /// </remarks>
    public bool IsLegendary => Tier == QualityTier.MythForged;

    /// <summary>
    /// Creates a new TierMetadata instance with validation.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <param name="displayName">The singular display name.</param>
    /// <param name="displayNamePlural">The plural display name.</param>
    /// <param name="description">Flavor text description.</param>
    /// <param name="colorDefinition">Color specification.</param>
    /// <param name="iconGlyph">Terminal icon character.</param>
    /// <param name="dropWeightMultiplier">Drop rate multiplier.</param>
    /// <returns>A new TierMetadata instance.</returns>
    /// <exception cref="ArgumentException">Thrown when string parameters are null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dropWeightMultiplier is negative.</exception>
    public static TierMetadata Create(
        QualityTier tier,
        string displayName,
        string displayNamePlural,
        string description,
        TierColorDefinition colorDefinition,
        char iconGlyph,
        decimal dropWeightMultiplier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayNamePlural);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfNegative(dropWeightMultiplier);

        return new TierMetadata(
            tier,
            displayName,
            displayNamePlural,
            description,
            colorDefinition,
            iconGlyph,
            dropWeightMultiplier);
    }

    /// <summary>
    /// Formats an item name with the tier prefix for display.
    /// </summary>
    /// <param name="itemName">The base item name.</param>
    /// <returns>The formatted name with tier prefix (e.g., "[Clan-Forged] Iron Sword").</returns>
    /// <exception cref="ArgumentException">Thrown when itemName is null or empty.</exception>
    public string FormatItemName(string itemName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);
        return $"[{DisplayName}] {itemName}";
    }

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    /// <returns>A formatted string representation.</returns>
    public override string ToString() =>
        $"{DisplayName} (Tier {TierValue}, Weight: {DropWeightMultiplier:F2})";
}
