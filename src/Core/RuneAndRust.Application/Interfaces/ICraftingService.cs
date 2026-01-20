using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

// NOTE: Methods that require room context accept Room as a parameter.
// The caller is responsible for providing the room context (typically from the game session).

/// <summary>
/// Service for crafting items at crafting stations.
/// </summary>
/// <remarks>
/// <para>
/// The crafting service orchestrates the complete crafting process:
/// <list type="number">
///   <item><description>Validates that the player is at an appropriate crafting station</description></item>
///   <item><description>Verifies the player knows the recipe</description></item>
///   <item><description>Checks resource availability in the player's inventory</description></item>
///   <item><description>Executes the dice check (d20 + skill modifier vs DC)</description></item>
///   <item><description>On success: consumes resources and creates the item with quality</description></item>
///   <item><description>On failure: applies partial resource loss based on margin</description></item>
/// </list>
/// </para>
/// <para>
/// This service integrates with:
/// <list type="bullet">
///   <item><description><see cref="IRecipeProvider"/> - For recipe definitions</description></item>
///   <item><description><see cref="IRecipeService"/> - For recipe knowledge validation</description></item>
///   <item><description><see cref="ICraftingStationProvider"/> - For station definitions and skill lookups</description></item>
///   <item><description><see cref="IResourceProvider"/> - For resource information</description></item>
///   <item><description><see cref="IGameEventLogger"/> - For publishing crafting events</description></item>
/// </list>
/// </para>
/// <para>
/// Quality is determined by the roll margin:
/// <list type="bullet">
///   <item><description>Natural 20: Legendary (regardless of total)</description></item>
///   <item><description>Margin >= 10: Masterwork</description></item>
///   <item><description>Margin >= 5: Fine</description></item>
///   <item><description>Otherwise: Standard</description></item>
/// </list>
/// </para>
/// <para>
/// Failure resource loss is graduated:
/// <list type="bullet">
///   <item><description>Close failure (margin >= -5): 25% of each ingredient lost</description></item>
///   <item><description>Bad failure (margin &lt; -5): 50% of each ingredient lost</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Check if crafting is possible
/// var validation = craftingService.CanCraft(player, "iron-sword");
/// if (validation.IsValid)
/// {
///     Console.WriteLine($"Ready to craft at {validation.Station!.Name}");
///
///     // Attempt crafting
///     var result = craftingService.Craft(player, "iron-sword");
///
///     if (result.IsSuccess)
///     {
///         Console.WriteLine($"Crafted {result.CraftedItem!.Name} ({result.Quality} quality)!");
///         Console.WriteLine(result.GetRollDisplay()); // d20(15) +4 = 19 vs DC 12
///     }
///     else if (result.WasDiceRollFailure)
///     {
///         Console.WriteLine($"Failed! Lost {result.TotalResourcesLost} resources.");
///         Console.WriteLine(result.GetRollDisplay());
///     }
///     else
///     {
///         Console.WriteLine(result.FailureReason);
///     }
/// }
/// else if (validation.IsMissingIngredients)
/// {
///     Console.WriteLine("Missing ingredients:");
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
public interface ICraftingService
{
    // ═══════════════════════════════════════════════════════════════
    // VALIDATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates whether a player can craft a specific recipe.
    /// </summary>
    /// <param name="player">The player attempting to craft.</param>
    /// <param name="recipeId">The recipe identifier (case-insensitive).</param>
    /// <param name="room">The room where the player is located (for station lookup).</param>
    /// <returns>A validation result indicating success or failure with details.</returns>
    /// <remarks>
    /// <para>
    /// Performs the following checks in order:
    /// <list type="number">
    ///   <item><description>Player is at an appropriate crafting station</description></item>
    ///   <item><description>Recipe exists in the system</description></item>
    ///   <item><description>Player knows the recipe</description></item>
    ///   <item><description>Station supports the recipe's category</description></item>
    ///   <item><description>Player has all required resources</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method does not consume any resources or roll dice. It only validates
    /// prerequisites. Use <see cref="Craft"/> to actually perform the crafting.
    /// </para>
    /// <para>
    /// On success, the returned <see cref="CraftValidation"/> includes:
    /// <list type="bullet">
    ///   <item><description><see cref="CraftValidation.Recipe"/> - The recipe definition</description></item>
    ///   <item><description><see cref="CraftValidation.Station"/> - The crafting station being used</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player or room is null.</exception>
    /// <exception cref="ArgumentException">Thrown when recipeId is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// var validation = craftingService.CanCraft(player, "iron-sword", currentRoom);
    ///
    /// if (validation.IsValid)
    /// {
    ///     Console.WriteLine($"Ready to craft at {validation.Station!.Name}!");
    /// }
    /// else if (validation.IsMissingIngredients)
    /// {
    ///     Console.WriteLine("Missing resources:");
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
    CraftValidation CanCraft(Player player, string recipeId, Room room);

