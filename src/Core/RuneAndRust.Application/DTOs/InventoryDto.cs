namespace RuneAndRust.Application.DTOs;

public record InventoryDto(
    IReadOnlyList<ItemDto> Items,
    int Capacity,
    int Count,
    bool IsFull
);
