using RuneAndRust.Core.EndlessMode;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine.EndlessMode;

/// <summary>
/// v0.40.4: Endless Mode Service
/// Main orchestration for endless mode runs, waves, scoring, and leaderboards
/// </summary>
public class EndlessModeService
{
    private static readonly ILogger _log = Log.ForContext<EndlessModeService>();
    private readonly EndlessModeRepository _repository;
    private readonly WaveScalingService _waveScaling;

    public EndlessModeService(EndlessModeRepository repository)
    {
        _repository = repository;
        _waveScaling = new WaveScalingService();

        _log.Information("EndlessModeService initialized");
    }

    // ═══════════════════════════════════════════════════════════
    // RUN MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Start a new endless mode run
    /// </summary>
    public EndlessRun StartEndlessRun(int characterId, string? seed = null,
        string? characterBuildHash = null)
    {
        // Check for active run
        var activeRun = _repository.GetActiveRun(characterId);
        if (activeRun != null)
        {
            throw new InvalidOperationException($"Character {characterId} already has an active endless mode run");
        }

        // Generate seed if not provided
        if (string.IsNullOrEmpty(seed))
        {
            seed = SeedGenerator.GenerateDailySeed();
        }

        // Validate seed
        if (!SeedGenerator.IsValidSeed(seed))
        {
            throw new ArgumentException($"Invalid seed format: {seed}", nameof(seed));
        }

        // Create run
        var run = new EndlessRun
        {
            CharacterId = characterId,
            Seed = seed,
            StartTime = DateTime.UtcNow,
            IsActive = true,
            CurrentWave = 1,
            HighestWaveReached = 1,
            CharacterBuildHash = characterBuildHash
        };

        run.RunId = _repository.CreateRun(run);

        // Create first wave
        CreateWaveForRun(run);

        _log.Information("Started endless run {RunId} for character {CharacterId} with seed {Seed}",
            run.RunId, characterId, seed);

        return run;
    }

    /// <summary>
    /// Get active run for a character
    /// </summary>
    public EndlessRun? GetActiveRun(int characterId)
    {
        return _repository.GetActiveRun(characterId);
    }

    /// <summary>
    /// Get a specific run by ID
    /// </summary>
    public EndlessRun? GetRunById(int runId)
    {
        return _repository.GetRunById(runId);
    }

    // ═══════════════════════════════════════════════════════════
    // WAVE MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Complete current wave and advance to next
    /// </summary>
    public void CompleteWave(int runId, int enemiesKilled, int damageTaken, int damageDealt)
    {
        var run = _repository.GetRunById(runId);
        if (run == null)
        {
            throw new InvalidOperationException($"Run {runId} not found");
        }

        if (!run.IsActive)
        {
            throw new InvalidOperationException($"Run {runId} is not active");
        }

        // Get current wave
        var waves = _repository.GetWavesForRun(runId);
        var currentWave = waves.FirstOrDefault(w => w.WaveNumber == run.CurrentWave);

        if (currentWave != null)
        {
            // Update wave completion data
            currentWave.EndTime = DateTime.UtcNow;
            currentWave.WaveTimeSeconds = (int)(currentWave.EndTime.Value - currentWave.StartTime).TotalSeconds;
            currentWave.EnemiesKilled = enemiesKilled;
            currentWave.DamageTaken = damageTaken;
            currentWave.DamageDealt = damageDealt;

            _repository.UpdateWave(currentWave);

            // Update run statistics
            run.TotalEnemiesKilled += enemiesKilled;
            run.TotalDamageTaken += damageTaken;
            run.TotalDamageDealt += damageDealt;

            if (currentWave.IsBossWave)
            {
                run.TotalBossesKilled++;
            }
        }

        // Advance to next wave
        run.CurrentWave++;
        run.HighestWaveReached = Math.Max(run.HighestWaveReached, run.CurrentWave);

        _repository.UpdateRun(run);

        // Create next wave
        CreateWaveForRun(run);

        _log.Information("Completed wave {Wave} for run {RunId}, advancing to wave {NextWave}",
            run.CurrentWave - 1, runId, run.CurrentWave);
    }

    /// <summary>
    /// Create a new wave for a run
    /// </summary>
    private void CreateWaveForRun(EndlessRun run)
    {
        var config = _waveScaling.GenerateWaveConfig(run.CurrentWave);

        var wave = new EndlessWave
        {
            RunId = run.RunId,
            WaveNumber = run.CurrentWave,
            EnemyCount = config.EnemyCount,
            EnemyLevel = config.EnemyLevel,
            DifficultyMultiplier = config.DifficultyMultiplier,
            IsBossWave = config.IsBossWave,
            StartTime = DateTime.UtcNow
        };

        wave.WaveId = _repository.CreateWave(wave);

        _log.Debug("Created wave {Wave} for run {RunId}: {Count} enemies, Level {Level}, {Diff}x difficulty",
            run.CurrentWave, run.RunId, config.EnemyCount, config.EnemyLevel, config.DifficultyMultiplier);
    }

