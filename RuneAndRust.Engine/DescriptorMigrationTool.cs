using RuneAndRust.Core.Descriptors;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38: Migration tool for converting existing biome content to the Descriptor Framework
/// Analyzes v0.29-v0.32 biome implementations and suggests descriptor migrations
/// </summary>
public class DescriptorMigrationTool
{
    private static readonly ILogger _log = Log.ForContext<DescriptorMigrationTool>();
    private readonly DescriptorRepository _descriptorRepository;
    private readonly string _connectionString;

    public DescriptorMigrationTool(DescriptorRepository descriptorRepository, string connectionString)
    {
        _descriptorRepository = descriptorRepository;
        _connectionString = connectionString;
        _log.Information("DescriptorMigrationTool initialized");
    }

    /// <summary>
    /// Analyzes existing biome room templates and suggests descriptor migrations
    /// </summary>
    public BiomeMigrationReport AnalyzeBiomeRoomTemplates(int biomeId, string biomeName)
    {
        _log.Information("Analyzing biome room templates: {BiomeId} ({BiomeName})", biomeId, biomeName);

        var report = new BiomeMigrationReport
        {
            BiomeId = biomeId,
            BiomeName = biomeName,
            Suggestions = new List<MigrationSuggestion>()
        };

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
        connection.Open();

        // Query existing room templates
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                template_id, template_name, template_description,
                room_size_category, hazard_density
            FROM Biome_RoomTemplates
            WHERE biome_id = $biomeId
        ";
        command.Parameters.AddWithValue("$biomeId", biomeId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var templateName = reader.GetString(1);
            var description = reader.GetString(2);
            var roomSize = reader.GetString(3);
            var hazardDensity = reader.GetString(4);

            // Analyze template name for patterns
            var suggestion = AnalyzeRoomTemplateName(templateName, description, biomeName);
            if (suggestion != null)
            {
                report.Suggestions.Add(suggestion);
            }
        }

        _log.Information("Analysis complete: {SuggestionCount} suggestions generated",
            report.Suggestions.Count);

        return report;
    }

    /// <summary>
    /// Analyzes existing biome environmental features and suggests descriptor migrations
    /// </summary>
    public BiomeMigrationReport AnalyzeBiomeEnvironmentalFeatures(int biomeId, string biomeName)
    {
        _log.Information("Analyzing biome environmental features: {BiomeId} ({BiomeName})", biomeId, biomeName);

        var report = new BiomeMigrationReport
        {
            BiomeId = biomeId,
            BiomeName = biomeName,
            Suggestions = new List<MigrationSuggestion>()
        };

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
        connection.Open();

        // Query existing environmental features
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                feature_id, feature_name, feature_description, feature_type,
                damage_dice, damage_type
            FROM Biome_EnvironmentalFeatures
            WHERE biome_id = $biomeId
        ";
        command.Parameters.AddWithValue("$biomeId", biomeId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var featureName = reader.GetString(1);
            var description = reader.GetString(2);
            var featureType = reader.GetString(3);

            // Analyze feature name for patterns
            var suggestion = AnalyzeFeatureName(featureName, description, featureType, biomeName);
            if (suggestion != null)
            {
                report.Suggestions.Add(suggestion);
            }
        }

        _log.Information("Analysis complete: {SuggestionCount} suggestions generated",
            report.Suggestions.Count);

        return report;
    }

