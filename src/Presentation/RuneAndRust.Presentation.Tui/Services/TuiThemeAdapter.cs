using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Services;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Tui.Services;

/// <summary>
/// TUI-specific implementation of <see cref="IThemeService"/> that provides
/// ConsoleColor and Spectre.Console color conversions.
/// </summary>
/// <remarks>
/// <para>TuiThemeAdapter wraps a <see cref="ThemeDefinition"/> and provides
/// methods to convert <see cref="ThemeColor"/> to platform-specific formats:</para>
/// <list type="bullet">
///   <item><description><see cref="ToConsoleColor"/> - Maps RGB to nearest of 16 ConsoleColors</description></item>
///   <item><description><see cref="ToSpectreColorString"/> - Returns Spectre.Console color string</description></item>
/// </list>
/// <para>Color blind adjustments are applied when <see cref="ColorBlindMode"/> is set.</para>
/// </remarks>
public class TuiThemeAdapter : IThemeService
{
    private readonly ThemeDefinition _theme;
    private readonly ILogger<TuiThemeAdapter>? _logger;

    // Console color mapping table - RGB values for each ConsoleColor
    private static readonly Dictionary<ConsoleColor, (int R, int G, int B)> ConsoleColorRgb = new()
    {
        { ConsoleColor.Black, (0, 0, 0) },
        { ConsoleColor.DarkBlue, (0, 0, 139) },
        { ConsoleColor.DarkGreen, (0, 100, 0) },
        { ConsoleColor.DarkCyan, (0, 139, 139) },
        { ConsoleColor.DarkRed, (139, 0, 0) },
        { ConsoleColor.DarkMagenta, (139, 0, 139) },
        { ConsoleColor.DarkYellow, (128, 128, 0) },
        { ConsoleColor.Gray, (128, 128, 128) },
        { ConsoleColor.DarkGray, (169, 169, 169) },
        { ConsoleColor.Blue, (0, 0, 255) },
        { ConsoleColor.Green, (0, 255, 0) },
        { ConsoleColor.Cyan, (0, 255, 255) },
        { ConsoleColor.Red, (255, 0, 0) },
        { ConsoleColor.Magenta, (255, 0, 255) },
        { ConsoleColor.Yellow, (255, 255, 0) },
        { ConsoleColor.White, (255, 255, 255) }
    };

    /// <summary>
    /// Initializes a new TuiThemeAdapter with the specified theme and logger.
    /// </summary>
    /// <param name="theme">The theme definition to use.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when theme is null.</exception>
    public TuiThemeAdapter(ThemeDefinition theme, ILogger<TuiThemeAdapter>? logger = null)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _logger = logger;

        _logger?.LogInformation(
            "TuiThemeAdapter initialized with theme '{ThemeName}'",
            _theme.Name);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IThemeService Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public ThemeDefinition ThemeDefinition => _theme;

    /// <inheritdoc />
    public bool IsColorBlindModeEnabled => ColorBlindMode != ColorBlindMode.None;

    /// <inheritdoc />
    public ColorBlindMode ColorBlindMode { get; set; } = ColorBlindMode.None;

    /// <inheritdoc />
    public bool UseAsciiIcons { get; set; } = false;

    /// <inheritdoc />
    public ThemeColor GetColor(ColorKey key)
    {
        var color = _theme.Palette.GetColor(key);
        var adjusted = ApplyColorBlindAdjustment(color);

        _logger?.LogDebug(
            "GetColor: {ColorKey} => {Color} (ColorBlind: {ColorBlindMode})",
            key, adjusted, ColorBlindMode);

        return adjusted;
    }

    /// <inheritdoc />
    public ThemeColor GetHealthColor(double percentage)
    {
        // Clamp percentage to valid range
        percentage = Math.Clamp(percentage, 0.0, 1.0);

        var key = percentage switch
        {
            >= 0.76 => ColorKey.HealthFull,
            >= 0.51 => ColorKey.HealthGood,
            >= 0.26 => ColorKey.HealthLow,
            _ => ColorKey.HealthCritical
        };

        _logger?.LogDebug(
            "GetHealthColor: {Percentage:P0} => {ColorKey}",
            percentage, key);

        return GetColor(key);
    }

    /// <inheritdoc />
    public ThemeColor GetResourceColor(string resourceTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceTypeName);

        // Map resource type name to ColorKey
        var key = resourceTypeName.ToLowerInvariant() switch
        {
            "mana" or "mp" or "arcanepower" => ColorKey.Mana,
            "rage" or "fury" => ColorKey.Rage,
            "energy" or "ki" => ColorKey.Energy,
            "focus" or "concentration" => ColorKey.Focus,
            "stamina" or "endurance" => ColorKey.Stamina,
            _ => ColorKey.Muted // Fallback for unknown resources
        };

        _logger?.LogDebug(
            "GetResourceColor: '{ResourceType}' => {ColorKey}",
            resourceTypeName, key);

