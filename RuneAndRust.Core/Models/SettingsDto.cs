namespace RuneAndRust.Core.Models;

/// <summary>
/// Data Transfer Object for serializing/deserializing user settings to/from JSON (v0.3.10a).
/// Maps to the static GameSettings class at runtime.
/// </summary>
public record SettingsDto
{
    /// <summary>
    /// When true, disables visual effects like border flashes and screen animations.
    /// Default: false.
    /// </summary>
    public bool ReduceMotion { get; init; } = false;

    /// <summary>
    /// The theme type as an integer value (maps to ThemeType enum).
    /// Default: 0 (Standard).
    /// </summary>
    public int Theme { get; init; } = 0;

    /// <summary>
    /// Text display speed as a percentage (10-200).
    /// Lower values = slower text, higher values = faster text.
    /// Default: 100 (normal speed).
    /// </summary>
    public int TextSpeed { get; init; } = 100;

    /// <summary>
    /// Master audio volume as a percentage (0-100).
    /// Default: 100 (full volume).
    /// </summary>
    public int MasterVolume { get; init; } = 100;

    /// <summary>
    /// Whether ambient soundscape is enabled (v0.3.19c).
    /// Default: true (enabled).
    /// </summary>
    public bool AmbienceEnabled { get; init; } = true;

    /// <summary>
    /// Interval between autosaves in minutes (1-60).
    /// Default: 5 minutes.
    /// </summary>
    public int AutosaveIntervalMinutes { get; init; } = 5;

    /// <summary>
    /// The language/locale code (e.g., "en-US", "de-DE").
    /// Default: "en-US". Added in v0.3.15b.
    /// </summary>
    public string Language { get; init; } = "en-US";
}
