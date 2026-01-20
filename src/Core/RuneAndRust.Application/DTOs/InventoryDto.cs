namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object representing a player's inventory for display.
/// </summary>
/// <param name="Items">The items currently in the inventory.</param>
/// <param name="Capacity">The maximum number of items the inventory can hold.</param>
/// <param name="Count">The current number of items in the inventory.</param>
/// <param name="IsFull">True if the inventory is at capacity; otherwise, false.</param>
public record InventoryDto(
    IReadOnlyList<ItemDto> Items,
    int Capacity,
    int Count,
    bool IsFull
);
