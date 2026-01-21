// ═══════════════════════════════════════════════════════════════════════════════
// CraftingDisplayDtos.cs
// Data transfer objects for crafting station interface display.
// Version: 0.13.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for crafting station display.
/// </summary>
/// <remarks>
/// Contains station information for rendering in the crafting station menu.
/// Used by CraftingStationMenu to display station header and load recipes.
/// </remarks>
/// <param name="StationId">The station's unique identifier.</param>
/// <param name="Name">The display name of the station (e.g., "Blacksmith Forge").</param>
/// <param name="Description">The station description.</param>
/// <param name="StationType">The type of crafting station (e.g., "Forge", "Alchemy").</param>
/// <param name="SkillRequired">The skill required to use this station, if any.</param>
/// <example>
/// <code>
/// var stationDto = new CraftingStationDisplayDto(
///     StationId: "forge-1",
///     Name: "Blacksmith Forge",
///     Description: "A well-used forge for metalworking",
///     StationType: "Forge",
///     SkillRequired: "Smithing");
/// </code>
/// </example>
public record CraftingStationDisplayDto(
    string StationId,
    string Name,
    string Description,
    string StationType,
    string? SkillRequired);

/// <summary>
/// Data transfer object for station recipe display.
/// </summary>
/// <remarks>
/// Represents a recipe in the crafting station menu with craftability status
/// and material requirements for display purposes.
/// </remarks>
/// <param name="RecipeId">The recipe's unique identifier.</param>
/// <param name="Name">The display name of the recipe.</param>
/// <param name="Description">The recipe description.</param>
/// <param name="Materials">The required materials with availability info.</param>
/// <param name="OutputItemId">The ID of the output item.</param>
/// <param name="OutputItemName">The display name of the output item.</param>
/// <param name="OutputQuantity">The quantity produced.</param>
/// <param name="IsCraftable">Whether the player can craft this recipe.</param>
/// <example>
/// <code>
/// var recipeDto = new StationRecipeDisplayDto(
///     RecipeId: "steel-blade",
///     Name: "Steel Blade",
///     Description: "A sturdy steel blade",
///     Materials: materialList,
///     OutputItemId: "steel-blade-item",
///     OutputItemName: "Steel Blade",
///     OutputQuantity: 1,
///     IsCraftable: true);
/// </code>
/// </example>
public record StationRecipeDisplayDto(
    string RecipeId,
    string Name,
    string Description,
    IReadOnlyList<MaterialRequirementDto> Materials,
    string OutputItemId,
    string OutputItemName,
    int OutputQuantity,
    bool IsCraftable);

/// <summary>
/// Data transfer object for material requirements.
/// </summary>
/// <remarks>
/// Shows required vs owned quantities for material availability display.
/// Used to determine craftability and highlight missing materials.
/// </remarks>
/// <param name="MaterialId">The material's unique identifier (e.g., "iron-ore").</param>
/// <param name="MaterialName">The display name of the material (e.g., "Iron Ore").</param>
/// <param name="RequiredQuantity">The quantity required for crafting.</param>
/// <param name="OwnedQuantity">The quantity currently owned by the player.</param>
/// <param name="IsOptional">Whether the material is optional for the recipe.</param>
/// <example>
/// <code>
/// var materialDto = new MaterialRequirementDto(
///     MaterialId: "iron-ore",
///     MaterialName: "Iron Ore",
///     RequiredQuantity: 2,
///     OwnedQuantity: 24,
///     IsOptional: false);
/// </code>
/// </example>
public record MaterialRequirementDto(
    string MaterialId,
    string MaterialName,
    int RequiredQuantity,
    int OwnedQuantity,
    bool IsOptional);

/// <summary>
/// Data transfer object for crafting progress.
/// </summary>
/// <remarks>
/// Tracks the progress of an active crafting operation for progress bar display.
/// </remarks>
/// <param name="RecipeId">The recipe being crafted.</param>
/// <param name="RecipeName">The display name of the recipe.</param>
/// <param name="Progress">The progress value (0.0 to 1.0).</param>
/// <param name="IsComplete">Whether crafting is complete.</param>
/// <example>
/// <code>
/// var progressDto = new CraftingProgressDto(
///     RecipeId: "steel-blade",
///     RecipeName: "Steel Blade",
///     Progress: 0.75f,
///     IsComplete: false);
/// </code>
/// </example>
public record CraftingProgressDto(
    string RecipeId,
    string RecipeName,
    float Progress,
    bool IsComplete);
