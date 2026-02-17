---
id: SPEC-AUDIT-001
title: Audit Framework
version: 1.0.0
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-LOOT-001, SPEC-COMBAT-001]
---

# SPEC-AUDIT-001: Audit Framework

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `LootAuditService`, `CombatAuditService`
> **Location:** `RuneAndRust.Engine/Services/LootAuditService.cs`, `RuneAndRust.Engine/Services/CombatAuditService.cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

The **Audit Framework** provides Monte Carlo simulation tools for validating game economy and combat balance. Developers can run thousands of loot generation cycles or combat encounters instantly to ensure drop rates and win rates match design intentions before players encounter them.

### Key Responsibilities

1. **Monte Carlo Simulation**: Run N iterations of game systems, accumulate statistics
2. **Variance Analysis**: Compare actual vs expected rates, classify severity (OK/Warning/Critical)
3. **Report Generation**: Produce human-readable markdown reports with tables and anomaly sections
4. **CLI Integration**: Developer-accessible via command-line flags with parameter parsing

### Architecture Pattern

```
CLI Trigger → Configuration → Simulation Loop → Statistics Accumulation
                                                        ↓
Report Output ← Variance Analysis ← Accumulated Statistics
```

**Key Design Decision**: Sequential execution is mandatory for thread safety. `GameState` and `Random` are not thread-safe, so parallel execution would cause race conditions.

**Technology Stack**:
- **DI Scope Management**: Fresh scope per iteration for isolated state
- **Statistics Models**: `LootStatistics`, `CombatStatistics` accumulators
- **Report Format**: Markdown with tables, headers, anomaly sections

---

## Core Concepts

### 1. Monte Carlo Simulation Pattern

**Definition**: Run N iterations of a game system, record outcomes, and analyze statistical distributions.

**Application**:
- Loot Audit: Generate loot N times, count quality tiers and item types
- Combat Audit: Simulate N battles, track wins, rounds, damage, resources

**Benefits**:
- Detect imbalances before players encounter them
- Validate that probabilities match design specifications
- Identify edge cases and outliers

---

### 2. Variance Flagging System

**Definition**: Compare observed percentages against expected values, classify deviations by severity.

**Loot Audit Thresholds**:
| Severity | Threshold | Meaning |
|----------|-----------|---------|
| OK | ≤ 1% | Within acceptable statistical variance |
| Warning | 1% - 5% | Notable deviation, warrants attention |
| Critical | > 5% | Significant imbalance, requires action |

**Combat Audit Thresholds**:
| Severity | Threshold | Meaning |
|----------|-----------|---------|
| OK | Within bounds | Metric is within expected min/max range |
| Warning | 5% outside bounds | Moderate deviation from expected range |
| Critical | > 15% outside bounds | Severe imbalance requiring immediate attention |

---

### 3. Statistics Accumulator Pattern

**Definition**: Model classes that aggregate metrics across iterations with derived calculations.

**LootStatistics Tracking**:
- Total iterations, items dropped, Scrip value, weight
- Quality tier distribution (Dictionary<QualityTier, int>)
- Item type distribution (Dictionary<ItemType, int>)
- Derived: average Scrip/items per iteration, success rate

**CombatStatistics Tracking**:
- Total encounters, wins/losses, rounds, turns
- Player damage dealt/received, hits/misses
- Enemy damage dealt, hits/misses
- Stamina spent, HP remaining on wins
- Derived: win rate, hit rates, damage per hit, averages

---

### 4. Simulated Player Agent

**Definition**: Heuristic AI that makes "reasonable" player decisions for combat simulation.

**Strategy Priorities**:
1. **Finisher**: Heavy Attack if enemy HP ≤ 2× weapon die (likely kill)
2. **Resource-Aware Attack Selection**:
   - Stamina ≥ 40: Heavy Attack
   - Stamina ≥ 20: Standard Attack
   - Stamina < 20: Light Attack

---

## Behaviors

### Primary Behaviors

#### 1. Run Loot Audit (`ILootAuditService.RunAuditAsync`)

```csharp
Task<LootAuditReport> RunAuditAsync(LootAuditConfiguration config)
```

**Purpose**: Runs a batch loot generation simulation and produces an analysis report.

**Logic**:
1. Initialize `LootStatistics` accumulator
2. Create `LootGenerationContext` from config (biome, danger, WITS)
3. Loop N times:
   - Call `LootService.GenerateLoot(context)`
   - Record result in statistics
   - Log progress every 10%
4. Calculate variance flags for quality tiers and item types
5. Generate markdown report
6. Return `LootAuditReport` with statistics, markdown, and flags

**Example**:
```csharp
var config = new LootAuditConfiguration(10000, BiomeType.Ruin, DangerLevel.Safe, witsBonus: 0);
var report = await _auditService.RunAuditAsync(config);
File.WriteAllText("loot_audit.md", report.MarkdownReport);
```

---

#### 2. Run Combat Audit (`ICombatAuditService.RunAuditAsync`)

```csharp
Task<CombatAuditReport> RunAuditAsync(CombatAuditConfiguration config)
```

**Purpose**: Runs a batch combat simulation and produces an analysis report.

**Logic**:
1. Initialize `CombatStatistics` accumulator
2. Loop N times:
   - Create fresh DI scope
   - Reset GameState, create character and enemy
   - Start combat, create `SimulatedPlayerAgent`
   - Execute headless combat loop until victory/defeat/timeout
   - Record match result
   - Log progress every 10%
3. Calculate variance flags for combat metrics
4. Generate markdown report
5. Return `CombatAuditReport` with statistics, markdown, and flags

**Example**:
```csharp
var config = new CombatAuditConfiguration(1000, ArchetypeType.Warrior, "und_draugr_01", level: 1);
var report = await _auditService.RunAuditAsync(config);
Console.WriteLine($"Win Rate: {report.Statistics.WinRate:F1}%");
```

---

#### 3. Record Loot Result (`LootStatistics.Record`)

```csharp
void Record(LootResult result)
```

**Purpose**: Records a single loot generation cycle into statistics.

**Logic**:
1. Increment TotalIterations
2. If result is successful with items:
   - Increment SuccessfulIterations
   - For each item:
     - Increment TotalItemsDropped
     - Add to TotalScripValue and TotalWeight
     - Increment QualityTierCounts and ItemTypeCounts

---

#### 4. Record Combat Encounter (`CombatStatistics.RecordEncounterResult`)

```csharp
void RecordEncounterResult(CombatMatchResult result)
```

**Purpose**: Records a single combat encounter into statistics.

**Logic**:
1. Increment TotalEncounters
2. Add RoundsElapsed to TotalRounds
3. Add StaminaSpent to TotalPlayerStaminaSpent
4. If player won: increment PlayerWins, add HP remaining
5. If enemy won: increment EnemyWins

---

#### 5. Simulated Player Decision (`SimulatedPlayerAgent.DecideAction`)

```csharp
CombatAction DecideAction(Combatant player, CombatState state)
```

**Purpose**: Determines the best action for a simulated player.

**Logic**:
1. Find valid target (alive, not player)
2. If no target: return Pass action
3. If target HP ≤ 2× weapon die AND stamina ≥ 40: Heavy Attack (finisher)
4. Else select attack type by stamina threshold

---

## Restrictions

### What This System MUST NOT Do

1. **Modify Production Code Paths**: Audit tools must not affect normal gameplay execution
2. **Use Parallel Execution**: Thread safety requires sequential processing
3. **Generate User-Facing Content**: Reports are for developers only
4. **Persist Audit Data**: Reports are written to files, not database

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Max iterations per audit | No hard limit | Performance scales linearly; 10,000 iterations ≈ 500ms (loot) or 2s (combat) |
| Thread safety | Sequential only | GameState singleton and Random are not thread-safe |
| Combat simulation accuracy | ~80% | SimulatedPlayerAgent uses heuristics, not optimal play |
| Ability usage | Not implemented | v0.3.13b only supports basic attacks |
| Multi-enemy encounters | Not supported | 1v1 simulation only |

---

## Use Cases

### UC-1: Validate Loot Drop Rates

**Actor:** Developer
**Trigger:** Suspecting loot tables are imbalanced
**Preconditions:** Game builds successfully

```bash
dotnet run --project RuneAndRust.Terminal -- --audit-loot --iterations=10000 --biome=Industrial --danger=Hostile
```

**Postconditions:** Report generated at `docs/audits/loot_audit_YYYYMMDD_HHMMSS.md`

---

### UC-2: Validate Combat Balance

**Actor:** Developer
**Trigger:** New enemy added, need to validate difficulty
**Preconditions:** Enemy template exists in database

```bash
dotnet run --project RuneAndRust.Terminal -- --audit-combat --iterations=1000 --archetype=Warrior --enemy=und_draugr_01 --level=3
```

**Postconditions:** Report shows win rate, TTK, hit rates with variance flags

---

### UC-3: Compare Archetypes

**Actor:** Game Designer
**Trigger:** Need to ensure all archetypes are viable
**Preconditions:** All archetype configurations complete

```csharp
foreach (var archetype in Enum.GetValues<ArchetypeType>())
{
    var config = new CombatAuditConfiguration(1000, archetype, "und_draugr_01");
    var report = await auditService.RunAuditAsync(config);
    results.Add(archetype, report.Statistics.WinRate);
}
```

**Postconditions:** Comparison data showing relative performance

---

### UC-4: Detect Economy Inflation

**Actor:** Developer
**Trigger:** Players reporting too much/too little loot
**Preconditions:** Loot tables configured

```csharp
var config = new LootAuditConfiguration(50000, BiomeType.Void, DangerLevel.Lethal);
var report = await auditService.RunAuditAsync(config);
// Check if average Scrip per iteration exceeds design targets
```

**Postconditions:** Economy metrics available for analysis

---

## Decision Trees

### Variance Classification (Loot)

```
Input: |Actual% - Expected%|
                │
                ▼
    ┌───────────────────────┐
    │ AbsoluteVariance > 5% │
    └───────────┬───────────┘
                │
       ┌────────┴────────┐
       │ YES             │ NO
       ▼                 ▼
  ┌──────────┐   ┌───────────────────────┐
  │ CRITICAL │   │ AbsoluteVariance > 1% │
  └──────────┘   └───────────┬───────────┘
                             │
                   ┌─────────┴─────────┐
                   │ YES               │ NO
                   ▼                   ▼
              ┌─────────┐         ┌────────┐
              │ WARNING │         │   OK   │
              └─────────┘         └────────┘
