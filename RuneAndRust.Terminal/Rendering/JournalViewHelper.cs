using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper methods for formatting Journal UI elements (v0.3.7c).
/// Provides tab colors, completion indicators, category icons, and glitch effects.
/// Updated with IThemeService overloads in v0.3.14a.
/// </summary>
public static class JournalViewHelper
{
    #region Tab Display

    /// <summary>
    /// Gets the display name for a Journal tab.
    /// </summary>
    /// <param name="tab">The tab to get the name for.</param>
    /// <returns>Human-readable tab name.</returns>
    public static string GetTabDisplayName(JournalTab tab) => tab switch
    {
        JournalTab.Codex => "Codex",
        JournalTab.Bestiary => "Bestiary",
        JournalTab.FieldGuide => "Field Guide",
        JournalTab.Contracts => "Contracts",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the hotkey character for a Journal tab.
    /// </summary>
    /// <param name="tab">The tab to get the hotkey for.</param>
    /// <returns>Single character hotkey.</returns>
    public static char GetTabHotkey(JournalTab tab) => tab switch
    {
        JournalTab.Codex => 'C',
        JournalTab.Bestiary => 'B',
        JournalTab.FieldGuide => 'F',
        JournalTab.Contracts => 'Q',
        _ => '?'
    };

    /// <summary>
    /// Gets the themed Spectre.Console color for a tab based on active state (v0.3.14a).
    /// </summary>
    /// <param name="tab">The tab to get the color for.</param>
    /// <param name="isActive">Whether this tab is currently active.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>Color string for Spectre.Console markup.</returns>
    public static string GetTabColor(JournalTab tab, bool isActive, IThemeService theme)
    {
        if (!isActive)
        {
            return theme.GetColor("InactiveColor");
        }

        return tab switch
        {
            JournalTab.Codex => theme.GetColor("TabActive"),
            JournalTab.Bestiary => theme.GetColor("EnemyColor"),
            JournalTab.FieldGuide => theme.GetColor("InfoColor"),
            JournalTab.Contracts => theme.GetColor("SuccessColor"),
            _ => theme.GetColor("LabelColor")
        };
    }

    /// <summary>
    /// Gets the Spectre.Console color for a tab based on active state.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="tab">The tab to get the color for.</param>
    /// <param name="isActive">Whether this tab is currently active.</param>
    /// <returns>Color string for Spectre.Console markup.</returns>
    public static string GetTabColor(JournalTab tab, bool isActive)
    {
        if (!isActive)
        {
            return "grey";
        }

        return tab switch
        {
            JournalTab.Codex => "gold1",
            JournalTab.Bestiary => "red",
            JournalTab.FieldGuide => "cyan",
            JournalTab.Contracts => "green",
            _ => "white"
        };
    }

    #endregion

    #region Completion Display

    /// <summary>
    /// Gets the themed completion indicator character (v0.3.14a).
    /// </summary>
    /// <param name="isComplete">Whether the entry is 100% complete.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>Unicode indicator character with themed color markup.</returns>
    public static string GetCompletionIndicator(bool isComplete, IThemeService theme)
    {
        return isComplete
            ? $"[{theme.GetColor("SuccessColor")}]★[/]"
            : $"[{theme.GetColor("DimColor")}]●[/]";
    }

    /// <summary>
    /// Gets the completion indicator character (★ for complete, ● for incomplete).
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="isComplete">Whether the entry is 100% complete.</param>
    /// <returns>Unicode indicator character with color markup.</returns>
    public static string GetCompletionIndicator(bool isComplete)
    {
        return isComplete
            ? "[green]★[/]"
            : "[grey]●[/]";
    }

    /// <summary>
    /// Gets the themed color for a completion percentage display (v0.3.14a).
    /// </summary>
    /// <param name="percent">Completion percentage (0-100).</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>Themed color: success (100%), warning (50-99%), dim (0-49%).</returns>
    public static string GetCompletionColor(int percent, IThemeService theme)
    {
        if (percent >= 100)
        {
            return theme.GetColor("SuccessColor");
        }

        if (percent >= 50)
        {
            return theme.GetColor("WarningColor");
        }

        return theme.GetColor("DimColor");
    }

    /// <summary>
    /// Gets the color for a completion percentage display.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="percent">Completion percentage (0-100).</param>
    /// <returns>Color string: green (100%), yellow (50-99%), grey (0-49%).</returns>
    public static string GetCompletionColor(int percent)
    {
        if (percent >= 100)
        {
            return "green";
        }

        if (percent >= 50)
        {
            return "yellow";
        }

        return "grey";
    }

    /// <summary>
    /// Formats a themed completion percentage for display (v0.3.14a).
    /// </summary>
    /// <param name="percent">Completion percentage (0-100).</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>Formatted string like "(75%)" with themed color.</returns>
    public static string FormatCompletionPercent(int percent, IThemeService theme)
    {
        var color = GetCompletionColor(percent, theme);
        return $"[{color}]({percent}%)[/]";
    }

    /// <summary>
    /// Formats a completion percentage for display.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="percent">Completion percentage (0-100).</param>
    /// <returns>Formatted string like "(75%)" with color.</returns>
    public static string FormatCompletionPercent(int percent)
    {
        var color = GetCompletionColor(percent);
        return $"[{color}]({percent}%)[/]";
    }

    #endregion

    #region Category Display

    /// <summary>
    /// Gets the Unicode icon for an entry category.
    /// </summary>
    /// <param name="category">The entry category.</param>
    /// <returns>Unicode icon character.</returns>
    public static string GetCategoryIcon(EntryCategory category) => category switch
    {
        EntryCategory.FieldGuide => "\u2139",    // ℹ Information
        EntryCategory.BlightOrigin => "\u2623",  // ☣ Biohazard
        EntryCategory.Bestiary => "\u2620",      // ☠ Skull
        EntryCategory.Factions => "\u2694",      // ⚔ Crossed Swords
        EntryCategory.Technical => "\u2699",     // ⚙ Gear
        EntryCategory.Geography => "\u2302",     // ⌂ House (landmark)
        _ => "\u25CF"                            // ● Bullet
    };

    /// <summary>
    /// Gets the display name for an entry category.
    /// </summary>
    /// <param name="category">The entry category.</param>
    /// <returns>Human-readable category name.</returns>
    public static string GetCategoryDisplayName(EntryCategory category) => category switch
    {
        EntryCategory.FieldGuide => "Field Guide",
        EntryCategory.BlightOrigin => "Blight Origin",
        EntryCategory.Bestiary => "Bestiary",
        EntryCategory.Factions => "Factions",
        EntryCategory.Technical => "Technical",
        EntryCategory.Geography => "Geography",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the themed color for an entry category (v0.3.14a).
    /// </summary>
    /// <param name="category">The entry category.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>Color string for Spectre.Console markup.</returns>
    public static string GetCategoryColor(EntryCategory category, IThemeService theme) => category switch
    {
        EntryCategory.FieldGuide => theme.GetColor("InfoColor"),
        EntryCategory.BlightOrigin => theme.GetColor("StressHigh"),
        EntryCategory.Bestiary => theme.GetColor("EnemyColor"),
        EntryCategory.Factions => theme.GetColor("WarningColor"),
        EntryCategory.Technical => theme.GetColor("QualityRare"),
        EntryCategory.Geography => theme.GetColor("SuccessColor"),
        _ => theme.GetColor("LabelColor")
    };

    /// <summary>
    /// Gets the color for an entry category.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="category">The entry category.</param>
    /// <returns>Color string for Spectre.Console markup.</returns>
    public static string GetCategoryColor(EntryCategory category) => category switch
    {
        EntryCategory.FieldGuide => "cyan",
        EntryCategory.BlightOrigin => "magenta1",
        EntryCategory.Bestiary => "red",
        EntryCategory.Factions => "orange1",
        EntryCategory.Technical => "blue",
        EntryCategory.Geography => "green",
        _ => "white"
    };

    #endregion

    #region Threshold Formatting

    /// <summary>
    /// Formats a threshold tag from SCREAMING_SNAKE_CASE to Title Case.
    /// </summary>
    /// <param name="tag">The tag to format (e.g., "WEAKNESS_REVEALED").</param>
    /// <returns>Formatted tag (e.g., "Weakness Revealed").</returns>
    public static string FormatThreshold(string tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return tag;
        }

        return string.Join(" ", tag.Split('_').Select(word =>
            word.Length > 0
                ? char.ToUpper(word[0]) + word[1..].ToLower()
                : word));
    }

    #endregion

    #region Glitch Effects

    /// <summary>
    /// Applies a glitch effect to text based on stress level.
    /// Characters are randomly replaced with block characters when stress exceeds 50.
    /// Uses stable pseudo-random based on text hash for consistent rendering.
    /// </summary>
    /// <param name="text">The text to potentially glitch.</param>
    /// <param name="stressLevel">Current stress level (0-100).</param>
    /// <returns>Text with glitch characters if stress > 50, otherwise original text.</returns>
    public static string ApplyGlitchEffect(string text, int stressLevel)
    {
        if (stressLevel < 50 || string.IsNullOrEmpty(text))
        {
            return text;
        }

        var glitchChars = "░▒▓█▀▄";
        var glitchRate = (stressLevel - 50) / 100.0; // 0-50% of chars
        var random = new Random(text.GetHashCode()); // Stable per-text

        return new string(text.Select(c =>
            random.NextDouble() < glitchRate && !char.IsWhiteSpace(c)
                ? glitchChars[random.Next(glitchChars.Length)]
                : c
        ).ToArray());
    }

    #endregion

    #region Progress Formatting

    /// <summary>
    /// Formats themed fragment progress for display (v0.3.14a).
    /// </summary>
    /// <param name="collected">Number of fragments collected.</param>
    /// <param name="required">Total fragments required.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>Formatted string like "12/15 fragments" with themed colors.</returns>
    public static string FormatFragmentProgress(int collected, int required, IThemeService theme)
    {
        var color = collected >= required
            ? theme.GetColor("SuccessColor")
            : theme.GetColor("WarningColor");
        var dimColor = theme.GetColor("DimColor");
        return $"[{color}]{collected}[/]/[{dimColor}]{required}[/] fragments";
    }

    /// <summary>
    /// Formats fragment progress for display.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="collected">Number of fragments collected.</param>
    /// <param name="required">Total fragments required.</param>
    /// <returns>Formatted string like "12/15 fragments".</returns>
    public static string FormatFragmentProgress(int collected, int required)
    {
        var color = collected >= required ? "green" : "yellow";
        return $"[{color}]{collected}[/]/[grey]{required}[/] fragments";
    }

    #endregion
}