    /// <summary>
    /// Analyzes a room template name and suggests a descriptor migration
    /// </summary>
    private MigrationSuggestion? AnalyzeRoomTemplateName(
        string templateName,
        string description,
        string biomeName)
    {
        // Common room archetypes
        var archetypes = new Dictionary<string, string>
        {
            { "corridor", "Corridor_Base" },
            { "passage", "Corridor_Base" },
            { "chamber", "Chamber_Base" },
            { "hall", "Hall_Base" },
            { "junction", "Junction_Base" },
            { "bay", "Bay_Base" },
            { "platform", "Platform_Base" },
            { "tunnel", "Tunnel_Base" }
        };

        // Find matching archetype
        string? suggestedBaseTemplate = null;
        foreach (var kvp in archetypes)
        {
            if (templateName.ToLower().Contains(kvp.Key))
            {
                suggestedBaseTemplate = kvp.Value;
                break;
            }
        }

        if (suggestedBaseTemplate == null)
        {
            _log.Debug("No archetype match for room template: {TemplateName}", templateName);
            return null;
        }

        // Determine modifier based on biome-specific adjectives in the name
        string? suggestedModifier = null;

        if (templateName.Contains("Scorched", StringComparison.OrdinalIgnoreCase) ||
            templateName.Contains("Heat", StringComparison.OrdinalIgnoreCase) ||
            templateName.Contains("Geothermal", StringComparison.OrdinalIgnoreCase))
        {
            suggestedModifier = "Scorched";
        }
        else if (templateName.Contains("Frozen", StringComparison.OrdinalIgnoreCase) ||
                 templateName.Contains("Ice", StringComparison.OrdinalIgnoreCase) ||
                 templateName.Contains("Cryo", StringComparison.OrdinalIgnoreCase))
        {
            suggestedModifier = "Frozen";
        }
        else if (templateName.Contains("Rusted", StringComparison.OrdinalIgnoreCase) ||
                 templateName.Contains("Corroded", StringComparison.OrdinalIgnoreCase) ||
                 templateName.Contains("Decayed", StringComparison.OrdinalIgnoreCase))
        {
            suggestedModifier = "Rusted";
        }
        else if (templateName.Contains("Crystal", StringComparison.OrdinalIgnoreCase) ||
                 templateName.Contains("Glowing", StringComparison.OrdinalIgnoreCase) ||
                 templateName.Contains("Radiant", StringComparison.OrdinalIgnoreCase))
        {
            suggestedModifier = "Crystalline";
        }

        return new MigrationSuggestion
        {
            OriginalName = templateName,
            OriginalDescription = description,
            SuggestedBaseTemplate = suggestedBaseTemplate,
            SuggestedModifier = suggestedModifier,
            Category = "Room",
            Archetype = suggestedBaseTemplate.Replace("_Base", ""),
            ConfidenceScore = suggestedModifier != null ? 0.9f : 0.6f,
            MigrationNotes = $"Detected room archetype: {suggestedBaseTemplate}"
        };
    }

    /// <summary>
    /// Analyzes a feature name and suggests a descriptor migration
    /// </summary>
    private MigrationSuggestion? AnalyzeFeatureName(
        string featureName,
        string description,
        string featureType,
        string biomeName)
    {
        // Common feature archetypes
        var archetypes = new Dictionary<string, string>
        {
            { "pillar", "Pillar_Base" },
            { "column", "Pillar_Base" },
            { "rubble", "Rubble_Base" },
            { "debris", "Debris_Base" },
            { "wreckage", "Wreckage_Base" },
            { "machinery", "Machinery_Base" },
            { "vent", "Vent_Base" },
            { "conduit", "Conduit_Base" },
            { "pipe", "Pipe_Base" },
            { "chasm", "Chasm_Base" },
            { "pit", "Pit_Base" }
        };

        // Find matching archetype
        string? suggestedBaseTemplate = null;
        foreach (var kvp in archetypes)
        {
            if (featureName.ToLower().Contains(kvp.Key))
            {
                suggestedBaseTemplate = kvp.Value;
                break;
            }
        }

        if (suggestedBaseTemplate == null)
        {
            _log.Debug("No archetype match for feature: {FeatureName}", featureName);
            return null;
        }

        // Determine modifier based on biome-specific adjectives
        string? suggestedModifier = DetermineModifierFromName(featureName);

        // Determine archetype type
        string archetype = featureType switch
        {
            "Hazard" => "Hazard",
            "Terrain" => "Cover",
            _ => "Feature"
        };

        return new MigrationSuggestion
        {
            OriginalName = featureName,
            OriginalDescription = description,
            SuggestedBaseTemplate = suggestedBaseTemplate,
            SuggestedModifier = suggestedModifier,
            Category = "Feature",
            Archetype = archetype,
            ConfidenceScore = suggestedModifier != null ? 0.85f : 0.5f,
            MigrationNotes = $"Detected feature type: {featureType}, archetype: {suggestedBaseTemplate}"
        };
    }

