namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the generated loot from defeating a monster.
/// </summary>
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
    /// Dictionary of currency ID to amount.
    /// </remarks>
    public IReadOnlyDictionary<string, int> Currency { get; init; }

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
    /// Gets an empty loot drop.
    /// </summary>
    public static LootDrop Empty => new()
    {
        Items = [],
        Currency = new Dictionary<string, int>()
    };

    /// <summary>
    /// Creates a loot drop with the specified contents.
    /// </summary>
    /// <param name="items">The dropped items (optional).</param>
    /// <param name="currency">The dropped currency amounts (optional).</param>
    /// <returns>A new LootDrop instance.</returns>
    public static LootDrop Create(
        IEnumerable<DroppedItem>? items = null,
        IDictionary<string, int>? currency = null)
    {
        return new LootDrop
        {
            Items = items?.ToList() ?? [],
            Currency = currency != null
                ? new Dictionary<string, int>(currency, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
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

        return new LootDrop
        {
            Items = combinedItems,
            Currency = combinedCurrency
        };
    }
}
