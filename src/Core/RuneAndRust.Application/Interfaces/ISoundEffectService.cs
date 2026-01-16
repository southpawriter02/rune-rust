namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Manages sound effect playback across the application.
/// </summary>
/// <remarks>
/// <para>
/// Provides SFX playback features:
/// <list type="bullet">
///   <item><description>Play effects by ID</description></item>
///   <item><description>Play random effects from category</description></item>
///   <item><description>Volume control via Effects channel</description></item>
///   <item><description>Max simultaneous sound enforcement</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ISoundEffectService
{
    /// <summary>
    /// Plays a sound effect by its ID.
    /// </summary>
    /// <param name="effectId">The effect ID (e.g., "attack-hit", "button-click").</param>
    /// <param name="volume">Optional volume multiplier (0.0-1.0).</param>
    void Play(string effectId, float volume = 1.0f);

    /// <summary>
    /// Plays a random sound from a category.
    /// </summary>
    /// <param name="category">The category (e.g., "combat", "ui").</param>
    /// <param name="volume">Optional volume multiplier.</param>
    void PlayRandom(string category, float volume = 1.0f);

    /// <summary>
    /// Stops all currently playing effects.
    /// </summary>
    void StopAll();

    /// <summary>
    /// Sets the global effects volume.
    /// </summary>
    /// <param name="volume">Volume level (0.0-1.0).</param>
    void SetVolume(float volume);

    /// <summary>
    /// Gets the global effects volume.
    /// </summary>
    /// <returns>Volume level (0.0-1.0).</returns>
    float GetVolume();

    /// <summary>
    /// Preloads all effects in a category.
    /// </summary>
    /// <param name="category">Category name to preload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task representing the preload operation.</returns>
    Task PreloadCategoryAsync(string category, CancellationToken ct = default);

    /// <summary>
    /// Gets all available effect IDs.
    /// </summary>
    /// <returns>List of effect IDs.</returns>
    IReadOnlyList<string> GetAvailableEffects();

    /// <summary>
    /// Gets all categories.
    /// </summary>
    /// <returns>List of category names.</returns>
    IReadOnlyList<string> GetCategories();
}
