---
id: ABILITY-SKJALDMAER-26026
title: "Aegis of the Clan"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Aegis of the Clan

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free (Automatic) |
| **Target** | Allies in Crisis |
| **Resource Cost** | None |
| **Trigger** | Ally's Psychic Stress reaches 66%+ |
| **Limit** | Once per ally per combat |
| **Ranks** | None (full power when unlocked) |

---

## Description

Automatic protection triggers when ally enters mental crisis. The Skjaldmær's bond with her clanmates runs deep—she senses their distress and responds instinctively.

---

## Mechanical Effect

- **Trigger:** When any ally's Psychic Stress reaches or exceeds 66% of maximum (66/100)
- **Effect:** Automatically apply Oath of the Protector to that ally for 2 turns
- **Bonus:** Immediately reduce that ally's Psychic Stress by 10
- **Limit:** Can only trigger once per ally per combat

**Formula:**
```
Trigger: Ally.PsychicStress >= 66 AND NOT AegisUsedOn(Ally)

If Triggered:
    ApplyOathOfProtector(Ally, Duration: 2)  // +3 Soak, +2 Stress dice
    Ally.PsychicStress -= 10
    MarkAegisUsedOn(Ally)
```

**Tooltip:** "Aegis of the Clan: When ally reaches 66% Stress, auto-apply Oath of the Protector and reduce Stress by 10. Once per ally per combat."

---

## Combat Log Examples

- "AEGIS OF THE CLAN: [Ally] enters mental crisis (66% Stress)!"
- "Aegis of the Clan automatically shields [Ally] (+3 Soak, +2 Stress resistance for 2 turns)"
- "[Ally]'s Psychic Stress reduced by 10 (Aegis of the Clan)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
| [Oath of the Protector](oath-of-the-protector.md) | Applied buff |
| [Stress](../../../../01-core/resources/stress.md) | Trigger threshold |
