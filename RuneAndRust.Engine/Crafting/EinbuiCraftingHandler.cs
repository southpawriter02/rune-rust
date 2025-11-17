using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// v0.36.4: Einbui Field Crafting Handler
/// Enables Einbui specialization to craft basic consumables in the field
/// without requiring a crafting station
/// </summary>
public class EinbuiCraftingHandler
{
    private static readonly ILogger _log = Log.ForContext<EinbuiCraftingHandler>();
    private readonly AdvancedCraftingService _craftingService;
    private readonly RecipeService _recipeService;
    private readonly CraftingRepository _repository;

    // Virtual field station ID for Einbui crafting
    private const int FIELD_STATION_ID = 100;

    // Einbui specialization ID (from v0.27.2)
    private const int EINBUI_SPECIALIZATION_ID = 27002;

    public EinbuiCraftingHandler(
        AdvancedCraftingService craftingService,
        RecipeService recipeService,
        CraftingRepository repository)
    {
        _craftingService = craftingService;
        _recipeService = recipeService;
        _repository = repository;
        _log.Debug("EinbuiCraftingHandler initialized");
    }

    /// <summary>
    /// Check if character can field craft (has Einbui specialization)
    /// </summary>
    public bool CanFieldCraft(int characterId)
    {
        // Note: This would query Character_Specializations table when available
        // For now, return false - to be implemented when specialization system is integrated
        _log.Debug("Checking field craft capability for character {CharacterId}", characterId);

        // TODO: Implement specialization check when Character_Specializations table exists
        // For now, assume any character can attempt (will be restricted by recipe availability)
        return true;
    }

    /// <summary>
    /// Get field-craftable recipes (consumables only, Basic tier or Field_Station)
    /// </summary>
    public List<CraftableRecipe> GetFieldCraftableRecipes(int characterId)
    {
        _log.Debug("Getting field craftable recipes for character {CharacterId}", characterId);

        if (!CanFieldCraft(characterId))
        {
            _log.Information("Character {CharacterId} cannot field craft (missing Einbui specialization)",
                characterId);
            return new List<CraftableRecipe>();
        }

        var knownRecipes = _recipeService.GetKnownRecipes(characterId);

        // Filter to field-craftable only: Consumables that are Basic tier OR require Field_Station
        var fieldRecipes = knownRecipes
            .Where(r => r.CraftedItemType == "Consumable" &&
                       (r.RequiredStation == "Field_Station" || r.RecipeTier == "Basic"))
            .ToList();

        // Check component availability for each
        var craftable = new List<CraftableRecipe>();
        foreach (var recipe in fieldRecipes)
        {
            bool hasComponents = CheckComponentAvailability(characterId, recipe.RequiredComponents);

            craftable.Add(new CraftableRecipe
            {
                Recipe = recipe,
                Components = recipe.RequiredComponents,
                CanCraft = hasComponents
            });
        }

        _log.Information("Found {Total} field craftable recipes, {Available} with components for character {CharacterId}",
            craftable.Count, craftable.Count(c => c.CanCraft), characterId);

        return craftable;
    }

    /// <summary>
    /// Craft item in field (Einbui ability)
    /// </summary>
    public CraftedItemResult FieldCraft(int characterId, int recipeId)
    {
        _log.Information("Field craft: Character={CharacterId}, Recipe={RecipeId}",
            characterId, recipeId);

        try
        {
            // Validate Einbui specialization
            if (!CanFieldCraft(characterId))
            {
                return CraftedItemResult.FailureResult(
                    "Field crafting requires the Einbui specialization");
            }

            // Verify recipe is field-craftable
            var recipe = _repository.GetRecipeById(recipeId);
            if (recipe == null)
            {
                return CraftedItemResult.FailureResult("Recipe not found");
            }

            if (recipe.CraftedItemType != "Consumable" ||
                (recipe.RequiredStation != "Field_Station" && recipe.RecipeTier != "Basic"))
            {
                return CraftedItemResult.FailureResult(
                    "This recipe cannot be field crafted. Only Basic consumables or Field_Station recipes are allowed.");
            }

            // Get or create virtual field station
            var fieldStation = GetOrCreateFieldStation();

            // Use virtual field station for crafting
            var result = _craftingService.CraftItem(characterId, recipeId, fieldStation.StationId);

            if (result.Success)
            {
                _log.Information("Field crafted {ItemName} for character {CharacterId}",
                    result.CraftedItemName, characterId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to field craft: Character={CharacterId}, Recipe={RecipeId}",
                characterId, recipeId);
            throw;
        }
    }

    #region Helper Methods

    /// <summary>
    /// Check if player has required components
    /// </summary>
    private bool CheckComponentAvailability(int characterId, List<RecipeComponent> components)
    {
        var playerComponents = _repository.GetPlayerComponents(characterId);

        foreach (var required in components)
        {
            int available = playerComponents
                .Where(pc => pc.ItemId == required.ComponentItemId &&
                            pc.QualityTier >= required.MinimumQuality)
                .Sum(pc => pc.Quantity);

            if (available < required.QuantityRequired)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get or create virtual field station for Einbui crafting
    /// </summary>
    private CraftingStation GetOrCreateFieldStation()
    {
        // Try to get existing field station
        var station = _repository.GetStationById(FIELD_STATION_ID);

        if (station != null)
            return station;

        // Return a virtual field station (not persisted to database)
        // This allows field crafting without requiring database modifications
        return new CraftingStation
        {
            StationId = FIELD_STATION_ID,
            StationType = "Field_Station",
            StationName = "Field Crafting (Einbui)",
            MaxQualityTier = 2, // Field crafting limited to Tier 2
            LocationSectorId = null, // Portable
            LocationRoomId = null,
            RequiresControlling = false,
            UsageCostCredits = 0,
            StationDescription = "Portable field crafting enabled by Einbui specialization"
        };
    }

    #endregion
}
