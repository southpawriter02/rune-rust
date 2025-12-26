# Damage Types & Resistances

## Codex Entry: CODX-C-DMG-001

> "The world kills you in many ways.
>
> Steel kills you by opening you up. Fire kills you by eating what you are. Frost kills you by stopping you from being. The Aether... the Aether kills you by rewriting the rules of why you're alive in the first place.
>
> Wear plate for the steel. Wear leathers for the fire. But for the Aether?
>
> For the Aether, you just pray."
>
> — Hestia the Scarred, Ranger of the Ashen Waste

---

## Overview

| Property | Value |
|----------|-------|
| Document ID | AAM-SPEC-COMBAT-DAMAGE-v1.0 |
| Classification | AETHELGARD-ARCHIVES-MASTER // COMBAT SYSTEMS |
| Subject Scope | Damage Types, Resistances, Soak Mechanics |
| Document Layer | Layer 2 (Diagnostic) |
| Core Doctrine | "Defense must be specific to the threat." |

This specification defines the canonical damage types in Aethelgard and the mechanics for mitigating them. It distinguishes between **Soak** (damage absorption via material) and **Resistance** (damage deflection via properties).

---

## 1. Damage Categories

### 1.1 Physical Damage
Kinetic force applied to biological or mechanical structures.

| Type | Primary Effect | Common Sources | Status Association |
|------|----------------|----------------|--------------------|
| **Slashing** | Soft tissue trauma | Swords, Axes, Claws | [Bleeding] |
| **Piercing** | Penetration | Spears, Arrows, Daggers | [Armor Penetration] |
| **Bludgeoning** | Concussive force | Hammers, Mauls, Falls | [Stunned], [Dazed] |

### 1.2 Elemental Damage
Energy-based damage derived from environmental or alchemical sources.

| Type | Primary Effect | Common Sources | Status Association |
|------|----------------|----------------|--------------------|
| **Fire** | Thermal degradation | Flame, Explosions, Muspelheim Tech | [Burning] |
| **Frost** | Molecular deceleration | Ice, Cryo-tech, Niflheim Tech | [Slowed], [Frozen] |
| **Shock** | Electrical disruption | Lightning, Automata discharge | [Stunned], [Disrupted] |
| **Acid** | Chemical corrosion | Biological fluids, Alchemy | [Corroded] (Soak reduction) |

### 1.3 Esoteric Damage
Exotic damage types affecting the mind, soul, or reality cohesion.

| Type | Primary Effect | Common Sources | Status Association |
|------|----------------|----------------|--------------------|
| **Psychic** | Cognitive stress | Mind-magic, Horrors, Stress | [Fear], [Hallucinating] |
| **Radiant** | Anti-Corruption | Pre-Glitch tech, Healing mishaps | [Blinded] |
| **Necrotic** | Entropy/Decay | Undead, Blight, Corruption | [Withered] |
| **True** | Reality deletion | Glitch anomalies, God-weapons | **None** (Cannot be mitigated) |

---

## 2. Defense Mechanics

Characters defend against damage through two distinct layers: **Resistance (Deflection)** and **Soak (Absorption)**.

### 2.1 The Defense Equation

```
Final Damage = ([Incoming Damage] - [Flat Resistance]) - [Soak] * (1 - [Vulnerability])
```

*Order of Operations:*
1.  **Vulnerability**: Multiplier applies first (increasing the incoming raw value).
2.  **Resistance**: Deflects damage *before* it hits armor.
3.  **Soak**: Armor absorbs the remaining kinetic/thermal energy.

### 2.2 Resistance (The Shield)
Resistance represents specific protection that stops damage from reaching the target. It is usually provided by enchantments, tech-fields, or specialized coatings.

*   **Mechanic**: Flat reduction.
*   **Stacking**: Stacks from different sources (Subject to 75% Hard Cap).
*   **Example**: *Ring of Fire Warding* (+5 Fire Res) reduces a 15 damage Fireball to 10.

