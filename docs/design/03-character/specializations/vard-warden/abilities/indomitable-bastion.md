---
id: ABILITY-VARD-WARDEN-28018
title: "Indomitable Bastion"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Indomitable Bastion

**Type:** Reaction | **Tier:** 4 (Capstone) | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Reaction (Free Action on trigger) |
| **Target** | One ally about to take fatal damage |
| **Trigger** | Ally would be reduced to 0 HP |
| **Resource Cost** | 40 Aether |
| **Cooldown** | Once per Expedition |
| **Tags** | [Reaction], [Life-Save], [Construct] |
| **Ranks** | None (full power when unlocked) |

---

## Description

*"Not while I stand."*

The ultimate expression of the Warden's oath. When death reaches for your allies, you interpose yourself—not physically, but through sheer force of will. In that moment, a barrier of pure Aether manifests between your ally and oblivion, negating what should have been a killing blow.

---

## Mechanical Effect

**Fatal Damage Negation:**
- Trigger: When any ally would be reduced to 0 HP or below
- Effect: Negate ALL damage from the triggering attack
- Bonus: Create a 30 HP barrier adjacent to the saved ally
- Cost: 40 Aether
- Limitation: Once per expedition

**Formula:**
```
OnAllyWouldDie(Ally, IncomingDamage):
    If Caster.Aether >= 40 AND NOT Expedition.Used("IndomitableBastion"):
        // Negate the fatal damage
        IncomingDamage = 0
        Log("INDOMITABLE BASTION! Fatal damage negated on {Ally}!")

        // Spend resources
        Caster.Aether -= 40
        Expedition.MarkUsed("IndomitableBastion")

        // Create protective barrier
        Barrier = CreateEntity("RunicBarrier")
        Barrier.HP = 30
        Barrier.MaxHP = 30
        Barrier.Duration = 3
        Barrier.Position = AdjacentTo(Ally)
        Log("Protective barrier created adjacent to {Ally} (30 HP)")

        Return NEGATE_DAMAGE
```

**Tooltip:** "Indomitable Bastion: REACTION. Negate fatal damage on ally, create 30 HP barrier. Once per expedition. Cost: 40 Aether"

---

## Effect Summary

| Benefit | Value |
|---------|-------|
| Damage Negated | ALL (from triggering attack) |
| Barrier Created | 30 HP, 3 turns |
| Trigger | Ally would die |
| Limitation | Once per expedition |

---

## Reaction Mechanics

**How Reactions Work:**
- Triggers automatically when condition is met
- No action cost (interrupts normal flow)
- Can be declined (Warden chooses whether to activate)
- Happens BEFORE damage is applied

**Trigger Specifics:**
- Any ally (not self)
- Damage that would reduce HP to 0 or below
- Works against any damage type
- Works against any source (attack, spell, trap, etc.)

---

## Once Per Expedition

**What This Means:**
- Cannot be refreshed by resting
- Resets only when starting a new expedition
- Use wisely—one save per adventure

**Strategic Implication:**
- Save for truly critical moments
- Consider party composition (who is most likely to die?)
- Don't panic-use on chip damage that looks scary

---

## Optimal Use Cases

| Scenario | Why Use |
|----------|---------|
| Boss execution attack | Massive one-shot damage |
| Key ally at risk | Healer/damage dealer death spiral |
| Late-expedition crisis | When retreat isn't option |
| Overkill damage | When healing can't keep up |

**Avoid Using When:**
- Ally can be healed back
- Multiple allies at risk (can only save one)
- Early in expedition (save for harder content)

---

## Barrier Bonus

The created barrier:
- Provides immediate cover for saved ally
- 30 HP, 3 turn duration
- Normal barrier properties (blocks movement/LoS)
- Benefits from Aegis of Sanctity reflection

---

## Combat Log Examples

- "INDOMITABLE BASTION! Fatal damage negated on [Berserkr]!"
- "[Dragon Breath] would deal 67 damage → NEGATED"
- "Protective barrier created adjacent to [Berserkr] (30 HP)"
- "Indomitable Bastion expended for this expedition"

---

## Capstone Upgrade Effect

When Indomitable Bastion is trained:
- All Tier 1 abilities upgrade to Rank 3
- All Tier 2 abilities upgrade to Rank 3

**Affected Abilities:**
- Runic Barrier → Rank 3 (50 HP, detonation)
- Consecrate Ground → Rank 3 (2d6 healing, +Resolve)
- Rune of Shielding → Rank 3 (+3 Soak, +2d10 vs Corruption)
- Reinforce Ward → Rank 3 (25 HP heal, +2 zone bonus)

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Runic Barrier](runic-barrier.md) | Barrier created on trigger |
| [Glyph of Sanctuary](glyph-of-sanctuary.md) | Other major defensive ability |
