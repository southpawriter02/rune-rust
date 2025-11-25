using RuneAndRust.Core;
using RuneAndRust.Core.AI;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.17: Danger level for boss mechanic warnings.
/// </summary>
public enum DangerLevel
{
    /// <summary>Low danger - minor damage or debuff.</summary>
    Low,

    /// <summary>Medium danger - significant damage.</summary>
    Medium,

    /// <summary>High danger - heavy damage, must respond.</summary>
    High,

    /// <summary>Lethal danger - potential one-shot, must avoid.</summary>
    Lethal
}

/// <summary>
/// v0.43.17: Represents a boss mechanic warning for UI display.
/// </summary>
public class BossMechanicWarning
{
    /// <summary>Unique identifier for the warning.</summary>
    public string WarningId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Name of the mechanic being telegraphed.</summary>
    public string MechanicName { get; set; } = string.Empty;

    /// <summary>Description of what to do.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Time in seconds before the mechanic executes.</summary>
    public float WarningTime { get; set; }

    /// <summary>Turns remaining until execution.</summary>
    public int TurnsRemaining { get; set; }

    /// <summary>Danger level of the mechanic.</summary>
    public DangerLevel DangerLevel { get; set; }

    /// <summary>Whether this mechanic can be interrupted.</summary>
    public bool CanBeInterrupted { get; set; }

    /// <summary>Icon to display for this warning.</summary>
    public string Icon { get; set; } = "\u26A0"; // Warning sign
}

/// <summary>
/// v0.43.17: Represents a health segment for phase-based boss health bars.
/// </summary>
public class PhaseHealthSegment
{
    /// <summary>Phase number (1-based).</summary>
    public int PhaseNumber { get; set; }

    /// <summary>Whether this is the current active phase.</summary>
    public bool IsCurrentPhase { get; set; }

    /// <summary>Whether this phase has been completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>HP percentage threshold for this phase (0.0-1.0).</summary>
    public float HealthThreshold { get; set; }

    /// <summary>Display name for this phase.</summary>
    public string PhaseName { get; set; } = string.Empty;
}

/// <summary>
/// v0.43.17: Display data for a boss encounter.
/// </summary>
public class BossDisplayData
{
    /// <summary>Boss name.</summary>
    public string BossName { get; set; } = string.Empty;

    /// <summary>Boss title/epithet.</summary>
    public string BossTitle { get; set; } = string.Empty;

    /// <summary>Current HP.</summary>
    public int CurrentHP { get; set; }

    /// <summary>Maximum HP.</summary>
    public int MaxHP { get; set; }

    /// <summary>HP percentage (0.0-1.0).</summary>
    public float HPPercentage => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

    /// <summary>Current phase number.</summary>
    public int CurrentPhase { get; set; } = 1;

    /// <summary>Total number of phases.</summary>
    public int TotalPhases { get; set; } = 3;

    /// <summary>Phase health segments for display.</summary>
    public List<PhaseHealthSegment> PhaseSegments { get; set; } = new();

    /// <summary>Active mechanic warnings.</summary>
    public List<BossMechanicWarning> ActiveWarnings { get; set; } = new();

    /// <summary>Enrage progress (0.0-1.0, 1.0 = enraged).</summary>
    public float EnrageProgress { get; set; }

    /// <summary>Whether the boss is currently enraged.</summary>
    public bool IsEnraged => EnrageProgress >= 1.0f;

    /// <summary>Whether the boss is in a vulnerable state.</summary>
    public bool IsVulnerable { get; set; }

    /// <summary>Turns remaining in vulnerable state.</summary>
    public int VulnerableTurnsRemaining { get; set; }

    /// <summary>Whether a phase transition is in progress.</summary>
    public bool IsTransitioning { get; set; }

    /// <summary>The boss enemy reference.</summary>
    public Enemy? Boss { get; set; }
}

/// <summary>
/// v0.43.17: Service interface for boss display information.
/// Provides UI-ready data for boss encounters.
/// </summary>
public interface IBossDisplayService
{
    /// <summary>
    /// Gets display data for a boss enemy.
    /// </summary>
    BossDisplayData GetBossDisplayData(Enemy boss);

    /// <summary>
    /// Gets phase segments for a boss's health bar.
    /// </summary>
    List<PhaseHealthSegment> GetPhaseSegments(Enemy boss, int currentPhase);

    /// <summary>
    /// Converts a telegraphed ability to a mechanic warning.
    /// </summary>
    BossMechanicWarning CreateMechanicWarning(string abilityName, string description, int turnsRemaining, DangerLevel dangerLevel, bool canInterrupt);

    /// <summary>
    /// Gets the boss title/epithet based on enemy type.
    /// </summary>
    string GetBossTitle(EnemyType enemyType);

    /// <summary>
    /// Calculates enrage progress based on turn count and boss configuration.
    /// </summary>
    float CalculateEnrageProgress(int turnCount, int enrageTurn);

    /// <summary>
    /// Gets the danger level for an ability based on its damage potential.
    /// </summary>
    DangerLevel GetAbilityDangerLevel(int damageDice, int damageBonus, bool isAoE);

    /// <summary>
    /// Gets phase-specific description text.
    /// </summary>
    string GetPhaseDescription(BossPhase phase);
}
