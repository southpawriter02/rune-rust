// ═══════════════════════════════════════════════════════════════════════════════
// GatheringDisplayConfig.cs
// Configuration for gathering display components.
// Version: 0.13.3d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for gathering display components.
/// </summary>
/// <remarks>
/// <para>Controls the dimensions and colors for harvestable indicators
/// and gathering display panels.</para>
/// </remarks>
public class GatheringDisplayConfig
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Indicator Settings
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Width of the harvestable indicators section.</summary>
    public int IndicatorWidth { get; set; } = 60;

    /// <summary>Height of the harvestable indicators section.</summary>
    public int IndicatorHeight { get; set; } = 10;

    /// <summary>Maximum harvestable nodes visible at once.</summary>
    public int MaxVisibleNodes { get; set; } = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // Display Settings
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Width of the gathering display.</summary>
    public int DisplayWidth { get; set; } = 50;

    /// <summary>Height of the gathering display.</summary>
    public int DisplayHeight { get; set; } = 12;

    /// <summary>Delay between animation frames in milliseconds.</summary>
    public int AnimationDelayMs { get; set; } = 300;

    // ═══════════════════════════════════════════════════════════════════════════
    // Color Settings
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Color for selected nodes.</summary>
    public ConsoleColor SelectionColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>Color for difficulty class display.</summary>
    public ConsoleColor DifficultyColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>Color for gather prompts.</summary>
    public ConsoleColor PromptColor { get; set; } = ConsoleColor.Green;

    /// <summary>Color for headers.</summary>
    public ConsoleColor HeaderColor { get; set; } = ConsoleColor.White;

    /// <summary>Color for success indicators.</summary>
    public ConsoleColor SuccessColor { get; set; } = ConsoleColor.Green;

    /// <summary>Color for failure indicators.</summary>
    public ConsoleColor FailureColor { get; set; } = ConsoleColor.Red;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a default configuration instance.
    /// </summary>
    /// <returns>A new configuration with default values.</returns>
    public static GatheringDisplayConfig CreateDefault()
    {
        return new GatheringDisplayConfig();
    }
}
