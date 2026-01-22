---
id: ABILITY-GORGE-MAW-26017
title: "Inner Stillness"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Inner Stillness

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self + Adjacent Allies |
| **Resource Cost** | None |
| **Tags** | [Mental], [Immunity], [Aura] |
| **Ranks** | None (full power when unlocked) |

---

## Description

Your meditative discipline has reached its apex. Your mind is an absolute fortress—no fear, confusion, or charm can find purchase. And in your presence, allies find their own minds steadied.

---

## Mechanical Effect

**Self - Complete Mental Immunity:**
- IMMUNE to [Feared]
- IMMUNE to [Disoriented]
- IMMUNE to [Charmed]

**Aura - Adjacent Ally Protection:**
- Adjacent allies (within 1 tile) gain +2 dice vs [Feared]
- Adjacent allies gain +2 dice vs [Disoriented]
- Adjacent allies gain +2 dice vs [Charmed]

**Formula:**
```
// Self immunity
Self.AddImmunity("Feared")
Self.AddImmunity("Disoriented")
Self.AddImmunity("Charmed")

// Aura for adjacent allies
OnAllyWithinRange(1):
    Ally.ResolveBonus_Feared += 2
    Ally.ResolveBonus_Disoriented += 2
    Ally.ResolveBonus_Charmed += 2

OnAllyLeavesRange(1):
    Ally.ResolveBonus_Feared -= 2
    Ally.ResolveBonus_Disoriented -= 2
    Ally.ResolveBonus_Charmed -= 2
```

**Tooltip:** "Inner Stillness: Immune to Fear/Disoriented/Charmed. Adjacent allies +2 dice vs all three."

---

## Aura Range

```
    [ ][ ][ ]
    [ ][A][ ]   A = Ascetic
    [ ][ ][ ]   All 8 adjacent tiles receive aura
```

Allies in any of the 8 surrounding tiles receive the +2 dice bonus.

---

## Mental Status Effects Protected Against

| Status | Effect | Self | Allies |
|--------|--------|------|--------|
| [Feared] | Flee, cannot approach source | Immune | +2 dice |
| [Disoriented] | -2 Accuracy, cannot use complex abilities | Immune | +2 dice |
| [Charmed] | Cannot attack source, may aid them | Immune | +2 dice |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Fear-heavy enemies | Complete immunity |
| Echo-Caller fights | Counter psychic assault |
| Ally protection | Position adjacent to vulnerable casters |
| Boss encounters | Reliable against mental attacks |

---

## Positioning Considerations

**Optimal Formation:**
```
[Caster][Ascetic][Fighter]
         ↑
    Caster receives aura protection
```

Stay adjacent to mentally vulnerable party members to extend your protection.

---

## Combat Log Examples

- "Inner Stillness: [Ascetic] is IMMUNE to Fear!"
- "[Adjacent Ally] gains +2 dice vs Charmed (Inner Stillness aura)"
- "[Fear Cascade] - [Ascetic] immune (Inner Stillness)"
- "[Ally] resists Fear with Inner Stillness aura bonus"

---

## GUI Display

- Passive icon: Tranquil mind with protective aura
- Aura indicator on adjacent allies (subtle golden glow)
- Immunity icon displayed when mental effects target you

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Sensory Discipline](sensory-discipline.md) | Precursor mental resistance |
| [Feared Status](../../../../04-systems/status-effects/feared.md) | Status effect details |
