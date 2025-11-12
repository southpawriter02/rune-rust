namespace RuneAndRust.Core.Quests;

/// <summary>
/// Represents rewards for completing a quest (v0.8, v0.9, enhanced in v0.14)
/// </summary>
public class QuestReward
{
    // Experience/Legend
    public int Experience { get; set; } = 0;

    // Currency
    public int Currency { get; set; } = 0; // Dvergr Cogs (v0.9)

    // Items
    public List<string> ItemIds { get; set; } = new(); // Item IDs to grant
    public List<ItemReward> DetailedItems { get; set; } = new(); // v0.14: Items with quantities/quality

    // Reputation
    public int ReputationChange { get; set; } = 0; // Legacy single faction
    public FactionType? Faction { get; set; } = null; // Legacy single faction
    public Dictionary<string, int> ReputationGains { get; set; } = new(); // v0.14: Multiple factions

    // Unlocks (v0.14)
    public List<string> UnlockedAbilities { get; set; } = new();
    public List<string> UnlockedAreas { get; set; } = new();
    public List<string> UnlockedQuests { get; set; } = new();

    /// <summary>
    /// Gets all reputation changes (combines legacy and new formats)
    /// </summary>
    public Dictionary<string, int> GetAllReputationGains()
    {
        var gains = new Dictionary<string, int>(ReputationGains);

        // Add legacy reputation if specified
        if (Faction.HasValue && ReputationChange != 0)
        {
            var factionKey = Faction.Value.ToString();
            if (!gains.ContainsKey(factionKey))
                gains[factionKey] = ReputationChange;
        }

        return gains;
    }

    /// <summary>
    /// Gets all items to grant (combines legacy and new formats)
    /// </summary>
    public List<ItemReward> GetAllItems()
    {
        var items = new List<ItemReward>(DetailedItems);

        // Add legacy item IDs as ItemRewards
        foreach (var itemId in ItemIds)
        {
            if (!items.Any(i => i.ItemId == itemId))
            {
                items.Add(new ItemReward
                {
                    ItemId = itemId,
                    Quantity = 1
                });
            }
        }

        return items;
    }
}

/// <summary>
/// v0.14: Detailed item reward with quantity and optional quality
/// </summary>
public class ItemReward
{
    public string ItemId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public QualityTier? ForcedQuality { get; set; } = null; // Force specific quality tier

    /// <summary>
    /// Gets display string for this item reward
    /// </summary>
    public string GetDisplayString()
    {
        var qualityPrefix = ForcedQuality.HasValue ? $"{ForcedQuality.Value} " : "";
        var quantityString = Quantity > 1 ? $"{Quantity}x " : "";
        return $"{quantityString}{qualityPrefix}{ItemId}";
    }
}
