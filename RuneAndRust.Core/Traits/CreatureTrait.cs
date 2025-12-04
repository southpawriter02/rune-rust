namespace RuneAndRust.Core.Traits;

/// <summary>
/// Enumeration of all creature traits organized by category.
/// Each category has a reserved numeric range for future expansion.
/// See SPEC-COMBAT-015 for full trait specifications.
/// </summary>
public enum CreatureTrait
{
    // ========================================
    // Category 1: Temporal Traits (100-199)
    // Theme: Glitch-induced time anomalies
    // ========================================

    /// <summary>Movement ignores attacks of opportunity; can move through occupied tiles.</summary>
    TemporalPhase = 100,

    /// <summary>+X Evasion (anticipates incoming attacks). PrimaryValue = evasion bonus.</summary>
    TemporalPrescience = 101,

    /// <summary>Movement inflicts Psychic Stress to nearby characters. PrimaryValue = stress, SecondaryValue = range.</summary>
    ChronoDistortion = 102,

    /// <summary>AI behavior: randomly teleports to valid tile each turn.</summary>
    RandomBlink = 103,

    /// <summary>On death, 25% chance to resurrect at 25% HP (once per combat).</summary>
    TimeLoop = 104,

    /// <summary>Attacks have 20% chance to hit twice (echo from alternate timeline).</summary>
    CausalEcho = 105,

    /// <summary>Can spend turn to become invulnerable until next turn.</summary>
    TemporalStasis = 106,

    /// <summary>Once per combat, can undo last action taken against it.</summary>
    Rewind = 107,

    // ========================================
    // Category 2: Corruption Traits (200-299)
    // Theme: Runic Blight infection, data corruption
    // ========================================

    /// <summary>Inflicts +X Corruption per turn to characters within range. PrimaryValue = corruption, SecondaryValue = range.</summary>
    BlightAura = 200,

    /// <summary>Melee attacks inflict Corroded status (stacking armor reduction).</summary>
    CorrosiveTouch = 201,

    /// <summary>On death, releases information burst: nearby characters gain +5 Stress OR +3 Corruption.</summary>
    DataFragment = 202,

    /// <summary>15% chance each turn to perform random action instead of AI choice.</summary>
    Glitched = 203,

    /// <summary>Characters who kill this enemy gain 1 Corruption.</summary>
    Infectious = 204,

    /// <summary>Special attack creates hazardous tile (3 turns duration).</summary>
    RealityTear = 205,

    /// <summary>Regenerates X HP per turn while below 50% HP. PrimaryValue = HP/turn.</summary>
    Reforming = 206,

    /// <summary>Immune to Psychic damage; deals Psychic damage instead of Physical.</summary>
    VoidTouched = 207,

    // ========================================
    // Category 3: Mechanical Traits (300-399)
    // Theme: Pre-Glitch automation, Iron Heart power systems
    // ========================================

    /// <summary>Immune to Bleeding, Poison; resistant to Psychic (-50% Psychic damage).</summary>
    IronHeart = 300,

    /// <summary>+X Soak (flat damage reduction). PrimaryValue = soak bonus.</summary>
    ArmoredPlating = 301,

    /// <summary>Can spend 10 HP to gain +2 damage dice on next attack.</summary>
    Overcharge = 302,

    /// <summary>At less than 25% HP, gains +2 Defense and +1 damage for remainder of combat.</summary>
    EmergencyProtocol = 303,

    /// <summary>Takes 50% damage from first hit each combat (armor absorbs).</summary>
    ModularConstruction = 304,

    /// <summary>Heals X HP at end of turn if didn't attack. PrimaryValue = HP/turn.</summary>
    SelfRepair = 305,

    /// <summary>On death, deals 2d6 Lightning damage to adjacent tiles.</summary>
    PowerSurge = 306,

    /// <summary>+1 Accuracy for each allied Mechanical creature within 3 tiles.</summary>
    Networked = 307,

    // ========================================
    // Category 4: Psychic Traits (400-499)
    // Theme: Mental attacks, Forlorn corruption, trauma infliction
    // ========================================

    /// <summary>Passive Psychic Stress aura: +X Stress/turn to characters within range. PrimaryValue = stress, SecondaryValue = range.</summary>
    ForlornAura = 400,

    /// <summary>Attack deals Psychic damage (ignores Soak).</summary>
    MindSpike = 401,

    /// <summary>Successful attack reduces target's next action's accuracy by 2.</summary>
    MemoryDrain = 402,

