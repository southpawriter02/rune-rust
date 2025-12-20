using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders the combat TUI using Spectre.Console.
/// Displays player stats, turn order, and combat log in a structured layout.
/// </summary>
public class CombatScreenRenderer : ICombatScreenRenderer
{
    private readonly ILogger<CombatScreenRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public CombatScreenRenderer(ILogger<CombatScreenRenderer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Render(CombatViewModel vm)
    {
        AnsiConsole.Clear();

        // 1. Header - Player Stats Bar
        RenderHeader(vm.PlayerStats);

        AnsiConsole.WriteLine();

        // 2. Turn Order Table
        RenderTurnOrder(vm.TurnOrder);

        AnsiConsole.WriteLine();

        // 3. Combat Log Panel
        RenderCombatLog(vm.RoundNumber, vm.CombatLog);

        AnsiConsole.WriteLine();

        _logger.LogTrace("Rendered combat screen");
    }

    /// <summary>
    /// Renders the player stats header bar.
    /// </summary>
    private static void RenderHeader(PlayerStatsView stats)
    {
        var hpColor = stats.CurrentHp <= stats.MaxHp / 4 ? "red"
            : stats.CurrentHp <= stats.MaxHp / 2 ? "yellow"
            : "green";

        var staminaColor = stats.CurrentStamina <= stats.MaxStamina / 4 ? "red"
            : stats.CurrentStamina <= stats.MaxStamina / 2 ? "yellow"
            : "cyan";

        var header = new Rule("[bold]COMBAT[/]")
        {
            Justification = Justify.Left,
            Style = Style.Parse("blue")
        };
        AnsiConsole.Write(header);

        AnsiConsole.MarkupLine(
            $"  [bold]HP:[/] [{hpColor}]{stats.CurrentHp}/{stats.MaxHp}[/]    " +
            $"[bold]Stamina:[/] [{staminaColor}]{stats.CurrentStamina}/{stats.MaxStamina}[/]");
    }

    /// <summary>
    /// Renders the turn order as a table.
    /// </summary>
    private static void RenderTurnOrder(List<CombatantView> turnOrder)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Expand();

        table.AddColumn(new TableColumn("[grey]Init[/]").Centered().Width(6));
        table.AddColumn(new TableColumn("[grey]Name[/]").Width(20));
        table.AddColumn(new TableColumn("[grey]Condition[/]").Width(12));

        foreach (var combatant in turnOrder)
        {
            var turnMarker = combatant.IsActive ? "[bold yellow]>[/] " : "  ";
            var nameColor = combatant.IsPlayer ? "cyan" : "red";
            var nameMarkup = $"{turnMarker}[{nameColor}]{EscapeMarkup(combatant.Name)}[/]";

            // HealthStatus already contains markup for enemies
            var healthDisplay = combatant.IsPlayer
                ? $"[green]{combatant.HealthStatus}[/]"
                : combatant.HealthStatus;

            table.AddRow(
                $"[grey]{combatant.InitiativeDisplay}[/]",
                nameMarkup,
                healthDisplay
            );
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Renders the combat log panel.
    /// </summary>
    private static void RenderCombatLog(int roundNumber, List<string> combatLog)
    {
        var logContent = combatLog.Count > 0
            ? string.Join("\n", combatLog)
            : "[grey]No combat events yet.[/]";

        var panel = new Panel(new Markup(logContent))
        {
            Header = new PanelHeader($"[bold]Round {roundNumber}[/]"),
            Border = BoxBorder.Heavy,
            BorderStyle = Style.Parse("grey"),
            Padding = new Padding(1, 0, 1, 0)
        };

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Escapes special Spectre.Console markup characters.
    /// </summary>
    private static string EscapeMarkup(string text)
    {
        return text.Replace("[", "[[").Replace("]", "]]");
    }
}
