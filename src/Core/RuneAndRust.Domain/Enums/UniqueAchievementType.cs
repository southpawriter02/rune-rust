namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Achievement types related to unique item collection.
/// Defines milestones for tracking Myth-Forged item discoveries.
/// </summary>
/// <remarks>
/// <para>
/// This enum provides the foundation for the achievement system's unique item
/// collection tracking. Each achievement type represents a milestone that players
/// can earn by discovering Myth-Forged items throughout their gameplay.
/// </para>
/// <para>
/// Achievement thresholds:
/// <list type="bullet">
///   <item><description><see cref="FirstMythForged"/>: Awarded on first legendary drop</description></item>
///   <item><description><see cref="CollectorBronze"/>: Awarded at 5 unique items</description></item>
///   <item><description><see cref="CollectorSilver"/>: Awarded at 15 unique items</description></item>
///   <item><description><see cref="CollectorGold"/>: Awarded when all unique items are found</description></item>
///   <item><description><see cref="ClassMaster"/>: Awarded when all class-specific uniques are found</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.AchievementThreshold"/>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.UniqueItemAchievements"/>
public enum UniqueAchievementType
{
    /// <summary>
    /// No achievement (default value).
    /// </summary>
    /// <remarks>
    /// Used as a sentinel value when no specific achievement is referenced.
    /// </remarks>
    None = 0,

    /// <summary>
    /// First Myth-Forged item ever found.
    /// </summary>
    /// <remarks>
    /// Awarded immediately when the player obtains their first legendary drop.
    /// This is a milestone achievement that celebrates beginning the collection journey.
    /// </remarks>
    FirstMythForged = 1,

    /// <summary>
    /// Found 5 unique items (bronze tier collector).
    /// </summary>
    /// <remarks>
    /// The entry-level collector achievement, recognizing players who have
    /// begun amassing a meaningful collection of Myth-Forged items.
    /// </remarks>
    CollectorBronze = 2,

    /// <summary>
    /// Found 15 unique items (silver tier collector).
    /// </summary>
    /// <remarks>
    /// Mid-tier collector achievement for dedicated players who have
    /// discovered a substantial portion of available unique items.
    /// </remarks>
    CollectorSilver = 3,

    /// <summary>
    /// Found all unique items in the game (gold tier collector).
    /// </summary>
    /// <remarks>
    /// The ultimate collection achievement, awarded only to completionists
    /// who have discovered every Myth-Forged item available in the game.
    /// The required count is dynamically determined based on total available uniques.
    /// </remarks>
    CollectorGold = 4,

    /// <summary>
    /// Found all class-specific unique items for a particular class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A specialized achievement that tracks collection of items with affinity
    /// for a specific archetype (e.g., Warrior, Skirmisher, Mystic, Adept).
    /// </para>
    /// <para>
    /// Unlike other achievements, this requires a class ID to be specified
    /// when checking progress or earned status.
    /// </para>
    /// </remarks>
    ClassMaster = 5
}
