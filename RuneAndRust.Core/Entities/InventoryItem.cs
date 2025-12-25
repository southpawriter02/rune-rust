namespace RuneAndRust.Core.Entities;

/// <summary>
/// Join entity representing an item in a character's inventory.
/// Supports stacking, slot positioning, and equipment state.
/// </summary>
/// <remarks>See: SPEC-INV-001 for Inventory & Equipment System design.</remarks>
public class InventoryItem
{
    #region Keys

    /// <summary>
    /// Gets or sets the ID of the character who owns this inventory entry.
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the item in this inventory slot.
    /// </summary>
    public Guid ItemId { get; set; }

    #endregion

    #region Stack Properties

    /// <summary>
    /// Gets or sets the quantity of items in this stack.
    /// For non-stackable items, this is always 1.
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Gets or sets the position in the inventory UI.
    /// Lower values appear first.
    /// </summary>
    public int SlotPosition { get; set; } = 0;

    #endregion

    #region Equipment State

    /// <summary>
    /// Gets or sets whether this item is currently equipped.
    /// Only valid for Equipment-type items.
    /// </summary>
    public bool IsEquipped { get; set; } = false;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the character who owns this inventory entry.
    /// </summary>
    public Character Character { get; set; } = null!;

    /// <summary>
    /// Gets or sets the item in this inventory slot.
    /// </summary>
    public Item Item { get; set; } = null!;

    #endregion

    #region Metadata

    /// <summary>
    /// Gets or sets the timestamp when this item was added to inventory.
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this entry was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    #endregion
}
