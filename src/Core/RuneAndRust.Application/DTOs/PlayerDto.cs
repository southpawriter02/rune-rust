namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object representing a player character's state for display.
/// </summary>
/// <param name="Id">The unique identifier of the player.</param>
/// <param name="Name">The player's display name.</param>
/// <param name="Health">The player's current health points.</param>
/// <param name="MaxHealth">The player's maximum health points.</param>
/// <param name="Attack">The player's attack stat.</param>
/// <param name="Defense">The player's defense stat.</param>
/// <param name="InventoryCount">The number of items in the player's inventory.</param>
/// <param name="InventoryCapacity">The maximum number of items the inventory can hold.</param>
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
