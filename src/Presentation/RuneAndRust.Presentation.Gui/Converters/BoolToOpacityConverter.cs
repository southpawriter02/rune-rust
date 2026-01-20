namespace RuneAndRust.Presentation.Gui.Converters;

using Avalonia.Data.Converters;
using System.Globalization;

/// <summary>
/// Converts a boolean to an opacity value.
/// </summary>
/// <remarks>
/// Used to dim locked exits by converting IsLocked to 0.5 opacity.
/// </remarks>
public class BoolToOpacityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to opacity (true = dim, false = full).
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>0.5 if true, 1.0 if false.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isLocked)
            return isLocked ? 0.5 : 1.0;
        return 1.0;
    }

    /// <summary>
    /// Converts back from opacity to boolean (not implemented).
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
