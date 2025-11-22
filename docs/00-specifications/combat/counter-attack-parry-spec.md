# SPEC-COMBAT-006: Counter-Attack & Parry System

**Version**: 1.0
**Status**: Completed
**Last Updated**: 2025-11-22
**Implemented In**: v0.21.4

---

## Executive Summary

### Purpose

The Counter-Attack & Parry System provides a universal reactive defense mechanism that allows all characters to actively defend against incoming attacks through skilled parrying, with the potential to execute devastating ripostes (counter-attacks) based on parry quality. This system transforms defense from a passive attribute into an active tactical choice, rewarding player skill and creating dramatic combat moments where a single perfect parry can turn the tide of battle.

### Scope

This specification covers:
- **Universal Parry Mechanics**: Core parry pool calculation, outcome determination, and action economy
- **Parry Quality Tiers**: Four outcome levels (Failed, Standard, Superior, Critical) with distinct effects
- **Riposte System**: Free counter-attacks triggered by high-quality parries
- **Specialization Integration**: Hólmgangr Reactive Parry and Atgeir-wielder parry bonuses
- **Trauma Economy Integration**: Stress relief/gain based on parry outcomes
- **Statistics and Persistence**: Long-term tracking of parry performance and combat effectiveness

### Success Criteria

A successful implementation must:
1. ✅ Provide all characters with baseline parry capability (1 parry per round)
2. ✅ Create clear mechanical distinctions between parry quality tiers
3. ✅ Trigger ripostes appropriately based on parry outcome and character abilities
4. ✅ Integrate seamlessly with existing combat, specialization, and trauma systems
5. ✅ Track parry statistics persistently for player feedback and balance tuning
6. ✅ Reward skilled defensive play without overshadowing offensive tactics

### Key Metrics

| Metric | Target | Actual (v0.21.4) |
|--------|--------|------------------|
| Baseline Parry Success Rate | 40-60% | ~55% (playtesting) |
| Critical Parry Rate | 10-15% | ~12% (playtesting) |
| Riposte Damage Contribution | 15-25% of total player damage | ~20% (Hólmgangr) |
| Parry Limit (Standard) | 1/round | 1/round |
| Parry Limit (Hólmgangr Rank 3) | 2/round | 2/round |
| Database Tables | 3 (Statistics, Bonuses, Attempts) | 3 |

---

## Related Documentation

### Dependencies

**Upstream Systems** (must exist before this system):
- **SPEC-CORE-001: Core Attributes System** - FINESSE attribute for parry pool
- **SPEC-COMBAT-001: Core Combat Mechanics** - Attack accuracy calculation
- **SPEC-COMBAT-003: Weapon & Equipment System** - Weapon skill integration
- **SPEC-PROGRESSION-001: Archetype & Specialization System** - Hólmgangr Reactive Parry
- **SPEC-MENTAL-001: Trauma Economy System** - Stress integration

**Downstream Systems** (depend on this system):
- Future specialization bonuses (shields, defensive weapons)
- Potential enemy parrying mechanics
- Advanced reactive abilities (Skjaldmær Ally Parry)

### Code References

**Primary Implementation**:
- `/RuneAndRust.Engine/CounterAttackService.cs` (578 lines) - Core parry/riposte logic
- `/RuneAndRust.Core/CounterAttack.cs` (92 lines) - Data models and enums
- `/RuneAndRust.Persistence/CounterAttackRepository.cs` (418 lines) - Database persistence
- `/RuneAndRust.Engine/Commands/ParryCommand.cs` (95 lines) - Player command interface

**Integration Points**:
- `/RuneAndRust.Engine/CombatEngine.cs:594-650` - PrepareParry() method
- `/RuneAndRust.Engine/CombatEngine.cs:23,47` - CounterAttackService dependency injection
- `/RuneAndRust.Core/PlayerCharacter.cs` - ParriesRemainingThisTurn, ParryReactionPrepared fields

**Documentation**:
- `/COUNTER_ATTACK_INTEGRATION.md` (454 lines) - Integration guide and examples

### Layer 1 Documents

- `v2.0 Parry System Specification` - Canonical parry mechanics
- `v2.0 Hólmgangr Reactive Parry Specification` - Specialization design
- `v0.15 Trauma Economy Specification` - Stress integration design
- `v0.21 Advanced Combat Mechanics` - Combat system extensions

---

## Design Philosophy

The Counter-Attack & Parry System is built on five core principles:

### 1. **Universal Accessibility with Specialized Mastery**

**Principle**: All characters can parry, but specialists excel.

**Design Rationale**: Every character should have access to reactive defense as a fundamental combat option, preventing parrying from becoming a "Hólmgangr-only" mechanic. However, specialized builds (Hólmgangr Reactive Parry, Atgeir-wielders) gain significant advantages through bonus dice and expanded capabilities, rewarding investment in defensive playstyles.

**Implementation**:
- Base parry pool (FINESSE + Weapon Skill) available to all characters
- 1 parry per round for all characters by default
- Critical parries (5+ margin) allow ALL characters to riposte
- Specializations add bonus d10s and Superior Riposte capability

### 2. **Quality-Based Outcomes Over Binary Success**

**Principle**: Parry effectiveness exists on a spectrum, not pass/fail.

**Design Rationale**: A binary "parry succeeds or fails" system lacks drama and tactical depth. The four-tier outcome system (Failed/Standard/Superior/Critical) creates memorable moments, rewards exceptional rolls, and provides varied tactical consequences. A barely-successful parry (Standard) blocks damage but feels different from a perfect deflection (Critical) that opens riposte opportunities.

**Implementation**:
- Failed (Parry < Accuracy): Attack hits, stress penalty
- Standard (Parry = Accuracy): Attack blocked, no stress change
- Superior (Parry > Accuracy by 1-4): Attack deflected, stress relief, possible riposte
- Critical (Parry > Accuracy by 5+): Perfect deflection, major stress relief, guaranteed riposte

### 3. **Integrated Risk-Reward with Trauma Economy**

**Principle**: Parrying is high-risk, high-reward through stress mechanics.

**Design Rationale**: Parrying consumes a limited resource (1-2 attempts per round) and carries psychological consequences. Failed parries generate stress (+5) representing the fear of being overwhelmed, while successful parries relieve stress (-3 to -8) representing confidence and control. Riposte kills provide massive stress relief (-10), rewarding aggressive defensive play.

**Implementation**:
- Failed Parry: +5 Stress (punishment for failure)
- Superior Parry: -3 Stress (relief from successful defense)
- Critical Parry: -8 Stress (confidence from perfect technique)
- Riposte Kill: -10 Stress (triumph over adversary)
- Riposte Miss: +3 Stress (frustration from wasted opportunity)

### 4. **Specialization as Capability Expansion**

**Principle**: Specializations unlock new capabilities, not just bigger numbers.

**Design Rationale**: Rather than making Hólmgangr "better at the same thing," Reactive Parry fundamentally expands what's possible. Superior Riposte allows counter-attacks on Superior parries (not just Critical), and Rank 3 Mastery doubles parry attempts per round. This creates qualitatively different gameplay for defensive specialists.

**Implementation**:
- Hólmgangr Rank 1: +1d10 bonus dice, Superior Riposte enabled
- Hólmgangr Rank 2: +2d10 bonus dice (higher consistency)
- Hólmgangr Rank 3: 2 parries per round (double defensive capacity)
- Atgeir-wielder: +1d10 bonus dice (reach weapon advantage)

### 5. **Data-Driven Balance Through Persistent Tracking**

**Principle**: Statistics enable balance tuning and player mastery.

**Design Rationale**: The system tracks comprehensive statistics (total attempts, success rate, outcome distribution, riposte effectiveness) to support both game design (identifying balance issues) and player mastery (understanding performance trends). This data persistence allows developers to tune difficulty and players to measure improvement.

**Implementation**:
- ParryStatistics table: Persistent character-level statistics
- ParryAttempts table: Combat log for detailed analysis
- Success rate calculation: (SuccessfulParries / TotalParryAttempts) × 100%
- Riposte effectiveness: (RiposteKills / RipostesLanded) ratio

---

## System Overview

### System Lifecycle

The parry system operates within the standard combat flow:

