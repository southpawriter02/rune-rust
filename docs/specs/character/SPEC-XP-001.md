---
id: SPEC-XP-001
title: Experience & Leveling System
version: 1.0.0
status: Scaffolded
last_updated: 2025-12-23
related_specs: [SPEC-COMBAT-001, SPEC-CHAR-001, SPEC-ADVANCEMENT-001]
---

# SPEC-XP-001: Experience & Leveling System

> **Version:** 1.0.0
> **Status:** Scaffolded (Properties exist, logic incomplete)
> **Service:** None (planned: `IProgressionService`)
> **Location:** `RuneAndRust.Core/Entities/Character.cs`

---

## Overview

The Experience & Leveling System tracks character progression through combat victories and awards experience points (XP) that accumulate toward level thresholds. When thresholds are crossed, characters advance from Level 1 to a maximum of Level 5, unlocking attribute increases and ability tiers.

This system provides vertical progression within a capped framework, emphasizing horizontal advancement through equipment and abilities rather than endless level grinding.

---

## Core Concepts

### Experience Points (XP)

**Definition:** Numerical measure of character progress earned through combat victories.

**Current Implementation:**
- Stored in `Character.ExperiencePoints` (integer, default: 0)
- Awarded by `CombatService.EndCombat()` via `CombatResult.XpEarned`
- **Not currently applied to character** (calculated but discarded)

**XP Sources (Current):**
| Source | XP Award | Status |
|--------|----------|--------|
| Combat Victory | 50 (flat) | Implemented (placeholder) |
| Enemy Kills | Varies by enemy | NOT IMPLEMENTED |
| Exploration | Varies | NOT IMPLEMENTED |
| Quests | Varies | NOT IMPLEMENTED |

---

### Level Progression

**Level Range:** 1 to 5 (hard cap)
- **Level 1:** Starting level (0 XP)
- **Level 2:** 100 XP cumulative
- **Level 3:** 400 XP cumulative (300 XP from Level 2)
- **Level 4:** 1000 XP cumulative (600 XP from Level 3)
- **Level 5:** 2000 XP cumulative (1000 XP from Level 4)

**Level Storage:**
- `Character.Level` (integer, default: 1)
- Persisted to PostgreSQL `Characters` table

**Design Philosophy:**
- Low level cap encourages mastery over grinding
- Focus shifts to horizontal progression (equipment, abilities, traits)
- Level 5 represents experienced scavenger, not demigod

---

## Behaviors

### Primary Behaviors

#### 1. Award Experience (`AwardExperience`) - NOT IMPLEMENTED

```csharp
void AwardExperience(Character character, int xpAmount)
```

**Planned Sequence:**
1. Add `xpAmount` to `Character.ExperiencePoints`
2. Log XP gain event
3. Call `CheckLevelUp(character)`
4. Persist character state

**Example:**
```csharp
// Player defeats enemies, earns 50 XP
_progressionService.AwardExperience(character, 50);
// Character.ExperiencePoints: 0 → 50
```

---

#### 2. Check Level Up (`CheckLevelUp`) - NOT IMPLEMENTED

```csharp
bool CheckLevelUp(Character character)
```

**Planned Sequence:**
1. Get current XP threshold from level table
2. Compare `Character.ExperiencePoints` to threshold
3. If XP >= threshold:
   - Return `true`
   - Trigger level-up event
4. Else return `false`

**Example:**
```csharp
// Character at Level 1 with 120 XP
var shouldLevelUp = _progressionService.CheckLevelUp(character);
// Returns: true (120 >= 100 threshold for Level 2)
```

---

#### 3. Get XP For Level (`GetXpForLevel`) - NOT IMPLEMENTED

```csharp
int GetXpForLevel(int targetLevel)
```

**Returns:** Cumulative XP required to reach target level.

**Level Thresholds:**
```csharp
return targetLevel switch
{
    1 => 0,
    2 => 100,
    3 => 400,
    4 => 1000,
    5 => 2000,
    _ => int.MaxValue  // Prevents over-leveling
};
```

