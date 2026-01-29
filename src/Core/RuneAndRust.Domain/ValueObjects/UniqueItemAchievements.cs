namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks a player's unique item collection progress and achievements.
/// Provides statistics for achievement calculations.
/// </summary>
/// <remarks>
/// <para>
/// UniqueItemAchievements is a mutable tracking object that maintains the state of
/// a player's Myth-Forged item collection across gameplay sessions. It provides
/// methods for recording drops, querying collection status, and calculating
/// progress toward various collection achievements.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Tracks total uniques found vs available</description></item>
///   <item><description>Maintains per-class item counts for ClassMaster achievements</description></item>
///   <item><description>Records timestamps for first and most recent drops</description></item>
///   <item><description>Calculates progress toward all achievement types</description></item>
///   <item><description>Prevents duplicate item tracking with case-insensitive ID matching</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a new tracker
/// var achievements = UniqueItemAchievements.Create(totalUniquesAvailable: 20);
/// 
/// // Record a drop
/// bool isNew = achievements.RecordDrop("shadowfang-blade", new[] { "warrior", "skirmisher" });
/// // isNew == true (first time finding this item)
/// 
/// // Check progress
/// var progress = achievements.GetProgressToward(UniqueAchievementType.CollectorBronze);
/// // 0.2m (1/5 items toward bronze)
/// 
/// // Check earned achievements
/// var earned = achievements.GetEarnedAchievements();
/// // [FirstMythForged] (earned after first drop)
/// </code>
/// </example>
/// <seealso cref="UniqueAchievementType"/>
/// <seealso cref="AchievementThreshold"/>
public sealed class UniqueItemAchievements
{
    #region Fields

    private readonly HashSet<string> _foundItemIds;
    private readonly Dictionary<string, int> _uniquesByClass;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the total number of unique items found across all runs.
    /// </summary>
    /// <value>A non-negative integer count of discovered items.</value>
    public int TotalUniquesFound => _foundItemIds.Count;

    /// <summary>
    /// Gets the total number of unique items available in the game.
    /// </summary>
    /// <value>A non-negative integer representing the complete collection size.</value>
    public int TotalUniquesAvailable { get; private set; }

    /// <summary>
    /// Gets the count of unique items found per class.
    /// </summary>
    /// <value>
    /// A read-only dictionary mapping lowercase class IDs to their respective
    /// found item counts.
    /// </value>
    public IReadOnlyDictionary<string, int> UniquesByClass => _uniquesByClass;

    /// <summary>
    /// Gets when the first Myth-Forged item was found (null if none).
    /// </summary>
    /// <value>
    /// A UTC <see cref="DateTime"/> of the first drop, or <c>null</c> if no
    /// items have been found yet.
    /// </value>
    public DateTime? FirstMythForgedAt { get; private set; }

    /// <summary>
    /// Gets when the most recent unique item was found.
    /// </summary>
    /// <value>
    /// A UTC <see cref="DateTime"/> of the last drop, or <c>null</c> if no
    /// items have been found yet.
    /// </value>
    public DateTime? LastDropAt { get; private set; }

    /// <summary>
    /// Gets the collection completion progress (0.0 = none, 1.0 = complete).
    /// </summary>
    /// <value>
    /// A decimal between 0.0 and 1.0 representing the percentage of items found.
    /// Returns 0 if <see cref="TotalUniquesAvailable"/> is 0.
    /// </value>
    public decimal CollectionProgress =>
        TotalUniquesAvailable > 0
            ? (decimal)TotalUniquesFound / TotalUniquesAvailable
            : 0m;

    /// <summary>
    /// Gets the set of all found unique item IDs.
    /// </summary>
    /// <value>A read-only set of lowercase item IDs.</value>
    public IReadOnlySet<string> FoundItemIds => _foundItemIds;

    /// <summary>
    /// Gets whether the player has found at least one unique.
    /// </summary>
    /// <value><c>true</c> if at least one item has been found; otherwise, <c>false</c>.</value>
    public bool HasFoundAny => TotalUniquesFound > 0;

    #endregion

    #region Constructor

    /// <summary>
    /// Private constructor for creating UniqueItemAchievements instances.
    /// </summary>
    private UniqueItemAchievements()
    {
        _foundItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _uniquesByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new achievements tracker with zero items found.
    /// </summary>
    /// <param name="totalUniquesAvailable">Total unique items available in the game (non-negative).</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A new UniqueItemAchievements instance with zero progress.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="totalUniquesAvailable"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var achievements = UniqueItemAchievements.Create(totalUniquesAvailable: 20);
    /// </code>
    /// </example>
    public static UniqueItemAchievements Create(int totalUniquesAvailable, ILogger? logger = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalUniquesAvailable, nameof(totalUniquesAvailable));

        logger?.LogDebug(
            "Creating UniqueItemAchievements tracker with {TotalAvailable} items available",
            totalUniquesAvailable);

        return new UniqueItemAchievements
        {
            TotalUniquesAvailable = totalUniquesAvailable
        };
    }

