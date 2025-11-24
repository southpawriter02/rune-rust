using Microsoft.Data.Sqlite;
using RuneAndRust.Core.BossGauntlet;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.40.3: Boss Gauntlet Repository
/// Data access layer for gauntlet sequences, runs, encounters, and leaderboards
/// </summary>
public class BossGauntletRepository
{
    private static readonly ILogger _log = Log.ForContext<BossGauntletRepository>();
    private readonly string _connectionString;

    public BossGauntletRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
        _log.Debug("BossGauntletRepository initialized with database: {DatabasePath}", databasePath);
    }

    // ═══════════════════════════════════════════════════════════
    // GAUNTLET SEQUENCES
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get all gauntlet sequences
    /// </summary>
    public List<GauntletSequence> GetAllSequences()
    {
        var sequences = new List<GauntletSequence>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT sequence_id, sequence_name, description, difficulty_tier,
                   boss_count, boss_ids, max_full_heals, max_revives,
                   required_ng_plus_tier, prerequisite_runs,
                   completion_reward_id, title_reward, active, created_at
            FROM Boss_Gauntlet_Sequences
            WHERE active = 1
            ORDER BY required_ng_plus_tier, difficulty_tier";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            sequences.Add(MapSequenceFromReader(reader));
        }

        _log.Debug("Loaded {Count} gauntlet sequences", sequences.Count);
        return sequences;
    }

    /// <summary>
    /// Get a specific gauntlet sequence by ID
    /// </summary>
    public GauntletSequence? GetSequenceById(string sequenceId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT sequence_id, sequence_name, description, difficulty_tier,
                   boss_count, boss_ids, max_full_heals, max_revives,
                   required_ng_plus_tier, prerequisite_runs,
                   completion_reward_id, title_reward, active, created_at
            FROM Boss_Gauntlet_Sequences
            WHERE sequence_id = @sequenceId";
        cmd.Parameters.AddWithValue("@sequenceId", sequenceId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapSequenceFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Get gauntlet sequences available for a character
    /// </summary>
    public List<GauntletSequence> GetAvailableSequences(int characterId, int ngPlusTier)
    {
        var allSequences = GetAllSequences();
        var completedSequenceIds = GetCompletedSequenceIds(characterId);
        var availableSequences = new List<GauntletSequence>();

        foreach (var sequence in allSequences)
        {
            // Check NG+ tier requirement
            if (sequence.RequiredNGPlusTier > ngPlusTier)
                continue;

            // Check prerequisites
            if (sequence.PrerequisiteRuns.Any(prereq => !completedSequenceIds.Contains(prereq)))
                continue;

            availableSequences.Add(sequence);
        }

        _log.Debug("Character {CharacterId} has {Count} available gauntlet sequences at NG+{Tier}",
            characterId, availableSequences.Count, ngPlusTier);

        return availableSequences;
    }

    // ═══════════════════════════════════════════════════════════
    // GAUNTLET RUNS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Start a new gauntlet run
    /// </summary>
    public int StartRun(int characterId, string sequenceId, int ngPlusTier, string startingCharacterState)
    {
        var sequence = GetSequenceById(sequenceId);
        if (sequence == null)
        {
            throw new InvalidOperationException($"Sequence {sequenceId} not found");
        }

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Boss_Gauntlet_Runs
            (character_id, sequence_id, status, current_boss_index,
             full_heals_remaining, revives_remaining,
             starting_character_state, ng_plus_tier, started_at)
            VALUES
            (@characterId, @sequenceId, 'in_progress', 0,
             @fullHeals, @revives, @startingState, @ngPlusTier, @startedAt)";

        cmd.Parameters.AddWithValue("@characterId", characterId);
        cmd.Parameters.AddWithValue("@sequenceId", sequenceId);
        cmd.Parameters.AddWithValue("@fullHeals", sequence.MaxFullHeals);
        cmd.Parameters.AddWithValue("@revives", sequence.MaxRevives);
        cmd.Parameters.AddWithValue("@startingState", startingCharacterState);
        cmd.Parameters.AddWithValue("@ngPlusTier", ngPlusTier);
        cmd.Parameters.AddWithValue("@startedAt", DateTime.UtcNow);

        cmd.ExecuteNonQuery();

        var runId = (int)conn.LastInsertRowId;
        _log.Information("Started gauntlet run {RunId} for character {CharacterId} on sequence {SequenceId}",
            runId, characterId, sequenceId);

        return runId;
    }

    /// <summary>
    /// Get a gauntlet run by ID
    /// </summary>
    public GauntletRun? GetRunById(int runId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT run_id, character_id, sequence_id, status, current_boss_index,
                   full_heals_remaining, revives_remaining,
                   total_time_seconds, total_damage_taken, total_damage_dealt, total_deaths,
                   starting_character_state, ng_plus_tier, started_at, completed_at
            FROM Boss_Gauntlet_Runs
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
    public GauntletRun? GetActiveRun(int characterId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT run_id, character_id, sequence_id, status, current_boss_index,
                   full_heals_remaining, revives_remaining,
                   total_time_seconds, total_damage_taken, total_damage_dealt, total_deaths,
                   starting_character_state, ng_plus_tier, started_at, completed_at
            FROM Boss_Gauntlet_Runs
            WHERE character_id = @characterId
              AND status = 'in_progress'
            ORDER BY started_at DESC
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
    /// Update run progress (boss index, resources, statistics)
    /// </summary>
    public void UpdateRun(GauntletRun run)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Boss_Gauntlet_Runs
            SET status = @status,
                current_boss_index = @bossIndex,
                full_heals_remaining = @healsRemaining,
                revives_remaining = @revivesRemaining,
                total_time_seconds = @totalTime,
                total_damage_taken = @damageTaken,
                total_damage_dealt = @damageDealt,
                total_deaths = @deaths,
                completed_at = @completedAt
            WHERE run_id = @runId";

        cmd.Parameters.AddWithValue("@runId", run.RunId);
        cmd.Parameters.AddWithValue("@status", run.Status.ToString().ToLowerInvariant());
        cmd.Parameters.AddWithValue("@bossIndex", run.CurrentBossIndex);
        cmd.Parameters.AddWithValue("@healsRemaining", run.FullHealsRemaining);
        cmd.Parameters.AddWithValue("@revivesRemaining", run.RevivesRemaining);
        cmd.Parameters.AddWithValue("@totalTime", (object?)run.TotalTimeSeconds ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@damageTaken", run.TotalDamageTaken);
        cmd.Parameters.AddWithValue("@damageDealt", run.TotalDamageDealt);
        cmd.Parameters.AddWithValue("@deaths", run.TotalDeaths);
        cmd.Parameters.AddWithValue("@completedAt", (object?)run.CompletedAt ?? DBNull.Value);

        cmd.ExecuteNonQuery();

        _log.Debug("Updated run {RunId}: Status={Status}, Boss={BossIndex}",
            run.RunId, run.Status, run.CurrentBossIndex);
    }

    /// <summary>
    /// Complete a gauntlet run (victory or defeat)
    /// </summary>
    public void CompleteRun(int runId, GauntletRunStatus status, int totalTimeSeconds)
    {
        var run = GetRunById(runId);
        if (run == null) return;

        run.Status = status;
        run.TotalTimeSeconds = totalTimeSeconds;
        run.CompletedAt = DateTime.UtcNow;

        UpdateRun(run);

        _log.Information("Completed run {RunId} with status {Status} in {Time}s",
            runId, status, totalTimeSeconds);
    }

    // ═══════════════════════════════════════════════════════════
    // BOSS ENCOUNTERS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Log a boss encounter within a gauntlet run
    /// </summary>
    public int LogEncounter(GauntletBossEncounter encounter)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Boss_Gauntlet_Boss_Encounters
            (run_id, boss_index, boss_id, result,
             completion_time_seconds, damage_taken, damage_dealt, deaths,
             heals_used, revive_used, ending_character_state, completed_at)
            VALUES
            (@runId, @bossIndex, @bossId, @result,
             @completionTime, @damageTaken, @damageDealt, @deaths,
             @healsUsed, @reviveUsed, @endingState, @completedAt)";

        cmd.Parameters.AddWithValue("@runId", encounter.RunId);
        cmd.Parameters.AddWithValue("@bossIndex", encounter.BossIndex);
        cmd.Parameters.AddWithValue("@bossId", encounter.BossId);
        cmd.Parameters.AddWithValue("@result", encounter.Result.ToString().ToLowerInvariant());
        cmd.Parameters.AddWithValue("@completionTime", (object?)encounter.CompletionTimeSeconds ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@damageTaken", encounter.DamageTaken);
        cmd.Parameters.AddWithValue("@damageDealt", encounter.DamageDealt);
        cmd.Parameters.AddWithValue("@deaths", encounter.Deaths);
        cmd.Parameters.AddWithValue("@healsUsed", encounter.HealsUsed);
        cmd.Parameters.AddWithValue("@reviveUsed", encounter.ReviveUsed ? 1 : 0);
        cmd.Parameters.AddWithValue("@endingState", (object?)encounter.EndingCharacterState ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@completedAt", encounter.CompletedAt);

        cmd.ExecuteNonQuery();

        var encounterId = (int)conn.LastInsertRowId;
        _log.Debug("Logged encounter {EncounterId} for run {RunId}: Boss={BossId}, Result={Result}",
            encounterId, encounter.RunId, encounter.BossId, encounter.Result);

        return encounterId;
    }

    /// <summary>
    /// Get all encounters for a gauntlet run
    /// </summary>
    public List<GauntletBossEncounter> GetEncountersForRun(int runId)
    {
        var encounters = new List<GauntletBossEncounter>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT encounter_id, run_id, boss_index, boss_id, result,
                   completion_time_seconds, damage_taken, damage_dealt, deaths,
                   heals_used, revive_used, ending_character_state, completed_at
            FROM Boss_Gauntlet_Boss_Encounters
            WHERE run_id = @runId
            ORDER BY boss_index";
        cmd.Parameters.AddWithValue("@runId", runId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            encounters.Add(MapEncounterFromReader(reader));
        }

        return encounters;
    }

    // ═══════════════════════════════════════════════════════════
    // LEADERBOARDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Add or update a leaderboard entry
    /// </summary>
    public void UpsertLeaderboardEntry(GauntletLeaderboardEntry entry)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // Check if entry already exists
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = @"
            SELECT entry_id FROM Boss_Gauntlet_Leaderboard
            WHERE sequence_id = @sequenceId
              AND category = @category
              AND character_id = @characterId";
        checkCmd.Parameters.AddWithValue("@sequenceId", entry.SequenceId);
        checkCmd.Parameters.AddWithValue("@category", entry.Category.ToString().ToLowerInvariant());
        checkCmd.Parameters.AddWithValue("@characterId", entry.CharacterId);

        var existingId = checkCmd.ExecuteScalar();

        if (existingId != null)
        {
            // Update existing entry if new time is better
            var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Boss_Gauntlet_Leaderboard
                SET run_id = @runId,
                    total_time_seconds = @totalTime,
                    total_deaths = @deaths,
                    heals_used = @healsUsed,
                    ng_plus_tier = @ngPlusTier,
                    achieved_at = @achievedAt
                WHERE entry_id = @entryId
                  AND total_time_seconds > @totalTime";  // Only update if new time is better

            updateCmd.Parameters.AddWithValue("@entryId", existingId);
            updateCmd.Parameters.AddWithValue("@runId", entry.RunId);
            updateCmd.Parameters.AddWithValue("@totalTime", entry.TotalTimeSeconds);
            updateCmd.Parameters.AddWithValue("@deaths", entry.TotalDeaths);
            updateCmd.Parameters.AddWithValue("@healsUsed", entry.HealsUsed);
            updateCmd.Parameters.AddWithValue("@ngPlusTier", entry.NgPlusTier);
            updateCmd.Parameters.AddWithValue("@achievedAt", entry.AchievedAt);

            updateCmd.ExecuteNonQuery();
        }
        else
        {
            // Insert new entry
            var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Boss_Gauntlet_Leaderboard
                (sequence_id, category, run_id, character_id, character_name,
                 total_time_seconds, total_deaths, heals_used, ng_plus_tier, achieved_at)
                VALUES
                (@sequenceId, @category, @runId, @characterId, @characterName,
                 @totalTime, @deaths, @healsUsed, @ngPlusTier, @achievedAt)";

            insertCmd.Parameters.AddWithValue("@sequenceId", entry.SequenceId);
            insertCmd.Parameters.AddWithValue("@category", entry.Category.ToString().ToLowerInvariant());
            insertCmd.Parameters.AddWithValue("@runId", entry.RunId);
            insertCmd.Parameters.AddWithValue("@characterId", entry.CharacterId);
            insertCmd.Parameters.AddWithValue("@characterName", entry.CharacterName);
            insertCmd.Parameters.AddWithValue("@totalTime", entry.TotalTimeSeconds);
            insertCmd.Parameters.AddWithValue("@deaths", entry.TotalDeaths);
            insertCmd.Parameters.AddWithValue("@healsUsed", entry.HealsUsed);
            insertCmd.Parameters.AddWithValue("@ngPlusTier", entry.NgPlusTier);
            insertCmd.Parameters.AddWithValue("@achievedAt", entry.AchievedAt);

            insertCmd.ExecuteNonQuery();
        }

        // Recalculate ranks for this category and sequence
        RecalculateRanks(entry.SequenceId, entry.Category);

        _log.Information("Updated leaderboard: Sequence={SequenceId}, Category={Category}, Character={CharacterId}",
            entry.SequenceId, entry.Category, entry.CharacterId);
    }

    /// <summary>
    /// Get leaderboard for a sequence and category
    /// </summary>
    public List<GauntletLeaderboardEntry> GetLeaderboard(string sequenceId, GauntletLeaderboardCategory category, int limit = 100)
    {
        var entries = new List<GauntletLeaderboardEntry>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT entry_id, sequence_id, category, run_id, character_id, character_name,
                   total_time_seconds, total_deaths, heals_used, ng_plus_tier, rank, achieved_at
            FROM Boss_Gauntlet_Leaderboard
            WHERE sequence_id = @sequenceId
              AND category = @category
            ORDER BY rank
            LIMIT @limit";
        cmd.Parameters.AddWithValue("@sequenceId", sequenceId);
        cmd.Parameters.AddWithValue("@category", category.ToString().ToLowerInvariant());
        cmd.Parameters.AddWithValue("@limit", limit);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            entries.Add(MapLeaderboardEntryFromReader(reader));
        }

        return entries;
    }

    // ═══════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════

    private HashSet<string> GetCompletedSequenceIds(int characterId)
    {
        var completedIds = new HashSet<string>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT DISTINCT sequence_id
            FROM Boss_Gauntlet_Runs
            WHERE character_id = @characterId
              AND status = 'victory'";
        cmd.Parameters.AddWithValue("@characterId", characterId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            completedIds.Add(reader.GetString(0));
        }

        return completedIds;
    }

    private void RecalculateRanks(string sequenceId, GauntletLeaderboardCategory category)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // Get all entries sorted by time
        var entries = GetLeaderboard(sequenceId, category, 1000);

        // Update ranks
        for (int i = 0; i < entries.Count; i++)
        {
            var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Boss_Gauntlet_Leaderboard
                SET rank = @rank
                WHERE entry_id = @entryId";
            updateCmd.Parameters.AddWithValue("@rank", i + 1);
            updateCmd.Parameters.AddWithValue("@entryId", entries[i].EntryId);
            updateCmd.ExecuteNonQuery();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // MAPPING METHODS
    // ═══════════════════════════════════════════════════════════

    private GauntletSequence MapSequenceFromReader(SqliteDataReader reader)
    {
        var bossIdsJson = reader.GetString(5);
        var prerequisiteJson = reader.IsDBNull(9) ? null : reader.GetString(9);

        return new GauntletSequence
        {
            SequenceId = reader.GetString(0),
            SequenceName = reader.GetString(1),
            Description = reader.GetString(2),
            DifficultyTier = reader.GetString(3),
            BossCount = reader.GetInt32(4),
            BossIds = JsonSerializer.Deserialize<List<string>>(bossIdsJson) ?? new List<string>(),
            MaxFullHeals = reader.GetInt32(6),
            MaxRevives = reader.GetInt32(7),
            RequiredNGPlusTier = reader.GetInt32(8),
            PrerequisiteRuns = prerequisiteJson != null
                ? JsonSerializer.Deserialize<List<string>>(prerequisiteJson) ?? new List<string>()
                : new List<string>(),
            CompletionRewardId = reader.IsDBNull(10) ? null : reader.GetString(10),
            TitleReward = reader.IsDBNull(11) ? null : reader.GetString(11),
            Active = reader.GetBoolean(12),
            CreatedAt = reader.GetDateTime(13)
        };
    }

    private GauntletRun MapRunFromReader(SqliteDataReader reader)
    {
        var statusStr = reader.GetString(3);
        var status = statusStr switch
        {
            "in_progress" => GauntletRunStatus.InProgress,
            "victory" => GauntletRunStatus.Victory,
            "defeat" => GauntletRunStatus.Defeat,
            _ => GauntletRunStatus.InProgress
        };

        return new GauntletRun
        {
            RunId = reader.GetInt32(0),
            CharacterId = reader.GetInt32(1),
            SequenceId = reader.GetString(2),
            Status = status,
            CurrentBossIndex = reader.GetInt32(4),
            FullHealsRemaining = reader.GetInt32(5),
            RevivesRemaining = reader.GetInt32(6),
            TotalTimeSeconds = reader.IsDBNull(7) ? null : reader.GetInt32(7),
            TotalDamageTaken = reader.GetInt32(8),
            TotalDamageDealt = reader.GetInt32(9),
            TotalDeaths = reader.GetInt32(10),
            StartingCharacterState = reader.GetString(11),
            NgPlusTier = reader.GetInt32(12),
            StartedAt = reader.GetDateTime(13),
            CompletedAt = reader.IsDBNull(14) ? null : reader.GetDateTime(14)
        };
    }

    private GauntletBossEncounter MapEncounterFromReader(SqliteDataReader reader)
    {
        var resultStr = reader.GetString(4);
        var result = resultStr == "victory" ? GauntletEncounterResult.Victory : GauntletEncounterResult.Defeat;

        return new GauntletBossEncounter
        {
            EncounterId = reader.GetInt32(0),
            RunId = reader.GetInt32(1),
            BossIndex = reader.GetInt32(2),
            BossId = reader.GetString(3),
            Result = result,
            CompletionTimeSeconds = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            DamageTaken = reader.GetInt32(6),
            DamageDealt = reader.GetInt32(7),
            Deaths = reader.GetInt32(8),
            HealsUsed = reader.GetInt32(9),
            ReviveUsed = reader.GetBoolean(10),
            EndingCharacterState = reader.IsDBNull(11) ? null : reader.GetString(11),
            CompletedAt = reader.GetDateTime(12)
        };
    }

    private GauntletLeaderboardEntry MapLeaderboardEntryFromReader(SqliteDataReader reader)
    {
        var categoryStr = reader.GetString(2);
        var category = categoryStr switch
        {
            "fastest" => GauntletLeaderboardCategory.Fastest,
            "flawless" => GauntletLeaderboardCategory.Flawless,
            "no_heal" => GauntletLeaderboardCategory.NoHeal,
            "ng_plus" => GauntletLeaderboardCategory.NGPlus,
            _ => GauntletLeaderboardCategory.Fastest
        };

        return new GauntletLeaderboardEntry
        {
            EntryId = reader.GetInt32(0),
            SequenceId = reader.GetString(1),
            Category = category,
            RunId = reader.GetInt32(3),
            CharacterId = reader.GetInt32(4),
            CharacterName = reader.GetString(5),
            TotalTimeSeconds = reader.GetInt32(6),
            TotalDeaths = reader.GetInt32(7),
            HealsUsed = reader.GetInt32(8),
            NgPlusTier = reader.GetInt32(9),
            Rank = reader.IsDBNull(10) ? null : reader.GetInt32(10),
            AchievedAt = reader.GetDateTime(11)
        };
    }
}
