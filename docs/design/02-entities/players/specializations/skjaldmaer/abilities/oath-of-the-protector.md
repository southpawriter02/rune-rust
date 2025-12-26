---
id: ABILITY-SKJALDMAER-26021
title: "Oath of the Protector"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Oath of the Protector

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Ally |
| **Resource Cost** | 35 Stamina |
| **Buff Applied** | [Oath of the Protector] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Extend protective aura to single ally, shielding flesh and mind. The Skjaldmær speaks words of ancient power, binding her fate to her ward's survival.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- Target ally gains +2 Soak for 2 turns
- Target ally gains +1 bonus die to Psychic Stress resistance for 2 turns

**Formula:**
```
Target.Soak += 2 (for 2 turns)
Target.StressResistanceDice += 1 (for 2 turns)
```

**Tooltip:** "Oath of the Protector (Rank 1): Grant ally +2 Soak, +1 Stress resistance die for 2 turns. Cost: 35 Stamina"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Target ally gains +3 Soak for 2 turns
- Target ally gains +2 bonus dice to Psychic Stress resistance for 2 turns

**Formula:**
```
Target.Soak += 3 (for 2 turns)
Target.StressResistanceDice += 2 (for 2 turns)
```

**Tooltip:** "Oath of the Protector (Rank 2): Grant ally +3 Soak, +2 Stress resistance dice for 2 turns. Cost: 35 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Target ally gains +4 Soak for 3 turns
- Target ally gains +2 bonus dice to Psychic Stress resistance for 3 turns
- **NEW:** Immediately cleanse 1 mental debuff from target (priority: [Fear] > [Disoriented] > [Charmed])

**Formula:**
```
Target.Soak += 4 (for 3 turns)
Target.StressResistanceDice += 2 (for 3 turns)
Target.RemoveFirstMentalDebuff()  // [Fear], [Disoriented], or [Charmed]
```

**Tooltip:** "Oath of the Protector (Rank 3): Grant ally +4 Soak, +2 Stress resistance dice for 3 turns. Cleanses 1 mental debuff. Cost: 35 Stamina"

---

## Combat Log Examples

- "Oath of the Protector shields [Ally Name] (+2 Soak, +1 Stress resistance for 2 turns)"
- "Oath of the Protector (Rank 3) shields [Ally Name] (+4 Soak, +2 Stress resistance for 3 turns)"
- "Oath of the Protector (Rank 3) cleanses [Fear] from [Ally Name]!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
| [Aegis of the Clan](aegis-of-the-clan.md) | Auto-applies this buff |