```
TURN START
    ↓
┌─────────────────────────────┐
│ 1. Reset Parry Availability │ ← CounterAttackService.ResetParriesForNewTurn()
│    - Restore parries (1-2)  │
│    - Clear prepared state    │
└─────────────────────────────┘
    ↓
┌─────────────────────────────┐
│ 2. Player Chooses Action    │
│    - Attack                  │
│    - Defend                  │
│  ► PARRY (prepare reaction) │ ← Player types "parry" command
│    - Ability                 │
│    - Item                    │
└─────────────────────────────┘
    ↓
┌─────────────────────────────┐
│ 3. Parry Preparation         │ ← ParryCommand.Execute()
│    - Validate parry limit    │     CombatEngine.PrepareParry()
│    - Set prepared flag       │
│    - Display parry pool info │
└─────────────────────────────┘
    ↓
ENEMY TURN
    ↓
┌─────────────────────────────┐
│ 4. Enemy Attack Declared     │
│    - Roll attack accuracy    │
│    - Check for prepared parry│ ← Check player.ParryReactionPrepared
└─────────────────────────────┘
    ↓
┌─────────────────────────────┐
│ 5. Execute Parry Attempt     │ ← CounterAttackService.ExecuteParry()
│    - Calculate parry pool    │     • CalculateParryPool()
│    - Determine outcome       │     • DetermineParryOutcome()
│    - Consume parry attempt   │     • ConsumeParryAttempt()
└─────────────────────────────┘
    ↓
┌─────────────────────────────┐
│ 6. Apply Parry Result        │
│    - Success: Block damage   │
│    - Failure: Take damage    │
│    - Apply stress change     │
└─────────────────────────────┘
    ↓
┌─────────────────────────────┐
│ 7. Check Riposte Trigger     │ ← CanRiposte()
│    - Critical: Always        │
│    - Superior: If has bonus  │
└─────────────────────────────┘
    ↓ (if triggered)
┌─────────────────────────────┐
│ 8. Execute Riposte           │ ← ExecuteRiposte()
│    - Free melee attack       │
│    - Apply damage (if hit)   │
│    - Check for kill          │
│    - Apply stress effects    │
└─────────────────────────────┘
    ↓
┌─────────────────────────────┐
│ 9. Update Statistics         │ ← CounterAttackRepository
│    - Increment counters      │     • UpdateParryStatistics()
│    - Record combat log       │     • RecordParryAttempt()
└─────────────────────────────┘
    ↓
TURN END
```

### Parry Action Economy

Parrying operates as a **Reaction** (not a standard action):

| Timing | Action Cost | Resource Cost | Effect |
|--------|-------------|---------------|--------|
| **Preparation Phase** | Player's standard action | None | Sets `ParryReactionPrepared = true` |
| **Reactive Phase** | Free (automatic on enemy attack) | 1 parry attempt | Executes parry roll, potentially blocks damage |
| **Riposte Phase** | Free (if triggered) | None | Free melee attack (cannot be parried) |

**Key Design Notes**:
- Parrying uses the player's **standard action** to prepare (trade-off vs attacking)
- The actual parry attempt is **reactive** (triggers automatically on next enemy attack)
- Ripostes are **completely free** (no action cost, bonus damage)
- Parry limit resets each turn (1-2 attempts, depending on specialization)

### Parry vs Other Defensive Actions

| Defensive Option | Action Cost | Success Rate | Damage Reduction | Offensive Potential | Stress Impact |
|------------------|-------------|--------------|------------------|---------------------|---------------|
| **Parry** | Standard action (prepare) | ~55% (variable) | 100% (on success) | Riposte on Superior/Critical | High variance (-8 to +5) |
| **Defend** | Standard action | ~100% (always applies) | 25-50% (partial) | None | Minimal |
| **Dodge** | Movement + action | ~60-70% | 100% (on success) | None | None |
| **Armor (Passive)** | None (passive) | 100% (always applies) | Soak value (fixed) | None | None |

**Parry's Niche**: High-risk, high-reward active defense with potential for offensive counterplay. Best when:
- Facing single powerful attacks (not swarms)
- Playing Hólmgangr or defensive specialist
- Seeking stress relief through successful defense
- Attempting to conserve HP through complete damage prevention

---

## FR-001: Universal Parry Mechanics

**Status**: ✅ Implemented (v0.21.4)
**Code Reference**: `CounterAttackService.cs:44-202`

### FR-001.1: Parry Pool Calculation

**Requirement**: All characters must be able to calculate a Parry Pool based on FINESSE, weapon skill, and applicable bonuses.

**Formula** (v2.0 Canonical):
```
Parry Pool = FINESSE + Weapon Skill + Roll(Bonus Dice × d10)
```

**Component Breakdown**:

| Component | Source | Value Range | Notes |
|-----------|--------|-------------|-------|
| **FINESSE** | Character attribute | 1-8 | Primary defensive attribute |
| **Weapon Skill** | Equipped weapon | 1-5 | Base 3 for equipped weapons, 1 for unarmed |
| **Bonus Dice** | Specializations/Equipment | 0-2 d10s | Hólmgangr: +1d10 (Rank 1), +2d10 (Rank 2+); Atgeir: +1d10 |
| **Bonus Roll** | Rolled each parry | 0-20 (avg ~10) | Variable component, creates unpredictability |

**Implementation** (`CounterAttackService.cs:128-153`):
```csharp
public int CalculateParryPool(PlayerCharacter character)
{
    // Base: FINESSE + Weapon Skill
    int finesse = character.Attributes.Finesse;
    int weaponSkill = GetWeaponSkill(character);
    int basePool = finesse + weaponSkill;

    // Get bonus dice from specializations/equipment
    int bonusDice = GetParryBonusDice(character);

    // Roll bonus dice (d10s)
    int bonusRoll = 0;
    if (bonusDice > 0)
    {
        bonusRoll = _diceService.Roll(bonusDice, 10);
    }

    int totalPool = basePool + bonusRoll;
    return totalPool;
}
```

**Example Calculations**:

**Example 1: Base Character (no specialization)**
- FINESSE: 4
- Weapon Skill: 3 (equipped sword)
- Bonus Dice: 0
- **Parry Pool**: 4 + 3 + 0 = **7**

**Example 2: Hólmgangr Rank 1**
- FINESSE: 5
- Weapon Skill: 3
- Bonus Dice: 1d10 → rolls **7**
- **Parry Pool**: 5 + 3 + 7 = **15**

**Example 3: Hólmgangr Rank 2 (Expert)**
- FINESSE: 6
- Weapon Skill: 3
- Bonus Dice: 2d10 → rolls **6 + 8** = 14
- **Parry Pool**: 6 + 3 + 14 = **23**

**Example 4: Atgeir-wielder (no Reactive Parry)**
- FINESSE: 4
- Weapon Skill: 3
- Bonus Dice: 1d10 → rolls **5**
- **Parry Pool**: 4 + 3 + 5 = **12**

### FR-001.2: Parry Outcome Determination

**Requirement**: The system must categorize parry attempts into four quality tiers based on the margin between the parry pool and attack accuracy.

**Outcome Tiers** (v2.0 Canonical):

| Outcome | Condition | Damage Blocked | Riposte Eligibility | Stress Change | Description |
|---------|-----------|----------------|---------------------|---------------|-------------|
| **Failed** | Parry < Accuracy | 0% (attack hits) | None | +5 Stress | Parry overwhelmed, defender off-balance |
| **Standard** | Parry = Accuracy | 100% | None (Critical only) | 0 Stress | Attack blocked, no opening created |
| **Superior** | Parry > Accuracy (by 1-4) | 100% | Hólmgangr only | -3 Stress | Attack deflected, small opening |
| **Critical** | Parry > Accuracy (by 5+) | 100% | All characters | -8 Stress | Perfect deflection, major opening |

**Implementation** (`CounterAttackService.cs:163-175`):
```csharp
public ParryOutcome DetermineParryOutcome(int parryRoll, int accuracyRoll)
{
    int difference = parryRoll - accuracyRoll;

    if (difference < 0)
        return ParryOutcome.Failed;
    else if (difference == 0)
        return ParryOutcome.Standard;
    else if (difference >= CriticalParryThreshold) // 5+
        return ParryOutcome.Critical;
    else
        return ParryOutcome.Superior; // 1-4
}
```

**Critical Threshold**: `5` (constant, defined in `CounterAttackService.cs:22`)

**Design Notes**:
- **Standard Parry** (exact tie) is intentionally rare, creating tension in close rolls
- **5-point threshold** for Critical ensures meaningful separation between Superior and Critical
- Failed parries still consume the parry attempt (no retry)

### FR-001.3: Parry Execution Flow

**Requirement**: Parry attempts must follow a standardized execution flow that integrates with combat state, statistics tracking, and trauma economy.

**Execution Steps** (`CounterAttackService.cs:55-122`):

```csharp
public ParryResult ExecuteParry(
    PlayerCharacter defender,
    Enemy attacker,
    int attackAccuracy,
    int combatInstanceId = 0,
    string? attackAbility = null)
{
    // 1. Initialize result object
    var result = new ParryResult { /* ... */ };

    // 2. Calculate Parry Pool
    int parryPool = CalculateParryPool(defender);
    result.ParryRoll = parryPool;

    // 3. Determine Parry Outcome
    result.Outcome = DetermineParryOutcome(parryPool, attackAccuracy);
    result.Success = result.Outcome != ParryOutcome.Failed;

    // 4. Check for Riposte
    if (CanRiposte(defender, result.Outcome))
    {
        result.RiposteTriggered = true;
        result.Riposte = ExecuteRiposte(defender, attacker);
    }

    // 5. Update statistics
    _repository.UpdateParryStatistics(result);

    // 6. Record combat log (if in combat)
    if (combatInstanceId > 0)
    {
        _repository.RecordParryAttempt(new ParryAttempt { /* ... */ });
    }

    // 7. Apply trauma economy effects
    result.StressChange = ApplyTraumaEconomyEffects(defender, result);

    return result;
}
```

**ParryResult Structure** (`CounterAttack.cs:17-29`):
```csharp
public class ParryResult
{
    public bool Success { get; set; }                // True if outcome != Failed
    public ParryOutcome Outcome { get; set; }        // Failed/Standard/Superior/Critical
    public int ParryRoll { get; set; }               // Calculated parry pool
    public int AccuracyRoll { get; set; }            // Enemy's attack accuracy
    public bool RiposteTriggered { get; set; }       // True if riposte executed
    public RiposteResult? Riposte { get; set; }      // Riposte details (if triggered)
    public int StressChange { get; set; }            // Net stress change (-8 to +5)
    public int DefenderID { get; set; }              // Database ID
    public int AttackerID { get; set; }              // Database ID
    public string? AttackAbility { get; set; }       // Attack name (for logging)
}
```

