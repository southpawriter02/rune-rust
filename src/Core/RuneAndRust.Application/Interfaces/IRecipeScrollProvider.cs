using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to recipe scroll configurations loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the loading and retrieval of recipe scroll configurations,
/// allowing for different implementations (JSON, database, etc.) while maintaining
/// a consistent API for the application layer.
/// </para>
/// <para>
/// Recipe scroll providers are typically registered as singletons in the DI container,
/// loading all configurations once at startup and caching them for efficient access.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Case-insensitive recipe ID lookups</description></item>
///   <item><description>Filtering by dungeon level for level-appropriate drops</description></item>
///   <item><description>Filtering by loot source type for source-specific drops</description></item>
///   <item><description>Combined level and source filtering for eligible scrolls</description></item>
///   <item><description>Configurable drop chances per loot source</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get all eligible scrolls for a level 5 chest
/// var eligibleScrolls = scrollProvider.GetEligibleScrolls(5, LootSourceType.Chest);
///
/// // Check drop chance for chests
/// var chance = scrollProvider.GetDropChance(LootSourceType.Chest);
/// if (random.NextDouble() &lt; chance)
/// {
///     // Roll for which scroll drops
///     var scroll = SelectWeightedRandom(eligibleScrolls);
/// }
///
/// // Get scroll config for specific recipe
/// var config = scrollProvider.GetScrollConfig("steel-sword");
/// if (config != null)
/// {
///     Console.WriteLine($"Drops at level {config.MinDungeonLevel}+ with weight {config.DropWeight}");
/// }
/// </code>
/// </example>
public interface IRecipeScrollProvider
{
    /// <summary>
    /// Gets a scroll configuration by its recipe identifier.
    /// </summary>
    /// <param name="recipeId">The recipe identifier (case-insensitive).</param>
    /// <returns>The scroll configuration, or null if no scroll exists for this recipe.</returns>
    /// <remarks>
    /// Not all recipes have scroll configurations - default recipes are typically
    /// learned without scrolls.
    /// </remarks>
    /// <example>
    /// <code>
    /// var config = scrollProvider.GetScrollConfig("steel-sword");
    /// if (config != null)
    /// {
    ///     Console.WriteLine($"Steel Sword scroll drops at level {config.MinDungeonLevel}+");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("No scroll exists for this recipe.");
    /// }
    /// </code>
    /// </example>
    RecipeScrollConfig? GetScrollConfig(string recipeId);

    /// <summary>
    /// Gets all registered scroll configurations.
    /// </summary>
    /// <returns>A read-only list of all scroll configurations.</returns>
    /// <remarks>
    /// The returned list is a snapshot and will not reflect any subsequent changes
    /// if the provider supports dynamic reloading.
    /// </remarks>
    IReadOnlyList<RecipeScrollConfig> GetAllScrollConfigs();

    /// <summary>
    /// Gets all scrolls that can drop at a specific dungeon level.
    /// </summary>
    /// <param name="dungeonLevel">The dungeon level to filter by.</param>
    /// <returns>A read-only list of scroll configs eligible at the specified level.</returns>
    /// <remarks>
    /// Returns scrolls where MinDungeonLevel &lt;= dungeonLevel and
    /// (MaxDungeonLevel is null OR MaxDungeonLevel &gt;= dungeonLevel).
    /// </remarks>
    /// <example>
    /// <code>
    /// var level5Scrolls = scrollProvider.GetScrollsForLevel(5);
    /// Console.WriteLine($"Possible scrolls at level 5: {level5Scrolls.Count}");
    /// foreach (var scroll in level5Scrolls)
    /// {
    ///     Console.WriteLine($"- {scroll.RecipeId} (weight: {scroll.DropWeight})");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeScrollConfig> GetScrollsForLevel(int dungeonLevel);

    /// <summary>
    /// Gets all scrolls that can drop from a specific loot source.
    /// </summary>
    /// <param name="source">The loot source type to filter by.</param>
    /// <returns>A read-only list of scroll configs eligible from the specified source.</returns>
    /// <example>
    /// <code>
    /// var bossScrolls = scrollProvider.GetScrollsForSource(LootSourceType.Boss);
    /// Console.WriteLine($"Boss-only scrolls: {bossScrolls.Count}");
    /// </code>
    /// </example>
    IReadOnlyList<RecipeScrollConfig> GetScrollsForSource(LootSourceType source);

    /// <summary>
    /// Gets all scrolls eligible to drop at a specific level from a specific source.
    /// </summary>
    /// <param name="dungeonLevel">The dungeon level to filter by.</param>
    /// <param name="source">The loot source type to filter by.</param>
    /// <returns>A read-only list of scroll configs meeting both criteria.</returns>
    /// <remarks>
    /// This is the primary method used for loot generation, combining both
    /// level and source requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get scrolls that can drop from a level 5 chest
    /// var eligibleScrolls = scrollProvider.GetEligibleScrolls(5, LootSourceType.Chest);
    /// if (eligibleScrolls.Count > 0)
    /// {
    ///     // Perform weighted selection
    ///     var selectedScroll = SelectByWeight(eligibleScrolls);
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeScrollConfig> GetEligibleScrolls(int dungeonLevel, LootSourceType source);

    /// <summary>
    /// Gets the drop chance for recipe scrolls from a specific loot source.
    /// </summary>
    /// <param name="source">The loot source type.</param>
    /// <returns>The drop chance as a decimal (0.0 to 1.0), or 0 if not configured.</returns>
    /// <remarks>
    /// <para>
    /// This is the base chance that any recipe scroll will drop from this source.
    /// If the roll succeeds, a specific scroll is then selected from eligible scrolls.
    /// </para>
    /// <para>
    /// Typical values:
    /// <list type="bullet">
    ///   <item><description>Chest: 0.15 (15%)</description></item>
    ///   <item><description>Boss: 0.30 (30%)</description></item>
    ///   <item><description>Monster: 0.02 (2%)</description></item>
    ///   <item><description>Quest: 0.50 (50%)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dropChance = scrollProvider.GetDropChance(LootSourceType.Chest);
    /// if (random.NextDouble() &lt; (double)dropChance)
    /// {
    ///     // A scroll will drop - now select which one
    /// }
    /// </code>
    /// </example>
    decimal GetDropChance(LootSourceType source);

    /// <summary>
    /// Checks if a scroll configuration exists for the specified recipe.
    /// </summary>
    /// <param name="recipeId">The recipe identifier (case-insensitive).</param>
    /// <returns>True if a scroll exists for this recipe, false otherwise.</returns>
    /// <example>
    /// <code>
    /// if (scrollProvider.HasScrollConfig("mithril-blade"))
    /// {
    ///     Console.WriteLine("Mithril Blade can be found as a scroll!");
    /// }
    /// </code>
    /// </example>
    bool HasScrollConfig(string recipeId);

    /// <summary>
    /// Gets the total count of registered scroll configurations.
    /// </summary>
    /// <returns>The number of scroll configurations loaded.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total discoverable recipes: {scrollProvider.GetScrollCount()}");
    /// </code>
    /// </example>
    int GetScrollCount();
}
