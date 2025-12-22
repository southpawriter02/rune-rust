using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
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

        // 2. Tactical Grid (v0.3.6a)
        var gridPanel = CombatGridRenderer.Render(vm);
        AnsiConsole.Write(gridPanel);

        AnsiConsole.WriteLine();

        // 3. Initiative Timeline (v0.3.6b) - replaces turn order table
        var timelinePanel = TimelineRenderer.Render(vm.TimelineProjection, vm.RoundNumber);
        AnsiConsole.Write(timelinePanel);

        AnsiConsole.WriteLine();

        // 4. Player Abilities (v0.2.3c)
        if (vm.PlayerAbilities != null && vm.PlayerAbilities.Count > 0)
        {
            RenderAbilities(vm.PlayerAbilities);
            AnsiConsole.WriteLine();
        }

        // 5. Combat Log Panel
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

        // Stress uses 6-tier color system based on StressStatus thresholds
        var stressColor = stats.CurrentStress switch
        {
            >= 100 => "bold red",   // Breaking
            >= 80 => "red",          // Fractured
            >= 60 => "magenta",      // Distressed
            >= 40 => "yellow",       // Shaken
            >= 20 => "cyan",         // Unsettled
            _ => "green"             // Stable
        };

        var stressLabel = stats.CurrentStress switch
        {
            >= 100 => "BREAKING",
            >= 80 => "Fractured",
            >= 60 => "Distressed",
            >= 40 => "Shaken",
            >= 20 => "Unsettled",
            _ => "Stable"
        };

        // Corruption uses 6-tier dark color system (v0.3.0b)
        var corruptionColor = stats.CurrentCorruption switch
        {
            >= 100 => "bold white on red",  // Terminal - inverted for emphasis
            >= 81 => "bold red",             // Fractured
            >= 61 => "red",                  // Blighted
            >= 41 => "maroon",               // Corrupted
            >= 21 => "grey",                 // Tainted
            _ => "green"                     // Pristine
        };

        var corruptionLabel = stats.CurrentCorruption switch
        {
            >= 100 => "TERMINAL",
            >= 81 => "Fractured",
            >= 61 => "Blighted",
            >= 41 => "Corrupted",
            >= 21 => "Tainted",
            _ => "Pristine"
        };

        var header = new Rule("[bold]COMBAT[/]")
        {
            Justification = Justify.Left,
            Style = Style.Parse("blue")
        };
        AnsiConsole.Write(header);

        AnsiConsole.MarkupLine(
            $"  [bold]HP:[/] [{hpColor}]{stats.CurrentHp}/{stats.MaxHp}[/]    " +
            $"[bold]Stamina:[/] [{staminaColor}]{stats.CurrentStamina}/{stats.MaxStamina}[/]    " +
            $"[bold]Stress:[/] [{stressColor}]{stats.CurrentStress}[/] [grey]({stressLabel})[/]");

        // Second line for corruption (only show if > 0 to avoid clutter when pristine)
        if (stats.CurrentCorruption > 0)
        {
            AnsiConsole.MarkupLine(
                $"  [bold darkred]Blight:[/] [{corruptionColor}]{stats.CurrentCorruption}[/] [grey]({corruptionLabel})[/]");
        }
    }

    /// <summary>
    /// Renders the turn order as a table with status effects.
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
        table.AddColumn(new TableColumn("[grey]Status[/]").Width(18));

        foreach (var combatant in turnOrder)
        {
            var turnMarker = combatant.IsActive ? "[bold yellow]>[/] " : "  ";
            var nameColor = combatant.IsPlayer ? "cyan" : "red";
            var nameMarkup = $"{turnMarker}[{nameColor}]{EscapeMarkup(combatant.Name)}[/]";

            // HealthStatus already contains markup for enemies
            var healthDisplay = combatant.IsPlayer
                ? $"[green]{combatant.HealthStatus}[/]"
                : combatant.HealthStatus;

            // StatusEffects already contain Spectre markup from MapToView
            var statusDisplay = combatant.StatusEffects;

            table.AddRow(
                $"[grey]{combatant.InitiativeDisplay}[/]",
                nameMarkup,
                healthDisplay,
                statusDisplay
            );
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Renders the player abilities panel with hotkeys and status.
    /// </summary>
    private static void RenderAbilities(List<AbilityView> abilities)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .Title("[bold yellow]YOUR ABILITIES[/]");

        table.AddColumn(new TableColumn("[grey]#[/]").Centered().Width(3));
        table.AddColumn(new TableColumn("[grey]Ability[/]").Width(20));
        table.AddColumn(new TableColumn("[grey]Cost[/]").Width(12));
        table.AddColumn(new TableColumn("[grey]Status[/]").Width(12));

        foreach (var ability in abilities)
        {
            var hotkeyColor = ability.IsUsable ? "yellow" : "grey";
            var nameColor = ability.IsUsable ? "white" : "grey";
            var costColor = ability.IsUsable ? "cyan" : "grey";

            string statusText;
            if (ability.CooldownRemaining > 0)
            {
                statusText = $"[red]CD: {ability.CooldownRemaining}[/]";
            }
            else if (ability.IsUsable)
            {
                statusText = "[green]READY[/]";
            }
            else
            {
                statusText = "[grey]N/A[/]";
            }

            table.AddRow(
                $"[{hotkeyColor}][{ability.Hotkey}][/]",
                $"[{nameColor}]{EscapeMarkup(ability.Name)}[/]",
                $"[{costColor}]{ability.CostDisplay}[/]",
                statusText
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
