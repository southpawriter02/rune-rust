namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Represents a component in a player's inventory
/// </summary>
public class PlayerComponent
{
    public int InventoryId { get; set; }
    public int CharacterId { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public int QualityTier { get; set; } = 1; // 1-5
    public string ItemType { get; set; } = string.Empty; // Component
}
