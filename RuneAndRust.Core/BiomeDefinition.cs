namespace RuneAndRust.Core;

/// <summary>
/// Defines a biome for procedural generation (v0.10)
/// Acts as a style guide for the Dynamic Room Engine
/// </summary>
public class BiomeDefinition
{
    // Identity
    public string BiomeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Template Pool (v0.10)
    public List<string> AvailableTemplates { get; set; } = new();

    // Aesthetic Properties for Description Generation (v0.10)
    public Dictionary<string, List<string>> DescriptorCategories { get; set; } = new();
    // Examples:
    // "Adjectives": ["Corroded", "Decaying", "Twisted"]
    // "Details": ["Runic glyphs flicker weakly", "Condensation drips from pipes"]
    // "Sounds": ["hissing steam", "groaning metal"]
    // "Smells": ["ozone", "rust", "decay"]

    // Generation Parameters (v0.10)
    public int MinRoomCount { get; set; } = 5;
    public int MaxRoomCount { get; set; } = 7;
    public float BranchingProbability { get; set; } = 0.4f; // 40% chance per eligible node
    public float SecretRoomProbability { get; set; } = 0.2f; // 20% chance

    // v0.11+ Properties (prepared for future use)
    public BiomeElementTable? Elements { get; set; } = null;

    /// <summary>
    /// Gets a random adjective from this biome
    /// </summary>
    public string? GetRandomAdjective(Random rng)
    {
        if (!DescriptorCategories.TryGetValue("Adjectives", out var adjectives) || adjectives.Count == 0)
            return null;

        return adjectives[rng.Next(adjectives.Count)];
    }

    /// <summary>
    /// Gets a random detail from this biome
    /// </summary>
    public string? GetRandomDetail(Random rng)
    {
        if (!DescriptorCategories.TryGetValue("Details", out var details) || details.Count == 0)
            return null;

        return details[rng.Next(details.Count)];
    }

    /// <summary>
    /// Gets a random ambient sound from this biome
    /// </summary>
    public string? GetRandomSound(Random rng)
    {
        if (!DescriptorCategories.TryGetValue("Sounds", out var sounds) || sounds.Count == 0)
            return null;

        return sounds[rng.Next(sounds.Count)];
    }

    /// <summary>
    /// Validates that the biome has sufficient data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(BiomeId)) return false;
        if (string.IsNullOrEmpty(Name)) return false;
        if (AvailableTemplates.Count == 0) return false;
        if (MinRoomCount < 3) return false;
        if (MaxRoomCount < MinRoomCount) return false;

        return true;
    }
}

/// <summary>
/// Biome element table for v0.11+ (enemy/hazard/loot population)
/// Implements the v2.0 data-driven population system
/// </summary>
public class BiomeElementTable
{
    public List<BiomeElement> Elements { get; set; } = new();

    /// <summary>
    /// Gets all elements of a specific type
    /// </summary>
    public List<BiomeElement> GetElementsByType(BiomeElementType type)
    {
        return Elements.Where(e => e.ElementType == type).ToList();
    }

    /// <summary>
    /// Gets all elements that meet spawn rules for a given room
    /// </summary>
    public List<BiomeElement> GetEligibleElements(BiomeElementType type, Room room, Random rng)
    {
        return Elements
            .Where(e => e.ElementType == type && e.MeetsSpawnRules(room, rng))
            .ToList();
    }

    /// <summary>
    /// Performs weighted random selection from a list of elements
    /// </summary>
    public BiomeElement? WeightedRandomSelection(List<BiomeElement> elements, Random rng)
    {
        if (elements.Count == 0) return null;

        // Calculate total weight
        float totalWeight = elements.Sum(e => e.Weight);
        if (totalWeight <= 0) return null;

        // Random selection
        float randomValue = (float)(rng.NextDouble() * totalWeight);
        float cumulativeWeight = 0f;

        foreach (var element in elements)
        {
            cumulativeWeight += element.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return element;
            }
        }

