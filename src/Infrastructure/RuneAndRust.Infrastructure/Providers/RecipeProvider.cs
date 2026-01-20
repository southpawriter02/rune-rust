using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for recipe definitions with lazy loading and indexed lookups.
/// </summary>
/// <remarks>
/// <para>
/// RecipeProvider loads recipe definitions from a JSON configuration file
/// and provides thread-safe access to the loaded definitions with multiple
/// indexes for efficient lookups by category, station, and output item.
/// </para>
/// <para>
/// Recipes are loaded lazily on first access, allowing the application to
/// start quickly while deferring the loading cost until recipes are needed.
/// </para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "recipes": [
///     {
///       "id": "iron-sword",
///       "name": "Iron Sword",
///       "description": "Forge a basic but reliable iron sword",
///       "category": "Weapon",
///       "requiredStation": "anvil",
///       "ingredients": [
///         { "resourceId": "iron-ore", "quantity": 5 },
///         { "resourceId": "leather", "quantity": 2 }
///       ],
///       "output": {
///         "itemId": "iron-sword",
///         "quantity": 1
///       },
///       "difficultyClass": 12,
///       "isDefault": true,
///       "craftingTimeSeconds": 30,
///       "icon": "icons/recipes/iron_sword.png"
///     }
///   ]
/// }
/// </code>
/// <para>
/// Multiple indexes are built for efficient lookups:
/// <list type="bullet">
///   <item><description>Primary index by recipe ID (case-insensitive)</description></item>
///   <item><description>Secondary index by category</description></item>
///   <item><description>Secondary index by required station</description></item>
///   <item><description>Secondary index by output item</description></item>
///   <item><description>Separate list for default recipes</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class RecipeProvider : IRecipeProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: recipe ID -> RecipeDefinition.
    /// </summary>
    private readonly Dictionary<string, RecipeDefinition> _recipes =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Secondary index: category -> list of recipes.
    /// </summary>
    private readonly Dictionary<RecipeCategory, List<RecipeDefinition>> _byCategory = [];

    /// <summary>
    /// Secondary index: station ID -> list of recipes.
    /// </summary>
    private readonly Dictionary<string, List<RecipeDefinition>> _byStation =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Secondary index: output item ID -> list of recipes.
    /// </summary>
    private readonly Dictionary<string, List<RecipeDefinition>> _byOutputItem =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// List of default (starter) recipes.
    /// </summary>
    private readonly List<RecipeDefinition> _defaultRecipes = [];

    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private readonly string _configPath;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<RecipeProvider> _logger;

    /// <summary>
    /// Flag indicating whether recipes have been loaded.
    /// </summary>
    private bool _isLoaded;

    /// <summary>
    /// Lock object for thread-safe lazy loading.
    /// </summary>
    private readonly object _loadLock = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new RecipeProvider instance with lazy loading.
    /// </summary>
    /// <param name="configPath">Path to the recipes.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when configPath or logger is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Recipes are not loaded until first access. This allows the application
    /// to start quickly while deferring the loading cost.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var provider = new RecipeProvider(
    ///     "config/recipes.json",
    ///     loggerFactory.CreateLogger&lt;RecipeProvider&gt;());
    ///
    /// // Recipes loaded on first access
    /// var ironSword = provider.GetRecipe("iron-sword");
    /// </code>
    /// </example>
    public RecipeProvider(string configPath, ILogger<RecipeProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "Recipe provider created with configuration path: {ConfigPath}",
            configPath);
    }

    // ═══════════════════════════════════════════════════════════════
    // IRecipeProvider IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public RecipeDefinition? GetRecipe(string recipeId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(recipeId))
        {
            _logger.LogDebug("GetRecipe called with null/empty recipeId - returning null");
            return null;
        }

        var found = _recipes.TryGetValue(recipeId, out var definition);

        _logger.LogDebug(
            "GetRecipe: {RecipeId} -> {Result}",
            recipeId,
            found ? definition!.Name : "not found");

        return definition;
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeDefinition> GetAllRecipes()
    {
        EnsureLoaded();

        var result = _recipes.Values.ToList().AsReadOnly();
        _logger.LogDebug("GetAllRecipes: returning {Count} recipes", result.Count);
        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeDefinition> GetRecipesByCategory(RecipeCategory category)
    {
        EnsureLoaded();

        if (_byCategory.TryGetValue(category, out var recipes))
        {
            _logger.LogDebug(
                "GetRecipesByCategory({Category}): returning {Count} recipes",
                category,
                recipes.Count);
            return recipes.AsReadOnly();
        }

        _logger.LogDebug(
            "GetRecipesByCategory({Category}): no recipes found",
            category);
        return Array.Empty<RecipeDefinition>();
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeDefinition> GetRecipesForStation(string stationId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(stationId))
        {
            _logger.LogDebug("GetRecipesForStation called with null/empty stationId");
            return Array.Empty<RecipeDefinition>();
        }

        if (_byStation.TryGetValue(stationId, out var recipes))
        {
            _logger.LogDebug(
                "GetRecipesForStation({Station}): returning {Count} recipes",
                stationId,
                recipes.Count);
            return recipes.AsReadOnly();
        }

        _logger.LogDebug(
            "GetRecipesForStation({Station}): no recipes found",
            stationId);
        return Array.Empty<RecipeDefinition>();
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeDefinition> GetDefaultRecipes()
    {
        EnsureLoaded();

        _logger.LogDebug(
            "GetDefaultRecipes: returning {Count} default recipes",
            _defaultRecipes.Count);
        return _defaultRecipes.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeDefinition> GetRecipesForItem(string itemId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(itemId))
        {
            _logger.LogDebug("GetRecipesForItem called with null/empty itemId");
            return Array.Empty<RecipeDefinition>();
        }

        if (_byOutputItem.TryGetValue(itemId, out var recipes))
        {
            _logger.LogDebug(
                "GetRecipesForItem({Item}): returning {Count} recipes",
                itemId,
                recipes.Count);
            return recipes.AsReadOnly();
        }

        _logger.LogDebug(
            "GetRecipesForItem({Item}): no recipes found",
            itemId);
        return Array.Empty<RecipeDefinition>();
    }

    /// <inheritdoc />
    public bool Exists(string recipeId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return false;
        }

        var exists = _recipes.ContainsKey(recipeId);
        _logger.LogDebug("Exists({RecipeId}): {Result}", recipeId, exists);
        return exists;
    }

    /// <inheritdoc />
    public int GetRecipeCount()
    {
        EnsureLoaded();
        return _recipes.Count;
    }

    // ═══════════════════════════════════════════════════════════════
    // LOADING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Ensures recipes are loaded from configuration.
    /// </summary>
    /// <remarks>
    /// Thread-safe lazy loading using double-checked locking pattern.
    /// </remarks>
    private void EnsureLoaded()
    {
        if (_isLoaded)
        {
            return;
        }

        lock (_loadLock)
        {
            if (_isLoaded)
            {
                return;
            }

            LoadRecipes();
            _isLoaded = true;
        }
    }

    /// <summary>
    /// Loads recipe definitions from the JSON configuration file.
    /// </summary>
    private void LoadRecipes()
    {
        _logger.LogInformation("Loading recipes from {Path}", _configPath);

        // Check file exists
        if (!File.Exists(_configPath))
        {
            _logger.LogWarning("Recipe configuration file not found: {Path}", _configPath);
            return;
        }

        try
        {
            // Read and parse JSON
            var json = File.ReadAllText(_configPath);
            _logger.LogDebug("Read {ByteCount} bytes from configuration file", json.Length);

            var config = JsonSerializer.Deserialize<RecipeConfiguration>(json, GetJsonOptions());

            // Validate configuration
            if (config?.Recipes is null || config.Recipes.Count == 0)
            {
                _logger.LogWarning("No recipes found in configuration");
                return;
            }

            _logger.LogDebug(
                "Parsing {Count} recipe entries from configuration",
                config.Recipes.Count);

            // Parse each recipe entry
            var loadedCount = 0;
            var skippedCount = 0;

            foreach (var recipeData in config.Recipes)
            {
                try
                {
                    var recipe = CreateRecipeFromData(recipeData);

                    if (recipe is not null && ValidateRecipe(recipe))
                    {
                        AddRecipeToIndexes(recipe);
                        loadedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load recipe: {RecipeId}", recipeData.Id);
                    skippedCount++;
                }
            }

            _logger.LogInformation(
                "Loaded {Loaded} recipes, skipped {Skipped}",
                loadedCount,
                skippedCount);

            // Log summary by category and station
            LogLoadingSummary();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse recipe configuration: {Path}", _configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading recipes from: {Path}", _configPath);
        }
    }

    /// <summary>
    /// Creates a RecipeDefinition from configuration data.
    /// </summary>
    /// <param name="data">The recipe data from JSON.</param>
    /// <returns>The created recipe definition, or null if creation failed.</returns>
    private RecipeDefinition? CreateRecipeFromData(RecipeData data)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(data.Id))
        {
            _logger.LogWarning("Recipe entry missing ID - skipping");
            return null;
        }

        if (string.IsNullOrWhiteSpace(data.Name))
        {
            _logger.LogWarning("Recipe missing name: {Id}", data.Id);
            return null;
        }

        if (string.IsNullOrWhiteSpace(data.RequiredStation))
        {
            _logger.LogWarning("Recipe missing required station: {Id}", data.Id);
            return null;
        }

        // Validate ingredients
        if (data.Ingredients is null || data.Ingredients.Count == 0)
        {
            _logger.LogWarning("Recipe has no ingredients: {Id}", data.Id);
            return null;
        }

        // Validate output
        if (data.Output is null)
        {
            _logger.LogWarning("Recipe has no output: {Id}", data.Id);
            return null;
        }

        // Parse category
        if (!Enum.TryParse<RecipeCategory>(data.Category, ignoreCase: true, out var category))
        {
            _logger.LogWarning(
                "Invalid recipe category '{Category}' for: {Id}",
                data.Category,
                data.Id);
            return null;
        }

        // Create ingredients list
        var ingredients = new List<RecipeIngredient>();
        foreach (var ingredientData in data.Ingredients)
        {
            if (string.IsNullOrWhiteSpace(ingredientData.ResourceId))
            {
                _logger.LogWarning(
                    "Recipe {Id} has ingredient with missing resourceId - skipping ingredient",
                    data.Id);
                continue;
            }

            var quantity = ingredientData.Quantity > 0 ? ingredientData.Quantity : 1;
            if (ingredientData.Quantity <= 0)
            {
                _logger.LogWarning(
                    "Recipe {Id} has ingredient '{ResourceId}' with invalid quantity {Quantity} - defaulting to 1",
                    data.Id,
                    ingredientData.ResourceId,
                    ingredientData.Quantity);
            }

            ingredients.Add(new RecipeIngredient(ingredientData.ResourceId, quantity));
        }

        if (ingredients.Count == 0)
        {
            _logger.LogWarning("Recipe {Id} has no valid ingredients after parsing", data.Id);
            return null;
        }

        // Create output
        if (string.IsNullOrWhiteSpace(data.Output.ItemId))
        {
            _logger.LogWarning("Recipe {Id} has output with missing itemId", data.Id);
            return null;
        }

        var outputQuantity = data.Output.Quantity > 0 ? data.Output.Quantity : 1;
        var output = new RecipeOutput(
            data.Output.ItemId,
            outputQuantity,
            data.Output.QualityFormula);

        // Validate DC
        var dc = data.DifficultyClass;
        if (dc < 1)
        {
            _logger.LogWarning(
                "Recipe {Id} has invalid DC {DC} - defaulting to 10",
                data.Id,
                dc);
            dc = 10;
        }
        else if (dc > 30)
        {
            _logger.LogWarning(
                "Recipe {Id} has DC {DC} exceeding maximum 30 - capping at 30",
                data.Id,
                dc);
            dc = 30;
        }

        // Validate crafting time
        var craftingTime = data.CraftingTimeSeconds >= 0 ? data.CraftingTimeSeconds : 30;
        if (data.CraftingTimeSeconds < 0)
        {
            _logger.LogWarning(
                "Recipe {Id} has negative crafting time {Time} - defaulting to 30",
                data.Id,
                data.CraftingTimeSeconds);
        }

        // Create the recipe definition
        return RecipeDefinition.Create(
            recipeId: data.Id,
            name: data.Name,
            description: data.Description ?? string.Empty,
            category: category,
            requiredStationId: data.RequiredStation,
            ingredients: ingredients,
            output: output,
            difficultyClass: dc,
            isDefault: data.IsDefault,
            craftingTimeSeconds: craftingTime,
            iconPath: data.Icon);
    }

    /// <summary>
    /// Validates a recipe definition.
    /// </summary>
    /// <param name="recipe">The recipe to validate.</param>
    /// <returns>True if the recipe is valid, false otherwise.</returns>
    private bool ValidateRecipe(RecipeDefinition recipe)
    {
        // Check for duplicates
        if (_recipes.ContainsKey(recipe.RecipeId))
        {
            _logger.LogWarning("Duplicate recipe ID: {RecipeId}", recipe.RecipeId);
            return false;
        }

        // Validate DC range (additional check)
        if (recipe.DifficultyClass < 1 || recipe.DifficultyClass > 30)
        {
            _logger.LogWarning(
                "Recipe {RecipeId} has invalid DC: {DC}",
                recipe.RecipeId,
                recipe.DifficultyClass);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Adds a recipe to all lookup indexes.
    /// </summary>
    /// <param name="recipe">The recipe to index.</param>
    private void AddRecipeToIndexes(RecipeDefinition recipe)
    {
        // Primary index
        _recipes[recipe.RecipeId] = recipe;

        // Index by category
        if (!_byCategory.TryGetValue(recipe.Category, out var categoryList))
        {
            categoryList = [];
            _byCategory[recipe.Category] = categoryList;
        }
        categoryList.Add(recipe);

        // Index by station
        if (!_byStation.TryGetValue(recipe.RequiredStationId, out var stationList))
        {
            stationList = [];
            _byStation[recipe.RequiredStationId] = stationList;
        }
        stationList.Add(recipe);

        // Index by output item
        if (!_byOutputItem.TryGetValue(recipe.Output.ItemId, out var itemList))
        {
            itemList = [];
            _byOutputItem[recipe.Output.ItemId] = itemList;
        }
        itemList.Add(recipe);

        // Track default recipes
        if (recipe.IsDefault)
        {
            _defaultRecipes.Add(recipe);
        }

        _logger.LogDebug(
            "Loaded recipe: {RecipeId} ({Category}, Station: {Station}, DC: {DC}, Default: {IsDefault})",
            recipe.RecipeId,
            recipe.Category,
            recipe.RequiredStationId,
            recipe.DifficultyClass,
            recipe.IsDefault);
    }

    /// <summary>
    /// Logs a summary of loaded recipes by category and station.
    /// </summary>
    private void LogLoadingSummary()
    {
        // Log by category
        foreach (var (category, recipes) in _byCategory.OrderBy(kvp => kvp.Key))
        {
            _logger.LogDebug(
                "Category {Category}: {Count} recipes",
                category,
                recipes.Count);
        }

        // Log by station
        foreach (var (station, recipes) in _byStation.OrderBy(kvp => kvp.Key))
        {
            _logger.LogDebug(
                "Station {Station}: {Count} recipes",
                station,
                recipes.Count);
        }

        // Log default recipes count
        _logger.LogInformation(
            "Recipe provider initialized: {Total} total, {Default} default recipes",
            _recipes.Count,
            _defaultRecipes.Count);
    }

    /// <summary>
    /// Gets JSON serializer options.
    /// </summary>
    /// <returns>Configured JsonSerializerOptions.</returns>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTOs
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root DTO for deserializing recipes.json.
    /// </summary>
    private sealed class RecipeConfiguration
    {
        public List<RecipeData> Recipes { get; set; } = [];
    }

    /// <summary>
    /// DTO for a single recipe entry.
    /// </summary>
    private sealed class RecipeData
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Category { get; set; } = null!;
        public string RequiredStation { get; set; } = null!;
        public List<IngredientData> Ingredients { get; set; } = [];
        public OutputData Output { get; set; } = null!;
        public int DifficultyClass { get; set; } = 10;
        public bool IsDefault { get; set; }
        public int CraftingTimeSeconds { get; set; } = 30;
        public string? Icon { get; set; }
    }

    /// <summary>
    /// DTO for a recipe ingredient.
    /// </summary>
    private sealed class IngredientData
    {
        public string ResourceId { get; set; } = null!;
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// DTO for recipe output.
    /// </summary>
    private sealed class OutputData
    {
        public string ItemId { get; set; } = null!;
        public int Quantity { get; set; } = 1;
        public string? QualityFormula { get; set; }
    }
}
