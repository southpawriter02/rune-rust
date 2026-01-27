# Tier 2 Ability: Structural Insight

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-JOTUNREADER-STRUCTURALINSIGHT-v5.0
Mechanical Role: Utility/Versatility
Parent item: Jötun-Reader (System Analyst) — Specialization Specification v5.0 (J%C3%B6tun-Reader%20(System%20Analyst)%20%E2%80%94%20Specialization%20Spe%2049d598674f0341dfa20494d7c3230427.md)
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
| **Specialization** | Jötun-Reader (System Analyst) |
| **Tier** | 2 (Advanced Analysis) |
| **Type** | Passive (Environmental Awareness) |
| **Prerequisite** | 8 PP spent in Jötun-Reader tree |
| **Cost** | None (Passive) |
| **Effect** | Automatically identify all interactable/hazardous environmental objects |

---

## I. Design Context (Layer 4)

### Core Design Intent

Structural Insight is the Jötun-Reader's **primary environmental analysis tool**—an almost instinctual understanding of architecture and structural engineering. When they enter a room, they don't see walls and pillars; they see a **living architectural blueprint**.

### Mechanical Role

- **Primary:** Auto-identify all interactive environmental elements
- **Secondary:** Reveal hazard locations and properties automatically
- **Fantasy Delivery:** The mind that reads ruins like an open book

### Balance Considerations

- **Power Level:** High (massive information advantage)
- **Quality of Life:** Eliminates tedious investigation of every object
- **Always Active:** No resource cost for passive perception
- **Tactical Surveyor:** Party relies on this for environmental combat

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Jötun-Reader's mind is a living repository of pre-crash blueprints and stress-test data. When they enter a room, they see **layers invisible to others**—load-bearing columns, cracked support beams, unstable flooring, volatile machinery.

*"That pillar is structural. The pipes along the ceiling contain pressurized steam—one good hit would rupture them. And that flooring section? It's ready to collapse."*

They are the party's **tactical surveyor**.

### Thematic Resonance

Structural Insight transforms every battlefield into a tactical playground—proof that the Jötun-Reader sees opportunities where others see only obstacles.

---

## III. Mechanical Specification (Layer 3)

### Automatic Identification

**Passive Effect:**

- Triggers automatically on entering new room or using `look` command
- Auto-succeed on Perception checks to detect/identify environmental features

### Identified Features

| Category | Examples |
| --- | --- |
| **Dynamic Hazards** | [High-Pressure Steam Vent], [Live Power Conduit], [Unstable Ceiling] |
| **Destructible Obstacles** | Low-durability [Cover], smashable [Debris Piles] |
| **Static Terrain** | [Difficult Terrain], [Slippery Terrain] properties |

### Enhanced Look Output

- **Standard Player:** *"A massive, cracked pillar dominates the center."*
- **Jötun-Reader:** *"A massive, cracked pillar dominates the center; your insight tells you it is a key [Structural Support] and is destructible. The pipes along the ceiling include a [High-Pressure Steam Vent] that could be ruptured."*

### Resolution Pipeline

1. **Trigger:** Character enters room or uses `look`
2. **Passive Check:** Does character have Structural Insight?
3. **If Yes:** Iterate through all Hazard/Obstacle objects
4. **Output:** Append analytical descriptions to look output

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Auto-identify all interactive/hazardous elements

### Rank 2 (Expert — 20 PP)

- As Rank 1
- **New:** Also identify the **Target Area** of hazards (blast radius, etc.)

### Rank 3 (Mastery — Capstone)

- As Rank 2
- **New:** Automatically identify **Difficulty Class** of traps and puzzle mechanisms
- **New:** Identify structural weaknesses in enemy fortifications

---

## V. Tactical Applications

1. **Hazard Exploitation:** Know exactly what environmental tools are available
2. **Party Coordination:** Call out opportunities for Battlefield Controllers
3. **Trap Awareness:** Identify dangers before triggering them
4. **Combat Planning:** Pre-battle tactical assessment
5. **Cover Identification:** Know which cover is destructible

---

## VI. Synergies & Interactions

### Positive Synergies

- **Hlekkr-master:** Identify hazards to push enemies into
- **Environmental combat:** Maximum value in interactive battlefields
- **Navigational Bypass:** Know what traps exist to bypass
- **Party coordination:** Information sharing multiplies value

### Negative Synergies

- **Static environments:** Less value in featureless arenas
- **Combat focus:** Information without action is incomplete