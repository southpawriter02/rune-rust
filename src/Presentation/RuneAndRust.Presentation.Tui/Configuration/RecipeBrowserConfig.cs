// ═══════════════════════════════════════════════════════════════════════════════
// RecipeBrowserConfig.cs
// Configuration for the recipe browser UI components.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration settings for the recipe browser UI components.
/// </summary>
/// <remarks>
/// Controls display settings for the recipe browser, recipe details panel,
/// recipe book, notifications, and quality tier rendering.
/// </remarks>
public class RecipeBrowserConfig
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Browser Settings
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the browser panel width.</summary>
    public int BrowserWidth { get; set; } = 70;

    /// <summary>Gets or sets the browser panel height.</summary>
    public int BrowserHeight { get; set; } = 25;

    /// <summary>Gets or sets the search box width.</summary>
    public int SearchBoxWidth { get; set; } = 15;

    /// <summary>Gets or sets the maximum recipes per category in browser.</summary>
    public int MaxRecipesPerCategory { get; set; } = 6;

    // ═══════════════════════════════════════════════════════════════════════════
    // Details Panel Settings
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the details panel width.</summary>
    public int DetailsWidth { get; set; } = 60;

    /// <summary>Gets or sets the details panel height.</summary>
    public int DetailsHeight { get; set; } = 16;

    // ═══════════════════════════════════════════════════════════════════════════
    // Recipe Book Settings
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the recipe book width.</summary>
    public int BookWidth { get; set; } = 50;

    /// <summary>Gets or sets the recipe book height.</summary>
    public int BookHeight { get; set; } = 20;

    /// <summary>Gets or sets the maximum recipes per station in book view.</summary>
    public int MaxRecipesPerStation { get; set; } = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // Notification Settings
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the notification popup width.</summary>
    public int NotificationWidth { get; set; } = 55;

    /// <summary>Gets or sets the notification auto-dismiss duration in seconds.</summary>
    public int NotificationDurationSeconds { get; set; } = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // Color Settings
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the category header color.</summary>
    public ConsoleColor CategoryHeaderColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>Gets or sets the station header color in recipe book.</summary>
    public ConsoleColor StationHeaderColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>Gets or sets the selected item color.</summary>
    public ConsoleColor SelectionColor { get; set; } = ConsoleColor.Green;

    /// <summary>Gets or sets the active filter tab color.</summary>
    public ConsoleColor ActiveFilterColor { get; set; } = ConsoleColor.White;

    /// <summary>Gets or sets the craftable recipe indicator color.</summary>
    public ConsoleColor CraftableColor { get; set; } = ConsoleColor.Green;

    /// <summary>Gets or sets the non-craftable recipe indicator color.</summary>
    public ConsoleColor NotCraftableColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>Gets or sets the sufficient material indicator color.</summary>
    public ConsoleColor SufficientMaterialColor { get; set; } = ConsoleColor.Green;

    /// <summary>Gets or sets the insufficient material indicator color.</summary>
    public ConsoleColor InsufficientMaterialColor { get; set; } = ConsoleColor.Red;

    /// <summary>Gets or sets the new recipe highlight color.</summary>
    public ConsoleColor NewRecipeColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>Gets or sets the progress bar color.</summary>
    public ConsoleColor ProgressBarColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>Gets or sets the notification border color.</summary>
    public ConsoleColor NotificationBorderColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>Gets or sets the notification highlight color.</summary>
    public ConsoleColor NotificationHighlightColor { get; set; } = ConsoleColor.Green;

    // ═══════════════════════════════════════════════════════════════════════════
    // Quality Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the Common quality color.</summary>
    public ConsoleColor CommonQualityColor { get; set; } = ConsoleColor.Gray;

    /// <summary>Gets or sets the Uncommon quality color.</summary>
    public ConsoleColor UncommonQualityColor { get; set; } = ConsoleColor.Green;

    /// <summary>Gets or sets the Rare quality color.</summary>
    public ConsoleColor RareQualityColor { get; set; } = ConsoleColor.Blue;

    /// <summary>Gets or sets the Epic quality color.</summary>
    public ConsoleColor EpicQualityColor { get; set; } = ConsoleColor.Magenta;

    /// <summary>Gets or sets the Legendary quality color.</summary>
    public ConsoleColor LegendaryQualityColor { get; set; } = ConsoleColor.Yellow;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a default configuration instance.
    /// </summary>
    /// <returns>A new RecipeBrowserConfig with default values.</returns>
    public static RecipeBrowserConfig CreateDefault() => new();
}