    /// <summary>
    /// Get wave history for a run
    /// </summary>
    public List<EndlessWave> GetWaveHistory(int runId)
    {
        return _repository.GetWavesForRun(runId);
    }

    /// <summary>
    /// Get current wave configuration for a run
    /// </summary>
    public WaveConfiguration? GetCurrentWaveConfig(int runId)
    {
        var run = _repository.GetRunById(runId);
        if (run == null || !run.IsActive)
        {
            return null;
        }

        return _waveScaling.GenerateWaveConfig(run.CurrentWave);
    }

    // ═══════════════════════════════════════════════════════════
    // RUN COMPLETION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// End an endless mode run (death)
    /// </summary>
    public void EndRun(int runId)
    {
        var run = _repository.GetRunById(runId);
        if (run == null)
        {
            throw new InvalidOperationException($"Run {runId} not found");
        }

        if (!run.IsActive)
        {
            throw new InvalidOperationException($"Run {runId} is already ended");
        }

        // Finalize run
        run.EndTime = DateTime.UtcNow;
        run.IsActive = false;
        run.TotalTimeSeconds = (int)(run.EndTime.Value - run.StartTime).TotalSeconds;

        // Calculate final score
        var score = EndlessScore.Calculate(run);
        run.WaveScore = score.WaveScore;
        run.KillScore = score.KillScore;
        run.BossScore = score.BossScore;
        run.TimeBonus = score.TimeBonus;
        run.SurvivalBonus = score.SurvivalBonus;
        run.TotalScore = score.TotalScore;

        // Mark as verified (can add more sophisticated verification later)
        run.IsVerified = true;

        _repository.UpdateRun(run);

        // Submit to leaderboards
        SubmitToLeaderboards(run);

        _log.Information("Ended endless run {RunId}: Wave {Wave}, Score {Score}, Time {Time}s",
            runId, run.HighestWaveReached, run.TotalScore, run.TotalTimeSeconds);
    }

    /// <summary>
    /// Forfeit/abandon a run
    /// </summary>
    public void ForfeitRun(int runId)
    {
        var run = _repository.GetRunById(runId);
        if (run == null || !run.IsActive)
        {
            return;
        }

        run.EndTime = DateTime.UtcNow;
        run.IsActive = false;
        run.TotalTimeSeconds = (int)(run.EndTime.Value - run.StartTime).TotalSeconds;

        // Calculate score even for forfeit
        var score = EndlessScore.Calculate(run);
        run.WaveScore = score.WaveScore;
        run.KillScore = score.KillScore;
        run.BossScore = score.BossScore;
        run.TimeBonus = score.TimeBonus;
        run.SurvivalBonus = score.SurvivalBonus;
        run.TotalScore = score.TotalScore;

        _repository.UpdateRun(run);

        _log.Information("Forfeited endless run {RunId} at wave {Wave}",
            runId, run.CurrentWave);
    }

    // ═══════════════════════════════════════════════════════════
    // SCORING
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Calculate score for a run
    /// </summary>
    public EndlessScore CalculateScore(int runId)
    {
        var run = _repository.GetRunById(runId);
        if (run == null)
        {
            throw new InvalidOperationException($"Run {runId} not found");
        }

        return EndlessScore.Calculate(run);
    }

    /// <summary>
    /// Get score breakdown for a run
    /// </summary>
    public EndlessScore GetScoreBreakdown(int runId)
    {
        return CalculateScore(runId);
    }

    // ═══════════════════════════════════════════════════════════
    // LEADERBOARDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Submit run to leaderboards
    /// </summary>
    private void SubmitToLeaderboards(EndlessRun run)
    {
        var activeSeason = _repository.GetActiveSeason();
        var seasonId = activeSeason?.SeasonId;

        // Get character name (would come from character service in real implementation)
        var playerName = $"Character_{run.CharacterId}";

        // Submit to "Highest Wave" leaderboard
        var waveEntry = new EndlessLeaderboardEntry
        {
            RunId = run.RunId,
            CharacterId = run.CharacterId,
            PlayerName = playerName,
            Category = EndlessLeaderboardCategory.HighestWave,
            HighestWaveReached = run.HighestWaveReached,
            TotalScore = run.TotalScore,
            TotalTimeSeconds = run.TotalTimeSeconds,
            Seed = run.Seed,
            CharacterBuildHash = run.CharacterBuildHash,
            IsVerified = run.IsVerified,
            SeasonId = seasonId,
            SubmittedAt = DateTime.UtcNow
        };

        _repository.UpsertLeaderboardEntry(waveEntry);

        // Submit to "Highest Score" leaderboard
        var scoreEntry = new EndlessLeaderboardEntry
        {
            RunId = run.RunId,
            CharacterId = run.CharacterId,
            PlayerName = playerName,
            Category = EndlessLeaderboardCategory.HighestScore,
            HighestWaveReached = run.HighestWaveReached,
            TotalScore = run.TotalScore,
            TotalTimeSeconds = run.TotalTimeSeconds,
            Seed = run.Seed,
            CharacterBuildHash = run.CharacterBuildHash,
            IsVerified = run.IsVerified,
            SeasonId = seasonId,
            SubmittedAt = DateTime.UtcNow
        };

        _repository.UpsertLeaderboardEntry(scoreEntry);

        _log.Information("Submitted run {RunId} to leaderboards: Wave {Wave}, Score {Score}",
            run.RunId, run.HighestWaveReached, run.TotalScore);
    }

