# Aether Pool (AP) System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-AP-v5.0
Parent item: Resource Systems — Core System Specification v5.0 (Resource%20Systems%20%E2%80%94%20Core%20System%20Specification%20v5%200%20341e0d234c7d4e0d9be146f9579c2bd2.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Aether Pool (AP) measures a Mystic's capacity to **safely cache corrupted Aether** without mental overwrite. AP is not serene energy—it is a volatile, dangerous reservoir of the world's corrupted "software."

**The Death Spiral:** Casting spells → gaining Corruption → losing AP capacity → needing more expensive spells → gaining more Corruption.

**Archetype Lock:** Non-Mystics have 0 AP and cannot gain it from any source.

---

## Calculation Formulas

### Max AP

```
Max AP = (50 + [WILL × 10] + Flat Bonuses) × (1 + % Bonuses) × (1 - Corruption Penalty)
```

**Base AP:** 50 for Mystics, 0 for all others

**Floor:** Minimum 10 AP (Mystics only)

### Corruption Penalty

```
Corruption Penalty = MIN((Corruption ÷ 10) × 0.05, 0.50)
```

### Focus Aether Regeneration

```
AP Regen = Base (10) + Passive Bonuses + Active Buffs
```

**Cost:** Standard Action

---

## Spell AP Costs

| Spell Tier | AP Cost | Corruption |
| --- | --- | --- |
| Tier 1 (Basic) | 10-20 AP | 1-2 Corruption |
| Tier 2 (Intermediate) | 25-40 AP | 3-5 Corruption |
| Tier 3 (Advanced) | 50-70 AP | 6-10 Corruption |
| Capstone | 80-100 AP | 12-15 Corruption |

**Note:** Every Mystic spell inflicts guaranteed Corruption per cast.

---

## Regeneration Methods

| Method | AP Restored | Cost/Limitation |
| --- | --- | --- |
| Sanctuary Rest | Full (100%) | Must rest at Runic Anchor |
| Focus Aether | 10-25 AP | Standard Action, once/turn |
| Consumables | 20-40 AP | Rare, often inflict Psychic Stress |

---

## The Mystic Time Limit

Mystics are **time-limited** archetypes. Every spell brings them closer to Terminal Error (100 Corruption) or the death spiral. Strategic spell conservation is essential.

---

## Integration Points

**Dependencies:** Attributes (WILL), Archetype System, Trauma Economy, Equipment ([Aether-Infused] bonuses)

**Referenced By:** All Mystic Specializations, Combat System, Corruption System