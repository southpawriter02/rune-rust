using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to recipe definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the loading and retrieval of recipe definitions,
/// allowing for different implementations (JSON, database, etc.) while
/// maintaining a consistent API for the application layer.
/// </para>
/// <para>
/// Recipe providers are typically registered as singletons in the DI container,
/// loading all definitions once at startup and caching them for efficient access.
/// Multiple indexes are maintained for fast lookups by category, station, and output item.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Case-insensitive recipe ID lookups</description></item>
///   <item><description>Filtering by category for recipe book UI</description></item>
///   <item><description>Filtering by station for crafting UI</description></item>
///   <item><description>Retrieval of default (starter) recipes</description></item>
///   <item><description>Reverse lookup by output item ID</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get a specific recipe
/// var ironSword = recipeProvider.GetRecipe("iron-sword");
///
/// // Get all weapon recipes
/// var weapons = recipeProvider.GetRecipesByCategory(RecipeCategory.Weapon);
///
/// // Get recipes available at the anvil
/// var anvilRecipes = recipeProvider.GetRecipesForStation("anvil");
///
/// // Get starter recipes for new players
/// var defaults = recipeProvider.GetDefaultRecipes();
///
/// // Find recipes that produce a specific item
/// var swordRecipes = recipeProvider.GetRecipesForItem("iron-sword");
/// </code>
/// </example>
public interface IRecipeProvider
{
    /// <summary>
    /// Gets a recipe definition by its unique string identifier.
    /// </summary>
    /// <param name="recipeId">The recipe identifier (case-insensitive).</param>
    /// <returns>The recipe definition, or null if not found.</returns>
    /// <example>
    /// <code>
    /// var recipe = recipeProvider.GetRecipe("iron-sword");
    /// if (recipe is not null)
    /// {
    ///     Console.WriteLine($"Found: {recipe.Name}, DC: {recipe.DifficultyClass}");
    /// }
    /// </code>
    /// </example>
    RecipeDefinition? GetRecipe(string recipeId);

    /// <summary>
    /// Gets all registered recipe definitions.
    /// </summary>
    /// <returns>A read-only list of all recipe definitions.</returns>
    /// <remarks>
    /// The returned list is a snapshot and will not reflect any subsequent changes
    /// if the provider supports dynamic reloading.
    /// </remarks>
    IReadOnlyList<RecipeDefinition> GetAllRecipes();

    /// <summary>
    /// Gets all recipes belonging to a specific category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>A read-only list of matching recipe definitions.</returns>
    /// <remarks>
    /// Used by the recipe book UI to display recipes organized by category.
    /// </remarks>
    /// <example>
    /// <code>
    /// var potions = recipeProvider.GetRecipesByCategory(RecipeCategory.Potion);
    /// foreach (var potion in potions)
    /// {
    ///     Console.WriteLine($"- {potion.Name}: DC {potion.DifficultyClass}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetRecipesByCategory(RecipeCategory category);

    /// <summary>
    /// Gets all recipes that can be crafted at a specific station.
    /// </summary>
    /// <param name="stationId">The station identifier (case-insensitive).</param>
    /// <returns>A read-only list of recipes requiring the specified station.</returns>
    /// <remarks>
    /// Used by the crafting UI to show available recipes when a player
    /// interacts with a crafting station.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Player approaches the anvil
    /// var anvilRecipes = recipeProvider.GetRecipesForStation("anvil");
    /// Console.WriteLine($"Available recipes at anvil: {anvilRecipes.Count}");
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetRecipesForStation(string stationId);

    /// <summary>
    /// Gets all default (starter) recipes.
    /// </summary>
    /// <returns>A read-only list of recipes with IsDefault = true.</returns>
    /// <remarks>
    /// Default recipes are automatically added to new players' recipe books.
    /// Non-default recipes must be discovered through recipe scrolls.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Initialize new player's recipe book
    /// var starterRecipes = recipeProvider.GetDefaultRecipes();
    /// foreach (var recipe in starterRecipes)
    /// {
    ///     player.RecipeBook.Add(recipe.RecipeId);
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetDefaultRecipes();

    /// <summary>
    /// Gets all recipes that produce a specific item.
    /// </summary>
    /// <param name="itemId">The output item identifier (case-insensitive).</param>
    /// <returns>A read-only list of recipes that produce the specified item.</returns>
    /// <remarks>
    /// Used for reverse lookups, such as showing how to craft an item
    /// from the item's tooltip or the recipe discovery system.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find how to craft a healing potion
    /// var recipes = recipeProvider.GetRecipesForItem("healing-potion");
    /// if (recipes.Count > 0)
    /// {
    ///     var recipe = recipes[0];
    ///     Console.WriteLine($"Craft at: {recipe.RequiredStationId}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetRecipesForItem(string itemId);

    /// <summary>
    /// Checks if a recipe with the specified ID exists.
    /// </summary>
    /// <param name="recipeId">The recipe identifier to check (case-insensitive).</param>
    /// <returns>True if the recipe exists, false otherwise.</returns>
    /// <example>
    /// <code>
    /// if (recipeProvider.Exists("mithril-blade"))
    /// {
    ///     Console.WriteLine("Mithril blade recipe is available!");
    /// }
    /// </code>
    /// </example>
    bool Exists(string recipeId);

    /// <summary>
    /// Gets the total count of registered recipes.
    /// </summary>
    /// <returns>The number of recipe definitions loaded.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total recipes available: {recipeProvider.GetRecipeCount()}");
    /// </code>
    /// </example>
    int GetRecipeCount();
}
