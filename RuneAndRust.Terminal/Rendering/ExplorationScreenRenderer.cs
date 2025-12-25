using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the persistent exploration HUD using Spectre.Console Layout (v0.3.5a).
/// Displays a three-pane interface: Header (stats), Body (room/minimap), Footer (turn/tips).
/// Updated with RoomRenderer integration in v0.3.5c.
/// Updated with context help tips in expanded footer in v0.3.9c.
/// Updated with IThemeService for accessibility themes in v0.3.14a.
/// </summary>
public class ExplorationScreenRenderer : IExplorationScreenRenderer
{
    private readonly ILogger<ExplorationScreenRenderer> _logger;
    private readonly IThemeService _theme;

    public ExplorationScreenRenderer(ILogger<ExplorationScreenRenderer> logger, IThemeService theme)
    {
        _logger = logger;
        _theme = theme;
    }

    /// <inheritdoc />
    public void Render(ExplorationViewModel vm)
    {
        _logger.LogTrace("[HUD] Rendering exploration screen. HP: {HP}/{MaxHP}, Room: {Room}",
            vm.CurrentHp, vm.MaxHp, vm.RoomName);

        // Calculate footer size based on tips (v0.3.9c)
        var hasTips = vm.ContextTips != null && vm.ContextTips.Count > 0;
        var footerSize = hasTips ? 3 : 1;

        // Build fresh layout each render (Layout is mutable, safer to rebuild)
        var rootLayout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("Body").SplitColumns(
                    new Layout("Main").Ratio(7),
                    new Layout("Sidebar").Ratio(3)
                ),
                new Layout("Footer").Size(footerSize)
            );

        // 1. Update Header (Status Bar)
        rootLayout["Header"].Update(CreateStatusBar(vm));

        // 2. Update Main Content (Room View - v0.3.5c, themed v0.3.14a)
        var roomPanel = RoomRenderer.Render(vm, _theme);
        rootLayout["Main"].Update(roomPanel);

        // 3. Update Sidebar (Minimap - v0.3.5b)
        var minimapPanel = MinimapRenderer.Render(
            vm.PlayerPosition,
            vm.LocalMapRooms,
            vm.VisitedRoomIds);
        rootLayout["Sidebar"].Update(minimapPanel);

        // 4. Update Footer (Turn counter + tips - v0.3.9c)
        rootLayout["Footer"].Update(CreateFooter(vm));

        // 5. Clear and render complete layout
        AnsiConsole.Clear();
        AnsiConsole.Write(rootLayout);

        _logger.LogTrace("[HUD] Render complete");
    }

    /// <summary>
    /// Creates the status bar panel with colored resource bars.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    private Panel CreateStatusBar(ExplorationViewModel vm)
    {
        // Get colors based on current values (themed v0.3.14a)
        var hpColor = StatusWidget.GetHpColor(vm.CurrentHp, vm.MaxHp, _theme);
        var stamColor = StatusWidget.GetStaminaColor(vm.CurrentStamina, vm.MaxStamina, _theme);
        var stressColor = StatusWidget.GetStressColor(vm.CurrentStress, _theme);
        var headerColor = _theme.GetColor("HeaderColor");
        var dimColor = _theme.GetColor("DimColor");

        // Build status grid
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadRight(2));  // Name
        grid.AddColumn(new GridColumn().Width(30));              // HP
        grid.AddColumn(new GridColumn().Width(30));              // Stamina
        grid.AddColumn(new GridColumn().Width(18));              // Stress

        // HP bar with color
        var hpBar = StatusWidget.RenderBar(vm.CurrentHp, vm.MaxHp);
        var hpMarkup = $"[{hpColor.ToMarkup()}]{hpBar}[/] [{dimColor}]{vm.CurrentHp}/{vm.MaxHp}[/]";

        // Stamina bar with color
        var stamBar = StatusWidget.RenderBar(vm.CurrentStamina, vm.MaxStamina);
        var stamMarkup = $"[{stamColor.ToMarkup()}]{stamBar}[/] [{dimColor}]{vm.CurrentStamina}/{vm.MaxStamina}[/]";

        // Stress display (no bar, just colored value)
        var stressMarkup = $"[{stressColor.ToMarkup()}]Stress: {vm.CurrentStress}%[/]";

        grid.AddRow(
            new Markup($"[bold {headerColor}]{Markup.Escape(vm.CharacterName)}[/]"),
            new Markup($"[bold]HP:[/] {hpMarkup}"),
            new Markup($"[bold]STA:[/] {stamMarkup}"),
            new Markup(stressMarkup)
        );

        return new Panel(grid).Border(BoxBorder.None);
    }

    /// <summary>
    /// Creates the footer with turn counter and optional context tips (v0.3.9c).
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    private IRenderable CreateFooter(ExplorationViewModel vm)
    {
        var dimColor = _theme.GetColor("DimColor");
        var lines = new List<string>
        {
            $"[{dimColor}]Turn {vm.TurnCount}[/]"
        };

        // Add context tips if present
        if (vm.ContextTips != null && vm.ContextTips.Count > 0)
        {
            foreach (var tip in vm.ContextTips.Take(2))  // Max 2 tips in footer
            {
                var icon = tip.Priority switch
                {
                    >= HelpTip.PriorityCritical => "!",
                    >= HelpTip.PriorityWarning => "*",
                    _ => "-"
                };
                lines.Add($"  [{tip.Color}]{icon} {tip.Title}:[/] {tip.Message}");
            }
        }

        return new Markup(string.Join("\n", lines));
    }
}
