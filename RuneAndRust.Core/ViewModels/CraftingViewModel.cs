using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Immutable snapshot of crafting state for UI rendering (v0.3.7b).
/// Transforms raw recipe data into display-ready format with ingredient availability.
/// </summary>
/// <remarks>See: SPEC-UI-001 for UI Framework System design.</remarks>
/// <param name="CharacterName">The player character's display name.</param>
/// <param name="CrafterWits">The crafter's WITS attribute for DC comparison display.</param>
/// <param name="SelectedTrade">The currently active trade filter.</param>
/// <param name="Recipes">Filtered recipe list for the selected trade.</param>
/// <param name="SelectedRecipeIndex">Currently selected recipe index (0-based, for navigation).</param>
/// <param name="SelectedRecipeDetails">Detail panel data for the selected recipe (null if no selection).</param>
public record CraftingViewModel(
    string CharacterName,
    int CrafterWits,
    CraftingTrade SelectedTrade,
    List<RecipeView> Recipes,
    int SelectedRecipeIndex,
    RecipeDetailsView? SelectedRecipeDetails
);

/// <summary>
/// Display-ready view of a recipe for the recipe list panel.
/// </summary>
/// <param name="Index">1-based display index for navigation.</param>
/// <param name="RecipeId">Unique recipe identifier for crafting execution.</param>
/// <param name="Name">Display name of the recipe.</param>
/// <param name="Trade">Crafting trade for icon/color coding.</param>
/// <param name="DifficultyClass">Base DC for the crafting roll.</param>
/// <param name="CanCraft">Whether all ingredients are available.</param>
public record RecipeView(
    int Index,
    string RecipeId,
    string Name,
    CraftingTrade Trade,
    int DifficultyClass,
    bool CanCraft
);

/// <summary>
/// Display-ready view of recipe details for the detail panel.
/// Includes ingredient breakdown with availability status.
/// </summary>
/// <param name="RecipeId">Unique recipe identifier.</param>
/// <param name="Name">Display name of the recipe.</param>
/// <param name="Description">Narrative description of the crafting process.</param>
/// <param name="Trade">Crafting trade for display.</param>
/// <param name="DifficultyClass">Base DC for the crafting roll.</param>
/// <param name="CrafterWits">Crafter's WITS for success chance calculation.</param>
/// <param name="Ingredients">List of required ingredients with availability.</param>
/// <param name="OutputItemName">Name of the item produced on success.</param>
/// <param name="OutputQuantity">Number of items produced on success.</param>
/// <param name="CanCraft">Whether all ingredients are satisfied.</param>
public record RecipeDetailsView(
    string RecipeId,
    string Name,
    string Description,
    CraftingTrade Trade,
    int DifficultyClass,
    int CrafterWits,
    List<IngredientView> Ingredients,
    string OutputItemName,
    int OutputQuantity,
    bool CanCraft
);

/// <summary>
/// Display-ready view of a single ingredient requirement.
/// </summary>
/// <param name="ItemName">Display name of the required item.</param>
/// <param name="RequiredQuantity">Number of items needed for the recipe.</param>
/// <param name="AvailableQuantity">Number of items in the crafter's inventory.</param>
/// <param name="IsSatisfied">Whether AvailableQuantity >= RequiredQuantity.</param>
public record IngredientView(
    string ItemName,
    int RequiredQuantity,
    int AvailableQuantity,
    bool IsSatisfied
);