### 2.3 Soak (The Armor)
Soak represents physical material absorbing impact.

*   **Mechanic**: Flat reduction.
*   **Interaction**:
    *   **Physical Soak**: Applies to Slashing, Piercing, Bludgeoning. (Standard Armor).
    *   **Elemental Soak**: Applies ONLY if the armor is specifically sealed/treated.
        *   *Standard Plate*: 0 Soak vs Fire.
        *   *Insulated Plate*: Disadvantage on Heat checks, but applies Soak vs Fire.
*   **Bypass**: Some attacks have [Armor Penetration] which ignores Soak but *not* Resistance.

### 2.4 Specific Defense Gear (User Request)

Certain equipment is designed to counter specific threats.

| Gear Archetype | Defense Type | Logic |
|----------------|--------------|-------|
| **Cryo-Suit** | [Frost Soak] | Thermal insulation absorbs cold damage. |
| **Hazmat Rig** | [Acid Soak] | Chem-resistant layers absorb acid. |
| **Faraday Cage** | [Shock Resistance] | Conductive mesh deflects lightning around user. |
| **Aegis Plate** | [Physical Soak] | Hardened steel absorbs kinetic impact. |
| **Mind-Shield** | [Psychic Resistance] | Psychic dampeners deflect mental stress. |

> [!NOTE]
> **Why distinction matters**: A Faraday Cage (Resistance) stops the lightning from touching you. A Cryo-Suit (Soak) lets the cold touch you but keeps you warm. Armor Penetration (e.g., a railgun slug) punches through the Cryo-Suit (Soak) but might still be deflected by a force field (Resistance).

---

## 3. Advanced Rules

### 3.1 Vulnerability
Character takes **+50% damage** (rounded down) from the specified type.
*   *Causes*: Biome effects, Curse status, oil-soaked (vs Fire).

### 3.2 Immunity
Character takes **0 damage** from the specified type. Neither Resistance nor Soak is calculated.
*   *Causes*: Phase-shift, God-tier artifacts.

### 3.3 True Damage
Ignores **ALL** Resistance, Soak, Vulnerability, and Immunity.
*   *Sources*: Reality failures, Timeline collapse.

### 3.4 Shield Interaction
Shields provide **Defense** (avoidance), but can also provide **Resistance** via the [Block] action.

*   **Standard Block**: Adds Shield Defense bonus to Armor Class.
*   **Active Block**: On reaction, shield can provide Cover (Resistance) vs Elemental AoE.
    *   *Tower Shield*: Can block Dragonfire breath (applies Shield Soak to Fire damage).
    *   *Buckler*: Cannot block AoE.

---

## 4. Voice Guidance

### Layer 1 (Mythic) — The Ranger's Lesson

> "Fire is a hungry beast. It eats leather, it eats flesh, it eats bone. Plate won't stop it—pours right through the joints. You need beast-skin for the beast-fire.
>
> Lightning is a snake. It wants the path of least resistance. Your fancy steel suit? That's a highway for the snake. You need rubber, leather, wood.
>
> But steel... steel respects steel. If a man comes at you with an axe, you want the thickest plate you can find. Let the iron argue with the iron."

### Layer 2 (Diagnostic) — Threat Assessment

> "DAMAGE MITIGATION ANALYSIS
>
> KINETIC THREATS (Slashing/Piercing/Bludgeoning): Mitigated by material density (Soak). High-density plating recommended.
>
> THERMAL THREATS (Fire/Frost): Ignore standard material density. Requires specialized insulation (Specific Soak) or thermal dispersion fields (Resistance). Standard alloy plating operates at 0% efficiency against thermal transfer.
>
> ENTROPIC THREATS (Necrotic/Radiant): Ignore physical matter entirely. Mitigation requires Aetheric interference patterns (Esoteric Resistance). Physical armor provides 0 protection."
