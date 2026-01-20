// ═══════════════════════════════════════════════════════════════════════════════
// ComboDisplayConfig.cs
// Configuration for combo chain display visuals.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for combo chain display visuals.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/combo-display.json</c> via dependency injection.
/// Provides configurable symbols, colors, and display settings for combo chains.
/// </para>
/// </remarks>
public record ComboDisplayConfig
{
    /// <summary>Symbols for combo display elements.</summary>
    public required ComboSymbolConfig Symbols { get; init; }

    /// <summary>Colors for combo display elements.</summary>
    public required ComboColorConfig Colors { get; init; }

    /// <summary>Display behavior settings.</summary>
    public required ComboDisplaySettings Display { get; init; }

    /// <summary>Message templates.</summary>
    public required ComboMessageConfig Messages { get; init; }

    /// <summary>Creates default configuration.</summary>
    public static ComboDisplayConfig CreateDefault() => new()
    {
        Symbols = new ComboSymbolConfig
        {
            CompletedStep = "✓",
            CurrentStep = ">",
            PendingStep = "?",
            ChainArrow = " → ",
            CompletedStepAscii = "*",
            CurrentStepAscii = ">",
            PendingStepAscii = "?",
            ChainArrowAscii = " -> "
        },
        Colors = new ComboColorConfig
        {
            CompletedStep = ConsoleColor.Green,
            CurrentStep = ConsoleColor.Yellow,
            PendingStep = ConsoleColor.DarkGray,
            HighBonus = ConsoleColor.Cyan,
            MediumBonus = ConsoleColor.Green,
            LowBonus = ConsoleColor.Yellow,
            NoBonus = ConsoleColor.Gray,
            UrgentWindow = ConsoleColor.Red,
            WarningWindow = ConsoleColor.Yellow,
            NormalWindow = ConsoleColor.White,
            ComboBreak = ConsoleColor.Red,
            ComboComplete = ConsoleColor.Cyan
        },
        Display = new ComboDisplaySettings
        {
            BonusMultiplierPerStep = 10,
            UrgentWindowThreshold = 2,
            ShowBonusPreview = true,
            ShowTargetRequirements = false,
            MaxContinuationOptions = 5
        },
        Messages = new ComboMessageConfig
        {
            ComboBreak = "COMBO BROKEN!",
            ComboComplete = "COMBO COMPLETE!",
            WindowExpired = "Window expired",
            ContinuePrompt = "Continue combo:",
            BuildingCombo = "Building combo...",
            NoActiveCombo = ""
        }
    };
}

/// <summary>Symbols for combo display.</summary>
public record ComboSymbolConfig
{
    /// <summary>Symbol for completed steps (default: ✓).</summary>
    public string CompletedStep { get; init; } = "✓";

    /// <summary>Symbol for current step (default: &gt;).</summary>
    public string CurrentStep { get; init; } = ">";

    /// <summary>Symbol for pending steps (default: ?).</summary>
    public string PendingStep { get; init; } = "?";

    /// <summary>Arrow between chain steps (default: →).</summary>
    public string ChainArrow { get; init; } = " → ";

    /// <summary>ASCII fallback for completed steps.</summary>
    public string CompletedStepAscii { get; init; } = "*";

    /// <summary>ASCII fallback for current step.</summary>
    public string CurrentStepAscii { get; init; } = ">";

    /// <summary>ASCII fallback for pending steps.</summary>
    public string PendingStepAscii { get; init; } = "?";

    /// <summary>ASCII fallback for chain arrow.</summary>
    public string ChainArrowAscii { get; init; } = " -> ";
}

/// <summary>Colors for combo display.</summary>
public record ComboColorConfig
{
    /// <summary>Color for completed steps.</summary>
    public ConsoleColor CompletedStep { get; init; }

    /// <summary>Color for current step.</summary>
    public ConsoleColor CurrentStep { get; init; }

    /// <summary>Color for pending steps.</summary>
    public ConsoleColor PendingStep { get; init; }

    /// <summary>Color for high bonus (75-100%).</summary>
    public ConsoleColor HighBonus { get; init; }

    /// <summary>Color for medium bonus (50-74%).</summary>
    public ConsoleColor MediumBonus { get; init; }

    /// <summary>Color for low bonus (25-49%).</summary>
    public ConsoleColor LowBonus { get; init; }

    /// <summary>Color for no bonus (0-24%).</summary>
    public ConsoleColor NoBonus { get; init; }

    /// <summary>Color for urgent window (1 turn).</summary>
    public ConsoleColor UrgentWindow { get; init; }

    /// <summary>Color for warning window (2 turns).</summary>
    public ConsoleColor WarningWindow { get; init; }

    /// <summary>Color for normal window (3+ turns).</summary>
    public ConsoleColor NormalWindow { get; init; }

    /// <summary>Color for combo break messages.</summary>
    public ConsoleColor ComboBreak { get; init; }

    /// <summary>Color for combo complete messages.</summary>
    public ConsoleColor ComboComplete { get; init; }
}

/// <summary>Display behavior settings.</summary>
public record ComboDisplaySettings
{
    /// <summary>Bonus percentage per completed step.</summary>
    public int BonusMultiplierPerStep { get; init; }

    /// <summary>Turns remaining threshold for urgent display.</summary>
    public int UrgentWindowThreshold { get; init; }

    /// <summary>Whether to show bonus preview in continuations.</summary>
    public bool ShowBonusPreview { get; init; }

    /// <summary>Whether to show target requirements.</summary>
    public bool ShowTargetRequirements { get; init; }

    /// <summary>Maximum continuation options to display.</summary>
    public int MaxContinuationOptions { get; init; }
}

/// <summary>Message templates.</summary>
public record ComboMessageConfig
{
    /// <summary>Message for combo break.</summary>
    public string ComboBreak { get; init; } = "COMBO BROKEN!";

    /// <summary>Message for combo completion.</summary>
    public string ComboComplete { get; init; } = "COMBO COMPLETE!";

    /// <summary>Reason for window expiration.</summary>
    public string WindowExpired { get; init; } = "Window expired";

    /// <summary>Prompt for continuation options.</summary>
    public string ContinuePrompt { get; init; } = "Continue combo:";

    /// <summary>Message when building combo.</summary>
    public string BuildingCombo { get; init; } = "Building combo...";

    /// <summary>Message when no active combo.</summary>
    public string NoActiveCombo { get; init; } = "";
}
