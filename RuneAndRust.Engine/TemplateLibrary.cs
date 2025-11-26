using RuneAndRust.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages room template loading and retrieval for procedural generation (v0.10)
/// </summary>
public class TemplateLibrary
{
    private static readonly ILogger _log = Log.ForContext<TemplateLibrary>();
    private readonly Dictionary<string, RoomTemplate> _templates = new();
    private readonly string _templateDataPath;

    public TemplateLibrary(string dataPath = "Data/RoomTemplates")
    {
        _templateDataPath = dataPath;
    }

    /// <summary>
    /// Loads all room template definitions from JSON files
    /// </summary>
    /// <param name="dataPath">Optional path to load templates from. If not provided, uses the default path.</param>
    public void LoadTemplates(string? dataPath = null)
    {
        var pathToUse = dataPath ?? _templateDataPath;
        _log.Debug("Loading room templates from: {DataPath}", pathToUse);

        if (!Directory.Exists(pathToUse))
        {
            _log.Warning("Template data path not found: {DataPath}", pathToUse);
            Console.WriteLine($"Warning: Template data path not found: {pathToUse}");
            return;
        }

        // Load templates from all subdirectories
        var templateFiles = Directory.GetFiles(pathToUse, "*.json", SearchOption.AllDirectories);
        _log.Debug("Found {FileCount} template files to load", templateFiles.Length);

        int loadedCount = 0;
        int invalidCount = 0;

        // Configure JSON options to handle string enum values in template files
        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        foreach (var file in templateFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var template = JsonSerializer.Deserialize<RoomTemplate>(json, jsonOptions);

                if (template != null && !string.IsNullOrEmpty(template.TemplateId))
                {
                    if (template.IsValid())
                    {
                        _templates[template.TemplateId] = template;
                        loadedCount++;

                        _log.Debug("Loaded template: {TemplateId} (Archetype={Archetype}, Difficulty={Difficulty}) from {FileName}",
                            template.TemplateId, template.Archetype, template.Difficulty, Path.GetFileName(file));
                    }
                    else
                    {
                        invalidCount++;
                        _log.Warning("Template validation failed for {TemplateId} from {FileName}",
                            template.TemplateId, Path.GetFileName(file));
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading template from file: {FileName}", Path.GetFileName(file));
                Console.WriteLine($"Error loading template from {file}: {ex.Message}");
            }
        }

        _log.Information("Loaded {LoadedCount} valid templates ({InvalidCount} invalid, {TotalFiles} total files)",
            loadedCount, invalidCount, templateFiles.Length);
        Console.WriteLine($"Loaded {loadedCount} room templates");
    }

    /// <summary>
    /// Gets a template by ID
    /// </summary>
    public RoomTemplate? GetTemplate(string templateId)
    {
        return _templates.GetValueOrDefault(templateId);
    }

    /// <summary>
    /// Gets all templates matching a specific archetype
    /// </summary>
    public List<RoomTemplate> GetTemplatesByArchetype(RoomArchetype archetype)
    {
        return _templates.Values
            .Where(t => t.Archetype == archetype)
            .ToList();
    }

    /// <summary>
    /// Gets all templates matching a specific archetype and difficulty
    /// </summary>
    public List<RoomTemplate> GetTemplatesByArchetypeAndDifficulty(RoomArchetype archetype, RoomDifficulty difficulty)
    {
        return _templates.Values
            .Where(t => t.Archetype == archetype && t.Difficulty == difficulty)
            .ToList();
    }

    /// <summary>
    /// Gets all templates for a specific biome
    /// </summary>
    public List<RoomTemplate> GetTemplatesByBiome(string biome)
    {
        return _templates.Values
            .Where(t => t.Biome.Equals(biome, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets templates that can connect to a specific archetype
    /// </summary>
    public List<RoomTemplate> GetTemplatesConnectingTo(RoomArchetype targetArchetype)
    {
        return _templates.Values
            .Where(t => t.CanConnectTo(targetArchetype))
            .ToList();
    }

    /// <summary>
    /// Gets a random template matching the specified archetype
    /// </summary>
    public RoomTemplate? GetRandomTemplate(Random rng, RoomArchetype archetype)
    {
        var matches = GetTemplatesByArchetype(archetype);

        if (matches.Count == 0)
        {
            _log.Warning("No templates found for archetype: {Archetype}", archetype);
            return null;
        }

        return matches[rng.Next(matches.Count)];
    }

    /// <summary>
    /// Gets a random template matching the specified archetype and difficulty
    /// </summary>
    public RoomTemplate? GetRandomTemplate(Random rng, RoomArchetype archetype, RoomDifficulty difficulty)
    {
        var matches = GetTemplatesByArchetypeAndDifficulty(archetype, difficulty);

        if (matches.Count == 0)
        {
            // Fallback: Try getting any template of the archetype regardless of difficulty
            _log.Debug("No templates found for {Archetype}/{Difficulty}, falling back to archetype only",
                archetype, difficulty);
            return GetRandomTemplate(rng, archetype);
        }

        return matches[rng.Next(matches.Count)];
    }

    /// <summary>
    /// Gets all available templates (for debugging/testing)
    /// </summary>
    public List<RoomTemplate> GetAllTemplates()
    {
        return _templates.Values.ToList();
    }

    /// <summary>
    /// Gets the count of loaded templates
    /// </summary>
    public int GetTemplateCount()
    {
        return _templates.Count;
    }

    /// <summary>
    /// Gets statistics about loaded templates
    /// </summary>
    public Dictionary<string, int> GetTemplateStatistics()
    {
        var stats = new Dictionary<string, int>
        {
            ["Total"] = _templates.Count,
            ["EntryHalls"] = GetTemplatesByArchetype(RoomArchetype.EntryHall).Count,
            ["Corridors"] = GetTemplatesByArchetype(RoomArchetype.Corridor).Count,
            ["Chambers"] = GetTemplatesByArchetype(RoomArchetype.Chamber).Count,
            ["Junctions"] = GetTemplatesByArchetype(RoomArchetype.Junction).Count,
            ["BossArenas"] = GetTemplatesByArchetype(RoomArchetype.BossArena).Count,
            ["SecretRooms"] = GetTemplatesByArchetype(RoomArchetype.SecretRoom).Count
        };

        return stats;
    }

    /// <summary>
    /// Validates that the template library has sufficient templates for generation
    /// </summary>
    public bool ValidateLibrary()
    {
        bool isValid = true;

        // Check for at least one template of each critical archetype
        var requiredArchetypes = new[]
        {
            RoomArchetype.EntryHall,
            RoomArchetype.Corridor,
            RoomArchetype.Chamber,
            RoomArchetype.BossArena
        };

        foreach (var archetype in requiredArchetypes)
        {
            var count = GetTemplatesByArchetype(archetype).Count;
            if (count == 0)
            {
                _log.Error("Missing required template archetype: {Archetype}", archetype);
                isValid = false;
            }
            else
            {
                _log.Debug("Found {Count} templates for {Archetype}", count, archetype);
            }
        }

        return isValid;
    }
}
