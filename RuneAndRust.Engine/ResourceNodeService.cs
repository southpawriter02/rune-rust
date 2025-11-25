using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;
using DescriptorResourceNode = RuneAndRust.Core.Descriptors.ResourceNode;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.5: Resource Node Service
/// Generates procedural resource nodes with biome-appropriate distribution
/// </summary>
public class ResourceNodeService
{
    private static readonly ILogger _log = Log.ForContext<ResourceNodeService>();
    private readonly DescriptorRepository _repository;
    private readonly Random _random;

    public ResourceNodeService(DescriptorRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _random = new Random();
        _log.Information("ResourceNodeService initialized");
    }

    /// <summary>
    /// Generates resource nodes for a room based on biome profile
    /// </summary>
    /// <param name="roomId">Room ID to populate</param>
    /// <param name="biomeName">Biome name (e.g., "Muspelheim", "The_Roots")</param>
    /// <param name="roomSize">Room size ("Small", "Medium", "Large")</param>
    /// <returns>List of generated resource nodes</returns>
    public List<DescriptorResourceNode> GenerateResourceNodes(int roomId, string biomeName, string roomSize)
    {
        _log.Debug("Generating resource nodes for Room {RoomId}, Biome: {Biome}, Size: {Size}",
            roomId, biomeName, roomSize);

        // Get biome resource profile
        var profile = _repository.GetBiomeResourceProfile(biomeName);

        if (profile == null)
        {
            _log.Warning("No resource profile found for biome {Biome}, using generic distribution",
                biomeName);
            return GenerateGenericResourceNodes(roomId, roomSize);
        }

        // Determine number of nodes based on room size
        var nodeCount = profile.GetSpawnDensity(roomSize);

        var nodes = new List<DescriptorResourceNode>();

        for (int i = 0; i < nodeCount; i++)
        {
            var node = GenerateNode(roomId, biomeName, profile);
            if (node != null)
            {
                nodes.Add(node);
            }
        }

        _log.Debug("Generated {Count} resource nodes for Room {RoomId}",
            nodes.Count, roomId);

        return nodes;
    }

    /// <summary>
    /// Generates a single resource node for a room
    /// </summary>
    private DescriptorResourceNode? GenerateNode(int roomId, string biomeName, BiomeResourceProfile profile)
    {
        // Roll for rarity tier (70% common, 25% uncommon, 5% rare)
        var rarityRoll = _random.NextDouble();
        var (rarityTier, resourceDef) = SelectResource(profile, rarityRoll);

        if (resourceDef == null)
        {
            _log.Warning("No resource definition selected for Room {RoomId}", roomId);
            return null;
        }

        // Get base template
        var baseTemplate = _repository.GetBaseTemplateByName(resourceDef.Template);

        if (baseTemplate == null)
        {
            _log.Warning("Base template not found: {Template}", resourceDef.Template);
            return null;
        }

        // Get modifier for biome
        var modifier = GetModifierForBiome(biomeName);

        // Instantiate resource node
        var node = InstantiateNode(
            baseTemplate,
            modifier,
            resourceDef.Resource,
            rarityTier,
            roomId,
            biomeName);

        return node;
    }

    /// <summary>
    /// Selects a resource based on rarity roll
    /// </summary>
    private (RarityTier, ResourceDefinition?) SelectResource(
        BiomeResourceProfile profile,
        double rarityRoll)
    {
        // Rarity distribution: 70% common, 25% uncommon, 5% rare, <1% legendary
        if (rarityRoll < 0.70)
        {
            var commonDefs = profile.GetCommonResourceDefinitions();
            return (RarityTier.Common, SelectWeighted(commonDefs));
        }
        else if (rarityRoll < 0.95)
        {
            var uncommonDefs = profile.GetUncommonResourceDefinitions();
            return (RarityTier.Uncommon, SelectWeighted(uncommonDefs));
        }
        else if (rarityRoll < 0.99)
        {
            var rareDefs = profile.GetRareResourceDefinitions();
            return (RarityTier.Rare, SelectWeighted(rareDefs));
        }
        else
        {
            var legendaryDefs = profile.GetLegendaryResourceDefinitions();
            if (legendaryDefs.Count > 0)
            {
                return (RarityTier.Legendary, SelectWeighted(legendaryDefs));
            }
            else
            {
                // Fall back to rare if no legendary defined
                var rareDefs = profile.GetRareResourceDefinitions();
                return (RarityTier.Rare, SelectWeighted(rareDefs));
            }
        }
    }

