using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Shared.Interfaces;

/// <summary>
/// Provides accessibility services for color blind support, screen reader integration,
/// and keyboard navigation hints.
/// </summary>
/// <remarks>
/// <para>This interface is implemented by platform-specific services:
/// <c>TuiAccessibilityService</c> for terminal and <c>GuiAccessibilityService</c> for Avalonia.</para>
/// <para>The service integrates with <c>IThemeService</c> to provide
/// accessible color transformations based on the user's color blind mode setting.</para>
/// <para><b>Logging:</b> Setting changes are logged at Information level,
/// color transforms and screen reader announcements at Debug level.</para>
/// </remarks>
/// <example>
/// <code>
/// // Get accessible color for current color blind mode
/// var accessibleColor = accessibilityService.GetAccessibleColor(themeColor);
/// 
/// // Announce important event to screen reader
/// accessibilityService.Announce("Combat started!", AnnouncementPriority.Important);
/// </code>
/// </example>
public interface IAccessibilityService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COLOR BLIND SUPPORT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the current color blind mode for color transformations.
    /// </summary>
    /// <remarks>
    /// <para>When set, all subsequent calls to <see cref="GetAccessibleColor"/>
    /// will use the appropriate color transformation matrix.</para>
    /// <para>Changes to this property are persisted to application settings.</para>
    /// </remarks>
    ColorBlindMode ColorBlindMode { get; set; }

    /// <summary>
    /// Transforms a theme color for the current color blind mode.
    /// </summary>
    /// <param name="color">The original color to transform.</param>
    /// <returns>
    /// The transformed color appropriate for the current color blind mode,
    /// or the original color if <see cref="ColorBlindMode"/> is <see cref="Enums.ColorBlindMode.None"/>.
    /// </returns>
    /// <remarks>
    /// Uses scientifically validated transformation matrices based on
    /// Machado, Oliveira &amp; Fernandes (2009) research.
    /// </remarks>
    ThemeColor GetAccessibleColor(ThemeColor color);

    // ═══════════════════════════════════════════════════════════════════════════
    // VISUAL SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets whether high contrast mode is enabled.
    /// </summary>
    /// <remarks>
    /// When enabled, enhances color contrast for improved visibility
    /// by increasing the difference between foreground and background colors.
    /// </remarks>
    bool IsHighContrastEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether reduced motion is preferred.
    /// </summary>
    /// <remarks>
    /// <para>When enabled, animations are disabled or significantly shortened
    /// to accommodate users with vestibular disorders or motion sensitivity.</para>
    /// <para>Checked by <see cref="Utilities.AnimationOptimizer"/> to determine
    /// whether to skip or shorten animations.</para>
    /// </remarks>
    bool IsReducedMotionEnabled { get; set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCREEN READER SUPPORT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether a screen reader is detected and active.
    /// </summary>
    /// <remarks>
    /// Detection is platform-specific:
    /// <list type="bullet">
    /// <item><description>TUI: Always returns false (console cannot detect screen readers)</description></item>
    /// <item><description>GUI: Checks system accessibility settings via Avalonia automation</description></item>
    /// </list>
    /// </remarks>
    bool IsScreenReaderActive { get; }

    /// <summary>
    /// Gets the alternative text description for a theme icon.
    /// </summary>
    /// <param name="iconKey">The icon key to get alt text for.</param>
    /// <returns>
    /// A human-readable description of the icon suitable for screen readers.
    /// </returns>
    /// <example>
    /// <code>
    /// var altText = service.GetAltText(IconKey.Health); // Returns "Health"
    /// var altText = service.GetAltText(IconKey.Fire);   // Returns "Fire damage"
    /// </code>
    /// </example>
    string GetAltText(IconKey iconKey);

    /// <summary>
    /// Gets the alternative text description for a custom icon character.
    /// </summary>
    /// <param name="iconCharacter">The icon character or string to get alt text for.</param>
    /// <returns>
    /// A human-readable description of the icon, or the original character
    /// if no mapping exists.
    /// </returns>
    /// <remarks>
    /// Commonly maps direction arrows and other Unicode symbols to descriptions.
    /// </remarks>
    string GetAltText(string iconCharacter);

    /// <summary>
    /// Announces a message to the screen reader with normal priority.
    /// </summary>
    /// <param name="message">The message to announce.</param>
    /// <remarks>
    /// Normal priority announcements are queued after currently speaking text.
    /// Use <see cref="Announce(string, AnnouncementPriority)"/> for urgent messages.
    /// </remarks>
    void Announce(string message);

    /// <summary>
    /// Announces a message to the screen reader with specified priority.
    /// </summary>
    /// <param name="message">The message to announce.</param>
    /// <param name="priority">
    /// The announcement priority determining how urgently it should be read.
    /// </param>
    /// <remarks>
    /// <para><b>Normal:</b> Queued after current speech.</para>
    /// <para><b>Important:</b> Interrupts current speech.</para>
    /// <para><b>Assertive:</b> Read immediately, use sparingly for critical alerts.</para>
    /// </remarks>
    void Announce(string message, AnnouncementPriority priority);

    // ═══════════════════════════════════════════════════════════════════════════
    // KEYBOARD NAVIGATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets whether keyboard shortcut hints should be displayed on UI elements.
    /// </summary>
    /// <remarks>
    /// When enabled, UI elements display their associated keyboard shortcuts
    /// (e.g., "[Tab] Next" or "[Esc] Close") to assist keyboard-only navigation.
    /// </remarks>
    bool ShowKeyboardHints { get; set; }

    /// <summary>
    /// Gets the keyboard shortcut for a given action identifier.
    /// </summary>
    /// <param name="actionId">The action identifier (e.g., "OpenInventory", "Attack").</param>
    /// <returns>
    /// The keyboard shortcut string (e.g., "I", "Ctrl+A"), or <c>null</c> if no shortcut exists.
    /// </returns>
    string? GetKeyboardShortcut(string actionId);
}
