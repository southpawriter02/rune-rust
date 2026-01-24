using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an enemy detected during scouting operations.
/// </summary>
/// <remarks>
/// <para>
/// DetectedEnemy provides reconnaissance information about hostile creatures
/// in adjacent rooms without revealing detailed combat statistics. This allows
/// players to make tactical decisions about whether to engage, prepare, or avoid.
/// </para>
/// <para>
/// Information revealed through scouting:
/// <list type="bullet">
///   <item><description>EnemyType: General category of the enemy (e.g., "Raiders", "Feral Dogs")</description></item>
///   <item><description>Count: Number of enemies of this type</description></item>
///   <item><description>ThreatLevel: Assessment of danger (Low, Moderate, High, Extreme)</description></item>
///   <item><description>Position: Where in the room they are located</description></item>
/// </list>
/// </para>
/// <para>
/// Scouting intentionally does not reveal:
/// <list type="bullet">
///   <item><description>Exact hit points or combat statistics</description></item>
///   <item><description>Special abilities or weaknesses</description></item>
///   <item><description>Specific loot or equipment</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="EnemyType">General type or category of the enemy.</param>
/// <param name="Count">Number of enemies of this type detected.</param>
/// <param name="ThreatLevel">Assessed threat level based on challenge rating.</param>
/// <param name="Position">Location description within the room.</param>
public readonly record struct DetectedEnemy(
    string EnemyType,
    int Count,
    ThreatLevel ThreatLevel,
    string Position)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether multiple enemies of this type were detected.
    /// </summary>
    public bool IsGroup => Count > 1;

    /// <summary>
    /// Gets whether this represents a high-priority threat.
    /// </summary>
    /// <remarks>
    /// High-priority threats are High or Extreme threat level,
    /// or groups of Moderate threats.
    /// </remarks>
    public bool IsHighPriority =>
        ThreatLevel >= ThreatLevel.High ||
        (ThreatLevel == ThreatLevel.Moderate && Count > 1);

    /// <summary>
    /// Gets a tactical assessment of this enemy group.
    /// </summary>
    public string TacticalAssessment =>
        ThreatLevel.GetTacticalRecommendation();

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a DetectedEnemy from basic information.
    /// </summary>
    /// <param name="enemyType">The type of enemy.</param>
    /// <param name="count">Number of enemies.</param>
    /// <param name="challengeRating">The enemy's challenge rating for threat assessment.</param>
    /// <param name="position">Location in the room.</param>
    /// <returns>A new DetectedEnemy instance.</returns>
    public static DetectedEnemy Create(
        string enemyType,
        int count,
        int challengeRating,
        string? position = null)
    {
        return new DetectedEnemy(
            EnemyType: enemyType,
            Count: count,
            ThreatLevel: ThreatLevelExtensions.FromChallengeRating(challengeRating),
            Position: position ?? "unknown location");
    }

    /// <summary>
    /// Creates a DetectedEnemy with a specific threat level.
    /// </summary>
    /// <param name="enemyType">The type of enemy.</param>
    /// <param name="count">Number of enemies.</param>
    /// <param name="threatLevel">The assessed threat level.</param>
    /// <param name="position">Location in the room.</param>
    /// <returns>A new DetectedEnemy instance.</returns>
    public static DetectedEnemy CreateWithThreatLevel(
        string enemyType,
        int count,
        ThreatLevel threatLevel,
        string? position = null)
    {
        return new DetectedEnemy(
            EnemyType: enemyType,
            Count: count,
            ThreatLevel: threatLevel,
            Position: position ?? "unknown location");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for this detected enemy.
    /// </summary>
    /// <returns>A formatted string suitable for player display.</returns>
    /// <example>
    /// "2 Raiders (moderate threat)" or "1 Glitch Horror (deadly threat)"
    /// </example>
    public string ToDisplayString()
    {
        string countStr = Count.ToString();
        string threatStr = ThreatLevel.GetShortDescriptor();
        return $"{countStr} {EnemyType} ({threatStr} threat)";
    }

    /// <summary>
    /// Creates a detailed display string including position.
    /// </summary>
    /// <returns>A formatted string with full details.</returns>
    public string ToDetailedString()
    {
        string countStr = Count.ToString();
        string threatStr = ThreatLevel.GetDisplayName();
        return $"{countStr} {EnemyType} [{threatStr}] at {Position}";
    }

    /// <summary>
    /// Creates a warning message for this enemy detection.
    /// </summary>
    /// <returns>A warning message appropriate for the threat level.</returns>
    public string ToWarningString()
    {
        return ThreatLevel switch
        {
            ThreatLevel.Low => $"Minor threat detected: {EnemyType}",
            ThreatLevel.Moderate => $"[CAUTION] {Count} {EnemyType} spotted",
            ThreatLevel.High => $"[WARNING] Dangerous enemies: {Count} {EnemyType}",
            ThreatLevel.Extreme => $"[DANGER] Extreme threat: {Count} {EnemyType}!",
            _ => $"Enemies detected: {Count} {EnemyType}"
        };
    }

    /// <summary>
    /// Returns a human-readable summary of the detected enemy.
    /// </summary>
    /// <returns>A formatted string describing the detection.</returns>
    public override string ToString() => ToDisplayString();
}
