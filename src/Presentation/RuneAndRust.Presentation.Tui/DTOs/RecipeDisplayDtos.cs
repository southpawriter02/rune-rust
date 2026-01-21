// ═══════════════════════════════════════════════════════════════════════════════
// RecipeDisplayDtos.cs
// Data transfer objects for recipe browser, details, and notifications.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for recipe browser display.
/// </summary>
/// <param name="RecipeId">The recipe's unique identifier.</param>
/// <param name="Name">The display name of the recipe.</param>
/// <param name="Description">The recipe description.</param>
/// <param name="Category">The recipe category.</param>
/// <param name="StationName">The required crafting station name.</param>
/// <param name="IsCraftable">Whether the player can craft this recipe.</param>
/// <param name="IsNew">Whether the recipe was recently discovered.</param>
/// <remarks>
/// Used by the RecipeBrowser to display recipes in a searchable, filterable list.
/// </remarks>
public record RecipeBrowserDisplayDto(
    string RecipeId,
    string Name,
    string Description,
    RecipeCategory Category,
    string StationName,
    bool IsCraftable,
    bool IsNew);

/// <summary>
/// Data transfer object for recipe details display.
/// </summary>
/// <param name="RecipeId">The recipe's unique identifier.</param>
/// <param name="Name">The display name of the recipe.</param>
/// <param name="Description">The recipe description.</param>
/// <param name="Category">The recipe category.</param>
/// <param name="Materials">The required materials.</param>
/// <param name="OutputItemId">The ID of the output item.</param>
/// <param name="OutputItemName">The display name of the output item.</param>
/// <param name="OutputQuantity">The quantity produced.</param>
/// <param name="QualityRange">Description of possible quality range.</param>
/// <param name="ExpectedQuality">The expected quality based on player skill.</param>
/// <param name="IsCraftable">Whether the player can craft this recipe.</param>
/// <remarks>
/// Used by the RecipeDetails component to show full recipe information
/// including material requirements and expected output.
/// </remarks>
public record RecipeDetailsDisplayDto(
    string RecipeId,
    string Name,
    string Description,
    RecipeCategory Category,
    IReadOnlyList<MaterialRequirementDto> Materials,
    string OutputItemId,
    string OutputItemName,
    int OutputQuantity,
    string QualityRange,
    ItemQuality ExpectedQuality,
    bool IsCraftable);

/// <summary>
/// Data transfer object for recipe book entry.
/// </summary>
/// <param name="RecipeId">The recipe's unique identifier.</param>
/// <param name="Name">The display name of the recipe.</param>
/// <param name="StationId">The required crafting station ID.</param>
/// <param name="StationName">The required crafting station name.</param>
/// <param name="Category">The recipe category.</param>
/// <param name="DiscoveredAt">When the recipe was discovered.</param>
/// <remarks>
/// Used by the RecipeBook to display discovered recipes organized by station.
/// </remarks>
public record RecipeBookEntryDto(
    string RecipeId,
    string Name,
    string StationId,
    string StationName,
    RecipeCategory Category,
    DateTime DiscoveredAt);

/// <summary>
/// Data transfer object for recipe discovery notification.
/// </summary>
/// <param name="RecipeId">The recipe's unique identifier.</param>
/// <param name="Name">The display name of the recipe.</param>
/// <param name="StationName">The required crafting station name.</param>
/// <remarks>
/// Used by the RecipeDiscoveryNotification to show newly discovered recipes.
/// </remarks>
public record RecipeDiscoveryDto(
    string RecipeId,
    string Name,
    string StationName);

/// <summary>
/// Data transfer object for crafting result notification.
/// </summary>
/// <param name="ItemId">The crafted item's ID.</param>
/// <param name="ItemName">The crafted item's name.</param>
/// <param name="Quantity">The quantity crafted.</param>
/// <param name="Quality">The quality tier achieved.</param>
/// <remarks>
/// Used by the CraftingResultNotification to show crafted item quality.
/// </remarks>
public record CraftingResultDto(
    string ItemId,
    string ItemName,
    int Quantity,
    ItemQuality Quality);

/// <summary>
/// Data transfer object for quality result display.
/// </summary>
/// <param name="Quality">The quality tier.</param>
/// <param name="QualityName">The display name of the quality.</param>
/// <param name="Stars">The star rating string.</param>
/// <remarks>
/// Used for displaying quality information in various UI contexts.
/// </remarks>
public record QualityResultDto(
    ItemQuality Quality,
    string QualityName,
    string Stars);
