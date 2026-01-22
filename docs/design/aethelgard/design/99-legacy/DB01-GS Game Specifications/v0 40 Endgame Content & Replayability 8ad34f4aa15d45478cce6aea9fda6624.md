# v0.40: Endgame Content & Replayability

Type: Feature
Description: Implements endgame transformation converting completable campaign into infinitely replayable challenge system. Delivers New Game+ with 5 difficulty tiers, 20-30 handcrafted Challenge Sectors, Boss Gauntlet mode with 8-10 sequential bosses, Endless Mode with wave-based survival, and global leaderboards with seed verification.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.1-v0.29 (Complete), v0.23 (Boss Encounters), v0.15 (Trauma Economy), v0.14 (Quest System), v0.10-v0.12 (Dynamic Room Engine)
Implementation Difficulty: Very Complex
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.40.2: Challenge Sectors (v0%2040%202%20Challenge%20Sectors%20c67ece491acb40ca8eb4f6969c7c5b07.md), v0.40.1: New Game+ System (v0%2040%201%20New%20Game+%20System%20f0b38a74d2b345eba786561f53a88f54.md), v0.40.3: Boss Gauntlet Mode (v0%2040%203%20Boss%20Gauntlet%20Mode%2008e165b43c0e4db68c2bd60ac1f72a80.md), v0.40.4: Endless Mode & Leaderboards (v0%2040%204%20Endless%20Mode%20&%20Leaderboards%20419053f382ef4e959177b8d1caf6f4b2.md)
Template Validated: No
Voice Validated: No

# SPEC-ENDGAME-001: Endgame Content & Replayability

**Version**: 1.0

**Status**: Design Phase

**Last Updated**: 2025-11-23

**Implemented In**: v0.40 (Planned)

**Prerequisites**: v0.1-v0.29 (Complete), v0.23 (Boss Encounters), v0.15 (Trauma Economy), v0.14 (Quest System), v0.10-v0.12 (Dynamic Room Engine)

**Timeline**: 30-45 hours (4-6 weeks part-time)

**Philosophy**: Reward mastery with escalating challenge and meaningful progression beyond initial victory

---

## I. Executive Summary

v0.40 represents the **endgame transformation** of Rune & Rust, converting a completable campaign into an infinitely replayable challenge system with meta-progression, competitive leaderboards, and escalating difficulty.

**What v0.40 Delivers:**

- **New Game+** (v0.40.1) - Difficulty scaling with carryover progression
- **Challenge Sectors** (v0.40.2) - Extreme difficulty modifiers and special conditions
- **Boss Gauntlet Mode** (v0.40.3) - Sequential boss encounters with limited resources
- **Endless Mode & Leaderboards** (v0.40.4) - Competitive survival scoring

**Strategic Purpose:**

The current game (v0.1-v0.39) delivers **complete campaign experience**:

- ✅ Full campaign with quest chains and narrative arcs
- ✅ Boss encounters and climactic battles
- ✅ Character progression from novice to legendary
- ✅ Procedural generation for varied playthroughs

**But endgame lacks:**

- ❌ No reason to replay after campaign completion
- ❌ No escalating challenge for mastered mechanics
- ❌ No competitive elements or leaderboards
- ❌ No account-wide progression between runs
- ❌ Limited replayability beyond procedural variation

**v0.40 Solution:**

Build **infinite replayability through challenge escalation**:

- ✅ New Game+ with +50% difficulty per tier, max 5 tiers
- ✅ Challenge Sectors with 15+ extreme modifiers
- ✅ Boss Gauntlet mode: 8-10 sequential bosses, single run
- ✅ Endless Mode with wave-based survival and scoring
- ✅ Global leaderboards for competitive play
- ✅ Unlock-based progression across account

### Before v0.40 (Current State)

```jsx
Endgame:
- Complete campaign → Credits roll
- Can start new character
- No difficulty progression
- No competitive elements

Result: Limited replay value after first completion
```

### After v0.40 (Target State)

```jsx
Endgame:
- New Game+ (5 tiers of escalating challenge)
- Challenge Sectors (extreme difficulty modifiers)
- Boss Gauntlet (skill test with limited resources)
- Endless Mode (competitive survival)
- Leaderboards (global rankings)

Result: Infinite replayability with clear progression
```

---

## II. Related Documentation

### Dependencies

**Upstream Systems** (must exist before v0.40):

- **v0.1: Combat System** ✅ Complete
    - Core combat resolution
    - Damage calculations
    - Turn order system
    - Victory/defeat conditions
- **v0.15: Trauma Economy** ✅ Complete
    - Psychic Stress accumulation
    - Breaking Points and Trauma acquisition
    - Corruption thresholds
    - Mental state tracking
- **v0.14: Quest System** ✅ Complete
    - Quest generation and tracking
    - Objective system
    - Reward distribution
    - Narrative integration
- **v0.23: Boss Encounter System** ✅ Complete
    - Multi-phase boss fights
    - Boss-specific mechanics
    - Telegraphed attacks
    - Boss loot tables
- **v0.10-v0.12: Dynamic Room Engine** ✅ Complete
    - Procedural sector generation
    - WFC algorithm
    - Room population
    - Seed-based reproducibility
- **v0.20-v0.22: Tactical Combat Systems** ✅ Complete
    - Grid-based positioning
    - Environmental combat
    - Advanced status effects
    - Tactical movement

**Downstream Systems** (will depend on v0.40):

- **v0.41: Meta-Progression & Unlocks** ⏳ Planned
    - Account-wide unlocks use Challenge Sector completion
    - Alternative starts locked behind New Game+ tiers
    - Achievements track Boss Gauntlet victories
    - Cosmetics earned from Endless Mode scores
