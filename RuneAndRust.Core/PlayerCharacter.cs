namespace RuneAndRust.Core;

public class PlayerCharacter
{
    public string Name { get; set; } = "Survivor";
    public CharacterClass Class { get; set; }
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

    // Combat
    public string WeaponName { get; set; } = string.Empty;
    public string WeaponAttribute { get; set; } = string.Empty; // Which attribute the weapon uses
    public int BaseDamage { get; set; } = 1; // Number of d6s

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
