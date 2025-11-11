namespace RuneAndRust.Core;

public enum EnemyType
{
    // v0.1-v0.3 enemies
    CorruptedServitor,
    BlightDrone,
    RuinWarden,

    // v0.4 new enemies
    ScrapHound,         // Tier 0 - Fast harasser
    TestSubject,        // Tier 1 - Glass cannon
    WarFrame,           // Tier 2 - Elite/Mini-boss
    ForlornScholar,     // Tier 2 - Caster (can be talked to)
    AethericAberration  // Boss - Magic focus
}

public class Enemy
{
    public string Name { get; set; } = string.Empty;
    public EnemyType Type { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public Attributes Attributes { get; set; } = new();

    // Combat
    public int BaseDamageDice { get; set; } = 1; // Number of d6s
    public int DamageBonus { get; set; } = 0; // Flat bonus to damage

    // Defense
    public int DefenseBonus { get; set; } = 0;
    public int DefenseTurnsRemaining { get; set; } = 0;

    // Boss specific
    public bool IsBoss { get; set; } = false;
    public int Phase { get; set; } = 1; // For phase-based AI

    // Status effects
    public bool IsStunned { get; set; } = false;
    public int StunTurnsRemaining { get; set; } = 0;
    public int BleedingTurnsRemaining { get; set; } = 0; // Scavenger Lv3 ability

    // Progression (Aethelgard Saga System)
    public int BaseLegendValue { get; set; } = 0;

    public bool IsAlive => HP > 0;

    public int GetAttributeValue(string attributeName)
    {
        return attributeName.ToLower() switch
        {
            "might" => Attributes.Might,
            "finesse" => Attributes.Finesse,
            "sturdiness" => Attributes.Sturdiness,
            _ => 0
        };
    }
}
