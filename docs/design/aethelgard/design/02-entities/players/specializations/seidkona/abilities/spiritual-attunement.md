---
id: ABILITY-SEIDKONA-27001
title: "Spiritual Attunement I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Spiritual Attunement I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your mind is attuned to the psychic frequencies of the crashed reality. You perceive what others cannot.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- +1d10 to WILL checks to perceive magical phenomena
- See aura around [Psychic Resonance] zones and Forlorn entities

**Formula:**
```
If CheckType == "PerceiveMagical":
    WILLCheckPool += 1d10
CanSee("PsychicResonanceAura")
CanSee("ForlornEntityAura")
```

**Tooltip:** "Spiritual Attunement I (Rank 1): +1d10 to perceive magical phenomena. See Psychic Resonance and Forlorn auras."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +2d10 to WILL checks
- See auras with enhanced clarity

**Formula:**
```
If CheckType == "PerceiveMagical":
    WILLCheckPool += 2d10
```

**Tooltip:** "Spiritual Attunement I (Rank 2): +2d10 to perceive magical phenomena."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +2d10 to WILL checks
- **NEW:** Sense Forlorn entities through walls (30 ft radius)

**Formula:**
```
If CheckType == "PerceiveMagical":
    WILLCheckPool += 2d10
CanSenseThrough("Walls", EntityType: "Forlorn", Radius: 30)
```

**Tooltip:** "Spiritual Attunement I (Rank 3): +2d10 perception. Sense Forlorn through walls (30 ft)."

---

## Combat Log Examples

- "Spiritual Attunement: +1d10 to perception check"
- "Spiritual Attunement: Forlorn aura detected ahead"
- "Spiritual Attunement (Rank 3): Sensing Forlorn entity through wall!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
