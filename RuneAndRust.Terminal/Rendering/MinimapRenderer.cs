using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders a 3x3 minimap grid centered on the player position (v0.3.5b).
/// Displays room symbols based on features, Fog of War for unvisited rooms,
/// and Z-level context indicators.
/// </summary>
public static class MinimapRenderer
{
    private const int Radius = 1;  // 3x3 grid (center ± 1)

    /// <summary>
    /// Renders the minimap panel for the exploration HUD sidebar.
    /// </summary>
    /// <param name="center">The player's current position (grid center).</param>
    /// <param name="localMap">List of rooms within the grid bounds.</param>
    /// <param name="visited">Set of room IDs the player has visited.</param>
    /// <returns>A Panel containing the minimap grid.</returns>
    public static Panel Render(Coordinate center, List<Room> localMap, HashSet<Guid> visited)
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

                rowItems.Add(ResolveTile(room, center, cellPos, visited));
            }

            grid.AddRow(rowItems.ToArray());
        }

        // Add Z-level indicator row
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
    /// Resolves the symbol and color for a minimap tile.
    /// </summary>
    /// <param name="room">The room at this position (may be null).</param>
    /// <param name="center">The player's current position.</param>
    /// <param name="cellPos">The coordinate of this cell.</param>
    /// <param name="visited">Set of room IDs the player has visited.</param>
    /// <returns>Markup with the appropriate symbol and color.</returns>
    public static Markup ResolveTile(Room? room, Coordinate center, Coordinate cellPos, HashSet<Guid> visited)
    {
        // Player position - always visible
        if (cellPos.X == center.X && cellPos.Y == center.Y && cellPos.Z == center.Z)
        {
            return new Markup("[cyan]@[/]");
        }

        // No room at this position
        if (room == null)
        {
            return new Markup("[grey]·[/]");  // Empty/void
        }

        // Fog of War - room exists but not visited
        if (!visited.Contains(room.Id))
        {
            return new Markup("[grey]░[/]");  // Fog
        }

        // Visited room - resolve by feature priority
        var (symbol, color) = GetRoomSymbol(room);
        return new Markup($"[{color}]{symbol}[/]");
    }

    /// <summary>
    /// Gets the symbol and color for a visited room based on its features.
    /// Priority order: Boss > Stairs > Settlement > Anchor > Workstation > Standard
    /// </summary>
    /// <param name="room">The room to get a symbol for.</param>
    /// <returns>A tuple of (Symbol, Color) for Spectre.Console markup.</returns>
    public static (string Symbol, string Color) GetRoomSymbol(Room room)
    {
        // Priority order: Boss > Stairs > Settlement > Anchor > Workstation > Standard

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

        // Danger level coloring for standard rooms
        return room.DangerLevel switch
        {
            DangerLevel.Lethal => ("O", "red"),
            DangerLevel.Hostile => ("O", "orange1"),
            DangerLevel.Unstable => ("O", "yellow"),
            _ => ("O", "grey")  // Safe
        };
    }

    /// <summary>
    /// Renders Z-level context indicator showing available vertical movement.
    /// </summary>
    /// <param name="center">The player's current position.</param>
    /// <param name="localMap">List of rooms within the grid bounds.</param>
    /// <returns>Markup string for the Z-level indicator.</returns>
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
