---
id: SPEC-AI-001
title: Enemy AI & Behavior System
version: 2.0.0
status: Implemented
last_updated: 2025-12-23
related_specs: [SPEC-DICE-001, SPEC-COMBAT-001, SPEC-ATTACK-001, SPEC-ENEMY-001, SPEC-ABILITY-001]
---

# SPEC-AI-001: Enemy AI & Behavior System

> **Version:** 2.0.0
> **Status:** Implemented (v0.2.4c)
> **Service:** `EnemyAIService`
> **Location:** `RuneAndRust.Engine/Services/EnemyAIService.cs`

---

## Overview

The Enemy AI & Behavior System governs autonomous enemy decision-making during combat using a **utility-based scoring algorithm** (v0.2.4b). Each turn, the AI evaluates all valid actions, assigns scores based on context (HP, stamina, target state, archetype), and uses weighted random selection to choose an action.

This system creates distinct enemy personalities through archetype-specific score modifiers while maintaining tactical adaptability through context-aware bonuses.

> **Note:** This specification focuses on **AI decision logic and scoring constants**. For enemy template definitions and archetype properties, see [SPEC-ENEMY-001](./SPEC-ENEMY-001.md).

---

## Core Concepts

### Utility-Based Scoring

The AI uses a **utility scoring system** rather than fixed probability tables:

1. **All valid actions are evaluated** in parallel (attacks, abilities, defend, flee)
2. **Each action receives a score** based on base value + context modifiers + archetype bonuses
3. **Scores are normalized** and used for weighted random selection
4. **Higher scores = more likely** but not guaranteed (maintains unpredictability)

### Enemy Archetypes

Seven distinct behavioral patterns define enemy tendencies through score modifiers:

| Archetype | Philosophy | Score Tendencies | HP Threshold |
|-----------|------------|------------------|--------------|
| **Tank** | Protect allies, absorb damage | +55 Defend when wounded, +10 always | < 40% |
| **DPS** | Consistent damage output | No special modifiers | None |
| **GlassCannon** | High-risk offense | +20 damage abilities | None |
| **Support** | Enable allies | +15 Defend when wounded | < 50% |
| **Swarm** | Rapid weak attacks | +20 Light attacks | None |
| **Caster** | Ranged abilities | Standard scoring | None |
| **Boss** | Balanced threat | +10 Heavy attacks | None |

### Decision Factors

The AI evaluates five primary factors each turn:

1. **Current HP Percentage** - Affects heal scoring, defend bonuses, flee triggers
2. **Available Stamina** - Constrains action affordability, conservation penalties
3. **Target HP Percentage** - Kill range bonuses for damage actions
4. **Target Status Effects** - Redundant debuff penalties
5. **Archetype Profile** - Defines score modifiers

---

## Behaviors

### Primary Behavior: Determine Action (`DetermineAction`)

```csharp
CombatAction DetermineAction(Combatant enemy, CombatState state)
```

**Sequence:**
1. Find target (typically the player)
2. Check if locked in Chanting state (v0.2.4c charge abilities)
3. Build scored action list:
   - `EvaluateBasicAttacks()` → Heavy, Standard, Light
   - `EvaluateAbilities()` → All usable abilities
   - `EvaluateDefend()` → Defend action
   - `EvaluateFlee()` → Flee (if Cowardly tag)
4. Filter actions with score < 0
5. `SelectBestAction()` → Weighted random selection
6. Return CombatAction

---

## Scoring Constants

### Base Action Scores

| Action | Base Score | Notes |
|--------|------------|-------|
| Heavy Attack | 65 | BaseScore (50) + 15 |
| Standard Attack | 55 | BaseScore (50) + 5 |
| Light Attack | 40 | BaseScore (50) - 10 |
| Defend | 35 | BaseScore (50) - 15 |
| Ability | 50 | BaseScore, modified by effect type |

### Context Modifiers

| Modifier | Value | Trigger Condition |
|----------|-------|-------------------|
| `CriticalHpHealBonus` | +50 | HEAL ability when user HP < 30% |
| `WastefulHealPenalty` | -40 | HEAL ability when user HP > 80% |
| `KillRangeBonus` | +30 | DAMAGE ability when target HP < 20% |
| `KillRangeBonus / 2` | +15 | Standard attack when target HP < 20% |
| `StaminaConservationPenalty` | -20 | Ability cost > 50% of current stamina |
| `RedundantDebuffPenalty` | -100 | STATUS ability on target with that status |

