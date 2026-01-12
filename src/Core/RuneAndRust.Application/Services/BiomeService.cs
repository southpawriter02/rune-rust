using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for biome management and selection.
/// </summary>
public class BiomeService : IBiomeService
{
    private readonly Dictionary<string, BiomeDefinition> _biomes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISeededRandomService _random;
    private readonly ILogger<BiomeService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    /// <summary>
    /// Default biome ID used when no valid biomes are found.
    /// </summary>
    public const string DefaultBiomeId = "stone-corridors";

    public BiomeService(
        ISeededRandomService random,
        ILogger<BiomeService>? logger = null,
        IGameEventLogger? eventLogger = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<BiomeService>.Instance;
        _eventLogger = eventLogger;

        RegisterDefaultBiomes();
        _logger.LogDebug("BiomeService initialized with {Count} biomes", _biomes.Count);
    }

    /// <inheritdoc/>
    public BiomeDefinition? GetBiome(string biomeId)
    {
        return _biomes.TryGetValue(biomeId, out var biome) ? biome : null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<BiomeDefinition> GetBiomesForDepth(int depth)
    {
        return _biomes.Values
            .Where(b => b.IsValidForDepth(depth))
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<BiomeDefinition> GetBiomesByTags(IEnumerable<string> tags, bool matchAll = true)
    {
        var tagList = tags.ToList();
        return _biomes.Values
            .Where(b => matchAll ? b.HasAllTags(tagList) : tagList.Any(b.HasTag))
            .ToList();
    }

    /// <inheritdoc/>
    public BiomeDefinition SelectBiomeForPosition(Position3D position)
    {
        var depth = Math.Abs(position.Z);
        var validBiomes = GetBiomesForDepth(depth);

        if (validBiomes.Count == 0)
        {
            _logger.LogWarning("No valid biomes for depth {Depth}, using default", depth);
            return GetBiome(DefaultBiomeId) ?? throw new InvalidOperationException("Default biome not found");
        }

        var weightedItems = validBiomes
            .Select(b => (b, b.BaseWeight))
            .ToList();

        var selected = _random.SelectWeighted(position, weightedItems, "biome_selection");

        _logger.LogDebug(
            "Selected biome {BiomeId} for position {Position} (depth={Depth})",
            selected.BiomeId, position, depth);

        _eventLogger?.LogSystem("BiomeSelected", $"Selected biome {selected.Name}",
            data: new Dictionary<string, object>
            {
                ["biomeId"] = selected.BiomeId,
                ["biomeName"] = selected.Name,
                ["position"] = position.ToString(),
                ["depth"] = depth,
                ["candidateCount"] = validBiomes.Count
            });

        return selected;
    }

    /// <inheritdoc/>
    public string? GetRandomDescriptor(string biomeId, string category, Position3D position)
    {
        var biome = GetBiome(biomeId);
        if (biome == null)
            return null;

        var seed = _random.NextForPosition(position, $"descriptor_{category}");
        var random = new Random(seed);

        return biome.Descriptors.GetRandom(category, random);
    }

    /// <inheritdoc/>
    public void RegisterBiome(BiomeDefinition biome)
    {
        ArgumentNullException.ThrowIfNull(biome);
        _biomes[biome.BiomeId] = biome;
        _logger.LogDebug("Registered biome: {BiomeId}", biome.BiomeId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<BiomeDefinition> GetAllBiomes() => _biomes.Values.ToList();

    /// <summary>
    /// Registers default biomes.
    /// </summary>
    private void RegisterDefaultBiomes()
    {
        // Stone Corridors - starter biome (depth 0-2)
        RegisterBiome(BiomeDefinition.Create(
            "stone-corridors",
            "Stone Corridors",
            "Ancient hewn stone passages, worn smooth by centuries of use.",
            minDepth: 0,
            maxDepth: 2,
            baseWeight: 100,
            tags: ["underground", "constructed"],
            descriptors: new BiomeDescriptors
            {
                Atmospheric = ["dusty", "echoing", "ancient"],
                SensoryVisual = ["rough-hewn walls", "worn flagstones", "iron sconces"],
                SensoryAudio = ["dripping water", "distant echoes", "creaking timbers"],
                Lighting = ["dim torchlight", "flickering shadows", "pools of darkness"]
            }));

        // Fungal Caverns - mid-depth (depth 2-5)
        RegisterBiome(BiomeDefinition.Create(
            "fungal-caverns",
            "Fungal Caverns",
            "Vast caves carpeted with bioluminescent fungi and strange growths.",
            minDepth: 2,
            maxDepth: 5,
            baseWeight: 80,
            tags: ["underground", "natural", "fungal"],
            descriptors: new BiomeDescriptors
            {
                Atmospheric = ["humid", "organic", "otherworldly"],
                SensoryVisual = ["glowing mushrooms", "phosphorescent moss", "towering fungi"],
                SensorySmell = ["earthy spores", "musty decay", "sweet fungal essence"],
                Lighting = ["soft bioluminescence", "pulsing glows", "eerie radiance"]
            },
            rules: new BiomeRules { VisibilityModifier = 0.8f }));

        // Flooded Depths - deep (depth 4+)
        RegisterBiome(BiomeDefinition.Create(
            "flooded-depths",
            "Flooded Depths",
            "Partially submerged chambers where darkness pools like water.",
            minDepth: 4,
            maxDepth: null,
            baseWeight: 60,
            tags: ["underground", "aquatic", "hazardous"],
            descriptors: new BiomeDescriptors
            {
                Atmospheric = ["damp", "oppressive", "treacherous"],
                SensoryVisual = ["murky water", "slick walls", "submerged passages"],
                SensoryAudio = ["lapping water", "bubbling depths", "echoing drips"],
                Surfaces = ["flooded floor", "waterlogged debris", "dripping ceiling"]
            },
            rules: new BiomeRules { MovementModifier = 0.7f, VisibilityModifier = 0.6f }));
    }
}
