namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines available color themes for accessibility support (v0.3.9b).
/// Each theme provides optimized color palettes for different visual needs.
/// </summary>
public enum ThemeType
{
    /// <summary>
    /// Default color scheme with full color range.
    /// Uses standard game colors: cyan (player), red (enemy), green (health), etc.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// High contrast theme for users with low vision.
    /// Uses bold, bright colors with maximum differentiation.
    /// </summary>
    HighContrast = 1,

    /// <summary>
    /// Optimized for red-green color blindness (protanopia).
    /// Replaces red/green distinctions with blue/orange alternatives.
    /// </summary>
    Protanopia = 2,

    /// <summary>
    /// Optimized for green-red color blindness (deuteranopia).
    /// Similar to Protanopia but with slightly different blue/orange mapping.
    /// </summary>
    Deuteranopia = 3,

    /// <summary>
    /// Optimized for blue-yellow color blindness (tritanopia).
    /// Replaces blue/yellow distinctions with magenta/cyan alternatives.
    /// </summary>
    Tritanopia = 4
}
