namespace RuneAndRust.Core;

/// <summary>
/// v0.19.10: Runeforging recipe for Rúnasmiðr specialization
/// Unlike CraftingRecipe (which creates consumables), RuneforgeRecipe enchants existing equipment
/// </summary>
public class RuneforgeRecipe
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EnchantmentName { get; set; } = string.Empty; // e.g., "Bull's Strength", "Warding Rune"

    // Runeforging requirements
    public Dictionary<ComponentType, int> RequiredComponents { get; set; } = new();
    public int ForgingDC { get; set; } = 10; // Target number for WITS + WILL check
    public EquipmentType TargetType { get; set; } = EquipmentType.Weapon; // Weapon or Armor

    // Results
    public int BaseCharges { get; set; } = 2; // Number of charges on success
    public int MasterworkCharges { get; set; } = 3; // Number of charges on critical success (DC+5)
    public string ChargeEffect { get; set; } = string.Empty; // Description of what activating a charge does

    // Restrictions
    public bool RequiresRunasmidrSpecialization { get; set; } = true;

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