### Archetype Modifiers

| Archetype | Action | Modifier | Condition |
|-----------|--------|----------|-----------|
| **Tank** | Defend | +10 | Always |
| **Tank** | Defend | +55 | HP < 40% (25 + 30) |
| **GlassCannon** | Heavy | +20 | Always |
| **GlassCannon** | Damage Ability | +20 | Always |
| **GlassCannon** | Standard | +10 | Always |
| **Swarm** | Light | +20 | Always |
| **Boss** | Heavy | +10 | Always |
| **Support** | Defend | +15 | HP < 50% |

### Charge Ability Modifiers (v0.2.4c)

| Modifier | Value | Trigger Condition |
|----------|-------|-------------------|
| `ChargeAbilityBonus` | +20 | Ability has ChargeTurns > 0 |
| `ChargeLowHpPenalty` | -50 | User HP < 30% (risky to stand still) |
| `ChargePlayerStunnedBonus` | +30 | Target has Stunned status |

### Flee Scoring

| Modifier | Value | Trigger Condition |
|----------|-------|-------------------|
| Cowardly Flee | +80 | Has "Cowardly" tag AND HP < 25% |

---

## Weighted Selection Algorithm

The `SelectBestAction()` method uses weighted random selection:

```
1. Filter actions with score >= MinimumActionScore (0)
2. Find minimum score among valid actions
3. Normalize scores: Weight = Max(1, Score - MinScore + 1)
4. Calculate totalWeight = Sum of all normalized weights
5. Roll random number from 0 to totalWeight
6. Iterate actions, accumulating weights until roll < cumulative
7. Return selected action
```

**Example:**
```
Actions: Heavy (65), Standard (55), Defend (35)
MinScore: 35
Normalized: Heavy (31), Standard (21), Defend (1)
TotalWeight: 53

Roll 25 → Cumulative: 31 → Heavy selected (25 < 31)
Roll 40 → Cumulative: 31 + 21 = 52 → Standard selected (40 < 52)
Roll 52 → Cumulative: 52 + 1 = 53 → Defend selected (52 < 53)
```

**Key Property:** Higher scores have proportionally higher selection probability, but any valid action can be chosen.

---

## Charge Ability Handling (v0.2.4c)

Enemies can use **telegraphed charge abilities** that require multiple turns:

### Charge Initiation
1. AI selects a charge ability (ChargeTurns > 0)
2. Enemy gains `Chanting` status for ChargeTurns duration
3. `ChanneledAbilityId` is set to the ability being charged
4. Resources are deducted immediately

### During Charge
- If `Chanting` status is active with duration > 0:
  - AI returns `Pass` action ("continues focusing...")
  - No other actions evaluated

### Charge Release
- When `Chanting` duration reaches 0:
  - AI returns `UseAbility` action for the channeled ability
  - Ability effect is executed
  - Cooldown is set on release

### Charge Interruption
Handled by combat system (not AI):
- Damage exceeding `InterruptThreshold` (default 10% MaxHP) breaks chant
- Interrupted enemy gains `Stunned` status

---

## Restrictions

### Decision Constraints

1. **Stamina Gating** - Actions not added to scored list if unaffordable
   - Heavy Attack: Uses `CanAffordAttack()` check
   - Ability: Uses `CanUse()` check
   - Standard Attack: Uses `CanAffordAttack()` check
   - Light Attack: Uses `CanAffordAttack()` check
   - Defend: Always available (0 stamina)

2. **Ability Cooldowns** - Abilities on cooldown excluded from evaluation

3. **Score Filtering** - Actions with score < 0 are excluded from selection

---

## Limitations

### Numerical Bounds

| Constraint | Value | Notes |
|------------|-------|-------|
| BaseScore | 50 | Starting score for all actions |
| MinimumActionScore | 0 | Threshold for filtering out actions |
| HP threshold (Tank wounded) | 40% | Triggers +55 defend bonus |
| HP threshold (Support wounded) | 50% | Triggers +15 defend bonus |
| HP threshold (Heal critical) | 30% | Triggers +50 heal bonus |
| HP threshold (Cowardly flee) | 25% | Triggers +80 flee bonus |
| HP threshold (Kill range) | 20% | Triggers +30 damage bonus |

