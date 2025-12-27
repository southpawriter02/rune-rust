---
id: ABILITY-RUST-WITCH-25006
title: "Accelerated Entropy"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Accelerated Entropy

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self (affects all [Corroded] effects) |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

Your curses are particularly potent. The corrosion you inflict eats through material faster than normal decay.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- ALL [Corroded] effects you inflict deal 2d6 damage per stack instead of 1d4
- Massive damage increase (avg 7 vs avg 2.5 per stack)

**Formula:**
```
For [Corroded] effects inflicted by Caster:
    DamagePerStack = Roll(2d6)  // Instead of 1d4
```

**Tooltip:** "Accelerated Entropy (Rank 2): Your [Corroded] deals 2d6/stack (base: 1d4/stack)."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- [Corroded] damage remains 2d6 per stack
- Additionally, [Corroded] ignores 1 Soak per stack
- At 5 stacks, your [Corroded] effects ignore 5 Soak completely

**Formula:**
```
For [Corroded] effects inflicted by Caster:
    DamagePerStack = Roll(2d6)
    SoakIgnored = Target.GetStatusStacks("Corroded")
```

**Tooltip:** "Accelerated Entropy (Rank 3): Your [Corroded] deals 2d6/stack and ignores 1 Soak per stack."

---

## Damage Comparison

| Condition | 1 Stack | 5 Stacks |
|-----------|---------|----------|
| Base [Corroded] | 1d4 (avg 2.5) | 5d4 (avg 12.5) |
| With Accelerated Entropy | 2d6 (avg 7) | 10d6 (avg 35) |
| Rank 3 vs 5 Armor | 2d6 - 0 Soak | 10d6 - 0 Soak |

At 5 stacks with Rank 3: ~35 damage/turn ignoring armor completely.

---

## Combat Log Examples

- "Accelerated Entropy: [Corroded] deals 2d6/stack instead of 1d4"
- "[Corroded] Tick (Accelerated): 14 damage (2 stacks × 2d6)"
- "[Corroded] Tick (Rank 3): 35 damage, ignoring 5 Soak"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
| [Corroded Status](../../../../04-systems/status-effects/corroded.md) | Status effect details |
