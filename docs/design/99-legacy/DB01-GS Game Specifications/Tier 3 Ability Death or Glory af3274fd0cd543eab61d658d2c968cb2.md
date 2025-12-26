# Tier 3 Ability: Death or Glory

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-BERSERKR-DEATHORGLORY-v5.0
Parent item: Berserkr (The Roaring Fire) — Specialization Specification v5.0 (Berserkr%20(The%20Roaring%20Fire)%20%E2%80%94%20Specialization%20Speci%20edde253a7f9d46ff8e936655162ef4f7.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 3
Template Validated: No
Voice Validated: No

## Core Identity

**Specialization:** Berserkr (The Roaring Fire)

**Ability ID:** 1108

**Tier:** 3 (Ultimate Fury)

**Type:** Passive (Always Active when Bloodied)

**Prerequisite:** 20 PP invested in Berserkr tree

**Cost:** 5 PP

---

## Description

When brought to the brink of defeat, when most warriors would falter or retreat, the Berserkr becomes **more dangerous than ever**. The closer they are to death, the harder they fight. The pain, the exhaustion, the proximity to death—all of it fuels a desperate, ferocious intensity.

This is the Berserkr's refusal to accept defeat. The [Bloodied] threshold (50% HP) is not a warning sign—it is an **activation trigger**.

---

## Mechanics

### Core Formulas

```jsx
Trigger: HP ≤ 50% MaxHP → [Bloodied] → Death or Glory activates

By Rank:
  Rank 2: +3 damage, +50% Fury gen, 19-20 crit (10%)
  Rank 3: +5 damage, +75% Fury gen, 18-20 crit (15%), +2 Defense
```

### Progression & Milestones

**Rank 2 (Starting — when ability is learned)**

- **Trigger:** Automatically activates when HP drops to 50% or below ([Bloodied])
- **Damage Bonus:** +3 to all damage rolls
- **Fury Generation:** +50% Fury from all sources
- **Critical Range:** 19-20 on d20 (10% crit chance)

**Rank 3 (Mastery — Capstone trained)**

- **Damage Bonus:** +5 to all damage rolls (increased from +3)
- **Fury Generation:** +75% Fury from all sources (increased from +50%)
- **Critical Range:** 18-20 on d20 (15% crit chance)
- **Defense Bonus:** +2 Defense (new)

---

## Worked Examples

### Example 1: Fury Explosion (Rank 2)

> **Situation:** Berserkr at 48% HP ([Bloodied]), has Blood-Fueled Rank 2. Takes 25 damage.
> 

> 
> 

> **Base Fury from damage:** 25 HP
> 

> **Blood-Fueled (2×):** 25 × 2 = 50 Fury
> 

> **Death or Glory (+50%):** 50 × 1.50 = **75 Fury**
> 

> 
> 

> **Result:** Single hit while [Bloodied] with Blood-Fueled = near Fury cap.
> 

### Example 2: Maximum Fury Engine (Rank 3)

> **Situation:** Berserkr at 45% HP ([Bloodied]), has Blood-Fueled Rank 3. Takes 30 damage.
> 

> 
> 

> **Base Fury from damage:** 30 HP
> 

> **Blood-Fueled (3×):** 30 × 3 = 90 Fury
> 

> **Death or Glory (+75%):** 90 × 1.75 = **157.5 → 100 Fury (capped)**
> 

> 
> 

> **Result:** Single hit while [Bloodied] with Blood-Fueled R3 = instant Fury cap.
> 

### Example 3: Damage Amplification (Rank 3)

> **Situation:** [Bloodied] Berserkr uses Whirlwind of Destruction vs 4 enemies. MIGHT 6.
> 

> 
> 

> **Per-target damage:**
> 

> - Base: 5d10 + MIGHT = ~33
> 

> - Death or Glory: +5
> 

> - **Total per target:** ~38 damage
> 

> 
> 

> **4 targets:** 38 × 4 = **152 damage** (vs 132 without Death or Glory)
> 

> 
> 

> **Crit potential:** 15% chance per target = ~0.6 expected crits for bonus damage spikes.
> 

### Example 4: Survival Through Offense (Rank 3)

> **Situation:** [Bloodied] Berserkr being focused by enemies.
> 

> 
> 

> **Defense Bonus:** +2 Defense makes attacks less likely to hit
> 

> 
> 

> **Combined with Primal Vigor:** High Fury = high Stamina regen
> 

> 
> 

> **Combined with [Inspired]:** 50% chance from Blood-Fueled damage = Fear immunity
> 

> 
> 

> **Result:** [Bloodied] Berserkr is actually harder to kill than healthy Berserkr.
> 

---

## Failure Modes

### Healed Above 50%

- **Effect ends:** Death or Glory deactivates when HP > 50%
- **Strategy:** Communicate with healer to maintain 40-50% HP range

### Burst Damage While Bloodied

- **Death risk:** Low HP + big hit = death
- **Mitigation:** Unstoppable Fury (Capstone) provides safety net at 1 HP

### Short Encounters

- **Insufficient time:** May not drop to [Bloodied] before combat ends
- **Strategy:** Use Reckless Assault [Vulnerable] to accelerate HP loss

---

## Tactical Applications

### Optimal HP Range

Maintain 40-50% HP sweet spot:

- **Above 50%:** Inactive, no bonuses
- **40-50%:** Full bonuses, survivable
- **Below 30%:** High death risk

### Controlled Descent

Intentionally drop into [Bloodied]:

1. Use Reckless Assault → [Vulnerable] (+15-25% damage taken)
2. Let enemy hit you → Blood-Fueled generates massive Fury
3. Death or Glory activates → damage/Fury/crit bonuses

### Fury Math with Blood-Fueled

| Damage Taken | Blood-Fueled R2 Only | + Death or Glory R2 | + Death or Glory R3 |
| --- | --- | --- | --- |
| 20 HP | 40 Fury | 60 Fury | 70 Fury |
| 30 HP | 60 Fury | 90 Fury | 105 Fury (capped) |
| 40 HP | 80 Fury | 120 Fury (capped) | 140 Fury (capped) |

**Note:** With Blood-Fueled R3 (3×), numbers are even higher.

### Healer Coordination

Communicate with Bone-Setter:

- "Keep me between 40-50% HP"
- Small heals to stay alive, not full heals
- Sustained [Bloodied] uptime = sustained power spike

### Critical Strike Value

Expanded crit range on multi-hit abilities:

- **Rank 2 (10%):** ~0.4 expected crits on 4-target Whirlwind
- **Rank 3 (15%):** ~0.6 expected crits on 4-target Whirlwind
- Crits deal double damage = significant DPS increase

---

## Integration Notes

**Role:** Power spike when [Bloodied]. Rewards staying in the danger zone.

**Design Philosophy:**

- Inverts typical low-HP vulnerability into strength
- Encourages aggressive play at low HP
- High-skill expression through HP manipulation
- Rank 2 provides meaningful bonuses; Rank 3 adds Defense for survivability
- Synergizes perfectly with Blood-Fueled

**Notable Synergies:**

- **Blood-Fueled** (Tier 2): 2×/3× Fury × 1.5/1.75 = massive effective Fury from damage
- **Unstoppable Fury** (Capstone): Safety net at 1 HP when [Bloodied]
- **Hemorrhaging Strike** (Tier 3): +3/+5 damage makes single-target devastating
- **Whirlwind of Destruction** (Tier 2): Each hit benefits from damage/crit bonuses
- **Primal Vigor** (Tier 1): Capped Fury = maximum Stamina regen

**External Synergies:**

- **Bone-Setter:** Essential partner for HP management
- **Skald:** Saga buffs stack with Death or Glory bonuses

**Related Abilities:**

- Blood-Fueled (Tier 2): Core Fury amplification synergy
- Unstoppable Fury (Capstone): Survival at 1 HP enables sustained [Bloodied]
- All damage abilities benefit from damage bonus and expanded crits