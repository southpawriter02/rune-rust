using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for recipe scroll drop behavior.
/// </summary>
/// <remarks>
/// <para>
/// Defines when and where a recipe scroll can drop as loot. Each scroll config
/// is associated with a specific recipe and includes level requirements, drop
/// weights, and valid loot sources.
/// </para>
/// <para>
/// Drop behavior:
/// <list type="bullet">
///   <item><description>Scrolls only drop within their level range (MinDungeonLevel to MaxDungeonLevel)</description></item>
///   <item><description>Scrolls only drop from specified loot sources</description></item>
///   <item><description>Drop weight affects selection probability when multiple scrolls are eligible</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a config for Steel Sword recipe scroll
/// var config = RecipeScrollConfig.Create(
///     recipeId: "steel-sword",
///     dropWeight: 10,
///     minDungeonLevel: 3,
///     maxDungeonLevel: null,  // No upper limit
///     lootSources: new[] { LootSourceType.Chest, LootSourceType.Boss },
///     baseValue: 150);
///
/// // Check if scroll can drop at level 5
/// if (config.CanDropAtLevel(5))
/// {
///     // Scroll is eligible for drop
/// }
/// </code>
/// </example>
public sealed record RecipeScrollConfig
{
    /// <summary>
    /// Gets the recipe ID this scroll teaches.
    /// </summary>
    /// <remarks>
    /// References a RecipeDefinition.RecipeId. Stored in lowercase for case-insensitive matching.
    /// </remarks>
    public string RecipeId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the relative weight for drop selection among eligible scrolls.
    /// </summary>
    /// <remarks>
    /// Higher weights increase the probability of this scroll being selected
    /// when multiple scrolls are eligible at the current dungeon level.
    /// </remarks>
    public int DropWeight { get; init; }

    /// <summary>
    /// Gets the minimum dungeon level required for this scroll to drop.
    /// </summary>
    /// <remarks>
    /// The scroll will not appear in loot below this level. Levels are 1-indexed.
    /// </remarks>
    public int MinDungeonLevel { get; init; }

    /// <summary>
    /// Gets the maximum dungeon level at which this scroll can drop, or null for no upper limit.
    /// </summary>
    /// <remarks>
    /// If null, the scroll can drop at any level at or above MinDungeonLevel.
    /// Setting a max level allows scroll drops to be phased out at higher levels.
    /// </remarks>
    public int? MaxDungeonLevel { get; init; }

    /// <summary>
    /// Gets the loot sources that can generate this scroll.
    /// </summary>
    /// <remarks>
    /// The scroll will only drop from these specific sources. For example,
    /// boss-only scrolls would include only LootSourceType.Boss.
    /// </remarks>
    public IReadOnlyList<LootSourceType> LootSources { get; init; } = Array.Empty<LootSourceType>();

    /// <summary>
    /// Gets the base gold value of the scroll.
    /// </summary>
    /// <remarks>
    /// Used when creating the Item for the scroll drop. Higher tier recipes
    /// typically have higher base values.
    /// </remarks>
    public int BaseValue { get; init; }

    /// <summary>
    /// Checks if this scroll can drop at the specified dungeon level.
    /// </summary>
    /// <param name="level">The dungeon level to check.</param>
    /// <returns>True if the scroll is eligible to drop at this level.</returns>
    /// <example>
    /// <code>
    /// // Steel Sword scroll (level 3+)
    /// var scroll = RecipeScrollConfig.Create("steel-sword", 10, 3, null, sources, 150);
    /// scroll.CanDropAtLevel(2); // false - below minimum
    /// scroll.CanDropAtLevel(3); // true - at minimum
    /// scroll.CanDropAtLevel(10); // true - no maximum
    /// </code>
    /// </example>
    public bool CanDropAtLevel(int level)
    {
        if (level < MinDungeonLevel)
            return false;

        if (MaxDungeonLevel.HasValue && level > MaxDungeonLevel.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if this scroll can drop from the specified loot source.
    /// </summary>
    /// <param name="source">The loot source type to check.</param>
    /// <returns>True if the scroll is eligible to drop from this source.</returns>
    /// <example>
    /// <code>
    /// // Mithril Blade scroll (boss/quest only)
    /// var scroll = RecipeScrollConfig.Create(
    ///     "mithril-blade", 2, 8, null,
    ///     new[] { LootSourceType.Boss, LootSourceType.Quest }, 500);
    ///
    /// scroll.CanDropFromSource(LootSourceType.Boss); // true
    /// scroll.CanDropFromSource(LootSourceType.Chest); // false
    /// </code>
    /// </example>
    public bool CanDropFromSource(LootSourceType source)
    {
        return LootSources.Contains(source);
    }

    /// <summary>
    /// Checks if this scroll is eligible to drop given both level and source.
    /// </summary>
    /// <param name="level">The dungeon level to check.</param>
    /// <param name="source">The loot source type to check.</param>
    /// <returns>True if the scroll can drop at this level from this source.</returns>
    public bool CanDrop(int level, LootSourceType source)
    {
        return CanDropAtLevel(level) && CanDropFromSource(source);
    }

    /// <summary>
    /// Creates a validated RecipeScrollConfig.
    /// </summary>
    /// <param name="recipeId">The recipe ID this scroll teaches (will be normalized to lowercase).</param>
    /// <param name="dropWeight">The relative weight for drop selection (must be positive).</param>
    /// <param name="minDungeonLevel">The minimum dungeon level required (must be at least 1).</param>
    /// <param name="maxDungeonLevel">The maximum dungeon level, or null for no limit.</param>
    /// <param name="lootSources">The loot sources that can generate this scroll.</param>
    /// <param name="baseValue">The base gold value of the scroll (must be non-negative).</param>
    /// <returns>A validated RecipeScrollConfig.</returns>
    /// <exception cref="ArgumentException">Thrown when recipeId is null or whitespace, or lootSources is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when numeric parameters are out of valid range.</exception>
    public static RecipeScrollConfig Create(
        string recipeId,
        int dropWeight,
        int minDungeonLevel,
        int? maxDungeonLevel,
        IReadOnlyList<LootSourceType> lootSources,
        int baseValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeId, nameof(recipeId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dropWeight, nameof(dropWeight));
        ArgumentOutOfRangeException.ThrowIfLessThan(minDungeonLevel, 1, nameof(minDungeonLevel));
        ArgumentOutOfRangeException.ThrowIfNegative(baseValue, nameof(baseValue));

        if (lootSources == null || lootSources.Count == 0)
        {
            throw new ArgumentException("At least one loot source must be specified.", nameof(lootSources));
        }

        if (maxDungeonLevel.HasValue && maxDungeonLevel.Value < minDungeonLevel)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDungeonLevel),
                $"MaxDungeonLevel ({maxDungeonLevel.Value}) cannot be less than MinDungeonLevel ({minDungeonLevel}).");
        }

        return new RecipeScrollConfig
        {
            RecipeId = recipeId.ToLowerInvariant(),
            DropWeight = dropWeight,
            MinDungeonLevel = minDungeonLevel,
            MaxDungeonLevel = maxDungeonLevel,
            LootSources = lootSources,
            BaseValue = baseValue
        };
    }
}
