using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Shared.Services;

/// <summary>
/// Provides centralized theme definitions including colors, icons,
/// and animation timings for consistent styling across TUI and GUI.
/// </summary>
/// <remarks>
/// <para>IThemeService is the primary interface for accessing visual styling
/// in both TUI and GUI presentation layers. It provides:</para>
/// <list type="bullet">
///   <item><description>Type-safe color access via <see cref="ColorKey"/></description></item>
///   <item><description>Threshold-based colors for health/resources</description></item>
///   <item><description>Icon access with Unicode/ASCII fallback</description></item>
///   <item><description>Animation timing consistency</description></item>
///   <item><description>Color blind mode support</description></item>
/// </list>
/// <para>Implementations:</para>
/// <list type="bullet">
///   <item><description>TuiThemeAdapter - Console/Spectre color conversion</description></item>
///   <item><description>GuiThemeAdapter - Avalonia brush/color conversion</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Inject IThemeService in constructor
/// public MyComponent(IThemeService themeService)
/// {
///     // Get colors
///     var healthColor = themeService.GetHealthColor(0.75); // Good health
///     var manaColor = themeService.GetColor(ColorKey.Mana);
///     
///     // Get icons
///     var healthIcon = themeService.GetIcon(IconKey.Health);
///     
///     // Get animation timing
///     var duration = themeService.GetAnimationDuration(AnimationKey.HealthChange);
/// }
/// </code>
/// </example>
public interface IThemeService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Color Access
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a color by its key from the theme palette.
    /// </summary>
    /// <param name="key">The color key to retrieve.</param>
    /// <returns>The theme color for the specified key.</returns>
    ThemeColor GetColor(ColorKey key);

    /// <summary>
    /// Gets the appropriate health color based on health percentage.
    /// </summary>
    /// <param name="percentage">Health percentage (0.0 to 1.0).</param>
    /// <returns>
    /// The appropriate health color for the percentage:
    /// <list type="bullet">
    ///   <item><description>76-100%: HealthFull (Forest Green)</description></item>
    ///   <item><description>51-75%: HealthGood (Lime Green)</description></item>
    ///   <item><description>26-50%: HealthLow (Gold)</description></item>
    ///   <item><description>0-25%: HealthCritical (Crimson)</description></item>
    /// </list>
    /// </returns>
    ThemeColor GetHealthColor(double percentage);

    /// <summary>
    /// Gets the color for a specific resource type by name.
    /// </summary>
    /// <param name="resourceTypeName">The resource type name (e.g., "Mana", "Rage").</param>
    /// <returns>The theme color for the resource type.</returns>
    ThemeColor GetResourceColor(string resourceTypeName);

    /// <summary>
    /// Gets the color for a status effect category.
    /// </summary>
    /// <param name="category">The status effect category (e.g., "Buff", "Fire").</param>
    /// <returns>The theme color for the status effect.</returns>
    ThemeColor GetStatusEffectColor(string category);

    // ═══════════════════════════════════════════════════════════════════════════
    // Icon Access
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an icon string by its key.
    /// </summary>
    /// <param name="key">The icon key to retrieve.</param>
    /// <returns>The icon string (Unicode or ASCII based on settings).</returns>
    string GetIcon(IconKey key);

    /// <summary>
    /// Gets the icon for a status effect by category name.
    /// </summary>
    /// <param name="category">The status effect category.</param>
    /// <returns>The icon string for the status effect.</returns>
    string GetStatusIcon(string category);

    /// <summary>
    /// Gets the icon for a resource type by name.
    /// </summary>
    /// <param name="resourceTypeName">The resource type name.</param>
    /// <returns>The icon string for the resource.</returns>
    string GetResourceIcon(string resourceTypeName);

    // ═══════════════════════════════════════════════════════════════════════════
    // Animation Timing
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the animation duration for a specific animation type.
    /// </summary>
    /// <param name="key">The animation key.</param>
    /// <returns>The duration for the animation.</returns>
    TimeSpan GetAnimationDuration(AnimationKey key);

    // ═══════════════════════════════════════════════════════════════════════════
    // Theme Configuration
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the complete theme definition.
    /// </summary>
    ThemeDefinition ThemeDefinition { get; }

    /// <summary>
    /// Gets whether color blind mode is enabled.
    /// </summary>
    bool IsColorBlindModeEnabled { get; }

    /// <summary>
    /// Gets or sets the current color blind mode.
    /// </summary>
    ColorBlindMode ColorBlindMode { get; set; }

    /// <summary>
    /// Gets or sets whether to use ASCII fallback icons instead of Unicode.
    /// </summary>
    bool UseAsciiIcons { get; set; }
}