- **v0.42: Seasonal Events** ⏳ Planned
    - Seasonal Challenge Sectors with unique modifiers
    - Time-limited leaderboards
    - Seasonal cosmetic rewards
- **v0.43: Advanced Difficulty Systems** ⏳ Planned
    - Custom difficulty modifiers
    - Player-created challenges
    - Challenge sharing and verification

### Code References

**Primary Implementation** (files to be created in v0.40):

- `NewGamePlusService.cs` (~600 lines): Difficulty scaling and progression carryover
- `ChallengeSectorService.cs` (~700 lines): Challenge modifier application and validation
- `BossGauntletService.cs` (~500 lines): Sequential boss encounter management
- `EndlessModeService.cs` (~800 lines): Wave generation and survival scoring
- `LeaderboardService.cs` (~400 lines): Score tracking and rankings

**Integration Points** (existing files to be modified):

- `DungeonGenerationService.cs:lines 200-450`: Add challenge modifier hooks
- `CombatService.cs:lines 300-600`: Apply difficulty scaling
- `BossEncounterService.cs:lines 100-300`: Boss Gauntlet integration
- `ProgressionService.cs:lines 150-400`: New Game+ carryover logic
- `SaveService.cs:lines 100-250`: Account-wide data persistence

---

## III. Design Philosophy

### 1. Mastery Rewarded, Not Punished

**Principle**: Endgame difficulty respects player mastery while providing meaningful challenge.

**Design Rationale**:

Many endgame systems punish players for succeeding—stripping progression, forcing restarts, or creating artificial difficulty through cheap mechanics. v0.40 respects the time investment players made mastering the game.

**New Game+** allows progression carryover:

- ✅ Keep character level and Progression Points
- ✅ Keep specialization unlocks and abilities
- ✅ Keep equipment and resources
- ❌ Lose quest-specific items and temporary buffs

**Difficulty scales meaningfully**:

- +50% enemy HP and damage per tier
- +2 enemy levels per tier
- Enhanced enemy abilities at higher tiers
- Boss phases trigger earlier

**Why This Works**:

- Players feel powerful wielding mastered builds
- Challenge comes from enhanced enemy capabilities, not stripping player power
- Fair difficulty—players have tools to overcome challenges
- Rewards optimization and build refinement

### 2. Challenge Diversity Over Monotony

**Principle**: Multiple endgame modes cater to different player motivations.

**Design Rationale**:

Not all players want the same endgame experience. Some want narrative closure with higher stakes. Others want competitive rankings. Some enjoy testing builds against extreme conditions.

v0.40 provides **4 distinct endgame modes**:

**New Game+ (Narrative)**:

- Replay campaign with higher difficulty
- Maintain story progression and quest chains
- Carryover progression for smoother experience
- Appeals to: Story-focused players who want challenge

**Challenge Sectors (Build Testing)**:

- Extreme modifiers test specific builds
- No-heal sectors, permadeath rooms, double corruption
- Handcrafted challenges with unique rewards
- Appeals to: Theorycrafters and optimizers

**Boss Gauntlet (Skill Check)**:

- Pure skill test with limited resources
- No grinding, no overlevel cheese
- Sequential boss encounters without rest
- Appeals to: Challenge seekers and speedrunners

**Endless Mode (Competitive)**:

- Survival scoring with global leaderboards
- Escalating waves with increasing difficulty
- Limited resources force strategic resource management
- Appeals to: Competitive players and content creators

### 3. Respect Player Time and Investment

**Principle**: Endgame systems honor time investment without requiring infinite grinding.

**Design Rationale**:

Many endgame systems trap players in infinite progression treadmills—endless paragon levels, artifact power, or gear tiers. This disrespects player time and creates FOMO.

v0.40 uses **finite, achievable goals**:

**New Game+ Tiers: 5 Maximum**

- NG+ Tier 5 is hardest difficulty
- No infinite scaling
- Clear endpoint: Beat NG+5

**Challenge Sectors: 20-30 Total**

- Handcrafted challenges, not procedurally generated
- Each has unique modifiers and rewards
- Complete all = mastery achievement

**Boss Gauntlet: Single Clear = Victory**

- No need to repeat
- Speedrun leaderboards for replayability
- Victory is achievement, not grind

**Endless Mode: Score-Based**

- Leaderboards provide replayability
- No forced grind—play for fun or competition
- Seasonal resets keep competition fresh

**Why This Matters**:

- Players can "complete" endgame in 40-60 hours
- Additional play is optional (leaderboards, speedruns)
- No FOMO or mandatory daily grinds
- Respects that players have other games/life commitments

### 4. Procedural Foundation, Curated Peaks

**Principle**: Procedural generation provides variety; handcrafted challenges provide memorable peaks.

**Design Rationale**:

v0.10-v0.39 built a robust procedural generation engine. v0.40 leverages this while adding **curated experiences** for endgame highlights.

**Procedural Elements**:

- New Game+ uses existing sector generation with difficulty scaling
- Endless Mode generates infinite waves procedurally
- Challenge Sector layouts can be procedural

**Handcrafted Elements**:

- Challenge Sector modifiers are designed, not random
- Boss Gauntlet sequence is curated for pacing
- Specific challenge combinations tested for fairness

**Example: "The Silence Falls" Challenge Sector**

Handcrafted Modifiers:

- [Psychic Dampening]: No Aether regeneration (Mystics challenged)
- [Forlorn Surge]: +50% Forlorn enemies (thematic)
- [The Great Silence]: Ambient Psychic Stress +2 per turn

Procedural Layout:

- 8-12 rooms generated via WFC algorithm
- Enemy types pulled from Forlorn pool
- Hazards and loot procedurally placed

Result: Repeatable challenge with varied layouts, but consistent difficulty and theme.

### 5. Competitive Without Toxicity

