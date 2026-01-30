using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Instantiates Room entities from DungeonNodes using weighted template selection.
/// Supports both legacy placeholder system and new three-tier descriptor composition.
/// </summary>
public class RoomInstantiator : IRoomInstantiator
{
    private readonly IReadOnlyList<RoomTemplate> _templates;
    private readonly IRoomDescriptorService? _descriptorService;
    private readonly IDescriptorRepository? _descriptorRepository;
    private readonly Dictionary<string, string[]> _sizeAdjectives;
    private readonly Dictionary<string, string[]> _conditionAdjectives;
    private readonly Dictionary<string, string[]> _atmosphereAdjectives;

    /// <summary>
    /// Creates a RoomInstantiator with legacy placeholder support only.
    /// </summary>
    public RoomInstantiator(IRoomTemplateProvider templateProvider)
        : this(templateProvider, null, null)
    {
    }

    /// <summary>
    /// Creates a RoomInstantiator with full three-tier descriptor support.
    /// </summary>
    public RoomInstantiator(
        IRoomTemplateProvider templateProvider,
        IRoomDescriptorService? descriptorService,
        IDescriptorRepository? descriptorRepository)
    {
        _templates = templateProvider?.GetAllTemplates()
            ?? throw new ArgumentNullException(nameof(templateProvider));
        _descriptorService = descriptorService;
        _descriptorRepository = descriptorRepository;

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
        // Select matching template from legacy system
        var template = SelectTemplate(node, biome, random);

        string name;
        string description;

        // Try three-tier descriptor system if available
        if (_descriptorService != null && _descriptorRepository != null)
        {
            var baseTemplate = _descriptorRepository.GetBaseTemplate(node.Archetype);
            if (baseTemplate != null)
            {
                var modifier = _descriptorRepository.GetModifier(biome);
                var roomTags = template.Tags.Concat(node.Tags).ToList();

                // Optionally select a room function for chambers
                RoomFunction? roomFunction = null;
                if (node.Archetype == RoomArchetype.Chamber && random.NextDouble() < 0.3)
                {
                    var functions = _descriptorRepository.GetFunctionsByBiome(biome);
                    if (functions.Count > 0)
                    {
                        roomFunction = SelectWeightedFunction(functions, random);
                    }
                }

                // Generate using three-tier system
                name = _descriptorService.GenerateRoomName(baseTemplate, modifier, roomFunction);
                description = _descriptorService.GenerateRoomDescription(
                    baseTemplate, modifier, roomTags, random, roomFunction);

                // Create room
                var position = new Position3D(node.Coordinate.X, node.Coordinate.Y, 0);
                var room = new Room(name, description, position, biome);

                // Propagate tags
                room.AddTags(template.Tags);
                room.AddTags(node.Tags);
                room.AddTag(node.Archetype.ToString());

                // Apply mechanical effects from modifier
                ApplyModifierEffects(room, modifier);

                // Spawn features from template
                SpawnFeatures(room, template, random);

                // Mark special rooms
                if (node.IsStartNode)
                    room.AddTag("StartRoom");
                if (node.IsBossArena)
                    room.AddTag("BossRoom");

                return room;
            }
        }

        // Fallback to legacy placeholder system
        var placeholders = GeneratePlaceholders(node, random);
        name = template.ProcessName(placeholders);
        description = template.ProcessDescription(placeholders);

        // Create room at node position (convert 3D to 2D for now)
        var legacyPosition = new Position3D(node.Coordinate.X, node.Coordinate.Y, 0);
        var legacyRoom = new Room(name, description, legacyPosition, biome);

        // Propagate tags from template and node
        legacyRoom.AddTags(template.Tags);
        legacyRoom.AddTags(node.Tags);

        // Add archetype tag
        legacyRoom.AddTag(node.Archetype.ToString());

        // Spawn features from template
        SpawnFeatures(legacyRoom, template, random);

        // Mark special rooms
        if (node.IsStartNode)
            legacyRoom.AddTag("StartRoom");
        if (node.IsBossArena)
            legacyRoom.AddTag("BossRoom");

        return legacyRoom;
    }

    private static void ApplyModifierEffects(Room room, ThematicModifier modifier)
    {
        // Add modifier-based tags for gameplay effects
        foreach (var effectTag in modifier.GetEffectTags())
        {
            room.AddTag(effectTag);
        }
    }

    private static void SpawnFeatures(Room room, RoomTemplate template, Random random)
    {
        foreach (var feature in template.Features)
        {
            if (feature.ShouldSpawn(random))
            {
                var instance = RoomFeatureInstance.Create(
                    feature.Type,
                    feature.FeatureId,
                    GetFeatureDisplayName(feature.FeatureId),
                    room.Id,
                    feature.DescriptorOverride);
                room.AddFeature(instance);
            }
        }
    }

    private static string GetFeatureDisplayName(string featureId)
    {
        // Convert snake_case to Title Case: "weapon_rack" -> "Weapon Rack"
        if (string.IsNullOrWhiteSpace(featureId))
            return featureId;

        return string.Join(" ", featureId.Split('_')
            .Where(w => !string.IsNullOrEmpty(w))
            .Select(w => char.ToUpper(w[0]) + w[1..].ToLower()));
    }

    private static RoomFunction SelectWeightedFunction(IReadOnlyList<RoomFunction> functions, Random random)
    {
        var totalWeight = functions.Sum(f => f.Weight);
        var roll = random.NextDouble() * totalWeight;

        foreach (var function in functions)
        {
            roll -= function.Weight;
            if (roll <= 0)
                return function;
        }

        return functions[^1];
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
