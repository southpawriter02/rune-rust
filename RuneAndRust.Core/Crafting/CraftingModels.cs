using System.Text.Json;

namespace RuneAndRust.Core.Crafting;

/// <summary>
/// Represents a crafting recipe loaded from the database
/// </summary>
public class Recipe
{
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public string RecipeTier { get; set; } = string.Empty; // Basic, Advanced, Expert, Master
    public string CraftedItemType { get; set; } = string.Empty; // Weapon, Armor, Consumable, Inscription
    public string RequiredStation { get; set; } = string.Empty; // Forge, Workshop, Laboratory, Runic_Altar, Field_Station, Any
    public int QualityBonus { get; set; } = 0; // 0-2 bonus to final quality
    public int BaseValue { get; set; } = 0;
    public int CraftingTimeMinutes { get; set; } = 0;
    public string SkillAttribute { get; set; } = string.Empty; // WITS, BRAWN, etc.
    public int SkillCheckDC { get; set; } = 10;
    public string DiscoveryMethod { get; set; } = string.Empty; // Default, Merchant, Quest, Loot, Ability
    public string RecipeDescription { get; set; } = string.Empty;

    /// <summary>
    /// Components required for this recipe
    /// </summary>
    public List<RecipeComponent> RequiredComponents { get; set; } = new();

    /// <summary>
    /// Special effects granted by this recipe (JSON-serialized)
    /// </summary>
    public string? SpecialEffectsJson { get; set; }

    /// <summary>
    /// Parse special effects from JSON
    /// </summary>
    public Dictionary<string, object>? GetSpecialEffects()
    {
        if (string.IsNullOrWhiteSpace(SpecialEffectsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(SpecialEffectsJson);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Represents a component required for a recipe
/// </summary>
public class RecipeComponent
{
    public int ComponentId { get; set; }
    public int RecipeId { get; set; }
    public int ComponentItemId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public int QuantityRequired { get; set; } = 1;
    public int MinimumQuality { get; set; } = 1; // 1-5
    public bool IsOptional { get; set; } = false;
}

/// <summary>
/// Extended recipe information including discovery metadata
/// </summary>
public class RecipeDetails : Recipe
{
    public int TimesCrafted { get; set; }
    public DateTime? DiscoveredAt { get; set; }
    public string DiscoverySource { get; set; } = string.Empty;
}

/// <summary>
/// Represents a crafting station that can be used for crafting
/// </summary>
public class CraftingStation
{
    public int StationId { get; set; }
    public string StationType { get; set; } = string.Empty; // Forge, Workshop, Laboratory, Runic_Altar, Field_Station
    public string StationName { get; set; } = string.Empty;
    public int MaxQualityTier { get; set; } = 1; // 1-5: Maximum quality this station can produce
    public int? LocationSectorId { get; set; } // null for portable stations
    public int? LocationRoomId { get; set; }
    public bool RequiresControlling { get; set; } = false; // Must control the sector to use
    public int UsageCostCredits { get; set; } = 0;
    public string StationDescription { get; set; } = string.Empty;
}

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

/// <summary>
/// Represents a component that was consumed during crafting
/// </summary>
public class ConsumedComponent
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int QualityTier { get; set; }
}

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

/// <summary>
/// Result of a crafting operation using the Advanced Crafting System
/// </summary>
public class CraftedItemResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CraftedItemId { get; set; }
    public string? CraftedItemName { get; set; }
    public int FinalQuality { get; set; } = 1; // 1-4
    public int SkillCheckRoll { get; set; } = 0;
    public int SkillCheckDC { get; set; } = 0;
    public bool SkillCheckPassed { get; set; } = false;

    /// <summary>
    /// Components that were consumed in the crafting attempt
    /// </summary>
    public List<ConsumedComponent> ConsumedComponents { get; set; } = new();

    /// <summary>
    /// Calculation breakdown for transparency
    /// </summary>
    public QualityCalculation? QualityCalculation { get; set; }

    /// <summary>
    /// Create a failure result
    /// </summary>
    public static CraftedItemResult FailureResult(string message)
    {
        return new CraftedItemResult
        {
            Success = false,
            Message = message
        };
    }
}

/// <summary>
/// Breakdown of quality calculation for transparency
/// </summary>
public class QualityCalculation
{
    public int LowestComponentQuality { get; set; }
    public int StationMaxQuality { get; set; }
    public int RecipeQualityBonus { get; set; }
    public int BaseQuality { get; set; } // min(LowestComponentQuality, StationMaxQuality)
    public int FinalQuality { get; set; } // BaseQuality + RecipeBonus, clamped to 1-4

    public override string ToString()
    {
        return $"Quality Calculation: min({LowestComponentQuality}, {StationMaxQuality}) + {RecipeQualityBonus} = {FinalQuality}";
    }
}
