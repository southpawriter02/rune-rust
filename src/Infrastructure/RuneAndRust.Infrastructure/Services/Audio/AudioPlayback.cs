namespace RuneAndRust.Infrastructure.Services.Audio;

using RuneAndRust.Application.Enums;

/// <summary>
/// Represents an active audio playback instance.
/// </summary>
/// <remarks>
/// <para>
/// Tracks playback state including:
/// <list type="bullet">
///   <item><description>Unique playback ID</description></item>
///   <item><description>Channel assignment</description></item>
///   <item><description>Volume and loop settings</description></item>
///   <item><description>Pause/play state</description></item>
/// </list>
/// </para>
/// </remarks>
public class AudioPlayback
{
    /// <summary>
    /// Gets the unique playback ID.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the channel this playback is on.
    /// </summary>
    public AudioChannel Channel { get; }

    /// <summary>
    /// Gets the cached audio data.
    /// </summary>
    public CachedAudio Audio { get; }

    /// <summary>
    /// Gets the base volume (before channel/master modifiers).
    /// </summary>
    public float BaseVolume { get; }

    /// <summary>
    /// Gets or sets the effective volume (after modifiers).
    /// </summary>
    public float EffectiveVolume { get; private set; }

    /// <summary>
    /// Gets whether this playback is looping.
    /// </summary>
    public bool IsLooping { get; }

    /// <summary>
    /// Gets whether this playback is paused.
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    /// Gets whether this playback is stopped.
    /// </summary>
    public bool IsStopped { get; private set; }

    /// <summary>
    /// Creates a new audio playback instance.
    /// </summary>
    /// <param name="id">Unique playback ID.</param>
    /// <param name="channel">Channel to play on.</param>
    /// <param name="audio">Cached audio data.</param>
    /// <param name="effectiveVolume">Calculated effective volume.</param>
    /// <param name="loop">Whether to loop.</param>
    public AudioPlayback(Guid id, AudioChannel channel, CachedAudio audio, float effectiveVolume, bool loop)
    {
        Id = id;
        Channel = channel;
        Audio = audio;
        BaseVolume = effectiveVolume;
        EffectiveVolume = effectiveVolume;
        IsLooping = loop;
    }

    /// <summary>
    /// Starts playback.
    /// </summary>
    public void Start()
    {
        IsPaused = false;
        IsStopped = false;
        // Future: Start actual audio playback via NAudio or similar
    }

    /// <summary>
    /// Stops playback.
    /// </summary>
    public void Stop()
    {
        IsStopped = true;
        IsPaused = false;
        // Future: Stop actual audio playback
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public void Pause()
    {
        if (!IsStopped)
        {
            IsPaused = true;
            // Future: Pause actual audio playback
        }
    }

    /// <summary>
    /// Resumes paused playback.
    /// </summary>
    public void Resume()
    {
        if (!IsStopped && IsPaused)
        {
            IsPaused = false;
            // Future: Resume actual audio playback
        }
    }

    /// <summary>
    /// Sets the effective volume.
    /// </summary>
    /// <param name="volume">New effective volume (0.0-1.0).</param>
    public void SetVolume(float volume)
    {
        EffectiveVolume = Math.Clamp(volume, 0f, 1f);
        // Future: Update actual audio playback volume
    }
}