    /// <summary>
    /// Selects a resource definition using weighted random selection
    /// </summary>
    private ResourceDefinition? SelectWeighted(List<ResourceDefinition> definitions)
    {
        if (definitions.Count == 0)
            return null;

        // Calculate total weight
        float totalWeight = definitions.Sum(d => d.Weight);

        if (totalWeight <= 0)
        {
            // If no weights, select randomly
            return definitions[_random.Next(definitions.Count)];
        }

        // Weighted selection
        float roll = (float)_random.NextDouble() * totalWeight;
        float cumulative = 0;

        foreach (var def in definitions)
        {
            cumulative += def.Weight;
            if (roll <= cumulative)
            {
                return def;
            }
        }

        // Fallback (shouldn't reach here)
        return definitions[^1];
    }

    /// <summary>
    /// Instantiates a resource node from template and parameters
    /// </summary>
    private DescriptorResourceNode InstantiateNode(
        DescriptorBaseTemplate baseTemplate,
        ThematicModifier? modifier,
        string resourceType,
        RarityTier rarityTier,
        int roomId,
        string biomeName)
    {
        // Parse base mechanics
        var mechanics = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            baseTemplate.BaseMechanics ?? "{}");

        // Build node name and description
        var name = BuildNodeName(baseTemplate.NameTemplate, modifier, resourceType);
        var description = BuildNodeDescription(baseTemplate.DescriptionTemplate, modifier, resourceType);

        // Create resource node
        var node = new DescriptorResourceNode
        {
            RoomId = roomId,
            Name = name,
            Description = description,
            NodeType = ParseNodeType(baseTemplate.Archetype),
            ExtractionType = ParseExtractionType(GetMechanicString(mechanics, "extraction_type")),
            ExtractionDC = GetMechanicInt(mechanics, "extraction_dc"),
            ExtractionTime = GetMechanicInt(mechanics, "extraction_time"),
            RequiresTool = GetMechanicBool(mechanics, "requires_tool"),
            RequiredTool = GetRequiredTool(baseTemplate.Archetype),
            YieldMin = GetMechanicInt(mechanics, "yield_min"),
            YieldMax = GetMechanicInt(mechanics, "yield_max"),
            ResourceType = resourceType,
            RarityTier = rarityTier,
            Depleted = false,
            UsesRemaining = GetMechanicInt(mechanics, "uses"),
            MaxUses = GetMechanicInt(mechanics, "uses"),
            Hazardous = GetMechanicBool(mechanics, "hazardous"),
            TrapChance = GetMechanicFloat(mechanics, "trap_chance"),
            Hidden = GetMechanicBool(mechanics, "hidden"),
            DetectionDC = GetMechanicInt(mechanics, "detection_dc", 18),
            Unstable = GetMechanicBool(mechanics, "unstable"),
            RequiresGaldr = GetMechanicBool(mechanics, "requires_galdr"),
            BiomeRestriction = biomeName,
            Tags = baseTemplate.Tags
        };

