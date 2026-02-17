# Action System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-ACTION-v5.0
Parent item: Encounter System — Core System Specification v5.0 (Encounter%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20c82c42d7129c4843a86f2e69cd72f0d7.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

The Action System is the **fundamental currency of the combat turn**. Every action represents a finite resource—a small bubble of agency in battle chaos.

---

## Action Allotment Per Turn

| Action Type | Quantity | Reset |
| --- | --- | --- |
| Standard Action | 1 | Start of turn |
| Free Action | 1 | Start of turn |
| Reaction | 1 | Start of round |

---

## Standard Actions (Primary Command)

| Action | Description |
| --- | --- |
| `attack` | Basic attack with equipped weapon |
| `[ability]` | Use specialization ability |
| `block` | Full defensive stance |
| `glitch-dash` | High-mobility repositioning |
| `use [item]` | Use consumable/tool |
| `struggle` | Break free from [Rooted] |
| `move` | Move between combat rows |

**Design:** Making `move` a Standard Action creates positional trade-offs

---

## Free Actions (Incidental Act)

| Action | Description |
| --- | --- |
| `stance [name]` | Change combat stance |
| `drop [item]` | Drop item |
| `command [companion]` | Issue companion order |
| `activate [charge]` | Expend runic charge |
| `speak` | Shout, taunt, coordinate |

---

## Reactions (Instantaneous Interruption)

**Universal:** `parry` — Reduce damage from incoming melee attack

**Specialization Examples:**

- Skjaldmær: Interposing Shield (protect adjacent ally)
- Örlög-bound: Fated Intervention (modify ally's critical hit/fumble)
- Shadow-Thief: Vanish (disappear when targeted)

---

## Status Effect Modifications

| Effect | Impact |
| --- | --- |
| [Hasted] | -25% resource costs |
| [Slowed] | +50% resource costs |
| [Stunned] | Lose Standard Action |
| [Rooted] | Cannot use `move` |

---

## Action Economy Manipulation by Specialization

| Specialization | Feature | Identity |
| --- | --- | --- |
| Skjaldmær | Reaction protects allies | Protector |
| Beast-Binder | Companion command is Free | Two actions/turn |
| Strandhögg | Free move on kill | Hit-and-run |

---

## Integration Points

**Dependencies:** Turn System, Combat System, Abilities Database

**Referenced By:** All Specialization Abilities, Status Effect System, Equipment System