**Example:**
```csharp
var xpNeeded = _progressionService.GetXpForLevel(3);
// Returns: 400
```

---

#### 4. Calculate XP Reward (`CalculateXpReward`) - PARTIALLY IMPLEMENTED

```csharp
int CalculateXpReward(List<Combatant> defeatedEnemies)
```

**Current Implementation (CombatService.cs:573-580):**
```csharp
var character = _gameState.CurrentCharacter;
var witsBonus = character?.GetEffectiveAttribute(CharacterAttribute.Wits) ?? 0;

// Placeholder XP calculation (will be expanded with enemy XP values)
xp = 50;  // HARDCODED
```

**Planned Formula:**
```csharp
int totalXp = 0;
foreach (var enemy in defeatedEnemies)
{
    int baseXp = enemy.Template.XpValue;  // NOT YET IN ENEMY ENTITY
    int witsMultiplier = character.GetEffectiveAttribute(Attribute.Wits);
    int xpBonus = (int)(baseXp * (witsMultiplier * 0.05f));  // 5% per WITS
    totalXp += baseXp + xpBonus;
}
return totalXp;
```

**Proposed Enemy XP Values:**
| Enemy Tier | Base XP |
|------------|---------|
| Swarm (Blight-Rat) | 10 |
| Standard (Iron-Husk) | 25 |
| Elite (1 trait) | 50 |
| Champion (2 traits) | 100 |
| Boss (3 traits) | 200 |

---

## Restrictions

### Level Cap

**Hard Cap:** Level 5
- Prevents power creep
- Encourages strategic mastery over stat stacking
- Focus on horizontal progression (equipment, abilities)

**Enforcement:**
```csharp
if (character.Level >= 5)
{
    return false;  // Cannot level beyond 5
}
```

---

### XP Gain Limits

**Single-Source Cap:** None (if you defeat 10 enemies in one combat, you earn XP for all)

**Negative XP:** NOT ALLOWED
- XP cannot decrease (no death penalties to XP)
- Minimum XP: 0

---

## Limitations

### Numerical Bounds

| Constraint | Value | Notes |
|------------|-------|-------|
| Min Level | 1 | Starting level |
| Max Level | 5 | Hard cap |
| Min XP | 0 | Cannot go negative |
| Max XP | Unbounded | Can exceed Level 5 threshold (no benefit) |
| XP for Level 2 | 100 | Cumulative |
| XP for Level 5 | 2000 | Cumulative |

---

### System Gaps

- **No enemy XP values** - `EnemyTemplate` lacks `XpValue` property
- **No XP application** - `CombatResult.XpEarned` is calculated but never applied to character
- **No level-up detection** - No service checks XP thresholds
- **No non-combat XP** - Only combat awards XP (no exploration, discovery, quests)
- **No XP scaling** - Flat 50 XP per combat regardless of difficulty
- **No WITS bonus integration** - Formula exists but unused

---

## Use Cases

### UC-1: First Combat Victory (Level 1 → 2)

```csharp
// Character starts at Level 1, 0 XP
var character = _characterFactory.Create(Archetype.Ranger, Lineage.Human);

// Combat ends
var result = _combatService.EndCombat();
// CombatResult.XpEarned = 50

// PLANNED (not implemented):
_progressionService.AwardExperience(character, result.XpEarned);
// Character.ExperiencePoints: 0 → 50

// Second combat
var result2 = _combatService.EndCombat();
_progressionService.AwardExperience(character, result2.XpEarned);
// Character.ExperiencePoints: 50 → 100

// Check level-up
if (_progressionService.CheckLevelUp(character))
{
    _advancementService.ProcessLevelUp(character);
    // Character.Level: 1 → 2
    // Full heal, +10 MaxHP, +5 MaxStamina, +1 attribute point
}
```

---

### UC-2: WITS-Based XP Scaling (Planned)