```

### Variance Classification (Combat)

```
Input: ActualValue, ExpectedMin, ExpectedMax
                │
                ▼
    ┌───────────────────────────────┐
    │ ActualValue within [Min,Max]? │
    └───────────────┬───────────────┘
                    │
           ┌────────┴────────┐
           │ YES             │ NO
           ▼                 ▼
      ┌────────┐     ┌────────────────────┐
      │   OK   │     │ Deviation > 15%?   │
      └────────┘     └─────────┬──────────┘
                               │
                      ┌────────┴────────┐
                      │ YES             │ NO
                      ▼                 ▼
                 ┌──────────┐      ┌─────────┐
                 │ CRITICAL │      │ WARNING │
                 └──────────┘      └─────────┘
```

### Simulated Player Action Selection

```
Input: player.CurrentStamina, target.CurrentHP
                │
                ▼
    ┌─────────────────────────────────┐
    │ target.HP ≤ 2 × WeaponDie?      │
    │ AND Stamina ≥ 40?               │
    └───────────────┬─────────────────┘
                    │
           ┌────────┴────────┐
           │ YES             │ NO
           ▼                 ▼
      ┌──────────┐   ┌─────────────────┐
      │ HEAVY    │   │ Stamina ≥ 40?   │
      │ (Finish) │   └────────┬────────┘
      └──────────┘            │
                     ┌────────┴────────┐
                     │ YES             │ NO
                     ▼                 ▼
                ┌─────────┐    ┌─────────────────┐
                │  HEAVY  │    │ Stamina ≥ 20?   │
                └─────────┘    └────────┬────────┘
                                        │
                               ┌────────┴────────┐
                               │ YES             │ NO
                               ▼                 ▼
                          ┌──────────┐      ┌─────────┐
                          │ STANDARD │      │  LIGHT  │
                          └──────────┘      └─────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `ILootService` | [SPEC-LOOT-001](../economy/SPEC-LOOT-001.md) | Loot generation for Monte Carlo simulation |
