namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// View model representing an equipment slot.
/// </summary>
/// <remarks>
/// Represents a single equipment slot showing the slot type, icon,
/// equipped item name, and any bonus from the equipped item.
/// </remarks>
public record EquipmentSlotViewModel
{
    /// <summary>
    /// Gets the slot name (e.g., "Weapon", "Armor").
    /// </summary>
    public string SlotName { get; }
    
    /// <summary>
    /// Gets the slot icon (emoji).
    /// </summary>
    public string SlotIcon { get; }
    
    /// <summary>
    /// Gets the equipped item name or "None".
    /// </summary>
    public string ItemName { get; }
    
    /// <summary>
    /// Gets the item bonus text (e.g., "+4 atk").
    /// </summary>
    public string? ItemBonus { get; }
    
    /// <summary>
    /// Gets whether this slot is empty.
    /// </summary>
    public bool IsEmpty { get; }
    
    /// <summary>
    /// Gets whether the item has a bonus.
    /// </summary>
    public bool HasBonus { get; }

    /// <summary>
    /// Creates an empty equipment slot.
    /// </summary>
    /// <param name="slotName">The slot name.</param>
    /// <param name="slotIcon">The slot icon.</param>
    public EquipmentSlotViewModel(string slotName, string slotIcon)
    {
        SlotName = slotName;
        SlotIcon = slotIcon;
        ItemName = "None";
        ItemBonus = null;
        IsEmpty = true;
        HasBonus = false;
    }

    /// <summary>
    /// Creates an equipment slot with an item.
    /// </summary>
    /// <param name="slotName">The slot name.</param>
    /// <param name="slotIcon">The slot icon.</param>
    /// <param name="itemName">The item name or null if empty.</param>
    /// <param name="itemBonus">The item bonus text.</param>
    public EquipmentSlotViewModel(string slotName, string slotIcon, string? itemName, string? itemBonus)
    {
        SlotName = slotName;
        SlotIcon = slotIcon;
        ItemName = itemName ?? "None";
        ItemBonus = itemBonus;
        IsEmpty = itemName is null;
        HasBonus = !string.IsNullOrEmpty(itemBonus);
    }
}
