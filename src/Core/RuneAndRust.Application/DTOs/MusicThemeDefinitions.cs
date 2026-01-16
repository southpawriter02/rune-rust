namespace RuneAndRust.Application.DTOs;

using System.Text.Json.Serialization;
using RuneAndRust.Application.Enums;

/// <summary>
/// Configuration DTO for theme definitions.
/// </summary>
/// <remarks>
/// Represents a single theme definition with tracks, volume, and playback settings.
/// </remarks>
public class ThemeDefinition
{
    /// <summary>
    /// Gets or sets the music theme.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MusicTheme Theme { get; set; }

    /// <summary>
    /// Gets or sets the list of track paths for this theme.
    /// </summary>
    public List<string> Tracks { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional intro track path.
    /// </summary>
    public string? IntroTrack { get; set; }

    /// <summary>
    /// Gets or sets the volume multiplier (0.0-1.0).
    /// </summary>
    public float Volume { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether the theme should loop.
    /// </summary>
    public bool Loop { get; set; } = true;

    /// <summary>
    /// Gets or sets whether tracks should be shuffled.
    /// </summary>
    public bool Shuffle { get; set; }
}

/// <summary>
/// Configuration DTO for stinger definitions.
/// </summary>
/// <remarks>
/// Represents a one-shot audio stinger (victory, defeat, level-up, etc.).
/// </remarks>
public class StingerDefinition
{
    /// <summary>
    /// Gets or sets the stinger name (e.g., "victory", "defeat").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the track path for this stinger.
    /// </summary>
    public string Track { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the volume for this stinger (0.0-1.0).
    /// </summary>
    public float Volume { get; set; } = 1.0f;
}

/// <summary>
/// Root configuration DTO for music-themes.json.
/// </summary>
public class MusicThemesConfig
{
    /// <summary>
    /// Gets or sets the list of theme definitions.
    /// </summary>
    public List<ThemeDefinition> Themes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of stinger definitions.
    /// </summary>
    public List<StingerDefinition> Stingers { get; set; } = new();

    /// <summary>
    /// Gets or sets the transition settings.
    /// </summary>
    public TransitionSettings Transitions { get; set; } = new();
}

/// <summary>
/// Transition settings for crossfade (used in v0.9.1c).
/// </summary>
public class TransitionSettings
{
    /// <summary>
    /// Gets or sets the default crossfade duration in seconds.
    /// </summary>
    public float DefaultCrossfade { get; set; } = 2.0f;

    /// <summary>
    /// Gets or sets the combat crossfade duration in seconds.
    /// </summary>
    public float CombatCrossfade { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets the stinger fade-out duration in seconds.
    /// </summary>
    public float StingerFadeOut { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the delay before resuming after a stinger in seconds.
    /// </summary>
    public float StingerResumeDelay { get; set; } = 0.5f;
}
