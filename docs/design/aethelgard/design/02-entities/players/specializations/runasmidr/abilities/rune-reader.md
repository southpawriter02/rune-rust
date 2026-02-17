---
id: SPEC-ABILITY-1501
title: "Rune Reader"
parent: runasmidr
tier: 1
type: passive
version: 1.0
---

# Rune Reader

**Ability ID:** 1501 | **Tier:** 1 | **Type:** Passive | **PP Cost:** 3

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

> The carved symbols speak to you. Where others see meaningless scratches, you perceive patterns — ancient, powerful, waiting.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Mechanical Effects:**
- +1d10 to identifying runic inscriptions
- Detect runic inscriptions within 15 ft
- Can identify common runes (DC ≤ 12)

**Formula:**
```
IdentificationBonus = 1d10
DetectionRange = 15 ft
IdentifiableRuneDC <= 12
```

**GUI Display:**
- Passive icon: Eye with rune
- Tooltip: "Rune Reader (Rank 1): +1d10 to identify runes. Detect within 15 ft. Identify common runes."
- Border: Bronze

---

### Rank 2 (Upgrade Cost: +2 PP)

**Mechanical Effects:**
- +2d10 to identification
- Detect range: 30 ft
- Can identify standard runes (DC ≤ 16)
- **NEW:** Sense if rune is beneficial or harmful

**Formula:**
```
IdentificationBonus = 2d10
DetectionRange = 30 ft
IdentifiableRuneDC <= 16
CanSenseIntent = true
```

**GUI Display:**
- Tooltip: "Rune Reader (Rank 2): +2d10. Detect 30 ft. Standard runes. Sense intent."
- Border: Silver

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- +3d10 to identification
- Detect range: 50 ft
- Can identify all runes including forbidden
- **NEW:** See rune's creator and age

**Formula:**
```
IdentificationBonus = 3d10
DetectionRange = 50 ft
IdentifiableRuneDC = ALL
CanSeeHistory = true
```

**GUI Display:**
- Tooltip: "Rune Reader (Rank 3): +3d10. Detect 50 ft. All runes. See creator/age."
- Border: Gold

---

## Example Scenario

> **Situation:** Kira enters a ruined chamber. Her Rune Reader (Rank 2) activates.
>
> **Detection:** Senses runic inscriptions on the far wall (25 ft away)
>
> **Identification Check:**
> - WITS: 6
> - Rune Reader: +2d10
> - Pool: 8d10 → [8, 7, 3, 9, 4, 7, 2, 8] = 5 successes
> - vs DC 14 (standard rune) = SUCCESS
>
> **Result:** Identifies as ᛉ Algiz (protection ward). Senses it is **beneficial** — likely protecting something within.

---

## Implementation Status

### Balance Data

#### Utility
- **Detection:** Prevents walking into runic traps.
- **History:** Rank 3's "See creator/age" is mostly flavor but useful for lore.

---

### Phased Implementation Guide

#### Phase 1: Mechanics
- [ ] **Sense**: Hook `OnEnvironmentEnter` -> Scan for Rune objects in range.
- [ ] **Identify**: WITS check vs Rune DC -> Return rune info.

#### Phase 2: Logic Integration
- [ ] **Intent**: Add `Rune.Intent` (Beneficial/Harmful/Neutral).
- [ ] **History**: Add `Rune.Creator`, `Rune.Age`.

#### Phase 3: Visuals
- [ ] **UI**: Highlight detected runes with icon.

---

### Testing Requirements

#### Unit Tests
- [ ] **Range**: Rune 20ft away (Rank 1 range 15ft) -> Not detected.
- [ ] **Identify**: WITS check succeeds vs DC 12 -> Rune info returned.

#### Integration Tests
- [ ] **Trap**: Detect rune trap -> Player can disarm or avoid.

#### Manual QA
- [ ] **Tooltip**: Hover on detected rune shows "ᛉ Algiz - Protection".

---

### Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

#### Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Detect | Debug | "Rune detected at {Location}." | `Location` |
| Identify | Info | "Identified as {Rune}." | `Rune` |

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
