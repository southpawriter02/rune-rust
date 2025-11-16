namespace RuneAndRust.Core;

/// <summary>
/// v0.34.2: Represents an ability that a companion can use
/// Abilities are defined in the database and loaded per companion
/// </summary>
public class CompanionAbility
{
    public int AbilityID { get; set; }
    public string AbilityName { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty; // Companion name
    public string Description { get; set; } = string.Empty;

    // Resource Cost
    public string? ResourceCostType { get; set; } // "Stamina", "Aether Pool", null for passive
    public int ResourceCost { get; set; } = 0;

    // Targeting
    public string TargetType { get; set; } = "single_target"; // "single_target", "area_2x2", "self", "all_allies"
    public string RangeType { get; set; } = "melee"; // "melee", "ranged", "passive", "self"
    public int RangeTiles { get; set; } = 0; // 0 for melee/self, >0 for ranged

    // Damage/Healing
    public string? DamageType { get; set; } // "Physical", "Magic", "Psychic", "Healing", null
    public int DurationTurns { get; set; } = 0; // For buffs/debuffs

    // Special Properties
    public string? SpecialEffects { get; set; } // Additional effect description
    public string? Conditions { get; set; } // Conditions applied (Stun, Fear, etc.)

    // Ability Type
    public string AbilityCategory { get; set; } = "companion_ability";

    // Derived Properties (for AI evaluation)
    public bool IsAOE => TargetType.Contains("area") || TargetType == "all_allies";
    public bool IsPassive => RangeType == "passive";
    public bool IsHeal => DamageType == "Healing";
    public bool IsBuff => AbilityCategory.Contains("Buff") || SpecialEffects?.Contains("buff") == true;
    public double DamageMultiplier
    {
        get
        {
            // Parse damage from description (simplified)
            // e.g., "3d6" = 3.0 multiplier, "2d6" = 2.0
            if (Description.Contains("3d6"))
                return 3.0;
            if (Description.Contains("2d6"))
                return 2.0;
            if (Description.Contains("1d8"))
                return 1.5;
            return 1.0;
        }
    }

    public int AOERadius
    {
        get
        {
            // Parse AOE radius from target type
            // e.g., "area_2x2" = radius 2
            if (TargetType.Contains("2x2"))
                return 2;
            if (TargetType.Contains("3x3"))
                return 3;
            return 0;
        }
    }

    // Stamina/Aether costs for specific resource types
    public int StaminaCost => ResourceCostType == "Stamina" ? ResourceCost : 0;
    public int AetherCost => ResourceCostType == "Aether Pool" ? ResourceCost : 0;
}
