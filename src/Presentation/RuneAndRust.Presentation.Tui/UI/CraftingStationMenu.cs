// ═══════════════════════════════════════════════════════════════════════════════
// CraftingStationMenu.cs
// Displays the crafting station interface with recipes and crafting controls.
// Version: 0.13.3b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the crafting station interface with recipes and crafting controls.
/// </summary>
/// <remarks>
/// <para>Shows station-specific recipes with craftability indicators, material requirements,
/// and provides crafting progress feedback during item creation.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item><description>Station header with name display</description></item>
///   <item><description>Scrollable recipe list with craftability indicators</description></item>
///   <item><description>Material availability panel for selected recipe</description></item>
///   <item><description>Navigation with ↑/↓ arrows</description></item>
///   <item><description>Crafting with [C] key</description></item>
///   <item><description>Close with Escape key</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var menu = new CraftingStationMenu(materialDisplay, progressRenderer, terminal, config);
/// menu.SetPosition(10, 5);
/// menu.RenderStation(stationDto, recipes, playerResources);
/// </code>
/// </example>
public class CraftingStationMenu
{
    private readonly MaterialAvailabilityDisplay _materialDisplay;
    private readonly CraftingProgressRenderer _progressRenderer;
    private readonly ITerminalService _terminalService;
    private readonly CraftingStationConfig _config;
    private readonly ILogger<CraftingStationMenu> _logger;

    private CraftingStationDisplayDto? _currentStation;
    private IReadOnlyList<StationRecipeDisplayDto> _recipes = Array.Empty<StationRecipeDisplayDto>();
    private IReadOnlyList<ResourceStackDisplayDto> _playerResources = Array.Empty<ResourceStackDisplayDto>();
    private int _selectedIndex;
    private bool _isCrafting;
    private (int X, int Y) _menuPosition;

    /// <summary>
    /// Event raised when a craft action is requested.
    /// </summary>
    public event Action<string>? CraftRequested;

    /// <summary>
    /// Event raised when the menu is closed.
    /// </summary>
    public event Action? MenuClosed;

    /// <summary>
    /// Gets whether the menu is currently visible.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Gets the currently selected recipe index.
    /// </summary>
    public int SelectedIndex => _selectedIndex;

    /// <summary>
    /// Gets the current station being displayed.
    /// </summary>
    public CraftingStationDisplayDto? CurrentStation => _currentStation;

    /// <summary>
    /// Gets the number of recipes in the list.
    /// </summary>
    public int RecipeCount => _recipes.Count;

