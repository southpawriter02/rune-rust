namespace RuneAndRust.Core;

/// <summary>
/// Quality tiers for equipment (Aethelgard Saga System)
/// </summary>
public enum QualityTier
{
    JuryRigged = 0,    // Scrap held together with hope and wire
    Scavenged = 1,     // Salvaged from ruins, functional but worn
    ClanForged = 2,    // Crafted by survivor communities, solid work
    Optimized = 3,     // Pre-Glitch tech, carefully maintained
    MythForged = 4     // Legendary artifacts, touched by the Blight or ancient craft
}

/// <summary>
/// Types of equipment
/// </summary>
public enum EquipmentType
{
    Weapon,
    Armor,
    Accessory  // v0.16: Accessories (rings, amulets, trinkets)
}

/// <summary>
/// Weapon types for different classes
/// </summary>
public enum WeaponCategory
{
    Axe,          // Warrior - MIGHT-based
    Greatsword,   // Warrior - MIGHT-based, two-handed
    Spear,        // Scavenger - FINESSE-based, reach
    Dagger,       // Scavenger - FINESSE-based, fast
    Staff,        // Mystic - WILL-based, Aether cost reduction
    Focus,        // Mystic - WILL-based, ability enhancement

    // v0.16 new weapon types
    Blade,        // One-handed blade (machete, heretical blade, rust-eater)
    Blunt,        // One-handed blunt (cudgel, hammer)
    EnergyMelee,  // Energy melee weapon (shock-baton, plasma cutter)
    Rifle,        // Ranged firearm (sentinel's rifle, arc-cannon)
    HeavyBlunt    // Two-handed blunt (thunder hammer)
}

/// <summary>
/// Armor types with different stat tradeoffs
/// </summary>
public enum ArmorCategory
{
    Light,   // Low HP, low defense, +FINESSE, +Evasion
    Medium,  // Balanced stats
    Heavy    // High HP, high defense, -FINESSE, +STURDINESS
}

/// <summary>
/// Bonus effects that equipment can provide
/// </summary>
public class EquipmentBonus
{
    public string AttributeName { get; set; } = string.Empty; // "MIGHT", "FINESSE", etc.
    public int BonusValue { get; set; } = 0;
    public string Description { get; set; } = string.Empty; // Human-readable effect description
}

/// <summary>
/// Represents a piece of equipment (weapon or armor)
/// </summary>
public class Equipment
{
    // Identity
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QualityTier Quality { get; set; } = QualityTier.Scavenged;
    public EquipmentType Type { get; set; } = EquipmentType.Weapon;

    // Weapon-specific properties
    public WeaponCategory? WeaponCategory { get; set; }
    public string WeaponAttribute { get; set; } = string.Empty; // "MIGHT", "FINESSE", "WILL"
    public int DamageDice { get; set; } = 1; // Number of d6s
    public int DamageBonus { get; set; } = 0; // Flat bonus to damage
    public int StaminaCost { get; set; } = 5; // Stamina per attack
    public int AccuracyBonus { get; set; } = 0; // Bonus/penalty to attack rolls

    // Armor-specific properties
    public ArmorCategory? ArmorCategory { get; set; }
    public int HPBonus { get; set; } = 0; // Added to MaxHP when equipped
    public int DefenseBonus { get; set; } = 0; // Reduces enemy attack rolls

    // Attribute bonuses (both weapons and armor can have these)
    public List<EquipmentBonus> Bonuses { get; set; } = new();

    // Special properties (for Myth-Forged items)
    public bool IgnoresArmor { get; set; } = false;
    public string SpecialEffect { get; set; } = string.Empty; // Description of unique effect

    /// <summary>
    /// Get a formatted display name with quality tier
    /// </summary>
    public string GetDisplayName()
    {
        return $"{Name} ({GetQualityName()})";
    }

    /// <summary>
    /// Get the human-readable quality tier name
    /// </summary>
    public string GetQualityName()
    {
        return Quality switch
        {
            QualityTier.JuryRigged => "Jury-Rigged",
            QualityTier.Scavenged => "Scavenged",
            QualityTier.ClanForged => "Clan-Forged",
            QualityTier.Optimized => "Optimized",
            QualityTier.MythForged => "Myth-Forged",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get weapon damage description (e.g., "1d6+1", "2d8+2")
    /// </summary>
    public string GetDamageDescription()
    {
        if (Type != EquipmentType.Weapon) return "N/A";

        if (DamageBonus > 0)
            return $"{DamageDice}d6+{DamageBonus}";
        else if (DamageBonus < 0)
            return $"{DamageDice}d6{DamageBonus}";
        else
            return $"{DamageDice}d6";
    }

    /// <summary>
    /// Get formatted bonuses description
    /// </summary>
    public string GetBonusesDescription()
    {
        if (Bonuses.Count == 0 && string.IsNullOrEmpty(SpecialEffect))
            return "None";

        var descriptions = new List<string>();

        foreach (var bonus in Bonuses)
        {
            descriptions.Add(bonus.Description);
        }

        if (!string.IsNullOrEmpty(SpecialEffect))
        {
            descriptions.Add(SpecialEffect);
        }

        return string.Join(", ", descriptions);
    }

    // Backward compatibility aliases
    public string Slot => Type.ToString();
    public int Tier => (int)Quality;
    public int DamageDieSize { get; set; } = 6; // d6 by default
    public bool IsTwoHanded { get; set; } = false;
    public string HandRequirement => IsTwoHanded ? "TwoHanded" : "OneHanded";
    public string Category => WeaponCategory?.ToString() ?? ArmorCategory?.ToString() ?? "Unknown";
    public int Damage => DamageDice;
}
