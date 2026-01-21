// ═══════════════════════════════════════════════════════════════════════════════
// RecipeDetails.cs
// Displays detailed recipe information including materials and output.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays detailed recipe information including materials and output.
/// </summary>
/// <remarks>
/// <para>Shows full recipe details with material requirements, owned quantities,
/// expected output item, and quality range based on player skill.</para>
/// <para>Display format:</para>
/// <code>
/// ┌─────────────────────────────────────────────────────────────────────┐
/// │ STEEL BLADE                                      Quality: ★★★       │
/// │ A finely crafted blade of hardened steel                            │
/// ├─────────────────────────────────────────────────────────────────────┤
/// │ Required Materials:           You Have:                             │
/// │ |-- Iron Ore x2               24 [x]                                │
/// │ +-- Coal x1                   8 [x]                                 │
/// └─────────────────────────────────────────────────────────────────────┘
/// </code>
/// </remarks>
public class RecipeDetails
{
    private readonly QualityTierRenderer _qualityRenderer;
    private readonly ITerminalService _terminalService;
    private readonly RecipeBrowserConfig _config;
    private readonly ILogger<RecipeDetails> _logger;

    private RecipeDetailsDisplayDto? _currentRecipe;
    private (int X, int Y) _detailsPosition;

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Gets whether the details panel is visible.</summary>
    public bool IsVisible { get; private set; }

