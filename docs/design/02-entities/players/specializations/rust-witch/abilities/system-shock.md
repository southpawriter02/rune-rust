---
id: ABILITY-RUST-WITCH-25004
title: "System Shock"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# System Shock

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 25 AP |
| **Self-Corruption** | +3 (Rank 2), +2 (Rank 3) |
| **Status Effects** | [Corroded], [Stunned] |
| **Ranks** | 2 → 3 |

---

## Description

You overload the target's systems with corruptive energy. Living tissue seizes, mechanical systems fail, and the corrupted mind locks up.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- WILL-based attack (Success Threshold: 2)
- Apply 2 stacks of [Corroded]
- If target has [Mechanical] tag: apply [Stunned] for 1 turn
- **Self-Corruption:** +3

**Formula:**
```
AttackRoll = Roll(WILL) vs Target.Resolve
SuccessThreshold = 2

If Success:
    Target.AddStatus("Corroded", Stacks: 2)
    If Target.HasTag("Mechanical"):
        Target.AddStatus("Stunned", Duration: 1)
    Caster.Corruption += 3
```

**Tooltip:** "System Shock (Rank 2): Apply 2 [Corroded] stacks. [Stunned] 1 turn vs Mechanical. Cost: 25 AP. Self-Corruption: +3"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Apply 3 stacks instead of 2
- [Stunned] now triggers on ANY target with 3+ total [Corroded] stacks
- **Self-Corruption reduced to +2**

**Formula:**
```
If Success:
    Target.AddStatus("Corroded", Stacks: 3)
    TotalStacks = Target.GetStatusStacks("Corroded")
    If Target.HasTag("Mechanical") OR TotalStacks >= 3:
        Target.AddStatus("Stunned", Duration: 1)
    Caster.Corruption += 2
```

**Tooltip:** "System Shock (Rank 3): Apply 3 [Corroded] stacks. [Stunned] vs Mechanical OR targets with 3+ stacks. Self-Corruption: +2"

---

## Anti-Mechanical Synergy

System Shock is particularly effective against:
- Automatons
- Corrupted Constructs
- Mechanical enemies
- Cyborg-type enemies

These enemies are especially vulnerable to entropy effects.

---

## Combat Log Examples

- "System Shock: 2 [Corroded] stacks applied. Self-Corruption +3"
- "System Shock vs [Mechanical]: [Stunned] for 1 turn!"
- "System Shock (Rank 3): 3 [Corroded] stacks. Target has 5 total - [Stunned]!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
| [Stunned Status](../../../../04-systems/status-effects/stunned.md) | Status effect details |
