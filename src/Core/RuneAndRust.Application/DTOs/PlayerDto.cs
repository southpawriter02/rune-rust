namespace RuneAndRust.Application.DTOs;

public record PlayerDto(
    Guid Id,
    string Name,
    int Health,
    int MaxHealth,
    int Attack,
    int Defense,
    int InventoryCount,
    int InventoryCapacity
);
