namespace RuneAndRust.Presentation.Shared.Enums;

/// <summary>
/// Defines the types of color blindness for accessibility color adjustments.
/// </summary>
/// <remarks>
/// <para>Used by <see cref="Services.IThemeService"/> to provide color-adjusted
/// palettes for users with color vision deficiencies.</para>
/// <para>The color blind modes adjust the theme colors to improve contrast
/// and distinguishability for users with specific types of color blindness.</para>
/// </remarks>
public enum ColorBlindMode
{
    /// <summary>
    /// Normal color vision with no adjustments.
    /// </summary>
    None = 0,

    /// <summary>
    /// Red-blind color adjustment (red weakness).
    /// </summary>
    /// <remarks>
    /// Affects approximately 1% of males. Red appears as brown/dark gray,
    /// and red-green differentiation is difficult.
    /// </remarks>
    Protanopia,

    /// <summary>
    /// Green-blind color adjustment (green weakness).
    /// </summary>
    /// <remarks>
    /// Affects approximately 1% of males. Green appears as brown/yellow,
    /// and red-green differentiation is difficult.
    /// </remarks>
    Deuteranopia,

    /// <summary>
    /// Blue-blind color adjustment (blue weakness).
    /// </summary>
    /// <remarks>
    /// Affects approximately 0.003% of people. Blue appears as green,
    /// and blue-yellow differentiation is difficult.
    /// </remarks>
    Tritanopia,

    /// <summary>
    /// Complete color blindness (grayscale).
    /// </summary>
    /// <remarks>
    /// Rare condition where all colors are perceived as shades of gray.
    /// Uses luminance and patterns for differentiation.
    /// </remarks>
    Achromatopsia
}
