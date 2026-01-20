using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Generates dungeon topology using the "Sprouting Vine" algorithm.
/// Creates connected graphs of dungeon nodes by growing from a starting point.
/// </summary>
public class SproutingVineTopologyGenerator : ITopologyGenerator
{
    private const int MaxConnectionsPerNode = 4;
    private const double LoopClosureChance = 0.20; // 20% chance to connect to existing node
    private const double StairwellChance = 0.10; // 10% chance for vertical connections

    private static readonly Direction[] CardinalDirections =
        [Direction.North, Direction.South, Direction.East, Direction.West];

    private static readonly Direction[] VerticalDirections =
        [Direction.Up, Direction.Down];

    public Sector GenerateSector(
        Biome biome,
        int targetRoomCount,
        int depth,
        int? seed = null)
    {
        if (targetRoomCount < 3)
            throw new ArgumentOutOfRangeException(nameof(targetRoomCount),
                "Minimum room count is 3 (start, at least one middle, boss)");

        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        var sectorName = GenerateSectorName(biome, depth);
        var sector = new Sector(sectorName, biome, depth);

        // Phase 1: Initialize with start node at origin
        var startNode = new DungeonNode($"Sec{depth}_Rm01", Coordinate3D.Origin);
        sector.AddNode(startNode);
        sector.SetStartNode(startNode.Id);

        var roomCounter = 1;

        // Phase 2: Growth Loop - Sprouting Vine algorithm
        while (sector.GetNodeCount() < targetRoomCount)
        {
            var activeNodes = sector.GetActiveNodes(MaxConnectionsPerNode).ToList();
            if (activeNodes.Count == 0)
                break; // No more growth points

            // Select random active node
            var sourceNode = activeNodes[random.Next(activeNodes.Count)];

            // Pick random direction
            var direction = PickDirection(sourceNode, random);
            var targetCoordinate = sourceNode.Coordinate.Move(direction);

            // Check if coordinate is occupied
            var existingNode = sector.GetNodeByCoordinate(targetCoordinate);

            if (existingNode == null)
            {
                // Create new node
                roomCounter++;
                var newNode = new DungeonNode(
                    $"Sec{depth}_Rm{roomCounter:D2}",
                    targetCoordinate);

                // Add vertical tag for stairwell rooms
                if (direction is Direction.Up or Direction.Down)
                {
                    newNode.SetArchetype(RoomArchetype.Stairwell);
                    newNode.AddTag("Vertical");
                }

                sector.AddNode(newNode);
                sector.ConnectNodes(sourceNode.Id, direction, newNode.Id);
            }
            else if (CanCloseLoop(sourceNode, existingNode, direction, random))
            {
                // Loop closure - connect to existing node
                sector.ConnectNodes(sourceNode.Id, direction, existingNode.Id);
            }
            // else: skip this iteration, try again
        }

        // Phase 3: Validation and finalization
        FinalizeTopology(sector, random);

        return sector;
    }

    private Direction PickDirection(DungeonNode node, Random random)
    {
        // Get available directions (not already connected)
        var available = CardinalDirections
            .Where(d => !node.HasConnection(d))
            .ToList();

        // Add vertical directions for stairwell possibility
        if (random.NextDouble() < StairwellChance)
        {
            var verticalAvailable = VerticalDirections
                .Where(d => !node.HasConnection(d))
                .ToList();
            available.AddRange(verticalAvailable);
        }

        if (available.Count == 0)
            return CardinalDirections[random.Next(CardinalDirections.Length)];

        return available[random.Next(available.Count)];
    }

    private static bool CanCloseLoop(DungeonNode source, DungeonNode target, Direction direction, Random random)
    {
        // Don't connect if either node has too many connections
        if (source.GetConnectionCount() >= MaxConnectionsPerNode ||
            target.GetConnectionCount() >= MaxConnectionsPerNode)
            return false;

        // Don't connect if already connected in opposite direction
        var oppositeDirection = GetOppositeDirection(direction);
        if (target.HasConnection(oppositeDirection))
            return false;

        // Random chance for loop closure
        return random.NextDouble() < LoopClosureChance;
    }

    private void FinalizeTopology(Sector sector, Random random)
    {
        // Update archetypes based on connection counts
        sector.FinalizeNodeArchetypes();

        // Find boss arena using simplified Dijkstra (find furthest leaf node)
        var startNodeId = sector.StartNodeId!.Value;
        var bossNodeId = FindFurthestLeafNode(sector, startNodeId);

        if (bossNodeId.HasValue)
        {
            sector.SetBossNode(bossNodeId.Value);
        }
        else
        {
            // Fallback: use any leaf node, or the last added node
            var leafNodes = sector.GetLeafNodes().ToList();
            if (leafNodes.Count > 0)
            {
                var bossNode = leafNodes[random.Next(leafNodes.Count)];
                sector.SetBossNode(bossNode.Id);
            }
        }

        // Propagate biome-specific tags to nodes
        PropagateBaseTagsToNodes(sector);
    }

    private static Guid? FindFurthestLeafNode(Sector sector, Guid startNodeId)
    {
        // Simple BFS to find distances
        var distances = new Dictionary<Guid, int> { [startNodeId] = 0 };
        var queue = new Queue<Guid>();
        queue.Enqueue(startNodeId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var currentNode = sector.GetNode(currentId);
            if (currentNode == null) continue;

            var currentDistance = distances[currentId];

            foreach (var (_, neighborId) in currentNode.Connections)
            {
                if (!distances.ContainsKey(neighborId))
                {
                    distances[neighborId] = currentDistance + 1;
                    queue.Enqueue(neighborId);
                }
            }
        }

        // Find the furthest leaf node (1 connection only)
        Guid? furthestId = null;
        var maxDistance = -1;

        foreach (var node in sector.GetLeafNodes())
        {
            if (node.Id == startNodeId) continue; // Skip start node

            if (distances.TryGetValue(node.Id, out var distance) && distance > maxDistance)
            {
                maxDistance = distance;
                furthestId = node.Id;
            }
        }

        return furthestId;
    }

    private static void PropagateBaseTagsToNodes(Sector sector)
    {
        // Add biome-specific base tags
        var biomeTags = GetBiomeTags(sector.Biome);

        foreach (var node in sector.GetAllNodes())
        {
            node.AddTags(biomeTags);
        }
    }

    private static IEnumerable<string> GetBiomeTags(Biome biome) => biome switch
    {
        Biome.Citadel => ["Stone", "Ancient", "Dark"],
        Biome.TheRoots => ["Organic", "Damp", "Overgrown"],
        Biome.Muspelheim => ["Hot", "Volcanic", "Glowing"],
        Biome.Niflheim => ["Cold", "Icy", "Mist"],
        Biome.Jotunheim => ["Massive", "Runic", "Ancient"],
        _ => ["Unknown"]
    };

    private static string GenerateSectorName(Biome biome, int depth) => biome switch
    {
        Biome.Citadel => $"The Fallen Citadel - Level {depth}",
        Biome.TheRoots => $"The Roots - Depth {depth}",
        Biome.Muspelheim => $"Muspelheim Depths - Stratum {depth}",
        Biome.Niflheim => $"Niflheim - Frost Layer {depth}",
        Biome.Jotunheim => $"Jotunheim Ruins - Tier {depth}",
        _ => $"Unknown Sector - Level {depth}"
    };

    private static Direction GetOppositeDirection(Direction direction) => direction switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Northeast => Direction.Southwest,
        Direction.Northwest => Direction.Southeast,
        Direction.Southeast => Direction.Northwest,
        Direction.Southwest => Direction.Northeast,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };
}