        // Fallback to last element (should never happen)
        return elements[^1];
    }
}

/// <summary>
/// Individual element that can appear in a biome (v0.11+)
/// Represents enemies, hazards, terrain, loot nodes per v2.0 spec
/// </summary>
public class BiomeElement
{
    public string ElementName { get; set; } = string.Empty;
    public BiomeElementType ElementType { get; set; } = BiomeElementType.DescriptionDetail;
    public float Weight { get; set; } = 1.0f;
    public string? AssociatedDataId { get; set; } = null;
    public SpawnRules? SpawnRules { get; set; } = null;

    // Spawn budget (for enemies)
    public int SpawnCost { get; set; } = 1;

    /// <summary>
    /// Checks if this element meets spawn rules for a given room
    /// </summary>
    public bool MeetsSpawnRules(Room room, Random rng)
    {
        if (SpawnRules == null) return true; // No restrictions

        // Room size restrictions
        if (SpawnRules.OnlyInLargeRooms && room.GeneratedNodeType != NodeType.Boss &&
            room.GeneratedNodeType != NodeType.Main)
        {
            return false;
        }

        // Room archetype restrictions
        if (SpawnRules.RequiredArchetype != null)
        {
            // Would need room archetype tracking - for now, skip
            // Could be enhanced by storing archetype on Room
        }

        // Never spawn in certain rooms
        if (SpawnRules.NeverInEntryHall && room.IsStartRoom)
        {
            return false;
        }

        if (SpawnRules.NeverInBossArena && room.IsBossRoom)
        {
            return false;
        }

        // Secret room weight modifier
        if (SpawnRules.HigherWeightInSecretRooms && room.GeneratedNodeType == NodeType.Secret)
        {
            // Weight is already applied in selection algorithm
            return true;
        }

        // Conditional spawning (e.g., "only if Rust-Horror present")
        if (SpawnRules.RequiresEnemyType != null)
        {
            bool hasRequiredEnemy = room.Enemies.Any(e => e.Type.ToString() == SpawnRules.RequiresEnemyType);
            if (!hasRequiredEnemy) return false;
        }

        return true;
    }
}

/// <summary>
/// Spawn rules for BiomeElements (v0.11)
/// Defines constraints and conditions for element placement
/// </summary>
public class SpawnRules
{
    // Size constraints
    public bool OnlyInLargeRooms { get; set; } = false;
    public bool OnlyInSmallRooms { get; set; } = false;

    // Archetype constraints
    public string? RequiredArchetype { get; set; } = null; // "Chamber", "Corridor", etc.
    public bool NeverInEntryHall { get; set; } = false;
    public bool NeverInBossArena { get; set; } = false;
    public bool NeverInSecretRooms { get; set; } = false;

    // Weight modifiers
    public bool HigherWeightInSecretRooms { get; set; } = false;
    public float SecretRoomWeightMultiplier { get; set; } = 2.0f;

    // Conditional spawning
    public string? RequiresEnemyType { get; set; } = null; // "RustHorror" - must have this enemy
    public string? RequiresHazardType { get; set; } = null; // "SteamVent" - must have this hazard
    public string? RequiresCondition { get; set; } = null; // "Flooded" - must have this ambient condition

    // Thematic constraints
    public List<string>? RequiresRoomNameContains { get; set; } = null; // ["Geothermal", "Pumping"]
}

/// <summary>
/// Types of elements that can appear in biomes (v0.10 - v0.12)
/// </summary>
public enum BiomeElementType
{
    // v0.10 (used now)
    RoomTemplate,
    DescriptionDetail,
    AmbientCondition,

    // v0.11 (population phase)
    DormantProcess,      // Enemy spawn
    DynamicHazard,       // Environmental dangers
    StaticTerrain,       // Cover, chasms, etc.
    LootNode,            // Resource veins, containers

    // v0.12 (polish phase)
    CoherentGlitchRule   // Environmental storytelling rules
}