### FR-001.4: Parry Limit and Turn Management

**Requirement**: Characters must be limited in the number of parries they can attempt per round, with limits resetting at turn boundaries.

**Parry Limits**:

| Character Type | Parries Per Round | Source |
|----------------|-------------------|--------|
| **Standard** (all characters) | 1 | Base parry limit (constant) |
| **Hólmgangr Rank 3** (Mastery) | 2 | Reactive Parry specialization |
| **Future**: Shield-wielders | 1-2 (potential) | Equipment bonus (not yet implemented) |

**PlayerCharacter State Fields** (`PlayerCharacter.cs`):
```csharp
// v0.21.4: Counter-Attack System (Parry tracking)
public int ParriesRemainingThisTurn { get; set; } = 1; // Resets each turn
public bool ParryReactionPrepared { get; set; } = false; // Whether parry is prepared
```

**Turn Management Methods** (`CounterAttackService.cs:439-469`):

**Reset Parries** (called at turn start):
```csharp
public void ResetParriesForNewTurn(PlayerCharacter character)
{
    int parriesPerRound = GetParriesPerRound(character);
    character.ParriesRemainingThisTurn = parriesPerRound;
    character.ParryReactionPrepared = false;
}
```

**Check Parry Availability**:
```csharp
public bool CanParryThisTurn(PlayerCharacter character)
{
    return character.ParriesRemainingThisTurn > 0;
}
```

**Consume Parry Attempt**:
```csharp
public void ConsumeParryAttempt(PlayerCharacter character)
{
    if (character.ParriesRemainingThisTurn > 0)
    {
        character.ParriesRemainingThisTurn--;
    }
}
```

**Get Parries Per Round**:
```csharp
public int GetParriesPerRound(PlayerCharacter character)
{
    var bonuses = _repository.GetParryBonuses(GetCharacterId(character));

    if (bonuses.Count == 0)
        return BaseParryLimit; // 1

    return bonuses.Max(b => b.ParriesPerRound);
}
```

**Integration with CombatEngine** (`CombatEngine.cs:594-650`):

**Parry Preparation** (player action):
```csharp
public void PrepareParry(CombatState combatState)
{
    var player = combatState.Player;

    // Check if can parry this turn
    if (!_counterAttackService.CanParryThisTurn(player))
    {
        int maxParries = _counterAttackService.GetParriesPerRound(player);
        combatState.AddLogEntry($"You have no parry attempts remaining this turn! (Max: {maxParries})");
        return;
    }

    // Mark parry as prepared
    player.ParryReactionPrepared = true;

    combatState.AddLogEntry("You prepare to parry the next attack!");
    combatState.AddLogEntry($"  Parries remaining: {player.ParriesRemainingThisTurn - 1}/{_counterAttackService.GetParriesPerRound(player)}");

    // Display parry pool info
    int parryBonusDice = _counterAttackService.GetParryBonusDice(player);
    if (parryBonusDice > 0)
    {
        combatState.AddLogEntry($"  Parry bonus: +{parryBonusDice}d10");
    }
}
```

**Design Notes**:
- Parry limit is a **per-round** limit (not per-combat)
- Preparing a parry does NOT consume the limit; only the actual parry attempt does
- Hólmgangr Rank 3 Mastery is the only current source of 2 parries/round
- Future equipment (shields, parrying daggers) could add additional parry limits

### FR-001.5: Parry Command Interface

**Requirement**: Players must be able to prepare parries through a simple, intuitive command interface.

**Command Syntax**:
```
parry
p      (alias)
```

**Command Implementation** (`ParryCommand.cs:23-79`):
```csharp
public CommandResult Execute(GameState state, string[] args)
{
    // Validation 1: Must be in combat
    if (state.CurrentPhase != GamePhase.Combat || state.Combat == null)
        return CommandResult.Failure("You can only parry during combat.");

    var combat = state.Combat;

    // Validation 2: Combat must be active
    if (!combat.IsActive)
        return CommandResult.Failure("Combat has ended.");

    // Validation 3: Player must have a turn
    if (!IsPlayerTurn(combat))
        return CommandResult.Failure("Wait for your turn!");

    // Clear combat log for this action
    combat.ClearLogForNewAction();

    // Execute parry preparation via CombatEngine
    _combatEngine.PrepareParry(combat);

    // Get combat log output
    var result = new StringBuilder();
    foreach (var logEntry in combat.CombatLog)
    {
        result.AppendLine(logEntry);
    }

    return CommandResult.Success(result.ToString());
}
```

**Example Output** (Standard character):
```
> parry

You prepare to parry the next attack!
  Parries remaining: 0/1
```

**Example Output** (Hólmgangr Rank 1 with Reactive Parry):
```
> parry

You prepare to parry the next attack!
  Parries remaining: 0/1
  Parry bonus: +1d10
  [Reactive Parry]: Superior parries trigger Riposte!
```

**Example Output** (Hólmgangr Rank 3 Mastery):
```
> parry

You prepare to parry the next attack!
  Parries remaining: 1/2
  Parry bonus: +2d10
  [Reactive Parry]: Superior parries trigger Riposte!
```

**Example Output** (No parries remaining):
```
> parry

You have no parry attempts remaining this turn! (Max: 1)
```

**Validation Rules**:
1. Must be in combat phase (not exploration, dialogue, etc.)
2. Combat must be active (not ended)
3. Must be player's turn (not enemy's turn)
4. Player must have parry attempts remaining

---

## FR-002: Riposte and Counter-Attack System

**Status**: ✅ Implemented (v0.21.4)
**Code Reference**: `CounterAttackService.cs:204-260`

### FR-002.1: Riposte Trigger Conditions

**Requirement**: Ripostes must trigger automatically based on parry outcome quality and character abilities.

**Trigger Matrix**:

| Parry Outcome | Standard Characters | Hólmgangr (Reactive Parry) | Atgeir-wielder (no Reactive Parry) |
|---------------|---------------------|----------------------------|-------------------------------------|
| **Failed** | No riposte | No riposte | No riposte |
| **Standard** | No riposte | No riposte | No riposte |
| **Superior** | **No riposte** | **✓ Riposte** | No riposte |
| **Critical** | **✓ Riposte** | **✓ Riposte** | **✓ Riposte** |

**Key Design Points**:
- **Critical Parries** are the universal riposte trigger (all characters)
- **Superior Parries** only trigger ripostes for characters with Superior Riposte ability (Hólmgangr exclusive in v0.21.4)
- Standard parries never trigger ripostes (insufficient opening)
- Failed parries never trigger ripostes (defender off-balance)

**Implementation** (`CounterAttackService.cs:183-200`):
```csharp
public bool CanRiposte(PlayerCharacter character, ParryOutcome outcome)
{
    // Critical parries always trigger riposte for all characters
    if (outcome == ParryOutcome.Critical)
    {
        return true;
    }

    // Superior parries only trigger for characters with Superior Riposte bonus
    if (outcome == ParryOutcome.Superior && HasSuperiorRiposte(character))
    {
        return true;
    }

    return false;
}
```

**Superior Riposte Check** (`CounterAttackService.cs:296-300`):
```csharp
public bool HasSuperiorRiposte(PlayerCharacter character)
{
    var bonuses = _repository.GetParryBonuses(GetCharacterId(character));
    return bonuses.Any(b => b.AllowsSuperiorRiposte);
}
```

**ParryBonus Structure** (`CounterAttack.cs:65-73`):
```csharp
public class ParryBonus
{
    public int BonusID { get; set; }
    public int CharacterID { get; set; }
    public string Source { get; set; } = string.Empty; // 'Reactive Parry', 'Equipment', 'Buff'
    public int BonusDice { get; set; } = 0; // Extra d10s to Parry Pool
    public bool AllowsSuperiorRiposte { get; set; } = false; // ← KEY FIELD
    public int ParriesPerRound { get; set; } = 1;
}
```

### FR-002.2: Riposte Execution

**Requirement**: Ripostes must execute as free, unavoidable melee attacks that respect standard damage rules.

**Riposte Characteristics**:
- **Action Cost**: Free (no action economy cost)
- **Attack Type**: Basic melee attack
- **Hit Formula**: FINESSE + Weapon Skill + d10 vs Enemy Defense
- **Damage Formula**: Weapon Damage + MIGHT modifier
- **Cannot Be Parried**: Ripostes cannot be countered (v2.0 canonical)
- **Respects Armor**: Riposte damage is reduced by enemy Soak (standard damage rules)

