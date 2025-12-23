namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for managing user settings persistence (v0.3.10a).
/// Handles loading, saving, and resetting game preferences to/from options.json.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Loads settings from the options.json file and applies them to GameSettings.
    /// Creates default settings file if it doesn't exist.
    /// Validates and clamps values to valid ranges.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Saves the current GameSettings values to options.json.
    /// Creates the data directory if it doesn't exist.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Resets all settings to their default values and saves to disk.
    /// </summary>
    Task ResetToDefaultsAsync();
}
