using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for displaying an equipped item in a specific slot.
/// </summary>
/// <param name="Slot">The equipment slot.</param>
/// <param name="ItemName">The name of the equipped item, or null if empty.</param>
/// <param name="ItemDescription">The description of the equipped item, or null if empty.</param>
public record EquippedItemDto(
    EquipmentSlot Slot,
    string? ItemName,
    string? ItemDescription)
{
    /// <summary>
    /// Whether this slot has an item equipped.
    /// </summary>
    public bool IsOccupied => ItemName != null;

    /// <summary>
    /// Gets the display name for the slot.
    /// </summary>
    public string SlotDisplayName => Slot.ToString();
}

/// <summary>
/// DTO for displaying all equipment slots.
/// </summary>
/// <param name="Slots">The list of all equipment slots with their items.</param>
public record EquipmentSlotsDto(IReadOnlyList<EquippedItemDto> Slots)
{
    /// <summary>
    /// Creates an EquipmentSlotsDto from a player's equipment.
    /// </summary>
    /// <param name="player">The player whose equipment to display.</param>
    /// <returns>A DTO with all slots.</returns>
    public static EquipmentSlotsDto FromPlayer(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var slots = Enum.GetValues<EquipmentSlot>()
            .Select(slot =>
            {
                var item = player.GetEquippedItem(slot);
                return new EquippedItemDto(
                    slot,
                    item?.Name,
                    item?.Description);
            })
            .ToList();

        return new EquipmentSlotsDto(slots);
    }

    /// <summary>
    /// Gets the number of occupied slots.
    /// </summary>
    public int OccupiedSlotCount => Slots.Count(s => s.IsOccupied);

    /// <summary>
    /// Gets the total number of slots.
    /// </summary>
    public int TotalSlotCount => Slots.Count;
}

/// <summary>
/// DTO for displaying an equip/unequip result.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">The result message.</param>
/// <param name="ReplacedItemName">Name of replaced item if swapped.</param>
public record EquipResultDto(
    bool Success,
    string Message,
    string? ReplacedItemName = null)
{
    /// <summary>
    /// Whether an item was swapped during this operation.
    /// </summary>
    public bool WasSwapped => ReplacedItemName != null;

    /// <summary>
    /// Creates a DTO from an EquipResult.
    /// </summary>
    /// <param name="result">The domain result.</param>
    /// <returns>A DTO for rendering.</returns>
    public static EquipResultDto FromResult(EquipResult result) =>
        new(result.Success, result.Message, result.UnequippedItem?.Name);
}