**Implementation** (`CounterAttackService.cs:210-260`):
```csharp
public RiposteResult ExecuteRiposte(PlayerCharacter attacker, Enemy target)
{
    var result = new RiposteResult();

    // Riposte is a free basic melee attack (v2.0 canonical)
    // Hit Roll: FINESSE + Weapon Skill + d10
    int finesse = attacker.Attributes.Finesse;
    int weaponSkill = GetWeaponSkill(attacker);
    int rollBonus = _diceService.Roll(1, 10);
    int attackRoll = finesse + weaponSkill + rollBonus;

    int defenseScore = target.Defense;

    result.AttackRoll = attackRoll;
    result.DefenseScore = defenseScore;

    // Check if hit
    if (attackRoll >= defenseScore)
    {
        result.Hit = true;

        // Calculate damage: Weapon Damage + MIGHT modifier
        int weaponDamage = GetWeaponDamage(attacker);
        int mightBonus = attacker.Attributes.Might;
        int totalDamage = weaponDamage + mightBonus;

        // Apply damage (riposte respects armor, as per standard melee attacks)
        int effectiveDamage = ApplyDamageToEnemy(target, totalDamage);
        result.DamageDealt = effectiveDamage;

        // Check if target killed
        result.KilledTarget = target.HP <= 0;
    }
    else
    {
        result.Hit = false;
    }

    return result;
}
```

**Damage Application** (`CounterAttackService.cs:265-278`):
```csharp
private int ApplyDamageToEnemy(Enemy target, int damage)
{
    // Apply Soak (armor)
    int effectiveSoak = Math.Max(0, target.Soak - (target.CorrodedStacks * 2));
    int effectiveDamage = Math.Max(0, damage - effectiveSoak);

    // Apply damage
    target.HP -= effectiveDamage;

    return effectiveDamage;
}
```

**RiposteResult Structure** (`CounterAttack.cs:34-41`):
```csharp
public class RiposteResult
{
    public bool Hit { get; set; }              // True if attack roll >= defense
    public int DamageDealt { get; set; }       // Effective damage (after Soak)
    public bool KilledTarget { get; set; }     // True if target HP <= 0
    public int AttackRoll { get; set; }        // FINESSE + Weapon Skill + d10
    public int DefenseScore { get; set; }      // Enemy Defense value
}
```

### FR-002.3: Riposte Damage Calculation Examples

**Example 1: Standard Riposte (Critical Parry, No Specialization)**
```
Character:
  FINESSE: 4
  MIGHT: 3
  Weapon: Longsword (1d8 damage)
  Weapon Skill: 3

Riposte Attack Roll:
  FINESSE (4) + Weapon Skill (3) + d10 (rolls 6) = 13

Enemy:
  Defense: 10
  Soak: 4
  HP: 25

Hit Check: 13 >= 10 → HIT
Damage Roll: 1d8 (rolls 5) + MIGHT (3) = 8 damage
Effective Damage: 8 - 4 (Soak) = 4 damage
Enemy HP: 25 → 21
```

**Example 2: Hólmgangr Riposte (Superior Parry, Reactive Parry Active)**
```
Character (Hólmgangr Rank 2):
  FINESSE: 6
  MIGHT: 5
  Weapon: Atgeir (1d10 damage)
  Weapon Skill: 4

Riposte Attack Roll:
  FINESSE (6) + Weapon Skill (4) + d10 (rolls 8) = 18

Enemy (Aetheric Aberration):
  Defense: 12
  Soak: 2
  HP: 18

Hit Check: 18 >= 12 → HIT
Damage Roll: 1d10 (rolls 9) + MIGHT (5) = 14 damage
Effective Damage: 14 - 2 (Soak) = 12 damage
Enemy HP: 18 → 6
```

**Example 3: Riposte Miss (Critical Parry, Low Roll)**
```
Character:
  FINESSE: 3
  MIGHT: 4
  Weapon: Axe (1d10 damage)
  Weapon Skill: 3

Riposte Attack Roll:
  FINESSE (3) + Weapon Skill (3) + d10 (rolls 2) = 8

Enemy (Armored Enforcer):
  Defense: 14
  Soak: 6
  HP: 30

Hit Check: 8 < 14 → MISS
Damage: 0 (attack did not connect)
Stress Penalty: +3 Stress (riposte miss frustration)
```

**Example 4: Riposte Kill (Critical Parry, Finishing Blow)**
```
Character (Hólmgangr Rank 1):
  FINESSE: 5
  MIGHT: 4
  Weapon: Longsword (1d8 damage)
  Weapon Skill: 3

Riposte Attack Roll:
  FINESSE (5) + Weapon Skill (3) + d10 (rolls 7) = 15

Enemy (Wounded Scavenger):
  Defense: 9
  Soak: 1
  HP: 5 (already damaged)

Hit Check: 15 >= 9 → HIT
Damage Roll: 1d8 (rolls 6) + MIGHT (4) = 10 damage
Effective Damage: 10 - 1 (Soak) = 9 damage
Enemy HP: 5 → -4 (KILLED)
Stress Relief: -10 Stress (riposte kill triumph)
```

### FR-002.4: Riposte Integration with Combat Flow

**Requirement**: Ripostes must execute immediately after parry resolution, within the same enemy attack action.

**Combat Flow Timeline**:

```
ENEMY TURN: Enemy attacks player
    ↓
[1] Enemy rolls attack accuracy (e.g., 12)
    ↓
[2] Check if player has ParryReactionPrepared == true
    ↓
[3] Execute parry: CalculateParryPool() → 17
    ↓
[4] Determine outcome: 17 > 12 by 5 → Critical Parry
    ↓
[5] Display parry result to player:
    "✦ CRITICAL PARRY! ✦"
    "Parry Roll: 17 vs Accuracy: 12"
    "Attack completely deflected!"
    ↓
[6] Check riposte trigger: CanRiposte() → True (Critical)
    ↓
[7] Execute riposte IMMEDIATELY:
    "⚔ RIPOSTE! You counter-attack!"
    Attack Roll: 15 vs Defense: 10 → HIT
    Damage: 8 damage → Enemy HP: 25 → 17
    ↓
[8] Apply stress effects:
    Critical Parry: -8 Stress
    Riposte Hit: +0 Stress
    Net: -8 Stress
    ↓
[9] Consume parry attempt (ParriesRemainingThisTurn--)
    ↓
[10] Update statistics (ParryStatistics, ParryAttempts)
    ↓
ENEMY TURN ENDS (no damage dealt to player, riposte damage dealt to enemy)
```

**Integration Point** (CombatEngine, enemy attack handling):
```csharp
// Pseudocode from COUNTER_ATTACK_INTEGRATION.md
public void EnemyAttack(CombatState combatState, Enemy enemy)
{
    // ... calculate attack roll ...

    // Check if player has parry prepared
    if (player.ParryReactionPrepared)
    {
        // Execute parry
        var parryResult = _counterAttackService.ExecuteParry(
            player, enemy, attackRoll, combatState.CombatInstanceID);

        // Consume parry
        _counterAttackService.ConsumeParryAttempt(player);
        player.ParryReactionPrepared = false;

        // Display parry outcome
        DisplayParryOutcome(parryResult);

        // Handle Riposte (already executed in ExecuteParry)
        if (parryResult.RiposteTriggered && parryResult.Riposte != null)
        {
            if (parryResult.Riposte.Hit)
            {
                combatState.AddLogEntry($"⚔ RIPOSTE! Your counter-attack strikes for {parryResult.Riposte.DamageDealt} damage!");

                if (parryResult.Riposte.KilledTarget)
                {
                    combatState.AddLogEntry($"✦ {enemy.Name} is destroyed by your riposte! ✦");
                    // Remove enemy from combat
                }
            }
            else
            {
                combatState.AddLogEntry($"Your riposte misses! ({parryResult.Riposte.AttackRoll} vs Defense {parryResult.Riposte.DefenseScore})");
            }
        }

        // If parry succeeded, skip normal damage
        if (parryResult.Success)
        {
            return; // Attack was parried - no damage dealt
        }
    }

    // ... normal damage calculation if parry failed or wasn't used ...
}
```

### FR-002.5: Riposte Cannot Be Parried

**Requirement**: Enemy targets cannot parry ripostes (v2.0 canonical rule).

**Design Rationale**:
Ripostes represent instantaneous counter-attacks exploiting a momentary opening in the enemy's guard. The enemy is off-balance from their failed attack and cannot react defensively to the riposte. This prevents infinite riposte loops (player parries → ripostes → enemy parries → ripostes → ...) and maintains action economy balance.

**Implementation**:
- Ripostes target `Enemy` entities, which have no parry capability in v0.21.4
- Even if enemy parrying is implemented in future versions, ripostes should bypass parry checks
- Combat log displays riposte damage directly without defensive rolls

**Future-Proofing**:
If enemy parrying is added:
```csharp
public RiposteResult ExecuteRiposte(PlayerCharacter attacker, Enemy target)
{
    // ... existing riposte logic ...

    // FUTURE: Even if target has parry capability, ripostes cannot be parried
    // This flag would prevent parry triggers in enemy defensive logic
    result.CanBeParried = false;

    return result;
}
```

---

## FR-003: Specialization Integration

**Status**: ✅ Implemented (v0.21.4)
**Code Reference**: `CounterAttackService.cs:345-436`

### FR-003.1: Hólmgangr Reactive Parry Specialization

**Requirement**: The Hólmgangr archetype specialization "Reactive Parry" must provide escalating parry bonuses across three ranks.

**Reactive Parry Progression**:

| Rank | Bonus Dice | Superior Riposte | Parries Per Round | Unlock Requirement |
|------|------------|------------------|-------------------|-------------------|
| **Rank 1** (Basic) | +1d10 | ✓ Enabled | 1 | Hólmgangr Tier 2 unlock |
| **Rank 2** (Expert) | +2d10 | ✓ Enabled | 1 | Progression points investment |
| **Rank 3** (Mastery) | +2d10 | ✓ Enabled | **2** | Mastery-level investment |

