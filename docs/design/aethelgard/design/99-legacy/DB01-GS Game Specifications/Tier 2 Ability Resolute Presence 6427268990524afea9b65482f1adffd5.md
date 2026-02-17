# Tier 2 Ability: Resolute Presence

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-THUL-RESOLUTEPRESENCE-v5.0
Mechanical Role: Controller/Debuffer, Support/Healer
Parent item: Thul (Jötun-Reader Diagnostician) — Specialization Specification v5.0 (Thul%20(J%C3%B6tun-Reader%20Diagnostician)%20%E2%80%94%20Specialization%206740a2ac8e2a4a4fafa8694c56818d48.md)
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
| **Specialization** | Thul (Jötun-Reader Diagnostician) |
| **Tier** | 2 (Advanced Oration) |
| **Type** | Passive (Aura) |
| **Prerequisite** | 8 PP spent in Thul tree |
| **Cost** | None (Passive) |
| **Range** | Same row as Thul |

---

## I. Design Context (Layer 4)

### Core Design Intent

Resolute Presence is the Thul's **defensive aura**—a passive that provides Fear resistance to allies in the same row. This establishes the Thul as a stabilizing presence, their cold logic serving as an anchor against panic.

### Mechanical Role

- **Primary:** Allies in same row gain +1 die to WILL-based Resolve Checks vs [Fear]
- **Secondary:** Positional play reward (row-based aura)
- **Fantasy Delivery:** The calm, analytical presence that cuts through terror

### Balance Considerations

- **Power Level:** Moderate (conditional bonus die)
- **Position Requirement:** Same row as Thul
- **Comparison:** Similar to Skjaldmær's Bastion of Sanity but less powerful and earlier access

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Thul doesn't panic. When the Forlorn screams and lesser warriors break, the Thul stands unmoved, analyzing the threat with detached curiosity. This calm is **contagious**—allies near the Thul find their own terror dampened by proximity to someone so utterly unaffected.

"It's trying to frighten us," the Thul observes, voice flat. "Observe how predictable the behavior pattern is. It will feint left, then strike right. Yawn."

Somehow, the boredom in their voice makes the terror less... terrifying.

### Thematic Resonance

Fear is irrational. The Thul is the embodiment of rationality. Their mere presence is a reminder that fear can be analyzed, understood, and dismissed as the primitive response it is.

---

## III. Mechanical Specification (Layer 3)

### Effect

**All allies in the same row as the Thul gain:**

- **+1 bonus die (+1d10)** to **WILL-based Resolve Checks** against **[Fear]** effects

### Position Requirement

- Thul must be conscious and in a specific row
- Allies must be in the **same row** as the Thul
- Dynamic: If Thul or ally moves to different row, bonus is lost/gained accordingly

### Resolution Pipeline

1. **Row Detection:** System identifies Thul's current row
2. **Ally Identification:** System identifies all allies in same row
3. **Fear Check Trigger:** Ally in same row must make [Fear] Resolve Check
4. **Bonus Application:** Add +1d10 to ally's WILL-based Resolve Check
5. **Resolution:** Proceed with standard Resolve Check resolution

### Edge Cases

- **Thul [Unconscious]:** Aura deactivates
- **Multiple Thuls:** Bonuses do not stack (one aura per row)
- **Row Change Mid-Combat:** Bonus immediately updates based on new positions

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +1 bonus die to [Fear] Resolve Checks for allies in same row

### Rank 2 (Expert — 20 PP)

- +2 bonus dice to [Fear] Resolve Checks for allies in same row
- **New:** Also applies to [Disoriented] Resolve Checks

### Rank 3 (Mastery — Capstone)

- +2 bonus dice to [Fear], [Disoriented], and [Confused] Resolve Checks
- **New:** Range expanded to include adjacent rows
- **New:** First [Fear] effect per combat on any ally in range is automatically resisted

---

## V. Tactical Applications

1. **Formation Anchor:** Thul becomes stabilizing point for fear-prone allies
2. **Forlorn Counter:** Essential against fear-inducing enemies
3. **Berserkr Support:** Low-WILL Berserkr benefits massively
4. **Position Incentive:** Encourages tactical row grouping
5. **Passive Value:** Always active without action cost

---

## VI. Synergies & Interactions

### Positive Synergies

- **Berserkr:** Low WILL makes Fear resistance extremely valuable
- **Skjaldmær (Sanctified Resolve):** Stacking Fear resistance creates near-immunity
- **Skald (Saga of Courage):** Multiple Fear resistance sources
- **Front-line positioning:** Same row as melee allies provides maximum coverage

### Negative Synergies

- **Spread formations:** Allies in different rows don't benefit
- **Back-row Thul:** May not be in same row as allies who need Fear resistance
- **Fear-light encounters:** Passive is wasted if enemies don't use Fear