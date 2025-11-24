using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RuneAndRust.DesktopUI.Converters;

/// <summary>
/// Converts a boolean to an upgrade/downgrade display string.
/// Used for showing equipment comparison results in the inventory view.
/// </summary>
public class BoolToUpgradeConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance for use in XAML.
    /// </summary>
    public static readonly BoolToUpgradeConverter Instance = new();

    /// <summary>
    /// Converts a boolean to "UPGRADE" (green) or "DOWNGRADE" (red) text.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUpgrade)
        {
            return isUpgrade ? "UPGRADE" : "DOWNGRADE";
        }

        return "COMPARE";
    }

    /// <summary>
    /// Not implemented - one-way conversion only.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean upgrade flag to a foreground color.
/// Green for upgrade, red for downgrade.
/// </summary>
public class BoolToUpgradeColorConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance for use in XAML.
    /// </summary>
    public static readonly BoolToUpgradeColorConverter Instance = new();

    /// <summary>
    /// Converts a boolean to a color brush.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUpgrade)
        {
            return isUpgrade
                ? new SolidColorBrush(Color.Parse("#228B22"))  // Green
                : new SolidColorBrush(Color.Parse("#DC143C")); // Red
        }

        return new SolidColorBrush(Color.Parse("#888888")); // Gray
    }

    /// <summary>
    /// Not implemented - one-way conversion only.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
