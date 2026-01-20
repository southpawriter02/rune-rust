using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Handlers;

/// <summary>
/// Handler for processing recipe scroll item use actions.
/// </summary>
/// <remarks>
/// <para>
/// RecipeScrollUseHandler implements the <see cref="IItemUseHandler"/> interface to process
/// items with the <see cref="ItemEffect.LearnRecipe"/> effect. When a player uses a recipe
/// scroll, this handler validates the scroll, checks if the recipe is already known,
/// and teaches the recipe if appropriate.
/// </para>
/// <para>
/// The handler follows these steps:
/// <list type="number">
///   <item><description>Validate the item has the LearnRecipe effect</description></item>
///   <item><description>Validate the item has a RecipeId</description></item>
///   <item><description>Check if the recipe exists in the game</description></item>
///   <item><description>Check if the player already knows the recipe (preserve if known)</description></item>
///   <item><description>Teach the recipe to the player</description></item>
///   <item><description>Log the discovery event</description></item>
///   <item><description>Return the appropriate result</description></item>
/// </list>
/// </para>
/// <para>
/// This handler does NOT remove items from inventory. It returns an <see cref="ItemUseResult"/>
/// indicating whether the item should be consumed. The caller is responsible for managing
/// inventory updates based on the <see cref="ItemUseResult.WasConsumed"/> property.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Handler usage (typically through IItemUseHandler abstraction)
/// var handlers = serviceProvider.GetServices&lt;IItemUseHandler&gt;();
/// var handler = handlers.FirstOrDefault(h => h.HandledEffect == item.Effect);
///
/// if (handler != null)
/// {
///     var result = handler.Handle(player, recipeScroll);
///     if (result.WasConsumed)
///     {
///         player.Inventory.RemoveItem(recipeScroll);
///     }
///     DisplayMessage(result.Message);
/// }
/// </code>
/// </example>
public sealed class RecipeScrollUseHandler : IItemUseHandler
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Service for managing recipe learning and knowledge.
    /// </summary>
    private readonly IRecipeService _recipeService;

    /// <summary>
    /// Provider for recipe definitions.
    /// </summary>
    private readonly IRecipeProvider _recipeProvider;

    /// <summary>
    /// Logger for game events.
    /// </summary>
    private readonly IGameEventLogger _eventLogger;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<RecipeScrollUseHandler> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeScrollUseHandler"/> class.
    /// </summary>
    /// <param name="recipeService">The recipe service for learning recipes.</param>
    /// <param name="recipeProvider">The recipe provider for recipe lookups.</param>
    /// <param name="eventLogger">The game event logger.</param>
    /// <param name="logger">The diagnostic logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
    /// <remarks>
    /// Dependencies are typically injected via the DI container. Register this handler
    /// as a scoped service implementing <see cref="IItemUseHandler"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// // DI registration
    /// services.AddScoped&lt;IItemUseHandler, RecipeScrollUseHandler&gt;();
    /// </code>
    /// </example>
    public RecipeScrollUseHandler(
        IRecipeService recipeService,
        IRecipeProvider recipeProvider,
        IGameEventLogger eventLogger,
        ILogger<RecipeScrollUseHandler> logger)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _recipeProvider = recipeProvider ?? throw new ArgumentNullException(nameof(recipeProvider));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "RecipeScrollUseHandler initialized with dependencies: " +
            "RecipeService={RecipeServiceType}, RecipeProvider={RecipeProviderType}",
            recipeService.GetType().Name,
            recipeProvider.GetType().Name);
    }

    // ═══════════════════════════════════════════════════════════════
    // IITEMUSEHANDLER IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the item effect this handler processes.
    /// </summary>
    /// <remarks>
    /// This handler processes items with <see cref="ItemEffect.LearnRecipe"/>,
    /// which are recipe scroll items that teach crafting recipes when used.
    /// </remarks>
    public ItemEffect HandledEffect => ItemEffect.LearnRecipe;

    /// <summary>
    /// Handles the use of a recipe scroll item on a player.
    /// </summary>
    /// <param name="player">The player using the recipe scroll.</param>
    /// <param name="item">The recipe scroll being used.</param>
    /// <returns>
    /// An <see cref="ItemUseResult"/> indicating:
    /// <list type="bullet">
    ///   <item><description><see cref="ItemUseResult.IsSuccess"/> - Whether the operation completed successfully</description></item>
    ///   <item><description><see cref="ItemUseResult.WasConsumed"/> - Whether the scroll should be removed from inventory</description></item>
    ///   <item><description><see cref="ItemUseResult.Message"/> - A player-facing message describing the result</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when player or item is null.</exception>
    /// <remarks>
    /// <para>
    /// Result scenarios:
    /// <list type="bullet">
    ///   <item><description><b>Consumed</b>: Recipe was learned successfully. Scroll should be removed.</description></item>
    ///   <item><description><b>Preserved</b>: Player already knows the recipe. Scroll is kept.</description></item>
    ///   <item><description><b>Failed</b>: Invalid item, missing recipe ID, or recipe not found. Scroll is kept.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When a recipe is successfully learned, a <see cref="RecipeDiscoveredEvent"/> is logged
    /// via the game event logger with <see cref="DiscoverySource.Scroll"/> as the source.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = handler.Handle(player, recipeScroll);
    ///
    /// switch (result.ResultType)
    /// {
    ///     case ItemUseResultType.Consumed:
    ///         player.Inventory.RemoveItem(recipeScroll);
    ///         ShowSuccessMessage(result.Message);
    ///         break;
    ///     case ItemUseResultType.Preserved:
    ///         ShowInfoMessage(result.Message);
    ///         break;
    ///     case ItemUseResultType.Failed:
    ///         ShowErrorMessage(result.Message);
    ///         break;
    /// }
    /// </code>
    /// </example>
    public ItemUseResult Handle(Player player, Item item)
    {
        // ───────────────────────────────────────────────────────────
        // Validate arguments
        // ───────────────────────────────────────────────────────────
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        _logger.LogDebug(
            "Processing recipe scroll use: PlayerId={PlayerId}, PlayerName={PlayerName}, " +
            "ItemId={ItemId}, ItemName={ItemName}",
            player.Id,
            player.Name,
            item.Id,
            item.Name);

        // ───────────────────────────────────────────────────────────
        // Validate item has correct effect type
        // ───────────────────────────────────────────────────────────
        if (item.Effect != HandledEffect)
        {
            _logger.LogWarning(
                "Invalid item effect passed to RecipeScrollUseHandler: " +
                "Expected={ExpectedEffect}, Actual={ActualEffect}, ItemId={ItemId}",
                HandledEffect,
                item.Effect,
                item.Id);

            return ItemUseResult.Failed(
                "This item cannot be used as a recipe scroll.");
        }

        // ───────────────────────────────────────────────────────────
        // Validate item has a recipe ID
        // ───────────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(item.RecipeId))
        {
            _logger.LogWarning(
                "Recipe scroll has no RecipeId: ItemId={ItemId}, ItemName={ItemName}",
                item.Id,
                item.Name);

            return ItemUseResult.Failed(
                "This scroll appears to be blank or illegible.");
        }

        var recipeId = item.RecipeId;

        _logger.LogDebug(
            "Validating recipe existence: RecipeId={RecipeId}",
            recipeId);

        // ───────────────────────────────────────────────────────────
        // Validate recipe exists in the game
        // ───────────────────────────────────────────────────────────
        var recipe = _recipeProvider.GetRecipe(recipeId);
        if (recipe == null)
        {
            _logger.LogWarning(
                "Recipe scroll references non-existent recipe: " +
                "RecipeId={RecipeId}, ItemId={ItemId}, ItemName={ItemName}",
                recipeId,
                item.Id,
                item.Name);

            return ItemUseResult.Failed(
                "The recipe on this scroll has faded beyond recognition.");
        }

        _logger.LogDebug(
            "Recipe found: RecipeId={RecipeId}, RecipeName={RecipeName}",
            recipe.RecipeId,
            recipe.Name);

        // ───────────────────────────────────────────────────────────
        // Check if player already knows the recipe
        // ───────────────────────────────────────────────────────────
        if (_recipeService.IsRecipeKnown(player, recipeId))
        {
            _logger.LogDebug(
                "Player already knows recipe: PlayerId={PlayerId}, RecipeId={RecipeId}, RecipeName={RecipeName}",
                player.Id,
                recipeId,
                recipe.Name);

            return ItemUseResult.Preserved(
                $"You already know how to craft {recipe.Name}. The scroll has been preserved.");
        }

        // ───────────────────────────────────────────────────────────
        // Learn the recipe
        // ───────────────────────────────────────────────────────────
        _logger.LogDebug(
            "Attempting to learn recipe: PlayerId={PlayerId}, RecipeId={RecipeId}",
            player.Id,
            recipeId);

        var learnResult = _recipeService.LearnRecipe(player, recipeId);

        if (!learnResult.IsSuccess)
        {
            // This shouldn't happen if we validated correctly, but handle gracefully
            _logger.LogError(
                "Failed to learn recipe despite validation passing: " +
                "PlayerId={PlayerId}, RecipeId={RecipeId}, ResultType={ResultType}, FailureReason={FailureReason}",
                player.Id,
                recipeId,
                learnResult.ResultType,
                learnResult.FailureReason);

            return ItemUseResult.Failed(
                learnResult.FailureReason ?? "Failed to learn the recipe from this scroll.");
        }

        // ───────────────────────────────────────────────────────────
        // Log discovery event
        // ───────────────────────────────────────────────────────────
        var discoveredEvent = new RecipeDiscoveredEvent(
            PlayerId: player.Id,
            RecipeId: recipeId,
            RecipeName: recipe.Name,
            DiscoverySource: DiscoverySource.Scroll);

        _eventLogger.LogInventory(
            eventType: "RecipeDiscovered",
            message: $"{player.Name} discovered the recipe for {recipe.Name} from a scroll.",
            playerId: player.Id,
            data: new Dictionary<string, object>
            {
                ["recipeId"] = recipeId,
                ["recipeName"] = recipe.Name,
                ["discoverySource"] = DiscoverySource.Scroll.ToString(),
                ["itemId"] = item.Id.ToString(),
                ["itemName"] = item.Name,
                ["timestamp"] = discoveredEvent.Timestamp.ToString("O")
            });

        _logger.LogInformation(
            "Recipe learned from scroll: PlayerId={PlayerId}, PlayerName={PlayerName}, " +
            "RecipeId={RecipeId}, RecipeName={RecipeName}, DiscoverySource={DiscoverySource}",
            player.Id,
            player.Name,
            recipeId,
            recipe.Name,
            DiscoverySource.Scroll);

        // ───────────────────────────────────────────────────────────
        // Return success - scroll consumed
        // ───────────────────────────────────────────────────────────
        return ItemUseResult.ConsumedWithMessage(
            $"You carefully study the scroll and learn how to craft {recipe.Name}!");
    }
}