**Principle**: Leaderboards celebrate achievement without fostering negative competition.

**Design Rationale**:

Competitive systems often create toxic environments—elitism, cheating, harassment. v0.40 designs leaderboards to celebrate mastery without toxicity.

**Anti-Toxicity Design**:

**1. Multiple Leaderboards (No Single "Best")**:

- Boss Gauntlet fastest clear time
- Endless Mode highest wave reached
- Endless Mode highest score (not just wave count)
- Challenge Sector completion percentage
- New Game+ tier reached

No single metric = "best player"—excellence in multiple dimensions.

**2. Seed-Based Verification**:

- Endless Mode runs use known seeds
- Leaderboard entries include seed for verification
- Community can verify suspicious scores
- Replay system (v2.0+) allows full run verification

**3. Seasonal Resets**:

- Leaderboards reset seasonally (quarterly)
- New season = fresh start for all players
- Prevents permanent stratification
- Historical records preserved but separate

**4. Anonymous Option**:

- Players can opt for anonymous leaderboard names
- No forced public profile
- Celebrate achievement without social pressure

**5. Cooperative Elements**:

- Challenge Sectors can be completed cooperatively (v2.0+)
- Boss Gauntlet co-op mode (v2.0+)
- Leaderboards include solo and co-op categories

---

## IV. System Overview

### Current State Analysis (v0.1-v0.39)

The game currently delivers **complete campaign experience**:

**Combat & Progression (v0.1-v0.9)**:

- Turn-based tactical combat
- Character progression with Legend and PP
- Equipment and loot systems
- Full attribute and skill systems

**Procedural Generation (v0.10-v0.12)**:

- WFC algorithm generates sectors
- Biome-based room templates
- Enemy and hazard population
- Seed-based reproducibility

**Core Content (v0.14-v0.16)**:

- Quest system with narrative integration
- Trauma Economy with Breaking Points
- 20+ enemy types, 60+ equipment pieces
- Boss encounter system

**Advanced Systems (v0.20-v0.39)**:

- Tactical grid combat with positioning
- Environmental combat and hazards
- 3D vertical exploration
- Biome transitions and blending
- Content density management

**Strengths**:

- ✅ Complete gameplay loop from character creation to campaign end
- ✅ Robust procedural generation for variety
- ✅ Deep tactical combat with positioning
- ✅ Rich Trauma Economy for psychological horror

**Limitations (Why v0.40 is Needed)**:

- ❌ **No Post-Campaign Content**: Credits roll → no further progression
- ❌ **No Difficulty Scaling**: First playthrough = only playthrough difficulty
- ❌ **Limited Replayability**: Procedural variety alone insufficient
- ❌ **No Competitive Elements**: No reason to optimize builds beyond campaign
- ❌ **No Long-Term Goals**: Character progression ends with campaign

### Scope Definition

**✅ In Scope (v0.40):**

- New Game+ system with 5 difficulty tiers
- Difficulty scaling formulas and enemy enhancements
- Progression carryover logic
- 20-30 handcrafted Challenge Sectors with unique modifiers
- Challenge Sector generation and modifier application
- Boss Gauntlet mode with 8-10 sequential bosses
- Resource limitation system for Gauntlet
- Endless Mode with wave-based survival
- Scoring system for Endless Mode
- Global leaderboards with seed verification
- Database schema for endgame tracking
- Service architecture for all 4 modes
- Integration with existing systems
- 80%+ unit test coverage

**❌ Out of Scope:**

- Custom difficulty modifiers (defer to v0.42)
- Cooperative endgame modes (defer to v2.0+)
- Seasonal events (defer to v0.42)
- Replay recording system (defer to v2.0+)
- Account-wide unlocks (v0.41 focus)
- Achievements system (v0.41 focus)
- Cosmetic rewards (v0.41 focus)
- New enemy types or bosses (use existing v0.1-v0.39 content)
- New abilities or equipment (content, not architecture)

**Why These Limits**: v0.40 is endgame architecture only. Account-wide progression and unlocks are v0.41. New content is v2.0+.

### System Lifecycle

```jsx
ENDGAME MODE SELECTION
  ↓
┌─────────────────────────────────────────────────────────────┐
│ PLAYER CHOICE: Which Endgame Mode?                         │
│   1. New Game+ (Replay campaign with higher difficulty)    │
│   2. Challenge Sector (Extreme modifiers, unique rewards)  │
│   3. Boss Gauntlet (Sequential bosses, no rest)            │
│   4. Endless Mode (Wave survival, leaderboards)            │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ MODE-SPECIFIC INITIALIZATION                                │
│   - New Game+: Load campaign with difficulty tier          │
│   - Challenge: Select sector and apply modifiers           │
│   - Gauntlet: Initialize boss sequence and resources       │
│   - Endless: Start wave 1 with fresh character             │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ GAMEPLAY LOOP (Mode-Specific)                               │
│   - Apply difficulty scaling and modifiers                  │
│   - Track mode-specific metrics (score, time, completion)  │
│   - Enforce mode-specific rules (no rest, limited healing)  │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ VICTORY/DEFEAT RESOLUTION                                   │
│   - Award mode-specific rewards                             │
│   - Update leaderboards (if applicable)                     │
│   - Track completion for meta-progression (v0.41)           │
└─────────────────────────────────────────────────────────────┘
  ↓
RETURN TO MODE SELECTION
```

---

## V. Functional Requirements

### FR-001: New Game+ System

**Requirement**: Players shall replay the campaign with escalating difficulty tiers (NG+1 through NG+5) while carrying over character progression.

**Difficulty Tiers:**

