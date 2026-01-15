namespace RuneAndRust.Presentation.Gui.Converters;

using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

/// <summary>
/// Converts boolean (isEmpty) to muted or primary text brush.
/// </summary>
/// <remarks>
/// Used to dim empty equipment slot text.
/// </remarks>
public class BoolToMutedConverter : IValueConverter
{
    /// <summary>
    /// Converts boolean to a text brush (muted if true, primary if false).
    /// </summary>
    /// <param name="value">The boolean value (true = empty/muted).</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>Gray brush if true (empty), White brush if false.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEmpty && isEmpty)
        {
            return new SolidColorBrush(Color.Parse("#888888"));
        }
        return new SolidColorBrush(Color.Parse("#E0E0E0"));
    }

    /// <summary>
    /// Converts back from brush to boolean (not implemented).
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

