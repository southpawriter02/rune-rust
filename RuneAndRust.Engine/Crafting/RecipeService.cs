using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// v0.36.4: Recipe Discovery and Management Service
/// Handles recipe discovery, purchasing, and filtering
/// </summary>
public class RecipeService
{
    private static readonly ILogger _log = Log.ForContext<RecipeService>();
    private readonly CraftingRepository _repository;

    public RecipeService(CraftingRepository repository)
    {
        _repository = repository;
        _log.Debug("RecipeService initialized");
    }

    /// <summary>
    /// Discover a recipe for a character
    /// </summary>
    public bool DiscoverRecipe(int characterId, int recipeId, string discoverySource)
    {
        _log.Information("Discover recipe: Character={CharacterId}, Recipe={RecipeId}, Source={Source}",
            characterId, recipeId, discoverySource);

        try
        {
            return _repository.DiscoverRecipe(characterId, recipeId, discoverySource);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to discover recipe: Character={CharacterId}, Recipe={RecipeId}",
                characterId, recipeId);
            throw;
        }
    }

    /// <summary>
    /// Get all recipes known by a character
    /// </summary>
    public List<RecipeDetails> GetKnownRecipes(int characterId)
    {
        return _repository.GetKnownRecipes(characterId);
    }

    /// <summary>
    /// Get craftable recipes (player has components and optional station filter)
    /// </summary>
    public List<CraftableRecipe> GetCraftableRecipes(
        int characterId,
        int? stationId = null)
    {
        var knownRecipes = _repository.GetKnownRecipes(characterId);
        var craftable = new List<CraftableRecipe>();

        // Get station if specified
        CraftingStation? station = null;
        if (stationId.HasValue)
        {
            station = _repository.GetStationById(stationId.Value);
        }

        foreach (var recipe in knownRecipes)
        {
            // Filter by station if provided
            if (station != null &&
                recipe.RequiredStation != "Any" &&
                recipe.RequiredStation != station.StationType)
            {
                continue; // Wrong station type
            }

            // Check if player has components
            bool hasComponents = CheckComponentAvailability(characterId, recipe.RequiredComponents);

            craftable.Add(new CraftableRecipe
            {
                Recipe = recipe,
                Components = recipe.RequiredComponents,
                CanCraft = hasComponents
            });
        }

        _log.Debug("Found {Total} known recipes, {Craftable} craftable for character {CharacterId}",
            knownRecipes.Count, craftable.Count(c => c.CanCraft), characterId);

        return craftable;
    }

    /// <summary>
    /// Purchase recipe from merchant
    /// </summary>
    public PurchaseResult PurchaseRecipe(
        int characterId,
        int recipeId,
        int merchantId)
    {
        _log.Information("Purchase recipe: Character={CharacterId}, Recipe={RecipeId}, Merchant={MerchantId}",
            characterId, recipeId, merchantId);

        try
        {
            // Get recipe details
            var recipe = _repository.GetRecipeById(recipeId);
            if (recipe == null)
            {
                return PurchaseResult.Failure("Recipe not found");
            }

            // Calculate cost based on tier
            int cost = CalculateRecipeCost(recipe.RecipeTier);

            // Check if player has credits
            int credits = _repository.GetCharacterCredits(characterId);
            if (credits < cost)
            {
                _log.Information("Insufficient credits: need {Cost}, have {Credits}",
                    cost, credits);
                return PurchaseResult.Failure(
                    $"Insufficient credits: need {cost}, have {credits}");
            }

            // Discover recipe
            bool discovered = _repository.DiscoverRecipe(characterId, recipeId, "Merchant");

            if (!discovered)
            {
                // Already known - don't charge
                return PurchaseResult.Failure("You already know this recipe");
            }

            // Deduct credits
            _repository.DeductCredits(characterId, cost);

            _log.Information("Character {CharacterId} purchased recipe {RecipeName} for {Cost} credits",
                characterId, recipe.RecipeName, cost);

            return PurchaseResult.Successful(recipe.RecipeName, cost);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to purchase recipe: Character={CharacterId}, Recipe={RecipeId}",
                characterId, recipeId);
            throw;
        }
    }

    /// <summary>
    /// Get recipes available from merchants
    /// </summary>
    public List<Recipe> GetMerchantRecipes()
    {
        return _repository.GetMerchantRecipes();
    }

    /// <summary>
    /// Increment times crafted counter for a recipe
    /// </summary>
    public void IncrementTimesCrafted(int characterId, int recipeId)
    {
        _repository.IncrementTimesCrafted(characterId, recipeId);
    }

    #region Helper Methods

    /// <summary>
    /// Calculate recipe purchase cost based on tier
    /// </summary>
    private int CalculateRecipeCost(string tier)
    {
        return tier switch
        {
            "Basic" => 75,
            "Advanced" => 225,
            "Expert" => 525,
            "Master" => 1000,
            _ => 100
        };
    }

    /// <summary>
    /// Check if player has all required components for a recipe
    /// </summary>
    private bool CheckComponentAvailability(
        int characterId,
        List<RecipeComponent> components)
    {
        var playerComponents = _repository.GetPlayerComponents(characterId);

        foreach (var required in components)
        {
            // Sum available quantity across all quality tiers >= minimum
            int available = playerComponents
                .Where(pc => pc.ItemId == required.ComponentItemId &&
                            pc.QualityTier >= required.MinimumQuality)
                .Sum(pc => pc.Quantity);

            if (available < required.QuantityRequired)
                return false;
        }

        return true;
    }

    #endregion
}
