using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for displaying loot drop information to the player.
/// </summary>
public record LootDropDto
{
    /// <summary>
    /// Gets the items that dropped.
    /// </summary>
    public IReadOnlyList<DroppedItemDto> Items { get; init; } = [];

    /// <summary>
    /// Gets the currency amounts that dropped.
    /// </summary>
    public IReadOnlyList<CurrencyAmountDto> Currency { get; init; } = [];

    /// <summary>
    /// Gets whether this loot drop is empty.
    /// </summary>
    public bool IsEmpty => Items.Count == 0 && Currency.Count == 0;

    /// <summary>
    /// Creates a LootDropDto from a domain LootDrop.
    /// </summary>
    /// <param name="loot">The loot drop to convert.</param>
    /// <returns>A new LootDropDto.</returns>
    public static LootDropDto FromDomain(LootDrop loot)
    {
        var items = loot.Items?
            .Select(i => new DroppedItemDto
            {
                ItemId = i.ItemId,
                Name = i.Name,
                Quantity = i.Quantity
            })
            .ToList() ?? [];

        var currency = loot.Currency?
            .Select(kvp => new CurrencyAmountDto
            {
                CurrencyId = kvp.Key,
                Amount = kvp.Value
            })
            .ToList() ?? [];

        return new LootDropDto
        {
            Items = items,
            Currency = currency
        };
    }

    /// <summary>
    /// Gets an empty loot drop DTO.
    /// </summary>
    public static LootDropDto Empty => new();
}

/// <summary>
/// DTO for a single dropped item.
/// </summary>
public record DroppedItemDto
{
    /// <summary>
    /// Gets the ID of the dropped item.
    /// </summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the quantity dropped.
    /// </summary>
    public int Quantity { get; init; }
}

/// <summary>
/// DTO for a currency amount.
/// </summary>
public record CurrencyAmountDto
{
    /// <summary>
    /// Gets the currency ID.
    /// </summary>
    public string CurrencyId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the amount of currency.
    /// </summary>
    public int Amount { get; init; }
}
