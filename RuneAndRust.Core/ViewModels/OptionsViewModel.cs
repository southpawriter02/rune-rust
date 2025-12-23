using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Mutable state container for the Options screen UI (v0.3.10b, extended v0.3.10c).
/// Updated in real-time as user navigates and modifies settings.
/// Unlike other ViewModels, this is mutable because settings are modified during the modal loop.
/// </summary>
public class OptionsViewModel
{
    /// <summary>
    /// Gets or sets the currently active tab.
    /// </summary>
    public OptionsTab ActiveTab { get; set; } = OptionsTab.General;

    /// <summary>
    /// Gets or sets the currently selected setting index within the active tab (0-based).
    /// </summary>
    public int SelectedIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets the list of setting items for the current tab.
    /// Refreshed when the tab changes or values are modified.
    /// </summary>
    public List<SettingItemView> CurrentItems { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of key binding items for the Controls tab (v0.3.10c).
    /// Refreshed when bindings are modified.
    /// </summary>
    public List<BindingItemView> Bindings { get; set; } = new();

    /// <summary>
    /// Gets or sets whether motion/animation effects are reduced.
    /// </summary>
    public bool ReduceMotion { get; set; }

    /// <summary>
    /// Gets or sets the current theme as an integer (maps to ThemeType enum).
    /// </summary>
    public int Theme { get; set; }

    /// <summary>
    /// Gets or sets the text display speed (10-200%).
    /// </summary>
    public int TextSpeed { get; set; }

    /// <summary>
    /// Gets or sets the master audio volume (0-100%).
    /// </summary>
    public int MasterVolume { get; set; }

    /// <summary>
    /// Gets or sets the autosave interval in minutes (1-60).
    /// </summary>
    public int AutosaveIntervalMinutes { get; set; }
}

/// <summary>
/// Display-ready representation of a single setting row (v0.3.10b).
/// Used by the renderer to display settings with appropriate controls.
/// </summary>
/// <param name="Name">Display name of the setting.</param>
/// <param name="ValueDisplay">Pre-formatted display string for the current value.</param>
/// <param name="Type">The type of control used to modify this setting.</param>
/// <param name="IsSelected">Whether this setting is currently selected for modification.</param>
/// <param name="MinValue">Minimum value for Slider types (null for others).</param>
/// <param name="MaxValue">Maximum value for Slider types (null for others).</param>
/// <param name="Step">Step increment for Slider types (null for others).</param>
/// <param name="PropertyName">Internal property name for binding (e.g., "MasterVolume").</param>
public record SettingItemView(
    string Name,
    string ValueDisplay,
    SettingType Type,
    bool IsSelected,
    int? MinValue = null,
    int? MaxValue = null,
    int? Step = null,
    string? PropertyName = null
);

/// <summary>
/// Display-ready representation of a key binding row (v0.3.10c).
/// Used by the renderer to display key bindings in the Controls tab.
/// </summary>
/// <param name="ActionName">Human-readable name of the action (e.g., "Move North").</param>
/// <param name="KeyDisplay">Formatted display string for the bound key (e.g., "[cyan]N[/]").</param>
/// <param name="Command">Internal command string for binding (e.g., "north").</param>
/// <param name="Category">Category grouping for visual organization (e.g., "Movement").</param>
/// <param name="IsSelected">Whether this binding is currently selected.</param>
/// <param name="IsUnbound">Whether this action has no key bound.</param>
public record BindingItemView(
    string ActionName,
    string KeyDisplay,
    string Command,
    string Category,
    bool IsSelected,
    bool IsUnbound
);
