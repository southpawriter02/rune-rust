using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.29.4: Data repository for Muspelheim biome.
/// Provides database-driven access to room templates, enemies, hazards, and resources.
/// </summary>
public class MuspelheimDataRepository
{
    private static readonly ILogger _log = Log.ForContext<MuspelheimDataRepository>();
    private readonly string _connectionString;
    private const int MUSPELHEIM_BIOME_ID = 4;

    public MuspelheimDataRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("MuspelheimDataRepository initialized");
    }

    #region Room Templates

    /// <summary>
    /// Get all room templates for Muspelheim biome
    /// </summary>
    public List<MuspelheimRoomTemplate> GetRoomTemplates()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                template_id,
                template_name,
                template_description,
                room_size_category,
                min_connections,
                max_connections,
                can_be_entrance,
                can_be_exit,
                can_be_hub,
                hazard_density,
                enemy_spawn_weight,
                resource_spawn_chance,
                wfc_adjacency_rules
            FROM Biome_RoomTemplates
            WHERE biome_id = $biomeId
        ";
        command.Parameters.AddWithValue("$biomeId", MUSPELHEIM_BIOME_ID);

        var templates = new List<MuspelheimRoomTemplate>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            templates.Add(new MuspelheimRoomTemplate
            {
                TemplateId = reader.GetInt32(0),
                TemplateName = reader.GetString(1),
                Description = reader.GetString(2),
                RoomSize = reader.GetString(3),
                MinConnections = reader.GetInt32(4),
                MaxConnections = reader.GetInt32(5),
                CanBeEntrance = reader.GetInt32(6) == 1,
                CanBeExit = reader.GetInt32(7) == 1,
                CanBeHub = reader.GetInt32(8) == 1,
                HazardDensity = reader.GetString(9),
                EnemySpawnWeight = reader.GetInt32(10),
                ResourceSpawnChance = reader.GetDouble(11),
                WfcAdjacencyRules = reader.GetString(12)
            });
        }

        _log.Information("Loaded {Count} room templates for Muspelheim", templates.Count);
        return templates;
    }

    /// <summary>
    /// Get room templates that can be entrance rooms
    /// </summary>
    public List<MuspelheimRoomTemplate> GetEntranceTemplates()
    {
        return GetRoomTemplates().Where(t => t.CanBeEntrance).ToList();
    }

    /// <summary>
    /// Get room templates that can be exit rooms
    /// </summary>
    public List<MuspelheimRoomTemplate> GetExitTemplates()
    {
        return GetRoomTemplates().Where(t => t.CanBeExit).ToList();
    }

    #endregion

    #region Enemy Spawns

    /// <summary>
    /// Get all enemy spawn definitions for Muspelheim
    /// </summary>
    public List<MuspelheimEnemySpawn> GetEnemySpawns(int? minLevel = null, int? maxLevel = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                spawn_id,
                enemy_name,
                enemy_type,
                min_level,
                max_level,
                spawn_weight,
                spawn_rules_json
            FROM Biome_EnemySpawns
            WHERE biome_id = $biomeId
        ";

        if (minLevel.HasValue)
        {
            command.CommandText += " AND max_level >= $minLevel";
            command.Parameters.AddWithValue("$minLevel", minLevel.Value);
        }

        if (maxLevel.HasValue)
        {
            command.CommandText += " AND min_level <= $maxLevel";
            command.Parameters.AddWithValue("$maxLevel", maxLevel.Value);
        }

        command.Parameters.AddWithValue("$biomeId", MUSPELHEIM_BIOME_ID);

        var spawns = new List<MuspelheimEnemySpawn>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            spawns.Add(new MuspelheimEnemySpawn
            {
                SpawnId = reader.GetInt32(0),
                EnemyName = reader.GetString(1),
                EnemyType = reader.GetString(2),
                MinLevel = reader.GetInt32(3),
                MaxLevel = reader.GetInt32(4),
                SpawnWeight = reader.GetInt32(5),
                SpawnRulesJson = reader.GetString(6)
            });
        }

        _log.Information("Loaded {Count} enemy spawns for Muspelheim (level range: {MinLevel}-{MaxLevel})",
            spawns.Count, minLevel, maxLevel);
        return spawns;
    }

    /// <summary>
    /// Get boss enemy spawn (Surtur's Herald)
    /// </summary>
    public MuspelheimEnemySpawn? GetBossSpawn()
    {
        return GetEnemySpawns().FirstOrDefault(e => e.EnemyType == "Boss");
    }

    /// <summary>
    /// Get resistances from spawn rules JSON
    /// </summary>
    public Dictionary<string, int> GetEnemyResistances(string spawnRulesJson)
    {
        try
        {
            var rules = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(spawnRulesJson);
            var resistances = new Dictionary<string, int>();

            if (rules != null)
            {
                if (rules.ContainsKey("fire_resistance"))
                {
                    resistances["Fire"] = rules["fire_resistance"].GetInt32();
                }
                if (rules.ContainsKey("ice_resistance"))
                {
                    resistances["Ice"] = rules["ice_resistance"].GetInt32();
                }
                if (rules.ContainsKey("physical_resistance"))
                {
                    resistances["Physical"] = rules["physical_resistance"].GetInt32();
                }
                if (rules.ContainsKey("lightning_resistance"))
                {
                    resistances["Lightning"] = rules["lightning_resistance"].GetInt32();
                }
            }

            return resistances;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse spawn rules JSON: {Json}", spawnRulesJson);
            return new Dictionary<string, int>();
        }
    }

    #endregion

    #region Environmental Hazards

    /// <summary>
    /// Get all environmental hazards for Muspelheim
    /// </summary>
    public List<MuspelheimHazard> GetEnvironmentalHazards(string? hazardDensity = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                feature_id,
                feature_name,
                feature_type,
                feature_description,
                damage_per_turn,
                damage_type,
                tile_coverage_percent,
                is_destructible,
                blocks_movement,
                blocks_line_of_sight,
                hazard_density_category,
                special_rules
            FROM Biome_EnvironmentalFeatures
            WHERE biome_id = $biomeId
        ";

        if (!string.IsNullOrEmpty(hazardDensity))
        {
            command.CommandText += @"
                AND (
                    hazard_density_category = $density
                    OR hazard_density_category = 'Low'
                    OR (hazard_density_category = 'Medium' AND $density IN ('Medium', 'High', 'Extreme'))
                    OR (hazard_density_category = 'High' AND $density IN ('High', 'Extreme'))
                )
            ";
            command.Parameters.AddWithValue("$density", hazardDensity);
        }

        command.Parameters.AddWithValue("$biomeId", MUSPELHEIM_BIOME_ID);

        var hazards = new List<MuspelheimHazard>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            hazards.Add(new MuspelheimHazard
            {
                FeatureId = reader.GetInt32(0),
                FeatureName = reader.GetString(1),
                FeatureType = reader.GetString(2),
                Description = reader.GetString(3),
                DamagePerTurn = reader.GetInt32(4),
                DamageType = reader.IsDBNull(5) ? null : reader.GetString(5),
                TileCoveragePercent = reader.GetInt32(6),
                IsDestructible = reader.GetInt32(7) == 1,
                BlocksMovement = reader.GetInt32(8) == 1,
                BlocksLineOfSight = reader.GetInt32(9) == 1,
                HazardDensityCategory = reader.GetString(10),
                SpecialRules = reader.GetString(11)
            });
        }

        _log.Information("Loaded {Count} hazards for Muspelheim (density filter: {Density})",
            hazards.Count, hazardDensity ?? "all");
        return hazards;
    }

    #endregion

    #region Resource Drops

    /// <summary>
    /// Get all resource drops for Muspelheim
    /// </summary>
    public List<MuspelheimResource> GetResourceDrops()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                resource_drop_id,
                resource_name,
                resource_description,
                resource_tier,
                rarity,
                base_drop_chance,
                min_quantity,
                max_quantity,
                requires_special_node,
                weight
            FROM Biome_ResourceDrops
            WHERE biome_id = $biomeId
            ORDER BY resource_tier DESC, weight DESC
        ";
        command.Parameters.AddWithValue("$biomeId", MUSPELHEIM_BIOME_ID);

        var resources = new List<MuspelheimResource>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            resources.Add(new MuspelheimResource
            {
                ResourceDropId = reader.GetInt32(0),
                ResourceName = reader.GetString(1),
                Description = reader.GetString(2),
                Tier = reader.GetInt32(3),
                Rarity = reader.GetString(4),
                BaseDropChance = reader.GetDouble(5),
                MinQuantity = reader.GetInt32(6),
                MaxQuantity = reader.GetInt32(7),
                RequiresSpecialNode = reader.GetInt32(8) == 1,
                Weight = reader.GetInt32(9)
            });
        }

        _log.Information("Loaded {Count} resources for Muspelheim", resources.Count);
        return resources;
    }

    #endregion

    #region Biome Metadata

    /// <summary>
    /// Get Muspelheim biome metadata
    /// </summary>
    public MuspelheimBiomeMetadata GetBiomeMetadata()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                biome_name,
                biome_description,
                z_level_restriction,
                min_character_level,
                max_character_level
            FROM Biomes
            WHERE biome_id = $biomeId
        ";
        command.Parameters.AddWithValue("$biomeId", MUSPELHEIM_BIOME_ID);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new MuspelheimBiomeMetadata
            {
                BiomeId = MUSPELHEIM_BIOME_ID,
                BiomeName = reader.GetString(0),
                Description = reader.GetString(1),
                ZLevelRestriction = reader.GetString(2),
                MinCharacterLevel = reader.GetInt32(3),
                MaxCharacterLevel = reader.GetInt32(4)
            };
        }

        throw new InvalidOperationException("Muspelheim biome metadata not found in database");
    }

    #endregion

    #region Character Biome Status

    /// <summary>
    /// v0.29.5: Tracks deaths from environmental hazards (forced movement into lava, etc.)
    /// Separate from heat deaths (ambient condition).
    /// </summary>
    /// <param name="characterId">Character ID from saves table</param>
    /// <param name="hazardName">Name of the hazard that killed the character</param>
    public void IncrementEnvironmentalDeaths(int characterId, string hazardName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        _log.Information(
            "Environmental death tracked: Character={CharacterId}, Hazard={Hazard}, Biome=Muspelheim",
            characterId, hazardName);

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_BiomeStatus
            SET times_died_to_environment = times_died_to_environment + 1,
                last_updated = CURRENT_TIMESTAMP
            WHERE character_id = $characterId AND biome_id = $biomeId
        ";
        command.Parameters.AddWithValue("$characterId", characterId);
        command.Parameters.AddWithValue("$biomeId", MUSPELHEIM_BIOME_ID);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            _log.Warning(
                "No biome status record found for Character={CharacterId}, Biome={BiomeId}. Creating new record.",
                characterId, MUSPELHEIM_BIOME_ID);

            // Create new record if it doesn't exist
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO Characters_BiomeStatus (character_id, biome_id, times_died_to_environment)
                VALUES ($characterId, $biomeId, 1)
            ";
            insertCommand.Parameters.AddWithValue("$characterId", characterId);
            insertCommand.Parameters.AddWithValue("$biomeId", MUSPELHEIM_BIOME_ID);
            insertCommand.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// v0.29.5: Gets biome status for a character
    /// </summary>
    public CharacterBiomeStatus? GetBiomeStatus(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                status_id,
                character_id,
                biome_id,
                total_time_seconds,
                rooms_explored,
                enemies_defeated,
                heat_damage_taken,
                times_died_to_heat,
                times_died_to_environment,
                resources_collected,
                has_reached_surtur
            FROM Characters_BiomeStatus
            WHERE character_id = $characterId AND biome_id = $biomeId
        ";
        command.Parameters.AddWithValue("$characterId", characterId);
        command.Parameters.AddWithValue("$biomeId", MUSPELHEIM_BIOME_ID);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new CharacterBiomeStatus
            {
                StatusId = reader.GetInt32(0),
                CharacterId = reader.GetInt32(1),
                BiomeId = reader.GetInt32(2),
                TotalTimeSeconds = reader.GetInt32(3),
                RoomsExplored = reader.GetInt32(4),
                EnemiesDefeated = reader.GetInt32(5),
                HeatDamageTaken = reader.GetInt32(6),
                TimesDiedToHeat = reader.GetInt32(7),
                TimesDiedToEnvironment = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                ResourcesCollected = reader.GetInt32(9),
                HasReachedSurtur = reader.GetInt32(10) == 1
            };
        }

        return null;
    }

    #endregion
}

