using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using Serilog;

namespace RuneAndRust.Engine.Spatial;

/// <summary>
/// v0.39.1: Validates spatial coherence of 3D dungeon layouts
/// Checks for overlaps, reachability, layer bounds, and connection validity
/// </summary>
public interface ISpatialValidationService
{
    List<ValidationIssue> ValidateSector(
        Dictionary<string, RoomPosition> positions,
        List<VerticalConnection> connections,
        DungeonGraph graph);

    bool IsReachableFromOrigin(
        string roomId,
        Dictionary<string, RoomPosition> positions,
        List<VerticalConnection> connections,
        DungeonGraph graph);

    void LogValidationIssues(int sectorId, List<ValidationIssue> issues);
}

public class SpatialValidationService : ISpatialValidationService
{
    private readonly ILogger _logger;

    public SpatialValidationService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Performs comprehensive spatial validation on a sector
    /// </summary>
    public List<ValidationIssue> ValidateSector(
        Dictionary<string, RoomPosition> positions,
        List<VerticalConnection> connections,
        DungeonGraph graph)
    {
        var issues = new List<ValidationIssue>();

        _logger.Information(
            "Starting spatial validation: {RoomCount} rooms, {ConnectionCount} vertical connections",
            positions.Count,
            connections.Count);

        // Validation 1: Check for room overlaps
        issues.AddRange(CheckForOverlaps(positions));

        // Validation 2: Check all rooms reachable from origin
        issues.AddRange(CheckReachability(positions, connections, graph));

        // Validation 3: Validate vertical connections have valid targets
        issues.AddRange(CheckVerticalConnections(positions, connections));

        // Validation 4: Check rooms don't exceed layer bounds (-3 to +3)
        issues.AddRange(CheckLayerBounds(positions));

        // Validation 5: Check entry hall is at origin
        issues.AddRange(CheckOriginPlacement(positions, graph));

        var criticalCount = issues.Count(i => i.Severity == "Critical");
        var errorCount = issues.Count(i => i.Severity == "Error");
        var warningCount = issues.Count(i => i.Severity == "Warning");

        _logger.Information(
            "Spatial validation complete: {Total} issues ({Critical} critical, {Error} errors, {Warning} warnings)",
            issues.Count,
            criticalCount,
            errorCount,
            warningCount);

        return issues;
    }

    #region Validation Checks

    /// <summary>
    /// Checks for rooms occupying the same spatial position
    /// </summary>
    private List<ValidationIssue> CheckForOverlaps(Dictionary<string, RoomPosition> positions)
    {
        var issues = new List<ValidationIssue>();
        var occupiedPositions = new Dictionary<RoomPosition, string>();

        foreach (var kvp in positions)
        {
            var roomId = kvp.Key;
            var position = kvp.Value;

            // For now, assuming all rooms are 1x1x1 footprint
            // TODO: Expand for larger rooms (Medium 2x2x1, Large 3x3x1)

            if (occupiedPositions.ContainsKey(position))
            {
                issues.Add(new ValidationIssue
                {
                    Type = "Overlap",
                    Severity = "Critical",
                    Description = $"Room {roomId} overlaps with {occupiedPositions[position]} at position {position}",
                    AffectedRoomIds = new List<string> { roomId, occupiedPositions[position] },
                    Position = position
                });

                _logger.Error(
                    "Room overlap detected: {Room1} and {Room2} at {Position}",
                    roomId,
                    occupiedPositions[position],
                    position);
            }
            else
            {
                occupiedPositions[position] = roomId;
            }
        }

        if (issues.Count == 0)
        {
            _logger.Debug("Overlap check passed: No overlapping rooms found");
        }

        return issues;
    }

    /// <summary>
    /// Checks that all rooms are reachable from the entry hall
    /// </summary>
    private List<ValidationIssue> CheckReachability(
        Dictionary<string, RoomPosition> positions,
        List<VerticalConnection> connections,
        DungeonGraph graph)
    {
        var issues = new List<ValidationIssue>();

        // Find entry hall
        var startNode = graph.StartNode;
        if (startNode == null)
        {
            issues.Add(new ValidationIssue
            {
                Type = "MissingConnection",
                Severity = "Critical",
                Description = "No entry hall (start node) found in graph",
                AffectedRoomIds = new List<string>()
            });
            return issues;
        }

        var entryHallId = startNode.Id.ToString();

        // BFS to find all reachable rooms
        var reachable = new HashSet<string>();
        var queue = new Queue<string>();

        queue.Enqueue(entryHallId);
        reachable.Add(entryHallId);

        while (queue.Count > 0)
        {
            var currentRoomId = queue.Dequeue();

            // Check horizontal connections (from graph edges)
            var currentNode = graph.GetNodes().FirstOrDefault(n => n.Id.ToString() == currentRoomId);
            if (currentNode != null)
            {
                var edges = graph.GetEdgesFrom(currentNode);
                foreach (var edge in edges)
                {
                    var nextRoomId = edge.To.Id.ToString();
                    if (!reachable.Contains(nextRoomId))
                    {
                        reachable.Add(nextRoomId);
                        queue.Enqueue(nextRoomId);
                    }
                }
            }

            // Check vertical connections (unblocked only)
            var verticalConnections = connections
                .Where(c => !c.IsBlocked &&
                            ((c.FromRoomId == currentRoomId) ||
                             (c.IsBidirectional && c.ToRoomId == currentRoomId)))
                .ToList();

            foreach (var connection in verticalConnections)
            {
                var nextRoomId = connection.FromRoomId == currentRoomId
                    ? connection.ToRoomId
                    : connection.FromRoomId;

                if (!reachable.Contains(nextRoomId))
                {
                    reachable.Add(nextRoomId);
                    queue.Enqueue(nextRoomId);
                }
            }
        }

        // Find unreachable rooms
        var unreachableRooms = positions.Keys
            .Where(roomId => !reachable.Contains(roomId))
            .ToList();

        foreach (var roomId in unreachableRooms)
        {
            issues.Add(new ValidationIssue
            {
                Type = "Unreachable",
                Severity = "Error",
                Description = $"Room {roomId} is not reachable from entry hall",
                AffectedRoomIds = new List<string> { roomId },
                Position = positions[roomId]
            });

            _logger.Warning("Unreachable room detected: {RoomId} at {Position}",
                roomId, positions[roomId]);
        }

        if (unreachableRooms.Count == 0)
        {
            _logger.Debug("Reachability check passed: All {RoomCount} rooms reachable", positions.Count);
        }

        return issues;
    }

