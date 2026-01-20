using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Validates and fixes thematic coherence in generated dungeons.
/// Applies rules for tag conflicts, path validation, and optional glitch injection.
/// </summary>
public class CoherenceValidator : ICoherenceValidator
{
    private const double GlitchChance = 0.05; // 5% chance for glitch injection

    // Tag conflict resolution rules: (tag1, tag2) -> (remove1, remove2, add)
    private readonly Dictionary<(string, string), (bool, bool, string?)> _conflictRules;

    // Tags that require prerequisite tags
    private readonly Dictionary<string, string[]> _tagDependencies;

    // Glitch biome fragments
    private readonly Dictionary<Biome, string[]> _glitchTags;

    public CoherenceValidator()
    {
        _conflictRules = new Dictionary<(string, string), (bool, bool, string?)>()
        {
            // Hot + Cold = Steam (remove both, add Steam)
            [("Hot", "Cold")] = (true, true, "Steam"),
            [("Cold", "Hot")] = (true, true, "Steam"),

            // Fire + Ice = Mist (remove both, add Mist)
            [("Fire", "Icy")] = (true, true, "Mist"),
            [("Icy", "Fire")] = (true, true, "Mist"),

            // Dry + Wet = Damp (keep wet aspects)
            [("Dry", "Wet")] = (true, false, "Damp"),
            [("Wet", "Dry")] = (false, true, "Damp"),

            // Dark + Bright = Dim
            [("Dark", "Bright")] = (true, true, "Dim"),
            [("Bright", "Dark")] = (true, true, "Dim"),

            // Ancient + Modern = Transitional
            [("Ancient", "Modern")] = (true, true, "Transitional"),
            [("Modern", "Ancient")] = (true, true, "Transitional"),

            // Living + Dead = Haunted
            [("Living", "Dead")] = (true, true, "Haunted"),
            [("Dead", "Living")] = (true, true, "Haunted"),
        };

        _tagDependencies = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            // Steam requires heat source or water
            ["Steam"] = ["Hot", "Wet", "Volcanic", "Fire"],

            // Ice requires cold
            ["Icy"] = ["Cold", "Frozen", "Niflheim"],

            // Lava requires volcanic/hot
            ["Lava"] = ["Hot", "Volcanic", "Muspelheim"],

            // Spores require organic
            ["Spores"] = ["Organic", "Fungal", "TheRoots"],

            // Runic requires ancient or magical
            ["Runic"] = ["Ancient", "Magical", "Jotunheim"],
        };

        _glitchTags = new Dictionary<Biome, string[]>
        {
            [Biome.Citadel] = ["Stone", "Ancient", "Dark", "Runic"],
            [Biome.TheRoots] = ["Organic", "Spores", "Wet", "Overgrown"],
            [Biome.Muspelheim] = ["Hot", "Volcanic", "Fire", "Glowing"],
            [Biome.Niflheim] = ["Cold", "Icy", "Frozen", "Mist"],
            [Biome.Jotunheim] = ["Massive", "Runic", "Ancient", "Stone"],
        };
    }

    public void ValidateAndFix(Sector sector, IReadOnlyDictionary<Guid, Room> rooms, Random random)
    {
        // Step 1: Resolve tag conflicts in all rooms
        foreach (var room in rooms.Values)
        {
            ResolveTagConflicts(room, random);
        }

        // Step 2: Validate tag dependencies
        foreach (var room in rooms.Values)
        {
            ValidateTagDependencies(room);
        }

        // Step 3: Validate path from start to boss
        if (!ValidatePath(sector))
        {
            // If path validation fails, we have a serious issue
            // In production, this would trigger a regeneration or fix
            throw new InvalidOperationException("No valid path from start to boss arena");
        }

        // Step 4: Glitch injection (5% chance for one room)
        if (random.NextDouble() < GlitchChance)
        {
            InjectGlitch(sector, rooms, random);
        }
    }

    public bool ValidatePath(Sector sector)
    {
        if (!sector.StartNodeId.HasValue || !sector.BossNodeId.HasValue)
            return false;

        // BFS from start to boss
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(sector.StartNodeId.Value);
        visited.Add(sector.StartNodeId.Value);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            if (currentId == sector.BossNodeId.Value)
                return true; // Found path!

            var node = sector.GetNode(currentId);
            if (node == null) continue;

            foreach (var (_, neighborId) in node.Connections)
            {
                if (visited.Add(neighborId))
                {
                    queue.Enqueue(neighborId);
                }
            }
        }

        return false; // No path found
    }

    public void ResolveTagConflicts(Room room, Random random)
    {
        var tags = room.Tags.ToList();
        var tagsToRemove = new HashSet<string>();
        var tagsToAdd = new HashSet<string>();

        // Check all tag pairs for conflicts
        for (var i = 0; i < tags.Count; i++)
        {
            for (var j = i + 1; j < tags.Count; j++)
            {
                var key = (tags[i], tags[j]);
                if (_conflictRules.TryGetValue(key, out var rule))
                {
                    var (remove1, remove2, add) = rule;

                    if (remove1) tagsToRemove.Add(tags[i]);
                    if (remove2) tagsToRemove.Add(tags[j]);
                    if (!string.IsNullOrEmpty(add)) tagsToAdd.Add(add);
                }
            }
        }

        // Apply changes
        foreach (var tag in tagsToRemove)
        {
            room.RemoveTag(tag);
        }
        foreach (var tag in tagsToAdd)
        {
            room.AddTag(tag);
        }
    }

    private void ValidateTagDependencies(Room room)
    {
        var tags = room.Tags.ToList();

        foreach (var tag in tags)
        {
            if (_tagDependencies.TryGetValue(tag, out var requiredTags))
            {
                // Check if any required tag is present
                var hasRequired = requiredTags.Any(r => room.HasTag(r));

                if (!hasRequired)
                {
                    // Remove the dependent tag if no prerequisite exists
                    room.RemoveTag(tag);
                }
            }
        }
    }

    private void InjectGlitch(Sector sector, IReadOnlyDictionary<Guid, Room> rooms, Random random)
    {
        // Select a random non-special room for glitch injection
        var candidateNodes = sector.GetAllNodes()
            .Where(n => !n.IsStartNode && !n.IsBossArena)
            .ToList();

        if (candidateNodes.Count == 0)
            return;

        var glitchNode = candidateNodes[random.Next(candidateNodes.Count)];
        if (!rooms.TryGetValue(glitchNode.Id, out var glitchRoom))
            return;

        // Select a foreign biome (different from current)
        var foreignBiomes = Enum.GetValues<Biome>()
            .Where(b => b != sector.Biome)
            .ToList();

        if (foreignBiomes.Count == 0)
            return;

        var foreignBiome = foreignBiomes[random.Next(foreignBiomes.Count)];

        // Add glitch marker and foreign biome tags
        glitchRoom.AddTag("Glitch");
        glitchRoom.AddTag("Paradox");
        glitchRoom.AddTag($"Fragment:{foreignBiome}");

        if (_glitchTags.TryGetValue(foreignBiome, out var foreignTags))
        {
            // Add 1-2 foreign tags
            var count = random.Next(1, 3);
            var shuffled = foreignTags.OrderBy(_ => random.Next()).Take(count);
            foreach (var tag in shuffled)
            {
                glitchRoom.AddTag(tag);
            }
        }
    }
}