    /// <summary>
    /// Determines the appropriate modifier based on descriptive words in the name
    /// </summary>
    private string? DetermineModifierFromName(string name)
    {
        var nameLower = name.ToLower();

        // Scorched (Muspelheim)
        if (nameLower.Contains("scorched") || nameLower.Contains("molten") ||
            nameLower.Contains("heat") || nameLower.Contains("lava") ||
            nameLower.Contains("burning") || nameLower.Contains("charred"))
        {
            return "Scorched";
        }

        // Frozen (Niflheim)
        if (nameLower.Contains("frozen") || nameLower.Contains("ice") ||
            nameLower.Contains("frost") || nameLower.Contains("cryo") ||
            nameLower.Contains("glacial") || nameLower.Contains("chilled"))
        {
            return "Frozen";
        }

        // Rusted (The Roots)
        if (nameLower.Contains("rusted") || nameLower.Contains("corroded") ||
            nameLower.Contains("decayed") || nameLower.Contains("rotted") ||
            nameLower.Contains("deteriorated") || nameLower.Contains("brittle"))
        {
            return "Rusted";
        }

        // Crystalline (Alfheim)
        if (nameLower.Contains("crystal") || nameLower.Contains("glowing") ||
            nameLower.Contains("radiant") || nameLower.Contains("luminous") ||
            nameLower.Contains("prismatic") || nameLower.Contains("shimmering"))
        {
            return "Crystalline";
        }

        return null;
    }

    /// <summary>
    /// Generates SQL migration script to create base templates and modifiers
    /// based on analysis results
    /// </summary>
    public string GenerateMigrationSQL(List<BiomeMigrationReport> reports)
    {
        _log.Information("Generating migration SQL from {ReportCount} reports", reports.Count);

        var sql = new System.Text.StringBuilder();
        sql.AppendLine("-- Auto-generated Descriptor Migration SQL");
        sql.AppendLine("-- Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        sql.AppendLine();
        sql.AppendLine("BEGIN TRANSACTION;");
        sql.AppendLine();

        // Collect unique base templates
        var baseTemplates = new HashSet<string>();
        foreach (var report in reports)
        {
            foreach (var suggestion in report.Suggestions)
            {
                if (!string.IsNullOrEmpty(suggestion.SuggestedBaseTemplate))
                {
                    baseTemplates.Add(suggestion.SuggestedBaseTemplate);
                }
            }
        }

        // Generate base template INSERT statements
        sql.AppendLine("-- Base Templates");
        foreach (var baseTemplate in baseTemplates.OrderBy(x => x))
        {
            sql.AppendLine($"-- TODO: Define mechanics for {baseTemplate}");
            sql.AppendLine($"-- INSERT OR IGNORE INTO Descriptor_Base_Templates (...) VALUES (...);");
            sql.AppendLine();
        }

        // Generate composite descriptor INSERT statements
        sql.AppendLine("-- Composite Descriptors");
        foreach (var report in reports)
        {
            sql.AppendLine($"-- Biome: {report.BiomeName}");
            foreach (var suggestion in report.Suggestions.Where(s => s.ConfidenceScore >= 0.7))
            {
                sql.AppendLine($"-- Original: {suggestion.OriginalName}");
                sql.AppendLine($"-- Suggested: {suggestion.SuggestedBaseTemplate} + {suggestion.SuggestedModifier}");
                sql.AppendLine($"-- INSERT INTO Descriptor_Composites (...) VALUES (...);");
                sql.AppendLine();
            }
        }

        sql.AppendLine("COMMIT;");

        _log.Information("Migration SQL generated: {BaseTemplateCount} base templates suggested",
            baseTemplates.Count);

        return sql.ToString();
    }
}

/// <summary>
/// Report of suggested migrations for a biome
/// </summary>
public class BiomeMigrationReport
{
    public int BiomeId { get; set; }
    public string BiomeName { get; set; } = string.Empty;
    public List<MigrationSuggestion> Suggestions { get; set; } = new();

    public int HighConfidenceSuggestions => Suggestions.Count(s => s.ConfidenceScore >= 0.8);
    public int MediumConfidenceSuggestions => Suggestions.Count(s => s.ConfidenceScore >= 0.5 && s.ConfidenceScore < 0.8);
    public int LowConfidenceSuggestions => Suggestions.Count(s => s.ConfidenceScore < 0.5);
}

/// <summary>
/// Suggested migration for a single biome element
/// </summary>
public class MigrationSuggestion
{
    public string OriginalName { get; set; } = string.Empty;
    public string OriginalDescription { get; set; } = string.Empty;
    public string? SuggestedBaseTemplate { get; set; }
    public string? SuggestedModifier { get; set; }
    public string Category { get; set; } = string.Empty;  // Room, Feature, Object
    public string Archetype { get; set; } = string.Empty;
    public float ConfidenceScore { get; set; }  // 0.0-1.0
    public string MigrationNotes { get; set; } = string.Empty;
}
