# Advanced Status Effect Interactions — Mechanic Specification v5.0

Type: Mechanic
Description: Extends v2.0 status effects with stacking mechanics, effect interactions (conversion, amplification, suppression), and cascade behaviors.
Priority: Should-Have
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-STATUSINTERACTIONS-v5.0
Proof-of-Concept Flag: No
Sub-Type: Status
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

## I. Core Philosophy

Status effects transform combat from simple damage trades into **condition management warfare** where debuffs create tactical windows and buff timing becomes critical.

> ⚠️ **Source Authority Note:** This specification consolidated from SPEC-COMBAT-003 (Imported Game Docs / codebase reflection). Uses **d6 dice system** with 5-6 success threshold per codebase implementation.
> 

---

## II. Effect Categories

| Category | Examples | Stack Rule | Default Duration |
| --- | --- | --- | --- |
| **Damage Over Time** | [Bleeding], [Burning], [Poison] | Additive | 3 rounds |
| **Buff** | [Defense], [Inspired], [Haste] | Highest/Refresh | 3 turns |
| **Debuff** | [Vulnerable], [Weakened], [Slowed] | Additive | 3 turns |
| **Control** | [Stunned], [Seized], [Silenced] | No stack | 1-2 turns |
| **Conditional** | [Guarding], [Performing], [Analyzing] | Single | Until triggered |

---

## III. Duration Types

| Type | Decrement Trigger | Example |
| --- | --- | --- |
| **Per-Turn** | Each affected character's turn | [Defense] buff |
| **Per-Round** | After all turns complete | [Bleeding] |
| **Permanent** | Never (until removed) | Trauma effects |
| **Conditional** | Event-based trigger | "Until hit" |

### Duration Decrement Logic

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

---

## IV. Stacking Rules

| Effect | Stack Behavior | Max Stacks | Per-Stack Value |
| --- | --- | --- | --- |
| **[Bleeding]** | Additive damage | 5 | 2 damage/round |
| **[Burning]** | Refresh duration | 3 | 3 damage/round |
| **[Poison]** | Additive (ignores armor) | 3 | 1 damage/round |
| **[Vulnerable]** | Stack multiplier | 3 | +25% damage taken |
| **[Defense]** | Additive | 2 | +2 defense |
| **[Inspired]** | Highest only | 1 | +1 to all rolls |

### Stack Application Logic

```
1. Check target immunity
2. Apply resistance modifiers to duration
3. Check existing stacks of same effect
4. If at stack limit: refresh duration only
5. If under limit: add new stack
6. Update target stats/modifiers
```

---

## V. Complete Status Effect List

| Effect | Type | Duration | Stacks | Description |
| --- | --- | --- | --- | --- |
| [Bleeding] | DoT | 3 rounds | 5 | 2 damage per stack per round |
| [Burning] | DoT | 3 rounds | 3 | 3 damage per stack per round |
| [Poison] | DoT | 5 rounds | 3 | 1 damage per stack, ignores armor |
| [Defense] | Buff | 3 turns | 2 | +2 defense per stack |
| [Inspired] | Buff | 3 turns | 1 | +1 to all rolls |
| [Vulnerable] | Debuff | 3 turns | 3 | +25% damage taken per stack |
| [Stunned] | Control | 1 turn | 1 | Cannot act |
| [Silenced] | Control | 2 turns | 1 | Cannot use abilities |

---

## VI. Immunity & Resistance

### Application Logic

```
If target has IMMUNITY to effect type:
  → Effect blocked entirely (no application)

If target has RESISTANCE to effect type:
  → Duration reduced by 50%
  → Stack damage reduced by 25%
```

### Counter Effects

| Effect | Counter | Result |
| --- | --- | --- |
| [Burning] | [Frozen] | Both removed |
| [Inspired] | [Demoralized] | Both removed |
| [Haste] | [Slowed] | Both removed |

### Effect Synergies

| Effect A | Effect B | Interaction |
| --- | --- | --- |
| [Burning] | [Oil] | Burn damage ×2 |
| [Vulnerable] | Any Damage | Damage +50% |
| [Stunned] | Any Action | Action blocked |
| [Silenced] | Ability Use | Non-physical blocked |

---

## VII. Removal Triggers

Effects are removed when:

1. **Duration expires** (reaches 0)
2. **Cleanse ability used** (targeted removal)
3. **Counter-effect applied** (see counter table)
4. **Combat ends** (combat-only effects)
5. **Death** (all effects removed)

---

## VIII. Database Schema

```sql
CREATE TABLE StatusEffects (
    effect_id INTEGER PRIMARY KEY,
    effect_name VARCHAR(50) NOT NULL,
    category ENUM('DoT', 'Buff', 'Debuff', 'Control', 'Conditional'),
    duration_type ENUM('PerTurn', 'PerRound', 'Permanent', 'Conditional'),
    default_duration INTEGER NOT NULL DEFAULT 3,
    max_stacks INTEGER NOT NULL DEFAULT 1,
    stack_behavior ENUM('Additive', 'Refresh', 'Highest', 'Single')
);

CREATE TABLE ActiveEffects (
    instance_id INTEGER PRIMARY KEY AUTO_INCREMENT,
    character_id INTEGER NOT NULL,
    effect_id INTEGER NOT NULL,
    current_stacks INTEGER NOT NULL DEFAULT 1,
    remaining_duration INTEGER NOT NULL,
    source_character_id INTEGER,
    applied_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (effect_id) REFERENCES StatusEffects(effect_id)
);
```

---

## IX. Service Architecture

### AdvancedStatusEffectService

```csharp
public interface IAdvancedStatusEffectService
{
    // Application
    StatusEffectResult ApplyEffect(int targetId, StatusEffect effect, int sourceId);
    bool CheckImmunity(int targetId, EffectCategory category);
    int ApplyResistance(int targetId, int baseDuration);
    
    // Stack Management
    int GetCurrentStacks(int targetId, int effectId);
    void AddStack(int targetId, int effectId);
    void RefreshDuration(int targetId, int effectId);
    
    // Duration Processing
    void ProcessTurnEnd(int characterId);
    void ProcessRoundEnd();
    void RemoveExpiredEffects(int characterId);
}
```

---

## X. Balance Targets

### Target 1: DoT Contribution

- **Metric:** DoT damage as % of total combat damage
- **Target:** 15-25%
- **Levers:** Stack limits, base damage per stack

### Target 2: Buff Uptime

- **Metric:** Average buff uptime per combat
- **Target:** 40-60% of combat duration
- **Levers:** Duration values, cooldowns

---

## XI. Integration Points

**Dependencies:**

- Combat Resolution System → turn/round timing
- Damage Calculation → DoT damage application
- Dice Service → resistance checks

**Referenced By:**

- All combat abilities (apply effects)
- Enemy AI (effect targeting logic)
- Trauma Economy (stress from debuffs)

---

*Consolidated from SPEC-COMBAT-003 (Advanced Status Effects System Specification) per Source Authority guidelines.*