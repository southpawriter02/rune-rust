using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing player recipe knowledge and crafting validation.
/// </summary>
/// <remarks>
/// <para>
/// The RecipeService acts as the mediator between recipe definitions (what exists
/// in the game) and player recipe books (what players know). It provides methods for:
/// <list type="bullet">
///   <item><description>Querying known recipes for a player</description></item>
///   <item><description>Learning new recipes</description></item>
///   <item><description>Initializing default recipes for new players</description></item>
///   <item><description>Validating crafting prerequisites</description></item>
/// </list>
/// </para>
/// <para>
/// All recipe ID lookups are case-insensitive. The service normalizes IDs to
/// lowercase before storage and comparison.
/// </para>
/// <para>
/// Dependencies:
/// <list type="bullet">
///   <item><description><see cref="IRecipeProvider"/> - For accessing recipe definitions</description></item>
///   <item><description><see cref="IResourceProvider"/> - For resource lookups during validation</description></item>
///   <item><description><see cref="IGameEventLogger"/> - For publishing learning events</description></item>
///   <item><description><see cref="ILogger{TCategoryName}"/> - For diagnostic logging</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var recipeService = new RecipeService(
///     recipeProvider,
///     resourceProvider,
///     gameEventLogger,
///     logger);
///
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
public class RecipeService : IRecipeService
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for accessing recipe definitions.
    /// </summary>
    private readonly IRecipeProvider _recipeProvider;

    /// <summary>
    /// Provider for resource definitions used in ingredient validation.
    /// </summary>
    private readonly IResourceProvider _resourceProvider;

    /// <summary>
    /// Logger for game events (recipe learning, etc.).
    /// </summary>
    private readonly IGameEventLogger _eventLogger;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<RecipeService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new RecipeService instance.
    /// </summary>
    /// <param name="recipeProvider">Provider for recipe definitions.</param>
    /// <param name="resourceProvider">Provider for resource definitions.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <example>
    /// <code>
    /// var recipeService = new RecipeService(
    ///     recipeProvider,
    ///     resourceProvider,
    ///     gameEventLogger,
    ///     logger);
    /// </code>
    /// </example>
    public RecipeService(
        IRecipeProvider recipeProvider,
        IResourceProvider resourceProvider,
        IGameEventLogger eventLogger,
        ILogger<RecipeService> logger)
    {
        // Validate all dependencies are provided
        _recipeProvider = recipeProvider ?? throw new ArgumentNullException(nameof(recipeProvider));
        _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Log service initialization with recipe count for diagnostics
        _logger.LogInformation(
            "RecipeService initialized with {RecipeCount} recipe definitions and {ResourceCount} resource definitions",
            _recipeProvider.GetRecipeCount(),
            _resourceProvider.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Returns only recipes that:
    /// <list type="number">
    ///   <item><description>Exist in the recipe provider</description></item>
    ///   <item><description>Are in the player's recipe book</description></item>
    /// </list>
    /// Recipes in the book that no longer exist in the provider are filtered out.
    /// </para>
    /// </remarks>
    public IReadOnlyList<RecipeDefinition> GetKnownRecipes(Player player)
    {
        // Validate player parameter
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug(
            "Getting known recipes for player {PlayerId} ({PlayerName})",
            player.Id,
            player.Name);

        // Ensure the player has a recipe book (handles legacy players)
        player.EnsureRecipeBook();

        // Filter recipe definitions to only those known by the player
        // This also handles cleanup of orphaned recipe IDs in the book
        var knownRecipes = player.RecipeBook.KnownRecipeIds
            .Select(id => _recipeProvider.GetRecipe(id))
            .Where(recipe => recipe is not null)
            .Cast<RecipeDefinition>()
            .ToList()
            .AsReadOnly();

        _logger.LogDebug(
            "Player {PlayerId} knows {KnownCount} recipes (book contains {BookCount} IDs)",
            player.Id,
            knownRecipes.Count,
            player.RecipeBook.KnownCount);

        return knownRecipes;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Filters the player's known recipes to only those matching the specified category.
    /// </remarks>
    public IReadOnlyList<RecipeDefinition> GetKnownRecipesByCategory(Player player, RecipeCategory category)
    {
        // Validate player parameter
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug(
            "Getting known recipes for player {PlayerId} in category {Category}",
            player.Id,
            category);

        // Get all known recipes, then filter by category
        var recipes = GetKnownRecipes(player)
            .Where(r => r.Category == category)
            .ToList()
            .AsReadOnly();

        _logger.LogDebug(
            "Player {PlayerId} knows {Count} recipes in category {Category}",
            player.Id,
            recipes.Count,
            category);

        return recipes;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Filters the player's known recipes to only those that can be crafted at
    /// the specified station. Station ID comparison is case-insensitive.
    /// </para>
    /// <para>
    /// Use this method to populate the crafting UI when a player interacts with
    /// a crafting station.
    /// </para>
    /// </remarks>
    public IReadOnlyList<RecipeDefinition> GetCraftableRecipes(Player player, string stationId)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(stationId);

        // Normalize station ID to lowercase for case-insensitive comparison
        var normalizedStationId = stationId.ToLowerInvariant();

        _logger.LogDebug(
            "Getting craftable recipes for player {PlayerId} at station '{StationId}'",
            player.Id,
            normalizedStationId);

        // Get known recipes that match the station requirement
        var recipes = GetKnownRecipes(player)
            .Where(r => r.RequiredStationId.Equals(normalizedStationId, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        _logger.LogDebug(
            "Player {PlayerId} can craft {Count} recipes at station '{StationId}'",
            player.Id,
            recipes.Count,
            normalizedStationId);

        return recipes;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Returns false if:
    /// <list type="bullet">
    ///   <item><description>The recipe doesn't exist in the provider</description></item>
    ///   <item><description>The player doesn't have it in their recipe book</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Recipe ID comparison is case-insensitive.
    /// </para>
    /// </remarks>
    public bool IsRecipeKnown(Player player, string recipeId)
    {
        // Validate player parameter
        ArgumentNullException.ThrowIfNull(player);

        // Return false for null/empty recipe IDs (defensive programming)
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            _logger.LogDebug(
                "IsRecipeKnown called with null/empty recipeId for player {PlayerId}",
                player.Id);
            return false;
        }

        // Ensure the player has a recipe book
        player.EnsureRecipeBook();

        // Check if recipe exists in provider AND in player's book
        var exists = _recipeProvider.Exists(recipeId);
        var known = player.RecipeBook.IsKnown(recipeId);

        _logger.LogDebug(
            "Recipe '{RecipeId}' for player {PlayerId}: exists={Exists}, known={Known}",
            recipeId,
            player.Id,
            exists,
            known);

        // Both conditions must be true
        return exists && known;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Unlike <see cref="IRecipeProvider.GetRecipe"/>, this method only returns
    /// the recipe if the player actually knows it.
    /// </para>
    /// <para>
    /// Returns null if:
    /// <list type="bullet">
    ///   <item><description>The recipe ID is null or whitespace</description></item>
    ///   <item><description>The recipe doesn't exist in the provider</description></item>
    ///   <item><description>The player doesn't know the recipe</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public RecipeDefinition? GetKnownRecipe(Player player, string recipeId)
    {
        // Validate player parameter
        ArgumentNullException.ThrowIfNull(player);

        // Return null for invalid recipe IDs
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            _logger.LogDebug(
                "GetKnownRecipe called with null/empty recipeId for player {PlayerId}",
                player.Id);
            return null;
        }

        _logger.LogDebug(
            "Getting known recipe '{RecipeId}' for player {PlayerId}",
            recipeId,
            player.Id);

        // Only return the recipe if the player knows it
        if (!IsRecipeKnown(player, recipeId))
        {
            _logger.LogDebug(
                "Player {PlayerId} does not know recipe '{RecipeId}'",
                player.Id,
                recipeId);
            return null;
        }

        // Recipe is known, fetch and return the definition
        return _recipeProvider.GetRecipe(recipeId);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the count of recipe IDs in the player's recipe book that also
    /// exist in the recipe provider. Orphaned IDs are not counted.
    /// </remarks>
    public int GetKnownRecipeCount(Player player)
    {
        // Validate player parameter
        ArgumentNullException.ThrowIfNull(player);

        // GetKnownRecipes handles filtering and returns only valid recipes
        var count = GetKnownRecipes(player).Count;

        _logger.LogDebug(
            "Player {PlayerId} knows {Count} valid recipes",
            player.Id,
            count);

        return count;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Delegates to the recipe provider's count method.
    /// </remarks>
    public int GetTotalRecipeCount()
    {
        var count = _recipeProvider.GetRecipeCount();

        _logger.LogDebug(
            "Total recipes in provider: {Count}",
            count);

        return count;
    }

    // ═══════════════════════════════════════════════════════════════
    // LEARNING METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Learning flow:
    /// <list type="number">
    ///   <item><description>Validate recipe ID is not null/whitespace</description></item>
    ///   <item><description>Check if recipe exists in provider</description></item>
    ///   <item><description>Check if player already knows the recipe</description></item>
    ///   <item><description>Add recipe to player's recipe book</description></item>
    ///   <item><description>Publish RecipeLearnedEvent via game event logger</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method does NOT grant default recipes. Use <see cref="InitializeDefaultRecipes"/>
    /// for that purpose.
    /// </para>
    /// </remarks>
    public LearnRecipeResult LearnRecipe(Player player, string recipeId)
    {
        // Validate player parameter
        ArgumentNullException.ThrowIfNull(player);

        // Validate recipe ID parameter
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeId);

        // Normalize recipe ID for consistent lookups
        var normalizedId = recipeId.ToLowerInvariant();

        _logger.LogDebug(
            "Player {PlayerId} ({PlayerName}) attempting to learn recipe '{RecipeId}'",
            player.Id,
            player.Name,
            normalizedId);

        // Ensure the player has a recipe book
        player.EnsureRecipeBook();

        // Step 1: Check if recipe exists in provider
        var recipe = _recipeProvider.GetRecipe(normalizedId);
        if (recipe is null)
        {
            _logger.LogWarning(
                "Player {PlayerId} attempted to learn non-existent recipe '{RecipeId}'",
                player.Id,
                normalizedId);

            return LearnRecipeResult.RecipeNotFound(normalizedId);
        }

        // Step 2: Check if player already knows this recipe
        if (player.RecipeBook.IsKnown(normalizedId))
        {
            _logger.LogDebug(
                "Player {PlayerId} already knows recipe '{RecipeId}' ({RecipeName})",
                player.Id,
                normalizedId,
                recipe.Name);

            return LearnRecipeResult.AlreadyKnown(normalizedId, recipe.Name);
        }

        // Step 3: Add recipe to player's book
        var learned = player.RecipeBook.Learn(normalizedId);
        if (!learned)
        {
            // This shouldn't happen since we checked IsKnown above, but handle defensively
            _logger.LogWarning(
                "Unexpected: RecipeBook.Learn returned false for recipe '{RecipeId}' (player {PlayerId})",
                normalizedId,
                player.Id);

            return LearnRecipeResult.AlreadyKnown(normalizedId, recipe.Name);
        }

        _logger.LogInformation(
            "Player {PlayerId} ({PlayerName}) learned recipe '{RecipeId}' ({RecipeName})",
            player.Id,
            player.Name,
            normalizedId,
            recipe.Name);

        // Step 4: Publish event via game event logger
        PublishRecipeLearnedEvent(player, recipe);

        return LearnRecipeResult.Success(normalizedId, recipe.Name);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// This method should be called during player creation to ensure new players
    /// have access to basic crafting recipes. It does not publish events for
    /// each recipe learned (unlike <see cref="LearnRecipe"/>).
    /// </para>
    /// <para>
    /// Default recipes are determined by <see cref="IRecipeProvider.GetDefaultRecipes"/>.
    /// Typically includes basic weapon, armor, and consumable recipes.
    /// </para>
    /// <para>
    /// If called on a player who already has recipes, new default recipes are
    /// added without removing existing ones.
    /// </para>
    /// </remarks>
    public void InitializeDefaultRecipes(Player player)
    {
        // Validate player parameter
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug(
            "Initializing default recipes for player {PlayerId} ({PlayerName})",
            player.Id,
            player.Name);

        // Ensure the player has a recipe book
        player.EnsureRecipeBook();

        // Get default recipe IDs from provider
        var defaultRecipes = _recipeProvider.GetDefaultRecipes();
        var defaultRecipeIds = defaultRecipes.Select(r => r.RecipeId);

        // Initialize the recipe book with default IDs
        // This handles duplicates internally (won't add if already known)
        player.RecipeBook.InitializeDefaults(defaultRecipeIds);

        _logger.LogInformation(
            "Initialized {DefaultCount} default recipes for player {PlayerId} ({PlayerName}). " +
            "Player now knows {TotalCount} recipes",
            defaultRecipes.Count,
            player.Id,
            player.Name,
            player.RecipeBook.KnownCount);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Performs the following validation checks in order:
    /// <list type="number">
    ///   <item><description>Recipe exists in the system</description></item>
    ///   <item><description>Player knows the recipe</description></item>
    ///   <item><description>Player is at the correct crafting station (if stationId provided)</description></item>
    ///   <item><description>Player has all required ingredients</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Validation stops at the first failure and returns appropriate feedback.
    /// </para>
    /// <para>
    /// This method does NOT consume any resources. It only validates prerequisites.
    /// Actual crafting and resource consumption is handled by the crafting service (v0.11.2).
    /// </para>
    /// </remarks>
    public CraftValidation CanCraft(Player player, string recipeId, string? stationId)
    {
        // Validate player parameter
        ArgumentNullException.ThrowIfNull(player);

        // Validate recipe ID parameter
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeId);

        // Normalize recipe ID
        var normalizedId = recipeId.ToLowerInvariant();

        _logger.LogDebug(
            "Validating craft attempt: Player {PlayerId}, Recipe '{RecipeId}', Station '{StationId}'",
            player.Id,
            normalizedId,
            stationId ?? "(none)");

        // Ensure the player has a recipe book
        player.EnsureRecipeBook();

        // ─────────────────────────────────────────────────────────────
        // Step 1: Check if recipe exists
        // ─────────────────────────────────────────────────────────────
        var recipe = _recipeProvider.GetRecipe(normalizedId);
        if (recipe is null)
        {
            _logger.LogDebug(
                "Validation failed: Recipe '{RecipeId}' not found",
                normalizedId);

            return CraftValidation.Failed("Recipe not found.");
        }

        // ─────────────────────────────────────────────────────────────
        // Step 2: Check if player knows recipe
        // ─────────────────────────────────────────────────────────────
        if (!player.RecipeBook.IsKnown(normalizedId))
        {
            _logger.LogDebug(
                "Validation failed: Player {PlayerId} doesn't know recipe '{RecipeId}'",
                player.Id,
                normalizedId);

            return CraftValidation.Failed("You don't know this recipe.", recipe);
        }

        // ─────────────────────────────────────────────────────────────
        // Step 3: Check station requirement (if stationId provided)
        // ─────────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(stationId))
        {
            var normalizedStationId = stationId.ToLowerInvariant();

            if (!recipe.RequiredStationId.Equals(normalizedStationId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug(
                    "Validation failed: Recipe '{RecipeId}' requires station '{RequiredStation}', " +
                    "but player is at '{CurrentStation}'",
                    normalizedId,
                    recipe.RequiredStationId,
                    normalizedStationId);

                // Provide friendly error message with the required station
                var stationName = FormatStationName(recipe.RequiredStationId);
                return CraftValidation.Failed($"Requires {stationName} to craft.", recipe);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Step 4: Check ingredient availability
        // ─────────────────────────────────────────────────────────────
        var missingIngredients = ValidateIngredients(player, recipe);

        if (missingIngredients.Count > 0)
        {
            _logger.LogDebug(
                "Validation failed: Player {PlayerId} missing {MissingCount} ingredients for recipe '{RecipeId}'",
                player.Id,
                missingIngredients.Count,
                normalizedId);

            // Log details of missing ingredients
            foreach (var missing in missingIngredients)
            {
                _logger.LogDebug(
                    "  Missing: {ResourceName} ({ResourceId}) x{Quantity}",
                    missing.ResourceName,
                    missing.ResourceId,
                    missing.QuantityNeeded);
            }

            return CraftValidation.InsufficientResources(recipe, missingIngredients);
        }

        // ─────────────────────────────────────────────────────────────
        // All checks passed
        // ─────────────────────────────────────────────────────────────
        _logger.LogDebug(
            "Validation passed: Player {PlayerId} can craft '{RecipeId}' ({RecipeName})",
            player.Id,
            normalizedId,
            recipe.Name);

        return CraftValidation.Success(recipe);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that the player has all required ingredients for a recipe.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="recipe">The recipe to validate against.</param>
    /// <returns>A list of missing ingredients (empty if all ingredients are available).</returns>
    /// <remarks>
    /// <para>
    /// Checks the player's resource pools for each ingredient in the recipe.
    /// If the player doesn't have a resource pool or has insufficient quantity,
    /// the ingredient is added to the missing list.
    /// </para>
    /// <para>
    /// Resource names are looked up via the resource provider for user-friendly display.
    /// </para>
    /// </remarks>
    private IReadOnlyList<MissingIngredient> ValidateIngredients(Player player, RecipeDefinition recipe)
    {
        var missing = new List<MissingIngredient>();

        foreach (var ingredient in recipe.Ingredients)
        {
            // Get the player's resource pool for this ingredient
            var resourcePool = player.GetResource(ingredient.ResourceId);

            // Determine how much the player has
            var playerHas = resourcePool?.Current ?? 0;

            // Check if player has enough
            if (playerHas < ingredient.Quantity)
            {
                // Calculate how many more are needed
                var quantityNeeded = ingredient.Quantity - playerHas;

                // Look up the resource name for display
                var resourceDef = _resourceProvider.GetResource(ingredient.ResourceId);
                var resourceName = resourceDef?.Name ?? ingredient.ResourceId;

                missing.Add(new MissingIngredient(
                    ResourceId: ingredient.ResourceId,
                    ResourceName: resourceName,
                    QuantityNeeded: quantityNeeded));
            }
        }

        return missing.AsReadOnly();
    }

    /// <summary>
    /// Formats a station ID into a user-friendly display name.
    /// </summary>
    /// <param name="stationId">The station ID (e.g., "anvil", "alchemy-table").</param>
    /// <returns>A formatted name (e.g., "an Anvil", "an Alchemy Table").</returns>
    /// <remarks>
    /// Converts kebab-case IDs to title case and adds appropriate article.
    /// </remarks>
    private static string FormatStationName(string stationId)
    {
        // Convert kebab-case to title case with spaces
        var words = stationId.Split('-')
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..])
            .ToArray();

        var name = string.Join(" ", words);

        // Add article based on first letter
        var article = "aeiouAEIOU".Contains(name[0]) ? "an" : "a";
        return $"{article} {name}";
    }

    /// <summary>
    /// Publishes a RecipeLearnedEvent to the game event logger.
    /// </summary>
    /// <param name="player">The player who learned the recipe.</param>
    /// <param name="recipe">The recipe that was learned.</param>
    /// <remarks>
    /// <para>
    /// Uses LogCharacter to record the event since recipe learning is a
    /// character progression event.
    /// </para>
    /// <para>
    /// The event data includes:
    /// <list type="bullet">
    ///   <item><description>playerId - The player's unique identifier</description></item>
    ///   <item><description>recipeId - The recipe's string identifier</description></item>
    ///   <item><description>recipeName - The recipe's display name</description></item>
    ///   <item><description>category - The recipe category</description></item>
    ///   <item><description>timestamp - When the recipe was learned</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private void PublishRecipeLearnedEvent(Player player, RecipeDefinition recipe)
    {
        // Create the event record for potential event bus integration
        var recipeEvent = new RecipeLearnedEvent(
            PlayerId: player.Id,
            RecipeId: recipe.RecipeId,
            RecipeName: recipe.Name);

        _logger.LogDebug(
            "Publishing RecipeLearnedEvent: Player {PlayerId}, Recipe {RecipeId} ({RecipeName})",
            player.Id,
            recipe.RecipeId,
            recipe.Name);

        // Log to game event logger using LogCharacter (recipe learning is character progression)
        _eventLogger.LogCharacter(
            "RecipeLearned",
            $"{player.Name} learned the recipe: {recipe.Name}",
            player.Id,
            new Dictionary<string, object>
            {
                ["playerId"] = player.Id,
                ["recipeId"] = recipe.RecipeId,
                ["recipeName"] = recipe.Name,
                ["category"] = recipe.Category.ToString(),
                ["timestamp"] = recipeEvent.Timestamp
            });
    }
}