    /// <summary>
    /// Get leaderboard for a category
    /// </summary>
    public List<EndlessLeaderboardEntry> GetLeaderboard(EndlessLeaderboardCategory category,
        string? seasonId = null, int limit = 100)
    {
        return _repository.GetLeaderboard(category, seasonId, limit);
    }

    /// <summary>
    /// Get all leaderboards for current season
    /// </summary>
    public Dictionary<EndlessLeaderboardCategory, List<EndlessLeaderboardEntry>> GetAllLeaderboards(int limit = 100)
    {
        var activeSeason = _repository.GetActiveSeason();
        var seasonId = activeSeason?.SeasonId;

        var leaderboards = new Dictionary<EndlessLeaderboardCategory, List<EndlessLeaderboardEntry>>();

        foreach (EndlessLeaderboardCategory category in Enum.GetValues(typeof(EndlessLeaderboardCategory)))
        {
            leaderboards[category] = GetLeaderboard(category, seasonId, limit);
        }

        return leaderboards;
    }

    // ═══════════════════════════════════════════════════════════
    // SEASONS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get active season
    /// </summary>
    public EndlessSeason? GetActiveSeason()
    {
        return _repository.GetActiveSeason();
    }

    // ═══════════════════════════════════════════════════════════
    // STATISTICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get performance summary for a run
    /// </summary>
    public EndlessRunSummary GetRunSummary(int runId)
    {
        var run = _repository.GetRunById(runId);
        if (run == null)
        {
            throw new InvalidOperationException($"Run {runId} not found");
        }

        var waves = _repository.GetWavesForRun(runId);
        var score = EndlessScore.Calculate(run);

        return new EndlessRunSummary
        {
            Run = run,
            Score = score,
            TotalWaves = waves.Count,
            BossWavesCompleted = waves.Count(w => w.IsBossWave && w.IsComplete),
            AverageWaveTime = waves.Any(w => w.WaveTimeSeconds.HasValue)
                ? (float)waves.Where(w => w.WaveTimeSeconds.HasValue).Average(w => w.WaveTimeSeconds!.Value)
                : 0f,
            FastestWave = waves.Where(w => w.WaveTimeSeconds.HasValue).MinBy(w => w.WaveTimeSeconds!.Value),
            SlowestWave = waves.Where(w => w.WaveTimeSeconds.HasValue).MaxBy(w => w.WaveTimeSeconds!.Value)
        };
    }
}

/// <summary>
/// Summary of an endless mode run
/// </summary>
public class EndlessRunSummary
{
    /// <summary>The run</summary>
    public required EndlessRun Run { get; init; }

    /// <summary>Score breakdown</summary>
    public required EndlessScore Score { get; init; }

    /// <summary>Total waves attempted</summary>
    public int TotalWaves { get; init; }

    /// <summary>Boss waves completed</summary>
    public int BossWavesCompleted { get; init; }

    /// <summary>Average wave completion time (seconds)</summary>
    public float AverageWaveTime { get; init; }

    /// <summary>Fastest wave</summary>
    public EndlessWave? FastestWave { get; init; }

    /// <summary>Slowest wave</summary>
    public EndlessWave? SlowestWave { get; init; }

    /// <summary>Display summary</summary>
    public string DisplaySummary => $@"
=== Endless Run Summary ===
Wave Reached: {Run.HighestWaveReached}
Total Score: {Run.TotalScore:N0}

Combat:
  Enemies Killed: {Run.TotalEnemiesKilled:N0}
  Bosses Killed: {Run.TotalBossesKilled}
  Damage Dealt: {Run.TotalDamageDealt:N0}
  Damage Taken: {Run.TotalDamageTaken:N0}

Performance:
  Total Time: {Run.DurationMinutes:F1} minutes
  Average Wave Time: {AverageWaveTime:F1}s
  Boss Waves Completed: {BossWavesCompleted}

Score Breakdown:
{Score.BreakdownDisplay}
".Trim();
}
