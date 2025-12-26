---
id: SPEC-SPECIALIZATION-RUNASMIDR
title: "Rúnasmiðr (Rune-Smith)"
version: 1.0.0
status: draft
last-updated: 2024-05-22
related_specs: [SPEC-CHAR-001, SPEC-COMBAT-001]
---

# SPEC-SPECIALIZATION-RUNASMIDR: Rúnasmiðr (Rune-Smith)

> **Version:** 1.0.0
> **Status:** Draft
> **Service:** `RuneAndRust.Core/Entities/Specialization.cs` (Proposed)
> **Location:** `docs/specs/character/SPEC-RUNASMIDR-001.md`

---

## Table of Contents

- [Identity](#identity)
- [Design Philosophy](#design-philosophy)
- [Mechanics](#mechanics)
- [Rank Progression](#rank-progression)
- [Voice Guidance](#voice-guidance)
- [Post-Glitch Compliance](#post-glitch-compliance)
- [Synergies](#synergies)
- [Changelog](#changelog)

---

## Identity

> "We do not write the future. We carve the present."

The **Rúnasmiðr** (Rune-Smith) is an architect of Aetheric resonance, channeling the raw potential of the world through inscribed sigils. They are not mages in the high-fantasy sense, but laborers who understand the weight of words. Every rune is a burden; every inscription requires a sacrifice of endurance and will. They are the smiths of the unseen, hammering reality into new shapes with the force of their conviction.

---

## Design Philosophy

### Mechanics First
The Rúnasmiðr operates on a **Preparation & Activation** cycle. They must spend Action Points (AP) or Stamina to "Scribe" runes, which act as loaded charges, and then "Activate" them for effects.

### Lore Integration
Runes are dangerous. They are not spells but "Aetheric conduits." Using them causes "Strain" (a specialized resource or cost). The Rúnasmiðr treats magic as heavy industry—loud, impactful, and exhausting.

### Balance
- **Early Game:** High utility, low damage. Can create zones of safety or minor buffs.
- **Late Game:** High burst potential, capable of rewriting battlefield conditions, but high resource cost.

### Dual-Implementation
- **Terminal:** Rune states are displayed as `[FEHU: ACTIVE]` or `[URUZ: INERT]`.
- **GUI:** Visual runes glowing or dull on the character HUD.

---

## Mechanics

### Core Resolution
**WITS + d10 vs DC**
Most Rúnasmiðr abilities rely on **WITS** (understanding the rune) and **WILL** (forcing it to work).

### Resource: Runic Charge
- **Maximum Charges:** 3 (Base)
- **Regeneration:** Requires "Scribe" action.

### Damage Tiering
- **Tier 1 (Basic Inscription):** d6 (Generic environmental effect)
- **Tier 2 (Runic Hammer):** 2d8 + WITS
- **Tier 3 (Master Inscription):** d10 + WITS + WILL

---

## Rank Progression

### Tier 1 (Novice)

| Ability | Cost | Effect |
|---------|------|--------|
| **Scribe Rune** | 2 AP | Inscribe a rune on a surface or weapon. Grants 1 Runic Charge. |
| **Ignite (Kenaz)** | 1 Charge | Deal **1d6 Fire** damage to a target within range. Light source. |
| **Ward (Algiz)** | 1 Charge | Grant **+2 Armor** to self or ally for 1 round. |

### Tier 2 (Adept) - *Requires 8 PP*

| Ability | Cost | Effect |
|---------|------|--------|
| **Runic Hammer** | 2 AP + 1 Charge | Melee Attack. Deals **2d8 + WITS** Force damage. Knocks target back. |
| **Flow (Laguz)** | 1 Charge | Clear 1 Negative Status Effect (e.g., Bleeding, Slowed). |
| **Bind (Nauthiz)** | 2 Charges | Target must pass **WILL vs DC** or be **Immobilized** for 1 round. |

### Tier 3 (Master) - *Requires 16 PP*

| Ability | Cost | Effect |
|---------|------|--------|
| **Thunder-Script (Thurisaz)** | 3 Charges | Area of Effect. Deals **1d10 + WITS** Lightning damage to all enemies in range. |
| **Overcharge** | 1 Trauma | Immediately regain all Runic Charges. Take **1d6 Internal** damage. |

### Capstone - *Requires 24 PP*

| Ability | Cost | Effect |
|---------|------|--------|
| **World-Carver** | All Charges + 4 AP | Rewrite the terrain. Create high cover, remove obstacles, or deal **3d10 + WITS** damage to a single target. |

---

## Voice Guidance

* **Tone:** Methodical, weary, reverent. Refer to runes as living burdens, not tools.
* **Keywords:** Inscription, Etch, Channel, Resonance, Burden, Weight, Anchor.
* **Avoid:** "Cast", "Spell", "Mana", "Magic", "Instant".
* **Narrator Persona:** The Rúnasmiðr speaks like a tired engineer explaining a dangerous machine. "The rune wants to break the stone. I must convince it to break the enemy instead."

### Examples

| Context | Bad Voice | Good Voice |
|---------|-----------|------------|
| **Attack** | "I cast Fireball!" | "I carve Kenaz into the air; the Aether ignites." |
| **Healing** | "I use a healing spell." | "I trace Uruz on your wound. Hold still, the knitting will burn." |
| **Failure** | "The spell fizzled." | "The inscription failed to take hold. The geometry was wrong." |

---

## Post-Glitch Compliance (Domain 4)

* **No Tech Terms:** Do not use "Upload", "Download", "Program", "Energy Field".
* **Replacements:**
    * Electricity -> Lightning / Static
    * Radiation -> Invisible Fire / Sickness
    * Robot -> Iron-Walker
    * Data -> Memories / Echoes

---

## Synergies

| Role | Interaction | Type |
|------|-------------|------|
| **Berserkr** | **Positive:** Rúnasmiðr can Ward the Berserkr, allowing them to tank more hits while generating Fury. | Defensive |
| **Ruin Stalker** | **Positive:** Immobilized enemies (Nauthiz) are easy targets for Sneak Attacks. | Offensive |
| **Bone-Setter** | **Negative:** Rúnasmiðr's "Overcharge" ability creates Trauma that the Bone-Setter must heal, draining resources. | Resource Drain |

---

## Changelog

### v1.0.0 (2024-05-22)
- Initial specification created by Worldbuilder.
- Defined Tier 1-3 progression and Capstone.
- Established WITS-based mechanics.