    /// <summary>AoE attack: 1d6 Psychic damage + 5 Stress to all characters in 3-tile radius.</summary>
    PsychicScream = 403,

    /// <summary>Characters entering combat must pass WILL check (DC 2) or suffer Frightened (1 turn).</summary>
    FearAura = 404,

    /// <summary>Heals HP equal to Stress inflicted.</summary>
    ThoughtLeech = 405,

    /// <summary>20% chance attacks against this creature target random ally instead.</summary>
    Hallucination = 406,

    /// <summary>At end of each round, lowest-WILL character gains +3 Stress.</summary>
    Whispers = 407,

    // ========================================
    // Category 5: Mobility Traits (500-599)
    // Theme: Movement advantages, positioning control
    // ========================================

    /// <summary>Ignores ground-based hazards; +2 Defense vs melee attacks.</summary>
    Flight = 500,

    /// <summary>Can move through Impassable terrain; emerges with +2 Accuracy on next attack.</summary>
    Burrowing = 501,

    /// <summary>Can move through occupied tiles and obstacles.</summary>
    Phasing = 502,

    /// <summary>+X tiles movement per turn. PrimaryValue = bonus tiles.</summary>
    Swiftness = 503,

    /// <summary>Cannot be forcibly moved; immune to knockback/pull effects.</summary>
    Anchored = 504,

    /// <summary>If hidden at start of combat, first attack is automatic critical hit.</summary>
    Ambush = 505,

    /// <summary>After attacking, may move 1 tile without provoking attacks of opportunity.</summary>
    HitAndRun = 506,

    /// <summary>+2 Accuracy and +2 Defense while in starting zone.</summary>
    Territorial = 507,

    // ========================================
    // Category 6: Defensive Traits (600-699)
    // Theme: Damage mitigation, survival mechanics
    // ========================================

    /// <summary>Heals X HP at start of turn. PrimaryValue = HP/turn.</summary>
    Regeneration = 600,

    /// <summary>Ignores attacks dealing less than X damage. PrimaryValue = threshold.</summary>
    DamageThreshold = 601,

    /// <summary>20% of damage taken is dealt back to attacker.</summary>
    Reflective = 602,

    /// <summary>Starts combat with X temporary HP that doesn't regenerate. PrimaryValue = temp HP.</summary>
    ShieldGenerator = 603,

    /// <summary>Cannot be reduced below 1 HP for first 2 turns of combat.</summary>
    LastStand = 604,

    /// <summary>After taking damage of a type, gains +2 Soak vs that type.</summary>
    AdaptiveArmor = 605,

    /// <summary>-2 Accuracy for ranged attacks against this creature.</summary>
    Camouflage = 606,

    /// <summary>Immune to Stun, Root, and Slow effects.</summary>
    Unstoppable = 607,

    // ========================================
    // Category 7: Offensive Traits (700-799)
    // Theme: Enhanced damage, special attack modifiers
    // ========================================

    /// <summary>Critical hits deal triple damage instead of double.</summary>
    Brutal = 700,

    /// <summary>Can attack twice per turn (second attack at -2 Accuracy).</summary>
    Relentless = 701,

    /// <summary>+50% damage against targets below 25% HP.</summary>
    Executioner = 702,

    /// <summary>Attacks ignore X points of target's Soak. PrimaryValue = soak penetration.</summary>
    ArmorPiercing = 703,

    /// <summary>Can attack targets 2 tiles away with melee attacks.</summary>
    Reach = 704,

    /// <summary>Basic attacks hit all adjacent enemies.</summary>
    Sweeping = 705,

    /// <summary>At less than 50% HP, +2 damage but -2 Defense.</summary>
    Enrage = 706,

    /// <summary>+2 Accuracy against isolated targets (no allies within 2 tiles).</summary>
    PredatorInstinct = 707,

    // ========================================
    // Category 8: Unique/Exotic Traits (800-899)
    // Theme: One-of-a-kind mechanics
    // ========================================

    /// <summary>When killed, spawns 2 smaller versions (50% HP, 50% damage each).</summary>
    SplitOnDeath = 800,

    /// <summary>Starts combat with 2 illusory duplicates (1 HP each, destroyed on hit).</summary>
    MirrorImage = 801,

    /// <summary>Heals 50% of damage dealt.</summary>
    Vampiric = 802,

    /// <summary>On death, deals Xd6 damage to all characters within 2 tiles. PrimaryValue = dice count.</summary>
    Explosive = 803,

