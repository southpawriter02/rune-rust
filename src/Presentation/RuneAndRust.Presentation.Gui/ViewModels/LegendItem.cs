namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Legend item for ASCII art display.
/// </summary>
/// <param name="Symbol">The legend symbol character.</param>
/// <param name="Description">What the symbol represents.</param>
public record LegendItem(string Symbol, string Description);
