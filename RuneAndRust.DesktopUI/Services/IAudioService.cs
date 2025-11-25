namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.18: Service for managing audio playback and volume control.
/// Provides control over master, music, and SFX volume levels.
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Gets the current master volume level (0.0 - 1.0).
    /// </summary>
    float MasterVolume { get; }

    /// <summary>
    /// Gets the current music volume level (0.0 - 1.0).
    /// </summary>
    float MusicVolume { get; }

    /// <summary>
    /// Gets the current SFX volume level (0.0 - 1.0).
    /// </summary>
    float SFXVolume { get; }

    /// <summary>
    /// Gets whether audio is muted.
    /// </summary>
    bool IsMuted { get; }

    /// <summary>
    /// Sets the master volume level.
    /// </summary>
    /// <param name="volume">Volume level (0.0 - 1.0)</param>
    void SetMasterVolume(float volume);

    /// <summary>
    /// Sets the music volume level.
    /// </summary>
    /// <param name="volume">Volume level (0.0 - 1.0)</param>
    void SetMusicVolume(float volume);

    /// <summary>
    /// Sets the SFX volume level.
    /// </summary>
    /// <param name="volume">Volume level (0.0 - 1.0)</param>
    void SetSFXVolume(float volume);

    /// <summary>
    /// Toggles mute state for all audio.
    /// </summary>
    void ToggleMute();

    /// <summary>
    /// Sets the mute state for all audio.
    /// </summary>
    /// <param name="muted">True to mute, false to unmute</param>
    void SetMuted(bool muted);

    /// <summary>
    /// Plays a UI sound effect.
    /// </summary>
    /// <param name="soundName">Name of the sound to play</param>
    void PlayUISound(string soundName);

    /// <summary>
    /// Plays background music.
    /// </summary>
    /// <param name="trackName">Name of the music track</param>
    /// <param name="fadeIn">Whether to fade in the music</param>
    void PlayMusic(string trackName, bool fadeIn = true);

    /// <summary>
    /// Stops the current background music.
    /// </summary>
    /// <param name="fadeOut">Whether to fade out the music</param>
    void StopMusic(bool fadeOut = true);

    /// <summary>
    /// Initializes the audio system with saved settings.
    /// </summary>
    /// <param name="config">Audio configuration to apply</param>
    void Initialize(AudioConfig config);
}