        return GetColor(key);
    }

    /// <inheritdoc />
    public ThemeColor GetStatusEffectColor(string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        // Map status effect category to ColorKey
        var key = category.ToLowerInvariant() switch
        {
            "buff" or "positive" => ColorKey.Buff,
            "debuff" or "negative" => ColorKey.Debuff,
            "fire" or "burning" => ColorKey.Fire,
            "ice" or "cold" or "frozen" => ColorKey.Ice,
            "poison" or "toxic" => ColorKey.Poison,
            "lightning" or "shock" or "electric" => ColorKey.Lightning,
            "holy" or "divine" or "light" => ColorKey.Holy,
            "shadow" or "dark" or "necrotic" => ColorKey.Shadow,
            _ => ColorKey.Muted // Fallback for unknown effects
        };

        _logger?.LogDebug(
            "GetStatusEffectColor: '{Category}' => {ColorKey}",
            category, key);

        return GetColor(key);
    }

    /// <inheritdoc />
    public string GetIcon(IconKey key)
    {
        var icon = UseAsciiIcons
            ? _theme.Icons.GetAsciiIcon(key)
            : _theme.Icons.GetUnicodeIcon(key);

        _logger?.LogDebug(
            "GetIcon: {IconKey} => '{Icon}' (UseAscii: {UseAscii})",
            key, icon, UseAsciiIcons);

        return icon;
    }

    /// <inheritdoc />
    public string GetStatusIcon(string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        // Map status effect category to IconKey
        var key = category.ToLowerInvariant() switch
        {
            "buff" or "positive" => IconKey.Buff,
            "debuff" or "negative" => IconKey.Debuff,
            "fire" or "burning" => IconKey.Fire,
            "ice" or "cold" or "frozen" => IconKey.Ice,
            "poison" or "toxic" => IconKey.Poison,
            "lightning" or "shock" or "electric" => IconKey.Lightning,
            "stun" or "daze" => IconKey.Stun,
            "shield" or "barrier" => IconKey.Shield,
            _ => IconKey.Info // Fallback
        };

        return GetIcon(key);
    }

    /// <inheritdoc />
    public string GetResourceIcon(string resourceTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceTypeName);

        // Map resource type to IconKey
        var key = resourceTypeName.ToLowerInvariant() switch
        {
            "health" or "hp" => IconKey.Health,
            "mana" or "mp" => IconKey.Mana,
            _ => IconKey.Star // Fallback for unknown resources
        };

        return GetIcon(key);
    }

    /// <inheritdoc />
    public TimeSpan GetAnimationDuration(AnimationKey key)
    {
        var duration = _theme.Animations.GetDuration(key);

        _logger?.LogDebug(
            "GetAnimationDuration: {AnimationKey} => {Duration}ms",
            key, duration.TotalMilliseconds);

        return duration;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TUI-Specific Conversions
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts a ThemeColor to the nearest ConsoleColor.
    /// </summary>
    /// <param name="color">The theme color to convert.</param>
    /// <returns>The nearest ConsoleColor based on Euclidean distance.</returns>
    /// <remarks>
    /// <para>Console only supports 16 colors, so this finds the closest match
    /// using Euclidean distance in RGB space.</para>
    /// </remarks>
    public ConsoleColor ToConsoleColor(ThemeColor color)
    {
        var nearest = FindNearestConsoleColor(color.R, color.G, color.B);

        _logger?.LogDebug(
            "ToConsoleColor: {ThemeColor} => {ConsoleColor}",
            color, nearest);

        return nearest;
    }

    /// <summary>
    /// Gets the ConsoleColor for a color key.
    /// </summary>
    /// <param name="key">The color key.</param>
    /// <returns>The nearest ConsoleColor.</returns>
    public ConsoleColor GetConsoleColor(ColorKey key) =>
        ToConsoleColor(GetColor(key));

    /// <summary>
    /// Gets the ConsoleColor for a health percentage.
    /// </summary>
    /// <param name="percentage">Health percentage (0.0 to 1.0).</param>
    /// <returns>The appropriate ConsoleColor for the health level.</returns>
    public ConsoleColor GetHealthConsoleColor(double percentage) =>
        ToConsoleColor(GetHealthColor(percentage));

    /// <summary>
    /// Converts a ThemeColor to a Spectre.Console color string.
    /// </summary>
    /// <param name="color">The theme color to convert.</param>
    /// <returns>Hex color string usable in Spectre.Console markup.</returns>
    /// <remarks>
    /// <para>Returns the hex color directly since Spectre.Console supports
    /// 24-bit colors in most terminals.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var colorStr = adapter.ToSpectreColorString(color);
    /// AnsiConsole.MarkupLine($"[{colorStr}]Colored text[/]");
    /// </code>
    /// </example>
    public string ToSpectreColorString(ThemeColor color) => color.Hex.TrimStart('#');

    /// <summary>
    /// Gets the TUI-appropriate icon for the given key.
    /// </summary>
    /// <param name="key">The icon key.</param>
    /// <returns>Unicode or ASCII icon based on settings.</returns>
    public string GetTuiIcon(IconKey key) => GetIcon(key);

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Finds the nearest ConsoleColor using Euclidean distance in RGB space.
    /// </summary>
    private static ConsoleColor FindNearestConsoleColor(byte r, byte g, byte b)
    {
        ConsoleColor nearest = ConsoleColor.White;
        double minDistance = double.MaxValue;

        foreach (var (consoleColor, rgb) in ConsoleColorRgb)
        {
            // Calculate Euclidean distance
            var distance = Math.Sqrt(
                Math.Pow(r - rgb.R, 2) +
                Math.Pow(g - rgb.G, 2) +
                Math.Pow(b - rgb.B, 2));

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = consoleColor;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Applies color blind adjustment to a theme color.
    /// </summary>
    /// <remarks>
    /// <para>Basic implementation - full color blind simulation in v0.13.5f.</para>
    /// </remarks>
    private ThemeColor ApplyColorBlindAdjustment(ThemeColor color)
    {
        if (ColorBlindMode == ColorBlindMode.None)
            return color;

        // For now, return original color. Full implementation in v0.13.5f.
        // This placeholder ensures the API is ready for accessibility features.
        _logger?.LogDebug(
            "Color blind adjustment placeholder: {Mode} for {Color}",
            ColorBlindMode, color);

        return color;
    }
}