    /// <summary>Gets the currently displayed recipe.</summary>
    public RecipeDetailsDisplayDto? CurrentRecipe => _currentRecipe;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the RecipeDetails component.
    /// </summary>
    /// <param name="qualityRenderer">The quality tier renderer.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
    public RecipeDetails(
        QualityTierRenderer qualityRenderer,
        ITerminalService terminalService,
        RecipeBrowserConfig? config = null,
        ILogger<RecipeDetails>? logger = null)
    {
        _qualityRenderer = qualityRenderer ?? throw new ArgumentNullException(nameof(qualityRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? RecipeBrowserConfig.CreateDefault();
        _logger = logger ?? NullLogger<RecipeDetails>.Instance;

        _logger.LogDebug(
            "RecipeDetails initialized with {Width}x{Height} dimensions",
            _config.DetailsWidth,
            _config.DetailsHeight);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the details panel position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _detailsPosition = (x, y);
        _logger.LogDebug("Position set to ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Renders the recipe details panel.
    /// </summary>
    /// <param name="recipe">The recipe to display.</param>
    public void RenderRecipe(RecipeDetailsDisplayDto recipe)
    {
        _currentRecipe = recipe;
        IsVisible = true;

        // Clear details area
        ClearDetails();

        // Render components
        RenderRecipeHeader(recipe);
        ShowMaterials(recipe.Materials);
        ShowExpectedOutput(recipe.OutputItemName, recipe.QualityRange);
        RenderCraftHint(recipe.IsCraftable);

        _logger.LogDebug("Rendered details for recipe: {RecipeName}", recipe.Name);
    }

    /// <summary>
    /// Shows the material requirements for a recipe.
    /// </summary>
    /// <param name="materials">The material requirements.</param>
    public void ShowMaterials(IEnumerable<MaterialRequirementDto> materials)
    {
        var materialsY = _detailsPosition.Y + 5;

        // Materials header
        _terminalService.WriteAt(_detailsPosition.X + 2, materialsY, "Required Materials:");
        _terminalService.WriteAt(_detailsPosition.X + 32, materialsY, "You Have:");

        // Render each material
        var y = materialsY + 1;
        foreach (var material in materials)
        {
            // Material requirement with tree structure (|-- for required, +-- for optional)
            var prefix = material.IsOptional ? "+-- " : "|-- ";
            var materialLine = $"{prefix}{material.MaterialName} x{material.RequiredQuantity}";

            _terminalService.WriteAt(_detailsPosition.X + 2, y, materialLine);

            // Owned quantity with availability indicator
            var owned = material.OwnedQuantity;
            var isSufficient = owned >= material.RequiredQuantity;
            var ownedText = owned.ToString();
            var indicator = isSufficient ? " [x]" : " [ ]";
            var indicatorColor = isSufficient ? _config.SufficientMaterialColor : _config.InsufficientMaterialColor;

            _terminalService.WriteAt(_detailsPosition.X + 32, y, ownedText);
            _terminalService.WriteColoredAt(_detailsPosition.X + 36, y, indicator, indicatorColor);

            y++;
        }
    }

    /// <summary>
    /// Shows the expected output item and quality range.
    /// </summary>
    /// <param name="itemName">The output item name.</param>
    /// <param name="qualityRange">The quality range description.</param>
    public void ShowExpectedOutput(string itemName, string qualityRange)
    {
        var outputY = _detailsPosition.Y + _config.DetailsHeight - 5;

        _terminalService.WriteAt(_detailsPosition.X + 2, outputY, $"Output: {itemName}");
        _terminalService.WriteAt(_detailsPosition.X + 2, outputY + 1, $"Quality: {qualityRange}");
    }

    /// <summary>
    /// Shows the quality result after crafting.
    /// </summary>
    /// <param name="quality">The crafted item's quality tier.</param>
    public void ShowQualityResult(ItemQuality quality)
    {
        var qualityY = _detailsPosition.Y + 1;
        var qualityX = _detailsPosition.X + _config.DetailsWidth - 15;

        var stars = _qualityRenderer.GetQualityStars(quality);
        var color = _qualityRenderer.GetQualityColor(quality);

        _terminalService.WriteColoredAt(qualityX, qualityY, $"Quality: {stars}", color);

        _logger.LogDebug("Displayed quality result: {Quality}", quality);
    }

    /// <summary>
    /// Hides the details panel.
    /// </summary>
    public void Hide()
    {
        if (!IsVisible) return;

        ClearDetails();
        IsVisible = false;
        _currentRecipe = null;

        _logger.LogDebug("Recipe details hidden");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods
    // ═══════════════════════════════════════════════════════════════════════════

    #region Private Methods

    private void ClearDetails()
    {
        var blankLine = new string(' ', _config.DetailsWidth);
        for (var y = 0; y < _config.DetailsHeight; y++)
        {
            _terminalService.WriteAt(_detailsPosition.X, _detailsPosition.Y + y, blankLine);
        }
    }

    private void RenderRecipeHeader(RecipeDetailsDisplayDto recipe)
    {
        var headerY = _detailsPosition.Y;
        var width = _config.DetailsWidth;

        // Top border
        var topBorder = $"┌{new string('─', width - 2)}┐";
        _terminalService.WriteAt(_detailsPosition.X, headerY, topBorder);

        // Recipe name with quality indicator
        var qualityStars = _qualityRenderer.GetQualityStars(recipe.ExpectedQuality);
        var qualityColor = _qualityRenderer.GetQualityColor(recipe.ExpectedQuality);
        var titleLine = $"│ {recipe.Name.ToUpperInvariant()}";
        var qualityPadding = width - titleLine.Length - qualityStars.Length - 2;
        titleLine += new string(' ', Math.Max(0, qualityPadding)) + qualityStars + " │";

        _terminalService.WriteAt(_detailsPosition.X, headerY + 1, titleLine);

        // Color the quality stars
        _terminalService.WriteColoredAt(
            _detailsPosition.X + width - qualityStars.Length - 2,
            headerY + 1,
            qualityStars,
            qualityColor);

        // Description
        var descLine = $"│ {recipe.Description}";
        var descPadding = width - descLine.Length - 1;
        descLine += new string(' ', Math.Max(0, descPadding)) + "│";
        _terminalService.WriteAt(_detailsPosition.X, headerY + 2, descLine);

        // Separator
        var separator = $"├{new string('─', width - 2)}┤";
        _terminalService.WriteAt(_detailsPosition.X, headerY + 3, separator);
    }

    private void RenderCraftHint(bool canCraft)
    {
        var hintY = _detailsPosition.Y + _config.DetailsHeight - 2;
        var width = _config.DetailsWidth;

        // Bottom separator
        var separator = $"├{new string('─', width - 2)}┤";
        _terminalService.WriteAt(_detailsPosition.X, hintY - 1, separator);

        // Craft action hint
        var hintText = canCraft ? "[C]raft" : "Insufficient materials";
        var hintColor = canCraft ? _config.CraftableColor : _config.NotCraftableColor;

        var hintLine = $"│  {hintText}";
        var padding = width - hintLine.Length - 1;
        _terminalService.WriteAt(_detailsPosition.X, hintY, hintLine + new string(' ', Math.Max(0, padding)) + "│");
        _terminalService.WriteColoredAt(_detailsPosition.X + 3, hintY, hintText, hintColor);

        // Bottom border
        var bottomBorder = $"└{new string('─', width - 2)}┘";
        _terminalService.WriteAt(_detailsPosition.X, hintY + 1, bottomBorder);
    }

    #endregion
}
