---
id: SPEC-AI-001
title: Enemy AI & Behavior System
version: 1.0.0
status: Implemented
related_specs: [SPEC-DICE-001, SPEC-COMBAT-001, SPEC-ATTACK-001, SPEC-ENEMY-001]
---

# SPEC-AI-001: Enemy AI & Behavior System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `EnemyAIService`
> **Location:** `RuneAndRust.Engine/Services/EnemyAIService.cs`

---

## Overview

The Enemy AI & Behavior System governs autonomous enemy decision-making during combat. It translates enemy archetypes into tactical combat actions, balancing offensive pressure, defensive positioning, and ability usage based on battlefield state.

This system creates distinct enemy personalities through archetype-driven behavior patterns without requiring complex state machines.

---

## Core Concepts

### Enemy Archetypes

Seven distinct behavioral patterns define enemy decision-making:

| Archetype | Philosophy | Primary Behavior | Secondary Behavior |
|-----------|------------|------------------|-------------------|
| **Tank** | Protect allies, absorb damage | Defend when HP < 40% | Heavy attacks when healthy |
| **DPS** | Consistent damage output | Standard attacks (70%) | Heavy attacks (30%) |
| **GlassCannon** | High-risk offense | Heavy attacks (60%) | Abilities (40%) |
| **Support** | Enable allies | Abilities (80%) | Defend when threatened |
| **Swarm** | Rapid weak attacks | Light attacks (100%) | None |
| **Caster** | Ranged abilities | Abilities (80%) | Light attacks (20%) |
| **Boss** | Balanced threat | Even split (33% each) | Context-adaptive |

### Decision Factors

The AI evaluates three primary factors each turn:

1. **Current HP Percentage** - Triggers defensive behavior thresholds
2. **Available Stamina** - Constrains action affordability
3. **Archetype Profile** - Defines probability distributions

---

## Behaviors

### Primary Behaviors

#### 1. Choose Combat Action (`ChooseCombatAction`)

```csharp
CombatAction ChooseCombatAction(
    Combatant enemy,
    List<Combatant> allCombatants)
```

**Sequence:**
1. Identify enemy archetype from template
2. Check current HP percentage
3. Evaluate stamina availability
4. Apply archetype behavior logic
5. Validate stamina sufficiency
6. Return CombatAction (Attack type or Defend)

**State Triggers:**

| Archetype | HP Threshold | Behavior Override |
|-----------|--------------|-------------------|
| Tank | < 40% | Always Defend |
| Support | < 30% | Always Defend |
| Cowardly | < 25% | Flee (if implemented) |
| All Others | N/A | No HP-based override |

**Example:**
```csharp
// Tank at 35% HP with 50 stamina
var action = _aiService.ChooseCombatAction(tankEnemy, allCombatants);
// Returns: CombatAction.Defend (HP < 40% threshold)

// Tank at 80% HP with 50 stamina
var action = _aiService.ChooseCombatAction(tankEnemy, allCombatants);
// Returns: CombatAction.HeavyAttack (75% probability when healthy)
```

---

### Archetype-Specific Logic

#### Tank Archetype

**Philosophy:** "Protect allies through damage absorption and threat presence."

**Decision Tree:**
```
IF HP < 40%
  RETURN Defend
ELSE IF Stamina >= 40
  ROLL d100
  IF roll <= 75
    RETURN HeavyAttack
  ELSE
    RETURN StandardAttack
ELSE IF Stamina >= 25
  RETURN StandardAttack
ELSE IF Stamina >= 15
  RETURN LightAttack
ELSE
  RETURN Defend
```

**Key Characteristics:**
- Wounded threshold: 40% HP
- Heavy attack preference: 75% when healthy
- Fallback: Always has stamina for Defend

---

#### DPS Archetype

**Philosophy:** "Consistent, reliable damage without overcommitment."

**Decision Tree:**
```
IF Stamina >= 40
  ROLL d100
  IF roll <= 30
    RETURN HeavyAttack
  ELSE
    RETURN StandardAttack
ELSE IF Stamina >= 25
  RETURN StandardAttack
ELSE IF Stamina >= 15
  RETURN LightAttack
ELSE
  RETURN Defend
```

