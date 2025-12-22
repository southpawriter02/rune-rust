using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Data;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Crafting;
using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements crafting operations using WITS-based dice rolls.
/// Handles recipe lookup, ingredient validation, and item creation.
/// Supports trade-specific catastrophe effects (v0.3.1c).
/// </summary>
public class CraftingService : ICraftingService
{
    private readonly IDiceService _diceService;
    private readonly IInventoryService _inventoryService;
    private readonly IItemRepository _itemRepository;
    private readonly ITraumaService _traumaService;
    private readonly ILogger<CraftingService> _logger;

    /// <summary>
    /// Threshold above DC for masterwork quality (DC + 5).
    /// </summary>
    private const int MasterworkThreshold = 5;

    public CraftingService(
        IDiceService diceService,
        IInventoryService inventoryService,
        IItemRepository itemRepository,
        ITraumaService traumaService,
        ILogger<CraftingService> logger)
    {
        _diceService = diceService;
        _inventoryService = inventoryService;
        _itemRepository = itemRepository;
        _traumaService = traumaService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CraftingResult> CraftItemAsync(Character crafter, string recipeId)
    {
        _logger.LogInformation("Crafting attempt: {RecipeId} by {CharacterName}", recipeId, crafter.Name);

        // Step 1: Look up recipe
        var recipe = RecipeRegistry.GetById(recipeId);
        var found = recipe != null;
        _logger.LogDebug("Recipe lookup: {RecipeId} found={Found}", recipeId, found);

        if (recipe == null)
        {
            return CreateFailedResult(recipeId, "Unknown Recipe", 0, 0, 0, 0, 0,
                $"Recipe '{recipeId}' not found.", Array.Empty<int>());
        }

        // Step 2: Validate ingredients
        if (!HasIngredients(crafter, recipe))
        {
            return CreateFailedResult(recipe.RecipeId, recipe.Name, 0, 0, 0, 0, recipe.BaseDc,
                $"Missing ingredients for {recipe.Name}.", Array.Empty<int>());
        }

        // Step 3: Consume ingredients (before rolling - ingredients are always consumed)
        foreach (var (itemId, required) in recipe.Ingredients)
        {
            var removeResult = await _inventoryService.RemoveItemAsync(crafter, itemId, required);
            if (removeResult.Success)
            {
                _logger.LogDebug("Consumed: {Quantity}x {ItemId}", required, itemId);
            }
            else
            {
                _logger.LogWarning("Failed to consume ingredient {ItemId}: {Message}", itemId, removeResult.Message);
            }
        }

        // Step 4: Roll WITS dice pool
        var wits = crafter.Wits;
        var diceResult = _diceService.Roll(wits, $"Craft {recipe.Name}");
        var netSuccesses = diceResult.Successes - diceResult.Botches;

        _logger.LogDebug(
            "Craft roll: {Wits}d10 = {Successes}S/{Botches}B, Net={Net}, DC={DC}",
            wits, diceResult.Successes, diceResult.Botches, netSuccesses, recipe.BaseDc);

        // Step 5: Determine outcome
        var outcome = DetermineOutcome(netSuccesses, recipe.BaseDc);

        // Step 6: Handle result based on outcome
        return outcome switch
        {
            CraftingOutcome.Masterwork => await HandleMasterworkAsync(crafter, recipe, diceResult, netSuccesses),
            CraftingOutcome.Success => await HandleSuccessAsync(crafter, recipe, diceResult, netSuccesses),
            CraftingOutcome.Failure => HandleFailure(recipe, diceResult, netSuccesses),
            CraftingOutcome.Catastrophe => HandleCatastrophe(crafter, recipe, diceResult, netSuccesses),
            _ => HandleFailure(recipe, diceResult, netSuccesses)
        };
    }

    /// <inheritdoc />
    public bool HasIngredients(Character crafter, Recipe recipe)
    {
        foreach (var (itemId, required) in recipe.Ingredients)
        {
            var inventoryItem = crafter.Inventory
                .FirstOrDefault(inv => inv.Item.Name.Equals(itemId, StringComparison.OrdinalIgnoreCase));

            var available = inventoryItem?.Quantity ?? 0;
            _logger.LogDebug("Ingredient check: {ItemId} required={Required} has={Available}",
                itemId, required, available);

            if (available < required)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public IReadOnlyList<Recipe> GetAvailableRecipes(Character crafter)
    {
        return RecipeRegistry.GetAll()
            .Where(recipe => HasIngredients(crafter, recipe))
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<Recipe> GetRecipesByTrade(CraftingTrade trade)
    {
        return RecipeRegistry.GetByTrade(trade);
    }

    /// <inheritdoc />
    public Recipe? GetRecipe(string recipeId)
    {
        return RecipeRegistry.GetById(recipeId);
    }

    /// <inheritdoc />
    public IReadOnlyList<Recipe> GetAllRecipes()
    {
        return RecipeRegistry.GetAll();
    }

    /// <inheritdoc />
    public CraftingViewModel BuildViewModel(Character crafter, CraftingTrade trade, int selectedIndex = 0)
    {
        _logger.LogTrace("[Crafting] Building ViewModel for {Character}, Trade={Trade}", crafter.Name, trade);

        // Get filtered recipes
        var tradeRecipes = GetRecipesByTrade(trade);
        _logger.LogDebug("[Crafting] Found {Count} recipes for {Trade}", tradeRecipes.Count, trade);

        var recipes = tradeRecipes
            .Select((r, i) => new RecipeView(
                Index: i + 1,
                RecipeId: r.RecipeId,
                Name: r.Name,
                Trade: r.Trade,
                DifficultyClass: r.BaseDc,
                CanCraft: HasIngredients(crafter, r)
            ))
            .ToList();

        // Build details for selected recipe
        RecipeDetailsView? details = null;
        if (selectedIndex >= 0 && selectedIndex < recipes.Count)
        {
            var selected = GetRecipe(recipes[selectedIndex].RecipeId);
            if (selected != null)
            {
                details = BuildRecipeDetails(crafter, selected);
            }
        }

        return new CraftingViewModel(
            CharacterName: crafter.Name,
            CrafterWits: crafter.Wits,
            SelectedTrade: trade,
            Recipes: recipes,
            SelectedRecipeIndex: selectedIndex,
            SelectedRecipeDetails: details
        );
    }

    /// <summary>
    /// Builds the details view for a specific recipe with ingredient availability.
    /// </summary>
    /// <param name="crafter">The character to check inventory against.</param>
    /// <param name="recipe">The recipe to build details for.</param>
    /// <returns>A RecipeDetailsView with ingredient breakdown.</returns>
    private RecipeDetailsView BuildRecipeDetails(Character crafter, Recipe recipe)
    {
        _logger.LogTrace("[Crafting] Building details for {RecipeId}", recipe.RecipeId);

        var ingredients = recipe.Ingredients
            .Select(kvp =>
            {
                var available = crafter.Inventory
                    .FirstOrDefault(i => i.Item.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    ?.Quantity ?? 0;
                return new IngredientView(
                    ItemName: kvp.Key,
                    RequiredQuantity: kvp.Value,
                    AvailableQuantity: available,
                    IsSatisfied: available >= kvp.Value
                );
            })
            .ToList();

        return new RecipeDetailsView(
            RecipeId: recipe.RecipeId,
            Name: recipe.Name,
            Description: recipe.Description,
            Trade: recipe.Trade,
            DifficultyClass: recipe.BaseDc,
            CrafterWits: crafter.Wits,
            Ingredients: ingredients,
            OutputItemName: recipe.OutputItemId,
            OutputQuantity: recipe.OutputQuantity,
            CanCraft: ingredients.All(i => i.IsSatisfied)
        );
    }

    /// <summary>
    /// Determines the crafting outcome based on net successes and DC.
    /// </summary>
    private static CraftingOutcome DetermineOutcome(int netSuccesses, int dc)
    {
        if (netSuccesses < 0)
            return CraftingOutcome.Catastrophe;

        if (netSuccesses >= dc + MasterworkThreshold)
            return CraftingOutcome.Masterwork;

        if (netSuccesses >= dc)
            return CraftingOutcome.Success;

        return CraftingOutcome.Failure;
    }

    /// <summary>
    /// Handles a masterwork crafting result.
    /// </summary>
    private async Task<CraftingResult> HandleMasterworkAsync(
        Character crafter,
        Recipe recipe,
        DiceResult diceResult,
        int netSuccesses)
    {
        var outputItem = await _itemRepository.GetByNameAsync(recipe.OutputItemId);
        if (outputItem != null)
        {
            // Create masterwork version (MythForged quality - legendary craftsmanship)
            var masterworkItem = CloneItemWithQuality(outputItem, QualityTier.MythForged);
            await _inventoryService.AddItemAsync(crafter, masterworkItem, recipe.OutputQuantity);
        }

        var message = $"MASTERWORK! You craft a flawless {recipe.Name}.";
        _logger.LogInformation(
            "Craft MASTERWORK: {RecipeName} -> {Quantity}x {ItemId}",
            recipe.Name, recipe.OutputQuantity, recipe.OutputItemId);

        return new CraftingResult(
            IsSuccess: true,
            Outcome: CraftingOutcome.Masterwork,
            RecipeId: recipe.RecipeId,
            RecipeName: recipe.Name,
            DiceRolled: diceResult.Rolls.Count,
            Successes: diceResult.Successes,
            Botches: diceResult.Botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: recipe.BaseDc,
            OutputItemId: recipe.OutputItemId,
            OutputQuantity: recipe.OutputQuantity,
            OutputQuality: QualityTier.MythForged,
            Message: message,
            Rolls: diceResult.Rolls);
    }

    /// <summary>
    /// Handles a successful crafting result.
    /// </summary>
    private async Task<CraftingResult> HandleSuccessAsync(
        Character crafter,
        Recipe recipe,
        DiceResult diceResult,
        int netSuccesses)
    {
        var outputItem = await _itemRepository.GetByNameAsync(recipe.OutputItemId);
        if (outputItem != null)
        {
            // Create standard crafted version (ClanForged quality - proper craftsmanship)
            var craftedItem = CloneItemWithQuality(outputItem, QualityTier.ClanForged);
            await _inventoryService.AddItemAsync(crafter, craftedItem, recipe.OutputQuantity);
        }

        var message = $"Success! You craft {recipe.Name}.";
        _logger.LogInformation(
            "Craft SUCCESS: {RecipeName} -> {Quantity}x {ItemId} (ClanForged)",
            recipe.Name, recipe.OutputQuantity, recipe.OutputItemId);

        return new CraftingResult(
            IsSuccess: true,
            Outcome: CraftingOutcome.Success,
            RecipeId: recipe.RecipeId,
            RecipeName: recipe.Name,
            DiceRolled: diceResult.Rolls.Count,
            Successes: diceResult.Successes,
            Botches: diceResult.Botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: recipe.BaseDc,
            OutputItemId: recipe.OutputItemId,
            OutputQuantity: recipe.OutputQuantity,
            OutputQuality: QualityTier.ClanForged,
            Message: message,
            Rolls: diceResult.Rolls);
    }

    /// <summary>
    /// Handles a failed crafting result.
    /// </summary>
    private CraftingResult HandleFailure(Recipe recipe, DiceResult diceResult, int netSuccesses)
    {
        var message = $"Failure. Your attempt at {recipe.Name} falls short. Materials wasted.";
        _logger.LogInformation(
            "Craft FAILURE: {RecipeName} (Net {Net} < DC {DC})",
            recipe.Name, netSuccesses, recipe.BaseDc);

        return new CraftingResult(
            IsSuccess: false,
            Outcome: CraftingOutcome.Failure,
            RecipeId: recipe.RecipeId,
            RecipeName: recipe.Name,
            DiceRolled: diceResult.Rolls.Count,
            Successes: diceResult.Successes,
            Botches: diceResult.Botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: recipe.BaseDc,
            OutputItemId: null,
            OutputQuantity: 0,
            OutputQuality: null,
            Message: message,
            Rolls: diceResult.Rolls);
    }

    /// <summary>
    /// Handles a catastrophic crafting result.
    /// Applies trade-specific consequences based on CatastropheType (v0.3.1c).
    /// </summary>
    private CraftingResult HandleCatastrophe(Character crafter, Recipe recipe, DiceResult diceResult, int netSuccesses)
    {
        string message;
        int? damageDealt = null;
        int? corruptionAdded = null;

        switch (recipe.CatastropheType)
        {
            case CatastropheType.Explosive:
                // Alchemy: Explosive catastrophe deals physical damage
                damageDealt = recipe.CatastropheDamage;
                crafter.CurrentHP = Math.Max(0, crafter.CurrentHP - recipe.CatastropheDamage);
                message = $"CATASTROPHE! Your {recipe.Name} explodes violently! You take {recipe.CatastropheDamage} damage!";
                _logger.LogWarning(
                    "ALCHEMY CATASTROPHE: {CharacterName} takes {Damage} explosive damage!",
                    crafter.Name, recipe.CatastropheDamage);
                break;

            case CatastropheType.Corruption:
                // Runeforging: Corruption catastrophe adds permanent Corruption
                corruptionAdded = recipe.CatastropheCorruption;
                _traumaService.AddCorruption(crafter, recipe.CatastropheCorruption, "Runic Backlash");
                message = $"CATASTROPHE! The runes of {recipe.Name} twist against you! You gain {recipe.CatastropheCorruption} Corruption!";
                _logger.LogWarning(
                    "RUNEFORGING CATASTROPHE: {CharacterName} gains {Corruption} Corruption!",
                    crafter.Name, recipe.CatastropheCorruption);
                break;

            default:
                // Standard catastrophe: Materials lost only
                message = $"CATASTROPHE! Your {recipe.Name} attempt goes terribly wrong. Materials destroyed.";
                _logger.LogWarning("Craft CATASTROPHE: {RecipeName} - materials lost!", recipe.Name);
                break;
        }

        return new CraftingResult(
            IsSuccess: false,
            Outcome: CraftingOutcome.Catastrophe,
            RecipeId: recipe.RecipeId,
            RecipeName: recipe.Name,
            DiceRolled: diceResult.Rolls.Count,
            Successes: diceResult.Successes,
            Botches: diceResult.Botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: recipe.BaseDc,
            OutputItemId: null,
            OutputQuantity: 0,
            OutputQuality: null,
            Message: message,
            Rolls: diceResult.Rolls,
            DamageDealt: damageDealt,
            CorruptionAdded: corruptionAdded);
    }

    /// <summary>
    /// Creates a failed result for validation errors (before rolling).
    /// </summary>
    private static CraftingResult CreateFailedResult(
        string recipeId,
        string recipeName,
        int diceRolled,
        int successes,
        int botches,
        int netSuccesses,
        int dc,
        string message,
        IReadOnlyList<int> rolls)
    {
        return new CraftingResult(
            IsSuccess: false,
            Outcome: CraftingOutcome.Failure,
            RecipeId: recipeId,
            RecipeName: recipeName,
            DiceRolled: diceRolled,
            Successes: successes,
            Botches: botches,
            NetSuccesses: netSuccesses,
            DifficultyClass: dc,
            OutputItemId: null,
            OutputQuantity: 0,
            OutputQuality: null,
            Message: message,
            Rolls: rolls);
    }

    /// <summary>
    /// Creates a copy of an item with a different quality tier.
    /// </summary>
    private static Item CloneItemWithQuality(Item original, QualityTier quality)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Name = original.Name,
            ItemType = original.ItemType,
            Description = original.Description,
            DetailedDescription = original.DetailedDescription,
            Weight = original.Weight,
            Value = original.Value,
            Quality = quality,
            IsStackable = original.IsStackable,
            MaxStackSize = original.MaxStackSize,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
    }
}