```csharp
// Character with WITS 8 defeats 3 enemies
var defeatedEnemies = new List<Combatant>
{
    new Combatant { Template = new EnemyTemplate { XpValue = 25 } },  // Standard
    new Combatant { Template = new EnemyTemplate { XpValue = 25 } },
    new Combatant { Template = new EnemyTemplate { XpValue = 50 } }   // Elite
};

var xp = _progressionService.CalculateXpReward(defeatedEnemies);
// Base XP: 25 + 25 + 50 = 100
// WITS Bonus: 100 × (8 × 0.05) = 100 × 0.40 = 40
// Total XP: 100 + 40 = 140

_progressionService.AwardExperience(character, xp);
```

**Narrative Impact:** High-WITS characters (investigators, scholars) learn faster from combat encounters.

---

### UC-3: Level Cap Enforcement

```csharp
// Character at Level 5 with 2500 XP (500 beyond cap)
var character = new Character { Level = 5, ExperiencePoints = 2500 };

// Defeat another enemy
var result = _combatService.EndCombat();
_progressionService.AwardExperience(character, result.XpEarned);
// Character.ExperiencePoints: 2500 → 2550
// Character.Level: 5 (unchanged)

var canLevel = _progressionService.CheckLevelUp(character);
// Returns: false (already at max level)
```

**Narrative Impact:** Players focus on equipment optimization and ability mastery, not grinding for XP.

---

### UC-4: Multi-Enemy Combat XP

```csharp
// Combat with 5 Swarm enemies (Blight-Rats)
var defeatedEnemies = new List<Combatant>
{
    // Each Swarm enemy: 10 XP base
    new Combatant { Template = new EnemyTemplate { XpValue = 10 } },
    new Combatant { Template = new EnemyTemplate { XpValue = 10 } },
    new Combatant { Template = new EnemyTemplate { XpValue = 10 } },
    new Combatant { Template = new EnemyTemplate { XpValue = 10 } },
    new Combatant { Template = new EnemyTemplate { XpValue = 10 } }
};

var xp = _progressionService.CalculateXpReward(defeatedEnemies);
// Base XP: 10 × 5 = 50
// WITS Bonus (WITS 5): 50 × 0.25 = 12.5 → 12
// Total XP: 62

_progressionService.AwardExperience(character, xp);
```

---

### UC-5: Boss Victory XP Surge

```csharp
// Defeat boss with 2 Standard minions
var defeatedEnemies = new List<Combatant>
{
    new Combatant { Template = new EnemyTemplate { XpValue = 200 } },  // Boss
    new Combatant { Template = new EnemyTemplate { XpValue = 25 } },   // Minion
    new Combatant { Template = new EnemyTemplate { XpValue = 25 } }    // Minion
};

var xp = _progressionService.CalculateXpReward(defeatedEnemies);
// Base XP: 200 + 25 + 25 = 250
// WITS Bonus (WITS 6): 250 × 0.30 = 75
// Total XP: 325

_progressionService.AwardExperience(character, xp);
// Likely triggers multiple level-ups if low-level character
```

**Narrative Impact:** Boss victories provide significant progression surges, rewarding risk-taking.

---

## Decision Trees

### XP Award Flow (Planned)

```
┌─────────────────────────────────┐
│  Combat Ends (Victory)          │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ Calculate XP    │
    │ from enemies    │
    └────────┬────────┘
             │
    ┌────────┴────────────┐
    │ Sum Base XP Values  │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Apply WITS Bonus    │
    │ (+5% per point)     │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Award XP to Char    │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Check XP Threshold  │
    └────────┬────────────┘
         ┌───┴───┐
         │       │
       YES      NO
         │       │
         │       └─────> End
         │
         ▼
    ┌────────────┐
    │ Trigger    │
    │ Level-Up   │
    └────────────┘
```

---

### Level-Up Threshold Check

