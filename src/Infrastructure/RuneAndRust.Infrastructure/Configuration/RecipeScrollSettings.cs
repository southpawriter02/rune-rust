namespace RuneAndRust.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for recipe scroll drop behavior.
/// </summary>
/// <remarks>
/// <para>
/// This class is used for JSON binding via IOptions pattern. It contains
/// the raw DTO data loaded from recipe-scrolls.json configuration file.
/// </para>
/// <para>
/// Expected JSON format:
/// </para>
/// <code>
/// {
///   "recipeScrolls": [
///     {
///       "recipeId": "steel-sword",
///       "dropWeight": 10,
///       "minDungeonLevel": 3,
///       "maxDungeonLevel": null,
///       "lootSources": ["Chest", "Boss"],
///       "baseValue": 100
///     }
///   ],
///   "scrollDropChances": {
///     "Chest": 0.15,
///     "Boss": 0.30,
///     "Monster": 0.02,
///     "Quest": 0.50
///   }
/// }
/// </code>
/// </remarks>
public class RecipeScrollSettings
{
    /// <summary>
    /// Gets or sets the list of recipe scroll configurations.
    /// </summary>
    /// <remarks>
    /// Each entry defines a recipe that can be found as a scroll, along with
    /// its drop weight, level requirements, and eligible loot sources.
    /// </remarks>
    public List<RecipeScrollConfigDto> RecipeScrolls { get; set; } = [];

    /// <summary>
    /// Gets or sets the drop chances per loot source type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The dictionary maps loot source type names (e.g., "Chest", "Boss")
    /// to their base drop chances as decimals (0.0 to 1.0).
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
    public Dictionary<string, decimal> ScrollDropChances { get; set; } = [];
}

/// <summary>
/// DTO for a single recipe scroll configuration entry.
/// </summary>
/// <remarks>
/// This DTO is used for JSON deserialization. The provider converts these
/// DTOs into <see cref="Domain.ValueObjects.RecipeScrollConfig"/> value objects.
/// </remarks>
public class RecipeScrollConfigDto
{
    /// <summary>
    /// Gets or sets the recipe identifier this scroll teaches.
    /// </summary>
    /// <remarks>
    /// Must match a recipe ID in the recipe provider. Case-insensitive lookup
    /// is performed when validating.
    /// </remarks>
    public string RecipeId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the drop weight for weighted random selection.
    /// </summary>
    /// <remarks>
    /// Higher weights mean higher probability of selection when multiple
    /// scrolls are eligible. Typical range: 1-20.
    /// </remarks>
    public int DropWeight { get; set; } = 10;

    /// <summary>
    /// Gets or sets the minimum dungeon level required for this scroll to drop.
    /// </summary>
    /// <remarks>
    /// The scroll will not appear in loot tables until the player reaches
    /// at least this dungeon level. Defaults to 1 (available from the start).
    /// </remarks>
    public int MinDungeonLevel { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum dungeon level for this scroll to drop, or null for no limit.
    /// </summary>
    /// <remarks>
    /// If specified, the scroll will stop appearing in loot tables after
    /// exceeding this level. Useful for keeping low-tier scrolls out of
    /// high-level areas. Null means no upper limit.
    /// </remarks>
    public int? MaxDungeonLevel { get; set; }

    /// <summary>
    /// Gets or sets the list of loot source types this scroll can drop from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Valid values: "Chest", "Boss", "Monster", "Quest", "Shop", "Crafting"
    /// </para>
    /// <para>
    /// If empty or null, the scroll cannot drop naturally (must be obtained
    /// through other means like trainers or specific quests).
    /// </para>
    /// </remarks>
    public List<string> LootSources { get; set; } = [];

    /// <summary>
    /// Gets or sets the base gold value of the scroll item.
    /// </summary>
    /// <remarks>
    /// Used for item valuation, shop pricing, and vendor sell value.
    /// Defaults to 100 gold.
    /// </remarks>
    public int BaseValue { get; set; } = 100;
}
