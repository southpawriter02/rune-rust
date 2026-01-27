---
id: ABILITY-RUST-WITCH-25009
title: "Entropic Cascade"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Entropic Cascade

**Type:** Active (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 50 AP |
| **Self-Corruption** | +6 |
| **Cooldown** | Once per combat |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Ranks** | None (full power when unlocked) |

---

## Description

You channel the full force of entropy through your target. If their body or systems are sufficiently compromised, they simply... stop. Otherwise, the wave of dissolution deals catastrophic damage.

---

## Mechanical Effect

**Execution Condition Check:**
1. Check if target has Corruption > 50
2. OR check if target has 5 stacks of [Corroded]
3. If EITHER is true: **EXECUTE** (reduce to 0 HP instantly)
4. If NEITHER is true: Deal 6d6 Arcane damage
5. Regardless of outcome: +6 Self-Corruption

**Formula:**
```
ExecutionCheck:
    Condition1 = Target.Corruption > 50
    Condition2 = Target.GetStatusStacks("Corroded") >= 5

If Condition1 OR Condition2:
    Target.HP = 0  // EXECUTE
    Log("Entropic Cascade: EXECUTE!")
Else:
    Damage = Roll(6d6, "Arcane")
    Target.TakeDamage(Damage)
    Log("Entropic Cascade: {Damage} Arcane damage")

Caster.Corruption += 6
```

**Tooltip:** "Entropic Cascade: EXECUTE if target has >50 Corruption OR 5 [Corroded] stacks. Otherwise: 6d6 Arcane. Cost: 50 AP. Self-Corruption: +6. Once per combat."

---

## Execution Setup

**Reliable [Corroded] Method:**
1. Flash Rust: 2 stacks to all enemies
2. Target single enemy with System Shock: +3 stacks = 5 total
3. Entropic Cascade: EXECUTE (target has 5 stacks)

**Corruption-Based Method:**
- Rust-Witch abilities don't directly inflict target Corruption
- Must rely on other sources or naturally high-Corruption enemies
- [Corroded] stack method is more reliable

**Damage Comparison:**
| Outcome | Value |
|---------|-------|
| 6d6 Arcane (non-execute) | avg 21 damage |
| Execute | Bypasses 50-200+ HP |

---

## Combat Log Examples

- "Entropic Cascade: Execution threshold CHECK..."
- "Entropic Cascade: TARGET HAS 5 [CORRODED] STACKS - EXECUTE!"
- "Entropic Cascade: Target Corruption at 67% - EXECUTE!"
- "Entropic Cascade: Threshold not met. 24 Arcane damage."
- "Self-Corruption +6 (Entropic Cascade)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
| [Corrosive Curse](corrosive-curse.md) | Stack application |
| [Flash Rust](flash-rust.md) | AoE stack setup |