```
┌─────────────────────────────────┐
│  CheckLevelUp(character)        │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ Get Current XP  │
    │ Get Current Lvl │
    └────────┬────────┘
             │
    ┌────────┴──────────┐
    │ Level >= 5?       │
    └────────┬──────────┘
         ┌───┴───┐
         │       │
        YES     NO
         │       │
         │       └───> Get Next Level Threshold
         │                    │
         └─────────────> Return false
                              │
                    ┌─────────┴─────────┐
                    │ XP >= Threshold?  │
                    └─────────┬─────────┘
                          ┌───┴───┐
                          │       │
                         YES     NO
                          │       │
                          │       └─> Return false
                          │
                          ▼
                     Return true
                     (Trigger Level-Up)
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IRepository<Character>` | Infrastructure | Persist XP and level to database |
| `ILogger` | Infrastructure | XP gain and level-up event tracing |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Calls `AwardExperience()` after combat victory |
| `CharacterAdvancementService` | [SPEC-ADVANCEMENT-001](SPEC-ADVANCEMENT-001.md) | Triggers level-up processing when threshold crossed |

### Related Systems

- [SPEC-COMBAT-001](SPEC-COMBAT-001.md) - **Combat Orchestration**: Awards XP via `CombatResult.XpEarned`
- [SPEC-CHAR-001](SPEC-CHAR-001.md) - **Character System**: Defines `ExperiencePoints` and `Level` properties
- [SPEC-ADVANCEMENT-001](SPEC-ADVANCEMENT-001.md) - **Character Advancement**: Consumes level-up triggers to apply stat bonuses

---

## Related Services

### Primary Implementation (Planned)

| File | Purpose |
|------|----------|
| `ProgressionService.cs` (NOT YET CREATED) | XP award, threshold checking, level-up triggers |

### Supporting Types

| File | Purpose |
|------|----------|
| `Character.cs` | XP and Level property storage |
| `CombatResult.cs` | XP award transport object |

---

## Data Models

### Character Entity (Relevant Fields)

```csharp
public class Character
{
    /// <summary>
    /// Experience points accumulated by the character.
    /// </summary>
    public int ExperiencePoints { get; set; } = 0;

    /// <summary>
    /// Character's current level. Starts at 1, max 5.
    /// </summary>
    public int Level { get; set; } = 1;
}
```

### CombatResult Record

```csharp
public record CombatResult(
    bool Victory,
    int XpEarned,          // XP awarded from this combat
    List<Item> LootFound,
    string Summary
);
```

### EnemyTemplate (Planned Addition)

```csharp
public class EnemyTemplate
{
    // Existing properties...

    /// <summary>
    /// Base XP value awarded for defeating this enemy.
    /// Scales with enemy tier (Swarm: 10, Standard: 25, Boss: 200).
    /// </summary>
    public int XpValue { get; set; } = 25;  // PLANNED, NOT YET ADDED
}
```

---

## Configuration

### Level Thresholds

**Hardcoded Progression Curve:**
```csharp
public static class LevelThresholds
{
    public static readonly Dictionary<int, int> CumulativeXP = new()
    {
        { 1, 0 },
        { 2, 100 },
        { 3, 400 },
        { 4, 1000 },
        { 5, 2000 }
    };

    public static int GetXpForLevel(int level)
    {
        return CumulativeXP.TryGetValue(level, out var xp) ? xp : int.MaxValue;
    }

    public static int GetXpToNextLevel(int currentLevel, int currentXp)
    {
        if (currentLevel >= 5) return 0;  // Already at cap
        var nextLevelXp = CumulativeXP[currentLevel + 1];
        return Math.Max(0, nextLevelXp - currentXp);
    }
}
```

---

### WITS Bonus Formula

**XP Scaling:**
```csharp
public const float WitsXpMultiplierPerPoint = 0.05f;  // 5% per WITS point

public int CalculateWitsBonus(int baseXp, int witsAttribute)
{
    float multiplier = witsAttribute * WitsXpMultiplierPerPoint;
    return (int)(baseXp * multiplier);
}
```

