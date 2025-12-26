---
id: ABILITY-RUST-WITCH-25007
title: "Unmaking Word"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Unmaking Word

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy with [Corroded] |
| **Resource Cost** | 30 AP |
| **Self-Corruption** | +4 |
| **Requirement** | Target must have [Corroded] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You speak a word that should not be spoken—a syllable of pure dissolution that accelerates entropy to catastrophic levels.

---

## Mechanical Effect

**Stack Multiplication:**
- Target must already have at least 1 stack of [Corroded]
- DOUBLE the current [Corroded] stacks
- Maximum 5 stacks (hard cap)
- **Self-Corruption:** +4

**Formula:**
```
Requires: Target.HasStatus("Corroded")

CurrentStacks = Target.GetStatusStacks("Corroded")
NewStacks = Min(CurrentStacks * 2, 5)
Target.SetStatusStacks("Corroded", NewStacks)

Caster.Corruption += 4
```

**Tooltip:** "Unmaking Word: DOUBLE target's [Corroded] stacks (max 5). Requires [Corroded] target. Cost: 30 AP. Self-Corruption: +4"

---

## Strategic Timing

| Starting Stacks | After Unmaking Word |
|-----------------|---------------------|
| 1 stack | 2 stacks |
| 2 stacks | 4 stacks |
| 3+ stacks | 5 stacks (cap) |

**Optimal Usage:**
- Best when target has 2 stacks → doubles to 4
- At 3+ stacks, doubling caps at 5 (wastes potential)
- Use BEFORE reaching 3 stacks for maximum value

**Recommended Combo:**
1. Corrosive Curse (2 stacks)
2. Unmaking Word (doubles to 4 stacks)
3. System Shock (adds 3, caps at 5)

---

## Combat Log Examples

- "Unmaking Word: [Corroded] stacks doubled! (2 → 4). Self-Corruption +4"
- "Unmaking Word: [Corroded] stacks doubled! (3 → 5, capped). Self-Corruption +4"
- "Unmaking Word FAILED: Target has no [Corroded] stacks"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
| [Corrosive Curse](corrosive-curse.md) | Stack application |
