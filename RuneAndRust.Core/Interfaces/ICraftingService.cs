using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Crafting;
using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Provides crafting operations including recipe lookup, ingredient validation, and item creation.
/// Crafting uses WITS-based dice rolls to determine success and quality.
/// </summary>
/// <remarks>See: SPEC-CRAFT-001 for Crafting System design.</remarks>
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

    /// <summary>
    /// Builds a ViewModel for the crafting UI from current character state (v0.3.7b).
    /// </summary>
    /// <param name="crafter">The character to build the view for.</param>
    /// <param name="trade">The trade to filter recipes by.</param>
    /// <param name="selectedIndex">The currently selected recipe index (0-based).</param>
    /// <returns>A CraftingViewModel with all display-ready data.</returns>
    CraftingViewModel BuildViewModel(Character crafter, CraftingTrade trade, int selectedIndex = 0);
}