**Example Scaling:**
| WITS | Multiplier | 100 Base XP → Bonus |
|------|------------|---------------------|
| 1 | 5% | 100 → 105 |
| 4 | 20% | 100 → 120 |
| 7 | 35% | 100 → 135 |
| 10 | 50% | 100 → 150 |

---

## Testing

### Test Coverage (Planned)

`ProgressionServiceTests.cs` (NOT YET CREATED) should cover:

1. **XP Award Tests** (5 tests)
   - Award XP to character at Level 1
   - Award XP when already above threshold (delayed level-up)
   - Award XP to Level 5 character (still adds XP, no level change)
   - Award negative XP (should fail or clamp to 0)
   - Award XP to null character (should throw)

2. **Threshold Check Tests** (6 tests)
   - CheckLevelUp returns true when XP >= threshold
   - CheckLevelUp returns false when XP < threshold
   - CheckLevelUp returns false at Level 5 (max cap)
   - CheckLevelUp handles exact threshold match (100 XP for Level 2)
   - Multiple levels at once (0 XP → 1000 XP awarded)
   - GetXpForLevel returns correct cumulative values

3. **XP Calculation Tests** (7 tests)
   - CalculateXpReward with single enemy
   - CalculateXpReward with multiple enemies
   - CalculateXpReward with WITS bonus (WITS 6 = +30%)
   - CalculateXpReward with WITS 1 (5% bonus)
   - CalculateXpReward with WITS 10 (50% bonus)
   - CalculateXpReward with mixed enemy tiers (Swarm + Boss)
   - CalculateXpReward with zero WITS (0% bonus)

4. **Level Threshold Tests** (3 tests)
   - GetXpToNextLevel at Level 1 with 0 XP (returns 100)
   - GetXpToNextLevel at Level 4 with 1500 XP (returns 500)
   - GetXpToNextLevel at Level 5 (returns 0, already max)

---

## Design Rationale

### Why 5 Level Cap?

**Decision:** Hard cap at Level 5 instead of traditional 20+ levels.

**Rationale:**
- **Focus on horizontal progression:** Equipment, abilities, and traits provide variety without stat inflation
- **Prevents power creep:** Enemies remain challenging without requiring exponential HP/damage scaling
- **Respects player time:** Progression feels meaningful without requiring 100+ hour grinds
- **Narrative consistency:** Scavengers are mortal, not ascending demigods

**Alternative Considered:** No levels (only equipment). Rejected because:
- Players expect tangible progression milestones
- Level-up moments provide dopamine hits and natural save points
- Ability tier gating needs a progression metric

---

### Why WITS Scales XP?

**Decision:** WITS attribute provides XP bonus (5% per point, up to 50%).

**Rationale:**
- **Thematic fit:** WITS represents learning capacity, pattern recognition
- **Attribute value:** Encourages builds beyond pure combat stats (STU/FIN)
- **Catch-up mechanic:** High-WITS characters level faster, reducing grind for alt characters
- **Player agency:** Choosing to invest in WITS has tangible progression benefit

**Balance:** 50% bonus at WITS 10 is significant but not game-breaking (150 XP vs 100 XP).

---

### Why Cumulative Thresholds?

**Decision:** Each level requires cumulative XP (Level 3 = 400 total, not 300 from Level 2).

**Rationale:**
- **Simplifies checks:** `if (character.ExperiencePoints >= 400)` vs tracking "XP toward next level"
- **No rollover complexity:** Excess XP naturally carries into next threshold
- **Standard RPG pattern:** Familiar to players (D&D, most CRPGs use cumulative)

**Alternative Considered:** "XP to next level" resets on level-up. Rejected for added complexity.

---

### Why No Non-Combat XP?

**Decision:** Currently only combat awards XP (no exploration, discovery, quests).

**Rationale:**
- **Scope management:** Quest system not yet implemented
- **Clear feedback:** Players immediately see XP after combat
- **Combat-focused design:** Rune & Rust emphasizes tactical combat over sandbox exploration

**Future Enhancement:** Could add XP for:
- Discovering new rooms (5 XP)
- Unlocking Codex entries (10-30 XP based on fragment quality)
- Completing quests (varies)

