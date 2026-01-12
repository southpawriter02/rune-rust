using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for architectural style management.
/// </summary>
public class ArchitecturalStyleService : IArchitecturalStyleService
{
    private readonly Dictionary<string, ArchitecturalStyle> _styles = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISeededRandomService _random;
    private readonly ILogger<ArchitecturalStyleService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public ArchitecturalStyleService(
        ISeededRandomService random,
        ILogger<ArchitecturalStyleService>? logger = null,
        IGameEventLogger? eventLogger = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ArchitecturalStyleService>.Instance;
        _eventLogger = eventLogger;

        RegisterDefaultStyles();
        _logger.LogDebug("ArchitecturalStyleService initialized with {Count} styles", _styles.Count);
    }

    /// <inheritdoc/>
    public ArchitecturalStyle? GetStyle(string styleId) =>
        _styles.TryGetValue(styleId, out var style) ? style : null;

    /// <inheritdoc/>
    public IReadOnlyList<ArchitecturalStyle> GetStylesForBiome(string biomeId) =>
        _styles.Values.Where(s => s.IsCompatibleWith(biomeId)).ToList();

    /// <inheritdoc/>
    public IReadOnlyList<ArchitecturalStyle> GetStylesForDepth(int depth) =>
        _styles.Values.Where(s => s.IsValidAtDepth(depth)).ToList();

    /// <inheritdoc/>
    public string SelectStyleForPosition(Position3D position, string biomeId)
    {
        var depth = Math.Abs(position.Z);
        var validStyles = _styles.Values
            .Where(s => s.IsCompatibleWith(biomeId) && s.IsValidAtDepth(depth))
            .ToList();

        if (validStyles.Count == 0)
        {
            _logger.LogDebug("No valid styles for biome {Biome} at depth {Depth}, using default",
                biomeId, depth);
            return "rough-hewn";
        }

        var weighted = validStyles.Select(s => (s.StyleId, s.Rules.BaseWeight)).ToList();
        var selectedStyle = _random.SelectWeighted(position, weighted, "style_selection");

        _eventLogger?.LogEnvironment("StyleSelected", $"Selected {selectedStyle} for position",
            data: new Dictionary<string, object>
            {
                ["styleId"] = selectedStyle,
                ["biomeId"] = biomeId,
                ["depth"] = depth,
                ["validStyleCount"] = validStyles.Count
            });

        return selectedStyle;
    }

    /// <inheritdoc/>
    public string? GetRandomDescriptor(string styleId, string category, Position3D position)
    {
        var style = GetStyle(styleId);
        if (style == null) return null;

        var seed = _random.NextForPosition(position, "descriptor_" + category);
        var rng = new Random(seed);
        return style.Descriptors.GetRandom(category, rng);
    }

    /// <inheritdoc/>
    public void RegisterStyle(ArchitecturalStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        _styles[style.StyleId] = style;
        _logger.LogDebug("Registered architectural style: {StyleId}", style.StyleId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ArchitecturalStyle> GetAllStyles() => _styles.Values.ToList();

    private void RegisterDefaultStyles()
    {
        // Rough-Hewn (universal, common)
        RegisterStyle(ArchitecturalStyle.Create(
            "rough-hewn", "Rough-Hewn",
            "Crudely carved tunnels and chambers.",
            descriptors: StyleDescriptors.RoughHewn,
            rules: StyleRules.Cramped));

        // Ornate Temple (deeper areas)
        RegisterStyle(ArchitecturalStyle.Create(
            "ornate-temple", "Ornate Temple",
            "Sacred halls with intricate decorations.",
            minDepth: 3,
            descriptors: StyleDescriptors.OrnateTemple,
            rules: StyleRules.Grand));

        // Flooded Cistern (flooded biomes)
        RegisterStyle(ArchitecturalStyle.Create(
            "flooded-cistern", "Flooded Cistern",
            "Water-damaged chambers with dripping walls.",
            compatibleBiomes: ["flooded-depths"],
            descriptors: new StyleDescriptors
            {
                Walls = ["waterlogged brickwork", "algae-covered stone", "crumbling plaster"],
                Floors = ["submerged tiles", "flooded walkways", "rotting wooden planks"],
                Ceilings = ["dripping vault", "water-stained arches", "collapsed sections"],
                Passages = ["flooded archway", "waterfall curtain", "submerged tunnel"],
                Decorations = ["broken fountain", "flooded basin", "waterlogged crates"]
            },
            rules: new StyleRules
            {
                MinRoomSize = 4,
                MaxRoomSize = 12,
                PrefersRegularShapes = true,
                CommonFeatures = ["water-pool", "drain", "spillway"],
                BaseWeight = 100
            }));

        // Fungal Growth (fungal biomes)
        RegisterStyle(ArchitecturalStyle.Create(
            "fungal-growth", "Fungal Growth",
            "Organic chambers overtaken by fungi.",
            compatibleBiomes: ["fungal-caverns"],
            descriptors: new StyleDescriptors
            {
                Walls = ["spore-covered surfaces", "bioluminescent patches", "fungal tendrils"],
                Floors = ["spongy mycelium", "mushroom clusters", "decomposing matter"],
                Ceilings = ["hanging caps", "glowing spores", "organic stalactites"],
                Passages = ["organic archway", "fungal curtain", "spore tunnel"],
                Decorations = ["giant mushroom", "spore pod", "luminescent patch"]
            },
            rules: new StyleRules
            {
                MinRoomSize = 3,
                MaxRoomSize = 8,
                PrefersRegularShapes = false,
                CommonFeatures = ["mushroom-cluster", "spore-cloud", "mycelium-mat"],
                BaseWeight = 100
            }));
    }
}
