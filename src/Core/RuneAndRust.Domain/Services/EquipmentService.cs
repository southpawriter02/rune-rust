using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Services;

/// <summary>
/// Service for managing player equipment operations.
/// </summary>
/// <remarks>
/// <para>Handles the logic for equipping and unequipping items, including validation,
/// inventory management, and slot swapping. This service coordinates between
/// the player's equipment slots and inventory.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
/// <item>Validate items are equippable</item>
/// <item>Handle automatic slot swapping</item>
/// <item>Manage inventory space during operations</item>
/// <item>Parse slot names from user input</item>
/// </list>
/// </remarks>
public class EquipmentService
{
    private readonly ILogger<EquipmentService> _logger;

    /// <summary>
    /// Creates a new EquipmentService instance.
    /// </summary>
    /// <param name="logger">The logger for equipment operations.</param>
    public EquipmentService(ILogger<EquipmentService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Attempts to equip an item from the player's inventory by name.
    /// </summary>
    /// <param name="player">The player equipping the item.</param>
    /// <param name="itemName">The name of the item to equip (case-insensitive).</param>
    /// <returns>The result of the equip operation.</returns>
    /// <remarks>
    /// If the target slot is occupied, the existing item is automatically
    /// swapped back to inventory (if space available).
    /// </remarks>
    public EquipResult TryEquipByName(Player player, string itemName)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);

        _logger.LogDebug("Attempting to equip '{ItemName}' for player {Player}",
            itemName, player.Name);

        // Find item in inventory
        var item = player.Inventory.GetByName(itemName);
        if (item == null)
        {
            _logger.LogDebug("Item '{ItemName}' not found in inventory", itemName);
            return EquipResult.ItemNotFound(itemName);
        }

        return TryEquip(player, item);
    }

    /// <summary>
    /// Attempts to equip a specific item from the player's inventory.
    /// </summary>
    /// <param name="player">The player equipping the item.</param>
    /// <param name="item">The item to equip.</param>
    /// <returns>The result of the equip operation.</returns>
    public EquipResult TryEquip(Player player, Item item)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(item);

        _logger.LogDebug("Equipping '{ItemName}' for player {Player}",
            item.Name, player.Name);

        // Check if item is equippable
        if (!item.IsEquippable)
        {
            _logger.LogDebug("Item '{ItemName}' is not equippable", item.Name);
            return EquipResult.NotEquippable(item);
        }

        var slot = item.EquipmentSlot!.Value;
        Item? replacedItem = null;

        // Check if slot is occupied
        if (player.IsSlotOccupied(slot))
        {
            // Need to unequip current item first
            var currentItem = player.GetEquippedItem(slot)!;

            // Check if inventory has space for the current item
            if (player.Inventory.IsFull)
            {
                _logger.LogDebug("Cannot swap equipment - inventory is full");
                return EquipResult.InventoryFull();
            }

            // Unequip current item and add to inventory
            player.Unequip(slot);
            player.Inventory.TryAdd(currentItem);
            replacedItem = currentItem;

            _logger.LogDebug("Unequipped '{OldItem}' to make room for '{NewItem}'",
                currentItem.Name, item.Name);
        }

        // Remove item from inventory and equip it
        player.Inventory.Remove(item);
        player.TryEquip(item);

        _logger.LogInformation("Player {Player} equipped '{Item}' to {Slot} slot",
            player.Name, item.Name, slot);

        return EquipResult.Equipped(item, replacedItem);
    }

    /// <summary>
    /// Attempts to unequip an item from the specified slot.
    /// </summary>
    /// <param name="player">The player unequipping the item.</param>
    /// <param name="slot">The slot to unequip from.</param>
    /// <returns>The result of the unequip operation.</returns>
    public EquipResult TryUnequip(Player player, EquipmentSlot slot)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug("Attempting to unequip from {Slot} slot for player {Player}",
            slot, player.Name);

        // Check if slot has an item
        if (!player.IsSlotOccupied(slot))
        {
            _logger.LogDebug("Slot {Slot} is empty", slot);
            return EquipResult.SlotEmpty(slot);
        }

        // Check inventory space
        if (player.Inventory.IsFull)
        {
            _logger.LogDebug("Cannot unequip - inventory is full");
            return EquipResult.InventoryFull();
        }

        // Unequip and add to inventory
        var item = player.Unequip(slot)!;
        player.Inventory.TryAdd(item);

        _logger.LogInformation("Player {Player} unequipped '{Item}' from {Slot} slot",
            player.Name, item.Name, slot);

        return EquipResult.Unequipped(item);
    }

    /// <summary>
    /// Parses a slot name string to an EquipmentSlot enum value.
    /// </summary>
    /// <param name="slotName">The slot name to parse (case-insensitive).</param>
    /// <param name="slot">The parsed slot value.</param>
    /// <returns>True if parsing succeeded.</returns>
    public static bool TryParseSlot(string? slotName, out EquipmentSlot slot)
    {
        slot = default;

        if (string.IsNullOrWhiteSpace(slotName))
            return false;

        return Enum.TryParse(slotName.Trim(), ignoreCase: true, out slot);
    }

    /// <summary>
    /// Gets a formatted list of all valid slot names.
    /// </summary>
    /// <returns>Comma-separated list of slot names in lowercase.</returns>
    public static string GetValidSlotNames()
    {
        return string.Join(", ", Enum.GetNames<EquipmentSlot>().Select(n => n.ToLowerInvariant()));
    }
}
