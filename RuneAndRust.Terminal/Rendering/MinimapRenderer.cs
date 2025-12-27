using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders a 3x3 minimap grid centered on the player position (v0.3.9b).
/// Displays room symbols based on features, Fog of War for unvisited rooms,
/// and Z-level context indicators. Updated with theme support for accessibility.
/// </summary>
public static class MinimapRenderer
{
    private const int Radius = 1;  // 3x3 grid (center ± 1)

    /// <summary>
    /// Renders the minimap panel with theme support.
    /// </summary>
    /// <param name="center">The player's current position (grid center).</param>
    /// <param name="localMap">List of rooms within the grid bounds.</param>
    /// <param name="visited">Set of room IDs the player has visited.</param>
    /// <param name="userNotes">User-defined room notes for annotation display (v0.3.20a).</param>
    /// <param name="themeService">The theme service for color lookups.</param>
    /// <returns>A Panel containing the minimap grid.</returns>
    public static Panel Render(Coordinate center, List<Room> localMap, HashSet<Guid> visited, Dictionary<Guid, string> userNotes, IThemeService themeService)
    {
        var grid = new Grid();

        // Add 3 columns for the 3x3 grid
        for (int i = 0; i < (Radius * 2) + 1; i++)
        {
            grid.AddColumn(new GridColumn().Width(3).NoWrap());
        }

        // Iterate Y from top to bottom (Y+ at top, Y- at bottom)
        for (int y = center.Y + Radius; y >= center.Y - Radius; y--)
        {
            var rowItems = new List<IRenderable>();

            for (int x = center.X - Radius; x <= center.X + Radius; x++)
            {
                var cellPos = new Coordinate(x, y, center.Z);
                var room = localMap.FirstOrDefault(r =>
                    r.Position.X == x && r.Position.Y == y && r.Position.Z == center.Z);

                rowItems.Add(ResolveTile(room, center, cellPos, visited, userNotes, themeService));
            }

            grid.AddRow(rowItems.ToArray());
        }

        // Add Z-level indicator row
        var zIndicator = GetZLevelIndicator(center, localMap, themeService);
        var zRow = new List<IRenderable>
        {
            new Markup(zIndicator),
            new Text(""),
            new Text("")
        };
        grid.AddRow(zRow.ToArray());

        var headerColor = themeService.GetColor("HeaderColor");
        return new Panel(grid)
            .Header($"[{headerColor}]Sector Map[/]")
            .Border(BoxBorder.Rounded);
    }

    /// <summary>
    /// Renders the minimap panel (legacy, non-themed).
    /// </summary>
    /// <param name="center">The player's current position (grid center).</param>
    /// <param name="localMap">List of rooms within the grid bounds.</param>
    /// <param name="visited">Set of room IDs the player has visited.</param>
    /// <param name="userNotes">User-defined room notes for annotation display (v0.3.20a).</param>
    public static Panel Render(Coordinate center, List<Room> localMap, HashSet<Guid> visited, Dictionary<Guid, string> userNotes)
    {
        var grid = new Grid();

        for (int i = 0; i < (Radius * 2) + 1; i++)
        {
            grid.AddColumn(new GridColumn().Width(3).NoWrap());
        }

        for (int y = center.Y + Radius; y >= center.Y - Radius; y--)
        {
            var rowItems = new List<IRenderable>();

            for (int x = center.X - Radius; x <= center.X + Radius; x++)
            {
                var cellPos = new Coordinate(x, y, center.Z);
                var room = localMap.FirstOrDefault(r =>
                    r.Position.X == x && r.Position.Y == y && r.Position.Z == center.Z);

                rowItems.Add(ResolveTile(room, center, cellPos, visited, userNotes));
            }

            grid.AddRow(rowItems.ToArray());
        }

        var zIndicator = GetZLevelIndicator(center, localMap);
        var zRow = new List<IRenderable>
        {
            new Markup(zIndicator),
            new Text(""),
            new Text("")
        };
        grid.AddRow(zRow.ToArray());

        return new Panel(grid)
            .Header("[yellow]Sector Map[/]")
            .Border(BoxBorder.Rounded);
    }

    /// <summary>
    /// Resolves the symbol and color for a minimap tile with theme support.
    /// </summary>
    /// <param name="room">The room at this position (null if empty).</param>
    /// <param name="center">The player's current position.</param>
    /// <param name="cellPos">The cell position being resolved.</param>
    /// <param name="visited">Set of visited room IDs.</param>
    /// <param name="userNotes">User-defined room notes (v0.3.20a).</param>
    /// <param name="themeService">The theme service for color lookups.</param>
    public static Markup ResolveTile(Room? room, Coordinate center, Coordinate cellPos, HashSet<Guid> visited, Dictionary<Guid, string> userNotes, IThemeService themeService)
    {
        var playerColor = themeService.GetColor("PlayerColor");
        var neutralColor = themeService.GetColor("NeutralColor");

        // Player position - always visible
        if (cellPos.X == center.X && cellPos.Y == center.Y && cellPos.Z == center.Z)
        {
            return new Markup($"[{playerColor}]@[/]");
        }

        // No room at this position
        if (room == null)
        {
            return new Markup($"[{neutralColor}]·[/]");  // Empty/void
        }

        // Fog of War - room exists but not visited
        if (!visited.Contains(room.Id))
        {
            return new Markup($"[{neutralColor}]░[/]");  // Fog
        }

        // User annotation - show yellow ! for rooms with notes (v0.3.20a)
        if (userNotes.ContainsKey(room.Id))
        {
            return new Markup($"[{themeService.GetColor("WarningColor")}]![/]");
        }

        // Visited room - resolve by feature priority
        var (symbol, color) = GetRoomSymbol(room, themeService);
        return new Markup($"[{color}]{symbol}[/]");
    }

