using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Engine.Helpers;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders a rich, formatted view of the current room (v0.3.5c).
/// Displays room name, description, visible entities, and exits.
/// Updated in v0.3.14a to support IThemeService for accessibility themes.
/// </summary>
public static class RoomRenderer
{
    /// <summary>
    /// Renders the complete room panel with themed colors (v0.3.14a).
    /// </summary>
    /// <param name="vm">The exploration view model containing room data.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>A Spectre.Console Panel with themed room content.</returns>
    public static Panel Render(ExplorationViewModel vm, IThemeService theme)
    {
        var separatorColor = theme.GetColor("SeparatorColor");
        var content = new Rows(
            BuildDescription(vm.RoomDescription, theme),
            new Rule().RuleStyle(separatorColor),
            BuildEntitySection(vm.VisibleObjects, vm.VisibleEnemies, theme),
            new Rule().RuleStyle(separatorColor),
            BuildExitsSection(vm.Exits, theme)
        );

        return new Panel(content)
            .Header($"[{vm.BiomeColor}]{Markup.Escape(vm.RoomName)}[/]")
            .Border(BoxBorder.Rounded)
            .Expand();
    }

    /// <summary>
    /// Renders the complete room panel for the exploration HUD Body section.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="vm">The exploration view model containing room data.</param>
    /// <returns>A Spectre.Console Panel with formatted room content.</returns>
    public static Panel Render(ExplorationViewModel vm)
    {
        var content = new Rows(
            BuildDescription(vm.RoomDescription),
            new Rule().RuleStyle("grey"),
            BuildEntitySection(vm.VisibleObjects, vm.VisibleEnemies),
            new Rule().RuleStyle("grey"),
            BuildExitsSection(vm.Exits)
        );

        return new Panel(content)
            .Header($"[{vm.BiomeColor}]{Markup.Escape(vm.RoomName)}[/]")
            .Border(BoxBorder.Rounded)
            .Expand();
    }

    /// <summary>
    /// Builds the room description section with themed colors (v0.3.14a).
    /// </summary>
    /// <param name="description">The room's narrative description.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>A renderable markup element.</returns>
    public static IRenderable BuildDescription(string description, IThemeService theme)
    {
        var narrativeColor = theme.GetColor("NarrativeColor");
        return new Markup($"[{narrativeColor}]{Markup.Escape(description)}[/]");
    }

    /// <summary>
    /// Builds the room description section.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="description">The room's narrative description.</param>
    /// <returns>A renderable markup element.</returns>
    public static IRenderable BuildDescription(string description)
    {
        return new Markup($"[grey]{Markup.Escape(description)}[/]");
    }

    /// <summary>
    /// Builds the entity section with themed colors (v0.3.14a).
    /// </summary>
    /// <param name="objects">List of pre-formatted object names with markup.</param>
    /// <param name="enemies">List of pre-formatted enemy names with health status.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>A renderable rows element containing entity lists.</returns>
    public static IRenderable BuildEntitySection(List<string> objects, List<string> enemies, IThemeService theme)
    {
        var lines = new List<IRenderable>();
        var warningColor = theme.GetColor("WarningColor");
        var enemyColor = theme.GetColor("EnemyColor");
        var dimColor = theme.GetColor("DimColor");

        // Objects section
        if (objects.Count > 0)
        {
            lines.Add(new Markup($"[{warningColor}]You see:[/]"));
            foreach (var obj in objects)
            {
                lines.Add(new Markup($"  {obj}"));
            }
        }

        // Enemies section
        if (enemies.Count > 0)
        {
            if (lines.Count > 0) lines.Add(new Text(""));  // Spacer
            lines.Add(new Markup($"[{enemyColor}]Hostiles:[/]"));
            foreach (var enemy in enemies)
            {
                lines.Add(new Markup($"  {enemy}"));
            }
        }

        // Empty state
        if (lines.Count == 0)
        {
            lines.Add(new Markup($"[{dimColor}]The area appears empty.[/]"));
        }

        return new Rows(lines);
    }