**Design Progression**:
- **Rank 1**: Unlocks the core capability (Superior Riposte) and adds consistency (+1d10)
- **Rank 2**: Doubles consistency (+2d10), making Superior/Critical parries more reliable
- **Rank 3**: Adds defensive capacity (2 parries/round), allowing defensive playstyle in multi-enemy scenarios

**Implementation** (`CounterAttackService.cs:353-387`):
```csharp
public void ApplyHolmgangrReactiveParry(PlayerCharacter character, int rank = 1)
{
    int characterId = GetCharacterId(character);

    // Remove any existing Reactive Parry bonuses (upgrade case)
    var existingBonuses = _repository.GetParryBonuses(characterId)
        .Where(b => b.Source == "Reactive Parry")
        .ToList();

    foreach (var bonus in existingBonuses)
    {
        _repository.RemoveParryBonus(bonus.BonusID);
    }

    // Apply new bonus based on rank
    var newBonus = new ParryBonus
    {
        CharacterID = characterId,
        Source = "Reactive Parry",
        BonusDice = rank >= 2 ? 2 : 1, // Rank 2+ = +2d10, Rank 1 = +1d10
        AllowsSuperiorRiposte = true, // Hólmgangr always gets Superior Riposte
        ParriesPerRound = rank >= 3 ? 2 : 1 // Rank 3 = 2 parries per round
    };

    _repository.AddParryBonus(newBonus);

    // Update character's parries per turn
    character.ParriesRemainingThisTurn = newBonus.ParriesPerRound;
}
```

**Example Impact**:

**Scenario**: Hólmgangr Rank 2 vs Armored Enforcer
```
Character (FINESSE 6, Weapon Skill 3):
  Parry Pool WITHOUT Reactive Parry: 6 + 3 = 9
  Parry Pool WITH Reactive Parry Rank 2: 6 + 3 + 2d10 (rolls 7 + 5 = 12) = 21

Enemy Attack Accuracy: 14

Without Reactive Parry:
  9 < 14 → Failed Parry (+5 Stress, takes full damage)

With Reactive Parry Rank 2:
  21 > 14 by 7 → Critical Parry (-8 Stress, riposte triggered, no damage)
```

**Integration with Specialization System** (pseudocode from COUNTER_ATTACK_INTEGRATION.md):
```csharp
// When character learns/upgrades "Reactive Parry" ability
if (abilityName == "Reactive Parry")
{
    int rank = ability.CurrentRank;

    var counterAttackService = new CounterAttackService(
        _diceService,
        new CounterAttackRepository(),
        _traumaService);

    counterAttackService.ApplyHolmgangrReactiveParry(character, rank);

    // Display upgrade notification
    if (rank == 1)
    {
        Console.WriteLine("✦ REACTIVE PARRY UNLOCKED!");
        Console.WriteLine("  +1d10 to Parry Pool");
        Console.WriteLine("  Superior Parries trigger Riposte!");
    }
    else if (rank == 2)
    {
        Console.WriteLine("✦ REACTIVE PARRY RANK 2!");
        Console.WriteLine("  +2d10 to Parry Pool");
    }
    else if (rank == 3)
    {
        Console.WriteLine("✦ REACTIVE PARRY MASTERY!");
        Console.WriteLine("  Can parry TWICE per round!");
    }
}
```

### FR-003.2: Atgeir-wielder Parry Bonus

**Requirement**: Characters wielding Atgeir (reach weapons) must receive a parry bonus representing reach advantage in defense.

**Atgeir-wielder Bonus**:
- **Bonus Dice**: +1d10 to Parry Pool
- **Superior Riposte**: Not enabled (Hólmgangr exclusive)
- **Parries Per Round**: 1 (standard)
- **Rationale**: Reach weapons allow defensive maneuvering, but lack the technique for Superior Riposte

**Implementation** (`CounterAttackService.cs:393-423`):
```csharp
public void ApplyAtgeirWielderParryBonus(PlayerCharacter character)
{
    int characterId = GetCharacterId(character);

    // Check if already has this bonus
    var existingBonus = _repository.GetParryBonuses(characterId)
        .FirstOrDefault(b => b.Source == "Atgeir-wielder Reach");

    if (existingBonus != null)
    {
        return; // Already applied
    }

    // Apply bonus
    var newBonus = new ParryBonus
    {
        CharacterID = characterId,
        Source = "Atgeir-wielder Reach",
        BonusDice = 1, // +1d10 to Parry Pool
        AllowsSuperiorRiposte = false, // No Superior Riposte (Hólmgangr exclusive)
        ParriesPerRound = 1
    };

    _repository.AddParryBonus(newBonus);
}
```

**Bonus Stacking**:
Hólmgangr wielding Atgeir receives **both** bonuses:
- Reactive Parry Rank 2: +2d10, Superior Riposte, 1 parry/round
- Atgeir-wielder: +1d10
- **Total**: +3d10 to Parry Pool, Superior Riposte enabled, 1 parry/round

**Example** (Hólmgangr Rank 2 + Atgeir):
```
Character (FINESSE 6, Weapon Skill 3):
  Base: 6 + 3 = 9
  Reactive Parry R2: +2d10
  Atgeir-wielder: +1d10
  Total Roll: 9 + 3d10 (rolls 6 + 8 + 4 = 18) = 27

Enemy Attack Accuracy: 15
Result: 27 > 15 by 12 → Critical Parry (riposte guaranteed)
```

### FR-003.3: Bonus Management and Removal

**Requirement**: The system must support adding, removing, and querying parry bonuses for dynamic equipment/buff scenarios.

**Bonus Management Methods**:

**Get Bonus Dice** (`CounterAttackService.cs:287-291`):
```csharp
public int GetParryBonusDice(PlayerCharacter character)
{
    var bonuses = _repository.GetParryBonuses(GetCharacterId(character));
    return bonuses.Sum(b => b.BonusDice); // Sums all active bonuses
}
```

**Add Bonus**:
```csharp
public int AddParryBonus(ParryBonus bonus)
{
    return _repository.AddParryBonus(bonus);
}
```

**Remove Bonus**:
```csharp
public void RemoveParryBonus(int bonusId)
{
    _repository.RemoveParryBonus(bonusId);
}
```

**Remove All Bonuses** (for respec/testing):
```csharp
public void RemoveAllParryBonuses(PlayerCharacter character)
{
    int characterId = GetCharacterId(character);
    _repository.RemoveAllParryBonuses(characterId);
}
```

**Future Extensions**:
- **Equipment Bonuses**: Shields (+1d10), Parrying Daggers (+1d10)
- **Temporary Buffs**: "Defensive Stance" spell (+2d10 for 3 turns)
- **Status Effects**: "Disoriented" (-1d10), "Enhanced Reflexes" (+1d10)

---

## FR-004: Trauma Economy Integration

**Status**: ✅ Implemented (v0.21.4)
**Code Reference**: `CounterAttackService.cs:509-556`

### FR-004.1: Stress Changes by Parry Outcome

**Requirement**: Parry outcomes must generate or relieve stress based on psychological impact.

**Stress Change Table**:

| Event | Stress Change | Rationale | Code Reference |
|-------|---------------|-----------|----------------|
| **Failed Parry** | +5 Stress | Fear of being overwhelmed, loss of control | Line 550 |
| **Standard Parry** | 0 Stress | Neutral outcome, no psychological impact | N/A (no change) |
| **Superior Parry** | -3 Stress | Relief from successful defense, confidence boost | Line 525 |
| **Critical Parry** | -8 Stress | Perfect technique, mastery, control | Line 519 |
| **Riposte Hit** | +0 Stress | Already accounted in parry outcome | N/A |
| **Riposte Kill** | -10 Stress | Triumph over adversary, victory | Line 534 |
| **Riposte Miss** | +3 Stress | Frustration from wasted opportunity | Line 542 |

**Cumulative Stress Examples**:

**Example 1: Critical Parry + Riposte Kill**
- Critical Parry: -8 Stress
- Riposte Kill: -10 Stress
- **Total**: -18 Stress (massive relief)

**Example 2: Superior Parry + Riposte Miss (Hólmgangr)**
- Superior Parry: -3 Stress
- Riposte Miss: +3 Stress
- **Total**: 0 Stress (frustration cancels relief)

**Example 3: Failed Parry (Standard Character)**
- Failed Parry: +5 Stress
- Takes full damage from enemy attack
- **Total**: +5 Stress (plus damage trauma)

### FR-004.2: Stress Application Implementation

