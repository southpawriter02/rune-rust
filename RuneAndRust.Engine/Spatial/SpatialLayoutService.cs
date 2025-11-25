using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using Serilog;
using CoreDirection = RuneAndRust.Core.Direction;

namespace RuneAndRust.Engine.Spatial;

/// <summary>
/// v0.39.1: Converts abstract graph-based dungeons into 3D spatially-aware sectors
/// Places rooms at (X, Y, Z) coordinates and generates vertical connections
/// </summary>
public interface ISpatialLayoutService
{
    Dictionary<string, RoomPosition> ConvertGraphTo3DLayout(DungeonGraph graph, int seed);
    bool ValidateNoOverlaps(Dictionary<string, RoomPosition> positions, Dictionary<string, RoomTemplate> templates);
    List<VerticalConnection> GenerateVerticalConnections(Dictionary<string, RoomPosition> positions, Random rng);
    RoomPosition? GetRoomPosition(string roomId, Dictionary<string, RoomPosition> positions);
    List<string> GetRoomsAtLayer(VerticalLayer layer, Dictionary<string, RoomPosition> positions);
    List<string> GetRoomsInRange(RoomPosition center, int radius, Dictionary<string, RoomPosition> positions);
}

public class SpatialLayoutService : ISpatialLayoutService
{
    private readonly ILogger _logger;

    public SpatialLayoutService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts a DungeonGraph to 3D room positions using breadth-first traversal
    /// </summary>
    public Dictionary<string, RoomPosition> ConvertGraphTo3DLayout(DungeonGraph graph, int seed)
    {
        var rng = new Random(seed);
        var positionedRooms = new Dictionary<string, RoomPosition>();
        var visited = new HashSet<string>();
        var queue = new Queue<DungeonNode>();

        _logger.Information("Converting graph to 3D layout: Seed={Seed}, Nodes={NodeCount}", seed, graph.NodeCount);

        // Step 1: Place Entry Hall at origin (0, 0, 0)
        var startNode = graph.StartNode;
        if (startNode == null)
        {
            _logger.Error("Cannot convert graph to 3D: No start node found");
            throw new InvalidOperationException("Graph has no start node");
        }

        var startNodeId = startNode.Id.ToString();
        positionedRooms[startNodeId] = RoomPosition.Origin;
        visited.Add(startNodeId);
        queue.Enqueue(startNode);

        _logger.Debug("Entry Hall placed at origin: {RoomId} at {Position}", startNodeId, RoomPosition.Origin);

        // Step 2: Breadth-first traversal to assign positions
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            var currentNodeId = currentNode.Id.ToString();
            var currentPosition = positionedRooms[currentNodeId];

            var edges = graph.GetEdgesFrom(currentNode);
            _logger.Debug("Processing node {NodeId} at {Position}, {EdgeCount} edges",
                currentNodeId, currentPosition, edges.Count);

            foreach (var edge in edges)
            {
                var targetNodeId = edge.To.Id.ToString();

                if (visited.Contains(targetNodeId))
                {
                    _logger.Debug("Skipping already visited node: {NodeId}", targetNodeId);
                    continue;
                }

                // Calculate new position based on direction and vertical probability
                var newPosition = CalculateNewPosition(
                    currentPosition,
                    edge.FromDirection ?? CoreDirection.North,
                    currentNode,
                    edge.To,
                    rng);

                // Validate no overlap - if overlap exists, find nearby unoccupied position
                if (IsPositionOccupied(positionedRooms.Values, newPosition))
                {
                    _logger.Warning("Position {Position} occupied, finding alternative for {NodeId}",
                        newPosition, targetNodeId);
                    newPosition = FindNearbyUnoccupiedPosition(newPosition, positionedRooms.Values, rng);
                }

                positionedRooms[targetNodeId] = newPosition;
                visited.Add(targetNodeId);
                queue.Enqueue(edge.To);

                _logger.Debug("Positioned room: {NodeId} at {Position} (direction: {Direction})",
                    targetNodeId, newPosition, edge.FromDirection);
            }
        }

        _logger.Information("3D layout complete: {RoomCount} rooms positioned across {Layers} vertical layers",
            positionedRooms.Count,
            positionedRooms.Values.Select(p => p.Z).Distinct().Count());

