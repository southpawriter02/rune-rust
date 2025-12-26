# v0.40.3: Boss Gauntlet Mode

Type: Feature
Description: Implement sequential boss encounter mode with resource limitations. Delivers 8-10 boss sequential encounter system, resource limitation mechanics (3 full heals, 1 revive), run tracking, gauntlet-specific leaderboards, and integration with v0.23 boss encounters.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.40.1 (New Game+ System), v0.23 (Boss Encounters), v0.15 (Trauma Economy)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.40: Endgame Content & Replayability (v0%2040%20Endgame%20Content%20&%20Replayability%208ad34f4aa15d45478cce6aea9fda6624.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Design Phase

**Prerequisites:** v0.40.1 (New Game+ System), v0.23 (Boss Encounters), v0.15 (Trauma Economy)

**Timeline:** 7-10 hours (1-2 weeks part-time)

**Goal:** Implement sequential boss encounter mode with resource limitations

**Philosophy:** Pure skill test with finite resources—no grinding, no overlevel cheese

---

## I. Executive Summary

v0.40.3 implements **Boss Gauntlet Mode**, a skill-testing endgame challenge where players face 8-10 sequential boss encounters with limited healing resources and no rest between fights.

**What v0.40.3 Delivers:**

- 8-10 boss sequential encounter system
- Resource limitation mechanics (3 full heals, 1 revive)
- Run tracking and performance metrics
- Gauntlet-specific leaderboards
- Victory rewards (titles, legendaries)
- Integration with v0.23 boss encounter system

**Success Metric:**

Completing the Boss Gauntlet is a true test of mastery—only optimized builds with perfect execution succeed. Fastest clear times become competitive benchmarks.

---

## II. Design Philosophy

### A. Skill Over Grinding

**Principle:** Victory requires mastery, not overlevel cheese.

**Design Rationale:**

Many boss rush modes can be trivialized by overleveling or overgearing. Boss Gauntlet prevents this through:

**Level Normalization:**

- All bosses scale to character level
- No benefit from being 10+ levels over content
- Gear and build optimization matter, not raw stats

**Limited Resources:**

- 3 full heals total across all 8-10 bosses
- 1 revive if character dies
- Stamina/Aether refill between bosses (sustain resources)
- HP does NOT refill between bosses

**Why This Works:**

- Cannot brute force through mistakes
- Must learn boss patterns and mechanics
- Resource management critical (when to heal?)
- Encourages perfect execution

### B. Escalating Difficulty Curve

**Principle:** Each boss is harder than the last, building to climactic final encounter.

**8-Boss Sequence Design:**

```jsx
Boss 1-2: Warm-up (Tier 2 bosses)
  - Familiar mechanics
  - Forgiving patterns
  - Establish baseline difficulty

Boss 3-4: Skill Check (Tier 3 bosses)
  - Complex mechanics
  - Requires positioning
  - Punishes mistakes

Boss 5-6: Endurance Test (Tier 3+ bosses)
  - Multi-phase encounters
  - Resource pressure builds
  - Mental fatigue factor

Boss 7: Crisis Point (Tier 4 boss)
  - Near-impossible without optimization
  - Tests everything learned
  - Most use healing here

Boss 8: Final Trial (Unique endgame boss)
  - New mechanics not seen before
  - Requires adaptation
  - Victory = mastery proven
```

**Difficulty Scaling:**

- Boss 1: 100% baseline stats
- Boss 2: 110% stats
- Boss 3: 120% stats
- Boss 4: 130% stats
- Boss 5: 140% stats
- Boss 6: 150% stats
- Boss 7: 170% stats
- Boss 8: 200% stats (final boss)

### C. No Rest, No Respite

**Principle:** Gauntlet is a single continuous challenge—no breaks for preparation.

**Between-Boss Rules:**

```jsx
After Each Boss Victory:

✅ RESTORED:
- Stamina refilled to max
- Aether refilled to max
- Status effects cleared
- Action Points reset

❌ NOT RESTORED:
- HP remains at current value
- Healing charges not refunded
- Revive not refunded
- No equipment changes
- No ability respec

⏱️ BREAK TIME:
- 5-minute real-time pause allowed
- Player can rest, review strategy
- Next boss triggers when ready
```

**Why This Matters:**

- HP deficit accumulates across fights
- Forces healing resource decisions
- Cannot cheese with mid-gauntlet optimization
- True endurance test

### D. Death = Restart

**Principle:** Boss Gauntlet has no mid-run saves—death means starting over.

**Failure Conditions:**

```jsx
Run Ends If:
1. Character HP reaches 0 (death)
2. Player forfeits run
3. Disconnect/crash (run invalidated)

On Failure:
- No progress saved
- No partial rewards
- Must restart from Boss 1

Revive Mechanic:
- 1 automatic revive per run
- Restores to 50% HP
- Triggers on first death
- Second death = run over
```

**Why Permadeath:**

- Raises stakes significantly
- Makes victories more meaningful
- Encourages careful play
- Prevents "grinding through" with deaths

### E. Competitive Speedrunning

**Principle:** Fastest clear times create competitive replayability.

**Leaderboard Categories:**

1. **Fastest Clear Time** (any heals/revive used)
2. **No Heal Clear** (0 healing charges used)
3. **Deathless Clear** (revive not used)
4. **Perfect Clear** (no heals, no revive, no damage taken)

**Time Tracking:**

- Start: Boss 1 encounter begins
- Pause: Between-boss breaks NOT counted
- End: Boss 8 defeated
- Result: Total combat time only

**Why Leaderboards:**

- Provides infinite replayability
- Encourages build optimization
- Creates content creator opportunities
- Celebrates mastery publicly

---

## III. Database Schema

### Table Creation

```sql
-- =====================================================
-- NEW: BOSS GAUNTLET RUNS
-- =====================================================

CREATE TABLE Boss_Gauntlet_Runs (
    run_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    gauntlet_sequence_id TEXT NOT NULL,
    
    -- Run state
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    current_boss_index INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT 1,
    is_completed BOOLEAN DEFAULT 0,
    
    -- Resource tracking
    remaining_heals INTEGER DEFAULT 3,
    remaining_revives INTEGER DEFAULT 1,
    heals_used INTEGER DEFAULT 0,
    revive_used BOOLEAN DEFAULT 0,
    
    -- Performance metrics
    total_combat_time_seconds INTEGER DEFAULT 0,
    total_damage_taken INTEGER DEFAULT 0,
    total_damage_dealt INTEGER DEFAULT 0,
    bosses_defeated INTEGER DEFAULT 0,
    
    -- Victory conditions
    is_deathless BOOLEAN DEFAULT 1,
    is_no_heal BOOLEAN DEFAULT 1,
    is_perfect BOOLEAN DEFAULT 1,
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    FOREIGN KEY (gauntlet_sequence_id) REFERENCES Boss_Gauntlet_Sequences(sequence_id),
    
    CHECK (remaining_heals >= 0 AND remaining_heals <= 3),
    CHECK (remaining_revives >= 0 AND remaining_revives <= 1),
    CHECK (current_boss_index >= 0 AND current_boss_index <= 10)
);

CREATE INDEX idx_gauntlet_runs_character ON Boss_Gauntlet_Runs(character_id);
CREATE INDEX idx_gauntlet_runs_active ON Boss_Gauntlet_Runs(is_active);
CREATE INDEX idx_gauntlet_runs_completed ON Boss_Gauntlet_Runs(is_completed);

-- =====================================================
-- NEW: BOSS GAUNTLET SEQUENCES
-- =====================================================

CREATE TABLE Boss_Gauntlet_Sequences (
    sequence_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    difficulty_tier INTEGER DEFAULT 1,
    boss_order TEXT NOT NULL, -- JSON array of boss IDs
    is_active BOOLEAN DEFAULT 1,
    
    CHECK (difficulty_tier >= 1 AND difficulty_tier <= 5)
);

-- Seed default gauntlet sequence
INSERT INTO Boss_Gauntlet_Sequences (sequence_id, name, description, difficulty_tier, boss_order)
VALUES (
    'trial_of_the_forsaken',
    'Trial of the Forsaken',
    'Eight sequential boss encounters testing all aspects of combat mastery.',
    3,
    '["blighted_champion", "forgemaster_thrain", "frost_warden_ymir", "reality_torn_anomaly", "haugbui_automaton", "twin_draugr_patterns", "surturs_herald", "the_silenced_one"]'
);

-- =====================================================
-- NEW: BOSS GAUNTLET BOSS ENCOUNTERS
-- =====================================================

CREATE TABLE Boss_Gauntlet_Boss_Encounters (
    encounter_id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    boss_id TEXT NOT NULL,
    boss_index INTEGER NOT NULL,
    
    -- Encounter state
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    is_completed BOOLEAN DEFAULT 0,
    
    -- Performance
    combat_time_seconds INTEGER DEFAULT 0,
    damage_taken INTEGER DEFAULT 0,
    damage_dealt INTEGER DEFAULT 0,
    deaths INTEGER DEFAULT 0,
    heals_used_in_fight INTEGER DEFAULT 0,
    
    FOREIGN KEY (run_id) REFERENCES Boss_Gauntlet_Runs(run_id) ON DELETE CASCADE,
    FOREIGN KEY (boss_id) REFERENCES Bosses(boss_id)
);

CREATE INDEX idx_gauntlet_encounters_run ON Boss_Gauntlet_Boss_Encounters(run_id);
CREATE INDEX idx_gauntlet_encounters_boss ON Boss_Gauntlet_Boss_Encounters(boss_id);

-- =====================================================
-- NEW: BOSS GAUNTLET LEADERBOARD
-- =====================================================

CREATE TABLE Boss_Gauntlet_Leaderboard (
    entry_id INTEGER PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    character_id INTEGER NOT NULL,
    player_name TEXT NOT NULL,
    
    -- Category
    category TEXT NOT NULL, -- 'fastest', 'no_heal', 'deathless', 'perfect'
    
    -- Performance
    total_combat_time_seconds INTEGER NOT NULL,
    heals_used INTEGER DEFAULT 0,
    revive_used BOOLEAN DEFAULT 0,
    damage_taken INTEGER DEFAULT 0,
    
    -- Metadata
    character_level INTEGER,
    specialization_name TEXT,
    submitted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    season_id TEXT,
    
    FOREIGN KEY (run_id) REFERENCES Boss_Gauntlet_Runs(run_id) ON DELETE CASCADE,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    
    CHECK (category IN ('fastest', 'no_heal', 'deathless', 'perfect'))
);

CREATE INDEX idx_gauntlet_leaderboard_category ON Boss_Gauntlet_Leaderboard(category);
CREATE INDEX idx_gauntlet_leaderboard_time ON Boss_Gauntlet_Leaderboard(total_combat_time_seconds);
CREATE INDEX idx_gauntlet_leaderboard_season ON Boss_Gauntlet_Leaderboard(season_id);
```

### Example Data

```sql
-- Example: Active gauntlet run in progress
INSERT INTO Boss_Gauntlet_Runs (
    character_id, gauntlet_sequence_id, current_boss_index,
    remaining_heals, remaining_revives, bosses_defeated,
    total_combat_time_seconds, total_damage_taken
)
VALUES (
    201, 'trial_of_the_forsaken', 4,
    2, 1, 4,
    1847, 342
);

-- Example: Completed boss encounter in gauntlet
INSERT INTO Boss_Gauntlet_Boss_Encounters (
    run_id, boss_id, boss_index, is_completed,
    combat_time_seconds, damage_taken, damage_dealt, heals_used_in_fight
)
VALUES (
    1, 'forgemaster_thrain', 1, 1,
    287, 68, 1420, 0
);

-- Example: Leaderboard entry for fastest clear
INSERT INTO Boss_Gauntlet_Leaderboard (
    run_id, character_id, player_name, category,
    total_combat_time_seconds, heals_used, revive_used, damage_taken,
    character_level, specialization_name
)
VALUES (
    5, 202, 'Raven_Storm', 'fastest',
    2843, 0, 0, 1240,
    28, 'Berserkr'
);
```

---

## IV. Service Implementation

### BossGauntletService

```csharp
public interface IBossGauntletService
{
    Task<Result<GauntletRun>> StartGauntletRunAsync(int characterId, string sequenceId);
    Task<Result> AdvanceToNextBossAsync(int runId);
    Task<Result> UseHealingChargeAsync(int runId, int healAmount);
    Task<Result> UseReviveAsync(int runId);
    Task<Result> CompleteGauntletAsync(int runId);
    Task<Result> ForfeitGauntletAsync(int runId);
    Task<GauntletRun> GetActiveRunAsync(int characterId);
}

public class BossGauntletService : IBossGauntletService
{
    private readonly ILogger<BossGauntletService> _logger;
    private readonly IBossGauntletRepository _gauntletRepo;
    private readonly IBossEncounterService _bossEncounterService;
    private readonly ICharacterRepository _characterRepo;
    
    public async Task<Result<GauntletRun>> StartGauntletRunAsync(
        int characterId, 
        string sequenceId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Starting Boss Gauntlet run for character {CharacterId}, sequence {SequenceId}",
            characterId, sequenceId);
        
        // Validation: No active run already
        var existingRun = await GetActiveRunAsync(characterId);
        if (existingRun != null)
        {
            return Result.Failure<GauntletRun>("Character already has active gauntlet run");
        }
        
        // Validation: Sequence exists
        var sequence = await _gauntletRepo.GetSequenceAsync(sequenceId);
        if (sequence == null)
        {
            return Result.Failure<GauntletRun>($"Gauntlet sequence not found: {sequenceId}");
        }
        
        // Create new run
        var run = new GauntletRun
        {
            CharacterId = characterId,
            GauntletSequenceId = sequenceId,
            StartTime = DateTime.UtcNow,
            CurrentBossIndex = 0,
            IsActive = true,
            RemainingHeals = 3,
            RemainingRevives = 1,
            IsDeathless = true,
            IsNoHeal = true,
            IsPerfect = true
        };
        
        await _gauntletRepo.CreateRunAsync(run);
        
        _logger.Information(
            "Boss Gauntlet run {RunId} started for character {CharacterId}",
            run.RunId, characterId);
        
        // Initialize first boss encounter
        await InitializeBossEncounterAsync(run, bosIndex: 0);
        
        return Result.Success(run);
    }
    
    public async Task<Result> AdvanceToNextBossAsync(int runId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Advancing to next boss in gauntlet run {RunId}",
            runId);
        
        var run = await _gauntletRepo.GetRunAsync(runId);
        if (run == null)
        {
            return Result.Failure("Gauntlet run not found");
        }
        
        if (!run.IsActive)
        {
            return Result.Failure("Gauntlet run is not active");
        }
        
        // Mark current boss as complete
        await CompleteBossEncounterAsync(run.RunId, run.CurrentBossIndex);
        
        // Increment boss index
        run.CurrentBossIndex++;
        run.BossesDefeated++;
        
        var sequence = await _gauntletRepo.GetSequenceAsync(run.GauntletSequenceId);
        var bossOrder = JsonSerializer.Deserialize<List<string>>(sequence.BossOrder);
        
        // Check if gauntlet complete
        if (run.CurrentBossIndex >= bossOrder.Count)
        {
            _logger.Information(
                "Gauntlet run {RunId} completed all bosses",
                runId);
            
            return await CompleteGauntletAsync(runId);
        }
        
        // Restore sustain resources between bosses
        await RestoreSustainResourcesAsync(run.CharacterId);
        
        // Initialize next boss
        await InitializeBossEncounterAsync(run, run.CurrentBossIndex);
        
        await _gauntletRepo.UpdateRunAsync(run);
        
        _logger.Information(
            "Gauntlet run {RunId} advanced to boss {BossIndex}",
            runId, run.CurrentBossIndex);
        
        return Result.Success();
    }
    
    public async Task<Result> UseHealingChargeAsync(int runId, int healAmount)
    {
        using var operation = _logger.BeginTimedOperation(
            "Using healing charge in gauntlet run {RunId}",
            runId);
        
        var run = await _gauntletRepo.GetRunAsync(runId);
        if (run == null)
        {
            return Result.Failure("Gauntlet run not found");
        }
        
        if (run.RemainingHeals <= 0)
        {
            _logger.Warning(
                "Healing charge denied: No remaining heals in run {RunId}",
                runId);
            return Result.Failure("No healing charges remaining");
        }
        
        // Deduct healing charge
        run.RemainingHeals--;
        run.HealsUsed++;
        run.IsNoHeal = false;
        run.IsPerfect = false;
        
        await _gauntletRepo.UpdateRunAsync(run);
        
        // Apply healing to character
        var character = await _characterRepo.GetByIdAsync(run.CharacterId);
        character.CurrentHP = Math.Min(character.MaxHP, character.CurrentHP + healAmount);
        await _characterRepo.UpdateAsync(character);
        
        _logger.Information(
            "Healing charge used in run {RunId}: {HealAmount} HP restored, {Remaining} charges left",
            runId, healAmount, run.RemainingHeals);
        
        return Result.Success();
    }
    
    public async Task<Result> UseReviveAsync(int runId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Using revive in gauntlet run {RunId}",
            runId);
        
        var run = await _gauntletRepo.GetRunAsync(runId);
        if (run == null)
        {
            return Result.Failure("Gauntlet run not found");
        }
        
        if (run.RemainingRevives <= 0)
        {
            _logger.Warning(
                "Revive denied: No remaining revives in run {RunId}",
                runId);
            return Result.Failure("No revives remaining");
        }
        
        // Deduct revive
        run.RemainingRevives--;
        run.ReviveUsed = true;
        run.IsDeathless = false;
        run.IsPerfect = false;
        
        await _gauntletRepo.UpdateRunAsync(run);
        
        // Restore character to 50% HP
        var character = await _characterRepo.GetByIdAsync(run.CharacterId);
        character.CurrentHP = character.MaxHP / 2;
        character.IsAlive = true;
        await _characterRepo.UpdateAsync(character);
        
        _logger.Information(
            "Revive used in run {RunId}: Character restored to 50% HP",
            runId);
        
        return Result.Success();
    }
    
    public async Task<Result> CompleteGauntletAsync(int runId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Completing gauntlet run {RunId}",
            runId);
        
        var run = await _gauntletRepo.GetRunAsync(runId);
        if (run == null)
        {
            return Result.Failure("Gauntlet run not found");
        }
        
        run.EndTime = DateTime.UtcNow;
        run.IsActive = false;
        run.IsCompleted = true;
        
        await _gauntletRepo.UpdateRunAsync(run);
        
        // Submit to leaderboards
        await SubmitLeaderboardEntriesAsync(run);
        
        // Award rewards
        await AwardGauntletRewardsAsync(run);
        
        _logger.Information(
            "Gauntlet run {RunId} completed: Time={Time}s, Heals={Heals}, Revive={Revive}",
            runId, run.TotalCombatTimeSeconds, run.HealsUsed, run.ReviveUsed);
        
        return Result.Success();
    }
    
    public async Task<Result> ForfeitGauntletAsync(int runId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Forfeiting gauntlet run {RunId}",
            runId);
        
        var run = await _gauntletRepo.GetRunAsync(runId);
        if (run == null)
        {
            return Result.Failure("Gauntlet run not found");
        }
        
        run.EndTime = DateTime.UtcNow;
        run.IsActive = false;
        run.IsCompleted = false;
        
        await _gauntletRepo.UpdateRunAsync(run);
        
        _logger.Information(
            "Gauntlet run {RunId} forfeited at boss {BossIndex}",
            runId, run.CurrentBossIndex);
        
        return Result.Success();
    }
    
    private async Task InitializeBossEncounterAsync(GauntletRun run, int bossIndex)
    {
        var sequence = await _gauntletRepo.GetSequenceAsync(run.GauntletSequenceId);
        var bossOrder = JsonSerializer.Deserialize<List<string>>(sequence.BossOrder);
        var bossId = bossOrder[bossIndex];
        
        // Apply gauntlet scaling
        var scalingMultiplier = 1.0f + (bossIndex * 0.1f);
        
        var boss = await _bossEncounterService.SpawnBossAsync(
            bossId, 
            difficultyMultiplier: scalingMultiplier);
        
        // Create encounter record
        var encounter = new GauntletBossEncounter
        {
            RunId = run.RunId,
            BossId = bossId,
            BossIndex = bossIndex,
            StartTime = DateTime.UtcNow
        };
        
        await _gauntletRepo.CreateBossEncounterAsync(encounter);
        
        _logger.Information(
            "Boss encounter initialized: Run {RunId}, Boss {BossIndex}/{Total} ({BossId}), Scaling {Scaling}x",
            run.RunId, bossIndex + 1, bossOrder.Count, bossId, scalingMultiplier);
    }
    
    private async Task RestoreSustainResourcesAsync(int characterId)
    {
        var character = await _characterRepo.GetByIdAsync(characterId);
        
        // Restore Stamina and Aether (HP intentionally NOT restored)
        character.CurrentStamina = character.MaxStamina;
        character.CurrentAether = character.MaxAether;
        
        // Clear status effects
        await ClearStatusEffectsAsync(characterId);
        
        await _characterRepo.UpdateAsync(character);
        
        _logger.Debug(
            "Sustain resources restored for character {CharacterId}: Stamina={Stamina}, Aether={Aether}",
            characterId, character.MaxStamina, character.MaxAether);
    }
}
```

---

## V. Integration with v0.23 Boss Encounters

### Boss Encounter Modification

```csharp
// In BossEncounterService.cs

public class BossEncounterContext
{
    public bool IsGauntletMode { get; set; }
    public int? GauntletRunId { get; set; }
    public float DifficultyMultiplier { get; set; } = 1.0f;
}

public async Task<Boss> SpawnBossAsync(
    string bossId, 
    BossEncounterContext context = null)
{
    var boss = await _bossRepo.GetByIdAsync(bossId);
    
    if (context?.IsGauntletMode == true)
    {
        _logger.Information(
            "Spawning boss {BossId} in Gauntlet Mode: Multiplier {Mult}x",
            bossId, context.DifficultyMultiplier);
        
        // Apply gauntlet scaling
        boss.MaxHP = (int)(boss.MaxHP * context.DifficultyMultiplier);
        boss.CurrentHP = boss.MaxHP;
        boss.BaseDamage = (int)(boss.BaseDamage * context.DifficultyMultiplier);
        
        // Disable loot drops in gauntlet
        boss.DropLootOnDefeat = false;
    }
    
    return boss;
}
```

### Death Handling in Gauntlet

```csharp
// In DeathService.cs

public async Task<DeathResult> HandleCharacterDeathAsync(int characterId)
{
    // Check if in gauntlet mode
    var gauntletRun = await _gauntletService.GetActiveRunAsync(characterId);
    
    if (gauntletRun != null)
    {
        _logger.Warning(
            "Character {CharacterId} died in Boss Gauntlet run {RunId}",
            characterId, gauntletRun.RunId);
        
        // Check if revive available
        if (gauntletRun.RemainingRevives > 0)
        {
            _logger.Information(
                "Auto-revive triggered in gauntlet run {RunId}",
                gauntletRun.RunId);
            
            await _gauntletService.UseReviveAsync(gauntletRun.RunId);
            
            return new DeathResult
            {
                Revived = true,
                Message = "Revive used. This is your last chance."
            };
        }
        else
        {
            _logger.Information(
                "Gauntlet run {RunId} failed: Character death with no revives remaining",
                gauntletRun.RunId);
            
            await _gauntletService.ForfeitGauntletAsync(gauntletRun.RunId);
            
            return new DeathResult
            {
                GauntletFailed = true,
                Message = "Boss Gauntlet failed. You must restart from the beginning."
            };
        }
    }
    
    // Normal death handling
    return await HandleStandardDeathAsync(characterId);
}
```

---

## VI. Leaderboard System

### Leaderboard Submission

```csharp
public class LeaderboardSubmissionService
{
    public async Task SubmitGauntletRunAsync(GauntletRun run)
    {
        var character = await _characterRepo.GetByIdAsync(run.CharacterId);
        
        // Submit to all applicable categories
        var categories = DetermineLeaderboardCategories(run);
        
        foreach (var category in categories)
        {
            var entry = new GauntletLeaderboardEntry
            {
                RunId = run.RunId,
                CharacterId = run.CharacterId,
                PlayerName = [character.Name](http://character.Name),
                Category = category,
                TotalCombatTimeSeconds = run.TotalCombatTimeSeconds,
                HealsUsed = run.HealsUsed,
                ReviveUsed = run.ReviveUsed,
                DamageTaken = run.TotalDamageTaken,
                CharacterLevel = character.Level,
                SpecializationName = character.SpecializationName,
                SeasonId = await GetCurrentSeasonIdAsync()
            };
            
            await _leaderboardRepo.SubmitEntryAsync(entry);
            
            _logger.Information(
                "Leaderboard entry submitted: Run {RunId}, Category {Category}, Time {Time}s",
                run.RunId, category, run.TotalCombatTimeSeconds);
        }
    }
    
    private List<string> DetermineLeaderboardCategories(GauntletRun run)
    {
        var categories = new List<string> { "fastest" }; // Always eligible
        
        if (!run.ReviveUsed)
        {
            categories.Add("deathless");
        }
        
        if (run.HealsUsed == 0)
        {
            categories.Add("no_heal");
        }
        
        if (!run.ReviveUsed && run.HealsUsed == 0 && run.TotalDamageTaken == 0)
        {
            categories.Add("perfect");
        }
        
        return categories;
    }
}
```

### Leaderboard Display

```jsx
=== BOSS GAUNTLET LEADERBOARD ===
[Fastest Clear Time]

Rank | Player          | Time    | Heals | Revive | Build
-----|-----------------|---------|-------|--------|----------------
1    | Raven_Storm     | 47:23   | 0     | No     | Berserkr L28
2    | IronWill_99     | 52:18   | 1     | No     | Skjaldmaer L30
3    | MysticAsh       | 58:44   | 2     | No     | Seidkona L27
4    | BossSlayer42    | 1:03:17 | 2     | Yes    | Einbui L29
5    | YOU             | 1:08:32 | 3     | No     | Berserkr L26

[Filters: This Season | Category: Fastest | Top 100]

Press [V] to view run details | [C] to change category
```

---

## VII. Testing Requirements

### Unit Tests (Target: 85%+ Coverage)

```csharp
[TestClass]
public class BossGauntletServiceTests
{
    [TestMethod]
    public async Task StartGauntletRun_ValidCharacter_Success()
    {
        // Arrange
        var characterId = 301;
        var mockRepo = new Mock<IBossGauntletRepository>();
        mockRepo.Setup(r => r.GetActiveRunAsync(characterId))
            .ReturnsAsync((GauntletRun)null);
        mockRepo.Setup(r => r.GetSequenceAsync("trial_of_the_forsaken"))
            .ReturnsAsync(new GauntletSequence { SequenceId = "trial_of_the_forsaken" });
        
        var service = CreateBossGauntletService(mockRepo.Object);
        
        // Act
        var result = await service.StartGauntletRunAsync(characterId, "trial_of_the_forsaken");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(3, result.Value.RemainingHeals);
        Assert.AreEqual(1, result.Value.RemainingRevives);
    }
    
    [TestMethod]
    public async Task UseHealingCharge_ValidRun_DecrementsCharges()
    {
        // Arrange
        var runId = 1;
        var mockRun = new GauntletRun { RunId = runId, RemainingHeals = 3, CharacterId = 301 };
        var mockRepo = new Mock<IBossGauntletRepository>();
        mockRepo.Setup(r => r.GetRunAsync(runId)).ReturnsAsync(mockRun);
        
        var service = CreateBossGauntletService(mockRepo.Object);
        
        // Act
        var result = await service.UseHealingChargeAsync(runId, healAmount: 50);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, mockRun.RemainingHeals);
        Assert.AreEqual(1, mockRun.HealsUsed);
        Assert.IsFalse(mockRun.IsNoHeal);
    }
    
    [TestMethod]
    public async Task UseHealingCharge_NoChargesRemaining_Failure()
    {
        // Arrange
        var runId = 2;
        var mockRun = new GauntletRun { RunId = runId, RemainingHeals = 0 };
        var mockRepo = new Mock<IBossGauntletRepository>();
        mockRepo.Setup(r => r.GetRunAsync(runId)).ReturnsAsync(mockRun);
        
        var service = CreateBossGauntletService(mockRepo.Object);
        
        // Act
        var result = await service.UseHealingChargeAsync(runId, healAmount: 50);
        
        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Error.Contains("No healing charges"));
    }
}
```

---

## VIII. Success Criteria

**v0.40.3 is DONE when:**

- [ ]  **Boss Gauntlet System:**
    - [ ]  8-10 boss sequence defined
    - [ ]  Sequential boss encounters work
    - [ ]  No rest between bosses enforced
    - [ ]  Gauntlet completion triggers rewards
- [ ]  **Resource Limitation:**
    - [ ]  3 healing charges functional
    - [ ]  1 revive functional
    - [ ]  Healing charge depletion prevents further heals
    - [ ]  Revive restores to 50% HP
    - [ ]  Second death ends run
- [ ]  **Between-Boss Logic:**
    - [ ]  Stamina refilled
    - [ ]  Aether refilled
    - [ ]  HP NOT refilled
    - [ ]  Status effects cleared
    - [ ]  5-minute pause allowed
- [ ]  **Death Handling:**
    - [ ]  First death triggers revive
    - [ ]  Second death forfeits run
    - [ ]  Run progress NOT saved on death
    - [ ]  Must restart from Boss 1
- [ ]  **Difficulty Scaling:**
    - [ ]  Each boss scales (+10% per boss)
    - [ ]  Final boss at 200% stats
    - [ ]  Level normalization functional
- [ ]  **Database Schema:**
    - [ ]  Boss_Gauntlet_Runs table created
    - [ ]  Boss_Gauntlet_Sequences table created
    - [ ]  Boss_Gauntlet_Boss_Encounters table created
    - [ ]  Boss_Gauntlet_Leaderboard table created
    - [ ]  All indexes created
- [ ]  **Service Layer:**
    - [ ]  BossGauntletService operational
    - [ ]  LeaderboardSubmissionService operational
    - [ ]  All interfaces documented
- [ ]  **Integration:**
    - [ ]  v0.23 boss encounters support gauntlet mode
    - [ ]  Death system triggers revive in gauntlet
    - [ ]  Leaderboard submission on completion
- [ ]  **Leaderboards:**
    - [ ]  4 categories functional (fastest, no_heal, deathless, perfect)
    - [ ]  Top 100 displayed
    - [ ]  Seasonal tracking
- [ ]  **Testing:**
    - [ ]  85%+ unit test coverage
    - [ ]  12+ unit tests passing
    - [ ]  2+ integration tests passing
- [ ]  **Logging:**
    - [ ]  Serilog structured logging throughout
    - [ ]  Gauntlet start/end logged
    - [ ]  Healing/revive usage logged
    - [ ]  Boss victories logged

---

## IX. Timeline

**Week 1: Core Gauntlet System** — 3-4 hours

- Database schema
- BossGauntletService basic structure
- Boss sequence management
- Run tracking

**Week 2: Resource Management** — 2-3 hours

- Healing charge system
- Revive mechanic
- Between-boss restoration
- Death handling integration

**Week 3: Leaderboards & Rewards** — 2-3 hours

- Leaderboard submission
- Category determination
- Victory rewards
- Completion tracking

**Week 4: Testing & Polish** — 2-3 hours

- Unit tests (12+)
- Integration tests
- Performance validation
- Edge case handling

**Total: 7-10 hours (1-2 weeks part-time)**

---

**Ready to test true mastery.**