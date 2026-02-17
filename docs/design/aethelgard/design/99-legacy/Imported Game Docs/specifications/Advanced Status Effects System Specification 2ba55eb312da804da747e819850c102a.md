# Advanced Status Effects System Specification

Parent item: Specs: Systems (Specs%20Systems%202ba55eb312da80c6aa36ce6564319160.md)

> Template Version: 1.0
Last Updated: 2025-11-27
Status: Active
Specification ID: SPEC-COMBAT-003
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-27 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [x]  **Review**: Ready for stakeholder review
- [x]  **Approved**: Approved for implementation
- [x]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Combat Designer
- **Design**: Effect types, stacking rules, duration mechanics
- **Balance**: Effect power, stack limits, immunity rules
- **Implementation**: AdvancedStatusEffectService.cs
- **QA/Testing**: Stack limit enforcement, edge cases

---

## Executive Summary

### Purpose Statement

The Advanced Status Effects System manages complex buff, debuff, and condition mechanics including stacking, duration tracking, conditional triggers, and immunity/resistance interactions.

### Scope

**In Scope**:

- Status effect types (buffs, debuffs, conditions)
- Stacking rules and limits
- Duration mechanics (turns, rounds, permanent)
- Conditional effects
- Immunity and resistance
- Effect removal logic
- Flavor text generation

**Out of Scope**:

- Damage calculation → `SPEC-COMBAT-002`
- Ability definitions → `SPEC-PROGRESSION-003`
- Combat turn order → `SPEC-COMBAT-001`
- Trauma system → `SPEC-ECONOMY-003`

---

## Design Philosophy

### Design Pillars

1. **Meaningful Duration**
    - **Rationale**: Effects should last long enough to matter
    - **Examples**: 3-turn buffs require tactical timing
2. **Stack Control**
    - **Rationale**: Prevent infinite scaling exploits
    - **Examples**: Max 5 stacks of Bleeding
3. **Clear Interactions**
    - **Rationale**: Players should understand effect combinations
    - **Examples**: Immunity prevents application, Resistance reduces duration

---

## Functional Requirements

### FR-001: Apply Status Effect

**Priority**: Critical
**Status**: Implemented

**Description**:
System must apply status effects to characters with proper stack handling.

**Logic Flow**:

```
1. Check target immunity
2. Apply resistance modifiers to duration
3. Check existing stacks of same effect
4. If at stack limit: refresh duration
5. If under limit: add new stack
6. Update target stats/modifiers

```

### FR-002: Track Effect Duration

**Priority**: Critical
**Status**: Implemented

**Duration Types**:

| Type | Decrement Trigger | Example |
| --- | --- | --- |
| Per-Turn | Each affected's turn | Defense buff |
| Per-Round | After all turns | Bleeding |
| Permanent | Never | Trauma |
| Conditional | Event-based | Until hit |

### FR-003: Stack Status Effects

**Priority**: High
**Status**: Implemented

**Stacking Rules**:

| Effect | Stack Behavior | Max Stacks |
| --- | --- | --- |
| Bleeding | Additive damage | 5 |
| Burning | Refresh duration | 3 |
| Inspired | Highest only | 1 |
| Vulnerable | Stack multiplier | 3 |
| Defense | Additive | 2 |

### FR-004: Remove Effects

**Priority**: High
**Status**: Implemented

**Removal Triggers**:

- Duration expires (reaches 0)
- Cleanse ability used
- Counter-effect applied
- Combat ends (combat-only effects)
- Death (all effects)

### FR-005: Apply Immunity/Resistance

**Priority**: Medium
**Status**: Implemented

**Immunity/Resistance Logic**:

```
If target has immunity to effect type:
  → Effect blocked entirely

If target has resistance to effect type:
  → Duration reduced by 50%
  → Stack damage reduced by 25%

```

---

## System Mechanics

### Mechanic 1: Effect Categories

**Effect Categories**:

| Category | Examples | Stack Rule | Default Duration |
| --- | --- | --- | --- |
| Damage Over Time | Bleeding, Burning, Poison | Additive | 3 rounds |
| Buff | Defense, Inspired, Haste | Highest/Refresh | 3 turns |
| Debuff | Vulnerable, Weakened, Slowed | Additive | 3 turns |
| Control | Stunned, Seized, Silenced | No stack | 1-2 turns |
| Conditional | Guarding, Performing, Analyzing | Single | Until triggered |

### Mechanic 2: Duration Decrement

**End-of-Turn Processing**:

```
For each status effect on character:
  If effect.DurationType == PerTurn AND it's character's turn:
    effect.Duration -= 1
  Else if effect.DurationType == PerRound AND round is ending:
    effect.Duration -= 1

  If effect.Duration <= 0:
    RemoveEffect(character, effect)
    TriggerExpirationEvent(effect)

```

### Mechanic 3: Effect Interactions

**Effect Synergies**:

| Effect A | Effect B | Interaction |
| --- | --- | --- |
| Burning | Oil | Burn damage ×2 |
| Vulnerable | Any Damage | Damage +50% |
| Stunned | Any Action | Action blocked |
| Silenced | Ability Use | Non-physical blocked |

**Counter Effects**:

| Effect | Counter | Result |
| --- | --- | --- |
| Burning | Frozen | Both removed |
| Inspired | Demoralized | Both removed |
| Haste | Slowed | Both removed |

---

## State Management

### System State

**State Variables**:

| Variable | Type | Persistence | Default | Description |
| --- | --- | --- | --- | --- |
| ActiveEffects | List<StatusEffect> | Combat | [] | Current effects |
| StackCount | int | Combat | 1 | Stacks of effect |
| RemainingDuration | int | Combat | varies | Turns/rounds left |

### Persistence Requirements

**Must Persist** (across save/load):

- Permanent effects (Traumas)
- Effects with long durations

**Transient** (combat only):

- Most buffs/debuffs
- Combat conditions

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Impact |
| --- | --- | --- | --- |
| MaxBleedStacks | StatusEffectConfig | 5 | DoT ceiling |
| ResistanceDurationReduction | StatusEffectConfig | 0.5 | Resistance power |
| DefaultBuffDuration | StatusEffectConfig | 3 | Buff uptime |

### Balance Targets

**Target 1: DoT Contribution**

- **Metric**: DoT damage as % of total combat damage
- **Target**: 15-25%
- **Levers**: Stack limits, base damage

**Target 2: Buff Uptime**

- **Metric**: Average buff uptime per combat
- **Target**: 40-60% of combat
- **Levers**: Duration, cooldowns

---

## Appendix

### Appendix A: Status Effect List

| Effect | Type | Duration | Stacks | Description |
| --- | --- | --- | --- | --- |
| Bleeding | DoT | 3 rounds | 5 | 2 damage per stack per round |
| Burning | DoT | 3 rounds | 3 | 3 damage per stack per round |
| Poison | DoT | 5 rounds | 3 | 1 damage per stack, ignores armor |
| Defense | Buff | 3 turns | 2 | +2 defense per stack |
| Inspired | Buff | 3 turns | 1 | +1 to all rolls |
| Vulnerable | Debuff | 3 turns | 3 | +25% damage taken per stack |
| Stunned | Control | 1 turn | 1 | Cannot act |
| Silenced | Control | 2 turns | 1 | Cannot use abilities |

---

**End of Specification**