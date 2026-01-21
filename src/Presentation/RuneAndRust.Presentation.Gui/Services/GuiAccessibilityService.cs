using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Configuration;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Interfaces;
using RuneAndRust.Presentation.Shared.Utilities;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Gui.Services;

/// <summary>
/// Avalonia GUI implementation of <see cref="IAccessibilityService"/>.
/// </summary>
/// <remarks>
/// <para>Provides accessibility features optimized for graphical environments:</para>
/// <list type="bullet">
/// <item><description>Color blind mode color transformations</description></item>
/// <item><description>AutomationProperties integration for screen reader support</description></item>
/// <item><description>Alt text for icons and symbols</description></item>
/// <item><description>Keyboard shortcut hints with visual displays</description></item>
/// </list>
/// <para><b>Screen Reader Support:</b> Uses Avalonia's automation peer system
/// to communicate with system screen readers (NVDA, JAWS, VoiceOver, Narrator).</para>
/// <para><b>Logging:</b> Setting changes logged at Information level,
/// announcements and color transforms at Debug level.</para>
/// </remarks>
public class GuiAccessibilityService : IAccessibilityService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for accessibility events.
    /// </summary>
    private readonly ILogger<GuiAccessibilityService>? _logger;

    /// <summary>
    /// Accessibility settings.
    /// </summary>
    private readonly AccessibilitySettings _settings;

    /// <summary>
    /// Icon key to alt text mappings.
    /// </summary>
    private readonly Dictionary<IconKey, string> _iconAltText;

    /// <summary>
    /// Character to alt text mappings for custom icons.
    /// </summary>
    private readonly Dictionary<string, string> _characterAltText;

    /// <summary>
    /// Keyboard shortcut mappings: actionId -> shortcut string.
    /// </summary>
    private readonly Dictionary<string, string> _keyboardShortcuts;

    /// <summary>
    /// Cached screen reader detection result.
    /// </summary>
    private bool? _cachedScreenReaderActive;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiAccessibilityService"/> class.
    /// </summary>
    /// <param name="settings">Optional accessibility settings. Uses defaults if null.</param>
    /// <param name="logger">Optional logger for debug output.</param>
    public GuiAccessibilityService(
        AccessibilitySettings? settings = null,
        ILogger<GuiAccessibilityService>? logger = null)
    {
        _settings = settings ?? AccessibilitySettings.CreateDefault();
        _logger = logger;
        _iconAltText = InitializeIconAltText();
        _characterAltText = InitializeCharacterAltText();
        _keyboardShortcuts = InitializeKeyboardShortcuts();

        _logger?.LogInformation(
            "GUI Accessibility service initialized with ColorBlindMode={Mode}",
            _settings.ColorBlindMode);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COLOR BLIND SUPPORT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public ColorBlindMode ColorBlindMode
    {
        get => _settings.ColorBlindMode;
        set
        {
            if (_settings.ColorBlindMode != value)
            {
                var oldValue = _settings.ColorBlindMode;
                _settings.ColorBlindMode = value;

                _logger?.LogInformation(
                    "Accessibility setting changed: ColorBlindMode = {Value} (was {OldValue})",
                    value,
                    oldValue);
            }
        }
    }

    /// <inheritdoc/>
    public ThemeColor GetAccessibleColor(ThemeColor color)
    {
        return ColorBlindTransform.Transform(color, ColorBlindMode, _logger);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VISUAL SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool IsHighContrastEnabled
    {
        get => _settings.HighContrastEnabled;
        set
        {
            if (_settings.HighContrastEnabled != value)
            {
                _settings.HighContrastEnabled = value;

                _logger?.LogInformation(
                    "Accessibility setting changed: HighContrastEnabled = {Value}",
                    value);
            }
        }
    }

    /// <inheritdoc/>
    public bool IsReducedMotionEnabled
    {
        get => _settings.ReducedMotionEnabled;
        set
        {
            if (_settings.ReducedMotionEnabled != value)
            {
                _settings.ReducedMotionEnabled = value;

                _logger?.LogInformation(
                    "Accessibility setting changed: ReducedMotionEnabled = {Value}",
                    value);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCREEN READER SUPPORT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// <para>Attempts to detect active screen readers through Avalonia's automation system.</para>
    /// <para>Detection is cached for performance; call <see cref="RefreshScreenReaderDetection"/>
    /// to re-check.</para>
    /// </remarks>
    public bool IsScreenReaderActive
    {
        get
        {
            // Use cached value if available
            if (_cachedScreenReaderActive.HasValue)
            {
                return _cachedScreenReaderActive.Value;
            }

            // Try to detect screen reader through system accessibility APIs
            // This is a simplified implementation; full detection would require
            // platform-specific code or Avalonia automation peer checking
            _cachedScreenReaderActive = DetectScreenReader();
            return _cachedScreenReaderActive.Value;
        }
    }

    /// <summary>
    /// Refreshes screen reader detection cache.
    /// </summary>
    /// <remarks>
    /// Call this when screen reader state may have changed (e.g., after app resume).
    /// </remarks>
    public void RefreshScreenReaderDetection()
    {
        _cachedScreenReaderActive = null;
    }

    /// <inheritdoc/>
    public string GetAltText(IconKey iconKey)
    {
        return _iconAltText.TryGetValue(iconKey, out var altText)
            ? altText
            : iconKey.ToString();
    }

    /// <inheritdoc/>
    public string GetAltText(string iconCharacter)
    {
        if (string.IsNullOrEmpty(iconCharacter))
        {
            return string.Empty;
        }

        return _characterAltText.TryGetValue(iconCharacter, out var altText)
            ? altText
            : iconCharacter;
    }

    /// <inheritdoc/>
    public void Announce(string message)
    {
        Announce(message, AnnouncementPriority.Normal);
    }

    /// <inheritdoc/>
    public void Announce(string message, AnnouncementPriority priority)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _logger?.LogDebug(
            "Screen reader announcement ({Priority}): {Message}",
            priority,
            message);

        // In a full implementation, this would use Avalonia's automation system
        // to raise a LiveRegionChanged event for screen readers to pick up
        // Example: AutomationPeer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)
        RaiseScreenReaderAnnouncement(message, priority);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // KEYBOARD NAVIGATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool ShowKeyboardHints
    {
        get => _settings.ShowKeyboardHints;
        set
        {
            if (_settings.ShowKeyboardHints != value)
            {
                _settings.ShowKeyboardHints = value;

                _logger?.LogInformation(
                    "Accessibility setting changed: ShowKeyboardHints = {Value}",
                    value);
            }
        }
    }

    /// <inheritdoc/>
    public string? GetKeyboardShortcut(string actionId)
    {
        if (string.IsNullOrEmpty(actionId))
        {
            return null;
        }

        return _keyboardShortcuts.TryGetValue(actionId.ToLowerInvariant(), out var shortcut)
            ? shortcut
            : null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Detects whether a screen reader is active.
    /// </summary>
    /// <returns><c>true</c> if a screen reader is detected; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// This is a simplified implementation. Full detection would require:
    /// - Windows: Check for NVDA, JAWS, Narrator via accessibility API
    /// - macOS: Check for VoiceOver via NSAccessibility
    /// - Linux: Check for Orca via AT-SPI
    /// </remarks>
    private bool DetectScreenReader()
    {
        // Simplified: Check if settings indicate screen reader labels are enabled
        // A full implementation would query system accessibility APIs
        var detected = _settings.ScreenReaderLabelsEnabled;

        _logger?.LogDebug("Screen reader detection: {Detected}", detected);
        return detected;
    }

    /// <summary>
    /// Raises a screen reader announcement through the automation system.
    /// </summary>
    /// <param name="message">The message to announce.</param>
    /// <param name="priority">The announcement priority.</param>
    /// <remarks>
    /// Placeholder implementation. Full version would use Avalonia's
    /// AutomationPeer system to raise LiveRegion events.
    /// </remarks>
    private void RaiseScreenReaderAnnouncement(string message, AnnouncementPriority priority)
    {
        // In a complete implementation:
        // 1. Find or create a LiveRegion control
        // 2. Set its content to the message
        // 3. Set aria-live based on priority:
        //    - Normal -> "polite"
        //    - Important -> "polite" with higher precedence
        //    - Assertive -> "assertive"
        // 4. Trigger the automation event

        // For now, just log the announcement
        _logger?.LogDebug(
            "Would announce to screen reader: '{Message}' with priority {Priority}",
            message,
            priority);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE INITIALIZATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes icon alt text mappings.
    /// </summary>
    private static Dictionary<IconKey, string> InitializeIconAltText()
    {
        return new Dictionary<IconKey, string>
        {
            // Stats
            [IconKey.Health] = "Health",
            [IconKey.Mana] = "Mana",
            [IconKey.Attack] = "Attack",
            [IconKey.Defense] = "Defense",
            [IconKey.Speed] = "Speed",
            [IconKey.Luck] = "Luck",

            // Status Effects
            [IconKey.Buff] = "Buff active",
            [IconKey.Debuff] = "Debuff active",
            [IconKey.Fire] = "Fire damage",
            [IconKey.Ice] = "Ice damage",
            [IconKey.Poison] = "Poison damage",
            [IconKey.Lightning] = "Lightning damage",
            [IconKey.Stun] = "Stunned",
            [IconKey.Shield] = "Shielded",

            // Resources
            [IconKey.Ore] = "Ore",
            [IconKey.Herb] = "Herb",
            [IconKey.Leather] = "Leather",
            [IconKey.Gem] = "Gem",
            [IconKey.Wood] = "Wood",

            // Navigation
            [IconKey.ArrowUp] = "Up",
            [IconKey.ArrowDown] = "Down",
            [IconKey.ArrowLeft] = "Left",
            [IconKey.ArrowRight] = "Right",

            // UI Indicators
            [IconKey.Check] = "Completed",
            [IconKey.Cross] = "Failed",
            [IconKey.Warning] = "Warning",
            [IconKey.Info] = "Information",
            [IconKey.Lock] = "Locked",
            [IconKey.Unlock] = "Unlocked",
            [IconKey.Star] = "Star",
            [IconKey.StarEmpty] = "Empty star",

            // Entities
            [IconKey.Player] = "Player",
            [IconKey.Enemy] = "Enemy",
            [IconKey.Boss] = "Boss enemy",
            [IconKey.Npc] = "Non-player character",

            // Dice
            [IconKey.D20] = "20-sided die",
            [IconKey.CriticalSuccess] = "Critical hit!",
            [IconKey.CriticalFailure] = "Critical miss!"
        };
    }

    /// <summary>
    /// Initializes character alt text mappings.
    /// </summary>
    private static Dictionary<string, string> InitializeCharacterAltText()
    {
        return new Dictionary<string, string>
        {
            // Direction arrows (Unicode)
            ["↑"] = "Up",
            ["↓"] = "Down",
            ["←"] = "Left",
            ["→"] = "Right",
            ["↗"] = "Up-right",
            ["↘"] = "Down-right",
            ["↙"] = "Down-left",
            ["↖"] = "Up-left",

            // Common symbols
            ["♥"] = "Health",
            ["❤"] = "Health",
            ["✦"] = "Mana",
            ["⚔"] = "Attack",
            ["🛡"] = "Defense",
            ["⚡"] = "Speed or Lightning",
            ["☘"] = "Luck",
            ["🔥"] = "Fire",
            ["❄"] = "Ice",
            ["☠"] = "Poison or Death",
            ["★"] = "Star",
            ["☆"] = "Empty star",
            ["✓"] = "Check",
            ["✗"] = "Cross",
            ["⚠"] = "Warning",
            ["ⓘ"] = "Information",
            ["🔒"] = "Locked",
            ["🔓"] = "Unlocked",
            ["@"] = "Player",
            ["🎲"] = "Dice"
        };
    }

    /// <summary>
    /// Initializes keyboard shortcut mappings.
    /// </summary>
    private static Dictionary<string, string> InitializeKeyboardShortcuts()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Global navigation
            ["help"] = "F1",
            ["quit"] = "Alt+F4",
            ["menu"] = "Esc",
            ["fullscreen"] = "F11",

            // Game actions
            ["inventory"] = "I",
            ["character"] = "C",
            ["map"] = "M",
            ["abilities"] = "A",
            ["equipment"] = "E",
            ["quests"] = "Q",
            ["save"] = "Ctrl+S",
            ["load"] = "Ctrl+L",

            // Combat
            ["attack"] = "Space",
            ["defend"] = "D",
            ["flee"] = "F",
            ["use_item"] = "U",

            // Navigation
            ["move_up"] = "↑ or W",
            ["move_down"] = "↓ or S",
            ["move_left"] = "← or A",
            ["move_right"] = "→ or D",
            ["confirm"] = "Enter",
            ["cancel"] = "Esc",
            ["next_panel"] = "Tab",
            ["previous_panel"] = "Shift+Tab"
        };
    }
}
