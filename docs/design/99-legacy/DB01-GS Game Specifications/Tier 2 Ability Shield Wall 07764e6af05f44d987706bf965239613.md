# Tier 2 Ability: Shield Wall

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKJALDMAER-SHIELDWALL-v5.0
Mechanical Role: Support/Healer, Tank/Durability
Parent item: Skjaldmær (Bastion of Coherence) — Specialization Specification v5.0 (Skjaldm%C3%A6r%20(Bastion%20of%20Coherence)%20%E2%80%94%20Specialization%20%2083c338d903f54a5692dbaa63a5cf7b07.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Skjaldmær (Bastion of Coherence) |
| **Tier** | 2 (Advanced) |
| **Type** | Active (Formation Buff) |
| **Prerequisite** | 8 PP spent in Skjaldmær tree |
| **Cost** | 45 Stamina |
| **Target** | Skjaldmær + adjacent Front Row allies |
| **Duration** | 2 turns |

---

## I. Design Context (Layer 4)

### Core Design Intent

Shield Wall is the Skjaldmær's **formation-based defensive cooldown**—a powerful group buff that rewards positioning and party coordination. This establishes the Skjaldmær as the anchor of shield-wall tactics.

### Mechanical Role

- **Primary:** +3 Soak, Push/Pull Resistance, and +1 die to Psychic Stress Resolve for formation
- **Secondary:** Rewards positional play and party coordination
- **Fantasy Delivery:** The legendary shield-wall formation of the old world made manifest

### Balance Considerations

- **Power Level:** Very high (multi-target, multi-effect)
- **Position Requirement:** Front Row, adjacent allies only
- **Opportunity Cost:** High Stamina cost limits other actions

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skjaldmær plants her feet and raises her shield—not just as protection, but as an **anchor point for reality itself**. Allies instinctively rally to this bastion of coherence. Shields lock together, breath synchronizes, and for a moment the formation becomes a single, unified entity. A wall of flesh, steel, and stubborn refusal to break.

### Thematic Resonance

The shield-wall is one of humanity's oldest defensive formations. In Aethelgard, it takes on metaphysical significance—a coordinated act of coherence that pushes back against the Glitch's entropy. The Skjaldmær is the cornerstone; without her, the wall crumbles.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 45 Stamina
- **Targets:** Skjaldmær + all **adjacent allies in Front Row**

### Effect

**All affected characters gain for 2 turns:**

1. **Soak Bonus:** +3 Soak
2. **Position Lock:** Resistance to Push/Pull effects
3. **Resolve Bonus:** +1 bonus die (+1d10) to Resolve Checks against Psychic Stress

### Position Requirements

- **Skjaldmær:** Must be in Front Row
- **Allies:** Must be in Front Row AND adjacent to Skjaldmær
- **Dynamic Update:** If ally moves out of adjacent position, they lose the buff

### Resolution Pipeline

1. **Position Check:** Verify Skjaldmær is in Front Row
2. **Cost Payment:** Skjaldmær spends 45 Stamina
3. **Target Identification:** Identify all Front Row allies adjacent to Skjaldmær
4. **Buff Application:** Apply all three effects to Skjaldmær and valid allies
5. **Position Tracking:** Monitor positions; remove buff from allies who move away
6. **Duration Track:** Effect lasts 2 turns

### Worked Example

> **Formation:**
> 

> `
> 

> [Berserkr] [Skjaldmær] [Gorge-Maw]
> 

> [Skald]     [Jötun-Reader]
> 

> `
> 

> **Shield Wall affects:** Skjaldmær, Berserkr, Gorge-Maw (all Front Row, adjacent)
> 

> **NOT affected:** Skald, Jötun-Reader (Back Row)
> 

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +3 Soak, Push/Pull Resistance, +1 die to Psychic Stress Resolve
- Duration: 2 turns
- Cost: 45 Stamina

### Rank 2 (Expert — 20 PP)

- +4 Soak, Push/Pull Immunity, +2 dice to Psychic Stress Resolve
- Duration: 3 turns
- Cost: 40 Stamina
- **New:** Adjacent allies also gain +1 Defense

### Rank 3 (Mastery — Capstone)

- +5 Soak, Push/Pull Immunity, +2 dice to Psychic Stress Resolve
- Duration: 4 turns
- Cost: 35 Stamina
- **New:** Shield Wall now affects ALL Front Row allies (not just adjacent)
- **New:** While Shield Wall is active, Skjaldmær can use Interposing Shield without Stamina cost

---

## V. Tactical Applications

1. **Formation Defense:** Create impenetrable front line during high-damage phases
2. **Position Lock:** Prevent enemies from disrupting formation with Push/Pull
3. **Stress Mitigation:** Party-wide mental defense in Blight-heavy areas
4. **Boss Phases:** Activate during predictable high-damage boss attacks
5. **Coordinated Tanking:** Multiple Front Row characters share enhanced survivability

---

## VI. Synergies & Interactions

### Positive Synergies

- **Berserkr:** Front Row Berserkr gains Soak they normally lack
- **Skald (Lay of the Iron Wall):** Soak stacking creates massive damage reduction
- **Guardian's Taunt:** Combine for ultimate "all damage on Skjaldmær" strategy
- **Formation-focused parties:** More adjacent allies = more value

### Negative Synergies

- **Mobile/ranged parties:** Few adjacent Front Row allies reduces value
- **Solo Skjaldmær:** Full value requires Front Row allies
- **Position-fluid encounters:** Benefits lost when formation breaks