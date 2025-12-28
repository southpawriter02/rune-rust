---
id: ABILITY-ECHO-CALLER-28015
title: "Terror Feedback"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Terror Feedback

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (triggered) |
| **Target** | Self |
| **Trigger** | When you apply [Feared] |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

The terror you inflict resonates back to you as power. Fear becomes fuel.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Whenever you apply [Feared] to any enemy, restore 15 Aether

**Formula:**
```
OnFearApplied:
    Caster.Aether += 15
```

**Tooltip:** "Terror Feedback (Rank 2): +15 Aether when you apply [Feared]."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Restore 20 Aether when applying [Feared]
- **NEW:** Gain [Empowered] for 1 turn (+2 dice to damage rolls)

**Formula:**
```
OnFearApplied:
    Caster.Aether += 20
    Caster.AddStatus("Empowered", Duration: 1, BonusDice: 2)
```

**Tooltip:** "Terror Feedback (Rank 3): +20 Aether + [Empowered] 1 turn when applying Fear."

---

## Aether Economy Impact

| Ability | Cost | Fear Applied | Net with Terror Feedback |
|---------|------|--------------|-------------------------|
| Phantom Menace | 30 | Yes | 10-15 net cost |
| Fear Cascade | 45 | Yes (AoE) | -15 to +35 net (per enemy Feared) |
| Silence Made Weapon | 60 | Yes (AoE) | Variable based on targets |

---

## [Empowered] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 1 turn |
| **Effect** | +2 dice to all damage rolls |
| **Stacks** | No |

---

## Combat Log Examples

- "Terror Feedback: +15 Aether (Fear applied to [Enemy])"
- "Terror Feedback (Rank 3): +20 Aether + [Empowered]!"
- "Fear Cascade: 3 enemies Feared! Terror Feedback: +60 Aether total"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
| [Phantom Menace](phantom-menace.md) | Fear source |
| [Fear Cascade](fear-cascade.md) | AoE Fear source |
