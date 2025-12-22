using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders the combat TUI using Spectre.Console.
/// Displays player stats, turn order, and combat log in a structured layout.
/// Updated v0.3.9a: Added visual effect service for border flash effects.
/// Updated v0.3.9b: Added theme service for accessibility color support.
/// </summary>
public class CombatScreenRenderer : ICombatScreenRenderer
{
    private readonly ILogger<CombatScreenRenderer> _logger;
    private readonly IVisualEffectService _visualEffectService;
    private readonly IThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="visualEffectService">The visual effect service for border flash effects (v0.3.9a).</param>
    /// <param name="themeService">The theme service for accessibility color support (v0.3.9b).</param>
    public CombatScreenRenderer(
        ILogger<CombatScreenRenderer> logger,
        IVisualEffectService visualEffectService,
        IThemeService themeService)
    {
        _logger = logger;
        _visualEffectService = visualEffectService;
        _themeService = themeService;
    }

    /// <inheritdoc/>
    public void Render(CombatViewModel vm)
    {
        AnsiConsole.Clear();

        // 1. Header - Player Stats Bar
        RenderHeader(vm.PlayerStats, _themeService);

        AnsiConsole.WriteLine();

        // 2. Tactical Grid (v0.3.6a) - with VFX border override (v0.3.9a) and theme (v0.3.9b)
        var borderOverride = _visualEffectService.GetBorderOverride();
        var gridPanel = CombatGridRenderer.Render(vm, _themeService, borderOverride);
        AnsiConsole.Write(gridPanel);

        AnsiConsole.WriteLine();

        // 3. Initiative Timeline (v0.3.6b) - with theme support (v0.3.9b)
        var timelinePanel = TimelineRenderer.Render(vm.TimelineProjection, vm.RoundNumber, _themeService);
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
    /// Renders the player stats header bar with theme support.
    /// </summary>
    private static void RenderHeader(PlayerStatsView stats, IThemeService themeService)
    {
        // Use themed colors for health states
        var hpColor = stats.CurrentHp <= stats.MaxHp / 4 ? themeService.GetColor("HealthCritical")
            : stats.CurrentHp <= stats.MaxHp / 2 ? themeService.GetColor("HealthHigh")
            : themeService.GetColor("HealthFull");

        var staminaColor = stats.CurrentStamina <= stats.MaxStamina / 4 ? themeService.GetColor("HealthCritical")
            : stats.CurrentStamina <= stats.MaxStamina / 2 ? themeService.GetColor("HealthHigh")
            : themeService.GetColor("StaminaColor");

        // Stress uses themed color system
        var stressColor = stats.CurrentStress switch
        {
            >= 100 => $"bold {themeService.GetColor("HealthCritical")}",  // Breaking
            >= 80 => themeService.GetColor("HealthCritical"),              // Fractured
            >= 60 => themeService.GetColor("StressHigh"),                  // Distressed
            >= 40 => themeService.GetColor("StressMid"),                   // Shaken
            >= 20 => themeService.GetColor("StaminaColor"),                // Unsettled
            _ => themeService.GetColor("HealthFull")                       // Stable
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

        // Corruption uses themed color system (v0.3.0b/v0.3.9b)
        var corruptionColor = stats.CurrentCorruption switch
        {
            >= 100 => $"bold white on {themeService.GetColor("HealthCritical")}",  // Terminal
            >= 81 => $"bold {themeService.GetColor("HealthCritical")}",             // Fractured
            >= 61 => themeService.GetColor("HealthCritical"),                        // Blighted
            >= 41 => themeService.GetColor("HealthLow"),                             // Corrupted
            >= 21 => themeService.GetColor("NeutralColor"),                          // Tainted
            _ => themeService.GetColor("HealthFull")                                 // Pristine
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

        var headerColor = themeService.GetColor("InfoColor");
        var header = new Rule("[bold]COMBAT[/]")
        {
            Justification = Justify.Left,
            Style = Style.Parse(headerColor)
        };
        AnsiConsole.Write(header);

        var neutralColor = themeService.GetColor("NeutralColor");
        AnsiConsole.MarkupLine(
            $"  [bold]HP:[/] [{hpColor}]{stats.CurrentHp}/{stats.MaxHp}[/]    " +
            $"[bold]Stamina:[/] [{staminaColor}]{stats.CurrentStamina}/{stats.MaxStamina}[/]    " +
            $"[bold]Stress:[/] [{stressColor}]{stats.CurrentStress}[/] [{neutralColor}]({stressLabel})[/]");

        // Second line for corruption (only show if > 0 to avoid clutter when pristine)
        if (stats.CurrentCorruption > 0)
        {
            AnsiConsole.MarkupLine(
                $"  [bold {themeService.GetColor("HealthCritical")}]Blight:[/] [{corruptionColor}]{stats.CurrentCorruption}[/] [{neutralColor}]({corruptionLabel})[/]");
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
