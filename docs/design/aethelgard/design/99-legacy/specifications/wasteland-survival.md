# Wasteland Survival — Skill Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-SKILL-WASTELANDSURVIVAL-v5.0
Parent item: Skills System — Core System Specification v5.0 (Skills%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20f074e9ec58e64ae6865c52dca47e733d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Wasteland Survival resolves: **Can this character navigate, track, and scavenge in the broken technological ruins of Aethelgard?** This is reading metal debris fields, following boot-prints through glitched terrain, and recognizing which salvage is safe to touch.

**Governing Attribute:** WITS

---

## Trigger Events

- **Tracking:** Following tracks through ruins
- **Foraging:** Searching for salvageable resources
- **Navigation:** Finding paths through unfamiliar terrain
- **Hazard Detection:** Identifying environmental dangers
- **Scavenger Sign Reading:** Interpreting territorial markers

---

## DC Tables

### Tracking

| Difficulty | DC | Example |
| --- | --- | --- |
| Obvious Trail | 8 | Caravan through ash |
| Fresh Trail | 12 | Tracks <1 hour old |
| Standard Trail | 16 | Tracks 2-8 hours old |
| Old Trail | 20 | Tracks 12-24 hours old |
| Obscured Trail | 24 | Deliberately hidden |

### Foraging

| Target | DC | Yield |
| --- | --- | --- |
| Common Salvage | 10 | 2d10 scrap units |
| Useful Supplies | 14 | 1d6 rations/1d4 water |
| Valuable Components | 18 | 1d4 rare components |
| Hidden Cache | 22 | 1d100 Marks + items |

### Navigation

| Terrain | DC | Example |
| --- | --- | --- |
| Open Wasteland | 8 | Flat ash plains |
| Moderate Ruins | 12 | Partially collapsed |
| Dense Ruins | 16 | Heavy rubble |
| Labyrinthine | 20 | Multi-level centers |
| [Glitched] Labyrinth | 24 | Non-Euclidean geometry |

---

## Dice Pool Calculation

```
Pool = WITS + Rank + Equipment Mod + Environmental
```

**Environmental Modifiers:**

- Good visibility: +1d10
- Fresh trail: +1d10
- Old trail: -2d10
- Static Storm: -4d10

---

## Master Rank Benefits (Rank 5)

- **Wasteland Cartographer:** Auto-succeed navigation DC ≤ 10
- **Apex Scavenger:** Never come up empty, +50% yields
- **Master Tracker:** Follow trails up to 48 hours old
- **Hazard Sense:** Auto-detect DC ≤ 16 hazards

---

## Integration Points

**Dependencies:** Attributes (WITS), Dice Pool System, Environmental Systems

**Referenced By:** Exploration System, Crafting System, Veiðimaðr Specialization