### System Gaps

- **No multi-turn planning** - Decisions made turn-by-turn without foresight
- **No target selection logic** - AI targets player only (CombatService controls)
- **No environmental awareness** - Cannot react to hazards or conditions strategically
- **No ally coordination** - Archetype decisions are independent, no team tactics
- **No learning/adaptation** - Static scoring constants throughout combat

---

## Use Cases

### UC-1: Tank Behavior Shift (Utility Scoring)

```csharp
// Combat Start - Tank at 100% HP, 100 stamina
var action = _aiService.DetermineAction(tank, state);
// Scores: Heavy(65+10=75), Standard(55), Defend(35+10=45)
// Weighted selection favors Heavy (75 highest score)

// Turn 5 - Tank at 38% HP (wounded), 60 stamina
var action = _aiService.DetermineAction(tank, state);
// Scores: Heavy(75), Standard(55), Defend(35+55=90) ← Wounded bonus!
// Weighted selection likely picks Defend (90 highest score)

// Turn 7 - Tank at 42% HP, 100 stamina (above threshold)
var action = _aiService.DetermineAction(tank, state);
// Scores: Heavy(75), Standard(55), Defend(45) ← No wounded bonus
// Weighted selection likely picks Heavy
```

**Narrative Impact:** Tank transitions from aggressive to defensive when wounded, but the probabilistic nature means they might still attack occasionally (weighted selection, not deterministic).

---

### UC-2: Swarm Light Attack Preference

```csharp
// Three Swarm enemies (Blight-Rats) engage player
foreach (var swarmEnemy in swarmEnemies)
{
    var action = _aiService.DetermineAction(swarmEnemy, state);
    // Scores: Heavy(65), Standard(55), Light(40+20=60)
    // Heavy still has highest score, but Light competes well
}

// With weighted selection, most Swarm enemies favor Light attacks
// but occasionally may use heavier attacks if available
```

**Narrative Impact:** "A tide of rusted claws overwhelms careful defenses through sheer numbers." Swarm TENDS toward light attacks but isn't locked to them.

---

### UC-3: Boss Weighted Distribution

```csharp
// Boss (Ancient Guardian) - 100% HP, has abilities
var action = _aiService.DetermineAction(boss, state);
// Scores: Heavy(65+10=75), Standard(55), Ability(50), Defend(35)
// Weighted selection: Heavy has edge, but all viable

// Example selection probabilities (normalized):
// Heavy: ~35%, Standard: ~25%, Ability: ~22%, Defend: ~18%
// (Actual ratios depend on all available actions and scores)
```

**Narrative Impact:** Players cannot rely on predictable patterns. Boss actions are weighted toward damage but unpredictable.

---

### UC-4: Heal Priority When Wounded

```csharp
// Enemy with Heal ability at 25% HP
var action = _aiService.DetermineAction(enemy, state);
// Scores: Heavy(65), Heal(50+50=100) ← CriticalHpHealBonus!
// Weighted selection strongly favors Heal

// Same enemy at 90% HP
var action = _aiService.DetermineAction(enemy, state);
// Scores: Heavy(65), Heal(50-40=10) ← WastefulHealPenalty
// Heal is deprioritized, likely filters out (score < minimum)
```

**Narrative Impact:** Enemies intelligently prioritize healing when wounded but don't waste heal abilities when healthy.

---

### UC-5: Redundant Debuff Avoidance

```csharp
// Caster with Slow debuff ability, target already Slowed
var action = _aiService.DetermineAction(caster, state);
// Scores: Slow(50-100=-50) ← RedundantDebuffPenalty!
// Slow action filtered out (score < 0), other actions selected

// Target clears Slow status
var action = _aiService.DetermineAction(caster, state);
// Scores: Slow(50) ← No penalty, viable option again
```

**Narrative Impact:** Enemies don't stack redundant debuffs, creating tactical depth around status management.

---

### UC-6: GlassCannon Damage Focus