    /// <summary>
    /// Resolves the symbol and color for a minimap tile (legacy, non-themed).
    /// </summary>
    /// <param name="room">The room at this position (null if empty).</param>
    /// <param name="center">The player's current position.</param>
    /// <param name="cellPos">The cell position being resolved.</param>
    /// <param name="visited">Set of visited room IDs.</param>
    /// <param name="userNotes">User-defined room notes (v0.3.20a).</param>
    public static Markup ResolveTile(Room? room, Coordinate center, Coordinate cellPos, HashSet<Guid> visited, Dictionary<Guid, string> userNotes)
    {
        if (cellPos.X == center.X && cellPos.Y == center.Y && cellPos.Z == center.Z)
        {
            return new Markup("[cyan]@[/]");
        }

        if (room == null)
        {
            return new Markup("[grey]·[/]");
        }

        if (!visited.Contains(room.Id))
        {
            return new Markup("[grey]░[/]");
        }

        // User annotation - show yellow ! for rooms with notes (v0.3.20a)
        if (userNotes.ContainsKey(room.Id))
        {
            return new Markup("[yellow]![/]");
        }

        var (symbol, color) = GetRoomSymbol(room);
        return new Markup($"[{color}]{symbol}[/]");
    }

    /// <summary>
    /// Gets the symbol and color for a visited room with theme support.
    /// </summary>
    public static (string Symbol, string Color) GetRoomSymbol(Room room, IThemeService themeService)
    {
        var enemyColor = themeService.GetColor("EnemyColor");
        var playerColor = themeService.GetColor("PlayerColor");
        var warningColor = themeService.GetColor("WarningColor");
        var infoColor = themeService.GetColor("InfoColor");

        // Priority order: Boss > Stairs > Settlement > Anchor > Workstation > Standard

        if (room.HasFeature(RoomFeature.BossLair))
            return ("☠", enemyColor);

        if (room.HasFeature(RoomFeature.StairsUp))
            return ("▲", themeService.GetColor("QualityCommon"));

        if (room.HasFeature(RoomFeature.StairsDown))
            return ("▼", themeService.GetColor("QualityCommon"));

        if (room.HasFeature(RoomFeature.Settlement))
            return ("⌂", infoColor);

        if (room.HasFeature(RoomFeature.RunicAnchor))
            return ("◆", playerColor);

        if (room.HasFeature(RoomFeature.Workbench) || room.HasFeature(RoomFeature.AlchemyTable))
            return ("⚒", warningColor);

        // Danger level coloring for standard rooms
        return room.DangerLevel switch
        {
            DangerLevel.Lethal => ("O", themeService.GetColor("DangerLethal")),
            DangerLevel.Hostile => ("O", themeService.GetColor("DangerHostile")),
            DangerLevel.Unstable => ("O", themeService.GetColor("DangerUnstable")),
            _ => ("O", themeService.GetColor("DangerSafe"))
        };
    }

    /// <summary>
    /// Gets the symbol and color for a visited room (legacy, non-themed).
    /// </summary>
    public static (string Symbol, string Color) GetRoomSymbol(Room room)
    {
        if (room.HasFeature(RoomFeature.BossLair))
            return ("☠", "red");

        if (room.HasFeature(RoomFeature.StairsUp))
            return ("▲", "white");

        if (room.HasFeature(RoomFeature.StairsDown))
            return ("▼", "white");

        if (room.HasFeature(RoomFeature.Settlement))
            return ("⌂", "blue");

        if (room.HasFeature(RoomFeature.RunicAnchor))
            return ("◆", "cyan");

        if (room.HasFeature(RoomFeature.Workbench) || room.HasFeature(RoomFeature.AlchemyTable))
            return ("⚒", "yellow");

        return room.DangerLevel switch
        {
            DangerLevel.Lethal => ("O", "red"),
            DangerLevel.Hostile => ("O", "orange1"),
            DangerLevel.Unstable => ("O", "yellow"),
            _ => ("O", "grey")
        };
    }

    /// <summary>
    /// Renders Z-level context indicator with theme support.
    /// </summary>
    public static string GetZLevelIndicator(Coordinate center, List<Room> localMap, IThemeService themeService)
    {
        var hasUp = localMap.Any(r => r.HasFeature(RoomFeature.StairsUp));
        var hasDown = localMap.Any(r => r.HasFeature(RoomFeature.StairsDown));
        var neutralColor = themeService.GetColor("NeutralColor");

        return (hasUp, hasDown) switch
        {
            (true, true) => $"[{neutralColor}]Z:{center.Z} ▲▼[/]",
            (true, false) => $"[{neutralColor}]Z:{center.Z} ▲[/]",
            (false, true) => $"[{neutralColor}]Z:{center.Z} ▼[/]",
            _ => $"[{neutralColor}]Z:{center.Z}[/]"
        };
    }

    /// <summary>
    /// Renders Z-level context indicator (legacy, non-themed).
    /// </summary>
    public static string GetZLevelIndicator(Coordinate center, List<Room> localMap)
    {
        var hasUp = localMap.Any(r => r.HasFeature(RoomFeature.StairsUp));
        var hasDown = localMap.Any(r => r.HasFeature(RoomFeature.StairsDown));

        return (hasUp, hasDown) switch
        {
            (true, true) => $"[grey]Z:{center.Z} ▲▼[/]",
            (true, false) => $"[grey]Z:{center.Z} ▲[/]",
            (false, true) => $"[grey]Z:{center.Z} ▼[/]",
            _ => $"[grey]Z:{center.Z}[/]"
        };
    }
}