    /// <summary>
    /// Validates that vertical connections reference valid rooms
    /// </summary>
    private List<ValidationIssue> CheckVerticalConnections(
        Dictionary<string, RoomPosition> positions,
        List<VerticalConnection> connections)
    {
        var issues = new List<ValidationIssue>();

        foreach (var connection in connections)
        {
            // Check FROM room exists
            if (!positions.ContainsKey(connection.FromRoomId))
            {
                issues.Add(new ValidationIssue
                {
                    Type = "MissingConnection",
                    Severity = "Critical",
                    Description = $"Vertical connection {connection.ConnectionId} references non-existent room: {connection.FromRoomId}",
                    AffectedRoomIds = new List<string> { connection.FromRoomId }
                });

                _logger.Error(
                    "Invalid vertical connection: FROM room {RoomId} does not exist",
                    connection.FromRoomId);
            }

            // Check TO room exists
            if (!positions.ContainsKey(connection.ToRoomId))
            {
                issues.Add(new ValidationIssue
                {
                    Type = "MissingConnection",
                    Severity = "Critical",
                    Description = $"Vertical connection {connection.ConnectionId} references non-existent room: {connection.ToRoomId}",
                    AffectedRoomIds = new List<string> { connection.ToRoomId }
                });

                _logger.Error(
                    "Invalid vertical connection: TO room {RoomId} does not exist",
                    connection.ToRoomId);
            }

            // Check that connection actually connects rooms at different Z levels
            if (positions.ContainsKey(connection.FromRoomId) &&
                positions.ContainsKey(connection.ToRoomId))
            {
                var fromPos = positions[connection.FromRoomId];
                var toPos = positions[connection.ToRoomId];

                if (fromPos.Z == toPos.Z)
                {
                    issues.Add(new ValidationIssue
                    {
                        Type = "InvalidFootprint",
                        Severity = "Warning",
                        Description = $"Vertical connection {connection.ConnectionId} connects rooms at same Z level ({fromPos.Z})",
                        AffectedRoomIds = new List<string> { connection.FromRoomId, connection.ToRoomId },
                        Position = fromPos
                    });

                    _logger.Warning(
                        "Vertical connection on same Z level: {From} → {To} at Z={Z}",
                        connection.FromRoomId,
                        connection.ToRoomId,
                        fromPos.Z);
                }

                // Check that rooms are actually above/below each other
                if (!fromPos.IsDirectlyAboveOrBelow(toPos))
                {
                    issues.Add(new ValidationIssue
                    {
                        Type = "InvalidFootprint",
                        Severity = "Warning",
                        Description = $"Vertical connection {connection.ConnectionId} connects rooms not directly above/below: {fromPos} → {toPos}",
                        AffectedRoomIds = new List<string> { connection.FromRoomId, connection.ToRoomId },
                        Position = fromPos
                    });

                    _logger.Warning(
                        "Vertical connection not aligned: {From} at {FromPos} → {To} at {ToPos}",
                        connection.FromRoomId,
                        fromPos,
                        connection.ToRoomId,
                        toPos);
                }
            }
        }

        if (issues.Count == 0)
        {
            _logger.Debug("Vertical connection check passed: All {ConnectionCount} connections valid",
                connections.Count);
        }

        return issues;
    }

