// ═══════════════════════════════════════════════════════════════════════════════
// StanceDisplayConfig.cs
// Configuration for stance indicator display.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for stance indicator display.
/// </summary>
public record StanceDisplayConfig
{
    /// <summary>Stance icons.</summary>
    public required StanceIconConfig Icons { get; init; }

    /// <summary>Stance colors.</summary>
    public required StanceColorConfig Colors { get; init; }

    /// <summary>Display behavior settings.</summary>
    public required StanceDisplaySettings Display { get; init; }

    /// <summary>Creates default configuration.</summary>
    public static StanceDisplayConfig CreateDefault() => new()
    {
        Icons = new StanceIconConfig
        {
            Aggressive = "⚔",
            Defensive = "🛡",
            Balanced = "⚖",
            Tactical = "🎯",
            Special = "✦",
            Default = "◆",
            AggressiveAscii = "[A]",
            DefensiveAscii = "[D]",
            BalancedAscii = "[B]",
            TacticalAscii = "[T]",
            SpecialAscii = "[S]",
            DefaultAscii = "[?]"
        },
        Colors = new StanceColorConfig
        {
            Aggressive = ConsoleColor.Red,
            Defensive = ConsoleColor.Blue,
            Balanced = ConsoleColor.Yellow,
            Tactical = ConsoleColor.Cyan,
            Special = ConsoleColor.Magenta,
            PositiveModifier = ConsoleColor.Green,
            NegativeModifier = ConsoleColor.Red,
            NeutralModifier = ConsoleColor.Gray
        },
        Display = new StanceDisplaySettings
        {
            ShowDescription = false,
            ShowAllModifiers = true,
            CompactMode = true
        }
    };
}

/// <summary>Stance icon configuration.</summary>
public record StanceIconConfig
{
    /// <summary>Icon for aggressive stance.</summary>
    public string Aggressive { get; init; } = "⚔";

    /// <summary>Icon for defensive stance.</summary>
    public string Defensive { get; init; } = "🛡";

    /// <summary>Icon for balanced stance.</summary>
    public string Balanced { get; init; } = "⚖";

    /// <summary>Icon for tactical stance.</summary>
    public string Tactical { get; init; } = "🎯";

    /// <summary>Icon for special stance.</summary>
    public string Special { get; init; } = "✦";

    /// <summary>Default icon.</summary>
    public string Default { get; init; } = "◆";

    /// <summary>ASCII fallback for aggressive.</summary>
    public string AggressiveAscii { get; init; } = "[A]";

    /// <summary>ASCII fallback for defensive.</summary>
    public string DefensiveAscii { get; init; } = "[D]";

    /// <summary>ASCII fallback for balanced.</summary>
    public string BalancedAscii { get; init; } = "[B]";

    /// <summary>ASCII fallback for tactical.</summary>
    public string TacticalAscii { get; init; } = "[T]";

    /// <summary>ASCII fallback for special.</summary>
    public string SpecialAscii { get; init; } = "[S]";

    /// <summary>ASCII fallback for default.</summary>
    public string DefaultAscii { get; init; } = "[?]";
}

/// <summary>Stance color configuration.</summary>
public record StanceColorConfig
{
    /// <summary>Color for aggressive stance.</summary>
    public ConsoleColor Aggressive { get; init; }

    /// <summary>Color for defensive stance.</summary>
    public ConsoleColor Defensive { get; init; }

    /// <summary>Color for balanced stance.</summary>
    public ConsoleColor Balanced { get; init; }

    /// <summary>Color for tactical stance.</summary>
    public ConsoleColor Tactical { get; init; }

    /// <summary>Color for special stance.</summary>
    public ConsoleColor Special { get; init; }

    /// <summary>Color for positive modifiers.</summary>
    public ConsoleColor PositiveModifier { get; init; }

    /// <summary>Color for negative modifiers.</summary>
    public ConsoleColor NegativeModifier { get; init; }

    /// <summary>Color for neutral modifiers.</summary>
    public ConsoleColor NeutralModifier { get; init; }
}

/// <summary>Stance display behavior settings.</summary>
public record StanceDisplaySettings
{
    /// <summary>Whether to show stance description.</summary>
    public bool ShowDescription { get; init; }

    /// <summary>Whether to show all modifiers.</summary>
    public bool ShowAllModifiers { get; init; }

    /// <summary>Whether to use compact display mode.</summary>
    public bool CompactMode { get; init; }
}