**Implementation** (`CounterAttackService.cs:509-556`):
```csharp
private int ApplyTraumaEconomyEffects(PlayerCharacter character, ParryResult result)
{
    int stressChange = 0;

    if (result.Success)
    {
        // Successful parry reduces stress
        if (result.Outcome == ParryOutcome.Critical)
        {
            stressChange = CriticalParryStressRelief; // -8
            _traumaService.AddStress(character, stressChange, "Critical Parry");
        }
        else if (result.Outcome == ParryOutcome.Superior)
        {
            stressChange = SuperiorParryStressRelief; // -3
            _traumaService.AddStress(character, stressChange, "Superior Parry");
        }

        // Riposte kill provides additional stress relief
        if (result.Riposte?.KilledTarget == true)
        {
            int killRelief = RiposteKillStressRelief; // -10
            _traumaService.AddStress(character, killRelief, "Riposte Kill");
            stressChange += killRelief;
        }
        // Riposte miss generates minor stress
        else if (result.RiposteTriggered && result.Riposte?.Hit == false)
        {
            int missStress = RiposteMissStress; // +3
            _traumaService.AddStress(character, missStress, "Riposte Missed");
            stressChange += missStress;
        }
    }
    else
    {
        // Failed parry generates stress
        stressChange = FailedParryStress; // +5
        _traumaService.AddStress(character, stressChange, "Failed Parry");
    }

    return stressChange;
}
```

**Stress Constants** (`CounterAttackService.cs:24-29`):
```csharp
private const int FailedParryStress = 5;
private const int SuperiorParryStressRelief = -3;
private const int CriticalParryStressRelief = -8;
private const int RiposteMissStress = 3;
private const int RiposteKillStressRelief = -10;
```

### FR-004.3: Trauma Economy Integration Point

**Dependency**: `TraumaEconomyService` (injected via constructor)

**Constructor** (`CounterAttackService.cs:31-42`):
```csharp
public CounterAttackService(
    DiceService diceService,
    CounterAttackRepository repository,
    TraumaEconomyService traumaService, // ← Trauma integration
    SpecializationService? specializationService = null)
{
    _diceService = diceService;
    _repository = repository;
    _traumaService = traumaService; // ← Store reference
    _specializationService = specializationService;
}
```

**Stress Method Signature**:
```csharp
// TraumaEconomyService.AddStress() method signature
public void AddStress(PlayerCharacter character, int amount, string reason)
{
    // amount: Can be positive (add stress) or negative (relieve stress)
    // reason: Logged for trauma narrative tracking ("Critical Parry", "Riposte Kill")
}
```

---

## FR-005: Statistics and Persistence

**Status**: ✅ Implemented (v0.21.4)
**Code Reference**: `CounterAttackRepository.cs:93-417`

### FR-005.1: Parry Statistics Tracking

**Requirement**: The system must track persistent, character-level statistics for all parry attempts and outcomes.

**ParryStatistics Data Model** (`CounterAttack.cs:46-60`):
```csharp
public class ParryStatistics
{
    public int CharacterID { get; set; }
    public int TotalParryAttempts { get; set; }
    public int SuccessfulParries { get; set; }
    public int SuperiorParries { get; set; }
    public int CriticalParries { get; set; }
    public int FailedParries { get; set; }
    public int RipostesLanded { get; set; }
    public int RiposteKills { get; set; }

    public float SuccessRate => TotalParryAttempts > 0
        ? (float)SuccessfulParries / TotalParryAttempts * 100f
        : 0f;
}
```

**Database Schema** (`CounterAttackRepository.cs:37-49`):
```sql
CREATE TABLE IF NOT EXISTS ParryStatistics (
    CharacterID INTEGER PRIMARY KEY,
    TotalParryAttempts INTEGER DEFAULT 0,
    SuccessfulParries INTEGER DEFAULT 0,
    SuperiorParries INTEGER DEFAULT 0,
    CriticalParries INTEGER DEFAULT 0,
    FailedParries INTEGER DEFAULT 0,
    RipostesLanded INTEGER DEFAULT 0,
    RiposteKills INTEGER DEFAULT 0
)
```

**Update Statistics** (`CounterAttackRepository.cs:138-197`):
```csharp
public void UpdateParryStatistics(ParryResult result)
{
    // Check if stats exist, create if not
    // ... initialization logic ...

    // Update stats
    var updateCommand = connection.CreateCommand();
    updateCommand.CommandText = @"
        UPDATE ParryStatistics
        SET TotalParryAttempts = TotalParryAttempts + 1,
            SuccessfulParries = SuccessfulParries + @success,
            SuperiorParries = SuperiorParries + @superior,
            CriticalParries = CriticalParries + @critical,
            FailedParries = FailedParries + @failed,
            RipostesLanded = RipostesLanded + @riposte,
            RiposteKills = RiposteKills + @kill
        WHERE CharacterID = @characterId
    ";

    updateCommand.Parameters.AddWithValue("@characterId", result.DefenderID);
    updateCommand.Parameters.AddWithValue("@success", result.Success ? 1 : 0);
    updateCommand.Parameters.AddWithValue("@superior", result.Outcome == ParryOutcome.Superior ? 1 : 0);
    updateCommand.Parameters.AddWithValue("@critical", result.Outcome == ParryOutcome.Critical ? 1 : 0);
    updateCommand.Parameters.AddWithValue("@failed", !result.Success ? 1 : 0);
    updateCommand.Parameters.AddWithValue("@riposte",
        result.RiposteTriggered && result.Riposte?.Hit == true ? 1 : 0);
    updateCommand.Parameters.AddWithValue("@kill",
        result.Riposte?.KilledTarget == true ? 1 : 0);

    updateCommand.ExecuteNonQuery();
}
```

**Get Statistics**:
```csharp
public ParryStatistics GetParryStatistics(int characterId)
{
    // Query database, return ParryStatistics object
    // Returns empty stats if no record exists
}
```

### FR-005.2: Combat Log (Parry Attempts)

**Requirement**: Every parry attempt must be logged to a combat log table for detailed post-combat analysis.

**ParryAttempt Data Model** (`CounterAttack.cs:78-91`):
```csharp
public class ParryAttempt
{
    public int AttemptID { get; set; }
    public int CombatInstanceID { get; set; }
    public int DefenderID { get; set; }
    public int AttackerID { get; set; }
    public string? AttackAbility { get; set; }
    public int ParryPoolRoll { get; set; }
    public int AttackerAccuracyRoll { get; set; }
    public ParryOutcome Outcome { get; set; }
    public bool RiposteTriggered { get; set; }
    public int RiposteDamage { get; set; }
    public DateTime Timestamp { get; set; }
}
```

**Database Schema** (`CounterAttackRepository.cs:66-82`):
```sql
CREATE TABLE IF NOT EXISTS ParryAttempts (
    AttemptID INTEGER PRIMARY KEY AUTOINCREMENT,
    CombatInstanceID INTEGER NOT NULL,
    DefenderID INTEGER NOT NULL,
    AttackerID INTEGER NOT NULL,
    AttackAbility TEXT,
    ParryPoolRoll INTEGER NOT NULL,
    AttackerAccuracyRoll INTEGER NOT NULL,
    Outcome TEXT NOT NULL,
    RiposteTriggered INTEGER DEFAULT 0,
    RiposteDamage INTEGER DEFAULT 0,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
)
```

**Record Parry Attempt** (`CounterAttackRepository.cs:328-367`):
```csharp
public int RecordParryAttempt(ParryAttempt attempt)
{
    // Insert parry attempt record
    // Returns AttemptID of inserted record
}
```

**Get Parry Attempts**:
```csharp
public List<ParryAttempt> GetParryAttempts(int combatInstanceId)
{
    // Query all parry attempts for a specific combat instance
    // Returns list ordered by Timestamp
}
```

### FR-005.3: Parry Bonus Persistence

**Requirement**: Active parry bonuses (from specializations, equipment, buffs) must be stored persistently and queried efficiently.

**Database Schema** (`CounterAttackRepository.cs:52-63`):
```sql
CREATE TABLE IF NOT EXISTS ParryBonuses (
    BonusID INTEGER PRIMARY KEY AUTOINCREMENT,
    CharacterID INTEGER NOT NULL,
    Source TEXT NOT NULL,
    BonusDice INTEGER DEFAULT 0,
    AllowsSuperiorRiposte INTEGER DEFAULT 0,
    ParriesPerRound INTEGER DEFAULT 1
)
```

**Repository Methods** (`CounterAttackRepository.cs:201-320`):

**Get Bonuses**:
```csharp
public List<ParryBonus> GetParryBonuses(int characterId)
{
    // Query all active bonuses for character
    // Returns list of ParryBonus objects
}
```

**Add Bonus**:
```csharp
public int AddParryBonus(ParryBonus bonus)
{
    // Insert new bonus record
    // Returns BonusID of inserted record
}
```

**Remove Bonus**:
```csharp
public void RemoveParryBonus(int bonusId)
{
    // Delete specific bonus by ID
}
```

**Remove All Bonuses**:
```csharp
public void RemoveAllParryBonuses(int characterId)
{
    // Delete all bonuses for character (respec scenario)
}
```

### FR-005.4: Statistics Display (UI Integration)

**Example Statistics Display** (from COUNTER_ATTACK_INTEGRATION.md):
```
> stats parry

═══════════════════════════════════════
         PARRY STATISTICS
═══════════════════════════════════════
Total Attempts:     47
Success Rate:       72% (34/47)

Parry Outcomes:
  • Failed:         13
  • Standard:       11
  • Superior:       18
  • Critical:       5

Ripostes:
  • Landed:         21
  • Kills:          7
═══════════════════════════════════════
```

**Derived Metrics**:
- **Success Rate**: (SuccessfulParries / TotalParryAttempts) × 100%
- **Critical Rate**: (CriticalParries / TotalParryAttempts) × 100%
- **Riposte Efficiency**: (RiposteKills / RipostesLanded) × 100%