    /// <summary>
    /// Creates a new instance of the CraftingStationMenu component.
    /// </summary>
    /// <param name="materialDisplay">The material availability display component.</param>
    /// <param name="progressRenderer">The crafting progress renderer.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for station menu display.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when materialDisplay, progressRenderer, or terminalService is null.
    /// </exception>
    public CraftingStationMenu(
        MaterialAvailabilityDisplay materialDisplay,
        CraftingProgressRenderer progressRenderer,
        ITerminalService terminalService,
        CraftingStationConfig? config = null,
        ILogger<CraftingStationMenu>? logger = null)
    {
        _materialDisplay = materialDisplay ?? throw new ArgumentNullException(nameof(materialDisplay));
        _progressRenderer = progressRenderer ?? throw new ArgumentNullException(nameof(progressRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? CraftingStationConfig.CreateDefault();
        _logger = logger ?? NullLogger<CraftingStationMenu>.Instance;

        _logger.LogDebug(
            "CraftingStationMenu initialized with {Width}x{Height} dimensions",
            _config.MenuWidth,
            _config.MenuHeight);
    }

    /// <summary>
    /// Sets the menu position on the terminal.
    /// </summary>
    /// <param name="x">The X coordinate (column).</param>
    /// <param name="y">The Y coordinate (row).</param>
    public void SetPosition(int x, int y)
    {
        _menuPosition = (x, y);

        _logger.LogDebug("Menu position set to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Renders the crafting station menu with recipes.
    /// </summary>
    /// <param name="station">The crafting station to display.</param>
    /// <param name="recipes">The recipes available at this station.</param>
    /// <param name="playerResources">The player's current resources.</param>
    /// <remarks>
    /// This method displays the full menu including header, recipe list,
    /// material panel for the selected recipe, and control hints.
    /// </remarks>
    public void RenderStation(
        CraftingStationDisplayDto station,
        IEnumerable<StationRecipeDisplayDto> recipes,
        IEnumerable<ResourceStackDisplayDto> playerResources)
    {
        _currentStation = station;
        _recipes = recipes.ToList();
        _playerResources = playerResources.ToList();
        _selectedIndex = 0;
        IsVisible = true;

        _logger.LogInformation(
            "Opened crafting station: {StationName}",
            station.Name);

        // Clear menu area
        ClearMenu();

        // Render station header
        RenderHeader(station.Name);

        // Render recipe list
        RenderRecipeList();

        // Render material panel for selected recipe
        if (_recipes.Count > 0)
        {
            RenderMaterialPanel(_recipes[_selectedIndex]);
        }

        // Render controls hint
        RenderControls();

        _logger.LogDebug(
            "Rendered crafting station {StationName} with {RecipeCount} recipes",
            station.Name,
            _recipes.Count);
    }

    /// <summary>
    /// Renders the list of available recipes.
    /// </summary>
    public void RenderRecipeList()
    {
        var listY = _menuPosition.Y + 4;
        var listWidth = _config.RecipeListWidth;

        // Column headers
        _terminalService.WriteAt(_menuPosition.X + 2, listY, "AVAILABLE RECIPES");
        _terminalService.WriteAt(_menuPosition.X + 2, listY + 1, new string('─', 17));

        // Handle empty recipe list
        if (_recipes.Count == 0)
        {
            _terminalService.WriteColoredAt(
                _menuPosition.X + 2,
                listY + 3,
                "No recipes available.",
                ConsoleColor.DarkGray);
            return;
        }

        // Calculate visible range with scrolling
        var displayCount = Math.Min(_recipes.Count, _config.MaxVisibleRecipes);
        var startIndex = Math.Max(0, _selectedIndex - displayCount / 2);
        startIndex = Math.Min(startIndex, Math.Max(0, _recipes.Count - displayCount));

        // Render recipe entries
        for (var i = 0; i < displayCount; i++)
        {
            var recipeIndex = startIndex + i;
            var recipe = _recipes[recipeIndex];
            var isSelected = recipeIndex == _selectedIndex;

            var entryY = listY + 3 + i;
            RenderRecipeEntry(recipe, entryY, isSelected);
        }

        // Scroll indicators
        if (startIndex > 0)
        {
            _terminalService.WriteAt(_menuPosition.X + listWidth - 4, listY + 2, "▲");
        }

        if (startIndex + displayCount < _recipes.Count)
        {
            _terminalService.WriteAt(_menuPosition.X + listWidth - 4, listY + 3 + displayCount, "▼");
        }
    }

    /// <summary>
    /// Selects a recipe by index.
    /// </summary>
    /// <param name="index">The recipe index to select.</param>
    public void SelectRecipe(int index)
    {
        if (index < 0 || index >= _recipes.Count)
        {
            return;
        }

        _selectedIndex = index;

        _logger.LogDebug(
            "Selected recipe at index {Index}: {RecipeName}",
            index,
            _recipes[index].Name);
    }

    /// <summary>
    /// Selects the previous recipe in the list.
    /// </summary>
    public void SelectPrevious()
    {
        if (_recipes.Count == 0)
        {
            return;
        }

        _selectedIndex = (_selectedIndex - 1 + _recipes.Count) % _recipes.Count;
        RefreshRecipeDisplay();

        _logger.LogDebug(
            "Selected previous recipe, now at index {Index}",
            _selectedIndex);
    }

    /// <summary>
    /// Selects the next recipe in the list.
    /// </summary>
    public void SelectNext()
    {
        if (_recipes.Count == 0)
        {
            return;
        }

        _selectedIndex = (_selectedIndex + 1) % _recipes.Count;
        RefreshRecipeDisplay();

        _logger.LogDebug(
            "Selected next recipe, now at index {Index}",
            _selectedIndex);
    }

    /// <summary>
    /// Gets the currently selected recipe.
    /// </summary>
    /// <returns>The selected recipe, or null if none.</returns>
    public StationRecipeDisplayDto? GetSelectedRecipe()
    {
        if (_recipes.Count == 0 || _selectedIndex < 0 || _selectedIndex >= _recipes.Count)
        {
            return null;
        }

        return _recipes[_selectedIndex];
    }

    /// <summary>
    /// Shows crafting progress.
    /// </summary>
    /// <param name="progress">The progress value (0.0 to 1.0).</param>
    /// <remarks>
    /// Displays a progress bar for the currently selected recipe.
    /// When progress reaches 1.0, shows the completion message.
    /// </remarks>
    public void ShowCraftingProgress(float progress)
    {
        _isCrafting = progress < 1.0f;
        var progressY = _menuPosition.Y + _config.MenuHeight - 4;

        var selectedRecipe = GetSelectedRecipe();
        var recipeName = selectedRecipe?.Name ?? "Unknown";

        // Render progress bar
        var progressLine = _progressRenderer.RenderProgress(progress, recipeName);
        var lines = progressLine.Split('\n');

        // Clear area
        var clearLine = new string(' ', _config.ProgressBarWidth + 30);
        _terminalService.WriteAt(_menuPosition.X + 2, progressY, clearLine);
        _terminalService.WriteAt(_menuPosition.X + 2, progressY + 1, clearLine);

        // Write progress
        _terminalService.WriteAt(_menuPosition.X + 2, progressY, lines[0]);
        _terminalService.WriteColoredAt(_menuPosition.X + 2, progressY + 1, lines[1], _config.ProgressBarColor);

        // Show completion if done
        if (progress >= 1.0f)
        {
            _progressRenderer.ShowComplete(_menuPosition.X + 2, progressY + 2, _terminalService);
            _isCrafting = false;

            _logger.LogInformation(
                "Crafting complete: {RecipeName}",
                recipeName);
        }
    }

    /// <summary>
    /// Hides the crafting station menu.
    /// </summary>
    public void Hide()
    {
        if (!IsVisible)
        {
            return;
        }

        ClearMenu();
        IsVisible = false;
        _currentStation = null;
        _recipes = Array.Empty<StationRecipeDisplayDto>();
        _selectedIndex = 0;

        MenuClosed?.Invoke();

        _logger.LogDebug("Crafting station menu hidden");
    }

    /// <summary>
    /// Handles keyboard input for the menu.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <returns>True if the input was handled.</returns>
    /// <remarks>
    /// <para>Key mappings:</para>
    /// <list type="bullet">
    ///   <item><description>↑/W: Select previous recipe</description></item>
    ///   <item><description>↓/S: Select next recipe</description></item>
    ///   <item><description>C: Craft selected recipe</description></item>
    ///   <item><description>Esc: Close menu</description></item>
    /// </list>
    /// </remarks>
    public bool HandleInput(ConsoleKey key)
    {
        // Ignore input while crafting
        if (_isCrafting)
        {
            return false;
        }

        return key switch
        {
            ConsoleKey.UpArrow or ConsoleKey.W => HandleNavigateUp(),
            ConsoleKey.DownArrow or ConsoleKey.S => HandleNavigateDown(),
            ConsoleKey.C => HandleCraftAction(),
            ConsoleKey.Escape => HandleClose(),
            _ => false
        };
    }

    #region Private Methods

    /// <summary>
    /// Clears the entire menu area.
    /// </summary>
    private void ClearMenu()
    {
        var blankLine = new string(' ', _config.MenuWidth);
        for (var y = 0; y < _config.MenuHeight; y++)
        {
            _terminalService.WriteAt(_menuPosition.X, _menuPosition.Y + y, blankLine);
        }
    }

    /// <summary>
    /// Renders the menu header with station name.
    /// </summary>
    /// <param name="stationName">The station name to display.</param>
    private void RenderHeader(string stationName)
    {
        var headerY = _menuPosition.Y;
        var width = _config.MenuWidth;

        // Top border: ┌─────────┐
        var topBorder = $"┌{new string('─', width - 2)}┐";
        _terminalService.WriteAt(_menuPosition.X, headerY, topBorder);

        // Station name (centered and uppercase)
        var title = stationName.ToUpperInvariant();
        var titlePadding = (width - 2 - title.Length) / 2;
        var rightPadding = width - 2 - titlePadding - title.Length;
        var titleLine = $"│{new string(' ', titlePadding)}{title}{new string(' ', rightPadding)}│";
        _terminalService.WriteColoredAt(_menuPosition.X, headerY + 1, titleLine, _config.HeaderColor);

        // Separator: ├─────────┤
        var separator = $"├{new string('─', width - 2)}┤";
        _terminalService.WriteAt(_menuPosition.X, headerY + 2, separator);
    }

    /// <summary>
    /// Renders a single recipe entry in the list.
    /// </summary>
    /// <param name="recipe">The recipe to render.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="isSelected">Whether this recipe is selected.</param>
    private void RenderRecipeEntry(StationRecipeDisplayDto recipe, int y, bool isSelected)
    {
        // Craftability indicator
        var indicator = recipe.IsCraftable ? "[x]" : "[ ]";
        var indicatorColor = recipe.IsCraftable ? _config.CraftableColor : _config.NotCraftableColor;

        // Recipe name (truncate if needed)
        var maxNameWidth = _config.RecipeListWidth - 14;
        var entryText = recipe.Name;
        if (entryText.Length > maxNameWidth)
        {
            entryText = entryText.Substring(0, maxNameWidth - 2) + "..";
        }

        // Clear entry line
        var clearLine = new string(' ', _config.RecipeListWidth);
        _terminalService.WriteAt(_menuPosition.X + 2, y, clearLine);

        // Selection indicator (►)
        if (isSelected)
        {
            _terminalService.WriteColoredAt(_menuPosition.X + 2, y, "►", _config.SelectionColor);
        }

        // Craftability indicator [x] or [ ]
        _terminalService.WriteColoredAt(_menuPosition.X + 4, y, indicator, indicatorColor);

        // Recipe name
        var nameColor = isSelected ? _config.SelectionColor : ConsoleColor.White;
        _terminalService.WriteColoredAt(_menuPosition.X + 8, y, entryText, nameColor);

        // Material count summary
        var materialSummary = FormatMaterialSummary(recipe.Materials);
        var summaryX = _menuPosition.X + _config.RecipeListWidth - materialSummary.Length - 2;
        _terminalService.WriteColoredAt(summaryX, y, materialSummary, ConsoleColor.DarkGray);
    }

    /// <summary>
    /// Renders the material panel for a selected recipe.
    /// </summary>
    /// <param name="recipe">The selected recipe.</param>
    private void RenderMaterialPanel(StationRecipeDisplayDto recipe)
    {
        var panelX = _menuPosition.X + _config.RecipeListWidth + 2;
        var panelY = _menuPosition.Y + 4;

        _materialDisplay.SetPosition(panelX, panelY);
        _materialDisplay.RenderMaterials(recipe.Materials, _playerResources);
    }

    /// <summary>
    /// Renders the control hints at the bottom of the menu.
    /// </summary>
    private void RenderControls()
    {
        var controlsY = _menuPosition.Y + _config.MenuHeight - 2;
        var width = _config.MenuWidth;

        // Bottom separator
        var separator = $"├{new string('─', width - 2)}┤";
        _terminalService.WriteAt(_menuPosition.X, controlsY - 1, separator);

        // Controls hint
        var controls = "│  [↑/↓] Navigate   [C] Craft   [Esc] Close";
        var padding = width - controls.Length - 1;
        _terminalService.WriteAt(_menuPosition.X, controlsY, controls + new string(' ', padding) + "│");

        // Bottom border
        var bottomBorder = $"└{new string('─', width - 2)}┘";
        _terminalService.WriteAt(_menuPosition.X, controlsY + 1, bottomBorder);
    }

    /// <summary>
    /// Refreshes the recipe list and material panel after selection change.
    /// </summary>
    private void RefreshRecipeDisplay()
    {
        RenderRecipeList();

        if (_recipes.Count > 0)
        {
            RenderMaterialPanel(_recipes[_selectedIndex]);
        }
    }

    /// <summary>
    /// Formats a material count summary.
    /// </summary>
    /// <param name="materials">The materials to summarize.</param>
    /// <returns>A formatted string like "[3 mat]".</returns>
    private static string FormatMaterialSummary(IReadOnlyList<MaterialRequirementDto> materials)
    {
        var count = materials.Count(m => !m.IsOptional);
        return $"[{count} mat]";
    }

    /// <summary>
    /// Handles navigate up input.
    /// </summary>
    /// <returns>True if handled.</returns>
    private bool HandleNavigateUp()
    {
        SelectPrevious();
        return true;
    }

    /// <summary>
    /// Handles navigate down input.
    /// </summary>
    /// <returns>True if handled.</returns>
    private bool HandleNavigateDown()
    {
        SelectNext();
        return true;
    }

    /// <summary>
    /// Handles craft action input.
    /// </summary>
    /// <returns>True if handled.</returns>
    private bool HandleCraftAction()
    {
        var recipe = GetSelectedRecipe();
        if (recipe == null)
        {
            return false;
        }

        // Check if craftable
        if (!recipe.IsCraftable)
        {
            ShowMessage("Insufficient materials!", ConsoleColor.Red);
            _logger.LogWarning("Cannot craft {RecipeName} - insufficient materials", recipe.Name);
            return true;
        }

        // Raise craft event
        _logger.LogInformation("Craft requested: {RecipeName}", recipe.Name);
        CraftRequested?.Invoke(recipe.RecipeId);

        return true;
    }

    /// <summary>
    /// Handles close menu input.
    /// </summary>
    /// <returns>True if handled.</returns>
    private bool HandleClose()
    {
        Hide();
        return true;
    }

    /// <summary>
    /// Shows a temporary message in the menu.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="color">The message color.</param>
    private void ShowMessage(string message, ConsoleColor color)
    {
        var messageY = _menuPosition.Y + _config.MenuHeight - 4;
        var clearLine = new string(' ', _config.MenuWidth - 4);
        _terminalService.WriteAt(_menuPosition.X + 2, messageY, clearLine);
        _terminalService.WriteColoredAt(_menuPosition.X + 2, messageY, message, color);
    }

    #endregion
}
