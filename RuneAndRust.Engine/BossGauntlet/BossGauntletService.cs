using RuneAndRust.Core.BossGauntlet;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine.BossGauntlet;

/// <summary>
/// v0.40.3: Boss Gauntlet Service
/// Main orchestration for gauntlet runs, resource management, and leaderboards
/// </summary>
public class BossGauntletService
{
    private static readonly ILogger _log = Log.ForContext<BossGauntletService>();
    private readonly BossGauntletRepository _repository;

    public BossGauntletService(BossGauntletRepository repository)
    {
        _repository = repository;
        _log.Information("BossGauntletService initialized");
    }

    // ═══════════════════════════════════════════════════════════
    // GAUNTLET SEQUENCES
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get all available gauntlet sequences
    /// </summary>
    public List<GauntletSequence> GetAllSequences()
    {
        return _repository.GetAllSequences();
    }

    /// <summary>
    /// Get a specific gauntlet sequence by ID
    /// </summary>
    public GauntletSequence? GetSequenceById(string sequenceId)
    {
        return _repository.GetSequenceById(sequenceId);
    }

    /// <summary>
    /// Get gauntlet sequences available for a character
    /// </summary>
    public List<GauntletSequence> GetAvailableSequences(int characterId, int ngPlusTier)
    {
        return _repository.GetAvailableSequences(characterId, ngPlusTier);
    }

    // ═══════════════════════════════════════════════════════════
    // GAUNTLET RUN MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Start a new gauntlet run
    /// </summary>
    public GauntletRun StartGauntlet(int characterId, string sequenceId, int ngPlusTier, object characterState)
    {
        // Check if character already has an active run
        var activeRun = _repository.GetActiveRun(characterId);
        if (activeRun != null)
        {
            throw new InvalidOperationException($"Character {characterId} already has an active gauntlet run");
        }

        // Verify sequence exists and is available
        var sequence = _repository.GetSequenceById(sequenceId);
        if (sequence == null)
        {
            throw new InvalidOperationException($"Gauntlet sequence {sequenceId} not found");
        }

        if (!sequence.Active)
        {
            throw new InvalidOperationException($"Gauntlet sequence {sequenceId} is not currently active");
        }

        // Serialize character state
        var stateJson = JsonSerializer.Serialize(characterState);

        // Start the run
        var runId = _repository.StartRun(characterId, sequenceId, ngPlusTier, stateJson);

        _log.Information("Started gauntlet {SequenceId} for character {CharacterId} (NG+{Tier})",
            sequenceId, characterId, ngPlusTier);

        return _repository.GetRunById(runId)!;
    }

    /// <summary>
    /// Get active run for a character
    /// </summary>
    public GauntletRun? GetActiveRun(int characterId)
    {
        return _repository.GetActiveRun(characterId);
    }

    /// <summary>
    /// Get a specific run by ID
    /// </summary>
    public GauntletRun? GetRunById(int runId)
    {
        return _repository.GetRunById(runId);
    }

    // ═══════════════════════════════════════════════════════════
    // BOSS ENCOUNTER MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get current boss for an active run
    /// </summary>
    public string? GetCurrentBossId(GauntletRun run)
    {
        var sequence = _repository.GetSequenceById(run.SequenceId);
        if (sequence == null || run.CurrentBossIndex >= sequence.BossIds.Count)
        {
            return null;
        }

        return sequence.BossIds[run.CurrentBossIndex];
    }

    /// <summary>
    /// Complete a boss encounter (victory)
    /// </summary>
    public void CompleteBossEncounter(int runId, string bossId,
        int completionTimeSeconds, int damageTaken, int damageDealt,
        int deaths, int healsUsed, object? characterState = null)
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

        // Create encounter record
        var encounter = new GauntletBossEncounter
        {
            RunId = runId,
            BossIndex = run.CurrentBossIndex,
            BossId = bossId,
            Result = GauntletEncounterResult.Victory,
            CompletionTimeSeconds = completionTimeSeconds,
            DamageTaken = damageTaken,
            DamageDealt = damageDealt,
            Deaths = deaths,
            HealsUsed = healsUsed,
            ReviveUsed = deaths > 0,  // If deaths occurred, auto-revive was used
            EndingCharacterState = characterState != null ? JsonSerializer.Serialize(characterState) : null,
            CompletedAt = DateTime.UtcNow
        };

        _repository.LogEncounter(encounter);

        // Update run statistics
        run.TotalDamageTaken += damageTaken;
        run.TotalDamageDealt += damageDealt;
        run.TotalDeaths += deaths;
        run.FullHealsRemaining -= healsUsed;

        // Use revive if there were deaths
        if (deaths > 0 && run.RevivesRemaining > 0)
        {
            run.RevivesRemaining--;
            _log.Information("Auto-revive used for run {RunId}, {Revives} remaining",
                runId, run.RevivesRemaining);
        }

        // Move to next boss
        run.CurrentBossIndex++;

