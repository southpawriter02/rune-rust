using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RuneAndRust.DesktopUI.Converters;

/// <summary>
/// v0.43.20: Converts help category to display color.
/// </summary>
public class HelpCategoryColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string category)
        {
            return category switch
            {
                "Combat" => Brush.Parse("#DC143C"),        // Red
                "Stats" => Brush.Parse("#4A90E2"),         // Blue
                "Status Effects" => Brush.Parse("#9400D3"), // Purple
                "Items" => Brush.Parse("#FFD700"),         // Gold
                "Abilities" => Brush.Parse("#32CD32"),     // Green
                "Dungeon" => Brush.Parse("#8B4513"),       // Brown
                "Progression" => Brush.Parse("#FF8C00"),   // Orange
                "UI" => Brush.Parse("#20B2AA"),            // Teal
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
/// v0.43.20: Converts category to icon.
/// </summary>
public class CategoryIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string category)
        {
            return category switch
            {
                "Combat" => "\u2694",         // Crossed swords
                "Stats" => "\U0001F4CA",      // Bar chart
                "Status Effects" => "\u2728", // Sparkles
                "Items" => "\U0001F4E6",      // Package
                "Abilities" => "\U0001F52E",  // Crystal ball
                "Dungeon" => "\U0001F5FA",    // Map
                "Progression" => "\u2B50",    // Star
                "UI" => "\u2699",             // Gear
                _ => "\u2022"                 // Bullet
            };
        }
        return "\u2022";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


/// <summary>
/// v0.43.20: Inverts a boolean for visibility binding.
/// </summary>
public class BoolInverterConverter : IValueConverter
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
/// v0.43.20: Formats shortcut text with key styling.
/// </summary>
public class ShortcutDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string shortcut && !string.IsNullOrEmpty(shortcut))
        {
            return $"[{shortcut}]";
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
