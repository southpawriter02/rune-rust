# Armor System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-ARMOR-v5.0
Parent item: Equipment System — Core System Specification v5.0 (Equipment%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%200ec604d185934907915e1ba9cd3e8800.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Armor is a **stability layer** shielding against physical trauma while potentially constraining agility. Three weight classes with distinct trade-offs between protection and mobility.

---

## Armor Weight Classes

### Light Armor (Cloth, Leather)

- **Soak per Slot:** Head 1, Chest 2, Hands 1, Legs 1, Feet 1
- **Full Set Total:** 6 Soak
- **Penalties:** None
- **Proficiency:** All archetypes

### Medium Armor (Chain, Scale)

- **Soak per Slot:** Head 2, Chest 4, Hands 2, Legs 2, Feet 2
- **Full Set Total:** 12 Soak
- **Penalties:** -1d10 Agility, +2 Stamina for movement
- **Proficiency:** Warrior, Skirmisher

### Heavy Armor (Plate)

- **Soak per Slot:** Head 3, Chest 6, Hands 3, Legs 3, Feet 3
- **Full Set Total:** 18 Soak
- **Penalties:** -2d10 Agility, +5 Stamina for movement, Stealth disadvantage
- **Proficiency:** Warrior only

---

## Soak Mechanic

```
Final Damage = Max(0, Raw Damage - Total Soak)
```

**Design:** Heavy armor excels against many weak attacks, less effective against single massive hits.

---

## Non-Proficiency Penalty

**Effect:** Doubles all penalties

**Example:** Mystic in Medium Armor

- Normal: -1d10 Agility, +2 Stamina
- Non-Proficient: -2d10 Agility, +4 Stamina

---

## Quality Tier Effects

| Quality | Soak Mod | Defense | Special |
| --- | --- | --- | --- |
| Jury-Rigged | -1 | None | -50% Durability |
| Scavenged | Base | None | Standard |
| Clan-Forged | +1 | +1 (Medium+) | +10 max HP |
| Optimized | +2 | +2 (Heavy) | +20 max HP |
| Myth-Forged | +3 | +3 | Unique abilities |

---

## Mixed Armor Sets

**Rule:** Heaviest piece determines penalty tier

- ANY Medium piece = Medium penalties
- ANY Heavy piece = Heavy penalties

---

## Integration Points

**Dependencies:** Equipment System, Attributes (STURDINESS)

**Referenced By:** Combat System, Damage System, Resource Systems