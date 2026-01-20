using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object representing an item for display.
/// </summary>
/// <param name="Id">The unique identifier of the item.</param>
/// <param name="Name">The item's display name.</param>
/// <param name="Description">A description of the item.</param>
/// <param name="Type">The category of item (Weapon, Armor, Consumable, etc.).</param>
/// <param name="Value">The item's value in gold or other currency.</param>
public record ItemDto(
    Guid Id,
    string Name,
    string Description,
    ItemType Type,
    int Value
);
