using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Performance optimization: Caches BiomeElement tables in memory (v0.12)
/// Eliminates database lookups during generation
/// Target: Sub-500ms generation time
/// </summary>
public class BiomeElementCache
{
    private static readonly ILogger _log = Log.ForContext<BiomeElementCache>();
    private static readonly Lazy<BiomeElementCache> _instance =
        new Lazy<BiomeElementCache>(() => new BiomeElementCache());

    private Dictionary<string, BiomeElementTable> _cache = new Dictionary<string, BiomeElementTable>();
    private bool _isInitialized = false;

    private BiomeElementCache()
    {
        // Private constructor for singleton
    }

    public static BiomeElementCache Instance => _instance.Value;

    /// <summary>
    /// Initializes the cache with all biome element tables
    /// Call once at application startup
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            _log.Warning("BiomeElementCache already initialized, skipping");
            return;
        }

        _log.Information("Initializing BiomeElementCache...");
        var startTime = DateTime.UtcNow;

        // Load [The Roots] biome elements
        LoadTheRootsBiome();

        // Future: Load other biomes here
        // LoadTheLowerDepthsBiome();
        // LoadTheVerdantCoreBiome();

        _isInitialized = true;

        var loadTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _log.Information("BiomeElementCache initialized: {BiomeCount} biomes loaded in {LoadTime}ms",
            _cache.Count, loadTime);
    }

    /// <summary>
    /// Gets a biome element table by ID
    /// </summary>
    public BiomeElementTable? GetBiome(string biomeId)
    {
        if (!_isInitialized)
        {
            _log.Warning("BiomeElementCache not initialized, calling Initialize()");
            Initialize();
        }

        if (_cache.TryGetValue(biomeId, out var table))
        {
            return table;
        }

        _log.Error("Biome not found in cache: {BiomeId}", biomeId);
        return null;
    }

    /// <summary>
    /// Clears the cache (for testing or hot-reload scenarios)
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _isInitialized = false;
        _log.Information("BiomeElementCache cleared");
    }

    #region Biome Loading

    /// <summary>
    /// Loads element table for [The Roots] biome (v0.11/v0.12)
    /// </summary>
    private void LoadTheRootsBiome()
    {
        var table = new BiomeElementTable();

        // Enemy Types (v0.11) - adjusted weights from v0.12 balance tuning
        table.Elements.Add(new BiomeElement
        {
            ElementName = "rusted_servitor",
            ElementType = BiomeElementType.DormantProcess,
            Weight = 0.28f, // Was 0.25, increased (most fun enemy)
            AssociatedDataId = "rusted_servitor"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "rust_horror",
            ElementType = BiomeElementType.DormantProcess,
            Weight = 0.22f, // Was 0.20, increased (good variety)
            AssociatedDataId = "rust_horror"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "construction_hauler",
            ElementType = BiomeElementType.DormantProcess,
            Weight = 0.08f, // Was 0.10, decreased (too tanky)
            AssociatedDataId = "construction_hauler"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "haugbui_class",
            ElementType = BiomeElementType.DormantProcess,
            Weight = 0.12f, // Champion enemy
            AssociatedDataId = "haugbui_class"
        });

        // Dynamic Hazards (v0.11) - adjusted weights from v0.12 balance tuning
        table.Elements.Add(new BiomeElement
        {
            ElementName = "steam_vent",
            ElementType = BiomeElementType.DynamicHazard,
            Weight = 0.18f, // Was 0.30, reduced (too common)
            AssociatedDataId = "steam_vent"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "live_power_conduit",
            ElementType = BiomeElementType.DynamicHazard,
            Weight = 0.20f, // Was 0.15, increased (more interesting)
            AssociatedDataId = "live_power_conduit"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "unstable_ceiling",
            ElementType = BiomeElementType.DynamicHazard,
            Weight = 0.12f, // Was 0.10, increased (creates tension)
            AssociatedDataId = "unstable_ceiling"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "toxic_spore_cloud",
            ElementType = BiomeElementType.DynamicHazard,
            Weight = 0.10f, // Was 0.12, reduced (annoying)
            AssociatedDataId = "toxic_spore_cloud"
        });

        // Static Terrain (v0.11)
        table.Elements.Add(new BiomeElement
        {
            ElementName = "rubble_pile",
            ElementType = BiomeElementType.StaticTerrain,
            Weight = 0.40f, // Common cover
            AssociatedDataId = "rubble_pile"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "corroded_pillar",
            ElementType = BiomeElementType.StaticTerrain,
            Weight = 0.25f,
            AssociatedDataId = "corroded_pillar"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "machinery_wreckage",
            ElementType = BiomeElementType.StaticTerrain,
            Weight = 0.30f,
            AssociatedDataId = "machinery_wreckage"
        });

        // Loot Nodes (v0.11) - adjusted weights from v0.12 balance tuning
        table.Elements.Add(new BiomeElement
        {
            ElementName = "resource_vein",
            ElementType = BiomeElementType.LootNode,
            Weight = 0.35f,
            AssociatedDataId = "resource_vein"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "salvageable_wreckage",
            ElementType = BiomeElementType.LootNode,
            Weight = 0.40f,
            AssociatedDataId = "salvageable_wreckage"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "hidden_container",
            ElementType = BiomeElementType.LootNode,
            Weight = 0.08f, // Was 0.05, increased (needs to be found more)
            AssociatedDataId = "hidden_container"
        });

        // Ambient Conditions (v0.11)
        table.Elements.Add(new BiomeElement
        {
            ElementName = "flooded",
            ElementType = BiomeElementType.AmbientCondition,
            Weight = 0.15f,
            AssociatedDataId = "flooded"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "darkness",
            ElementType = BiomeElementType.AmbientCondition,
            Weight = 0.20f,
            AssociatedDataId = "darkness"
        });

        table.Elements.Add(new BiomeElement
        {
            ElementName = "psychic_resonance",
            ElementType = BiomeElementType.AmbientCondition,
            Weight = 0.07f, // Was 0.05, increased (good stress mechanic)
            AssociatedDataId = "psychic_resonance"
        });

        _cache["the_roots"] = table;

        _log.Debug("Loaded The Roots biome: {ElementCount} elements",
            table.Elements.Count);
    }

    #endregion
}
