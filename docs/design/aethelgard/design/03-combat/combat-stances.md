---
id: SPEC-COMBAT-STANCES
title: "Combat Stances — Complete Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/StanceService.cs"
    status: Planned
---

# Combat Stances — Tactical Positioning

> *"Your stance is your statement. Aggressive, defensive, or balanced—each choice shapes how you fight and how you die."*

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
| Spec ID | `SPEC-COMBAT-STANCES` |
| Category | Combat System |
| Type | Mechanics |
| Dependencies | Combat Resolution |

### 1.2 Core Philosophy

Stances provide **moment-to-moment tactical choices** that trade offense for defense or vice versa. Stance selection should be a meaningful decision made each turn based on battlefield conditions.

---

## 2. Available Stances

### 2.1 Standard Stances

| Stance | Offense | Defense | Special |
|--------|---------|---------|---------|
| **Neutral** | — | — | No modifiers |
| **Aggressive** | +4 flat damage | +25% damage taken | — |
| **Defensive** | -25% damage dealt | +25% damage reduction | +1d10 parry |
| **Mobile** | — | — | +1 movement, -1 attack die |

### 2.2 Specialization Stances

| Stance | Specialization | Effect |
|--------|----------------|--------|
| **Berserker Fury** | Berserkr | +50% damage, no defense |
| **Shield Wall** | Skjaldmaer | +50% block, no movement |
| **Hunter's Focus** | Veiðimaðr | +2 accuracy, no melee |
| **Resonance** | Echo-Caller | +Aether regen, -HP regen |

---

## 3. Neutral Stance

### 3.1 Properties

| Property | Value |
|----------|-------|
| Offense Modifier | None |
| Defense Modifier | None |
| Movement Modifier | None |
| Special | Default stance |

### 3.2 When to Use

- **Uncertain Situations**: Unknown enemy capabilities
- **Balanced Combat**: Neither offense nor defense urgent
- **Resource Conservation**: No Stamina cost to maintain

---

## 4. Aggressive Stance

### 4.1 Properties

| Property | Value |
|----------|-------|
| Offense Modifier | +4 flat damage |
| Defense Modifier | +25% damage taken |
| Movement Modifier | None |
| Stamina Cost | 0 (passive) |

### 4.2 Damage Calculation

```
Normal: Longsword 2d8+3 = 12 avg
Aggressive: 2d8+3+4 = 16 avg (+33%)
```

### 4.3 Defensive Penalty

```
Normal: Take 20 damage
Aggressive: Take 20 × 1.25 = 25 damage
```

### 4.4 When to Use

- **Low Threat**: Enemy nearly dead
- **Glass Cannon Build**: Already low defense
- **Burst Damage**: Need to end fight quickly

---

## 5. Defensive Stance

### 5.1 Properties

| Property | Value |
|----------|-------|
| Offense Modifier | -25% damage dealt |
| Defense Modifier | +25% damage reduction |
| Parry Modifier | +1d10 to parry pool |
| Movement Modifier | None |

### 5.2 Damage Calculation

```
Normal: Longsword 2d8+3 = 12 avg
Defensive: 12 × 0.75 = 9 avg
```

### 5.3 Defense Bonus

```
Normal: Take 20 damage
Defensive: Take 20 × 0.75 = 15 damage
```

### 5.4 When to Use

- **High Threat**: Boss or dangerous enemy
- **Low HP**: Survival priority
- **Parry Specialist**: Maximize riposte chance

---

## 6. Mobile Stance

### 6.1 Properties

| Property | Value |
|----------|-------|
| Offense Modifier | -1 attack die |
| Defense Modifier | None |
| Movement Modifier | +1 tile per turn |
| Special | Can disengage freely |

### 6.2 When to Use

- **Skirmisher Builds**: Hit and run tactics
- **Ranged Priority**: Maintain distance
- **Positioning**: Flank or retreat

---

## 7. Stance Switching

### 7.1 Switching Rules

| Property | Value |
|----------|-------|
| Action Cost | Free action |
| Timing | Start of turn only |
| Limit | Once per turn |
| Interruption | Cannot switch as reaction |

### 7.2 Stance Persistence

- Stances persist until changed
- Stances end at combat end
- Stances do not persist between combats

---

## 8. Specialization Stances

### 8.1 Berserker Fury (Berserkr)

| Property | Value |
|----------|-------|
| Requirement | Berserkr specialization |
| Activation | Costs 10 Rage |
| Effect | +50% damage, cannot use Defend |
| Duration | Until Rage depleted or deactivated |

### 8.2 Shield Wall (Skjaldmaer)

| Property | Value |
|----------|-------|
| Requirement | Skjaldmaer specialization + shield |
| Activation | Standard action |
| Effect | +50% block effectiveness, no movement |
| Duration | Until deactivated |

