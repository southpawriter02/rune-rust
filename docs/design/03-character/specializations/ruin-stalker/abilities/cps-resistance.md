---
id: SPEC-ABILITY-4006
title: "CPS Resistance"
parent: ruin-stalker
tier: 2
type: passive
version: 1.0
---

# CPS Resistance

**Ability ID:** 4006 | **Tier:** 2 | **Type:** Passive | **PP Cost:** 4

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Free (always active) |
| **Target** | Self (+ allies at Rank 3) |
| **Resource Cost** | None |
| **Prerequisite** | 8 PP in Ruin-Stalker tree |
| **Starting Rank** | 2 |

---

## 2. Description

> "You've trained your mind against the paradoxes. The looped whispers, the recursive visions, the semantic drift—you've built walls against them all."

---

## 3. What is CPS?

**Cognitive Paradox Syndrome** — the occupational hazard of ruin work.

### Stage 1 Precursors

| Precursor | Description |
|-----------|-------------|
| **Auditory paresthesia** | Sounds feel textured |
| **Semantic drift** | Words lose meaning |
| **Looped prosody** | Speech patterns repeat |
| **Fixation** | Cannot look away |

> [!WARNING]
> **Abort Criteria:** Any 2 Stage 1 precursors = immediate exit.

---

## 4. Mechanical Effects

### 4.1 Primary Effect

```
Resolve bonus = +1d10 vs CPS/Psychic Stress
Stress reduction = -2 from anomaly exposure
```

---

## 5. Rank Progression

### Rank 2 (Starting Rank)

**Mechanical Effects:**
- +1d10 to Resolve checks vs CPS/Psychic Stress
- Reduce Psychic Stress from anomalies by 2

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- +2d10 to Resolve checks
- Reduce Stress by 4
- **Immune** to Stage 1 CPS precursors
- **Resistant** to Stage 2
- **NEW:** Can use Silent Room protocols on allies
  - Removes 1d6 Psychic Stress from ally

---

## 6. Silent Room Protocol

| Step | Action |
|------|--------|
| **Dim** | Reduce visual stimulation |
| **Dampen** | Reduce auditory input |
| **De-pattern** | Break repetitive elements |
| **Document** | Record observations |

---

## 7. Synergies

| Combination | Effect |
|-------------|--------|
| + Bone-Setter | Double stress management |
| + Long ruin exploration | Essential for safety |
| + High-CPS zones | Enables longer entry |

---

## 8. Balance Data

### 8.1 Defensive Value
- **Stress Reduction:** -2 Stress per event is massive over a dungeon run (e.g. 50 events = 100 stress saved = 1 Trauma prevented).
- **Immunity:** Stage 1 Immunity allows safe exploration of high-tier ruins.

### 8.2 Comparison
- **Vs Bone-Setter:** Bone-Setter heals damage; Ruin-Stalker prevents mental damage. They are complementary.

---

## 9. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Data**: Update `PsychicStress` calculation to checking `CPSResistance` capability.
- [ ] **Modifier**: Apply -2/-4 distinct reduction.

### Phase 2: Logic Integration
- [ ] **Trigger**: Hook `OnEnvironmentTick` for CPS accumulation.
- [ ] **Immunity**: If `Stage == 1` and `Rank >= 3`, ignore effect.
- [ ] **Cleanse**: Implement Rank 3 Active ability (Peaceful Room).

### Phase 3: Visuals
- [ ] **UI**: Show "Resisted" text when stress is mitigated.
- [ ] **VFX**: "Mental Shield" ripple when anomaly triggers.

---

## 10. Testing Requirements

### 10.1 Unit Tests
- [ ] **Reduction**: Take 10 Stress -> Actual 8 (Rank 2) / 6 (Rank 3).
- [ ] **Immunity**: Apply Stage 1 precursor -> No effect. Apply Stage 2 -> Effect reduced.
- [ ] **Cleanse**: Ally has 50 Stress -> Action -> Ally has 50-1d6 Stress.

### 10.2 Integration Tests
- [ ] **Dungeon Run**: Simulate 100 turns in Red Zone. Verify total stress accumulation is ~40% lower.

### 10.3 Manual QA
- [ ] **Log**: Verify "CPS Resistance mitigated X stress" appears.

---

## 11. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 11.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Mitigate | Info | "{Character} resists the anomaly's whispers (-{Amount} Stress)." | `Character`, `Amount` |
| Cleanse | Info | "{Character} calms {Ally} using silent protocols." | `Character`, `Ally` |

---

## 12. Related Specifications
| Document | Purpose |
|----------|---------|
| [Trauma Economy](../../../../01-core/trauma-economy.md) | Stress mechanics |
| [Resources](../../../../01-core/resources/stress.md) | Stress resource |

---

## 13. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
