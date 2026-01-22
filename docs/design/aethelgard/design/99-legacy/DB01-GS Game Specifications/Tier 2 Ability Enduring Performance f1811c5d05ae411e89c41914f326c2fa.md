# Tier 2 Ability: Enduring Performance

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKALD-ENDURINGPERFORMANCE-v5.0
Mechanical Role: Support/Healer
Parent item: Skald (Chronicler of Coherence) — Specialization Specification v5.0 (Skald%20(Chronicler%20of%20Coherence)%20%E2%80%94%20Specialization%20S%203faadeffffc94a9fb7f3ce1e643ad740.md)
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
| **Specialization** | Skald (Chronicler of Coherence) |
| **Tier** | 2 (Advanced) |
| **Type** | Passive |
| **Prerequisite** | 8 PP spent in Skald tree |
| **Cost** | None (Passive) |

---

## I. Design Context (Layer 4)

### Core Design Intent

Enduring Performance is a **duration multiplier** for all Skald Performances. This passive transforms the Skald from a short-burst buffer into a **sustained battlefield presence**, dramatically increasing the value of high-WILL investments.

### Mechanical Role

- **Primary:** Extends all Performance durations by +2 rounds
- **Secondary:** Increases Stamina efficiency (longer buffs from same activation cost)
- **Fantasy Delivery:** The master performer whose voice echoes long after they've stopped singing

### Balance Considerations

- **Power Level:** High multiplier effect (affects all Performances)
- **Scaling:** Stacks additively with WILL-based duration
- **No Counterplay:** Passive cannot be disabled (unlike active Performances)

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skald's voice has developed a **resonance** that lingers. Their words don't simply fade—they echo in the minds of listeners, the narrative coherence they create persisting like ripples in still water. Even after the Skald falls silent, the story continues to work its effect.

### Thematic Resonance

In a world of corrupted signals and garbled data, the Skald's ability to create lasting, stable narratives is remarkable. Enduring Performance represents mastery over linguistic persistence—the ability to make stories "stick" in reality's volatile substrate.

---

## III. Mechanical Specification (Layer 3)

### Effect

**All Performances gain +2 rounds to their base duration.**

- **New Duration Formula:** `WILL + 2` rounds (instead of `WILL` rounds)
- **Applies to:** Saga of Courage, Dirge of Defeat, Lay of the Iron Wall, Saga of the Einherjar, and any future Performance abilities

### Resolution Pipeline

1. **Trigger:** Skald activates any Performance ability
2. **Duration Calculation:** System calculates base duration (WILL)
3. **Passive Application:** Add +2 rounds from Enduring Performance
4. **Final Duration:** Performance lasts WILL + 2 rounds

### Worked Example

> **Skald with WILL 12:**
> 

> - Base Duration: 12 rounds
> 

> - Enduring Performance Bonus: +2 rounds
> 

> - **Final Duration:** 14 rounds
> 

> - **Impact:** ~17% duration increase (more impactful at lower WILL)
> 

### Edge Cases

- **Multiple Performances:** Bonus applies to each Performance individually
- **Interruption:** Duration extension doesn't prevent interruption effects
- **Stacking:** Stacks with other duration modifiers (Rank 2/3 upgrades)

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +2 rounds to all Performance durations

### Rank 2 (Expert — 20 PP)

- +3 rounds to all Performance durations
- **New:** First round of any Performance cannot be interrupted

### Rank 3 (Mastery — Capstone)

- +4 rounds to all Performance durations
- **New:** When a Performance ends naturally (not interrupted), Skald recovers 15 Stamina
- **New:** Performance effects linger for 1 round after ending

---

## V. Tactical Applications

1. **Extended Coverage:** Single activation covers longer fight segments
2. **Resource Efficiency:** Fewer re-activations needed, conserving Stamina
3. **Low-WILL Builds:** Compensates for builds that don't invest heavily in WILL
4. **Boss Fights:** Ensures Performance lasts through multi-phase encounters
5. **Exploration:** Maintain protective Performances during long dungeon traversals

---

## VI. Synergies & Interactions

### Positive Synergies

- **All Skald Performances:** Universal multiplier increases value of entire toolkit
- **High-WILL Builds:** Additive bonus stacks with already-long durations
- **Saga of the Einherjar (Capstone):** Extended duration maximizes party-wide buff uptime
- **Multi-Performance Skalds:** More value when maintaining multiple Performances

### Negative Synergies

- **Short Encounters:** Duration extension wasted if fights end quickly
- **Interrupt-heavy Enemies:** Bonus doesn't help if Performances get canceled early