```csharp
public enum NewGamePlusTier
{
    None = 0,           // First playthrough
    PlusOne = 1,        // +50% difficulty
    PlusTwo = 2,        // +100% difficulty
    PlusThree = 3,      // +150% difficulty
    PlusFour = 4,       // +200% difficulty
    PlusFive = 5        // +250% difficulty (maximum)
}

public class NewGamePlusScaling
{
    public static float GetDifficultyMultiplier(NewGamePlusTier tier)
    {
        return 1.0f + ((int)tier * 0.5f);
    }
    
    public static int GetLevelIncrease(NewGamePlusTier tier)
    {
        return (int)tier * 2;  // +2 levels per tier
    }
}
```

**Scaling Formula:**

| NG+ Tier | Enemy HP | Enemy Damage | Enemy Level | Boss Phases | Corruption Rate |
| --- | --- | --- | --- | --- | --- |
| Normal | 100% | 100% | Base | Normal | 100% |
| NG+1 | 150% | 150% | Base +2 | -10% HP per phase | 125% |
| NG+2 | 200% | 200% | Base +4 | -20% HP per phase | 150% |
| NG+3 | 250% | 250% | Base +6 | -30% HP per phase | 175% |
| NG+4 | 300% | 300% | Base +8 | -40% HP per phase | 200% |
| NG+5 | 350% | 350% | Base +10 | -50% HP per phase | 225% |

**Progression Carryover:**

✅ **Retained Across New Game+:**

- Character level and Legend
- Progression Points (spent and unspent)
- Specialization unlocks
- Learned abilities
- Equipment inventory (weapons, armor, accessories)
- Crafting materials and consumables
- Currency (Scrap)
- Unlocked recipes and crafting stations

❌ **Reset on New Game+:**

- Quest progression (all quests reset)
- World state (sectors regenerate)
- Quest-specific items (removed from inventory)
- Temporary buffs (removed)
- Active Traumas (removed—fresh psychological slate)
- Corruption (reset to 0)

**Service Implementation:**

```csharp
public class NewGamePlusService
{
    private readonly ILogger<NewGamePlusService> _logger;
    private readonly ICharacterRepository _characterRepo;
    private readonly IWorldStateRepository _worldStateRepo;
    
    public async Task<Result> InitializeNewGamePlus(
        int characterId,
        NewGamePlusTier targetTier)
    {
        _logger.Information(
            "Initializing New Game+: CharacterID={CharacterID}, TargetTier={Tier}",
            characterId, targetTier);
        
        // Validate prerequisites
        var character = await _characterRepo.GetByIdAsync(characterId);
        if (!character.HasCompletedCampaign)
        {
            return Result.Failure("Campaign must be completed before New Game+");
        }
        
        if (targetTier > character.HighestNewGamePlusTier + 1)
        {
            return Result.Failure(
                $"Must complete NG+{character.HighestNewGamePlusTier} before attempting NG+{targetTier}");
        }
        
        // Snapshot carryover data
        var carryoverData = CreateCarryoverSnapshot(character);
        
        // Reset world state
        await _worldStateRepo.ResetForNewGamePlusAsync(characterId);
        
        // Apply carryover
        await ApplyCarryoverDataAsync(characterId, carryoverData);
        
        // Set difficulty tier
        character.CurrentNewGamePlusTier = targetTier;
        await _characterRepo.UpdateAsync(character);
        
        _logger.Information(
            "New Game+ initialized: CharacterID={CharacterID}, Tier={Tier}",
            characterId, targetTier);
        
        return Result.Success();
    }
}
```

### FR-002: Challenge Sectors

**Requirement**: Players shall access 20-30 handcrafted Challenge Sectors with extreme difficulty modifiers and unique rewards.

**Challenge Modifier Categories:**

```csharp
public enum ChallengeModifierType
{
    CombatModifier,        // Affects combat mechanics
    ResourceModifier,      // Affects resources and economy
    EnvironmentalModifier, // Affects hazards and environment
    PsychologicalModifier, // Affects Trauma Economy
    SpecializationModifier // Restricts or enhances archetypes
}

public class ChallengeModifier
{
    public string ModifierId { get; set; }
    public string Name { get; set; }
    public ChallengeModifierType Type { get; set; }
    public string Description { get; set; }
    public float DifficultyMultiplier { get; set; }  // 1.0 = baseline
    public Dictionary<string, object> Parameters { get; set; }
}
```

**Example Challenge Sectors:**

| Challenge Sector | Modifiers | Difficulty | Unique Reward |
| --- | --- | --- | --- |
| "The Silence Falls" | [Psychic Dampening], [Forlorn Surge], [The Great Silence] | 2.5x | Forlorn Echo Relic |
| "Blood Price" | [No Healing], [Permadeath Rooms], [Double Loot] | 3.0x | Bloodforged Armor Set |
| "Runic Instability" | [Wild Magic], [Aether Flux], [Reality Tears] | 2.0x | Chaos Rune |
| "Iron Gauntlet" | [Jötun-Forged Only], [Triple HP], [Lava Floors] | 3.5x | Jötun-Slayer Title |
| "Speedrun" | [20 Turn Limit], [Boss Rush], [No Rest] | 2.5x | Chrono Crystal |

**20+ Challenge Modifiers:**

**Combat Modifiers:**

1. **[No Healing]**: Healing abilities and items have no effect
2. **[Permadeath Rooms]**: Death in this room = character deleted
3. **[Boss Rush]**: Every room has a boss-tier enemy
4. **[One-Hit Wonder]**: All attacks deal 1 damage, rely on status effects
5. **[Berserk Mode]**: Cannot use defensive abilities or stances

**Resource Modifiers:**

1. **[Zero Loot]**: No loot drops, rewards only at end
2. **[Double Corruption]**: Corruption gains doubled
3. **[Stamina Drain]**: Stamina regeneration halved
4. **[Aether Drought]**: No Aether regeneration (Mystics challenged)
5. **[Resource Scarcity]**: Start with 50% resources

