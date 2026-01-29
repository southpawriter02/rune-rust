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
    // SMART LOOT METADATA (v0.16.3d)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the primary item was selected from the class-appropriate pool.
    /// </summary>
    /// <remarks>
    /// When true, the loot bias algorithm successfully selected an item matching
    /// the player's archetype. When false, either random selection was used or
    /// no class-appropriate items were available (fallback).
    /// </remarks>
    public bool WasClassAppropriate { get; init; }

    /// <summary>
    /// Gets the player archetype ID used for smart loot generation, if any.
    /// </summary>
    /// <remarks>
    /// Null when no player context was available (e.g., environmental loot).
    /// </remarks>
    public string? PlayerArchetypeId { get; init; }

    /// <summary>
    /// Gets a human-readable explanation of the selection logic.
    /// </summary>
    /// <remarks>
    /// Examples: "Class-appropriate bias selection", "Random selection",
    /// "Fallback: no class-appropriate items", "Random only: no archetype".
    /// </remarks>
    public string SelectionReason { get; init; }

    /// <summary>
    /// Gets the bias roll (0-99) used for selection, or -1 if not applicable.
    /// </summary>
    /// <remarks>
    /// With default 60% bias: rolls 0-59 favor class-appropriate,
    /// rolls 60-99 favor random selection.
    /// </remarks>
    public int BiasRoll { get; init; }

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

    /// <summary>
    /// Gets whether this loot drop was generated with player context.
    /// </summary>
    /// <remarks>
    /// Returns true when <see cref="PlayerArchetypeId"/> is set,
    /// indicating the smart loot algorithm considered player class.
    /// </remarks>
    public bool HasPlayerContext => !string.IsNullOrWhiteSpace(PlayerArchetypeId);

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
        SourceMonster = string.Empty,
        WasClassAppropriate = false,
        PlayerArchetypeId = null,
        SelectionReason = string.Empty,
        BiasRoll = -1
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
            SourceMonster = sourceMonster ?? string.Empty,
            WasClassAppropriate = false,
            PlayerArchetypeId = null,
            SelectionReason = "Direct creation",
            BiasRoll = -1
        };
    }

    /// <summary>
    /// Creates a loot drop from a <see cref="SmartLootResult"/>.
    /// </summary>
    /// <param name="result">The smart loot selection result.</param>
    /// <param name="playerArchetypeId">The player's archetype ID, if available.</param>
    /// <param name="sourceMonster">The monster or source that generated this loot.</param>
    /// <returns>A new LootDrop with smart loot metadata populated.</returns>
    /// <remarks>
    /// This factory method preserves all metadata from the smart loot algorithm
    /// for debugging, statistics, and UI display.
    /// </remarks>
    public static LootDrop CreateFrom(
        SmartLootResult result,
        string? playerArchetypeId,
        string? sourceMonster = null)
    {
        if (!result.HasSelection || result.SelectedItem is null)
        {
            return Empty;
        }

        // Create dropped item from the selected loot entry
        var item = DroppedItem.CreateFromLootEntry(result.SelectedItem.Value);
        var items = new List<DroppedItem> { item };

        // Calculate tier statistics for the single item
        var tierCounts = new Dictionary<QualityTier, int>
        {
            [item.QualityTier] = 1
        };

        return new LootDrop
        {
            Items = items,
            Currency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            HighestTier = item.QualityTier,
            HasMythForged = item.QualityTier == QualityTier.MythForged,
            TierCounts = tierCounts,
            SourceMonster = sourceMonster ?? string.Empty,
            WasClassAppropriate = result.WasClassAppropriate,
            PlayerArchetypeId = playerArchetypeId,
            SelectionReason = result.SelectionReason,
            BiasRoll = result.BiasRoll
        };
    }

    /// <summary>
    /// Creates a loot drop from a random selection (no player context).
    /// </summary>
    /// <param name="entry">The randomly selected loot entry.</param>
    /// <param name="sourceMonster">The monster or source that generated this loot.</param>
    /// <returns>A new LootDrop with random selection metadata.</returns>
    /// <remarks>
    /// Use this factory when generating loot without player context,
    /// such as environmental drops or pre-placed loot.
    /// </remarks>
    public static LootDrop CreateRandom(
        LootEntry entry,
        string? sourceMonster = null)
    {
        var item = DroppedItem.CreateFromLootEntry(entry);
        var items = new List<DroppedItem> { item };

        var tierCounts = new Dictionary<QualityTier, int>
        {
            [item.QualityTier] = 1
        };

        return new LootDrop
        {
            Items = items,
            Currency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            HighestTier = item.QualityTier,
            HasMythForged = item.QualityTier == QualityTier.MythForged,
            TierCounts = tierCounts,
            SourceMonster = sourceMonster ?? string.Empty,
            WasClassAppropriate = false,
            PlayerArchetypeId = null,
            SelectionReason = "Random selection: no player context",
            BiasRoll = -1
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
