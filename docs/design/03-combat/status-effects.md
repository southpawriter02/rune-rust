---
id: SPEC-COMBAT-STATUS
title: "Status Effects — Complete Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/StatusEffectService.cs"
    status: Planned
---

# Status Effects — Buffs, Debuffs, and Conditions

> *"The battlefield is not just steel and flesh—it is layers of advantage, disadvantage, and corruption woven through every combatant."*

---

## Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-COMBAT-STATUS` |
| Category | Combat System |
| Type | Status Registry |
| Dependencies | Combat Resolution |

### 1.2 Core Philosophy

Status effects create **tactical depth** and **combo potential**. Setup turns (applying debuffs) should pay off in subsequent turns (exploiting debuffs for bonus damage).

---

## 2. Status Categories

### 2.1 Category Overview

| Category | Effect Type | Examples |
|----------|-------------|----------|
| **Offensive Buffs** | Increase damage/accuracy | [Inspired], Battle Rage |
| **Defensive Buffs** | Reduce damage taken | Defense Bonus, Shield |
| **Debuffs** | Weaken target | [Vulnerable], [Analyzed] |
| **Control** | Disable actions | [Stunned], [Silenced] |
| **Damage over Time** | Ongoing damage | [Bleeding], [Burning] |
| **Corruption** | Heretical effects | [Corrupted], [Blighted] |

---

## 3. Offensive Buffs

### 3.1 [Inspired]

| Property | Value |
|----------|-------|
| Source | Skald performance |
| Duration | Performance duration (3-5 turns) |
| Effect | +3 damage dice before rolling |
| Stack | No (refreshes duration) |

**Example:**
```
Longsword 2d8 → [Inspired] → 5d8
Average: 9 → 22.5 damage
```

### 3.2 Battle Rage

| Property | Value |
|----------|-------|
| Source | Warrior ability |
| Duration | 3 turns |
| Effect | +2 attack dice |
| Trade-off | +25% damage taken |

### 3.3 Saga of Courage

| Property | Value |
|----------|-------|
| Source | Skald performance |
| Duration | Performance duration |
| Effect | +2 accuracy dice |
| Stack | No |

---

## 4. Defensive Buffs

### 4.1 Defense Bonus

| Property | Value |
|----------|-------|
| Source | Defend action |
| Duration | 1 turn or first hit |
| Effect | 0-75% damage reduction |
| Consumed | After first hit |

**Calculation:**
```
Defense Bonus = STURDINESS successes × 25%
Cap: 75%
```

### 4.2 Shield Absorption

| Property | Value |
|----------|-------|
| Source | Aetheric Shield (Mystic) |
| Duration | Until depleted |
| Effect | Absorbs next X damage (15 typical) |
| Stack | No (refreshes pool) |

### 4.3 Temporary HP

| Property | Value |
|----------|-------|
| Source | Saga of the Einherjar |
| Duration | Until depleted or combat end |
| Effect | Extra HP pool (2d8, ~9 avg) |
| Priority | Damage hits Temp HP first |

---

## 5. Debuffs

### 5.1 [Vulnerable]

| Property | Value |
|----------|-------|
| Source | Bone-Setter ability |
| Duration | 3 turns |
| Effect | +25% damage taken |
| Stack | No (refreshes) |

**Calculation:**
```
Normal: 20 damage
[Vulnerable]: 20 × 1.25 = 25 damage
```

### 5.2 [Analyzed]

| Property | Value |
|----------|-------|
| Source | Jötun-Reader ability |
| Duration | 4 turns |
| Effect | +2 accuracy for ALL attackers |
| Stack | No |

**Party-Wide Benefit:** Every ally gains +2 accuracy against this target.

### 5.3 [Corrupted]

| Property | Value |
|----------|-------|
| Source | Heretical abilities |
| Duration | 3 turns |
| Effect | +20% Stress gain |
| Stack | No |

---

## 6. Control Effects

### 6.1 [Stunned]

| Property | Value |
|----------|-------|
| Source | Disrupt ability, special attacks |
| Duration | 1 turn |
| Effect | Target skips turn |
| Stack | No (refreshes) |

### 6.2 [Silenced]

| Property | Value |
|----------|-------|
| Source | Song of Silence |
| Duration | 3 turns |
| Effect | Cannot cast spells or perform |
| Stack | No |

### 6.3 [Seized]

| Property | Value |
|----------|-------|
| Source | Architect of the Silence |
| Duration | 2 turns |
| Effect | Cannot move or act |
| Stack | No |

### 6.4 [Staggered]

| Property | Value |
|----------|-------|
| Source | Riposte (Hólmgangr Rank 3) |
| Duration | 1 turn |
| Effect | -2 to all dice pools |
| Stack | No |

### 6.5 [Blinded]

