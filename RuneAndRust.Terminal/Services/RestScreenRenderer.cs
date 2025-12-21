using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders the rest result screen with recovery deltas and ambush warnings.
/// Uses Spectre.Console for rich terminal formatting.
/// </summary>
public class RestScreenRenderer : IRestScreenRenderer
{
    private readonly ILogger<RestScreenRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public RestScreenRenderer(ILogger<RestScreenRenderer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Render(RestResult result)
    {
        _logger.LogInformation(
            "Rendering rest screen. HP+{Hp}, Stamina+{Stamina}, Stress-{Stress}, Exhausted: {Exhausted}",
            result.HpRecovered, result.StaminaRecovered, result.StressRecovered, result.IsExhausted);

        AnsiConsole.WriteLine();

        // Rest Banner
        RenderRestBanner();

        // Exhaustion Warning
        if (result.IsExhausted)
        {
            RenderExhaustionWarning();
        }

        // Recovery Table
        RenderRecoveryTable(result);

        // Supply Consumption
        if (result.SuppliesConsumed)
        {
            RenderSupplyConsumption();
        }

        // Time passage
        RenderTimePassage(result.TimeAdvancedMinutes);

        // Continue prompt
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
        Console.ReadKey(true);

        _logger.LogTrace("Rest screen rendered and dismissed");
    }

    /// <inheritdoc/>
    public void RenderAmbushWarning(AmbushResult ambushResult)
    {
        _logger.LogWarning(
            "Rendering ambush warning. BaseRisk: {Base}%, Final: {Final}%, Roll: {Roll}",
            ambushResult.BaseRiskPercent, ambushResult.FinalRiskPercent, ambushResult.RollValue);

        AnsiConsole.WriteLine();

        // Ambush Banner
        RenderAmbushBanner();

        // Ambush Message
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"  [red]{EscapeMarkup(ambushResult.Message)}[/]");

        // Risk Details
        RenderRiskDetails(ambushResult);

        // Transition prompt
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Enemies approach...[/]");
        AnsiConsole.MarkupLine("[grey]Press Enter to engage.[/]");
        Console.ReadKey(true);

        _logger.LogTrace("Ambush warning rendered and dismissed");
    }

    /// <summary>
    /// Renders the rest banner at the top of the display.
    /// </summary>
    private static void RenderRestBanner()
    {
        var rule = new Rule("[bold yellow]Rest & Recovery[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("yellow")
        };
        AnsiConsole.Write(rule);
    }

    /// <summary>
    /// Renders the ambush warning banner.
    /// </summary>
    private static void RenderAmbushBanner()
    {
        var rule = new Rule("[bold red]AMBUSH![/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("red")
        };
        AnsiConsole.Write(rule);
    }

    /// <summary>
    /// Renders the exhaustion warning panel.
    /// </summary>
    private static void RenderExhaustionWarning()
    {
        AnsiConsole.WriteLine();
        var panel = new Panel("[red]You are Exhausted! Recovery is halved due to missing supplies.[/]")
        {
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("red"),
            Padding = new Padding(1, 0)
        };
        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Renders the recovery statistics table.
    /// </summary>
    /// <param name="result">The rest result containing recovery values.</param>
    private void RenderRecoveryTable(RestResult result)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Width(40);

        table.AddColumn(new TableColumn("[grey]Attribute[/]").Width(15));
        table.AddColumn(new TableColumn("[grey]Change[/]").Centered().Width(15));

        // Health recovery
        if (result.HpRecovered > 0)
        {
            table.AddRow("[white]Health[/]", $"[green]+{result.HpRecovered}[/]");
        }
        else
        {
            table.AddRow("[white]Health[/]", "[grey]+0[/]");
        }

        // Stamina recovery
        if (result.StaminaRecovered > 0)
        {
            table.AddRow("[white]Stamina[/]", $"[cyan]+{result.StaminaRecovered}[/]");
        }
        else
        {
            table.AddRow("[white]Stamina[/]", "[grey]+0[/]");
        }

        // Stress recovery
        if (result.StressRecovered > 0)
        {
            table.AddRow("[white]Stress[/]", $"[purple]-{result.StressRecovered}[/]");
        }
        else
        {
            table.AddRow("[white]Stress[/]", "[grey]-0[/]");
        }

        AnsiConsole.Write(table);

        _logger.LogDebug(
            "Rendered recovery: HP+{Hp}, Stamina+{Stamina}, Stress-{Stress}",
            result.HpRecovered, result.StaminaRecovered, result.StressRecovered);
    }

    /// <summary>
    /// Renders the supply consumption message.
    /// </summary>
    private static void RenderSupplyConsumption()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  [yellow]Consumed: 1 Ration, 1 Water[/]");
    }

    /// <summary>
    /// Renders the time passage message.
    /// </summary>
    /// <param name="minutes">The time advanced in minutes.</param>
    private static void RenderTimePassage(int minutes)
    {
        var hours = minutes / 60;
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"  [grey]{hours} hours have passed...[/]");
    }

    /// <summary>
    /// Renders the ambush risk calculation details.
    /// </summary>
    /// <param name="ambush">The ambush result with risk details.</param>
    private void RenderRiskDetails(AmbushResult ambush)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Width(40);

        table.AddColumn(new TableColumn("[grey]Detail[/]").Width(20));
        table.AddColumn(new TableColumn("[grey]Value[/]").Centered().Width(15));

        table.AddRow("[white]Base Risk[/]", $"[yellow]{ambush.BaseRiskPercent}%[/]");
        table.AddRow("[white]Wits Mitigation[/]", $"[green]-{ambush.MitigationPercent}%[/]");
        table.AddRow("[white]Final Risk[/]", $"[red]{ambush.FinalRiskPercent}%[/]");
        table.AddRow("[white]Roll[/]", $"[grey]{ambush.RollValue}[/]");

        AnsiConsole.Write(table);

        _logger.LogDebug(
            "Rendered ambush details: Base {Base}%, Mitigation -{Mit}%, Final {Final}%, Roll {Roll}",
            ambush.BaseRiskPercent, ambush.MitigationPercent, ambush.FinalRiskPercent, ambush.RollValue);
    }

    /// <summary>
    /// Escapes special Spectre.Console markup characters.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped text safe for Spectre markup.</returns>
    private static string EscapeMarkup(string text)
    {
        return text.Replace("[", "[[").Replace("]", "]]");
    }
}
