using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Base entity for all items in Rune &amp; Rust.
/// Uses Table-per-Hierarchy (TPH) inheritance - Equipment extends this class.
/// </summary>
public class Item
{
    #region Identity

    /// <summary>
    /// Gets or sets the unique identifier for this item.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display name of the item.
    /// Used for player commands (e.g., "take sword").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the functional category of the item.
    /// Determines UI placement and usage options.
    /// </summary>
    public ItemType ItemType { get; set; } = ItemType.Junk;

    #endregion

    #region Description

    /// <summary>
    /// Gets or sets the short description shown in inventory lists.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description shown when examining the item.
    /// </summary>
    public string? DetailedDescription { get; set; }

    #endregion

    #region Physical Properties

    /// <summary>
    /// Gets or sets the weight of the item in grams.
    /// Used for burden calculations.
    /// </summary>
    public int Weight { get; set; } = 0;

    /// <summary>
    /// Gets or sets the value in Scrip (currency).
    /// Used for trading and selling.
    /// </summary>
    public int Value { get; set; } = 0;

    #endregion

    #region Quality

    /// <summary>
    /// Gets or sets the craftsmanship quality tier.
    /// Affects stats and rarity.
    /// </summary>
    public QualityTier Quality { get; set; } = QualityTier.Scavenged;

    #endregion

    #region Stacking

    /// <summary>
    /// Gets or sets whether multiple instances can stack in inventory.
    /// </summary>
    public bool IsStackable { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number that can stack together.
    /// Only relevant if IsStackable is true.
    /// </summary>
    public int MaxStackSize { get; set; } = 1;

    #endregion

    #region Metadata

    /// <summary>
    /// Gets or sets the timestamp when this item was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this item was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    #endregion

    #region Enchantment Properties (v0.3.1c)

    /// <summary>
    /// Gets or sets the magical properties applied to this item through Runeforging.
    /// Empty by default - properties are added when items are enchanted.
    /// </summary>
    public List<ItemProperty> Properties { get; set; } = new();

    #endregion
}
