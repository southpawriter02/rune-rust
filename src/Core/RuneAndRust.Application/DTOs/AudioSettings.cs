namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Audio settings for persistence.
/// </summary>
/// <remarks>
/// <para>
/// Stores all user audio preferences:
/// <list type="bullet">
///   <item><description>Global enable/mute state</description></item>
///   <item><description>Per-channel volume levels (0.0-1.0)</description></item>
///   <item><description>TUI console bell toggle</description></item>
/// </list>
/// </para>
/// </remarks>
public class AudioSettings
{
    /// <summary>
    /// Gets or sets whether audio is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the master volume (0.0-1.0).
    /// </summary>
    public float MasterVolume { get; set; } = 0.8f;

    /// <summary>
    /// Gets or sets the music volume (0.0-1.0).
    /// </summary>
    public float MusicVolume { get; set; } = 0.6f;

    /// <summary>
    /// Gets or sets the sound effects volume (0.0-1.0).
    /// </summary>
    public float EffectsVolume { get; set; } = 0.8f;

    /// <summary>
    /// Gets or sets the UI sounds volume (0.0-1.0).
    /// </summary>
    public float UiVolume { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets whether all audio is muted.
    /// </summary>
    public bool Muted { get; set; }

    /// <summary>
    /// Gets or sets whether TUI console bells are enabled.
    /// </summary>
    public bool TuiBellEnabled { get; set; } = true;

    /// <summary>
    /// Creates a new instance with default values.
    /// </summary>
    public AudioSettings()
    {
    }

    /// <summary>
    /// Creates a copy of the settings.
    /// </summary>
    /// <returns>A new AudioSettings with the same values.</returns>
    public AudioSettings Clone() => new()
    {
        Enabled = Enabled,
        MasterVolume = MasterVolume,
        MusicVolume = MusicVolume,
        EffectsVolume = EffectsVolume,
        UiVolume = UiVolume,
        Muted = Muted,
        TuiBellEnabled = TuiBellEnabled
    };
}
