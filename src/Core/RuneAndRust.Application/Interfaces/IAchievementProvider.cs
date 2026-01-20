// ═══════════════════════════════════════════════════════════════════════════════
// IAchievementProvider.cs
// Interface for providing achievement definitions loaded from configuration.
// Version: 0.12.1a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to achievement definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the data source for achievement definitions,
/// following the provider pattern established by <see cref="IQualityTierProvider"/>
/// and similar infrastructure services. Implementations typically load definitions
/// from JSON configuration files using lazy loading with caching.
/// </para>
/// <para>
/// The provider enables:
/// </para>
/// <list type="bullet">
///   <item><description>Unit testing with mock data</description></item>
///   <item><description>Different configuration sources (JSON, database, etc.)</description></item>
///   <item><description>Cached definitions for performance</description></item>
/// </list>
/// <para>
/// Achievement providers are typically registered as singletons in the DI container
/// since the definitions are loaded once and remain constant for the application lifetime.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get all achievements
/// var provider = serviceProvider.GetRequiredService&lt;IAchievementProvider&gt;();
/// var allAchievements = provider.GetAllAchievements();
///
/// // Get a specific achievement by ID
/// var firstBlood = provider.GetAchievementById("first-blood");
///
/// // Get achievements by category for display
/// var combatAchievements = provider.GetAchievementsByCategory(AchievementCategory.Combat);
///
/// // Get total possible achievement points
/// int maxPoints = provider.GetMaxPossiblePoints();
/// </code>
/// </example>
public interface IAchievementProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Collection Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all achievement definitions.
    /// </summary>
    /// <returns>
    /// A read-only list of all loaded <see cref="AchievementDefinition"/> instances.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns all achievements from the configuration file. The list is cached
    /// after the first call for optimal performance.
    /// </para>
    /// <para>
    /// If the configuration file is missing or invalid, returns an empty list
    /// rather than throwing an exception.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var achievements = provider.GetAllAchievements();
    /// Console.WriteLine($"Total achievements: {achievements.Count}");
    /// foreach (var achievement in achievements)
    /// {
    ///     Console.WriteLine($"- {achievement.Name} ({achievement.Tier})");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<AchievementDefinition> GetAllAchievements();

    // ═══════════════════════════════════════════════════════════════════════════
    // Retrieval Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an achievement definition by its string ID.
    /// </summary>
    /// <param name="achievementId">The achievement ID to look up (e.g., "first-blood").</param>
    /// <returns>
    /// The <see cref="AchievementDefinition"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Achievement IDs are the stable string identifiers used in configuration files.
    /// This method performs a case-sensitive lookup.
    /// </para>
    /// <para>
    /// Returns null rather than throwing an exception if the achievement is not found,
    /// allowing callers to safely check for existence.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var achievement = provider.GetAchievementById("monster-slayer");
    /// if (achievement != null)
    /// {
    ///     Console.WriteLine($"Found: {achievement.Name} - {achievement.Description}");
    /// }
    /// </code>
    /// </example>
    AchievementDefinition? GetAchievementById(string achievementId);

    // ═══════════════════════════════════════════════════════════════════════════
    // Filter Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all achievements in a specific category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>
    /// A read-only list of achievements in the specified category.
    /// Returns an empty list if no achievements match.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is useful for displaying achievements grouped by category
    /// in the achievement UI (Combat, Exploration, Progression, etc.).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var combatAchievements = provider.GetAchievementsByCategory(AchievementCategory.Combat);
    /// Console.WriteLine($"Combat achievements: {combatAchievements.Count}");
    /// </code>
    /// </example>
    IReadOnlyList<AchievementDefinition> GetAchievementsByCategory(AchievementCategory category);

    /// <summary>
    /// Gets all achievements of a specific tier.
    /// </summary>
    /// <param name="tier">The tier to filter by.</param>
    /// <returns>
    /// A read-only list of achievements of the specified tier.
    /// Returns an empty list if no achievements match.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is useful for displaying achievements grouped by difficulty/rarity
    /// (Bronze, Silver, Gold, Platinum).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var platinumAchievements = provider.GetAchievementsByTier(AchievementTier.Platinum);
    /// Console.WriteLine($"Platinum achievements: {platinumAchievements.Count}");
    /// </code>
    /// </example>
    IReadOnlyList<AchievementDefinition> GetAchievementsByTier(AchievementTier tier);

    // ═══════════════════════════════════════════════════════════════════════════
    // Aggregation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of achievements.
    /// </summary>
    /// <returns>The count of all achievement definitions.</returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to <c>GetAllAchievements().Count</c>.
    /// Useful for displaying achievement completion progress (e.g., "5/20 achievements").
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// int total = provider.GetAchievementCount();
    /// Console.WriteLine($"Total achievements available: {total}");
    /// </code>
    /// </example>
    int GetAchievementCount();

    /// <summary>
    /// Gets the maximum possible achievement points.
    /// </summary>
    /// <returns>
    /// The sum of all achievement point values (based on their tiers).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Points are determined by achievement tier: Bronze=10, Silver=25, Gold=50, Platinum=100.
    /// This method sums all achievement point values.
    /// </para>
    /// <para>
    /// Useful for displaying achievement point progress (e.g., "150/500 points").
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// int maxPoints = provider.GetMaxPossiblePoints();
    /// Console.WriteLine($"Maximum achievement points: {maxPoints}");
    /// </code>
    /// </example>
    int GetMaxPossiblePoints();
}
