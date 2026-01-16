namespace RuneAndRust.Presentation.Gui.ViewModels.Settings;

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// View model for the audio settings tab.
/// </summary>
/// <remarks>
/// <para>
/// Provides binding for:
/// <list type="bullet">
///   <item><description>Enable/mute toggles</description></item>
///   <item><description>Volume sliders with percentage display</description></item>
///   <item><description>Real-time audio service updates</description></item>
///   <item><description>Settings save/load/revert</description></item>
/// </list>
/// </para>
/// </remarks>
public partial class AudioSettingsViewModel : ViewModelBase
{
    private readonly IAudioService? _audioService;
    private readonly ILogger<AudioSettingsViewModel> _logger;
    private AudioSettings _savedSettings = new();

    /// <summary>
    /// Gets or sets whether audio is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _isAudioEnabled = true;

    /// <summary>
    /// Gets or sets the master volume (0.0-1.0).
    /// </summary>
    [ObservableProperty]
    private float _masterVolume = 0.8f;

    /// <summary>
    /// Gets or sets the music volume (0.0-1.0).
    /// </summary>
    [ObservableProperty]
    private float _musicVolume = 0.6f;

    /// <summary>
    /// Gets or sets the effects volume (0.0-1.0).
    /// </summary>
    [ObservableProperty]
    private float _effectsVolume = 0.8f;

    /// <summary>
    /// Gets or sets the UI volume (0.0-1.0).
    /// </summary>
    [ObservableProperty]
    private float _uiVolume = 0.7f;

    /// <summary>
    /// Gets or sets whether all audio is muted.
    /// </summary>
    [ObservableProperty]
    private bool _isMuted;

    /// <summary>
    /// Gets or sets whether TUI console bells are enabled.
    /// </summary>
    [ObservableProperty]
    private bool _tuiBellEnabled = true;

    /// <summary>
    /// Gets the master volume as a percentage string.
    /// </summary>
    public string MasterVolumePercent => $"{(int)(MasterVolume * 100)}%";

    /// <summary>
    /// Gets the music volume as a percentage string.
    /// </summary>
    public string MusicVolumePercent => $"{(int)(MusicVolume * 100)}%";

    /// <summary>
    /// Gets the effects volume as a percentage string.
    /// </summary>
    public string EffectsVolumePercent => $"{(int)(EffectsVolume * 100)}%";

    /// <summary>
    /// Gets the UI volume as a percentage string.
    /// </summary>
    public string UiVolumePercent => $"{(int)(UiVolume * 100)}%";

    /// <summary>
    /// Creates a new audio settings view model.
    /// </summary>
    /// <param name="audioService">Optional audio service for real-time updates.</param>
    /// <param name="logger">Logger for settings operations.</param>
    public AudioSettingsViewModel(
        IAudioService? audioService,
        ILogger<AudioSettingsViewModel> logger)
    {
        _audioService = audioService;
        _logger = logger;
        _logger.LogDebug("AudioSettingsViewModel initialized");
    }

    /// <summary>
    /// Creates a new audio settings view model with default logger.
    /// </summary>
    /// <remarks>For design-time and testing scenarios.</remarks>
    public AudioSettingsViewModel() : this(null, CreateNullLogger())
    {
    }

    partial void OnIsAudioEnabledChanged(bool value)
    {
        _audioService?.SetEnabled(value);
        _logger.LogDebug("Audio enabled: {Enabled}", value);
    }

    partial void OnMasterVolumeChanged(float value)
    {
        _audioService?.SetChannelVolume(AudioChannel.Master, value);
        OnPropertyChanged(nameof(MasterVolumePercent));
        _logger.LogDebug("Master volume: {Volume:P0}", value);
    }

    partial void OnMusicVolumeChanged(float value)
    {
        _audioService?.SetChannelVolume(AudioChannel.Music, value);
        OnPropertyChanged(nameof(MusicVolumePercent));
        _logger.LogDebug("Music volume: {Volume:P0}", value);
    }

    partial void OnEffectsVolumeChanged(float value)
    {
        _audioService?.SetChannelVolume(AudioChannel.Effects, value);
        OnPropertyChanged(nameof(EffectsVolumePercent));
        _logger.LogDebug("Effects volume: {Volume:P0}", value);
    }

    partial void OnUiVolumeChanged(float value)
    {
        _audioService?.SetChannelVolume(AudioChannel.UI, value);
        OnPropertyChanged(nameof(UiVolumePercent));
        _logger.LogDebug("UI volume: {Volume:P0}", value);
    }

    partial void OnIsMutedChanged(bool value)
    {
        _audioService?.SetChannelMuted(AudioChannel.Master, value);
        _logger.LogDebug("Audio muted: {Muted}", value);
    }

    /// <summary>
    /// Loads settings from the provided AudioSettings.
    /// </summary>
    /// <param name="settings">Settings to load.</param>
    public void LoadSettings(AudioSettings settings)
    {
        _savedSettings = settings.Clone();

        IsAudioEnabled = settings.Enabled;
        MasterVolume = settings.MasterVolume;
        MusicVolume = settings.MusicVolume;
        EffectsVolume = settings.EffectsVolume;
        UiVolume = settings.UiVolume;
        IsMuted = settings.Muted;
        TuiBellEnabled = settings.TuiBellEnabled;

        _logger.LogDebug("Loaded audio settings");
    }

    /// <summary>
    /// Gets the current settings as an AudioSettings object.
    /// </summary>
    /// <returns>Current audio settings.</returns>
    public AudioSettings GetSettings() => new()
    {
        Enabled = IsAudioEnabled,
        MasterVolume = MasterVolume,
        MusicVolume = MusicVolume,
        EffectsVolume = EffectsVolume,
        UiVolume = UiVolume,
        Muted = IsMuted,
        TuiBellEnabled = TuiBellEnabled
    };

    /// <summary>
    /// Saves the current settings.
    /// </summary>
    public void SaveSettings()
    {
        _savedSettings = GetSettings();
        _logger.LogInformation("Saved audio settings");
    }

    /// <summary>
    /// Reverts to the last saved settings.
    /// </summary>
    public void RevertSettings()
    {
        LoadSettings(_savedSettings);
        _logger.LogDebug("Reverted audio settings to saved values");
    }

    private static ILogger<AudioSettingsViewModel> CreateNullLogger() =>
        Microsoft.Extensions.Logging.Abstractions.NullLogger<AudioSettingsViewModel>.Instance;
}
