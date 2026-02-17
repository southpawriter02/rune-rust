---
id: ABILITY-SKAR-HORDE-29008
title: "The Price of Power"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# The Price of Power

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action |
| **Target** | Self |
| **Resource Cost** | None |
| **Risk** | Psychic Stress generation |
| **Ranks** | 2 → 3 |

---

## Description

The rush of transhuman power is intoxicating. The whispers in your mind grow louder. You do not care. Power is worth any price.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- +75% Savagery generation from ALL sources
- Gain 1 Psychic Stress per 10 Savagery generated

**Formula:**
```
SavageryGeneration *= 1.75
OnSavageryGenerated:
    PsychicStress += Floor(SavageryGenerated / 10)
```

**Tooltip:** "The Price of Power (Rank 2): +75% Savagery generation. Gain 1 Stress per 10 Savagery generated."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +100% Savagery generation (double)
- Stress generation reduced: 1 Stress per 15 Savagery

**Formula:**
```
SavageryGeneration *= 2.0
OnSavageryGenerated:
    PsychicStress += Floor(SavageryGenerated / 15)
```

**Tooltip:** "The Price of Power (Rank 3): +100% Savagery (double). 1 Stress per 15 Savagery."

---

## Savagery Multiplier Examples

| Source | Base | Rank 2 (+75%) | Rank 3 (+100%) |
|--------|------|---------------|----------------|
| Savage Strike (Rank 1) | 15 | 26 | 30 |
| Savage Strike (Rank 3) | 25 | 43 | 50 |
| Pain Fuels (20 dmg) | 3 | 5 | 6 |
| Horrific Form Fear | 5 | 8 | 10 |

---

## Stress Accumulation

| Savagery Generated | Rank 2 Stress | Rank 3 Stress |
|-------------------|---------------|---------------|
| 10 | +1 | +0 |
| 15 | +1 | +1 |
| 30 | +3 | +2 |
| 50 | +5 | +3 |

---

## Combat Example (Full Combat)

**Rank 2 scenario:**
- Round 1: Savage Strike +26 Savagery, +2 Stress
- Round 2: Took 40 damage +7 Savagery, +0 Stress
- Round 3: Savage Strike +26 Savagery, +2 Stress
- Round 4: Fear proc +8 Savagery, +0 Stress
- **Total: +67 Savagery, +4 Stress**

---

## Combat Log Examples

- "The Price of Power: Savage Strike generates 26 Savagery (+75%)"
- "The Price of Power: +2 Psychic Stress (26 Savagery / 10)"
- "The Price of Power (Rank 3): Savagery doubled!"
- "Warning: Approaching Stress threshold"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress mechanics |
