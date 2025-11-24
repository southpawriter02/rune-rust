using Microsoft.Data.Sqlite;
using RuneAndRust.Core.EndlessMode;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.40.4: Endless Mode Repository
/// Data access layer for endless mode runs, waves, seasons, and leaderboards
/// </summary>
public class EndlessModeRepository
{
    private static readonly ILogger _log = Log.ForContext<EndlessModeRepository>();
    private readonly string _connectionString;

    public EndlessModeRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
        _log.Debug("EndlessModeRepository initialized with database: {DatabasePath}", databasePath);
    }

    // ═══════════════════════════════════════════════════════════
    // RUNS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Create a new endless mode run
    /// </summary>
    public int CreateRun(EndlessRun run)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Endless_Mode_Runs
            (character_id, seed, start_time, is_active, highest_wave_reached, current_wave,
             character_build_hash)
            VALUES
            (@characterId, @seed, @startTime, @isActive, @highestWave, @currentWave,
             @buildHash)";

        cmd.Parameters.AddWithValue("@characterId", run.CharacterId);
        cmd.Parameters.AddWithValue("@seed", run.Seed);
        cmd.Parameters.AddWithValue("@startTime", run.StartTime);
        cmd.Parameters.AddWithValue("@isActive", run.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@highestWave", run.HighestWaveReached);
        cmd.Parameters.AddWithValue("@currentWave", run.CurrentWave);
        cmd.Parameters.AddWithValue("@buildHash", (object?)run.CharacterBuildHash ?? DBNull.Value);

        cmd.ExecuteNonQuery();

        var runId = (int)conn.LastInsertRowId;
        _log.Information("Created endless run {RunId} for character {CharacterId} with seed {Seed}",
            runId, run.CharacterId, run.Seed);

        return runId;
    }

    /// <summary>
    /// Get a run by ID
    /// </summary>
    public EndlessRun? GetRunById(int runId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT run_id, character_id, seed, start_time, end_time, is_active,
                   highest_wave_reached, current_wave, total_enemies_killed, total_bosses_killed,
                   total_damage_taken, total_damage_dealt, total_time_seconds,
                   wave_score, kill_score, boss_score, time_bonus, survival_bonus, total_score,
                   character_build_hash, is_verified
            FROM Endless_Mode_Runs
            WHERE run_id = @runId";
        cmd.Parameters.AddWithValue("@runId", runId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapRunFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Get active run for a character
    /// </summary>
    public EndlessRun? GetActiveRun(int characterId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT run_id, character_id, seed, start_time, end_time, is_active,
                   highest_wave_reached, current_wave, total_enemies_killed, total_bosses_killed,
                   total_damage_taken, total_damage_dealt, total_time_seconds,
                   wave_score, kill_score, boss_score, time_bonus, survival_bonus, total_score,
                   character_build_hash, is_verified
            FROM Endless_Mode_Runs
            WHERE character_id = @characterId
              AND is_active = 1
            ORDER BY start_time DESC
            LIMIT 1";
        cmd.Parameters.AddWithValue("@characterId", characterId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapRunFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Update an existing run
    /// </summary>
    public void UpdateRun(EndlessRun run)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Endless_Mode_Runs
            SET end_time = @endTime,
                is_active = @isActive,
                highest_wave_reached = @highestWave,
                current_wave = @currentWave,
                total_enemies_killed = @enemiesKilled,
                total_bosses_killed = @bossesKilled,
                total_damage_taken = @damageTaken,
                total_damage_dealt = @damageDealt,
                total_time_seconds = @totalTime,
                wave_score = @waveScore,
                kill_score = @killScore,
                boss_score = @bossScore,
                time_bonus = @timeBonus,
                survival_bonus = @survivalBonus,
                total_score = @totalScore,
                is_verified = @isVerified
            WHERE run_id = @runId";

        cmd.Parameters.AddWithValue("@runId", run.RunId);
        cmd.Parameters.AddWithValue("@endTime", (object?)run.EndTime ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@isActive", run.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@highestWave", run.HighestWaveReached);
        cmd.Parameters.AddWithValue("@currentWave", run.CurrentWave);
        cmd.Parameters.AddWithValue("@enemiesKilled", run.TotalEnemiesKilled);
        cmd.Parameters.AddWithValue("@bossesKilled", run.TotalBossesKilled);
        cmd.Parameters.AddWithValue("@damageTaken", run.TotalDamageTaken);
        cmd.Parameters.AddWithValue("@damageDealt", run.TotalDamageDealt);
        cmd.Parameters.AddWithValue("@totalTime", run.TotalTimeSeconds);
        cmd.Parameters.AddWithValue("@waveScore", run.WaveScore);
        cmd.Parameters.AddWithValue("@killScore", run.KillScore);
        cmd.Parameters.AddWithValue("@bossScore", run.BossScore);
        cmd.Parameters.AddWithValue("@timeBonus", run.TimeBonus);
        cmd.Parameters.AddWithValue("@survivalBonus", run.SurvivalBonus);
        cmd.Parameters.AddWithValue("@totalScore", run.TotalScore);
        cmd.Parameters.AddWithValue("@isVerified", run.IsVerified ? 1 : 0);

        cmd.ExecuteNonQuery();

        _log.Debug("Updated run {RunId}: Wave {Wave}, Score {Score}",
            run.RunId, run.HighestWaveReached, run.TotalScore);
    }

    // ═══════════════════════════════════════════════════════════
    // WAVES
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Create a new wave record
    /// </summary>
    public int CreateWave(EndlessWave wave)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Endless_Mode_Waves
            (run_id, wave_number, enemy_count, enemy_level, difficulty_multiplier,
             is_boss_wave, start_time)
            VALUES
            (@runId, @waveNumber, @enemyCount, @enemyLevel, @difficultyMultiplier,
             @isBossWave, @startTime)";

        cmd.Parameters.AddWithValue("@runId", wave.RunId);
        cmd.Parameters.AddWithValue("@waveNumber", wave.WaveNumber);
        cmd.Parameters.AddWithValue("@enemyCount", wave.EnemyCount);
        cmd.Parameters.AddWithValue("@enemyLevel", wave.EnemyLevel);
        cmd.Parameters.AddWithValue("@difficultyMultiplier", wave.DifficultyMultiplier);
        cmd.Parameters.AddWithValue("@isBossWave", wave.IsBossWave ? 1 : 0);
        cmd.Parameters.AddWithValue("@startTime", wave.StartTime);

        cmd.ExecuteNonQuery();

        var waveId = (int)conn.LastInsertRowId;
        _log.Debug("Created wave {WaveId}: Run {RunId}, Wave {WaveNumber}",
            waveId, wave.RunId, wave.WaveNumber);

        return waveId;
    }

    /// <summary>
    /// Update wave completion data
    /// </summary>
    public void UpdateWave(EndlessWave wave)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Endless_Mode_Waves
            SET end_time = @endTime,
                wave_time_seconds = @waveTime,
                enemies_killed = @enemiesKilled,
                damage_taken = @damageTaken,
                damage_dealt = @damageDealt
            WHERE wave_id = @waveId";

        cmd.Parameters.AddWithValue("@waveId", wave.WaveId);
        cmd.Parameters.AddWithValue("@endTime", (object?)wave.EndTime ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@waveTime", (object?)wave.WaveTimeSeconds ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@enemiesKilled", wave.EnemiesKilled);
        cmd.Parameters.AddWithValue("@damageTaken", wave.DamageTaken);
        cmd.Parameters.AddWithValue("@damageDealt", wave.DamageDealt);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Get all waves for a run
    /// </summary>
    public List<EndlessWave> GetWavesForRun(int runId)
    {
        var waves = new List<EndlessWave>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT wave_id, run_id, wave_number, enemy_count, enemy_level,
                   difficulty_multiplier, is_boss_wave, start_time, end_time,
                   wave_time_seconds, enemies_killed, damage_taken, damage_dealt
            FROM Endless_Mode_Waves
            WHERE run_id = @runId
            ORDER BY wave_number";
        cmd.Parameters.AddWithValue("@runId", runId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            waves.Add(MapWaveFromReader(reader));
        }

        return waves;
    }

    // ═══════════════════════════════════════════════════════════
    // LEADERBOARDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Upsert a leaderboard entry
    /// </summary>
    public void UpsertLeaderboardEntry(EndlessLeaderboardEntry entry)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // Check if entry exists for this character/category/season
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = @"
            SELECT entry_id FROM Endless_Mode_Leaderboards
            WHERE character_id = @characterId
              AND category = @category
              AND season_id = @seasonId";
        checkCmd.Parameters.AddWithValue("@characterId", entry.CharacterId);
        checkCmd.Parameters.AddWithValue("@category", entry.Category.ToString().ToLowerInvariant().Replace("highest", "highest_"));
        checkCmd.Parameters.AddWithValue("@seasonId", (object?)entry.SeasonId ?? DBNull.Value);

        var existingId = checkCmd.ExecuteScalar();

        if (existingId != null)
        {
            // Update if new score is better
            var categoryField = entry.Category == EndlessLeaderboardCategory.HighestWave
                ? "highest_wave_reached"
                : "total_score";

            var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = $@"
                UPDATE Endless_Mode_Leaderboards
                SET run_id = @runId,
                    player_name = @playerName,
                    highest_wave_reached = @highestWave,
                    total_score = @totalScore,
                    total_time_seconds = @totalTime,
                    character_level = @characterLevel,
                    specialization_name = @specializationName,
                    seed = @seed,
                    character_build_hash = @buildHash,
                    is_verified = @isVerified,
                    submitted_at = @submittedAt
                WHERE entry_id = @entryId
                  AND {categoryField} < @newValue";

            updateCmd.Parameters.AddWithValue("@entryId", existingId);
            updateCmd.Parameters.AddWithValue("@runId", entry.RunId);
            updateCmd.Parameters.AddWithValue("@playerName", entry.PlayerName);
            updateCmd.Parameters.AddWithValue("@highestWave", entry.HighestWaveReached);
            updateCmd.Parameters.AddWithValue("@totalScore", entry.TotalScore);
            updateCmd.Parameters.AddWithValue("@totalTime", entry.TotalTimeSeconds);
            updateCmd.Parameters.AddWithValue("@characterLevel", (object?)entry.CharacterLevel ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@specializationName", (object?)entry.SpecializationName ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@seed", entry.Seed);
            updateCmd.Parameters.AddWithValue("@buildHash", (object?)entry.CharacterBuildHash ?? DBNull.Value);
            updateCmd.Parameters.AddWithValue("@isVerified", entry.IsVerified ? 1 : 0);
            updateCmd.Parameters.AddWithValue("@submittedAt", entry.SubmittedAt);
            updateCmd.Parameters.AddWithValue("@newValue", entry.Category == EndlessLeaderboardCategory.HighestWave
                ? entry.HighestWaveReached
                : entry.TotalScore);

            updateCmd.ExecuteNonQuery();
        }
        else
        {
            // Insert new entry
            var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Endless_Mode_Leaderboards
                (run_id, character_id, player_name, category, highest_wave_reached, total_score,
                 total_time_seconds, character_level, specialization_name, seed,
                 character_build_hash, is_verified, season_id, submitted_at)
                VALUES
                (@runId, @characterId, @playerName, @category, @highestWave, @totalScore,
                 @totalTime, @characterLevel, @specializationName, @seed,
                 @buildHash, @isVerified, @seasonId, @submittedAt)";

            insertCmd.Parameters.AddWithValue("@runId", entry.RunId);
            insertCmd.Parameters.AddWithValue("@characterId", entry.CharacterId);
            insertCmd.Parameters.AddWithValue("@playerName", entry.PlayerName);
            insertCmd.Parameters.AddWithValue("@category", entry.Category.ToString().ToLowerInvariant().Replace("highest", "highest_"));
            insertCmd.Parameters.AddWithValue("@highestWave", entry.HighestWaveReached);
            insertCmd.Parameters.AddWithValue("@totalScore", entry.TotalScore);
            insertCmd.Parameters.AddWithValue("@totalTime", entry.TotalTimeSeconds);
            insertCmd.Parameters.AddWithValue("@characterLevel", (object?)entry.CharacterLevel ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@specializationName", (object?)entry.SpecializationName ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@seed", entry.Seed);
            insertCmd.Parameters.AddWithValue("@buildHash", (object?)entry.CharacterBuildHash ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@isVerified", entry.IsVerified ? 1 : 0);
            insertCmd.Parameters.AddWithValue("@seasonId", (object?)entry.SeasonId ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@submittedAt", entry.SubmittedAt);

            insertCmd.ExecuteNonQuery();
        }

        // Recalculate ranks
        RecalculateRanks(entry.Category, entry.SeasonId);

        _log.Information("Upserted leaderboard entry: Category={Category}, Character={CharacterId}, Wave={Wave}, Score={Score}",
            entry.Category, entry.CharacterId, entry.HighestWaveReached, entry.TotalScore);
    }

    /// <summary>
    /// Get leaderboard entries for a category
    /// </summary>
    public List<EndlessLeaderboardEntry> GetLeaderboard(EndlessLeaderboardCategory category,
        string? seasonId = null, int limit = 100)
    {
        var entries = new List<EndlessLeaderboardEntry>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();

        var categoryStr = category.ToString().ToLowerInvariant().Replace("highest", "highest_");
        var orderBy = category == EndlessLeaderboardCategory.HighestWave
            ? "highest_wave_reached DESC, total_score DESC"
            : "total_score DESC, highest_wave_reached DESC";

        if (seasonId != null)
        {
            cmd.CommandText = $@"
                SELECT entry_id, run_id, character_id, player_name, category,
                       highest_wave_reached, total_score, total_time_seconds,
                       character_level, specialization_name, seed, character_build_hash,
                       is_verified, report_count, season_id, submitted_at, rank
                FROM Endless_Mode_Leaderboards
                WHERE category = @category
                  AND season_id = @seasonId
                ORDER BY {orderBy}
                LIMIT @limit";
            cmd.Parameters.AddWithValue("@seasonId", seasonId);
        }
        else
        {
            cmd.CommandText = $@"
                SELECT entry_id, run_id, character_id, player_name, category,
                       highest_wave_reached, total_score, total_time_seconds,
                       character_level, specialization_name, seed, character_build_hash,
                       is_verified, report_count, season_id, submitted_at, rank
                FROM Endless_Mode_Leaderboards
                WHERE category = @category
                ORDER BY {orderBy}
                LIMIT @limit";
        }

        cmd.Parameters.AddWithValue("@category", categoryStr);
        cmd.Parameters.AddWithValue("@limit", limit);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            entries.Add(MapLeaderboardEntryFromReader(reader));
        }

        return entries;
    }

    /// <summary>
    /// Recalculate ranks for a leaderboard category
    /// </summary>
    private void RecalculateRanks(EndlessLeaderboardCategory category, string? seasonId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var entries = GetLeaderboard(category, seasonId, 1000);

        for (int i = 0; i < entries.Count; i++)
        {
            var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Endless_Mode_Leaderboards
                SET rank = @rank
                WHERE entry_id = @entryId";
            updateCmd.Parameters.AddWithValue("@rank", i + 1);
            updateCmd.Parameters.AddWithValue("@entryId", entries[i].EntryId);
            updateCmd.ExecuteNonQuery();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SEASONS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get active season
    /// </summary>
    public EndlessSeason? GetActiveSeason()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT season_id, name, start_date, end_date, is_active
            FROM Endless_Mode_Seasons
            WHERE is_active = 1
            LIMIT 1";

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapSeasonFromReader(reader);
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════
    // MAPPING METHODS
    // ═══════════════════════════════════════════════════════════

    private EndlessRun MapRunFromReader(SqliteDataReader reader)
    {
        return new EndlessRun
        {
            RunId = reader.GetInt32(0),
            CharacterId = reader.GetInt32(1),
            Seed = reader.GetString(2),
            StartTime = reader.GetDateTime(3),
            EndTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            IsActive = reader.GetBoolean(5),
            HighestWaveReached = reader.GetInt32(6),
            CurrentWave = reader.GetInt32(7),
            TotalEnemiesKilled = reader.GetInt32(8),
            TotalBossesKilled = reader.GetInt32(9),
            TotalDamageTaken = reader.GetInt32(10),
            TotalDamageDealt = reader.GetInt32(11),
            TotalTimeSeconds = reader.GetInt32(12),
            WaveScore = reader.GetInt32(13),
            KillScore = reader.GetInt32(14),
            BossScore = reader.GetInt32(15),
            TimeBonus = reader.GetInt32(16),
            SurvivalBonus = reader.GetInt32(17),
            TotalScore = reader.GetInt32(18),
            CharacterBuildHash = reader.IsDBNull(19) ? null : reader.GetString(19),
            IsVerified = reader.GetBoolean(20)
        };
    }

    private EndlessWave MapWaveFromReader(SqliteDataReader reader)
    {
        return new EndlessWave
        {
            WaveId = reader.GetInt32(0),
            RunId = reader.GetInt32(1),
            WaveNumber = reader.GetInt32(2),
            EnemyCount = reader.GetInt32(3),
            EnemyLevel = reader.GetInt32(4),
            DifficultyMultiplier = reader.GetFloat(5),
            IsBossWave = reader.GetBoolean(6),
            StartTime = reader.GetDateTime(7),
            EndTime = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
            WaveTimeSeconds = reader.IsDBNull(9) ? null : reader.GetInt32(9),
            EnemiesKilled = reader.GetInt32(10),
            DamageTaken = reader.GetInt32(11),
            DamageDealt = reader.GetInt32(12)
        };
    }

    private EndlessLeaderboardEntry MapLeaderboardEntryFromReader(SqliteDataReader reader)
    {
        var categoryStr = reader.GetString(4);
        var category = categoryStr == "highest_wave"
            ? EndlessLeaderboardCategory.HighestWave
            : EndlessLeaderboardCategory.HighestScore;

        return new EndlessLeaderboardEntry
        {
            EntryId = reader.GetInt32(0),
            RunId = reader.GetInt32(1),
            CharacterId = reader.GetInt32(2),
            PlayerName = reader.GetString(3),
            Category = category,
            HighestWaveReached = reader.GetInt32(5),
            TotalScore = reader.GetInt32(6),
            TotalTimeSeconds = reader.GetInt32(7),
            CharacterLevel = reader.IsDBNull(8) ? null : reader.GetInt32(8),
            SpecializationName = reader.IsDBNull(9) ? null : reader.GetString(9),
            Seed = reader.GetString(10),
            CharacterBuildHash = reader.IsDBNull(11) ? null : reader.GetString(11),
            IsVerified = reader.GetBoolean(12),
            ReportCount = reader.GetInt32(13),
            SeasonId = reader.IsDBNull(14) ? null : reader.GetString(14),
            SubmittedAt = reader.GetDateTime(15),
            Rank = reader.IsDBNull(16) ? null : reader.GetInt32(16)
        };
    }

    private EndlessSeason MapSeasonFromReader(SqliteDataReader reader)
    {
        return new EndlessSeason
        {
            SeasonId = reader.GetString(0),
            Name = reader.GetString(1),
            StartDate = reader.GetDateTime(2),
            EndDate = reader.GetDateTime(3),
            IsActive = reader.GetBoolean(4)
        };
    }
}
