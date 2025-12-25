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
/// Updated with IThemeService for accessibility themes in v0.3.14a.
/// </summary>
public class VictoryScreenRenderer : IVictoryScreenRenderer
{
    private readonly ILogger<VictoryScreenRenderer> _logger;
    private readonly IThemeService _theme;

    /// <summary>
    /// Initializes a new instance of the <see cref="VictoryScreenRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="theme">The theme service for accessibility colors.</param>
    public VictoryScreenRenderer(ILogger<VictoryScreenRenderer> logger, IThemeService theme)
    {
        _logger = logger;
        _theme = theme;
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
            var dimColor = _theme.GetColor("DimColor");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"  [{dimColor}]No loot found.[/]");
        }

        // Continue prompt
        AnsiConsole.WriteLine();
        var promptColor = _theme.GetColor("DimColor");
        AnsiConsole.MarkupLine($"[{promptColor}]Press any key to continue...[/]");
        ConsoleInputHelper.WaitForKeyPress();

        _logger.LogTrace("Victory screen rendered and dismissed");
    }

    /// <summary>
    /// Renders the victory banner at the top of the screen.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    private void RenderVictoryBanner()
    {
        var successColor = _theme.GetColor("SuccessColor");
        var rule = new Rule($"[bold {successColor}]VICTORY[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse(successColor)
        };
        AnsiConsole.Write(rule);
    }

    /// <summary>
    /// Renders the experience points section.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    /// <param name="xp">The experience points earned.</param>
    private void RenderXpSection(int xp)
    {
        var warningColor = _theme.GetColor("WarningColor");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"  [bold]Experience Earned:[/] [{warningColor}]+{xp} XP[/]");
    }

    /// <summary>
    /// Renders the loot table with quality-based coloring.
    /// Updated with themed colors in v0.3.14a.
    /// </summary>
    /// <param name="loot">The list of items found.</param>
    private void RenderLootTable(List<Item> loot)
    {
        var dimColor = _theme.GetColor("DimColor");
        var warningColor = _theme.GetColor("WarningColor");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  [bold]Loot Found:[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Expand();

        table.AddColumn(new TableColumn($"[{dimColor}]Item[/]").Width(25));
        table.AddColumn(new TableColumn($"[{dimColor}]Quality[/]").Width(12));
        table.AddColumn(new TableColumn($"[{dimColor}]Type[/]").Width(10));
        table.AddColumn(new TableColumn($"[{dimColor}]Value[/]").Centered().Width(8));

        foreach (var item in loot)
        {
            var qualityColor = GetQualityColor(item.Quality, _theme);
            var qualityName = item.Quality.ToString();

            table.AddRow(
                $"[{qualityColor}]{EscapeMarkup(item.Name)}[/]",
                $"[{qualityColor}]{qualityName}[/]",
                $"[{dimColor}]{item.ItemType}[/]",
                $"[{warningColor}]{item.Value}[/]"
            );

            _logger.LogDebug("Rendered loot: {Name} ({Quality})", item.Name, item.Quality);
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Maps a QualityTier to a themed Spectre.Console color name (v0.3.14a).
    /// </summary>
    /// <param name="quality">The quality tier to map.</param>
    /// <param name="theme">The theme service for color lookups.</param>
    /// <returns>The color name for Spectre markup.</returns>
    public static string GetQualityColor(QualityTier quality, IThemeService theme) => quality switch
    {
        QualityTier.JuryRigged => theme.GetColor("QualityJunk"),
        QualityTier.Scavenged => theme.GetColor("QualityCommon"),
        QualityTier.ClanForged => theme.GetColor("QualityUncommon"),
        QualityTier.Optimized => theme.GetColor("QualityRare"),
        QualityTier.MythForged => theme.GetColor("QualityLegendary"),
        _ => theme.GetColor("QualityCommon")
    };

    /// <summary>
    /// Maps a QualityTier to a Spectre.Console color name.
    /// Legacy method - prefer themed overload (v0.3.14a).
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
