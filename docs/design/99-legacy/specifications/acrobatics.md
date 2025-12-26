# Acrobatics — Skill Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-SKILL-ACROBATICS-v5.0
Parent item: Skills System — Core System Specification v5.0 (Skills%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20f074e9ec58e64ae6865c52dca47e733d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Acrobatics resolves: **Can this character safely traverse the vertical, hazardous geometry of Aethelgard's broken infrastructure?** This is climbing rusted girders, leaping across crumbling platforms, and moving silently over debris that creaks with every step.

**Governing Attribute:** FINESSE

---

## Trigger Events

- **Climbing:** Scaling vertical surfaces
- **Leaping:** Jumping across gaps
- **Balancing:** Traversing narrow/unstable surfaces
- **Stealth Movement:** Moving silently through noisy environments
- **Crash Landing:** Reducing fall damage

---

## DC Tables

### Climbing

| Difficulty | DC | Example |
| --- | --- | --- |
| Easy | 8 | Intact ladder |
| Moderate | 12 | Brick wall, rubble pile |
| Challenging | 16 | Rusted girder |
| Difficult | 20 | Crumbling server tower |
| Extreme | 24 | Collapsing gantry |

### Leaping

| Distance | DC |
| --- | --- |
| Short (5 ft) | 8 |
| Medium (10 ft) | 12 |
| Long (15 ft) | 16 |
| Extreme (20 ft) | 20 |

### Stealth Movement

| Surface | DC |
| --- | --- |
| Silent (carpet) | 10 |
| Normal (concrete) | 14 |
| Noisy (loose rubble) | 18 |
| Very Noisy (scrap metal) | 22 |

---

## Fall Damage

```
Fall Damage = (Height in 10-ft increments) × 1d10, max 10d10
```

**Reduction:** Successful crash landing check reduces by 1d10

---

## Dice Pool Calculation

```
Pool = FINESSE + Rank + Equipment Mod + Surface Mod + Stealth Mod
```

**Equipment Modifiers:**

- Climbing Gear: +1d10
- Heavy Armor: -1d10

**Surface Modifiers:**

- Stable: +1d10
- Structurally Compromised: -2d10

---

## Master Rank Benefits (Rank 5)

- **Spider Climb:** Auto-succeed DC ≤ 12, full speed climbing
- **Cat's Grace:** Fall 30 ft with no damage, always land on feet
- **Ghost Walk:** Auto-succeed stealth DC ≤ 14, even in heavy armor
- **Death-Defying Leap:** +10 ft to max leap distance
- **Perfect Balance:** Auto-succeed balance DC ≤ 16

---

## Integration Points

**Dependencies:** Attributes (FINESSE), Dice Pool System, Environmental Systems

**Referenced By:** Exploration System, Stealth System, Strandhögg Specialization, Myrk-gengr Specialization