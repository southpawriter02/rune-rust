namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Enums;

/// <summary>
/// Configuration for music themes and track selection.
/// </summary>
/// <remarks>
/// <para>
/// Provides theme configuration:
/// <list type="bullet">
///   <item><description>Track selection (shuffle or sequential)</description></item>
///   <item><description>Intro tracks for themes</description></item>
///   <item><description>Per-theme volume settings</description></item>
///   <item><description>Stinger lookup</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IMusicThemeConfig
{
    /// <summary>
    /// Gets a track path for a theme (respects shuffle/sequential).
    /// </summary>
    /// <param name="theme">The music theme.</param>
    /// <returns>Path to a track, or null if no tracks defined.</returns>
    string? GetTrackForTheme(MusicTheme theme);

    /// <summary>
    /// Gets all tracks for a theme.
    /// </summary>
    /// <param name="theme">The music theme.</param>
    /// <returns>List of track paths.</returns>
    IReadOnlyList<string> GetAllTracksForTheme(MusicTheme theme);

    /// <summary>
    /// Gets the intro track for a theme, if any.
    /// </summary>
    /// <param name="theme">The music theme.</param>
    /// <returns>Intro track path, or null if none.</returns>
    string? GetIntroTrack(MusicTheme theme);

    /// <summary>
    /// Gets the volume multiplier for a theme.
    /// </summary>
    /// <param name="theme">The music theme.</param>
    /// <returns>Volume multiplier (0.0-1.0).</returns>
    float GetThemeVolume(MusicTheme theme);

    /// <summary>
    /// Gets whether a theme should shuffle tracks.
    /// </summary>
    /// <param name="theme">The music theme.</param>
    /// <returns>True if tracks should be shuffled.</returns>
    bool ShouldShuffle(MusicTheme theme);

    /// <summary>
    /// Gets whether a theme should loop.
    /// </summary>
    /// <param name="theme">The music theme.</param>
    /// <returns>True if the theme should loop.</returns>
    bool ShouldLoop(MusicTheme theme);

    /// <summary>
    /// Gets a stinger track path by name.
    /// </summary>
    /// <param name="stingerName">Name of the stinger (e.g., "victory").</param>
    /// <returns>Stinger track path, or null if not found.</returns>
    string? GetStingerTrack(string stingerName);

    /// <summary>
    /// Gets the volume for a stinger.
    /// </summary>
    /// <param name="stingerName">Name of the stinger.</param>
    /// <returns>Volume multiplier (0.0-1.0).</returns>
    float GetStingerVolume(string stingerName);

    /// <summary>
    /// Gets all available stinger names.
    /// </summary>
    /// <returns>List of stinger names.</returns>
    IReadOnlyList<string> GetAvailableStingers();
}
