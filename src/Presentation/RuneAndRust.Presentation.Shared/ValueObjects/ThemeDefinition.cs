namespace RuneAndRust.Presentation.Shared.ValueObjects;

/// <summary>
/// Complete theme definition containing colors, icons, and animation timings.
/// </summary>
/// <remarks>
/// <para>ThemeDefinition is the root object containing all visual styling
/// information for the game. It aggregates:</para>
/// <list type="bullet">
///   <item><description><see cref="ColorPalette"/> - All color definitions</description></item>
///   <item><description><see cref="IconSet"/> - Unicode and ASCII icons</description></item>
///   <item><description><see cref="AnimationTimings"/> - Animation durations</description></item>
/// </list>
/// <para>Use <see cref="CreateDefault"/> to get the standard game theme.</para>
/// </remarks>
/// <example>
/// <code>
/// var theme = ThemeDefinition.CreateDefault();
/// var healthColor = theme.Palette.GetColor(ColorKey.HealthFull);
/// var healthIcon = theme.Icons.GetUnicodeIcon(IconKey.Health);
/// var animDuration = theme.Animations.GetDuration(AnimationKey.HealthChange);
/// </code>
/// </example>
public class ThemeDefinition
{
    /// <summary>
    /// Gets the color palette for this theme.
    /// </summary>
    public ColorPalette Palette { get; }

    /// <summary>
    /// Gets the icon set for this theme.
    /// </summary>
    public IconSet Icons { get; }

    /// <summary>
    /// Gets the animation timings for this theme.
    /// </summary>
    public AnimationTimings Animations { get; }

    /// <summary>
    /// Gets the name of this theme.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new ThemeDefinition with the specified components.
    /// </summary>
    /// <param name="name">The name of the theme.</param>
    /// <param name="palette">The color palette.</param>
    /// <param name="icons">The icon set.</param>
    /// <param name="animations">The animation timings.</param>
    public ThemeDefinition(
        string name,
        ColorPalette palette,
        IconSet icons,
        AnimationTimings animations)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));
        Icons = icons ?? throw new ArgumentNullException(nameof(icons));
        Animations = animations ?? throw new ArgumentNullException(nameof(animations));
    }

    /// <summary>
    /// Creates the default game theme.
    /// </summary>
    /// <returns>A new ThemeDefinition with the standard dark fantasy theme.</returns>
    /// <remarks>
    /// <para>The default theme uses a dark fantasy aesthetic appropriate for
    /// the game's atmosphere.</para>
    /// </remarks>
    public static ThemeDefinition CreateDefault() =>
        new(
            name: "Dark Fantasy",
            palette: ColorPalette.CreateDefault(),
            icons: IconSet.CreateDefault(),
            animations: AnimationTimings.CreateDefault()
        );

    /// <summary>
    /// Returns the theme name for debugging and logging.
    /// </summary>
    public override string ToString() => $"Theme: {Name}";
}
