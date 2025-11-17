namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Represents a runic inscription that can be applied to equipment
/// </summary>
public class RunicInscription
{
    public int InscriptionId { get; set; }
    public string InscriptionName { get; set; } = string.Empty;
    public int InscriptionTier { get; set; }
    public string TargetEquipmentType { get; set; } = string.Empty; // Weapon, Armor, Both
    public string EffectType { get; set; } = string.Empty; // Elemental, Stat_Boost, Resistance, Status, Special
    public string EffectValue { get; set; } = string.Empty; // JSON-serialized effect data
    public bool IsTemporary { get; set; }
    public int UsesIfTemporary { get; set; }
    public string ComponentRequirements { get; set; } = string.Empty; // JSON-serialized component list
    public int CraftingCostCredits { get; set; }
    public string InscriptionDescription { get; set; } = string.Empty;
}
