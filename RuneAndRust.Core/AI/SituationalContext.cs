namespace RuneAndRust.Core.AI;

/// <summary>
/// Represents the AI's analysis of the current battlefield situation.
/// Provides context for tactical decision-making.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public class SituationalContext
{
    // ===== NUMERICAL ADVANTAGE =====

    /// <summary>
    /// Number of living allies (including self).
    /// </summary>
    public int AllyCount { get; set; }

    /// <summary>
    /// Number of living enemies (player characters).
    /// </summary>
    public int EnemyCount { get; set; }

    /// <summary>
    /// True if enemies outnumber allies.
    /// </summary>
    public bool IsOutnumbered { get; set; }

    // ===== HEALTH STATUS =====

    /// <summary>
    /// AI's current HP as a percentage (0.0 to 1.0).
    /// </summary>
    public float SelfHPPercent { get; set; }

    /// <summary>
    /// True if HP is below 30%.
    /// </summary>
    public bool IsLowHP { get; set; }

    /// <summary>
    /// True if HP is below 15% (critical).
    /// </summary>
    public bool IsCriticalHP { get; set; }

    /// <summary>
    /// Average HP percentage of all living allies.
    /// </summary>
    public float AverageAllyHP { get; set; }

    /// <summary>
    /// True if any ally has HP below 20%.
    /// </summary>
    public bool HasCriticalAllies { get; set; }

    // ===== POSITIONING =====

    /// <summary>
    /// True if AI is being flanked (enemies on multiple sides).
    /// </summary>
    public bool IsFlanked { get; set; }

    /// <summary>
    /// True if AI has elevation advantage over all enemies.
    /// </summary>
    public bool HasHighGround { get; set; }

    /// <summary>
    /// True if AI is in cover (physical or metaphysical).
    /// </summary>
    public bool IsInCover { get; set; }

    /// <summary>
    /// True if AI is isolated from allies (no allies within 2 tiles).
    /// </summary>
    public bool IsIsolated { get; set; }

    // ===== COMBAT PHASE =====

    /// <summary>
    /// Current turn number in the encounter.
    /// </summary>
    public int TurnNumber { get; set; }

    /// <summary>
    /// True if turns 1-3 (early game).
    /// </summary>
    public bool IsEarlyGame { get; set; }

    /// <summary>
    /// True if turns 4-8 (mid game).
    /// </summary>
    public bool IsMidGame { get; set; }

    /// <summary>
    /// True if turn 9+ (late game).
    /// </summary>
    public bool IsLateGame { get; set; }

    // ===== OVERALL ASSESSMENT =====

    /// <summary>
    /// Overall tactical advantage assessment.
    /// </summary>
    public TacticalAdvantage Advantage { get; set; }

    /// <summary>
    /// Human-readable summary of the situation.
    /// </summary>
    public string Summary { get; set; } = string.Empty;
}