**Key Characteristics:**
- No HP-based behavior changes
- 70% Standard / 30% Heavy attack split
- Stamina-efficient combat rhythm

---

#### GlassCannon Archetype

**Philosophy:** "Maximize damage output, ignore sustainability."

**Decision Tree:**
```
IF Stamina >= 40
  ROLL d100
  IF roll <= 60
    RETURN HeavyAttack
  ELSE IF HasAbility AND Stamina >= 30
    RETURN UseAbility
  ELSE
    RETURN HeavyAttack
ELSE IF Stamina >= 25
  RETURN StandardAttack
ELSE IF Stamina >= 15
  RETURN LightAttack
ELSE
  RETURN Defend
```

**Key Characteristics:**
- No defensive behavior until out of stamina
- 60% Heavy attack / 40% Ability split
- High damage, high risk playstyle

---

#### Support Archetype

**Philosophy:** "Enable allies through buffs and debuffs, preserve self."

**Decision Tree:**
```
IF HP < 30%
  RETURN Defend
ELSE IF HasAbility AND Stamina >= 30
  ROLL d100
  IF roll <= 80
    RETURN UseAbility
  ELSE
    RETURN LightAttack
ELSE IF Stamina >= 15
  RETURN LightAttack
ELSE
  RETURN Defend
```

**Key Characteristics:**
- Threatened threshold: 30% HP
- 80% Ability usage when available
- Minimal offensive pressure

---

#### Swarm Archetype

**Philosophy:** "Rapid, relentless light attacks to overwhelm defenses."

**Decision Tree:**
```
IF Stamina >= 15
  RETURN LightAttack
ELSE
  RETURN Defend
```

**Key Characteristics:**
- No complex decision-making
- 100% Light attacks when able
- Designed for numerical superiority tactics

---

#### Caster Archetype

**Philosophy:** "Maintain distance, prioritize abilities over physical attacks."

**Decision Tree:**
```
IF HasAbility AND Stamina >= 30
  ROLL d100
  IF roll <= 80
    RETURN UseAbility
  ELSE
    RETURN LightAttack
ELSE IF Stamina >= 15
  RETURN LightAttack
ELSE
  RETURN Defend
```

**Key Characteristics:**
- 80% Ability preference
- No Heavy attacks (thematic: weak physical strength)
- Light attacks as filler actions

---

#### Boss Archetype

**Philosophy:** "Balanced, unpredictable threat with access to all tactics."

**Decision Tree:**
```
IF Stamina >= 40
  ROLL d100
  IF roll <= 33
    RETURN HeavyAttack
  ELSE IF roll <= 66 AND HasAbility
    RETURN UseAbility
  ELSE
    RETURN StandardAttack
ELSE IF Stamina >= 25
  RETURN StandardAttack
ELSE IF Stamina >= 15
  RETURN LightAttack
ELSE
  RETURN Defend
```

**Key Characteristics:**
- 33% / 33% / 33% action distribution
- No HP-based behavior changes (represents confidence)
- Uses full combat toolkit

---

## Restrictions

### Decision Constraints

1. **Stamina Gating** - Actions unavailable if stamina insufficient
   - Heavy Attack: 40 stamina
   - Ability: 30 stamina (typical, varies by ability)
   - Standard Attack: 25 stamina
   - Light Attack: 15 stamina
   - Defend: 0 stamina (always available)

2. **Ability Availability** - UseAbility only valid if enemy has learned abilities

3. **HP-Based Overrides** - Take precedence over archetype probabilities

---

## Limitations

### Numerical Bounds

| Constraint | Value | Notes |
|------------|-------|-------|
| Probability range | 0-100 | d100 roll for weighted decisions |
| HP threshold (Tank) | 40% | Triggers Defend override |
| HP threshold (Support) | 30% | Triggers Defend override |
| Heavy attack cost | 40 stamina | Highest action cost |
| Ability cost | 30 stamina | Typical cost (varies) |
| Standard attack cost | 25 stamina | Default action |
| Light attack cost | 15 stamina | Economy option |

