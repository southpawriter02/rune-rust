using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines the loot specifications for a container type.
/// </summary>
/// <remarks>
/// <para>
/// This value object is immutable and loaded from container-types.json configuration.
/// It specifies item counts, quality tier ranges, currency amounts, and special behaviors.
/// </para>
/// <para>
/// The <see cref="ContainerTypeDefinition"/> provides complete metadata for loot generation,
/// including:
/// <list type="bullet">
///     <item><description>Item count ranges (min/max items)</description></item>
///     <item><description>Quality tier ranges (tier 0-4)</description></item>
///     <item><description>Currency ranges (optional)</description></item>
///     <item><description>Item category filters (consumables, weapons, armor)</description></item>
///     <item><description>Discovery difficulty class for hidden containers</description></item>
///     <item><description>Special behavior flags</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a small chest definition
/// var smallChest = ContainerTypeDefinition.Create(
///     type: ContainerType.SmallChest,
///     minItems: 1,
///     maxItems: 2,
///     minTier: 0,
///     maxTier: 1,
///     minCurrency: 10,
///     maxCurrency: 30);
/// 
/// // Create a weapon rack with category filter
/// var weaponRack = ContainerTypeDefinition.Create(
///     type: ContainerType.WeaponRack,
///     minItems: 1,
///     maxItems: 2,
///     minTier: 1,
///     maxTier: 3,
///     itemCategoryFilter: "weapons");
/// </code>
/// </example>
/// <param name="Type">The container type this definition applies to.</param>
/// <param name="MinItems">Minimum number of items the container can hold (0 or greater).</param>
/// <param name="MaxItems">Maximum number of items the container can hold (must be >= MinItems).</param>
/// <param name="MinTier">Minimum quality tier for items (0-4 scale, 0 = JuryRigged).</param>
/// <param name="MaxTier">Maximum quality tier for items (0-4 scale, 4 = MythForged).</param>
/// <param name="MinCurrency">Minimum currency amount, or null if container awards no currency.</param>
/// <param name="MaxCurrency">Maximum currency amount, or null if container awards no currency.</param>
/// <param name="ItemCategoryFilter">Item category restriction (e.g., "weapons", "armor", "consumables"), or null for any item type.</param>
/// <param name="DiscoveryDC">Difficulty class to discover (Perception check), or null if container is always visible.</param>
/// <param name="Flags">Special behavior flags controlling container generation and interaction.</param>
public readonly record struct ContainerTypeDefinition(
    ContainerType Type,
    int MinItems,
    int MaxItems,
    int MinTier,
    int MaxTier,
    int? MinCurrency,
    int? MaxCurrency,
    string? ItemCategoryFilter,
    int? DiscoveryDC,
    ContainerFlags Flags)
{
    #region Computed Properties

    /// <summary>
    /// Gets whether this container can potentially be empty (0 items).
    /// </summary>
    /// <remarks>
    /// A container can be empty if either its MinItems is 0 or it has the
    /// <see cref="ContainerFlags.MayBeEmpty"/> flag set.
    /// </remarks>
    public bool CanBeEmpty => MinItems == 0 || Flags.HasFlag(ContainerFlags.MayBeEmpty);

    /// <summary>
    /// Gets whether this container can spawn Myth-Forged (tier 4) items.
    /// </summary>
    /// <remarks>
    /// Myth-Forged items are legendary-tier equipment with unique properties.
    /// Only containers with the <see cref="ContainerFlags.MythForgedChance"/> flag
    /// have a chance to generate these items.
    /// </remarks>
    public bool HasMythForgedChance => Flags.HasFlag(ContainerFlags.MythForgedChance);

    /// <summary>
    /// Gets whether this container requires a discovery check to find.
    /// </summary>
    /// <remarks>
    /// A container requires discovery if it has a DiscoveryDC set or the
    /// <see cref="ContainerFlags.RequiresDiscovery"/> flag is set.
    /// </remarks>
    public bool RequiresDiscovery => DiscoveryDC.HasValue || Flags.HasFlag(ContainerFlags.RequiresDiscovery);

    /// <summary>
    /// Gets whether this container has a category restriction on loot.
    /// </summary>
    /// <remarks>
    /// When true, only items matching the <see cref="ItemCategoryFilter"/>
    /// can be generated (e.g., weapons only, armor only, consumables only).
    /// </remarks>
    public bool HasCategoryFilter => !string.IsNullOrWhiteSpace(ItemCategoryFilter);

    /// <summary>
    /// Gets whether this container awards currency in addition to items.
    /// </summary>
    /// <remarks>
    /// Currency is awarded when both MinCurrency and MaxCurrency have values.
    /// Some containers (weapon racks, armor stands) never award currency.
    /// </remarks>
    public bool AwardsCurrency => MinCurrency.HasValue && MaxCurrency.HasValue;

    /// <summary>
    /// Gets the average number of items for this container type.
    /// </summary>
    /// <remarks>
    /// Useful for analysis and balancing calculations.
    /// </remarks>
    public double AverageItemCount => (MinItems + MaxItems) / 2.0;

    /// <summary>
    /// Gets the average currency for this container type, or 0 if no currency.
    /// </summary>
    /// <remarks>
    /// Returns 0 if the container does not award currency.
    /// </remarks>
    public double AverageCurrency => AwardsCurrency
        ? (MinCurrency!.Value + MaxCurrency!.Value) / 2.0
        : 0;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new <see cref="ContainerTypeDefinition"/> with full validation.
    /// </summary>
    /// <param name="type">The container type this definition applies to.</param>
    /// <param name="minItems">Minimum number of items (must be >= 0).</param>
    /// <param name="maxItems">Maximum number of items (must be >= minItems).</param>
    /// <param name="minTier">Minimum quality tier (must be 0-4).</param>
    /// <param name="maxTier">Maximum quality tier (must be minTier-4).</param>
    /// <param name="minCurrency">Minimum currency, or null for no currency.</param>
    /// <param name="maxCurrency">Maximum currency, or null for no currency.</param>
    /// <param name="itemCategoryFilter">Category restriction, or null for any.</param>
    /// <param name="discoveryDC">Discovery difficulty class, or null if visible.</param>
    /// <param name="flags">Special behavior flags.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated <see cref="ContainerTypeDefinition"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any parameter values are outside their valid ranges.
    /// </exception>
    public static ContainerTypeDefinition Create(
        ContainerType type,
        int minItems,
        int maxItems,
        int minTier,
        int maxTier,
        int? minCurrency = null,
        int? maxCurrency = null,
        string? itemCategoryFilter = null,
        int? discoveryDC = null,
        ContainerFlags flags = ContainerFlags.None,
        ILogger? logger = null)
    {
        // Validate item count range
        ArgumentOutOfRangeException.ThrowIfNegative(minItems, nameof(minItems));
        ArgumentOutOfRangeException.ThrowIfNegative(maxItems, nameof(maxItems));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxItems, minItems, nameof(maxItems));

        // Validate tier range (0-4 scale per QualityTier enum)
        ArgumentOutOfRangeException.ThrowIfNegative(minTier, nameof(minTier));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minTier, 4, nameof(minTier));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxTier, 4, nameof(maxTier));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTier, minTier, nameof(maxTier));

        // Validate currency range
        if (minCurrency.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(minCurrency.Value, nameof(minCurrency));
        }

        if (maxCurrency.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxCurrency.Value, nameof(maxCurrency));

            if (minCurrency.HasValue && maxCurrency.Value < minCurrency.Value)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxCurrency),
                    maxCurrency.Value,
                    $"MaxCurrency ({maxCurrency.Value}) must be >= MinCurrency ({minCurrency.Value}).");
            }
        }

        // Validate discovery DC
        if (discoveryDC.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(discoveryDC.Value, 1, nameof(discoveryDC));
        }

        // Normalize category filter to lowercase for case-insensitive matching
        var normalizedFilter = itemCategoryFilter?.ToLowerInvariant();

        logger?.LogDebug(
            "Created ContainerTypeDefinition for {Type}: Items={MinItems}-{MaxItems}, " +
            "Tier={MinTier}-{MaxTier}, Currency={MinCurrency}-{MaxCurrency}, " +
            "Filter={Filter}, DC={DC}, Flags={Flags}",
            type,
            minItems,
            maxItems,
            minTier,
            maxTier,
            minCurrency?.ToString() ?? "none",
            maxCurrency?.ToString() ?? "none",
            normalizedFilter ?? "any",
            discoveryDC?.ToString() ?? "visible",
            flags);

        return new ContainerTypeDefinition(
            type,
            minItems,
            maxItems,
            minTier,
            maxTier,
            minCurrency,
            maxCurrency,
            normalizedFilter,
            discoveryDC,
            flags);
    }

    /// <summary>
    /// Creates a simple container definition with minimal configuration.
    /// </summary>
    /// <param name="type">The container type.</param>
    /// <param name="itemRange">Tuple of (minItems, maxItems).</param>
    /// <param name="tierRange">Tuple of (minTier, maxTier).</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated <see cref="ContainerTypeDefinition"/> with no currency or filters.</returns>
    public static ContainerTypeDefinition CreateSimple(
        ContainerType type,
        (int min, int max) itemRange,
        (int min, int max) tierRange,
        ILogger? logger = null) =>
        Create(
            type,
            itemRange.min,
            itemRange.max,
            tierRange.min,
            tierRange.max,
            logger: logger);

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates that the specified item count is within this container's range.
    /// </summary>
    /// <param name="itemCount">The item count to validate.</param>
    /// <returns>True if the item count is valid for this container type.</returns>
    public bool IsValidItemCount(int itemCount) =>
        itemCount >= MinItems && itemCount <= MaxItems;

    /// <summary>
    /// Validates that the specified tier is within this container's range.
    /// </summary>
    /// <param name="tier">The tier to validate (0-4).</param>
    /// <returns>True if the tier is valid for this container type.</returns>
    public bool IsValidTier(int tier) =>
        tier >= MinTier && tier <= MaxTier;

    /// <summary>
    /// Validates that the specified currency amount is within this container's range.
    /// </summary>
    /// <param name="currency">The currency amount to validate.</param>
    /// <returns>True if the currency is valid, or true if this container doesn't award currency.</returns>
    public bool IsValidCurrency(int currency)
    {
        if (!AwardsCurrency)
        {
            return currency == 0;
        }

        return currency >= MinCurrency!.Value && currency <= MaxCurrency!.Value;
    }

    #endregion

    #region ToString

    /// <inheritdoc />
    public override string ToString()
    {
        var result = $"{Type}: Items={MinItems}-{MaxItems}, Tier={MinTier}-{MaxTier}";

        if (AwardsCurrency)
        {
            result += $", Currency={MinCurrency}-{MaxCurrency}";
        }

        if (HasCategoryFilter)
        {
            result += $", Filter={ItemCategoryFilter}";
        }

        if (RequiresDiscovery)
        {
            result += $", DC={DiscoveryDC}";
        }

        if (Flags != ContainerFlags.None)
        {
            result += $", Flags={Flags}";
        }

        return result;
    }

    #endregion
}
