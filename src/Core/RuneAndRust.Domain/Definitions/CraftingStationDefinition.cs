using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a type of crafting station where players can craft items.
/// </summary>
/// <remarks>
/// <para>
/// Crafting station definitions are immutable templates loaded from JSON configuration.
/// Each station type supports specific recipe categories and requires a corresponding
/// crafting skill for dice check modifiers.
/// </para>
/// <para>
/// Stations are instantiated as <see cref="CraftingStation"/> room features when
/// placed in rooms. The definition provides the template data, while instances
/// track state like availability.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Category-based filtering for recipe compatibility</description></item>
///   <item><description>Skill linkage for crafting check modifiers</description></item>
///   <item><description>Instance creation via <see cref="CreateInstance()"/> factory</description></item>
///   <item><description>Case-insensitive ID normalization to lowercase</description></item>
/// </list>
/// </para>
/// <para>
/// Station types defined in v0.11.2a:
/// <list type="bullet">
///   <item><description>anvil: Weapon, Tool, Material (smithing)</description></item>
///   <item><description>workbench: Armor, Accessory (leatherworking)</description></item>
///   <item><description>alchemy-table: Potion, Consumable (alchemy)</description></item>
///   <item><description>enchanting-altar: Accessory, Weapon, Armor (enchanting)</description></item>
///   <item><description>cooking-fire: Consumable (cooking)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var definition = CraftingStationDefinition.Create(
///     stationId: "anvil",
///     name: "Anvil",
///     description: "A sturdy anvil for smithing metal items",
///     supportedCategories: new[] { RecipeCategory.Weapon, RecipeCategory.Tool, RecipeCategory.Material },
///     craftingSkillId: "smithing",
///     iconPath: "icons/stations/anvil.png");
///
/// // Check if station supports a category
/// if (definition.SupportsCategory(RecipeCategory.Weapon))
/// {
///     Console.WriteLine($"{definition.Name} can craft weapons");
/// }
///
/// // Create an instance for placement in a room
/// var station = definition.CreateInstance();
/// room.AddCraftingStation(station);
/// </code>
/// </example>
public sealed class CraftingStationDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique entity identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique station type identifier used for lookups.
    /// </summary>
    /// <remarks>
    /// Station IDs use kebab-case formatting (lowercase with hyphens).
    /// The Create method automatically lowercases the provided ID.
    /// </remarks>
    /// <example>"anvil", "alchemy-table", "enchanting-altar"</example>
    public string StationId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the station.
    /// </summary>
    /// <example>"Anvil", "Alchemy Table", "Enchanting Altar"</example>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the description shown when examining the station.
    /// </summary>
    /// <remarks>
    /// This text is displayed when a player examines a crafting station
    /// in a room, providing atmospheric description of the station.
    /// </remarks>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the recipe categories this station can craft.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A station can only craft recipes whose <see cref="RecipeDefinition.Category"/>
    /// is contained in this list. The <see cref="SupportsCategory"/> and
    /// <see cref="CanCraftRecipe"/> methods provide convenient validation.
    /// </para>
    /// <para>
    /// Category assignments are determined by the physical nature of the station:
    /// <list type="bullet">
    ///   <item><description>Anvil: Weapon, Tool, Material (metal work)</description></item>
    ///   <item><description>Workbench: Armor, Accessory (leather/cloth work)</description></item>
    ///   <item><description>Alchemy Table: Potion, Consumable (brewing)</description></item>
    ///   <item><description>Enchanting Altar: Weapon, Armor, Accessory (magical enhancement)</description></item>
    ///   <item><description>Cooking Fire: Consumable (food preparation)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public IReadOnlyList<RecipeCategory> SupportedCategories { get; private set; } = [];

    /// <summary>
    /// Gets the skill ID used for crafting checks at this station.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a player attempts to craft at this station, their modifier for
    /// this skill is added to their 2d6 roll for the crafting check.
    /// </para>
    /// <para>
    /// Common crafting skills:
    /// <list type="bullet">
    ///   <item><description>smithing: Anvil crafting</description></item>
    ///   <item><description>leatherworking: Workbench crafting</description></item>
    ///   <item><description>alchemy: Potion brewing</description></item>
    ///   <item><description>enchanting: Magical enhancement</description></item>
    ///   <item><description>cooking: Food preparation</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public string CraftingSkillId { get; private set; } = null!;

    /// <summary>
    /// Gets the optional icon path for UI display.
    /// </summary>
    /// <remarks>
    /// Path is relative to the game's icon directory.
    /// Returns null if no icon is specified.
    /// </remarks>
    /// <example>"icons/stations/anvil.png"</example>
    public string? IconPath { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory pattern and serialization.
    /// </summary>
    private CraftingStationDefinition() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new crafting station definition.
    /// </summary>
    /// <param name="stationId">Unique station type identifier (will be lowercased).</param>
    /// <param name="name">Display name shown in UI and room descriptions.</param>
    /// <param name="description">Description text shown when examining the station.</param>
    /// <param name="supportedCategories">Recipe categories this station can craft (at least 1).</param>
    /// <param name="craftingSkillId">Skill used for crafting checks (will be lowercased).</param>
    /// <param name="iconPath">Optional icon path for UI display.</param>
    /// <returns>A new CraftingStationDefinition instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when required string parameters are null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when supportedCategories is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when supportedCategories is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// var definition = CraftingStationDefinition.Create(
    ///     stationId: "anvil",
    ///     name: "Anvil",
    ///     description: "A sturdy anvil for smithing metal items",
    ///     supportedCategories: new[] { RecipeCategory.Weapon, RecipeCategory.Tool },
    ///     craftingSkillId: "smithing");
    /// </code>
    /// </example>
    public static CraftingStationDefinition Create(
        string stationId,
        string name,
        string description,
        IEnumerable<RecipeCategory> supportedCategories,
        string craftingSkillId,
        string? iconPath = null)
    {
        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(stationId, nameof(stationId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentException.ThrowIfNullOrWhiteSpace(craftingSkillId, nameof(craftingSkillId));

        // Validate categories collection
        ArgumentNullException.ThrowIfNull(supportedCategories, nameof(supportedCategories));

        var categoryList = supportedCategories.ToList();
        if (categoryList.Count == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(supportedCategories),
                "Station must support at least one recipe category.");
        }

        return new CraftingStationDefinition
        {
            Id = Guid.NewGuid(),
            StationId = stationId.ToLowerInvariant(),
            Name = name,
            Description = description,
            SupportedCategories = categoryList.AsReadOnly(),
            CraftingSkillId = craftingSkillId.ToLowerInvariant(),
            IconPath = iconPath
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if this station supports a specific recipe category.
    /// </summary>
    /// <param name="category">The recipe category to check.</param>
    /// <returns>True if the station supports the category; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (anvilDefinition.SupportsCategory(RecipeCategory.Weapon))
    /// {
    ///     Console.WriteLine("Anvil can craft weapons!");
    /// }
    /// </code>
    /// </example>
    public bool SupportsCategory(RecipeCategory category)
    {
        return SupportedCategories.Contains(category);
    }

    /// <summary>
    /// Checks if this station can craft a specific recipe.
    /// </summary>
    /// <param name="recipe">The recipe to check.</param>
    /// <returns>True if the station can craft the recipe; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when recipe is null.
    /// </exception>
    /// <remarks>
    /// This method validates that the recipe's category is supported by this station.
    /// It does not check other requirements like player skills or available ingredients.
    /// </remarks>
    /// <example>
    /// <code>
    /// var ironSwordRecipe = recipeProvider.GetRecipe("iron-sword");
    /// if (anvilDefinition.CanCraftRecipe(ironSwordRecipe))
    /// {
    ///     Console.WriteLine("Iron sword can be crafted at the anvil");
    /// }
    /// </code>
    /// </example>
    public bool CanCraftRecipe(RecipeDefinition recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe, nameof(recipe));
        return SupportsCategory(recipe.Category);
    }

    /// <summary>
    /// Gets a formatted string of supported categories for display.
    /// </summary>
    /// <returns>A comma-separated list of category names.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Crafts: {definition.GetCategoriesDisplay()}");
    /// // Output: Crafts: Weapon, Tool, Material
    /// </code>
    /// </example>
    public string GetCategoriesDisplay()
    {
        return string.Join(", ", SupportedCategories.Select(c => c.ToString()));
    }

    // ═══════════════════════════════════════════════════════════════
    // INSTANCE CREATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an instance of this station type for placement in a room.
    /// </summary>
    /// <returns>A new CraftingStation instance based on this definition.</returns>
    /// <remarks>
    /// The returned station is initialized as available with the definition's
    /// name and description. The station's FeatureType is set to CraftingStation,
    /// IsInteractable is true, and InteractionVerb is "use".
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = stationProvider.GetStation("anvil");
    /// var station = definition.CreateInstance();
    /// room.AddCraftingStation(station);
    /// </code>
    /// </example>
    public CraftingStation CreateInstance()
    {
        return CraftingStation.Create(this);
    }

    /// <summary>
    /// Creates an instance with a custom description.
    /// </summary>
    /// <param name="customDescription">Custom description for this specific instance.</param>
    /// <returns>A new CraftingStation instance with the custom description.</returns>
    /// <remarks>
    /// Useful for creating unique station instances with room-specific flavor text
    /// while maintaining the same crafting capabilities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var station = definition.CreateInstance(
    ///     "An ancient anvil, its surface scarred by centuries of legendary smithing.");
    /// </code>
    /// </example>
    public CraftingStation CreateInstance(string customDescription)
    {
        return CraftingStation.Create(this, customDescription);
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the station definition.
    /// </summary>
    /// <returns>The station name and ID in format "Name (stationId)".</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(definition.ToString());
    /// // Output: Anvil (anvil)
    /// </code>
    /// </example>
    public override string ToString() => $"{Name} ({StationId})";
}
