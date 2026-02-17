# Tier 3 Ability: Aegis of the Clan

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKJALDMAER-AEGISCLAN-v5.0
Mechanical Role: Support/Healer, Tank/Durability
Parent item: Skjaldmær (Bastion of Coherence) — Specialization Specification v5.0 (Skjaldm%C3%A6r%20(Bastion%20of%20Coherence)%20%E2%80%94%20Specialization%20%2083c338d903f54a5692dbaa63a5cf7b07.md)
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
| **Specialization** | Skjaldmær (Bastion of Coherence) |
| **Tier** | 3 (Mastery) |
| **Type** | Passive (Reactive) |
| **Prerequisite** | 20 PP spent in Skjaldmær tree |
| **Cost** | None (automatic trigger) |
| **Trigger** | Ally's Psychic Stress enters "High" threshold |
| **Frequency** | Once per ally per combat |

---

## I. Design Context (Layer 4)

### Core Design Intent

Aegis of the Clan is the Skjaldmær's **automatic clutch save**—a passive that triggers Oath of the Protector for free when an ally hits dangerous Psychic Stress levels. This rewards attentive play and reinforces the "sanity anchor" identity.

### Mechanical Role

- **Primary:** Auto-apply Oath of the Protector when ally reaches High Psychic Stress
- **Secondary:** Free action economy—protection without spending actions
- **Fantasy Delivery:** Protective instincts so ingrained they activate automatically

### Balance Considerations

- **Power Level:** High (free ability application)
- **Limitation:** Once per ally per combat
- **Trigger Condition:** Requires ally to actually reach High Stress threshold
- **Dependency:** Requires Oath of the Protector to be unlocked

---

## II. Narrative Context (Layer 2)

### In-World Framing

The Skjaldmær's protective instincts have become **second nature**. She doesn't consciously decide to protect a struggling ally—her body moves on its own, her oath spoken before she realizes it. The sight of a fracturing mind triggers an overwhelming need to shield them.

### Thematic Resonance

True protection isn't reactive—it's anticipatory. The Skjaldmær doesn't wait for an ally to ask for help; she sees the warning signs and acts. Aegis of the Clan is the culmination of a lifetime of training to recognize when others need her.

---

## III. Mechanical Specification (Layer 3)

### Trigger Condition

- **When:** An ally's Psychic Stress meter first enters the "High" threshold during combat
- **High Threshold:** Typically 60-80% of maximum Psychic Stress (varies by system)

### Effect

- Skjaldmær automatically applies **Oath of the Protector** to that ally
- **Duration:** 1 turn (reduced from standard 2 turns)
- **Cost:** Free (no Stamina expenditure)
- **Action Economy:** Does not consume Skjaldmær's action

### Oath of the Protector Effect (Reminder)

- +2 Soak
- +1 bonus die to Psychic Stress Resolve Checks

### Resolution Pipeline

1. **Stress Monitoring:** System tracks ally Psychic Stress levels
2. **Threshold Detection:** Ally's Stress crosses into "High" range
3. **First-Time Check:** Verify this is first time this ally has triggered this combat
4. **Auto-Application:** Oath of the Protector applied to ally for 1 turn
5. **No Cost Deduction:** Skjaldmær's Stamina is not reduced
6. **Tracking Update:** Mark ally as having triggered Aegis this combat

### Limitations

- **Once per ally per combat:** Cannot trigger multiple times for same ally
- **Reduced duration:** 1 turn instead of standard 2 turns
- **Requires Oath of the Protector:** If Oath is not unlocked, this passive does nothing
- **Not Cumulative:** If Oath is already active on target, effect is wasted (doesn't extend duration)

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- Auto-apply Oath of the Protector (1 turn) when ally hits High Stress
- Once per ally per combat
- Free

### Rank 2 (Expert — 20 PP)

- Auto-apply Oath of the Protector (2 turns) when ally hits High Stress
- Once per ally per combat
- **New:** Also grants ally 5 temporary HP when triggered

### Rank 3 (Mastery — Capstone)

- Auto-apply Oath of the Protector (3 turns) when ally hits High Stress
- Twice per ally per combat
- Grants ally 10 temporary HP when triggered
- **New:** If Skjaldmær also has Bastion of Sanity capstone, Aegis can prevent Trauma once per combat (shared cooldown)

---

## V. Tactical Applications

1. **Emergency Protection:** Automatic safety net when allies hit dangerous Stress levels
2. **Action Economy Savings:** Free protection without spending Standard Actions
3. **Multi-ally Coverage:** Can trigger for each ally once per combat
4. **Stress Prevention:** Resolve bonus helps ally avoid further Stress accumulation
5. **High-Stress Builds:** Enables allies to operate in High Stress range more safely

---

## VI. Synergies & Interactions

### Positive Synergies

- **Jötun-Reader:** High self-inflicted Stress makes Aegis trigger reliably
- **Berserkr:** Low WILL means faster Stress accumulation; Aegis catches them
- **Skald (Saga of the Einherjar):** Termination Stress may trigger Aegis for multiple allies
- **Bone-Setter:** Combined with Bone-Setter healing, creates robust Stress management

### Negative Synergies

- **Low-Stress encounters:** May never trigger
- **Parties with strong Stress management:** May not reach High threshold
- **Multiple simultaneous triggers:** Can only protect one ally at a time (others must wait)