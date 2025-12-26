# Runic Blight Corruption System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-CORRUPTION-v5.0
Parent item: Trauma Economy System — Core System Specification v5.0 (Trauma%20Economy%20System%20%E2%80%94%20Core%20System%20Specification%20%205f91a2cde13340e98ec5c4754f476ea0.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Runic Blight Corruption measures **long-term infection** by the All-Rune's paradoxical anti-logic—the permanent rewriting of a character's "source code." Unlike volatile Stress, Corruption is nearly impossible to remove and catastrophic at its peak.

**Range:** 0-100

**Terminal Error:** At 100, character risks transformation into Forlorn

---

## Corruption Threshold Tiers

| Tier | Range | HP/AP Penalty | Glitch Chance |
| --- | --- | --- | --- |
| 0 (Minimal) | 0-20 | None | 0% |
| 1 (Tainted) | 21-40 | None | 5% |
| 2 (Corrupted) | 41-60 | -5% | 10% |
| 3 (Blighted) | 61-80 | -15% | 15% |
| 4 (Extreme) | 81-99 | -25% | 25% |
| 5 (Terminal) | 100 | -25% | Transformation |

---

## Corruption Accumulation Sources

| Source | Corruption | Frequency |
| --- | --- | --- |
| Tier 1 Mystic Spell | 2 | Per cast |
| Tier 2 Mystic Spell | 5 | Per cast |
| Tier 3 Mystic Spell | 8 | Per cast |
| [Glitched Artifact] Minor | 1-2 | Per rest |
| [Glitched Artifact] Major | 3-5 | Per rest |
| [Metaphysical Corruption] Zone | 1-5 | Per day |

---

## Terminal Error

**Trigger:** Corruption reaches 100

**Transformation Probability:** Base 50% + (Corruption over 100 × 5%)

**If Transformation Succeeds:**

- Character becomes hostile NPC (Forlorn, Blighted One, etc.)
- Player's saga ends (character retired)
- Former character becomes boss encounter

**If Transformation Fails:**

- Stabilize at 99 Corruption (hard cap)
- Gain [Terminal Error Survivor] Trauma
- Cannot gain more Corruption

---

## Corruption Reduction (Extremely Rare)

| Source | Removed | Availability |
| --- | --- | --- |
| Runic Suppressant | 1 | Rare consumable |
| Aetheric Purifier | 5-10 | Very rare reagent |
| Grove of Silence | 20-40 | Legendary location |
| Saga Quest Reward | 10-50 | Major quest |

---

## Special Conditions

**Dvergar:** Accumulate Corruption normally (affects code, not perception)

**99 Cap Survivors:** Can use Corruption-costing abilities freely (no additional gain)

---

## Integration Points

**Dependencies:** Dice Pool System, Combat System

**Referenced By:** All Mystic Specializations, All Heretical Specializations, Equipment System, Reality Glitch System