**Environmental Modifiers:**

1. **[Lava Floors]**: All tiles deal fire damage per turn
2. **[Frozen Wasteland]**: Movement costs doubled
3. **[Reality Tears]**: Random Aetheric damage per turn
4. **[Glitched Grid]**: Grid tiles randomize each turn
5. **[Total Darkness]**: Vision range reduced to 1 tile

**Psychological Modifiers:**

1. **[The Great Silence]**: +2 Psychic Stress per turn (ambient)
2. **[Breaking Point]**: Start with Stress at 80
3. **[Trauma Amplification]**: Trauma effects doubled
4. **[Forlorn Surge]**: +100% Forlorn enemy spawn rate
5. **[Madness Cascade]**: Corruption threshold at 50 instead of 100

**Challenge Sector Structure:**

```csharp
public class ChallengeSector
{
    public string SectorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ChallengeModifier> Modifiers { get; set; }
    public float TotalDifficultyMultiplier { get; set; }
    public string UniqueRewardId { get; set; }
    public int RequiredNewGamePlusTier { get; set; }  // 0-5
    public bool IsCompleted { get; set; }
}
```

### FR-003: Boss Gauntlet Mode

**Requirement**: Players shall face 8-10 sequential boss encounters with no rest and limited resources.

**Gauntlet Structure:**

```jsx
Boss Gauntlet: "Trial of the Forsaken"

Sequence (8 Bosses):
1. Warming Up: Blighted Champion (Tier 2 boss)
2. First Test: Forge-Master Thrain (Fire boss)
3. Skill Check: Frost-Warden Ymir (Ice boss)
4. Pressure: Reality-Torn Anomaly (Aetheric boss)
5. Endurance: Haugbui-Class Automaton (Undying boss)
6. Crisis: Twin Draugr-Patterns (Double boss)
7. Breaking Point: Surtur's Herald (NG+2 difficulty)
8. Final Trial: The Silenced One (Unique endgame boss)

Rules:
- No rest between bosses (5-minute break for player)
- Limited healing: 3 full heals total across all 8 bosses
- Resource reset: Stamina/Aether refill between bosses
- Death = run over, restart from boss 1
- Victory = Gauntlet Champion title + unique legendary
```

**Resource Limitation System:**

```csharp
public class BossGauntletRun
{
    public int RunId { get; set; }
    public int CharacterId { get; set; }
    public DateTime StartTime { get; set; }
    public int CurrentBossIndex { get; set; }  // 0-7
    
    // Resource tracking
    public int RemainingFullHeals { get; set; } = 3;
    public int RemainingRevives { get; set; } = 1;
    public List<string> UsedConsumables { get; set; } = new();
    
    // Performance tracking
    public TimeSpan ElapsedTime { get; set; }
    public int TotalDamageTaken { get; set; }
    public int TotalDamageDealt { get; set; }
    public bool IsCompleted { get; set; }
}
```

**Gauntlet Leaderboard:**

| Rank | Player | Time | Damage Taken | Heals Used | Date |
| --- | --- | --- | --- | --- | --- |
| 1 | Raven_Storm | 47:23 | 1,240 | 0 | 2025-11-15 |
| 2 | IronWill_99 | 52:18 | 2,105 | 1 | 2025-11-14 |
| 3 | MysticAsh | 58:44 | 3,420 | 2 | 2025-11-12 |

### FR-004: Endless Mode & Wave System

**Requirement**: Players shall survive infinitely escalating waves with competitive scoring and leaderboards.

**Wave Escalation:**

```csharp
public class EndlessModeWave
{
    public int WaveNumber { get; set; }
    public int EnemyCount { get; set; }
    public int EnemyLevel { get; set; }
    public float DifficultyMultiplier { get; set; }
    
    public static EndlessModeWave Generate(int waveNumber)
    {
        return new EndlessModeWave
        {
            WaveNumber = waveNumber,
            EnemyCount = 3 + (waveNumber / 2),  // Scales slowly
            EnemyLevel = 5 + waveNumber,
            DifficultyMultiplier = 1.0f + (waveNumber * 0.1f)
        };
    }
}
```

**Wave Progression:**

| Wave | Enemy Count | Enemy Level | Difficulty | Special Event |
| --- | --- | --- | --- | --- |
| 1-5 | 3-5 | 6-10 | 1.0x-1.5x | None |
| 6-10 | 6-8 | 11-15 | 1.6x-2.0x | Elite enemy spawns |
| 11-20 | 8-13 | 16-25 | 2.1x-3.0x | Boss wave every 5 |
| 21-30 | 13-18 | 26-35 | 3.1x-4.0x | Double bosses |
| 31+ | 18+ | 36+ | 4.0x+ | Extreme modifiers |

**Scoring System:**

```csharp
public class EndlessModeScore
{
    public int WavesCompleted { get; set; }
    public int EnemiesKilled { get; set; }
    public int BossesKilled { get; set; }
    public TimeSpan TotalTime { get; set; }
    public int DamageTaken { get; set; }
    
    // Score components
    public int WaveScore => WavesCompleted * 1000;
    public int KillScore => EnemiesKilled * 50;
    public int BossScore => BossesKilled * 500;
    public int TimeBonus => Math.Max(0, 10000 - (int)TotalTime.TotalSeconds);
    public int SurvivalBonus => Math.Max(0, 5000 - DamageTaken);
    
    public int TotalScore => WaveScore + KillScore + BossScore + TimeBonus + SurvivalBonus;
}
```

**Endless Mode Leaderboard:**

