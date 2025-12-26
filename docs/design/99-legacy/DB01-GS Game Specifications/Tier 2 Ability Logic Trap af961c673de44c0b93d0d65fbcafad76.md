# Tier 2 Ability: Logic Trap

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-THUL-LOGICTRAP-v5.0
Mechanical Role: Controller/Debuffer
Parent item: Thul (Jötun-Reader Diagnostician) — Specialization Specification v5.0 (Thul%20(J%C3%B6tun-Reader%20Diagnostician)%20%E2%80%94%20Specialization%206740a2ac8e2a4a4fafa8694c56818d48.md)
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
| **Specialization** | Thul (Jötun-Reader Diagnostician) |
| **Tier** | 2 (Advanced Oration) |
| **Type** | Active (Debuff) |
| **Prerequisite** | 8 PP spent in Thul tree |
| **Cost** | 50 Stamina |
| **Target** | Single **intelligent** enemy |
| **Duration** | 1 round |

---

## I. Design Context (Layer 4)

### Core Design Intent

Logic Trap is the Thul's **hard control ability**—a paradoxical question or flawed proposition that overloads an enemy's mind, causing them to freeze in confusion. This provides short but powerful crowd control.

### Mechanical Role

- **Primary:** Inflict [Stunned] on intelligent enemy for 1 round
- **Secondary:** Premium single-target hard CC
- **Targeting Restriction:** Only affects intelligent enemies
- **Fantasy Delivery:** The unsolvable riddle that breaks the mind

### Balance Considerations

- **Power Level:** Very high ([Stunned] is most powerful debuff)
- **Short Duration:** 1 round balances extreme power
- **Higher Cost:** 50 Stamina reflects premium control
- **WITS vs WITS:** Tests intelligence rather than willpower

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Thul poses a question—not a riddle for entertainment, but a **logical paradox** constructed with surgical precision. The question seems simple, but every answer leads to a contradiction. Every path of thought loops back on itself.

"If you strike me down, you prove my words true. If you do not, you prove yourself a coward. Which truth do you prefer?"

The target's mind **locks up**, caught in an endless loop of recursive logic. For a precious moment, they can do nothing but think—and think—and think.

### Thematic Resonance

The Thul weaponizes logic itself. In a world where corrupted code can crash reality, a sufficiently paradoxical statement can crash a mind.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 50 Stamina
- **Range:** 6 meters (voice range, requires attention)
- **Target:** Single **intelligent** enemy

### Resolution

**Opposed Check:**

- Thul rolls: **WITS + Rhetoric**
- Target rolls: **WITS** (Mental Defense)

### On Success

- Target gains **[Stunned]** for 1 round

### [Stunned] Effect

- Cannot take any actions (Standard, Move, Bonus, Reaction)
- Automatically fails Defense checks (or Defense reduced to minimum)
- Duration: 1 round (until end of target's next turn)

### Targeting Restriction

**Valid Targets (Intelligent):**

- Humanoids capable of logical thought
- Intelligent corrupted creatures
- Any entity that can process complex language

**Invalid Targets (Non-Intelligent):**

- Mindless Undying
- Blighted Beasts
- Constructs without reasoning capability
- Creatures that don't understand language

### Resolution Pipeline

1. **Targeting:** Thul selects intelligent enemy within 6 meters
2. **Attention Check:** Target must be able to hear and understand Thul
3. **Intelligence Check:** System verifies target is intelligent
4. **Cost Payment:** Thul spends 50 Stamina
5. **Opposed Roll:** Thul (WITS + Rhetoric) vs Target (WITS)
6. **Success/Failure:** On success, apply [Stunned]; on failure, no effect
7. **Duration Track:** [Stunned] expires at end of target's next turn

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Inflict [Stunned] for 1 round on intelligent enemy
- Opposed WITS + Rhetoric vs WITS check
- Cost: 50 Stamina

### Rank 2 (Expert — 20 PP)

- Inflict [Stunned] for 1 round + [Disoriented] for 1 additional round
- +1 bonus die to the opposed check
- Cost: 45 Stamina
- **New:** Range increased to 8 meters

### Rank 3 (Mastery — Capstone)

- Inflict [Stunned] for 2 rounds
- +2 bonus dice to the opposed check
- Cost: 40 Stamina
- **New:** Can affect semi-intelligent creatures
- **New:** If target breaks free early (via ally help), they still suffer [Disoriented] for 2 rounds

---

## V. Tactical Applications

1. **Hard CC:** Completely remove enemy from combat for 1 round
2. **Action Denial:** Prevent dangerous enemy from acting at critical moment
3. **Setup Window:** Create opening for party burst damage
4. **Interrupt Prevention:** [Stunned] enemy can't use Reactions
5. **Boss Control:** Even 1 round of boss inactivity is extremely valuable

---

## VI. Synergies & Interactions

### Positive Synergies

- **Burst damage dealers:** Stunned target can't defend; unload everything
- **Berserkr (Whirlwind):** Stunned target can't escape AoE positioning
- **Keeper of Sagas I:** Bonus die improves opposed check
- **The Sage's Insight:** [Analyzed] + [Stunned] = devastating combo

### Negative Synergies

- **Mindless enemies:** Completely ineffective
- **High-WITS targets:** May resist the opposed check
- **Multiple enemies:** Single-target only; less efficient vs groups
- **Deafened targets:** Cannot hear the logic trap