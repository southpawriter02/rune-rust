namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to quality tier metadata for equipment items.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the source of tier metadata, allowing it to be
/// loaded from configuration files, databases, or hardcoded defaults.
/// </para>
/// <para>
/// Implementations should cache loaded metadata for performance and validate
/// that all quality tiers have corresponding metadata entries.
/// </para>
/// </remarks>
public interface ITierMetadataProvider
{
    /// <summary>
    /// Gets the metadata for a specific quality tier.
    /// </summary>
    /// <param name="tier">The quality tier to get metadata for.</param>
    /// <returns>The TierMetadata for the specified tier.</returns>
    /// <exception cref="ArgumentException">Thrown when metadata for the tier is not found.</exception>
    TierMetadata GetMetadata(QualityTier tier);

    /// <summary>
    /// Gets metadata for all quality tiers.
    /// </summary>
    /// <returns>
    /// A read-only list of all tier metadata, ordered by tier value ascending.
    /// </returns>
    IReadOnlyList<TierMetadata> GetAllMetadata();

    /// <summary>
    /// Gets the color definition for a specific quality tier.
    /// </summary>
    /// <param name="tier">The quality tier to get the color for.</param>
    /// <returns>The TierColorDefinition for the specified tier.</returns>
    /// <exception cref="ArgumentException">Thrown when metadata for the tier is not found.</exception>
    TierColorDefinition GetTierColor(QualityTier tier);

    /// <summary>
    /// Gets the display name for a specific quality tier.
    /// </summary>
    /// <param name="tier">The quality tier to get the name for.</param>
    /// <returns>The display name (e.g., "Clan-Forged").</returns>
    /// <exception cref="ArgumentException">Thrown when metadata for the tier is not found.</exception>
    string GetTierDisplayName(QualityTier tier);

    /// <summary>
    /// Gets the icon glyph for a specific quality tier.
    /// </summary>
    /// <param name="tier">The quality tier to get the icon for.</param>
    /// <returns>The icon character for terminal display.</returns>
    /// <exception cref="ArgumentException">Thrown when metadata for the tier is not found.</exception>
    char GetTierIcon(QualityTier tier);

    /// <summary>
    /// Formats an item name with the appropriate tier prefix.
    /// </summary>
    /// <param name="tier">The quality tier of the item.</param>
    /// <param name="itemName">The base item name.</param>
    /// <returns>The formatted name (e.g., "[Clan-Forged] Iron Sword").</returns>
    string FormatItemName(QualityTier tier, string itemName);

    /// <summary>
    /// Gets the drop weight multiplier for a specific tier.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <returns>The relative drop weight (1.0 = baseline).</returns>
    decimal GetDropWeight(QualityTier tier);

    /// <summary>
    /// Validates that all required tiers have metadata entries.
    /// </summary>
    /// <returns>True if all tiers are configured, false otherwise.</returns>
    bool ValidateConfiguration();
}
