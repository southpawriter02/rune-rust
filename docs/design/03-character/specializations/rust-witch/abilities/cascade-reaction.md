---
id: ABILITY-RUST-WITCH-25008
title: "Cascade Reaction"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Cascade Reaction

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (triggered) |
| **Target** | Self (affects enemies on death) |
| **Trigger** | Enemy with [Corroded] dies |
| **Resource Cost** | None |
| **Ranks** | None (full power when unlocked) |

---

## Description

Entropy is contagious. When a corroded target finally succumbs, the decay spreads to those nearby like a virulent plague.

---

## Mechanical Effect

**On Trigger:**
- When ANY enemy with [Corroded] dies (regardless of cause)
- All adjacent enemies receive 1 stack of [Corroded]
- "Adjacent" = same row + enemies in adjacent rows
- Can trigger chain reactions if spread causes another death
- Does NOT inflict self-Corruption (passive trigger)

**Formula:**
```
OnEnemyDeath:
    If DeadEnemy.HasStatus("Corroded"):
        For each Enemy in AdjacentTo(DeadEnemy):
            Enemy.AddStatus("Corroded", Stacks: 1)
            // Check for chain reaction deaths at end of processing
```

**Tooltip:** "Cascade Reaction: When a [Corroded] enemy dies, spread 1 [Corroded] stack to all adjacent enemies."

---

## Chain Reaction Potential

**Example Scenario:**
1. Enemy A (5 stacks) dies
2. Enemies B, C, D (adjacent) each gain 1 stack
3. Enemy B (was at 4 stacks) now at 5 stacks
4. End of turn: Enemy B dies from [Corroded] damage
5. Enemies C, D, E (adjacent to B) each gain 1 stack
6. Continue until no more deaths

This transforms Flash Rust into a cascade bomb against clustered enemies.

---

## Synergy with Flash Rust

| Scenario | Effect |
|----------|--------|
| Flash Rust on 5 enemies | All start with 2 stacks |
| First enemy dies | Adjacent gain +1 (now 3 stacks) |
| Second enemy dies | More spreading (4+ stacks) |
| Cascade chain | Potential full wipe |

---

## Combat Log Examples

- "Cascade Reaction: [Enemy A] death spreads [Corroded] to 3 adjacent enemies"
- "Cascade Reaction CHAIN: [Enemy B] also dies from [Corroded]!"
- "Cascade Reaction: [Corroded] spreads to [Enemy C], [Enemy D]"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
| [Flash Rust](flash-rust.md) | AoE setup ability |
