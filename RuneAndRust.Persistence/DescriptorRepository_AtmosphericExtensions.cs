using Microsoft.Data.Sqlite;
using RuneAndRust.Core.Descriptors;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.4: Atmospheric descriptor extensions for DescriptorRepository
/// Provides database access to atmospheric descriptors and biome atmosphere profiles
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _atmosphericLog = Log.ForContext<DescriptorRepository>();

    #region Atmospheric Descriptors

    /// <summary>
    /// Gets all atmospheric descriptors
    /// </summary>
    public List<AtmosphericDescriptor> GetAllAtmosphericDescriptors()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, category, intensity,
                descriptor_text, biome_affinity, tags
            FROM Atmospheric_Descriptors
            ORDER BY category, intensity
        ";

        var descriptors = new List<AtmosphericDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapAtmosphericDescriptor(reader));
        }

        _atmosphericLog.Debug("Loaded {Count} atmospheric descriptors", descriptors.Count);
        return descriptors;
    }

    /// <summary>
    /// Gets atmospheric descriptors by category
    /// </summary>
    public List<AtmosphericDescriptor> GetAtmosphericDescriptorsByCategory(
        AtmosphericCategory category,
        AtmosphericIntensity? intensity = null,
        string? biomeAffinity = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, category, intensity,
                descriptor_text, biome_affinity, tags
            FROM Atmospheric_Descriptors
            WHERE category = $category
        ";

        command.Parameters.AddWithValue("$category", category.ToString());

        if (intensity.HasValue)
        {
            sql += " AND intensity = $intensity";
            command.Parameters.AddWithValue("$intensity", intensity.Value.ToString());
        }

        if (!string.IsNullOrEmpty(biomeAffinity))
        {
            sql += " AND (biome_affinity = $biome OR biome_affinity IS NULL)";
            command.Parameters.AddWithValue("$biome", biomeAffinity);
        }

        sql += " ORDER BY intensity";
        command.CommandText = sql;

        var descriptors = new List<AtmosphericDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapAtmosphericDescriptor(reader));
        }

        _atmosphericLog.Debug("Loaded {Count} atmospheric descriptors for category {Category}",
            descriptors.Count, category);
        return descriptors;
    }

    /// <summary>
    /// Gets atmospheric descriptors by IDs
    /// </summary>
    public List<AtmosphericDescriptor> GetAtmosphericDescriptorsByIds(List<int> descriptorIds)
    {
        if (descriptorIds == null || descriptorIds.Count == 0)
            return new List<AtmosphericDescriptor>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var idParams = string.Join(",", descriptorIds.Select((_, i) => $"$id{i}"));
        command.CommandText = $@"
            SELECT
                descriptor_id, category, intensity,
                descriptor_text, biome_affinity, tags
            FROM Atmospheric_Descriptors
            WHERE descriptor_id IN ({idParams})
        ";

        for (int i = 0; i < descriptorIds.Count; i++)
        {
            command.Parameters.AddWithValue($"$id{i}", descriptorIds[i]);
        }

        var descriptors = new List<AtmosphericDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapAtmosphericDescriptor(reader));
        }

        _atmosphericLog.Debug("Loaded {Count} atmospheric descriptors by IDs", descriptors.Count);
        return descriptors;
    }

    /// <summary>
    /// Gets atmospheric descriptor by ID
    /// </summary>
    public AtmosphericDescriptor? GetAtmosphericDescriptorById(int descriptorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, category, intensity,
                descriptor_text, biome_affinity, tags
            FROM Atmospheric_Descriptors
            WHERE descriptor_id = $descriptorId
        ";
        command.Parameters.AddWithValue("$descriptorId", descriptorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAtmosphericDescriptor(reader);
        }

        _atmosphericLog.Warning("Atmospheric descriptor not found: ID {DescriptorId}", descriptorId);
        return null;
    }

    private AtmosphericDescriptor MapAtmosphericDescriptor(SqliteDataReader reader)
    {
        return new AtmosphericDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            Category = Enum.Parse<AtmosphericCategory>(reader.GetString(1)),
            Intensity = Enum.Parse<AtmosphericIntensity>(reader.GetString(2)),
            DescriptorText = reader.GetString(3),
            BiomeAffinity = reader.IsDBNull(4) ? null : reader.GetString(4),
            Tags = reader.IsDBNull(5) ? null : reader.GetString(5)
        };
    }

    #endregion

    #region Biome Atmosphere Profiles

    /// <summary>
    /// Gets all biome atmosphere profiles
    /// </summary>
    public List<BiomeAtmosphereProfile> GetAllBiomeAtmosphereProfiles()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                profile_id, biome_name,
                lighting_descriptors, sound_descriptors,
                smell_descriptors, temperature_descriptors,
                psychic_descriptors, composite_template,
                default_intensity
            FROM Biome_Atmosphere_Profiles
            ORDER BY biome_name
        ";

        var profiles = new List<BiomeAtmosphereProfile>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            profiles.Add(MapBiomeAtmosphereProfile(reader));
        }

        _atmosphericLog.Debug("Loaded {Count} biome atmosphere profiles", profiles.Count);
        return profiles;
    }

    /// <summary>
    /// Gets biome atmosphere profile by biome name
    /// </summary>
    public BiomeAtmosphereProfile? GetBiomeAtmosphereProfile(string biomeName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                profile_id, biome_name,
                lighting_descriptors, sound_descriptors,
                smell_descriptors, temperature_descriptors,
                psychic_descriptors, composite_template,
                default_intensity
            FROM Biome_Atmosphere_Profiles
            WHERE biome_name = $biomeName
        ";
        command.Parameters.AddWithValue("$biomeName", biomeName);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapBiomeAtmosphereProfile(reader);
        }

        _atmosphericLog.Warning("Biome atmosphere profile not found: {BiomeName}", biomeName);
        return null;
    }

    private BiomeAtmosphereProfile MapBiomeAtmosphereProfile(SqliteDataReader reader)
    {
        return new BiomeAtmosphereProfile
        {
            ProfileId = reader.GetInt32(0),
            BiomeName = reader.GetString(1),
            LightingDescriptors = reader.GetString(2),
            SoundDescriptors = reader.GetString(3),
            SmellDescriptors = reader.GetString(4),
            TemperatureDescriptors = reader.GetString(5),
            PsychicDescriptors = reader.GetString(6),
            CompositeTemplate = reader.GetString(7),
            DefaultIntensity = Enum.Parse<AtmosphericIntensity>(reader.GetString(8))
        };
    }

    #endregion

    #region Atmospheric Statistics

    /// <summary>
    /// Gets statistics about atmospheric descriptors
    /// </summary>
    public Dictionary<string, int> GetAtmosphericDescriptorStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT category, COUNT(*) as count
            FROM Atmospheric_Descriptors
            GROUP BY category
            ORDER BY category
        ";

        var stats = new Dictionary<string, int>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            stats[reader.GetString(0)] = reader.GetInt32(1);
        }

        _atmosphericLog.Information("Atmospheric descriptor stats: {Stats}",
            string.Join(", ", stats.Select(kvp => $"{kvp.Key}={kvp.Value}")));

        return stats;
    }

    #endregion
}