        var sequence = _repository.GetSequenceById(run.SequenceId);
        if (sequence != null && run.CurrentBossIndex >= sequence.BossCount)
        {
            // Gauntlet completed!
            CompleteGauntlet(run, GauntletRunStatus.Victory);
        }
        else
        {
            _repository.UpdateRun(run);
        }

        _log.Information("Boss encounter completed: Run={RunId}, Boss={BossId}, Deaths={Deaths}",
            runId, bossId, deaths);
    }

    /// <summary>
    /// Handle player death in gauntlet
    /// </summary>
    public GauntletDeathResult HandleDeath(int runId)
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

        // Check if revives are available
        if (run.RevivesRemaining > 0)
        {
            _log.Information("Death in gauntlet run {RunId}, auto-revive available ({Revives} remaining)",
                runId, run.RevivesRemaining);

            return new GauntletDeathResult
            {
                AutoRevive = true,
                RevivesRemaining = run.RevivesRemaining,
                GauntletEnded = false
            };
        }
        else
        {
            // No revives left - gauntlet ends in defeat
            _log.Information("Death in gauntlet run {RunId}, no revives remaining - DEFEAT",
                runId);

            CompleteGauntlet(run, GauntletRunStatus.Defeat);

            return new GauntletDeathResult
            {
                AutoRevive = false,
                RevivesRemaining = 0,
                GauntletEnded = true
            };
        }
    }

    /// <summary>
    /// Use a full heal during gauntlet
    /// </summary>
    public bool UseFullHeal(int runId)
    {
        var run = _repository.GetRunById(runId);
        if (run == null || !run.IsActive)
        {
            return false;
        }

        if (run.FullHealsRemaining <= 0)
        {
            _log.Warning("Attempted to use heal in run {RunId} but none remaining", runId);
            return false;
        }

        run.FullHealsRemaining--;
        _repository.UpdateRun(run);

        _log.Information("Full heal used in run {RunId}, {Heals} remaining",
            runId, run.FullHealsRemaining);

        return true;
    }

    /// <summary>
    /// Abandon/forfeit a gauntlet run
    /// </summary>
    public void ForfeitGauntlet(int runId)
    {
        var run = _repository.GetRunById(runId);
        if (run == null || !run.IsActive)
        {
            return;
        }

        CompleteGauntlet(run, GauntletRunStatus.Defeat);

        _log.Information("Gauntlet run {RunId} forfeited by player", runId);
    }

    // ═══════════════════════════════════════════════════════════
    // COMPLETION & REWARDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Complete a gauntlet run (victory or defeat)
    /// </summary>
    private void CompleteGauntlet(GauntletRun run, GauntletRunStatus status)
    {
        var totalTime = (int)(DateTime.UtcNow - run.StartedAt).TotalSeconds;

        run.Status = status;
        run.TotalTimeSeconds = totalTime;
        run.CompletedAt = DateTime.UtcNow;

        _repository.UpdateRun(run);

        if (status == GauntletRunStatus.Victory)
        {
            // Update leaderboards
            UpdateLeaderboards(run);

            _log.Information("Gauntlet VICTORY: Run={RunId}, Time={Time}s, Deaths={Deaths}",
                run.RunId, totalTime, run.TotalDeaths);
        }
        else
        {
            _log.Information("Gauntlet DEFEAT: Run={RunId}, BossReached={BossIndex}",
                run.RunId, run.CurrentBossIndex + 1);
        }
    }

    /// <summary>
    /// Update leaderboards after a victorious run
    /// </summary>
    private void UpdateLeaderboards(GauntletRun run)
    {
        // Get character name (would come from character service in real implementation)
        var characterName = $"Character_{run.CharacterId}";

        var sequence = _repository.GetSequenceById(run.SequenceId);
        if (sequence == null) return;

        // Calculate heals used
        var healsUsed = sequence.MaxFullHeals - run.FullHealsRemaining;

        // 1. Fastest category - all victories
        var fastestEntry = new GauntletLeaderboardEntry
        {
            SequenceId = run.SequenceId,
            Category = GauntletLeaderboardCategory.Fastest,
            RunId = run.RunId,
            CharacterId = run.CharacterId,
            CharacterName = characterName,
            TotalTimeSeconds = run.TotalTimeSeconds ?? 0,
            TotalDeaths = run.TotalDeaths,
            HealsUsed = healsUsed,
            NgPlusTier = run.NgPlusTier,
            AchievedAt = DateTime.UtcNow
        };
        _repository.UpsertLeaderboardEntry(fastestEntry);

        // 2. Flawless category - no deaths
        if (run.IsFlawless)
        {
            var flawlessEntry = new GauntletLeaderboardEntry
            {
                SequenceId = run.SequenceId,
                Category = GauntletLeaderboardCategory.Flawless,
                RunId = run.RunId,
                CharacterId = run.CharacterId,
                CharacterName = characterName,
                TotalTimeSeconds = run.TotalTimeSeconds ?? 0,
                TotalDeaths = 0,
                HealsUsed = healsUsed,
                NgPlusTier = run.NgPlusTier,
                AchievedAt = DateTime.UtcNow
            };
            _repository.UpsertLeaderboardEntry(flawlessEntry);
        }

        // 3. No Heal category - didn't use heals
        if (run.IsNoHeal)
        {
            var noHealEntry = new GauntletLeaderboardEntry
            {
                SequenceId = run.SequenceId,
                Category = GauntletLeaderboardCategory.NoHeal,
                RunId = run.RunId,
                CharacterId = run.CharacterId,
                CharacterName = characterName,
                TotalTimeSeconds = run.TotalTimeSeconds ?? 0,
                TotalDeaths = run.TotalDeaths,
                HealsUsed = 0,
                NgPlusTier = run.NgPlusTier,
                AchievedAt = DateTime.UtcNow
            };
            _repository.UpsertLeaderboardEntry(noHealEntry);
        }

        // 4. NG+ category - NG+ tier 1 or higher
        if (run.NgPlusTier > 0)
        {
            var ngPlusEntry = new GauntletLeaderboardEntry
            {
                SequenceId = run.SequenceId,
                Category = GauntletLeaderboardCategory.NGPlus,
                RunId = run.RunId,
                CharacterId = run.CharacterId,
                CharacterName = characterName,
                TotalTimeSeconds = run.TotalTimeSeconds ?? 0,
                TotalDeaths = run.TotalDeaths,
                HealsUsed = healsUsed,
                NgPlusTier = run.NgPlusTier,
                AchievedAt = DateTime.UtcNow
            };
            _repository.UpsertLeaderboardEntry(ngPlusEntry);
        }

        _log.Information("Updated leaderboards for run {RunId}", run.RunId);
    }

    // ═══════════════════════════════════════════════════════════
    // LEADERBOARDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get leaderboard for a sequence and category
    /// </summary>
    public List<GauntletLeaderboardEntry> GetLeaderboard(string sequenceId,
        GauntletLeaderboardCategory category, int limit = 100)
    {
        return _repository.GetLeaderboard(sequenceId, category, limit);
    }

    /// <summary>
    /// Get all leaderboards for a sequence
    /// </summary>
    public Dictionary<GauntletLeaderboardCategory, List<GauntletLeaderboardEntry>> GetAllLeaderboards(string sequenceId, int limit = 100)
    {
        var leaderboards = new Dictionary<GauntletLeaderboardCategory, List<GauntletLeaderboardEntry>>();

        foreach (GauntletLeaderboardCategory category in Enum.GetValues(typeof(GauntletLeaderboardCategory)))
        {
            leaderboards[category] = GetLeaderboard(sequenceId, category, limit);
        }

        return leaderboards;
    }

    // ═══════════════════════════════════════════════════════════
    // STATISTICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get encounter history for a run
    /// </summary>
    public List<GauntletBossEncounter> GetRunEncounters(int runId)
    {
        return _repository.GetEncountersForRun(runId);
    }

    /// <summary>
    /// Get resource status for a run
    /// </summary>
    public GauntletResourceStatus GetResourceStatus(int runId)
    {
        var run = _repository.GetRunById(runId);
        if (run == null)
        {
            throw new InvalidOperationException($"Run {runId} not found");
        }

        var sequence = _repository.GetSequenceById(run.SequenceId);

        return new GauntletResourceStatus
        {
            FullHealsRemaining = run.FullHealsRemaining,
            RevivesRemaining = run.RevivesRemaining,
            MaxFullHeals = sequence?.MaxFullHeals ?? 3,
            MaxRevives = sequence?.MaxRevives ?? 1,
            CurrentBossIndex = run.CurrentBossIndex,
            TotalBosses = sequence?.BossCount ?? 0
        };
    }
}

