---
id: ABILITY-VARD-WARDEN-28012
title: "Consecrate Ground"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Consecrate Ground

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Area (3x3 tiles centered on target) |
| **Resource Cost** | 25-30 Aether |
| **Cooldown** | 4 turns |
| **Attribute** | WILL |
| **Tags** | [Zone], [Healing], [Anti-Blighted], [Persistent] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You sanctify the ground itself, inscribing protective runes that persist across turns. The consecrated area becomes a haven for allies and anathema to the corrupted—healing flesh while searing Blight.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Create a 3x3 sanctified zone
- Duration: 3 turns
- Allies in zone: Heal 1d6 HP per turn
- Blighted/Undying in zone: Take 1d6 Arcane damage per turn
- Cost: 25 Aether

**Formula:**
```
Caster.Aether -= 25

Zone = CreateZone("SanctifiedGround")
Zone.Size = 3x3
Zone.Position = TargetTile (center)
Zone.Duration = 3

Zone.OnTurnStart = Function(Entity):
    If Entity.IsAlly:
        HealAmount = Roll(1d6)
        Entity.HP += HealAmount
        Log("[Sanctified Ground] heals {Entity} for {HealAmount}")

    If Entity.HasTag("Blighted") OR Entity.HasTag("Undying"):
        Damage = Roll(1d6)
        Entity.TakeDamage(Damage, "Arcane")
        Log("[Sanctified Ground] burns {Entity} for {Damage}")

Log("Sanctified Ground created (3x3, 3 turns)")
```

**Tooltip:** "Consecrate Ground (Rank 1): 3x3 zone. Allies heal 1d6/turn. Blighted/Undying take 1d6/turn. 3 turns."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Duration: 4 turns
- Allies: Heal 1d6+2 HP per turn
- Blighted/Undying: Take 1d6+2 Arcane damage per turn
- Cost: 27 Aether

**Formula:**
```
Caster.Aether -= 27

Zone = CreateZone("SanctifiedGround")
Zone.Size = 3x3
Zone.Duration = 4

Zone.OnTurnStart = Function(Entity):
    If Entity.IsAlly:
        HealAmount = Roll(1d6) + 2
        Entity.HP += HealAmount
        Log("[Sanctified Ground] heals {Entity} for {HealAmount}")

    If Entity.HasTag("Blighted") OR Entity.HasTag("Undying"):
        Damage = Roll(1d6) + 2
        Entity.TakeDamage(Damage, "Arcane")
        Log("[Sanctified Ground] burns {Entity} for {Damage}")

Log("Sanctified Ground created (3x3, 4 turns)")
```

**Tooltip:** "Consecrate Ground (Rank 2): 3x3 zone. Allies heal 1d6+2/turn. Blighted/Undying take 1d6+2/turn. 4 turns."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Duration: 4 turns
- Allies: Heal 2d6 HP per turn
- Blighted/Undying: Take 2d6 Arcane damage per turn
- **NEW:** Allies in zone gain +1d10 Resolve (Stress resistance)
- Cost: 30 Aether

**Formula:**
```
Caster.Aether -= 30

Zone = CreateZone("SanctifiedGround")
Zone.Size = 3x3
Zone.Duration = 4

Zone.OnTurnStart = Function(Entity):
    If Entity.IsAlly:
        HealAmount = Roll(2d6)
        Entity.HP += HealAmount
        Entity.AddBuff("Resolve", "+1d10", 1)
        Log("[Sanctified Ground] heals {Entity} for {HealAmount}, +1d10 Resolve")

    If Entity.HasTag("Blighted") OR Entity.HasTag("Undying"):
        Damage = Roll(2d6)
        Entity.TakeDamage(Damage, "Arcane")
        Log("[Sanctified Ground] burns {Entity} for {Damage}")

Log("Sanctified Ground created (3x3, 4 turns, +Resolve)")
```

**Tooltip:** "Consecrate Ground (Rank 3): 3x3 zone. Allies heal 2d6/turn + Resolve. Blighted/Undying take 2d6/turn."

---

## Zone Properties

| Property | R1 | R2 | R3 |
|----------|----|----|------|
| Size | 3x3 | 3x3 | 3x3 |
| Duration | 3 turns | 4 turns | 4 turns |
| Ally Healing | 1d6/turn | 1d6+2/turn | 2d6/turn |
| Enemy Damage | 1d6/turn | 1d6+2/turn | 2d6/turn |
| Resolve Bonus | — | — | +1d10 |

---

## Valid Targets for Damage

| Enemy Type | Affected |
|------------|----------|
| Blighted | ✓ |
| Undying | ✓ |
| Corrupted | ✓ (if tagged) |
| Normal enemies | ✗ |
| Mechanical | ✗ |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Defensive position | Heal party while holding ground |
| Choke point | Damage Blighted forcing through |
| Stress management | Rank 3 Resolve for pressure fights |
| Blight encounters | Extra damage vs corrupted enemies |

---

## Zone Interaction Rules

| Interaction | Result |
|-------------|--------|
| Multiple allies | Each heals independently |
| Multiple enemies | Each takes damage independently |
| Zone overlap | Effects stack (multiple zones) |
| Reinforce Ward | Can boost duration/healing |
| Movement through | Instant tick on turn start only |

---

## Combat Log Examples

- "Sanctified Ground created at [position] (3x3, 4 turns)"
- "[Sanctified Ground] heals [Ally] for 8 HP"
- "[Sanctified Ground] burns [Blighted Husk] for 9 Arcane damage"
- "[Ally] gains +1d10 Resolve while in Sanctified Ground"
- "Sanctified Ground fades (duration ended)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Reinforce Ward](reinforce-ward.md) | Zone enhancement |
| [Aegis of Sanctity](aegis-of-sanctity.md) | Zone cleansing upgrade |
