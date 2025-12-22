using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Terminal.Helpers;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Renders the post-combat victory screen with loot and XP display.
/// Uses Spectre.Console for rich terminal formatting with quality-based coloring.
/// </summary>
public class VictoryScreenRenderer : IVictoryScreenRenderer
{
    private readonly ILogger<VictoryScreenRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VictoryScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public VictoryScreenRenderer(ILogger<VictoryScreenRenderer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Render(CombatResult result)
    {
        _logger.LogInformation(
            "Rendering victory screen. Victory: {Victory}, XP: {Xp}, Loot: {Count}",
            result.Victory, result.XpEarned, result.LootFound.Count);

        AnsiConsole.Clear();

        // Victory Banner
        RenderVictoryBanner();

        // XP Earned
        RenderXpSection(result.XpEarned);

        // Loot Table
        if (result.LootFound.Count > 0)
        {
            RenderLootTable(result.LootFound);
        }
        else
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("  [grey]No loot found.[/]");
        }

        // Continue prompt
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        ConsoleInputHelper.WaitForKeyPress();

        _logger.LogTrace("Victory screen rendered and dismissed");
    }

    /// <summary>
    /// Renders the victory banner at the top of the screen.
    /// </summary>
    private static void RenderVictoryBanner()
    {
        var rule = new Rule("[bold green]VICTORY[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("green")
        };
        AnsiConsole.Write(rule);
    }

    /// <summary>
    /// Renders the experience points section.
    /// </summary>
    /// <param name="xp">The experience points earned.</param>
    private static void RenderXpSection(int xp)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"  [bold]Experience Earned:[/] [yellow]+{xp} XP[/]");
    }

    /// <summary>
    /// Renders the loot table with quality-based coloring.
    /// </summary>
    /// <param name="loot">The list of items found.</param>
    private void RenderLootTable(List<Item> loot)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  [bold]Loot Found:[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Expand();

        table.AddColumn(new TableColumn("[grey]Item[/]").Width(25));
        table.AddColumn(new TableColumn("[grey]Quality[/]").Width(12));
        table.AddColumn(new TableColumn("[grey]Type[/]").Width(10));
        table.AddColumn(new TableColumn("[grey]Value[/]").Centered().Width(8));

        foreach (var item in loot)
        {
            var qualityColor = GetQualityColor(item.Quality);
            var qualityName = item.Quality.ToString();

            table.AddRow(
                $"[{qualityColor}]{EscapeMarkup(item.Name)}[/]",
                $"[{qualityColor}]{qualityName}[/]",
                $"[grey]{item.ItemType}[/]",
                $"[yellow]{item.Value}[/]"
            );

            _logger.LogDebug("Rendered loot: {Name} ({Quality})", item.Name, item.Quality);
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Maps a QualityTier to a Spectre.Console color name.
    /// </summary>
    /// <param name="quality">The quality tier to map.</param>
    /// <returns>The color name for Spectre markup.</returns>
    public static string GetQualityColor(QualityTier quality) => quality switch
    {
        QualityTier.JuryRigged => "grey",
        QualityTier.Scavenged => "white",
        QualityTier.ClanForged => "green",
        QualityTier.Optimized => "blue",
        QualityTier.MythForged => "magenta",
        _ => "white"
    };

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
