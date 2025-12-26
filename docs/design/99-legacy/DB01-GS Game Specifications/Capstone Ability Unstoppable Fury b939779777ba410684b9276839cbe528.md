# Capstone Ability: Unstoppable Fury

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-UNSTOPPABLEFURY-v5.0
Parent item: Berserkr (The Roaring Fire) — Specialization Specification v5.0 (Berserkr%20(The%20Roaring%20Fire)%20%E2%80%94%20Specialization%20Speci%20edde253a7f9d46ff8e936655162ef4f7.md)
Proof-of-Concept Flag: Yes
Sub-Type: Capstone
Template Validated: No
Voice Validated: No

## Core Identity

**Specialization:** Berserkr (The Roaring Fire)

**Ability ID:** 1109

**Tier:** Capstone (Ultimate Expression)

**Type:** Passive (Triggered + Permanent)

**Prerequisite:** 34 PP invested in Berserkr tree (all Tier 1-3 abilities)

**Cost:** 6 PP

---

## Description

You have become fury incarnate. Your rage transcends mere emotion and becomes a force of nature. Fear cannot touch you, and death itself recoils from the intensity of your wrath.

When your body should fail, your rage keeps you standing. For one desperate moment, you refuse to die—and in that refusal, you become more dangerous than ever.

---

## Mechanics

### Core Formulas

```jsx
Permanent Effects (always active):
  Rank 1: Immune to [Feared]
  Rank 2: Immune to [Feared], [Stunned]
  Rank 3: Immune to [Feared], [Stunned], death prevention

Death Prevention (Rank 3 only, once per combat):
  Trigger: HP reduced to 0
  Effect: HP = 1, Fury = 100
  Limit: Once per combat
```

### Progression & Milestones

**Rank 1 (Starting — when ability is learned)**

- **Permanent Immunity:** Cannot be affected by [Feared] effects
- **Design Note:** Immediately solves the -2 WILL vulnerability that plagues Berserkrs holding Fury

**Rank 2 (Expert — tree progression)**

- **Permanent Immunities:**
    - **Immune to [Feared]:** Cannot be affected by Fear effects
    - **Immune to [Stunned]:** Cannot be Stunned
- **Design Note:** Full action denial immunity — Berserkr can always act

**Rank 3 (Mastery — full tree completion)**

- **Permanent Immunities:** [Feared] and [Stunned] (as Rank 2)
- **Death Prevention (Once Per Combat):**
    - **Trigger:** The first time you would be reduced to 0 HP in combat
    - **Effect:** HP is set to 1 instead of 0
    - **Fury Surge:** Instantly gain 100 Fury (capped)
    - **Limit:** Can only trigger once per combat

---

## Worked Examples

### Example 1: Fear Immunity Value (Rank 1)

> **Situation:** Berserkr at 80 Fury (suffering -2 WILL penalty). Nightmare enemy uses mass Fear.
> 

> 
> 

> **Without Unstoppable Fury:** Roll WILL Resolve at -2 dice → likely failure → [Feared]
> 

> 
> 

> **With Unstoppable Fury R1:** **Immune** — Fear effect has no effect
> 

> 
> 

> **Result:** Even Rank 1 solves the Berserkr's biggest weakness.
> 

### Example 2: Full Action Freedom (Rank 2)

> **Situation:** Controller enemy attempts to [Stun] Berserkr to prevent Whirlwind of Destruction.
> 

> 
> 

> **With Unstoppable Fury R2:** **Immune** — Stun has no effect
> 

> 
> 

> **Result:** Berserkr cannot be locked out of actions. Always dangerous.
> 

### Example 3: Death Prevention Trigger (Rank 3)

> **Situation:** Berserkr at 15 HP. Boss crits for 40 damage.
> 

> 
> 

> **Normal result:** 15 - 40 = -25 HP → Unconscious/Dying
> 

> 
> 

> **With Unstoppable Fury R3:**
> 

> - Death Prevention triggers
> 

> - HP set to 1 (not 0)
> 

> - Fury instantly set to 100
> 

> 
> 

> **Next turn:** At 1 HP with 100 Fury:
> 

> - Death or Glory active (+5 damage, +75% Fury, 15% crit, +2 Defense)
> 

> - Blood-Fueled at maximum efficiency
> 

> - Can use Whirlwind (100 Fury available) or Hemorrhaging Strike
> 

