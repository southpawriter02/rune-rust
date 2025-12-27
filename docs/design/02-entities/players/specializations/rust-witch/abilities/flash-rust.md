---
id: ABILITY-RUST-WITCH-25005
title: "Flash Rust"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Flash Rust

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | ALL Enemies |
| **Resource Cost** | 35 AP |
| **Self-Corruption** | +4 (Rank 2), +3 (Rank 3) |
| **Ranks** | 2 → 3 |

---

## Description

You release an instantaneous entropy cascade, causing everything metal or organic in the area to begin decaying simultaneously.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- No attack roll required (automatic hit)
- Apply 2 stacks of [Corroded] to ALL enemies in combat
- **Self-Corruption:** +4

**Formula:**
```
For each Enemy in Combat:
    Enemy.AddStatus("Corroded", Stacks: 2)
Caster.Corruption += 4
```

**Tooltip:** "Flash Rust (Rank 2): Apply 2 [Corroded] stacks to ALL enemies. Cost: 35 AP. Self-Corruption: +4"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Apply 2 stacks to all enemies
- [Mechanical] enemies receive +1 additional stack (3 total)
- **Self-Corruption reduced to +3**

**Formula:**
```
For each Enemy in Combat:
    BaseStacks = 2
    If Enemy.HasTag("Mechanical"):
        BaseStacks = 3
    Enemy.AddStatus("Corroded", Stacks: BaseStacks)
Caster.Corruption += 3
```

**Tooltip:** "Flash Rust (Rank 3): 2 [Corroded] stacks to all (3 vs Mechanical). Self-Corruption: +3"

---

## AoE Efficiency Analysis

| Enemy Count | Total Stacks Applied |
|-------------|---------------------|
| 3 enemies | 6 stacks |
| 5 enemies | 10 stacks |
| 8 enemies | 16 stacks |

Value increases dramatically with enemy count. Combined with Cascade Reaction, creates chain-reaction potential.

---

## Combat Log Examples

- "Flash Rust: 2 [Corroded] stacks applied to all 5 enemies. Self-Corruption +4"
- "Flash Rust (Rank 3): 3 [Corroded] stacks to [Mechanical], 2 to others. Self-Corruption +3"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
| [Cascade Reaction](cascade-reaction.md) | Synergy ability |