### System Gaps

- **No multi-turn planning** - Decisions made turn-by-turn without foresight
- **No target selection logic** - Assumes CombatService handles target choice
- **No ability-specific selection** - If multiple abilities available, selection is not AI-driven
- **No environmental awareness** - Cannot react to hazards or conditions strategically
- **No ally coordination** - Archetype decisions are independent, no team tactics
- **No learning/adaptation** - Static behavior patterns throughout combat

---

## Use Cases

### UC-1: Tank Behavior Shift

```csharp
// Combat Start - Tank at 100% HP, 100 stamina
var action = _aiService.ChooseCombatAction(tank, allCombatants);
// d100 roll: 42 → HeavyAttack (75% chance when healthy)

// Turn 5 - Tank at 38% HP, 60 stamina (after taking damage)
var action = _aiService.ChooseCombatAction(tank, allCombatants);
// Returns: Defend (HP < 40% threshold override)

// Turn 7 - Tank at 42% HP, 100 stamina (after rest/heal)
var action = _aiService.ChooseCombatAction(tank, allCombatants);
// d100 roll: 88 → StandardAttack (above 40%, but roll failed 75% Heavy chance)
```

**Narrative Impact:** Tank transitions from aggressive front-line combatant to defensive turtle when wounded, creating tactical openings for players.

---

### UC-2: Swarm Overwhelm Tactics

```csharp
// Three Swarm enemies (Blight-Rats) engage player
foreach (var swarmEnemy in swarmEnemies)
{
    var action = _aiService.ChooseCombatAction(swarmEnemy, allCombatants);
    // All return: LightAttack (100% behavior)
}

// Player receives 3 separate Light attacks (3 × 1d8+STR)
// Even with low individual damage, cumulative pressure forces resource spending
```

**Narrative Impact:** "A tide of rusted claws overwhelms careful defenses through sheer numbers."

---

### UC-3: Boss Unpredictability

```csharp
// Boss (Ancient Guardian) Turn 1 - 100% HP, 100 stamina
var action = _aiService.ChooseCombatAction(boss, allCombatants);
// d100 roll: 25 → HeavyAttack (33% range)

// Boss Turn 2 - 95% HP, 60 stamina
var action = _aiService.ChooseCombatAction(boss, allCombatants);
// d100 roll: 50 → UseAbility (34-66% range)

// Boss Turn 3 - 90% HP, 100 stamina (regenerated)
var action = _aiService.ChooseCombatAction(boss, allCombatants);
// d100 roll: 75 → StandardAttack (67-100% range)
```

**Narrative Impact:** Players cannot rely on predictable patterns, forcing adaptive tactical responses.

---

### UC-4: Support Preservation Instinct

```csharp
// Support (Dvergr Shaman) at 45% HP, 50 stamina
var action = _aiService.ChooseCombatAction(shaman, allCombatants);
// d100 roll: 35 → UseAbility (80% chance, applies Fortified to Tank ally)

// Support takes 15 damage → now at 28% HP, 20 stamina
var action = _aiService.ChooseCombatAction(shaman, allCombatants);
// Returns: Defend (HP < 30% threshold override)
```

**Narrative Impact:** Support enemies become defensive when threatened, forcing players to choose between eliminating buffers or focusing primary threats.

---

### UC-5: DPS Stamina Management

```csharp
// DPS (Iron-Husk Marauder) Turn 1 - 100 stamina
var action = _aiService.ChooseCombatAction(dps, allCombatants);
// d100 roll: 15 → HeavyAttack (30% chance, costs 40 stamina → 60 remaining)

// DPS Turn 2 - 60 stamina
var action = _aiService.ChooseCombatAction(dps, allCombatants);
// d100 roll: 55 → StandardAttack (70% chance, costs 25 stamina → 35 remaining)

// DPS Turn 3 - 35 stamina (< 40 threshold, Heavy unavailable)
var action = _aiService.ChooseCombatAction(dps, allCombatants);
// Returns: StandardAttack (only affordable offensive option)

// DPS Turn 4 - 10 stamina (< 15 threshold)
var action = _aiService.ChooseCombatAction(dps, allCombatants);
// Returns: Defend (out of stamina for attacks)
```

