using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages biome definitions for procedural generation (v0.10)
/// </summary>
public class BiomeLibrary
{
    private static readonly ILogger _log = Log.ForContext<BiomeLibrary>();
    private readonly Dictionary<string, BiomeDefinition> _biomes = new();
    private readonly string _biomeDataPath;

    public BiomeLibrary(string dataPath = "Data/Biomes")
    {
        _biomeDataPath = dataPath;
    }

    /// <summary>
    /// Loads all biome definitions from JSON files
    /// </summary>
    public void LoadBiomes()
    {
        _log.Debug("Loading biomes from: {DataPath}", _biomeDataPath);

        if (!Directory.Exists(_biomeDataPath))
        {
            _log.Warning("Biome data path not found: {DataPath}", _biomeDataPath);
            Console.WriteLine($"Warning: Biome data path not found: {_biomeDataPath}");
            return;
        }

        var biomeFiles = Directory.GetFiles(_biomeDataPath, "*.json", SearchOption.AllDirectories);
        _log.Debug("Found {FileCount} biome files to load", biomeFiles.Length);

        int loadedCount = 0;
        int invalidCount = 0;

        foreach (var file in biomeFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var biome = JsonSerializer.Deserialize<BiomeDefinition>(json);

                if (biome != null && !string.IsNullOrEmpty(biome.BiomeId))
                {
                    if (biome.IsValid())
                    {
                        _biomes[biome.BiomeId] = biome;
                        loadedCount++;

                        _log.Debug("Loaded biome: {BiomeId} ({BiomeName}, Templates={TemplateCount}) from {FileName}",
                            biome.BiomeId, biome.Name, biome.AvailableTemplates.Count, Path.GetFileName(file));
                    }
                    else
                    {
                        invalidCount++;
                        _log.Warning("Biome validation failed for {BiomeId} from {FileName}",
                            biome.BiomeId, Path.GetFileName(file));
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading biome from file: {FileName}", Path.GetFileName(file));
                Console.WriteLine($"Error loading biome from {file}: {ex.Message}");
            }
        }

        _log.Information("Loaded {LoadedCount} valid biomes ({InvalidCount} invalid, {TotalFiles} total files)",
            loadedCount, invalidCount, biomeFiles.Length);
        Console.WriteLine($"Loaded {loadedCount} biomes");
    }

    /// <summary>
    /// Gets a biome by ID
    /// </summary>
    public BiomeDefinition? GetBiome(string biomeId)
    {
        return _biomes.GetValueOrDefault(biomeId);
    }

    /// <summary>
    /// Gets the default biome ("the_roots")
    /// </summary>
    public BiomeDefinition GetDefaultBiome()
    {
        var defaultBiome = GetBiome("the_roots");

        if (defaultBiome == null)
        {
            _log.Warning("Default biome 'the_roots' not found, creating fallback");
            return CreateFallbackBiome();
        }

        return defaultBiome;
    }

    /// <summary>
    /// Gets all loaded biomes
    /// </summary>
    public List<BiomeDefinition> GetAllBiomes()
    {
        return _biomes.Values.ToList();
    }

    /// <summary>
    /// Gets the count of loaded biomes
    /// </summary>
    public int GetBiomeCount()
    {
        return _biomes.Count;
    }

    /// <summary>
    /// Creates a fallback biome if none are loaded
    /// </summary>
    private BiomeDefinition CreateFallbackBiome()
    {
        return new BiomeDefinition
        {
            BiomeId = "fallback",
            Name = "[Fallback Biome]",
            Description = "A generic dungeon environment.",
            AvailableTemplates = new List<string>(), // Will use all templates
            DescriptorCategories = new Dictionary<string, List<string>>
            {
                ["Adjectives"] = new List<string> { "Dark", "Empty", "Ancient" },
                ["Details"] = new List<string> { "Dust covers the floor", "Shadows fill the corners" }
            },
            MinRoomCount = 5,
            MaxRoomCount = 7,
            BranchingProbability = 0.4f,
            SecretRoomProbability = 0.2f
        };
    }

    /// <summary>
    /// Validates that the library has usable biomes
    /// </summary>
    public bool ValidateLibrary()
    {
        if (_biomes.Count == 0)
        {
            _log.Error("No biomes loaded");
            return false;
        }

        // Check for default biome
        if (GetBiome("the_roots") == null)
        {
            _log.Warning("Default biome 'the_roots' not found");
        }

        return true;
    }
}
