namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Enums;

/// <summary>
/// Manages smooth audio transitions between music tracks.
/// </summary>
/// <remarks>
/// <para>
/// Provides transition effects:
/// <list type="bullet">
///   <item><description>Crossfade between tracks</description></item>
///   <item><description>Fade in/out for pause handling</description></item>
///   <item><description>Stinger ducking with volume restore</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ICrossfadeManager
{
    /// <summary>
    /// Gets whether a transition is currently in progress.
    /// </summary>
    bool IsTransitioning { get; }

    /// <summary>
    /// Crossfades from current track to a new track.
    /// </summary>
    /// <param name="newTrack">Path to the new track.</param>
    /// <param name="duration">Crossfade duration in seconds.</param>
    /// <param name="onComplete">Callback when transition completes.</param>
    void CrossfadeTo(string newTrack, float duration, Action? onComplete = null);

    /// <summary>
    /// Crossfades to a new theme.
    /// </summary>
    /// <param name="theme">The theme to transition to.</param>
    /// <param name="duration">Crossfade duration in seconds.</param>
    void CrossfadeToTheme(MusicTheme theme, float duration);

    /// <summary>
    /// Fades out current music.
    /// </summary>
    /// <param name="duration">Fade duration in seconds.</param>
    void FadeOut(float duration);

    /// <summary>
    /// Fades in current music.
    /// </summary>
    /// <param name="duration">Fade duration in seconds.</param>
    void FadeIn(float duration);

    /// <summary>
    /// Ducks background music and plays a stinger.
    /// </summary>
    /// <param name="stingerPath">Path to the stinger audio file.</param>
    /// <param name="duckLevel">Background volume during stinger (0.0-1.0).</param>
    /// <param name="onComplete">Callback when stinger completes.</param>
    void DuckAndPlay(string stingerPath, float duckLevel = 0.2f, Action? onComplete = null);

    /// <summary>
    /// Cancels any active transition.
    /// </summary>
    void Cancel();
}
