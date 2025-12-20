namespace RuneAndRust.Core.Models.Crafting;

/// <summary>
/// Represents the result of a salvage operation.
/// Contains details about the destroyed item and Scrap yield.
/// </summary>
/// <param name="IsSuccess">Whether the salvage operation completed successfully.</param>
/// <param name="ItemName">The name of the item that was salvaged.</param>
/// <param name="ScrapYield">The amount of Scrap obtained from salvaging.</param>
/// <param name="Message">A descriptive message about the salvage result.</param>
public record SalvageResult(
    bool IsSuccess,
    string ItemName,
    int ScrapYield,
    string Message
);
