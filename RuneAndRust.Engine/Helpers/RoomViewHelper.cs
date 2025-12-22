using RuneAndRust.Core.Enums;

namespace RuneAndRust.Engine.Helpers;

/// <summary>
/// Helper methods for room view formatting (v0.3.5c).
/// Provides biome color mapping and object/enemy name formatting.
/// Used by GameService to prepare data for ExplorationViewModel.
/// </summary>
public static class RoomViewHelper
{
    /// <summary>
    /// Maps BiomeType to Spectre.Console color name.
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
    /// Formats an object name with appropriate color based on type.
    /// Uses Spectre.Console markup syntax.
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
    /// Formats an enemy name with health status.
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
