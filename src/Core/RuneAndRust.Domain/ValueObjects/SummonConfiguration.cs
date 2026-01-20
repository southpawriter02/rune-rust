namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for boss summoning behavior during a phase.
/// </summary>
/// <remarks>
/// <para>
/// SummonConfiguration defines how a boss summons minions:
/// <list type="bullet">
///   <item><description>Which monster type to summon via <see cref="MonsterDefinitionId"/></description></item>
///   <item><description>How many to summon at once via <see cref="Count"/></description></item>
///   <item><description>How often summons occur via <see cref="IntervalTurns"/></description></item>
///   <item><description>Maximum active summons via <see cref="MaxActive"/></description></item>
/// </list>
/// </para>
/// <para>
/// This is an immutable value object. Use <see cref="Create"/> factory method
/// for validated construction with optional parameters set via fluent methods.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a summon config for skeleton minions
/// var config = SummonConfiguration.Create("skeleton-minion", count: 2)
///     .WithIntervalTurns(3)
///     .WithMaxActive(6);
/// </code>
/// </example>
public readonly record struct SummonConfiguration
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the monster definition ID to summon.
    /// </summary>
    /// <remarks>
    /// References a monster definition that will be instantiated when summoning.
    /// Must match a valid monster ID in the monster configuration.
    /// </remarks>
    public string MonsterDefinitionId { get; init; }

    /// <summary>
    /// Gets the number of monsters to summon per summon action.
    /// </summary>
    /// <remarks>
    /// Defaults to 1. Each summon action spawns this many monsters
    /// (subject to <see cref="MaxActive"/> limit).
    /// </remarks>
    public int Count { get; init; }

    /// <summary>
    /// Gets the interval in turns between summon actions.
    /// </summary>
    /// <remarks>
    /// Defaults to 2. The boss will attempt to summon every N turns
    /// while in a phase with this summon configuration.
    /// </remarks>
    public int IntervalTurns { get; init; }

    /// <summary>
    /// Gets the maximum number of summoned monsters active at once.
    /// </summary>
    /// <remarks>
    /// Defaults to 4. No new summons occur if this limit is reached.
    /// Dead summons do not count toward this limit.
    /// </remarks>
    public int MaxActive { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="SummonConfiguration"/> struct.
    /// </summary>
    /// <remarks>
    /// Private constructor. Use <see cref="Create"/> factory method for construction.
    /// </remarks>
    private SummonConfiguration(string monsterDefinitionId, int count, int intervalTurns, int maxActive)
    {
        MonsterDefinitionId = monsterDefinitionId;
        Count = count;
        IntervalTurns = intervalTurns;
        MaxActive = maxActive;
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new summon configuration with required parameters.
    /// </summary>
    /// <param name="monsterDefinitionId">The monster definition ID to summon.</param>
    /// <param name="count">Number of monsters per summon action (default: 1).</param>
    /// <returns>A new <see cref="SummonConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="monsterDefinitionId"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// var config = SummonConfiguration.Create("fire-elemental", count: 2);
    /// </code>
    /// </example>
    public static SummonConfiguration Create(string monsterDefinitionId, int count = 1)
    {
        if (string.IsNullOrWhiteSpace(monsterDefinitionId))
        {
            throw new ArgumentException("Monster definition ID cannot be null or empty.", nameof(monsterDefinitionId));
        }

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be at least 1.");
        }

        return new SummonConfiguration(monsterDefinitionId, count, intervalTurns: 2, maxActive: 4);
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a copy with a different summon interval.
    /// </summary>
    /// <param name="turns">The number of turns between summon actions.</param>
    /// <returns>A new <see cref="SummonConfiguration"/> with the specified interval.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="turns"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// var config = SummonConfiguration.Create("skeleton")
    ///     .WithIntervalTurns(3); // Summon every 3 turns
    /// </code>
    /// </example>
    public SummonConfiguration WithIntervalTurns(int turns)
    {
        if (turns < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(turns), turns, "Interval must be at least 1 turn.");
        }

        return this with { IntervalTurns = turns };
    }

    /// <summary>
    /// Creates a copy with a different maximum active limit.
    /// </summary>
    /// <param name="max">The maximum number of active summons allowed.</param>
    /// <returns>A new <see cref="SummonConfiguration"/> with the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="max"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// var config = SummonConfiguration.Create("zombie", count: 2)
    ///     .WithMaxActive(8); // Allow up to 8 active zombies
    /// </code>
    /// </example>
    public SummonConfiguration WithMaxActive(int max)
    {
        if (max < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(max), max, "Max active must be at least 1.");
        }

        return this with { MaxActive = max };
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this configuration is valid.
    /// </summary>
    /// <remarks>
    /// A valid configuration has a non-empty monster ID and positive values
    /// for count, interval, and max active.
    /// </remarks>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(MonsterDefinitionId) &&
        Count >= 1 &&
        IntervalTurns >= 1 &&
        MaxActive >= 1;

    /// <summary>
    /// Returns an empty/invalid summon configuration.
    /// </summary>
    /// <remarks>
    /// Used as a default when no summoning is configured for a phase.
    /// Check <see cref="IsValid"/> before using summon configuration.
    /// </remarks>
    public static SummonConfiguration Empty => default;
}
