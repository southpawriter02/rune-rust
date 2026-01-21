// ═══════════════════════════════════════════════════════════════════════════════
// MaterialAvailabilityDisplay.cs
// Displays material requirements with availability indicators.
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
/// Displays material requirements with availability indicators.
/// </summary>
/// <remarks>
/// <para>Shows required materials compared to owned quantities, highlighting
/// materials the player has insufficient quantities of.</para>
/// <para>Display format:</para>
/// <code>
/// YOUR MATERIALS
/// ──────────────
/// 
/// Iron Ore       24/2  [x]
/// Mithril Ore    0/5   [ ]
/// </code>
/// </remarks>
/// <example>
/// <code>
/// var display = new MaterialAvailabilityDisplay(renderer, terminalService, config);
/// display.SetPosition(50, 5);
/// display.RenderMaterials(materials, playerResources);
/// </code>
/// </example>
public class MaterialAvailabilityDisplay
{
    private readonly ResourceStackRenderer _resourceRenderer;
    private readonly ITerminalService _terminalService;
    private readonly CraftingStationConfig _config;
    private readonly ILogger<MaterialAvailabilityDisplay> _logger;

    private (int X, int Y) _position;

    /// <summary>
    /// Creates a new instance of the MaterialAvailabilityDisplay component.
    /// </summary>
    /// <param name="resourceRenderer">The resource stack renderer for formatting.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when required dependencies are null.
    /// </exception>
    public MaterialAvailabilityDisplay(
        ResourceStackRenderer resourceRenderer,
        ITerminalService terminalService,
        CraftingStationConfig? config = null,
        ILogger<MaterialAvailabilityDisplay>? logger = null)
    {
        _resourceRenderer = resourceRenderer ?? throw new ArgumentNullException(nameof(resourceRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? CraftingStationConfig.CreateDefault();
        _logger = logger ?? NullLogger<MaterialAvailabilityDisplay>.Instance;

        _logger.LogDebug(
            "MaterialAvailabilityDisplay initialized with {PanelWidth} width",
            _config.MaterialPanelWidth);
    }

    /// <summary>
    /// Sets the display position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _position = (x, y);

        _logger.LogDebug("Position set to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Renders material requirements with availability indicators.
    /// </summary>
    /// <param name="materials">The required materials.</param>
    /// <param name="playerResources">The player's current resources.</param>
    /// <remarks>
    /// <para>Each material is rendered with:</para>
    /// <list type="bullet">
    ///   <item><description>Material name</description></item>
    ///   <item><description>Owned/Required quantity display</description></item>
    ///   <item><description>Availability indicator ([x] or [ ])</description></item>
    ///   <item><description>Optional marker if applicable</description></item>
    /// </list>
    /// </remarks>
    public void RenderMaterials(
        IEnumerable<MaterialRequirementDto> materials,
        IEnumerable<ResourceStackDisplayDto> playerResources)
    {
        var materialList = materials.ToList();
        var resourceDict = playerResources.ToDictionary(r => r.ResourceId, r => r.Quantity);

        // Clear material panel
        ClearPanel();

        // Header
        _terminalService.WriteAt(_position.X, _position.Y, "YOUR MATERIALS");
        _terminalService.WriteAt(_position.X, _position.Y + 1, new string('─', 14));

        // Render each material
        var y = _position.Y + 3;
        var displayCount = Math.Min(materialList.Count, _config.MaxMaterialsVisible);

        for (var i = 0; i < displayCount; i++)
        {
            var material = materialList[i];
            var owned = resourceDict.GetValueOrDefault(material.MaterialId, 0);
            RenderMaterialRow(material, owned, y + i);
        }

        // Overflow indicator
        if (materialList.Count > _config.MaxMaterialsVisible)
        {
            var overflowCount = materialList.Count - displayCount;
            _terminalService.WriteColoredAt(
                _position.X,
                y + displayCount,
                $"...+{overflowCount} more",
                ConsoleColor.DarkGray);
        }

        _logger.LogDebug(
            "Rendered {Count} material requirements",
            materialList.Count);
    }

    /// <summary>
    /// Gets the availability indicator for a material.
    /// </summary>
    /// <param name="material">The material requirement.</param>
    /// <param name="ownedQuantity">The quantity owned by the player.</param>
    /// <returns>The availability indicator string ("[x]" or "[ ]").</returns>
    /// <remarks>
    /// <para>Returns "[x]" if owned is greater than or equal to required.</para>
    /// <para>Returns "[ ]" if owned is less than required.</para>
    /// </remarks>
    public string GetAvailabilityIndicator(MaterialRequirementDto material, int ownedQuantity)
    {
        if (ownedQuantity >= material.RequiredQuantity)
        {
            return "[x]";
        }

        return "[ ]";
    }

    /// <summary>
    /// Highlights a missing material visually.
    /// </summary>
    /// <param name="material">The material to highlight.</param>
    /// <remarks>
    /// Provides visual feedback for missing materials, such as flashing or color changes.
    /// </remarks>
    public void HighlightMissing(MaterialRequirementDto material)
    {
        // Visual feedback for missing material
        // This could flash or animate the material row
        _logger.LogDebug(
            "Highlighted missing material: {MaterialId}",
            material.MaterialId);
    }

    /// <summary>
    /// Checks if a recipe is craftable with current resources.
    /// </summary>
    /// <param name="materials">The recipe's material requirements.</param>
    /// <param name="playerResources">The player's current resources.</param>
    /// <returns>True if all required (non-optional) materials are available.</returns>
    /// <remarks>
    /// Optional materials are not considered when determining craftability.
    /// </remarks>
    public bool IsCraftable(
        IEnumerable<MaterialRequirementDto> materials,
        IEnumerable<ResourceStackDisplayDto> playerResources)
    {
        var resourceDict = playerResources.ToDictionary(r => r.ResourceId, r => r.Quantity);

        var isCraftable = materials
            .Where(m => !m.IsOptional)
            .All(m => resourceDict.GetValueOrDefault(m.MaterialId, 0) >= m.RequiredQuantity);

        _logger.LogDebug(
            "Craftability check: {IsCraftable}",
            isCraftable);

        return isCraftable;
    }

    #region Private Methods

    /// <summary>
    /// Clears the material panel area.
    /// </summary>
    private void ClearPanel()
    {
        var blankLine = new string(' ', _config.MaterialPanelWidth);
        for (var y = 0; y < _config.MaxMaterialsVisible + 4; y++)
        {
            _terminalService.WriteAt(_position.X, _position.Y + y, blankLine);
        }
    }

    /// <summary>
    /// Renders a single material row with availability.
    /// </summary>
    /// <param name="material">The material requirement.</param>
    /// <param name="ownedQuantity">The quantity owned by the player.</param>
    /// <param name="y">The Y coordinate for rendering.</param>
    private void RenderMaterialRow(MaterialRequirementDto material, int ownedQuantity, int y)
    {
        // Determine if sufficient
        var isSufficient = ownedQuantity >= material.RequiredQuantity;
        var indicator = GetAvailabilityIndicator(material, ownedQuantity);

        // Material name (truncate if needed)
        var maxNameWidth = _config.MaterialPanelWidth - 16; // Leave room for quantity + indicator
        var name = material.MaterialName;
        if (name.Length > maxNameWidth)
        {
            name = name.Substring(0, maxNameWidth - 2) + "..";
        }

        // Quantity display: "owned/required"
        var quantityText = $"{ownedQuantity}/{material.RequiredQuantity}";

        // Color based on sufficiency
        var nameColor = isSufficient ? ConsoleColor.White : _config.MissingMaterialColor;
        var indicatorColor = isSufficient ? _config.SufficientMaterialColor : _config.MissingMaterialColor;

        // Render row components
        _terminalService.WriteColoredAt(_position.X, y, name, nameColor);

        // Quantity right-aligned
        var quantityX = _position.X + maxNameWidth + 2;
        _terminalService.WriteAt(quantityX, y, quantityText);

        // Indicator
        var indicatorX = _position.X + _config.MaterialPanelWidth - 4;
        _terminalService.WriteColoredAt(indicatorX, y, indicator, indicatorColor);

        // Optional indicator
        if (material.IsOptional)
        {
            _terminalService.WriteColoredAt(
                _position.X + _config.MaterialPanelWidth - 9,
                y,
                "(opt)",
                ConsoleColor.DarkGray);
        }
    }

    #endregion
}
