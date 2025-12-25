using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing color themes and accessibility palettes (v0.3.9b).
/// Provides semantic color lookups that adapt based on the current theme setting.
/// </summary>
/// <remarks>See: SPEC-THEME-001 for Theme System design.</remarks>
public interface IThemeService
{
    /// <summary>
    /// Gets the current active theme.
    /// </summary>
    ThemeType CurrentTheme { get; }

    /// <summary>
    /// Gets the color string for a semantic role in the current theme.
    /// Used for Spectre.Console markup like "[{color}]text[/]".
    /// </summary>
    /// <param name="colorRole">The semantic role name (e.g., "PlayerColor", "EnemyColor", "HealthCritical").</param>
    /// <returns>The color string for the current theme, or "grey" if role is unknown.</returns>
    string GetColor(string colorRole);

    /// <summary>
    /// Changes the active theme and updates GameSettings.Theme.
    /// </summary>
    /// <param name="theme">The theme to switch to.</param>
    void SetTheme(ThemeType theme);
}
