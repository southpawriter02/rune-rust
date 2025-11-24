# v0.40.4: Endless Mode & Leaderboards

## Overview

Endless Mode is a wave-based survival challenge with infinite difficulty scaling, competitive scoring, and global leaderboards. Players face increasingly difficult waves until death, competing for highest wave reached and highest total score.

## Core Mechanics

### Wave Progression

**Infinite Scaling:**
- Wave 1: 3 enemies, Level 6, 1.0x difficulty
- Wave 10: 8 enemies, Level 15, 2.0x difficulty
- Wave 20: 13 enemies, Level 25, 3.0x difficulty
- Wave 30: 18 enemies, Level 35, 4.0x difficulty
- Wave 50: 28 enemies, Level 55, 6.0x difficulty
- Wave 100: 53 enemies, Level 105, 11.0x difficulty

**Boss Waves:** Every 5 waves (5, 10, 15, 20, etc.)

**Scaling Formulas:**
```
Enemy Count = 3 + (wave / 2)
Enemy Level = 5 + wave
Difficulty Multiplier = 1.0 + (wave × 0.1)
```

### Scoring System

**Total Score = WaveScore + KillScore + BossScore + TimeBonus + SurvivalBonus**

**Components:**
- **Wave Score**: WavesCompleted × 1,000
- **Kill Score**: EnemiesKilled × 50
- **Boss Score**: BossesKilled × 500
- **Time Bonus**: Max(0, 10,000 - TotalTimeSeconds)
- **Survival Bonus**: Max(0, 5,000 - TotalDamageTaken)

**Example Calculation:**
```
Wave 32 reached:
- Wave Score: 32 × 1,000 = 32,000
- Kill Score: 487 × 50 = 24,350
- Boss Score: 8 × 500 = 4,000
- Time Bonus: 10,000 - 7,420 = 2,580
- Survival Bonus: 5,000 - 2,180 = 2,820

Total Score: 65,750
```

### Seed System

**Daily Seeds:**
- Format: `ENDLESS-20251124`
- Generated daily for competitive play
- Same seed = same enemy composition per wave

**Custom Seeds:**
- Format: `ENDLESS-CUSTOM-NAME`
- Practice specific scenarios
- Verify leaderboard runs

**Reproducibility:**
- All wave generation is seeded
- Build hash prevents impossible builds
- Community can replay to verify

## Architecture

### Core Components

1. **EndlessModeService** - Main orchestration
   - Run management (start/end)
   - Wave completion
   - Score calculation
   - Leaderboard submission

2. **WaveScalingService** - Difficulty scaling
   - Wave configuration generation
   - Scaling formula calculations
   - Difficulty estimation

3. **EndlessModeRepository** - Data access
   - Run CRUD operations
   - Wave tracking
   - Leaderboard management
   - Season handling

4. **SeedGenerator** - Seed management
   - Daily seed generation
   - Custom seed validation
   - Wave-specific seeding

### Models

**EndlessRun** - Individual run tracking
**EndlessWave** - Per-wave performance
**EndlessLeaderboardEntry** - Competitive rankings
**EndlessSeason** - 3-month competitive periods
**EndlessScore** - Score breakdown

## Database Schema

### Tables

**Endless_Mode_Seasons**:
- 3-month competitive periods
- Active season flagging
- Historical preservation

**Endless_Mode_Runs**:
- Run state and progress
- Combat metrics
- Scoring components
- Verification data

**Endless_Mode_Waves**:
- Wave composition
- Performance tracking
- Time and damage records

**Endless_Mode_Leaderboards**:
- 2 categories per season
- Ranked entries
- Verification status
- Community reports

### Views

**vw_endless_top_waves** - Top 100 by wave
**vw_endless_top_scores** - Top 100 by score
**vw_character_endless_stats** - Per-character statistics
**vw_endless_wave_stats** - Wave difficulty analytics

## Leaderboard Categories

