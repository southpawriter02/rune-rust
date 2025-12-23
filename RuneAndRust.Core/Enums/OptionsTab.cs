namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the available tabs in the Options screen (v0.3.10b, extended v0.3.10c).
/// Each tab groups related settings for easier navigation.
/// </summary>
public enum OptionsTab
{
    /// <summary>
    /// General game settings (autosave interval, reset to defaults).
    /// </summary>
    General,

    /// <summary>
    /// Display settings (theme, reduce motion, text speed).
    /// </summary>
    Display,

    /// <summary>
    /// Audio settings (master volume).
    /// </summary>
    Audio,

    /// <summary>
    /// Controls settings for key rebinding (v0.3.10c).
    /// </summary>
    Controls
}
