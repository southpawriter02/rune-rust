using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a resource type that can be gathered and used in crafting.
/// </summary>
/// <remarks>
/// <para>
/// Resource definitions are data-driven templates loaded from JSON configuration.
/// They define the properties of gatherable materials including their category,
/// quality tier, base value, and stacking behavior.
/// </para>
/// <para>
/// Resources are identified by a unique string ID (e.g., "iron-ore") and can
/// be filtered by category or quality for recipe matching and inventory display.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Category-based filtering for crafting recipes</description></item>
///   <item><description>Quality tiers affecting value and crafting outcomes</description></item>
///   <item><description>Stack size limits for inventory management</description></item>
///   <item><description>Value calculation with quality multipliers</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var ironOre = ResourceDefinition.Create(
///     "iron-ore",
///     "Iron Ore",
///     "Raw iron ore, can be smelted into iron ingots",
///     ResourceCategory.Ore,
///     ResourceQuality.Common,
///     baseValue: 5,
///     stackSize: 20);
///
/// var actualValue = ironOre.GetActualValue(); // 5 (base * 1.0 multiplier)
/// var craftingBonus = ironOre.GetCraftingBonus(); // 0
/// </code>
/// </example>
public class ResourceDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this resource definition.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the string identifier used for lookups and configuration references.
    /// </summary>
    /// <remarks>
    /// Resource IDs use kebab-case formatting (lowercase with hyphens).
    /// The Create method automatically lowercases the provided ID.
    /// </remarks>
    /// <example>"iron-ore", "healing-herb", "dragon-scale"</example>
    public string ResourceId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the resource.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the description shown in tooltips and inventory.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the category of this resource for filtering and recipes.
    /// </summary>
    public ResourceCategory Category { get; private set; }

    /// <summary>
    /// Gets the quality tier affecting value and crafting bonuses.
    /// </summary>
    public ResourceQuality Quality { get; private set; }

    /// <summary>
    /// Gets the base gold value before quality multiplier.
    /// </summary>
    public int BaseValue { get; private set; }

    /// <summary>
    /// Gets the maximum stack size in inventory.
    /// </summary>
    /// <remarks>
    /// Common resources typically stack to 20-50, while rare resources
    /// may have lower stack sizes (5-10) to reflect their scarcity.
    /// </remarks>
    public int StackSize { get; private set; }

    /// <summary>
    /// Gets the optional path to the resource's icon.
    /// </summary>
    public string? IconPath { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory method and JSON deserialization.
    /// </summary>
    private ResourceDefinition() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new resource definition with the specified properties.
    /// </summary>
    /// <param name="resourceId">Unique string identifier (will be lowercased).</param>
    /// <param name="name">Display name.</param>
    /// <param name="description">Description text.</param>
    /// <param name="category">Resource category.</param>
    /// <param name="quality">Quality tier.</param>
    /// <param name="baseValue">Base gold value (must be positive).</param>
    /// <param name="stackSize">Maximum stack size (must be positive, default 20).</param>
    /// <returns>A new <see cref="ResourceDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when resourceId or name is null/whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when baseValue or stackSize is non-positive.</exception>
    /// <example>
    /// <code>
    /// var goldOre = ResourceDefinition.Create(
    ///     "gold-ore",
    ///     "Gold Ore",
    ///     "Valuable gold ore for fine crafting",
    ///     ResourceCategory.Ore,
    ///     ResourceQuality.Fine,
    ///     baseValue: 25,
    ///     stackSize: 20);
    /// </code>
    /// </example>
    public static ResourceDefinition Create(
        string resourceId,
        string name,
        string description,
        ResourceCategory category,
        ResourceQuality quality,
        int baseValue,
        int stackSize = 20)
    {
        // Validate required parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId, nameof(resourceId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baseValue, nameof(baseValue));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stackSize, nameof(stackSize));

        return new ResourceDefinition
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            Category = category,
            Quality = quality,
            BaseValue = baseValue,
            StackSize = stackSize
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT BUILDER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the icon path for this resource.
    /// </summary>
    /// <param name="iconPath">Path to the icon file.</param>
    /// <returns>This instance for fluent chaining.</returns>
    /// <example>
    /// <code>
    /// var resource = ResourceDefinition.Create(...)
    ///     .WithIcon("icons/resources/iron_ore.png");
    /// </code>
    /// </example>
    public ResourceDefinition WithIcon(string iconPath)
    {
        IconPath = iconPath;
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // VALUE CALCULATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the actual value after applying quality multiplier.
    /// </summary>
    /// <returns>The gold value adjusted for quality.</returns>
    /// <remarks>
    /// <para>
    /// Calculation: BaseValue * Quality.GetValueMultiplier()
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    ///   <item><description>Iron Ore (Common, BaseValue=5): 5 * 1.0 = 5</description></item>
    ///   <item><description>Gold Ore (Fine, BaseValue=25): 25 * 1.5 = 37</description></item>
    ///   <item><description>Mithril Ore (Rare, BaseValue=100): 100 * 3.0 = 300</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var sellPrice = resource.GetActualValue();
    /// </code>
    /// </example>
    public int GetActualValue()
    {
        return (int)(BaseValue * Quality.GetValueMultiplier());
    }

    /// <summary>
    /// Gets the crafting bonus provided by this resource's quality.
    /// </summary>
    /// <returns>The bonus to add to crafting rolls.</returns>
    /// <example>
    /// <code>
    /// var craftRoll = diceResult + skillBonus + resource.GetCraftingBonus();
    /// </code>
    /// </example>
    public int GetCraftingBonus()
    {
        return Quality.GetCraftingBonus();
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPARISON AND QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if this resource can stack with another resource.
    /// </summary>
    /// <param name="other">The other resource to check.</param>
    /// <returns>True if the resources can stack (same ResourceId).</returns>
    /// <remarks>
    /// <para>
    /// Resources can only stack if they have the same ResourceId.
    /// Quality is part of the definition, so different quality versions
    /// are different resources and cannot stack.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
    /// <example>
    /// <code>
    /// if (resource1.CanStackWith(resource2))
    /// {
    ///     // Add to existing stack
    /// }
    /// </code>
    /// </example>
    public bool CanStackWith(ResourceDefinition other)
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        return ResourceId == other.ResourceId;
    }

    /// <summary>
    /// Checks if this resource matches the specified category.
    /// </summary>
    /// <param name="category">The category to check.</param>
    /// <returns>True if the resource belongs to the category.</returns>
    /// <example>
    /// <code>
    /// if (resource.IsCategory(ResourceCategory.Ore))
    /// {
    ///     // Can be used in smithing recipes
    /// }
    /// </code>
    /// </example>
    public bool IsCategory(ResourceCategory category)
    {
        return Category == category;
    }

    /// <summary>
    /// Checks if this resource meets a minimum quality requirement.
    /// </summary>
    /// <param name="minimumQuality">The minimum required quality.</param>
    /// <returns>True if this resource's quality is equal to or higher.</returns>
    /// <example>
    /// <code>
    /// if (resource.MeetsQualityRequirement(ResourceQuality.Fine))
    /// {
    ///     // Acceptable for recipe requiring Fine or better quality
    /// }
    /// </code>
    /// </example>
    public bool MeetsQualityRequirement(ResourceQuality minimumQuality)
    {
        return Quality >= minimumQuality;
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a display string for this resource.
    /// </summary>
    /// <returns>
    /// For Common quality: just the name.
    /// For other qualities: "Name (Quality)".
    /// </returns>
    /// <example>
    /// <code>
    /// var ironOre = ...; // Common quality
    /// ironOre.ToString(); // "Iron Ore"
    ///
    /// var goldOre = ...; // Fine quality
    /// goldOre.ToString(); // "Gold Ore (Fine)"
    /// </code>
    /// </example>
    public override string ToString()
    {
        return Quality == ResourceQuality.Common
            ? Name
            : $"{Name} ({Quality.GetDisplayName()})";
    }
}