        _log.Verbose("Instantiated resource node: {Name} ({RarityTier})", name, rarityTier);
        return node;
    }

    #region Helper Methods

    private string BuildNodeName(string template, ThematicModifier? modifier, string resourceType)
    {
        var name = template
            .Replace("{Modifier}", modifier?.ModifierName ?? "")
            .Replace("{Resource_Type}", FormatResourceType(resourceType))
            .Replace("{Wreckage_Type}", FormatResourceType(resourceType))
            .Replace("{Fungus_Type}", FormatResourceType(resourceType))
            .Replace("{Chemical_Type}", FormatResourceType(resourceType))
            .Replace("{Anomaly_Type}", FormatResourceType(resourceType))
            .Replace("{Crystal_Type}", FormatResourceType(resourceType))
            .Replace("{Culture}", FormatResourceType(resourceType));

        return name.Trim();
    }

    private string BuildNodeDescription(string template, ThematicModifier? modifier, string resourceType)
    {
        var description = template
            .Replace("{Modifier_Adj}", modifier?.Adjective ?? "")
            .Replace("{Resource_Type}", FormatResourceType(resourceType).ToLower())
            .Replace("{Modifier_Detail}", modifier?.DetailFragment ?? "")
            .Replace("{Article}", GetArticle(resourceType))
            .Replace("{Article_Cap}", GetArticle(resourceType, capitalize: true));

        return description.Trim();
    }

    private string FormatResourceType(string resourceType)
    {
        return resourceType.Replace("_", " ");
    }

    private string GetArticle(string word, bool capitalize = false)
    {
        var vowels = new[] { 'a', 'e', 'i', 'o', 'u' };
        var firstChar = word.ToLower().FirstOrDefault();
        var article = vowels.Contains(firstChar) ? "an" : "a";

        return capitalize ? article.Substring(0, 1).ToUpper() + article.Substring(1) : article;
    }

    private ThematicModifier? GetModifierForBiome(string biome)
    {
        var modifierName = biome switch
        {
            "Muspelheim" => "Scorched",
            "Niflheim" => "Frozen",
            "The_Roots" => "Rusted",
            "Alfheim" => "Crystalline",
            "Jotunheim" => "Monolithic",
            _ => null
        };

        if (modifierName == null)
            return null;

        return _repository.GetModifierByName(modifierName);
    }

    private ResourceNodeType ParseNodeType(string archetype)
    {
        return archetype switch
        {
            "MineralVein" => ResourceNodeType.MineralVein,
            "SalvageWreckage" => ResourceNodeType.SalvageWreckage,
            "OrganicHarvest" => ResourceNodeType.OrganicHarvest,
            "AethericAnomaly" => ResourceNodeType.AethericAnomaly,
            _ => ResourceNodeType.MineralVein
        };
    }

    private ExtractionType ParseExtractionType(string extractionType)
    {
        return extractionType switch
        {
            "Mining" => ExtractionType.Mining,
            "Salvaging" => ExtractionType.Salvaging,
            "Harvesting" => ExtractionType.Harvesting,
            "Siphoning" => ExtractionType.Siphoning,
            "Search" => ExtractionType.Search,
            _ => ExtractionType.Harvesting
        };
    }

    private string? GetRequiredTool(string archetype)
    {
        return archetype switch
        {
            "MineralVein" => "Mining_Tool",
            "SalvageWreckage" => "Salvage_Kit",
            "AethericAnomaly" => "Aether_Siphon",
            _ => null
        };
    }

    private string GetMechanicString(Dictionary<string, JsonElement> mechanics, string key)
    {
        if (mechanics.TryGetValue(key, out var value))
        {
            return value.GetString() ?? string.Empty;
        }
        return string.Empty;
    }

    private int GetMechanicInt(Dictionary<string, JsonElement> mechanics, string key, int defaultValue = 0)
    {
        if (mechanics.TryGetValue(key, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number)
                return value.GetInt32();
        }
        return defaultValue;
    }

    private float GetMechanicFloat(Dictionary<string, JsonElement> mechanics, string key, float defaultValue = 0)
    {
        if (mechanics.TryGetValue(key, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number)
                return (float)value.GetDouble();
        }
        return defaultValue;
    }

    private bool GetMechanicBool(Dictionary<string, JsonElement> mechanics, string key)
    {
        if (mechanics.TryGetValue(key, out var value))
        {
            if (value.ValueKind == JsonValueKind.True)
                return true;
            if (value.ValueKind == JsonValueKind.False)
                return false;
        }
        return false;
    }

    private List<DescriptorResourceNode> GenerateGenericResourceNodes(int roomId, string roomSize)
    {
        // Fallback generic generation
        var nodeCount = roomSize?.ToLower() switch
        {
            "small" => 0,
            "medium" => 1,
            "large" => 2,
            _ => 1
        };

        var nodes = new List<DescriptorResourceNode>();

        for (int i = 0; i < nodeCount; i++)
        {
            // Create simple generic node
            nodes.Add(new DescriptorResourceNode
            {
                RoomId = roomId,
                Name = "Iron Ore Vein",
                Description = "A vein of iron ore.",
                NodeType = ResourceNodeType.MineralVein,
                ExtractionType = ExtractionType.Mining,
                ExtractionDC = 12,
                ExtractionTime = 2,
                YieldMin = 2,
                YieldMax = 4,
                ResourceType = "Iron_Ore",
                RarityTier = RarityTier.Common,
                UsesRemaining = 3,
                MaxUses = 3
            });
        }

        return nodes;
    }

    #endregion

    #region Validation & Utility

    /// <summary>
    /// Validates that resource node system is properly configured
    /// </summary>
    public bool ValidateResourceNodeSystem()
    {
        _log.Information("Validating resource node system...");

        var profiles = _repository.GetAllBiomeResourceProfiles();
        if (profiles.Count == 0)
        {
            _log.Error("No biome resource profiles found");
            return false;
        }

        _log.Information("Resource node system validation passed: {ProfileCount} profiles",
            profiles.Count);

        foreach (var profile in profiles)
        {
            var common = profile.GetCommonResourceDefinitions().Count;
            var uncommon = profile.GetUncommonResourceDefinitions().Count;
            var rare = profile.GetRareResourceDefinitions().Count;

            _log.Information("  {Biome}: {Common} common, {Uncommon} uncommon, {Rare} rare",
                profile.BiomeName, common, uncommon, rare);
        }

        return true;
    }

    /// <summary>
    /// Gets all available biome names with resource profiles
    /// </summary>
    public List<string> GetAvailableBiomes()
    {
        return _repository.GetAllBiomeResourceProfiles()
            .Select(p => p.BiomeName)
            .ToList();
    }

    #endregion
}
