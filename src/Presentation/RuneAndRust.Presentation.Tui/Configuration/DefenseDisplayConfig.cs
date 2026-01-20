// ═══════════════════════════════════════════════════════════════════════════════
// DefenseDisplayConfig.cs
// Configuration for defense action prompt display.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for defense action prompt display.
/// </summary>
public record DefenseDisplayConfig
{
    /// <summary>Action key mappings.</summary>
    public required DefenseActionKeyConfig ActionKeys { get; init; }

    /// <summary>Display symbols.</summary>
    public required DefenseSymbolConfig Symbols { get; init; }

    /// <summary>Display colors.</summary>
    public required DefenseColorConfig Colors { get; init; }

    /// <summary>Display behavior settings.</summary>
    public required DefenseDisplaySettings Display { get; init; }

    /// <summary>Creates default configuration.</summary>
    public static DefenseDisplayConfig CreateDefault() => new()
    {
        ActionKeys = new DefenseActionKeyConfig
        {
            Block = 'B',
            Dodge = 'D',
            Parry = 'P',
            Counter = 'C'
        },
        Symbols = new DefenseSymbolConfig
        {
            TimingBarFilled = '=',
            TimingBarEmpty = '.',
            Separator = '─'
        },
        Colors = new DefenseColorConfig
        {
            IncomingAttack = ConsoleColor.Red,
            TimingNormal = ConsoleColor.Green,
            TimingWarning = ConsoleColor.Yellow,
            TimingCritical = ConsoleColor.Red,
            DefenseSuccess = ConsoleColor.Green,
            DefenseFailure = ConsoleColor.Red,
            ActionAvailable = ConsoleColor.White,
            ActionUnavailable = ConsoleColor.DarkGray
        },
        Display = new DefenseDisplaySettings
        {
            TimingBarWidth = 10,
            ShowSuccessChance = true,
            ShowStaminaCost = true,
            ShowDamageReduction = true
        }
    };
}

/// <summary>Action key mappings.</summary>
public record DefenseActionKeyConfig
{
    /// <summary>Block action key.</summary>
    public char Block { get; init; }

    /// <summary>Dodge action key.</summary>
    public char Dodge { get; init; }

    /// <summary>Parry action key.</summary>
    public char Parry { get; init; }

    /// <summary>Counter action key.</summary>
    public char Counter { get; init; }
}

/// <summary>Defense display symbols.</summary>
public record DefenseSymbolConfig
{
    /// <summary>Character for filled portion of timing bar.</summary>
    public char TimingBarFilled { get; init; }

    /// <summary>Character for empty portion of timing bar.</summary>
    public char TimingBarEmpty { get; init; }

    /// <summary>Separator character.</summary>
    public char Separator { get; init; }
}

/// <summary>Defense display colors.</summary>
public record DefenseColorConfig
{
    /// <summary>Color for incoming attack header.</summary>
    public ConsoleColor IncomingAttack { get; init; }

    /// <summary>Color for timing bar with plenty of time.</summary>
    public ConsoleColor TimingNormal { get; init; }

    /// <summary>Color for timing bar with moderate time.</summary>
    public ConsoleColor TimingWarning { get; init; }

    /// <summary>Color for timing bar with critical time.</summary>
    public ConsoleColor TimingCritical { get; init; }

    /// <summary>Color for successful defense.</summary>
    public ConsoleColor DefenseSuccess { get; init; }

    /// <summary>Color for failed defense.</summary>
    public ConsoleColor DefenseFailure { get; init; }

    /// <summary>Color for available actions.</summary>
    public ConsoleColor ActionAvailable { get; init; }

    /// <summary>Color for unavailable actions.</summary>
    public ConsoleColor ActionUnavailable { get; init; }
}

/// <summary>Defense display behavior settings.</summary>
public record DefenseDisplaySettings
{
    /// <summary>Width of the timing bar in characters.</summary>
    public int TimingBarWidth { get; init; }

    /// <summary>Whether to show success chance.</summary>
    public bool ShowSuccessChance { get; init; }

    /// <summary>Whether to show stamina cost.</summary>
    public bool ShowStaminaCost { get; init; }

    /// <summary>Whether to show damage reduction.</summary>
    public bool ShowDamageReduction { get; init; }
}
