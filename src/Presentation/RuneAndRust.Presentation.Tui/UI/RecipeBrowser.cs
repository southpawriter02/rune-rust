// ═══════════════════════════════════════════════════════════════════════════════
// RecipeBrowser.cs
// Displays all discovered recipes with search and category filtering.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays all discovered recipes with search and category filtering.
/// </summary>
/// <remarks>
/// <para>Provides recipe browsing functionality including text search by name,
/// category filtering, and recipe selection for detailed view.</para>
/// <para>Recipes are organized by category in a multi-column layout with
/// selection highlighting and craftability indicators.</para>
/// </remarks>
/// <example>
/// <code>
/// var browser = new RecipeBrowser(terminalService, config);
/// browser.SetPosition(5, 3);
/// browser.RenderRecipes(recipeList);
/// browser.FilterByCategory(RecipeCategory.Weapons);
/// </code>
/// </example>
public class RecipeBrowser
{
    private readonly ITerminalService _terminalService;
    private readonly RecipeBrowserConfig _config;
    private readonly ILogger<RecipeBrowser> _logger;

    // Current state
    private string _searchText = string.Empty;
    private RecipeCategory? _activeCategory;
    private IReadOnlyList<RecipeBrowserDisplayDto> _recipes = Array.Empty<RecipeBrowserDisplayDto>();
    private IReadOnlyList<RecipeBrowserDisplayDto> _filteredRecipes = Array.Empty<RecipeBrowserDisplayDto>();
    private int _selectedIndex;
    private (int X, int Y) _browserPosition;

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets whether the browser is currently visible.</summary>
    public bool IsVisible { get; private set; }

    /// <summary>Gets the currently selected recipe index.</summary>
    public int SelectedIndex => _selectedIndex;

    /// <summary>Gets the number of filtered recipes.</summary>
    public int RecipeCount => _filteredRecipes.Count;

    /// <summary>Gets the current search text.</summary>
    public string SearchText => _searchText;

    /// <summary>Gets the active category filter.</summary>
    public RecipeCategory? ActiveCategory => _activeCategory;

    /// <summary>Event raised when a recipe is selected for detailed view.</summary>
    public event Action<RecipeBrowserDisplayDto>? RecipeSelected;