| Property | Value |
|----------|-------|
| Source | [Absolute Darkness] condition, Flash abilities, Eye injuries |
| Duration | Varies (condition-based = while in zone, ability-based = 1-3 turns) |
| Effect | See detailed effects below |
| Stack | No |
| Counter | Tremorsense (Gorge-Maw Ascetic), Echolocation (Echo-Caller) |

**Detailed Effects:**
```
[BLINDED] — Sight-based perception eliminated
  • 50% chance for attacks to miss completely (target empty adjacent tile)
  • Defense Score reduced to ZERO (cannot see attacks to evade/parry)
  • Cannot target specific enemies in groups — attacks hit random enemy in row
  • Perception (WITS) checks based on sight automatically fail
  • Ranged attacks suffer additional -2 dice (beyond miss chance)
  • Cannot benefit from [Concealed] or cover bonuses
```

**Immunity:**
- Gorge-Maw Ascetic with `Tremorsense` — perceives through seismic vibration
- Echo-Caller with `Echolocation` — partial mitigation (-2 dice instead of full blindness)
- Constructs with non-visual sensors

> See [Absolute Darkness](../07-environment/ambient-conditions.md#331-absolute-darkness) for the most severe application of this status.

---

## 7. Damage over Time (DoT)

### 7.1 [Bleeding]

| Property | Value |
|----------|-------|
| Source | Precision Strike, critical hits |
| Duration | 2-3 turns |
| Effect | 1d6 damage per turn |
| Tick Timing | Start of target's turn |
| Can Kill | Yes |
| Stack | Duration refreshes, damage stacks (max 3) |

### 7.2 [Burning]

| Property | Value |
|----------|-------|
| Source | Fire abilities, hazards |
| Duration | 2 turns |
| Effect | 2d6 damage per turn |
| Spread | May spread to adjacent targets |
| Can Kill | Yes |

### 7.3 [Poisoned]

| Property | Value |
|----------|-------|
| Source | Poison weapons, enemies |
| Duration | 4 turns |
| Effect | 1d4 damage, -1 to rolls |
| Cure | Antidote consumable |

### 7.4 [Exhausted]

| Property | Value |
|----------|-------|
| Source | Resting without resources |
| Duration | Until next full rest (with resources) |
| Effect | Recovery capped 50%, -1 all dice pools, no stress reduction |
| Stack | No |
| Ambush Modifier | +10% ambush chance |

**Full Effect Description:**
```
[EXHAUSTED]
- Recovery capped at 50% (was 75%) during wilderness rest
- -1 to all dice pools
- Cannot benefit from stress reduction during wilderness rest
- +10% ambush chance while camping
- Cure: Complete a rest with full resources (1 Ration + 1 Water)
```

> See [resting-system.md](../04-systems/resting-system.md) for resource consumption mechanics.

---

## 8. Duration Tracking

### 8.1 Duration Table

| Effect | Duration | Tick Timing | Expires When |
|--------|----------|-------------|--------------|
| Defense Bonus | 1 turn | On hit | First hit or turn end |
| Battle Rage | 3 turns | End of round | Duration = 0 |
| [Stunned] | 1 turn | End of round | 1 turn skipped |
| [Blinded] | Varies | End of round | Duration = 0 or leave zone |
| [Bleeding] | 2-3 turns | Start of turn | Duration = 0 |
| [Vulnerable] | 3 turns | End of round | Duration = 0 |
| [Analyzed] | 4 turns | End of round | Duration = 0 |
| [Exhausted] | Until cured | On rest | Full rest with resources |
| Temp HP | Until depleted | On damage | HP = 0 |
| Performance | Varies | End of round | Interrupted/ended |

### 8.2 Turn vs Round

| Term | Definition |
|------|------------|
| **Turn** | One participant's action |
| **Round** | All participants act once |

Most status effects decrement at end of **round** (after everyone has acted).

---

## 9. Stacking Rules

### 9.1 General Rules

| Case | Behavior |
|------|----------|
| Same effect, same source | Duration refreshed |
| Same effect, different source | Duration refreshed (no stack) |
| Different effects | All apply simultaneously |

### 9.2 Multiplicative Stacking

When multiple multipliers apply:

```
[Vulnerable] +25% × Aggressive Stance +25% = 1.25 × 1.25 = 1.5625 (+56%)
```

### 9.3 DoT Stacking

| DoT | Stacking |
|-----|----------|
| [Bleeding] | Max 3 stacks (3d6/turn) |
| [Burning] | Duration refreshes only |
| [Poisoned] | Duration refreshes only |

---

## 10. Status Interactions

### 10.1 Immunity

| Character Type | Immune To |
|----------------|-----------|
| Undying (Undead) | [Bleeding], [Poisoned] |
| Constructs | [Silenced], [Poisoned] |
| Bosses | [Stunned] (50% resist) |

### 10.2 Cleansing

| Method | Clears |
|--------|--------|
| Antidote | [Poisoned] |
| Purify (Ability) | All debuffs |
| Combat End | All temporary statuses |

---

## 11. Technical Implementation

### 11.1 Data Model
```csharp
public class StatusEffect
{
    public string Id { get; set; }
    public string Name { get; set; }
    public StatusCategory Category { get; set; }
    public int Duration { get; set; }
    public int Stacks { get; set; }
    public StatusEffectData Data { get; set; }
}

public enum StatusCategory { OffensiveBuff, DefensiveBuff, Debuff, Control, DamageOverTime, Corruption }

public class StatusEffectData
{
    public int DamageDiceBonus { get; set; } // +3 dice
    public int AccuracyBonus { get; set; }
    public float DamageMultiplier { get; set; } = 1.0f; // 1.25 for Vuln
    public float DefenseMultiplier { get; set; } = 1.0f;
    public int DotDamageDice { get; set; }
    public bool SkipsTurn { get; set; }
    public bool BlocksSpells { get; set; }
}
```

### 11.2 Service Interface
```csharp
public interface IStatusEffectService
{
    void ApplyStatus(Character target, StatusEffect effect);
    void RemoveStatus(Character target, string statusId);
    void TickStatuses(Character character); // End of turn
    
    // Queries
    bool HasStatus(Character character, string statusId);
    List<StatusEffect> GetAllStatuses(Character character);
    
    // Calculation Helpers
    int GetTotalAccuracyBonus(Character attacker, Character defender);
    int GetTotalDamageDiceBonus(Character attacker);
    float GetTotalDamageMultiplier(Character target);
}
```

---

## 12. Phased Implementation Guide

### Phase 1: Core System
- [ ] **Data Model**: Implement `StatusEffect` and `StatusCategory`.
- [ ] **Service**: Implement `ApplyStatus`, `RemoveStatus`, `TickStatuses`.
- [ ] **Registry**: create `StatusRegistry` with definitions for [Inspired], [Bleeding], etc.

### Phase 2: Combat Integration
- [ ] **Modifiers**: Hook `GetTotalDamageMultiplier` into Combat Resolution.
- [ ] **Control**: Hook `SkipsTurn` to Turn Manager (skip logic).
- [ ] **Events**: Trigger `OnAttack` and `OnDefend` status hooks.

### Phase 3: Advanced Logic
- [ ] **DoT**: Implement "Tick Damage" logic at start of turn.
- [ ] **Stacking**: Implement "Refresh vs Stack" logic per effect type.
- [ ] **Immunity**: Implement `CanApply` check using `CreatureTraits`.

### Phase 4: UI & Feedback
- [ ] **Icons**: Status Icons above unit frames.
- [ ] **Floaters**: "Bleeding!" text when applied.
- [ ] **Tooltips**: Hover icon shows "Lasts 2 turns. +25% Damage taken."

---

## 13. Testing Requirements

### 13.1 Unit Tests
- [ ] **Inspired**: +3 Damage Dice confirmed.
- [ ] **Vulnerable**: Damage x1.25 confirmed.
- [ ] **Stunned**: HasStatus(Stunned) -> CanAct returns false.
- [ ] **Bleeding**: Tick -> HP decreased by 1d6.
- [ ] **Expiration**: Duration 1 -> Tick -> Removed.
- [ ] **Stacking**: 1 Stack + 1 Stack -> 2 Stacks (if stackable).

### 13.2 Integration Tests
- [ ] **Flow**: Apply Poison -> Wait 4 Turns -> Poison Gone.
- [ ] **Combo**: Apply Vulnerable + Aggressive Stance -> Damage x1.56.
- [ ] **Immunity**: Apply Bleed to Skeleton -> Failed.

### 13.3 Manual QA
- [ ] **UI**: Icon appears on application, disappears on expiry.
- [ ] **Log**: "Player is Inspired!" message verified.

---

## 14. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Status Apply | Info | "{Target} gained {Status} ({Duration} turns)." | `Target`, `Status`, `Duration` |
| Status Remove | Info | "{Target} lost {Status}." | `Target`, `Status` |
| Status Resist | Info | "{Target} resisted {Status} (Immune)." | `Target`, `Status` |
| Status Tick | Debug | "{Status} ticked on {Target}: {Effect}." | `Status`, `Target`, `Effect` |

---

## 15. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| [Combat Resolution](combat-resolution.md) | Status applied in combat |
| [Active Abilities](active-abilities.md) | Abilities that apply statuses |
| [Attack Outcomes](attack-outcomes.md) | Damage modifiers |
| [Trauma System](../01-core/trauma.md) | Corruption effects |