```csharp
// GlassCannon at 25% HP with damage ability
var action = _aiService.DetermineAction(glassCannon, state);
// Scores: Heavy(65+20=85), Damage Ability(50+20=70), Defend(35)
// GlassCannon continues attacking even when wounded (no HP override)

// GlassCannon with charge ability, target stunned
var action = _aiService.DetermineAction(glassCannon, state);
// Scores: Charge Ability(50+20+20+30=120) ← Archetype + Charge + Stunned bonuses!
// Strongly favors devastating charged attack
```

**Narrative Impact:** High-risk enemies remain dangerous until eliminated, favoring damage output over self-preservation.

---

### UC-7: Cowardly Flee Behavior

```csharp
// Enemy with "Cowardly" tag at 50% HP
var action = _aiService.DetermineAction(cowardEnemy, state);
// Flee not evaluated (HP > 25% threshold)
// Normal action selection

// Same enemy at 20% HP
var action = _aiService.DetermineAction(cowardEnemy, state);
// Scores: Heavy(65), Flee(50+80=130) ← Cowardly + Low HP!
// Weighted selection strongly favors Flee
```

**Narrative Impact:** Cowardly enemies attempt escape when wounded, creating opportunities for finishing blows or chase decisions.

---

## Architecture Diagram

### Utility Scoring Flow

```
┌─────────────────────────────────┐
│  DetermineAction(enemy, state)  │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│  Check Chanting Status          │
│  (v0.2.4c charge abilities)     │
└────────────┬────────────────────┘
         ┌───┴───┐
         │       │
   Chanting?    No
         │       │
         ▼       ▼
    ┌────────┐  ┌──────────────────┐
    │ Pass / │  │ Build Scored     │
    │ Release│  │ Action List      │
    └────────┘  └────────┬─────────┘
                         │
         ┌───────────────┼───────────────┐
         │               │               │
         ▼               ▼               ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│ Evaluate    │ │ Evaluate    │ │ Evaluate    │
│ Attacks     │ │ Abilities   │ │ Defend/Flee │
└──────┬──────┘ └──────┬──────┘ └──────┬──────┘
       │               │               │
       └───────────────┴───────────────┘
                       │
                       ▼
              ┌────────────────┐
              │ Filter & Norm- │
              │ alize Scores   │
              └───────┬────────┘
                      │
                      ▼
              ┌────────────────┐
              │ Weighted Rand- │
              │ om Selection   │
              └───────┬────────┘
                      │
                      ▼
              ┌────────────────┐
              │ Return Action  │
              └────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IDiceService` | [SPEC-DICE-001](SPEC-DICE-001.md) | Weighted random selection for action choice |
| `IAttackResolutionService` | [SPEC-ATTACK-001](SPEC-ATTACK-001.md) | Stamina affordability checks |
| `IAbilityService` | [SPEC-ABILITY-001](SPEC-ABILITY-001.md) | Ability cooldown and usage validation |
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
| `EnemyAIService.cs` | Utility scoring logic, weighted selection |

### Supporting Types

| File | Purpose |
|------|------------|
| `CombatAction.cs` | Action record (Type, SourceId, TargetId, AttackType, AbilityId) |
| `EnemyArchetype.cs` | Archetype enum (Tank, DPS, GlassCannon, Support, Swarm, Caster, Boss) |
| `CombatState.cs` | Combat state with turn order for target lookup |

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

### Scoring Constants (Hardcoded)

All scoring constants are defined as `private const int` in `EnemyAIService.cs`:

| Constant Name | Value | Category |
|---------------|-------|----------|
| `BaseScore` | 50 | Base |
| `CriticalHpHealBonus` | 50 | Context |
| `WastefulHealPenalty` | -40 | Context |
| `KillRangeBonus` | 30 | Context |
| `StaminaConservationPenalty` | -20 | Context |
| `RedundantDebuffPenalty` | -100 | Context |
| `ArchetypeDamageBonus` | 20 | Archetype |
| `TankDefendBonus` | 25 | Archetype |
| `ChargeAbilityBonus` | 20 | Charge (v0.2.4c) |
| `ChargeLowHpPenalty` | -50 | Charge (v0.2.4c) |
| `ChargePlayerStunnedBonus` | 30 | Charge (v0.2.4c) |
| `MinimumActionScore` | 0 | Filter |

