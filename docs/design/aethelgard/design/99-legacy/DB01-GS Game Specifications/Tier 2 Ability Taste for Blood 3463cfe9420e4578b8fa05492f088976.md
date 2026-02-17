# Tier 2 Ability: Taste for Blood

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-VARGRBORN-TASTEBLOOD-v5.0
Mechanical Role: Utility/Versatility
Parent item: Vargr-Born (Uncorrupted Predator) — Specialization Specification v5.0 (Vargr-Born%20(Uncorrupted%20Predator)%20%E2%80%94%20Specialization%203c9731930e1d4cef9c89565e7941ceac.md)
Proof-of-Concept Flag: No
Resource System: Charges/Uses
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Ability Overview

| Attribute | Value |
| --- | --- |
| **Specialization** | Vargr-Born (Uncorrupted Predator) |
| **Tier** | 2 (Advanced Predation) |
| **Type** | Passive (Resource Engine) |
| **Prerequisite** | 8 PP spent in Vargr-Born tree |
| **Cost** | None (Passive) |
| **Effect** | Generate bonus Fury from [Bleeding] damage |

---

## I. Design Context (Layer 4)

### Core Design Intent

Taste for Blood is the Vargr-Born's **core resource engine**—a passive that generates bonus Feral Fury whenever they deal [Bleeding] damage. This accelerates their hunting pattern by rewarding sustained bleed application.

### Mechanical Role

- **Primary:** Generate +5 Feral Fury whenever dealing [Bleeding] damage
- **Secondary:** Reward multi-target bleed application
- **Fantasy Delivery:** The predator's primal attunement to blood

### Balance Considerations

- **Power Level:** High (significant resource acceleration)
- **Bleed Dependency:** Only functions with active [Bleeding] effects
- **Multi-Target Scaling:** More bleeds = more Fury
- **Sustain Enabler:** Allows extended high-damage rotations

---

## II. Narrative Context (Layer 2)

### In-World Framing

The scent of blood is intoxicating. Not in a corrupted, Blighted way—but in the pure, primal way of the wolf who has caught their prey. Each drop of blood that hits the ground, each tick of damage from a bleeding wound, sends a pulse of energy through the Vargr-Born's primal spirit.

They can *feel* the life draining from their prey. And that feeling is **fuel**.

### Thematic Resonance

Taste for Blood is the Vargr-Born's predatory instincts made mechanical. The hunt sustains them; the blood empowers them. This is not corruption—it is the natural function of an apex predator.

---

## III. Mechanical Specification (Layer 3)

### Passive Effect

**Trigger:** Whenever the Vargr-Born deals damage with a [Bleeding] effect

| Trigger Event | Fury Generated |
| --- | --- |
| Initial bleed application | No bonus (handled by attack) |
| Each bleed DoT tick | +5 Feral Fury |
| Multiple bleeds ticking | +5 per bleed per tick |

### Example Scenario

- 3 enemies with [Bleeding]
- At start of their turns, each takes bleed damage
- Vargr-Born gains 5 + 5 + 5 = 15 Feral Fury per round from bleed ticks alone

### Resolution Pipeline

1. **Bleed Tick:** Enemy with [Bleeding] takes DoT damage
2. **Source Check:** Confirm bleed was applied by this Vargr-Born
3. **Fury Generation:** Vargr-Born gains 5 Feral Fury
4. **Repeat:** For each bleed tick from any target

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- +5 Feral Fury per bleed tick
- Applies to all [Bleeding] effects applied by Vargr-Born

### Rank 2 (Expert — 20 PP)

- +7 Feral Fury per bleed tick
- **New:** Also gain +3 Fury when an enemy dies while [Bleeding]

### Rank 3 (Mastery — Capstone)

- +10 Feral Fury per bleed tick
- +5 Fury when enemy dies while [Bleeding]
- **New:** First bleed application each combat generates double Fury
- **New:** While at 80+ Fury, [Bleeding] effects deal +1d6 bonus damage

---

## V. Tactical Applications

1. **Fury Acceleration:** Dramatically speeds up Fury generation
2. **Multi-Target Reward:** Bleeds on multiple enemies compound Fury
3. **Sustained Offense:** Enables repeated finisher use
4. **Passive Income:** Fury generates without spending actions
5. **Kill Bonus:** Extra Fury from eliminating bleeding targets

---

## VI. Synergies & Interactions

### Positive Synergies

- **Savage Claws:** Primary bleed applicator feeds this passive
- **Feral Maelstrom (Tier 3):** AoE bleed = multiple Fury sources
- **Go for the Throat:** Sustained Fury enables repeated finishers
- **Aspect of the Great Wolf (Capstone):** High Fury sustains ultimate

### Negative Synergies

- **Bleed-immune enemies:** No bleeds = no passive value
- **Short fights:** Insufficient time for bleed ticks
- **Single-target focus:** Less value than multi-target bleeding