#region Data Transfer Objects

public class MuspelheimRoomTemplate
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RoomSize { get; set; } = string.Empty;
    public int MinConnections { get; set; }
    public int MaxConnections { get; set; }
    public bool CanBeEntrance { get; set; }
    public bool CanBeExit { get; set; }
    public bool CanBeHub { get; set; }
    public string HazardDensity { get; set; } = string.Empty;
    public int EnemySpawnWeight { get; set; }
    public double ResourceSpawnChance { get; set; }
    public string WfcAdjacencyRules { get; set; } = string.Empty;
}

public class MuspelheimEnemySpawn
{
    public int SpawnId { get; set; }
    public string EnemyName { get; set; } = string.Empty;
    public string EnemyType { get; set; } = string.Empty;
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public int SpawnWeight { get; set; }
    public string SpawnRulesJson { get; set; } = string.Empty;
}

public class MuspelheimHazard
{
    public int FeatureId { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DamagePerTurn { get; set; }
    public string? DamageType { get; set; }
    public int TileCoveragePercent { get; set; }
    public bool IsDestructible { get; set; }
    public bool BlocksMovement { get; set; }
    public bool BlocksLineOfSight { get; set; }
    public string HazardDensityCategory { get; set; } = string.Empty;
    public string SpecialRules { get; set; } = string.Empty;
}

public class MuspelheimResource
{
    public int ResourceDropId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Tier { get; set; }
    public string Rarity { get; set; } = string.Empty;
    public double BaseDropChance { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public bool RequiresSpecialNode { get; set; }
    public int Weight { get; set; }
}

public class MuspelheimBiomeMetadata
{
    public int BiomeId { get; set; }
    public string BiomeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ZLevelRestriction { get; set; } = string.Empty;
    public int MinCharacterLevel { get; set; }
    public int MaxCharacterLevel { get; set; }
}

/// <summary>
/// v0.29.5: Character biome status tracking
/// </summary>
public class CharacterBiomeStatus
{
    public int StatusId { get; set; }
    public int CharacterId { get; set; }
    public int BiomeId { get; set; }
    public int TotalTimeSeconds { get; set; }
    public int RoomsExplored { get; set; }
    public int EnemiesDefeated { get; set; }
    public int HeatDamageTaken { get; set; }
    public int TimesDiedToHeat { get; set; }
    public int TimesDiedToEnvironment { get; set; }
    public int ResourcesCollected { get; set; }
    public bool HasReachedSurtur { get; set; }
}

#endregion
