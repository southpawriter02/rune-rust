---
id: ABILITY-BLOT-PRIEST-30016
title: "Hemorrhaging Curse"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Hemorrhaging Curse

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (30 ft range) |
| **Resource Cost** | 60 AP (or 120 HP via Sacrificial Casting) |
| **Cooldown** | 4 turns |
| **Damage Type** | Arcane + Physical ([Bleeding]) |
| **Attribute** | WILL |
| **Tags** | [Curse], [DoT], [Anti-Heal], [Lifesteal] |
| **Ranks** | None (full power when unlocked) |

---

## Description

*"This curse doesn't just drain life; it corrupts it, causing the victim's blood to actively rebel and their wounds to resist the logic of healing."*

The ultimate DoT curse. Where Exsanguinate simply drains, Hemorrhaging Curse also inflicts [Bleeding] and prevents the target from being healed—your anti-healer, anti-sustain nightmare fuel.

---

## Mechanical Effect

**Curse Properties:**
- Duration: 3 turns
- Arcane Damage: 3d6 per turn
- [Bleeding] Status: 1d6 Physical per turn
- Anti-Heal Debuff: Reduce incoming healing by 50%
- Lifesteal: 25% of total damage (Arcane + Bleeding)
- Cost: 60 AP (or 120 HP)

**Formula:**
```
Caster.AP -= 60  // OR Caster.HP -= 120 via Sanguine Pact

Target.ApplyCurse("HemorrhagingCurse", {
    Duration: 3,
    ArcaneDamage: "3d6",
    BleedingDamage: "1d6",
    HealingReduction: 0.50,
    LifestealPercent: 0.25,
    Caster: Caster
})

Target.ApplyStatus("[Bleeding]", 3)

Log("Hemorrhaging Curse: {Target} cursed for 3 turns")
Log("{Target} cannot receive full healing!")
```

**On Each Turn:**
```
OnTurnStart(Target):
    If Target.HasCurse("HemorrhagingCurse"):
        ArcaneDmg = Roll(3d6)
        BleedDmg = Roll(1d6)
        TotalDmg = ArcaneDmg + BleedDmg

        Target.TakeDamage(ArcaneDmg, "Arcane")
        Target.TakeDamage(BleedDmg, "Physical")

        HealAmount = Floor(TotalDmg × 0.25)
        Curse.Caster.HP += HealAmount

        Log("Hemorrhaging Curse: {ArcaneDmg} Arcane + {BleedDmg} Bleeding")
        Log("Life drain: +{HealAmount} HP")
```

**Tooltip:** "Hemorrhaging Curse: 3d6 Arcane + 1d6 Bleeding/turn. -50% healing. 25% lifesteal. 3 turns."

---

## Rank 2 Enhancement (via Capstone)

**Upgraded Effects:**
- Arcane Damage: 4d6 per turn
- [Bleeding]: 2d6 per turn
- Healing Reduction: 75%
- Lifesteal: 25%

---

## Rank 3 Enhancement (via Capstone)

**Mastered Effects:**
- Arcane Damage: 5d6 per turn
- [Bleeding]: 3d6 per turn
- **Healing Reduction: 100%** (target CANNOT be healed)
- Lifesteal: 25%

---

## Damage Output

### Base Version

| Turn | Arcane | Bleeding | Total | Lifesteal |
|------|--------|----------|-------|-----------|
| 1 | 10.5 | 3.5 | 14 | 3.5 |
| 2 | 10.5 | 3.5 | 14 | 3.5 |
| 3 | 10.5 | 3.5 | 14 | 3.5 |
| **Total** | **31.5** | **10.5** | **42** | **10.5** |

### Rank 3 Version

| Turn | Arcane | Bleeding | Total | Lifesteal |
|------|--------|----------|-------|-----------|
| 1 | 17.5 | 10.5 | 28 | 7 |
| 2 | 17.5 | 10.5 | 28 | 7 |
| 3 | 17.5 | 10.5 | 28 | 7 |
| **Total** | **52.5** | **31.5** | **84** | **21** |

---

## Anti-Healing Mechanic

| Rank | Healing Reduction |
|------|-------------------|
| Base | 50% |
| R2 | 75% |
| R3 | 100% (complete block) |

**Example (Base):**
```
Enemy has Hemorrhaging Curse (-50% healing)
Healer casts 30 HP heal on enemy
Enemy receives: 30 × 0.50 = 15 HP

With R3 (-100% healing):
Enemy receives: 0 HP (heal completely blocked)
```

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Enemy healer | Block their self-healing |
| Regenerating boss | Negate regen mechanics |
| Sustain enemy | Prevent lifesteal/healing |
| Focus target | Ensure damage sticks |

---

## Synergy with Crimson Vigor

When [Bloodied], lifesteal is enhanced:

| State | Base Lifesteal | With R3 Crimson Vigor |
|-------|----------------|----------------------|
| Healthy | 25% | 25% |
| Bloodied | 25% | 40% (25 × 1.6) |

**Bloodied + R3 Hemorrhaging:**
- 84 total damage over 3 turns
- 33.6 total healing (40% lifesteal)

---

## Corruption Note

Unlike Exsanguinate, Hemorrhaging Curse does **not** generate Corruption per tick—only on cast if using Sacrificial Casting.

| Cast Method | Corruption |
|-------------|------------|
| AP cast | 0 |
| HP cast (Sanguine Pact R1) | +1 |
| HP cast (Sanguine Pact R3) | +0.5 |

This makes it "safer" from a Corruption standpoint than Exsanguinate.

---

## Stacking with Exsanguinate

Both curses can be active on the same target:

```
Target has both Hemorrhaging Curse and Exsanguinate:

Turn Start:
  → Hemorrhaging: 3d6 Arcane + 1d6 Bleeding (14 avg)
  → Exsanguinate: 4d6 Arcane (14 avg)
  → Total: 28 damage per turn
  → Lifesteal: ~9 HP
  → Healing blocked: 50-100%
```

---

## Combat Log Examples

- "Hemorrhaging Curse: [Boss] cursed for 3 turns"
- "[Boss] is [Bleeding]!"
- "[Boss] cannot receive full healing (-50%)"
- "Hemorrhaging tick: 12 Arcane + 4 Bleeding = 16 damage"
- "Life drain: +4 HP"
- "[Healer] attempts to heal [Boss] for 30 → 0 (healing blocked)"
- "Hemorrhaging Curse expires on [Boss]"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Exsanguinate](exsanguinate.md) | Tier 2 DoT curse |
| [Crimson Vigor](crimson-vigor.md) | [Bloodied] lifesteal bonus |
| [Bleeding Status](../../../../04-systems/status-effects/bleeding.md) | Bleeding mechanics |
