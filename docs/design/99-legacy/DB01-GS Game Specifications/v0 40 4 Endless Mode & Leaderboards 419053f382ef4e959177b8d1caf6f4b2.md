# v0.40.4: Endless Mode & Leaderboards

Type: Feature
Description: Implement wave-based survival mode with competitive scoring and global leaderboards. Delivers wave generation with escalating difficulty, scoring system (kills, time, survival bonus), global leaderboards, seed verification, anti-cheat measures, and seasonal resets.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.40.1 (New Game+ System), v0.10-v0.12 (Dynamic Room Engine), v0.15 (Trauma Economy)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.40: Endgame Content & Replayability (v0%2040%20Endgame%20Content%20&%20Replayability%208ad34f4aa15d45478cce6aea9fda6624.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Design Phase

**Prerequisites:** v0.40.1 (New Game+ System), v0.10-v0.12 (Dynamic Room Engine), v0.15 (Trauma Economy)

**Timeline:** 7-11 hours (1-2 weeks part-time)

**Goal:** Implement wave-based survival mode with competitive scoring and global leaderboards

**Philosophy:** Infinite challenge with finite sessions—compete for highest wave and total score

---

## I. Executive Summary

v0.40.4 implements **Endless Mode**, a wave-based survival challenge with competitive scoring, global leaderboards, and seasonal resets. Players face infinitely escalating waves until death.

**What v0.40.4 Delivers:**

- Wave generation with escalating difficulty
- Scoring system (kills, time, survival bonus)
- Global leaderboards (highest wave, highest score)
- Seed verification and anti-cheat
- Seasonal leaderboard resets
- Run tracking and performance metrics

**Success Metric:**

Endless Mode provides infinite replayability through competitive leaderboards. Reaching Wave 30+ is a badge of honor. Top scores require optimization and perfect execution.

---

## II. Design Philosophy

### A. Infinite Challenge, Finite Sessions

**Principle:** Endless Mode has no victory condition—only how far you survive.

**Design Rationale:**

Unlike Boss Gauntlet (clear endpoint) or Challenge Sectors (fixed content), Endless Mode is **truly endless**. You play until death.

**Wave Escalation:**

```jsx
Wave 1: 3 enemies, Level 6, 1.0x multiplier
Wave 10: 8 enemies, Level 15, 2.0x multiplier
Wave 20: 13 enemies, Level 25, 3.0x multiplier
Wave 30: 18 enemies, Level 35, 4.0x multiplier
Wave 50: 28 enemies, Level 55, 6.0x multiplier
Wave 100: 53 enemies, Level 105, 11.0x multiplier

No Cap: Difficulty scales forever
```

**Why Infinite:**

- Always a goal: "Can I reach Wave 40?"
- Leaderboards never stagnate
- Build optimization rewarded
- Content creators have infinite challenge

**Session Length:**

- Early waves: 2-3 minutes each
- Mid waves (20-30): 5-8 minutes each
- Late waves (40+): 10-15 minutes each
- Total session: 2-4 hours to reach Wave 30+

### B. Score Over Wave Count

**Principle:** Highest wave reached isn't the only metric—total score matters.

**Scoring Formula:**

```csharp
Total Score = WaveScore + KillScore + BossScore + TimeBonus + SurvivalBonus

WaveScore = WavesCompleted × 1000
KillScore = EnemiesKilled × 50
BossScore = BossesKilled × 500
TimeBonus = Max(0, 10000 - TotalTimeSeconds)
SurvivalBonus = Max(0, 5000 - TotalDamageTaken)
```

**Example Calculation:**

```jsx
Run Metrics:
- Waves Completed: 32
- Enemies Killed: 487
- Bosses Killed: 8
- Total Time: 7,420 seconds (2h 4m)
- Damage Taken: 2,180

Score Breakdown:
WaveScore: 32 × 1,000 = 32,000
KillScore: 487 × 50 = 24,350
BossScore: 8 × 500 = 4,000
TimeBonus: 10,000 - 7,420 = 2,580
SurvivalBonus: 5,000 - 2,180 = 2,820

Total Score: 65,750
```

**Why Score Matters:**

- Rewards efficiency (TimeBonus)
- Rewards survival skill (SurvivalBonus)
- Rewards aggression (KillScore)
- Creates multiple optimization paths

**Two Leaderboards:**

1. **Highest Wave Reached** (raw survival)
2. **Highest Total Score** (optimization)

### C. Competitive Without Toxicity

**Principle:** Leaderboards celebrate achievement without fostering negativity.

**Anti-Toxicity Design:**

**1. Seed-Based Verification**

```csharp
public class EndlessModeRun
{
    public string Seed { get; set; }  // Known seed for verification
    public string CharacterBuildHash { get; set; }  // Build snapshot
    public bool IsVerified { get; set; }
    public int CommunityReportCount { get; set; }
}
```

- All runs use known seeds
- Community can replay seed to verify legitimacy
- Suspicious scores flagged for review
- Build hash prevents impossible stat combinations

**2. Multiple Leaderboards**

- No single "best" metric
- Highest wave ≠ highest score
- Multiple archetypes excel in different categories
- Prevents stratification

**3. Seasonal Resets**

- Leaderboards reset every 3 months
- Fresh start for all players
- Prevents permanent domination
- Historical records preserved separately

**4. Anonymous Option**

- Players can hide name on leaderboards
- Opt-in public profiles
- No forced social pressure

### D. Seed Reproducibility

**Principle:** All runs are reproducible for verification and speedrunning.

**Seed System:**

```csharp
public class EndlessSeed
{
    public string SeedValue { get; set; }  // e.g., "ENDLESS-2025-1147"
    public DateTime GeneratedAt { get; set; }
    public int WaveCount { get; set; }  // Pre-generated wave count
}

public string GenerateDailySeed()
{
    var date = DateTime.UtcNow.ToString("yyyyMMdd");
    return $"ENDLESS-{date}";
}
```

**Seed Benefits:**

- Daily seeds for competition
- Custom seeds for practice
- Speedrun verification
- Anti-cheat validation
- Community challenges

**Seeded Elements:**

- Enemy types per wave
- Hazard placement
- Loot drops
- Boss spawn waves

**Non-Seeded:**

- Character build
- Player decisions
- Execution quality

### E. Fair Resource Economics

**Principle:** Loot drops provide resources, but not infinite sustain.

**Loot Drop Rates:**

```jsx
Waves 1-10: 30% chance per enemy
Waves 11-20: 20% chance per enemy
Waves 21-30: 15% chance per enemy
Waves 31+: 10% chance per enemy

Boss Waves (every 5): Guaranteed loot
```

**Loot Tables:**

- HP consumables (small/medium)
- Stamina/Aether consumables
- Temporary buffs (1-3 waves)
- Crafting materials

**No Infinite Sustain:**

- Healing is limited by drops
- Cannot outlast difficulty curve indefinitely
- Forces efficient play
- Eventual death guaranteed

---

## III. Database Schema

```sql
-- =====================================================
-- NEW: ENDLESS MODE RUNS
-- =====================================================

CREATE TABLE Endless_Mode_Runs (
    run_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    seed TEXT NOT NULL,
    
    -- Run state
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    is_active BOOLEAN DEFAULT 1,
    highest_wave_reached INTEGER DEFAULT 1,
    current_wave INTEGER DEFAULT 1,
    
    -- Combat metrics
    total_enemies_killed INTEGER DEFAULT 0,
    total_bosses_killed INTEGER DEFAULT 0,
    total_damage_taken INTEGER DEFAULT 0,
    total_damage_dealt INTEGER DEFAULT 0,
    
    -- Performance
    total_time_seconds INTEGER DEFAULT 0,
    
    -- Scoring
    wave_score INTEGER DEFAULT 0,
    kill_score INTEGER DEFAULT 0,
    boss_score INTEGER DEFAULT 0,
    time_bonus INTEGER DEFAULT 0,
    survival_bonus INTEGER DEFAULT 0,
    total_score INTEGER DEFAULT 0,
    
    -- Verification
    character_build_hash TEXT,
    is_verified BOOLEAN DEFAULT 0,
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    
    CHECK (highest_wave_reached >= 1),
    CHECK (current_wave >= 1)
);

CREATE INDEX idx_endless_runs_character ON Endless_Mode_Runs(character_id);
CREATE INDEX idx_endless_runs_active ON Endless_Mode_Runs(is_active);
CREATE INDEX idx_endless_runs_seed ON Endless_Mode_Runs(seed);
CREATE INDEX idx_endless_runs_score ON Endless_Mode_Runs(total_score DESC);

-- =====================================================
-- NEW: ENDLESS MODE WAVES
-- =====================================================

CREATE TABLE Endless_Mode_Waves (
    wave_id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    wave_number INTEGER NOT NULL,
    
    -- Wave composition
    enemy_count INTEGER NOT NULL,
    enemy_level INTEGER NOT NULL,
    difficulty_multiplier REAL NOT NULL,
    is_boss_wave BOOLEAN DEFAULT 0,
    
    -- Wave performance
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    wave_time_seconds INTEGER,
    enemies_killed INTEGER DEFAULT 0,
    damage_taken INTEGER DEFAULT 0,
    
    FOREIGN KEY (run_id) REFERENCES Endless_Mode_Runs(run_id) ON DELETE CASCADE,
    
    CHECK (wave_number >= 1),
    CHECK (enemy_count >= 1)
);

CREATE INDEX idx_endless_waves_run ON Endless_Mode_Waves(run_id);
CREATE INDEX idx_endless_waves_number ON Endless_Mode_Waves(wave_number);

-- =====================================================
-- NEW: ENDLESS MODE LEADERBOARDS
-- =====================================================

CREATE TABLE Endless_Mode_Leaderboards (
    entry_id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    character_id INTEGER NOT NULL,
    player_name TEXT NOT NULL,
    
    -- Category
    category TEXT NOT NULL, -- 'highest_wave', 'highest_score'
    
    -- Performance
    highest_wave_reached INTEGER NOT NULL,
    total_score INTEGER NOT NULL,
    total_time_seconds INTEGER NOT NULL,
    
    -- Metadata
    character_level INTEGER,
    specialization_name TEXT,
    seed TEXT NOT NULL,
    character_build_hash TEXT,
    is_verified BOOLEAN DEFAULT 0,
    report_count INTEGER DEFAULT 0,
    
    -- Seasonal
    season_id TEXT,
    submitted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (run_id) REFERENCES Endless_Mode_Runs(run_id) ON DELETE CASCADE,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    
    CHECK (category IN ('highest_wave', 'highest_score')),
    CHECK (highest_wave_reached >= 1)
);

CREATE INDEX idx_endless_leaderboard_category ON Endless_Mode_Leaderboards(category);
CREATE INDEX idx_endless_leaderboard_wave ON Endless_Mode_Leaderboards(highest_wave_reached DESC);
CREATE INDEX idx_endless_leaderboard_score ON Endless_Mode_Leaderboards(total_score DESC);
CREATE INDEX idx_endless_leaderboard_season ON Endless_Mode_Leaderboards(season_id);

-- =====================================================
-- NEW: ENDLESS MODE SEASONS
-- =====================================================

CREATE TABLE Endless_Mode_Seasons (
    season_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    start_date DATETIME NOT NULL,
    end_date DATETIME NOT NULL,
    is_active BOOLEAN DEFAULT 0,
    
    CHECK (end_date > start_date)
);

-- Seed current season
INSERT INTO Endless_Mode_Seasons (season_id, name, start_date, end_date, is_active)
VALUES (
    'season_1_2025_q4',
    'Season 1: The First Gauntlet',
    '2025-10-01 00:00:00',
    '2025-12-31 23:59:59',
    1
);
```

---

## IV. Wave Generation System

### Wave Scaling Formulas

```csharp
public class WaveScalingService
{
    public WaveConfiguration GenerateWaveConfig(int waveNumber)
    {
        return new WaveConfiguration
        {
            WaveNumber = waveNumber,
            EnemyCount = CalculateEnemyCount(waveNumber),
            EnemyLevel = CalculateEnemyLevel(waveNumber),
            DifficultyMultiplier = CalculateDifficultyMultiplier(waveNumber),
            IsBossWave = IsBossWave(waveNumber)
        };
    }
    
    private int CalculateEnemyCount(int wave)
    {
        // Formula: 3 + (wave / 2)
        // Wave 1: 3 enemies
        // Wave 10: 8 enemies
        // Wave 20: 13 enemies
        // Wave 50: 28 enemies
        return 3 + (wave / 2);
    }
    
    private int CalculateEnemyLevel(int wave)
    {
        // Formula: 5 + wave
        // Wave 1: Level 6
        // Wave 10: Level 15
        // Wave 30: Level 35
        return 5 + wave;
    }
    
    private float CalculateDifficultyMultiplier(int wave)
    {
        // Formula: 1.0 + (wave * 0.1)
        // Wave 1: 1.0x
        // Wave 10: 2.0x
        // Wave 20: 3.0x
        // Wave 50: 6.0x
        return 1.0f + (wave * 0.1f);
    }
    
    private bool IsBossWave(int wave)
    {
        // Boss waves every 5 waves
        return wave % 5 == 0;
    }
}
```

### Wave Composition

```csharp
public class WaveCompositionService
{
    public List<Enemy> GenerateWaveEnemies(
        WaveConfiguration config, 
        string seed)
    {
        var rng = new Random(GetWaveSeed(seed, config.WaveNumber));
        var enemies = new List<Enemy>();
        
        if (config.IsBossWave)
        {
            // Boss wave: 1 boss + (enemyCount - 1) adds
            var boss = SelectBossForWave(config, rng);
            enemies.Add(boss);
            
            for (int i = 1; i < config.EnemyCount; i++)
            {
                var add = SelectStandardEnemy(config, rng);
                enemies.Add(add);
            }
        }
        else
        {
            // Standard wave: all regular enemies
            for (int i = 0; i < config.EnemyCount; i++)
            {
                var enemy = SelectStandardEnemy(config, rng);
                enemies.Add(enemy);
            }
        }
        
        // Apply difficulty scaling
        foreach (var enemy in enemies)
        {
            ApplyDifficultyScaling(enemy, config.DifficultyMultiplier);
            enemy.Level = config.EnemyLevel;
        }
        
        return enemies;
    }
}
```

---

## V. Service Implementation

### EndlessModeService

```csharp
public interface IEndlessModeService
{
    Task<Result<EndlessRun>> StartEndlessRunAsync(int characterId, string seed = null);
    Task<Result> CompleteWaveAsync(int runId);
    Task<Result> EndRunAsync(int runId);
    Task<EndlessRun> GetActiveRunAsync(int characterId);
    Task<LeaderboardData> GetLeaderboardAsync(string category, string seasonId = null);
}

public class EndlessModeService : IEndlessModeService
{
    private readonly ILogger<EndlessModeService> _logger;
    private readonly IEndlessModeRepository _endlessRepo;
    private readonly IWaveScalingService _waveScaling;
    private readonly IWaveCompositionService _waveComposition;
    private readonly IScoringService _scoringService;
    
    public async Task<Result<EndlessRun>> StartEndlessRunAsync(
        int characterId, 
        string seed = null)
    {
        using var operation = _logger.BeginTimedOperation(
            "Starting Endless Mode run for character {CharacterId}",
            characterId);
        
        // Generate seed if not provided
        if (string.IsNullOrEmpty(seed))
        {
            seed = GenerateDailySeed();
        }
        
        // Validation: No active run
        var existingRun = await GetActiveRunAsync(characterId);
        if (existingRun != null)
        {
            return Result.Failure<EndlessRun>("Character already has active Endless Mode run");
        }
        
        // Create build hash for verification
        var character = await _characterRepo.GetByIdAsync(characterId);
        var buildHash = GenerateBuildHash(character);
        
        // Create run
        var run = new EndlessRun
        {
            CharacterId = characterId,
            Seed = seed,
            StartTime = DateTime.UtcNow,
            IsActive = true,
            CurrentWave = 1,
            HighestWaveReached = 1,
            CharacterBuildHash = buildHash
        };
        
        await _endlessRepo.CreateRunAsync(run);
        
        _logger.Information(
            "Endless Mode run {RunId} started: Character {CharId}, Seed {Seed}",
            run.RunId, characterId, seed);
        
        // Generate first wave
        await GenerateWaveAsync(run);
        
        return Result.Success(run);
    }
    
    public async Task<Result> CompleteWaveAsync(int runId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Completing wave for Endless run {RunId}",
            runId);
        
        var run = await _endlessRepo.GetRunAsync(runId);
        if (run == null)
        {
            return Result.Failure("Endless run not found");
        }
        
        if (!run.IsActive)
        {
            return Result.Failure("Endless run is not active");
        }
        
        // Mark current wave as complete
        await CompleteCurrentWaveAsync(run);
        
        // Advance to next wave
        run.CurrentWave++;
        run.HighestWaveReached = Math.Max(run.HighestWaveReached, run.CurrentWave);
        
        await _endlessRepo.UpdateRunAsync(run);
        
        // Generate next wave
        await GenerateWaveAsync(run);
        
        _logger.Information(
            "Endless run {RunId} advanced to wave {Wave}",
            runId, run.CurrentWave);
        
        return Result.Success();
    }
    
    public async Task<Result> EndRunAsync(int runId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Ending Endless run {RunId}",
            runId);
        
        var run = await _endlessRepo.GetRunAsync(runId);
        if (run == null)
        {
            return Result.Failure("Endless run not found");
        }
        
        run.EndTime = DateTime.UtcNow;
        run.IsActive = false;
        run.TotalTimeSeconds = (int)(run.EndTime.Value - run.StartTime).TotalSeconds;
        
        // Calculate final score
        var score = await _scoringService.CalculateFinalScoreAsync(run);
        run.WaveScore = score.WaveScore;
        run.KillScore = score.KillScore;
        run.BossScore = score.BossScore;
        run.TimeBonus = score.TimeBonus;
        run.SurvivalBonus = score.SurvivalBonus;
        run.TotalScore = score.TotalScore;
        
        await _endlessRepo.UpdateRunAsync(run);
        
        // Submit to leaderboards
        await SubmitLeaderboardEntriesAsync(run);
        
        _logger.Information(
            "Endless run {RunId} ended: Wave {Wave}, Score {Score}",
            runId, run.HighestWaveReached, run.TotalScore);
        
        return Result.Success();
    }
    
    private async Task GenerateWaveAsync(EndlessRun run)
    {
        var config = _waveScaling.GenerateWaveConfig(run.CurrentWave);
        var enemies = _waveComposition.GenerateWaveEnemies(config, run.Seed);
        
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
        
        await _endlessRepo.CreateWaveAsync(wave);
        
        _logger.Information(
            "Wave {Wave} generated: {Count} enemies, Level {Level}, Difficulty {Diff}x, Boss={IsBoss}",
            run.CurrentWave, config.EnemyCount, config.EnemyLevel, 
            config.DifficultyMultiplier, config.IsBossWave);
    }
}
```

### ScoringService

```csharp
public interface IScoringService
{
    Task<EndlessScore> CalculateFinalScoreAsync(EndlessRun run);
}

public class ScoringService : IScoringService
{
    public async Task<EndlessScore> CalculateFinalScoreAsync(EndlessRun run)
    {
        var score = new EndlessScore
        {
            // Base scores
            WaveScore = run.HighestWaveReached * 1000,
            KillScore = run.TotalEnemiesKilled * 50,
            BossScore = run.TotalBossesKilled * 500,
            
            // Bonus scores
            TimeBonus = Math.Max(0, 10000 - run.TotalTimeSeconds),
            SurvivalBonus = Math.Max(0, 5000 - run.TotalDamageTaken)
        };
        
        score.TotalScore = score.WaveScore + score.KillScore + score.BossScore + 
                           score.TimeBonus + score.SurvivalBonus;
        
        _logger.Information(
            "Final score calculated for run {RunId}: Wave={Wave}, Kill={Kill}, Boss={Boss}, Time={Time}, Survival={Survival}, Total={Total}",
            run.RunId, score.WaveScore, score.KillScore, score.BossScore,
            score.TimeBonus, score.SurvivalBonus, score.TotalScore);
        
        return score;
    }
}
```

---

## VI. Success Criteria

**v0.40.4 is DONE when:**

- [ ]  **Wave System:**
    - [ ]  Wave generation functional
    - [ ]  Enemy count scales correctly
    - [ ]  Difficulty multiplier applies
    - [ ]  Boss waves every 5 waves
    - [ ]  Infinite scaling works
- [ ]  **Scoring System:**
    - [ ]  All 5 score components calculate
    - [ ]  Total score sums correctly
    - [ ]  Score updates on run end
- [ ]  **Run Management:**
    - [ ]  Start run creates record
    - [ ]  Complete wave advances counter
    - [ ]  End run finalizes score
    - [ ]  Only one active run per character
- [ ]  **Seed System:**
    - [ ]  Daily seed generation
    - [ ]  Custom seed support
    - [ ]  Seed reproducibility validated
    - [ ]  Build hash verification
- [ ]  **Leaderboards:**
    - [ ]  Highest wave category
    - [ ]  Highest score category
    - [ ]  Top 100 displayed
    - [ ]  Seasonal filtering
- [ ]  **Database Schema:**
    - [ ]  Endless_Mode_Runs table
    - [ ]  Endless_Mode_Waves table
    - [ ]  Endless_Mode_Leaderboards table
    - [ ]  Endless_Mode_Seasons table
- [ ]  **Anti-Cheat:**
    - [ ]  Build hash validation
    - [ ]  Seed verification
    - [ ]  Report system functional
- [ ]  **Testing:**
    - [ ]  85%+ unit test coverage
    - [ ]  15+ unit tests passing
    - [ ]  3+ integration tests passing
- [ ]  **Logging:**
    - [ ]  Serilog structured logging
    - [ ]  Wave generation logged
    - [ ]  Score calculation logged
    - [ ]  Leaderboard submission logged

---

## VII. Timeline

**Week 1: Wave System** — 3-4 hours

- Database schema
- Wave generation
- Scaling formulas
- Enemy composition

**Week 2: Scoring & Runs** — 2-3 hours

- Scoring service
- Run management
- Wave completion
- End run logic

**Week 3: Leaderboards** — 2-3 hours

- Leaderboard submission
- Category management
- Seasonal system
- Verification

**Week 4: Testing** — 2-3 hours

- Unit tests (15+)
- Integration tests
- Anti-cheat validation

**Total: 7-11 hours (1-2 weeks part-time)**

---

**Ready for infinite challenge.**