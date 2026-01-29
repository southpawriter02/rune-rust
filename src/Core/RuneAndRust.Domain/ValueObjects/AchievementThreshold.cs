namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the requirements for earning a unique item achievement.
/// </summary>
/// <remarks>
/// <para>
/// AchievementThreshold is an immutable value object that encapsulates the
/// criteria required to earn a specific unique item collection achievement.
/// </para>
/// <para>
/// Two factory methods are provided for different achievement types:
/// <list type="bullet">
///   <item><description><see cref="CreateCountBased"/>: For achievements based on item counts</description></item>
///   <item><description><see cref="CreateClassBased"/>: For class-specific achievements</description></item>
/// </list>
/// </para>
/// <para>
/// Pre-defined thresholds are available through the <see cref="Defaults"/> class.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a count-based threshold
/// var bronzeThreshold = AchievementThreshold.CreateCountBased(
///     UniqueAchievementType.CollectorBronze,
///     "Collector: Bronze",
///     "Find 5 unique Myth-Forged items",
///     requiredCount: 5);
/// 
/// // Create a class-based threshold
/// var warriorMaster = AchievementThreshold.CreateClassBased(
///     "Warrior Master",
///     "Find all warrior uniques",
///     "warrior");
/// 
/// // Use pre-defined defaults
/// var allThresholds = AchievementThreshold.Defaults.All;
/// </code>
/// </example>
/// <seealso cref="UniqueAchievementType"/>
/// <seealso cref="UniqueItemAchievements"/>
public readonly record struct AchievementThreshold
{
    #region Properties

    /// <summary>
    /// Gets the achievement type.
    /// </summary>
    /// <value>The <see cref="UniqueAchievementType"/> this threshold applies to.</value>
    public UniqueAchievementType Type { get; }

    /// <summary>
    /// Gets the display name for the achievement.
    /// </summary>
    /// <value>A human-readable name suitable for UI display.</value>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the description of how to earn this achievement.
    /// </summary>
    /// <value>A descriptive string explaining the achievement requirements.</value>
    public string Description { get; }

    /// <summary>
    /// Gets the number of items required (0 for non-count based achievements).
    /// </summary>
    /// <value>
    /// A non-negative integer representing the required item count.
    /// For <see cref="UniqueAchievementType.CollectorGold"/>, this is 0 as
    /// the count is dynamically determined based on total available items.
    /// </value>
    public int RequiredCount { get; }

    /// <summary>
    /// Gets the class ID for ClassMaster achievement (null for others).
    /// </summary>
    /// <value>
    /// A lowercase class identifier for <see cref="UniqueAchievementType.ClassMaster"/> achievements;
    /// <c>null</c> for all other achievement types.
    /// </value>
    public string? ClassId { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Private constructor for creating AchievementThreshold instances.
    /// </summary>
    /// <param name="type">The achievement type.</param>
    /// <param name="displayName">Display name for the achievement.</param>
    /// <param name="description">Description of how to earn the achievement.</param>
    /// <param name="requiredCount">Number of items required.</param>
    /// <param name="classId">Class ID for ClassMaster achievements.</param>
    private AchievementThreshold(
        UniqueAchievementType type,
        string displayName,
        string description,
        int requiredCount,
        string? classId)
    {
        Type = type;
        DisplayName = displayName;
        Description = description;
        RequiredCount = requiredCount;
        ClassId = classId;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a count-based achievement threshold.
    /// </summary>
    /// <param name="type">The achievement type.</param>
    /// <param name="displayName">Display name for the achievement (required).</param>
    /// <param name="description">Description of how to earn the achievement.</param>
    /// <param name="requiredCount">Number of items required (non-negative).</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A new validated AchievementThreshold instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="requiredCount"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var threshold = AchievementThreshold.CreateCountBased(
    ///     UniqueAchievementType.CollectorBronze,
    ///     "Collector: Bronze",
    ///     "Find 5 unique Myth-Forged items",
    ///     requiredCount: 5);
    /// </code>
    /// </example>
    public static AchievementThreshold CreateCountBased(
        UniqueAchievementType type,
        string displayName,
        string description,
        int requiredCount,
        ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentOutOfRangeException.ThrowIfNegative(requiredCount, nameof(requiredCount));

        logger?.LogDebug(
            "Creating count-based achievement threshold: {Type} requiring {RequiredCount} items",
            type,
            requiredCount);

        return new AchievementThreshold(type, displayName, description, requiredCount, null);
    }

    /// <summary>
    /// Creates a class-specific achievement threshold.
    /// </summary>
    /// <param name="displayName">Display name for the achievement (required).</param>
    /// <param name="description">Description of how to earn the achievement.</param>
    /// <param name="classId">The class ID (required, will be normalized to lowercase).</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A new validated AchievementThreshold instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/> or <paramref name="classId"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var threshold = AchievementThreshold.CreateClassBased(
    ///     "Warrior Master",
    ///     "Find all warrior uniques",
    ///     "warrior");
    /// </code>
    /// </example>
    public static AchievementThreshold CreateClassBased(
        string displayName,
        string description,
        string classId,
        ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentException.ThrowIfNullOrWhiteSpace(classId, nameof(classId));

        var normalizedClassId = classId.ToLowerInvariant();

        logger?.LogDebug(
            "Creating class-based achievement threshold for class {ClassId}: {DisplayName}",
            normalizedClassId,
            displayName);

        return new AchievementThreshold(
            UniqueAchievementType.ClassMaster,
            displayName,
            description,
            requiredCount: 0,
            normalizedClassId);
    }

    #endregion

    #region Default Thresholds

    /// <summary>
    /// Pre-defined thresholds for standard achievements.
    /// </summary>
    /// <remarks>
    /// Contains the four standard collection achievement thresholds as defined
    /// in the design specification. ClassMaster achievements are not included
    /// as they require dynamic class ID specification.
    /// </remarks>
    public static class Defaults
    {
        /// <summary>
        /// Threshold for the first Myth-Forged drop achievement.
        /// </summary>
        public static readonly AchievementThreshold FirstMythForged =
            CreateCountBased(
                UniqueAchievementType.FirstMythForged,
                "First Legend",
                "Find your first Myth-Forged item",
                requiredCount: 1);

        /// <summary>
        /// Threshold for the bronze collector achievement (5 items).
        /// </summary>
        public static readonly AchievementThreshold CollectorBronze =
            CreateCountBased(
                UniqueAchievementType.CollectorBronze,
                "Collector: Bronze",
                "Find 5 unique Myth-Forged items",
                requiredCount: 5);

        /// <summary>
        /// Threshold for the silver collector achievement (15 items).
        /// </summary>
        public static readonly AchievementThreshold CollectorSilver =
            CreateCountBased(
                UniqueAchievementType.CollectorSilver,
                "Collector: Silver",
                "Find 15 unique Myth-Forged items",
                requiredCount: 15);

        /// <summary>
        /// Threshold for the gold collector achievement (all items).
        /// </summary>
        /// <remarks>
        /// RequiredCount is 0 as the actual count is dynamically determined
        /// based on <see cref="UniqueItemAchievements.TotalUniquesAvailable"/>.
        /// </remarks>
        public static readonly AchievementThreshold CollectorGold =
            CreateCountBased(
                UniqueAchievementType.CollectorGold,
                "Collector: Gold",
                "Find all Myth-Forged items in the game",
                requiredCount: 0);

        /// <summary>
        /// Gets all default thresholds as a read-only list.
        /// </summary>
        /// <value>An array containing the four standard achievement thresholds.</value>
        public static IReadOnlyList<AchievementThreshold> All =>
            new[] { FirstMythForged, CollectorBronze, CollectorSilver, CollectorGold };
    }

    #endregion

    #region Instance Methods

    /// <summary>
    /// Returns a string representation of the achievement threshold.
    /// </summary>
    /// <returns>A string containing the type and display name.</returns>
    public override string ToString() =>
        $"AchievementThreshold[{Type}: {DisplayName}]";

    #endregion
}