    /// <summary>Event raised when the browser is closed.</summary>
    public event Action? BrowserClosed;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the RecipeBrowser component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for browser display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalService is null.</exception>
    public RecipeBrowser(
        ITerminalService terminalService,
        RecipeBrowserConfig? config = null,
        ILogger<RecipeBrowser>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? RecipeBrowserConfig.CreateDefault();
        _logger = logger ?? NullLogger<RecipeBrowser>.Instance;

        _logger.LogDebug(
            "RecipeBrowser initialized with {Width}x{Height} dimensions",
            _config.BrowserWidth,
            _config.BrowserHeight);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the browser position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _browserPosition = (x, y);
        _logger.LogDebug("Position set to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Renders the recipe browser with all discovered recipes.
    /// </summary>
    /// <param name="recipes">The recipes to display.</param>
    public void RenderRecipes(IEnumerable<RecipeBrowserDisplayDto> recipes)
    {
        _recipes = recipes.ToList();
        ApplyFilters();
        IsVisible = true;

        // Clear browser area
        ClearBrowser();

        // Render browser components
        RenderHeader();
        RenderSearchBox();
        RenderCategoryTabs();
        RenderRecipeList();
        RenderControls();

        _logger.LogDebug(
            "Rendered recipe browser with {Total} recipes, {Filtered} after filtering",
            _recipes.Count,
            _filteredRecipes.Count);
    }

    /// <summary>
    /// Searches recipes by name.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>Matching recipes.</returns>
    public IEnumerable<RecipeBrowserDisplayDto> Search(string searchText)
    {
        _searchText = searchText ?? string.Empty;
        ApplyFilters();
        RenderRecipes(_recipes);

        _logger.LogDebug("Search applied: '{SearchText}', {Count} results",
            _searchText, _filteredRecipes.Count);

        return _filteredRecipes;
    }

    /// <summary>
    /// Filters recipes by category.
    /// </summary>
    /// <param name="category">The category to filter by, or null for all.</param>
    public void FilterByCategory(RecipeCategory? category)
    {
        _activeCategory = category;
        ApplyFilters();
        RenderRecipes(_recipes);

        _logger.LogDebug("Category filter applied: {Category}",
            category?.ToString() ?? "All");
    }

    /// <summary>
    /// Selects a recipe by index.
    /// </summary>
    /// <param name="index">The recipe index to select.</param>
    public void SelectRecipe(int index)
    {
        if (index < 0 || index >= _filteredRecipes.Count)
        {
            return;
        }

        _selectedIndex = index;
        var recipe = _filteredRecipes[index];
        RecipeSelected?.Invoke(recipe);

        _logger.LogDebug("Selected recipe: {RecipeName}", recipe.Name);
    }

    /// <summary>
    /// Selects the previous recipe in the list.
    /// </summary>
    public void SelectPrevious()
    {
        if (_filteredRecipes.Count == 0) return;

        _selectedIndex = (_selectedIndex - 1 + _filteredRecipes.Count) % _filteredRecipes.Count;
        RefreshSelection();
    }

    /// <summary>
    /// Selects the next recipe in the list.
    /// </summary>
    public void SelectNext()
    {
        if (_filteredRecipes.Count == 0) return;

        _selectedIndex = (_selectedIndex + 1) % _filteredRecipes.Count;
        RefreshSelection();
    }

    /// <summary>
    /// Gets the currently selected recipe.
    /// </summary>
    /// <returns>The selected recipe, or null if none.</returns>
    public RecipeBrowserDisplayDto? GetSelectedRecipe()
    {
        if (_filteredRecipes.Count == 0 || _selectedIndex < 0 || _selectedIndex >= _filteredRecipes.Count)
        {
            return null;
        }

        return _filteredRecipes[_selectedIndex];
    }

    /// <summary>
    /// Appends a character to the search text.
    /// </summary>
    /// <param name="c">The character to append.</param>
    public void AppendSearchChar(char c)
    {
        _searchText += c;
        ApplyFilters();
        RenderRecipes(_recipes);
    }

    /// <summary>
    /// Removes the last character from the search text.
    /// </summary>
    public void BackspaceSearch()
    {
        if (_searchText.Length > 0)
        {
            _searchText = _searchText[..^1];
            ApplyFilters();
            RenderRecipes(_recipes);
        }
    }

    /// <summary>
    /// Clears the search text.
    /// </summary>
    public void ClearSearch()
    {
        _searchText = string.Empty;
        ApplyFilters();
        RenderRecipes(_recipes);
    }

    /// <summary>
    /// Hides the recipe browser and clears state.
    /// </summary>
    public void Hide()
    {
        if (!IsVisible) return;

        ClearBrowser();
        IsVisible = false;
        _recipes = Array.Empty<RecipeBrowserDisplayDto>();
        _filteredRecipes = Array.Empty<RecipeBrowserDisplayDto>();
        _selectedIndex = 0;
        BrowserClosed?.Invoke();

        _logger.LogDebug("Recipe browser hidden");
    }

    /// <summary>
    /// Handles keyboard input for the browser.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <returns>True if the input was handled.</returns>
    public bool HandleInput(ConsoleKey key)
    {
        return key switch
        {
            ConsoleKey.UpArrow or ConsoleKey.W => HandleNavigateUp(),
            ConsoleKey.DownArrow or ConsoleKey.S => HandleNavigateDown(),
            ConsoleKey.Enter => HandleSelect(),
            ConsoleKey.Tab => HandleCycleCategory(),
            ConsoleKey.Backspace => HandleBackspace(),
            ConsoleKey.Escape => HandleClose(),
            _ => false
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Rendering Methods
    // ═══════════════════════════════════════════════════════════════════════════

    #region Private Methods

    private void ClearBrowser()
    {
        var blankLine = new string(' ', _config.BrowserWidth);
        for (var y = 0; y < _config.BrowserHeight; y++)
        {
            _terminalService.WriteAt(_browserPosition.X, _browserPosition.Y + y, blankLine);
        }
    }

    private void RenderHeader()
    {
        var headerY = _browserPosition.Y;
        var width = _config.BrowserWidth;

        // Top border
        var topBorder = $"┌{new string('─', width - 2)}┐";
        _terminalService.WriteAt(_browserPosition.X, headerY, topBorder);

        // Title
        var title = "RECIPE BROWSER";
        var titlePadding = (width - 2 - title.Length) / 2;
        var titleLine = $"│{new string(' ', titlePadding)}{title}{new string(' ', width - 2 - titlePadding - title.Length)}│";
        _terminalService.WriteAt(_browserPosition.X, headerY + 1, titleLine);

        // Separator
        var separator = $"├{new string('─', width - 2)}┤";
        _terminalService.WriteAt(_browserPosition.X, headerY + 2, separator);
    }

    private void RenderSearchBox()
    {
        var searchY = _browserPosition.Y + 3;

        // Search label and input box
        var displayText = _searchText.Length > 0
            ? _searchText.PadRight(_config.SearchBoxWidth)
            : new string('_', _config.SearchBoxWidth);

        // Truncate if search text is too long
        if (displayText.Length > _config.SearchBoxWidth)
        {
            displayText = displayText[..^_config.SearchBoxWidth];
        }

        var searchLine = $"│  Search: [{displayText}]";
        var padding = _config.BrowserWidth - searchLine.Length - 1;
        searchLine += new string(' ', Math.Max(0, padding)) + "│";

        _terminalService.WriteAt(_browserPosition.X, searchY, searchLine);
    }

    private void RenderCategoryTabs()
    {
        var tabY = _browserPosition.Y + 4;

        // Filter tabs line
        var filterLine = "│  Filter:";
        var categories = new (string Name, RecipeCategory? Value)[]
        {
            ("All", null),
            ("Weapons", RecipeCategory.Weapons),
            ("Armor", RecipeCategory.Armor),
            ("Potions", RecipeCategory.Potions)
        };

        foreach (var (name, category) in categories)
        {
            var isActive = _activeCategory == category;
            var bracket = isActive ? $" [{name}]" : $"  {name} ";
            filterLine += bracket;
        }

        var padding = _config.BrowserWidth - filterLine.Length - 1;
        filterLine += new string(' ', Math.Max(0, padding)) + "│";
        _terminalService.WriteAt(_browserPosition.X, tabY, filterLine);

        // Separator after filters
        var separator = $"├{new string('─', _config.BrowserWidth - 2)}┤";
        _terminalService.WriteAt(_browserPosition.X, tabY + 1, separator);
    }

    private void RenderRecipeList()
    {
        var listY = _browserPosition.Y + 6;
        var maxWidth = _config.BrowserWidth - 4;
        var recipesPerColumn = _config.MaxRecipesPerCategory;

        // Group by category
        var grouped = _filteredRecipes
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        var y = listY;
        foreach (var category in Enum.GetValues<RecipeCategory>())
        {
            if (!grouped.TryGetValue(category, out var recipes) || recipes.Count == 0)
            {
                continue;
            }

            // Category header
            var header = category.ToString().ToUpperInvariant();
            _terminalService.WriteColoredAt(_browserPosition.X + 2, y, header, _config.CategoryHeaderColor);
            y++;

            // Recipe entries
            var displayCount = Math.Min(recipes.Count, recipesPerColumn);
            for (var i = 0; i < displayCount; i++)
            {
                var recipe = recipes[i];
                var globalIndex = _filteredRecipes.ToList().IndexOf(recipe);
                var isSelected = globalIndex == _selectedIndex;

                RenderRecipeEntry(_browserPosition.X + 2, y, recipe, isSelected, maxWidth);
                y++;
            }

            // Overflow indicator
            if (recipes.Count > displayCount)
            {
                var overflowText = $"  ... +{recipes.Count - displayCount} more";
                _terminalService.WriteColoredAt(_browserPosition.X + 2, y, overflowText, ConsoleColor.DarkGray);
                y++;
            }

            y++; // Spacing between categories
        }
    }

    private void RenderRecipeEntry(int x, int y, RecipeBrowserDisplayDto recipe, bool isSelected, int maxWidth)
    {
        var name = recipe.Name;
        if (name.Length > maxWidth - 10)
        {
            name = name[..(maxWidth - 12)] + "..";
        }

        // Selection indicator
        if (isSelected)
        {
            _terminalService.WriteColoredAt(x, y, "►", _config.SelectionColor);
        }

        // Craftable indicator
        var craftableIndicator = recipe.IsCraftable ? "[x]" : "[ ]";
        var indicatorColor = recipe.IsCraftable ? _config.CraftableColor : _config.NotCraftableColor;
        _terminalService.WriteColoredAt(x + 2, y, craftableIndicator, indicatorColor);

        // Recipe name
        var nameColor = isSelected ? _config.SelectionColor : ConsoleColor.White;
        _terminalService.WriteColoredAt(x + 6, y, name, nameColor);

        // New indicator
        if (recipe.IsNew)
        {
            _terminalService.WriteColoredAt(x + 6 + name.Length + 1, y, "NEW", _config.NewRecipeColor);
        }
    }

    private void RenderControls()
    {
        var controlsY = _browserPosition.Y + _config.BrowserHeight - 2;
        var width = _config.BrowserWidth;

        // Bottom separator
        var separator = $"├{new string('─', width - 2)}┤";
        _terminalService.WriteAt(_browserPosition.X, controlsY - 1, separator);

        // Controls hint
        var controls = "│  [↑/↓] Navigate   [Enter] Details   [Tab] Filter   [Esc] Close";
        var padding = width - controls.Length - 1;
        _terminalService.WriteAt(_browserPosition.X, controlsY, controls + new string(' ', Math.Max(0, padding)) + "│");

        // Bottom border
        var bottomBorder = $"└{new string('─', width - 2)}┘";
        _terminalService.WriteAt(_browserPosition.X, controlsY + 1, bottomBorder);
    }

    private void ApplyFilters()
    {
        var filtered = _recipes.AsEnumerable();

        // Apply search filter (case-insensitive substring)
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var searchLower = _searchText.ToLowerInvariant();
            filtered = filtered.Where(r =>
                r.Name.ToLowerInvariant().Contains(searchLower));
        }

        // Apply category filter
        if (_activeCategory.HasValue)
        {
            filtered = filtered.Where(r => r.Category == _activeCategory.Value);
        }

        _filteredRecipes = filtered.ToList();
        _selectedIndex = Math.Min(_selectedIndex, Math.Max(0, _filteredRecipes.Count - 1));
    }

    private void RefreshSelection()
    {
        RenderRecipeList();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Input Handlers
    // ═══════════════════════════════════════════════════════════════════════════

    private bool HandleNavigateUp()
    {
        SelectPrevious();
        return true;
    }

    private bool HandleNavigateDown()
    {
        SelectNext();
        return true;
    }

    private bool HandleSelect()
    {
        var recipe = GetSelectedRecipe();
        if (recipe != null)
        {
            RecipeSelected?.Invoke(recipe);
        }
        return true;
    }

    private bool HandleCycleCategory()
    {
        var categories = new RecipeCategory?[]
        {
            null,
            RecipeCategory.Weapons,
            RecipeCategory.Armor,
            RecipeCategory.Potions,
            RecipeCategory.Jewelry,
            RecipeCategory.Tools
        };

        var currentIndex = Array.IndexOf(categories, _activeCategory);
        var nextIndex = (currentIndex + 1) % categories.Length;
        FilterByCategory(categories[nextIndex]);
        return true;
    }

    private bool HandleBackspace()
    {
        BackspaceSearch();
        return true;
    }

    private bool HandleClose()
    {
        Hide();
        return true;
    }

    #endregion
}
