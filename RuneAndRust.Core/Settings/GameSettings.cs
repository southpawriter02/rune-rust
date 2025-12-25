using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Settings;

/// <summary>
/// Static game settings for accessibility and visual preferences (v0.3.10a).
/// Provides global access to user preferences that affect visual effects, themes, and animations.
/// Persisted to data/options.json via SettingsService.
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

    /// <summary>
    /// Text display speed as a percentage (10-200).
    /// Lower values = slower typewriter effects, higher values = faster.
    /// Default: 100 (normal speed). Added in v0.3.10a.
    /// </summary>
    public static int TextSpeed { get; set; } = 100;

    /// <summary>
    /// Master audio volume as a percentage (0-100).
    /// Default: 100 (full volume). Added in v0.3.10a.
    /// </summary>
    public static int MasterVolume { get; set; } = 100;

    /// <summary>
    /// Interval between autosaves in minutes (1-60).
    /// Default: 5 minutes. Added in v0.3.10a.
    /// </summary>
    public static int AutosaveIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// The current language/locale code (e.g., "en-US", "de-DE").
    /// Default: "en-US". Added in v0.3.15b.
    /// </summary>
    public static string Language { get; set; } = "en-US";
}