**Narrative Impact:** Consistent damage pressure that naturally ebbs as stamina depletes, creating rhythm in extended combats.

---

### UC-6: GlassCannon All-In Strategy

```csharp
// GlassCannon (Blight-Caster Adept) at 25% HP, 100 stamina
var action = _aiService.ChooseCombatAction(glassCannon, allCombatants);
// d100 roll: 45 → HeavyAttack (60% chance, no HP-based override!)

// GlassCannon at 10% HP, 50 stamina
var action = _aiService.ChooseCombatAction(glassCannon, allCombatants);
// d100 roll: 70 → UseAbility (40% chance)
// Continues full offense even near death
```

**Narrative Impact:** High-risk enemies remain dangerous until eliminated, rewarding prioritization in target selection.

---

## Decision Trees

### Master Archetype Dispatcher

```
┌─────────────────────────────────┐
│  ChooseCombatAction(enemy)      │
└────────────┬────────────────────┘
             │
             ├─ Tank → TankLogic()
             ├─ DPS → DPSLogic()
             ├─ GlassCannon → GlassCannonLogic()
             ├─ Support → SupportLogic()
             ├─ Swarm → SwarmLogic()
             ├─ Caster → CasterLogic()
             └─ Boss → BossLogic()
```

---

### Stamina-Gated Action Selection

```
┌─────────────────────────────────┐
│  ValidateStaminaSufficiency     │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ Check desired   │
    │ action stamina  │
    └────────┬────────┘
             │
    ┌────────┴──────────┐
    │ Stamina >= Cost?  │
    └────────┬──────────┘
         ┌───┴───┐
         │       │
        YES     NO
         │       │
         │   ┌───┴────────────────┐
         │   │ Cascade to cheaper │
         │   │ affordable action: │
         │   │ Heavy → Standard → │
         │   │ Light → Defend     │
         │   └────────────────────┘
         │
         ▼
    Return validated action
```

---

### Tank HP-Based Behavior Tree

```
┌─────────────────────────────────┐
│  TankLogic(enemy)               │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ HP < 40%?       │
    └────────┬────────┘
         ┌───┴───┐
         │       │
        YES     NO
         │       │
         ▼       ▼
    ┌────────┐  ┌──────────────────┐
    │ DEFEND │  │ Stamina >= 40?   │
    └────────┘  └────────┬──────────┘
                     ┌───┴───┐
                     │       │
                    YES     NO
                     │       │
                     ▼       ▼
                ┌─────────┐ ┌─────────────┐
                │ Roll    │ │ Cascade:    │
                │ d100    │ │ Std → Light │
                └────┬────┘ │ → Defend    │
                 ┌───┴───┐  └─────────────┘
                 │       │
            <= 75?     > 75?
                 │       │
                 ▼       ▼
            ┌─────────┐ ┌──────────┐
            │ HEAVY   │ │ STANDARD │
            │ ATTACK  │ │ ATTACK   │
            └─────────┘ └──────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IDiceService` | [SPEC-DICE-001](SPEC-DICE-001.md) | d100 probability rolls for archetype behavior |
| `IRepository<EnemyTemplate>` | [SPEC-ENEMY-001](SPEC-ENEMY-001.md) | Archetype lookup from enemy entity |
| `ILogger` | Infrastructure | AI decision tracing for debugging |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Enemy turn decision-making orchestration |
| `AttackResolutionService` | [SPEC-ATTACK-001](SPEC-ATTACK-001.md) | Receives CombatAction for attack type resolution |

### Related Systems

- [SPEC-ENEMY-001](SPEC-ENEMY-001.md) - **Enemy Factory System**: Defines archetypes and templates consumed by AI
- [SPEC-COMBAT-001](SPEC-COMBAT-001.md) - **Combat Orchestration**: Orchestrates AI decision → action execution flow
- [SPEC-ATTACK-001](SPEC-ATTACK-001.md) - **Attack Resolution**: Executes AI-chosen attack types with stamina costs

---

## Related Services

### Primary Implementation

