---
id: ABILITY-GORGE-MAW-26013
title: "Sensory Discipline"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Sensory Discipline

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Attribute** | WILL |
| **Tags** | [Mental], [Resistance] |
| **Ranks** | 2 → 3 |

---

## Description

Your profound mental stillness grants exceptional resistance to effects that assault the mind. Fear and disorientation cannot take root in a consciousness anchored to the earth's eternal vibrations.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- +2 dice to resist [Feared] effects
- +2 dice to resist [Disoriented] effects

**Formula:**
```
OnStatusResist("Feared"):
    BonusDice += 2

OnStatusResist("Disoriented"):
    BonusDice += 2
```

**Tooltip:** "Sensory Discipline (Rank 2): +2 dice vs Fear and Disoriented effects."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- +4 dice to resist [Feared] effects
- +4 dice to resist [Disoriented] effects
- **NEW:** Immune to [Feared] from ambient sources (non-enemy Fear)

**Formula:**
```
OnStatusResist("Feared"):
    BonusDice += 4

OnStatusResist("Disoriented"):
    BonusDice += 4

OnFearSource:
    If Source.IsAmbient:  // Environmental, not from enemy ability
        Immune = true
        Log("Sensory Discipline: Immune to ambient Fear")
```

**Tooltip:** "Sensory Discipline (Rank 3): +4 dice vs Fear/Disoriented. Immune to ambient Fear."

---

## Ambient Fear Sources

| Source Type | Example | Rank 3 Effect |
|-------------|---------|---------------|
| Environmental | Haunted rooms | Immune |
| Psychic corruption | Blight zones | Immune |
| Enemy ability | Echo-Caller Fear | +4 dice to resist |
| Creature aura | Dragon presence | +4 dice to resist |

---

## Synergy with Inner Stillness

At Tier 3, Inner Stillness provides complete [Feared] immunity. Sensory Discipline serves as the stepping stone:

| Ability | Fear Resistance |
|---------|-----------------|
| Sensory Discipline (R2) | +2 dice |
| Sensory Discipline (R3) | +4 dice + ambient immune |
| Inner Stillness | Complete immunity |

---

## Combat Log Examples

- "Sensory Discipline: +2 dice to Fear resistance check"
- "Sensory Discipline (Rank 3): Ignoring ambient Fear aura"
- "WILL + 4 bonus vs [Disoriented] (Sensory Discipline)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Inner Stillness](inner-stillness.md) | Full mental immunity |
| [Feared Status](../../../../04-systems/status-effects/feared.md) | Status effect details |
