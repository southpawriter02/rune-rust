// ═══════════════════════════════════════════════════════════════════════════════
// EquipmentPreview.cs
// Preview of starting equipment based on background selection during character
// creation. Contains a list of equipment items with display names, quantities,
// and item types derived from the BackgroundEquipmentGrant configuration.
// Used by the ViewModelBuilder for the TUI equipment display panel.
// Version: 0.17.5b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Preview of starting equipment based on background selection.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EquipmentPreview"/> is a read-only presentation value object that lists
/// the equipment items a character will receive at creation based on their selected
/// background. It provides display-formatted item names and types suitable for the
/// TUI equipment panel.
/// </para>
/// <para>
/// The preview is populated when the player selects a background in Step 2. Each
/// background grants a specific set of equipment — for example, Village Smith grants
/// a Smith's Hammer and Leather Apron, while Clan Guard grants a Shield and Spear.
/// </para>
/// <para>
/// <strong>Data Transformation:</strong> The underlying <see cref="BackgroundEquipmentGrant"/>
/// uses kebab-case <c>ItemId</c> values (e.g., "smiths-hammer"). The ViewModelBuilder
/// transforms these into display-friendly names (e.g., "Smiths Hammer") stored in
/// <see cref="EquipmentPreviewItem.Name"/>. Item types are derived from the grant's
/// <c>IsEquipped</c>/<c>Slot</c>/<c>Quantity</c> properties since
/// <see cref="BackgroundEquipmentGrant"/> does not have a direct <c>ItemType</c> property.
/// </para>
/// </remarks>
/// <seealso cref="CharacterCreationViewModel"/>
/// <seealso cref="BackgroundEquipmentGrant"/>
/// <seealso cref="RuneAndRust.Domain.Entities.BackgroundDefinition"/>
public readonly record struct EquipmentPreview
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the list of equipment items granted by the background.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="EquipmentPreviewItem"/> instances, each
    /// containing a display name, quantity, and item type. Empty until a
    /// background is selected (Step 2).
    /// </value>
    /// <remarks>
    /// <para>
    /// Each background grants 2-4 items. Items may be auto-equipped (weapons,
    /// armor) or placed in inventory (consumables, tools).
    /// </para>
    /// </remarks>
    public IReadOnlyList<EquipmentPreviewItem> Items { get; init; }

    /// <summary>
    /// Gets the display name of the background that grants this equipment.
    /// </summary>
    /// <value>
    /// The human-readable background name (e.g., "Village Smith", "Clan Guard").
    /// Empty string when no background is selected.
    /// </value>
    public string FromBackground { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of equipment items.
    /// </summary>
    /// <value>
    /// The count of items in <see cref="Items"/>. Zero for <see cref="Empty"/>.
    /// </value>
    public int ItemCount => Items?.Count ?? 0;

    /// <summary>
    /// Gets whether this preview contains any equipment items.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Items"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasItems => ItemCount > 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an empty preview with no equipment.
    /// </summary>
    /// <value>
    /// An <see cref="EquipmentPreview"/> with an empty items list and
    /// empty background name.
    /// </value>
    /// <remarks>
    /// Returned by the ViewModelBuilder when no background has been selected yet.
    /// </remarks>
    public static EquipmentPreview Empty => new()
    {
        Items = Array.Empty<EquipmentPreviewItem>(),
        FromBackground = string.Empty
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted summary of the equipment for display.
    /// </summary>
    /// <returns>
    /// A comma-separated string of item names with quantities where applicable,
    /// e.g., "Smith's Hammer, Leather Apron" or "Rations ×3, Cloak".
    /// Returns "No equipment preview available" for <see cref="Empty"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var preview = new EquipmentPreview
    /// {
    ///     Items = new[]
    ///     {
    ///         new EquipmentPreviewItem { Name = "Shield", Quantity = 1, ItemType = "MainHand" },
    ///         new EquipmentPreviewItem { Name = "Spear", Quantity = 1, ItemType = "OffHand" }
    ///     },
    ///     FromBackground = "Clan Guard"
    /// };
    /// preview.GetSummary(); // "Shield, Spear"
    /// </code>
    /// </example>
    public string GetSummary()
    {
        if (!HasItems)
            return "No equipment preview available";

        return string.Join(", ", Items.Select(item =>
            item.Quantity > 1 ? $"{item.Name} ×{item.Quantity}" : item.Name));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of the preview for debugging.
    /// </summary>
    /// <returns>
    /// A string in the format "EquipmentPreview [From: Village Smith, Items: 2]".
    /// </returns>
    public override string ToString() =>
        $"EquipmentPreview [From: {FromBackground}, Items: {ItemCount}]";
}

/// <summary>
/// Single equipment item in the equipment preview.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EquipmentPreviewItem"/> represents a single item in the starting
/// equipment list. The <see cref="Name"/> is a display-formatted version of the
/// underlying <see cref="BackgroundEquipmentGrant.ItemId"/> (kebab-case converted
/// to title case). The <see cref="ItemType"/> is derived from the grant's
/// equipment slot and equipped status.
/// </para>
/// </remarks>
/// <seealso cref="EquipmentPreview"/>
/// <seealso cref="BackgroundEquipmentGrant"/>
public readonly record struct EquipmentPreviewItem
{
    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    /// <value>
    /// A human-readable item name derived from the kebab-case ItemId.
    /// For example, "smiths-hammer" becomes "Smiths Hammer".
    /// </value>
    public string Name { get; init; }

    /// <summary>
    /// Gets the quantity of this item (for stackable items like potions or rations).
    /// </summary>
    /// <value>
    /// The number of items to grant. Typically 1 for weapons and armor,
    /// higher for consumables (e.g., Bandages ×5, Rations ×3).
    /// </value>
    public int Quantity { get; init; }

    /// <summary>
    /// Gets the item type for categorization display.
    /// </summary>
    /// <value>
    /// A string describing the item category. Derived from the equipment grant:
    /// equipped items show their slot name (e.g., "MainHand", "Body"),
    /// multi-quantity items show "Consumable", and single non-equipped items
    /// show "Item".
    /// </value>
    /// <remarks>
    /// <para>
    /// Since <see cref="BackgroundEquipmentGrant"/> does not have a direct
    /// <c>ItemType</c> property, the ViewModelBuilder derives the type:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Equipped items with a slot → slot name (e.g., "MainHand", "Body")</description></item>
    ///   <item><description>Non-equipped items with Quantity &gt; 1 → "Consumable"</description></item>
    ///   <item><description>Non-equipped items with Quantity == 1 → "Item"</description></item>
    /// </list>
    /// </remarks>
    public string ItemType { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation for debugging.
    /// </summary>
    /// <returns>
    /// A string in the format "ItemName x1 (Type)" or "ItemName x3 (Consumable)".
    /// </returns>
    public override string ToString() =>
        $"{Name} x{Quantity} ({ItemType})";
}
