using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for generating and managing loot drops from defeated monsters.
/// </summary>
/// <remarks>
/// <para>
/// The loot service handles all aspects of loot generation including:
/// <list type="bullet">
///   <item><description>Item drops from monster loot tables</description></item>
///   <item><description>Currency drops with configurable amounts</description></item>
///   <item><description>Recipe scroll generation based on dungeon level and source</description></item>
///   <item><description>Loot collection and inventory management</description></item>
/// </list>
/// </para>
/// <para>
/// Recipe scroll generation (v0.11.1c) allows recipe scrolls to drop from various
/// sources throughout the dungeon, enabling players to discover new crafting recipes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Generate loot from a defeated monster
/// var loot = lootService.GenerateLoot(defeatedMonster);
///
/// // Generate a potential recipe scroll from a chest
/// var context = LootContext.Chest(dungeonLevel: 5, player: currentPlayer);
/// var scroll = lootService.TryGenerateRecipeScroll(context);
/// if (scroll != null)
/// {
///     player.Inventory.AddItem(scroll);
/// }
/// </code>
/// </example>
public interface ILootService
{
    // ═══════════════════════════════════════════════════════════════
    // LOOT GENERATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates loot from a defeated monster using its loot multiplier.
    /// </summary>
    /// <param name="monster">The defeated monster.</param>
    /// <returns>A LootDrop containing generated items and currency.</returns>
    /// <remarks>
    /// Uses the monster's definition to access its loot table and applies
    /// the monster's loot multiplier to drop chances.
    /// </remarks>
    /// <example>
    /// <code>
    /// var loot = lootService.GenerateLoot(defeatedGoblin);
    /// foreach (var item in loot.Items)
    /// {
    ///     Console.WriteLine($"Dropped: {item.Quantity}x {item.Name}");
    /// }
    /// </code>
    /// </example>
    LootDrop GenerateLoot(Monster monster);

    /// <summary>
    /// Generates loot from a monster definition with a specified multiplier.
    /// </summary>
    /// <param name="definition">The monster definition containing the loot table.</param>
    /// <param name="lootMultiplier">Multiplier applied to drop chances and currency amounts.</param>
    /// <returns>A LootDrop containing generated items and currency.</returns>
    /// <remarks>
    /// Allows direct loot generation from a definition without needing an instance.
    /// Useful for preview or testing scenarios.
    /// </remarks>
    LootDrop GenerateLoot(MonsterDefinition definition, float lootMultiplier = 1.0f);

    /// <summary>
    /// Generates loot from a defeated monster with player context for smart loot selection.
    /// </summary>
    /// <param name="monster">The defeated monster.</param>
    /// <param name="player">The player receiving the loot (enables class-appropriate bias).</param>
    /// <returns>A LootDrop containing generated items and currency with smart loot metadata.</returns>
    /// <remarks>
    /// <para>
    /// When a player is provided and <see cref="ISmartLootService"/> is available,
    /// the loot system uses the 60/40 smart loot algorithm to bias equipment drops
    /// toward items appropriate for the player's archetype.
    /// </para>
    /// <para>
    /// The returned LootDrop includes metadata about the selection process:
    /// <list type="bullet">
    ///   <item><description>WasClassAppropriate - Whether the item matched player's class</description></item>
    ///   <item><description>PlayerArchetypeId - The archetype used for filtering</description></item>
    ///   <item><description>SelectionReason - Human-readable selection explanation</description></item>
    ///   <item><description>BiasRoll - The 0-99 roll for debugging</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Generate smart loot with class bias
    /// var loot = lootService.GenerateLoot(defeatedGoblin, currentPlayer);
    /// if (loot.WasClassAppropriate)
    /// {
    ///     Console.WriteLine("Lucky! You got gear for your class!");
    /// }
    /// </code>
    /// </example>
    LootDrop GenerateLoot(Monster monster, Player player);

    /// <summary>
    /// Collects all dropped loot from a room and adds it to the player's inventory.
    /// </summary>
    /// <param name="player">The player collecting the loot.</param>
    /// <param name="room">The room containing the dropped loot.</param>
    /// <returns>A LootDrop containing the collected items and currency for display.</returns>
    /// <remarks>
    /// Currency is automatically added to the player. Items are returned in the
    /// LootDrop for the caller to handle (add to inventory, display, etc.).
    /// </remarks>
    LootDrop CollectLoot(Player player, Room room);

    // ═══════════════════════════════════════════════════════════════
    // RECIPE SCROLL GENERATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to generate a recipe scroll based on the loot context.
    /// </summary>
    /// <param name="context">The loot context containing source type, level, and player info.</param>
    /// <returns>A recipe scroll item if generated, or null if no scroll drops.</returns>
    /// <remarks>
    /// <para>
    /// Recipe scroll generation follows this process:
    /// <list type="number">
    ///   <item><description>Check if scroll provider is available</description></item>
    ///   <item><description>Roll against the drop chance for the loot source</description></item>
    ///   <item><description>Get eligible scrolls based on level and source</description></item>
    ///   <item><description>Optionally filter out known recipes if player is provided</description></item>
    ///   <item><description>Select a scroll using weighted random selection</description></item>
    ///   <item><description>Create and return the scroll item</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Drop chances vary by source type (configured in recipe-scrolls.json):
    /// <list type="bullet">
    ///   <item><description>Chest: ~15%</description></item>
    ///   <item><description>Boss: ~30%</description></item>
    ///   <item><description>Monster: ~2%</description></item>
    ///   <item><description>Quest: ~50%</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Generate scroll from a level 5 chest
    /// var context = LootContext.Chest(level: 5, player: currentPlayer, excludeKnown: true);
    /// var scroll = lootService.TryGenerateRecipeScroll(context);
    ///
    /// if (scroll != null)
    /// {
    ///     Console.WriteLine($"Found: {scroll.Name}");
    ///     player.Inventory.AddItem(scroll);
    /// }
    /// </code>
    /// </example>
    Item? TryGenerateRecipeScroll(LootContext context);