/// <summary>
/// Result of handling a player death
/// </summary>
public class GauntletDeathResult
{
    /// <summary>Was auto-revive triggered?</summary>
    public bool AutoRevive { get; set; }

    /// <summary>Revives remaining after this death</summary>
    public int RevivesRemaining { get; set; }

    /// <summary>Did the gauntlet end?</summary>
    public bool GauntletEnded { get; set; }
}

/// <summary>
/// Resource status for a gauntlet run
/// </summary>
public class GauntletResourceStatus
{
    /// <summary>Full heals remaining</summary>
    public int FullHealsRemaining { get; set; }

    /// <summary>Revives remaining</summary>
    public int RevivesRemaining { get; set; }

    /// <summary>Maximum full heals</summary>
    public int MaxFullHeals { get; set; }

    /// <summary>Maximum revives</summary>
    public int MaxRevives { get; set; }

    /// <summary>Current boss index</summary>
    public int CurrentBossIndex { get; set; }

    /// <summary>Total bosses</summary>
    public int TotalBosses { get; set; }

    /// <summary>Progress percentage</summary>
    public float ProgressPercentage => TotalBosses > 0
        ? (float)CurrentBossIndex / TotalBosses * 100
        : 0;

    /// <summary>Heals used so far</summary>
    public int HealsUsed => MaxFullHeals - FullHealsRemaining;

    /// <summary>Revives used so far</summary>
    public int RevivesUsed => MaxRevives - RevivesRemaining;
}
