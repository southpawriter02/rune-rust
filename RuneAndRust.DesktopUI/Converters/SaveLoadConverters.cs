using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RuneAndRust.DesktopUI.Converters;

/// <summary>
/// v0.43.19: Converts save type to appropriate icon.
/// Shows different icons for regular saves, auto-saves, and quick saves.
/// </summary>
public class SaveIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // If we receive SaveFileViewModel, check its properties
        if (value is bool isAutoSave)
        {
            return isAutoSave ? "\u23F0" : "\U0001F4BE"; // Clock for auto-save, floppy for regular
        }

        return "\U0001F4BE"; // Default floppy disk icon
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.19: Converts save type to full icon with quick-save awareness.
/// </summary>
public class SaveTypeIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ViewModels.SaveFileViewModel vm)
        {
            if (vm.IsQuickSave) return "\u26A1"; // Lightning for quick save
            if (vm.IsAutoSave) return "\u23F0";  // Clock for auto-save
            return "\U0001F4BE";                  // Floppy for regular
        }

        return "\U0001F4BE";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.19: Converts selected state to background color.
/// </summary>
public class SelectedBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? Brush.Parse("#3C5C3C")  // Selected green tint
                : Brush.Parse("#1C1C1C"); // Default dark
        }
        return Brush.Parse("#1C1C1C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.19: Converts selected state to border color.
/// </summary>
public class SelectedBorderConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? Brush.Parse("#4A90E2")  // Blue highlight
                : Brush.Parse("#4A4A4A"); // Default gray
        }
        return Brush.Parse("#4A4A4A");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.19: Formats DateTime for save file display.
/// </summary>
public class SaveDateFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            var now = DateTime.Now;
            var diff = now - date;

            // Show relative time for recent saves
            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";

            // Show full date for older saves
            return date.ToString("MMM dd, yyyy HH:mm");
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.19: Formats TimeSpan as play time display.
/// </summary>
public class PlayTimeFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            if (time.TotalMinutes >= 1)
                return $"{(int)time.TotalMinutes}m";
            return "< 1m";
        }
        return "0m";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.19: Converts character class to class color.
/// </summary>
public class ClassColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string className)
        {
            return className switch
            {
                "Shieldmaiden" => Brush.Parse("#4A90E2"),  // Blue
                "Berserker" => Brush.Parse("#DC143C"),     // Red
                "Skald" => Brush.Parse("#9400D3"),         // Purple
                "Seer" => Brush.Parse("#20B2AA"),          // Teal
                "Stalker" => Brush.Parse("#32CD32"),       // Green
                "Runesmith" => Brush.Parse("#FFD700"),     // Gold
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
/// v0.43.19: Converts legend level to display color (higher = more prestigious).
/// </summary>
public class LegendColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int legend)
        {
            return legend switch
            {
                >= 10 => Brush.Parse("#FFD700"),  // Gold for legendary
                >= 7 => Brush.Parse("#9400D3"),   // Purple for epic
                >= 4 => Brush.Parse("#4A90E2"),   // Blue for rare
                >= 2 => Brush.Parse("#32CD32"),   // Green for uncommon
                _ => Brush.Parse("#CCCCCC")       // Gray for common
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
/// v0.43.19: Converts save type (auto/quick/regular) to type label.
/// </summary>
public class SaveTypeLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ViewModels.SaveFileViewModel vm)
        {
            if (vm.IsQuickSave) return "[QUICK]";
            if (vm.IsAutoSave) return "[AUTO]";
            return string.Empty;
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.19: Converts save type to type label color.
/// </summary>
public class SaveTypeLabelColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ViewModels.SaveFileViewModel vm)
        {
            if (vm.IsQuickSave) return Brush.Parse("#FFD700"); // Gold
            if (vm.IsAutoSave) return Brush.Parse("#4A90E2");  // Blue
            return Brush.Parse("#FFFFFF");
        }
        return Brush.Parse("#FFFFFF");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.19: Converts boss defeated status to display text.
/// </summary>
public class BossStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool bossDefeated)
        {
            return bossDefeated ? "\u2694 Boss Defeated" : string.Empty;
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


/// <summary>
/// v0.43.19: Converts string to visibility (visible if not empty).
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
