using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Converts DungeonGraph to Dungeon with instantiated Room objects (v0.10)
/// </summary>
public class RoomInstantiator
{
    private static readonly ILogger _log = Log.ForContext<RoomInstantiator>();
    private Random _rng = null!;

    /// <summary>
    /// Instantiates a complete dungeon from a graph
    /// </summary>
    public Dungeon Instantiate(DungeonGraph graph, int dungeonId, int seed)
    {
        _rng = new Random(seed);

        _log.Information("Instantiating dungeon: DungeonId={DungeonId}, Seed={Seed}, Nodes={Nodes}",
            dungeonId, seed, graph.NodeCount);

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

        _log.Information("Dungeon instantiation complete: {RoomCount} rooms, {ExitCount} exits",
            dungeon.TotalRoomCount, dungeon.Rooms.Values.Sum(r => r.Exits.Count));

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
