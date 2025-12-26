using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for exporting explored maps as ASCII text files.
/// Generates normalized grid representations of visited rooms across depth levels.
/// </summary>
/// <remarks>See: SPEC-MAP-001 for Cartographer II (Map Export) design (v0.3.20b).</remarks>
public class MapExportService : IMapExportService
{
    private readonly ILogger<MapExportService> _logger;
    private readonly IRoomRepository _roomRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapExportService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="roomRepository">The room repository for batch fetching.</param>
    public MapExportService(
        ILogger<MapExportService> logger,
        IRoomRepository roomRepository)
    {
        _logger = logger;
        _roomRepository = roomRepository;
    }

    /// <inheritdoc/>
    public async Task<string> ExportMapAsync(
        string characterName,
        Coordinate playerPosition,
        HashSet<Guid> visitedRoomIds,
        Dictionary<Guid, string> userNotes)
    {
        _logger.LogInformation(
            "[MapExport] Starting export for {CharacterName} with {RoomCount} visited rooms",
            characterName, visitedRoomIds.Count);

        // Fetch all visited rooms in a single batch query
        var rooms = await _roomRepository.GetBatchAsync(visitedRoomIds);
        var roomList = rooms.ToList();

        _logger.LogDebug("[MapExport] Retrieved {Count} rooms from repository", roomList.Count);

        // Generate the ASCII content
        var content = GenerateMapContent(characterName, playerPosition, roomList, userNotes);

        // Determine export path (save in user's documents folder)
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var exportDir = Path.Combine(documentsPath, "RuneAndRust", "MapExports");
        Directory.CreateDirectory(exportDir);

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var safeCharacterName = SanitizeFileName(characterName);
        var fileName = $"atlas_{safeCharacterName}_{timestamp}.txt";
        var filePath = Path.Combine(exportDir, fileName);

        // Write to file
        await File.WriteAllTextAsync(filePath, content);

        _logger.LogInformation("[MapExport] Map exported successfully to {FilePath}", filePath);

        return filePath;
    }

