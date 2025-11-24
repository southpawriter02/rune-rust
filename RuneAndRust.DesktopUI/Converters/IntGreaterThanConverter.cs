using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace RuneAndRust.DesktopUI.Converters;

/// <summary>
/// Converts an integer to a boolean based on whether it is greater than a threshold.
/// Used for conditional visibility of warning badges in the character sheet.
/// </summary>
public class IntGreaterThanConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance for use in XAML.
    /// </summary>
    public static readonly IntGreaterThanConverter Instance = new();

    /// <summary>
    /// Converts an integer value to a boolean indicating if it's greater than the parameter.
    /// </summary>
    /// <param name="value">The integer value to check.</param>
    /// <param name="targetType">The target type (bool).</param>
    /// <param name="parameter">The threshold value to compare against.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>True if value > parameter, false otherwise.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string paramString)
        {
            if (int.TryParse(paramString, out int threshold))
            {
                return intValue > threshold;
            }
        }

        if (value is int val && parameter is int paramInt)
        {
            return val > paramInt;
        }

        return false;
    }

    /// <summary>
    /// Not implemented - one-way conversion only.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
