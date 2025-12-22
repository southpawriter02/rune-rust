namespace RuneAndRust.Core.Settings;

/// <summary>
/// Static game settings for accessibility and visual preferences (v0.3.9a).
/// Provides global access to user preferences that affect visual effects and animations.
/// </summary>
public static class GameSettings
{
    /// <summary>
    /// When true, disables visual effects like border flashes and screen animations.
    /// Supports accessibility requirements for users sensitive to motion or flashing.
    /// </summary>
    public static bool ReduceMotion { get; set; } = false;

    // Future settings (v0.3.9b+):
    // public static ThemeType Theme { get; set; } = ThemeType.Default;
    // public static int FontScale { get; set; } = 100;
}
