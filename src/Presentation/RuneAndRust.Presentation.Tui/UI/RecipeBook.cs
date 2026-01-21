// ═══════════════════════════════════════════════════════════════════════════════
// RecipeBook.cs
// Displays the player's discovered recipe collection with progress tracking.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the player's discovered recipe collection with progress tracking.
/// </summary>
/// <remarks>
/// <para>Shows all discovered recipes organized by station, with discovery
/// progress and highlighting for newly discovered recipes.</para>
/// <para>Display format:</para>
/// <code>
/// ┌────────────────────────────────────────────────┐
/// │                 RECIPE BOOK                     │
/// ├────────────────────────────────────────────────┤
/// │ Recipes Discovered: 12 / 35 (34%)              │
/// │ [############........................]          │
/// │                                                 │
/// │ BLACKSMITH                                      │
/// │   • Iron Sword                                  │
/// │   • Steel Blade        NEW                      │
/// │                                                 │
/// │ ALCHEMIST                                       │
/// │   • Health Potion                               │
/// └────────────────────────────────────────────────┘
/// </code>
/// </remarks>
public class RecipeBook
{
    private readonly ITerminalService _terminalService;
    private readonly RecipeBrowserConfig _config;
    private readonly ILogger<RecipeBook> _logger;

    // State
    private IReadOnlyList<RecipeBookEntryDto> _discoveredRecipes = Array.Empty<RecipeBookEntryDto>();
    private string? _stationFilter;
    private readonly HashSet<string> _newRecipeIds = new();
    private (int X, int Y) _bookPosition;
    private int _totalRecipeCount;

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets whether the recipe book is visible.</summary>
    public bool IsVisible { get; private set; }

    /// <summary>Gets the current station filter.</summary>
    public string? StationFilter => _stationFilter;

    /// <summary>Gets the count of discovered recipes.</summary>
    public int DiscoveredCount => _discoveredRecipes.Count;

