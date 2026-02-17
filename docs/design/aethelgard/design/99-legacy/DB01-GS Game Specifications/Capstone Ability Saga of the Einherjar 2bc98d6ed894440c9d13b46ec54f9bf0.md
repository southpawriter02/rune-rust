# Capstone Ability: Saga of the Einherjar

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKALD-SAGAEINHERJAR-v5.0
Mechanical Role: Damage Dealer, Support/Healer
Parent item: Skald (Chronicler of Coherence) — Specialization Specification v5.0 (Skald%20(Chronicler%20of%20Coherence)%20%E2%80%94%20Specialization%20S%203faadeffffc94a9fb7f3ce1e643ad740.md)
Proof-of-Concept Flag: No
Resource System: Corruption/Psychic Stress, Stamina
Sub-Type: Capstone
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Skald (Chronicler of Coherence) |
| **Tier** | Capstone (Ultimate Expression) |
| **Type** | Performance (Channeled) |
| **Prerequisite** | 40 PP spent in Skald tree |
| **Cost** | 70 Stamina (activation) |
| **Duration** | Rounds equal to WILL score |
| **Frequency** | Once per combat |

---

## I. Design Context (Layer 4)

### Core Design Intent

Saga of the Einherjar is the Skald's **ultimate combat buff**—a legendary Performance that transforms allies into quasi-mythical warriors at the cost of Psychic Stress when it ends. This is the "hero moment" ability that can turn the tide of desperate battles.

### Mechanical Role

- **Primary:** Party-wide [Inspired] status (+3 damage dice) and temporary HP
- **Secondary:** High-risk, high-reward Trauma Economy interaction
- **Fantasy Delivery:** The saga so powerful it lets mortals fight like the legendary dead

### Balance Considerations

- **Power Level:** Extremely high (+3 damage dice is massive)
- **Once Per Combat:** Cannot be spammed
- **Delayed Cost:** Psychic Stress occurs when Performance ends, not during
- **Counterplay:** Interrupting the Performance still triggers the Stress cost

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skald speaks the **Saga of the Einherjar**—the tale of the chosen dead who rose again to fight the corruption. As they speak, the veil between reality and legend thins. Allies don't just hear the story; they *become* the Einherjar, their bodies infused with the echo of ancient warriors. For a glorious moment, they fight with the fury and skill of the deathless.

But such power has a price. When the saga ends, the sudden return to mortal frailty is psychically devastating.

### Thematic Resonance

The Einherjar are Aethelgard's greatest legend—warriors who transcended death to fight the eternal battle. The Saga doesn't summon them; it convinces reality that the listeners *are* them, temporarily. The Psychic Stress represents the trauma of that identity dissolution when the song ends.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 70 Stamina
- **Frequency:** Once per combat
- **Effect:** Skald enters [Performing] status

### Performance Effect (While Active)

**All allies (including Skald) gain:**

1. **[Inspired] Status:** +3 bonus dice (+3d10) to all damage rolls
2. **Temporary HP:** Gain temporary HP equal to `10 + Skald's WILL`

### Duration & Maintenance

- **Base Duration:** Rounds equal to Skald's WILL score
- **With Enduring Performance:** WILL + 2 rounds
- **Restriction:** Cannot use another Standard Action while [Performing]

### Termination Cost

**When Performance ends (by any means):**

- All affected allies immediately suffer **8 Psychic Stress**
- This includes the Skald
- Occurs whether Performance ended naturally, by choice, or by interruption

### Interruption Conditions

Performance ends immediately if:

- Skald becomes [Stunned]
- Skald becomes [Silenced]
- Skald becomes [Unconscious]
- Duration expires
- Skald chooses to end Performance (Free Action)

### Resolution Pipeline

1. **Activation:** Skald spends 70 Stamina, declares Saga of the Einherjar
2. **Frequency Check:** Verify not already used this combat
3. **Status Application:** Skald gains [Performing: Saga of the Einherjar]
4. **Party Buff:** All allies gain [Inspired] and temporary HP
5. **Maintenance:** Each round, check duration and interruption conditions
6. **Termination:** When Performance ends, apply 8 Psychic Stress to all affected allies
7. **Cleanup:** Remove [Inspired] and remaining temporary HP

### Worked Example

> **Skald with WILL 13, party of 4:**
> 

> 
> 

> **Activation:**
> 

> - Cost: 70 Stamina
> 

> - Duration: 13 rounds (or 15 with Enduring Performance)
> 

> 
> 

> **During Performance:**
> 

> - Each ally: +3d10 damage, +23 temporary HP
> 

> - Berserkr attack: normally 3d10 → now 6d10 damage
> 

> 
> 

> **Termination:**
> 

> - All 4 party members take 8 Psychic Stress
> 

> - Bone-Setter or Skald's other abilities should prepare for this
> 

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- [Inspired]: +3 damage dice
- Temporary HP: 10 + WILL
- Termination Cost: 8 Psychic Stress to all
- Once per combat

### Rank 2 (Expert — 20 PP)

- [Inspired]: +4 damage dice
- Temporary HP: 15 + WILL
- Termination Cost: 6 Psychic Stress to all
- **New:** Allies also gain +1 bonus die to Accuracy while [Inspired]

### Rank 3 (Mastery — Capstone)

- [Inspired]: +5 damage dice
- Temporary HP: 20 + WILL
- Termination Cost: 4 Psychic Stress to all
- Allies gain +2 bonus dice to Accuracy while [Inspired]
- **New:** If any ally would be reduced to 0 HP during Performance, they instead stay at 1 HP and gain [Deathless] (cannot die) until Performance ends
- **New:** Can use twice per combat (second use has no termination Stress cost)

---

## V. Tactical Applications

1. **Boss Burst:** Activate at start of boss fight to maximize damage window
2. **Desperation Play:** When party is about to wipe, enable massive counterattack
3. **Planned Stress Management:** Pre-position Bone-Setter to heal Stress immediately after
4. **Temporary HP Buffer:** Absorb a wave of enemy attacks with temporary HP
5. **Timed Termination:** End Performance after threats are eliminated to control Stress timing

---

## VI. Synergies & Interactions

### Positive Synergies

- **Bone-Setter:** Essential for healing the termination Psychic Stress
- **Berserkr:** Massive damage bonuses multiply Berserkr's already-high output
- **Saga of Courage:** Active before Saga of the Einherjar to help resist termination Stress
- **High-damage Builds:** +3 dice is most valuable for already-strong attackers
- **Skjaldmær (Bastion of Sanity):** Can absorb Stress for one ally via capstone

### Negative Synergies

- **Low-WILL Parties:** Termination Stress may cause Trauma in vulnerable characters
- **Solo Play:** Full value requires multiple damage-dealers
- **Extended Fights:** Once-per-combat limits sustained value
- **Parties without Stress Management:** Termination cost can be devastating