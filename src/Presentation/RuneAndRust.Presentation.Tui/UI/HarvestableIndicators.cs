// ═══════════════════════════════════════════════════════════════════════════════
// HarvestableIndicators.cs
// Displays harvestable resource indicators in the room view.
// Version: 0.13.3d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays harvestable resource indicators in the room view.
/// </summary>
/// <remarks>
/// <para>Shows available harvestable nodes with resource type icons, quantities,
/// difficulty class, and gather action prompts.</para>
/// <para>Display format:</para>
/// <code>
/// Harvestable Resources:
/// ┌──────────────────────────────────────────────────────────┐
/// │  ► [H] Healing Herbs (x3)     DC 10  [G]ather            │
/// │    [M] Cave Mushrooms (x2)    DC 12  [G]ather            │
/// └──────────────────────────────────────────────────────────┘
/// </code>
/// </remarks>
public class HarvestableIndicators
{
    private readonly ResourceStackRenderer _stackRenderer;
    private readonly ITerminalService _terminalService;
    private readonly GatheringDisplayConfig _config;
    private readonly ILogger<HarvestableIndicators> _logger;

    private IReadOnlyList<HarvestableNodeDisplayDto> _harvestables = Array.Empty<HarvestableNodeDisplayDto>();
    private int _selectedIndex;
    private (int X, int Y) _indicatorPosition;

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets whether indicators are currently visible.</summary>
    public bool IsVisible { get; private set; }

    /// <summary>Gets the currently selected node index.</summary>
    public int SelectedIndex => _selectedIndex;

