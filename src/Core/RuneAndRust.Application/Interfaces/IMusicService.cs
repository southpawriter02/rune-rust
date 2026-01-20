namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Enums;

/// <summary>
/// Manages background music playback with theme support.
/// </summary>
/// <remarks>
/// <para>
/// Provides music management features:
/// <list type="bullet">
///   <item><description>Track playback with looping</description></item>
///   <item><description>Theme-based music selection</description></item>
///   <item><description>Stinger one-shot audio</description></item>
///   <item><description>Volume control</description></item>
///   <item><description>Pause/resume support</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IMusicService : IDisposable
{
    /// <summary>
    /// Gets the currently playing theme.
    /// </summary>
    MusicTheme CurrentTheme { get; }

    /// <summary>
    /// Gets the currently playing track path.
    /// </summary>
    string? CurrentTrack { get; }

    /// <summary>
    /// Gets whether music is currently playing.
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// Gets whether music is paused.
    /// </summary>
    bool IsPaused { get; }

    /// <summary>
    /// Plays a specific track.
    /// </summary>
    /// <param name="trackPath">Path to the music file.</param>
    /// <param name="loop">Whether to loop the track (default true).</param>
    void PlayTrack(string trackPath, bool loop = true);

    /// <summary>
    /// Stops the current music.
    /// </summary>
    void Stop();

    /// <summary>
    /// Pauses the current music.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resumes paused music.
    /// </summary>
    void Resume();

    /// <summary>
    /// Sets the music volume.
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0).</param>
    void SetVolume(float volume);

    /// <summary>
    /// Gets the current music volume.
    /// </summary>
    /// <returns>Volume level (0.0 to 1.0).</returns>
    float GetVolume();

    /// <summary>
    /// Sets the current music theme.
    /// </summary>
    /// <param name="theme">The theme to switch to.</param>
    /// <remarks>
    /// If the theme is different from current, triggers track change.
    /// Same theme call is idempotent.
    /// </remarks>
    void SetTheme(MusicTheme theme);

    /// <summary>
    /// Plays a one-shot stinger track, optionally resuming music after.
    /// </summary>
    /// <param name="stingerPath">Path to the stinger audio file.</param>
    /// <param name="onComplete">Optional callback when stinger finishes.</param>
    void PlayStinger(string stingerPath, Action? onComplete = null);

    /// <summary>
    /// Preloads music tracks for a theme.
    /// </summary>
    /// <param name="trackPaths">Paths to the tracks to preload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task representing the preload operation.</returns>
    Task PreloadAsync(IEnumerable<string> trackPaths, CancellationToken ct = default);

    /// <summary>
    /// Event fired when track changes.
    /// </summary>
    event Action<string>? OnTrackChanged;

    /// <summary>
    /// Event fired when theme changes.
    /// </summary>
    event Action<MusicTheme>? OnThemeChanged;
}