    /// <inheritdoc/>
    public string GenerateMapContent(
        string characterName,
        Coordinate playerPosition,
        IEnumerable<Room> rooms,
        Dictionary<Guid, string> userNotes)
    {
        _logger.LogDebug("[MapExport] Generating map content for {CharacterName}", characterName);

        var roomList = rooms.ToList();
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("╔══════════════════════════════════════════════════════════════════╗");
        sb.AppendLine($"║  ATLAS EXPORT - {characterName,-48} ║");
        sb.AppendLine($"║  Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss,-55} ║");
        sb.AppendLine($"║  Rooms Explored: {roomList.Count,-46} ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        if (roomList.Count == 0)
        {
            _logger.LogDebug("[MapExport] No rooms to export");
            sb.AppendLine("No rooms have been explored yet.");
            return sb.ToString();
        }

        // Group rooms by Z-level (depth)
        var roomsByDepth = roomList
            .GroupBy(r => r.PositionZ)
            .OrderBy(g => g.Key)
            .ToList();

        _logger.LogDebug("[MapExport] Processing {DepthCount} depth levels", roomsByDepth.Count);

        foreach (var depthGroup in roomsByDepth)
        {
            var depth = depthGroup.Key;
            var depthRooms = depthGroup.ToList();

            sb.AppendLine($"[DEPTH Z: {depth}]");
            sb.AppendLine(new string('-', 40));

            // Calculate grid bounds for this depth
            var minX = depthRooms.Min(r => r.PositionX);
            var maxX = depthRooms.Max(r => r.PositionX);
            var minY = depthRooms.Min(r => r.PositionY);
            var maxY = depthRooms.Max(r => r.PositionY);

            _logger.LogTrace(
                "[MapExport] Depth {Depth}: X=[{MinX},{MaxX}], Y=[{MinY},{MaxY}]",
                depth, minX, maxX, minY, maxY);

            // Create lookup for O(1) room access
            var roomLookup = depthRooms.ToDictionary(
                r => (r.PositionX, r.PositionY),
                r => r);

            // Generate grid (Y decreases top to bottom for proper map orientation)
            for (var y = maxY; y >= minY; y--)
            {
                var rowBuilder = new StringBuilder("  ");

                for (var x = minX; x <= maxX; x++)
                {
                    var hasRoom = roomLookup.TryGetValue((x, y), out var room);
                    var isPlayerPosition = hasRoom &&
                        playerPosition.X == x &&
                        playerPosition.Y == y &&
                        playerPosition.Z == depth;
                    var hasNote = hasRoom && userNotes.ContainsKey(room!.Id);

                    var symbol = ResolveSymbol(hasRoom ? room : null, isPlayerPosition, hasNote);
                    rowBuilder.Append(symbol);
                    rowBuilder.Append(' ');
                }

                sb.AppendLine(rowBuilder.ToString().TrimEnd());
            }

            sb.AppendLine();
        }

        // Append notes section if any notes exist for visited rooms
        var notesInExport = roomList
            .Where(r => userNotes.ContainsKey(r.Id))
            .OrderBy(r => r.PositionZ)
            .ThenBy(r => r.PositionY)
            .ThenBy(r => r.PositionX)
            .ToList();

        if (notesInExport.Count > 0)
        {
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("NOTES:");
            sb.AppendLine();

            foreach (var room in notesInExport)
            {
                var note = userNotes[room.Id];
                sb.AppendLine($"  [{room.PositionX},{room.PositionY},{room.PositionZ}] {room.Name}:");
                sb.AppendLine($"    \"{note}\"");
                sb.AppendLine();
            }
        }

        // Legend
        sb.AppendLine("═══════════════════════════════════════════════════════════════════");
        sb.AppendLine("LEGEND:");
        sb.AppendLine("  @  : You are here");
        sb.AppendLine("  !  : Note attached");
        sb.AppendLine("  X  : Boss Lair");
        sb.AppendLine("  $  : Settlement");
        sb.AppendLine("  *  : Runic Anchor");
        sb.AppendLine("  ^  : Stairs Up");
        sb.AppendLine("  v  : Stairs Down");
        sb.AppendLine("  +  : Workbench/Alchemy");
        sb.AppendLine("  #  : Explored Room");
        sb.AppendLine("  .  : Unexplored/Void");
        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════════════════════════");
        sb.AppendLine("Generated by Rune & Rust - The Atlas (v0.3.20b)");

        _logger.LogDebug("[MapExport] Content generation complete");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public char ResolveSymbol(Room? room, bool isPlayerPosition, bool hasNote)
    {
        // Empty cell
        if (room == null)
        {
            return '.';
        }

        // Priority 1: Player position
        if (isPlayerPosition)
        {
            return '@';
        }

        // Priority 2: User note
        if (hasNote)
        {
            return '!';
        }

        // Priority 3-8: Room features
        if (room.HasFeature(RoomFeature.BossLair))
        {
            return 'X';
        }

        if (room.HasFeature(RoomFeature.Settlement))
        {
            return '$';
        }

        if (room.HasFeature(RoomFeature.RunicAnchor))
        {
            return '*';
        }

        if (room.HasFeature(RoomFeature.StairsUp))
        {
            return '^';
        }

        if (room.HasFeature(RoomFeature.StairsDown))
        {
            return 'v';
        }

        if (room.HasFeature(RoomFeature.Workbench) || room.HasFeature(RoomFeature.AlchemyTable))
        {
            return '+';
        }

        // Default: Standard explored room
        return '#';
    }

    /// <summary>
    /// Sanitizes a file name by removing invalid characters.
    /// </summary>
    /// <param name="name">The name to sanitize.</param>
    /// <returns>A safe file name string.</returns>
    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder();

        foreach (var c in name)
        {
            if (!invalidChars.Contains(c))
            {
                sanitized.Append(c);
            }
            else
            {
                sanitized.Append('_');
            }
        }

        return sanitized.ToString();
    }
}
