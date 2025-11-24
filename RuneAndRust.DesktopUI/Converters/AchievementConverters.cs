using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RuneAndRust.DesktopUI.Converters;

/// <summary>
/// v0.43.15: Converts unlocked state to background color.
/// </summary>
public class UnlockedBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUnlocked)
        {
            return isUnlocked
                ? Brush.Parse("#2D3A2D")  // Greenish dark
                : Brush.Parse("#1C1C1C"); // Dark gray
        }
        return Brush.Parse("#1C1C1C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.15: Converts unlocked state to border color.
/// </summary>
public class UnlockedBorderConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUnlocked)
        {
            return isUnlocked
                ? Brush.Parse("#4CAF50")  // Green
                : Brush.Parse("#3C3C3C"); // Dark gray
        }
        return Brush.Parse("#3C3C3C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.15: Converts unlocked state to achievement icon.
/// </summary>
public class AchievementIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUnlocked)
        {
            return isUnlocked ? "★" : "☆";
        }
        return "☆";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.15: Converts category string to color.
/// </summary>
public class CategoryColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string category)
        {
            return category switch
            {
                "Milestone" => Brush.Parse("#FFD700"),   // Gold
                "Combat" => Brush.Parse("#FF6B6B"),     // Red
                "Exploration" => Brush.Parse("#4CAF50"), // Green
                "Challenge" => Brush.Parse("#9400D3"),  // Purple
                "Narrative" => Brush.Parse("#4A90E2"),  // Blue
                "Collection" => Brush.Parse("#FFA500"), // Orange
                _ => Brush.Parse("#CCCCCC")
            };
        }
        return Brush.Parse("#CCCCCC");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.15: Inverts a boolean value.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return false;
    }
}

/// <summary>
/// v0.43.15: Converts percentage (0.0-1.0) to display string.
/// </summary>
public class PercentageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float f)
        {
            return $"{f:P0}";
        }
        if (value is double d)
        {
            return $"{d:P0}";
        }
        return "0%";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.15: Converts string to brush color.
/// </summary>
public class StringToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
            try
            {
                return Brush.Parse(colorString);
            }
            catch
            {
                return Brush.Parse("#CCCCCC");
            }
        }
        return Brush.Parse("#CCCCCC");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
