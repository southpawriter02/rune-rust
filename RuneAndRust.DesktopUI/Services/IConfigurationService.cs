using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service for managing UI configuration and user preferences.
/// Full implementation in v0.43.18.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Loads user configuration from storage.
    /// </summary>
    UIConfiguration LoadConfiguration();

    /// <summary>
    /// Saves user configuration to storage.
    /// </summary>
    void SaveConfiguration(UIConfiguration config);

    /// <summary>
    /// Validates configuration for correctness.
    /// </summary>
    Task<bool> ValidateConfigurationAsync();
}

/// <summary>
/// UI configuration model (stub for v0.43.1).
/// Full implementation in v0.43.18.
/// </summary>
public class UIConfiguration
{
    public float MasterVolume { get; set; } = 1.0f;
    public float SFXVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 1.0f;
    public WindowMode WindowMode { get; set; } = WindowMode.Windowed;
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 720;
    public float UIScale { get; set; } = 1.0f;
}

/// <summary>
/// Window display mode.
/// </summary>
public enum WindowMode
{
    Windowed,
    Borderless,
    Fullscreen
}