| File | Purpose |
|------|------------|
| `EnemyAIService.cs` | Archetype behavior logic, decision trees |

### Supporting Types

| File | Purpose |
|------|------------|
| `CombatAction.cs` | Action type enum (Defend, Light/Standard/Heavy Attack, UseAbility) |
| `EnemyArchetype.cs` | Archetype enum (Tank, DPS, GlassCannon, Support, Swarm, Caster, Boss) |
| `EnemyTemplate.cs` | Enemy definition with Archetype field |

---

## Data Models

### CombatAction Enum

```csharp
public enum CombatAction
{
    Defend = 0,          // 0 stamina
    LightAttack = 1,     // 15 stamina
    StandardAttack = 2,  // 25 stamina
    HeavyAttack = 3,     // 40 stamina
    UseAbility = 4       // 30 stamina (typical)
}
```

### EnemyArchetype Enum

```csharp
public enum EnemyArchetype
{
    Tank = 0,         // Defensive, protects allies
    DPS = 1,          // Balanced offense
    GlassCannon = 2,  // High damage, no defense
    Support = 3,      // Buffs/debuffs, preserves self
    Swarm = 4,        // Relentless light attacks
    Caster = 5,       // Ability-focused
    Boss = 6          // Unpredictable, all tactics
}
```

### EnemyTemplate (Relevant Fields)

```csharp
public class EnemyTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public EnemyArchetype Archetype { get; set; }

    // Other fields (stats, loot, etc.) not directly used by AI
}
```

---

## Configuration

### Archetype Probability Tables

Hardcoded in `EnemyAIService` logic:

| Archetype | Heavy % | Standard % | Light % | Ability % | Defend Override |
|-----------|---------|------------|---------|-----------|-----------------|
| Tank | 75 (healthy) | 25 | Fallback | None | HP < 40% |
| DPS | 30 | 70 | Fallback | None | Never |
| GlassCannon | 60 | Fallback | Fallback | 40 | Never |
| Support | None | None | 20 | 80 | HP < 30% |
| Swarm | None | None | 100 | None | Never |
| Caster | None | None | 20 | 80 | Never |
| Boss | 33 | 34 | Fallback | 33 | Never |

**Fallback:** Action used when stamina insufficient for higher-priority choices.

---

## Testing

### Test Coverage

`EnemyAIServiceTests.cs` - 27 test methods covering:

1. **Archetype Behavior Validation** (7 tests)
   - One test per archetype verifying probability distributions

2. **HP Threshold Triggers** (3 tests)
   - Tank defense at < 40% HP
   - Support defense at < 30% HP
   - No override for non-defensive archetypes

3. **Stamina Gating** (5 tests)
   - Heavy attack unavailable when stamina < 40
   - Cascading to Standard when Heavy unaffordable
   - Cascading to Light when Standard unaffordable
   - Force Defend when all attacks unaffordable

4. **Ability Integration** (3 tests)
   - UseAbility only when enemy has learned abilities
   - Fallback to attacks when abilities unavailable
   - Ability percentage respected by archetype

5. **Edge Cases** (9 tests)
   - 0 stamina → always Defend
   - 100% HP Tank → still rolls for Heavy
   - 1% HP GlassCannon → still full offense
   - Boss equal 33% distribution validation
   - Swarm deterministic 100% Light

---

## Design Rationale

### Why Archetypes Over State Machines?

**Decision:** Use archetype-driven probability tables instead of complex FSMs.

**Rationale:**
- **Simplicity:** Each archetype = one function with clear decision tree
- **Predictability:** Players can learn archetype behaviors and adapt tactics
- **Maintainability:** Adding new archetype = add new function, no state graph refactoring
- **Performance:** Single function call per turn, no state transition overhead

**Trade-off:** Less emergent complexity, but combat clarity for player strategic planning.

---

### Why No Target Selection in AI?

**Decision:** `ChooseCombatAction` returns action type, not target.

**Rationale:**
- **Separation of Concerns:** AI chooses "what to do," CombatService chooses "who to target"
- **Extensibility:** Target selection logic can be added to CombatService without AI refactoring
- **Consistency:** Player target selection UI and enemy AI selection use same underlying system