    /// <summary>Gets the total recipe count.</summary>
    public int TotalRecipeCount => _totalRecipeCount;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the RecipeBook component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalService is null.</exception>
    public RecipeBook(
        ITerminalService terminalService,
        RecipeBrowserConfig? config = null,
        ILogger<RecipeBook>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? RecipeBrowserConfig.CreateDefault();
        _logger = logger ?? NullLogger<RecipeBook>.Instance;

        _logger.LogDebug(
            "RecipeBook initialized with {Width}x{Height} dimensions",
            _config.BookWidth,
            _config.BookHeight);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the recipe book position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _bookPosition = (x, y);
        _logger.LogDebug("Position set to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Sets the total recipe count for progress calculation.
    /// </summary>
    /// <param name="totalCount">The total number of recipes in the game.</param>
    public void SetTotalRecipeCount(int totalCount)
    {
        _totalRecipeCount = totalCount;
    }

    /// <summary>
    /// Renders the discovered recipes.
    /// </summary>
    /// <param name="recipes">The discovered recipes.</param>
    public void RenderDiscoveredRecipes(IEnumerable<RecipeBookEntryDto> recipes)
    {
        _discoveredRecipes = recipes.ToList();
        IsVisible = true;

        // Clear book area
        ClearBook();

        // Render book header with progress
        RenderHeader();
        ShowUnlockProgress(_discoveredRecipes.Count, _totalRecipeCount);
        RenderRecipesByStation();

        _logger.LogDebug("Rendered recipe book with {Count} discovered recipes",
            _discoveredRecipes.Count);
    }

    /// <summary>
    /// Shows the unlock progress display.
    /// </summary>
    /// <param name="discovered">Number of discovered recipes.</param>
    /// <param name="total">Total number of recipes.</param>
    public void ShowUnlockProgress(int discovered, int total)
    {
        var progressY = _bookPosition.Y + 3;
        var percentage = total > 0 ? (discovered * 100) / total : 0;

        // Progress text
        var progressText = $"Recipes Discovered: {discovered} / {total} ({percentage}%)";
        _terminalService.WriteAt(_bookPosition.X + 2, progressY, progressText);

        // Progress bar
        var barWidth = 30;
        var filledWidth = total > 0 ? (discovered * barWidth) / total : 0;
        var bar = $"[{new string('#', filledWidth)}{new string('.', barWidth - filledWidth)}]";
        _terminalService.WriteColoredAt(_bookPosition.X + 2, progressY + 1, bar, _config.ProgressBarColor);
    }

    /// <summary>
    /// Highlights a newly discovered recipe.
    /// </summary>
    /// <param name="recipeId">The ID of the new recipe.</param>
    public void HighlightNew(string recipeId)
    {
        _newRecipeIds.Add(recipeId);
        _logger.LogDebug("Marked recipe as new: {RecipeId}", recipeId);
    }

    /// <summary>
    /// Clears the new highlight from a recipe.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe.</param>
    public void ClearNewHighlight(string recipeId)
    {
        _newRecipeIds.Remove(recipeId);
    }

    /// <summary>
    /// Clears all new recipe highlights.
    /// </summary>
    public void ClearAllNewHighlights()
    {
        _newRecipeIds.Clear();
    }

    /// <summary>
    /// Filters recipes by crafting station.
    /// </summary>
    /// <param name="stationId">The station ID to filter by, or null for all.</param>
    public void FilterByStation(string? stationId)
    {
        _stationFilter = stationId;
        RenderDiscoveredRecipes(_discoveredRecipes);

        _logger.LogDebug("Station filter applied: {StationId}",
            stationId ?? "All");
    }

    /// <summary>
    /// Hides the recipe book.
    /// </summary>
    public void Hide()
    {
        if (!IsVisible) return;

        ClearBook();
        IsVisible = false;
        _discoveredRecipes = Array.Empty<RecipeBookEntryDto>();

        _logger.LogDebug("Recipe book hidden");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods
    // ═══════════════════════════════════════════════════════════════════════════

    #region Private Methods

    private void ClearBook()
    {
        var blankLine = new string(' ', _config.BookWidth);
        for (var y = 0; y < _config.BookHeight; y++)
        {
            _terminalService.WriteAt(_bookPosition.X, _bookPosition.Y + y, blankLine);
        }
    }

    private void RenderHeader()
    {
        var headerY = _bookPosition.Y;
        var width = _config.BookWidth;

        // Top border
        var topBorder = $"┌{new string('─', width - 2)}┐";
        _terminalService.WriteAt(_bookPosition.X, headerY, topBorder);

        // Title
        var title = "RECIPE BOOK";
        var titlePadding = (width - 2 - title.Length) / 2;
        var titleLine = $"│{new string(' ', titlePadding)}{title}{new string(' ', width - 2 - titlePadding - title.Length)}│";
        _terminalService.WriteAt(_bookPosition.X, headerY + 1, titleLine);

        // Separator
        var separator = $"├{new string('─', width - 2)}┤";
        _terminalService.WriteAt(_bookPosition.X, headerY + 2, separator);
    }

    private void RenderRecipesByStation()
    {
        var listY = _bookPosition.Y + 6;

        // Apply station filter
        var recipes = _stationFilter == null
            ? _discoveredRecipes
            : _discoveredRecipes.Where(r => r.StationId == _stationFilter).ToList();

        // Group by station
        var grouped = recipes
            .GroupBy(r => r.StationName)
            .ToDictionary(g => g.Key, g => g.ToList());

        var y = listY;
        foreach (var (stationName, stationRecipes) in grouped)
        {
            // Check if we have room
            if (y >= _bookPosition.Y + _config.BookHeight - 2)
            {
                break;
            }

            // Station header
            _terminalService.WriteColoredAt(
                _bookPosition.X + 2,
                y,
                stationName.ToUpperInvariant(),
                _config.StationHeaderColor);
            y++;

            // Recipe entries
            foreach (var recipe in stationRecipes.Take(_config.MaxRecipesPerStation))
            {
                if (y >= _bookPosition.Y + _config.BookHeight - 2)
                {
                    break;
                }

                var isNew = _newRecipeIds.Contains(recipe.RecipeId);
                RenderBookEntry(recipe, y, isNew);
                y++;
            }

            // Overflow indicator
            if (stationRecipes.Count > _config.MaxRecipesPerStation)
            {
                var overflow = $"  ... +{stationRecipes.Count - _config.MaxRecipesPerStation} more";
                _terminalService.WriteColoredAt(
                    _bookPosition.X + 2,
                    y,
                    overflow,
                    ConsoleColor.DarkGray);
                y++;
            }

            y++; // Spacing between stations
        }
    }

    private void RenderBookEntry(RecipeBookEntryDto recipe, int y, bool isNew)
    {
        var entryLine = $"  • {recipe.Name}";
        _terminalService.WriteAt(_bookPosition.X + 2, y, entryLine);

        if (isNew)
        {
            _terminalService.WriteColoredAt(
                _bookPosition.X + 4 + recipe.Name.Length + 2,
                y,
                "NEW",
                _config.NewRecipeColor);
        }
    }

    #endregion
}
