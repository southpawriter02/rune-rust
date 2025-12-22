using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the tactical combat grid showing team formations (v0.3.9b).
/// Displays combatants organized by row position: Enemy Back, Enemy Front, Player Front, Player Back.
/// Updated v0.3.9a: Added border color override support for visual effects.
/// Updated v0.3.9b: Added theme support for accessibility.
/// </summary>
public static class CombatGridRenderer
{
    /// <summary>
    /// Renders the tactical battlefield panel showing combatant positions with theme support.
    /// </summary>
    /// <param name="vm">The combat view model containing row-grouped combatants.</param>
    /// <param name="themeService">The theme service for color lookups.</param>
    /// <param name="borderColorOverride">Optional border color override for visual effects (v0.3.9a).</param>
    /// <returns>A Panel containing the battlefield grid.</returns>
    public static Panel Render(CombatViewModel vm, IThemeService themeService, string? borderColorOverride = null)
    {
        var neutralColor = themeService.GetColor("NeutralColor");
        var enemyColor = themeService.GetColor("EnemyColor");
        var playerColor = themeService.GetColor("PlayerColor");
        var headerColor = themeService.GetColor("HeaderColor");

        var rows = new List<IRenderable>
        {
            RenderRowLabel("ENEMY BACK", neutralColor),
            RenderCombatantRow(vm.EnemyBackRow, isEnemy: true, themeService),
            new Text(""),
            RenderRowLabel("ENEMY FRONT", enemyColor),
            RenderCombatantRow(vm.EnemyFrontRow, isEnemy: true, themeService),
            new Text(""),
            new Rule().RuleStyle(neutralColor),
            new Text(""),
            RenderRowLabel("FRONT LINE", playerColor),
            RenderCombatantRow(vm.PlayerFrontRow, isEnemy: false, themeService),
            new Text(""),
            RenderRowLabel("BACK LINE", neutralColor),
            RenderCombatantRow(vm.PlayerBackRow, isEnemy: false, themeService)
        };

        // Use override color if provided, otherwise use theme border color (v0.3.9a/b)
        var borderStyle = Style.Parse(borderColorOverride ?? themeService.GetColor("BorderColor"));

        return new Panel(new Rows(rows))
            .Header($"[bold {headerColor}]BATTLEFIELD[/]")
            .Border(BoxBorder.Double)
            .BorderStyle(borderStyle)
            .Expand();
    }

    /// <summary>
    /// Renders the tactical battlefield panel (legacy, non-themed).
    /// </summary>
    public static Panel Render(CombatViewModel vm, string? borderColorOverride = null)
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

        var borderStyle = Style.Parse(borderColorOverride ?? "grey");

        return new Panel(new Rows(rows))
            .Header("[bold yellow]BATTLEFIELD[/]")
            .Border(BoxBorder.Double)
            .BorderStyle(borderStyle)
            .Expand();
    }

    /// <summary>
    /// Renders a row label with the specified color.
    /// </summary>
    private static IRenderable RenderRowLabel(string label, string color) =>
        new Markup($"  [{color}]── {label} ──[/]");

    /// <summary>
    /// Renders a row of combatants with theme support.
    /// </summary>
    private static IRenderable RenderCombatantRow(List<CombatantView>? combatants, bool isEnemy, IThemeService themeService)
    {
        if (combatants == null || combatants.Count == 0)
            return new Markup($"    [{themeService.GetColor("NeutralColor")}](empty)[/]");

        var formatted = combatants.Select(c => FormatCombatant(c, isEnemy, themeService));
        return new Markup("    " + string.Join("  ", formatted));
    }

    /// <summary>
    /// Renders a row of combatants (legacy, non-themed).
    /// </summary>
    private static IRenderable RenderCombatantRow(List<CombatantView>? combatants, bool isEnemy)
    {
        if (combatants == null || combatants.Count == 0)
            return new Markup("    [grey](empty)[/]");

        var formatted = combatants.Select(c => FormatCombatant(c, isEnemy));
        return new Markup("    " + string.Join("  ", formatted));
    }

    /// <summary>
    /// Formats a single combatant for display with theme support.
    /// </summary>
    public static string FormatCombatant(CombatantView c, bool isEnemy, IThemeService themeService)
    {
        var color = isEnemy ? themeService.GetColor("EnemyColor") : themeService.GetColor("PlayerColor");
        var activeColor = themeService.GetColor("ActiveColor");
        var criticalColor = themeService.GetColor("HealthCritical");
        var warningColor = themeService.GetColor("WarningColor");

        var activeMarker = c.IsActive ? $"[bold {activeColor}]>[/]" : " ";

        var escapedName = Markup.Escape(c.Name);

        var healthIcon = "";
        if (isEnemy)
        {
            if (c.HealthStatus.Contains("Critical"))
                healthIcon = $" [{criticalColor}]!!![/]";
            else if (c.HealthStatus.Contains("Wounded"))
                healthIcon = $" [{warningColor}]!![/]";
        }

        var intentDisplay = "";
        if (isEnemy && !string.IsNullOrEmpty(c.IntentIcon))
        {
            intentDisplay = $" [{warningColor}]{c.IntentIcon}[/]";
        }

        var statusDisplay = "";
        if (!string.IsNullOrEmpty(c.StatusIcons))
        {
            statusDisplay = $" {c.StatusIcons}";
        }

        var targetIndicator = c.IsTargeted ? "[bold white]*[/]" : "";

        return $"{activeMarker}[{color}]{escapedName}[/]{healthIcon}{intentDisplay}{statusDisplay}{targetIndicator}";
    }

    /// <summary>
    /// Formats a single combatant for display (legacy, non-themed).
    /// </summary>
    public static string FormatCombatant(CombatantView c, bool isEnemy)
    {
        var color = isEnemy ? "red" : "cyan";
        var activeMarker = c.IsActive ? "[bold yellow]>[/]" : " ";

        var escapedName = Markup.Escape(c.Name);

        var healthIcon = "";
        if (isEnemy)
        {
            if (c.HealthStatus.Contains("Critical"))
                healthIcon = " [red]!!![/]";
            else if (c.HealthStatus.Contains("Wounded"))
                healthIcon = " [yellow]!![/]";
        }

        var intentDisplay = "";
        if (isEnemy && !string.IsNullOrEmpty(c.IntentIcon))
        {
            intentDisplay = $" [yellow]{c.IntentIcon}[/]";
        }

        var statusDisplay = "";
        if (!string.IsNullOrEmpty(c.StatusIcons))
        {
            statusDisplay = $" {c.StatusIcons}";
        }

        var targetIndicator = c.IsTargeted ? "[bold white]*[/]" : "";

        return $"{activeMarker}[{color}]{escapedName}[/]{healthIcon}{intentDisplay}{statusDisplay}{targetIndicator}";
    }
}