---

## FR-006: Balance Targets and Design Guidelines

**Status**: ✅ Defined (v0.21.4)

### FR-006.1: Target Success Rates by Character Type

**Requirement**: Parry success rates must be tuned to create meaningful choices without overshadowing offensive tactics.

**Target Success Rates** (vs enemy attacks of appropriate tier):

| Character Type | Expected Parry Success Rate | Expected Critical Rate | Riposte Damage Contribution |
|----------------|----------------------------|------------------------|----------------------------|
| **Standard Character** (FINESSE 3-4, no bonuses) | 40-50% | 5-10% | 5-10% of total damage |
| **FINESSE-focused** (FINESSE 5-6, no bonuses) | 50-60% | 10-15% | 10-15% of total damage |
| **Hólmgangr Rank 1** (FINESSE 5, +1d10) | 60-70% | 15-20% | 20-30% of total damage |
| **Hólmgangr Rank 2** (FINESSE 6, +2d10) | 70-80% | 25-35% | 30-40% of total damage |
| **Hólmgangr Rank 3** (FINESSE 7, +2d10, 2 parries/round) | 75-85% | 30-40% | 40-50% of total damage |

**Design Rationale**:
- Standard characters should succeed ~50% of the time (meaningful but not guaranteed)
- Hólmgangr specialization should significantly increase both success and critical rates
- Riposte damage should be substantial for defensive builds but not exceed offensive builds
- Critical rate increases with specialization create escalating reward for investment

### FR-006.2: Enemy Attack Accuracy Ranges

**Requirement**: Enemy attack accuracy should be balanced against expected player parry pools.

**Enemy Accuracy by Tier** (for parry balance):

| Enemy Tier | Typical Accuracy Roll | Expected Parry Pool (Standard) | Expected Parry Pool (Hólmgangr R2) | Outcome |
|------------|----------------------|-------------------------------|-----------------------------------|---------|
| **Tier 1** (Weak) | 6-10 | 7-8 | 16-20 | Standard: 50-70% success<br>Hólmgangr: 85-95% success |
| **Tier 2** (Moderate) | 10-14 | 7-8 | 16-20 | Standard: 30-50% success<br>Hólmgangr: 70-85% success |
| **Tier 3** (Strong) | 14-18 | 9-11 | 20-25 | Standard: 20-40% success<br>Hólmgangr: 60-75% success |
| **Tier 4** (Elite) | 18-22 | 10-12 | 23-28 | Standard: 10-30% success<br>Hólmgangr: 50-65% success |
| **Boss** | 20-28 | 11-14 | 25-32 | Standard: 5-25% success<br>Hólmgangr: 40-60% success |

**Design Notes**:
- Enemy accuracy should scale with tier to maintain challenge
- Hólmgangr should always have a meaningful chance to parry, even against bosses
- Standard characters should struggle against elite/boss enemies (encouraging specialization)
- Critical parries (5+ margin) should be achievable but not guaranteed for specialists

### FR-006.3: Parry vs Offensive Action Trade-off

**Requirement**: Parrying must be a meaningful tactical choice, not a dominant strategy.

**Damage Comparison** (per round):

| Scenario | Offensive Action (Attack) | Defensive Action (Parry) | Net Damage Differential |
|----------|--------------------------|-------------------------|-------------------------|
| **Standard vs Tier 2 Enemy** | +12 damage (player attack) | -0 damage (parry), potential +0 damage (unlikely riposte) | **Offense: +12 net** |
| **Hólmgangr R1 vs Tier 2 Enemy** | +14 damage (player attack) | -0 damage (parry), +6 damage (50% riposte chance) | **Offense: +14 vs Parry: +6 → Offense wins** |
| **Hólmgangr R2 vs Tier 3 Enemy** | +16 damage (player attack), -10 damage (enemy counter-attack) | -0 damage (parry, 75% success), +10 damage (40% riposte) | **Offense: +6 net vs Parry: +10 net → Parry wins** |
| **Hólmgangr R3 vs Multiple Enemies** | +18 damage (player attack to one), -20 damage (two enemy attacks) | -0 damage (parry both, 75% success each), +12 damage (2× riposte) | **Offense: -2 net vs Parry: +12 net → Parry wins** |

**Design Guidelines**:
1. **Offensive Dominance (Standard Characters)**: Without specialization, attacking is generally superior to parrying due to low riposte chance
2. **Defensive Viability (Hólmgangr R1-R2)**: Parrying becomes competitive when facing high-damage enemies (preventing damage > dealing damage)
3. **Defensive Superiority (Hólmgangr R3)**: Against multiple enemies, parrying twice/round becomes superior due to damage prevention + multiple ripostes
4. **Stress Economy Matters**: Parrying provides stress relief (-3 to -8) which has long-term value beyond immediate damage

**Balancing Lever**: If parrying becomes dominant, adjust:
- Reduce stress relief values (e.g., Critical: -8 → -5)
- Increase enemy accuracy scaling
- Reduce riposte damage multipliers (currently uses full MIGHT + weapon damage)

### FR-006.4: Parry Frequency Limits

**Requirement**: Parry frequency limits must prevent defensive stalling while allowing defensive playstyles.

**Parry Limit Justification**:

| Limit | Character Type | Justification | Prevents |
|-------|---------------|---------------|----------|
| **1/round** | Standard | Parrying is physically/mentally taxing; can't maintain constant vigilance | Defensive stalling (parry → wait → parry → wait) |
| **2/round** | Hólmgangr R3 | Master defensive training allows rapid successive parries | N/A (still limited, 2 enemies max) |

**Multi-Enemy Scaling**:
- **1 enemy**: Parrying blocks 1 attack/round (100% coverage with 1 parry limit)
- **2 enemies**: Parrying blocks 1-2 attacks/round (50-100% coverage)
- **3+ enemies**: Parrying blocks 1-2 attacks/round (33-67% coverage) → encourages offense or crowd control

**Design Note**: Parry limits prevent "turtle" strategies while rewarding defensive specialists in multi-enemy scenarios. Hólmgangr R3 is the only build that can fully defend against 2 enemies simultaneously.

### FR-006.5: Stress Economy Balance

**Requirement**: Stress values must create meaningful risk-reward dynamics without trivializing trauma economy.

**Stress Value Justification**:

| Event | Stress Change | Expected Frequency (Hólmgangr R2) | Net Stress/Combat (5 rounds) |
|-------|---------------|----------------------------------|----------------------------|
| **Failed Parry** | +5 | 2 attempts (20% fail rate) = 0.4 fails | +2 stress |
| **Superior Parry** | -3 | 2 attempts (40% superior rate) = 0.8 superiors | -2.4 stress |
| **Critical Parry** | -8 | 2 attempts (30% critical rate) = 0.6 criticals | -4.8 stress |
| **Riposte Kill** | -10 | 1.4 ripostes × 40% kill rate = 0.56 kills | -5.6 stress |
| **Riposte Miss** | +3 | 1.4 ripostes × 60% miss rate = 0.84 misses | +2.5 stress |
| **NET** | | | **-8.3 stress per combat** |

**Design Analysis**:
- **Hólmgangr** provides moderate stress relief over full combat (~8-10 stress reduction)
- Comparable to "Kill Enemy Quickly" stress relief (typically -5 to -15 depending on threat level)
- Failed parries punish poor timing/luck (+5 is significant penalty)
- Critical parry + riposte kill combo (-18 stress) is rare but impactful reward
- **Balance Target**: Parrying should provide ~5-15 stress relief per combat (defensive playstyle reward)

**Tuning Considerations**:
- If stress relief is too high, reduce Critical Parry relief (-8 → -5) or Riposte Kill relief (-10 → -7)
- If stress relief is too low, increase Superior Parry relief (-3 → -5) or reduce Riposte Miss penalty (+3 → +1)

### FR-006.6: Design Guidelines for Future Extensions

**Guideline 1: Avoid Parry Power Creep**
- New parry bonuses (equipment, buffs) should cap at +2d10 total (Hólmgangr R2 standard)
- Shields should provide defensive benefits beyond parry (e.g., passive DR, cover mechanics)
- Parries per round should cap at 2 (Hólmgangr R3 standard)

**Guideline 2: Maintain Critical Threshold (5+)**
- Critical parry threshold (5+ margin) should remain constant
- This ensures meaningful distinction between Superior (tactical advantage) and Critical (dominant advantage)
- Adjusting threshold would require rebalancing all stress values and riposte triggers

**Guideline 3: Enemy Parrying (Future)**
- If enemies gain parry capability, ripostes must bypass enemy parries (prevent infinite loops)
- Enemy parry limits should be 0-1/round (never 2) to maintain player advantage
- Enemy ripostes should deal reduced damage (50-75% of normal attack) to prevent lethality spikes

**Guideline 4: Equipment Parry Bonuses**
- **Shields**: +1d10 parry pool, +1 parries/round (conflicts with 2H weapons)
- **Parrying Daggers**: +1d10 parry pool when wielded in off-hand
- **Defensive Armor**: +0d10 parry pool (armor provides Soak, not parry bonuses)
- **Enchanted Weapons**: +1d10 parry pool (rare, high-tier only)

