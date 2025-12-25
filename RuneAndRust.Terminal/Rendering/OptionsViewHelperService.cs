using RuneAndRust.Core.Constants;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Service for Options screen formatting and styling (v0.3.15a - The Lexicon).
/// Provides methods for rendering sliders, toggles, and localized enum display names.
/// Converted from static class to support ILocalizationService injection.
/// </summary>
/// <remarks>See: SPEC-LOC-001 for Localization System design.</remarks>
public class OptionsViewHelperService
{
    private readonly ILocalizationService _loc;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsViewHelperService"/> class.
    /// </summary>
    /// <param name="localizationService">The localization service for string lookup.</param>
    public OptionsViewHelperService(ILocalizationService localizationService)
    {
        _loc = localizationService;
    }

    /// <summary>
    /// Renders a visual slider bar for percentage or range values.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="width">The width of the slider in characters (default 20).</param>
    /// <returns>A Spectre.Console markup string with filled and empty segments.</returns>
    public string RenderSlider(int value, int min, int max, int width = 20)
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
    /// Formats a boolean toggle value with localized text and color.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>Green localized "ON" or grey localized "OFF" markup string.</returns>
    public string FormatToggle(bool value)
        => value
            ? $"[green]{_loc.Get(LocKeys.UI_Options_Toggle_On)}[/]"
            : $"[grey]{_loc.Get(LocKeys.UI_Options_Toggle_Off)}[/]";

    /// <summary>
    /// Gets the localized display name for a ThemeType enum value.
    /// </summary>
    /// <param name="themeValue">The theme as an integer value.</param>
    /// <returns>Localized theme name.</returns>
    public string GetThemeName(int themeValue)
        => ((ThemeType)themeValue) switch
        {
            ThemeType.Standard => _loc.Get(LocKeys.UI_Options_Theme_Standard),
            ThemeType.HighContrast => _loc.Get(LocKeys.UI_Options_Theme_HighContrast),
            ThemeType.Protanopia => _loc.Get(LocKeys.UI_Options_Theme_Protanopia),
            ThemeType.Deuteranopia => _loc.Get(LocKeys.UI_Options_Theme_Deuteranopia),
            ThemeType.Tritanopia => _loc.Get(LocKeys.UI_Options_Theme_Tritanopia),
            _ => _loc.Get(LocKeys.UI_Options_Theme_Unknown)
        };

    /// <summary>
    /// Gets the localized display name for an OptionsTab enum value.
    /// </summary>
    /// <param name="tab">The tab enum value.</param>
    /// <returns>Localized tab name.</returns>
    public string GetTabDisplayName(OptionsTab tab)
        => tab switch
        {
            OptionsTab.General => _loc.Get(LocKeys.UI_Options_Tab_General),
            OptionsTab.Display => _loc.Get(LocKeys.UI_Options_Tab_Display),
            OptionsTab.Audio => _loc.Get(LocKeys.UI_Options_Tab_Audio),
            OptionsTab.Controls => _loc.Get(LocKeys.UI_Options_Tab_Controls),
            _ => tab.ToString()
        };

    /// <summary>
    /// Formats a slider value with its localized unit suffix.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="propertyName">The property name to determine the unit.</param>
    /// <returns>Formatted value with localized unit (e.g., "50%", "5 min").</returns>
    public string FormatSliderValue(int value, string? propertyName)
        => propertyName switch
        {
            "AutosaveIntervalMinutes" => _loc.Get(LocKeys.UI_Options_Unit_Minutes, value),
            "TextSpeed" or "MasterVolume" => _loc.Get(LocKeys.UI_Options_Unit_Percent, value),
            _ => value.ToString()
        };

    /// <summary>
    /// Gets the next theme value in the cycle.
    /// </summary>
    /// <param name="currentTheme">The current theme integer value.</param>
    /// <param name="direction">1 for next, -1 for previous.</param>
    /// <returns>The next/previous theme integer value, wrapping around.</returns>
    public int CycleTheme(int currentTheme, int direction)
    {
        var themeCount = Enum.GetValues<ThemeType>().Length;
        var next = (currentTheme + direction + themeCount) % themeCount;
        return next;
    }

