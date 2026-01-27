---
id: SPEC-ABILITY-1503
title: "Inscription Expertise"
parent: runasmidr
tier: 1
type: passive
version: 1.0
---

# Inscription Expertise

**Ability ID:** 1503 | **Tier:** 1 | **Type:** Passive | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Prerequisite** | Rúnasmiðr specialization |

---

## Description

> Your carving is precise, efficient. The patterns flow from muscle memory inherited across generations.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Mechanical Effects:**
- +1d10 to all runeforging checks
- 15% chance for Masterwork inscriptions

**Formula:**
```
RuneforgingBonus = 1d10
MasterworkChance = 0.15
```

**GUI Display:**
- Passive icon: Chisel with rune
- Tooltip: "Inscription Expertise (Rank 1): +1d10 runeforging. 15% Masterwork."
- Border: Bronze

---

### Rank 2 (Upgrade Cost: +2 PP)

**Mechanical Effects:**
- +2d10 to runeforging
- 25% Masterwork chance
- **NEW:** Runeforging time reduced 25%

**Formula:**
```
RuneforgingBonus = 2d10
MasterworkChance = 0.25
TimeMultiplier = 0.75
```

**GUI Display:**
- Tooltip: "Inscription Expertise (Rank 2): +2d10. 25% Masterwork. 25% faster."
- Border: Silver

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- +3d10 to runeforging
- 40% Masterwork chance
- **NEW:** Halve corruption from runic failures

**Formula:**
```
RuneforgingBonus = 3d10
MasterworkChance = 0.40
CorruptionMultiplier = 0.50
```

**GUI Display:**
- Tooltip: "Inscription Expertise (Rank 3): +3d10. 40% Masterwork. Halve corruption."
- Border: Gold

---

## Synergy Table

| Combination | Effect |
|-------------|--------|
| + Runelore Mastery | Stacking time reduction |
| + Elder Patterns | Bonus applies to combinations |
| + Runeforging craft | Primary skill enhancement |

---

## Example Application

> **Scenario:** Kira (Rank 2) inscribes ᚦ Thurisaz onto a sword.
>
> **Base Pool:**
> - WITS: 6
> - Inscription Expertise: +2d10
> - Total: 8d10
>
> **Time:**
> - Base: 2 hours
> - Reduction: 25%
> - Actual: 1.5 hours
>
> **Masterwork Check:**
> - If 5+ successes over DC: 25% chance for Masterwork bonus

---

## Implementation Status

### Balance Data

#### Efficiency
- **Dice:** +3d10 at Rank 3 = ~30% more successes on average.
- **Masterwork:** 40% chance for +50% effect is huge over many inscriptions.
- **Corruption Reduction:** Halves the downside of the spec.

---

### Phased Implementation Guide

#### Phase 1: Mechanics
- [ ] **Hook**: `GetRuneforgingBonus()` returns +Xd10.
- [ ] **Roll**: After success, RNG check for Masterwork.

#### Phase 2: Logic Integration
- [ ] **Time**: Apply TimeMultiplier.
- [ ] **Corruption**: Apply CorruptionMultiplier on failure.

#### Phase 3: Visuals
- [ ] **UI**: Passive icon with border color (Bronze/Silver/Gold).

---

### Testing Requirements

#### Unit Tests
- [ ] **Bonus**: WITS 6 + Rank 2 = 8d10 pool.
- [ ] **Masterwork**: Over many rolls, ~25% (Rank 2) are Masterwork.

#### Integration Tests
- [ ] **Time**: 2 hour base, Rank 2 = 1.5 hours.

#### Manual QA
- [ ] **Tooltip**: Displays correct rank information.

---

### Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

#### Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Bonus | Debug | "Inscription Expertise adds +{Dice}d10." | `Dice` |
| Masterwork | Info | "Masterwork quality achieved!" | - |

---

### Related Specifications
| Document | Purpose |
|----------|---------|
| [Runeforging](../../../../04-systems/crafting/runeforging.md) | Rune system |

---

### Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
