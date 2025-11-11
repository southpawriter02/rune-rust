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
    AethericAberration, // Boss - Magic focus

    // v0.6 new enemies (The Lower Depths)
    MaintenanceConstruct,  // Tier 1 - Self-healing balanced enemy
    SludgeCrawler,         // Tier 1 - Poison swarm enemy
    CorruptedEngineer,     // Tier 2 - Support/buffer caster
    VaultCustodian,        // Tier 3 - Mini-boss guardian
    ForlornArchivist,      // Boss - Psychic/summoner
    OmegaSentinel          // Boss - Physical tank
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

    // Trauma Economy (v0.5)
    public bool IsForlorn { get; set; } = false; // Inflicts passive Psychic Stress aura

    // Status effects
    public bool IsStunned { get; set; } = false;
    public int StunTurnsRemaining { get; set; } = 0;
    public int BleedingTurnsRemaining { get; set; } = 0; // Scavenger Lv3 ability
    public int AnalyzedTurnsRemaining { get; set; } = 0; // [v0.7] Adept status: +2 Accuracy for attackers

    // [v0.6] Additional enemy mechanics
    public int Soak { get; set; } = 0; // Damage reduction (armor)
    public int PoisonDamagePerTurn { get; set; } = 0; // Poison DoT inflicted on player
    public int PoisonTurnsRemaining { get; set; } = 0; // Duration of poison effect
    public bool HasUsedSpecialAbility { get; set; } = false; // For one-time abilities (Last Stand, etc.)

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
