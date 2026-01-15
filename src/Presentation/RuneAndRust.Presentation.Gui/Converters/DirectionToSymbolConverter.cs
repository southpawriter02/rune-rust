namespace RuneAndRust.Presentation.Gui.Converters;

using Avalonia.Data.Converters;
using RuneAndRust.Domain.Enums;
using System.Globalization;

/// <summary>
/// Converts Direction enum to directional arrow symbol.
/// </summary>
/// <remarks>
/// Maps each direction to a Unicode arrow symbol for display in exit buttons.
/// </remarks>
public class DirectionToSymbolConverter : IValueConverter
{
    /// <summary>
    /// Converts a Direction to its arrow symbol.
    /// </summary>
    /// <param name="value">The Direction enum value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>The arrow symbol string.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Direction direction)
            return "?";

        return direction switch
        {
            Direction.North => "↑",
            Direction.South => "↓",
            Direction.East => "→",
            Direction.West => "←",
            Direction.Up => "⬆",
            Direction.Down => "⬇",
            _ => "?"
        };
    }

    /// <summary>
    /// Converts back from symbol to Direction (not implemented).
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