### HP Thresholds (Hardcoded)

| Threshold Name | Value | Usage |
|----------------|-------|-------|
| `LowHpThreshold` | 0.25f | Cowardly flee trigger |
| `WoundedThreshold` | 0.40f | Tank defend bonus |

---

## Testing

### Test Coverage

`EnemyAIServiceTests.cs` - 31 test methods covering:

1. **Scoring Constant Validation** (8 tests)
   - Base score applied to all action candidates
   - Attack type bonuses (Heavy +15, Standard +5, Light -10)
   - Context modifiers (CriticalHpHealBonus, WastefulHealPenalty, etc.)
   - Archetype modifiers (TankDefendBonus, GlassCannonDamageBonus, etc.)

2. **Archetype Behavior Tendencies** (7 tests)
   - Tank favors defend when wounded (score bonuses applied)
   - GlassCannon favors damage abilities
   - Support favors healing when wounded
   - Boss balanced scoring across action types
   - Swarm light attack bonus applied

3. **Charge Ability Handling** (5 tests)
   - Charge abilities receive ChargeAbilityBonus (+20)
   - Low HP penalty applied (ChargeLowHpPenalty -50 at < 30% HP)
   - Stunned target bonus (ChargePlayerStunnedBonus +30)
   - Charge action returned when channeling

4. **Weighted Selection** (6 tests)
   - Score normalization produces valid probabilities
   - Zero/negative scores excluded from selection
   - Single viable action selected deterministically
   - Multiple actions selected proportionally to scores

5. **Edge Cases** (5 tests)
   - 0 stamina → Defend only viable action
   - All negative scores → forced Defend fallback
   - RedundantDebuffPenalty eliminates duplicate statuses
   - Cowardly flee scoring at low HP

---

## Design Rationale

### Why Utility Scoring Over Decision Trees?

**Decision:** Use utility-based scoring with weighted random selection instead of deterministic d100 probability tables.

**Rationale:**
- **Context Awareness:** Scores dynamically adjust based on HP, stamina, target state, abilities available
- **Soft Preferences:** Archetype tendencies expressed as score bonuses, not hard overrides
- **Emergent Behavior:** Same archetype can behave differently based on combat state
- **Extensibility:** New scoring factors added without refactoring decision trees

**Trade-off:** Less predictable than fixed percentages, but more adaptive and realistic combat AI.

---

### Why Archetypes as Score Modifiers?

**Decision:** Archetypes apply score bonuses rather than selecting from separate decision trees.

**Rationale:**
- **Unified Evaluation:** All actions scored by same algorithm, archetypes just tune weights
- **Predictability:** Players learn archetype tendencies while AI retains situational flexibility
- **Maintainability:** Adding new archetype = add new modifier constants, no new evaluation paths
- **Balance:** Easy to tune archetype behavior by adjusting single constants

**Trade-off:** Requires careful constant tuning for distinct archetype personalities.

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

### v2.0.0 (Utility Scoring Architecture)

**Breaking Changes:**
- **REPLACED** d100 probability tables with utility-based scoring system
- **REPLACED** archetype decision trees with score modifier constants
- **REMOVED** fixed percentage distributions (e.g., "70% Standard, 30% Heavy")

**Added:**
- Utility scoring constants (BaseScore, attack bonuses, context modifiers)
- Weighted random selection algorithm
- Archetype score modifiers (TankDefendBonus, GlassCannonDamageBonus, etc.)
- Charge ability handling (v0.2.4c) with ChargeAbilityBonus, ChargeLowHpPenalty
- Context-aware scoring (HP thresholds, stamina conservation, target state)
- 31 test methods covering scoring, selection, and charge abilities

**Design Decisions:**
- Utility scoring over decision trees for context-aware behavior
- Archetypes as score modifiers for unified evaluation
- Soft preferences via bonuses instead of hard overrides
- Weighted random selection for probabilistic variety

**Integration Points:**
- `CombatService.ProcessEnemyTurn()` calls `DetermineAction()`
- `AbilityService` provides ability evaluation for scoring
- `Combatant.ActiveAbilities` consumed for ability candidate generation

---

### v1.0.0 (Initial Implementation - Superseded)

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
