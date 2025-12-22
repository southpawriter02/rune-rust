using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the persistent exploration HUD using Spectre.Console Layout (v0.3.5a).
/// Displays a three-pane interface: Header (stats), Body (room/minimap), Footer (turn).
/// Updated with RoomRenderer integration in v0.3.5c.
/// </summary>
public class ExplorationScreenRenderer : IExplorationScreenRenderer
{
    private readonly ILogger<ExplorationScreenRenderer> _logger;

    public ExplorationScreenRenderer(ILogger<ExplorationScreenRenderer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void Render(ExplorationViewModel vm)
    {
        _logger.LogTrace("[HUD] Rendering exploration screen. HP: {HP}/{MaxHP}, Room: {Room}",
            vm.CurrentHp, vm.MaxHp, vm.RoomName);

        // Build fresh layout each render (Layout is mutable, safer to rebuild)
        var rootLayout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("Body").SplitColumns(
                    new Layout("Main").Ratio(7),
                    new Layout("Sidebar").Ratio(3)
                ),
                new Layout("Footer").Size(1)
            );

        // 1. Update Header (Status Bar)
        rootLayout["Header"].Update(CreateStatusBar(vm));

        // 2. Update Main Content (Room View - v0.3.5c)
        var roomPanel = RoomRenderer.Render(vm);
        rootLayout["Main"].Update(roomPanel);

        // 3. Update Sidebar (Minimap - v0.3.5b)
        var minimapPanel = MinimapRenderer.Render(
            vm.PlayerPosition,
            vm.LocalMapRooms,
            vm.VisitedRoomIds);
        rootLayout["Sidebar"].Update(minimapPanel);

        // 4. Update Footer (Turn counter)
        rootLayout["Footer"].Update(
            new Markup($"[grey]Turn {vm.TurnCount}[/]"));

        // 5. Clear and render complete layout
        AnsiConsole.Clear();
        AnsiConsole.Write(rootLayout);

        _logger.LogTrace("[HUD] Render complete");
    }

    /// <summary>
    /// Creates the status bar panel with colored resource bars.
    /// </summary>
    private Panel CreateStatusBar(ExplorationViewModel vm)
    {
        // Get colors based on current values
        var hpColor = StatusWidget.GetHpColor(vm.CurrentHp, vm.MaxHp);
        var stamColor = StatusWidget.GetStaminaColor(vm.CurrentStamina, vm.MaxStamina);
        var stressColor = StatusWidget.GetStressColor(vm.CurrentStress);

        // Build status grid
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadRight(2));  // Name
        grid.AddColumn(new GridColumn().Width(30));              // HP
        grid.AddColumn(new GridColumn().Width(30));              // Stamina
        grid.AddColumn(new GridColumn().Width(18));              // Stress

        // HP bar with color
        var hpBar = StatusWidget.RenderBar(vm.CurrentHp, vm.MaxHp);
        var hpMarkup = $"[{hpColor.ToMarkup()}]{hpBar}[/] [grey]{vm.CurrentHp}/{vm.MaxHp}[/]";

        // Stamina bar with color
        var stamBar = StatusWidget.RenderBar(vm.CurrentStamina, vm.MaxStamina);
        var stamMarkup = $"[{stamColor.ToMarkup()}]{stamBar}[/] [grey]{vm.CurrentStamina}/{vm.MaxStamina}[/]";

        // Stress display (no bar, just colored value)
        var stressMarkup = $"[{stressColor.ToMarkup()}]Stress: {vm.CurrentStress}%[/]";

        grid.AddRow(
            new Markup($"[bold gold1]{Markup.Escape(vm.CharacterName)}[/]"),
            new Markup($"[bold]HP:[/] {hpMarkup}"),
            new Markup($"[bold]STA:[/] {stamMarkup}"),
            new Markup(stressMarkup)
        );

        return new Panel(grid).Border(BoxBorder.None);
    }
}