    /// <summary>
    /// Gets the drop chance for recipe scrolls from a specific loot source.
    /// </summary>
    /// <param name="source">The loot source type.</param>
    /// <returns>The drop chance as a decimal (0.0 to 1.0), or 0 if not configured.</returns>
    /// <remarks>
    /// Returns 0 if no recipe scroll provider is configured.
    /// </remarks>
    /// <example>
    /// <code>
    /// var chestChance = lootService.GetScrollDropChance(LootSourceType.Chest);
    /// Console.WriteLine($"Chest scroll drop chance: {chestChance:P0}");
    /// </code>
    /// </example>
    decimal GetScrollDropChance(Domain.Enums.LootSourceType source);

    // ═══════════════════════════════════════════════════════════════
    // CURRENCY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a currency definition by ID.
    /// </summary>
    /// <param name="currencyId">The currency identifier.</param>
    /// <returns>The currency definition or null if not found.</returns>
    CurrencyDefinition? GetCurrency(string currencyId);

    /// <summary>
    /// Gets all available currency definitions.
    /// </summary>
    /// <returns>A read-only list of all currency definitions.</returns>
    IReadOnlyList<CurrencyDefinition> GetAllCurrencies();

    // ═══════════════════════════════════════════════════════════════
    // TIERED LOOT GENERATION (v0.16.0d)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates tiered loot from a defeated monster using quality tier probabilities.
    /// </summary>
    /// <param name="monster">The defeated monster entity.</param>
    /// <param name="random">Random number generator for tier and item rolling.</param>
    /// <returns>A LootDrop containing tier-scaled items with calculated statistics.</returns>
    /// <remarks>
    /// <para>
    /// This method integrates the quality tier system (v0.16.0a-c) to generate
    /// loot based on the monster's drop class. The process:
    /// </para>
    /// <list type="number">
    ///   <item><description>Determine the monster's EnemyDropClass</description></item>
    ///   <item><description>Query the drop table for tier probabilities</description></item>
    ///   <item><description>Roll for each item based on the monster's drop count</description></item>
    ///   <item><description>Apply attribute bonuses for Tier 2+ items</description></item>
    ///   <item><description>Aggregate tier statistics in the LootDrop</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var random = new Random(seed);
    /// var loot = lootService.GenerateTieredLoot(defeatedBoss, random);
    /// 
    /// Console.WriteLine($"Highest tier: {loot.HighestTier}");
    /// Console.WriteLine($"Myth-Forged drop: {loot.HasMythForged}");
    /// 
    /// foreach (var item in loot.Items)
    /// {
    ///     Console.WriteLine($"  {item.FormattedName}");
    /// }
    /// </code>
    /// </example>
    LootDrop GenerateTieredLoot(Monster monster, Random random);

    /// <summary>
    /// Generates a single item for a specific quality tier.
    /// </summary>
    /// <param name="tier">The quality tier for the item.</param>
    /// <param name="category">The item category (e.g., "weapon", "armor").</param>
    /// <param name="random">Random number generator for item selection and bonuses.</param>
    /// <returns>A DroppedItem of the specified tier with appropriate bonuses.</returns>
    /// <remarks>
    /// <para>
    /// This method creates items with tier-appropriate statistics:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Tier 0-1: No attribute bonuses</description></item>
    ///   <item><description>Tier 2+: Rolled attribute bonus based on tier scaling</description></item>
    ///   <item><description>Tier 4: Unique properties and highest bonus range</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var random = new Random();
    /// var sword = lootService.GenerateItemForTier(QualityTier.ClanForged, "weapon", random);
    /// 
    /// Console.WriteLine($"Created: {sword.FormattedName}");
    /// if (sword.HasAttributeBonus)
    /// {
    ///     Console.WriteLine($"  Bonus: +{sword.AttributeBonus} {sword.BonusAttribute}");
    /// }
    /// </code>
    /// </example>
    DroppedItem GenerateItemForTier(QualityTier tier, string category, Random random);

    /// <summary>
    /// Gets the drop class for a specific monster.
    /// </summary>
    /// <param name="monster">The monster entity to classify.</param>
    /// <returns>The EnemyDropClass determining loot quality probabilities.</returns>
    /// <remarks>
    /// <para>
    /// Drop class is determined by the monster's type and configuration.
    /// Unknown monsters default to <see cref="EnemyDropClass.Standard"/>.
    /// </para>
    /// <para>
    /// Drop class affects:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Tier probability distribution</description></item>
    ///   <item><description>Number of items dropped</description></item>
    ///   <item><description>Chance of no-drop (Trash class only)</description></item>
    /// </list>
    /// </remarks>
    EnemyDropClass GetDropClassForMonster(Monster monster);
}

