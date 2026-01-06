using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Instantiates Room entities from DungeonNodes using weighted template selection.
/// </summary>
public class RoomInstantiator : IRoomInstantiator
{
    private readonly IReadOnlyList<RoomTemplate> _templates;
    private readonly Dictionary<string, string[]> _sizeAdjectives;
    private readonly Dictionary<string, string[]> _conditionAdjectives;
    private readonly Dictionary<string, string[]> _atmosphereAdjectives;

    public RoomInstantiator(IRoomTemplateProvider templateProvider)
    {
        _templates = templateProvider?.GetAllTemplates()
            ?? throw new ArgumentNullException(nameof(templateProvider));

        // Size adjectives based on archetype
        _sizeAdjectives = new Dictionary<string, string[]>
        {
            ["small"] = ["cramped", "narrow", "tight", "confined"],
            ["medium"] = ["modest", "average", "standard", "unremarkable"],
            ["large"] = ["spacious", "expansive", "vast", "cavernous"],
            ["massive"] = ["enormous", "colossal", "immense", "titanic"]
        };

        // Condition adjectives
        _conditionAdjectives = new Dictionary<string, string[]>
        {
            ["pristine"] = ["pristine", "well-maintained", "intact", "preserved"],
            ["worn"] = ["worn", "weathered", "aged", "faded"],
            ["damaged"] = ["cracked", "crumbling", "damaged", "broken"],
            ["ruined"] = ["ruined", "destroyed", "collapsed", "shattered"]
        };

        // Atmosphere adjectives
        _atmosphereAdjectives = new Dictionary<string, string[]>
        {
            ["calm"] = ["peaceful", "still", "quiet", "serene"],
            ["eerie"] = ["eerie", "unsettling", "ominous", "foreboding"],
            ["threatening"] = ["menacing", "threatening", "dangerous", "hostile"],
            ["oppressive"] = ["oppressive", "suffocating", "crushing", "overwhelming"]
        };
    }

    public Room InstantiateRoom(DungeonNode node, Biome biome, Random random)
    {
        // Select matching template
        var template = SelectTemplate(node, biome, random);

        // Generate placeholder values
        var placeholders = GeneratePlaceholders(node, random);

        // Process name and description
        var name = template.ProcessName(placeholders);
        var description = template.ProcessDescription(placeholders);

        // Create room at node position (convert 3D to 2D for now)
        var position = new Position(node.Coordinate.X, node.Coordinate.Y);
        var room = new Room(name, description, position, biome);

        // Propagate tags from template and node
        room.AddTags(template.Tags);
        room.AddTags(node.Tags);

        // Add archetype tag
        room.AddTag(node.Archetype.ToString());

        // Mark special rooms
        if (node.IsStartNode)
            room.AddTag("StartRoom");
        if (node.IsBossArena)
            room.AddTag("BossRoom");

        return room;
    }

    public IReadOnlyDictionary<Guid, Room> InstantiateSector(Sector sector)
    {
        return InstantiateSector(sector, Environment.TickCount);
    }

    public IReadOnlyDictionary<Guid, Room> InstantiateSector(Sector sector, int seed)
    {
        var random = new Random(seed);
        var rooms = new Dictionary<Guid, Room>();

        foreach (var node in sector.GetAllNodes())
        {
            var room = InstantiateRoom(node, sector.Biome, random);
            rooms[node.Id] = room;
        }

        // Connect rooms based on node connections
        foreach (var node in sector.GetAllNodes())
        {
            var room = rooms[node.Id];
            foreach (var (direction, targetNodeId) in node.Connections)
            {
                if (rooms.TryGetValue(targetNodeId, out var targetRoom))
                {
                    room.AddExit(direction, targetRoom.Id);
                }
            }
        }

        return rooms;
    }

    private RoomTemplate SelectTemplate(DungeonNode node, Biome biome, Random random)
    {
        // Filter templates by compatibility
        var candidates = _templates
            .Where(t => t.IsCompatibleWithBiome(biome))
            .Where(t => t.IsCompatibleWithArchetype(node.Archetype))
            .Where(t => t.IsCompatibleWithExitCount(node.GetConnectionCount()))
            .ToList();

        if (candidates.Count == 0)
        {
            // Fallback: try any template for this biome and archetype
            candidates = _templates
                .Where(t => t.IsCompatibleWithBiome(biome))
                .Where(t => t.IsCompatibleWithArchetype(node.Archetype))
                .ToList();
        }

        if (candidates.Count == 0)
        {
            // Last resort: use any template for this biome
            candidates = _templates
                .Where(t => t.IsCompatibleWithBiome(biome))
                .ToList();
        }

        if (candidates.Count == 0)
        {
            // Ultimate fallback: first template
            return _templates[0];
        }

        // Weighted random selection
        return SelectWeighted(candidates, random);
    }

    private static RoomTemplate SelectWeighted(IReadOnlyList<RoomTemplate> templates, Random random)
    {
        var totalWeight = templates.Sum(t => t.Weight);
        var roll = random.Next(totalWeight);
        var cumulative = 0;

        foreach (var template in templates)
        {
            cumulative += template.Weight;
            if (roll < cumulative)
                return template;
        }

        return templates[^1];
    }

    private Dictionary<string, string> GeneratePlaceholders(DungeonNode node, Random random)
    {
        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Size based on archetype
        var sizeCategory = node.Archetype switch
        {
            RoomArchetype.DeadEnd => "small",
            RoomArchetype.Corridor => "medium",
            RoomArchetype.Chamber => random.NextDouble() < 0.5 ? "medium" : "large",
            RoomArchetype.Junction => "large",
            RoomArchetype.Stairwell => "medium",
            RoomArchetype.BossArena => "massive",
            _ => "medium"
        };
        placeholders["ADJ_SIZE"] = SelectRandom(_sizeAdjectives[sizeCategory], random);

        // Condition based on tags or random
        var conditionCategory = node.HasTag("Damaged") ? "damaged" :
                               node.HasTag("Ruined") ? "ruined" :
                               node.HasTag("Ancient") ? "worn" :
                               random.NextDouble() < 0.3 ? "worn" : "damaged";
        placeholders["ADJ_CONDITION"] = SelectRandom(_conditionAdjectives[conditionCategory], random);

        // Atmosphere based on special status
        var atmosphereCategory = node.IsBossArena ? "threatening" :
                                node.HasTag("Ominous") ? "eerie" :
                                node.HasTag("Quiet") ? "calm" :
                                random.NextDouble() < 0.5 ? "eerie" : "calm";
        placeholders["ADJ_ATMOSPHERE"] = SelectRandom(_atmosphereAdjectives[atmosphereCategory], random);

        return placeholders;
    }

    private static string SelectRandom(string[] options, Random random)
    {
        return options[random.Next(options.Length)];
    }
}