| Rank | Player | Highest Wave | Total Score | Time | Build |
| --- | --- | --- | --- | --- | --- |
| 1 | EndlessGrind | Wave 47 | 89,420 | 3h 24m | Berserkr/Tank |
| 2 | WaveRunner | Wave 42 | 78,150 | 2h 58m | Seidkona/Support |
| 3 | SurvivalKing | Wave 38 | 71,200 | 2h 45m | Einbui/Survivalist |

### FR-005: Leaderboard System

**Requirement**: Global leaderboards shall track player performance across all endgame modes with seed verification.

**Leaderboard Categories:**

```csharp
public enum LeaderboardCategory
{
    BossGauntletFastestTime,
    BossGauntletLowestDamage,
    BossGauntletNoHeals,
    EndlessModeHighestWave,
    EndlessModeHighestScore,
    ChallengeSectorCompletionPercent,
    NewGamePlusTierReached,
    SpeedrunAnyPercent,
    SpeedrunHundredPercent
}
```

**Seed Verification:**

```csharp
public class LeaderboardEntry
{
    public int EntryId { get; set; }
    public int CharacterId { get; set; }
    public string PlayerName { get; set; }
    public LeaderboardCategory Category { get; set; }
    public float Score { get; set; }
    public DateTime SubmittedAt { get; set; }
    
    // Verification data
    public string RunSeed { get; set; }  // Procedural generation seed
    public string CharacterBuildHash { get; set; }  // Build verification
    public bool IsVerified { get; set; }  // Manual verification flag
    public int ReportCount { get; set; }  // Suspicious reports
    
    // Metadata
    public string ArchetypeName { get; set; }
    public string SpecializationName { get; set; }
    public int CharacterLevel { get; set; }
    public string PlatformVersion { get; set; }  // Game version
}
```

**Anti-Cheat Measures:**

1. **Seed-Based Verification**:
    - All runs use known seeds
    - Community can replay seed to verify legitimacy
    - Suspicious scores flagged for manual review
2. **Build Hash Verification**:
    - Character build hashed and stored
    - Impossible stat combinations flagged
    - Equipment validation against drop tables
3. **Temporal Validation**:
    - Boss Gauntlet cannot complete faster than theoretical minimum
    - Endless Mode wave times validated
    - Impossible clear times rejected
4. **Community Reporting**:
    - Players can report suspicious entries
    - High report count triggers manual review
    - False reports penalize reporter

**Leaderboard UI Display:**

```jsx
=== BOSS GAUNTLET LEADERBOARD ===
[Fastest Clear Time]

Rank | Player          | Time    | Heals | Date
-----|-----------------|---------|-------|------------
1    | Raven_Storm     | 47:23   | 0     | 2025-11-15
2    | IronWill_99     | 52:18   | 1     | 2025-11-14
3    | MysticAsh       | 58:44   | 2     | 2025-11-12
4    | YOU             | 1:03:17 | 2     | 2025-11-10
5    | BossSlayer42    | 1:08:32 | 3     | 2025-11-08

[Filters: This Season | Platform: PC | Archetype: All]

Press [V] to view run details | [R] to report | [F] to filter
```

### FR-006: Endgame Rewards & Progression

**Requirement**: Endgame modes shall provide meaningful rewards that enhance future runs without creating mandatory grinds.

**Reward Types:**

| Mode | Reward Type | Example Rewards | Impact |
| --- | --- | --- | --- |
| New Game+ | Tier-locked equipment | NG+3 Mythic weapons, NG+5 Legendary sets | Build diversity |
| Challenge Sector | Unique legendaries | Forlorn Echo Relic, Chaos Rune, Chrono Crystal | Build-defining items |
| Boss Gauntlet | Titles & cosmetics | "Gauntlet Champion" title, Endgame armor skin | Prestige display |
| Endless Mode | Account unlocks | Wave 20: Alt start, Wave 30: Cosmetic slot, Wave 40: Bonus PP | Meta-progression (v0.41) |

**No Mandatory Grinds**:

- **New Game+ Rewards**: Powerful but not required
    - Can beat NG+5 with NG+0 equipment (hard but possible)
    - Rewards make runs smoother, not mandatory
- **Challenge Sector Rewards**: Build-enabling, not build-requiring
    - Unique legendaries enable new builds
    - Existing builds remain viable
- **Boss Gauntlet Rewards**: Cosmetic prestige
    - Titles and skins have zero gameplay impact
    - Pure bragging rights
- **Endless Mode Rewards**: Optional convenience
    - Account unlocks provide variety, not power
    - Can complete game without touching Endless Mode

---

## VI. Child Specifications Overview

### v0.40.1: New Game+ System (8-12 hours)

**Focus:** Implement difficulty scaling and progression carryover

**Deliverables:**

- New Game+ tier system (5 tiers)
- Difficulty scaling formulas
- Progression carryover logic
- World state reset
- NG+ unlock requirements
- Integration with save system

**Database Changes:**

```sql
ALTER TABLE Characters ADD COLUMN current_ng_plus_tier INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN highest_ng_plus_tier INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN has_completed_campaign BOOLEAN DEFAULT 0;

CREATE TABLE NG_Plus_Carryover (
    carryover_id INTEGER PRIMARY KEY,
    character_id INTEGER,
    ng_plus_tier INTEGER,
    carryover_data TEXT, -- JSON snapshot
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);
```

**Key Services:**

- `NewGamePlusService`: Manages NG+ initialization and scaling
- `DifficultyScalingService`: Applies tier-based multipliers
- `CarryoverService`: Handles progression snapshot and restore

### v0.40.2: Challenge Sectors (8-12 hours)

**Focus:** Handcrafted extreme difficulty challenges with unique modifiers

**Deliverables:**

- Challenge Sector definitions (20-30 sectors)
- Challenge modifier system (20+ modifiers)
- Modifier application and validation
- Unique reward system
- Completion tracking
- Integration with procedural generation

