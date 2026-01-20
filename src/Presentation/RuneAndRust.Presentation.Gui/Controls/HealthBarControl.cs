namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

/// <summary>
/// Custom Avalonia control for health/resource bars with color thresholds.
/// </summary>
/// <remarks>
/// Displays a visual bar with:
/// - Label (HP, MP, XP, etc.)
/// - Colored fill based on bar type and percentage
/// - Value display (current/max)
/// </remarks>
public class HealthBarControl : TemplatedControl
{
    /// <summary>
    /// Defines the CurrentValue styled property.
    /// </summary>
    public static readonly StyledProperty<int> CurrentValueProperty =
        AvaloniaProperty.Register<HealthBarControl, int>(nameof(CurrentValue));

    /// <summary>
    /// Defines the MaxValue styled property.
    /// </summary>
    public static readonly StyledProperty<int> MaxValueProperty =
        AvaloniaProperty.Register<HealthBarControl, int>(nameof(MaxValue), 100);

    /// <summary>
    /// Defines the BarType styled property.
    /// </summary>
    public static readonly StyledProperty<BarType> BarTypeProperty =
        AvaloniaProperty.Register<HealthBarControl, BarType>(nameof(BarType), BarType.Health);

    /// <summary>
    /// Defines the Label styled property.
    /// </summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<HealthBarControl, string>(nameof(Label), "HP");

    /// <summary>
    /// Defines the FillBrush styled property.
    /// </summary>
    public static readonly StyledProperty<IBrush> FillBrushProperty =
        AvaloniaProperty.Register<HealthBarControl, IBrush>(nameof(FillBrush), Brushes.ForestGreen);

    /// <summary>
    /// Defines the DisplayText styled property.
    /// </summary>
    public static readonly StyledProperty<string> DisplayTextProperty =
        AvaloniaProperty.Register<HealthBarControl, string>(nameof(DisplayText), "0/100");

    /// <summary>
    /// Defines the FillPercentage styled property.
    /// </summary>
    public static readonly StyledProperty<double> FillPercentageProperty =
        AvaloniaProperty.Register<HealthBarControl, double>(nameof(FillPercentage), 0);

    /// <summary>
    /// Static constructor to set up property change handlers.
    /// </summary>
    static HealthBarControl()
    {
        CurrentValueProperty.Changed.AddClassHandler<HealthBarControl>((c, _) => c.OnValueChanged());
        MaxValueProperty.Changed.AddClassHandler<HealthBarControl>((c, _) => c.OnValueChanged());
    }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public int CurrentValue
    {
        get => GetValue(CurrentValueProperty);
        set => SetValue(CurrentValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the bar type.
    /// </summary>
    public BarType BarType
    {
        get => GetValue(BarTypeProperty);
        set => SetValue(BarTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the label text.
    /// </summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the fill percentage (0.0 to 1.0).
    /// </summary>
    public double FillPercentage
    {
        get => GetValue(FillPercentageProperty);
        private set => SetValue(FillPercentageProperty, value);
    }

    /// <summary>
    /// Gets or sets the display text (e.g., "65/100").
    /// </summary>
    public string DisplayText
    {
        get => GetValue(DisplayTextProperty);
        private set => SetValue(DisplayTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the fill brush based on bar type and percentage.
    /// </summary>
    public IBrush FillBrush
    {
        get => GetValue(FillBrushProperty);
        private set => SetValue(FillBrushProperty, value);
    }

    /// <summary>
    /// Gets the appropriate health brush based on percentage thresholds.
    /// </summary>
    /// <returns>Color brush indicating health urgency.</returns>
    private IBrush GetHealthBrush()
    {
        var percent = FillPercentage * 100;
        return percent switch
        {
            > 75 => Brushes.ForestGreen,
            > 50 => Brushes.Gold,
            > 25 => Brushes.OrangeRed,
            _ => Brushes.DarkRed
        };
    }

    /// <summary>
    /// Called when CurrentValue or MaxValue changes.
    /// </summary>
    private void OnValueChanged()
    {
        // Recalculate computed properties
        FillPercentage = MaxValue > 0 ? (double)CurrentValue / MaxValue : 0;
        DisplayText = $"{CurrentValue}/{MaxValue}";
        FillBrush = BarType switch
        {
            BarType.Health => GetHealthBrush(),
            BarType.Mana => Brushes.RoyalBlue,
            BarType.Experience => Brushes.MediumPurple,
            BarType.Stamina => Brushes.Orange,
            _ => Brushes.Gray
        };
    }
}
