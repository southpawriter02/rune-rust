using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Configuration;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Interfaces;
using RuneAndRust.Presentation.Shared.Utilities;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Tui.Services;

/// <summary>
/// Terminal-based implementation of <see cref="IAccessibilityService"/>.
/// </summary>
/// <remarks>
/// <para>Provides accessibility features optimized for terminal/console environments:</para>
/// <list type="bullet">
/// <item><description>Color blind mode color transformations</description></item>
/// <item><description>Console-based announcements (output to stderr or accessibility buffer)</description></item>
/// <item><description>Alt text for icons and symbols</description></item>
/// <item><description>Keyboard shortcut hints</description></item>
/// </list>
/// <para><b>Limitations:</b> Screen reader detection is not available in console mode;
/// <see cref="IsScreenReaderActive"/> always returns <c>false</c>.</para>
/// <para><b>Logging:</b> Setting changes logged at Information level,
/// announcements and color transforms at Debug level.</para>
/// </remarks>
public class TuiAccessibilityService : IAccessibilityService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for accessibility events.
    /// </summary>
    private readonly ILogger<TuiAccessibilityService>? _logger;

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

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="TuiAccessibilityService"/> class.
    /// </summary>
    /// <param name="settings">Optional accessibility settings. Uses defaults if null.</param>
    /// <param name="logger">Optional logger for debug output.</param>
    public TuiAccessibilityService(
        AccessibilitySettings? settings = null,
        ILogger<TuiAccessibilityService>? logger = null)
    {
        _settings = settings ?? AccessibilitySettings.CreateDefault();
        _logger = logger;
        _iconAltText = InitializeIconAltText();
        _characterAltText = InitializeCharacterAltText();
        _keyboardShortcuts = InitializeKeyboardShortcuts();

        _logger?.LogInformation(
            "TUI Accessibility service initialized with ColorBlindMode={Mode}",
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
    /// Console applications cannot reliably detect screen readers.
    /// Always returns <c>false</c> in TUI mode.
    /// </remarks>
    public bool IsScreenReaderActive => false;

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

        // In TUI mode, we can't directly interface with screen readers
        // The announcement is logged for debugging purposes
        // Users with screen readers will use their screen reader's output functionality
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

            // Direction arrows (ASCII)
            ["^"] = "Up",
            ["v"] = "Down",
            ["<"] = "Left",
            [">"] = "Right",

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
            ["quit"] = "Esc",
            ["menu"] = "Esc",

            // Game actions
            ["inventory"] = "I",
            ["character"] = "C",
            ["map"] = "M",
            ["abilities"] = "A",
            ["equipment"] = "E",
            ["quests"] = "Q",

            // Combat
            ["attack"] = "Enter",
            ["defend"] = "D",
            ["flee"] = "F",
            ["use_item"] = "U",

            // Navigation
            ["move_up"] = "↑",
            ["move_down"] = "↓",
            ["move_left"] = "←",
            ["move_right"] = "→",
            ["confirm"] = "Enter",
            ["cancel"] = "Esc",
            ["next_panel"] = "Tab",
            ["previous_panel"] = "Shift+Tab"
        };
    }
}
