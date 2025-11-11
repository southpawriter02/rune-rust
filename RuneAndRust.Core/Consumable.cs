namespace RuneAndRust.Core;

/// <summary>
/// Types of consumable items
/// </summary>
public enum ConsumableType
{
    Medicine,      // Healing items (Bone-Setter specialty)
    Provision,     // Food, water (future)
    Tool,          // Single-use utility items (future)
    Ritual         // Special items (future)
}

/// <summary>
/// Quality tiers for crafted consumables
/// </summary>
public enum CraftQuality
{
    Standard,   // Normal success (meets DC)
    Masterwork  // Critical success (beats DC by 5+)
}

/// <summary>
/// Represents a consumable item (potions, medicines, etc.)
/// </summary>
public class Consumable
{
    // Identity
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ConsumableType Type { get; set; } = ConsumableType.Medicine;
    public CraftQuality Quality { get; set; } = CraftQuality.Standard;

    // Effects (for Field Medicine items)
    public int HPRestore { get; set; } = 0;           // Amount of HP restored
    public int StaminaRestore { get; set; } = 0;      // Amount of Stamina restored
    public int StressRestore { get; set; } = 0;       // Amount of Psychic Stress removed
    public bool ClearsBleeding { get; set; } = false; // Stops bleeding status
    public bool ClearsPoison { get; set; } = false;   // Cures poison status
    public bool ClearsDisease { get; set; } = false;  // Cures disease status
    public int TempHPGrant { get; set; } = 0;         // Grants temporary HP

    // Masterwork bonus effects
    public int MasterworkBonusHP { get; set; } = 0;   // Additional HP if Masterwork quality
    public int MasterworkBonusStamina { get; set; } = 0; // Additional Stamina if Masterwork

    /// <summary>
    /// Get formatted display name with quality indicator
    /// </summary>
    public string GetDisplayName()
    {
        if (Quality == CraftQuality.Masterwork)
            return $"{Name} ⭐"; // Star indicates masterwork
        return Name;
    }

    /// <summary>
    /// Get total HP restored (including masterwork bonus)
    /// </summary>
    public int GetTotalHPRestore()
    {
        if (Quality == CraftQuality.Masterwork)
            return HPRestore + MasterworkBonusHP;
        return HPRestore;
    }

    /// <summary>
    /// Get total Stamina restored (including masterwork bonus)
    /// </summary>
    public int GetTotalStaminaRestore()
    {
        if (Quality == CraftQuality.Masterwork)
            return StaminaRestore + MasterworkBonusStamina;
        return StaminaRestore;
    }

    /// <summary>
    /// Get formatted effects description
    /// </summary>
    public string GetEffectsDescription()
    {
        var effects = new List<string>();

        if (HPRestore > 0)
        {
            int total = GetTotalHPRestore();
            effects.Add($"Restore {total} HP");
        }

        if (StaminaRestore > 0)
        {
            int total = GetTotalStaminaRestore();
            effects.Add($"Restore {total} Stamina");
        }

        if (StressRestore > 0)
            effects.Add($"Reduce Stress by {StressRestore}");

        if (TempHPGrant > 0)
            effects.Add($"Grant {TempHPGrant} Temp HP");

        if (ClearsBleeding)
            effects.Add("Stop bleeding");

        if (ClearsPoison)
            effects.Add("Cure poison");

        if (ClearsDisease)
            effects.Add("Cure disease");

        if (effects.Count == 0)
            return "No effects";

        return string.Join(", ", effects);
    }
}
