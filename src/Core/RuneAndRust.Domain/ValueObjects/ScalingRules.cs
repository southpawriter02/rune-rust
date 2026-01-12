namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration rules for difficulty scaling.
/// </summary>
/// <remarks>
/// Loaded from scaling-rules.json and used by DifficultyRating.Calculate().
/// </remarks>
public class ScalingRules
{
    /// <summary>
    /// Base difficulty level for starting area.
    /// </summary>
    public int BaseDifficulty { get; init; } = 5;

    /// <summary>
    /// Difficulty added per Z-level depth.
    /// </summary>
    public float DepthMultiplier { get; init; } = 10.0f;

    /// <summary>
    /// Difficulty added per unit of distance from start.
    /// </summary>
    public float DistanceMultiplier { get; init; } = 0.5f;

    /// <summary>
    /// Difficulty modifier for treasure rooms (typically higher).
    /// </summary>
    public float TreasureRoomModifier { get; init; } = 1.5f;

    /// <summary>
    /// Difficulty modifier for boss rooms (typically much higher).
    /// </summary>
    public float BossRoomModifier { get; init; } = 2.0f;

    /// <summary>
    /// Difficulty modifier for trap rooms.
    /// </summary>
    public float TrapRoomModifier { get; init; } = 1.2f;

    /// <summary>
    /// Difficulty modifier for shrine rooms.
    /// </summary>
    public float ShrineRoomModifier { get; init; } = 1.1f;

    /// <summary>
    /// Creates default scaling rules.
    /// </summary>
    public static ScalingRules Default => new();
}
