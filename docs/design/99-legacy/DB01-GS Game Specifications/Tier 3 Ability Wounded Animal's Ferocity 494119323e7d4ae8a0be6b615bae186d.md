# Tier 3 Ability: Wounded Animal's Ferocity

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-VARGRBORN-WOUNDEDFEROCITY-v5.0
Mechanical Role: Damage Dealer, Tank/Durability
Parent item: Vargr-Born (Uncorrupted Predator) — Specialization Specification v5.0 (Vargr-Born%20(Uncorrupted%20Predator)%20%E2%80%94%20Specialization%203c9731930e1d4cef9c89565e7941ceac.md)
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
| **Specialization** | Vargr-Born (Uncorrupted Predator) |
| **Tier** | 3 (Mastery of the Hunt) |
| **Type** | Passive (Survivability) |
| **Prerequisite** | 20 PP spent in Vargr-Born tree |
| **Cost** | None (Passive) |
| **Trigger** | Active while [Bloodied] (below 50% HP) |

---

## I. Design Context (Layer 4)

### Core Design Intent

Wounded Animal's Ferocity is the Vargr-Born's **low-HP power spike**—a passive that transforms near-defeat into increased danger. This inverts the typical "low HP = retreat" dynamic into "low HP = attack harder."

### Mechanical Role

- **Primary:** Reduce Stamina costs by 10 while [Bloodied]
- **Secondary:** Deal bonus damage while [Bloodied]
- **Trigger:** Automatically active when below 50% HP
- **Fantasy Delivery:** The cornered wolf fights hardest

### Balance Considerations

- **Power Level:** High (significant resource + damage buff)
- **Risk/Reward:** Must take damage to activate
- **Sustain Concern:** Encourages aggressive play at low HP
- **No Action Cost:** Passive; always active when triggered

---

## II. Narrative Context (Layer 2)

### In-World Framing

A cornered wolf is at its most dangerous.

When the Vargr-Born bleeds, when their body screams with pain and their vision blurs at the edges, something *ancient* takes over. Their survival instincts, honed by generations of primal bloodline, sharpen to a killing edge. Pain becomes fuel. Fear becomes fury. Every strike is faster, harder, more vicious.

They are not retreating. They are not dying. They are becoming **more**.

### Thematic Resonance

Wounded Animal's Ferocity is the Vargr-Born transcending their human limitations through primal instinct. When civilization would tell them to surrender, the wolf spirit tells them to **fight**.

---

## III. Mechanical Specification (Layer 3)

### Trigger Condition

**[Bloodied]:** Current HP is below 50% of maximum HP

### Passive Effects (While [Bloodied])

| Effect | Value |
| --- | --- |
| **Stamina Reduction** | All attacks cost 10 less Stamina |
| **Bonus Damage** | All attacks deal +1d6 bonus Physical damage |

### Resolution Pipeline

1. **HP Check:** System monitors Vargr-Born's current HP
2. **Bloodied Trigger:** When HP drops below 50%, activate passive
3. **Cost Reduction:** Apply -10 Stamina to all attack costs
4. **Damage Bonus:** Add +1d6 Physical to all attack damage
5. **Deactivation:** If healed above 50% HP, passive deactivates

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- -10 Stamina cost to all attacks while [Bloodied]
- +1d6 bonus Physical damage while [Bloodied]

### Rank 2 (Expert — 20 PP)

- -15 Stamina cost to all attacks while [Bloodied]
- +2d6 bonus Physical damage while [Bloodied]
- **New:** +1 Defense while [Bloodied]

### Rank 3 (Mastery — Capstone)

- -20 Stamina cost to all attacks while [Bloodied]
- +3d6 bonus Physical damage while [Bloodied]
- +2 Defense while [Bloodied]
- **New:** First attack each round while [Bloodied] has +15% critical chance
- **New:** Regenerate 5 HP at start of turn while [Bloodied]

---

## V. Tactical Applications

1. **Power Spike:** Become more dangerous when wounded
2. **Stamina Efficiency:** Extended combat at low HP
3. **Healer Coordination:** Communicate to stay [Bloodied] for buff
4. **Risk/Reward Play:** Intentionally take damage for power
5. **Last Stand:** Maximum effectiveness when near defeat

---

## VI. Synergies & Interactions

### Positive Synergies

- **Savage Claws:** Reduced cost + bonus damage = efficient attack
- **Go for the Throat:** Reduced Stamina cost enables more finishers
- **Bone-Setter:** Precise healing keeps Vargr-Born at optimal [Bloodied] HP
- **Aspect of the Great Wolf (Capstone):** Combined power spike devastating

### Negative Synergies

- **Full healing:** Overheal removes the passive benefit
- **One-shot threats:** Risk of death before utilizing buff
- **Cautious playstyles:** Requires embracing low-HP danger