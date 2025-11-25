using ReactiveUI;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.43.18: View model for the settings menu.
/// Manages graphics, audio, gameplay, accessibility, and control settings.
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly IAudioService _audioService;
    private readonly INavigationService? _navigationService;

    private GameConfiguration _originalConfig = new();
    private bool _hasUnsavedChanges;

    #region Graphics Settings

    private bool _isFullscreen;
    private bool _vsyncEnabled = true;
    private int _selectedResolutionIndex;
    private int _targetFPS = 60;
    private int _qualityLevel = 2;
    private bool _particleEffects = true;
    private bool _screenShake = true;

    /// <summary>
    /// Whether fullscreen mode is enabled.
    /// </summary>
    public bool IsFullscreen
    {
        get => _isFullscreen;
        set
        {
            this.RaiseAndSetIfChanged(ref _isFullscreen, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether V-Sync is enabled.
    /// </summary>
    public bool VsyncEnabled
    {
        get => _vsyncEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _vsyncEnabled, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Index of selected resolution in AvailableResolutions.
    /// </summary>
    public int SelectedResolutionIndex
    {
        get => _selectedResolutionIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedResolutionIndex, value);
            this.RaisePropertyChanged(nameof(SelectedResolution));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Currently selected resolution string.
    /// </summary>
    public string SelectedResolution =>
        SelectedResolutionIndex >= 0 && SelectedResolutionIndex < AvailableResolutions.Count
            ? AvailableResolutions[SelectedResolutionIndex]
            : "1280x720";

    /// <summary>
    /// Target FPS (30-240).
    /// </summary>
    public int TargetFPS
    {
        get => _targetFPS;
        set
        {
            this.RaiseAndSetIfChanged(ref _targetFPS, Math.Clamp(value, 30, 240));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Quality level (0 = Low, 1 = Medium, 2 = High).
    /// </summary>
    public int QualityLevel
    {
        get => _qualityLevel;
        set
        {
            this.RaiseAndSetIfChanged(ref _qualityLevel, Math.Clamp(value, 0, 2));
            this.RaisePropertyChanged(nameof(QualityLevelText));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Quality level display text.
    /// </summary>
    public string QualityLevelText => QualityLevel switch
    {
        0 => "Low",
        1 => "Medium",
        2 => "High",
        _ => "Medium"
    };

    /// <summary>
    /// Whether particle effects are enabled.
    /// </summary>
    public bool ParticleEffects
    {
        get => _particleEffects;
        set
        {
            this.RaiseAndSetIfChanged(ref _particleEffects, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether screen shake effects are enabled.
    /// </summary>
    public bool ScreenShake
    {
        get => _screenShake;
        set
        {
            this.RaiseAndSetIfChanged(ref _screenShake, value);
            MarkAsChanged();
        }
    }

    #endregion

    #region Audio Settings

    private float _masterVolume = 1.0f;
    private float _musicVolume = 0.8f;
    private float _sfxVolume = 1.0f;
    private float _uiVolume = 1.0f;
    private float _ambientVolume = 0.7f;
    private bool _isMuted;

    /// <summary>
    /// Master volume (0.0 - 1.0).
    /// </summary>
    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            this.RaiseAndSetIfChanged(ref _masterVolume, Math.Clamp(value, 0f, 1f));
            _audioService?.SetMasterVolume(_masterVolume);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Music volume (0.0 - 1.0).
    /// </summary>
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            this.RaiseAndSetIfChanged(ref _musicVolume, Math.Clamp(value, 0f, 1f));
            _audioService?.SetMusicVolume(_musicVolume);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// SFX volume (0.0 - 1.0).
    /// </summary>
    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            this.RaiseAndSetIfChanged(ref _sfxVolume, Math.Clamp(value, 0f, 1f));
            _audioService?.SetSFXVolume(_sfxVolume);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// UI sounds volume (0.0 - 1.0).
    /// </summary>
    public float UIVolume
    {
        get => _uiVolume;
        set
        {
            this.RaiseAndSetIfChanged(ref _uiVolume, Math.Clamp(value, 0f, 1f));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Ambient sounds volume (0.0 - 1.0).
    /// </summary>
    public float AmbientVolume
    {
        get => _ambientVolume;
        set
        {
            this.RaiseAndSetIfChanged(ref _ambientVolume, Math.Clamp(value, 0f, 1f));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether audio is muted.
    /// </summary>
    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            this.RaiseAndSetIfChanged(ref _isMuted, value);
            _audioService?.SetMuted(_isMuted);
            MarkAsChanged();
        }
    }

    #endregion

    #region Gameplay Settings

    private bool _autoSaveEnabled = true;
    private int _autoSaveInterval = 5;
    private bool _showDamageNumbers = true;
    private bool _showHitConfirmation = true;
    private bool _pauseOnFocusLost = true;
    private bool _showGridCoordinates;
    private float _combatSpeed = 1.0f;
    private bool _allowAnimationSkip = true;
    private bool _showTutorialHints = true;
    private bool _confirmEndTurn;

    /// <summary>
    /// Whether auto-save is enabled.
    /// </summary>
    public bool AutoSaveEnabled
    {
        get => _autoSaveEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _autoSaveEnabled, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Auto-save interval in minutes (1-30).
    /// </summary>
    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set
        {
            this.RaiseAndSetIfChanged(ref _autoSaveInterval, Math.Clamp(value, 1, 30));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether to show damage numbers.
    /// </summary>
    public bool ShowDamageNumbers
    {
        get => _showDamageNumbers;
        set
        {
            this.RaiseAndSetIfChanged(ref _showDamageNumbers, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether to show hit confirmation effects.
    /// </summary>
    public bool ShowHitConfirmation
    {
        get => _showHitConfirmation;
        set
        {
            this.RaiseAndSetIfChanged(ref _showHitConfirmation, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether to pause when window loses focus.
    /// </summary>
    public bool PauseOnFocusLost
    {
        get => _pauseOnFocusLost;
        set
        {
            this.RaiseAndSetIfChanged(ref _pauseOnFocusLost, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether to show grid coordinates.
    /// </summary>
    public bool ShowGridCoordinates
    {
        get => _showGridCoordinates;
        set
        {
            this.RaiseAndSetIfChanged(ref _showGridCoordinates, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Combat speed multiplier (0.5 - 2.0).
    /// </summary>
    public float CombatSpeed
    {
        get => _combatSpeed;
        set
        {
            this.RaiseAndSetIfChanged(ref _combatSpeed, Math.Clamp(value, 0.5f, 2.0f));
            this.RaisePropertyChanged(nameof(CombatSpeedText));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Combat speed display text.
    /// </summary>
    public string CombatSpeedText => CombatSpeed switch
    {
        <= 0.6f => "Slow",
        <= 1.1f => "Normal",
        <= 1.6f => "Fast",
        _ => "Very Fast"
    };

    /// <summary>
    /// Whether animation skip is allowed.
    /// </summary>
    public bool AllowAnimationSkip
    {
        get => _allowAnimationSkip;
        set
        {
            this.RaiseAndSetIfChanged(ref _allowAnimationSkip, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether to show tutorial hints.
    /// </summary>
    public bool ShowTutorialHints
    {
        get => _showTutorialHints;
        set
        {
            this.RaiseAndSetIfChanged(ref _showTutorialHints, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether to confirm before ending turn.
    /// </summary>
    public bool ConfirmEndTurn
    {
        get => _confirmEndTurn;
        set
        {
            this.RaiseAndSetIfChanged(ref _confirmEndTurn, value);
            MarkAsChanged();
        }
    }

    #endregion

    #region Accessibility Settings

    private float _uiScale = 1.0f;
    private bool _colorblindMode;
    private int _colorblindTypeIndex;
    private bool _reducedMotion;
    private bool _highContrast;
    private float _fontScale = 1.0f;
    private bool _showSubtitles;
    private bool _screenReaderSupport;
    private bool _visualAudioCues;

    /// <summary>
    /// UI scale (0.8 - 1.5).
    /// </summary>
    public float UIScale
    {
        get => _uiScale;
        set
        {
            this.RaiseAndSetIfChanged(ref _uiScale, Math.Clamp(value, 0.8f, 1.5f));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether colorblind mode is enabled.
    /// </summary>
    public bool ColorblindMode
    {
        get => _colorblindMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _colorblindMode, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Selected colorblind type index.
    /// </summary>
    public int ColorblindTypeIndex
    {
        get => _colorblindTypeIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _colorblindTypeIndex, Math.Clamp(value, 0, 3));
            this.RaisePropertyChanged(nameof(SelectedColorblindType));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Currently selected colorblind type.
    /// </summary>
    public ColorblindType SelectedColorblindType => (ColorblindType)ColorblindTypeIndex;

    /// <summary>
    /// Whether reduced motion is enabled.
    /// </summary>
    public bool ReducedMotion
    {
        get => _reducedMotion;
        set
        {
            this.RaiseAndSetIfChanged(ref _reducedMotion, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether high contrast mode is enabled.
    /// </summary>
    public bool HighContrast
    {
        get => _highContrast;
        set
        {
            this.RaiseAndSetIfChanged(ref _highContrast, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Font scale (0.8 - 1.5).
    /// </summary>
    public float FontScale
    {
        get => _fontScale;
        set
        {
            this.RaiseAndSetIfChanged(ref _fontScale, Math.Clamp(value, 0.8f, 1.5f));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether subtitles are shown.
    /// </summary>
    public bool ShowSubtitles
    {
        get => _showSubtitles;
        set
        {
            this.RaiseAndSetIfChanged(ref _showSubtitles, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether screen reader support is enabled.
    /// </summary>
    public bool ScreenReaderSupport
    {
        get => _screenReaderSupport;
        set
        {
            this.RaiseAndSetIfChanged(ref _screenReaderSupport, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether visual audio cues are shown.
    /// </summary>
    public bool VisualAudioCues
    {
        get => _visualAudioCues;
        set
        {
            this.RaiseAndSetIfChanged(ref _visualAudioCues, value);
            MarkAsChanged();
        }
    }

    #endregion

    #region Controls Settings

    private bool _showKeyboardHints = true;
    private float _mouseSensitivity = 1.0f;
    private bool _invertScroll;
    private bool _edgeScrolling = true;
    private bool _doubleClickConfirm;

    /// <summary>
    /// Whether keyboard hints are shown.
    /// </summary>
    public bool ShowKeyboardHints
    {
        get => _showKeyboardHints;
        set
        {
            this.RaiseAndSetIfChanged(ref _showKeyboardHints, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Mouse sensitivity (0.1 - 3.0).
    /// </summary>
    public float MouseSensitivity
    {
        get => _mouseSensitivity;
        set
        {
            this.RaiseAndSetIfChanged(ref _mouseSensitivity, Math.Clamp(value, 0.1f, 3.0f));
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether scroll direction is inverted.
    /// </summary>
    public bool InvertScroll
    {
        get => _invertScroll;
        set
        {
            this.RaiseAndSetIfChanged(ref _invertScroll, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether edge scrolling is enabled.
    /// </summary>
    public bool EdgeScrolling
    {
        get => _edgeScrolling;
        set
        {
            this.RaiseAndSetIfChanged(ref _edgeScrolling, value);
            MarkAsChanged();
        }
    }

    /// <summary>
    /// Whether double-click is required to confirm actions.
    /// </summary>
    public bool DoubleClickConfirm
    {
        get => _doubleClickConfirm;
        set
        {
            this.RaiseAndSetIfChanged(ref _doubleClickConfirm, value);
            MarkAsChanged();
        }
    }

    #endregion

    #region UI State

    /// <summary>
    /// Whether there are unsaved changes.
    /// </summary>
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => this.RaiseAndSetIfChanged(ref _hasUnsavedChanges, value);
    }

    /// <summary>
    /// Available screen resolutions.
    /// </summary>
    public ObservableCollection<string> AvailableResolutions { get; } = new();

    /// <summary>
    /// Available colorblind types.
    /// </summary>
    public ObservableCollection<string> ColorblindTypes { get; } = new()
    {
        "None",
        "Protanopia (Red-blind)",
        "Deuteranopia (Green-blind)",
        "Tritanopia (Blue-blind)"
    };

    #endregion

    #region Commands

    /// <summary>
    /// Command to save settings.
    /// </summary>
    public ICommand SaveSettingsCommand { get; }

    /// <summary>
    /// Command to reset settings to defaults.
    /// </summary>
    public ICommand ResetToDefaultsCommand { get; }

    /// <summary>
    /// Command to cancel changes.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to go back to previous view.
    /// </summary>
    public ICommand BackCommand { get; }

    #endregion

    /// <summary>
    /// Creates a new instance for design-time support.
    /// </summary>
    public SettingsViewModel()
    {
        _configService = null!;
        _audioService = null!;

        SaveSettingsCommand = ReactiveCommand.Create(() => { });
        ResetToDefaultsCommand = ReactiveCommand.Create(() => { });
        CancelCommand = ReactiveCommand.Create(() => { });
        BackCommand = ReactiveCommand.Create(() => { });

        LoadAvailableResolutions();
        LoadDesignTimeData();
    }

    /// <summary>
    /// Creates a new instance with dependency injection.
    /// </summary>
    public SettingsViewModel(
        IConfigurationService configService,
        IAudioService audioService,
        INavigationService? navigationService = null)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _navigationService = navigationService;

        SaveSettingsCommand = ReactiveCommand.Create(SaveSettings);
        ResetToDefaultsCommand = ReactiveCommand.Create(ResetToDefaults);
        CancelCommand = ReactiveCommand.Create(Cancel);
        BackCommand = ReactiveCommand.Create(GoBack);

        LoadAvailableResolutions();
        LoadSettings();
    }

    /// <summary>
    /// Loads settings from configuration service.
    /// </summary>
    public void LoadSettings()
    {
        var config = _configService?.LoadConfiguration() ?? new GameConfiguration();
        _originalConfig = config;

        // Graphics
        _isFullscreen = config.Graphics.Fullscreen;
        _vsyncEnabled = config.Graphics.VSync;
        _targetFPS = config.Graphics.TargetFPS;
        _qualityLevel = config.Graphics.QualityLevel;
        _particleEffects = config.Graphics.ParticleEffects;
        _screenShake = config.Graphics.ScreenShake;

        // Find matching resolution index
        var resIndex = AvailableResolutions.IndexOf(config.Graphics.Resolution);
        _selectedResolutionIndex = resIndex >= 0 ? resIndex : 0;

        // Audio
        _masterVolume = config.Audio.MasterVolume;
        _musicVolume = config.Audio.MusicVolume;
        _sfxVolume = config.Audio.SFXVolume;
        _uiVolume = config.Audio.UIVolume;
        _ambientVolume = config.Audio.AmbientVolume;
        _isMuted = config.Audio.IsMuted;

        // Gameplay
        _autoSaveEnabled = config.Gameplay.AutoSave;
        _autoSaveInterval = config.Gameplay.AutoSaveInterval;
        _showDamageNumbers = config.Gameplay.ShowDamageNumbers;
        _showHitConfirmation = config.Gameplay.ShowHitConfirmation;
        _pauseOnFocusLost = config.Gameplay.PauseOnFocusLost;
        _showGridCoordinates = config.Gameplay.ShowGridCoordinates;
        _combatSpeed = config.Gameplay.CombatSpeed;
        _allowAnimationSkip = config.Gameplay.AllowAnimationSkip;
        _showTutorialHints = config.Gameplay.ShowTutorialHints;
        _confirmEndTurn = config.Gameplay.ConfirmEndTurn;

        // Accessibility
        _uiScale = config.Accessibility.UIScale;
        _colorblindMode = config.Accessibility.ColorblindMode;
        _colorblindTypeIndex = (int)config.Accessibility.ColorblindType;
        _reducedMotion = config.Accessibility.ReducedMotion;
        _highContrast = config.Accessibility.HighContrast;
        _fontScale = config.Accessibility.FontScale;
        _showSubtitles = config.Accessibility.ShowSubtitles;
        _screenReaderSupport = config.Accessibility.ScreenReaderSupport;
        _visualAudioCues = config.Accessibility.VisualAudioCues;

        // Controls
        _showKeyboardHints = config.Controls.ShowKeyboardHints;
        _mouseSensitivity = config.Controls.MouseSensitivity;
        _invertScroll = config.Controls.InvertScroll;
        _edgeScrolling = config.Controls.EdgeScrolling;
        _doubleClickConfirm = config.Controls.DoubleClickConfirm;

        // Notify all properties changed
        RaiseAllPropertiesChanged();

        HasUnsavedChanges = false;
        Console.WriteLine("[SETTINGS] Configuration loaded");
    }

    /// <summary>
    /// Saves current settings to configuration.
    /// </summary>
    private void SaveSettings()
    {
        var config = new GameConfiguration
        {
            Graphics = new GraphicsConfig
            {
                Fullscreen = IsFullscreen,
                VSync = VsyncEnabled,
                TargetFPS = TargetFPS,
                Resolution = SelectedResolution,
                QualityLevel = QualityLevel,
                ParticleEffects = ParticleEffects,
                ScreenShake = ScreenShake,
                WindowMode = IsFullscreen ? WindowMode.Fullscreen : WindowMode.Windowed
            },
            Audio = new AudioConfig
            {
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SFXVolume = SFXVolume,
                UIVolume = UIVolume,
                AmbientVolume = AmbientVolume,
                IsMuted = IsMuted
            },
            Gameplay = new GameplayConfig
            {
                AutoSave = AutoSaveEnabled,
                AutoSaveInterval = AutoSaveInterval,
                ShowDamageNumbers = ShowDamageNumbers,
                ShowHitConfirmation = ShowHitConfirmation,
                PauseOnFocusLost = PauseOnFocusLost,
                ShowGridCoordinates = ShowGridCoordinates,
                CombatSpeed = CombatSpeed,
                AllowAnimationSkip = AllowAnimationSkip,
                ShowTutorialHints = ShowTutorialHints,
                ConfirmEndTurn = ConfirmEndTurn
            },
            Accessibility = new AccessibilityConfig
            {
                UIScale = UIScale,
                ColorblindMode = ColorblindMode,
                ColorblindType = SelectedColorblindType,
                ReducedMotion = ReducedMotion,
                HighContrast = HighContrast,
                FontScale = FontScale,
                ShowSubtitles = ShowSubtitles,
                ScreenReaderSupport = ScreenReaderSupport,
                VisualAudioCues = VisualAudioCues
            },
            Controls = new ControlsConfig
            {
                ShowKeyboardHints = ShowKeyboardHints,
                MouseSensitivity = MouseSensitivity,
                InvertScroll = InvertScroll,
                EdgeScrolling = EdgeScrolling,
                DoubleClickConfirm = DoubleClickConfirm
            }
        };

        _configService.SaveConfiguration(config);
        _originalConfig = config;
        HasUnsavedChanges = false;

        Console.WriteLine("[SETTINGS] Configuration saved");

        // Play save confirmation sound
        _audioService?.PlayUISound("settings_saved");
    }

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    private void ResetToDefaults()
    {
        _configService?.ResetToDefaults();
        LoadSettings();
        Console.WriteLine("[SETTINGS] Reset to defaults");
    }

    /// <summary>
    /// Cancels changes and reverts to saved settings.
    /// </summary>
    private void Cancel()
    {
        LoadSettings();
        Console.WriteLine("[SETTINGS] Changes cancelled");
    }

    /// <summary>
    /// Navigates back to previous view.
    /// </summary>
    private void GoBack()
    {
        if (HasUnsavedChanges)
        {
            // In a full implementation, show confirmation dialog
            Console.WriteLine("[SETTINGS] Warning: Unsaved changes will be lost");
        }

        _navigationService?.GoBack();
    }

    /// <summary>
    /// Loads available screen resolutions.
    /// </summary>
    private void LoadAvailableResolutions()
    {
        AvailableResolutions.Clear();
        AvailableResolutions.Add("1280x720");
        AvailableResolutions.Add("1366x768");
        AvailableResolutions.Add("1600x900");
        AvailableResolutions.Add("1920x1080");
        AvailableResolutions.Add("2560x1440");
        AvailableResolutions.Add("3840x2160");
    }

    /// <summary>
    /// Marks settings as having unsaved changes.
    /// </summary>
    private void MarkAsChanged()
    {
        HasUnsavedChanges = true;
    }

    /// <summary>
    /// Raises property changed for all properties.
    /// </summary>
    private void RaiseAllPropertiesChanged()
    {
        // Graphics
        this.RaisePropertyChanged(nameof(IsFullscreen));
        this.RaisePropertyChanged(nameof(VsyncEnabled));
        this.RaisePropertyChanged(nameof(SelectedResolutionIndex));
        this.RaisePropertyChanged(nameof(SelectedResolution));
        this.RaisePropertyChanged(nameof(TargetFPS));
        this.RaisePropertyChanged(nameof(QualityLevel));
        this.RaisePropertyChanged(nameof(QualityLevelText));
        this.RaisePropertyChanged(nameof(ParticleEffects));
        this.RaisePropertyChanged(nameof(ScreenShake));

        // Audio
        this.RaisePropertyChanged(nameof(MasterVolume));
        this.RaisePropertyChanged(nameof(MusicVolume));
        this.RaisePropertyChanged(nameof(SFXVolume));
        this.RaisePropertyChanged(nameof(UIVolume));
        this.RaisePropertyChanged(nameof(AmbientVolume));
        this.RaisePropertyChanged(nameof(IsMuted));

        // Gameplay
        this.RaisePropertyChanged(nameof(AutoSaveEnabled));
        this.RaisePropertyChanged(nameof(AutoSaveInterval));
        this.RaisePropertyChanged(nameof(ShowDamageNumbers));
        this.RaisePropertyChanged(nameof(ShowHitConfirmation));
        this.RaisePropertyChanged(nameof(PauseOnFocusLost));
        this.RaisePropertyChanged(nameof(ShowGridCoordinates));
        this.RaisePropertyChanged(nameof(CombatSpeed));
        this.RaisePropertyChanged(nameof(CombatSpeedText));
        this.RaisePropertyChanged(nameof(AllowAnimationSkip));
        this.RaisePropertyChanged(nameof(ShowTutorialHints));
        this.RaisePropertyChanged(nameof(ConfirmEndTurn));

        // Accessibility
        this.RaisePropertyChanged(nameof(UIScale));
        this.RaisePropertyChanged(nameof(ColorblindMode));
        this.RaisePropertyChanged(nameof(ColorblindTypeIndex));
        this.RaisePropertyChanged(nameof(SelectedColorblindType));
        this.RaisePropertyChanged(nameof(ReducedMotion));
        this.RaisePropertyChanged(nameof(HighContrast));
        this.RaisePropertyChanged(nameof(FontScale));
        this.RaisePropertyChanged(nameof(ShowSubtitles));
        this.RaisePropertyChanged(nameof(ScreenReaderSupport));
        this.RaisePropertyChanged(nameof(VisualAudioCues));

        // Controls
        this.RaisePropertyChanged(nameof(ShowKeyboardHints));
        this.RaisePropertyChanged(nameof(MouseSensitivity));
        this.RaisePropertyChanged(nameof(InvertScroll));
        this.RaisePropertyChanged(nameof(EdgeScrolling));
        this.RaisePropertyChanged(nameof(DoubleClickConfirm));
    }

    /// <summary>
    /// Loads design-time sample data.
    /// </summary>
    private void LoadDesignTimeData()
    {
        _isFullscreen = false;
        _vsyncEnabled = true;
        _targetFPS = 60;
        _qualityLevel = 2;
        _selectedResolutionIndex = 3; // 1920x1080
        _masterVolume = 0.8f;
        _musicVolume = 0.6f;
        _sfxVolume = 1.0f;
        _autoSaveEnabled = true;
        _autoSaveInterval = 5;
        _uiScale = 1.0f;
    }
}