    /// <summary>
    /// Creates achievements tracker from existing data (for persistence loading).
    /// </summary>
    /// <param name="totalUniquesAvailable">Total unique items available in the game.</param>
    /// <param name="foundItemIds">Collection of previously found item IDs.</param>
    /// <param name="uniquesByClass">Dictionary of class IDs to found counts.</param>
    /// <param name="firstMythForgedAt">Timestamp of first drop (null if none).</param>
    /// <param name="lastDropAt">Timestamp of most recent drop (null if none).</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A UniqueItemAchievements instance restored from saved data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="totalUniquesAvailable"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="foundItemIds"/> or <paramref name="uniquesByClass"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var achievements = UniqueItemAchievements.CreateFromData(
    ///     totalUniquesAvailable: 20,
    ///     foundItemIds: new[] { "shadowfang-blade", "ironheart-plate" },
    ///     uniquesByClass: new Dictionary&lt;string, int&gt; { ["warrior"] = 2 },
    ///     firstMythForgedAt: new DateTime(2025, 1, 1),
    ///     lastDropAt: new DateTime(2025, 1, 15));
    /// </code>
    /// </example>
    public static UniqueItemAchievements CreateFromData(
        int totalUniquesAvailable,
        IEnumerable<string> foundItemIds,
        Dictionary<string, int> uniquesByClass,
        DateTime? firstMythForgedAt,
        DateTime? lastDropAt,
        ILogger? logger = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalUniquesAvailable, nameof(totalUniquesAvailable));
        ArgumentNullException.ThrowIfNull(foundItemIds, nameof(foundItemIds));
        ArgumentNullException.ThrowIfNull(uniquesByClass, nameof(uniquesByClass));

        var achievements = Create(totalUniquesAvailable);

        foreach (var itemId in foundItemIds)
        {
            achievements._foundItemIds.Add(itemId.ToLowerInvariant());
        }

        foreach (var kvp in uniquesByClass)
        {
            achievements._uniquesByClass[kvp.Key.ToLowerInvariant()] = kvp.Value;
        }

        achievements.FirstMythForgedAt = firstMythForgedAt;
        achievements.LastDropAt = lastDropAt;

        logger?.LogDebug(
            "Restored UniqueItemAchievements from data: {FoundCount}/{TotalAvailable} items, first drop: {FirstDrop}",
            achievements.TotalUniquesFound,
            totalUniquesAvailable,
            firstMythForgedAt?.ToString("yyyy-MM-dd") ?? "none");

