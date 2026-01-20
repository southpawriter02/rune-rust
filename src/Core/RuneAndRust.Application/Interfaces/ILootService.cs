using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
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
}
