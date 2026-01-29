namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Context for Myth-Forged generation, capturing all relevant parameters.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MythForgedContext"/> encapsulates all information needed to attempt
/// Myth-Forged (unique item) generation, including the drop source, player class
/// for affinity filtering, and player level for requirement validation.
/// </para>
/// <para>
/// The context is immutable and uses a factory method pattern to ensure validation
/// and ID normalization occur at creation time. All IDs are normalized to lowercase
/// for case-insensitive comparison throughout the system.
/// </para>
/// <para>
/// An optional <see cref="RandomSeed"/> can be provided for deterministic testing,
/// allowing unit tests to verify specific generation scenarios.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create context for a boss drop with class affinity
/// var context = MythForgedContext.Create(
///     DropSourceType.Boss,
///     "shadow-lord",
///     playerClassId: "warrior",
///     playerLevel: 10);
///
/// // Create context for deterministic testing
/// var testContext = MythForgedContext.Create(
///     DropSourceType.BossChest,
///     "ancient-treasury",
///     playerClassId: "mage",
///     playerLevel: 15,
///     randomSeed: 42);
/// </code>
/// </example>
public readonly record struct MythForgedContext
{
    /// <summary>
    /// Gets the type of source triggering the drop.
    /// </summary>
    /// <value>
    /// The <see cref="DropSourceType"/> indicating whether this is a boss,
    /// container, quest, or other source type.
    /// </value>
    /// <remarks>
    /// Source type affects drop chance calculations. Bosses have higher chances
    /// (70%) while regular containers have lower chances (2%).
    /// </remarks>
    public DropSourceType SourceType { get; }

    /// <summary>
    /// Gets the specific source identifier.
    /// </summary>
    /// <value>
    /// A lowercase, normalized identifier for the specific drop source
    /// (e.g., "shadow-lord" for a boss, "ancient-chest" for a container).
    /// </value>
    /// <remarks>
    /// The source ID is used to filter unique items that can drop from this
    /// specific source. Some unique items are restricted to particular bosses
    /// or container types.
    /// </remarks>
    public string SourceId { get; }

    /// <summary>
    /// Gets the player's current class ID for affinity filtering.
    /// </summary>
    /// <value>
    /// A lowercase, normalized class identifier (e.g., "warrior", "mage"),
    /// or <c>null</c> if no class filtering should be applied.
    /// </value>
    /// <remarks>
    /// <para>
    /// When provided, there is a 60% chance that the available unique pool
    /// will be filtered to items with affinity for this class, biasing drops
    /// toward class-appropriate gear.
    /// </para>
    /// <para>
    /// If the filtered pool would be empty, the full pool is used instead.
    /// </para>
    /// </remarks>
    public string? PlayerClassId { get; }

    /// <summary>
    /// Gets the player's current level for requirement filtering.
    /// </summary>
    /// <value>
    /// The player's level, which must be at least 1.
    /// </value>
    /// <remarks>
    /// Unique items may have level requirements. Items with requirements higher
    /// than the player's current level are excluded from the available pool.
    /// </remarks>
    public int PlayerLevel { get; }

    /// <summary>
    /// Gets the optional random seed for deterministic testing.
    /// </summary>
    /// <value>
    /// An optional seed value for the random number generator, or <c>null</c>
    /// for non-deterministic generation.
    /// </value>
    /// <remarks>
    /// <para>
    /// When provided, the Myth-Forged service uses a seeded random number
    /// generator, enabling reproducible results in unit tests.
    /// </para>
    /// <para>
    /// Production code should leave this <c>null</c> for true randomness.
    /// </para>
    /// </remarks>
    public int? RandomSeed { get; }

    /// <summary>
    /// Private constructor to enforce factory method usage.
    /// </summary>
    private MythForgedContext(
        DropSourceType sourceType,
        string sourceId,
        string? playerClassId,
        int playerLevel,
        int? randomSeed)
    {
        SourceType = sourceType;
        SourceId = sourceId;
        PlayerClassId = playerClassId;
        PlayerLevel = playerLevel;
        RandomSeed = randomSeed;
    }

    /// <summary>
    /// Creates a new <see cref="MythForgedContext"/> with validation.
    /// </summary>
    /// <param name="sourceType">The type of drop source.</param>
    /// <param name="sourceId">The specific source identifier (will be normalized to lowercase).</param>
    /// <param name="playerClassId">Optional player class for affinity filtering (will be normalized to lowercase).</param>
    /// <param name="playerLevel">The player's current level (must be at least 1).</param>
    /// <param name="randomSeed">Optional seed for deterministic testing.</param>
    /// <returns>A validated <see cref="MythForgedContext"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sourceId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="playerLevel"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// // Basic context for a boss encounter
    /// var context = MythForgedContext.Create(
    ///     DropSourceType.Boss,
    ///     "SHADOW-LORD",  // Normalized to "shadow-lord"
    ///     "WARRIOR",      // Normalized to "warrior"
    ///     playerLevel: 10);
    ///
    /// // Context with random seed for testing
    /// var testContext = MythForgedContext.Create(
    ///     DropSourceType.Container,
    ///     "legendary-chest",
    ///     randomSeed: 12345);
    /// </code>
    /// </example>
    public static MythForgedContext Create(
        DropSourceType sourceType,
        string sourceId,
        string? playerClassId = null,
        int playerLevel = 1,
        int? randomSeed = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId, nameof(sourceId));
        ArgumentOutOfRangeException.ThrowIfLessThan(playerLevel, 1, nameof(playerLevel));

        return new MythForgedContext(
            sourceType,
            sourceId.ToLowerInvariant(),
            playerClassId?.ToLowerInvariant(),
            playerLevel,
            randomSeed);
    }

    /// <summary>
    /// Returns a string representation of the context for debugging.
    /// </summary>
    /// <returns>
    /// A string in the format "MythForgedContext[{SourceType}:{SourceId}, Class:{ClassId}, Lv:{Level}]".
    /// </returns>
    /// <example>
    /// <code>
    /// var context = MythForgedContext.Create(DropSourceType.Boss, "shadow-lord", "warrior", 10);
    /// Console.WriteLine(context.ToString());
    /// // Output: MythForgedContext[Boss:shadow-lord, Class:warrior, Lv:10]
    /// </code>
    /// </example>
    public override string ToString() =>
        $"MythForgedContext[{SourceType}:{SourceId}, Class:{PlayerClassId ?? "none"}, Lv:{PlayerLevel}]";
}