    /// <summary>Gets the count of harvestable nodes.</summary>
    public int NodeCount => _harvestables.Count;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the HarvestableIndicators component.
    /// </summary>
    /// <param name="stackRenderer">The resource stack renderer from v0.13.3a.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for gathering display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
    public HarvestableIndicators(
        ResourceStackRenderer stackRenderer,
        ITerminalService terminalService,
        GatheringDisplayConfig? config = null,
        ILogger<HarvestableIndicators>? logger = null)
    {
        _stackRenderer = stackRenderer ?? throw new ArgumentNullException(nameof(stackRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? GatheringDisplayConfig.CreateDefault();
        _logger = logger ?? NullLogger<HarvestableIndicators>.Instance;

        _logger.LogDebug(
            "HarvestableIndicators initialized with {Width}x{Height} dimensions",
            _config.IndicatorWidth,
            _config.IndicatorHeight);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the indicator display position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _indicatorPosition = (x, y);
        _logger.LogDebug("Position set to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Renders the harvestable indicators.
    /// </summary>
    /// <param name="harvestables">The harvestable nodes to display.</param>
    public void RenderHarvestables(IEnumerable<HarvestableNodeDisplayDto> harvestables)
    {
        // Filter to available nodes only
        _harvestables = harvestables
            .Where(h => h.IsAvailable)
            .ToList();

        if (_harvestables.Count == 0)
        {
            IsVisible = false;
            _logger.LogWarning("No harvestables available in room");
            return;
        }

        IsVisible = true;

        // Clear indicator area
        ClearIndicators();

        // Render header
        RenderHeader();

        // Render node entries
        RenderNodeEntries();

        _logger.LogInformation("Rendered {Count} harvestable indicators", _harvestables.Count);
    }

    /// <summary>
    /// Gets the icon for a resource type.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <returns>The type icon string.</returns>
    public string GetNodeIcon(ResourceCategory resourceType)
    {
        return _stackRenderer.GetTypeIcon(resourceType);
    }

    /// <summary>
    /// Shows the gather prompt for a node.
    /// </summary>
    /// <param name="node">The harvestable node.</param>
    public void ShowGatherPrompt(HarvestableNodeDisplayDto node)
    {
        var promptY = _indicatorPosition.Y + _config.IndicatorHeight - 2;
        var promptText = $"Press [G] to gather {node.Name}";

        _terminalService.WriteColoredAt(
            _indicatorPosition.X + 2,
            promptY,
            promptText,
            _config.PromptColor);

        _logger.LogDebug("Showed gather prompt for node: {NodeName}", node.Name);
    }

    /// <summary>
    /// Highlights the selected node.
    /// </summary>
    /// <param name="index">The node index to highlight.</param>
    public void HighlightSelected(int index)
    {
        if (index < 0 || index >= _harvestables.Count)
        {
            return;
        }

        _selectedIndex = index;
        RenderNodeEntries();

        _logger.LogDebug("Highlighted node at index {Index}", index);
    }

    /// <summary>
    /// Gets the currently selected node.
    /// </summary>
    /// <returns>The selected node, or null if none.</returns>
    public HarvestableNodeDisplayDto? GetSelectedNode()
    {
        if (_harvestables.Count == 0 || _selectedIndex < 0 || _selectedIndex >= _harvestables.Count)
        {
            return null;
        }

        return _harvestables[_selectedIndex];
    }

    /// <summary>
    /// Selects the next harvestable node.
    /// </summary>
    public void SelectNext()
    {
        if (_harvestables.Count == 0) return;

        _selectedIndex = (_selectedIndex + 1) % _harvestables.Count;
        RenderNodeEntries();
    }

    /// <summary>
    /// Selects the previous harvestable node.
    /// </summary>
    public void SelectPrevious()
    {
        if (_harvestables.Count == 0) return;

        _selectedIndex = (_selectedIndex - 1 + _harvestables.Count) % _harvestables.Count;
        RenderNodeEntries();
    }

    /// <summary>
    /// Hides the indicators.
    /// </summary>
    public void Hide()
    {
        if (!IsVisible) return;

        ClearIndicators();
        IsVisible = false;
        _harvestables = Array.Empty<HarvestableNodeDisplayDto>();
        _selectedIndex = 0;

        _logger.LogDebug("Harvestable indicators hidden");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods
    // ═══════════════════════════════════════════════════════════════════════════

    #region Private Methods

    private void ClearIndicators()
    {
        var blankLine = new string(' ', _config.IndicatorWidth);
        for (var y = 0; y < _config.IndicatorHeight; y++)
        {
            _terminalService.WriteAt(_indicatorPosition.X, _indicatorPosition.Y + y, blankLine);
        }
    }

    private void RenderHeader()
    {
        var headerY = _indicatorPosition.Y;

        // Section label
        _terminalService.WriteAt(_indicatorPosition.X + 2, headerY, "Harvestable Resources:");

        // Box top border
        var boxY = headerY + 1;
        var boxWidth = _config.IndicatorWidth - 4;
        var topBorder = $"┌{new string('─', boxWidth - 2)}┐";
        _terminalService.WriteAt(_indicatorPosition.X + 2, boxY, topBorder);
    }

    private void RenderNodeEntries()
    {
        var entryY = _indicatorPosition.Y + 2;
        var boxWidth = _config.IndicatorWidth - 4;

        for (var i = 0; i < _harvestables.Count && i < _config.MaxVisibleNodes; i++)
        {
            var node = _harvestables[i];
            var isSelected = i == _selectedIndex;

            RenderNodeEntry(node, entryY + i, boxWidth, isSelected);
        }

        // Box bottom border
        var bottomY = entryY + Math.Min(_harvestables.Count, _config.MaxVisibleNodes);
        var bottomBorder = $"└{new string('─', boxWidth - 2)}┘";
        _terminalService.WriteAt(_indicatorPosition.X + 2, bottomY, bottomBorder);
    }

    private void RenderNodeEntry(HarvestableNodeDisplayDto node, int y, int width, bool isSelected)
    {
        // Entry format: │  ► [H] Healing Herbs (x3)     DC 10  [G]ather  │
        var icon = GetNodeIcon(node.ResourceType);
        var iconColor = _stackRenderer.GetTypeColor(node.ResourceType);
        var nameWithQty = $"{node.Name} (x{node.Quantity})";
        var dcText = $"DC {node.DifficultyClass}";
        var gatherText = "[G]ather";

        // Build entry line
        var x = _indicatorPosition.X + 2;

        // Left border
        _terminalService.WriteAt(x, y, "│  ");

        // Selection indicator
        if (isSelected)
        {
            _terminalService.WriteColoredAt(x + 3, y, "►", _config.SelectionColor);
        }
        else
        {
            _terminalService.WriteAt(x + 3, y, " ");
        }

        // Icon with type color
        _terminalService.WriteColoredAt(x + 5, y, icon, iconColor);

        // Name and quantity
        var nameX = x + 9;
        var nameColor = isSelected ? _config.SelectionColor : ConsoleColor.White;
        _terminalService.WriteColoredAt(nameX, y, nameWithQty, nameColor);

        // DC (right-aligned section)
        var dcX = x + width - 20;
        _terminalService.WriteColoredAt(dcX, y, dcText, _config.DifficultyColor);

        // Gather prompt
        var gatherX = x + width - 10;
        _terminalService.WriteColoredAt(gatherX, y, gatherText, _config.PromptColor);

        // Right border
        var rightBorderX = x + width - 1;
        _terminalService.WriteAt(rightBorderX, y, "│");
    }

    #endregion
}
