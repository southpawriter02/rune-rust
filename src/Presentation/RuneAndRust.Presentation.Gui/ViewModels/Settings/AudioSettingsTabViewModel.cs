namespace RuneAndRust.Presentation.Gui.ViewModels.Settings;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;

/// <summary>
/// ViewModel for the Audio settings tab.
/// </summary>
public partial class AudioSettingsTabViewModel : ViewModelBase
{
    private readonly Action? _onChanged;

    [ObservableProperty] private int _masterVolume = 75;
    [ObservableProperty] private int _musicVolume = 50;
    [ObservableProperty] private int _sfxVolume = 85;
    [ObservableProperty] private bool _enableSoundEffects = true;
    [ObservableProperty] private bool _muteAll;

    /// <summary>Creates from settings with change callback.</summary>
    public AudioSettingsTabViewModel(AudioSettings settings, Action onChanged)
    {
        _onChanged = onChanged;
        MasterVolume = settings.MasterVolume;
        MusicVolume = settings.MusicVolume;
        SfxVolume = settings.SfxVolume;
        EnableSoundEffects = settings.EnableSoundEffects;
        MuteAll = settings.MuteAll;
    }

    /// <summary>Design-time constructor.</summary>
    public AudioSettingsTabViewModel() { }

    /// <summary>Converts to settings model.</summary>
    public AudioSettings ToSettings() => new()
    {
        MasterVolume = MasterVolume,
        MusicVolume = MusicVolume,
        SfxVolume = SfxVolume,
        EnableSoundEffects = EnableSoundEffects,
        MuteAll = MuteAll
    };

    partial void OnMasterVolumeChanged(int value) { Log.Debug("Master volume: {Vol}", value); _onChanged?.Invoke(); }
    partial void OnMusicVolumeChanged(int value) { Log.Debug("Music volume: {Vol}", value); _onChanged?.Invoke(); }
    partial void OnSfxVolumeChanged(int value) { Log.Debug("SFX volume: {Vol}", value); _onChanged?.Invoke(); }
    partial void OnEnableSoundEffectsChanged(bool value) { _onChanged?.Invoke(); }
    partial void OnMuteAllChanged(bool value) { _onChanged?.Invoke(); }
}