### 1. Highest Wave Reached
- **Metric**: Furthest wave number
- **Tiebreaker**: Total score
- **Focus**: Pure survival

### 2. Highest Total Score
- **Metric**: Total score points
- **Tiebreaker**: Wave reached
- **Focus**: Optimization and efficiency

## Usage Examples

### Start an Endless Run

```csharp
var service = new EndlessModeService(repository);

// Start with daily seed
var run = service.StartEndlessRun(
    characterId: 1,
    seed: null,  // Uses daily seed
    characterBuildHash: "ABC123..."
);

Console.WriteLine($"Run {run.RunId} started on wave {run.CurrentWave}");
Console.WriteLine($"Seed: {run.Seed}");
```

### Complete a Wave

```csharp
// After defeating all enemies in wave
service.CompleteWave(
    runId: run.RunId,
    enemiesKilled: 8,
    damageTaken: 120,
    damageDealt: 2500
);

// Get next wave configuration
var nextWave = service.GetCurrentWaveConfig(run.RunId);
Console.WriteLine($"Next: {nextWave.DisplayText}");
```

### End a Run

```csharp
// Player dies
service.EndRun(run.RunId);

// Get final summary
var summary = service.GetRunSummary(run.RunId);
Console.WriteLine(summary.DisplaySummary);

/*
=== Endless Run Summary ===
Wave Reached: 32
Total Score: 65,750

Combat:
  Enemies Killed: 487
  Bosses Killed: 8
  Damage Dealt: 15,230
  Damage Taken: 2,180

Performance:
  Total Time: 124.3 minutes
  Average Wave Time: 3.9s
  Boss Waves Completed: 6

Score Breakdown:
  Wave Score:     32,000
  Kill Score:     24,350
  Boss Score:     4,000
  Time Bonus:     2,580
  Survival Bonus: 2,820

Total: 65,750
*/
```

### Check Leaderboards

```csharp
// Get top 10 highest waves
var topWaves = service.GetLeaderboard(
    EndlessLeaderboardCategory.HighestWave,
    seasonId: null,  // Current season
    limit: 10
);

foreach (var entry in topWaves)
{
    Console.WriteLine($"#{entry.Rank} - {entry.PlayerName}");
    Console.WriteLine($"  Wave {entry.HighestWaveReached} • {entry.TotalScore:N0} points");
    Console.WriteLine($"  Time: {entry.TimeDisplay}");
}

// Get top 10 highest scores
var topScores = service.GetLeaderboard(
    EndlessLeaderboardCategory.HighestScore,
    limit: 10
);

// Get all leaderboards
var allBoards = service.GetAllLeaderboards(limit: 100);
```

### Use Custom Seed

```csharp
// Practice specific seed
var customRun = service.StartEndlessRun(
    characterId: 1,
    seed: "ENDLESS-PRACTICE-123"
);

// Use specific date seed
var dateRun = service.StartEndlessRun(
    characterId: 1,
    seed: SeedGenerator.GenerateDailySeed(new DateTime(2025, 11, 15))
);
```

### Get Run Statistics

```csharp
// Get detailed summary
var summary = service.GetRunSummary(runId);

Console.WriteLine($"Total Waves: {summary.TotalWaves}");
Console.WriteLine($"Boss Waves: {summary.BossWavesCompleted}");
Console.WriteLine($"Avg Wave Time: {summary.AverageWaveTime:F1}s");

if (summary.FastestWave != null)
{
    Console.WriteLine($"Fastest: Wave {summary.FastestWave.WaveNumber} ({summary.FastestWave.WaveTimeSeconds}s)");
}

// Get score breakdown
var score = service.GetScoreBreakdown(runId);
Console.WriteLine(score.BreakdownDisplay);
```

### Forfeit a Run

```csharp
// Player wants to quit
service.ForfeitRun(runId);

// Score is still calculated and saved
// But not submitted to leaderboards (optional behavior)
```

