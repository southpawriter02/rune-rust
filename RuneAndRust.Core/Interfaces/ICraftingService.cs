using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Crafting;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Provides crafting operations including recipe lookup, ingredient validation, and item creation.
/// Crafting uses WITS-based dice rolls to determine success and quality.
/// </summary>
public interface ICraftingService
{
    /// <summary>
    /// Attempts to craft an item using a recipe.
    /// Consumes ingredients regardless of outcome, rolls WITS pool against DC.
    /// </summary>
    /// <param name="crafter">The character performing the craft.</param>
    /// <param name="recipeId">The unique identifier of the recipe to craft.</param>
    /// <returns>A CraftingResult containing the outcome and any produced items.</returns>
    Task<CraftingResult> CraftItemAsync(Character crafter, string recipeId);

    /// <summary>
    /// Checks if the character has all required ingredients for a recipe.
    /// </summary>
    /// <param name="crafter">The character whose inventory to check.</param>
    /// <param name="recipe">The recipe with ingredient requirements.</param>
    /// <returns>True if all ingredients are present in sufficient quantity.</returns>
    bool HasIngredients(Character crafter, Recipe recipe);

    /// <summary>
    /// Gets all recipes the character can currently craft (has ingredients for).
    /// </summary>
    /// <param name="crafter">The character whose inventory to check.</param>
    /// <returns>List of recipes with all required ingredients available.</returns>
    IReadOnlyList<Recipe> GetAvailableRecipes(Character crafter);

    /// <summary>
    /// Gets all recipes for a specific crafting trade.
    /// </summary>
    /// <param name="trade">The trade to filter by.</param>
    /// <returns>List of recipes belonging to the specified trade.</returns>
    IReadOnlyList<Recipe> GetRecipesByTrade(CraftingTrade trade);

    /// <summary>
    /// Gets a recipe by its unique identifier.
    /// </summary>
    /// <param name="recipeId">The recipe identifier.</param>
    /// <returns>The recipe if found, null otherwise.</returns>
    Recipe? GetRecipe(string recipeId);

    /// <summary>
    /// Gets all registered recipes regardless of availability.
    /// </summary>
    /// <returns>List of all recipes in the registry.</returns>
    IReadOnlyList<Recipe> GetAllRecipes();
}
