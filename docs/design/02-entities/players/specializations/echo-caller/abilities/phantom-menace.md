---
id: ABILITY-ECHO-CALLER-28012
title: "Phantom Menace"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Phantom Menace

**Type:** Active [Echo] | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 30 Aether |
| **Status Effect** | [Feared] |
| **Tags** | [Echo] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You implant phantom terrors in your target's mind—shapes that aren't there, sounds that can't exist. The fear is real even if the threat is not.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- Apply [Feared] to single target for 2 turns
- [Feared]: Cannot attack, -2 dice to all checks, flee if possible

**Formula:**
```
Target.AddStatus("Feared", Duration: 2)
// Feared effects handled by status system
```

**Tooltip:** "Phantom Menace (Rank 1): Apply [Feared] 2 turns. Cost: 30 Aether"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- [Feared] duration: 3 turns

**Formula:**
```
Target.AddStatus("Feared", Duration: 3)
```

**Tooltip:** "Phantom Menace (Rank 2): [Feared] 3 turns. Cost: 30 Aether"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- [Feared] duration: 3 turns
- **[Echo Chain]:** 50% chance to spread [Feared] (2 turns) to adjacent enemy

**Formula:**
```
Target.AddStatus("Feared", Duration: 3)

// Echo Chain
If AdjacentEnemy exists AND Random() < 0.50:
    AdjacentEnemy.AddStatus("Feared", Duration: 2)
    Log("Echo Chain: Fear spreads!")
```

**Tooltip:** "Phantom Menace (Rank 3): [Feared] 3 turns. 50% Echo Chain spreads Fear (2 turns)."

---

## [Feared] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 2-3 turns |
| **Effects** | Cannot attack |
| | -2 dice to all checks |
| | Must flee if possible (move away from fear source) |
| | Echo-Caller abilities deal bonus damage vs Feared |

---

## Synergy with Terror Feedback

When Phantom Menace applies [Feared]:
- Terror Feedback (Rank 2): +15 Aether restored
- Terror Feedback (Rank 3): +20 Aether + [Empowered] 1 turn

---

## Combat Log Examples

- "Phantom Menace: [Enemy] is [Feared] for 2 turns!"
- "[Enemy] flees in terror! (Feared)"
- "Echo Chain: [Feared] spreads to [Adjacent Enemy] for 2 turns!"
- "Terror Feedback: +15 Aether (Fear applied)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
| [Terror Feedback](terror-feedback.md) | Synergy ability |
| [Feared Status](../../../../04-systems/status-effects/feared.md) | Status effect details |