**Database Changes:**

```sql
CREATE TABLE Challenge_Sectors (
    sector_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    difficulty_multiplier REAL,
    required_ng_plus_tier INTEGER DEFAULT 0,
    unique_reward_id TEXT,
    is_active BOOLEAN DEFAULT 1
);

CREATE TABLE Challenge_Modifiers (
    modifier_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    modifier_type TEXT,
    description TEXT,
    difficulty_multiplier REAL,
    parameters TEXT -- JSON
);

CREATE TABLE Challenge_Sector_Modifiers (
    sector_id TEXT,
    modifier_id TEXT,
    PRIMARY KEY (sector_id, modifier_id),
    FOREIGN KEY (sector_id) REFERENCES Challenge_Sectors(sector_id),
    FOREIGN KEY (modifier_id) REFERENCES Challenge_Modifiers(modifier_id)
);

CREATE TABLE Challenge_Completions (
    completion_id INTEGER PRIMARY KEY,
    character_id INTEGER,
    sector_id TEXT,
    completed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id),
    FOREIGN KEY (sector_id) REFERENCES Challenge_Sectors(sector_id)
);
```

**Key Services:**

- `ChallengeSectorService`: Manages sector selection and completion
- `ChallengeModifierService`: Applies and validates modifiers
- `RewardService`: Distributes unique legendary rewards

### v0.40.3: Boss Gauntlet Mode (7-10 hours)

**Focus:** Sequential boss encounters with resource limitations

**Deliverables:**

- Boss Gauntlet sequence definition (8-10 bosses)
- Resource limitation system (heals, revives)
- Run tracking and progression
- Gauntlet-specific leaderboards
- Victory rewards (titles, legendaries)
- Integration with boss encounter system (v0.23)

**Database Changes:**

```sql
CREATE TABLE Boss_Gauntlet_Runs (
    run_id INTEGER PRIMARY KEY,
    character_id INTEGER,
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    current_boss_index INTEGER DEFAULT 0,
    remaining_heals INTEGER DEFAULT 3,
    remaining_revives INTEGER DEFAULT 1,
    total_damage_taken INTEGER DEFAULT 0,
    total_damage_dealt INTEGER DEFAULT 0,
    is_completed BOOLEAN DEFAULT 0,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);

CREATE TABLE Boss_Gauntlet_Sequences (
    sequence_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    boss_order TEXT, -- JSON array of boss IDs
    difficulty_tier INTEGER
);
```

**Key Services:**

- `BossGauntletService`: Manages gauntlet progression
- `ResourceLimitationService`: Enforces heal/revive limits
- `GauntletLeaderboardService`: Tracks fastest clears

### v0.40.4: Endless Mode & Leaderboards (7-11 hours)

**Focus:** Wave-based survival with competitive scoring

**Deliverables:**

- Endless Mode wave generation
- Wave escalation formulas
- Scoring system (kills, time, survival)
- Global leaderboard system
- Seed verification
- Anti-cheat measures
- Seasonal leaderboard resets

**Database Changes:**

```sql
CREATE TABLE Endless_Mode_Runs (
    run_id INTEGER PRIMARY KEY,
    character_id INTEGER,
    seed TEXT NOT NULL,
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    highest_wave INTEGER DEFAULT 1,
    total_kills INTEGER DEFAULT 0,
    total_bosses_killed INTEGER DEFAULT 0,
    total_damage_taken INTEGER DEFAULT 0,
    total_score INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT 1,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);

CREATE TABLE Leaderboards (
    entry_id INTEGER PRIMARY KEY,
    category TEXT NOT NULL,
    character_id INTEGER,
    player_name TEXT,
    score REAL NOT NULL,
    submitted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    run_seed TEXT,
    character_build_hash TEXT,
    is_verified BOOLEAN DEFAULT 0,
    report_count INTEGER DEFAULT 0,
    season_id TEXT,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);

CREATE TABLE Leaderboard_Seasons (
    season_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    start_date DATETIME,
    end_date DATETIME,
    is_active BOOLEAN DEFAULT 1
);
```

**Key Services:**

- `EndlessModeService`: Wave generation and scaling
- `ScoringService`: Calculates total scores
- `LeaderboardService`: Manages rankings and verification
- `SeasonService`: Handles seasonal resets

---

## VII. Integration with Existing Systems

### v0.23 Boss Encounters Integration

**Boss Gauntlet extends v0.23:**

```jsx
OLD Flow (v0.23):
1. Boss encounter triggered in sector
2. Boss phases based on HP thresholds
3. Loot drop on victory
4. Player can rest before next sector

NEW Flow (v0.40.3 Boss Gauntlet):
1. Boss encounter in gauntlet sequence
2. Boss phases (same as v0.23)
3. No loot drop (reward at end of gauntlet)
4. NO REST—next boss immediately [NEW]
5. Resource limitations enforced [NEW]
```

### v0.15 Trauma Economy Integration

**New Game+ affects Trauma Economy:**

```jsx
Corruption Rate Scaling:
- Normal: 100% corruption gain
- NG+1: 125% corruption gain
- NG+2: 150% corruption gain
- NG+3: 175% corruption gain
- NG+4: 200% corruption gain
- NG+5: 225% corruption gain

Challenge Modifiers:
- [Double Corruption]: 200% corruption gain
- [The Great Silence]: +2 Psychic Stress per turn
- [Breaking Point]: Start with 80 Stress
- [Trauma Amplification]: Trauma effects doubled
```

### v0.14 Quest System Integration

**Challenge Sectors as special quest types:**

