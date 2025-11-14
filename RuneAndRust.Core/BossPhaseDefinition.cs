namespace RuneAndRust.Core;

/// <summary>
/// v0.23.1: Defines a boss combat phase with HP thresholds and mechanics
/// Bosses transition through phases at specific HP percentages (75%, 50%, 25%)
/// Each phase can have unique abilities, add spawns, and behavioral changes
/// </summary>
public class BossPhaseDefinition
{
    /// <summary>
    /// Phase number (1-4)
    /// </summary>
    public int PhaseNumber { get; set; } = 1;

    /// <summary>
    /// HP percentage threshold to enter this phase (100 for Phase 1, 75 for Phase 2, etc.)
    /// </summary>
    public int HPPercentageThreshold { get; set; } = 100;

    /// <summary>
    /// Human-readable description of the phase transition event
    /// Example: "The Forlorn Commander's systems overload, sparks flying from damaged circuits!"
    /// </summary>
    public string TransitionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Number of turns the boss is invulnerable during phase transition (default 1)
    /// </summary>
    public int InvulnerabilityTurns { get; set; } = 1;

    /// <summary>
    /// List of ability IDs available in this phase
    /// </summary>
    public List<string> AvailableAbilityIds { get; set; } = new();

    /// <summary>
    /// List of ability IDs that become unavailable in this phase
    /// </summary>
    public List<string> DisabledAbilityIds { get; set; } = new();

    /// <summary>
    /// Stat multipliers applied during this phase
    /// </summary>
    public PhaseStatModifiers StatModifiers { get; set; } = new();

    /// <summary>
    /// Add wave configuration for this phase
    /// </summary>
    public AddWaveConfig? AddWave { get; set; } = null;

    /// <summary>
    /// Environmental changes triggered during this phase
    /// </summary>
    public List<string> EnvironmentalEvents { get; set; } = new();
}

/// <summary>
/// v0.23.1: Stat modifiers applied during a boss phase
/// </summary>
public class PhaseStatModifiers
{
    /// <summary>
    /// Damage multiplier (1.0 = normal, 1.5 = 50% more damage)
    /// </summary>
    public double DamageMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Defense bonus added to boss
    /// </summary>
    public int DefenseBonus { get; set; } = 0;

    /// <summary>
    /// Regeneration HP per turn
    /// </summary>
    public int RegenerationPerTurn { get; set; } = 0;

    /// <summary>
    /// Additional actions per turn (0 = normal, 1 = double actions)
    /// </summary>
    public int BonusActionsPerTurn { get; set; } = 0;

    /// <summary>
    /// Soak (damage reduction) bonus
    /// </summary>
    public int SoakBonus { get; set; } = 0;
}

/// <summary>
/// v0.23.1: Configuration for add wave spawning during boss phase transitions
/// </summary>
public class AddWaveConfig
{
    /// <summary>
    /// List of enemy types to spawn
    /// </summary>
    public List<EnemyType> EnemyTypes { get; set; } = new();

    /// <summary>
    /// Number of each enemy type to spawn
    /// </summary>
    public Dictionary<EnemyType, int> SpawnCounts { get; set; } = new();

    /// <summary>
    /// Description shown in combat log
    /// Example: "The Forlorn Commander summons reinforcements!"
    /// </summary>
    public string SpawnDescription { get; set; } = string.Empty;

    /// <summary>
    /// Delay before spawned adds can act (in turns, default 0)
    /// </summary>
    public int SpawnDelay { get; set; } = 0;
}
