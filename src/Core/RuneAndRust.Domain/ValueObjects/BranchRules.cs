namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration rules for branch generation.
/// </summary>
public class BranchRules
{
    /// <summary>
    /// Probability of creating a main path continuation (0.0-1.0).
    /// </summary>
    public float MainPathProbability { get; init; } = 0.5f;

    /// <summary>
    /// Probability of creating a side path (0.0-1.0).
    /// </summary>
    public float SidePathProbability { get; init; } = 0.25f;

    /// <summary>
    /// Probability of creating a dead end (0.0-1.0).
    /// </summary>
    public float DeadEndProbability { get; init; } = 0.15f;

    /// <summary>
    /// Probability of creating a loop connection (0.0-1.0).
    /// </summary>
    public float LoopProbability { get; init; } = 0.08f;

    /// <summary>
    /// Probability tiers for dead end content types.
    /// </summary>
    public DeadEndContentChances DeadEndChances { get; init; } = new();

    /// <summary>
    /// Creates default branch rules.
    /// </summary>
    public static BranchRules Default => new();
}

/// <summary>
/// Probability thresholds for dead end content types.
/// </summary>
public class DeadEndContentChances
{
    /// <summary>
    /// Threshold for treasure cache (roll less than this = treasure).
    /// </summary>
    public float TreasureCache { get; init; } = 0.30f;

    /// <summary>
    /// Threshold for monster lair (roll less than this = lair).
    /// </summary>
    public float MonsterLair { get; init; } = 0.50f;

    /// <summary>
    /// Threshold for secret shrine (roll less than this = shrine).
    /// </summary>
    public float SecretShrine { get; init; } = 0.70f;

    /// <summary>
    /// Threshold for trap room (roll less than this = trap).
    /// </summary>
    public float TrapRoom { get; init; } = 0.85f;
}
