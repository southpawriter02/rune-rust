using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of an equip or unequip operation.
/// </summary>
/// <remarks>
/// <para>Provides factory methods for common result scenarios:</para>
/// <list type="bullet">
/// <item>Equipped - Successfully equipped an item (optionally replacing another)</item>
/// <item>Unequipped - Successfully removed an item from a slot</item>
/// <item>Failure - Operation failed with a specific reason</item>
/// </list>
/// </remarks>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">A message describing the result.</param>
/// <param name="UnequippedItem">The item that was unequipped (if any).</param>
/// <param name="EquippedItem">The item that was equipped (if any).</param>
public readonly record struct EquipResult(
    bool Success,
    string Message,
    Item? UnequippedItem = null,
    Item? EquippedItem = null)
{
    /// <summary>
    /// Whether an item was swapped during this operation.
    /// </summary>
    public bool WasSwapped => UnequippedItem != null && EquippedItem != null;

    /// <summary>
    /// Creates a successful equip result.
    /// </summary>
    /// <param name="item">The item that was equipped.</param>
    /// <param name="replaced">The item that was replaced, if any.</param>
    /// <returns>A success result.</returns>
    public static EquipResult Equipped(Item item, Item? replaced = null) =>
        new(true, $"You equip the {item.Name}.", replaced, item);

    /// <summary>
    /// Creates a successful unequip result.
    /// </summary>
    /// <param name="item">The item that was unequipped.</param>
    /// <returns>A success result.</returns>
    public static EquipResult Unequipped(Item item) =>
        new(true, $"You unequip the {item.Name}.", item, null);

    /// <summary>
    /// Creates a failure result with the specified message.
    /// </summary>
    /// <param name="message">The failure reason.</param>
    /// <returns>A failure result.</returns>
    public static EquipResult Failure(string message) =>
        new(false, message, null, null);

    /// <summary>
    /// Creates a failure for item not found.
    /// </summary>
    /// <param name="itemName">The name that was searched for.</param>
    /// <returns>A failure result.</returns>
    public static EquipResult ItemNotFound(string itemName) =>
        Failure($"You don't have '{itemName}' in your inventory.");

    /// <summary>
    /// Creates a failure for item not equippable.
    /// </summary>
    /// <param name="item">The non-equippable item.</param>
    /// <returns>A failure result.</returns>
    public static EquipResult NotEquippable(Item item) =>
        Failure($"The {item.Name} cannot be equipped.");

    /// <summary>
    /// Creates a failure for slot empty.
    /// </summary>
    /// <param name="slot">The empty slot.</param>
    /// <returns>A failure result.</returns>
    public static EquipResult SlotEmpty(EquipmentSlot slot) =>
        Failure($"Nothing is equipped in the {slot} slot.");

    /// <summary>
    /// Creates a failure for inventory full (can't unequip).
    /// </summary>
    /// <returns>A failure result.</returns>
    public static EquipResult InventoryFull() =>
        Failure("Your inventory is full. Cannot unequip.");
}
