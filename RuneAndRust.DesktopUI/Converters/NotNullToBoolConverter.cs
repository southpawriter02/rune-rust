using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace RuneAndRust.DesktopUI.Converters;

/// <summary>
/// Converts a nullable value to a boolean.
/// Returns true if the value is not null, false otherwise.
/// </summary>
public class NotNullToBoolConverter : IValueConverter
{
    /// <summary>
    /// Static instance for XAML usage.
    /// </summary>
    public static readonly NotNullToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a nullable value to a boolean.
/// Returns true if the value is null, false otherwise.
/// (Inverse of NotNullToBoolConverter)
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    /// <summary>
    /// Static instance for XAML usage.
    /// </summary>
    public static readonly NullToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a count (int) to a boolean.
/// Returns true if count > 0, false otherwise.
/// </summary>
public class CountToBoolConverter : IValueConverter
{
    /// <summary>
    /// Static instance for XAML usage.
    /// </summary>
    public static readonly CountToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
