using Microsoft.Data.Sqlite;
using RuneAndRust.Core.ExaminationFlavor;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.9: Examination & perception flavor text extensions for DescriptorRepository
/// Provides database access to examination, perception, flora, and fauna descriptors
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _examinationFlavorLog = Log.ForContext<DescriptorRepository>();

    #region Examination Descriptors

    /// <summary>
    /// Gets examination descriptors with optional filtering
    /// </summary>
    public List<ExaminationDescriptor> GetExaminationDescriptors(
        string? objectCategory = null,
        string? objectType = null,
        string? detailLevel = null,
        string? biomeName = null,
        string? objectState = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, object_category, object_type, detail_level,
                biome_name, object_state, descriptor_text,
                weight, is_active, tags
            FROM Examination_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(objectCategory))
        {
            sql += " AND object_category = $objectCategory";
            command.Parameters.AddWithValue("$objectCategory", objectCategory);
        }

        if (!string.IsNullOrEmpty(objectType))
        {
            sql += " AND object_type = $objectType";
            command.Parameters.AddWithValue("$objectType", objectType);
        }

        if (!string.IsNullOrEmpty(detailLevel))
        {
            sql += " AND detail_level = $detailLevel";
            command.Parameters.AddWithValue("$detailLevel", detailLevel);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND (biome_name = $biomeName OR biome_name IS NULL)";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        if (!string.IsNullOrEmpty(objectState))
        {
            sql += " AND (object_state = $objectState OR object_state IS NULL)";
            command.Parameters.AddWithValue("$objectState", objectState);
        }

        command.CommandText = sql;

        var descriptors = new List<ExaminationDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapExaminationDescriptor(reader));
        }

        _examinationFlavorLog.Debug(
            "Loaded {Count} examination descriptors (Category: {Category}, Type: {Type}, Level: {Level})",
            descriptors.Count, objectCategory, objectType, detailLevel);

        return descriptors;
    }

    /// <summary>
    /// Gets a random examination descriptor matching criteria
    /// </summary>
    public ExaminationDescriptor? GetRandomExaminationDescriptor(
        string objectCategory,
        string? objectType = null,
        string? detailLevel = null,
        string? biomeName = null,
        string? objectState = null)
    {
        var descriptors = GetExaminationDescriptors(
            objectCategory, objectType, detailLevel, biomeName, objectState);

        if (descriptors.Count == 0)
        {
            // Fallback: try without object_type filter
            if (!string.IsNullOrEmpty(objectType))
            {
                descriptors = GetExaminationDescriptors(
                    objectCategory, null, detailLevel, biomeName, objectState);
            }
        }

        if (descriptors.Count == 0)
        {
            _examinationFlavorLog.Warning(
                "No examination descriptors found for: {Category} {Type} {Level}",
                objectCategory, objectType, detailLevel);
            return null;
        }

        // Weighted random selection
        var totalWeight = descriptors.Sum(d => d.Weight);
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;

        foreach (var descriptor in descriptors)
        {
            cumulativeWeight += descriptor.Weight;
            if (random <= cumulativeWeight)
            {
                return descriptor;
            }
        }

        return descriptors.Last();
    }

    private ExaminationDescriptor MapExaminationDescriptor(SqliteDataReader reader)
    {
        return new ExaminationDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            ObjectCategory = reader.GetString(1),
            ObjectType = reader.IsDBNull(2) ? null : reader.GetString(2),
            DetailLevel = reader.GetString(3),
            BiomeName = reader.IsDBNull(4) ? null : reader.GetString(4),
            ObjectState = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = (float)reader.GetDouble(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Perception Check Descriptors

    /// <summary>
    /// Gets perception check descriptors with optional filtering
    /// </summary>
    public List<PerceptionCheckDescriptor> GetPerceptionCheckDescriptors(
        string? detectionType = null,
        string? successLevel = null,
        int? maxDifficultyClass = null,
        string? biomeName = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, detection_type, success_level, difficulty_class,
                biome_name, descriptor_text, expert_insight,
                weight, is_active, tags
            FROM Perception_Check_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(detectionType))
        {
            sql += " AND detection_type = $detectionType";
            command.Parameters.AddWithValue("$detectionType", detectionType);
        }

        if (!string.IsNullOrEmpty(successLevel))
        {
            sql += " AND success_level = $successLevel";
            command.Parameters.AddWithValue("$successLevel", successLevel);
        }

        if (maxDifficultyClass.HasValue)
        {
            sql += " AND (difficulty_class IS NULL OR difficulty_class <= $maxDC)";
            command.Parameters.AddWithValue("$maxDC", maxDifficultyClass.Value);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND (biome_name = $biomeName OR biome_name IS NULL)";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        command.CommandText = sql;

        var descriptors = new List<PerceptionCheckDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapPerceptionCheckDescriptor(reader));
        }

        _examinationFlavorLog.Debug(
            "Loaded {Count} perception descriptors (Type: {Type}, Level: {Level}, DC: {DC})",
            descriptors.Count, detectionType, successLevel, maxDifficultyClass);

        return descriptors;
    }

    /// <summary>
    /// Gets a random perception check descriptor
    /// </summary>
    public PerceptionCheckDescriptor? GetRandomPerceptionCheckDescriptor(
        string detectionType,
        int checkResult,
        string? biomeName = null)
    {
        // Determine success level based on check result
        string successLevel = checkResult >= 20
            ? PerceptionCheckDescriptor.SuccessLevels.ExpertSuccess
            : PerceptionCheckDescriptor.SuccessLevels.Success;

        var descriptors = GetPerceptionCheckDescriptors(
            detectionType, successLevel, checkResult, biomeName);

        if (descriptors.Count == 0)
        {
            _examinationFlavorLog.Warning(
                "No perception descriptors found for: {Type} DC {DC}",
                detectionType, checkResult);
            return null;
        }

        // Weighted random selection
        var totalWeight = descriptors.Sum(d => d.Weight);
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;

        foreach (var descriptor in descriptors)
        {
            cumulativeWeight += descriptor.Weight;
            if (random <= cumulativeWeight)
            {
                return descriptor;
            }
        }

        return descriptors.Last();
    }

    private PerceptionCheckDescriptor MapPerceptionCheckDescriptor(SqliteDataReader reader)
    {
        return new PerceptionCheckDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            DetectionType = reader.GetString(1),
            SuccessLevel = reader.GetString(2),
            DifficultyClass = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            BiomeName = reader.IsDBNull(4) ? null : reader.GetString(4),
            DescriptorText = reader.GetString(5),
            ExpertInsight = reader.IsDBNull(6) ? null : reader.GetString(6),
            Weight = (float)reader.GetDouble(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Flora Descriptors

    /// <summary>
    /// Gets flora descriptors with optional filtering
    /// </summary>
    public List<FloraDescriptor> GetFloraDescriptors(
        string? floraName = null,
        string? floraType = null,
        string? detailLevel = null,
        string? biomeName = null,
        bool? harvestableOnly = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, flora_name, flora_type, detail_level,
                biome_name, is_harvestable, is_dangerous, alchemy_use,
                descriptor_text, weight, is_active, tags
            FROM Flora_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(floraName))
        {
            sql += " AND flora_name = $floraName";
            command.Parameters.AddWithValue("$floraName", floraName);
        }

        if (!string.IsNullOrEmpty(floraType))
        {
            sql += " AND flora_type = $floraType";
            command.Parameters.AddWithValue("$floraType", floraType);
        }

        if (!string.IsNullOrEmpty(detailLevel))
        {
            sql += " AND detail_level = $detailLevel";
            command.Parameters.AddWithValue("$detailLevel", detailLevel);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND biome_name = $biomeName";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        if (harvestableOnly.HasValue)
        {
            sql += " AND is_harvestable = $harvestable";
            command.Parameters.AddWithValue("$harvestable", harvestableOnly.Value ? 1 : 0);
        }

        command.CommandText = sql;

        var descriptors = new List<FloraDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapFloraDescriptor(reader));
        }

        _examinationFlavorLog.Debug(
            "Loaded {Count} flora descriptors (Name: {Name}, Biome: {Biome}, Level: {Level})",
            descriptors.Count, floraName, biomeName, detailLevel);

        return descriptors;
    }

    /// <summary>
    /// Gets a random flora descriptor for a biome
    /// </summary>
    public FloraDescriptor? GetRandomFloraDescriptor(
        string biomeName,
        string? detailLevel = null)
    {
        var descriptors = GetFloraDescriptors(
            floraName: null,
            floraType: null,
            detailLevel: detailLevel,
            biomeName: biomeName);

        if (descriptors.Count == 0)
        {
            _examinationFlavorLog.Warning(
                "No flora descriptors found for biome: {Biome}",
                biomeName);
            return null;
        }

        // Weighted random selection
        var totalWeight = descriptors.Sum(d => d.Weight);
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;

        foreach (var descriptor in descriptors)
        {
            cumulativeWeight += descriptor.Weight;
            if (random <= cumulativeWeight)
            {
                return descriptor;
            }
        }

        return descriptors.Last();
    }

    private FloraDescriptor MapFloraDescriptor(SqliteDataReader reader)
    {
        return new FloraDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            FloraName = reader.GetString(1),
            FloraType = reader.GetString(2),
            DetailLevel = reader.GetString(3),
            BiomeName = reader.GetString(4),
            IsHarvestable = reader.GetInt32(5) == 1,
            IsDangerous = reader.GetInt32(6) == 1,
            AlchemyUse = reader.IsDBNull(7) ? null : reader.GetString(7),
            DescriptorText = reader.GetString(8),
            Weight = (float)reader.GetDouble(9),
            IsActive = reader.GetInt32(10) == 1,
            Tags = reader.IsDBNull(11) ? null : reader.GetString(11)
        };
    }

    #endregion

    #region Fauna Descriptors

    /// <summary>
    /// Gets fauna descriptors with optional filtering
    /// </summary>
    public List<FaunaDescriptor> GetFaunaDescriptors(
        string? creatureName = null,
        string? creatureType = null,
        string? observationType = null,
        string? biomeName = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, creature_name, creature_type, observation_type,
                biome_name, is_hostile, ecological_role, expert_insight,
                descriptor_text, weight, is_active, tags
            FROM Fauna_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(creatureName))
        {
            sql += " AND creature_name = $creatureName";
            command.Parameters.AddWithValue("$creatureName", creatureName);
        }

        if (!string.IsNullOrEmpty(creatureType))
        {
            sql += " AND creature_type = $creatureType";
            command.Parameters.AddWithValue("$creatureType", creatureType);
        }

        if (!string.IsNullOrEmpty(observationType))
        {
            sql += " AND observation_type = $observationType";
            command.Parameters.AddWithValue("$observationType", observationType);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND (biome_name = $biomeName OR biome_name IS NULL)";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        command.CommandText = sql;

        var descriptors = new List<FaunaDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapFaunaDescriptor(reader));
        }

        _examinationFlavorLog.Debug(
            "Loaded {Count} fauna descriptors (Name: {Name}, Biome: {Biome}, Type: {Type})",
            descriptors.Count, creatureName, biomeName, observationType);

        return descriptors;
    }

    /// <summary>
    /// Gets a random fauna descriptor for ambient creature sightings
    /// </summary>
    public FaunaDescriptor? GetRandomFaunaDescriptor(
        string? biomeName = null,
        string? observationType = null)
    {
        var descriptors = GetFaunaDescriptors(
            creatureName: null,
            creatureType: null,
            observationType: observationType,
            biomeName: biomeName);

        if (descriptors.Count == 0)
        {
            _examinationFlavorLog.Warning(
                "No fauna descriptors found for biome: {Biome}",
                biomeName);
            return null;
        }

        // Weighted random selection
        var totalWeight = descriptors.Sum(d => d.Weight);
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;

        foreach (var descriptor in descriptors)
        {
            cumulativeWeight += descriptor.Weight;
            if (random <= cumulativeWeight)
            {
                return descriptor;
            }
        }

        return descriptors.Last();
    }

    private FaunaDescriptor MapFaunaDescriptor(SqliteDataReader reader)
    {
        return new FaunaDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            CreatureName = reader.GetString(1),
            CreatureType = reader.GetString(2),
            ObservationType = reader.GetString(3),
            BiomeName = reader.IsDBNull(4) ? null : reader.GetString(4),
            IsHostile = reader.GetInt32(5) == 1,
            EcologicalRole = reader.IsDBNull(6) ? null : reader.GetString(6),
            ExpertInsight = reader.IsDBNull(7) ? null : reader.GetString(7),
            DescriptorText = reader.GetString(8),
            Weight = (float)reader.GetDouble(9),
            IsActive = reader.GetInt32(10) == 1,
            Tags = reader.IsDBNull(11) ? null : reader.GetString(11)
        };
    }

    #endregion

    #region Examination Lore Fragments

    /// <summary>
    /// Gets lore fragments with optional filtering
    /// </summary>
    public List<ExaminationLoreFragment> GetExaminationLoreFragments(
        string? loreCategory = null,
        string? relatedObjectType = null,
        string? requiredDetailLevel = null,
        string? biomeName = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                lore_id, lore_category, related_object_type, required_detail_level,
                biome_name, lore_title, lore_text, is_active, tags
            FROM Examination_Lore_Fragments
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(loreCategory))
        {
            sql += " AND lore_category = $loreCategory";
            command.Parameters.AddWithValue("$loreCategory", loreCategory);
        }

        if (!string.IsNullOrEmpty(relatedObjectType))
        {
            sql += " AND (related_object_type = $objectType OR related_object_type IS NULL)";
            command.Parameters.AddWithValue("$objectType", relatedObjectType);
        }

        if (!string.IsNullOrEmpty(requiredDetailLevel))
        {
            sql += " AND required_detail_level = $detailLevel";
            command.Parameters.AddWithValue("$detailLevel", requiredDetailLevel);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND (biome_name = $biomeName OR biome_name IS NULL)";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        command.CommandText = sql;

        var fragments = new List<ExaminationLoreFragment>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            fragments.Add(MapExaminationLoreFragment(reader));
        }

        _examinationFlavorLog.Debug(
            "Loaded {Count} lore fragments (Category: {Category}, Object: {Object})",
            fragments.Count, loreCategory, relatedObjectType);

        return fragments;
    }

    private ExaminationLoreFragment MapExaminationLoreFragment(SqliteDataReader reader)
    {
        return new ExaminationLoreFragment
        {
            LoreId = reader.GetInt32(0),
            LoreCategory = reader.GetString(1),
            RelatedObjectType = reader.IsDBNull(2) ? null : reader.GetString(2),
            RequiredDetailLevel = reader.IsDBNull(3) ? null : reader.GetString(3),
            BiomeName = reader.IsDBNull(4) ? null : reader.GetString(4),
            LoreTitle = reader.GetString(5),
            LoreText = reader.GetString(6),
            IsActive = reader.GetInt32(7) == 1,
            Tags = reader.IsDBNull(8) ? null : reader.GetString(8)
        };
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets statistics about the examination flavor text library
    /// </summary>
    public ExaminationFlavorTextStats GetExaminationFlavorTextStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stats = new ExaminationFlavorTextStats
        {
            TotalExaminationDescriptors = GetCount(connection, "Examination_Descriptors"),
            TotalPerceptionDescriptors = GetCount(connection, "Perception_Check_Descriptors"),
            TotalFloraDescriptors = GetCount(connection, "Flora_Descriptors"),
            TotalFaunaDescriptors = GetCount(connection, "Fauna_Descriptors"),
            TotalLoreFragments = GetCount(connection, "Examination_Lore_Fragments"),
            ExaminationByCategory = GetGroupCount(connection, "Examination_Descriptors", "object_category"),
            FloraByBiome = GetGroupCount(connection, "Flora_Descriptors", "biome_name"),
            FaunaByBiome = GetGroupCount(connection, "Fauna_Descriptors", "biome_name")
        };

        _examinationFlavorLog.Information(
            "Examination flavor stats: {Exam} examination, {Perception} perception, {Flora} flora, {Fauna} fauna, {Lore} lore",
            stats.TotalExaminationDescriptors, stats.TotalPerceptionDescriptors,
            stats.TotalFloraDescriptors, stats.TotalFaunaDescriptors, stats.TotalLoreFragments);

        return stats;
    }

    #endregion
}

/// <summary>
/// Statistics about the examination flavor text library
/// </summary>
public class ExaminationFlavorTextStats
{
    public int TotalExaminationDescriptors { get; set; }
    public int TotalPerceptionDescriptors { get; set; }
    public int TotalFloraDescriptors { get; set; }
    public int TotalFaunaDescriptors { get; set; }
    public int TotalLoreFragments { get; set; }
    public Dictionary<string, int> ExaminationByCategory { get; set; } = new();
    public Dictionary<string, int> FloraByBiome { get; set; } = new();
    public Dictionary<string, int> FaunaByBiome { get; set; } = new();
}
