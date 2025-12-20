using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Crafting;

/// <summary>
/// Represents the complete result of a repair attempt.
/// Contains all details about the roll, outcome, and durability changes.
/// </summary>
/// <param name="IsSuccess">Whether the repair restored any durability.</param>
/// <param name="Outcome">The outcome category (Failure, Success, Masterwork, Catastrophe).</param>
/// <param name="ItemName">The name of the item that was repaired.</param>
/// <param name="DiceRolled">Number of d10 dice rolled (equal to character's WITS).</param>
/// <param name="Successes">Number of dice that rolled 8 or higher.</param>
/// <param name="Botches">Number of dice that rolled 1.</param>
/// <param name="NetSuccesses">Successes minus botches.</param>
/// <param name="DifficultyClass">The DC that needed to be met for success.</param>
/// <param name="DurabilityRestored">The amount of durability restored (0 on failure/catastrophe).</param>
/// <param name="ScrapConsumed">The amount of Scrap consumed for the repair attempt.</param>
/// <param name="MaxDurabilityLost">The permanent MaxDurability reduction on catastrophe, or null.</param>
/// <param name="Message">A descriptive message about the repair result.</param>
/// <param name="Rolls">The individual die roll values.</param>
public record RepairResult(
    bool IsSuccess,
    CraftingOutcome Outcome,
    string ItemName,
    int DiceRolled,
    int Successes,
    int Botches,
    int NetSuccesses,
    int DifficultyClass,
    int DurabilityRestored,
    int ScrapConsumed,
    int? MaxDurabilityLost,
    string Message,
    IReadOnlyList<int> Rolls
);
