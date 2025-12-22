using RuneAndRust.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders a stat preview panel with BarChart for the character creation wizard (v0.3.4b).
/// Shows attributes and derived stats in a visually appealing format.
/// </summary>
public static class StatPreviewRenderer
{
    /// <summary>
    /// Renders the stat preview as a Spectre.Console IRenderable.
    /// </summary>
    /// <param name="stats">Dictionary of attribute values.</param>
    /// <param name="derived">Calculated derived stats (HP, Stamina, AP).</param>
    /// <param name="lineageName">Display name of selected/previewed lineage (optional).</param>
    /// <param name="archetypeName">Display name of selected/previewed archetype (optional).</param>
    /// <returns>A renderable composition of stat preview elements.</returns>
    public static IRenderable Render(
        Dictionary<CharacterAttribute, int> stats,
        DerivedStats derived,
        string? lineageName = null,
        string? archetypeName = null)
    {
        var rows = new List<IRenderable>();

        // Header
        rows.Add(new Rule("[yellow]STAT PREVIEW[/]").LeftJustified());
        rows.Add(new Text(""));

        // Selection info
        if (!string.IsNullOrEmpty(lineageName))
            rows.Add(new Markup($"[grey]Lineage:[/] [cyan]{Markup.Escape(lineageName)}[/]"));
        if (!string.IsNullOrEmpty(archetypeName))
            rows.Add(new Markup($"[grey]Archetype:[/] [cyan]{Markup.Escape(archetypeName)}[/]"));
        if (!string.IsNullOrEmpty(lineageName) || !string.IsNullOrEmpty(archetypeName))
            rows.Add(new Text(""));

        // Attribute BarChart
        var chart = new BarChart()
            .Width(30)
            .AddItem("STU", stats[CharacterAttribute.Sturdiness], Color.Green)
            .AddItem("MIG", stats[CharacterAttribute.Might], Color.Red)
            .AddItem("WIT", stats[CharacterAttribute.Wits], Color.Blue)
            .AddItem("WIL", stats[CharacterAttribute.Will], Color.Purple)
            .AddItem("FIN", stats[CharacterAttribute.Finesse], Color.Yellow);

        rows.Add(chart);
        rows.Add(new Text(""));

        // Derived stats table
        var derivedTable = new Table().NoBorder().HideHeaders();
        derivedTable.AddColumn("Stat");
        derivedTable.AddColumn("Value");
        derivedTable.AddRow("[red]Max HP[/]", $"[white]{derived.MaxHP}[/]");
        derivedTable.AddRow("[yellow]Stamina[/]", $"[white]{derived.MaxStamina}[/]");
        derivedTable.AddRow("[blue]Action Pts[/]", $"[white]{derived.ActionPoints}[/]");

        rows.Add(derivedTable);

        return new Rows(rows);
    }
}
