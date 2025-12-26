# Psychic Stress System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-PSYCHICSTRESS-v5.0
Parent item: Trauma Economy System — Core System Specification v5.0 (Trauma%20Economy%20System%20%E2%80%94%20Core%20System%20Specification%20%205f91a2cde13340e98ec5c4754f476ea0.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Psychic Stress quantifies **short-term exposure to the Runic Blight's anti-logical trauma**—the Cursed Choir's eternal psychic scream. Unlike permanent Corruption, Stress is recoverable through rest, creating short-term tactical decisions.

**Range:** 0-100%

**Breaking Point:** 100% triggers Resolve Check with potential permanent Trauma

---

## Stress Threshold Tiers

| Tier | Range | Effects |
| --- | --- | --- |
| 0 (Calm) | 0-25% | No effects |
| 1 (Frayed) | 26-50% | UI distortion begins |
| 2 (Stressed) | 51-75% | -1d10 to Resolve Checks |
| 3 (Critical) | 76-99% | [System Lag] healing penalty |
| 4 (Breaking) | 100% | Breaking Point triggered |

---

## Stress Accumulation Sources

| Source | Base Stress | Frequency |
| --- | --- | --- |
| [Psychic Resonance] Low | 5 | Per turn |
| [Psychic Resonance] High | 10 | Per turn |
| Forlorn Presence | 5-10 | Per turn |
| Heretical Ability Use | 10-25 | Per use |
| Ally Defeated | 15 | Per event |
| [Corrupted Data-Slate] | 15-30 | Per read |

---

## Breaking Point Resolution

**DC:** 16 (WILL-based Resolve Check)

| Outcome | Stress Reset | Status | Trauma |
| --- | --- | --- | --- |
| Critical Success | 60% | None | None |
| Success | 75% | [Disoriented] 2 turns | None |
| Failure | 50% | [Stunned] 1 turn | +1 permanent |
| Critical Failure | 60% | [Stunned] 2 turns | +1 severe |

---

## Stress Reduction

| Method | Amount |
| --- | --- |
| Sanctuary Rest | Full reset to 0 |
| Wilderness Camp (DC 14) | 15-30 |
| Stabilizing Draught | 25 |
| Spiritual Anchor | 20 |

---

## Special Conditions

**Dvergar Immunity:** Full immunity to Psychic Stress (don't perceive the Cursed Choir)

**Corruption Interaction:** +5% Stress vulnerability per 20 Corruption

---

## Integration Points

**Dependencies:** Attributes (WILL), Dice Pool System, Combat System

**Referenced By:** All Heretical Specializations, Enemy Specifications, Environmental Systems