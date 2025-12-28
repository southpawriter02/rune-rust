using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the Saga progression UI ("The Shrine of Echoes") using Spectre.Console (v0.4.0c).
/// Displays Legend progress, PP balance, and attribute upgrade options with color-coding.
/// </summary>
/// <remarks>See: v0.4.0c (The Shrine) for Saga UI implementation.</remarks>
public class SagaScreenRenderer : ISagaScreenRenderer
{
    private readonly IThemeService _theme;
    private readonly ILogger<SagaScreenRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaScreenRenderer"/> class.
    /// </summary>
    /// <param name="theme">The theme service for color configuration.</param>
    /// <param name="logger">The logger for traceability.</param>
    public SagaScreenRenderer(IThemeService theme, ILogger<SagaScreenRenderer> logger)
    {
        _theme = theme;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Render(SagaViewModel vm)
    {
        AnsiConsole.Clear();

        // Header - The Shrine of Echoes
        RenderHeader();

        // Stats Row - Level and Legend progress
        RenderStatsRow(vm);

        // Attribute Table - Selection and upgrade options
        RenderAttributeTable(vm);

        // Footer - Control hints
        RenderFooter();

        _logger.LogTrace("[Saga UI] Rendered. Selected: {Index}", vm.SelectedIndex);
    }

    /// <summary>
    /// Renders the header with the Shrine title.
    /// </summary>
    private static void RenderHeader()
    {
        AnsiConsole.Write(new Rule("[bold gold1]\u2726 THE SHRINE OF ECHOES \u2726[/]")
            .RuleStyle("gold1"));
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the stats row with Level, Legend progress bar, and PP balance.
    /// </summary>
    private static void RenderStatsRow(SagaViewModel vm)
    {
        // Legend progress bar
        string legendBar;
        string legendProgress;

        if (vm.LegendForNextLevel < 0)
        {
            // At max level
            legendBar = StatusWidget.RenderBar(1, 1, 20);
            legendProgress = "[grey]MAX LEVEL[/]";
        }
        else
        {
            legendBar = StatusWidget.RenderBar(vm.CurrentLegend, vm.LegendForNextLevel, 20);
            var pct = vm.LegendForNextLevel > 0
                ? (int)((double)vm.CurrentLegend / vm.LegendForNextLevel * 100)
                : 100;
            legendProgress = $"{vm.CurrentLegend}/{vm.LegendForNextLevel} ({pct}%)";
        }

        AnsiConsole.MarkupLine($"  Level: [bold white]{vm.Level}[/]    Legend: [cyan]{legendBar}[/] {legendProgress}");
        AnsiConsole.MarkupLine($"                      Progression Points: [yellow bold][{vm.ProgressionPoints}][/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the attribute table with selection indicator and status colors.
    /// </summary>
    private void RenderAttributeTable(SagaViewModel vm)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[grey]ATTRIBUTE[/]").Width(20))
            .AddColumn(new TableColumn("[grey]VALUE[/]").Width(10).Centered())
            .AddColumn(new TableColumn("[grey]COST[/]").Width(10).Centered())
            .AddColumn(new TableColumn("[grey]STATUS[/]").Width(15));

        for (int i = 0; i < vm.Attributes.Count; i++)
        {
            var attr = vm.Attributes[i];
            var isSelected = i == vm.SelectedIndex;

            // Selection indicator
            var indicator = isSelected ? ">" : " ";

            // Attribute name with selection highlighting
            var attrName = attr.Type.ToString().ToUpperInvariant();
            var nameMarkup = isSelected
                ? $"[bold white]{indicator} {attrName}[/]"
                : $"  {attrName}";

            // Value display
            var valueMarkup = attr.CurrentValue.ToString();

            // Cost display
            var costMarkup = attr.Status == AttributeStatus.Maxed
                ? "[grey]-[/]"
                : $"{attr.UpgradeCost} PP";

            // Status with color coding
            var statusMarkup = attr.Status switch
            {
                AttributeStatus.Upgrade => "[green][UPGRADE][/]",
                AttributeStatus.Locked => "[red][LOCKED][/]",
                AttributeStatus.Maxed => "[grey][MAXED][/]",
                _ => ""
            };

            table.AddRow(nameMarkup, valueMarkup, costMarkup, statusMarkup);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the footer with control hints.
    /// </summary>
    private static void RenderFooter()
    {
        AnsiConsole.MarkupLine("[dim]\u2191\u2193 Navigate  \u2502  Enter: Purchase  \u2502  Esc: Return[/]");
    }
}
