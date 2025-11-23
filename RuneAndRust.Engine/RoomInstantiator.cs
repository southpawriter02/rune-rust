using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Converts DungeonGraph to Dungeon with instantiated Room objects (v0.10)
/// v0.39.1: Extended to apply 3D spatial positions and vertical connections
/// </summary>
public class RoomInstantiator
{
    private static readonly ILogger _log = Log.ForContext<RoomInstantiator>();
    private Random _rng = null!;

    /// <summary>
    /// Instantiates a complete dungeon from a graph
    /// v0.39.1: Optional parameters for 3D spatial layout
    /// </summary>
    public Dungeon Instantiate(
        DungeonGraph graph,
        int dungeonId,
        int seed,
        Dictionary<string, RoomPosition>? positions = null,
        List<VerticalConnection>? verticalConnections = null)
    {
        _rng = new Random(seed);

        _log.Information("Instantiating dungeon: DungeonId={DungeonId}, Seed={Seed}, Nodes={Nodes}, Spatial={HasSpatial}",
            dungeonId, seed, graph.NodeCount, positions != null);

        var dungeon = new Dungeon
        {
            DungeonId = dungeonId,
            Seed = seed,
            Biome = "the_roots" // TODO: Get from biome system when implemented
        };

        // Step 1: Create Room instances for all nodes
        var nodeToRoomId = new Dictionary<DungeonNode, string>();

        foreach (var node in graph.GetNodes())
        {
            var roomId = GenerateRoomId(dungeonId, node.Id);
            var room = InstantiateRoom(node, roomId, dungeonId);

            dungeon.Rooms[roomId] = room;
            nodeToRoomId[node] = roomId;

            // Track start and boss rooms
            if (node.Type == NodeType.Start)
            {
                dungeon.StartRoomId = roomId;
            }
            else if (node.Type == NodeType.Boss)
            {
                dungeon.BossRoomId = roomId;
            }

            _log.Debug("Instantiated room: {RoomId} ({TemplateId}, {NodeType})",
                roomId, node.Template.TemplateId, node.Type);
        }

        // Step 2: Build exit connections
        foreach (var node in graph.GetNodes())
        {
            var roomId = nodeToRoomId[node];
            var room = dungeon.Rooms[roomId];

            var edges = graph.GetEdgesFrom(node);

            foreach (var edge in edges)
            {
                if (edge.FromDirection.HasValue)
                {
                    var targetRoomId = nodeToRoomId[edge.To];
                    var directionString = edge.FromDirection.Value.ToNavigationString();

                    room.Exits[directionString] = targetRoomId;

                    _log.Debug("Connected room: {From} --{Direction}--> {To}",
                        roomId, directionString, targetRoomId);
                }
            }
        }

        // Step 3 (v0.39.1): Apply 3D spatial positions to rooms
        if (positions != null)
        {
            _log.Debug("Applying 3D spatial positions to {RoomCount} rooms", positions.Count);

            foreach (var kvp in positions)
            {
                var roomId = kvp.Key;
                var position = kvp.Value;

                if (dungeon.Rooms.TryGetValue(roomId, out var room))
                {
                    room.Position = position;
                    room.Layer = VerticalLayerExtensions.FromZCoordinate(position.Z);

                    _log.Debug("Applied position to room {RoomId}: {Position} ({Layer})",
                        roomId, position, room.Layer);
                }
            }

            // Store positions in dungeon
            dungeon.RoomPositions = new Dictionary<string, RoomPosition>(positions);

            _log.Information("Spatial positions applied: {PositionCount} rooms positioned", positions.Count);
        }

        // Step 4 (v0.39.1): Apply vertical connections to rooms
        if (verticalConnections != null && verticalConnections.Count > 0)
        {
            _log.Debug("Applying {ConnectionCount} vertical connections", verticalConnections.Count);

            foreach (var connection in verticalConnections)
            {
                // Add connection to FROM room
                if (dungeon.Rooms.TryGetValue(connection.FromRoomId, out var fromRoom))
                {
                    fromRoom.VerticalConnections.Add(connection);
                }

                // If bidirectional, also add reverse connection to TO room
                if (connection.IsBidirectional && dungeon.Rooms.TryGetValue(connection.ToRoomId, out var toRoom))
                {
                    // Create reverse connection reference (same object, bidirectional means both rooms can access it)
                    if (!toRoom.VerticalConnections.Contains(connection))
                    {
                        toRoom.VerticalConnections.Add(connection);
                    }
                }
            }

            // Store vertical connections in dungeon
            dungeon.VerticalConnections = new List<VerticalConnection>(verticalConnections);

            _log.Information("Vertical connections applied: {ConnectionCount} connections", verticalConnections.Count);
        }

        _log.Information("Dungeon instantiation complete: {RoomCount} rooms, {ExitCount} exits, {VerticalCount} vertical",
            dungeon.TotalRoomCount,
            dungeon.Rooms.Values.Sum(r => r.Exits.Count),
            dungeon.VerticalConnections.Count);

        // Validate
        var (isValid, errors) = dungeon.Validate();
        if (!isValid)
        {
            _log.Warning("Dungeon validation warnings: {Errors}", string.Join(", ", errors));
        }

        return dungeon;
    }

