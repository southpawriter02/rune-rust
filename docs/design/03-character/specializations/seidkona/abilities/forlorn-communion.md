---
id: ABILITY-SEIDKONA-27004
title: "Forlorn Communion"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Forlorn Communion

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action (out of combat) |
| **Target** | Forlorn entity or Psychic Resonance Zone |
| **Resource Cost** | Aether |
| **Psychic Stress** | +10-15 (unavoidable) |
| **Ranks** | 2 → 3 |

---

## Description

You open your mind to the fragmentary consciousness of the dead, trading sanity for forbidden knowledge.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- WITS check (DC 12)
- Success: Reveal enemy weaknesses, puzzle solutions, lore, hidden paths
- **COST: +15 Psychic Stress (unavoidable)**

**Formula:**
```
WITSCheck = Roll(WITS) vs DC 12

If Success:
    Reveal(EnemyWeaknesses)
    Reveal(PuzzleSolutions)
    Reveal(Lore)
    Reveal(HiddenPaths)

// Always applied, regardless of success
Caster.PsychicStress += 15
```

**Tooltip:** "Forlorn Communion (Rank 2): DC 12 WITS. Gain knowledge. COST: +15 Psychic Stress (unavoidable)"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- DC reduced to 8
- Stress cost reduced to +10
- **NEW:** Can ask 2 questions per communion

**Formula:**
```
WITSCheck = Roll(WITS) vs DC 8
QuestionsAllowed = 2
Caster.PsychicStress += 10
```

**Tooltip:** "Forlorn Communion (Rank 3): DC 8 WITS. 2 questions. +10 Psychic Stress."

---

## Knowledge Types Available

| Category | Examples |
|----------|----------|
| Enemy Weaknesses | Vulnerabilities, resistances, behaviors |
| Puzzle Solutions | Hints, key locations, mechanisms |
| Lore | Historical events, faction secrets |
| Hidden Paths | Secret doors, shortcuts, traps |

---

## Moment of Clarity Enhancement

During Moment of Clarity:
- 0 Aether cost
- +7 Psychic Stress (reduced from normal)
- Auto-success on WITS check

---

## Combat Log Examples

- "Forlorn Communion: Reaching out to the echoes..."
- "WITS check DC 12: SUCCESS"
- "Knowledge gained: Enemy vulnerable to Fire damage"
- "+15 Psychic Stress (Forlorn Communion cost)"
- "Rank 3: Second question available"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress mechanics |
