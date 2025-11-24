# v0.40.3: Boss Gauntlet Mode

## Overview

Boss Gauntlet Mode is a skill-testing endgame challenge where players face 8-10 sequential boss encounters with limited healing resources and no rest between fights. Success requires mastery of boss mechanics, resource management, and consistent execution under pressure.

## Core Mechanics

### Resource Limitations

**Full Heals: 3 per gauntlet**
- Restores character to full HP
- Limited to 3 uses across entire gauntlet
- Strategic timing is critical

**Revives: 1 per gauntlet**
- Auto-triggered on death
- Revives player to continue current boss fight
- Running out of revives ends the gauntlet in defeat

### Progression

1. **Start Gauntlet**: Select a sequence and begin
2. **Fight Bosses**: Face bosses in order with no breaks
3. **Manage Resources**: Use heals and revives strategically
4. **Complete or Fail**: Victory after final boss or defeat on death without revives

## Architecture

### Core Components

1. **BossGauntletService** - Main orchestration
   - Gauntlet run management
   - Resource tracking (heals/revives)
   - Auto-revive on death
   - Leaderboard updates

2. **BossGauntletRepository** - Data access layer
   - Sequence loading and availability
   - Run tracking and statistics
   - Boss encounter logging
   - Leaderboard management

3. **Gauntlet Models**
   - `GauntletSequence`: Gauntlet configuration
   - `GauntletRun`: Individual run tracking
   - `GauntletBossEncounter`: Boss fight records
   - `GauntletLeaderboardEntry`: Competitive rankings

## Database Schema

### Tables

**Boss_Gauntlet_Sequences**: 10 gauntlet configurations
- Moderate (2.0-2.5x): 2 gauntlets
- Hard (2.5-3.0x): 3 gauntlets
- Extreme (3.0-3.5x): 3 gauntlets
- Nightmare (3.5x+): 2 gauntlets

**Boss_Gauntlet_Runs**: Run tracking
- Status: in_progress, victory, defeat
- Resource counts: heals/revives remaining
- Statistics: time, damage, deaths
- NG+ tier context

**Boss_Gauntlet_Boss_Encounters**: Individual boss fights
- Fight outcome and statistics
- Resource usage per fight
- Character state snapshots

**Boss_Gauntlet_Leaderboard**: Competitive rankings
- 4 categories per gauntlet
- Auto-updated on victory
- Ranked by performance metrics

## Gauntlet Sequences

### Moderate Tier

#### Apprentice's Trial
- **Bosses:** 8
- **Theme:** Mixed fundamentals
- **Reward:** Gauntlet Blade (Tier 1)
- **Title:** Apprentice Champion
- **Prerequisites:** None

#### Elemental Gauntlet
- **Bosses:** 8
- **Theme:** Fire, frost, lightning, earth
- **Reward:** Elemental Focus
- **Title:** Elemental Master
- **Prerequisites:** None

### Hard Tier

#### Corruption's Descent
- **Bosses:** 9
- **Theme:** Void and corruption
- **Reward:** Void Breaker
- **Title:** Corruption's Bane
- **Prerequisites:** Apprentice's Trial

#### Titan's Challenge
- **Bosses:** 9
- **Theme:** Massive powerful enemies
- **Reward:** Titan Crusher
- **Title:** Titan Slayer
- **Prerequisites:** Elemental Gauntlet, NG+1

#### Speed Run Gauntlet
- **Bosses:** 8
- **Theme:** Optimized for speed
- **Reward:** Time Breaker
- **Title:** Speed Demon
- **Prerequisites:** Apprentice's Trial

### Extreme Tier

#### Nightmare Crucible
- **Bosses:** 10
- **Theme:** Nightmare-tier mechanics
- **Reward:** Nightmare Ender
- **Title:** Nightmare's End
- **Prerequisites:** Corruption's Descent, Titan's Challenge, NG+2

#### Flawless Gauntlet
- **Bosses:** 8
- **Theme:** Perfect execution required
- **Reward:** Perfection Blade
- **Title:** Flawless Victor
- **Prerequisites:** Corruption's Descent, NG+1

#### NG+ Elite Gauntlet
- **Bosses:** 9
- **Theme:** NG+ scaling showcase
- **Reward:** NG+ Crown
- **Title:** NG+ Elite
- **Prerequisites:** Nightmare Crucible, NG+2

### Nightmare Tier

#### Endgame Crucible
- **Bosses:** 10
- **Theme:** Maximum challenge
- **Reward:** Ultimate Weapon
- **Title:** Endgame Legend
- **Prerequisites:** Nightmare Crucible, Flawless Gauntlet, NG+ Elite Gauntlet, NG+3

#### The Impossible Gauntlet
- **Bosses:** 10
- **Theme:** Legendary difficulty
- **Reward:** God's Bane
- **Title:** The Impossible
- **Prerequisites:** Endgame Crucible, NG+5

## Leaderboard Categories

### 1. Fastest Time
- **Metric:** Completion time
- **Eligibility:** All victories
- **Focus:** Speed optimization

### 2. Flawless
- **Metric:** Completion time with 0 deaths
- **Eligibility:** Zero-death victories only
- **Focus:** Perfect execution

### 3. No Heal Challenge
- **Metric:** Completion time without using heals
- **Eligibility:** Victories without heal usage
- **Focus:** Resource conservation

### 4. NG+ Elite
- **Metric:** Completion time at highest NG+ tier
- **Eligibility:** NG+1 or higher
- **Focus:** Endgame mastery