    /// <summary>
    /// Builds the entity section showing objects and enemies.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="objects">List of pre-formatted object names with markup.</param>
    /// <param name="enemies">List of pre-formatted enemy names with health status.</param>
    /// <returns>A renderable rows element containing entity lists.</returns>
    public static IRenderable BuildEntitySection(List<string> objects, List<string> enemies)
    {
        var lines = new List<IRenderable>();

        // Objects section
        if (objects.Count > 0)
        {
            lines.Add(new Markup("[yellow]You see:[/]"));
            foreach (var obj in objects)
            {
                lines.Add(new Markup($"  {obj}"));
            }
        }

        // Enemies section
        if (enemies.Count > 0)
        {
            if (lines.Count > 0) lines.Add(new Text(""));  // Spacer
            lines.Add(new Markup("[red]Hostiles:[/]"));
            foreach (var enemy in enemies)
            {
                lines.Add(new Markup($"  {enemy}"));
            }
        }

        // Empty state
        if (lines.Count == 0)
        {
            lines.Add(new Markup("[grey]The area appears empty.[/]"));
        }

        return new Rows(lines);
    }

    /// <summary>
    /// Builds the exits section with themed colors (v0.3.14a).
    /// </summary>
    /// <param name="exits">Comma-separated lowercase exit directions.</param>
    /// <param name="theme">The theme service for semantic color lookup.</param>
    /// <returns>A renderable markup element with exit information.</returns>
    public static IRenderable BuildExitsSection(string exits, IThemeService theme)
    {
        var dimColor = theme.GetColor("DimColor");
        var labelColor = theme.GetColor("LabelColor");

        if (string.IsNullOrEmpty(exits))
        {
            return new Markup($"[{dimColor}]There are no obvious exits.[/]");
        }

        return new Markup($"[{labelColor}]Exits:[/] [{dimColor}]{Markup.Escape(exits)}[/]");
    }

    /// <summary>
    /// Builds the exits section showing available directions.
    /// Legacy method - prefer themed overload (v0.3.14a).
    /// </summary>
    /// <param name="exits">Comma-separated lowercase exit directions.</param>
    /// <returns>A renderable markup element with exit information.</returns>
    public static IRenderable BuildExitsSection(string exits)
    {
        if (string.IsNullOrEmpty(exits))
        {
            return new Markup("[grey]There are no obvious exits.[/]");
        }

        return new Markup($"[white]Exits:[/] [grey]{Markup.Escape(exits)}[/]");
    }

    /// <summary>
    /// Maps BiomeType to Spectre.Console color name.
    /// Delegates to RoomViewHelper for consistency.
    /// </summary>
    /// <param name="biome">The biome type to map.</param>
    /// <returns>A Spectre.Console color name string.</returns>
    public static string GetBiomeColor(BiomeType biome)
        => RoomViewHelper.GetBiomeColor(biome);

    /// <summary>
    /// Formats an object name with appropriate color based on type.
    /// Delegates to RoomViewHelper for consistency.
    /// </summary>
    /// <param name="name">The object's display name.</param>
    /// <param name="isContainer">Whether the object is a container.</param>
    /// <param name="isLocked">Whether the container is locked.</param>
    /// <returns>A markup-formatted string with appropriate coloring.</returns>
    public static string FormatObjectName(string name, bool isContainer, bool isLocked)
        => RoomViewHelper.FormatObjectName(name, isContainer, isLocked);

    /// <summary>
    /// Formats an enemy name with health status.
    /// Delegates to RoomViewHelper for consistency.
    /// </summary>
    /// <param name="name">The enemy's display name.</param>
    /// <param name="currentHp">The enemy's current hit points.</param>
    /// <param name="maxHp">The enemy's maximum hit points.</param>
    /// <returns>A markup-formatted string with name and narrative health.</returns>
    public static string FormatEnemyName(string name, int currentHp, int maxHp)
        => RoomViewHelper.FormatEnemyName(name, currentHp, maxHp);

    /// <summary>
    /// Converts HP percentage to narrative health status.
    /// Delegates to RoomViewHelper for consistency.
    /// </summary>
    /// <param name="percent">Health percentage (0-100).</param>
    /// <returns>A narrative health descriptor.</returns>
    public static string GetNarrativeHealth(double percent)
        => RoomViewHelper.GetNarrativeHealth(percent);
}