        return positionedRooms;
    }

    /// <summary>
    /// Calculates the new position for a room based on direction and vertical movement probability
    /// </summary>
    private RoomPosition CalculateNewPosition(
        RoomPosition current,
        CoreDirection direction,
        DungeonNode fromNode,
        DungeonNode toNode,
        Random rng)
    {
        // Step 1: Calculate horizontal movement based on direction
        var newPosition = direction switch
        {
            CoreDirection.North => new RoomPosition(current.X, current.Y + 1, current.Z),
            CoreDirection.South => new RoomPosition(current.X, current.Y - 1, current.Z),
            CoreDirection.East => new RoomPosition(current.X + 1, current.Y, current.Z),
            CoreDirection.West => new RoomPosition(current.X - 1, current.Y, current.Z),
            _ => current
        };

        // Step 2: Determine if vertical movement occurs (30% base chance)
        var verticalChance = CalculateVerticalMovementChance(fromNode, toNode);

        if (rng.NextDouble() < verticalChance)
        {
            var zDelta = DetermineVerticalChange(fromNode, toNode, current.Z, rng);
            newPosition = new RoomPosition(newPosition.X, newPosition.Y, newPosition.Z + zDelta);

            _logger.Debug("Vertical movement: {FromNode} → {ToNode}, Z delta: {ZDelta}",
                fromNode.Id, toNode.Id, zDelta);
        }

        return newPosition;
    }

    /// <summary>
    /// Calculates the probability of vertical movement based on node types
    /// </summary>
    private double CalculateVerticalMovementChance(DungeonNode fromNode, DungeonNode toNode)
    {
        // Base chance: 30%
        var chance = 0.30;

        // Boss rooms have higher chance of being on different level (60%)
        if (toNode.Type == NodeType.Boss)
        {
            chance = 0.60;
        }

        // Secret rooms have moderate chance (40%)
        else if (toNode.Type == NodeType.Secret)
        {
            chance = 0.40;
        }

        // Branch rooms have slightly higher chance (35%)
        else if (toNode.Type == NodeType.Branch)
        {
            chance = 0.35;
        }

        return chance;
    }

    /// <summary>
    /// Determines the Z coordinate change (-2 to +2)
    /// </summary>
    private int DetermineVerticalChange(DungeonNode fromNode, DungeonNode toNode, int currentZ, Random rng)
    {
        // Respect layer bounds (-3 to +3)
        var minZ = Math.Max(-3, currentZ - 2);
        var maxZ = Math.Min(3, currentZ + 2);

        // Boss rooms tend to be deeper (negative Z)
        if (toNode.Type == NodeType.Boss)
        {
            var options = new[] { -2, -1, 0 }.Where(z => currentZ + z >= minZ && currentZ + z <= maxZ).ToArray();
            return options.Length > 0 ? options[rng.Next(options.Length)] : 0;
        }

        // Secret rooms can be anywhere
        if (toNode.Type == NodeType.Secret)
        {
            var options = new[] { -2, -1, 0, 1 }.Where(z => currentZ + z >= minZ && currentZ + z <= maxZ).ToArray();
            return options.Length > 0 ? options[rng.Next(options.Length)] : 0;
        }

        // Main path: gradual descent (bias toward same level or down)
        var weighted = new[] { -1, -1, 0, 0, 0, 1 }; // 33% down, 50% same, 17% up
        var choice = weighted[rng.Next(weighted.Length)];

        // Ensure we don't exceed bounds
        if (currentZ + choice < minZ || currentZ + choice > maxZ)
        {
            return 0;
        }

        return choice;
    }

    /// <summary>
    /// Checks if a position is already occupied by another room
    /// </summary>
    private bool IsPositionOccupied(IEnumerable<RoomPosition> positions, RoomPosition target)
    {
        return positions.Contains(target);
    }

    /// <summary>
    /// Finds the nearest unoccupied position starting from a target position
    /// Uses spiral search pattern
    /// </summary>
    private RoomPosition FindNearbyUnoccupiedPosition(
        RoomPosition target,
        IEnumerable<RoomPosition> occupiedPositions,
        Random rng)
    {
        var occupied = new HashSet<RoomPosition>(occupiedPositions);

        // Try positions in expanding spiral pattern
        for (int radius = 1; radius <= 5; radius++)
        {
            var candidates = GeneratePositionsAtRadius(target, radius);

            // Shuffle candidates for variety
            var shuffled = candidates.OrderBy(_ => rng.Next()).ToList();

            foreach (var candidate in shuffled)
            {
                // Ensure within layer bounds
                if (candidate.Z < -3 || candidate.Z > 3)
                    continue;

                if (!occupied.Contains(candidate))
                {
                    _logger.Debug("Found alternative position: {Original} → {Alternative}",
                        target, candidate);
                    return candidate;
                }
            }
        }

        // Fallback: shift east
        _logger.Warning("Could not find unoccupied position near {Target}, using fallback",
            target);
        return new RoomPosition(target.X + 10, target.Y, target.Z);
    }

    /// <summary>
    /// Generates candidate positions at a given radius from center
    /// </summary>
    private List<RoomPosition> GeneratePositionsAtRadius(RoomPosition center, int radius)
    {
        var positions = new List<RoomPosition>();

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                // Only positions at exactly this radius (Manhattan distance)
                if (Math.Abs(dx) + Math.Abs(dy) == radius)
                {
                    positions.Add(new RoomPosition(center.X + dx, center.Y + dy, center.Z));

                    // Also try vertical variations
                    if (center.Z > -3)
                        positions.Add(new RoomPosition(center.X + dx, center.Y + dy, center.Z - 1));
                    if (center.Z < 3)
                        positions.Add(new RoomPosition(center.X + dx, center.Y + dy, center.Z + 1));
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// Validates that no rooms overlap in 3D space
    /// </summary>
    public bool ValidateNoOverlaps(Dictionary<string, RoomPosition> positions, Dictionary<string, RoomTemplate> templates)
    {
        var occupiedPositions = new HashSet<RoomPosition>();
        var hasOverlaps = false;

        foreach (var kvp in positions)
        {
            var roomId = kvp.Key;
            var position = kvp.Value;

            // For now, assume all rooms are 1x1x1 (will expand for larger rooms later)
            var footprint = new List<RoomPosition> { position };

            foreach (var pos in footprint)
            {
                if (occupiedPositions.Contains(pos))
                {
                    _logger.Error("Room overlap detected: {RoomId} at {Position}", roomId, pos);
                    hasOverlaps = true;
                }
                else
                {
                    occupiedPositions.Add(pos);
                }
            }
        }

        if (!hasOverlaps)
        {
            _logger.Information("Spatial validation passed: No overlaps detected");
        }

        return !hasOverlaps;
    }

    /// <summary>
    /// Generates vertical connections between rooms at different Z levels
    /// </summary>
    public List<VerticalConnection> GenerateVerticalConnections(Dictionary<string, RoomPosition> positions, Random rng)
    {
        var connections = new List<VerticalConnection>();

        _logger.Information("Generating vertical connections for {RoomCount} rooms", positions.Count);

        // Find room pairs that are directly above/below each other
        var roomList = positions.ToList();

        for (int i = 0; i < roomList.Count; i++)
        {
            for (int j = i + 1; j < roomList.Count; j++)
            {
                var room1 = roomList[i];
                var room2 = roomList[j];

                var pos1 = room1.Value;
                var pos2 = room2.Value;

                // Check if directly above/below (same X, Y, different Z)
                if (pos1.IsDirectlyAboveOrBelow(pos2))
                {
                    var zDifference = Math.Abs(pos1.Z - pos2.Z);

                    // Only connect rooms within 3 Z levels of each other
                    if (zDifference <= 3)
                    {
                        var connection = GenerateVerticalConnection(
                            room1.Key,
                            room2.Key,
                            zDifference,
                            rng);

                        connections.Add(connection);

                        _logger.Debug("Vertical connection created: {From} → {To}, Type: {Type}, Levels: {Levels}",
                            room1.Key, room2.Key, connection.Type, zDifference);
                    }
                }
            }
        }

        _logger.Information("Generated {ConnectionCount} vertical connections", connections.Count);

        return connections;
    }

    /// <summary>
    /// Generates a single vertical connection based on Z distance
    /// </summary>
    private VerticalConnection GenerateVerticalConnection(string fromRoomId, string toRoomId, int zDifference, Random rng)
    {
        var connectionType = SelectConnectionType(zDifference, rng);
        var traversalDC = connectionType.GetBaseTraversalDC();

        // 10% chance of being blocked
        var isBlocked = rng.NextDouble() < 0.10;

        var connection = new VerticalConnection
        {
            ConnectionId = $"vc_{fromRoomId}_{toRoomId}",
            FromRoomId = fromRoomId,
            ToRoomId = toRoomId,
            Type = connectionType,
            TraversalDC = traversalDC,
            IsBlocked = isBlocked && connectionType != VerticalConnectionType.Stairs, // Stairs never blocked
            LevelsSpanned = zDifference,
            Description = GenerateConnectionDescription(connectionType, zDifference),
            IsBidirectional = true
        };

        // Set blockage description if blocked
        if (connection.IsBlocked)
        {
            connection.BlockageDescription = GenerateBlockageDescription(connectionType);
            connection.ClearanceDC = 15;
            connection.ClearanceTimeMinutes = 10;
        }

        // Set power status for elevators
        if (connectionType == VerticalConnectionType.Elevator)
        {
            connection.IsPowered = rng.NextDouble() < 0.70; // 70% chance powered
        }

        return connection;
    }

    /// <summary>
    /// Selects appropriate connection type based on Z distance
    /// </summary>
    private VerticalConnectionType SelectConnectionType(int zDifference, Random rng)
    {
        return zDifference switch
        {
            1 => rng.NextDouble() < 0.70
                ? VerticalConnectionType.Stairs
                : VerticalConnectionType.Ladder,
            2 => rng.NextDouble() < 0.40
                ? VerticalConnectionType.Stairs
                : VerticalConnectionType.Shaft,
            3 => rng.NextDouble() < 0.30
                ? VerticalConnectionType.Elevator
                : VerticalConnectionType.Shaft,
            _ => VerticalConnectionType.Elevator // 4+ levels requires elevator
        };
    }

    /// <summary>
    /// Generates flavor description for a vertical connection
    /// </summary>
    private string GenerateConnectionDescription(VerticalConnectionType type, int levels)
    {
        var templates = type.GetDescriptionTemplates();
        if (templates.Count == 0)
            return $"A {type.ToString().ToLower()} spanning {levels} levels.";

        var template = templates[new Random().Next(templates.Count)];
        var direction = levels > 1 ? $"spanning {levels} levels" : "to the next level";
        var destination = levels > 2 ? "the depths" : "darkness";

        return template
            .Replace("{direction}", direction)
            .Replace("{destination}", destination);
    }

    /// <summary>
    /// Generates blockage description for collapsed connections
    /// </summary>
    private string GenerateBlockageDescription(VerticalConnectionType type)
    {
        return type switch
        {
            VerticalConnectionType.Shaft => "The shaft is choked with debris from a ceiling collapse. Clearing it would require significant effort.",
            VerticalConnectionType.Stairs => "The stairwell is blocked by fallen rubble and twisted metal.",
            VerticalConnectionType.Elevator => "The elevator is crushed by structural failure, completely impassable.",
            VerticalConnectionType.Ladder => "The ladder has been torn away from the wall, leaving the shaft inaccessible.",
            _ => "The passage is blocked by debris."
        };
    }

    #region Query Methods

    public RoomPosition? GetRoomPosition(string roomId, Dictionary<string, RoomPosition> positions)
    {
        return positions.TryGetValue(roomId, out var position) ? position : null;
    }

    public List<string> GetRoomsAtLayer(VerticalLayer layer, Dictionary<string, RoomPosition> positions)
    {
        var targetZ = (int)layer;
        return positions
            .Where(kvp => kvp.Value.Z == targetZ)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    public List<string> GetRoomsInRange(RoomPosition center, int radius, Dictionary<string, RoomPosition> positions)
    {
        return positions
            .Where(kvp => kvp.Value.ManhattanDistance3D(center) <= radius)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    #endregion
}
