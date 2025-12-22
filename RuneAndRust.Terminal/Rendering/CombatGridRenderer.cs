using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the tactical combat grid showing team formations (v0.3.6a).
/// Displays combatants organized by row position: Enemy Back, Enemy Front, Player Front, Player Back.
/// </summary>
public static class CombatGridRenderer
{
    /// <summary>
    /// Renders the tactical battlefield panel showing combatant positions.
    /// </summary>
    /// <param name="vm">The combat view model containing row-grouped combatants.</param>
    /// <returns>A Panel containing the battlefield grid.</returns>
    public static Panel Render(CombatViewModel vm)
    {
        var rows = new List<IRenderable>
        {
            RenderRowLabel("ENEMY BACK", "grey"),
            RenderCombatantRow(vm.EnemyBackRow, isEnemy: true),
            new Text(""),
            RenderRowLabel("ENEMY FRONT", "red"),
            RenderCombatantRow(vm.EnemyFrontRow, isEnemy: true),
            new Text(""),
            new Rule().RuleStyle("grey"),
            new Text(""),
            RenderRowLabel("FRONT LINE", "cyan"),
            RenderCombatantRow(vm.PlayerFrontRow, isEnemy: false),
            new Text(""),
            RenderRowLabel("BACK LINE", "grey"),
            RenderCombatantRow(vm.PlayerBackRow, isEnemy: false)
        };

        return new Panel(new Rows(rows))
            .Header("[bold yellow]BATTLEFIELD[/]")
            .Border(BoxBorder.Double)
            .Expand();
    }

    /// <summary>
    /// Renders a row label with the specified color.
    /// </summary>
    /// <param name="label">The row label text.</param>
    /// <param name="color">The Spectre.Console color name.</param>
    /// <returns>A markup renderable for the label.</returns>
    private static IRenderable RenderRowLabel(string label, string color) =>
        new Markup($"  [{color}]── {label} ──[/]");

    /// <summary>
    /// Renders a row of combatants as formatted markup.
    /// </summary>
    /// <param name="combatants">The list of combatants in this row.</param>
    /// <param name="isEnemy">Whether this row contains enemy combatants.</param>
    /// <returns>A markup renderable for the combatant row.</returns>
    private static IRenderable RenderCombatantRow(List<CombatantView>? combatants, bool isEnemy)
    {
        if (combatants == null || combatants.Count == 0)
            return new Markup("    [grey](empty)[/]");

        var formatted = combatants.Select(c => FormatCombatant(c, isEnemy));
        return new Markup("    " + string.Join("  ", formatted));
    }

    /// <summary>
    /// Formats a single combatant for display with health indicators.
    /// </summary>
    /// <param name="c">The combatant view to format.</param>
    /// <param name="isEnemy">Whether this is an enemy combatant.</param>
    /// <returns>A Spectre.Console markup string for the combatant.</returns>
    public static string FormatCombatant(CombatantView c, bool isEnemy)
    {
        var color = isEnemy ? "red" : "cyan";
        var activeMarker = c.IsActive ? "[bold yellow]>[/]" : " ";

        // Escape any brackets in the name for Spectre markup
        var escapedName = Markup.Escape(c.Name);

        // Add health indicators for enemies
        var healthIcon = "";
        if (isEnemy)
        {
            if (c.HealthStatus.Contains("Critical"))
                healthIcon = " [red]!!![/]";
            else if (c.HealthStatus.Contains("Wounded"))
                healthIcon = " [yellow]!![/]";
        }

        // Add targeting indicator
        var targetIndicator = c.IsTargeted ? "[bold white]*[/]" : "";

        return $"{activeMarker}[{color}]{escapedName}[/]{healthIcon}{targetIndicator}";
    }
}
