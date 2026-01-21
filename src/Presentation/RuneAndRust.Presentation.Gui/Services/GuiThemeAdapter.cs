using Avalonia.Media;
using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Services;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Gui.Services;

/// <summary>
/// GUI-specific implementation of <see cref="IThemeService"/> that provides
/// Avalonia Brush and Color conversions.
/// </summary>
/// <remarks>
/// <para>GuiThemeAdapter wraps a <see cref="ThemeDefinition"/> and provides
/// methods to convert <see cref="ThemeColor"/> to Avalonia formats:</para>
/// <list type="bullet">
///   <item><description><see cref="ToBrush"/> - Creates SolidColorBrush from ThemeColor</description></item>
///   <item><description><see cref="ToAvaloniaColor"/> - Converts to Avalonia.Media.Color</description></item>
///   <item><description><see cref="GetCachedBrush"/> - Returns cached brush for performance</description></item>
/// </list>
/// <para>Brush caching reduces allocations for frequently used colors.</para>
/// </remarks>
public class GuiThemeAdapter : IThemeService
{
    private readonly ThemeDefinition _theme;
    private readonly ILogger<GuiThemeAdapter>? _logger;
    private readonly Dictionary<ColorKey, ISolidColorBrush> _brushCache;

    /// <summary>
    /// Initializes a new GuiThemeAdapter with the specified theme and logger.
    /// </summary>
    /// <param name="theme">The theme definition to use.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when theme is null.</exception>
    public GuiThemeAdapter(ThemeDefinition theme, ILogger<GuiThemeAdapter>? logger = null)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _logger = logger;
        _brushCache = new Dictionary<ColorKey, ISolidColorBrush>();

        _logger?.LogInformation(
            "GuiThemeAdapter initialized with theme '{ThemeName}'",
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
        // GUI always uses Unicode icons (richer display capability)
        var icon = _theme.Icons.GetUnicodeIcon(key);

        _logger?.LogDebug(
            "GetIcon: {IconKey} => '{Icon}'",
            key, icon);

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
    // GUI-Specific Conversions
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts a ThemeColor to an Avalonia SolidColorBrush.
    /// </summary>
    /// <param name="color">The theme color to convert.</param>
    /// <returns>A new SolidColorBrush with the RGB values.</returns>
    public ISolidColorBrush ToBrush(ThemeColor color)
    {
        var avaloniaColor = Color.FromRgb(color.R, color.G, color.B);
        var brush = new SolidColorBrush(avaloniaColor);

        _logger?.LogDebug(
            "ToBrush: {ThemeColor} => SolidColorBrush",
            color);

        return brush;
    }

    /// <summary>
    /// Gets a cached brush for a color key to avoid repeated allocations.
    /// </summary>
    /// <param name="key">The color key.</param>
    /// <returns>A cached SolidColorBrush for the color.</returns>
    /// <remarks>
    /// <para>Brushes are cached on first access for performance.
    /// Call <see cref="ClearBrushCache"/> when theme changes.</para>
    /// </remarks>
    public ISolidColorBrush GetCachedBrush(ColorKey key)
    {
        if (!_brushCache.TryGetValue(key, out var brush))
        {
            brush = ToBrush(GetColor(key));
            _brushCache[key] = brush;

            _logger?.LogDebug(
                "Cached new brush for {ColorKey}, cache size: {CacheSize}",
                key, _brushCache.Count);
        }

        return brush;
    }

    /// <summary>
    /// Gets a cached brush for the health color at the given percentage.
    /// </summary>
    /// <param name="percentage">Health percentage (0.0 to 1.0).</param>
    /// <returns>A SolidColorBrush for the health level.</returns>
    public ISolidColorBrush GetHealthBrush(double percentage)
    {
        var key = percentage switch
        {
            >= 0.76 => ColorKey.HealthFull,
            >= 0.51 => ColorKey.HealthGood,
            >= 0.26 => ColorKey.HealthLow,
            _ => ColorKey.HealthCritical
        };

        return GetCachedBrush(key);
    }

    /// <summary>
    /// Converts a ThemeColor to an Avalonia Color.
    /// </summary>
    /// <param name="color">The theme color to convert.</param>
    /// <returns>An Avalonia Color with the RGB values.</returns>
    public Color ToAvaloniaColor(ThemeColor color) =>
        Color.FromRgb(color.R, color.G, color.B);

    /// <summary>
    /// Gets the GUI-appropriate icon for the given key.
    /// </summary>
    /// <param name="key">The icon key.</param>
    /// <returns>Unicode icon (GUI always supports Unicode).</returns>
    public string GetGuiIcon(IconKey key) => GetIcon(key);

    /// <summary>
    /// Clears the brush cache. Call when theme changes.
    /// </summary>
    /// <remarks>
    /// <para>This should be called when the theme is modified to ensure
    /// components receive updated colors.</para>
    /// </remarks>
    public void ClearBrushCache()
    {
        var previousCount = _brushCache.Count;
        _brushCache.Clear();

        _logger?.LogWarning(
            "Brush cache cleared (was {PreviousCount} items)",
            previousCount);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Helpers
    // ═══════════════════════════════════════════════════════════════════════════

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
