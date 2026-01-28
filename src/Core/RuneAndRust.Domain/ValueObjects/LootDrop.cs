using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the generated loot from defeating a monster.
/// </summary>
/// <remarks>
/// <para>
/// LootDrop is an immutable value object that aggregates all items and currency
/// from a single loot event. It provides tier-aware statistics (v0.16.0d) for
/// analyzing drop quality.
/// </para>
/// <para>
/// Use the <see cref="Create"/> factory method to automatically calculate
/// tier statistics from the provided items.
/// </para>
/// </remarks>
public readonly record struct LootDrop
{
    /// <summary>
    /// Gets the items that dropped.
    /// </summary>
    public IReadOnlyList<DroppedItem> Items { get; init; }

    /// <summary>
    /// Gets the currency amounts that dropped.
    /// </summary>
    /// <remarks>
    /// Dictionary mapping currency ID to amount (e.g., "gold" -> 50).
    /// </remarks>
    public IReadOnlyDictionary<string, int> Currency { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // TIER AGGREGATION (v0.16.0d)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the highest quality tier among all dropped items.
    /// </summary>
    /// <remarks>
    /// Returns null if no items were dropped.
    /// </remarks>
    public QualityTier? HighestTier { get; init; }

    /// <summary>
    /// Gets whether any Myth-Forged (Tier 4) items dropped.
    /// </summary>
    public bool HasMythForged { get; init; }

    /// <summary>
    /// Gets the count of items by quality tier.
    /// </summary>
    public IReadOnlyDictionary<QualityTier, int> TierCounts { get; init; }

    /// <summary>
    /// Gets the monster or source that generated this loot.
    /// </summary>
    public string SourceMonster { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this loot drop is empty.
    /// </summary>
    public bool IsEmpty => (Items == null || Items.Count == 0) &&
                           (Currency == null || Currency.Count == 0);

    /// <summary>
    /// Gets whether this drop contains any items.
    /// </summary>
    public bool HasItems => Items != null && Items.Count > 0;

    /// <summary>
    /// Gets whether this drop contains any currency.
    /// </summary>
    public bool HasCurrency => Currency != null && Currency.Count > 0;

    /// <summary>
    /// Gets the total number of items dropped.
    /// </summary>
    public int TotalItems => Items?.Count ?? 0;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an empty loot drop.
    /// </summary>
    public static LootDrop Empty => new()
    {
        Items = [],
        Currency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
        HighestTier = null,
        HasMythForged = false,
        TierCounts = new Dictionary<QualityTier, int>(),
        SourceMonster = string.Empty
    };

    /// <summary>
    /// Creates a loot drop with the specified contents.
    /// </summary>
    /// <param name="items">The dropped items (optional).</param>
    /// <param name="currency">The dropped currency amounts (optional).</param>
    /// <param name="sourceMonster">The monster or source that generated this loot (optional).</param>
    /// <returns>A new LootDrop instance with calculated tier statistics.</returns>
    /// <remarks>
    /// This method automatically calculates tier aggregation statistics:
    /// <list type="bullet">
    ///   <item><description>HighestTier - The maximum tier among all items</description></item>
    ///   <item><description>HasMythForged - Whether any Myth-Forged items exist</description></item>
    ///   <item><description>TierCounts - Count of items per tier</description></item>
    /// </list>
    /// </remarks>
    public static LootDrop Create(
        IEnumerable<DroppedItem>? items = null,
        IDictionary<string, int>? currency = null,
        string? sourceMonster = null)
    {
        var itemList = items?.ToList() ?? [];
        
        // Calculate tier statistics
        var tierCounts = new Dictionary<QualityTier, int>();
        QualityTier? highestTier = null;
        var hasMythForged = false;

        foreach (var item in itemList)
        {
            // Update tier counts
            if (!tierCounts.TryGetValue(item.QualityTier, out var count))
            {
                count = 0;
            }
            tierCounts[item.QualityTier] = count + 1;

            // Track highest tier
            if (!highestTier.HasValue || item.QualityTier > highestTier.Value)
            {
                highestTier = item.QualityTier;
            }

            // Check for Myth-Forged
            if (item.QualityTier == QualityTier.MythForged)
            {
                hasMythForged = true;
            }
        }

        return new LootDrop
        {
            Items = itemList,
            Currency = currency != null
                ? new Dictionary<string, int>(currency, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            HighestTier = highestTier,
            HasMythForged = hasMythForged,
            TierCounts = tierCounts,
            SourceMonster = sourceMonster ?? string.Empty
        };
    }

    /// <summary>
    /// Combines this loot drop with another.
    /// </summary>
    /// <param name="other">The other loot drop to combine with.</param>
    /// <returns>A new LootDrop containing the combined loot.</returns>
    public LootDrop CombineWith(LootDrop other)
    {
        var combinedItems = new List<DroppedItem>(Items ?? []);
        if (other.Items != null)
        {
            combinedItems.AddRange(other.Items);
        }

        var combinedCurrency = new Dictionary<string, int>(
            Currency ?? new Dictionary<string, int>(),
            StringComparer.OrdinalIgnoreCase);

        if (other.Currency != null)
        {
            foreach (var kvp in other.Currency)
            {
                if (combinedCurrency.TryGetValue(kvp.Key, out var existing))
                {
                    combinedCurrency[kvp.Key] = existing + kvp.Value;
                }
                else
                {
                    combinedCurrency[kvp.Key] = kvp.Value;
                }
            }
        }

        // Use Create to recalculate tier statistics
        var combinedSource = !string.IsNullOrEmpty(SourceMonster) 
            ? SourceMonster 
            : other.SourceMonster;
        
        return Create(combinedItems, combinedCurrency, combinedSource);
    }

    /// <summary>
    /// Returns a string representation for logging.
    /// </summary>
    public override string ToString()
    {
        if (IsEmpty)
        {
            return "[LootDrop: Empty]";
        }

        var parts = new List<string>();
        
        if (HasItems)
        {
            parts.Add($"{TotalItems} item(s)");
            if (HighestTier.HasValue)
            {
                parts.Add($"highest tier={HighestTier.Value}");
            }
            if (HasMythForged)
            {
                parts.Add("includes Myth-Forged");
            }
        }

        if (HasCurrency)
        {
            var currencyParts = Currency.Select(kvp => $"{kvp.Value} {kvp.Key}");
            parts.Add(string.Join(", ", currencyParts));
        }

        var source = !string.IsNullOrEmpty(SourceMonster) 
            ? $" from {SourceMonster}" 
            : string.Empty;

        return $"[LootDrop: {string.Join(", ", parts)}{source}]";
    }
}
