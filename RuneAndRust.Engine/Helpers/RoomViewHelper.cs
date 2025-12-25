using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Helpers;

/// <summary>
/// Helper methods for room view formatting (v0.3.5c).
/// Provides biome color mapping and object/enemy name formatting.
/// Used by GameService to prepare data for ExplorationViewModel.
/// Updated in v0.3.14a to support IThemeService for accessibility themes.
/// </summary>
public static class RoomViewHelper
{
    /// <summary>
    /// Maps BiomeType to themed Spectre.Console color name (v0.3.14a).
    /// </summary>
    /// <param name="biome">The biome type to map.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>A Spectre.Console color name string from the current theme.</returns>
    public static string GetBiomeColor(BiomeType biome, IThemeService theme)
    {
        var colorKey = biome switch
        {
            BiomeType.Industrial => "BiomeIndustrial",
            BiomeType.Organic => "BiomeOrganic",
            BiomeType.Void => "BiomeVoid",
            _ => "BiomeRuin"  // Ruin and default
        };
        return theme.GetColor(colorKey);
    }

    /// <summary>
    /// Maps BiomeType to Spectre.Console color name.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="biome">The biome type to map.</param>
    /// <returns>A Spectre.Console color name string.</returns>
    public static string GetBiomeColor(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Industrial => "orange1",
            BiomeType.Organic => "green",
            BiomeType.Void => "purple",
            _ => "grey"  // Ruin and default
        };
    }

    /// <summary>
    /// Formats an object name with themed color based on type (v0.3.14a).
    /// </summary>
    /// <param name="name">The object's display name.</param>
    /// <param name="isContainer">Whether the object is a container.</param>
    /// <param name="isLocked">Whether the container is locked.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>A markup-formatted string with themed coloring.</returns>
    public static string FormatObjectName(string name, bool isContainer, bool isLocked, IThemeService theme)
    {
        var escapedName = EscapeMarkup(name);
        var dimColor = theme.GetColor("DimColor");

        if (isContainer)
        {
            var containerColor = theme.GetColor("HeaderColor");  // gold1 in standard
            var lockIndicator = isLocked ? $" [{dimColor}](locked)[/]" : "";
            return $"[{containerColor}]{escapedName}[/]{lockIndicator}";
        }

        return $"[{dimColor}]{escapedName}[/]";
    }

    /// <summary>
    /// Formats an object name with appropriate color based on type.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="name">The object's display name.</param>
    /// <param name="isContainer">Whether the object is a container.</param>
    /// <param name="isLocked">Whether the container is locked.</param>
    /// <returns>A markup-formatted string with appropriate coloring.</returns>
    public static string FormatObjectName(string name, bool isContainer, bool isLocked)
    {
        // Escape special characters for Spectre.Console markup
        var escapedName = EscapeMarkup(name);

        // Container logic: gold color, show lock state
        if (isContainer)
        {
            var lockIndicator = isLocked ? " [grey](locked)[/]" : "";
            return $"[gold1]{escapedName}[/]{lockIndicator}";
        }

        // Default: grey for generic objects
        return $"[grey]{escapedName}[/]";
    }

    /// <summary>
    /// Formats an enemy name with themed health status (v0.3.14a).
    /// </summary>
    /// <param name="name">The enemy's display name.</param>
    /// <param name="currentHp">The enemy's current hit points.</param>
    /// <param name="maxHp">The enemy's maximum hit points.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>A markup-formatted string with themed name and health.</returns>
    public static string FormatEnemyName(string name, int currentHp, int maxHp, IThemeService theme)
    {
        var escapedName = EscapeMarkup(name);
        var healthPercent = maxHp > 0 ? (double)currentHp / maxHp * 100 : 0;
        var healthStatus = GetNarrativeHealth(healthPercent);
        var enemyColor = theme.GetColor("EnemyColor");
        var dimColor = theme.GetColor("DimColor");
        return $"[{enemyColor}]{escapedName}[/] [{dimColor}]({healthStatus})[/]";
    }

    /// <summary>
    /// Formats an enemy name with health status.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="name">The enemy's display name.</param>
    /// <param name="currentHp">The enemy's current hit points.</param>
    /// <param name="maxHp">The enemy's maximum hit points.</param>
    /// <returns>A markup-formatted string with name and narrative health.</returns>
    public static string FormatEnemyName(string name, int currentHp, int maxHp)
    {
        var escapedName = EscapeMarkup(name);
        var healthPercent = maxHp > 0 ? (double)currentHp / maxHp * 100 : 0;
        var healthStatus = GetNarrativeHealth(healthPercent);
        return $"[red]{escapedName}[/] [grey]({healthStatus})[/]";
    }

    /// <summary>
    /// Converts HP percentage to narrative health status.
    /// </summary>
    /// <param name="percent">Health percentage (0-100).</param>
    /// <returns>A narrative health descriptor.</returns>
    public static string GetNarrativeHealth(double percent)
    {
        return percent switch
        {
            > 75 => "Healthy",
            > 50 => "Wounded",
            > 25 => "Bloodied",
            > 0 => "Critical",
            _ => "Dead"
        };
    }

    /// <summary>
    /// Escapes special characters for Spectre.Console markup.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped text.</returns>
    private static string EscapeMarkup(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text.Replace("[", "[[").Replace("]", "]]");
    }
}