    // ═══════════════════════════════════════════════════════════════
    // CRAFTING METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to craft an item from a recipe.
    /// </summary>
    /// <param name="player">The player attempting to craft.</param>
    /// <param name="recipeId">The recipe identifier (case-insensitive).</param>
    /// <param name="room">The room where the player is located (for station lookup).</param>
    /// <returns>
    /// A craft result containing success/failure status, dice roll details,
    /// created item (on success), or lost resources (on failure).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method first validates using <see cref="CanCraft"/>. If validation fails,
    /// it returns a <see cref="CraftResult"/> with <see cref="CraftResult.IsSuccess"/> = false
    /// and the validation failure reason.
    /// </para>
    /// <para>
    /// The crafting dice check is: d20 + skill modifier vs recipe difficulty class (DC).
    /// The skill is determined by the station type (e.g., smithing for anvil).
    /// </para>
    /// <para>
    /// On success (roll total >= DC):
    /// <list type="bullet">
    ///   <item><description>All required resources are consumed</description></item>
    ///   <item><description>Item is created with quality based on roll</description></item>
    ///   <item><description>Item is added to player's inventory</description></item>
    ///   <item><description><see cref="Domain.Events.ItemCraftedEvent"/> is published</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// On failure (roll total &lt; DC):
    /// <list type="bullet">
    ///   <item><description>Partial resources are lost based on failure margin</description></item>
    ///   <item><description>Close failure (margin >= -5): 25% loss</description></item>
    ///   <item><description>Bad failure (margin &lt; -5): 50% loss</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// A <see cref="Domain.Events.CraftAttemptedEvent"/> is always published regardless of outcome.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player or room is null.</exception>
    /// <exception cref="ArgumentException">Thrown when recipeId is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// var result = craftingService.Craft(player, "iron-sword", currentRoom);
    ///
    /// Console.WriteLine(result.GetRollDisplay());
    /// // Output: d20(15) +4 = 19 vs DC 12
    ///
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine($"Crafted {result.CraftedItem!.Name} ({result.Quality})!");
    /// }
    /// else if (result.WasDiceRollFailure)
    /// {
    ///     var failType = result.IsBadFailure ? "BAD FAILURE" : "CLOSE FAILURE";
    ///     Console.WriteLine($"{failType}: {result.FailureReason}");
    ///     Console.WriteLine($"Lost {result.TotalResourcesLost} resources.");
    /// }
    /// else
    /// {
    ///     Console.WriteLine(result.FailureReason);
    /// }
    /// </code>
    /// </example>
    CraftResult Craft(Player player, string recipeId, Room room);

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all recipes the player can craft at a station in the specified room.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="room">The room to check for crafting stations.</param>
    /// <returns>
    /// Recipes the player knows that can be crafted at the room's station.
    /// Empty if no station is present or player knows no suitable recipes.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks:
    /// <list type="bullet">
    ///   <item><description>Room has an available crafting station</description></item>
    ///   <item><description>Player knows the recipe</description></item>
    ///   <item><description>Recipe's category is supported by the station</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Does NOT check resource availability. Use <see cref="GetReadyToCraftRecipes"/>
    /// for recipes that can be crafted immediately.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player or room is null.</exception>
    /// <example>
    /// <code>
    /// var recipes = craftingService.GetCraftableRecipesHere(player, currentRoom);
    ///
    /// if (recipes.Count == 0)
    /// {
    ///     Console.WriteLine("No recipes available at this station.");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Available recipes at {craftingService.GetCurrentStation(currentRoom)?.Name}:");
    ///     foreach (var recipe in recipes)
    ///     {
    ///         Console.WriteLine($"  - {recipe.Name} (DC {recipe.DifficultyClass})");
    ///     }
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetCraftableRecipesHere(Player player, Room room);

    /// <summary>
    /// Gets recipes the player has resources for at a station in the specified room.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="room">The room to check for crafting stations.</param>
    /// <returns>
    /// Recipes that can be immediately crafted (known, at correct station, have all resources).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a stricter filter than <see cref="GetCraftableRecipesHere"/>. It only
    /// returns recipes where the player has all required ingredients in their inventory.
    /// </para>
    /// <para>
    /// Useful for highlighting recipes that are "ready to craft" in the UI.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player or room is null.</exception>
    /// <example>
    /// <code>
    /// var readyRecipes = craftingService.GetReadyToCraftRecipes(player, currentRoom);
    ///
    /// Console.WriteLine("Ready to craft now:");
    /// foreach (var recipe in readyRecipes)
    /// {
    ///     Console.WriteLine($"  [*] {recipe.Name}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<RecipeDefinition> GetReadyToCraftRecipes(Player player, Room room);