    /// <summary>If killed while ally is alive, returns at 50% HP after 2 turns.</summary>
    Resurrection = 804,

    /// <summary>Shares damage with linked ally (50% each).</summary>
    SymbioticLink = 805,

    /// <summary>At less than 25% HP, attacks random target (including allies).</summary>
    Berserk = 806,

    /// <summary>+1 Accuracy per ally attacking same target this round.</summary>
    PackTactics = 807,

    // ========================================
    // Category 9: Resistance Traits (900-999)
    // Theme: Damage type resistances, elemental affinities
    // ========================================

    /// <summary>Takes 50% damage from Fire; immune to Burning status.</summary>
    FireResistant = 900,

    /// <summary>Takes 50% damage from Cold; immune to Frozen/Slowed from cold.</summary>
    ColdResistant = 901,

    /// <summary>Takes 50% damage from Lightning; immune to Stunned from electrical.</summary>
    LightningResistant = 902,

    /// <summary>Takes 50% damage from Psychic; +2 to WILL saves vs mental effects.</summary>
    PsychicResistant = 903,

    /// <summary>Takes 75% damage from Physical attacks (slashing, piercing, bludgeoning).</summary>
    PhysicalResistant = 904,

    /// <summary>Takes 50% damage from Acid/Corrosion; immune to Corroded status.</summary>
    AcidResistant = 905,

    /// <summary>Takes 150% damage from Fire; Burning lasts +1 turn.</summary>
    FireVulnerable = 906,

    /// <summary>Takes 150% damage from Cold; Frozen/Slowed lasts +1 turn.</summary>
    ColdVulnerable = 907,

    /// <summary>Takes 150% damage from Lightning; auto-Stunned for 1 turn on Lightning hit.</summary>
    LightningVulnerable = 908,

    /// <summary>Takes 200% damage from Holy/Radiant sources.</summary>
    HolyVulnerable = 909,

    /// <summary>Choose element: damage of that type heals instead of hurting.</summary>
    ElementalAbsorption = 910,

    /// <summary>Immune to Corroded; +2 Soak vs Physical; vulnerable to Lightning (150%).</summary>
    AlloyedForm = 911,

    /// <summary>+50% damage against organic (non-Mechanical) targets.</summary>
    OrganicBane = 912,

    /// <summary>+50% damage against Mechanical targets; attacks disable SelfRepair for 1 turn.</summary>
    MechanicalBane = 913,

    // ========================================
    // Category 10: Strategy/AI Behavior Traits (1000-1099)
    // Theme: Tactical decision-making patterns
    // ========================================

    /// <summary>Flees combat when HP drops below 25%.</summary>
    Cowardly = 1000,

    /// <summary>After taking damage, 40% chance to move away from attacker instead of acting.</summary>
    Skittish = 1001,

    /// <summary>Prioritizes targeting characters with healing abilities; +2 Accuracy vs healers.</summary>
    HealerHunter = 1002,

    /// <summary>Always attacks the character who dealt most damage to it last round.</summary>
    ThreatFocused = 1003,

    /// <summary>Prioritizes targets below 50% HP; +1 Accuracy vs wounded targets.</summary>
    WeaknessSensor = 1004,

    /// <summary>Moves to intercept attacks against allied creatures; can bodyblock.</summary>
    ProtectorInstinct = 1005,

    /// <summary>Prioritizes targets affected by status effects; +2 damage vs debuffed targets.</summary>
    Opportunist = 1006,

    /// <summary>Prioritizes targets with high WILL/WITS; interrupts abilities on hit (25% chance).</summary>
    CasterKiller = 1007,

    /// <summary>Never retreats; +1 damage per 10% HP lost (max +10 at 0% HP).</summary>
    BerserkerAI = 1008,

    /// <summary>Allied creatures within 3 tiles gain +1 Accuracy; this creature acts last in initiative.</summary>
    PackLeader = 1009,

    /// <summary>Will not engage until player moves adjacent OR player HP drops below 75%.</summary>
    AmbusherAI = 1010,

    /// <summary>Will not leave starting zone; +3 Accuracy and +3 Defense while in starting zone.</summary>
    TerritorialAI = 1011,

    /// <summary>After attacking, attempts to move to maximum range; won't pursue fleeing targets.</summary>
    HitAndFade = 1012,

    /// <summary>Attempts to surround isolated targets; +2 Accuracy when 2+ allies adjacent to target.</summary>
    Swarmer = 1013,

    /// <summary>At less than 10% HP, uses Explosive trait voluntarily or charges nearest enemy.</summary>
    SelfDestructive = 1014,