| `ICombatService` | [SPEC-COMBAT-001](../combat/SPEC-COMBAT-001.md) | Combat execution for battle simulation |
| `CharacterFactory` | [SPEC-CHAR-001](../character/SPEC-CHAR-001.md) | Create simulated player characters |
| `IEnemyFactory` | [SPEC-ENEMYFAC-001](../combat/SPEC-ENEMYFAC-001.md) | Create enemy combatants |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| (CLI only) | N/A | Developer tool, not consumed by game systems |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `LootAuditService.cs` | Monte Carlo loot simulation | Full file |
| `CombatAuditService.cs` | Monte Carlo combat simulation | Full file |
| `SimulatedPlayerAgent.cs` | Heuristic player AI | Full file |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `LootStatistics.cs` | Loot statistics accumulator | Full file |
| `CombatStatistics.cs` | Combat statistics accumulator | Full file |
| `ILootAuditService.cs` | Interface + records | Full file |
| `ICombatAuditService.cs` | Interface + records | Full file |

---

## Data Models

### LootStatistics

```csharp
public class LootStatistics
{
    // Aggregate Counts
    public int TotalIterations { get; private set; }
    public int TotalItemsDropped { get; private set; }
    public long TotalScripValue { get; private set; }
    public long TotalWeight { get; private set; }
    public int SuccessfulIterations { get; private set; }

    // Distribution Tracking
    public Dictionary<QualityTier, int> QualityTierCounts { get; }
    public Dictionary<ItemType, int> ItemTypeCounts { get; }

    // Derived Metrics
    public double AverageScripPerIteration { get; }
    public double AverageItemsPerIteration { get; }
    public double SuccessRate { get; }

    // Methods
    public void Record(LootResult result);
    public double GetQualityTierPercent(QualityTier tier);
    public double GetItemTypePercent(ItemType type);
}
```

