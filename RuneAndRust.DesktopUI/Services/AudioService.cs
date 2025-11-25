using System;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.18: Implementation of audio service.
/// Manages volume levels and audio playback.
/// Note: Actual audio playback will be implemented when audio assets are available.
/// </summary>
public class AudioService : IAudioService
{
    private float _masterVolume = 1.0f;
    private float _musicVolume = 0.8f;
    private float _sfxVolume = 1.0f;
    private bool _isMuted;

    /// <inheritdoc/>
    public float MasterVolume => _masterVolume;

    /// <inheritdoc/>
    public float MusicVolume => _musicVolume;

    /// <inheritdoc/>
    public float SFXVolume => _sfxVolume;

    /// <inheritdoc/>
    public bool IsMuted => _isMuted;

    /// <inheritdoc/>
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Math.Clamp(volume, 0f, 1f);
        ApplyVolumeSettings();
    }

    /// <inheritdoc/>
    public void SetMusicVolume(float volume)
    {
        _musicVolume = Math.Clamp(volume, 0f, 1f);
        ApplyVolumeSettings();
    }

    /// <inheritdoc/>
    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Math.Clamp(volume, 0f, 1f);
        ApplyVolumeSettings();
    }

    /// <inheritdoc/>
    public void ToggleMute()
    {
        _isMuted = !_isMuted;
        ApplyVolumeSettings();
    }

    /// <inheritdoc/>
    public void SetMuted(bool muted)
    {
        _isMuted = muted;
        ApplyVolumeSettings();
    }

    /// <inheritdoc/>
    public void PlayUISound(string soundName)
    {
        if (_isMuted) return;

        // Placeholder: Actual audio playback will be implemented
        // when audio assets and audio library are integrated
        Console.WriteLine($"[AUDIO] Playing UI sound: {soundName} (Volume: {GetEffectiveSFXVolume():P0})");
    }

    /// <inheritdoc/>
    public void PlayMusic(string trackName, bool fadeIn = true)
    {
        if (_isMuted) return;

        // Placeholder: Actual music playback will be implemented
        // when audio assets and audio library are integrated
        var fadeText = fadeIn ? "with fade-in" : "immediately";
        Console.WriteLine($"[AUDIO] Playing music: {trackName} {fadeText} (Volume: {GetEffectiveMusicVolume():P0})");
    }

    /// <inheritdoc/>
    public void StopMusic(bool fadeOut = true)
    {
        var fadeText = fadeOut ? "with fade-out" : "immediately";
        Console.WriteLine($"[AUDIO] Stopping music {fadeText}");
    }

    /// <inheritdoc/>
    public void Initialize(AudioConfig config)
    {
        _masterVolume = Math.Clamp(config.MasterVolume, 0f, 1f);
        _musicVolume = Math.Clamp(config.MusicVolume, 0f, 1f);
        _sfxVolume = Math.Clamp(config.SFXVolume, 0f, 1f);
        _isMuted = config.IsMuted;

        Console.WriteLine($"[AUDIO] Initialized - Master: {_masterVolume:P0}, Music: {_musicVolume:P0}, SFX: {_sfxVolume:P0}, Muted: {_isMuted}");
    }

    /// <summary>
    /// Gets the effective music volume (master * music).
    /// </summary>
    private float GetEffectiveMusicVolume()
    {
        return _isMuted ? 0f : _masterVolume * _musicVolume;
    }

    /// <summary>
    /// Gets the effective SFX volume (master * sfx).
    /// </summary>
    private float GetEffectiveSFXVolume()
    {
        return _isMuted ? 0f : _masterVolume * _sfxVolume;
    }

    /// <summary>
    /// Applies the current volume settings to the audio system.
    /// </summary>
    private void ApplyVolumeSettings()
    {
        // Placeholder: When audio library is integrated, apply settings here
        Console.WriteLine($"[AUDIO] Volume updated - Master: {_masterVolume:P0}, Music: {GetEffectiveMusicVolume():P0}, SFX: {GetEffectiveSFXVolume():P0}");
    }
}
