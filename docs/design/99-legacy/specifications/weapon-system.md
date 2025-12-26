# Weapon System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-WEAPON-v5.0
Parent item: Equipment System — Core System Specification v5.0 (Equipment%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%200ec604d185934907915e1ba9cd3e8800.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Weapons are **interfaces between intent and physical world**. Categorized by type (axes, swords, polearms, bows) rather than proficiency tiers. Each type has distinct mechanical properties creating emergent tactical gameplay.

---

## Weapon Categories

### One-Handed Melee

| Type | Damage | Special |
| --- | --- | --- |
| Axes | 2d10 + MIGHT | Can be thrown (Short) |
| Swords | 2d10 + MIGHT | Versatile (+1d10 two-handed) |
| Maces/Hammers | 2d10 + MIGHT | Ignores 2 Soak |
| Daggers | 1d10 + FINESSE | Thrown, stealth bonus |
| Spears | 2d10 + MIGHT | Thrown, Reach |

### Two-Handed Melee

| Type | Damage | Special |
| --- | --- | --- |
| Greataxes | 4d10 + MIGHT | Crits deal +2d10 |
| Greatswords | 4d10 + MIGHT | Can hit 2 adjacent enemies |
| Mauls | 4d10 + MIGHT | Ignores 4 Soak, shatters shields |
| Polearms | 3d10 + MIGHT | [Reach] from Back Row |
| Staves | 2d10 + MIGHT/WILL | Arcane focus, +1d10 block |

### Ranged

| Type | Damage | Special |
| --- | --- | --- |
| Bows | 3d10 + FINESSE | Requires arrows |
| Crossbows | 4d10 + FINESSE | Reload action required |
| Firearms | 5d10 + FINESSE | Loud (alerts enemies) |

---

## Proficiency by Archetype

| Archetype | Proficient | Non-Proficient |
| --- | --- | --- |
| **Warrior** | All weapons | None |
| **Skirmisher** | One-handed, ranged, daggers | Two-handed melee |
| **Mystic** | Daggers, staves, arcane | All other melee/ranged |
| **Adept** | Daggers, staves, maces | All martial/ranged |

**Non-Proficiency Penalty:** Botch range increases from [1] to [1, 2, 3]

---

## Dual-Wielding

**Requirements:** Two one-handed weapons, both proficient

**Mechanics:**

- Main Hand: Full damage (WeaponDice + Attribute)
- Off Hand: No attribute bonus (WeaponDice only)
- Both attacks use single Standard Action

---

## Integration Points

**Dependencies:** Equipment System, Attributes (MIGHT/FINESSE/WILL)

**Referenced By:** Combat System, Damage System, all Specializations