> 
> 

> **Result:** "Killing blow" becomes power spike.
> 

### Example 4: The Ultimate Combo (Rank 3)

> **Situation:** Death Prevention just triggered. Berserkr at 1 HP, 100 Fury.
> 

> 
> 

> **Active Effects:**
> 

> - Death or Glory R3: +5 damage, +75% Fury gen, 18-20 crit, +2 Defense
> 

> - Primal Vigor R3: +16 Stamina/turn, +2 HP/turn (at 100 Fury)
> 

> - Blood-Fueled R3: 3× Fury from damage, 50% [Inspired], [Furious] on 30+ hit
> 

> - Immune to Fear and Stun
> 

> 
> 

> **Action:** Whirlwind of Destruction (45 Stamina, 25 Fury)
> 

> - 5d10 + MIGHT + 5 per target
> 

> - 15% crit chance per target
> 

> - [Bleeding] on all targets
> 

> 
> 

> **Result:** Most dangerous state possible. One bad turn for enemies.
> 

---

## Failure Modes

### Death Prevention Already Used (Rank 3)

- **Limit:** Once per combat only
- **Second lethal hit:** Normal death/unconsciousness
- **Strategy:** After trigger, play more carefully or end fight quickly

### Healed Above Bloodied

- **Death or Glory deactivates:** Lose damage/Fury/crit bonuses
- **Still have:** Fear/Stun immunity, Fury from trigger
- **Strategy:** Stay in danger zone if possible

### Killed Before Capstone

- **No death prevention:** Ranks 1-2 don't have the trigger
- **Strategy:** Prioritize completing tree for full protection

---

## Tactical Applications

### Permanent Weakness Coverage

Fear and Stun immunity permanently solves two major Berserkr vulnerabilities:

- **Fear:** No longer care about -2 WILL penalty
- **Stun:** Cannot be locked out of actions

### Boss Insurance (Rank 3)

Save death prevention for critical moments:

- Boss enrage phase → survive lethal hit → counter-attack with 100 Fury
- Turns potential wipe into comeback

### The 1 HP Power Spike (Rank 3)

After death prevention triggers, you're at maximum power:

- 100 Fury (all thresholds active)
- [Bloodied] (Death or Glory active)
- Fear/Stun immune
- One turn to deal massive damage before needing healing

### Resource Management (Rank 3)

100 Fury on trigger enables:

- Whirlwind of Destruction (25 Fury) + Hemorrhaging Strike (40 Fury) = 65 Fury spent
- Or: Multiple smaller abilities
- Or: Hold for next turn's Death or Glory amplification

### Rank Progression Value

| Rank | Immunities | Death Prevention | Value |
| --- | --- | --- | --- |
| **Rank 1** | [Feared] | No | Solves WILL weakness |
| **Rank 2** | [Feared], [Stunned] | No | Full action freedom |
| **Rank 3** | [Feared], [Stunned] | Yes (1/combat) | True unstoppable |

---

## Integration Notes

**Role:** Ultimate capstone. Removes Fear/Stun weakness, provides death insurance at Rank 3.

**Design Philosophy:**

- Rank 1 immediately addresses the core Berserkr vulnerability (-2 WILL + Fear)
- Rank 2 adds Stun immunity for complete action freedom
- Rank 3 adds the iconic "refuse to die" moment
- Once-per-combat limit prevents abuse
- Unlocking this also upgrades all Tier 1 and Tier 2 abilities to Rank 3

**Notable Synergies:**

- **Death or Glory** (Tier 3): **CRITICAL** — Always active at 1 HP after trigger
- **Blood-Fueled** (Tier 2): Maximum Fury generation while at 1 HP
- **Primal Vigor** (Tier 1): 100 Fury = +16 Stamina/turn + 2 HP/turn at R3
- **All damage abilities:** Benefit from Death or Glory bonuses at 1 HP

**Capstone Unlock Bonus:**

When Unstoppable Fury is trained, all Tier 1 and Tier 2 abilities automatically upgrade to Rank 3.

**Related Abilities:**

- Death or Glory (Tier 3): Automatically active at 1 HP
- Blood-Fueled (Tier 2): Enables rapid Fury recovery
- Primal Vigor (Tier 1): Sustain at 100 Fury
- All Fury spenders: Use the 100 Fury immediately