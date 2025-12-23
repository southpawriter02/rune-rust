namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the type of a setting control in the Options screen (v0.3.10b).
/// Determines how the setting is displayed and modified.
/// </summary>
public enum SettingType
{
    /// <summary>
    /// Boolean on/off toggle. Modified with Enter/Space.
    /// </summary>
    Toggle,

    /// <summary>
    /// Numeric slider with min/max bounds. Modified with Left/Right arrows.
    /// </summary>
    Slider,

    /// <summary>
    /// Enum cycle through predefined values. Modified with Left/Right arrows.
    /// </summary>
    Enum,

    /// <summary>
    /// Action button (e.g., Reset to Defaults). Triggered with Enter/Space.
    /// </summary>
    Action
}
