namespace RuneAndRust.Core;

/// <summary>
/// Represents a recipe for crafting consumable items
/// </summary>
public class CraftingRecipe
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Crafting requirements
    public Dictionary<ComponentType, int> RequiredComponents { get; set; } = new();
    public int SkillCheckDC { get; set; } = 10; // Target number for WITS check
    public string SkillAttribute { get; set; } = "WITS"; // Attribute used for check

    // Results
    public string ResultItemName { get; set; } = string.Empty;
    public Func<CraftQuality, Consumable> CreateResult { get; set; } = null!;

    // Restrictions
    public bool RequiresBoneSetterSpecialization { get; set; } = false;

    /// <summary>
    /// Get formatted component requirements
    /// </summary>
    public string GetRequirementsDescription()
    {
        var requirements = RequiredComponents
            .Select(kvp => $"{kvp.Value}x {CraftingComponent.Create(kvp.Key).Name}")
            .ToArray();

        return string.Join(", ", requirements);
    }

    /// <summary>
    /// Check if player has required components
    /// </summary>
    public bool HasRequiredComponents(Dictionary<ComponentType, int> playerComponents)
    {
        foreach (var requirement in RequiredComponents)
        {
            if (!playerComponents.ContainsKey(requirement.Key) ||
                playerComponents[requirement.Key] < requirement.Value)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get list of missing components
    /// </summary>
    public List<string> GetMissingComponents(Dictionary<ComponentType, int> playerComponents)
    {
        var missing = new List<string>();

        foreach (var requirement in RequiredComponents)
        {
            int has = playerComponents.ContainsKey(requirement.Key) ? playerComponents[requirement.Key] : 0;
            int needed = requirement.Value;

            if (has < needed)
            {
                string componentName = CraftingComponent.Create(requirement.Key).Name;
                missing.Add($"{componentName} ({has}/{needed})");
            }
        }

        return missing;
    }
}
