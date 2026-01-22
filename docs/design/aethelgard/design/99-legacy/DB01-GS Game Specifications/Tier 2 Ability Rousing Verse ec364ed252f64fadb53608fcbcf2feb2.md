# Tier 2 Ability: Rousing Verse

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKALD-ROUSINGVERSE-v5.0
Mechanical Role: Support/Healer
Parent item: Skald (Chronicler of Coherence) — Specialization Specification v5.0 (Skald%20(Chronicler%20of%20Coherence)%20%E2%80%94%20Specialization%20S%203faadeffffc94a9fb7f3ce1e643ad740.md)
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
| **Specialization** | Skald (Chronicler of Coherence) |
| **Tier** | 2 (Advanced) |
| **Type** | Active (Instant) |
| **Prerequisite** | 8 PP spent in Skald tree |
| **Cost** | 40 Stamina |
| **Target** | Single ally |

---

## I. Design Context (Layer 4)

### Core Design Intent

Rousing Verse provides the Skald with **targeted Stamina restoration**—a critical support ability that extends ally combat effectiveness. This positions the Skald as a **resource battery** for Stamina-dependent allies.

### Mechanical Role

- **Primary:** Single-target Stamina restoration
- **Secondary:** Enables aggressive Stamina expenditure strategies
- **Fantasy Delivery:** The rallying cry that renews a warrior's fighting spirit

### Balance Considerations

- **Power Level:** High utility (Stamina is the universal combat resource)
- **Efficiency:** Net Stamina positive for party (costs 40, restores 50+)
- **Limitation:** Single target, not usable on self

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skald speaks a single, perfect verse—a fragment of some ancient victory hymn. The words strike the target like cold water, shocking their system back to alertness. Muscles that were burning with fatigue suddenly feel fresh. The verse doesn't heal wounds; it reminds the body that it has more to give.

### Thematic Resonance

Stamina in Aethelgard represents not just physical endurance but **will to continue fighting**. Rousing Verse is a narrative injection—a story of "you can do this" that the body believes.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 40 Stamina
- **Range:** 6 meters (hearing range)
- **Target:** Single ally (cannot target self)

### Effect

**Target ally immediately recovers Stamina:**

- **Base Recovery:** 50 Stamina
- **Scaling:** +5 Stamina per Skald's WILL above 10
- **Formula:** `50 + ((WILL - 10) × 5)` Stamina restored

### Worked Example

> **Skald with WILL 14:**
> 

> - Base: 50 Stamina
> 

> - WILL Bonus: (14 - 10) × 5 = 20 Stamina
> 

> - **Total Restored:** 70 Stamina
> 

> - **Net Party Gain:** 70 - 40 = +30 Stamina
> 

### Resolution Pipeline

1. **Targeting:** Skald selects ally within 6 meters
2. **Cost Payment:** Skald spends 40 Stamina
3. **Calculation:** Compute restoration amount based on Skald's WILL
4. **Application:** Target gains calculated Stamina (cannot exceed maximum)
5. **Overflow:** Stamina above maximum is lost

### Edge Cases

- **Full Stamina Target:** Ability can still be used but excess is wasted
- **Dead/Unconscious Ally:** Cannot target
- **Self-Target:** Not allowed
- **While [Performing]:** Can be used if not already using Standard Action that round

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Restore 50 + (WILL-10)×5 Stamina to single ally
- Cost: 40 Stamina
- Range: 6 meters

### Rank 2 (Expert — 20 PP)

- Restore 70 + (WILL-10)×5 Stamina to single ally
- Cost: 35 Stamina
- Range: 8 meters
- **New:** Also removes [Exhausted] status from target

### Rank 3 (Mastery — Capstone)

- Restore 90 + (WILL-10)×5 Stamina to single ally
- Cost: 30 Stamina
- Range: 10 meters
- **New:** Can target 2 allies simultaneously (split restoration evenly)
- **New:** Grants target +1 bonus die to next Stamina-based ability

---

## V. Tactical Applications

1. **Berserkr Battery:** Fuel Berserkr's expensive Fury-generating abilities
2. **Tank Sustainment:** Keep Skjaldmær's Shield Wall and Guardian's Taunt online
3. **Emergency Recovery:** Restore ally who exhausted Stamina on critical action
4. **Extended Fights:** Enable aggressive ability usage without Stamina rationing
5. **Boss Phases:** Refresh key ally between boss phases

---

## VI. Synergies & Interactions

### Positive Synergies

- **Berserkr:** High Stamina costs benefit massively from restoration
- **Skjaldmær:** Maintains expensive defensive abilities longer
- **Einbúi:** Extends survival toolkit availability
- **High-WILL Skald:** Scaling makes ability increasingly efficient

### Negative Synergies

- **Aether Pool users (Mystics):** Cannot restore Aether Pool, only Stamina
- **Self-sufficient builds:** Less value for characters who don't expend Stamina aggressively
- **Low-WILL Skalds:** Reduced efficiency