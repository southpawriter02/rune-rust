using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for managing player recipe knowledge and crafting validation.
/// </summary>
/// <remarks>
/// <para>
/// The recipe service acts as the mediator between recipe definitions (what exists
/// in the game) and player recipe books (what players know). It provides methods for:
/// <list type="bullet">
///   <item><description>Querying known recipes for a player</description></item>
///   <item><description>Learning new recipes</description></item>
///   <item><description>Initializing default recipes for new players</description></item>
///   <item><description>Validating crafting prerequisites</description></item>
/// </list>
/// </para>
/// <para>
/// This service depends on:
/// <list type="bullet">
///   <item><description><see cref="IRecipeProvider"/> - For accessing recipe definitions</description></item>
///   <item><description><see cref="IResourceProvider"/> - For resource lookups during validation</description></item>
///   <item><description><see cref="IGameEventLogger"/> - For publishing learning events</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Initialize default recipes for a new player
/// recipeService.InitializeDefaultRecipes(player);
///
/// // Get recipes a player can craft at an anvil
/// var anvilRecipes = recipeService.GetCraftableRecipes(player, "anvil");
///
/// // Learn a new recipe from a scroll
/// var result = recipeService.LearnRecipe(player, "steel-sword");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine($"Learned: {result.RecipeName}");
/// }
///
/// // Validate before crafting
/// var validation = recipeService.CanCraft(player, "iron-sword", "anvil");
/// if (validation.IsValid)
/// {
///     // Proceed with crafting
/// }
/// </code>
/// </example>
public interface IRecipeService
{
    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all recipes known by a player.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>A read-only list of recipe definitions the player knows.</returns>
    /// <remarks>
    /// Returns only recipes that exist in the recipe provider AND are
    /// in the player's recipe book. Recipes in the book that no longer
    /// exist are filtered out.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <example>
    /// <code>
    /// var recipes = recipeService.GetKnownRecipes(player);
    /// Console.WriteLine($"You know {recipes.Count} recipes");
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetKnownRecipes(Player player);

    /// <summary>
    /// Gets known recipes filtered by category.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="category">The recipe category to filter by.</param>
    /// <returns>A read-only list of known recipes in the specified category.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <example>
    /// <code>
    /// var weapons = recipeService.GetKnownRecipesByCategory(player, RecipeCategory.Weapon);
    /// foreach (var weapon in weapons)
    /// {
    ///     Console.WriteLine($"- {weapon.Name}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetKnownRecipesByCategory(Player player, RecipeCategory category);

    /// <summary>
    /// Gets recipes the player knows that can be crafted at a specific station.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="stationId">The crafting station ID (case-insensitive).</param>
    /// <returns>A read-only list of known recipes for the specified station.</returns>
    /// <remarks>
    /// Filters the player's known recipes to only those requiring the specified
    /// crafting station. This is useful for displaying available recipes when
    /// a player interacts with a crafting station.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <exception cref="ArgumentException">Thrown when stationId is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// // Show recipes when player uses an anvil
    /// var anvilRecipes = recipeService.GetCraftableRecipes(player, "anvil");
    /// Console.WriteLine("Available recipes at this anvil:");
    /// foreach (var recipe in anvilRecipes)
    /// {
    ///     Console.WriteLine($"  {recipe.Name} (DC {recipe.DifficultyClass})");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetCraftableRecipes(Player player, string stationId);

    /// <summary>
    /// Checks if a player knows a specific recipe.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="recipeId">The recipe ID to check (case-insensitive).</param>
    /// <returns>True if the player knows the recipe; otherwise, false.</returns>
    /// <remarks>
    /// Returns false if the recipe doesn't exist in the provider or
    /// if the player doesn't have it in their recipe book.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <example>
    /// <code>
    /// if (recipeService.IsRecipeKnown(player, "steel-sword"))
    /// {
    ///     Console.WriteLine("You can craft Steel Swords!");
    /// }
    /// </code>
    /// </example>
    bool IsRecipeKnown(Player player, string recipeId);

    /// <summary>
    /// Gets a specific recipe if the player knows it.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="recipeId">The recipe ID (case-insensitive).</param>
    /// <returns>The recipe definition if known; otherwise, null.</returns>
    /// <remarks>
    /// Unlike <see cref="IRecipeProvider.GetRecipe"/>, this method only returns
    /// the recipe if the player actually knows it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <example>
    /// <code>
    /// var recipe = recipeService.GetKnownRecipe(player, "iron-sword");
    /// if (recipe != null)
    /// {
    ///     Console.WriteLine($"Ingredients: {recipe.GetIngredientsDisplay()}");
    /// }
    /// </code>
    /// </example>
    RecipeDefinition? GetKnownRecipe(Player player, string recipeId);