## Design Philosophy

### 1. Infinite Challenge
- No victory condition - only survival
- Difficulty scales forever
- Always a higher goal to reach
- Prevents content exhaustion

### 2. Multiple Optimization Paths
- Wave Score: Reach furthest wave
- Kill Score: Maximize enemy kills
- Boss Score: Optimize boss wave efficiency
- Time Bonus: Complete quickly
- Survival Bonus: Minimize damage

Different builds excel in different areas.

### 3. Competitive Without Toxicity
- Seed-based verification
- Build hash anti-cheat
- Community report system
- Seasonal resets prevent permanent domination
- Multiple leaderboard categories

### 4. Reproducible and Fair
- Seeded wave generation
- Deterministic enemy composition
- No RNG advantages
- Speedrun-friendly
- Practice mode with custom seeds

### 5. Finite Session Length
- Early waves: 2-3 minutes each
- Mid waves (20-30): 5-8 minutes each
- Late waves (40+): 10-15 minutes each
- Typical session: 2-4 hours to Wave 30+

Respects player time while offering infinite replayability.

## Wave Difficulty Estimates

### Wave 10
- Enemies: 8
- Level: 15
- Difficulty: 2.0x
- Estimated Time: ~25 minutes total
- Total Enemies: ~65

### Wave 20
- Enemies: 13
- Level: 25
- Difficulty: 3.0x
- Estimated Time: ~60 minutes total
- Total Enemies: ~165

### Wave 30
- Enemies: 18
- Level: 35
- Difficulty: 4.0x
- Estimated Time: ~120 minutes total
- Total Enemies: ~315

### Wave 50
- Enemies: 28
- Level: 55
- Difficulty: 6.0x
- Estimated Time: ~250 minutes total
- Total Enemies: ~765

## Seasonal System

**Season Duration**: 3 months

**Current Season**: Season 1: The First Gauntlet
- Start: 2025-10-01
- End: 2025-12-31

**Season Benefits**:
- Fresh leaderboards every quarter
- No permanent elite class
- New player friendly
- Historical records preserved

**Between Seasons**:
- Old leaderboards archived
- New season activated
- All players start equal

## Anti-Cheat & Verification

### Build Hash
- Captures character stats at run start
- Prevents impossible stat combinations
- Validated on leaderboard submission

### Seed Verification
- All runs use known seeds
- Community can replay seed
- Verify claimed performance

### Community Reports
- Flag suspicious entries
- Report threshold for review
- Manual verification by admins

### Verification Status
- Auto-verified: Basic checks pass
- Flagged: Community reports
- Reviewed: Admin investigated

## Performance

- **Run Start**: <100ms
- **Wave Creation**: <50ms
- **Wave Completion**: <100ms
- **Score Calculation**: <50ms
- **Leaderboard Update**: <200ms

## Integration Points

### v0.40.1 New Game+
- NG+ tier affects starting difficulty
- Separate leaderboards per NG+ tier (future)

### v0.23 Boss Encounters
- Boss waves use boss definitions
- Boss scaling applies

### v0.15 Trauma Economy
- Corruption accumulates across waves
- Trauma effects apply

## Future Enhancements (v0.42+)

- **NG+ Leaderboards**: Separate boards per NG+ tier
- **Weekly Challenges**: Modifier-enhanced runs
- **Co-op Endless**: 2-4 player survival
- **Custom Modifiers**: Apply Challenge Sector modifiers
- **Spectator Mode**: Watch top runs
- **Replay System**: Watch recorded runs

---

**Implementation Status:** ✅ Complete
**Wave Scaling:** Infinite
**Leaderboard Categories:** 2 (Highest Wave, Highest Score)
**Scoring Components:** 5 (Wave, Kill, Boss, Time Bonus, Survival Bonus)
**Seed System:** Daily + Custom seeds
**Seasonal Resets:** Every 3 months
**Documentation:** Comprehensive
