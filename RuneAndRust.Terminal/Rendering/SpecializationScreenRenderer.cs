using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Spectre.Console implementation of the Specialization screen renderer.
/// Displays a two-panel layout: specialization list (left) and tree detail (right).
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public class SpecializationScreenRenderer : ISpecializationScreenRenderer
{
    private readonly ILogger<SpecializationScreenRenderer> _logger;
    private readonly IThemeService _theme;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="theme">The theme service for color configuration.</param>
    public SpecializationScreenRenderer(
        ILogger<SpecializationScreenRenderer> logger,
        IThemeService theme)
    {
        _logger = logger;
        _theme = theme;
    }

    /// <inheritdoc/>
    public void Render(SpecializationViewModel viewModel)
    {
        _logger.LogDebug("[SpecUI] Rendering SpecializationScreen: ViewMode={Mode}, SelectedSpec={SpecIdx}, SelectedNode={NodeIdx}",
            viewModel.ViewMode, viewModel.SelectedSpecIndex, viewModel.SelectedNodeIndex);

        AnsiConsole.Clear();

        // Header
        RenderHeader(viewModel);

        // Two-column layout
        var layout = new Layout("Root")
            .SplitColumns(
                new Layout("SpecList").Size(32),
                new Layout("TreeDetail"));

        layout["SpecList"].Update(BuildSpecListPanel(viewModel));
        layout["TreeDetail"].Update(BuildTreeDetailPanel(viewModel));

        AnsiConsole.Write(layout);

        // Status message (if any)
        if (!string.IsNullOrEmpty(viewModel.StatusMessage))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(viewModel.StatusMessage)}[/]");
        }

        // Footer with controls
        RenderFooter(viewModel);

        _logger.LogTrace("[SpecUI] Render complete");
    }

    /// <summary>
    /// Renders the header with title and PP balance.
    /// </summary>
    private static void RenderHeader(SpecializationViewModel viewModel)
    {
        var headerPanel = new Panel(
            new Markup($"[bold cyan]THE TREE OF RUNES[/]\n" +
                       $"[dim]{Markup.Escape(viewModel.CharacterName)}[/] | " +
                       $"[green]PP: {viewModel.AvailableProgressionPoints}[/]"))
            .Border(BoxBorder.Double)
            .BorderColor(Color.Cyan1);

        AnsiConsole.Write(headerPanel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Builds the left panel showing available specializations.
    /// </summary>
    private static Panel BuildSpecListPanel(SpecializationViewModel viewModel)
    {
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("Spec");

        for (int i = 0; i < viewModel.Specializations.Count; i++)
        {
            var spec = viewModel.Specializations[i];
            var isSelected = i == viewModel.SelectedSpecIndex;
            var isFocused = viewModel.ViewMode == SpecializationViewMode.SpecList;

            var prefix = isSelected ? (isFocused ? "> " : "  ") : "  ";
            var statusIcon = spec.IsUnlocked ? "[green]#[/]" : (spec.CanUnlock ? "[yellow]o[/]" : "[red]x[/]");
            var progress = $"[dim]{spec.NodesUnlocked}/{spec.TotalNodes}[/]";

            var style = isSelected && isFocused ? "bold white on blue" : (isSelected ? "bold" : "");
            var specName = Markup.Escape(spec.Name);
            var line = string.IsNullOrEmpty(style)
                ? $"{prefix}{statusIcon} {specName} {progress}"
                : $"[{style}]{prefix}[/]{statusIcon} [{style}]{specName}[/] {progress}";

            table.AddRow(new Markup(line));
        }

        var borderColor = viewModel.ViewMode == SpecializationViewMode.SpecList ? Color.Yellow : Color.Grey;
        return new Panel(table)
            .Header("[bold]Specializations[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(borderColor);
    }

    /// <summary>
    /// Builds the right panel showing the node tree for the selected specialization.
    /// </summary>
    private static Panel BuildTreeDetailPanel(SpecializationViewModel viewModel)
    {
        if (viewModel.CurrentTree == null)
        {
            return new Panel(new Markup("[dim]Select a specialization to view its tree[/]"))
                .Header("[bold]Tree[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Grey);
        }

        var tree = viewModel.CurrentTree;
        var isFocused = viewModel.ViewMode == SpecializationViewMode.TreeDetail;

        // Group nodes by tier for visual layout
        var tiers = tree.Nodes
            .GroupBy(n => n.Tier)
            .OrderBy(g => g.Key)
            .ToList();

        var grid = new Grid().AddColumn().AddColumn().AddColumn();

        foreach (var tierGroup in tiers)
        {
            var tierNodes = tierGroup.OrderBy(n => n.PositionX).ToList();
            var tierLabel = tierGroup.Key == 4 ? "[bold magenta]CAPSTONE[/]" : $"[dim]Tier {tierGroup.Key}[/]";

            grid.AddRow(new Markup(tierLabel), new Text(""), new Text(""));

            foreach (var node in tierNodes)
            {
                var nodeIndex = tree.Nodes.ToList().IndexOf(node);
                var isSelected = nodeIndex == viewModel.SelectedNodeIndex;

                var statusColor = node.Status switch
                {
                    NodeStatus.Unlocked => "green",
                    NodeStatus.Available => "yellow",
                    NodeStatus.InsufficientPP => "darkorange",
                    NodeStatus.Locked => "red",
                    _ => "grey"
                };

                var statusIcon = node.Status switch
                {
                    NodeStatus.Unlocked => "*",
                    NodeStatus.Available => "o",
                    NodeStatus.InsufficientPP => "~",
                    NodeStatus.Locked => "x",
                    _ => "?"
                };

                var prefix = isSelected && isFocused ? ">" : " ";
                var costDisplay = node.Status == NodeStatus.Unlocked ? "" : $" ({node.CostPP} PP)";
                var style = isSelected && isFocused ? $"bold {statusColor} on grey23" : statusColor;

                var nodeName = Markup.Escape(node.NodeName);
                var nodeLine = $"{prefix} [{style}]{statusIcon} {nodeName}{costDisplay}[/]";
                var abilityLine = isSelected ? $"   [dim]{Markup.Escape(node.AbilityDescription)}[/]" : "";

                grid.AddRow(new Markup(nodeLine), new Text(""), new Text(""));
                if (!string.IsNullOrEmpty(abilityLine))
                {
                    grid.AddRow(new Markup(abilityLine), new Text(""), new Text(""));
                }
            }

            grid.AddEmptyRow();
        }

        var borderColor = isFocused ? Color.Yellow : Color.Grey;
        return new Panel(grid)
            .Header($"[bold]{Markup.Escape(tree.SpecializationName)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(borderColor);
    }

    /// <summary>
    /// Renders the footer with contextual control hints.
    /// </summary>
    private static void RenderFooter(SpecializationViewModel viewModel)
    {
        AnsiConsole.WriteLine();
        var controls = viewModel.ViewMode switch
        {
            SpecializationViewMode.SpecList =>
                "[dim]Up/Down[/] Navigate | [dim]Right[/] View Tree | [dim]Enter[/] Unlock Spec | [dim]Esc[/] Exit",
            SpecializationViewMode.TreeDetail =>
                "[dim]Up/Down[/] Navigate | [dim]Left[/] Back to List | [dim]Enter[/] Unlock Node | [dim]Esc[/] Exit",
            _ => "[dim]Esc[/] Exit"
        };
        AnsiConsole.MarkupLine(controls);
    }
}