    /// <summary>
    /// Gets the localized display name for a command string.
    /// </summary>
    /// <param name="command">The internal command string.</param>
    /// <returns>Localized action name.</returns>
    public string GetCommandDisplayName(string command)
        => command switch
        {
            "north" => _loc.Get(LocKeys.UI_Options_Command_MoveNorth),
            "south" => _loc.Get(LocKeys.UI_Options_Command_MoveSouth),
            "east" => _loc.Get(LocKeys.UI_Options_Command_MoveEast),
            "west" => _loc.Get(LocKeys.UI_Options_Command_MoveWest),
            "up" => _loc.Get(LocKeys.UI_Options_Command_MoveUp),
            "down" => _loc.Get(LocKeys.UI_Options_Command_MoveDown),
            "confirm" => _loc.Get(LocKeys.UI_Options_Command_Confirm),
            "cancel" => _loc.Get(LocKeys.UI_Options_Command_Cancel),
            "menu" => _loc.Get(LocKeys.UI_Options_Command_Menu),
            "help" => _loc.Get(LocKeys.UI_Options_Command_Help),
            "inventory" => _loc.Get(LocKeys.UI_Options_Command_Inventory),
            "character" => _loc.Get(LocKeys.UI_Options_Command_Character),
            "journal" => _loc.Get(LocKeys.UI_Options_Command_Journal),
            "bench" => _loc.Get(LocKeys.UI_Options_Command_Crafting),
            "interact" => _loc.Get(LocKeys.UI_Options_Command_Interact),
            "look" => _loc.Get(LocKeys.UI_Options_Command_Look),
            "search" => _loc.Get(LocKeys.UI_Options_Command_Search),
            "wait" => _loc.Get(LocKeys.UI_Options_Command_Wait),
            "attack" => _loc.Get(LocKeys.UI_Options_Command_Attack),
            "light" => _loc.Get(LocKeys.UI_Options_Command_LightAttack),
            "heavy" => _loc.Get(LocKeys.UI_Options_Command_HeavyAttack),
            _ => command
        };

    /// <summary>
    /// Gets the localized category for a command string.
    /// </summary>
    /// <param name="command">The internal command string.</param>
    /// <returns>Localized category name for grouping.</returns>
    public string GetCommandCategory(string command)
        => command switch
        {
            "north" or "south" or "east" or "west" or "up" or "down" => _loc.Get(LocKeys.UI_Options_Category_Movement),
            "confirm" or "cancel" or "menu" or "help" => _loc.Get(LocKeys.UI_Options_Category_Core),
            "inventory" or "character" or "journal" or "bench" => _loc.Get(LocKeys.UI_Options_Category_Screens),
            "interact" or "look" or "search" or "wait" => _loc.Get(LocKeys.UI_Options_Category_Gameplay),
            "attack" or "light" or "heavy" => _loc.Get(LocKeys.UI_Options_Category_Combat),
            _ => _loc.Get(LocKeys.UI_Options_Category_Other)
        };

    /// <summary>
    /// Formats a ConsoleKey for display with localized key names and Spectre.Console markup.
    /// </summary>
    /// <param name="key">The console key, or null if unbound.</param>
    /// <returns>Formatted localized key display string.</returns>
    public string FormatKeyName(ConsoleKey? key)
        => key switch
        {
            null => $"[red]{_loc.Get(LocKeys.UI_Options_Key_Unbound)}[/]",
            ConsoleKey.Spacebar => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_Space)}[/]",
            ConsoleKey.Enter => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_Enter)}[/]",
            ConsoleKey.Escape => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_Escape)}[/]",
            ConsoleKey.Tab => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_Tab)}[/]",
            ConsoleKey.Backspace => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_Backspace)}[/]",
            ConsoleKey.Delete => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_Delete)}[/]",
            ConsoleKey.Insert => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_Insert)}[/]",
            ConsoleKey.Home => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_Home)}[/]",
            ConsoleKey.End => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_End)}[/]",
            ConsoleKey.PageUp => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_PageUp)}[/]",
            ConsoleKey.PageDown => $"[cyan]{_loc.Get(LocKeys.UI_Options_Key_PageDown)}[/]",
            ConsoleKey.UpArrow => "[cyan]↑[/]",
            ConsoleKey.DownArrow => "[cyan]↓[/]",
            ConsoleKey.LeftArrow => "[cyan]←[/]",
            ConsoleKey.RightArrow => "[cyan]→[/]",
            _ => $"[cyan]{key}[/]"
        };
}
