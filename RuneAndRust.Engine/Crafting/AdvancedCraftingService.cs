using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// v0.36.2: Advanced Crafting Service
/// Implements comprehensive crafting mechanics with quality calculation,
/// station validation, and component consumption
/// </summary>
public class AdvancedCraftingService
{
    private static readonly ILogger _log = Log.ForContext<AdvancedCraftingService>();
    private readonly CraftingRepository _repository;
    private readonly DiceService _diceService;

    public AdvancedCraftingService(CraftingRepository repository, DiceService? diceService = null)
    {
        _repository = repository;
        _diceService = diceService ?? new DiceService();
        _log.Debug("AdvancedCraftingService initialized");
    }

    /// <summary>
    /// Attempt to craft an item using a recipe at a specific station
    /// </summary>
    public CraftedItemResult CraftItem(
        PlayerCharacter character,
        int recipeId,
        int stationId)
    {
        _log.Information("Crafting attempt: Character={CharacterId}, Recipe={RecipeId}, Station={StationId}",
            character.CharacterID, recipeId, stationId);

        // Load recipe
        var recipe = _repository.GetRecipeById(recipeId);
        if (recipe == null)
        {
            return FailureResult("Recipe not found.");
        }

        // Load station
        var station = _repository.GetStationById(stationId);
        if (station == null)
        {
            return FailureResult("Crafting station not found.");
        }

        // Validate station type matches recipe requirement
        if (!ValidateStation(recipe, station, out string? stationError))
        {
            return FailureResult(stationError!);
        }

        // Get player's component inventory
        var playerComponents = _repository.GetPlayerComponents(character.CharacterID);

        // Validate player has required components
        if (!ValidateComponents(recipe, playerComponents, out var componentValidation))
        {
            return FailureResult(componentValidation.ErrorMessage);
        }

        // Perform skill check
        int attributeValue = character.GetAttributeValue(recipe.SkillAttribute);
        var skillCheck = _diceService.Roll(attributeValue);
        int rollTotal = skillCheck.TotalValue;
        int dc = recipe.SkillCheckDC;
        bool skillCheckPassed = rollTotal >= dc;

        _log.Debug("Skill check: {Attribute}={Value}, Roll={Roll}, DC={DC}, Result={Result}",
            recipe.SkillAttribute, attributeValue, rollTotal, dc, skillCheckPassed ? "PASS" : "FAIL");

        // Consume components (happens on both success and failure)
        bool consumed = _repository.ConsumeComponents(character.CharacterID, componentValidation.ComponentsToConsume);
        if (!consumed)
        {
            _log.Error("Failed to consume components - rolling back crafting attempt");
            return FailureResult("Failed to consume components from inventory.");
        }

        // If skill check failed, return failure (components already consumed)
        if (!skillCheckPassed)
        {
            _log.Information("Crafting failed: Skill check failed ({Roll} vs DC {DC})", rollTotal, dc);
            return new CraftedItemResult
            {
                Success = false,
                Message = $"Crafting failed! Your skill check of {rollTotal} did not meet the DC of {dc}.\nComponents were consumed in the failed attempt.",
                SkillCheckRoll = rollTotal,
                SkillCheckDC = dc,
                SkillCheckPassed = false,
                ConsumedComponents = componentValidation.ComponentsToConsume
            };
        }

        // Calculate quality
        var qualityCalc = CalculateQuality(
            componentValidation.LowestComponentQuality,
            station.MaxQualityTier,
            recipe.QualityBonus);

        _log.Debug("Quality calculation: {Calculation}", qualityCalc.ToString());

        // Add crafted item to inventory
        // Note: For now, using a placeholder item_id. In v0.36.4 this will be linked to actual Items table
        int craftedItemId = GetCraftedItemId(recipe);
        bool itemAdded = _repository.AddCraftedItem(
            character.CharacterID,
            craftedItemId,
            qualityCalc.FinalQuality,
            quantity: 1);

        if (!itemAdded)
        {
            _log.Error("Failed to add crafted item to inventory");
            return FailureResult("Crafting succeeded but failed to add item to inventory.");
        }

        _log.Information("Crafting succeeded: Recipe={RecipeName}, Quality={Quality}",
            recipe.RecipeName, qualityCalc.FinalQuality);

        return new CraftedItemResult
        {
            Success = true,
            Message = $"Successfully crafted {recipe.RecipeName}!\nQuality: {GetQualityName(qualityCalc.FinalQuality)}\nSkill check: {rollTotal} vs DC {dc}",
            CraftedItemId = craftedItemId,
            CraftedItemName = recipe.RecipeName,
            FinalQuality = qualityCalc.FinalQuality,
            SkillCheckRoll = rollTotal,
            SkillCheckDC = dc,
            SkillCheckPassed = true,
            ConsumedComponents = componentValidation.ComponentsToConsume,
            QualityCalculation = qualityCalc
        };
    }

