using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a crafting recipe with ingredients, output, and requirements.
/// </summary>
/// <remarks>
/// <para>
/// Recipe definitions are immutable templates loaded from JSON configuration.
/// They specify what resources are consumed and what items are produced
/// when crafting succeeds at a crafting station.
/// </para>
/// <para>
/// The <see cref="DifficultyClass"/> determines the target number for
/// the crafting skill check. Players must roll 2d6 + modifier >= DC to succeed.
/// </para>
/// <para>
/// Recipes marked with <see cref="IsDefault"/> are automatically added
/// to new players' recipe books upon character creation.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Category-based filtering for recipe book UI</description></item>
///   <item><description>Station requirements linking to crafting stations</description></item>
///   <item><description>Difficulty class for skill-based crafting checks</description></item>
///   <item><description>Optional quality scaling based on crafting roll</description></item>
///   <item><description>Crafting time in seconds for progress display</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var ingredients = new List&lt;RecipeIngredient&gt;
/// {
///     new("iron-ore", 5),
///     new("leather", 2)
/// };
/// var output = new RecipeOutput("iron-sword", 1);
///
/// var recipe = RecipeDefinition.Create(
///     recipeId: "iron-sword",
///     name: "Iron Sword",
///     description: "Forge a basic but reliable iron sword",
///     category: RecipeCategory.Weapon,
///     requiredStationId: "anvil",
///     ingredients: ingredients,
///     output: output,
///     difficultyClass: 12,
///     isDefault: true,
///     craftingTimeSeconds: 30);
///
/// Console.WriteLine($"{recipe.Name}: DC {recipe.DifficultyClass} ({recipe.GetDifficultyDescription()})");
/// // Output: Iron Sword: DC 12 (Moderate)
/// </code>
/// </example>
public sealed class RecipeDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique entity identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique recipe identifier used for lookups.
    /// </summary>
    /// <remarks>
    /// Recipe IDs use kebab-case formatting (lowercase with hyphens).
    /// The Create method automatically lowercases the provided ID.
    /// </remarks>
    /// <example>"iron-sword", "healing-potion", "mithril-blade"</example>
    public string RecipeId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the recipe.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the description of the recipe shown in the recipe book.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the category of the recipe for UI filtering.
    /// </summary>
    public RecipeCategory Category { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the required crafting station.
    /// </summary>
    /// <remarks>
    /// Players must be at this station to craft this recipe.
    /// Common stations include: anvil, alchemy-table, workbench, cooking-fire.
    /// </remarks>
    public string RequiredStationId { get; private set; } = null!;

    /// <summary>
    /// Gets the list of ingredients required for this recipe.
    /// </summary>
    /// <remarks>
    /// Each ingredient specifies a resource ID and quantity that will be
    /// consumed from the player's inventory when crafting.
    /// </remarks>
    public IReadOnlyList<RecipeIngredient> Ingredients { get; private set; } = [];

    /// <summary>
    /// Gets the output produced by this recipe.
    /// </summary>
    /// <remarks>
    /// Specifies the item ID, quantity, and optional quality scaling formula.
    /// </remarks>
    public RecipeOutput Output { get; private set; } = null!;

    /// <summary>
    /// Gets the difficulty class for the crafting skill check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The player must roll 2d6 + crafting modifier >= DC to succeed.
    /// </para>
    /// <para>
    /// DC ranges:
    /// <list type="bullet">
    ///   <item><description>5-8: Trivial (basic materials)</description></item>
    ///   <item><description>9-10: Easy (simple items)</description></item>
    ///   <item><description>11-12: Moderate (standard equipment)</description></item>
    ///   <item><description>13-14: Challenging (enhanced equipment)</description></item>
    ///   <item><description>15-16: Hard (rare items)</description></item>
    ///   <item><description>17-18: Very Hard (epic equipment)</description></item>
    ///   <item><description>19+: Legendary (mythic items)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int DifficultyClass { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this is a default recipe.
    /// </summary>
    /// <remarks>
    /// Default recipes are automatically known by new players and
    /// don't require discovery through recipe scrolls.
    /// </remarks>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Gets the time in seconds required to craft this recipe.
    /// </summary>
    /// <remarks>
    /// Used for crafting progress display. More complex recipes
    /// typically have longer crafting times.
    /// </remarks>
    public int CraftingTimeSeconds { get; private set; }

    /// <summary>
    /// Gets the optional icon path for UI display.
    /// </summary>
    public string? IconPath { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory pattern.
    /// </summary>
    private RecipeDefinition() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new recipe definition.
    /// </summary>
    /// <param name="recipeId">Unique recipe identifier (will be lowercased).</param>
    /// <param name="name">Display name.</param>
    /// <param name="description">Description text.</param>
    /// <param name="category">Recipe category for UI filtering.</param>
    /// <param name="requiredStationId">Required crafting station ID (will be lowercased).</param>
    /// <param name="ingredients">List of required ingredients (at least 1).</param>
    /// <param name="output">Recipe output specification.</param>
    /// <param name="difficultyClass">Crafting DC (must be >= 1).</param>
    /// <param name="isDefault">Whether this is a starter recipe.</param>
    /// <param name="craftingTimeSeconds">Time to craft in seconds (must be >= 0).</param>
    /// <param name="iconPath">Optional icon path for UI.</param>
    /// <returns>A new RecipeDefinition instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when required string parameters are null or whitespace,
    /// or when ingredients collection is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when output is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when difficultyClass is less than 1 or craftingTimeSeconds is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var recipe = RecipeDefinition.Create(
    ///     recipeId: "iron-sword",
    ///     name: "Iron Sword",
    ///     description: "A basic iron sword",
    ///     category: RecipeCategory.Weapon,
    ///     requiredStationId: "anvil",
    ///     ingredients: new[] { new RecipeIngredient("iron-ore", 5) },
    ///     output: new RecipeOutput("iron-sword", 1),
    ///     difficultyClass: 12,
    ///     isDefault: true);
    /// </code>
    /// </example>
    public static RecipeDefinition Create(
        string recipeId,
        string name,
        string description,
        RecipeCategory category,
        string requiredStationId,
        IEnumerable<RecipeIngredient> ingredients,
        RecipeOutput output,
        int difficultyClass,
        bool isDefault = false,
        int craftingTimeSeconds = 30,
        string? iconPath = null)
    {
        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeId, nameof(recipeId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredStationId, nameof(requiredStationId));

        // Validate output is not null
        ArgumentNullException.ThrowIfNull(output, nameof(output));

        // Validate numeric constraints
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(difficultyClass, nameof(difficultyClass));
        ArgumentOutOfRangeException.ThrowIfNegative(craftingTimeSeconds, nameof(craftingTimeSeconds));

        // Validate ingredients - must have at least one
        var ingredientList = ingredients?.ToList() ?? [];
        if (ingredientList.Count == 0)
        {
            throw new ArgumentException("Recipe must have at least one ingredient.", nameof(ingredients));
        }

        return new RecipeDefinition
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            Category = category,
            RequiredStationId = requiredStationId.ToLowerInvariant(),
            Ingredients = ingredientList.AsReadOnly(),
            Output = output,
            DifficultyClass = difficultyClass,
            IsDefault = isDefault,
            CraftingTimeSeconds = craftingTimeSeconds,
            IconPath = iconPath
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of resources required.
    /// </summary>
    /// <returns>Sum of all ingredient quantities.</returns>
    /// <example>
    /// <code>
    /// // Recipe requires 5 iron-ore and 2 leather
    /// var total = recipe.GetTotalIngredientCount(); // Returns 7
    /// </code>
    /// </example>
    public int GetTotalIngredientCount()
    {
        return Ingredients.Sum(i => i.Quantity);
    }

    /// <summary>
    /// Gets a specific ingredient by resource ID.
    /// </summary>
    /// <param name="resourceId">The resource ID to find (case-insensitive).</param>
    /// <returns>The ingredient if found; otherwise, null.</returns>
    /// <example>
    /// <code>
    /// var ironIngredient = recipe.GetIngredient("iron-ore");
    /// if (ironIngredient is not null)
    /// {
    ///     Console.WriteLine($"Requires {ironIngredient.Quantity} iron ore");
    /// }
    /// </code>
    /// </example>
    public RecipeIngredient? GetIngredient(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return null;
        }

        var normalizedId = resourceId.ToLowerInvariant();
        return Ingredients.FirstOrDefault(i => i.ResourceId == normalizedId);
    }

    /// <summary>
    /// Checks if the recipe requires a specific resource.
    /// </summary>
    /// <param name="resourceId">The resource ID to check (case-insensitive).</param>
    /// <returns>True if the resource is required; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (recipe.RequiresResource("iron-ore"))
    /// {
    ///     // Player needs iron ore to craft this
    /// }
    /// </code>
    /// </example>
    public bool RequiresResource(string resourceId)
    {
        return GetIngredient(resourceId) is not null;
    }

    /// <summary>
    /// Gets a difficulty description based on the DC.
    /// </summary>
    /// <returns>A human-readable difficulty string.</returns>
    /// <remarks>
    /// <para>
    /// DC to difficulty mapping:
    /// <list type="bullet">
    ///   <item><description>&lt;= 8: Trivial</description></item>
    ///   <item><description>&lt;= 10: Easy</description></item>
    ///   <item><description>&lt;= 12: Moderate</description></item>
    ///   <item><description>&lt;= 14: Challenging</description></item>
    ///   <item><description>&lt;= 16: Hard</description></item>
    ///   <item><description>&lt;= 18: Very Hard</description></item>
    ///   <item><description>&gt; 18: Legendary</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Difficulty: {recipe.GetDifficultyDescription()}");
    /// // Output: Difficulty: Moderate
    /// </code>
    /// </example>
    public string GetDifficultyDescription()
    {
        return DifficultyClass switch
        {
            <= 8 => "Trivial",
            <= 10 => "Easy",
            <= 12 => "Moderate",
            <= 14 => "Challenging",
            <= 16 => "Hard",
            <= 18 => "Very Hard",
            _ => "Legendary"
        };
    }

    /// <summary>
    /// Gets a formatted string showing the ingredient requirements.
    /// </summary>
    /// <returns>A comma-separated list of ingredients with quantities.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Requires: {recipe.GetIngredientsDisplay()}");
    /// // Output: Requires: iron-ore x5, leather x2
    /// </code>
    /// </example>
    public string GetIngredientsDisplay()
    {
        return string.Join(", ", Ingredients.Select(i => i.ToString()));
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the recipe.
    /// </summary>
    /// <returns>The recipe name and ID in format "Name (recipeId)".</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(recipe.ToString());
    /// // Output: Iron Sword (iron-sword)
    /// </code>
    /// </example>
    public override string ToString() => $"{Name} ({RecipeId})";
}
