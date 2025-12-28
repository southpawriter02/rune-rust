---
id: ABILITY-RUST-WITCH-25002
title: "Corrosive Curse"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Corrosive Curse

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 20 AP |
| **Self-Corruption** | +2 (Rank 1-2), +1 (Rank 3) |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You speak words of dissolution, cursing your target with accelerated decay. Metal rusts, flesh rots, and systems fail.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- WILL-based attack vs target's Resolve (Success Threshold: 2)
- Apply 1 stack of [Corroded] on hit
- [Corroded] is permanent until cleansed
- **Self-Corruption:** +2

**Formula:**
```
AttackRoll = Roll(WILL) vs Target.Resolve
SuccessThreshold = 2

If Success:
    Target.AddStatus("Corroded", Stacks: 1, Duration: PERMANENT)
    Caster.Corruption += 2
```

**Tooltip:** "Corrosive Curse (Rank 1): Apply 1 [Corroded] stack. Cost: 20 AP. Self-Corruption: +2"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Apply 2 stacks instead of 1
- Same self-Corruption cost (+2)

**Formula:**
```
If Success:
    Target.AddStatus("Corroded", Stacks: 2, Duration: PERMANENT)
    Caster.Corruption += 2
```

**Tooltip:** "Corrosive Curse (Rank 2): Apply 2 [Corroded] stacks. Cost: 20 AP. Self-Corruption: +2"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Apply 2 stacks
- **Self-Corruption reduced to +1**

**Formula:**
```
If Success:
    Target.AddStatus("Corroded", Stacks: 2, Duration: PERMANENT)
    Caster.Corruption += 1  // Reduced
```

**Tooltip:** "Corrosive Curse (Rank 3): Apply 2 [Corroded] stacks. Cost: 20 AP. Self-Corruption: +1"

---

## [Corroded] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | Permanent (requires cleanse) |
| **Max Stacks** | 5 |
| **Damage** | 1d4/stack/turn (2d6 with Accelerated Entropy) |
| **Armor Penalty** | -1 per stack |
| **Timing** | End of turn |

---

## Combat Log Examples

- "Corrosive Curse hits! [Corroded] applied (1 stack). Self-Corruption +2"
- "Corrosive Curse (Rank 2): 2 [Corroded] stacks applied. Self-Corruption +2"
- "Corrosive Curse (Rank 3): 2 [Corroded] stacks applied. Self-Corruption +1"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
| [Corroded Status](../../../../04-systems/status-effects/corroded.md) | Status effect details |