    /// <summary>
    /// Checks that no rooms exceed vertical layer bounds (-3 to +3)
    /// </summary>
    private List<ValidationIssue> CheckLayerBounds(Dictionary<string, RoomPosition> positions)
    {
        var issues = new List<ValidationIssue>();

        foreach (var kvp in positions)
        {
            var roomId = kvp.Key;
            var position = kvp.Value;

            if (position.Z < -3 || position.Z > 3)
            {
                issues.Add(new ValidationIssue
                {
                    Type = "LayerBounds",
                    Severity = "Critical",
                    Description = $"Room {roomId} exceeds layer bounds: Z={position.Z} (valid range: -3 to +3)",
                    AffectedRoomIds = new List<string> { roomId },
                    Position = position
                });

                _logger.Error(
                    "Room exceeds layer bounds: {RoomId} at Z={Z}",
                    roomId,
                    position.Z);
            }
        }

        if (issues.Count == 0)
        {
            _logger.Debug("Layer bounds check passed: All rooms within -3 to +3");
        }

        return issues;
    }

    /// <summary>
    /// Checks that entry hall is at origin (0, 0, 0)
    /// </summary>
    private List<ValidationIssue> CheckOriginPlacement(
        Dictionary<string, RoomPosition> positions,
        DungeonGraph graph)
    {
        var issues = new List<ValidationIssue>();

        var startNode = graph.StartNode;
        if (startNode == null)
        {
            // Already reported in reachability check
            return issues;
        }

        var entryHallId = startNode.Id.ToString();

        if (!positions.ContainsKey(entryHallId))
        {
            issues.Add(new ValidationIssue
            {
                Type = "MissingConnection",
                Severity = "Critical",
                Description = $"Entry hall {entryHallId} has no position assigned",
                AffectedRoomIds = new List<string> { entryHallId }
            });
            return issues;
        }

        var entryHallPosition = positions[entryHallId];

        if (entryHallPosition != RoomPosition.Origin)
        {
            issues.Add(new ValidationIssue
            {
                Type = "InvalidFootprint",
                Severity = "Error",
                Description = $"Entry hall is not at origin: Located at {entryHallPosition} instead of (0, 0, 0)",
                AffectedRoomIds = new List<string> { entryHallId },
                Position = entryHallPosition
            });

            _logger.Error(
                "Entry hall not at origin: {EntryHallId} at {Position}",
                entryHallId,
                entryHallPosition);
        }
        else
        {
            _logger.Debug("Origin placement check passed: Entry hall at (0, 0, 0)");
        }

        return issues;
    }

    #endregion

    /// <summary>
    /// Checks if a specific room is reachable from origin
    /// </summary>
    public bool IsReachableFromOrigin(
        string roomId,
        Dictionary<string, RoomPosition> positions,
        List<VerticalConnection> connections,
        DungeonGraph graph)
    {
        var startNode = graph.StartNode;
        if (startNode == null)
            return false;

        var entryHallId = startNode.Id.ToString();

        // BFS to find room
        var visited = new HashSet<string>();
        var queue = new Queue<string>();

        queue.Enqueue(entryHallId);
        visited.Add(entryHallId);

        while (queue.Count > 0)
        {
            var currentRoomId = queue.Dequeue();

            if (currentRoomId == roomId)
                return true;

            // Check graph edges
            var currentNode = graph.GetNodes().FirstOrDefault(n => n.Id.ToString() == currentRoomId);
            if (currentNode != null)
            {
                var edges = graph.GetEdgesFrom(currentNode);
                foreach (var edge in edges)
                {
                    var nextRoomId = edge.To.Id.ToString();
                    if (!visited.Contains(nextRoomId))
                    {
                        visited.Add(nextRoomId);
                        queue.Enqueue(nextRoomId);
                    }
                }
            }

            // Check vertical connections
            var verticalConnections = connections
                .Where(c => !c.IsBlocked &&
                            ((c.FromRoomId == currentRoomId) ||
                             (c.IsBidirectional && c.ToRoomId == currentRoomId)))
                .ToList();

            foreach (var connection in verticalConnections)
            {
                var nextRoomId = connection.FromRoomId == currentRoomId
                    ? connection.ToRoomId
                    : connection.FromRoomId;

                if (!visited.Contains(nextRoomId))
                {
                    visited.Add(nextRoomId);
                    queue.Enqueue(nextRoomId);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Logs validation issues to database for telemetry
    /// </summary>
    public void LogValidationIssues(int sectorId, List<ValidationIssue> issues)
    {
        foreach (var issue in issues)
        {
            _logger.Warning(
                "Validation issue in sector {SectorId}: Type={Type}, Severity={Severity}, Description={Description}",
                sectorId,
                issue.Type,
                issue.Severity,
                issue.Description);
        }

        _logger.Information(
            "Logged {IssueCount} validation issues for sector {SectorId}",
            issues.Count,
            sectorId);

        // TODO: Write to Spatial_Validation_Log table
    }
}

/// <summary>
/// Represents a spatial validation issue
/// </summary>
public class ValidationIssue
{
    /// <summary>
    /// Type of validation issue (Overlap, Unreachable, MissingConnection, LayerBounds, InvalidFootprint)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Severity level (Warning, Error, Critical)
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Room IDs affected by this issue
    /// </summary>
    public List<string> AffectedRoomIds { get; set; } = new List<string>();

    /// <summary>
    /// Position where the issue occurs (if applicable)
    /// </summary>
    public RoomPosition? Position { get; set; }

    public override string ToString()
    {
        return $"[{Severity}] {Type}: {Description}";
    }
}
