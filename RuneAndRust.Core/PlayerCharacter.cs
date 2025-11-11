namespace RuneAndRust.Core;

public class PlayerCharacter
{
    public string Name { get; set; } = "Survivor";
    public CharacterClass Class { get; set; }
    public Specialization Specialization { get; set; } = Specialization.None; // v0.7: Unlocked with 10 PP
    public Attributes Attributes { get; set; } = new();

    // Progression (Aethelgard Saga System)
    public int CurrentMilestone { get; set; } = 0;
    public int CurrentLegend { get; set; } = 0;
    public int LegendToNextMilestone { get; set; } = 100;
    public int ProgressionPoints { get; set; } = 2; // Start with 2 PP

    // Resources
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
    public int AP { get; set; }

    // Trauma Economy (v0.5)
    public int PsychicStress { get; set; } = 0; // 0-100, recoverable at Sanctuary
    public int Corruption { get; set; } = 0; // 0-100, permanent
    public int RoomsExploredSinceRest { get; set; } = 0; // For rest cooldown tracking

    // Combat (v0.1 - deprecated in favor of equipment system)
    public string WeaponName { get; set; } = string.Empty;
    public string WeaponAttribute { get; set; } = string.Empty; // Which attribute the weapon uses
    public int BaseDamage { get; set; } = 1; // Number of d6s

    // Equipment System (v0.3)
    public Equipment? EquippedWeapon { get; set; }
    public Equipment? EquippedArmor { get; set; }
    public List<Equipment> Inventory { get; set; } = new(); // Up to 5 items
    public int MaxInventorySize { get; set; } = 5;

    // Abilities
    public List<Ability> Abilities { get; set; } = new();

    // Defense
    public int DefenseBonus { get; set; } = 0; // Temporary defense from Defend action
    public int DefenseTurnsRemaining { get; set; } = 0;

    // Status Effects (v0.2)
    public int BattleRageTurnsRemaining { get; set; } = 0; // Warrior Lv5 ability
    public int ShieldAbsorptionRemaining { get; set; } = 0; // Mystic Lv3 ability

    public bool IsAlive => HP > 0;

    public int GetAttributeValue(string attributeName)
    {
        return attributeName.ToLower() switch
        {
            "might" => Attributes.Might,
            "finesse" => Attributes.Finesse,
            "wits" => Attributes.Wits,
            "will" => Attributes.Will,
            "sturdiness" => Attributes.Sturdiness,
            _ => 0
        };
    }
}
