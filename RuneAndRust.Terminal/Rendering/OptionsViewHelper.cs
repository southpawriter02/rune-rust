using RuneAndRust.Core.Enums;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper class for Options screen formatting and styling (v0.3.10b).
/// Provides methods for rendering sliders, toggles, and enum display names.
/// </summary>
public static class OptionsViewHelper
{
    /// <summary>
    /// Renders a visual slider bar for percentage or range values.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="width">The width of the slider in characters (default 20).</param>
    /// <returns>A Spectre.Console markup string with filled and empty segments.</returns>
    public static string RenderSlider(int value, int min, int max, int width = 20)
    {
        // Clamp value to valid range
        value = Math.Clamp(value, min, max);

        // Calculate ratio and filled blocks
        var ratio = (double)(value - min) / (max - min);
        var filled = (int)(width * ratio);
        var empty = width - filled;

        // Build the slider string
        var filledStr = new string('█', filled);
        var emptyStr = new string('░', empty);

        return $"[green]{filledStr}[/][grey]{emptyStr}[/]";
    }

    /// <summary>
    /// Formats a boolean toggle value with color.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>Green "ON" or grey "OFF" markup string.</returns>
    public static string FormatToggle(bool value)
        => value ? "[green]ON[/]" : "[grey]OFF[/]";

    /// <summary>
    /// Gets the display name for a ThemeType enum value.
    /// </summary>
    /// <param name="themeValue">The theme as an integer value.</param>
    /// <returns>Human-readable theme name.</returns>
    public static string GetThemeName(int themeValue)
        => ((ThemeType)themeValue) switch
        {
            ThemeType.Standard => "Standard",
            ThemeType.HighContrast => "High Contrast",
            ThemeType.Protanopia => "Protanopia",
            ThemeType.Deuteranopia => "Deuteranopia",
            ThemeType.Tritanopia => "Tritanopia",
            _ => "Unknown"
        };

    /// <summary>
    /// Gets the display name for an OptionsTab enum value.
    /// </summary>
    /// <param name="tab">The tab enum value.</param>
    /// <returns>Human-readable tab name.</returns>
    public static string GetTabDisplayName(OptionsTab tab)
        => tab switch
        {
            OptionsTab.General => "General",
            OptionsTab.Display => "Display",
            OptionsTab.Audio => "Audio",
            OptionsTab.Controls => "Controls",
            _ => tab.ToString()
        };

    /// <summary>
    /// Formats a slider value with its unit suffix.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="propertyName">The property name to determine the unit.</param>
    /// <returns>Formatted value with unit (e.g., "50%", "5 min").</returns>
    public static string FormatSliderValue(int value, string? propertyName)
        => propertyName switch
        {
            "AutosaveIntervalMinutes" => $"{value} min",
            "TextSpeed" or "MasterVolume" => $"{value}%",
            _ => value.ToString()
        };

    /// <summary>
    /// Gets the next theme value in the cycle.
    /// </summary>
    /// <param name="currentTheme">The current theme integer value.</param>
    /// <param name="direction">1 for next, -1 for previous.</param>
    /// <returns>The next/previous theme integer value, wrapping around.</returns>
    public static int CycleTheme(int currentTheme, int direction)
    {
        var themeCount = Enum.GetValues<ThemeType>().Length;
        var next = (currentTheme + direction + themeCount) % themeCount;
        return next;
    }

    /// <summary>
    /// Gets the human-readable display name for a command string (v0.3.10c).
    /// </summary>
    /// <param name="command">The internal command string.</param>
    /// <returns>Human-readable action name.</returns>
    public static string GetCommandDisplayName(string command)
        => command switch
        {
            "north" => "Move North",
            "south" => "Move South",
            "east" => "Move East",
            "west" => "Move West",
            "up" => "Move Up",
            "down" => "Move Down",
            "confirm" => "Confirm",
            "cancel" => "Cancel/Back",
            "menu" => "Menu",
            "help" => "Help",
            "inventory" => "Inventory",
            "character" => "Character",
            "journal" => "Journal",
            "bench" => "Crafting",
            "interact" => "Interact",
            "look" => "Look",
            "search" => "Search",
            "wait" => "Wait",
            "attack" => "Attack",
            "light" => "Light Attack",
            "heavy" => "Heavy Attack",
            _ => command
        };

    /// <summary>
    /// Gets the category for a command string (v0.3.10c).
    /// </summary>
    /// <param name="command">The internal command string.</param>
    /// <returns>Category name for grouping.</returns>
    public static string GetCommandCategory(string command)
        => command switch
        {
            "north" or "south" or "east" or "west" or "up" or "down" => "Movement",
            "confirm" or "cancel" or "menu" or "help" => "Core",
            "inventory" or "character" or "journal" or "bench" => "Screens",
            "interact" or "look" or "search" or "wait" => "Gameplay",
            "attack" or "light" or "heavy" => "Combat",
            _ => "Other"
        };

    /// <summary>
    /// Formats a ConsoleKey for display with Spectre.Console markup (v0.3.10c).
    /// </summary>
    /// <param name="key">The console key, or null if unbound.</param>
    /// <returns>Formatted key display string.</returns>
    public static string FormatKeyName(ConsoleKey? key)
        => key switch
        {
            null => "[red][Unbound][/]",
            ConsoleKey.Spacebar => "[cyan]Space[/]",
            ConsoleKey.Enter => "[cyan]Enter[/]",
            ConsoleKey.Escape => "[cyan]Esc[/]",
            ConsoleKey.Tab => "[cyan]Tab[/]",
            ConsoleKey.Backspace => "[cyan]Backspace[/]",
            ConsoleKey.Delete => "[cyan]Delete[/]",
            ConsoleKey.Insert => "[cyan]Insert[/]",
            ConsoleKey.Home => "[cyan]Home[/]",
            ConsoleKey.End => "[cyan]End[/]",
            ConsoleKey.PageUp => "[cyan]PgUp[/]",
            ConsoleKey.PageDown => "[cyan]PgDn[/]",
            ConsoleKey.UpArrow => "[cyan]↑[/]",
            ConsoleKey.DownArrow => "[cyan]↓[/]",
            ConsoleKey.LeftArrow => "[cyan]←[/]",
            ConsoleKey.RightArrow => "[cyan]→[/]",
            _ => $"[cyan]{key}[/]"
        };
}
