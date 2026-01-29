// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundEquipmentGrant.cs
// Value object representing starting equipment granted by a background during
// character creation. Each background provides profession-appropriate items
// that may be auto-equipped or placed in inventory.
// Version: 0.17.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents starting equipment granted by a background.
/// </summary>
/// <remarks>
/// <para>
/// BackgroundEquipmentGrant specifies an item to add to the character's
/// inventory at creation time. Items are identified by lowercase string
/// IDs to support configuration-driven item systems.
/// </para>
/// <para>
/// Grants can optionally specify auto-equip behavior:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <see cref="IsEquipped"/>: If true, the item is equipped during creation
///   </description></item>
///   <item><description>
///     <see cref="Slot"/>: The equipment slot to equip to (required if IsEquipped is true)
///   </description></item>
/// </list>
/// <para>
/// Quantity supports stackable items like bandages (x5) or rations (x3).
/// Equipment grants are defined in configuration (backgrounds.json) and
/// applied during character creation by the IBackgroundApplicationService (v0.17.1e).
/// </para>
/// </remarks>
/// <param name="ItemId">The lowercase item identifier (e.g., "smiths-hammer", "healers-kit").</param>
/// <param name="Quantity">Number of items to grant (default 1).</param>
/// <param name="IsEquipped">Whether to auto-equip the item at creation.</param>
/// <param name="Slot">Target equipment slot if auto-equipped.</param>
/// <seealso cref="EquipmentSlot"/>
/// <seealso cref="RuneAndRust.Domain.Entities.BackgroundDefinition"/>
public readonly record struct BackgroundEquipmentGrant(
    string ItemId,
    int Quantity,
    bool IsEquipped,
    EquipmentSlot? Slot)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during grant creation.
    /// </summary>
    private static ILogger<BackgroundEquipmentGrant>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this grant specifies auto-equip behavior.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="IsEquipped"/> is true and <see cref="Slot"/>
    /// has a value; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Auto-equip items are placed directly into equipment slots during
    /// character creation. Combat backgrounds (Village Smith, Clan Guard)
    /// typically auto-equip weapons and armor.
    /// </remarks>
    public bool HasAutoEquip => IsEquipped && Slot.HasValue;

    /// <summary>
    /// Gets whether this is a stackable item (quantity greater than 1).
    /// </summary>
    /// <value>
    /// <c>true</c> if the quantity is greater than 1; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Stackable items include consumables like bandages (x5) and rations (x3).
    /// Non-stackable items (tools, weapons, armor) have a quantity of 1.
    /// </remarks>
    public bool IsStackable => Quantity > 1;

    /// <summary>
    /// Gets whether this grant is for inventory only (not equipped).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="IsEquipped"/> is false; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Inventory-only items are placed in the character's inventory without
    /// being equipped. This includes consumables, tools, and utility items
    /// like healer's kits, lockpicks, and journals.
    /// </remarks>
    public bool IsInventoryOnly => !IsEquipped;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new BackgroundEquipmentGrant with validation.
    /// </summary>
    /// <param name="itemId">The item identifier (will be normalized to lowercase).</param>
    /// <param name="quantity">Number of items to grant (must be at least 1).</param>
    /// <param name="isEquipped">Whether to auto-equip the item at creation.</param>
    /// <param name="slot">Target equipment slot if equipped (required when isEquipped is true).</param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="BackgroundEquipmentGrant"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="quantity"/> is less than 1.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="isEquipped"/> is true but <paramref name="slot"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create an equipped weapon grant for Clan Guard
    /// var spear = BackgroundEquipmentGrant.Create("spear", 1, true, EquipmentSlot.Weapon);
    ///
    /// // Create an inventory-only consumable grant
    /// var bandages = BackgroundEquipmentGrant.Create("bandages", 5);
    /// </code>
    /// </example>
    public static BackgroundEquipmentGrant Create(
        string itemId,
        int quantity = 1,
        bool isEquipped = false,
        EquipmentSlot? slot = null,
        ILogger<BackgroundEquipmentGrant>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating BackgroundEquipmentGrant. ItemId='{ItemId}', Quantity={Quantity}, " +
            "IsEquipped={IsEquipped}, Slot={Slot}",
            itemId,
            quantity,
            isEquipped,
            slot?.ToString() ?? "null");

        // Validate required parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId, nameof(itemId));
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1, nameof(quantity));

        if (isEquipped && !slot.HasValue)
        {
            _logger?.LogWarning(
                "BackgroundEquipmentGrant creation failed: IsEquipped is true but Slot is null. " +
                "ItemId='{ItemId}'",
                itemId);

            throw new ArgumentException(
                "Slot must be specified when IsEquipped is true.",
                nameof(slot));
        }

        var normalizedItemId = itemId.ToLowerInvariant();

        _logger?.LogDebug(
            "Validation passed for BackgroundEquipmentGrant. " +
            "NormalizedItemId='{NormalizedItemId}', Quantity={Quantity}, " +
            "IsEquipped={IsEquipped}, Slot={Slot}",
            normalizedItemId,
            quantity,
            isEquipped,
            slot?.ToString() ?? "null");

        var grant = new BackgroundEquipmentGrant(
            normalizedItemId,
            quantity,
            isEquipped,
            slot);

        _logger?.LogInformation(
            "Created BackgroundEquipmentGrant: {Grant}",
            grant);

        return grant;
    }

    /// <summary>
    /// Creates an equipment grant for an item to be auto-equipped.
    /// </summary>
    /// <param name="itemId">The item identifier (will be normalized to lowercase).</param>
    /// <param name="slot">The equipment slot to equip the item to.</param>
    /// <returns>
    /// A new <see cref="BackgroundEquipmentGrant"/> with quantity 1, IsEquipped true,
    /// and the specified slot.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="itemId"/> is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// var shield = BackgroundEquipmentGrant.Equipped("shield", EquipmentSlot.Shield);
    /// var spear = BackgroundEquipmentGrant.Equipped("spear", EquipmentSlot.Weapon);
    /// </code>
    /// </example>
    public static BackgroundEquipmentGrant Equipped(
        string itemId,
        EquipmentSlot slot) =>
        Create(itemId, 1, true, slot);

    /// <summary>
    /// Creates an equipment grant for an inventory-only item.
    /// </summary>
    /// <param name="itemId">The item identifier (will be normalized to lowercase).</param>
    /// <param name="quantity">Number of items to grant (default 1).</param>
    /// <returns>
    /// A new <see cref="BackgroundEquipmentGrant"/> with IsEquipped false and no slot.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="itemId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quantity"/> is less than 1.</exception>
    /// <example>
    /// <code>
    /// var kit = BackgroundEquipmentGrant.Inventory("healers-kit");
    /// var lockpicks = BackgroundEquipmentGrant.Inventory("lockpicks");
    /// </code>
    /// </example>
    public static BackgroundEquipmentGrant Inventory(
        string itemId,
        int quantity = 1) =>
        Create(itemId, quantity, false, null);

    /// <summary>
    /// Creates an equipment grant for a consumable with quantity.
    /// </summary>
    /// <param name="itemId">The item identifier (will be normalized to lowercase).</param>
    /// <param name="quantity">Number of consumable items to grant.</param>
    /// <returns>
    /// A new <see cref="BackgroundEquipmentGrant"/> with the specified quantity,
    /// IsEquipped false, and no slot.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="itemId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quantity"/> is less than 1.</exception>
    /// <example>
    /// <code>
    /// var bandages = BackgroundEquipmentGrant.Consumable("bandages", 5);
    /// var rations = BackgroundEquipmentGrant.Consumable("rations", 3);
    /// </code>
    /// </example>
    public static BackgroundEquipmentGrant Consumable(
        string itemId,
        int quantity) =>
        Create(itemId, quantity, false, null);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this equipment grant.
    /// </summary>
    /// <returns>
    /// A string in the format "itemId" for single items,
    /// "itemId xN" for stackable items, or "itemId (SlotName)" for equipped items.
    /// </returns>
    /// <example>
    /// <code>
    /// BackgroundEquipmentGrant.Equipped("spear", EquipmentSlot.Weapon).ToString();  // "spear (Weapon)"
    /// BackgroundEquipmentGrant.Consumable("bandages", 5).ToString();                // "bandages x5"
    /// BackgroundEquipmentGrant.Inventory("lockpicks").ToString();                   // "lockpicks"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var quantityStr = Quantity > 1 ? $" x{Quantity}" : "";
        var slotStr = IsEquipped && Slot.HasValue ? $" ({Slot.Value})" : "";
        return $"{ItemId}{quantityStr}{slotStr}";
    }
}
