// ═══════════════════════════════════════════════════════════════════════════════
// CraftingStationConfig.cs
// Configuration for crafting station menu display settings.
// Version: 0.13.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for crafting station menu display settings.
/// </summary>
/// <remarks>
/// <para>Configures dimensions, colors, and progress bar settings for the
/// crafting station interface.</para>
/// <para>Load from config/crafting-station.json for runtime customization.</para>
/// </remarks>
/// <example>
/// <code>
/// var config = CraftingStationConfig.CreateDefault();
/// var menu = new CraftingStationMenu(..., config, ...);
/// </code>
/// </example>
public class CraftingStationConfig
{
    // ═══════════════════════════════════════════════════════════════════════════
    // MENU DIMENSIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Width of the crafting station menu.
    /// </summary>
    /// <value>Default: 70 characters.</value>
    public int MenuWidth { get; set; } = 70;

    /// <summary>
    /// Height of the crafting station menu.
    /// </summary>
    /// <value>Default: 20 lines.</value>
    public int MenuHeight { get; set; } = 20;

    /// <summary>
    /// Width of the recipe list panel (left side).
    /// </summary>
    /// <value>Default: 35 characters.</value>
    public int RecipeListWidth { get; set; } = 35;

    /// <summary>
    /// Width of the material panel (right side).
    /// </summary>
    /// <value>Default: 30 characters.</value>
    public int MaterialPanelWidth { get; set; } = 30;

    /// <summary>
    /// Maximum visible recipes in the list before scrolling.
    /// </summary>
    /// <value>Default: 8 recipes.</value>
    public int MaxVisibleRecipes { get; set; } = 8;

    /// <summary>
    /// Maximum visible materials in the panel.
    /// </summary>
    /// <value>Default: 6 materials.</value>
    public int MaxMaterialsVisible { get; set; } = 6;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROGRESS BAR SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Width of the progress bar in characters.
    /// </summary>
    /// <value>Default: 30 characters.</value>
    public int ProgressBarWidth { get; set; } = 30;

    /// <summary>
    /// Character for filled progress bar portion.
    /// </summary>
    /// <value>Default: '#'.</value>
    public char ProgressFilledChar { get; set; } = '#';

    /// <summary>
    /// Character for empty progress bar portion.
    /// </summary>
    /// <value>Default: '.'.</value>
    public char ProgressEmptyChar { get; set; } = '.';

    /// <summary>
    /// Number of animation steps for progress.
    /// </summary>
    /// <value>Default: 10 steps.</value>
    public int ProgressAnimationSteps { get; set; } = 10;

    /// <summary>
    /// Delay between animation steps in milliseconds.
    /// </summary>
    /// <value>Default: 100ms.</value>
    public int ProgressAnimationDelay { get; set; } = 100;

    // ═══════════════════════════════════════════════════════════════════════════
    // COLOR SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Color for station header.
    /// </summary>
    /// <value>Default: Yellow.</value>
    public ConsoleColor HeaderColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>
    /// Color for craftable recipe indicators ([x]).
    /// </summary>
    /// <value>Default: Green.</value>
    public ConsoleColor CraftableColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Color for non-craftable recipe indicators ([ ]).
    /// </summary>
    /// <value>Default: Red.</value>
    public ConsoleColor NotCraftableColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Color for selected recipe highlighting.
    /// </summary>
    /// <value>Default: Cyan.</value>
    public ConsoleColor SelectionColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Color for sufficient material indicators.
    /// </summary>
    /// <value>Default: Green.</value>
    public ConsoleColor SufficientMaterialColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Color for missing material highlighting.
    /// </summary>
    /// <value>Default: Red.</value>
    public ConsoleColor MissingMaterialColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Color for progress bar.
    /// </summary>
    /// <value>Default: Cyan.</value>
    public ConsoleColor ProgressBarColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Color for completion message.
    /// </summary>
    /// <value>Default: Green.</value>
    public ConsoleColor CompletionColor { get; set; } = ConsoleColor.Green;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a default configuration with standard settings.
    /// </summary>
    /// <returns>A new configuration with default values.</returns>
    public static CraftingStationConfig CreateDefault()
    {
        return new CraftingStationConfig();
    }
}
