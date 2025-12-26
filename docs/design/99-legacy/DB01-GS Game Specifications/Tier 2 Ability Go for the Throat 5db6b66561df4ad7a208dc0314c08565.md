# Tier 2 Ability: Go for the Throat

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-VARGRBORN-GOFORTHROAT-v5.0
Mechanical Role: Burst Damage, Damage Dealer
Parent item: Vargr-Born (Uncorrupted Predator) — Specialization Specification v5.0 (Vargr-Born%20(Uncorrupted%20Predator)%20%E2%80%94%20Specialization%203c9731930e1d4cef9c89565e7941ceac.md)
Proof-of-Concept Flag: No
Resource System: Charges/Uses, Stamina
Sub-Type: Active
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
| **Type** | Active (Finisher Attack) |
| **Prerequisite** | 8 PP spent in Vargr-Born tree |
| **Cost** | 45 Stamina + 30 Feral Fury |
| **Target** | Single enemy (melee range) |
| **Effect** | High damage; bonus crit vs [Bleeding]/[Feared] |

---

## I. Design Context (Layer 4)

### Core Design Intent

Go for the Throat is the Vargr-Born's **finisher ability**—the killing strike that ends the hunt. This rewards the setup/execute gameplay loop by granting massive bonus critical chance against targets suffering from [Bleeding] or [Feared].

### Mechanical Role

- **Primary:** Deal high Physical damage to single target
- **Secondary:** Significantly increased crit chance vs [Bleeding] or [Feared] targets
- **Cost:** High (Stamina + Fury investment)
- **Fantasy Delivery:** The patient hunter's killing strike

### Balance Considerations

- **Power Level:** Very high (finisher damage)
- **Setup Required:** Best value requires [Bleeding] or [Feared] on target
- **High Resource Cost:** 45 Stamina + 30 Fury is significant investment
- **Single-Target Focus:** Reinforces predator-style hunting pattern

---

## II. Narrative Context (Layer 2)

### In-World Framing

The prey is weakened. Bleeding, terrified, its defenses crumbling. The Vargr-Born has been patient, has applied pressure, has waited for this moment. Now they strike.

They move with impossible speed, their entire body a weapon aimed at the most vulnerable point—the **throat**. The impact is devastating. Claws sink into soft tissue, fangs find purchase, and the prey's life ends in a spray of crimson.

This is not rage. This is not frenzy. This is the cold, calculated execution of a predator who has done this a thousand times.

### Thematic Resonance

Go for the Throat is the culmination of the Vargr-Born's hunting pattern. They stalk, they wound, they terrify—and then they **kill**.

---

## III. Mechanical Specification (Layer 3)

### Activation

- **Action Type:** Standard Action
- **Cost:** 45 Stamina + 30 Feral Fury
- **Range:** Melee (adjacent tile)
- **Target:** Single enemy

### Effect

**Base Damage:** Deal **5d10 + MIGHT** Physical damage

**Conditional Critical Bonus:**

| Target Condition | Critical Chance Bonus |
| --- | --- |
| [Bleeding] | +25% crit chance |
| [Feared] | +25% crit chance |
| Both [Bleeding] AND [Feared] | +50% crit chance |

### Critical Hit Effect

- Double damage dice (10d10 + MIGHT)
- Apply [Hemorrhaging] (cannot be healed for 3 rounds)

### Resolution Pipeline

1. **Cost Payment:** Spend 45 Stamina + 30 Feral Fury
2. **Condition Check:** Determine if target is [Bleeding] and/or [Feared]
3. **Crit Bonus:** Apply appropriate critical chance bonus
4. **Attack Roll:** MIGHT-based attack vs target Defense
5. **Damage Application:** Deal 5d10 + MIGHT (or 10d10 + MIGHT on crit)
6. **Critical Effect:** If crit, apply [Hemorrhaging]

---

## IV. Progression Path

### Rank 1 (Base — This Ability)

- 5d10 + MIGHT Physical damage
- +25% crit vs [Bleeding] or [Feared]; +50% vs both
- Cost: 45 Stamina + 30 Feral Fury

### Rank 2 (Expert — 20 PP)

- 6d10 + MIGHT Physical damage
- +30% crit vs [Bleeding] or [Feared]; +60% vs both
- Cost: 40 Stamina + 25 Feral Fury
- **New:** Critical hits restore 15 Feral Fury

### Rank 3 (Mastery — Capstone)

- 7d10 + MIGHT Physical damage
- +40% crit vs [Bleeding] or [Feared]; +75% vs both
- Cost: 35 Stamina + 20 Feral Fury
- Critical hits restore 20 Feral Fury
- **New:** If target dies to this attack, next ability costs no Fury
- **New:** [Hemorrhaging] from critical cannot be cleansed

---

## V. Tactical Applications

1. **Finisher:** Execute weakened priority targets
2. **Setup Payoff:** Maximize value after applying bleed/fear
3. **Boss Damage:** High burst against single tough enemy
4. **Execution Combo:** Bleed → Fear → Throat for maximum crit chance
5. **Momentum Swing:** Kills can restore Fury for continued assault

---

## VI. Synergies & Interactions

### Positive Synergies

- **Savage Claws:** Apply [Bleeding] to enable crit bonus
- **Terrifying Howl:** Apply [Feared] to enable crit bonus
- **Taste for Blood (Tier 2):** Bleed ticks generate Fury for Throat cost
- **Wounded Animal's Ferocity (Tier 3):** Reduced Stamina cost when [Bloodied]

### Negative Synergies

- **Un-setup targets:** Much weaker without [Bleeding]/[Feared]
- **High resource cost:** Fury spent here cannot be used on Howl
- **Bleed/Fear immune enemies:** Loses conditional bonus entirely