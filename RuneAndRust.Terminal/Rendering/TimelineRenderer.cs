using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the horizontal initiative timeline (v0.3.9b).
/// Shows current round remaining turns + next round projection.
/// Stateless static class - no DI registration required.
/// Updated v0.3.9b: Added theme support for accessibility.
/// </summary>
public static class TimelineRenderer
{
    /// <summary>
    /// Default number of timeline slots to display.
    /// </summary>
    public const int DefaultWindowSize = 8;

    /// <summary>
    /// Renders the initiative timeline as a horizontal grid panel with theme support.
    /// </summary>
    /// <param name="projection">The timeline projection entries to display.</param>
    /// <param name="currentRound">The current combat round number.</param>
    /// <param name="themeService">The theme service for color lookups.</param>
    /// <returns>A Panel containing the timeline grid.</returns>
    public static Panel Render(List<TimelineEntryView>? projection, int currentRound, IThemeService themeService)
    {
        var neutralColor = themeService.GetColor("NeutralColor");
        var headerColor = themeService.GetColor("HeaderColor");

        if (projection == null || projection.Count == 0)
        {
            return new Panel(new Markup($"[{neutralColor}]No timeline data[/]"))
                .Header("[bold]INITIATIVE[/]")
                .Border(BoxBorder.Rounded);
        }

        var grid = new Grid();

        // Add columns for each timeline entry (up to window size)
        var entriesToShow = projection.Take(DefaultWindowSize).ToList();
        foreach (var _ in entriesToShow)
        {
            grid.AddColumn(new GridColumn().Width(10).NoWrap());
        }

        // Row 1: Round markers (only show when round changes)
        var roundMarkers = BuildRoundMarkers(entriesToShow, themeService);
        grid.AddRow(roundMarkers);

        // Row 2: Combatant names with indicators
        var names = entriesToShow
            .Select(e => FormatTimelineEntry(e, themeService))
            .ToArray();
        grid.AddRow(names);

        // Row 3: Initiative values
        var initiatives = entriesToShow
            .Select(e => $"[{neutralColor}]{e.Initiative}[/]")
            .ToArray();
        grid.AddRow(initiatives);

        return new Panel(grid)
            .Header($"[bold {headerColor}]INITIATIVE TIMELINE[/]")
            .Border(BoxBorder.Rounded)
            .Expand();
    }

    /// <summary>
    /// Renders the initiative timeline (legacy, non-themed).
    /// </summary>
    public static Panel Render(List<TimelineEntryView>? projection, int currentRound)
    {
        if (projection == null || projection.Count == 0)
        {
            return new Panel(new Markup("[grey]No timeline data[/]"))
                .Header("[bold]INITIATIVE[/]")
                .Border(BoxBorder.Rounded);
        }

        var grid = new Grid();

        var entriesToShow = projection.Take(DefaultWindowSize).ToList();
        foreach (var _ in entriesToShow)
        {
            grid.AddColumn(new GridColumn().Width(10).NoWrap());
        }

        var roundMarkers = BuildRoundMarkers(entriesToShow);
        grid.AddRow(roundMarkers);

        var names = entriesToShow
            .Select(FormatTimelineEntry)
            .ToArray();
        grid.AddRow(names);

        var initiatives = entriesToShow
            .Select(e => $"[grey]{e.Initiative}[/]")
            .ToArray();
        grid.AddRow(initiatives);

        return new Panel(grid)
            .Header("[bold yellow]INITIATIVE TIMELINE[/]")
            .Border(BoxBorder.Rounded)
            .Expand();
    }

    /// <summary>
    /// Builds round marker strings with theme support.
    /// </summary>
    private static string[] BuildRoundMarkers(List<TimelineEntryView> entries, IThemeService themeService)
    {
        var markers = new List<string>();
        int? lastRound = null;
        var neutralColor = themeService.GetColor("NeutralColor");

        foreach (var entry in entries)
        {
            if (entry.RoundNumber != lastRound)
            {
                markers.Add($"[{neutralColor}]R{entry.RoundNumber}[/]");
                lastRound = entry.RoundNumber;
            }
            else
            {
                markers.Add("");
            }
        }

        return markers.ToArray();
    }

    /// <summary>
    /// Builds round marker strings (legacy, non-themed).
    /// </summary>
    private static string[] BuildRoundMarkers(List<TimelineEntryView> entries)
    {
        var markers = new List<string>();
        int? lastRound = null;

        foreach (var entry in entries)
        {
            if (entry.RoundNumber != lastRound)
            {
                markers.Add($"[grey]R{entry.RoundNumber}[/]");
                lastRound = entry.RoundNumber;
            }
            else
            {
                markers.Add("");
            }
        }

        return markers.ToArray();
    }

    /// <summary>
    /// Formats a single timeline entry with theme support.
    /// </summary>
    public static string FormatTimelineEntry(TimelineEntryView entry, IThemeService themeService)
    {
        var color = entry.IsPlayer ? themeService.GetColor("PlayerColor") : themeService.GetColor("EnemyColor");
        var activeColor = themeService.GetColor("ActiveColor");
        var criticalColor = themeService.GetColor("HealthCritical");
        var warningColor = themeService.GetColor("WarningColor");
        var neutralColor = themeService.GetColor("NeutralColor");

        var marker = entry.IsActive ? $"[bold {activeColor}]►[/]" : " ";

        // Health status indicators with themed colors
        var healthIcon = entry.HealthIndicator switch
        {
            "critical" => $"[{criticalColor}]![/]",
            "wounded" => $"[{warningColor}]~[/]",
            "dead" => $"[{neutralColor}]✗[/]",
            _ => ""
        };

        var name = TruncateName(entry.Name, 8);

        return $"{marker}[{color}]{Markup.Escape(name)}[/]{healthIcon}";
    }

    /// <summary>
    /// Formats a single timeline entry (legacy, non-themed).
    /// </summary>
    public static string FormatTimelineEntry(TimelineEntryView entry)
    {
        var color = entry.IsPlayer ? "cyan" : "red";
        var marker = entry.IsActive ? "[bold yellow]►[/]" : " ";

        var healthIcon = entry.HealthIndicator switch
        {
            "critical" => "[red]![/]",
            "wounded" => "[yellow]~[/]",
            "dead" => "[grey]✗[/]",
            _ => ""
        };

        var name = TruncateName(entry.Name, 8);

        return $"{marker}[{color}]{Markup.Escape(name)}[/]{healthIcon}";
    }

    /// <summary>
    /// Truncates a name to fit within the specified length, adding ellipsis if needed.
    /// </summary>
    private static string TruncateName(string name, int maxLength)
    {
        if (name.Length <= maxLength)
            return name;

        return name[..(maxLength - 1)] + "…";
    }
}
