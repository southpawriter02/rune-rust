using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the Specialization Tree Grid using Spectre.Console (v0.4.1d).
/// Displays nodes by tier with visual selection and status indicators.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
public class SpecializationGridRenderer : ISpecializationGridRenderer
{
    private readonly IThemeService _theme;
    private readonly ILogger<SpecializationGridRenderer> _logger;

    public SpecializationGridRenderer(
        IThemeService theme,
        ILogger<SpecializationGridRenderer> logger)
    {
        _theme = theme;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Render(SpecializationGridViewModel vm)
    {
        AnsiConsole.Clear();

        // Header - Spec name and PP balance
        RenderHeader(vm);

        // Main Grid - Nodes by tier
        RenderGrid(vm);

        // Detail Panel - Selected node description
        RenderDetailPanel(vm);

        // Feedback - Purchase result
        RenderFeedback(vm);

        // Footer - Controls
        RenderFooter(vm);

        _logger.LogTrace("[Spec UI] Rendered. Selected: {Index}, Node: {NodeName}",
            vm.SelectedNodeIndex, vm.SelectedNode?.Name ?? "none");
    }

    private static void RenderHeader(SpecializationGridViewModel vm)
    {
        // Spec name with tab indicator for multi-spec (use parentheses to avoid Spectre markup interpretation)
        var tabIndicator = vm.TotalSpecCount > 1
            ? $" ({vm.CurrentSpecIndex + 1}/{vm.TotalSpecCount})"
            : "";

        AnsiConsole.Write(new Rule($"[bold gold1]{Markup.Escape(vm.SpecializationName).ToUpperInvariant()}{tabIndicator}[/]")
            .RuleStyle("gold1"));
        AnsiConsole.WriteLine();

        // Character and PP
        AnsiConsole.MarkupLine($"  {Markup.Escape(vm.CharacterName)}    " +
            $"Progression Points: [yellow bold]({vm.ProgressionPoints})[/]    " +
            $"Progress: [cyan]{vm.UnlockedCount}/{vm.TotalNodes}[/]");
        AnsiConsole.WriteLine();
    }

    private void RenderGrid(SpecializationGridViewModel vm)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[grey]TIER 1[/]").Width(18))
            .AddColumn(new TableColumn("[grey]TIER 2[/]").Width(18))
            .AddColumn(new TableColumn("[grey]TIER 3[/]").Width(18))
            .AddColumn(new TableColumn("[grey]CAPSTONE[/]").Width(18));

        // Find max nodes in any tier for row count
        var maxNodesPerTier = vm.NodesByTier.Count > 0
            ? vm.NodesByTier.Values.Max(n => n.Count)
            : 0;

        if (maxNodesPerTier == 0)
        {
            table.AddRow("[dim]No nodes[/]", "", "", "");
        }
        else
        {
            for (int row = 0; row < maxNodesPerTier; row++)
            {
                var cells = new string[4];

                for (int tier = 1; tier <= 4; tier++)
                {
                    if (vm.NodesByTier.TryGetValue(tier, out var tierNodes) && row < tierNodes.Count)
                    {
                        var node = tierNodes[row];
                        var isSelected = vm.SelectedNode?.NodeId == node.NodeId;
                        cells[tier - 1] = FormatNodeCell(node, isSelected);
                    }
                    else
                    {
                        cells[tier - 1] = "";
                    }
                }

                table.AddRow(cells);
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static string FormatNodeCell(NodeViewModel node, bool isSelected)
    {
        var indicator = isSelected ? "> " : "  ";
        var color = node.Status switch
        {
            NodeStatus.Unlocked => "green",
            NodeStatus.Available => "cyan",
            NodeStatus.Affordable => "yellow",
            NodeStatus.Locked => "red",
            _ => "grey"
        };

        var icon = node.Status switch
        {
            NodeStatus.Unlocked => "*",
            NodeStatus.Available => "o",
            NodeStatus.Affordable => "~",
            NodeStatus.Locked => "x",
            _ => "?"
        };

        var nameDisplay = node.Name.Length > 12
            ? node.Name[..12] + "..."
            : node.Name;

        var escapedName = Markup.Escape(nameDisplay);

        return isSelected
            ? $"[bold white]{indicator}{icon} {escapedName}[/]"
            : $"[{color}]{indicator}{icon} {escapedName}[/]";
    }

    private static void RenderDetailPanel(SpecializationGridViewModel vm)
    {
        if (vm.SelectedNode == null)
        {
            var emptyPanel = new Panel(new Markup("[dim]No node selected[/]"))
                .Header("[bold white]Details[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Grey)
                .Expand();
            AnsiConsole.Write(emptyPanel);
            AnsiConsole.WriteLine();
            return;
        }

        var node = vm.SelectedNode;

        var capstoneIndicator = node.IsCapstone ? " [gold1]CAPSTONE[/]" : "";
        var header = $"[bold white]{Markup.Escape(node.Name)} (Tier {node.Tier}){capstoneIndicator}[/]";

        var panel = new Panel(
            new Markup($"{Markup.Escape(node.Description)}\n\n" +
                       $"Cost: [yellow]{node.CostDisplay}[/]  |  " +
                       $"Status: {node.StatusMarkup}"))
            .Header(header)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Expand();

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static void RenderFeedback(SpecializationGridViewModel vm)
    {
        if (string.IsNullOrEmpty(vm.FeedbackMessage)) return;

        var color = vm.FeedbackIsSuccess ? "green" : "red";
        AnsiConsole.MarkupLine($"  [{color}]>> {Markup.Escape(vm.FeedbackMessage)}[/]");
        AnsiConsole.WriteLine();
    }

    private static void RenderFooter(SpecializationGridViewModel vm)
    {
        var tabHint = vm.TotalSpecCount > 1 ? "Tab: Switch Spec  |  " : "";
        AnsiConsole.MarkupLine($"[dim]Up/Down: Navigate  |  Left/Right: Tier  |  Enter: Inscribe  |  {tabHint}Esc: Return[/]");
    }
}
