namespace RuneAndRust.Presentation.Gui.Converters;

using Avalonia.Data.Converters;
using System.Globalization;

/// <summary>
/// Converts stat modifier to formatted string (+X or -X).
/// </summary>
/// <remarks>
/// Used to display stat modifiers in the player status panel.
/// Positive values show as (+X), negative as (-X).
/// </remarks>
public class ModifierToStringConverter : IValueConverter
{
    /// <summary>
    /// Converts an integer modifier to a formatted string.
    /// </summary>
    /// <param name="value">The modifier value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>Formatted modifier string like "(+3)" or "(-1)".</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int modifier)
            return "(+0)";

        return modifier >= 0 ? $"(+{modifier})" : $"({modifier})";
    }

    /// <summary>
    /// Converts back from string to modifier (not implemented).
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
