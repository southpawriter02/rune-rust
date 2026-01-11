using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for rendering ASCII dungeon maps.
/// </summary>
public class MapRendererService : IMapRendererService
{
    private readonly ILogger<MapRendererService> _logger;

    /// <summary>
    /// Character used for the player position.
    /// </summary>
    public const char PlayerSymbol = '@';

    /// <summary>
    /// Character used for unexplored rooms.
    /// </summary>
    public const char UnexploredSymbol = '?';

    /// <summary>
    /// Horizontal connection character.
    /// </summary>
    public const char HorizontalConnection = '─';

    /// <summary>
    /// Vertical connection character.
    /// </summary>
    public const char VerticalConnection = '│';

    /// <summary>
    /// Stairs up indicator.
    /// </summary>
    public const char StairsUpSymbol = '↑';

    /// <summary>
    /// Stairs down indicator.
    /// </summary>
    public const char StairsDownSymbol = '↓';

    /// <summary>
    /// Ladder indicator.
    /// </summary>
    public const char LadderSymbol = '≡';

    /// <summary>
    /// Creates a new MapRendererService.
    /// </summary>
    public MapRendererService(ILogger<MapRendererService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string RenderLevel(Dungeon dungeon, int z, Position3D playerPosition)
    {
        ArgumentNullException.ThrowIfNull(dungeon);

        var rooms = dungeon.GetRoomsOnLevel(z);
        if (rooms.Count == 0)
        {
            return $"No rooms found on level {z}.";
        }

        var (minX, maxX, minY, maxY) = dungeon.GetLevelBounds(z);

        // Add padding for connections
        var gridWidth = (maxX - minX + 1) * 4 + 1;
        var gridHeight = (maxY - minY + 1) * 2 + 1;

        var grid = new char[gridHeight, gridWidth];

        // Initialize with spaces
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[y, x] = ' ';
            }
        }

        // Place rooms and connections
        foreach (var room in rooms)
        {
            var gridX = (room.Position.X - minX) * 4 + 2;
            var gridY = (maxY - room.Position.Y) * 2 + 1; // Invert Y for display

            var isPlayerHere = room.Position.Equals(playerPosition);
            grid[gridY, gridX] = GetRoomSymbol(room, isPlayerHere);

            // Draw connections
            DrawConnections(grid, room, gridX, gridY);
        }

        // Build output
        var sb = new StringBuilder();
        sb.AppendLine($"=== {dungeon.Name} - Level {z + 1} (Z={z}) ===");
        sb.AppendLine();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                sb.Append(grid[y, x]);
            }
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine(GetLegend());

        // Add statistics
        var stats = dungeon.GetExplorationStatsForLevel(z);
        var visited = stats.GetValueOrDefault(ExplorationState.Visited, 0) +
                      stats.GetValueOrDefault(ExplorationState.Cleared, 0);
        var total = rooms.Count;
        sb.AppendLine();
        sb.AppendLine($"Rooms explored: {visited}/{total} on this level");

        _logger.LogDebug("Rendered map for level {Z} with {Rooms} rooms", z, rooms.Count);

        return sb.ToString();
    }

    /// <inheritdoc />
    public string RenderAllLevels(Dungeon dungeon, Position3D playerPosition)
    {
        ArgumentNullException.ThrowIfNull(dungeon);

        var levels = dungeon.GetExploredLevels();
        if (levels.Count == 0)
        {
            return "No explored levels to display.";
        }

        var sb = new StringBuilder();

        foreach (var z in levels)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine(RenderLevel(dungeon, z, playerPosition));
        }

        // Summary
        sb.AppendLine("─".PadRight(40, '─'));
        sb.AppendLine($"Current position: Level {playerPosition.Z + 1}, ({playerPosition.X}, {playerPosition.Y})");

        var totalVisited = dungeon.Rooms.Values.Count(r => r.IsVisited);
        sb.AppendLine($"Total rooms explored: {totalVisited}");

        return sb.ToString();
    }

    /// <inheritdoc />
    public char GetRoomSymbol(Room room, bool isPlayerHere)
    {
        ArgumentNullException.ThrowIfNull(room);

        if (isPlayerHere)
            return PlayerSymbol;

        if (!room.IsVisited)
            return UnexploredSymbol;

        return room.RoomType switch
        {
            RoomType.Standard => '#',
            RoomType.Treasure => '$',
            RoomType.Trap => '!',
            RoomType.Boss => 'B',
            RoomType.Safe => '+',
            RoomType.Shrine => '*',
            _ => '#'
        };
    }

    /// <inheritdoc />
    public string GetLegend()
    {
        return "Legend: @ You  # Room  $ Treasure  ! Trap  B Boss  + Safe  * Shrine  ? Unexplored\n" +
               "        ↑↓ Stairs  ≡ Ladder  ─│ Passages";
    }

    /// <inheritdoc />
    public char GetConnectionSymbol(Direction direction, bool isHidden)
    {
        if (isHidden)
            return ' '; // Hidden connections not shown

        return direction switch
        {
            Direction.North or Direction.South => VerticalConnection,
            Direction.East or Direction.West => HorizontalConnection,
            Direction.Up => StairsUpSymbol,
            Direction.Down => StairsDownSymbol,
            _ => ' '
        };
    }

    private void DrawConnections(char[,] grid, Room room, int gridX, int gridY)
    {
        foreach (var (direction, exit) in room.GetVisibleExits())
        {
            // Only draw connections to visible exits
            if (!exit.IsVisible) continue;

            switch (direction)
            {
                case Direction.North:
                    if (gridY > 0)
                        grid[gridY - 1, gridX] = VerticalConnection;
                    break;

                case Direction.South:
                    if (gridY < grid.GetLength(0) - 1)
                        grid[gridY + 1, gridX] = VerticalConnection;
                    break;

                case Direction.East:
                    if (gridX < grid.GetLength(1) - 3)
                    {
                        grid[gridY, gridX + 1] = HorizontalConnection;
                        grid[gridY, gridX + 2] = HorizontalConnection;
                        grid[gridY, gridX + 3] = HorizontalConnection;
                    }
                    break;

                case Direction.West:
                    if (gridX >= 3)
                    {
                        grid[gridY, gridX - 1] = HorizontalConnection;
                        grid[gridY, gridX - 2] = HorizontalConnection;
                        grid[gridY, gridX - 3] = HorizontalConnection;
                    }
                    break;

                case Direction.Up:
                    // Mark room has stairs up
                    if (gridY > 0)
                        grid[gridY - 1, gridX] = StairsUpSymbol;
                    break;

                case Direction.Down:
                    // Mark room has stairs down
                    if (gridY < grid.GetLength(0) - 1)
                        grid[gridY + 1, gridX] = StairsDownSymbol;
                    break;
            }
        }
    }
}