    /// <summary>
    /// Gets the count of recipes known by a player.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>The number of recipes the player knows.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <example>
    /// <code>
    /// var known = recipeService.GetKnownRecipeCount(player);
    /// var total = recipeService.GetTotalRecipeCount();
    /// Console.WriteLine($"Recipes: {known}/{total}");
    /// </code>
    /// </example>
    int GetKnownRecipeCount(Player player);

    /// <summary>
    /// Gets the total count of recipes in the game.
    /// </summary>
    /// <returns>The total number of recipe definitions.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total recipes in game: {recipeService.GetTotalRecipeCount()}");
    /// </code>
    /// </example>
    int GetTotalRecipeCount();

    // ═══════════════════════════════════════════════════════════════
    // LEARNING METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to learn a recipe for a player.
    /// </summary>
    /// <param name="player">The player learning the recipe.</param>
    /// <param name="recipeId">The recipe ID to learn (case-insensitive).</param>
    /// <returns>A result indicating success, already known, or not found.</returns>
    /// <remarks>
    /// <para>
    /// If successful, the recipe is added to the player's recipe book and a
    /// <see cref="Events.RecipeLearnedEvent"/> is published.
    /// </para>
    /// <para>
    /// This method does NOT grant default recipes. Use <see cref="InitializeDefaultRecipes"/>
    /// for that purpose.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <exception cref="ArgumentException">Thrown when recipeId is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// var result = recipeService.LearnRecipe(player, "mithril-blade");
    /// switch (result.ResultType)
    /// {
    ///     case LearnResultType.Success:
    ///         Console.WriteLine($"You learned {result.RecipeName}!");
    ///         break;
    ///     case LearnResultType.AlreadyKnown:
    ///         Console.WriteLine("You already know this recipe.");
    ///         break;
    ///     case LearnResultType.NotFound:
    ///         Console.WriteLine("Unknown recipe.");
    ///         break;
    /// }
    /// </code>
    /// </example>
    LearnRecipeResult LearnRecipe(Player player, string recipeId);

    /// <summary>
    /// Initializes a player's recipe book with default recipes.
    /// </summary>
    /// <param name="player">The player to initialize.</param>
    /// <remarks>
    /// <para>
    /// This method should be called during player creation to ensure new players
    /// have access to basic crafting recipes. It does not publish events for
    /// each recipe learned (unlike <see cref="LearnRecipe"/>).
    /// </para>
    /// <para>
    /// Default recipes are determined by the <see cref="IRecipeProvider.GetDefaultRecipes"/>
    /// method. Typically includes basic weapon, armor, and consumable recipes.
    /// </para>
    /// <para>
    /// If called on a player who already has recipes, new default recipes are
    /// added without removing existing ones.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <example>
    /// <code>
    /// // During character creation
    /// var player = new Player("Hero");
    /// recipeService.InitializeDefaultRecipes(player);
    /// Console.WriteLine($"Started with {player.RecipeBook.KnownCount} recipes");
    /// </code>
    /// </example>
    void InitializeDefaultRecipes(Player player);

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates whether a player can craft a specific recipe.
    /// </summary>
    /// <param name="player">The player attempting to craft.</param>
    /// <param name="recipeId">The recipe ID to validate (case-insensitive).</param>
    /// <param name="stationId">The current crafting station ID, or null to skip station check.</param>
    /// <returns>A validation result indicating whether crafting is possible.</returns>
    /// <remarks>
    /// <para>
    /// Performs the following checks in order:
    /// <list type="number">
    ///   <item><description>Recipe exists in the system</description></item>
    ///   <item><description>Player knows the recipe</description></item>
    ///   <item><description>Player is at the correct crafting station (if stationId provided)</description></item>
    ///   <item><description>Player has all required ingredients</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method does not consume any resources. It only validates prerequisites.
    /// Actual crafting should be performed by a separate crafting service (v0.11.2).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <exception cref="ArgumentException">Thrown when recipeId is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// var validation = recipeService.CanCraft(player, "iron-sword", "anvil");
    ///
    /// if (validation.IsValid)
    /// {
    ///     Console.WriteLine("Ready to craft!");
    /// }
    /// else if (validation.IsMissingIngredients)
    /// {
    ///     Console.WriteLine("You need:");
    ///     foreach (var missing in validation.MissingIngredients!)
    ///     {
    ///         Console.WriteLine($"  {missing.ResourceName} x{missing.QuantityNeeded}");
    ///     }
    /// }
    /// else
    /// {
    ///     Console.WriteLine(validation.FailureReason);
    /// }
    /// </code>
    /// </example>
    CraftValidation CanCraft(Player player, string recipeId, string? stationId);
}
