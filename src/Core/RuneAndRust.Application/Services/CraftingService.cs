using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for crafting items at crafting stations.
/// </summary>
/// <remarks>
/// <para>
/// The CraftingService orchestrates the complete crafting process including validation,
/// dice checks, resource management, item creation, and event publishing. It implements
/// the d20 + skill modifier vs DC mechanic with graduated quality outcomes.
/// </para>
/// <para>
/// Core responsibilities:
/// <list type="bullet">
///   <item><description>Station and recipe validation</description></item>
///   <item><description>Resource availability checking and consumption</description></item>
///   <item><description>Dice-based crafting checks</description></item>
///   <item><description>Quality determination based on roll margin</description></item>
///   <item><description>Graduated failure consequences (resource loss)</description></item>
///   <item><description>Domain event publishing</description></item>
/// </list>
/// </para>
/// <para>
/// Dependencies:
/// <list type="bullet">
///   <item><description><see cref="IRecipeProvider"/> - Recipe definitions</description></item>
///   <item><description><see cref="IRecipeService"/> - Recipe knowledge validation</description></item>
///   <item><description><see cref="ICraftingStationProvider"/> - Station definitions and skill lookups</description></item>
///   <item><description><see cref="IResourceProvider"/> - Resource name lookups</description></item>
///   <item><description><see cref="IDiceService"/> - Dice rolling</description></item>
///   <item><description><see cref="IGameEventLogger"/> - Event publishing</description></item>
///   <item><description><see cref="ILogger{TCategoryName}"/> - Diagnostic logging</description></item>
/// </list>
/// </para>
/// <para>
/// Quality determination (based on roll margin = Total - DC):
/// <list type="bullet">
///   <item><description>Natural 20 → <see cref="CraftedItemQuality.Legendary"/> (always)</description></item>
///   <item><description>Margin >= 10 → <see cref="CraftedItemQuality.Masterwork"/></description></item>
///   <item><description>Margin >= 5 → <see cref="CraftedItemQuality.Fine"/></description></item>
///   <item><description>Otherwise → <see cref="CraftedItemQuality.Standard"/></description></item>
/// </list>
/// </para>
/// <para>
/// Failure resource loss (margin = Total - DC when negative):
/// <list type="bullet">
///   <item><description>Close failure (margin >= -5): 25% of each ingredient lost</description></item>
///   <item><description>Bad failure (margin &lt; -5): 50% of each ingredient lost</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var craftingService = new CraftingService(
///     recipeProvider,
///     recipeService,
///     stationProvider,
///     resourceProvider,
///     diceService,
///     eventLogger,
///     logger);
///
/// // Validate crafting prerequisites
/// var validation = craftingService.CanCraft(player, "iron-sword");
/// if (validation.IsValid)
/// {
///     // Attempt to craft
///     var result = craftingService.Craft(player, "iron-sword");
///     if (result.IsSuccess)
///     {
///         Console.WriteLine($"Crafted {result.CraftedItem!.Name} ({result.Quality})");
///         Console.WriteLine(result.GetRollDisplay()); // d20(15) +4 = 19 vs DC 12
///     }
///     else if (result.WasDiceRollFailure)
///     {
///         Console.WriteLine($"Failed! Lost {result.TotalResourcesLost} resources.");
///     }
/// }
/// </code>
/// </example>
public class CraftingService : ICraftingService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Percentage of ingredients lost on a close failure (margin >= -5).
    /// </summary>
    /// <remarks>
    /// Close failures represent near-misses where the crafter almost succeeded.
    /// 25% loss is a relatively lenient penalty that encourages risk-taking.
    /// </remarks>
    private const float CloseFailureLossPercent = 0.25f;

    /// <summary>
    /// Percentage of ingredients lost on a bad failure (margin &lt; -5).
    /// </summary>
    /// <remarks>
    /// Bad failures represent significant mishaps where the crafter was clearly
    /// unprepared. 50% loss is a substantial penalty that encourages skill improvement.
    /// </remarks>
    private const float BadFailureLossPercent = 0.50f;

    /// <summary>
    /// Threshold for determining close vs bad failure.
    /// </summary>
    /// <remarks>
    /// Margin >= -5 is a close failure (25% loss).
    /// Margin &lt; -5 is a bad failure (50% loss).
    /// </remarks>
    private const int BadFailureThreshold = -5;

    /// <summary>
    /// Margin required for Masterwork quality.
    /// </summary>
    /// <remarks>
    /// Rolling 10 or more above the DC indicates exceptional craftsmanship.
    /// </remarks>
    private const int MasterworkMarginThreshold = 10;

    /// <summary>
    /// Margin required for Fine quality.
    /// </summary>
    /// <remarks>
    /// Rolling 5-9 above the DC indicates above-average craftsmanship.
    /// </remarks>
    private const int FineMarginThreshold = 5;

    /// <summary>
    /// Default bonus per skill proficiency level for crafting skills.
    /// </summary>
    /// <remarks>
    /// Used when the skill definition is not available. Each proficiency level
    /// adds this bonus to the crafting roll.
    /// </remarks>
    private const int DefaultSkillBonusPerLevel = 2;

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for accessing recipe definitions.
    /// </summary>
    private readonly IRecipeProvider _recipeProvider;

    /// <summary>
    /// Service for validating recipe knowledge.
    /// </summary>
    private readonly IRecipeService _recipeService;

    /// <summary>
    /// Provider for crafting station definitions.
    /// </summary>
    private readonly ICraftingStationProvider _stationProvider;

    /// <summary>
    /// Provider for resource definitions (name lookups).
    /// </summary>
    private readonly IResourceProvider _resourceProvider;

    /// <summary>
    /// Service for dice rolling operations.
    /// </summary>
    private readonly IDiceService _diceService;

    /// <summary>
    /// Logger for publishing game events.
    /// </summary>
    private readonly IGameEventLogger _eventLogger;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<CraftingService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new CraftingService instance.
    /// </summary>
    /// <param name="recipeProvider">Provider for recipe definitions.</param>
    /// <param name="recipeService">Service for recipe knowledge validation.</param>
    /// <param name="stationProvider">Provider for crafting station definitions.</param>
    /// <param name="resourceProvider">Provider for resource definitions.</param>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <example>
    /// <code>
    /// var craftingService = new CraftingService(
    ///     recipeProvider,
    ///     recipeService,
    ///     stationProvider,
    ///     resourceProvider,
    ///     diceService,
    ///     eventLogger,
    ///     logger);
    /// </code>
    /// </example>
    public CraftingService(
        IRecipeProvider recipeProvider,
        IRecipeService recipeService,
        ICraftingStationProvider stationProvider,
        IResourceProvider resourceProvider,
        IDiceService diceService,
        IGameEventLogger eventLogger,
        ILogger<CraftingService> logger)
    {
        // Validate all dependencies are provided
        _recipeProvider = recipeProvider ?? throw new ArgumentNullException(nameof(recipeProvider));
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _stationProvider = stationProvider ?? throw new ArgumentNullException(nameof(stationProvider));
        _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Log service initialization with configuration info
        _logger.LogInformation(
            "CraftingService initialized. CloseFailureLoss={ClosePercent:P0}, BadFailureLoss={BadPercent:P0}, " +
            "BadFailureThreshold={Threshold}, MasterworkMargin={Masterwork}, FineMargin={Fine}",
            CloseFailureLossPercent,
            BadFailureLossPercent,
            BadFailureThreshold,
            MasterworkMarginThreshold,
            FineMarginThreshold);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Validation checks performed in order:
    /// <list type="number">
    ///   <item><description>Room has an available crafting station</description></item>
    ///   <item><description>Recipe exists in the provider</description></item>
    ///   <item><description>Player knows the recipe</description></item>
    ///   <item><description>Station supports the recipe's category</description></item>
    ///   <item><description>Player has all required resources</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// On success, the returned validation includes both the recipe and station
    /// definitions for use by the caller.
    /// </para>
    /// </remarks>
    public CraftValidation CanCraft(Player player, string recipeId, Room room)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(room);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeId);

        // Normalize recipe ID for case-insensitive lookup
        var normalizedRecipeId = recipeId.ToLowerInvariant();

        _logger.LogDebug(
            "Validating craft attempt: Player={PlayerId} ({PlayerName}), Recipe='{RecipeId}', Room='{RoomName}'",
            player.Id,
            player.Name,
            normalizedRecipeId,
            room.Name);

        // ─────────────────────────────────────────────────────────────
        // Step 1: Check room has a crafting station
        // ─────────────────────────────────────────────────────────────
        var station = GetCurrentStation(room);
        if (station is null)
        {
            _logger.LogDebug(
                "Validation failed: Player {PlayerId} is not at a crafting station",
                player.Id);

            return CraftValidation.Failed("You need to be at a crafting station to craft.");
        }

        _logger.LogDebug(
            "Player {PlayerId} is at station '{StationId}' ({StationName})",
            player.Id,
            station.DefinitionId,
            station.Name);

        // Get station definition for category check
        var stationDefinition = _stationProvider.GetStation(station.DefinitionId);
        if (stationDefinition is null)
        {
            _logger.LogWarning(
                "Station definition not found for station instance '{StationId}'. This indicates data inconsistency",
                station.DefinitionId);

            return CraftValidation.Failed("Crafting station configuration error.");
        }

        // ─────────────────────────────────────────────────────────────
        // Step 2: Check recipe exists
        // ─────────────────────────────────────────────────────────────
        var recipe = _recipeProvider.GetRecipe(normalizedRecipeId);
        if (recipe is null)
        {
            _logger.LogDebug(
                "Validation failed: Recipe '{RecipeId}' not found",
                normalizedRecipeId);

            return CraftValidation.Failed("Recipe not found.");
        }

        _logger.LogDebug(
            "Found recipe '{RecipeId}': {RecipeName}, Category={Category}, DC={DC}",
            normalizedRecipeId,
            recipe.Name,
            recipe.Category,
            recipe.DifficultyClass);

        // ─────────────────────────────────────────────────────────────
        // Step 3: Check player knows the recipe
        // ─────────────────────────────────────────────────────────────
        if (!_recipeService.IsRecipeKnown(player, normalizedRecipeId))
        {
            _logger.LogDebug(
                "Validation failed: Player {PlayerId} doesn't know recipe '{RecipeId}'",
                player.Id,
                normalizedRecipeId);

            return CraftValidation.Failed("You don't know this recipe.", recipe);
        }

        _logger.LogDebug(
            "Player {PlayerId} knows recipe '{RecipeId}'",
            player.Id,
            normalizedRecipeId);

        // ─────────────────────────────────────────────────────────────
        // Step 4: Check station supports recipe category
        // ─────────────────────────────────────────────────────────────
        if (!stationDefinition.SupportsCategory(recipe.Category))
        {
            _logger.LogDebug(
                "Validation failed: Station '{StationId}' does not support category {Category}. " +
                "Supported categories: {SupportedCategories}",
                stationDefinition.StationId,
                recipe.Category,
                stationDefinition.GetCategoriesDisplay());

            var stationName = FormatStationNameForError(recipe.RequiredStationId);
            return CraftValidation.Failed($"This recipe requires {stationName}.", recipe);
        }

        _logger.LogDebug(
            "Station '{StationId}' supports recipe category {Category}",
            stationDefinition.StationId,
            recipe.Category);

        // ─────────────────────────────────────────────────────────────
        // Step 5: Check resource availability
        // ─────────────────────────────────────────────────────────────
        var missingIngredients = GetMissingIngredients(player, recipe);
        if (missingIngredients.Count > 0)
        {
            _logger.LogDebug(
                "Validation failed: Player {PlayerId} missing {MissingCount} ingredients for recipe '{RecipeId}'",
                player.Id,
                missingIngredients.Count,
                normalizedRecipeId);

            // Log details of each missing ingredient
            foreach (var missing in missingIngredients)
            {
                _logger.LogDebug(
                    "  Missing: {ResourceName} ({ResourceId}) x{Quantity}",
                    missing.ResourceName,
                    missing.ResourceId,
                    missing.QuantityNeeded);
            }

            return CraftValidation.InsufficientResources(recipe, missingIngredients, stationDefinition);
        }

        _logger.LogDebug(
            "Player {PlayerId} has all required ingredients for recipe '{RecipeId}'",
            player.Id,
            normalizedRecipeId);

        // ─────────────────────────────────────────────────────────────
        // All checks passed
        // ─────────────────────────────────────────────────────────────
        _logger.LogDebug(
            "Validation passed: Player {PlayerId} can craft '{RecipeId}' ({RecipeName}) at station '{StationId}'",
            player.Id,
            normalizedRecipeId,
            recipe.Name,
            stationDefinition.StationId);

        return CraftValidation.Success(recipe, stationDefinition);
    }

    // ═══════════════════════════════════════════════════════════════
    // CRAFTING METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// The crafting process:
    /// <list type="number">
    ///   <item><description>Validate using <see cref="CanCraft"/> - return early if fails</description></item>
    ///   <item><description>Calculate crafting modifier from player's skill</description></item>
    ///   <item><description>Roll d20, add modifier, compare to DC</description></item>
    ///   <item><description>Publish <see cref="CraftAttemptedEvent"/></description></item>
    ///   <item><description>On success: consume resources, create item, publish <see cref="ItemCraftedEvent"/></description></item>
    ///   <item><description>On failure: calculate and apply resource loss</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public CraftResult Craft(Player player, string recipeId, Room room)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(room);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeId);

        // Normalize recipe ID
        var normalizedRecipeId = recipeId.ToLowerInvariant();

        _logger.LogInformation(
            "Crafting attempt started: Player={PlayerId} ({PlayerName}), Recipe='{RecipeId}', Room='{RoomName}'",
            player.Id,
            player.Name,
            normalizedRecipeId,
            room.Name);

        // ─────────────────────────────────────────────────────────────
        // Step 1: Validate prerequisites
        // ─────────────────────────────────────────────────────────────
        var validation = CanCraft(player, normalizedRecipeId, room);
        if (!validation.IsValid)
        {
            _logger.LogInformation(
                "Crafting validation failed: Player={PlayerId}, Recipe='{RecipeId}', Reason='{Reason}'",
                player.Id,
                normalizedRecipeId,
                validation.FailureReason);

            return CraftResult.ValidationFailed(validation.FailureReason ?? "Validation failed.");
        }

        // Extract validated recipe and station
        var recipe = validation.Recipe!;
        var stationDefinition = validation.Station!;
        var station = GetCurrentStation(room)!;

        _logger.LogDebug(
            "Validation passed. Recipe='{RecipeId}' ({RecipeName}), DC={DC}, Station='{StationId}'",
            recipe.RecipeId,
            recipe.Name,
            recipe.DifficultyClass,
            stationDefinition.StationId);

        // ─────────────────────────────────────────────────────────────
        // Step 2: Calculate crafting modifier
        // ─────────────────────────────────────────────────────────────
        var modifier = GetCraftingModifier(player, stationDefinition.StationId);

        _logger.LogDebug(
            "Crafting modifier for player {PlayerId} at station '{StationId}': {Modifier:+0;-0;0}",
            player.Id,
            stationDefinition.StationId,
            modifier);

        // ─────────────────────────────────────────────────────────────
        // Step 3: Mark station as in use
        // ─────────────────────────────────────────────────────────────
        station.SetInUse();

        _logger.LogDebug(
            "Station '{StationId}' marked as in use",
            stationDefinition.StationId);

        try
        {
            // ─────────────────────────────────────────────────────────
            // Step 4: Roll the dice
            // ─────────────────────────────────────────────────────────
            var rollResult = _diceService.Roll("1d20");
            var roll = rollResult.Total;
            var total = roll + modifier;
            var dc = recipe.DifficultyClass;
            var margin = total - dc;
            var isNatural20 = roll == 20;
            var isSuccess = total >= dc;

            _logger.LogInformation(
                "Crafting roll: d20({Roll}) {ModSign}{Modifier} = {Total} vs DC {DC} " +
                "[Margin={Margin}, Natural20={IsNat20}, Success={IsSuccess}]",
                roll,
                modifier >= 0 ? "+" : "",
                modifier,
                total,
                dc,
                margin,
                isNatural20,
                isSuccess);

            // ─────────────────────────────────────────────────────────
            // Step 5: Publish craft attempted event
            // ─────────────────────────────────────────────────────────
            var attemptedEvent = CraftAttemptedEvent.Create(
                playerId: player.Id,
                recipeId: normalizedRecipeId,
                roll: roll,
                modifier: modifier,
                difficultyClass: dc,
                stationId: stationDefinition.StationId);

            _eventLogger.Log(attemptedEvent);

            _logger.LogDebug(
                "Published CraftAttemptedEvent: {EventMessage}",
                attemptedEvent.Message);

            // ─────────────────────────────────────────────────────────
            // Step 6: Handle success or failure
            // ─────────────────────────────────────────────────────────
            if (isSuccess)
            {
                return HandleCraftingSuccess(
                    player: player,
                    recipe: recipe,
                    stationDefinition: stationDefinition,
                    roll: roll,
                    modifier: modifier,
                    total: total,
                    dc: dc,
                    margin: margin,
                    isNatural20: isNatural20);
            }
            else
            {
                return HandleCraftingFailure(
                    player: player,
                    recipe: recipe,
                    roll: roll,
                    modifier: modifier,
                    total: total,
                    dc: dc,
                    margin: margin);
            }
        }
        finally
        {
            // Always restore station availability
            station.SetAvailable();

            _logger.LogDebug(
                "Station '{StationId}' restored to available",
                stationDefinition.StationId);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// Returns recipes that the player knows and can be crafted at the room's station,
    /// regardless of resource availability.
    /// </remarks>
    public IReadOnlyList<RecipeDefinition> GetCraftableRecipesHere(Player player, Room room)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug(
            "Getting craftable recipes for player {PlayerId} at room '{RoomName}'",
            player.Id,
            room.Name);

        // Check if room has a station
        var station = GetCurrentStation(room);
        if (station is null)
        {
            _logger.LogDebug(
                "Room '{RoomName}' has no available crafting station - returning empty list",
                room.Name);

            return Array.Empty<RecipeDefinition>();
        }

        // Get station definition
        var stationDefinition = _stationProvider.GetStation(station.DefinitionId);
        if (stationDefinition is null)
        {
            _logger.LogWarning(
                "Station definition not found for '{StationId}' - returning empty list",
                station.DefinitionId);

            return Array.Empty<RecipeDefinition>();
        }

        // Get all recipes the player knows
        var knownRecipes = _recipeService.GetKnownRecipes(player);

        // Filter to recipes that can be crafted at this station
        var craftableHere = knownRecipes
            .Where(r => stationDefinition.SupportsCategory(r.Category))
            .ToList()
            .AsReadOnly();

        _logger.LogDebug(
            "Player {PlayerId} can craft {Count} recipes at station '{StationId}'",
            player.Id,
            craftableHere.Count,
            station.DefinitionId);

        return craftableHere;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns only recipes where the player has all required resources in their inventory.
    /// This is a stricter filter than <see cref="GetCraftableRecipesHere"/>.
    /// </remarks>
    public IReadOnlyList<RecipeDefinition> GetReadyToCraftRecipes(Player player, Room room)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug(
            "Getting ready-to-craft recipes for player {PlayerId} at room '{RoomName}'",
            player.Id,
            room.Name);

        // Get all recipes craftable at this station
        var craftableHere = GetCraftableRecipesHere(player, room);
        if (craftableHere.Count == 0)
        {
            return Array.Empty<RecipeDefinition>();
        }

        // Filter to recipes with available resources
        var readyToCraft = craftableHere
            .Where(r => GetMissingIngredients(player, r).Count == 0)
            .ToList()
            .AsReadOnly();

        _logger.LogDebug(
            "Player {PlayerId} has resources for {ReadyCount} of {TotalCount} craftable recipes",
            player.Id,
            readyToCraft.Count,
            craftableHere.Count);

        return readyToCraft;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Modifier calculation:
    /// <list type="number">
    ///   <item><description>Get the crafting skill ID for the station</description></item>
    ///   <item><description>Get the player's skill proficiency level</description></item>
    ///   <item><description>Calculate modifier: (int)proficiency * bonusPerLevel</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Returns 0 if the station is not found, has no crafting skill, or the player
    /// has no skill (Untrained proficiency).
    /// </para>
    /// </remarks>
    public int GetCraftingModifier(Player player, string stationId)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(stationId);

        // Normalize station ID
        var normalizedStationId = stationId.ToLowerInvariant();

        // Get the crafting skill for this station
        var skillId = _stationProvider.GetCraftingSkill(normalizedStationId);
        if (string.IsNullOrWhiteSpace(skillId))
        {
            _logger.LogDebug(
                "No crafting skill defined for station '{StationId}' - returning modifier 0",
                normalizedStationId);

            return 0;
        }

        // Get player's skill proficiency
        var proficiency = player.GetSkillProficiency(skillId);

        // Calculate modifier: proficiency level * bonus per level
        // Using default bonus since we don't have access to skill definition here
        var modifier = (int)proficiency * DefaultSkillBonusPerLevel;

        _logger.LogDebug(
            "Crafting modifier for player {PlayerId}, station '{StationId}', skill '{SkillId}': " +
            "proficiency={Proficiency}, modifier={Modifier}",
            player.Id,
            normalizedStationId,
            skillId,
            proficiency,
            modifier);

        return modifier;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Searches for the first available crafting station in the specified room.
    /// Returns null if the room has no available stations.
    /// </remarks>
    public CraftingStation? GetCurrentStation(Room room)
    {
        // Validate parameter
        ArgumentNullException.ThrowIfNull(room);

        // Get the first available crafting station in the room
        var station = room.GetFirstAvailableCraftingStation();

        if (station is not null)
        {
            _logger.LogDebug(
                "Found available station '{StationId}' ({StationName}) in room '{RoomName}'",
                station.DefinitionId,
                station.Name,
                room.Name);
        }
        else
        {
            _logger.LogDebug(
                "No available crafting station in room '{RoomName}'",
                room.Name);
        }

        return station;
    }

    // ═══════════════════════════════════════════════════════════════
    // CALCULATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Loss formula for each ingredient:
    /// <code>lossAmount = MAX(1, CEILING(requiredQuantity * lossPercent))</code>
    /// </para>
    /// <para>
    /// This ensures at least 1 of each ingredient is always lost on failure,
    /// providing meaningful consequences even for single-ingredient recipes.
    /// </para>
    /// </remarks>
    public IReadOnlyList<ResourceLoss> CalculateFailureLoss(RecipeDefinition recipe, int rollMargin)
    {
        // Validate parameter
        ArgumentNullException.ThrowIfNull(recipe);

        // Determine loss percentage based on failure severity
        var isCloseFailure = rollMargin >= BadFailureThreshold;
        var lossPercent = isCloseFailure ? CloseFailureLossPercent : BadFailureLossPercent;

        _logger.LogDebug(
            "Calculating failure loss for recipe '{RecipeId}': margin={Margin}, " +
            "failureType={FailureType}, lossPercent={LossPercent:P0}",
            recipe.RecipeId,
            rollMargin,
            isCloseFailure ? "Close" : "Bad",
            lossPercent);

        // Calculate loss for each ingredient
        var losses = new List<ResourceLoss>();

        foreach (var ingredient in recipe.Ingredients)
        {
            // Calculate loss amount: at least 1, otherwise ceiling of percentage
            var requiredQuantity = ingredient.Quantity;
            var calculatedLoss = (int)Math.Ceiling(requiredQuantity * lossPercent);
            var lossAmount = Math.Max(1, calculatedLoss);

            // Look up resource name for display
            var resourceDef = _resourceProvider.GetResource(ingredient.ResourceId);
            var resourceName = resourceDef?.Name ?? ingredient.ResourceId;

            var loss = new ResourceLoss(
                ResourceId: ingredient.ResourceId,
                Name: resourceName,
                Amount: lossAmount);

            losses.Add(loss);

            _logger.LogDebug(
                "  Loss calculated: {ResourceName} ({ResourceId}) - " +
                "required={Required}, lossPercent={LossPercent:P0}, " +
                "calculated={Calculated}, final={Final}",
                resourceName,
                ingredient.ResourceId,
                requiredQuantity,
                lossPercent,
                calculatedLoss,
                lossAmount);
        }

        _logger.LogDebug(
            "Total loss calculated: {LossCount} resource types, {TotalLost} total items",
            losses.Count,
            losses.Sum(l => l.Amount));

        return losses.AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles a successful crafting attempt.
    /// </summary>
    /// <param name="player">The player who crafted.</param>
    /// <param name="recipe">The recipe that was crafted.</param>
    /// <param name="stationDefinition">The station definition used.</param>
    /// <param name="roll">The d20 roll value.</param>
    /// <param name="modifier">The skill modifier.</param>
    /// <param name="total">The total roll (roll + modifier).</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="margin">The roll margin (total - dc).</param>
    /// <param name="isNatural20">Whether the roll was a natural 20.</param>
    /// <returns>A successful craft result.</returns>
    /// <remarks>
    /// <para>
    /// Success handling:
    /// <list type="number">
    ///   <item><description>Determine item quality based on roll</description></item>
    ///   <item><description>Consume all required resources</description></item>
    ///   <item><description>Create the item with appropriate quality</description></item>
    ///   <item><description>Add item to player's inventory</description></item>
    ///   <item><description>Publish ItemCraftedEvent</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private CraftResult HandleCraftingSuccess(
        Player player,
        RecipeDefinition recipe,
        CraftingStationDefinition stationDefinition,
        int roll,
        int modifier,
        int total,
        int dc,
        int margin,
        bool isNatural20)
    {
        _logger.LogDebug(
            "Handling crafting success for player {PlayerId}, recipe '{RecipeId}'",
            player.Id,
            recipe.RecipeId);

        // ─────────────────────────────────────────────────────────────
        // Step 1: Determine quality
        // ─────────────────────────────────────────────────────────────
        var quality = DetermineQuality(roll, margin, isNatural20);

        _logger.LogDebug(
            "Quality determined: {Quality} (margin={Margin}, isNatural20={IsNat20})",
            quality,
            margin,
            isNatural20);

        // ─────────────────────────────────────────────────────────────
        // Step 2: Consume resources
        // ─────────────────────────────────────────────────────────────
        ConsumeResources(player, recipe);

        _logger.LogDebug(
            "Resources consumed for recipe '{RecipeId}'",
            recipe.RecipeId);

        // ─────────────────────────────────────────────────────────────
        // Step 3: Create the item
        // ─────────────────────────────────────────────────────────────
        var item = CreateCraftedItem(recipe, quality);

        _logger.LogDebug(
            "Created item: {ItemId} ({ItemName}), Quality={Quality}",
            item.Id,
            item.Name,
            quality);

        // ─────────────────────────────────────────────────────────────
        // Step 4: Add to player's inventory
        // ─────────────────────────────────────────────────────────────
        player.Inventory.TryAdd(item);

        _logger.LogDebug(
            "Item {ItemId} added to player {PlayerId}'s inventory",
            item.Id,
            player.Id);

        // ─────────────────────────────────────────────────────────────
        // Step 5: Publish item crafted event
        // ─────────────────────────────────────────────────────────────
        var craftedEvent = ItemCraftedEvent.Create(
            playerId: player.Id,
            recipeId: recipe.RecipeId,
            itemId: item.Id,
            itemName: item.Name,
            quality: quality,
            stationId: stationDefinition.StationId);

        _eventLogger.Log(craftedEvent);

        _logger.LogInformation(
            "Crafting succeeded: Player {PlayerId} ({PlayerName}) crafted {ItemName} ({Quality}) " +
            "from recipe '{RecipeId}' at station '{StationId}'",
            player.Id,
            player.Name,
            item.Name,
            quality,
            recipe.RecipeId,
            stationDefinition.StationId);

        // ─────────────────────────────────────────────────────────────
        // Return success result
        // ─────────────────────────────────────────────────────────────
        return CraftResult.Success(
            roll: roll,
            modifier: modifier,
            total: total,
            dc: dc,
            item: item,
            quality: quality);
    }

    /// <summary>
    /// Handles a failed crafting attempt.
    /// </summary>
    /// <param name="player">The player who attempted to craft.</param>
    /// <param name="recipe">The recipe that was being crafted.</param>
    /// <param name="roll">The d20 roll value.</param>
    /// <param name="modifier">The skill modifier.</param>
    /// <param name="total">The total roll (roll + modifier).</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="margin">The roll margin (total - dc, negative for failure).</param>
    /// <returns>A failed craft result with resource losses.</returns>
    /// <remarks>
    /// <para>
    /// Failure handling:
    /// <list type="number">
    ///   <item><description>Calculate resource losses based on failure severity</description></item>
    ///   <item><description>Apply the losses to player's resources</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private CraftResult HandleCraftingFailure(
        Player player,
        RecipeDefinition recipe,
        int roll,
        int modifier,
        int total,
        int dc,
        int margin)
    {
        var isCloseFailure = margin >= BadFailureThreshold;
        var failureType = isCloseFailure ? "Close" : "Bad";

        _logger.LogDebug(
            "Handling crafting failure for player {PlayerId}, recipe '{RecipeId}': " +
            "margin={Margin}, failureType={FailureType}",
            player.Id,
            recipe.RecipeId,
            margin,
            failureType);

        // ─────────────────────────────────────────────────────────────
        // Step 1: Calculate resource losses
        // ─────────────────────────────────────────────────────────────
        var losses = CalculateFailureLoss(recipe, margin);

        _logger.LogDebug(
            "Calculated {LossCount} resource losses, total {TotalLost} items",
            losses.Count,
            losses.Sum(l => l.Amount));

        // ─────────────────────────────────────────────────────────────
        // Step 2: Apply the losses
        // ─────────────────────────────────────────────────────────────
        ApplyResourceLosses(player, losses);

        _logger.LogInformation(
            "Crafting failed ({FailureType}): Player {PlayerId} ({PlayerName}) " +
            "failed to craft '{RecipeId}'. Lost {TotalLost} resources. " +
            "Roll: d20({Roll}) {ModSign}{Modifier} = {Total} vs DC {DC} (margin: {Margin})",
            failureType,
            player.Id,
            player.Name,
            recipe.RecipeId,
            losses.Sum(l => l.Amount),
            roll,
            modifier >= 0 ? "+" : "",
            modifier,
            total,
            dc,
            margin);

        // ─────────────────────────────────────────────────────────────
        // Return failure result
        // ─────────────────────────────────────────────────────────────
        return CraftResult.Failed(
            roll: roll,
            modifier: modifier,
            total: total,
            dc: dc,
            losses: losses);
    }

    /// <summary>
    /// Determines the quality of a crafted item based on the roll.
    /// </summary>
    /// <param name="roll">The raw d20 roll value.</param>
    /// <param name="margin">The roll margin (total - DC).</param>
    /// <param name="isNatural20">Whether the roll was a natural 20.</param>
    /// <returns>The quality tier for the crafted item.</returns>
    /// <remarks>
    /// Quality determination:
    /// <list type="bullet">
    ///   <item><description>Natural 20 → Legendary (always)</description></item>
    ///   <item><description>Margin >= 10 → Masterwork</description></item>
    ///   <item><description>Margin >= 5 → Fine</description></item>
    ///   <item><description>Otherwise → Standard</description></item>
    /// </list>
    /// </remarks>
    private static CraftedItemQuality DetermineQuality(int roll, int margin, bool isNatural20)
    {
        // Natural 20 always produces Legendary quality
        if (isNatural20)
        {
            return CraftedItemQuality.Legendary;
        }

        // Quality based on margin
        return margin switch
        {
            >= MasterworkMarginThreshold => CraftedItemQuality.Masterwork,
            >= FineMarginThreshold => CraftedItemQuality.Fine,
            _ => CraftedItemQuality.Standard
        };
    }

    /// <summary>
    /// Gets the list of ingredients the player is missing for a recipe.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="recipe">The recipe to check against.</param>
    /// <returns>A list of missing ingredients (empty if player has all).</returns>
    /// <remarks>
    /// For each ingredient, checks the player's resource pool. If the player
    /// doesn't have enough, adds to the missing list with the quantity still needed.
    /// </remarks>
    private IReadOnlyList<MissingIngredient> GetMissingIngredients(Player player, RecipeDefinition recipe)
    {
        var missing = new List<MissingIngredient>();

        foreach (var ingredient in recipe.Ingredients)
        {
            // Get player's resource pool for this ingredient
            var resourcePool = player.GetResource(ingredient.ResourceId);
            var playerHas = resourcePool?.Current ?? 0;

            // Check if player has enough
            if (playerHas < ingredient.Quantity)
            {
                var quantityNeeded = ingredient.Quantity - playerHas;

                // Look up resource name for display
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
    /// Consumes all required resources for a recipe from the player's inventory.
    /// </summary>
    /// <param name="player">The player whose resources to consume.</param>
    /// <param name="recipe">The recipe defining the resources to consume.</param>
    /// <remarks>
    /// Uses the ResourcePool.Spend method which only succeeds if the player
    /// has enough resources. This should only be called after validation confirms
    /// the player has all required ingredients.
    /// </remarks>
    private void ConsumeResources(Player player, RecipeDefinition recipe)
    {
        foreach (var ingredient in recipe.Ingredients)
        {
            var resourcePool = player.GetResource(ingredient.ResourceId);

            if (resourcePool is null)
            {
                // This shouldn't happen if validation passed
                _logger.LogWarning(
                    "Cannot consume resource '{ResourceId}' - player {PlayerId} has no pool for it",
                    ingredient.ResourceId,
                    player.Id);
                continue;
            }

            var spent = resourcePool.Spend(ingredient.Quantity);

            if (spent)
            {
                _logger.LogDebug(
                    "Consumed {Quantity} of {ResourceId} from player {PlayerId}. Remaining: {Remaining}",
                    ingredient.Quantity,
                    ingredient.ResourceId,
                    player.Id,
                    resourcePool.Current);
            }
            else
            {
                // This shouldn't happen if validation passed
                _logger.LogWarning(
                    "Failed to consume {Quantity} of {ResourceId} from player {PlayerId} - insufficient resources",
                    ingredient.Quantity,
                    ingredient.ResourceId,
                    player.Id);
            }
        }
    }

    /// <summary>
    /// Applies resource losses to the player's inventory.
    /// </summary>
    /// <param name="player">The player whose resources to reduce.</param>
    /// <param name="losses">The list of resource losses to apply.</param>
    /// <remarks>
    /// Uses the ResourcePool.Lose method which reduces the current amount
    /// down to zero (cannot go negative). Losses that exceed the available
    /// amount are capped at the available amount.
    /// </remarks>
    private void ApplyResourceLosses(Player player, IReadOnlyList<ResourceLoss> losses)
    {
        foreach (var loss in losses)
        {
            var resourcePool = player.GetResource(loss.ResourceId);

            if (resourcePool is null)
            {
                _logger.LogDebug(
                    "Cannot apply loss for resource '{ResourceId}' - player {PlayerId} has no pool for it",
                    loss.ResourceId,
                    player.Id);
                continue;
            }

            var previousAmount = resourcePool.Current;
            var actualLoss = resourcePool.Lose(loss.Amount);

            _logger.LogDebug(
                "Applied loss: {ResourceName} ({ResourceId}) - intended={IntendedLoss}, " +
                "actual={ActualLoss}, before={Before}, after={After}",
                loss.Name,
                loss.ResourceId,
                loss.Amount,
                actualLoss,
                previousAmount,
                resourcePool.Current);
        }
    }

    /// <summary>
    /// Creates a crafted item from a recipe definition.
    /// </summary>
    /// <param name="recipe">The recipe that was crafted.</param>
    /// <param name="quality">The quality tier of the item.</param>
    /// <returns>A new Item instance.</returns>
    /// <remarks>
    /// <para>
    /// The item name may be prefixed with quality tier for exceptional items:
    /// <list type="bullet">
    ///   <item><description>Standard: No prefix</description></item>
    ///   <item><description>Fine: "Fine " prefix</description></item>
    ///   <item><description>Masterwork: "Masterwork " prefix</description></item>
    ///   <item><description>Legendary: "Legendary " prefix</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private Item CreateCraftedItem(RecipeDefinition recipe, CraftedItemQuality quality)
    {
        // Determine item name with quality prefix
        // Use the recipe name as the base display name (e.g., "Iron Sword")
        var baseName = recipe.Name;
        var itemName = quality switch
        {
            CraftedItemQuality.Legendary => $"Legendary {baseName}",
            CraftedItemQuality.Masterwork => $"Masterwork {baseName}",
            CraftedItemQuality.Fine => $"Fine {baseName}",
            _ => baseName
        };

        // Generate description based on quality
        var description = quality switch
        {
            CraftedItemQuality.Legendary => $"A legendary {baseName.ToLowerInvariant()}, crafted with extraordinary skill.",
            CraftedItemQuality.Masterwork => $"A masterfully crafted {baseName.ToLowerInvariant()} of exceptional quality.",
            CraftedItemQuality.Fine => $"A finely crafted {baseName.ToLowerInvariant()} of above-average quality.",
            _ => $"A {baseName.ToLowerInvariant()} crafted with care."
        };

        // Map recipe category to item type
        var itemType = MapRecipeCategoryToItemType(recipe.Category);

        // Calculate item value with quality multiplier
        var itemValue = CalculateItemValue(recipe, quality);

        // Create the item using the actual Item constructor
        // Note: Using the Item constructor directly as IItemFactory doesn't exist
        var item = new Item(
            name: itemName,
            description: description,
            type: itemType,
            value: itemValue);

        return item;
    }

    /// <summary>
    /// Maps a recipe category to an item type.
    /// </summary>
    /// <param name="category">The recipe category.</param>
    /// <returns>The corresponding item type.</returns>
    private static ItemType MapRecipeCategoryToItemType(RecipeCategory category)
    {
        // Map recipe categories to available ItemType values
        // Note: ItemType has limited options (Weapon, Armor, Consumable, Quest, Misc, Key)
        // so some recipe categories map to Misc as a fallback
        return category switch
        {
            RecipeCategory.Weapon => ItemType.Weapon,
            RecipeCategory.Armor => ItemType.Armor,
            RecipeCategory.Potion => ItemType.Consumable,  // Potions are consumable
            RecipeCategory.Consumable => ItemType.Consumable,
            RecipeCategory.Accessory => ItemType.Misc,     // No Accessory type, use Misc
            RecipeCategory.Tool => ItemType.Misc,          // No Tool type, use Misc
            RecipeCategory.Material => ItemType.Misc,      // No Material type, use Misc
            _ => ItemType.Misc
        };
    }

    /// <summary>
    /// Calculates the gold value of a crafted item.
    /// </summary>
    /// <param name="recipe">The recipe that was crafted.</param>
    /// <param name="quality">The quality tier of the item.</param>
    /// <returns>The gold value.</returns>
    /// <remarks>
    /// <para>
    /// Base value is derived from the recipe's difficulty class (DC * 10).
    /// Higher DC recipes produce more valuable items.
    /// </para>
    /// <para>
    /// Quality multipliers:
    /// <list type="bullet">
    ///   <item><description>Legendary: 5x</description></item>
    ///   <item><description>Masterwork: 3x</description></item>
    ///   <item><description>Fine: 1.5x</description></item>
    ///   <item><description>Standard: 1x</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static int CalculateItemValue(RecipeDefinition recipe, CraftedItemQuality quality)
    {
        // Base value derived from difficulty class
        // Higher DC recipes produce more valuable items
        var baseValue = recipe.DifficultyClass * 10;

        var multiplier = quality switch
        {
            CraftedItemQuality.Legendary => 5.0f,
            CraftedItemQuality.Masterwork => 3.0f,
            CraftedItemQuality.Fine => 1.5f,
            _ => 1.0f
        };

        return (int)(baseValue * multiplier);
    }

    /// <summary>
    /// Formats a station ID into a user-friendly display name for error messages.
    /// </summary>
    /// <param name="stationId">The station ID (e.g., "anvil", "alchemy-table").</param>
    /// <returns>A formatted name with article (e.g., "an Anvil", "an Alchemy Table").</returns>
    private static string FormatStationNameForError(string stationId)
    {
        // Convert kebab-case to title case with spaces
        var words = stationId.Split('-')
            .Select(word => word.Length > 0
                ? char.ToUpperInvariant(word[0]) + word[1..]
                : word)
            .ToArray();

        var name = string.Join(" ", words);

        // Add appropriate article
        var article = "aeiouAEIOU".Contains(name[0]) ? "an" : "a";
        return $"{article} {name}";
    }
}