**Current Behavior:** CombatService defaults to attacking player character. Future enhancement: threat-based targeting.

---

### Why HP Percentage Over Absolute Values?

**Decision:** Thresholds like "< 40% HP" instead of "< 50 HP."

**Rationale:**
- **Scaling:** Works for both 100 HP Swarm and 500 HP Boss without separate logic
- **Intent Clarity:** "Wounded" = percentage-based concept, not absolute health
- **Balance:** Defensive behavior triggered proportionally across all enemy tiers

---

### Why No Multi-Turn Planning?

**Decision:** Turn-by-turn decisions without lookahead.

**Rationale:**
- **Development Scope:** Multi-turn planning requires prediction engine (complex)
- **Player Agency:** Reactive enemies create opportunities for player tactical disruption
- **Performance:** Single-turn decisions = O(1) complexity per enemy turn

**Future Enhancement:** Boss archetype could gain multi-turn "phases" (enrage at 50%, summon adds at 25%).

---

## Changelog

### v1.0.0 (Initial Implementation)

**Added:**
- Seven archetype behavior patterns (Tank, DPS, GlassCannon, Support, Swarm, Caster, Boss)
- HP-based defensive overrides (Tank 40%, Support 30%)
- Stamina-gated action cascading (Heavy → Standard → Light → Defend)
- Probability-weighted action selection (d100 rolls)
- 27 test methods covering all archetype behaviors and edge cases

**Design Decisions:**
- Archetype-driven probability tables over state machines
- Target selection delegated to CombatService
- HP percentage thresholds for scalable balance
- No multi-turn planning (turn-by-turn reactive)

**Integration Points:**
- `CombatService.ProcessEnemyTurn()` calls `ChooseCombatAction()`
- `AttackResolutionService` receives `CombatAction` enum for attack type execution
- `EnemyTemplate.Archetype` field consumed for archetype lookup

---

## Future Enhancements

### Phase-Based Boss Behavior

**Concept:** Boss archetype transitions to different probability tables at HP thresholds.

**Example:**
```csharp
// Boss Phase 1 (100-50% HP): Balanced 33/33/33
// Boss Phase 2 (50-25% HP): Aggressive 50% Heavy, 30% Ability, 20% Standard
// Boss Phase 3 (<25% HP): Desperate 70% Ability, 30% Heavy
```

**Benefit:** Creates narrative progression (cautious → aggressive → desperate).

---

### Threat-Based Target Selection

**Concept:** CombatService evaluates threat scores (damage dealt, healing done, debuffs applied) to select enemy targets.

**Example:**
```csharp
// Tank archetype prioritizes highest-threat player character
// Support archetype targets lowest-HP ally for buffs
// DPS archetype targets lowest-defense player character
```

**Benefit:** Forces player team composition consideration (protect healers, manage threat).

---

### Environmental Awareness

**Concept:** AI considers hazards and conditions when choosing actions.

**Example:**
```csharp
// If standing in Scorching Heat (SPEC-COND-001), prefer ranged abilities over melee
// If room has Blighted Ground, Support enemies attempt to move allies away
```

**Benefit:** Enemies leverage environmental advantages, increasing tactical depth.

---

### Adaptive Learning

**Concept:** AI tracks player action patterns and adjusts probabilities.

**Example:**
```csharp
// If player frequently uses Defend, GlassCannon increases Heavy attack %
// If player relies on healing, DPS enemies focus burst damage
```

**Benefit:** Discourages rote strategies, rewards adaptability.

**Complexity Warning:** Requires persistent combat history and probability adjustment system.

---

## AAM-VOICE Compliance

This specification describes mechanical systems and is exempt from Domain 4 constraints. In-game AI decision narration (if implemented) must follow AAM-VOICE guidelines:

**Compliant Example:**
```
The Iron-Husk Guardian shifts into a defensive posture, its corroded plating groaning
under strain. [Tank HP < 40% → Defend action]
```

**Non-Compliant Example:**
```
Enemy AI calculates 38% HP remaining and executes defensive subroutine. [Layer 4 bleed]
```
