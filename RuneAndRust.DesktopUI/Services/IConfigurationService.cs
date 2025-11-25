using System;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.18: Service for managing UI configuration and user preferences.
/// Handles graphics, audio, gameplay, and accessibility settings.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Loads user configuration from storage.
    /// </summary>
    GameConfiguration LoadConfiguration();

    /// <summary>
    /// Saves user configuration to storage.
    /// </summary>
    void SaveConfiguration(GameConfiguration config);

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Validates configuration for correctness.
    /// </summary>
    Task<bool> ValidateConfigurationAsync();

    /// <summary>
    /// Gets the default configuration values.
    /// </summary>
    GameConfiguration GetDefaultConfiguration();
}

#region Configuration Models

/// <summary>
/// v0.43.18: Complete game configuration containing all settings categories.
/// </summary>
public class GameConfiguration
{
    /// <summary>
    /// Graphics settings.
    /// </summary>
    public GraphicsConfig Graphics { get; set; } = new();

    /// <summary>
    /// Audio settings.
    /// </summary>
    public AudioConfig Audio { get; set; } = new();

    /// <summary>
    /// Gameplay settings.
    /// </summary>
    public GameplayConfig Gameplay { get; set; } = new();

    /// <summary>
    /// Accessibility settings.
    /// </summary>
    public AccessibilityConfig Accessibility { get; set; } = new();

    /// <summary>
    /// Control settings.
    /// </summary>
    public ControlsConfig Controls { get; set; } = new();
}

/// <summary>
/// v0.43.18: Graphics configuration settings.
/// </summary>
public class GraphicsConfig
{
    /// <summary>
    /// Whether the game runs in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; } = false;

    /// <summary>
    /// Whether V-Sync is enabled.
    /// </summary>
    public bool VSync { get; set; } = true;

    /// <summary>
    /// Target frames per second.
    /// </summary>
    public int TargetFPS { get; set; } = 60;

    /// <summary>
    /// Window mode (Windowed, Borderless, Fullscreen).
    /// </summary>
    public WindowMode WindowMode { get; set; } = WindowMode.Windowed;

    /// <summary>
    /// Window width in pixels.
    /// </summary>
    public int WindowWidth { get; set; } = 1280;

    /// <summary>
    /// Window height in pixels.
    /// </summary>
    public int WindowHeight { get; set; } = 720;

    /// <summary>
    /// Selected resolution string (e.g., "1920x1080").
    /// </summary>
    public string Resolution { get; set; } = "1280x720";

    /// <summary>
    /// Quality level for visual effects (0 = Low, 1 = Medium, 2 = High).
    /// </summary>
    public int QualityLevel { get; set; } = 2;

    /// <summary>
    /// Whether particle effects are enabled.
    /// </summary>
    public bool ParticleEffects { get; set; } = true;

    /// <summary>
    /// Whether screen shake effects are enabled.
    /// </summary>
    public bool ScreenShake { get; set; } = true;
}

/// <summary>
/// v0.43.18: Audio configuration settings.
/// </summary>
public class AudioConfig
{
    /// <summary>
    /// Master volume level (0.0 - 1.0).
    /// </summary>
    public float MasterVolume { get; set; } = 1.0f;

    /// <summary>
    /// Music volume level (0.0 - 1.0).
    /// </summary>
    public float MusicVolume { get; set; } = 0.8f;

    /// <summary>
    /// Sound effects volume level (0.0 - 1.0).
    /// </summary>
    public float SFXVolume { get; set; } = 1.0f;

    /// <summary>
    /// UI sounds volume level (0.0 - 1.0).
    /// </summary>
    public float UIVolume { get; set; } = 1.0f;

    /// <summary>
    /// Ambient sounds volume level (0.0 - 1.0).
    /// </summary>
    public float AmbientVolume { get; set; } = 0.7f;

    /// <summary>
    /// Whether all audio is muted.
    /// </summary>
    public bool IsMuted { get; set; } = false;
}

/// <summary>
/// v0.43.18: Gameplay configuration settings.
/// </summary>
public class GameplayConfig
{
    /// <summary>
    /// Whether auto-save is enabled.
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// Auto-save interval in minutes.
    /// </summary>
    public int AutoSaveInterval { get; set; } = 5;

    /// <summary>
    /// Whether to show floating damage numbers.
    /// </summary>
    public bool ShowDamageNumbers { get; set; } = true;

    /// <summary>
    /// Whether to show hit confirmation effects.
    /// </summary>
    public bool ShowHitConfirmation { get; set; } = true;

    /// <summary>
    /// Whether to pause the game when the window loses focus.
    /// </summary>
    public bool PauseOnFocusLost { get; set; } = true;