    /// <summary>
    /// Instantiates a single room from a node
    /// </summary>
    private Room InstantiateRoom(DungeonNode node, string roomId, int dungeonId)
    {
        var template = node.Template;

        var room = new Room
        {
            RoomId = roomId,
            Name = GenerateName(template),
            Description = GenerateDescription(template),
            TemplateId = template.TemplateId,
            GeneratedNodeType = node.Type,
            IsProcedurallyGenerated = true,

            // Set special flags based on node type
            IsStartRoom = node.Type == NodeType.Start,
            IsBossRoom = node.Type == NodeType.Boss,
            IsSanctuary = node.Type == NodeType.Start, // Start rooms are safe havens

            // v0.10: No enemies, loot, or hazards (added in v0.11)
            HasBeenCleared = node.Type == NodeType.Start // Start room pre-cleared
        };

        return room;
    }

    /// <summary>
    /// Generates a room name from template
    /// </summary>
    private string GenerateName(RoomTemplate template)
    {
        if (template.NameTemplates.Count == 0)
        {
            _log.Warning("Template {TemplateId} has no name templates", template.TemplateId);
            return "Unknown Room";
        }

        // Pick random name template
        var nameTemplate = template.NameTemplates[_rng.Next(template.NameTemplates.Count)];

        // Replace {Adjective} placeholders
        while (nameTemplate.Contains("{Adjective}"))
        {
            if (template.Adjectives.Count == 0)
            {
                _log.Warning("Template {TemplateId} has {Adjective} placeholder but no adjectives",
                    template.TemplateId);
                nameTemplate = nameTemplate.Replace("{Adjective}", "Ancient");
                break;
            }

            var adjective = template.Adjectives[_rng.Next(template.Adjectives.Count)];
            nameTemplate = ReplaceFirst(nameTemplate, "{Adjective}", adjective);
        }

        return nameTemplate;
    }

    /// <summary>
    /// Generates a room description from template
    /// </summary>
    private string GenerateDescription(RoomTemplate template)
    {
        if (template.DescriptionTemplates.Count == 0)
        {
            _log.Warning("Template {TemplateId} has no description templates", template.TemplateId);
            return "An unremarkable chamber in the depths of Aethelgard.";
        }

        // Pick random description template
        var descriptionTemplate = template.DescriptionTemplates[_rng.Next(template.DescriptionTemplates.Count)];

        // Replace {Adjective} placeholders
        while (descriptionTemplate.Contains("{Adjective}"))
        {
            if (template.Adjectives.Count == 0)
            {
                descriptionTemplate = descriptionTemplate.Replace("{Adjective}", "ancient");
                break;
            }

            var adjective = template.Adjectives[_rng.Next(template.Adjectives.Count)];
            // Use lowercase for mid-sentence adjectives
            adjective = char.ToLower(adjective[0]) + adjective.Substring(1);
            descriptionTemplate = ReplaceFirst(descriptionTemplate, "{Adjective}", adjective);
        }

        // Replace {Detail} placeholders
        while (descriptionTemplate.Contains("{Detail}"))
        {
            if (template.Details.Count == 0)
            {
                descriptionTemplate = descriptionTemplate.Replace("{Detail}", "The walls show signs of age");
                break;
            }

            var detail = template.Details[_rng.Next(template.Details.Count)];
            descriptionTemplate = ReplaceFirst(descriptionTemplate, "{Detail}", detail);
        }

        return descriptionTemplate;
    }

    /// <summary>
    /// Generates a unique room ID
    /// </summary>
    private string GenerateRoomId(int dungeonId, int nodeId)
    {
        return $"room_d{dungeonId}_n{nodeId}";
    }

    /// <summary>
    /// Replaces the first occurrence of a substring
    /// </summary>
    private string ReplaceFirst(string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }

        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
}
