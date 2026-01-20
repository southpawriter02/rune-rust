using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Context information for loot generation operations.
/// </summary>
/// <remarks>
/// <para>
/// LootContext provides all the information needed to determine what loot can be
/// generated, including the source of the loot, the current dungeon level, and
/// optionally the player receiving the loot.
/// </para>
/// <para>
/// Use cases:
/// <list type="bullet">
///   <item><description>Recipe scroll generation uses level and source to filter eligible scrolls</description></item>
///   <item><description>When player is provided and ExcludeKnownRecipes is true, already-known recipes are excluded</description></item>
///   <item><description>Different loot sources have different drop rates and eligible items</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Generate loot from a chest at level 5
/// var context = LootContext.Chest(dungeonLevel: 5, player: currentPlayer);
/// var scroll = lootService.TryGenerateRecipeScroll(context);
///
/// // Generate boss loot excluding recipes the player already knows
/// var bossContext = LootContext.Boss(level: 10, player: player, excludeKnown: true);
/// </code>
/// </example>
public sealed record LootContext
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the type of source generating this loot.
    /// </summary>
    /// <remarks>
    /// The source type affects which items can drop and at what rates.
    /// For example, bosses have higher drop rates for rare items than regular monsters.
    /// </remarks>
    public LootSourceType Source { get; init; }

    /// <summary>
    /// Gets the dungeon level where the loot is being generated.
    /// </summary>
    /// <remarks>
    /// Many items have minimum level requirements. The dungeon level is used
    /// to filter out items that shouldn't appear at lower levels.
    /// </remarks>
    public int DungeonLevel { get; init; }

    /// <summary>
    /// Gets the player receiving the loot, or null if not applicable.
    /// </summary>
    /// <remarks>
    /// When provided, enables player-specific filtering such as excluding
    /// recipes the player already knows. Null for generic loot generation.
    /// </remarks>
    public Player? Player { get; init; }

    /// <summary>
    /// Gets whether to exclude recipes the player already knows.
    /// </summary>
    /// <remarks>
    /// When true and Player is provided, recipe scrolls for recipes the player
    /// already knows will not be included in the eligible pool. This prevents
    /// duplicate scrolls from dropping.
    /// </remarks>
    public bool ExcludeKnownRecipes { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a loot context for chest loot generation.
    /// </summary>
    /// <param name="level">The dungeon level where the chest is located.</param>
    /// <param name="player">Optional player to enable filtering (default: null).</param>
    /// <param name="excludeKnown">Whether to exclude already-known recipes (default: false).</param>
    /// <returns>A LootContext configured for chest loot.</returns>
    /// <remarks>
    /// Chests have moderate drop rates for recipe scrolls (typically 15%).
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = LootContext.Chest(5);
    /// var contextWithPlayer = LootContext.Chest(5, player, excludeKnown: true);
    /// </code>
    /// </example>
    public static LootContext Chest(int level, Player? player = null, bool excludeKnown = false)
        => new()
        {
            Source = LootSourceType.Chest,
            DungeonLevel = level,
            Player = player,
            ExcludeKnownRecipes = excludeKnown
        };

    /// <summary>
    /// Creates a loot context for boss loot generation.
    /// </summary>
    /// <param name="level">The dungeon level of the boss.</param>
    /// <param name="player">Optional player to enable filtering (default: null).</param>
    /// <param name="excludeKnown">Whether to exclude already-known recipes (default: false).</param>
    /// <returns>A LootContext configured for boss loot.</returns>
    /// <remarks>
    /// Bosses have higher drop rates for recipe scrolls (typically 30%).
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = LootContext.Boss(10, player);
    /// </code>
    /// </example>
    public static LootContext Boss(int level, Player? player = null, bool excludeKnown = false)
        => new()
        {
            Source = LootSourceType.Boss,
            DungeonLevel = level,
            Player = player,
            ExcludeKnownRecipes = excludeKnown
        };

    /// <summary>
    /// Creates a loot context for monster loot generation.
    /// </summary>
    /// <param name="level">The dungeon level where the monster was defeated.</param>
    /// <param name="player">Optional player to enable filtering (default: null).</param>
    /// <param name="excludeKnown">Whether to exclude already-known recipes (default: false).</param>
    /// <returns>A LootContext configured for monster loot.</returns>
    /// <remarks>
    /// Regular monsters have low drop rates for recipe scrolls (typically 2%).
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = LootContext.Monster(3, player);
    /// </code>
    /// </example>
    public static LootContext Monster(int level, Player? player = null, bool excludeKnown = false)
        => new()
        {
            Source = LootSourceType.Monster,
            DungeonLevel = level,
            Player = player,
            ExcludeKnownRecipes = excludeKnown
        };

    /// <summary>
    /// Creates a loot context for quest reward generation.
    /// </summary>
    /// <param name="level">The effective level of the quest reward.</param>
    /// <param name="player">Optional player to enable filtering (default: null).</param>
    /// <param name="excludeKnown">Whether to exclude already-known recipes (default: false).</param>
    /// <returns>A LootContext configured for quest rewards.</returns>
    /// <remarks>
    /// Quest rewards have the highest drop rates for recipe scrolls (typically 50%).
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = LootContext.Quest(8, player, excludeKnown: true);
    /// </code>
    /// </example>
    public static LootContext Quest(int level, Player? player = null, bool excludeKnown = false)
        => new()
        {
            Source = LootSourceType.Quest,
            DungeonLevel = level,
            Player = player,
            ExcludeKnownRecipes = excludeKnown
        };

    /// <summary>
    /// Creates a custom loot context with specified parameters.
    /// </summary>
    /// <param name="source">The type of loot source.</param>
    /// <param name="level">The dungeon level.</param>
    /// <param name="player">Optional player to enable filtering (default: null).</param>
    /// <param name="excludeKnown">Whether to exclude already-known recipes (default: false).</param>
    /// <returns>A LootContext with the specified parameters.</returns>
    /// <remarks>
    /// Use this factory method when none of the specific methods (Chest, Boss, Monster, Quest)
    /// are appropriate.
    /// </remarks>
    public static LootContext Create(
        LootSourceType source,
        int level,
        Player? player = null,
        bool excludeKnown = false)
        => new()
        {
            Source = source,
            DungeonLevel = level,
            Player = player,
            ExcludeKnownRecipes = excludeKnown
        };
}
