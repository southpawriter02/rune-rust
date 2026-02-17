---
id: SPEC-CORE-RES-MOMENTUM
title: "Momentum — Skirmisher Resource Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/Services/MomentumService.cs"
    status: Planned
---

# Momentum — Skirmisher Flow State

> *"Momentum is the rhythm of battle—the flow state where every step leads to the next strike, every dodge sets up the next attack. Stop moving, lose momentum."*

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
| Spec ID | `SPEC-CORE-RES-MOMENTUM` |
| Category | Specialization Resource |
| Parent Spec | `SPEC-CORE-RESOURCES` |
| Availability | **Skirmisher archetypes** |
| Primary Attribute | FINESSE |
| Range | 0-100 |

### 1.2 Core Philosophy

Momentum is a **building resource** for Skirmisher-type characters. It rewards continuous aggressive action and punishes passive play.

**Key Properties:**
- **Builds from Movement + Attacks**: Gains from combos
- **Enables Finishers**: High Momentum unlocks powerful abilities
- **Decays on Inaction**: Standing still or defending loses Momentum
- **Rewards Flow**: Chain abilities to maintain/build

---

## 2. Momentum Mechanics

### 2.1 Momentum Range

| Value | State | Description |
|-------|-------|-------------|
| 0 | Static | No bonus |
| 1-24 | Moving | Minor evasion bonus |
| 25-49 | Flowing | Attack speed bonus |
| 50-74 | Swift | Significant bonuses |
| 75-99 | Blur | Maximum speed |
| 100 | **Cascade** | Unlocks Cascade abilities |

### 2.2 Momentum Generation

| Source | Momentum Gained |
|--------|-----------------|
| Move + Attack (same turn) | +15 |
| Successful hit | +10 |
| Dodge (avoid attack) | +15 |
| Kill enemy | +20 |
| Chain ability (3+ in row) | +25 |

### 2.3 Momentum Decay

| Action | Momentum Lost |
|--------|---------------|
| End turn without attacking | −20 |
| Use Defend action | −15 |
| Take hit | −10 |
| Get stunned/disabled | −30 |

---

## 3. Momentum Thresholds

### 3.1 Bonus Scaling

| Momentum | Defense Bonus | Extra Action |
|----------|---------------|--------------|
| 0-24 | +0 | None |
| 25-49 | +1 | None |
| 50-74 | +2 | None |
| 75-99 | +3 | Bonus Move |
| 100 | +4 | Cascade unlock |

### 3.2 Cascade State

At 100 Momentum, the Skirmisher enters **Cascade**:
- All attacks deal +25% damage
- Free disengage after each attack
- Cascade abilities become available
- Lasts until Momentum drops below 75

---

## 4. Momentum Abilities

### 4.1 Momentum-Powered Abilities

| Ability | Momentum Cost | Effect |
|---------|---------------|--------|
| Quick Strike | 0 (builds +10) | Fast attack |
| Blade Dance | -25 | Attack all adjacent |
| Shadow Step | -30 | Teleport short distance |
| Whirlwind | -50 | Spin attack + move |
| Cascade: Hundred Cuts | -100 | Multi-hit devastation |

---

## 5. Technical Implementation

### 5.1 Momentum Service

```csharp
public interface IMomentumService
{
    int GetCurrentMomentum(Character character);
    void GainMomentum(Character character, int amount, MomentumSource source);
    void DecayMomentum(Character character, MomentumDecayReason reason);
    MomentumThreshold GetThreshold(Character character);
    bool IsInCascade(Character character);
}

public enum MomentumSource { MoveAttack, Hit, Dodge, Kill, Chain }
public enum MomentumDecayReason { NoAttack, Defend, TookHit, Disabled }
```

---

## 6. Phased Implementation Guide

### Phase 1: Core Logic
- [ ] **Data**: Update `Character` model to track `CurrentMomentum`.
- [ ] **Service**: Implement `GainMomentum` (cap 100) and `DecayMomentum` (floor 0).
- [ ] **Thresholds**: Implement logic to detect thresholds (25, 50, 75, 100) and Cascade state.

### Phase 2: Combat Integration
- [ ] **Triggers**: Hook `OnAttackHit`, `OnDodge`, `OnKill` events to `GainMomentum`.
- [ ] **Decay**: Hook `OnTurnEnd` and `OnTakeDamage` events to `DecayMomentum`.
- [ ] **Bonuses**: Apply Defense/Speed bonuses based on Current Momentum.

### Phase 3: Cascade
- [ ] **State**: Implement Cascade Buff (+25% damage).
- [ ] **Exit**: Logic to end Cascade when dropping below 75.

### Phase 4: UI & Feedback
- [ ] **HUD**: Display Momentum Gauge (fills rapidly).
- [ ] **Feedback**: "MOMENTUM UP!" text on gains.
- [ ] **Cascade**: Special specific visual effect when hitting 100.

---

## 7. Testing Requirements

### 7.1 Unit Tests
- [ ] **Gain**: Verify gain amounts match table.
- [ ] **Decay**: Verify decay amounts match table.
- [ ] **Cap**: Verify cannot exceed 100 or drop below 0.
- [ ] **Cascade**: Verify Cascade state triggers at 100 and ends < 75.

### 7.2 Integration Tests
- [ ] **Combat**: Attack -> Hit -> Gain 10 Momentum.
- [ ] **Combo**: Attack + Kill -> Gain 10+20=30 Momentum.
- [ ] **Decay**: Pass Turn -> Lose 20 Momentum.

### 7.3 Manual QA
- [ ] **UI**: Hit enemy -> Watch bar fill.
- [ ] **Sound**: Verify distinct sound on Cascade entry.

---

## 8. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 8.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Gain | Info | "{Character} built {Amount} Momentum ({Source}). Total: {Total}" | `Character`, `Amount`, `Source`, `Total` |
| Decay | Info | "{Character} lost {Amount} Momentum ({Reason}). Total: {Total}" | `Character`, `Amount`, `Reason`, `Total` |
| Cascade | Warning | "{Character} entered CASCADE STATE!" | `Character` |

---

## 9. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-CORE-RESOURCES` | Parent overview spec |
| `SPEC-ARCHETYPE-SKIRMISHER` | Skirmisher archetype |
| `SPEC-CORE-ATTR-FINESSE` | Primary attribute |
