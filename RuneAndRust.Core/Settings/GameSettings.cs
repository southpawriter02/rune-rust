using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Settings;

/// <summary>
/// Static game settings for accessibility and visual preferences (v0.3.9b).
/// Provides global access to user preferences that affect visual effects, themes, and animations.
/// </summary>
public static class GameSettings
{
    /// <summary>
    /// When true, disables visual effects like border flashes and screen animations.
    /// Supports accessibility requirements for users sensitive to motion or flashing.
    /// </summary>
    public static bool ReduceMotion { get; set; } = false;

    /// <summary>
    /// The current color theme for the game UI.
    /// Supports accessibility for color vision deficiencies (v0.3.9b).
    /// </summary>
    public static ThemeType Theme { get; set; } = ThemeType.Standard;

    // Future settings (v0.3.10+):
    // public static int FontScale { get; set; } = 100;
}
