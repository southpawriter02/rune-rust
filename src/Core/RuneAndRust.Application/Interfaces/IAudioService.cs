namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Enums;

/// <summary>
/// Core audio playback service for the application.
/// </summary>
/// <remarks>
/// <para>
/// Provides audio playback capabilities with:
/// <list type="bullet">
///   <item><description>Channel-based volume control</description></item>
///   <item><description>Individual playback management</description></item>
///   <item><description>Audio caching for performance</description></item>
///   <item><description>Global enable/disable control</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Plays an audio file on a specified channel.
    /// </summary>
    /// <param name="audioPath">Path to the audio file.</param>
    /// <param name="channel">Channel to play on.</param>
    /// <param name="volume">Volume multiplier (0.0-1.0), default 1.0.</param>
    /// <param name="loop">Whether to loop the audio.</param>
    /// <returns>Playback ID for control, or <see cref="Guid.Empty"/> if failed or muted.</returns>
    Guid Play(string audioPath, AudioChannel channel, float volume = 1.0f, bool loop = false);

    /// <summary>
    /// Stops playback by ID.
    /// </summary>
    /// <param name="playbackId">The playback ID returned from <see cref="Play"/>.</param>
    void Stop(Guid playbackId);

    /// <summary>
    /// Stops all playback on a channel.
    /// </summary>
    /// <param name="channel">The channel to stop.</param>
    void StopChannel(AudioChannel channel);

    /// <summary>
    /// Stops all active playback.
    /// </summary>
    void StopAll();

    /// <summary>
    /// Pauses playback by ID.
    /// </summary>
    /// <param name="playbackId">The playback ID returned from <see cref="Play"/>.</param>
    void Pause(Guid playbackId);

    /// <summary>
    /// Pauses all playback on a channel.
    /// </summary>
    /// <param name="channel">The channel to pause.</param>
    void PauseChannel(AudioChannel channel);

    /// <summary>
    /// Resumes paused playback by ID.
    /// </summary>
    /// <param name="playbackId">The playback ID returned from <see cref="Play"/>.</param>
    void Resume(Guid playbackId);

    /// <summary>
    /// Resumes all paused playback on a channel.
    /// </summary>
    /// <param name="channel">The channel to resume.</param>
    void ResumeChannel(AudioChannel channel);

    /// <summary>
    /// Sets volume for a channel.
    /// </summary>
    /// <param name="channel">The channel to set.</param>
    /// <param name="volume">Volume level (0.0-1.0), clamped automatically.</param>
    void SetChannelVolume(AudioChannel channel, float volume);

    /// <summary>
    /// Gets current volume for a channel.
    /// </summary>
    /// <param name="channel">The channel to query.</param>
    /// <returns>Volume level (0.0-1.0).</returns>
    float GetChannelVolume(AudioChannel channel);

    /// <summary>
    /// Sets mute state for a channel.
    /// </summary>
    /// <param name="channel">The channel to mute/unmute.</param>
    /// <param name="muted">True to mute, false to unmute.</param>
    void SetChannelMuted(AudioChannel channel, bool muted);

    /// <summary>
    /// Gets mute state for a channel.
    /// </summary>
    /// <param name="channel">The channel to query.</param>
    /// <returns>True if muted.</returns>
    bool IsChannelMuted(AudioChannel channel);

    /// <summary>
    /// Preloads audio files into cache.
    /// </summary>
    /// <param name="audioPaths">Paths to audio files to preload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task representing the preload operation.</returns>
    Task PreloadAsync(IEnumerable<string> audioPaths, CancellationToken ct = default);

    /// <summary>
    /// Clears the audio cache, freeing resources.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Gets whether audio is currently enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Enables or disables all audio.
    /// </summary>
    /// <param name="enabled">True to enable, false to disable (stops all audio).</param>
    void SetEnabled(bool enabled);
}
