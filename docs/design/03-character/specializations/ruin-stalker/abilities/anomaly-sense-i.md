---
id: SPEC-ABILITY-4001
title: "Anomaly Sense I"
parent: ruin-stalker
tier: 1
type: passive
version: 1.0
---

# Anomaly Sense I

**Ability ID:** 4001 | **Tier:** 1 | **Type:** Passive | **PP Cost:** 3

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Free (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Prerequisite** | Ruin-Stalker specialization |

---

## 2. Description

> "Your senses are attuned to wrongness. The subtle displacement of air, the faint hum of active systems, the almost-imperceptible flicker of unstable geometry—you perceive what kills others."

---

## 3. Mechanical Effects

### 3.1 Detection Bonus

```
WITS check bonus = +1d10 (all trap/anomaly/hazard detection)
On success = Identify anomaly type
```

---

## 4. Rank Progression

### Rank 1 (Base — included with ability unlock)

**Mechanical Effects:**
- +1d10 to WITS checks for detecting traps, anomalies, hazards
- Can identify anomaly type on success

---

### Rank 2 (Upgrade Cost: +2 PP)

**Mechanical Effects:**
- +2d10 to detection checks
- Can estimate anomaly danger level:
  - Minor / Moderate / Severe / Lethal

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- +3d10 to detection checks
- **Automatic** detection of all anomalies within 10 meters
- Immune to surprise from environmental hazards

---

## 5. Anomaly Classification

| Danger Level | Description | Examples |
|--------------|-------------|----------|
| **Minor** | Low damage, easy to avoid | Unstable floor |
| **Moderate** | Medium damage, requires caution | Echo Surface |
| **Severe** | High damage, complex bypass | Loop Corridor |
| **Lethal** | Death risk, specialist needed | Frame Residue |

---

## 6. Balance Data

### 6.1 Passive Budget
| Rank | Bonus | Value |
|------|-------|-------|
| I | +1d10 | Standard Perk (Tier 1) |
| II | +2d10 | Advanced Perk (Tier 2) |
| III | +3d10 + Auto | Mastery (Tier 3) |

### 6.2 Comparison
- **Vs WITS:** Flat WITS investment is more expensive per die. This is highly efficient for specialized detection.

---

## 7. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Check**: Hook into `AttributeCheck` to add bonus dice when Context = `Trajectory` or `Trap`.
- [ ] **Reveal**: Implement `OnSuccess` logic to show Anomaly Type.

### Phase 2: Logic Integration
- [ ] **Rank Checks**: Ensure correct dice count per rank.
- [ ] **Data**: Update `Anomaly` class to support "Danger Level" output (Rank 2).

### Phase 3: Visuals
- [ ] **Highlight**: Shader effect for detected objects.
- [ ] **Log**: "Sensed anomaly: {Type} ({Danger})."

---

## 8. Testing Requirements

### 8.1 Unit Tests
- [ ] **Bonus**: WITS check for trap -> Adds 1/2/3 dice.
- [ ] **Identification**: Success -> Returns correct Anomaly.Type string.
- [ ] **Auto**: Rank 3 -> RevealAll() called without check.

### 8.2 Integration Tests
- [ ] **Exploration**: Walking near trap triggers passive check.
- [ ] **Ambush**: Surprise round form Hazard negated (Rank 3).

### 8.3 Manual QA
- [ ] **Visual**: Objects glow/outline when detected.

---

## 9. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 9.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Sense | Info | "You sense a {Type} anomaly nearby." | `Type` |
| Passive | Debug | "Anomaly Sense added {Dice}d10 to check." | `Dice` |

---

## 10. Related Specifications
| Document | Purpose |
|----------|---------|
| [Hazard Mapping](hazard-mapping.md) | Mapping discovered anomalies |
| [WITS Attribute](../../../../01-core/attributes/wits.md) | Base attribute |

---

## 11. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