### CombatStatistics

```csharp
public class CombatStatistics
{
    // Aggregate Counts
    public int TotalEncounters { get; private set; }
    public int PlayerWins { get; private set; }
    public int EnemyWins { get; private set; }
    public int TotalRounds { get; private set; }
    public int TotalPlayerTurns { get; private set; }
    public int TotalEnemyTurns { get; private set; }

    // Damage Tracking
    public long TotalPlayerDamageDealt { get; private set; }
    public long TotalPlayerDamageReceived { get; private set; }
    public int TotalPlayerHits { get; private set; }
    public int TotalPlayerMisses { get; private set; }
    public long TotalEnemyDamageDealt { get; private set; }
    public int TotalEnemyHits { get; private set; }
    public int TotalEnemyMisses { get; private set; }

    // Resource Tracking
    public long TotalPlayerStaminaSpent { get; private set; }
    public long TotalPlayerHPRemaining { get; private set; }

    // Derived Metrics
    public double WinRate { get; }
    public double AvgRoundsPerEncounter { get; }
    public double PlayerHitRate { get; }
    public double EnemyHitRate { get; }
    public double AvgPlayerDamagePerHit { get; }
    public double AvgEnemyDamagePerHit { get; }
    public double AvgStaminaPerEncounter { get; }
    public double AvgHPRemainingOnWin { get; }

    // Methods
    public void RecordEncounterResult(CombatMatchResult result);
    public void RecordPlayerAttack(bool hit, int damage);
    public void RecordEnemyAttack(bool hit, int damage);
}
```

### VarianceSeverity Enum

```csharp
public enum VarianceSeverity
{
    Ok,       // Within acceptable bounds
    Warning,  // Notable but not critical
    Critical  // Requires attention
}
```

### Configuration Records

```csharp
public record LootAuditConfiguration(
    int Iterations,           // Number of loot cycles
    BiomeType Biome,          // Affects item type distribution
    DangerLevel DangerLevel,  // Affects quality tier weights
    int WitsBonus = 0);       // Optional WITS bonus

public record CombatAuditConfiguration(
    int Iterations,              // Number of matches
    ArchetypeType PlayerArchetype, // Player class
    string EnemyTemplateId,      // Enemy template ID
    int PlayerLevel = 1,         // Player level
    int? Seed = null);           // Optional RNG seed
```

### Report Records

```csharp
public record LootAuditReport(
    LootStatistics Statistics,
    string MarkdownReport,
    IReadOnlyList<VarianceFlag> Flags);

public record CombatAuditReport(
    CombatStatistics Statistics,
    string MarkdownReport,
    IReadOnlyList<CombatVarianceFlag> Flags);
```

---

## Configuration

### Constants

```csharp
// Loot Audit Variance Thresholds
private const double WarningThreshold = 1.0;  // 1% deviation
private const double CriticalThreshold = 5.0; // 5% deviation

// Combat Audit Variance Thresholds
private const double WarningThreshold = 5.0;   // 5% outside bounds
private const double CriticalThreshold = 15.0; // 15% outside bounds

// Combat Expected Metric Bounds
private const double MinWinRate = 70.0;
private const double MaxWinRate = 90.0;
private const double MinAvgRounds = 3.0;
private const double MaxAvgRounds = 8.0;
private const double MinHitRate = 65.0;
private const double MaxHitRate = 85.0;

// Simulation Safety Limits
private const int MaxRoundsPerMatch = 100;

// Simulated Player Agent Thresholds
private const int HeavyAttackThreshold = 40;  // Stamina for Heavy Attack
private const int StandardAttackThreshold = 20; // Stamina for Standard Attack
```

