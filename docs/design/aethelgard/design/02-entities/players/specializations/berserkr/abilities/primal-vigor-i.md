---
id: SPEC-ABILITY-3001
title: "Primal Vigor I"
parent: berserkr
tier: 1
type: passive
version: 1.0
---

# Primal Vigor I

**Ability ID:** 3001 | **Tier:** 1 | **Type:** Passive | **PP Cost:** 3

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Free (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Prerequisite** | Berserkr specialization |

---

## 2. Description

> The Berserkr's very physiology is tied to their rage. As their fury builds, their body surges with adrenaline, accelerating recovery.

---

## 3. Mechanical Effects

### 3.1 Fury-Based Stamina Regeneration

```
Stamina Regen Bonus = Fury / 25 (rounded down)
```

| Fury Level | Bonus Regen |
|------------|-------------|
| 0-24 | +0 |
| 25-49 | +1 |
| 50-74 | +2 |
| 75-99 | +3 |
| 100 | +4 |

---

## 4. Rank Progression

### Rank 1 (Base — included with ability unlock)

**Mechanical Effects:**
- +1 Stamina regen per 25 Fury
- Maximum: +4 at 100 Fury

---

### Rank 2 (Upgrade Cost: +2 PP)

**Mechanical Effects:**
- +1.5 Stamina regen per 25 Fury
- Maximum: +6 at 100 Fury
- **NEW:** +5 maximum Stamina

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- +2 Stamina regen per 25 Fury
- Maximum: +8 at 100 Fury
- **NEW:** +10 maximum Stamina
- **NEW:** +1 HP regen per 50 Fury

---

## 5. Synergies

| Combination | Effect |
|-------------|--------|
| + Blood-Fueled | More Fury = more regen |
| + Death or Glory | Faster Fury = faster regen |
| + Long fights | Sustain advantage |

---

## 6. Balance Data

### 6.1 Economy
| Fury | Regen (Rank 1) | Value |
|------|----------------|-------|
| 25 | +1 | Minor |
| 50 | +2 | Good for sustained fights |
| 100 | +4 | Infinite Stamina (allows spamming *Reckless Assault*) |

### 6.2 Comparison
- **Vs Passive:** Most classes get flat +25% regen. Berserkr gets potentially +100% or more, but only when angry.

---

## 7. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Hook**: `OnTurnStart` regen calculation.
- [ ] **Formula**: `BaseRegen + (Fury / 25 * ScalingFactor)`.

### Phase 2: Logic Integration
- [ ] **Rank Levels**: Factor 1.0 (Rank 1), 1.5 (Rank 2), 2.0 (Rank 3).
- [ ] **Stats**: Apply Max Stamina buffs (+5 / +10).

### Phase 3: Visuals
- [ ] **UI**: Show green up-arrows on Stamina bar when effect is high.

---

## 8. Testing Requirements

### 8.1 Unit Tests
- [ ] **Calc**: 25 Fury -> +1 Stamina. 24 Fury -> +0 Stamina.
- [ ] **Rank**: Rank 3, 100 Fury -> +8 Stamina.
- [ ] **HP**: Rank 3, 50 Fury -> +1 HP Regen.

### 8.2 Integration Tests
- [ ] **Turn**: Verify Stamina actually increases by correct amount during turn processing.

### 8.3 Manual QA
- [ ] **Tooltip**: Verify tooltip updates dynamic values based on current Fury.

---

## 9. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 9.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Regen | Debug | "Primal Vigor restores {Stamina} Stamina." | `Stamina` |

---

## 10. Related Specifications
| Document | Purpose |
|----------|---------|
| [Stamina](../../../../01-core/resources/stamina.md) | Resource definition |
| [Fury](../../../../01-core/resources/fury.md) | Fury definition |

---

## 11. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