    /// <summary>
    /// Whether to show combat grid coordinates.
    /// </summary>
    public bool ShowGridCoordinates { get; set; } = false;

    /// <summary>
    /// Combat speed multiplier (0.5 = slow, 1.0 = normal, 2.0 = fast).
    /// </summary>
    public float CombatSpeed { get; set; } = 1.0f;

    /// <summary>
    /// Whether to skip combat animations when holding a key.
    /// </summary>
    public bool AllowAnimationSkip { get; set; } = true;

    /// <summary>
    /// Whether to show tutorial hints.
    /// </summary>
    public bool ShowTutorialHints { get; set; } = true;

    /// <summary>
    /// Whether to confirm before ending turn.
    /// </summary>
    public bool ConfirmEndTurn { get; set; } = false;
}

/// <summary>
/// v0.43.18: Accessibility configuration settings.
/// </summary>
public class AccessibilityConfig
{
    /// <summary>
    /// UI scale factor (0.8 - 1.5).
    /// </summary>
    public float UIScale { get; set; } = 1.0f;

    /// <summary>
    /// Whether colorblind mode is enabled.
    /// </summary>
    public bool ColorblindMode { get; set; } = false;

    /// <summary>
    /// Type of colorblind assistance.
    /// </summary>
    public ColorblindType ColorblindType { get; set; } = ColorblindType.None;

    /// <summary>
    /// Whether to reduce motion effects.
    /// </summary>
    public bool ReducedMotion { get; set; } = false;

    /// <summary>
    /// Whether to show high contrast UI.
    /// </summary>
    public bool HighContrast { get; set; } = false;

    /// <summary>
    /// Font size multiplier (0.8 - 1.5).
    /// </summary>
    public float FontScale { get; set; } = 1.0f;

    /// <summary>
    /// Whether to show subtitles for audio.
    /// </summary>
    public bool ShowSubtitles { get; set; } = false;

    /// <summary>
    /// Whether to enable screen reader support.
    /// </summary>
    public bool ScreenReaderSupport { get; set; } = false;

    /// <summary>
    /// Whether to show additional visual cues for audio events.
    /// </summary>
    public bool VisualAudioCues { get; set; } = false;
}

/// <summary>
/// v0.43.18: Controls configuration settings.
/// </summary>
public class ControlsConfig
{
    /// <summary>
    /// Whether to show keyboard shortcut hints.
    /// </summary>
    public bool ShowKeyboardHints { get; set; } = true;

    /// <summary>
    /// Mouse sensitivity multiplier.
    /// </summary>
    public float MouseSensitivity { get; set; } = 1.0f;

    /// <summary>
    /// Whether to invert scroll direction.
    /// </summary>
    public bool InvertScroll { get; set; } = false;

    /// <summary>
    /// Whether to enable edge scrolling for the map.
    /// </summary>
    public bool EdgeScrolling { get; set; } = true;

    /// <summary>
    /// Whether to use double-click to confirm actions.
    /// </summary>
    public bool DoubleClickConfirm { get; set; } = false;
}

#endregion

#region Enums

/// <summary>
/// Window display mode.
/// </summary>
public enum WindowMode
{
    Windowed,
    Borderless,
    Fullscreen
}

/// <summary>
/// v0.43.18: Types of colorblind assistance.
/// </summary>
public enum ColorblindType
{
    None,
    Protanopia,     // Red-blind
    Deuteranopia,   // Green-blind
    Tritanopia      // Blue-blind
}

#endregion

#region Legacy Support

/// <summary>
/// Legacy UI configuration model for backwards compatibility.
/// Use GameConfiguration for new code.
/// </summary>
[Obsolete("Use GameConfiguration instead. This class is maintained for backwards compatibility.")]
public class UIConfiguration
{
    public float MasterVolume { get; set; } = 1.0f;
    public float SFXVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 1.0f;
    public WindowMode WindowMode { get; set; } = WindowMode.Windowed;
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 720;
    public float UIScale { get; set; } = 1.0f;

    /// <summary>
    /// Converts legacy configuration to new GameConfiguration format.
    /// </summary>
    public GameConfiguration ToGameConfiguration()
    {
        return new GameConfiguration
        {
            Graphics = new GraphicsConfig
            {
                WindowMode = WindowMode,
                WindowWidth = WindowWidth,
                WindowHeight = WindowHeight,
                Resolution = $"{WindowWidth}x{WindowHeight}"
            },
            Audio = new AudioConfig
            {
                MasterVolume = MasterVolume,
                SFXVolume = SFXVolume,
                MusicVolume = MusicVolume
            },
            Accessibility = new AccessibilityConfig
            {
                UIScale = UIScale
            }
        };
    }
}

#endregion