    /// <summary>
    /// Validate that the station is appropriate for the recipe
    /// </summary>
    private bool ValidateStation(Recipe recipe, CraftingStation station, out string? errorMessage)
    {
        // Check if station type matches (or recipe allows "Any")
        if (recipe.RequiredStation != "Any" && recipe.RequiredStation != station.StationType)
        {
            errorMessage = $"This recipe requires a {recipe.RequiredStation} station, but {station.StationName} is a {station.StationType}.";
            _log.Debug("Station validation failed: Required={Required}, Actual={Actual}",
                recipe.RequiredStation, station.StationType);
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validate player has required components with sufficient quality
    /// </summary>
    private bool ValidateComponents(
        Recipe recipe,
        List<PlayerComponent> playerComponents,
        out ComponentValidation validation)
    {
        var componentsToConsume = new List<ConsumedComponent>();
        int lowestQuality = 5; // Start with max quality
        var missingComponents = new List<string>();

        foreach (var required in recipe.RequiredComponents)
        {
            if (required.IsOptional)
                continue; // Skip optional components for now

            // Find player components that match this requirement
            var availableComponents = playerComponents
                .Where(pc => pc.ItemId == required.ComponentItemId &&
                            pc.QualityTier >= required.MinimumQuality &&
                            pc.Quantity > 0)
                .OrderByDescending(pc => pc.QualityTier) // Prefer higher quality
                .ToList();

            int totalAvailable = availableComponents.Sum(c => c.Quantity);

            if (totalAvailable < required.QuantityRequired)
            {
                missingComponents.Add($"{required.ComponentName} (need {required.QuantityRequired}, have {totalAvailable})");
                continue;
            }

            // Allocate components to consume (highest quality first)
            int remaining = required.QuantityRequired;
            foreach (var component in availableComponents)
            {
                if (remaining <= 0)
                    break;

                int toConsume = Math.Min(remaining, component.Quantity);
                componentsToConsume.Add(new ConsumedComponent
                {
                    ItemId = component.ItemId,
                    ItemName = component.ItemName,
                    Quantity = toConsume,
                    QualityTier = component.QualityTier
                });

                // Track lowest quality used
                lowestQuality = Math.Min(lowestQuality, component.QualityTier);
                remaining -= toConsume;
            }
        }

        if (missingComponents.Any())
        {
            validation = new ComponentValidation
            {
                IsValid = false,
                ErrorMessage = $"Insufficient components:\n{string.Join("\n", missingComponents)}",
                ComponentsToConsume = new List<ConsumedComponent>(),
                LowestComponentQuality = 0
            };
            return false;
        }

        validation = new ComponentValidation
        {
            IsValid = true,
            ErrorMessage = string.Empty,
            ComponentsToConsume = componentsToConsume,
            LowestComponentQuality = lowestQuality
        };
        return true;
    }

    /// <summary>
    /// Calculate final quality based on components, station, and recipe bonus
    /// Formula: min(lowestComponentQuality, stationMaxQuality) + recipeBonus, clamped to 1-4
    /// </summary>
    private QualityCalculation CalculateQuality(
        int lowestComponentQuality,
        int stationMaxQuality,
        int recipeBonus)
    {
        int baseQuality = Math.Min(lowestComponentQuality, stationMaxQuality);
        int finalQuality = baseQuality + recipeBonus;
        finalQuality = Math.Clamp(finalQuality, 1, 4); // Crafted items are quality 1-4

        return new QualityCalculation
        {
            LowestComponentQuality = lowestComponentQuality,
            StationMaxQuality = stationMaxQuality,
            RecipeQualityBonus = recipeBonus,
            BaseQuality = baseQuality,
            FinalQuality = finalQuality
        };
    }

    /// <summary>
    /// Get crafted item ID (placeholder - will be implemented in v0.36.4)
    /// </summary>
    private int GetCraftedItemId(Recipe recipe)
    {
        // TODO v0.36.4: Map recipe to actual item_id in Items table
        // For now, use a placeholder based on recipe type
        return recipe.CraftedItemType switch
        {
            "Weapon" => 1000 + recipe.RecipeId,
            "Armor" => 2000 + recipe.RecipeId,
            "Consumable" => 3000 + recipe.RecipeId,
            "Inscription" => 8000 + recipe.RecipeId,
            _ => 9000 + recipe.RecipeId
        };
    }

    /// <summary>
    /// Get quality tier name for display
    /// </summary>
    private string GetQualityName(int quality)
    {
        return quality switch
        {
            1 => "Standard",
            2 => "Quality",
            3 => "Superior",
            4 => "Masterwork",
            _ => $"Tier {quality}"
        };
    }

    /// <summary>
    /// Create a failure result
    /// </summary>
    private CraftedItemResult FailureResult(string message)
    {
        return new CraftedItemResult
        {
            Success = false,
            Message = message,
            SkillCheckPassed = false
        };
    }

    /// <summary>
    /// Get list of recipes available to a character
    /// </summary>
    public List<Recipe> GetAvailableRecipes(int characterId)
    {
        return _repository.GetLearnedRecipes(characterId);
    }

    /// <summary>
    /// Get stations available at a location
    /// </summary>
    public List<CraftingStation> GetStationsBySector(int sectorId)
    {
        return _repository.GetStationsBySector(sectorId);
    }

    /// <summary>
    /// Get player's component inventory
    /// </summary>
    public List<PlayerComponent> GetPlayerComponents(int characterId)
    {
        return _repository.GetPlayerComponents(characterId);
    }
}

/// <summary>
/// Internal validation result for component checking
/// </summary>
internal class ComponentValidation
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<ConsumedComponent> ComponentsToConsume { get; set; } = new();
    public int LowestComponentQuality { get; set; }
}