    /// <summary>
    /// Calculates the player's crafting modifier for a specific station.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="stationId">The station identifier (case-insensitive).</param>
    /// <returns>
    /// The skill modifier based on the station's required crafting skill,
    /// or 0 if the station is not found or player lacks the skill.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The modifier is derived from the player's skill level in the station's
    /// associated crafting skill. For example:
    /// <list type="bullet">
    ///   <item><description>Anvil uses "smithing" skill</description></item>
    ///   <item><description>Workbench uses "leatherworking" skill</description></item>
    ///   <item><description>Alchemy Table uses "alchemy" skill</description></item>
    ///   <item><description>Enchanting Altar uses "enchanting" skill</description></item>
    ///   <item><description>Cooking Fire uses "cooking" skill</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The modifier is added to the d20 roll when crafting: d20 + modifier vs DC.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <exception cref="ArgumentException">Thrown when stationId is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// var modifier = craftingService.GetCraftingModifier(player, "anvil");
    /// Console.WriteLine($"Smithing modifier: {(modifier >= 0 ? "+" : "")}{modifier}");
    /// // Output: Smithing modifier: +4
    /// </code>
    /// </example>
    int GetCraftingModifier(Player player, string stationId);

    /// <summary>
    /// Gets the crafting station in the specified room.
    /// </summary>
    /// <param name="room">The room to check for crafting stations.</param>
    /// <returns>
    /// The first available crafting station in the room, or null if none exists.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Checks the specified room for crafting station features.
    /// Returns the first station that is available (not in use).
    /// </para>
    /// <para>
    /// Returns null if:
    /// <list type="bullet">
    ///   <item><description>Room is null</description></item>
    ///   <item><description>Room has no crafting stations</description></item>
    ///   <item><description>All stations are currently in use</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when room is null.</exception>
    /// <example>
    /// <code>
    /// var station = craftingService.GetCurrentStation(currentRoom);
    ///
    /// if (station is not null)
    /// {
    ///     Console.WriteLine($"You are at a {station.Name}.");
    ///     Console.WriteLine(station.GetInteractionPrompt());
    /// }
    /// else
    /// {
    ///     Console.WriteLine("No crafting station available here.");
    /// }
    /// </code>
    /// </example>
    CraftingStation? GetCurrentStation(Room room);

    // ═══════════════════════════════════════════════════════════════
    // CALCULATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates resource loss for a failed crafting attempt.
    /// </summary>
    /// <param name="recipe">The recipe that was being crafted.</param>
    /// <param name="rollMargin">The difference between roll total and DC (negative for failures).</param>
    /// <returns>
    /// A list of resource losses to apply. Each entry specifies the resource ID,
    /// name, and quantity lost.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Loss percentage depends on how badly the roll failed:
    /// <list type="bullet">
    ///   <item><description>Close failure (margin >= -5): 25% of each ingredient lost</description></item>
    ///   <item><description>Bad failure (margin &lt; -5): 50% of each ingredient lost</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The formula for each ingredient is: MAX(1, CEILING(quantity * lossPercent))
    /// This ensures at least 1 of each ingredient is lost.
    /// </para>
    /// <para>
    /// This is a pure calculation method that does not modify the player's inventory.
    /// Use it to preview losses or for testing purposes.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when recipe is null.</exception>
    /// <example>
    /// <code>
    /// // Calculate what would be lost on a close failure
    /// var losses = craftingService.CalculateFailureLoss(recipe, -3);
    ///
    /// Console.WriteLine("Close failure (25% loss):");
    /// foreach (var loss in losses)
    /// {
    ///     Console.WriteLine($"  - {loss.ToDisplayString()}"); // "Iron Ore x2"
    /// }
    ///
    /// // Calculate what would be lost on a bad failure
    /// var badLosses = craftingService.CalculateFailureLoss(recipe, -7);
    ///
    /// Console.WriteLine("Bad failure (50% loss):");
    /// foreach (var loss in badLosses)
    /// {
    ///     Console.WriteLine($"  - {loss.ToDisplayString()}"); // "Iron Ore x3"
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<ResourceLoss> CalculateFailureLoss(RecipeDefinition recipe, int rollMargin);
}
