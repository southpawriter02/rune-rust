using Avalonia.Data.Converters;
using Avalonia.Media;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Globalization;

namespace RuneAndRust.DesktopUI.Converters;

/// <summary>
/// v0.43.17: Converts phase segment completion state to background color.
/// </summary>
public class PhaseSegmentColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCompleted)
        {
            return isCompleted
                ? Brush.Parse("#4CAF50")  // Green for completed
                : Brush.Parse("#3C3C3C"); // Dark gray for incomplete
        }
        return Brush.Parse("#3C3C3C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts current phase state to border brush.
/// </summary>
public class CurrentPhaseBorderConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCurrentPhase)
        {
            return isCurrentPhase
                ? Brush.Parse("#FFD700")  // Gold for current
                : Brushes.Transparent;
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts current phase state to border thickness.
/// </summary>
public class CurrentPhaseBorderThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCurrentPhase)
        {
            return isCurrentPhase ? new Avalonia.Thickness(3) : new Avalonia.Thickness(0);
        }
        return new Avalonia.Thickness(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts danger level to warning color.
/// </summary>
public class DangerLevelColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DangerLevel level)
        {
            return level switch
            {
                DangerLevel.Low => Brush.Parse("#FFC107"),      // Yellow/Amber
                DangerLevel.Medium => Brush.Parse("#FF9800"),   // Orange
                DangerLevel.High => Brush.Parse("#F44336"),     // Red
                DangerLevel.Lethal => Brush.Parse("#B71C1C"),   // Dark Red
                _ => Brush.Parse("#FFFFFF")
            };
        }
        return Brush.Parse("#FFFFFF");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts danger level to icon text.
/// </summary>
public class DangerLevelIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DangerLevel level)
        {
            return level switch
            {
                DangerLevel.Low => "\u26A0",      // Warning sign
                DangerLevel.Medium => "\u2757",   // Exclamation mark
                DangerLevel.High => "\u2622",     // Radioactive
                DangerLevel.Lethal => "\u2620",   // Skull and crossbones
                _ => "\u26A0"
            };
        }
        return "\u26A0";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts value > 0 to true for visibility.
/// </summary>
public class GreaterThanZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            float f => f > 0,
            double d => d > 0,
            int i => i > 0,
            decimal dec => dec > 0,
            _ => false
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts enrage progress to warning visibility.
/// </summary>
public class EnrageWarningVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float progress)
        {
            // Show warning when enrage progress > 50%
            return progress > 0.5f;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts enrage progress to color (yellow -> orange -> red).
/// </summary>
public class EnrageProgressColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float progress)
        {
            return progress switch
            {
                >= 0.9f => Brush.Parse("#B71C1C"),  // Dark red (almost enraged)
                >= 0.7f => Brush.Parse("#F44336"),  // Red
                >= 0.5f => Brush.Parse("#FF9800"),  // Orange
                _ => Brush.Parse("#FFC107")         // Yellow
            };
        }
        return Brush.Parse("#DC143C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts HP percentage to boss health bar color.
/// </summary>
public class BossHealthColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float hpPercent)
        {
            return hpPercent switch
            {
                <= 0.25f => Brush.Parse("#B71C1C"),  // Dark red (critical)
                <= 0.50f => Brush.Parse("#F44336"),  // Red (danger)
                <= 0.75f => Brush.Parse("#FF9800"),  // Orange (caution)
                _ => Brush.Parse("#DC143C")          // Standard boss red
            };
        }
        return Brush.Parse("#DC143C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts vulnerable state to glow effect.
/// </summary>
public class VulnerableGlowConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isVulnerable)
        {
            return isVulnerable
                ? Brush.Parse("#00FF00")  // Green glow
                : Brushes.Transparent;
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts phase number to Roman numeral.
/// </summary>
public class PhaseRomanNumeralConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int phase)
        {
            return phase switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                _ => phase.ToString()
            };
        }
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts transitioning state to overlay visibility.
/// </summary>
public class TransitionOverlayVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTransitioning)
        {
            return isTransitioning;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts interruptible state to display text.
/// </summary>
public class InterruptibleTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool canInterrupt)
        {
            return canInterrupt ? "Can be interrupted!" : "Cannot be interrupted";
        }
        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.17: Converts interruptible state to text color.
/// </summary>
public class InterruptibleColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool canInterrupt)
        {
            return canInterrupt
                ? Brush.Parse("#4CAF50")  // Green
                : Brush.Parse("#888888"); // Gray
        }
        return Brush.Parse("#888888");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