        return achievements;
    }

    #endregion

    #region Drop Recording

    /// <summary>
    /// Records a unique item drop.
    /// </summary>
    /// <param name="itemId">The unique item's ID (required).</param>
    /// <param name="classAffinities">Classes the item has affinity for.</param>
    /// <param name="droppedAt">When the item dropped (defaults to UtcNow).</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns><c>true</c> if this was a new item; <c>false</c> if already found.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// bool isNew = achievements.RecordDrop(
    ///     "shadowfang-blade",
    ///     new[] { "warrior", "skirmisher" });
    /// </code>
    /// </example>
    public bool RecordDrop(
        string itemId,
        IReadOnlyList<string> classAffinities,
        DateTime? droppedAt = null,
        ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId, nameof(itemId));

        var normalizedId = itemId.ToLowerInvariant();
        var timestamp = droppedAt ?? DateTime.UtcNow;

        // Check if already found
        if (!_foundItemIds.Add(normalizedId))
        {
            logger?.LogDebug(
                "Item {ItemId} already found, skipping duplicate record",
                normalizedId);
            return false; // Already found
        }

        // Update timestamps
        FirstMythForgedAt ??= timestamp;
        LastDropAt = timestamp;

        // Update class counts
        if (classAffinities != null)
        {
            foreach (var classId in classAffinities)
            {
                var normalizedClassId = classId.ToLowerInvariant();
                _uniquesByClass.TryGetValue(normalizedClassId, out var count);
                _uniquesByClass[normalizedClassId] = count + 1;

                logger?.LogDebug(
                    "Updated class {ClassId} count to {Count}",
                    normalizedClassId,
                    count + 1);
            }
        }

        logger?.LogInformation(
            "New unique item discovered: {ItemId}, total: {TotalFound}/{TotalAvailable}",
            normalizedId,
            TotalUniquesFound,
            TotalUniquesAvailable);

        return true;
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Checks if a specific unique item has been found.
    /// </summary>
    /// <param name="itemId">The item ID to check (required).</param>
    /// <returns><c>true</c> if the item has been found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> is null or whitespace.
    /// </exception>
    public bool HasFound(string itemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId, nameof(itemId));
        return _foundItemIds.Contains(itemId.ToLowerInvariant());
    }

    /// <summary>
    /// Gets the progress toward a specific achievement (0.0 - 1.0).
    /// </summary>
    /// <param name="type">The achievement type to check progress for.</param>
    /// <param name="classId">Class ID for ClassMaster achievements (optional).</param>
    /// <returns>
    /// A decimal between 0.0 and 1.0 representing progress toward the achievement.
    /// Returns 1.0 if the achievement is earned.
    /// </returns>
    /// <example>
    /// <code>
    /// var bronzeProgress = achievements.GetProgressToward(UniqueAchievementType.CollectorBronze);
    /// // 0.6m if 3 of 5 required items found
    /// 
    /// var warriorProgress = achievements.GetProgressToward(UniqueAchievementType.ClassMaster, "warrior");
    /// </code>
    /// </example>
    public decimal GetProgressToward(UniqueAchievementType type, string? classId = null)
    {
        return type switch
        {
            UniqueAchievementType.FirstMythForged =>
                HasFoundAny ? 1.0m : 0.0m,

            UniqueAchievementType.CollectorBronze =>
                Math.Min(1.0m, TotalUniquesFound / 5.0m),

            UniqueAchievementType.CollectorSilver =>
                Math.Min(1.0m, TotalUniquesFound / 15.0m),

            UniqueAchievementType.CollectorGold =>
                CollectionProgress,

            UniqueAchievementType.ClassMaster when classId != null =>
                GetClassProgress(classId),

            _ => 0m
        };
    }

    /// <summary>
    /// Checks if an achievement has been earned.
    /// </summary>
    /// <param name="type">The achievement type to check.</param>
    /// <param name="classId">Class ID for ClassMaster achievements (optional).</param>
    /// <returns><c>true</c> if the achievement has been earned; otherwise, <c>false</c>.</returns>
    public bool IsAchievementEarned(UniqueAchievementType type, string? classId = null) =>
        GetProgressToward(type, classId) >= 1.0m;

    /// <summary>
    /// Gets all earned achievements.
    /// </summary>
    /// <returns>
    /// A read-only list of achievement types that have been earned.
    /// Does not include ClassMaster achievements as they require class-specific checking.
    /// </returns>
    public IReadOnlyList<UniqueAchievementType> GetEarnedAchievements()
    {
        var earned = new List<UniqueAchievementType>();

        if (IsAchievementEarned(UniqueAchievementType.FirstMythForged))
        {
            earned.Add(UniqueAchievementType.FirstMythForged);
        }

        if (IsAchievementEarned(UniqueAchievementType.CollectorBronze))
        {
            earned.Add(UniqueAchievementType.CollectorBronze);
        }

        if (IsAchievementEarned(UniqueAchievementType.CollectorSilver))
        {
            earned.Add(UniqueAchievementType.CollectorSilver);
        }

        if (IsAchievementEarned(UniqueAchievementType.CollectorGold))
        {
            earned.Add(UniqueAchievementType.CollectorGold);
        }

        return earned;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets progress toward a class-specific collection achievement.
    /// </summary>
    /// <param name="classId">The class ID to check progress for.</param>
    /// <returns>Progress from 0.0 to 1.0 based on class items found.</returns>
    private decimal GetClassProgress(string classId)
    {
        // This would require knowing total class-specific items
        // For now, return partial progress if any items found for class
        var normalizedClassId = classId.ToLowerInvariant();
        if (!_uniquesByClass.TryGetValue(normalizedClassId, out var found))
        {
            return 0m;
        }

        // Placeholder: actual implementation would need class-specific total from configuration
        // For now, indicate partial progress if any items found
        return found > 0 ? 0.5m : 0m;
    }

    #endregion

    #region Object Overrides

    /// <summary>
    /// Returns a string representation of the achievements state.
    /// </summary>
    /// <returns>A string containing found count, available count, and progress percentage.</returns>
    public override string ToString() =>
        $"UniqueItemAchievements[Found: {TotalUniquesFound}/{TotalUniquesAvailable}, Progress: {CollectionProgress:P0}]";

    #endregion
}
