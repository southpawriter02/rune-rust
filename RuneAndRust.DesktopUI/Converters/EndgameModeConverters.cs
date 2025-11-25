using Avalonia.Data.Converters;
using Avalonia.Media;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Globalization;

namespace RuneAndRust.DesktopUI.Converters;

/// <summary>
/// v0.43.16: Converts EndgameMode enum to display name.
/// </summary>
public class EndgameModeNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EndgameMode mode)
        {
            return mode switch
            {
                EndgameMode.NGPlus => "New Game +",
                EndgameMode.ChallengeSector => "Challenge Sectors",
                EndgameMode.BossGauntlet => "Boss Gauntlet",
                EndgameMode.EndlessMode => "Endless Mode",
                _ => mode.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.16: Converts EndgameMode enum to icon.
/// </summary>
public class EndgameModeIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EndgameMode mode)
        {
            return mode switch
            {
                EndgameMode.NGPlus => "\u2795",        // Plus sign
                EndgameMode.ChallengeSector => "\u2694", // Crossed swords
                EndgameMode.BossGauntlet => "\u2620",  // Skull
                EndgameMode.EndlessMode => "\u221E",   // Infinity
                _ => "\u2022"                          // Bullet
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
/// v0.43.16: Converts EndgameMode enum to color.
/// </summary>
public class EndgameModeColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EndgameMode mode)
        {
            return mode switch
            {
                EndgameMode.NGPlus => Brush.Parse("#FFD700"),       // Gold
                EndgameMode.ChallengeSector => Brush.Parse("#DC143C"), // Crimson
                EndgameMode.BossGauntlet => Brush.Parse("#9400D3"),  // Purple
                EndgameMode.EndlessMode => Brush.Parse("#4A90E2"),  // Blue
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
/// v0.43.16: Checks if EndgameMode is NGPlus for visibility binding.
/// </summary>
public class IsNGPlusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EndgameMode mode)
        {
            return mode == EndgameMode.NGPlus;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.16: Checks if EndgameMode is ChallengeSector for visibility binding.
/// </summary>
public class IsChallengeSectorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EndgameMode mode)
        {
            return mode == EndgameMode.ChallengeSector;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.16: Checks if EndgameMode is BossGauntlet for visibility binding.
/// </summary>
public class IsBossGauntletConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EndgameMode mode)
        {
            return mode == EndgameMode.BossGauntlet;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.16: Checks if EndgameMode is EndlessMode for visibility binding.
/// </summary>
public class IsEndlessModeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EndgameMode mode)
        {
            return mode == EndgameMode.EndlessMode;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.16: Converts modifier positivity to color (red for detrimental, green for beneficial).
/// </summary>
public class ModifierColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isDetrimental)
        {
            return isDetrimental
                ? Brush.Parse("#DC143C")  // Crimson for harder
                : Brush.Parse("#4CAF50"); // Green for easier
        }
        return Brush.Parse("#CCCCCC");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.16: Converts difficulty tier string to color.
/// </summary>
public class DifficultyTierColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string tier)
        {
            return tier switch
            {
                "Moderate" => Brush.Parse("#4CAF50"),      // Green
                "Hard" => Brush.Parse("#FFA500"),          // Orange
                "Extreme" => Brush.Parse("#DC143C"),       // Red
                "Near-Impossible" => Brush.Parse("#9400D3"), // Purple
                "Impossible" => Brush.Parse("#FF0000"),    // Bright red
                "Nightmare" => Brush.Parse("#9400D3"),     // Purple
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
/// v0.43.16: Converts multiplier value to formatted string.
/// </summary>
public class MultiplierFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float f)
        {
            return f > 1.0f ? $"x{f:F1}" : $"{f:F1}x";
        }
        if (value is double d)
        {
            return d > 1.0 ? $"x{d:F1}" : $"{d:F1}x";
        }
        return "x1.0";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.16: Converts NG+ tier number to roman numeral display.
/// </summary>
public class NGPlusTierDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int tier)
        {
            return tier switch
            {
                1 => "NG+I",
                2 => "NG+II",
                3 => "NG+III",
                4 => "NG+IV",
                5 => "NG+V",
                _ => $"NG+{tier}"
            };
        }
        return "NG+";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// v0.43.16: Converts boolean to selected button background.
/// </summary>
public class SelectedModeBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? Brush.Parse("#3C5C3C")  // Selected green
                : Brush.Parse("#2C2C2C"); // Default dark
        }
        return Brush.Parse("#2C2C2C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
