# Tier 3 Ability: Live off the Land

Type: Ability
Priority: Nice-to-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-EINBUI-LIVEOFFTHELAND-v5.0
Mechanical Role: Support/Healer, Utility/Versatility
Parent item: Jötun-Reader (System Analyst) — Specialization Specification v5.0 (J%C3%B6tun-Reader%20(System%20Analyst)%20%E2%80%94%20Specialization%20Spe%209f9d73ebaf304e2d94a7e84521648919.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Einbúi (The Hermit) |
| **Tier** | 3 (Master Survival) |
| **Type** | Passive + Granted Active |
| **Prerequisite** | 20 PP spent in Einbúi tree |
| **Passive Effect** | No rations required for Wilderness Rest |
| **Active Effect** | `find water` command (once per day) |

---

## I. Design Context (Layer 4)

### Core Design Intent

Live off the Land is the **pinnacle of the Einbúi's survivalist identity**—the moment they become a true **child of the wastes**. The concept of "scarcity" has become alien to them. They don't just endure the wilderness; they are part of its ecosystem.

### Mechanical Role

- **Primary:** Remove ration requirement for Wilderness Rest
- **Secondary:** Generate [Clean Water] for party once per day
- **Fantasy Delivery:** The ultimate survivor who needs nothing

### Balance Considerations

- **Power Level:** Very High (removes major resource pressure)
- **Logistical Impact:** Frees inventory and Data-Shards
- **Expedition Extension:** Enables longer delves without resupply

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Einbúi has become so attuned to the corrupted world that they know its **hidden oases and secret larders**. They subsist on strange flora that others would deem inedible. They find clean water in places others would never think to look.

*"Hungry? There's edible lichen on that rock. And I know a spring about an hour's walk east."*

### Thematic Resonance

Live off the Land transforms the Einbúi from survivor to **ecosystem native**—proof that complete adaptation is the ultimate form of self-reliance.

---

## III. Mechanical Specification (Layer 3)

### Part 1: Ration Immunity (Passive)

- Einbúi does NOT consume [Ration] during Wilderness Rest
- Avoids [Exhausted] debuff without food
- Other party members still require rations

### Part 2: Find Water (Granted Active)

- **Command:** `find water`
- **Use Context:** Out of Combat, Once per in-game day
- **Check:** WITS + Wasteland Survival vs Biome DC

| Biome | DC |
| --- | --- |
| Midgard (Forest) | 3 |
| Jotunheim (Industrial) | 5 |
| Muspelheim (Volcanic) | 7 |
| Result | Outcome |
| -------- | ---------- |
| **Success** | Party gains enough [Clean Water] for one rest |
| **Critical Success** | Water for TWO rests |
| **Failure** | No water found |

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Ration immunity + `find water` with skill check

### Rank 2 (Expert — Base form for Tier 3)

- As Rank 1 (this is the base form)

### Rank 3 (Mastery — Capstone)

- `find water` **automatically succeeds** (no check required)

---

## V. Tactical Applications

1. **Extended Expeditions:** Party can delve deeper without resupply
2. **Inventory Freedom:** No need to carry food/water supplies
3. **Economic Savings:** Data-Shards saved on consumables
4. **Emergency Recovery:** Find water when party runs out

---

## VI. Synergies & Interactions

### Positive Synergies

- **Long expeditions:** Maximum value on multi-day delves
- **Resource-tight parties:** Covers critical survival gap
- **High WITS:** Reliable water discovery

### Negative Synergies

- **Combat focus:** No combat application
- **Short missions:** Less value if resupply is easy