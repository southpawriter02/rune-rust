using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Crafting;

/// <summary>
/// Represents the complete result of a crafting attempt.
/// Contains all details about the roll, outcome, and any produced items.
/// </summary>
/// <param name="IsSuccess">Whether the craft produced an output item.</param>
/// <param name="Outcome">The outcome category (Failure, Success, Masterwork, Catastrophe).</param>
/// <param name="RecipeId">The ID of the recipe that was attempted.</param>
/// <param name="RecipeName">The display name of the recipe.</param>
/// <param name="DiceRolled">Number of d10 dice rolled (equal to crafter's WITS).</param>
/// <param name="Successes">Number of dice that rolled 8 or higher.</param>
/// <param name="Botches">Number of dice that rolled 1.</param>
/// <param name="NetSuccesses">Successes minus botches.</param>
/// <param name="DifficultyClass">The DC that needed to be met for success.</param>
/// <param name="OutputItemId">The ItemId of the produced item, or null if none.</param>
/// <param name="OutputQuantity">The quantity of items produced.</param>
/// <param name="OutputQuality">The quality tier of the produced item, or null if none.</param>
/// <param name="Message">A descriptive message about the crafting result.</param>
/// <param name="Rolls">The individual die roll values.</param>
public record CraftingResult(
    bool IsSuccess,
    CraftingOutcome Outcome,
    string RecipeId,
    string RecipeName,
    int DiceRolled,
    int Successes,
    int Botches,
    int NetSuccesses,
    int DifficultyClass,
    string? OutputItemId,
    int OutputQuantity,
    QualityTier? OutputQuality,
    string Message,
    IReadOnlyList<int> Rolls
);
