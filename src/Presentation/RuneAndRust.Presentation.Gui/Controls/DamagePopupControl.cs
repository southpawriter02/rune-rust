namespace RuneAndRust.Presentation.Gui.Controls;

using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using RuneAndRust.Presentation.Gui.Enums;

/// <summary>
/// Displays floating damage/healing numbers with animation support.
/// </summary>
/// <remarks>
/// <para>
/// This control renders combat feedback as floating text that rises and fades.
/// The display format and color are determined by the <see cref="PopupType"/>
/// and <see cref="IsCritical"/> properties.
/// </para>
/// <para>
/// Display formats:
/// <list type="bullet">
///   <item><description>Damage: "-{value}" in white, or "★ {value} ★" in gold if critical</description></item>
///   <item><description>Healing: "+{value}" in lime green</description></item>
///   <item><description>Miss: "MISS" in gray</description></item>
///   <item><description>Block: "BLOCKED" in steel blue</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var popup = new DamagePopupControl
/// {
///     Value = 25,
///     PopupType = DamagePopupType.Damage,
///     IsCritical = true
/// };
/// // Displays "★ 25 ★" in gold at 24pt font
/// </code>
/// </example>
public class DamagePopupControl : Control
{
    /// <summary>
    /// Defines the <see cref="Value"/> styled property.
    /// </summary>
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<DamagePopupControl, int>(nameof(Value));

    /// <summary>
    /// Defines the <see cref="PopupType"/> styled property.
    /// </summary>
    public static readonly StyledProperty<DamagePopupType> PopupTypeProperty =
        AvaloniaProperty.Register<DamagePopupControl, DamagePopupType>(nameof(PopupType));

    /// <summary>
    /// Defines the <see cref="IsCritical"/> styled property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCriticalProperty =
        AvaloniaProperty.Register<DamagePopupControl, bool>(nameof(IsCritical));

    /// <summary>
    /// Gets or sets the numeric value to display.
    /// </summary>
    /// <value>The damage or healing amount. Ignored for Miss/Block types.</value>
    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of popup, determining color and format.
    /// </summary>
    /// <value>The popup type (Damage, Healing, Miss, or Block).</value>
    public DamagePopupType PopupType
    {
        get => GetValue(PopupTypeProperty);
        set => SetValue(PopupTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this is a critical hit.
    /// </summary>
    /// <value>True for critical hits (larger gold text with stars); false otherwise.</value>
    /// <remarks>Only applies to <see cref="DamagePopupType.Damage"/> type.</remarks>
    public bool IsCritical
    {
        get => GetValue(IsCriticalProperty);
        set => SetValue(IsCriticalProperty, value);
    }

    /// <summary>
    /// Gets the formatted display text based on popup type and value.
    /// </summary>
    /// <value>
    /// The formatted string:
    /// <list type="bullet">
    ///   <item><description>Critical damage: "★ {value} ★"</description></item>
    ///   <item><description>Normal damage: "-{value}"</description></item>
    ///   <item><description>Healing: "+{value}"</description></item>
    ///   <item><description>Miss: "MISS"</description></item>
    ///   <item><description>Block: "BLOCKED"</description></item>
    /// </list>
    /// </value>
    public string DisplayText => PopupType switch
    {
        DamagePopupType.Damage when IsCritical => $"★ {Value} ★",
        DamagePopupType.Damage => $"-{Value}",
        DamagePopupType.Healing => $"+{Value}",
        DamagePopupType.Miss => "MISS",
        DamagePopupType.Block => "BLOCKED",
        _ => Value.ToString()
    };

    /// <summary>
    /// Gets the text color based on popup type and critical status.
    /// </summary>
    /// <value>
    /// The brush color:
    /// <list type="bullet">
    ///   <item><description>Critical damage: Gold</description></item>
    ///   <item><description>Normal damage: White</description></item>
    ///   <item><description>Healing: LimeGreen</description></item>
    ///   <item><description>Miss: Gray</description></item>
    ///   <item><description>Block: SteelBlue</description></item>
    /// </list>
    /// </value>
    public IBrush TextColor => PopupType switch
    {
        DamagePopupType.Damage when IsCritical => Brushes.Gold,
        DamagePopupType.Damage => Brushes.White,
        DamagePopupType.Healing => Brushes.LimeGreen,
        DamagePopupType.Miss => Brushes.Gray,
        DamagePopupType.Block => Brushes.SteelBlue,
        _ => Brushes.White
    };

    /// <summary>
    /// Gets the font size based on critical status.
    /// </summary>
    /// <value>24 for critical hits; 18 for normal.</value>
    public double FontSize => IsCritical ? 24 : 18;

    /// <summary>
    /// Renders the damage popup text centered at the control's position.
    /// </summary>
    /// <param name="context">The drawing context for rendering.</param>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var typeface = new Typeface(
            "Segoe UI",
            FontStyle.Normal,
            IsCritical ? FontWeight.Bold : FontWeight.SemiBold);

        var formattedText = new FormattedText(
            DisplayText,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            TextColor);

        // Center the text at the control's origin
        var origin = new Point(-formattedText.Width / 2, -formattedText.Height / 2);
        context.DrawText(formattedText, origin);
    }
}
