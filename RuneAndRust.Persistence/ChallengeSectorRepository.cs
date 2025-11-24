using Microsoft.Data.Sqlite;
using RuneAndRust.Core.ChallengeSectors;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.40.2: Repository for Challenge Sector data access
/// Manages modifiers, sectors, completions, and progress tracking
/// </summary>
public class ChallengeSectorRepository
{
    private static readonly ILogger _log = Log.ForContext<ChallengeSectorRepository>();
    private readonly string _connectionString;
    private const string DatabaseName = "runeandrust.db";

    public ChallengeSectorRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, DatabaseName);
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("ChallengeSectorRepository initialized with database path: {DbPath}", dbPath);
    }

    // ═════════════════════════════════════════════════════════════
    // CHALLENGE MODIFIERS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all challenge modifiers
    /// </summary>
    public List<ChallengeModifier> GetAllModifiers()
    {
        var modifiers = new List<ChallengeModifier>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Challenge_Modifiers
            WHERE is_active = 1
            ORDER BY sort_order, category, name";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            modifiers.Add(MapModifier(reader));
        }

        _log.Debug("Loaded {Count} challenge modifiers", modifiers.Count);
        return modifiers;
    }

    /// <summary>
    /// Get modifier by ID
    /// </summary>
    public ChallengeModifier? GetModifierById(string modifierId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Challenge_Modifiers
            WHERE modifier_id = $modifierId AND is_active = 1";
        command.Parameters.AddWithValue("$modifierId", modifierId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapModifier(reader);
        }

        return null;
    }

    /// <summary>
    /// Get modifiers by category
    /// </summary>
    public List<ChallengeModifier> GetModifiersByCategory(ChallengeModifierCategory category)
    {
        var modifiers = new List<ChallengeModifier>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Challenge_Modifiers
            WHERE category = $category AND is_active = 1
            ORDER BY sort_order, name";
        command.Parameters.AddWithValue("$category", category.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            modifiers.Add(MapModifier(reader));
        }

        return modifiers;
    }

    private ChallengeModifier MapModifier(SqliteDataReader reader)
    {
        var modifier = new ChallengeModifier
        {
            ModifierId = reader.GetString(reader.GetOrdinal("modifier_id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Description = reader.GetString(reader.GetOrdinal("description")),
            DifficultyMultiplier = reader.GetFloat(reader.GetOrdinal("difficulty_multiplier")),
            IsActive = reader.GetInt32(reader.GetOrdinal("is_active")) == 1,
            SortOrder = reader.GetInt32(reader.GetOrdinal("sort_order"))
        };

        // Parse category
        var categoryStr = reader.GetString(reader.GetOrdinal("category"));
        if (Enum.TryParse<ChallengeModifierCategory>(categoryStr, out var category))
        {
            modifier.Category = category;
        }

        // Parse parameters JSON
        var parametersJson = reader.IsDBNull(reader.GetOrdinal("parameters"))
            ? "{}"
            : reader.GetString(reader.GetOrdinal("parameters"));

        try
        {
            modifier.Parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson) ?? new();
        }
        catch
        {
            modifier.Parameters = new Dictionary<string, object>();
        }

        // Application logic
        modifier.ApplicationLogic = reader.IsDBNull(reader.GetOrdinal("application_logic"))
            ? null
            : reader.GetString(reader.GetOrdinal("application_logic"));

        return modifier;
    }

    // ═════════════════════════════════════════════════════════════
    // CHALLENGE SECTORS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all challenge sectors
    /// </summary>
    public List<ChallengeSector> GetAllSectors()
    {
        var sectors = new List<ChallengeSector>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Challenge_Sectors
            WHERE is_active = 1
            ORDER BY sort_order, total_difficulty_multiplier, name";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            sectors.Add(MapSector(reader));
        }

        _log.Debug("Loaded {Count} challenge sectors", sectors.Count);

        // Load modifiers for each sector
        foreach (var sector in sectors)
        {
            sector.ModifierIds = GetModifierIdsForSector(sector.SectorId);
        }

        return sectors;
    }

    /// <summary>
    /// Get sector by ID
    /// </summary>
    public ChallengeSector? GetSectorById(string sectorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Challenge_Sectors
            WHERE sector_id = $sectorId AND is_active = 1";
        command.Parameters.AddWithValue("$sectorId", sectorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var sector = MapSector(reader);
            sector.ModifierIds = GetModifierIdsForSector(sectorId);
            return sector;
        }

        return null;
    }

    /// <summary>
    /// Get sectors available for a character (based on NG+ tier and prerequisites)
    /// </summary>
    public List<ChallengeSector> GetAvailableSectorsForCharacter(int characterId, int ngPlusTier)
    {
        var allSectors = GetAllSectors();
        var completedSectorIds = GetCompletedSectorIds(characterId);

        return allSectors
            .Where(s => s.RequiredNGPlusTier <= ngPlusTier) // NG+ tier requirement met
            .Where(s => ArePrerequisitesMet(s.PrerequisiteSectorIds, completedSectorIds)) // Prerequisites met
            .ToList();
    }

    private List<string> GetModifierIdsForSector(string sectorId)
    {
        var modifierIds = new List<string>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT modifier_id FROM Challenge_Sector_Modifiers
            WHERE sector_id = $sectorId
            ORDER BY application_order";
        command.Parameters.AddWithValue("$sectorId", sectorId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            modifierIds.Add(reader.GetString(0));
        }

        return modifierIds;
    }

    private HashSet<string> GetCompletedSectorIds(int characterId)
    {
        var completedIds = new HashSet<string>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT DISTINCT sector_id FROM Challenge_Completions
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$characterId", characterId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            completedIds.Add(reader.GetString(0));
        }

        return completedIds;
    }

    private bool ArePrerequisitesMet(List<string> prerequisites, HashSet<string> completedSectorIds)
    {
        if (prerequisites == null || !prerequisites.Any())
        {
            return true; // No prerequisites
        }

        return prerequisites.All(prereq => completedSectorIds.Contains(prereq));
    }

    private ChallengeSector MapSector(SqliteDataReader reader)
    {
        var sector = new ChallengeSector
        {
            SectorId = reader.GetString(reader.GetOrdinal("sector_id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Description = reader.GetString(reader.GetOrdinal("description")),
            TotalDifficultyMultiplier = reader.GetFloat(reader.GetOrdinal("total_difficulty_multiplier")),
            BiomeTheme = reader.GetString(reader.GetOrdinal("biome_theme")),
            RoomCount = reader.GetInt32(reader.GetOrdinal("room_count")),
            RequiredNGPlusTier = reader.GetInt32(reader.GetOrdinal("required_ng_plus_tier")),
            IsActive = reader.GetInt32(reader.GetOrdinal("is_active")) == 1,
            SortOrder = reader.GetInt32(reader.GetOrdinal("sort_order"))
        };

        // Optional fields
        sector.LoreText = reader.IsDBNull(reader.GetOrdinal("lore_text"))
            ? null
            : reader.GetString(reader.GetOrdinal("lore_text"));

        sector.UniqueRewardId = reader.IsDBNull(reader.GetOrdinal("unique_reward_id"))
            ? null
            : reader.GetString(reader.GetOrdinal("unique_reward_id"));

        sector.UniqueRewardName = reader.IsDBNull(reader.GetOrdinal("unique_reward_name"))
            ? null
            : reader.GetString(reader.GetOrdinal("unique_reward_name"));

        sector.UniqueRewardDescription = reader.IsDBNull(reader.GetOrdinal("unique_reward_description"))
            ? null
            : reader.GetString(reader.GetOrdinal("unique_reward_description"));

        sector.DesignNotes = reader.IsDBNull(reader.GetOrdinal("design_notes"))
            ? null
            : reader.GetString(reader.GetOrdinal("design_notes"));

        // Parse JSON arrays
        var enemyPoolJson = reader.IsDBNull(reader.GetOrdinal("enemy_pool"))
            ? "[]"
            : reader.GetString(reader.GetOrdinal("enemy_pool"));
        sector.EnemyPool = JsonSerializer.Deserialize<List<string>>(enemyPoolJson) ?? new();

        var prerequisitesJson = reader.IsDBNull(reader.GetOrdinal("prerequisite_sectors"))
            ? "[]"
            : reader.GetString(reader.GetOrdinal("prerequisite_sectors"));
        sector.PrerequisiteSectorIds = JsonSerializer.Deserialize<List<string>>(prerequisitesJson) ?? new();

        return sector;
    }

    // ═════════════════════════════════════════════════════════════
    // COMPLETIONS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Log a challenge sector completion
    /// </summary>
    public int LogCompletion(int characterId, string sectorId, int? completionTimeSeconds = null,
        int deaths = 0, int damageTaken = 0, int damageDealt = 0, int enemiesKilled = 0, int ngPlusTier = 0)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if this is first completion
        var isFirstCompletion = !HasCompletedSector(characterId, sectorId);

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Challenge_Completions (
                character_id, sector_id, completed_at, completion_time_seconds,
                deaths_during_run, damage_taken, damage_dealt, enemies_killed,
                ng_plus_tier, is_first_completion
            ) VALUES (
                $characterId, $sectorId, $completedAt, $completionTime,
                $deaths, $damageTaken, $damageDealt, $enemiesKilled,
                $ngPlusTier, $isFirst
            )";

        command.Parameters.AddWithValue("$characterId", characterId);
        command.Parameters.AddWithValue("$sectorId", sectorId);
        command.Parameters.AddWithValue("$completedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("$completionTime", completionTimeSeconds.HasValue ? completionTimeSeconds.Value : DBNull.Value);
        command.Parameters.AddWithValue("$deaths", deaths);
        command.Parameters.AddWithValue("$damageTaken", damageTaken);
        command.Parameters.AddWithValue("$damageDealt", damageDealt);
        command.Parameters.AddWithValue("$enemiesKilled", enemiesKilled);
        command.Parameters.AddWithValue("$ngPlusTier", ngPlusTier);
        command.Parameters.AddWithValue("$isFirst", isFirstCompletion ? 1 : 0);

        command.ExecuteNonQuery();

        // Get inserted ID
        var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT last_insert_rowid()";
        var completionId = Convert.ToInt32(idCommand.ExecuteScalar());

        _log.Information(
            "Logged challenge completion: CharacterId={CharacterId}, SectorId={SectorId}, CompletionId={CompletionId}, FirstTime={First}",
            characterId, sectorId, completionId, isFirstCompletion);

        // Update progress tracking
        UpdateProgressTracking(characterId);

        return completionId;
    }

    /// <summary>
    /// Check if a character has completed a specific sector
    /// </summary>
    public bool HasCompletedSector(int characterId, string sectorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*) FROM Challenge_Completions
            WHERE character_id = $characterId AND sector_id = $sectorId";
        command.Parameters.AddWithValue("$characterId", characterId);
        command.Parameters.AddWithValue("$sectorId", sectorId);

        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }

    /// <summary>
    /// Get completion history for a character
    /// </summary>
    public List<ChallengeCompletion> GetCompletions(int characterId)
    {
        var completions = new List<ChallengeCompletion>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Challenge_Completions
            WHERE character_id = $characterId
            ORDER BY completed_at DESC";
        command.Parameters.AddWithValue("$characterId", characterId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            completions.Add(MapCompletion(reader));
        }

        return completions;
    }

    private ChallengeCompletion MapCompletion(SqliteDataReader reader)
    {
        return new ChallengeCompletion
        {
            CompletionId = reader.GetInt32(reader.GetOrdinal("completion_id")),
            CharacterId = reader.GetInt32(reader.GetOrdinal("character_id")),
            SectorId = reader.GetString(reader.GetOrdinal("sector_id")),
            CompletedAt = reader.GetDateTime(reader.GetOrdinal("completed_at")),
            CompletionTimeSeconds = reader.IsDBNull(reader.GetOrdinal("completion_time_seconds"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("completion_time_seconds")),
            DeathsDuringRun = reader.GetInt32(reader.GetOrdinal("deaths_during_run")),
            DamageTaken = reader.GetInt32(reader.GetOrdinal("damage_taken")),
            DamageDealt = reader.GetInt32(reader.GetOrdinal("damage_dealt")),
            EnemiesKilled = reader.GetInt32(reader.GetOrdinal("enemies_killed")),
            NGPlusTier = reader.GetInt32(reader.GetOrdinal("ng_plus_tier")),
            IsFirstCompletion = reader.GetInt32(reader.GetOrdinal("is_first_completion")) == 1
        };
    }

    // ═════════════════════════════════════════════════════════════
    // PROGRESS TRACKING
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get overall progress for a character
    /// </summary>
    public ChallengeSectorProgress GetProgress(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Challenge_Progress
            WHERE character_id = $characterId";
        command.Parameters.AddWithValue("$characterId", characterId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapProgress(reader);
        }

        // Create initial progress if doesn't exist
        return CreateInitialProgress(characterId);
    }

    private ChallengeSectorProgress MapProgress(SqliteDataReader reader)
    {
        return new ChallengeSectorProgress
        {
            CharacterId = reader.GetInt32(reader.GetOrdinal("character_id")),
            TotalSectorsCompleted = reader.GetInt32(reader.GetOrdinal("total_sectors_completed")),
            TotalAttempts = reader.GetInt32(reader.GetOrdinal("total_attempts")),
            FastestSectorId = reader.IsDBNull(reader.GetOrdinal("fastest_sector_id"))
                ? null
                : reader.GetString(reader.GetOrdinal("fastest_sector_id")),
            FastestCompletionTime = reader.IsDBNull(reader.GetOrdinal("fastest_completion_time"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("fastest_completion_time")),
            TotalChallengeTimeSeconds = reader.GetInt32(reader.GetOrdinal("total_challenge_time_seconds")),
            TotalDeathsInChallenges = reader.GetInt32(reader.GetOrdinal("total_deaths_in_challenges")),
            AllSectorsCompleted = reader.GetInt32(reader.GetOrdinal("all_sectors_completed")) == 1,
            PerfectRunCount = reader.GetInt32(reader.GetOrdinal("perfect_run_count")),
            LastUpdated = reader.GetDateTime(reader.GetOrdinal("last_updated")),
            TotalSectorsAvailable = GetAllSectors().Count
        };
    }

    private ChallengeSectorProgress CreateInitialProgress(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Challenge_Progress (character_id)
            VALUES ($characterId)";
        command.Parameters.AddWithValue("$characterId", characterId);
        command.ExecuteNonQuery();

        return new ChallengeSectorProgress
        {
            CharacterId = characterId,
            TotalSectorsAvailable = GetAllSectors().Count
        };
    }

    private void UpdateProgressTracking(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Challenge_Progress (
                character_id, total_sectors_completed, total_attempts,
                total_challenge_time_seconds, total_deaths_in_challenges,
                perfect_run_count, last_updated
            )
            SELECT
                $characterId,
                COUNT(DISTINCT sector_id) AS total_completed,
                COUNT(*) AS total_attempts,
                COALESCE(SUM(completion_time_seconds), 0) AS total_time,
                SUM(deaths_during_run) AS total_deaths,
                SUM(CASE WHEN deaths_during_run = 0 THEN 1 ELSE 0 END) AS perfect_runs,
                CURRENT_TIMESTAMP
            FROM Challenge_Completions
            WHERE character_id = $characterId";

        command.Parameters.AddWithValue("$characterId", characterId);
        command.ExecuteNonQuery();
    }
}
