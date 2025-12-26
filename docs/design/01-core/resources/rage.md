---
id: SPEC-CORE-RES-RAGE
title: "Rage — Berserker Resource Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/Services/RageService.cs"
    status: Planned
---

# Rage — Berserker Fury System

> *"Rage is the fire that burns away fear, pain, and reason. It is power—but power without control. The longer it burns, the harder it is to stop."*

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
| Spec ID | `SPEC-CORE-RES-RAGE` |
| Category | Specialization Resource |
| Parent Spec | `SPEC-CORE-RESOURCES` |
| Availability | **Berserker specialization only** |
| Primary Attribute | MIGHT |
| Range | 0-100 |

### 1.2 Core Philosophy

Rage is a **building resource** unique to the Berserker specialization. Unlike other resources that deplete, Rage **accumulates** during combat through dealing and receiving damage.

**Key Properties:**
- **Builds in Combat**: Increases from damage dealt/received
- **Powers Abilities**: Higher Rage = stronger Berserker abilities
- **Fades Out of Combat**: Drains rapidly when not fighting
- **Risk/Reward**: Maximum Rage grants power but risks losing control

---

## 2. Rage Mechanics

### 2.1 Rage Range

| Value | State | Description |
|-------|-------|-------------|
| 0 | Calm | No rage bonus |
| 1-24 | Simmering | Minor power increase |
| 25-49 | Angry | Moderate power |
| 50-74 | Furious | Significant power |
| 75-99 | Berserk | Maximum power, risk rising |
| 100 | **Rampage** | Peak power, control check |

### 2.2 Rage Generation

| Source | Rage Gained |
|--------|-------------|
| Deal damage (per 10 HP) | +5 Rage |
| Take damage (per 10 HP) | +10 Rage |
| Kill enemy | +15 Rage |
| Ally drops to 0 HP | +20 Rage |
| Use Rage ability | Variable |

### 2.3 Rage Decay

**Out of Combat:**
```
Rage Decay = 10 per round (exploration)
```

**Combat End:**
- Rage persists for 3 rounds before rapid decay begins

---

## 3. Rage Thresholds

### 3.1 Power Scaling

| Rage Level | Damage Bonus | Speed Bonus |
|------------|--------------|-------------|
| 0-24 | +0% | +0 |
| 25-49 | +15% | +1 Initiative |
| 50-74 | +30% | +2 Initiative |
| 75-99 | +50% | +3 Initiative |
| 100 | +75% | +4 Initiative |

### 3.2 Risk Mechanics

At high Rage levels, the Berserker risks losing control:

| Rage Level | Risk |
|------------|------|
| 75-99 | Cannot use non-attack actions voluntarily |
| 100 | **Control Check** required each turn |

**Control Check:**
```
Roll: WILL vs DC 3
Success: Retain control, choose actions
Failure: Auto-attack nearest target (friend or foe)
```

---

## 4. Rage Abilities

### 4.1 Rage-Powered Abilities

| Ability | Rage Cost | Effect |
|---------|-----------|--------|
| Raging Strike | 0 (passive) | +Rage% damage |
| Blood Frenzy | -25 | Heal (Rage/2) HP |
| Unstoppable | -30 | Ignore next disable |
| Earthshatter | -50 | AoE + knockdown |
| Capstone: Unstoppable Fury | -100 | Transform state |

### 4.2 Rage Dump

The Berserker can "dump" Rage into a devastating finisher:

```
Rage Dump:
Cost: All current Rage
Effect: Deal (Rage × 2)% bonus damage on next attack
After: Rage resets to 0, gain [Exhausted] for 2 turns
```

---

## 5. Technical Implementation

### 5.1 Rage Service

```csharp
public interface IRageService
{
    int GetCurrentRage(Character character);
    void GainRage(Character character, int amount, RageSource source);
    void SpendRage(Character character, int cost);
    void DecayRage(Character character);  // Called each out-of-combat round
    RageThreshold GetThreshold(Character character);
    ControlCheckResult PerformControlCheck(Character character);
}

public enum RageSource { DamageDealt, DamageTaken, Kill, AllyDown, Ability }
```

---

## 6. Phased Implementation Guide

### Phase 1: Core Logic
- [ ] **Data**: Update `Character` model to track `CurrentRage`.
- [ ] **Service**: Implement `GainRage` (cap 100) and `DecayRage` (floor 0).
- [ ] **Decay**: Implement logic for "Combat End" delay (3 rounds) before rapid decay.

### Phase 2: Combat Integration
- [ ] **Triggers**: Hook `OnDamageDealt`, `OnDamageTaken` events to `GainRage`.
- [ ] **Scaling**: Hook `RageThreshold` to Damage/Speed stats.
- [ ] **Risk**: Implement "Loss of Control" check logic.

### Phase 3: Abilities
- [ ] **Costs**: Implement "Spend Rage" logic for abilities.
- [ ] **Dump**: Implement `RageDump` mechanic (Reset to 0, Massive Bonus).

### Phase 4: UI & Feedback
- [ ] **HUD**: Display Rage gauge (Red/Fire).
- [ ] **Feedback**: "RAGE RISING!" text/FX on gain.
- [ ] **Warning**: "LOSING CONTROL!" warning at 100 Rage.

---

## 7. Testing Requirements

### 7.1 Unit Tests
- [ ] **Gain**: Verify gain amounts (5/10/15).
- [ ] **Calculations**: Verify Damage Bonus % matches table.
- [ ] **Decay**: Verify no decay for 3 rounds post-combat.
- [ ] **Control**: Verify failure forces specific action type.

### 7.2 Integration Tests
- [ ] **Combat**: Attack -> Hit -> Rage increases.
- [ ] **Scaling**: Rage 50 -> Verify Damage is +30%.
- [ ] **Dump**: Use Dump -> Verity 0 Rage and Exhausted status.

### 7.3 Manual QA
- [ ] **UI**: Take damage -> Verify gauge jumps up.
- [ ] **Control**: Reach 100 Rage -> Verify popup/forced action.

---

## 8. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 8.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Rage Gain | Info | "{Character} gained {Amount} Rage ({Source}). Total: {Total}" | `Character`, `Amount`, `Source`, `Total` |
| Control Check | Warning | "{Character} RAGE CONTROL CHECK: {Outcome} (DC {DC})" | `Character`, `Outcome`, `DC` |
| Rage Dump | Info | "{Character} UNLEASHED RAGE! ({Amount} consumed)" | `Character`, `Amount` |

---

## 9. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-CORE-RESOURCES` | Parent overview spec |
| `SPEC-SPEC-BERSERKER` | Berserker specialization |
| `SPEC-CORE-ATTR-MIGHT` | Scales certain Rage effects |
