using RuneAndRust.Core.ViewModels;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the horizontal initiative timeline (v0.3.6b).
/// Shows current round remaining turns + next round projection.
/// Stateless static class - no DI registration required.
/// </summary>
public static class TimelineRenderer
{
    /// <summary>
    /// Default number of timeline slots to display.
    /// </summary>
    public const int DefaultWindowSize = 8;

    /// <summary>
    /// Renders the initiative timeline as a horizontal grid panel.
    /// </summary>
    /// <param name="projection">The timeline projection entries to display.</param>
    /// <param name="currentRound">The current combat round number.</param>
    /// <returns>A Panel containing the timeline grid.</returns>
    public static Panel Render(List<TimelineEntryView>? projection, int currentRound)
    {
        if (projection == null || projection.Count == 0)
        {
            return new Panel(new Markup("[grey]No timeline data[/]"))
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
        var roundMarkers = BuildRoundMarkers(entriesToShow);
        grid.AddRow(roundMarkers);

        // Row 2: Combatant names with indicators
        var names = entriesToShow
            .Select(FormatTimelineEntry)
            .ToArray();
        grid.AddRow(names);

        // Row 3: Initiative values
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
    /// Builds round marker strings, only showing when round changes.
    /// </summary>
    /// <param name="entries">The timeline entries to build markers for.</param>
    /// <returns>Array of round marker strings.</returns>
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
    /// Formats a single timeline entry with color and status indicators.
    /// </summary>
    /// <param name="entry">The timeline entry to format.</param>
    /// <returns>A Spectre.Console markup string for the entry.</returns>
    public static string FormatTimelineEntry(TimelineEntryView entry)
    {
        var color = entry.IsPlayer ? "cyan" : "red";
        var marker = entry.IsActive ? "[bold yellow]►[/]" : " ";

        // Health status indicators
        var healthIcon = entry.HealthIndicator switch
        {
            "critical" => "[red]![/]",
            "wounded" => "[yellow]~[/]",
            "dead" => "[grey]✗[/]",
            _ => ""
        };

        // Truncate name to fit column width (8 chars + 1 marker + 1 health = 10 total)
        var name = TruncateName(entry.Name, 8);

        return $"{marker}[{color}]{Markup.Escape(name)}[/]{healthIcon}";
    }

    /// <summary>
    /// Truncates a name to fit within the specified length, adding ellipsis if needed.
    /// </summary>
    /// <param name="name">The name to truncate.</param>
    /// <param name="maxLength">Maximum length before truncation.</param>
    /// <returns>The truncated name.</returns>
    private static string TruncateName(string name, int maxLength)
    {
        if (name.Length <= maxLength)
            return name;

        return name[..(maxLength - 1)] + "…";
    }
}
