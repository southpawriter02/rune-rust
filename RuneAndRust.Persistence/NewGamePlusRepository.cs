using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using RuneAndRust.Core.NewGamePlus;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.40.1: Repository for New Game+ system data access
/// Manages NG+ tier tracking, carryover snapshots, scaling parameters, and completion history
/// </summary>
public class NewGamePlusRepository
{
    private static readonly ILogger _log = Log.ForContext<NewGamePlusRepository>();
    private readonly string _connectionString;
    private const string DatabaseName = "runeandrust.db";

    public NewGamePlusRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, DatabaseName);
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("NewGamePlusRepository initialized with database path: {DbPath}", dbPath);
    }

    // ═════════════════════════════════════════════════════════════
    // CHARACTER NG+ STATUS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get character's current NG+ tier
    /// </summary>
    public int GetCurrentNGPlusTier(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COALESCE(current_ng_plus_tier, 0)
            FROM Characters
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$characterId", characterId);

        var result = command.ExecuteScalar();
        return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
    }

    /// <summary>
    /// Get character's highest completed NG+ tier
    /// </summary>
    public int GetHighestNGPlusTier(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COALESCE(highest_ng_plus_tier, 0)
            FROM Characters
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$characterId", characterId);

        var result = command.ExecuteScalar();
        return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
    }

    /// <summary>
    /// Check if character has completed campaign
    /// </summary>
    public bool HasCompletedCampaign(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COALESCE(has_completed_campaign, 0)
            FROM Characters
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$characterId", characterId);

        var result = command.ExecuteScalar();
        return result != null && result != DBNull.Value && Convert.ToInt32(result) == 1;
    }

    /// <summary>
    /// Get total number of NG+ completions
    /// </summary>
    public int GetNGPlusCompletionCount(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COALESCE(ng_plus_completions, 0)
            FROM Characters
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$characterId", characterId);

        var result = command.ExecuteScalar();
        return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
    }

    /// <summary>
    /// Set character's current NG+ tier
    /// </summary>
    public void SetCurrentNGPlusTier(int characterId, int tier)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters
            SET current_ng_plus_tier = $tier
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$tier", tier);
        command.Parameters.AddWithValue("$characterId", characterId);

        command.ExecuteNonQuery();

        _log.Information("Set NG+ tier for character {CharacterId}: Tier {Tier}",
            characterId, tier);
    }

    /// <summary>
    /// Set character's highest completed NG+ tier
    /// </summary>
    public void SetHighestNGPlusTier(int characterId, int tier)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters
            SET highest_ng_plus_tier = $tier
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$tier", tier);
        command.Parameters.AddWithValue("$characterId", characterId);

        command.ExecuteNonQuery();

        _log.Information("Updated highest NG+ tier for character {CharacterId}: Tier {Tier}",
            characterId, tier);
    }

    /// <summary>
    /// Mark character as having completed campaign
    /// </summary>
    public void MarkCampaignComplete(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters
            SET has_completed_campaign = 1
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$characterId", characterId);

        command.ExecuteNonQuery();

        _log.Information("Marked campaign complete for character {CharacterId}", characterId);
    }

    /// <summary>
    /// Increment NG+ completion counter
    /// </summary>
    public void IncrementNGPlusCompletions(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters
            SET ng_plus_completions = COALESCE(ng_plus_completions, 0) + 1
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$characterId", characterId);

        command.ExecuteNonQuery();

        _log.Debug("Incremented NG+ completions for character {CharacterId}", characterId);
    }

    // ═════════════════════════════════════════════════════════════
    // CARRYOVER SNAPSHOTS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Save a carryover snapshot to database
    /// </summary>
    public int SaveCarryoverSnapshot(CarryoverSnapshot snapshot)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO NG_Plus_Carryover (
                character_id, ng_plus_tier, snapshot_timestamp,
                character_data, specialization_data, equipment_data,
                crafting_data, currency_data, quest_state_snapshot, world_state_snapshot
            ) VALUES (
                $characterId, $tier, $timestamp,
                $characterData, $specializationData, $equipmentData,
                $craftingData, $currencyData, $questSnapshot, $worldSnapshot
            )";

        command.Parameters.AddWithValue("$characterId", snapshot.CharacterId);
        command.Parameters.AddWithValue("$tier", snapshot.NGPlusTier);
        command.Parameters.AddWithValue("$timestamp", snapshot.TimestampUtc);

        // Serialize complex data as JSON
        command.Parameters.AddWithValue("$characterData", JsonSerializer.Serialize(new
        {
            Level = snapshot.CharacterLevel,
            LegendPoints = snapshot.LegendPoints,
            ProgressionPoints = snapshot.ProgressionPoints,
            UnspentProgressionPoints = snapshot.UnspentProgressionPoints,
            Attributes = snapshot.Attributes
        }));

        command.Parameters.AddWithValue("$specializationData", JsonSerializer.Serialize(new
        {
            UnlockedSpecializations = snapshot.UnlockedSpecializations,
            LearnedAbilities = snapshot.LearnedAbilities
        }));

        command.Parameters.AddWithValue("$equipmentData", JsonSerializer.Serialize(new
        {
            EquippedItems = snapshot.EquippedItems,
            InventoryItems = snapshot.InventoryItems
        }));

        command.Parameters.AddWithValue("$craftingData", JsonSerializer.Serialize(new
        {
            CraftingMaterials = snapshot.CraftingMaterials,
            UnlockedRecipes = snapshot.UnlockedRecipes
        }));

        command.Parameters.AddWithValue("$currencyData", JsonSerializer.Serialize(new
        {
            Scrap = snapshot.Scrap
        }));

        command.Parameters.AddWithValue("$questSnapshot", snapshot.QuestStateSnapshot ?? string.Empty);
        command.Parameters.AddWithValue("$worldSnapshot", snapshot.WorldStateSnapshot ?? string.Empty);

        command.ExecuteNonQuery();

        // Get the inserted ID
        var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT last_insert_rowid()";
        var carryoverId = Convert.ToInt32(idCommand.ExecuteScalar());

        _log.Information("Saved carryover snapshot: CharacterId={CharacterId}, Tier={Tier}, SnapshotId={SnapshotId}",
            snapshot.CharacterId, snapshot.NGPlusTier, carryoverId);

        return carryoverId;
    }

    /// <summary>
    /// Get the most recent carryover snapshot for a character
    /// </summary>
    public CarryoverSnapshot? GetLatestCarryoverSnapshot(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM NG_Plus_Carryover
            WHERE character_id = $characterId
            ORDER BY snapshot_timestamp DESC
            LIMIT 1";
        command.Parameters.AddWithValue("$characterId", characterId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapCarryoverSnapshot(reader);
        }

        return null;
    }

    /// <summary>
    /// Get all carryover snapshots for a character
    /// </summary>
    public List<CarryoverSnapshot> GetAllCarryoverSnapshots(int characterId)
    {
        var snapshots = new List<CarryoverSnapshot>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM NG_Plus_Carryover
            WHERE character_id = $characterId
            ORDER BY snapshot_timestamp DESC";
        command.Parameters.AddWithValue("$characterId", characterId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            snapshots.Add(MapCarryoverSnapshot(reader));
        }

        return snapshots;
    }

    private CarryoverSnapshot MapCarryoverSnapshot(SqliteDataReader reader)
    {
        var snapshot = new CarryoverSnapshot
        {
            CarryoverId = reader.GetInt32(reader.GetOrdinal("carryover_id")),
            CharacterId = reader.GetInt32(reader.GetOrdinal("character_id")),
            NGPlusTier = reader.GetInt32(reader.GetOrdinal("ng_plus_tier")),
            TimestampUtc = reader.GetDateTime(reader.GetOrdinal("snapshot_timestamp"))
        };

        // Deserialize JSON data
        var characterData = JsonSerializer.Deserialize<Dictionary<string, object>>(
            reader.GetString(reader.GetOrdinal("character_data")));
        if (characterData != null)
        {
            snapshot.CharacterLevel = Convert.ToInt32(characterData["Level"]);
            snapshot.LegendPoints = Convert.ToInt32(characterData["LegendPoints"]);
            snapshot.ProgressionPoints = Convert.ToInt32(characterData["ProgressionPoints"]);
            snapshot.UnspentProgressionPoints = Convert.ToInt32(characterData["UnspentProgressionPoints"]);

            if (characterData.ContainsKey("Attributes"))
            {
                var attributesJson = characterData["Attributes"].ToString();
                snapshot.Attributes = JsonSerializer.Deserialize<Dictionary<string, int>>(attributesJson!) ?? new();
            }
        }

        var specializationData = JsonSerializer.Deserialize<Dictionary<string, object>>(
            reader.GetString(reader.GetOrdinal("specialization_data")));
        if (specializationData != null)
        {
            if (specializationData.ContainsKey("UnlockedSpecializations"))
            {
                var specsJson = specializationData["UnlockedSpecializations"].ToString();
                snapshot.UnlockedSpecializations = JsonSerializer.Deserialize<List<string>>(specsJson!) ?? new();
            }
            if (specializationData.ContainsKey("LearnedAbilities"))
            {
                var abilitiesJson = specializationData["LearnedAbilities"].ToString();
                snapshot.LearnedAbilities = JsonSerializer.Deserialize<List<string>>(abilitiesJson!) ?? new();
            }
        }

        snapshot.QuestStateSnapshot = reader.IsDBNull(reader.GetOrdinal("quest_state_snapshot"))
            ? null
            : reader.GetString(reader.GetOrdinal("quest_state_snapshot"));

        snapshot.WorldStateSnapshot = reader.IsDBNull(reader.GetOrdinal("world_state_snapshot"))
            ? null
            : reader.GetString(reader.GetOrdinal("world_state_snapshot"));

        return snapshot;
    }

    // ═════════════════════════════════════════════════════════════
    // SCALING PARAMETERS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get scaling parameters for a specific NG+ tier
    /// </summary>
    public NGPlusScaling? GetScalingForTier(int tier)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM NG_Plus_Scaling
            WHERE tier = $tier";
        command.Parameters.AddWithValue("$tier", tier);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new NGPlusScaling
            {
                Tier = reader.GetInt32(reader.GetOrdinal("tier")),
                DifficultyMultiplier = reader.GetFloat(reader.GetOrdinal("difficulty_multiplier")),
                EnemyLevelIncrease = reader.GetInt32(reader.GetOrdinal("enemy_level_increase")),
                BossPhaseThresholdReduction = reader.GetFloat(reader.GetOrdinal("boss_phase_threshold_reduction")),
                CorruptionRateMultiplier = reader.GetFloat(reader.GetOrdinal("corruption_rate_multiplier")),
                LegendRewardMultiplier = reader.GetFloat(reader.GetOrdinal("legend_reward_multiplier")),
                Description = reader.GetString(reader.GetOrdinal("description"))
            };
        }

        return null;
    }

    /// <summary>
    /// Get all scaling tiers (1-5)
    /// </summary>
    public List<NGPlusScaling> GetAllScalingTiers()
    {
        var tiers = new List<NGPlusScaling>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM NG_Plus_Scaling ORDER BY tier";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tiers.Add(new NGPlusScaling
            {
                Tier = reader.GetInt32(reader.GetOrdinal("tier")),
                DifficultyMultiplier = reader.GetFloat(reader.GetOrdinal("difficulty_multiplier")),
                EnemyLevelIncrease = reader.GetInt32(reader.GetOrdinal("enemy_level_increase")),
                BossPhaseThresholdReduction = reader.GetFloat(reader.GetOrdinal("boss_phase_threshold_reduction")),
                CorruptionRateMultiplier = reader.GetFloat(reader.GetOrdinal("corruption_rate_multiplier")),
                LegendRewardMultiplier = reader.GetFloat(reader.GetOrdinal("legend_reward_multiplier")),
                Description = reader.GetString(reader.GetOrdinal("description"))
            });
        }

        return tiers;
    }

    // ═════════════════════════════════════════════════════════════
    // COMPLETION HISTORY
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Log completion of an NG+ tier
    /// </summary>
    public int LogCompletion(int characterId, int completedTier, int? playtimeSeconds = null,
        int deaths = 0, int bossesDefeated = 0)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO NG_Plus_Completions (
                character_id, completed_tier, completion_timestamp,
                total_playtime_seconds, deaths_during_run, bosses_defeated
            ) VALUES (
                $characterId, $tier, $timestamp,
                $playtime, $deaths, $bosses
            )";

        command.Parameters.AddWithValue("$characterId", characterId);
        command.Parameters.AddWithValue("$tier", completedTier);
        command.Parameters.AddWithValue("$timestamp", DateTime.UtcNow);
        command.Parameters.AddWithValue("$playtime", playtimeSeconds.HasValue ? playtimeSeconds.Value : DBNull.Value);
        command.Parameters.AddWithValue("$deaths", deaths);
        command.Parameters.AddWithValue("$bosses", bossesDefeated);

        command.ExecuteNonQuery();

        // Get the inserted ID
        var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT last_insert_rowid()";
        var completionId = Convert.ToInt32(idCommand.ExecuteScalar());

        _log.Information("Logged NG+ completion: CharacterId={CharacterId}, Tier={Tier}, CompletionId={CompletionId}",
            characterId, completedTier, completionId);

        return completionId;
    }

    /// <summary>
    /// Get all completions for a character
    /// </summary>
    public List<NGPlusCompletion> GetCompletions(int characterId)
    {
        var completions = new List<NGPlusCompletion>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM NG_Plus_Completions
            WHERE character_id = $characterId
            ORDER BY completion_timestamp DESC";
        command.Parameters.AddWithValue("$characterId", characterId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            completions.Add(new NGPlusCompletion
            {
                CompletionId = reader.GetInt32(reader.GetOrdinal("completion_id")),
                CharacterId = reader.GetInt32(reader.GetOrdinal("character_id")),
                CompletedTier = reader.GetInt32(reader.GetOrdinal("completed_tier")),
                CompletionTimestamp = reader.GetDateTime(reader.GetOrdinal("completion_timestamp")),
                TotalPlaytimeSeconds = reader.IsDBNull(reader.GetOrdinal("total_playtime_seconds"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("total_playtime_seconds")),
                DeathsDuringRun = reader.GetInt32(reader.GetOrdinal("deaths_during_run")),
                BossesDefeated = reader.GetInt32(reader.GetOrdinal("bosses_defeated"))
            });
        }

        return completions;
    }

    /// <summary>
    /// Get completions for a specific tier
    /// </summary>
    public List<NGPlusCompletion> GetCompletionsForTier(int characterId, int tier)
    {
        var completions = new List<NGPlusCompletion>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM NG_Plus_Completions
            WHERE character_id = $characterId AND completed_tier = $tier
            ORDER BY completion_timestamp DESC";
        command.Parameters.AddWithValue("$characterId", characterId);
        command.Parameters.AddWithValue("$tier", tier);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            completions.Add(new NGPlusCompletion
            {
                CompletionId = reader.GetInt32(reader.GetOrdinal("completion_id")),
                CharacterId = reader.GetInt32(reader.GetOrdinal("character_id")),
                CompletedTier = reader.GetInt32(reader.GetOrdinal("completed_tier")),
                CompletionTimestamp = reader.GetDateTime(reader.GetOrdinal("completion_timestamp")),
                TotalPlaytimeSeconds = reader.IsDBNull(reader.GetOrdinal("total_playtime_seconds"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("total_playtime_seconds")),
                DeathsDuringRun = reader.GetInt32(reader.GetOrdinal("deaths_during_run")),
                BossesDefeated = reader.GetInt32(reader.GetOrdinal("bosses_defeated"))
            });
        }

        return completions;
    }
}