**Guideline 5: Status Effect Interactions**
- **Disoriented/Stunned**: -2d10 parry pool (severe penalty)
- **Slowed**: -1d10 parry pool (moderate penalty)
- **Enhanced Reflexes**: +1d10 parry pool (buff spell)
- **Parry Disabled**: Certain attacks/abilities should bypass parry entirely (e.g., AoE attacks, mental assaults)

---

## Appendix A: Parry Outcome Reference Tables

### Table A.1: Parry Outcome Quick Reference

| Parry Roll | Attack Accuracy | Margin | Outcome | Riposte (Standard) | Riposte (Hólmgangr) | Stress Change | Damage Blocked |
|------------|----------------|--------|---------|-------------------|---------------------|---------------|----------------|
| 12 | 15 | -3 | **Failed** | No | No | +5 | 0% |
| 14 | 14 | 0 | **Standard** | No | No | 0 | 100% |
| 16 | 14 | +2 | **Superior** | No | **Yes** | -3 | 100% |
| 19 | 14 | +5 | **Critical** | **Yes** | **Yes** | -8 | 100% |
| 22 | 14 | +8 | **Critical** | **Yes** | **Yes** | -8 | 100% |

### Table A.2: Parry Pool Breakdown by Character Type

| Character Type | FINESSE | Weapon Skill | Bonus Dice | Avg Bonus Roll | Total Avg Parry Pool |
|----------------|---------|--------------|------------|----------------|---------------------|
| **Novice** | 3 | 1 (unarmed) | 0 | 0 | **4** |
| **Standard** | 4 | 3 (equipped) | 0 | 0 | **7** |
| **FINESSE Build** | 6 | 3 | 0 | 0 | **9** |
| **Atgeir-wielder** | 4 | 3 | +1d10 | +5.5 | **12.5** |
| **Hólmgangr Rank 1** | 5 | 3 | +1d10 | +5.5 | **13.5** |
| **Hólmgangr Rank 2** | 6 | 3 | +2d10 | +11 | **20** |
| **Hólmgangr R2 + Atgeir** | 6 | 3 | +3d10 | +16.5 | **25.5** |
| **Hólmgangr Rank 3 (Max)** | 7 | 4 | +2d10 | +11 | **22** |

### Table A.3: Riposte Damage by Weapon Type

| Weapon | Damage Dice | Avg Damage | + MIGHT (4) | Avg Total | Notes |
|--------|-------------|------------|-------------|-----------|-------|
| **Unarmed** | 1d6 | 3.5 | +4 | **7.5** | Baseline |
| **Dagger** | 1d6 | 3.5 | +4 | **7.5** | Fast, low damage |
| **Longsword** | 1d8 | 4.5 | +4 | **8.5** | Balanced |
| **Axe** | 1d10 | 5.5 | +4 | **9.5** | High damage |
| **Atgeir** | 1d10 | 5.5 | +4 | **9.5** | High damage + reach bonus |
| **Greatsword** | 2d6 | 7 | +4 | **11** | Highest damage (2H) |

---

## Appendix B: Full Combat Example (Hólmgangr vs Elite Enemy)

### Scenario Setup

**Player Character** (Hólmgangr Rank 2):
- Name: Bjorn the Ironclad
- FINESSE: 6, MIGHT: 5, STURDINESS: 5
- HP: 45/50
- Stress: 35/100
- Weapon: Atgeir (1d10 damage, +1d10 parry bonus)
- Specialization: Reactive Parry Rank 2 (+2d10 parry pool, Superior Riposte enabled)
- **Total Parry Bonus**: +3d10 (Reactive Parry R2 + Atgeir)
- Parries Remaining: 1/1

**Enemy** (Armored Enforcer - Tier 3 Elite):
- HP: 35/35
- Defense: 14
- Soak: 6
- Attack: +12 to hit (typical accuracy roll: 14-18)

### Round 1: Player Turn

**Bjorn's Action**: Prepare Parry

```
> parry

You prepare to parry the next attack!
  Parries remaining: 0/1
  Parry bonus: +3d10
  [Reactive Parry]: Superior parries trigger Riposte!
```

### Round 1: Enemy Turn

**Armored Enforcer attacks Bjorn**

```
Armored Enforcer swings its mace at you!
  Attack Accuracy Roll: 16

You attempt to parry!

Parry Pool Calculation:
  FINESSE: 6
  Weapon Skill: 3
  Bonus Dice: 3d10 → rolls [7, 5, 9] = 21
  Total Parry Pool: 6 + 3 + 21 = 30

Parry Check: 30 vs 16 (margin: +14)

✦ CRITICAL PARRY! ✦
  Parry Roll: 30 vs Accuracy: 16
  Attack completely deflected!

⚔ RIPOSTE! You counter-attack!
  Attack Roll: FINESSE (6) + Weapon Skill (3) + d10 (rolls 8) = 17
  Enemy Defense: 14
  HIT!

  Damage Roll: 1d10 (Atgeir, rolls 7) + MIGHT (5) = 12 damage
  Enemy Soak: 6
  Effective Damage: 12 - 6 = 6 damage

  Armored Enforcer HP: 35 → 29

Stress Changes:
  Critical Parry: -8 Stress
  Riposte Hit: +0 Stress
  Net: -8 Stress

Bjorn Stress: 35 → 27
```

**Analysis**:
- Bjorn's +3d10 bonus resulted in very high parry pool (30)
- Critical Parry triggered universal riposte
- Riposte dealt 6 effective damage (after Soak reduction)
- Bjorn avoided taking damage AND dealt damage (defensive playstyle success)
- Stress relief (-8) is significant psychological benefit

### Round 2: Player Turn

**Bjorn's Action**: Attack

```
> attack enforcer

You strike at the Armored Enforcer with your Atgeir!
  Attack Roll: FINESSE (6) + Weapon Skill (3) + d10 (rolls 6) = 15
  Enemy Defense: 14
  HIT!

  Damage Roll: 1d10 (rolls 8) + MIGHT (5) = 13 damage
  Enemy Soak: 6
  Effective Damage: 13 - 6 = 7 damage

  Armored Enforcer HP: 29 → 22
```

**Analysis**:
- Bjorn switches to offense after successful parry
- Standard attack dealt 7 damage (comparable to riposte damage)
- No stress change from standard attack

### Round 2: Enemy Turn

**Armored Enforcer attacks Bjorn (no parry prepared)**

```
Armored Enforcer swings its mace at you!
  Attack Roll: 18 vs Defense (12): HIT!

  Damage Roll: 1d10 (rolls 7) + 6 = 13 damage
  Your Soak: 4
  Effective Damage: 13 - 4 = 9 damage

  Bjorn HP: 45 → 36

Stress: +3 (damage trauma)
Bjorn Stress: 27 → 30
```

**Analysis**:
- Without parry, Bjorn takes full damage (9 HP)
- Demonstrates parry value: prevented 9 damage in Round 1 by parrying

### Combat Summary

**Total Damage**:
- **Bjorn dealt**: 6 (riposte) + 7 (attack) = **13 damage**
- **Bjorn received**: 0 (parried) + 9 (unparried) = **9 damage**
- **Net**: +4 HP advantage

**Stress Economy**:
- Parry-related: -8 (Critical Parry)
- Damage-related: +3 (taking damage in Round 2)
- **Net**: -5 Stress (improved psychological state)

**Tactical Outcome**:
Bjorn's defensive specialization (Hólmgangr R2 + Atgeir) provided:
1. Damage prevention (9 HP saved)
2. Free counter-attack damage (6 damage)
3. Stress relief (-8 stress)
4. Action economy advantage (parry → riposte = 1 action, 2 effects)

---

## Appendix C: Parry Design Checklist

Use this checklist when designing new parry-related features, bonuses, or mechanics.

### Balance Checklist

- [ ] **Parry Pool**: Does the new feature provide reasonable parry pool bonus? (Target: 0-2d10 max)
- [ ] **Success Rate**: Does the feature create 40-85% success rates against appropriate-tier enemies?
- [ ] **Critical Rate**: Does the feature allow 10-40% critical parry rates for specialists?
- [ ] **Riposte Damage**: Does riposte damage contribution stay within 5-50% of total player damage?
- [ ] **Stress Values**: Are stress changes meaningful (-10 to +5 range) without trivializing trauma economy?
- [ ] **Parry Limit**: Does the feature respect 1-2 parries/round limit?
- [ ] **Action Economy**: Does the feature maintain parry = 1 action cost (no free parries)?

### Integration Checklist

- [ ] **Database Schema**: Is the ParryBonuses table schema sufficient, or do we need new tables?
- [ ] **Specialization System**: Does the feature integrate with AbilityService/SpecializationService?
- [ ] **Equipment System**: If equipment-based, does it integrate with EquipmentService?
- [ ] **Status Effects**: Does the feature respect status effect modifiers (Disoriented, Stunned, etc.)?
- [ ] **Combat Flow**: Does the feature execute at the correct combat phase (preparation vs reactive)?
- [ ] **UI Display**: Can players easily see parry pool, bonuses, and success rates?

### Design Quality Checklist

- [ ] **Clarity**: Is the feature's effect clear and unambiguous to players?
- [ ] **Consistency**: Does the feature follow existing parry system conventions?
- [ ] **Extensibility**: Can the feature be extended without breaking existing mechanics?
- [ ] **Performance**: Does the feature avoid expensive database queries or complex calculations?
- [ ] **Testing**: Can the feature be unit tested with existing CounterAttackServiceTests framework?

---

