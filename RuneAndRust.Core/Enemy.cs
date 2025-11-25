using RuneAndRust.Core.Population;

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
    OmegaSentinel,         // Boss - Physical tank

    // v0.16 new enemies (Content Expansion)
    CorrodedSentry,        // Trivial - Draugr-Pattern security drone
    HuskEnforcer,          // Low - Symbiotic Plate reanimated corpse
    ArcWelderUnit,         // Low - Haugbui-Class industrial robot
    Shrieker,              // Medium - Symbiotic Plate with psychic scream
    JotunReaderFragment,   // Medium - J-Reader AI fragment
    ServitorSwarm,         // Medium - Collective of maintenance drones
    BoneKeeper,            // High - Symbiotic Plate skeletal construct
    FailureColossus,       // High - Haugbui-Class construction automaton
    RustWitch,             // Lethal - Advanced Symbiotic Plate colony
    SentinelPrime          // Lethal - Elite military automaton
}

public class Enemy
{
    // v0.24.2: Enemy ID for database tracking
    public int EnemyID { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public EnemyType Type { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public Attributes Attributes { get; set; } = new();

    // v0.24.2: Trauma Economy for enemies
    public int PsychicStress { get; set; } = 0; // 0-100, some abilities inflict this
    public int Corruption { get; set; } = 0; // 0-100, corruption level

    // Combat
    public int BaseDamageDice { get; set; } = 1; // Number of d6s
    public int DamageBonus { get; set; } = 0; // Flat bonus to damage

    // Defense
    public int DefenseBonus { get; set; } = 0;
    public int DefenseTurnsRemaining { get; set; } = 0;

    // Boss specific
    public bool IsBoss { get; set; } = false;
    public int Phase { get; set; } = 1; // For phase-based AI

    // Spawn/Population properties (v0.11+)
    public Vector2? SpawnPosition { get; set; } = null; // Tactical positioning from DormantProcess
    public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.Low; // Threat classification
    public bool IsChampion { get; set; } = false; // Elite variant with enhanced stats
    public string? BehaviorNote { get; set; } = null; // Environmental storytelling context
    public string ProcessType { get; set; } = string.Empty; // DormantProcess type identifier

    // Trauma Economy (v0.5)
    public bool IsForlorn { get; set; } = false; // Inflicts passive Psychic Stress aura

    // Status effects
    public bool IsStunned { get; set; } = false;
    public int StunTurnsRemaining { get; set; } = 0;
    public int BleedingTurnsRemaining { get; set; } = 0; // Scavenger Lv3 ability
    public int AnalyzedTurnsRemaining { get; set; } = 0; // [v0.7] Adept status: +2 Accuracy for attackers
    public int VulnerableTurnsRemaining { get; set; } = 0; // [v0.7] Adept status: +25% damage taken
    public int SilencedTurnsRemaining { get; set; } = 0; // [v0.7] Adept status: Prevents spellcasting/performances
    public int DisorientedTurnsRemaining { get; set; } = 0; // [v0.20.3] Disoriented: Gravity/spatial distortion

    // [v0.6] Additional enemy mechanics
    public int Soak { get; set; } = 0; // Damage reduction (armor)
    public int PoisonDamagePerTurn { get; set; } = 0; // Poison DoT inflicted on player
    public int PoisonTurnsRemaining { get; set; } = 0; // Duration of poison effect
    public bool HasUsedSpecialAbility { get; set; } = false; // For one-time abilities (Last Stand, etc.)

    // v0.19.8: Rust-Witch [Corroded] status effect
    public int CorrodedStacks { get; set; } = 0; // 0-5 stacks, each deals 1d6 damage and -2 Armor
    public List<int> CorrodedStackDurations { get; set; } = new(); // Independent 3-turn durations per stack

    // Progression (Aethelgard Saga System)
    public int BaseLegendValue { get; set; } = 0;

    // v0.20: Tactical Combat Grid System
    public GridPosition? Position { get; set; } = null; // Current position on the combat grid
    public int KineticEnergy { get; set; } = 0; // Movement-based resource (0-100)
    public int MaxKineticEnergy { get; set; } = 100; // Maximum KE
    public int TilesMovedThisTurn { get; set; } = 0; // Number of tiles moved this turn
    public bool HasMovedThisTurn { get; set; } = false; // Whether movement occurred this turn

    // v0.42.1: Tactical AI System
    public AI.AIArchetype AIArchetype { get; set; } = AI.AIArchetype.Tactical; // AI behavior pattern

    // v0.24.2: Advanced Status Effect System
    public List<StatusEffect> StatusEffects { get; set; } = new(); // Modern status effect tracking

    public bool IsAlive => HP > 0;

    // Convenience aliases for backward compatibility
    public int CurrentHP { get => HP; set => HP = value; }
    public string EnemyTypeId => Type.ToString();
    public string EnemyId => Id;
    public int CurrentPhase { get => Phase; set => Phase = value; }
    public int Defense { get => DefenseBonus; set => DefenseBonus = value; }
    public int Attack => BaseDamageDice * 3; // Approximation
    public int Tier => (int)ThreatLevel;
    public string FlavorText => BehaviorNote ?? string.Empty;
    public int WILL => Attributes.Will;
    public int Level { get; set; } = 1;
    public bool IsFlying { get; set; } = false;
    public bool IsHidden { get; set; } = false;
    public bool IsStealth { get; set; } = false;
    public bool IsEnraged { get; set; } = false;
    public int StaggeredTurnsRemaining { get; set; } = 0;
    public string BossEncounterId { get; set; } = string.Empty;
    public float VulnerabilityDamageMultiplier { get; set; } = 1.0f;
    public int Armor { get => Soak; set => Soak = value; }
    public int Speed { get; set; } = 3;
    public int Evasion { get; set; } = 0;
    public int Stamina { get; set; } = 100;
    public int MaxStamina { get; set; } = 100;
    public string Faction { get; set; } = string.Empty;
    public List<Ability> Abilities { get; set; } = new();
    public string LootTableId { get; set; } = string.Empty;

    public int GetAttributeValue(string attributeName)
    {
        return attributeName.ToLower() switch
        {
            "might" => Attributes.Might,
            "finesse" => Attributes.Finesse,
            "sturdiness" => Attributes.Sturdiness,
            "will" => Attributes.Will,
            "wits" => Attributes.Wits,
            _ => 0
        };
    }
}