```csharp
public class ChallengeSectorQuest : Quest
{
    public string ChallengeSectorId { get; set; }
    public List<ChallengeModifier> RequiredModifiers { get; set; }
    
    public override bool IsComplete()
    {
        return _completionService.HasCompletedChallenge(
            CharacterId, 
            ChallengeSectorId);
    }
}
```

### v0.10-v0.12 Dynamic Room Engine Integration

**Challenge Sectors modify procedural generation:**

```jsx
Challenge Modifier → Generation Parameter

[Lava Floors] → All tiles set to [Burning Ground] hazard
[Total Darkness] → Vision range = 1
[Glitched Grid] → Randomize tile types each turn
[Boss Rush] → Replace standard enemies with boss-tier
[Zero Loot] → Disable loot spawning
```

---

## VIII. Success Criteria

**v0.40 is DONE when:**

### ✅ v0.40.1: New Game+ System

- [ ]  5 NG+ tiers implemented (NG+1 through NG+5)
- [ ]  Difficulty scaling formulas working correctly
- [ ]  Progression carryover preserves level, abilities, equipment
- [ ]  World state resets on NG+ initialization
- [ ]  Campaign completion unlocks NG+1
- [ ]  Each NG+ tier unlocks next tier on completion
- [ ]  Integration with save/load system successful

### ✅ v0.40.2: Challenge Sectors

- [ ]  20-30 Challenge Sectors defined and playable
- [ ]  20+ Challenge Modifiers implemented
- [ ]  Modifier application affects gameplay correctly
- [ ]  Unique legendary rewards awarded on completion
- [ ]  Completion tracking persists across sessions
- [ ]  Integration with procedural generation successful
- [ ]  Challenge difficulty feels extreme but fair (playtest validation)

### ✅ v0.40.3: Boss Gauntlet Mode

- [ ]  8-10 boss sequence implemented
- [ ]  Resource limitation system working (3 heals, 1 revive)
- [ ]  No rest between bosses enforced
- [ ]  Gauntlet completion awards unique legendary
- [ ]  Fastest clear time leaderboard operational
- [ ]  Death restarts from boss 1
- [ ]  Integration with v0.23 boss system successful

### ✅ v0.40.4: Endless Mode & Leaderboards

- [ ]  Endless Mode wave generation working
- [ ]  Wave escalation formulas scale correctly
- [ ]  Scoring system calculates accurately
- [ ]  Global leaderboards display top 100
- [ ]  Seed verification prevents impossible scores
- [ ]  Anti-cheat measures flag suspicious entries
- [ ]  Seasonal leaderboard resets functional

### ✅ Quality Gates

- [ ]  80%+ unit test coverage achieved
- [ ]  All integration tests passing
- [ ]  Performance: Endgame mode initialization <500ms
- [ ]  Comprehensive Serilog logging implemented
- [ ]  No critical bugs or crashes
- [ ]  v5.0 setting compliance verified
- [ ]  Complete documentation (architecture, schemas, services)

### ✅ Player Experience Validation

- [ ]  Playtester feedback: "Endgame provides meaningful challenge"
- [ ]  Playtester feedback: "Rewards feel earned, not grindy"
- [ ]  Playtester feedback: "Multiple modes provide variety"
- [ ]  Leaderboards functional and competitive
- [ ]  Clear progression path through NG+ tiers

---

## IX. Timeline & Roadmap

**Phase 1: v0.40.1 - New Game+ System** — 8-12 hours

- Week 1: NG+ tier system and difficulty scaling
- Week 2: Progression carryover and world reset

**Phase 2: v0.40.2 - Challenge Sectors** — 8-12 hours

- Week 3: Challenge modifier system and sector definitions
- Week 4: Unique reward integration and completion tracking

**Phase 3: v0.40.3 - Boss Gauntlet Mode** — 7-10 hours

- Week 5: Gauntlet sequence and resource limitation
- Week 6: Leaderboard integration

**Phase 4: v0.40.4 - Endless Mode & Leaderboards** — 7-11 hours

- Week 7: Wave generation and scoring
- Week 8: Leaderboard system and anti-cheat

**Total: 30-45 hours (4-6 weeks part-time)**

---

## X. Benefits

### For Development

- ✅ **Replayability**: Infinite content from finite systems
- ✅ **Player Retention**: Endgame modes keep players engaged post-campaign
- ✅ **Competitive Scene**: Leaderboards create content creator interest
- ✅ **Build Diversity**: Challenge modifiers reward different builds

### For Gameplay

- ✅ **Mastery Rewarded**: NG+ respects player progression
- ✅ **Variety**: 4 distinct endgame modes for different playstyles
- ✅ **Fair Challenge**: Extreme difficulty without artificial limits
- ✅ **Prestige**: Leaderboards and titles provide bragging rights

### For Replayability

- ✅ **Multiple Goals**: NG+5, Challenge completion, Gauntlet clear, Wave 50
- ✅ **Build Testing**: Challenge Sectors test specific archetypes
- ✅ **Competitive Drive**: Leaderboards motivate optimization
- ✅ **Seasonal Content**: Seasonal resets keep competition fresh

---

## XI. After v0.40 Ships

**You'll Have:**

- ✅ Complete endgame progression (NG+1 through NG+5)
- ✅ 20-30 extreme Challenge Sectors
- ✅ Boss Gauntlet skill test with leaderboards
- ✅ Endless Mode with competitive scoring
- ✅ Global leaderboards with seed verification
- ✅ Foundation for account-wide progression (v0.41)

**Next Steps:**

- **v0.41:** Meta-Progression & Unlocks (account-wide unlocks, achievements, cosmetics)
- **v0.42:** Seasonal Events (time-limited challenges and rewards)
- **v0.43:** Custom Difficulty (player-created challenges)

**The endgame transforms from single completion to infinite replayability.**

---

**Ready to challenge the masters.**