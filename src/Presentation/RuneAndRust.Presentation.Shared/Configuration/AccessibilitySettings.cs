using RuneAndRust.Presentation.Shared.Enums;

namespace RuneAndRust.Presentation.Shared.Configuration;

/// <summary>
/// Configuration settings for accessibility features.
/// </summary>
/// <remarks>
/// <para>These settings are persisted to <c>settings.json</c> under the "Accessibility" section
/// and loaded on application startup.</para>
/// <para>All settings have sensible defaults that work for users without accessibility needs
/// while being easily configurable for users who require accommodations.</para>
/// </remarks>
/// <example>
/// <code>
/// // In settings.json:
/// {
///   "Accessibility": {
///     "ColorBlindMode": "None",
///     "HighContrastEnabled": false,
///     "ReducedMotionEnabled": false,
///     "ScreenReaderLabelsEnabled": true,
///     "ShowKeyboardHints": true
///   }
/// }
/// </code>
/// </example>
public class AccessibilitySettings
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COLOR VISION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color blind mode for color adjustments.
    /// </summary>
    /// <remarks>
    /// <para>Used by <see cref="Interfaces.IAccessibilityService"/> to transform colors
    /// for users with color vision deficiencies.</para>
    /// <para>Default: <see cref="Enums.ColorBlindMode.None"/> (no adjustment).</para>
    /// </remarks>
    public ColorBlindMode ColorBlindMode { get; set; } = ColorBlindMode.None;

    // ═══════════════════════════════════════════════════════════════════════════
    // VISUAL SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets whether high contrast mode is enabled.
    /// </summary>
    /// <remarks>
    /// <para>When enabled, increases the contrast between foreground and background colors
    /// to improve visibility for users with low vision.</para>
    /// <para>Default: <c>false</c>.</para>
    /// </remarks>
    public bool HighContrastEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to reduce or disable animations.
    /// </summary>
    /// <remarks>
    /// <para>When enabled, animations are disabled or significantly shortened
    /// to accommodate users with vestibular disorders or motion sensitivity.</para>
    /// <para>Checked by <see cref="Utilities.AnimationOptimizer"/> to determine animation behavior.</para>
    /// <para>Default: <c>false</c>.</para>
    /// </remarks>
    public bool ReducedMotionEnabled { get; set; } = false;

    // ═══════════════════════════════════════════════════════════════════════════
    // SCREEN READER SUPPORT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets whether screen reader text labels are enabled.
    /// </summary>
    /// <remarks>
    /// <para>When enabled, UI elements include descriptive text labels for screen readers
    /// via ARIA-like properties or platform-specific accessibility APIs.</para>
    /// <para>Default: <c>true</c>.</para>
    /// </remarks>
    public bool ScreenReaderLabelsEnabled { get; set; } = true;

    // ═══════════════════════════════════════════════════════════════════════════
    // KEYBOARD NAVIGATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets whether keyboard shortcut hints are shown on UI elements.
    /// </summary>
    /// <remarks>
    /// <para>When enabled, UI elements display their associated keyboard shortcuts
    /// (e.g., "[I] Inventory" or "[Esc] Close") to assist keyboard-only navigation.</para>
    /// <para>Default: <c>true</c>.</para>
    /// </remarks>
    public bool ShowKeyboardHints { get; set; } = true;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates default accessibility settings.
    /// </summary>
    /// <returns>A new instance with default values.</returns>
    public static AccessibilitySettings CreateDefault()
    {
        return new AccessibilitySettings();
    }

    /// <summary>
    /// Creates accessibility settings optimized for maximum accessibility.
    /// </summary>
    /// <returns>A new instance with all accessibility features enabled.</returns>
    /// <remarks>
    /// Enables high contrast, reduced motion, screen reader labels, and keyboard hints.
    /// Color blind mode remains at None as users should select their specific mode.
    /// </remarks>
    public static AccessibilitySettings CreateMaximumAccessibility()
    {
        return new AccessibilitySettings
        {
            ColorBlindMode = ColorBlindMode.None, // User should select specific mode
            HighContrastEnabled = true,
            ReducedMotionEnabled = true,
            ScreenReaderLabelsEnabled = true,
            ShowKeyboardHints = true
        };
    }

    /// <summary>
    /// Creates a copy of the current settings.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    public AccessibilitySettings Clone()
    {
        return new AccessibilitySettings
        {
            ColorBlindMode = ColorBlindMode,
            HighContrastEnabled = HighContrastEnabled,
            ReducedMotionEnabled = ReducedMotionEnabled,
            ScreenReaderLabelsEnabled = ScreenReaderLabelsEnabled,
            ShowKeyboardHints = ShowKeyboardHints
        };
    }
}