    /// <summary>Won't attack if it would provoke attack of opportunity; prefers ranged options.</summary>
    Cautious = 1015,

    // ========================================
    // Category 11: Sensory Traits (1100-1199)
    // Theme: Perception abilities, detection methods
    // ========================================

    /// <summary>Cannot target specific enemies; attacks random target in range; immune to visual effects.</summary>
    Blind = 1100,

    /// <summary>Unaffected by darkness, invisibility, or visual concealment within 3 tiles.</summary>
    Blindsense = 1101,

    /// <summary>+2 Accuracy in normal conditions; Stunned for 1 turn by loud sounds.</summary>
    SoundSensitive = 1102,

    /// <summary>Immune to sound-based attacks; cannot detect Stealthed enemies; -2 to Initiative.</summary>
    SoundBlind = 1103,

    /// <summary>Detects all ground-based creatures within 4 tiles regardless of stealth; cannot detect Flying.</summary>
    Tremorsense = 1104,

    /// <summary>+2 Accuracy vs warm-blooded (organic) targets; -2 Accuracy vs Mechanical/cold targets.</summary>
    ThermalVision = 1105,

    /// <summary>+3 Accuracy vs targets that moved last turn; -2 Accuracy vs stationary targets.</summary>
    MotionDetection = 1106,

    /// <summary>Unaffected by darkness penalties; can see in magical darkness.</summary>
    Darkvision = 1107,

    /// <summary>-2 Accuracy in bright light; Blinded (1 turn) by flash effects; +2 Accuracy in darkness.</summary>
    LightSensitive = 1108,

    /// <summary>Detects all creatures within 5 tiles regardless of stealth/invisibility; ignores cover.</summary>
    PsychicSense = 1109,

    /// <summary>Immune to Blind status and gaze attacks; relies on other senses.</summary>
    Eyeless = 1110,

    /// <summary>Can detect and target all enemies within 2 tiles simultaneously; AoE attacks don't require aiming.</summary>
    ScatterSense = 1111,

    // ========================================
    // Category 12: Combat Condition Traits (1200-1299)
    // Theme: Environmental adaptations, situational modifiers
    // ========================================

    /// <summary>No penalties in water; can move through water tiles freely; +2 Defense in water.</summary>
    Amphibious = 1200,

    /// <summary>No damage from hot terrain; +2 Accuracy in hot environments.</summary>
    HeatAdapted = 1201,

    /// <summary>No damage from cold terrain; immune to Slowed from cold; +2 Accuracy in cold.</summary>
    ColdAdapted = 1202,

    /// <summary>Immune to Poison, Corroded, and environmental toxin damage.</summary>
    ToxinImmune = 1203,

    /// <summary>Immune to radiation damage and Irradiated status.</summary>
    RadiationImmune = 1204,

    /// <summary>Can function in zero-atmosphere; immune to suffocation/pressure effects.</summary>
    VacuumSurvival = 1205,

    /// <summary>Immune to knockback, pull, and gravity-based effects; +2 Defense vs falling.</summary>
    GravityAdapted = 1206,

    /// <summary>Cannot enter water (floats on surface); +2 Defense vs ground-based attacks; -2 vs aerial.</summary>
    Buoyant = 1207,

    /// <summary>Immune to Lightning damage reflection; cannot be lifted/thrown; +2 Soak vs Lightning.</summary>
    Grounded = 1208,

    /// <summary>50% chance to ignore terrain effects; can pass through thin walls; -2 vs holy/psychic damage.</summary>
    Ethereal = 1209,

    /// <summary>In bright/natural light: -2 to all attributes; in darkness: +2 to all attributes.</summary>
    SunlightWeakness = 1210,

    /// <summary>During night/under moonlight: +2 Accuracy, +2 damage; during day: -1 Accuracy.</summary>
    MoonlightStrength = 1211,

    /// <summary>During storms/electrical hazards: +3 Accuracy, regenerate 5 HP/turn; calm weather: normal.</summary>
    StormBorn = 1212,

    /// <summary>+2 Accuracy against Bleeding targets; can detect Bleeding creatures through walls within 5 tiles.</summary>
    BloodScent = 1213,

    /// <summary>Heals 2 HP per point of Corruption on the battlefield; weakens in pure areas.</summary>
    CorruptionSustained = 1214,

    /// <summary>Takes 2 damage per turn in consecrated/holy areas; +2 damage in corrupted areas.</summary>
    HallowedBane = 1215
}
