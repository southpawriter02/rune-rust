namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Represents an equipment item in player inventory
/// </summary>
public class EquipmentItem
{
    public int InventoryId { get; set; } // Character_Inventory.inventory_id
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty; // Weapon, Armor
    public int QualityTier { get; set; }
}