### CLI Parameters

| Parameter | Default | Values | Description |
|-----------|---------|--------|-------------|
| `--audit-loot` | N/A | Flag | Trigger loot audit |
| `--audit-combat` | N/A | Flag | Trigger combat audit |
| `--iterations` | 10000 (loot) / 1000 (combat) | Positive int | Simulation cycles |
| `--biome` | Ruin | Ruin, Industrial, Organic, Void | Loot biome context |
| `--danger` | Safe | Safe, Unstable, Hostile, Lethal | Loot danger level |
| `--wits` | 0 | 0-10 | WITS bonus for quality upgrade |
| `--archetype` | Warrior | Warrior, Skirmisher, Adept | Player archetype |
| `--enemy` | und_draugr_01 | Template ID | Enemy to fight |
| `--level` | 1 | 1-10 | Enemy scaling level |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `LootStatisticsTests.cs` | 15 | Statistics accumulation, distributions, derived metrics |
| `LootAuditIntegrationTests.cs` | 12 | Sanity checks, report generation, variance flags, biome distributions |
| `CombatStatisticsTests.cs` | 24 | Encounter recording, attack tracking, derived metrics |
| `CombatAuditIntegrationTests.cs` | 16 | Sanity checks, archetypes, enemies, variance flags |

### Critical Test Scenarios

**Loot Audit:**
1. Record() correctly accumulates quality tier and item type counts
2. Variance flags generated for all quality tiers and item types
3. Safe zone produces no Optimized/MythForged items
4. Biome affects item type distribution (Organic favors Consumables)

**Combat Audit:**
1. Warrior vs standard enemy achieves > 70% win rate
2. All archetypes produce valid reports
3. All enemy templates work without exception
4. Zero iterations handled gracefully

### Validation Checklist

- [x] 67 tests passing (15 + 12 + 24 + 16)
- [x] All variance thresholds documented
- [x] CLI parameters parse correctly
- [x] Sequential execution prevents race conditions
- [x] Reports contain all required sections

---

## Design Rationale

### Why Sequential Execution (Not Parallel)?

- **Problem**: `GameState` is a singleton shared across the application
- **Problem**: `Random` internally used by services is not thread-safe
- **Decision**: Use sequential `for` loop with fresh DI scope per iteration
- **Trade-off**: 10,000 loot iterations take ~500ms; 1,000 combat iterations take ~2s
- **Benefit**: Thread safety guaranteed, no race conditions

### Why Fresh DI Scope Per Combat Iteration?

- **Problem**: Combat modifies `GameState` extensively
- **Problem**: Services retain state between calls
- **Decision**: Create new DI scope for each match, get fresh service instances
- **Benefit**: Each simulation is isolated, no cross-contamination

### Why Different Variance Thresholds for Loot vs Combat?

- **Loot**: Statistical distribution should converge tightly (1%/5% thresholds)
- **Combat**: More inherent variance from dice rolls and AI decisions (5%/15% thresholds)
- **Combat**: Uses range bounds [min, max] rather than expected percentage

### Why Add ProcessEnemyTurnSync Instead of Modifying Async?

- **Problem**: `ProcessEnemyTurnAsync()` has 750ms `Task.Delay` for UX
- **Risk**: Modifying async method could break production combat flow
- **Decision**: Add new `ProcessEnemyTurnSync()` with identical logic, no delay
- **Benefit**: Minimal change, non-invasive, preserves production behavior

### Why Heuristic Player Agent (Not Optimal)?

- **Goal**: Represent "reasonable" player behavior, not perfect play
- **Trade-off**: ~80% accurate vs actual player decisions
- **Benefit**: Faster implementation, good enough for balance testing
- **Future**: Could implement ability usage, defensive actions

---

## Changelog

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.13a and v0.3.13b implementation
- Loot Audit: ILootAuditService, LootStatistics, variance flagging
- Combat Audit: ICombatAuditService, CombatStatistics, SimulatedPlayerAgent
- CLI integration: `--audit-loot`, `--audit-combat` with parameters
- 67 unit and integration tests (100% passing)
