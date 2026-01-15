namespace RuneAndRust.Presentation.Gui.Converters;

using Avalonia.Data.Converters;
using Avalonia.Media;
using RuneAndRust.Presentation.Gui.Models;
using System.Globalization;

/// <summary>
/// Converts MessageType to appropriate color brush.
/// </summary>
/// <remarks>
/// Maps each MessageType to a distinct color for visual differentiation
/// in the message log panel.
/// </remarks>
public class MessageTypeToColorConverter : IValueConverter
{
    /// <summary>
    /// Converts a MessageType to a brush color.
    /// </summary>
    /// <param name="value">The MessageType value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>A brush for the message type's color.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MessageType type)
            return Brushes.White;

        return type switch
        {
            MessageType.Default => Brushes.White,
            MessageType.Info => Brushes.Cyan,
            MessageType.Warning => Brushes.Yellow,
            MessageType.Error => Brushes.Red,
            MessageType.CombatHit => Brushes.OrangeRed,
            MessageType.CombatMiss => Brushes.Gray,
            MessageType.CombatHeal => Brushes.LimeGreen,
            MessageType.CombatCritical => Brushes.Magenta,
            MessageType.LootCommon => Brushes.White,
            MessageType.LootUncommon => Brushes.LimeGreen,
            MessageType.LootRare => Brushes.RoyalBlue,
            MessageType.LootEpic => Brushes.Purple,
            MessageType.LootLegendary => Brushes.Gold,
            MessageType.Dialogue => Brushes.Yellow,
            MessageType.Success => Brushes.LimeGreen,
            MessageType.Failure => Brushes.OrangeRed,
            _ => Brushes.White
        };
    }

    /// <summary>
    /// Converts back from brush to MessageType (not implemented).
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
