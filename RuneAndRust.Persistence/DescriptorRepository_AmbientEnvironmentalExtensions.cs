using Microsoft.Data.Sqlite;
using RuneAndRust.Core.AmbientFlavor;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.13: Ambient Environmental Descriptors extensions for DescriptorRepository
/// Provides database access to ambient sound, smell, atmospheric detail, and background activity descriptors
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _ambientEnvironmentalLog = Log.ForContext<DescriptorRepository>();

    #region Ambient Sound Descriptors

    /// <summary>
    /// Gets ambient sound descriptors with optional filtering
    /// </summary>
    public List<AmbientSoundDescriptor> GetAmbientSoundDescriptors(
        string? biome = null,
        string? soundCategory = null,
        string? soundSubcategory = null,
        string? timeOfDay = null,
        string? intensity = null,
        string? locationContext = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, biome, sound_category, sound_subcategory,
                time_of_day, intensity, location_context,
                descriptor_text, weight, is_active, tags
            FROM Ambient_Sound_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(biome))
        {
            sql += " AND (biome = $biome OR biome = 'Generic')";
            command.Parameters.AddWithValue("$biome", biome);
        }

        if (!string.IsNullOrEmpty(soundCategory))
        {
            sql += " AND sound_category = $soundCategory";
            command.Parameters.AddWithValue("$soundCategory", soundCategory);
        }

        if (!string.IsNullOrEmpty(soundSubcategory))
        {
            sql += " AND sound_subcategory = $soundSubcategory";
            command.Parameters.AddWithValue("$soundSubcategory", soundSubcategory);
        }

        if (!string.IsNullOrEmpty(timeOfDay))
        {
            sql += " AND (time_of_day = $timeOfDay OR time_of_day IS NULL)";
            command.Parameters.AddWithValue("$timeOfDay", timeOfDay);
        }

        if (!string.IsNullOrEmpty(intensity))
        {
            sql += " AND (intensity = $intensity OR intensity IS NULL)";
            command.Parameters.AddWithValue("$intensity", intensity);
        }

        if (!string.IsNullOrEmpty(locationContext))
        {
            sql += " AND (location_context = $locationContext OR location_context IS NULL)";
            command.Parameters.AddWithValue("$locationContext", locationContext);
        }

        command.CommandText = sql;

        var descriptors = new List<AmbientSoundDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapAmbientSoundDescriptor(reader));
        }

        _ambientEnvironmentalLog.Debug(
            "Loaded {Count} ambient sound descriptors (Biome: {Biome}, Category: {Category})",
            descriptors.Count, biome, soundCategory);

        return descriptors;
    }

    /// <summary>
    /// Gets a random ambient sound descriptor matching the criteria
    /// </summary>
    public AmbientSoundDescriptor? GetRandomAmbientSoundDescriptor(
        string biome,
        string? soundCategory = null,
        string? timeOfDay = null,
        string? intensity = null)
    {
        var descriptors = GetAmbientSoundDescriptors(
            biome, soundCategory, null, timeOfDay, intensity, null);

        if (descriptors.Count == 0)
        {
            // Fallback: try without time of day
            if (!string.IsNullOrEmpty(timeOfDay))
            {
                descriptors = GetAmbientSoundDescriptors(
                    biome, soundCategory, null, null, intensity, null);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try Generic biome
            descriptors = GetAmbientSoundDescriptors(
                "Generic", soundCategory, null, timeOfDay, intensity, null);
        }

        if (descriptors.Count == 0)
        {
            _ambientEnvironmentalLog.Warning(
                "No ambient sound descriptors found for biome: {Biome}",
                biome);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private AmbientSoundDescriptor MapAmbientSoundDescriptor(SqliteDataReader reader)
    {
        return new AmbientSoundDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            Biome = reader.GetString(1),
            SoundCategory = reader.GetString(2),
            SoundSubcategory = reader.GetString(3),
            TimeOfDay = reader.IsDBNull(4) ? null : reader.GetString(4),
            Intensity = reader.IsDBNull(5) ? null : reader.GetString(5),
            LocationContext = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            Weight = reader.GetFloat(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Ambient Smell Descriptors

    /// <summary>
    /// Gets ambient smell descriptors with optional filtering
    /// </summary>
    public List<AmbientSmellDescriptor> GetAmbientSmellDescriptors(
        string? biome = null,
        string? smellCategory = null,
        string? smellSubcategory = null,
        string? intensity = null,
        string? proximity = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, biome, smell_category, smell_subcategory,
                intensity, proximity,
                descriptor_text, weight, is_active, tags
            FROM Ambient_Smell_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(biome))
        {
            sql += " AND (biome = $biome OR biome = 'Generic')";
            command.Parameters.AddWithValue("$biome", biome);
        }

        if (!string.IsNullOrEmpty(smellCategory))
        {
            sql += " AND smell_category = $smellCategory";
            command.Parameters.AddWithValue("$smellCategory", smellCategory);
        }

        if (!string.IsNullOrEmpty(smellSubcategory))
        {
            sql += " AND smell_subcategory = $smellSubcategory";
            command.Parameters.AddWithValue("$smellSubcategory", smellSubcategory);
        }

        if (!string.IsNullOrEmpty(intensity))
        {
            sql += " AND (intensity = $intensity OR intensity IS NULL)";
            command.Parameters.AddWithValue("$intensity", intensity);
        }

        if (!string.IsNullOrEmpty(proximity))
        {
            sql += " AND (proximity = $proximity OR proximity IS NULL)";
            command.Parameters.AddWithValue("$proximity", proximity);
        }

        command.CommandText = sql;

        var descriptors = new List<AmbientSmellDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapAmbientSmellDescriptor(reader));
        }

        _ambientEnvironmentalLog.Debug(
            "Loaded {Count} ambient smell descriptors (Biome: {Biome}, Category: {Category})",
            descriptors.Count, biome, smellCategory);

        return descriptors;
    }

    /// <summary>
    /// Gets a random ambient smell descriptor matching the criteria
    /// </summary>
    public AmbientSmellDescriptor? GetRandomAmbientSmellDescriptor(
        string biome,
        string? smellCategory = null,
        string? intensity = null)
    {
        var descriptors = GetAmbientSmellDescriptors(
            biome, smellCategory, null, intensity, null);

        if (descriptors.Count == 0)
        {
            // Fallback: try without intensity
            if (!string.IsNullOrEmpty(intensity))
            {
                descriptors = GetAmbientSmellDescriptors(
                    biome, smellCategory, null, null, null);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try Generic biome
            descriptors = GetAmbientSmellDescriptors(
                "Generic", smellCategory, null, intensity, null);
        }

        if (descriptors.Count == 0)
        {
            _ambientEnvironmentalLog.Warning(
                "No ambient smell descriptors found for biome: {Biome}",
                biome);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private AmbientSmellDescriptor MapAmbientSmellDescriptor(SqliteDataReader reader)
    {
        return new AmbientSmellDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            Biome = reader.GetString(1),
            SmellCategory = reader.GetString(2),
            SmellSubcategory = reader.GetString(3),
            Intensity = reader.IsDBNull(4) ? null : reader.GetString(4),
            Proximity = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = reader.GetFloat(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Ambient Atmospheric Detail Descriptors

    /// <summary>
    /// Gets ambient atmospheric detail descriptors with optional filtering
    /// </summary>
    public List<AmbientAtmosphericDetailDescriptor> GetAmbientAtmosphericDetailDescriptors(
        string? biome = null,
        string? detailCategory = null,
        string? detailSubcategory = null,
        string? timeOfDay = null,
        string? intensity = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, biome, detail_category, detail_subcategory,
                time_of_day, intensity,
                descriptor_text, weight, is_active, tags
            FROM Ambient_Atmospheric_Detail_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(biome))
        {
            sql += " AND (biome = $biome OR biome = 'Generic')";
            command.Parameters.AddWithValue("$biome", biome);
        }

        if (!string.IsNullOrEmpty(detailCategory))
        {
            sql += " AND detail_category = $detailCategory";
            command.Parameters.AddWithValue("$detailCategory", detailCategory);
        }

        if (!string.IsNullOrEmpty(detailSubcategory))
        {
            sql += " AND detail_subcategory = $detailSubcategory";
            command.Parameters.AddWithValue("$detailSubcategory", detailSubcategory);
        }

        if (!string.IsNullOrEmpty(timeOfDay))
        {
            sql += " AND (time_of_day = $timeOfDay OR time_of_day IS NULL)";
            command.Parameters.AddWithValue("$timeOfDay", timeOfDay);
        }

        if (!string.IsNullOrEmpty(intensity))
        {
            sql += " AND (intensity = $intensity OR intensity IS NULL)";
            command.Parameters.AddWithValue("$intensity", intensity);
        }

        command.CommandText = sql;

        var descriptors = new List<AmbientAtmosphericDetailDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapAmbientAtmosphericDetailDescriptor(reader));
        }

        _ambientEnvironmentalLog.Debug(
            "Loaded {Count} ambient atmospheric detail descriptors (Biome: {Biome}, Category: {Category})",
            descriptors.Count, biome, detailCategory);

        return descriptors;
    }

    /// <summary>
    /// Gets a random ambient atmospheric detail descriptor matching the criteria
    /// </summary>
    public AmbientAtmosphericDetailDescriptor? GetRandomAmbientAtmosphericDetailDescriptor(
        string biome,
        string? detailCategory = null,
        string? timeOfDay = null)
    {
        var descriptors = GetAmbientAtmosphericDetailDescriptors(
            biome, detailCategory, null, timeOfDay, null);

        if (descriptors.Count == 0)
        {
            // Fallback: try without time of day
            if (!string.IsNullOrEmpty(timeOfDay))
            {
                descriptors = GetAmbientAtmosphericDetailDescriptors(
                    biome, detailCategory, null, null, null);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try Generic biome
            descriptors = GetAmbientAtmosphericDetailDescriptors(
                "Generic", detailCategory, null, timeOfDay, null);
        }

        if (descriptors.Count == 0)
        {
            _ambientEnvironmentalLog.Warning(
                "No ambient atmospheric detail descriptors found for biome: {Biome}",
                biome);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private AmbientAtmosphericDetailDescriptor MapAmbientAtmosphericDetailDescriptor(SqliteDataReader reader)
    {
        return new AmbientAtmosphericDetailDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            Biome = reader.GetString(1),
            DetailCategory = reader.GetString(2),
            DetailSubcategory = reader.GetString(3),
            TimeOfDay = reader.IsDBNull(4) ? null : reader.GetString(4),
            Intensity = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = reader.GetFloat(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Ambient Background Activity Descriptors

    /// <summary>
    /// Gets ambient background activity descriptors with optional filtering
    /// </summary>
    public List<AmbientBackgroundActivityDescriptor> GetAmbientBackgroundActivityDescriptors(
        string? biome = null,
        string? activityCategory = null,
        string? activitySubcategory = null,
        string? timeOfDay = null,
        string? distance = null,
        string? threatLevel = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, biome, activity_category, activity_subcategory,
                time_of_day, distance, threat_level,
                descriptor_text, weight, is_active, tags
            FROM Ambient_Background_Activity_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(biome))
        {
            sql += " AND (biome = $biome OR biome = 'Generic')";
            command.Parameters.AddWithValue("$biome", biome);
        }

        if (!string.IsNullOrEmpty(activityCategory))
        {
            sql += " AND activity_category = $activityCategory";
            command.Parameters.AddWithValue("$activityCategory", activityCategory);
        }

        if (!string.IsNullOrEmpty(activitySubcategory))
        {
            sql += " AND activity_subcategory = $activitySubcategory";
            command.Parameters.AddWithValue("$activitySubcategory", activitySubcategory);
        }

        if (!string.IsNullOrEmpty(timeOfDay))
        {
            sql += " AND (time_of_day = $timeOfDay OR time_of_day IS NULL)";
            command.Parameters.AddWithValue("$timeOfDay", timeOfDay);
        }

        if (!string.IsNullOrEmpty(distance))
        {
            sql += " AND (distance = $distance OR distance IS NULL)";
            command.Parameters.AddWithValue("$distance", distance);
        }

        if (!string.IsNullOrEmpty(threatLevel))
        {
            sql += " AND (threat_level = $threatLevel OR threat_level IS NULL)";
            command.Parameters.AddWithValue("$threatLevel", threatLevel);
        }

        command.CommandText = sql;

        var descriptors = new List<AmbientBackgroundActivityDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapAmbientBackgroundActivityDescriptor(reader));
        }

        _ambientEnvironmentalLog.Debug(
            "Loaded {Count} ambient background activity descriptors (Biome: {Biome}, Category: {Category})",
            descriptors.Count, biome, activityCategory);

        return descriptors;
    }

    /// <summary>
    /// Gets a random ambient background activity descriptor matching the criteria
    /// </summary>
    public AmbientBackgroundActivityDescriptor? GetRandomAmbientBackgroundActivityDescriptor(
        string biome,
        string? activityCategory = null,
        string? timeOfDay = null)
    {
        var descriptors = GetAmbientBackgroundActivityDescriptors(
            biome, activityCategory, null, timeOfDay, null, null);

        if (descriptors.Count == 0)
        {
            // Fallback: try without time of day
            if (!string.IsNullOrEmpty(timeOfDay))
            {
                descriptors = GetAmbientBackgroundActivityDescriptors(
                    biome, activityCategory, null, null, null, null);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try Generic biome
            descriptors = GetAmbientBackgroundActivityDescriptors(
                "Generic", activityCategory, null, timeOfDay, null, null);
        }

        if (descriptors.Count == 0)
        {
            _ambientEnvironmentalLog.Warning(
                "No ambient background activity descriptors found for biome: {Biome}",
                biome);
            return null;
        }

        return SelectWeightedRandom(descriptors);
    }

    private AmbientBackgroundActivityDescriptor MapAmbientBackgroundActivityDescriptor(SqliteDataReader reader)
    {
        return new AmbientBackgroundActivityDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            Biome = reader.GetString(1),
            ActivityCategory = reader.GetString(2),
            ActivitySubcategory = reader.GetString(3),
            TimeOfDay = reader.IsDBNull(4) ? null : reader.GetString(4),
            Distance = reader.IsDBNull(5) ? null : reader.GetString(5),
            ThreatLevel = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            Weight = reader.GetFloat(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets statistics for ambient environmental descriptors
    /// </summary>
    public (int Sounds, int Smells, int AtmosphericDetails, int BackgroundActivities) GetAmbientEnvironmentalStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var soundCount = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Ambient_Sound_Descriptors WHERE is_active = 1");
        var smellCount = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Ambient_Smell_Descriptors WHERE is_active = 1");
        var atmosphericCount = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Ambient_Atmospheric_Detail_Descriptors WHERE is_active = 1");
        var backgroundCount = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Ambient_Background_Activity_Descriptors WHERE is_active = 1");

        return (soundCount, smellCount, atmosphericCount, backgroundCount);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Selects a random descriptor based on weighted probability
    /// </summary>
    private T? SelectWeightedRandom<T>(List<T> descriptors) where T : class
    {
        if (descriptors.Count == 0)
            return null;

        // Get total weight
        var totalWeight = descriptors.Sum(d =>
        {
            var weightProperty = typeof(T).GetProperty("Weight");
            return weightProperty != null ? (float)weightProperty.GetValue(d)! : 1.0f;
        });

        // Select random value
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0f;

        foreach (var descriptor in descriptors)
        {
            var weightProperty = typeof(T).GetProperty("Weight");
            var weight = weightProperty != null ? (float)weightProperty.GetValue(descriptor)! : 1.0f;
            cumulativeWeight += weight;

            if (random <= cumulativeWeight)
                return descriptor;
        }

        // Fallback to first descriptor
        return descriptors.First();
    }

    #endregion
}