---

## Changelog

### v1.0.0 (Current - Scaffolded)

**Implemented:**
- `Character.ExperiencePoints` property (persisted to database)
- `Character.Level` property (persisted to database)
- `CombatResult.XpEarned` calculation (hardcoded 50 XP)
- XP display in VictoryScreenRenderer UI

**NOT Implemented:**
- XP application to character (calculated but discarded)
- Level threshold checking
- Level-up triggers
- WITS-based XP scaling (formula exists in comments but unused)
- Enemy-specific XP values (`EnemyTemplate.XpValue` property missing)
- ProgressionService or equivalent service class

**Design Decisions:**
- 5 level cap chosen for horizontal progression focus
- Cumulative XP thresholds (100/400/1000/2000)
- WITS scaling formula (5% per point) documented but not implemented

**Integration Points:**
- `CombatService.EndCombat()` returns XP in result object (Line 573-594)
- `VictoryScreenRenderer` displays earned XP (not yet functional for character advancement)

---

## Future Enhancements

### Enemy-Specific XP Values

**Concept:** Replace hardcoded 50 XP with per-enemy values based on tier.

**Implementation:**
```csharp
// Add to EnemyTemplate.cs
public int XpValue { get; set; } = 25;

// Seed XP values during enemy creation
var swarmTemplate = new EnemyTemplate
{
    Name = "Blight-Rat",
    Archetype = EnemyArchetype.Swarm,
    XpValue = 10  // Low XP for weak enemy
};

var bossTemplate = new EnemyTemplate
{
    Name = "Ancient Guardian",
    Archetype = EnemyArchetype.Boss,
    XpValue = 200  // High XP for boss
};
```

**Benefit:** Combat difficulty matches XP reward, incentivizing challenging encounters.

---

### Non-Combat XP Sources

**Concept:** Award XP for exploration, discovery, and quest completion.

**Examples:**
```csharp
// Discover new room
_progressionService.AwardExperience(character, 5);

// Unlock Codex entry (from SPEC-CODEX-001)
var entryXp = entry.DataCaptures.Sum(dc => dc.Quality);  // 15-30 XP per fragment
_progressionService.AwardExperience(character, entryXp);

// Complete quest
_progressionService.AwardExperience(character, quest.XpReward);
```

**Benefit:** Rewards diverse playstyles (explorers, lore hunters, completionists).

---

### XP Multipliers & Events

**Concept:** Temporary XP boosts from consumables, conditions, or events.

**Examples:**
```csharp
// Consume "Ancient Tome" item
character.ApplyTemporaryEffect(new XpMultiplierEffect(1.5f, durationTurns: 10));

// Sanctuary blessing
if (character.InSanctuary)
{
    xpMultiplier = 1.2f;  // +20% XP while resting
}

// First victory of the day
if (IsFirstVictoryToday())
{
    xpMultiplier = 2.0f;  // Double XP
}
```

**Benefit:** Creates strategic resource management (when to use boosts).

---

### Retroactive Level-Ups

**Concept:** Award XP even when above threshold, process multiple levels at once.

**Implementation:**
```csharp
public void AwardExperience(Character character, int xp)
{
    character.ExperiencePoints += xp;

    // Process all pending level-ups
    while (CheckLevelUp(character) && character.Level < 5)
    {
        _advancementService.ProcessLevelUp(character);
    }
}
```

**Benefit:** Prevents "lost XP" if player defeats boss while already at threshold.

---

## AAM-VOICE Compliance

This specification describes mechanical systems and is exempt from Domain 4 constraints. In-game XP gain narration (if implemented) must follow AAM-VOICE guidelines:

**Compliant Example:**
```
You sense a growing understanding of the ruins' corrupted denizens. [+50 XP]
Your knowledge expands through hard-won survival. [Level 2 achieved]
```

**Non-Compliant Example:**
```
System: XP gained: 50. Total XP: 150/400 to Level 3. [Layer 4 technical bleed]
```