### 8.3 Hunter's Focus (Veiðimaðr)

| Property | Value |
|----------|-------|
| Requirement | Veiðimaðr specialization |
| Activation | Free action |
| Effect | +2 accuracy with ranged, cannot melee |
| Duration | Until deactivated |

---

## 9. Stance Interactions

### 9.1 With Status Effects

| Stance | Status | Interaction |
|--------|--------|-------------|
| Aggressive | [Inspired] | Stacks (+4 damage + +3 dice) |
| Aggressive | [Vulnerable] on self | Multiplicative (1.25 × 1.25) |
| Defensive | Defend action | Cap at 75% reduction |
| Defensive | Parry | +1d10 bonus applies |

### 9.2 With Archetype Abilities

| Archetype | Stance Synergy |
|-----------|----------------|
| Warrior | Aggressive + high HP |
| Skirmisher | Mobile + evasion |
| Mystic | Defensive + ranged |
| Adept | Varies by specialization |

---

## 10. Technical Implementation

### 10.1 Data Model

```csharp
public enum Stance
{
    Neutral, Aggressive, Defensive, Mobile,
    // Specialization stances
    BerserkerFury, ShieldWall, HunterFocus, Resonance
}

public class StanceEffect
{
    public int FlatDamageBonus { get; set; }
    public float DamageMultiplier { get; set; } = 1.0f;
    public float DefenseMultiplier { get; set; } = 1.0f;
    public int MovementBonus { get; set; }
    public int ParryBonus { get; set; }
    public int AttackDiceModifier { get; set; }
}
```

### 10.2 Service Interface

```csharp
public interface IStanceService
{
    Stance GetCurrentStance(Character character);
    void SetStance(Character character, Stance newStance);
    
    // Calculated Properties
    int GetFlatDamageBonus(Character character);
    float GetDamageMultiplier(Character character);
    float GetDefenseMultiplier(Character character);
    int GetMovementModifier(Character character);
    int GetParryBonus(Character character);
}
```

---

## 11. Phased Implementation Guide

### Phase 1: Core Systems
- [ ] **Data Model**: Implement `Stance` enum and `StanceEffect` class.
- [ ] **Service**: Implement `SetStance` and `GetStance` with state persistence.
- [ ] **Modifiers**: Implement simple retrievers (e.g. `If Aggressive return 4`).

### Phase 2: Combat Integration
- [ ] **Turns**: Hook `SetStance` to "Start of Turn" phase only.
- [ ] **Damage**: Hook `GetFlatDamageBonus` to `DamageCalculation`.
- [ ] **Defense**: Hook `GetDefenseMultiplier` to `DamageMitigation`.

### Phase 3: Specializations
- [ ] **Berserker**: Implement Rage Cost deduction per turn.
- [ ] **Shield Wall**: Implement "Start/Stop" toggle logic.
- [ ] **Mobile**: Implement "Disengage" flag logic.

### Phase 4: UI & Feedback
- [ ] **Selector**: "Stance Swap" buttons on Combat UI.
- [ ] **Indicator**: Icon near character showing current stance.
- [ ] **Log**: "Switched to Aggressive Stance" message.

---

## 12. Testing Requirements

### 12.1 Unit Tests
- [ ] **Aggressive**: Damage +4 verified. Defense x1.25 verified.
- [ ] **Defensive**: Damage x0.75 verified. Defense x0.75 verified.
- [ ] **Mobile**: Movement +1 verified. Attack Dice -1 verified.
- [ ] **Switching**: Cannot switch twice in one turn.
- [ ] **Reset**: All stances revert to Neutral on Combat End.

### 12.2 Integration Tests
- [ ] **Combat Flow**: Start Turn -> Swap Stance -> Attack -> Bonus Applied.
- [ ] **AI**: Enemy switches to Defensive when Low HP (if AI supported).
- [ ] **Rage**: Berserker Stance drops when Rage = 0.

### 12.3 Manual QA
- [ ] **UI**: Verify Stance Buttons disable after use.
- [ ] **Visual**: Shield Wall shows unique shield icon/aura.

---

## 13. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 13.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Stance Change | Info | "{Character} assumed {Stance} stance." | `Character`, `Stance` |
| Stance Drop | Info | "{Character} lost {Stance} stance (Reason: {Reason})." | `Character`, `Stance`, `Reason` |
| Stance Error | Error | "Failed to set stance {Stance}: {Error}" | `Stance`, `Error` |

---

## 14. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-COMBAT-RESOLUTION` | Stance applied in combat |
| `SPEC-COMBAT-OUTCOMES` | Damage modifiers |
| `SPEC-COMBAT-DEFENSE` | Defense modifiers |
