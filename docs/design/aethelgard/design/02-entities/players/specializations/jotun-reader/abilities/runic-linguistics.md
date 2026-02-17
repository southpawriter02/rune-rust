---
id: ABILITY-JOTUNREADER-203
title: "Runic Linguistics"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Runic Linguistics

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Attribute** | WITS |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You read the grammar of reality's operating system. You understand error messages in a dead language. Where others see meaningless scrawl, you see system logs and command syntax.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- Can read and translate Elder Futhark inscriptions (non-magical)
- Basic translation of intact text
- Unlocks ~10% of dungeon text content

**Formula:**
```
CanTranslate(inscription) = inscription.Type == "ElderFuthark"
                            AND inscription.Corruption <= 0%
```

**Tooltip:** "Runic Linguistics (Rank 1): Translate intact Elder Futhark inscriptions"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Instantaneous translation (no time cost)
- Can translate 30-40% corrupted/fragmentary text
- Can identify author/origin of inscription
- Unlocks ~20% of dungeon text content

**Formula:**
```
CanTranslate(inscription) = inscription.Type == "ElderFuthark"
                            AND inscription.Corruption <= 40%
TranslationTime = 0  // Instant
AuthorIdentification = true
```

**Tooltip:** "Runic Linguistics (Rank 2): Instant translation. Handles 40% corruption. Identifies author."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Translate ANY corruption level
- Extrapolate 70-80% of missing sections
- Reveal hidden subtext and encoded messages
- Unlocks ~30% of dungeon text content (all Jötun writing)

**Formula:**
```
CanTranslate(inscription) = inscription.Type == "ElderFuthark"
                            // No corruption limit
ExtrapolationAccuracy = 0.75
RevealHiddenSubtext = true
```

**Tooltip:** "Runic Linguistics (Rank 3): Translate any corruption. Extrapolate missing text. Reveal hidden messages."

---

## Exploration Examples

- "You decipher the inscription: 'WARNING: CONTAINMENT BREACH SECTOR 7'"
- "Instant translation: 'EMERGENCY PROTOCOL... [corrupted]... EVACUATE ALL PERSONNEL'"
- "Despite 90% corruption, you extrapolate: 'The weapon cache is behind the... eastern wall...'"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Jötun-Reader Overview](../jotun-reader-overview.md) | Parent specialization |
| [Room Engine: Descriptors](../../../../07-environment/room-engine/descriptors.md) | Exploration integration |
