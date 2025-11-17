namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Represents an active modification applied to equipment
/// </summary>
public class EquipmentModification
{
    public int ModificationId { get; set; }
    public int EquipmentItemId { get; set; } // FK to Character_Inventory.inventory_id
    public string ModificationType { get; set; } = string.Empty; // Elemental, Stat_Boost, Resistance, Status, Special
    public string ModificationName { get; set; } = string.Empty;
    public string ModificationValue { get; set; } = string.Empty; // JSON-serialized effect data
    public bool IsPermanent { get; set; }
    public int? RemainingUses { get; set; } // Null for permanent, > 0 for temporary
    public DateTime AppliedAt { get; set; }
    public int? AppliedByRecipeId { get; set; } // Optional: track which recipe applied this
}
