namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single dropped item with quantity.
/// </summary>
public readonly record struct DroppedItem
{
    /// <summary>
    /// Gets the ID of the dropped item.
    /// </summary>
    public string ItemId { get; init; }

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the quantity dropped.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Creates a DroppedItem.
    /// </summary>
    /// <param name="itemId">The ID of the dropped item.</param>
    /// <param name="name">The display name of the item.</param>
    /// <param name="quantity">The quantity dropped (default: 1).</param>
    /// <returns>A new DroppedItem instance.</returns>
    public static DroppedItem Create(string itemId, string name, int quantity = 1)
    {
        return new DroppedItem
        {
            ItemId = itemId,
            Name = name,
            Quantity = Math.Max(1, quantity)
        };
    }
}
