using System.Text.Json;

namespace RuneAndRust.Engine.Crafting;

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