## Usage Examples

### Start a Gauntlet

```csharp
var service = new BossGauntletService(repository);

// Get available gauntlets
var sequences = service.GetAvailableSequences(characterId, ngPlusTier: 2);

// Start gauntlet
var characterState = new { HP = 100, Level = 50, /* ... */ };
var run = service.StartGauntlet(characterId, "gauntlet_apprentice", ngPlusTier: 2, characterState);

Console.WriteLine($"Gauntlet started: {run.RunId}");
Console.WriteLine($"Resources: {run.FullHealsRemaining} heals, {run.RevivesRemaining} revives");
```

### Complete a Boss Encounter

```csharp
// Get current boss
var run = service.GetActiveRun(characterId);
var currentBoss = service.GetCurrentBossId(run);

// Fight boss...

// Log victory
service.CompleteBossEncounter(
    runId: run.RunId,
    bossId: currentBoss,
    completionTimeSeconds: 180,  // 3 minutes
    damageTaken: 200,
    damageDealt: 5000,
    deaths: 1,  // Auto-revive triggered
    healsUsed: 1,
    characterState: new { HP = 50, /* ... */ }
);
```

### Handle Death

```csharp
// Player dies during boss fight
var deathResult = service.HandleDeath(runId);

if (deathResult.AutoRevive)
{
    Console.WriteLine($"Auto-revive triggered! {deathResult.RevivesRemaining} revives remaining");
    // Continue fighting
}
else
{
    Console.WriteLine("No revives left - DEFEAT");
    // Gauntlet ended
}
```

### Use Resources

```csharp
// Use a full heal
var success = service.UseFullHeal(runId);
if (success)
{
    var status = service.GetResourceStatus(runId);
    Console.WriteLine($"Healed! {status.FullHealsRemaining} heals remaining");
}
else
{
    Console.WriteLine("No heals available!");
}
```

### Check Leaderboards

```csharp
// Get fastest times for a gauntlet
var fastest = service.GetLeaderboard(
    "gauntlet_nightmare",
    GauntletLeaderboardCategory.Fastest,
    limit: 10
);

foreach (var entry in fastest)
{
    Console.WriteLine($"#{entry.Rank} - {entry.CharacterName}");
    Console.WriteLine($"  Time: {entry.TimeDisplay}");
    Console.WriteLine($"  Deaths: {entry.TotalDeaths}");
}

// Get all leaderboards
var allBoards = service.GetAllLeaderboards("gauntlet_nightmare");
var flawless = allBoards[GauntletLeaderboardCategory.Flawless];
```

## Design Philosophy

### 1. Resource Management Over Attrition
- Limited heals and revives create strategic depth
- Players must decide when to heal vs. push through
- No infinite retries - each run is meaningful

### 2. Skill-Based Progression
- Success depends on player mastery, not RNG
- Same boss order every time (learnable)
- Leaderboards reward consistent execution

### 3. Multiple Competitive Tracks
- 4 leaderboard categories per gauntlet
- Different playstyles have different optimal strategies
- Encourages replayability and experimentation

### 4. Integration with NG+ System
- Gauntlets scale with NG+ tier
- Higher tier gauntlets require NG+ progression
- Elite endgame content reserved for veterans

### 5. Finite But Replayable
- 10 handcrafted gauntlets
- Each has unique boss composition and theme
- Leaderboard competition drives replayability

## Death and Defeat Mechanics

### Auto-Revive System

**On Death:**
1. Check if revives remaining > 0
2. If yes: Auto-revive triggered, continue fight
3. If no: Gauntlet ends in defeat

**Revive Behavior:**
- Instant resurrection during boss fight
- No HP restoration (use heals for that)
- Tracked in encounter statistics

### Defeat Conditions

1. **Death without revives:** Most common defeat
2. **Forfeit:** Player manually abandons run
3. **Disconnect (future):** Could auto-forfeit or suspend

### Victory Conditions

- Defeat final boss in sequence
- Resources don't need to be preserved
- Victory tracked with full statistics

## Performance

- **Gauntlet Start:** <100ms
- **Boss Encounter Logging:** <50ms
- **Resource Updates:** <20ms
- **Leaderboard Calculation:** <200ms

## Testing

### Unit Tests
Focus on:
- Resource tracking accuracy
- Auto-revive logic
- Leaderboard ranking calculation
- Prerequisite validation

### Integration Tests
- Full gauntlet run workflow
- Death and revive handling
- Leaderboard updates
- NG+ tier gating

## Dependencies

- **RuneAndRust.Core:** Entity models
- **RuneAndRust.Persistence:** Database repositories
- **v0.23 Boss Encounters:** Boss definitions
- **v0.40.1 New Game+:** NG+ scaling
- **Microsoft.Data.Sqlite:** 8.0.0
- **Serilog:** 4.0.0

## Future Enhancements (v0.42+)

- **Daily Gauntlets:** Rotating boss orders
- **Challenge Modifiers:** Apply v0.40.2 modifiers to gauntlets
- **Co-op Gauntlets:** 2-4 player gauntlet runs
- **Custom Gauntlets:** Player-created boss sequences
- **Seasonal Leaderboards:** Time-limited competitive seasons

---

**Implementation Status:** ✅ Complete
**Gauntlets Defined:** 10 (2 Moderate, 3 Hard, 3 Extreme, 2 Nightmare)
**Leaderboard Categories:** 4
**Resource System:** Full heals (3), Revives (1)
**Test Coverage:** Core functionality covered
**Documentation:** Comprehensive
