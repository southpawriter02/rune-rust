namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls.Primitives;

/// <summary>
/// Represents a single quick-slot for abilities or items.
/// </summary>
/// <remarks>
/// <para>
/// A quick-slot displays an assigned ability/item icon with:
/// <list type="bullet">
///   <item><description>Keyboard hint (1-8)</description></item>
///   <item><description>Cooldown overlay when on cooldown</description></item>
///   <item><description>Dimmed state when disabled</description></item>
/// </list>
/// </para>
/// </remarks>
public class QuickSlotControl : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="SlotNumber"/> property.
    /// </summary>
    public static readonly StyledProperty<int> SlotNumberProperty =
        AvaloniaProperty.Register<QuickSlotControl, int>(nameof(SlotNumber), 1);

    /// <summary>
    /// Defines the <see cref="Icon"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<QuickSlotControl, string?>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="SlotName"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> SlotNameProperty =
        AvaloniaProperty.Register<QuickSlotControl, string?>(nameof(SlotName));

    /// <summary>
    /// Defines the <see cref="IsOnCooldown"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOnCooldownProperty =
        AvaloniaProperty.Register<QuickSlotControl, bool>(nameof(IsOnCooldown));

    /// <summary>
    /// Defines the <see cref="CooldownRemaining"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CooldownRemainingProperty =
        AvaloniaProperty.Register<QuickSlotControl, double>(nameof(CooldownRemaining));

    /// <summary>
    /// Gets or sets the slot number (1-8).
    /// </summary>
    public int SlotNumber
    {
        get => GetValue(SlotNumberProperty);
        set => SetValue(SlotNumberProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon to display.
    /// </summary>
    /// <value>Unicode emoji or icon path.</value>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the slot name (ability or item name).
    /// </summary>
    public string? SlotName
    {
        get => GetValue(SlotNameProperty);
        set => SetValue(SlotNameProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the slot is on cooldown.
    /// </summary>
    public bool IsOnCooldown
    {
        get => GetValue(IsOnCooldownProperty);
        set => SetValue(IsOnCooldownProperty, value);
    }

    /// <summary>
    /// Gets or sets the remaining cooldown time.
    /// </summary>
    public double CooldownRemaining
    {
        get => GetValue(CooldownRemainingProperty);
        set => SetValue(CooldownRemainingProperty, value);
    }

    /// <summary>
    /// Gets whether the slot is empty (no icon assigned).
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Icon);

    /// <summary>
    /// Gets the keybind display text.
    /// </summary>
    public string Keybind => SlotNumber.ToString();

    static QuickSlotControl()
    {
        IsOnCooldownProperty.Changed.AddClassHandler<QuickSlotControl>((c, _) => c.OnCooldownChanged());
    }

    private void OnCooldownChanged()
    {
        Classes.Set("cooldown", IsOnCooldown);
    }
}
