# Tier 3 Ability: Overcharged Piston Slam

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-OVERCHARGEDPISTONSLAM-v5.0
Mechanical Role: Damage Dealer
Parent item: Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0 (Skar-Horde%20Aspirant%20(Augmented%20Brawler)%20%E2%80%94%20Speciali%20dcff21d0a06040698381b59039deaf60.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## Overview

| Property | Value | Property | Value |
| --- | --- | --- | --- |
| **Ability Name** | Overcharged Piston Slam | **Ability Type** | Active |
| **Tier** | 3 (Mastery) | **PP Cost** | 5 PP |
| **Specialization** | Skar-Horde Aspirant | **Ranks** | R2 → R3 |
| **Resource Cost** | 55 Stamina + 40 Savagery | **Action Type** | Standard Action |
| **Prerequisite** | All Tier 2 abilities trained | **Requirement** | [Blunt] augment equipped |

---

## Rank Progression

| Rank | Trigger | Key Unlock |
| --- | --- | --- |
| **Rank 2** | Train ability (5 PP) | 6d10 damage, 75% Stun, double damage buff |
| **Rank 3** | Train Capstone | 7d10 damage, 100% Stun, cooldown reduction on kill |

---

## Thematic Description

> *"Superheated steam vents. Pistons compress. And then—impact. A concussive blast that reduces bone to powder."*
> 

Overcharged Piston Slam is the Skar-Horde Aspirant's most devastating single-target attack—a mechanically-enhanced strike that channels pressurized force through the [Piston Hammer] augment. The impact isn't just damaging; it's stunning, overwhelming the target's nervous system with raw concussive force. This is the ability that defines the [Blunt] augment build path.

---

## Mechanical Implementation

### Rank 2 (Foundation — 5 PP)

- **Cost:** 55 Stamina + 40 Savagery | Standard Action
- **Cooldown:** 4 turns
- **Requirement:** [Blunt] tag augment ([Piston Hammer])
- **Target:** Single enemy within melee range (1 hex)
- **Attack Roll:** MIGHT vs. target Defense
- **Damage:** 6d10 Physical (Bludgeoning)
- **CC:** **75% chance** to apply **[Stunned]** for 1 turn
    - [Stunned]: Lose next turn, -4 Defense, cannot take reactions
- **Execute Bonus:** Your **next attack** after Overcharged Piston Slam deals **double damage**

### Rank 3 (Mastery — Capstone Trained)

- **Damage:** 7d10 Physical (increased)
- **Stun Chance:** **100% guaranteed** (increased from 75%)
- **Kill Bonus:** On kill, cooldown reduced by 2 turns (4 → 2)
- **Armor Shred:** Target loses 2 Soak for remainder of combat

---

## Resolution Pipeline

### Attack Resolution

```jsx
1. VALIDATE: Target in melee range (1 hex)
2. VALIDATE: Stamina >= 55 AND Savagery >= 40
3. VALIDATE: Cooldown available (not on cooldown)
4. VALIDATE: Augment has [Blunt] tag
   - Valid: [Piston Hammer]
   - Invalid: [Serrated Claw], [Injector Spike], [Taser Gauntlet]
   - IF invalid: BLOCK "Overcharged Piston Slam requires [Blunt] augment."
5. COST: Deduct 55 Stamina + 40 Savagery
6. SET: Cooldown = 4 turns
7. ROLL: MIGHT-based attack vs. target Defense
8. IF hit:
   a. DAMAGE: 6d10/7d10 Physical (by rank)
   b. APPLY: [Piston Hammer] [Armor Piercing] (-2 Soak)
   c. ROLL: d100 vs. Stun Threshold
      - Rank 2: 75% (roll 1-75 = Stun)
      - Rank 3: 100% (guaranteed)
   d. IF Stun triggers:
      i. APPLY: [Stunned] status (1 turn)
   e. SET: Aspirant.NextAttackDoubleDamage = TRUE
   f. IF Rank 3:
      i. APPLY: Target loses 2 Soak (permanent, this combat)
      ii. CHECK: Target HP <= 0?
         - IF yes: Cooldown reduced by 2 turns (4 → 2)
9. IF miss:
   a. No damage
   b. No [Stunned] applied
   c. No double damage buff
   d. Resources still consumed
   e. Cooldown still triggered
```

### Double Damage Resolution

```jsx
1. CHECK: Aspirant.NextAttackDoubleDamage = TRUE?
2. IF yes:
   a. ON next attack (any ability):
      i. CALCULATE: Final Damage = Base Damage × 2
      ii. SET: Aspirant.NextAttackDoubleDamage = FALSE
   b. EXPIRE: If no attack made before end of next turn
```

### Augment Gating Check

```jsx
1. CHECK: Character.EquippedAugment.Tag
2. IF Tag includes [Blunt]:
   a. PROCEED with ability
3. IF Tag does NOT include [Blunt]:
   a. BLOCK: "Cannot use Overcharged Piston Slam. Requires [Blunt] augment."
   b. GUI: Ability button grayed out with tooltip
   c. Suggest: "Swap to [Piston Hammer] at workbench."
```

---

## Worked Examples

### Example 1: Basic Slam with Stun (Rank 2)

```jsx
Grimnir uses Overcharged Piston Slam against Undying Sentinel.
├── Augment Check: [Piston Hammer] has [Blunt] tag ✓
├── Stamina: 90 → 35 (cost: 55)
├── Savagery: 65 → 25 (cost: 40)
├── Attack Roll: MIGHT 5 vs. Defense 3 → 2 successes (HIT)
├── Damage: 6d10[8,6,9,4,7,5] = 39 Physical
├── [Armor Piercing]: Target Soak 6 - 2 = 4 effective
├── Damage After Soak: 39 - 4 = 35 damage dealt
├── Stun Roll: d100 = 52 (≤75, STUN TRIGGERS)
├── [Stunned] Applied: 1 turn
│   ├── Sentinel loses next turn
│   ├── Sentinel suffers -4 Defense
│   └── Sentinel cannot take reactions
├── Double Damage Buff: ACTIVE for next attack
└── Result: Massive damage + Stun + setup for execute
```

### Example 2: Guaranteed Stun + Armor Shred (Rank 3)

```jsx
Grimnir (Rank 3) uses Overcharged Piston Slam against Elite Raider.
├── Damage: 7d10[8,6,9,4,7,5,10] = 49 Physical
├── [Stunned]: 100% guaranteed at Rank 3
├── Armor Shred: Target loses 2 Soak (permanent)
├── Double Damage Buff: ACTIVE
└── Result: Devastating burst + permanent debuff
```

### Example 3: Slam → Savage Strike Execute Combo

```jsx
Turn 1: Grimnir uses Overcharged Piston Slam
├── Damage: 35 (after Soak)
├── Target: [Stunned] for 1 turn
├── Double Damage: ACTIVE

Turn 2: Grimnir uses Savage Strike (Double Damage)
├── Target still [Stunned]: -4 Defense (very easy to hit)
├── Base Damage: 2d10[7,8] + MIGHT[5] = 20 Physical
├── Double Damage: 20 × 2 = 40 Physical
└── Result: 35 + 40 = 75 total damage in 2 turns
```

### Example 4: Kill + Cooldown Reduction (Rank 3)

```jsx
Grimnir uses Overcharged Piston Slam against wounded Corrupted Hound (30 HP).
├── Damage: 7d10[6,8,5,9,7,4,6] = 45 Physical
├── Target HP: 30 → -15 (DEAD)
├── Kill Bonus: Cooldown reduced by 2 turns
├── Cooldown: 4 → 2 turns
└── Result: Can use Slam again much sooner on next target
```

---

## Failure Modes

### Wrong Augment Equipped

```jsx
Grimnir has [Serrated Claw] equipped, attempts Overcharged Piston Slam.
├── Augment Tag: [Piercing] (not [Blunt])
├── BLOCK: "Cannot use Overcharged Piston Slam. Requires [Blunt] augment."
├── GUI: Ability grayed out
└── Suggest: "Swap to [Piston Hammer] at workbench."
```

### Stun Chance Failed (Rank 2 Only)

```jsx
Grimnir rolls for Stun at Rank 2.
├── Stun Roll: d100 = 82 (>75, FAILED)
├── Damage: Still dealt
├── [Stunned]: NOT applied
├── Double Damage: Still granted
└── Result: Damage dealt but no CC effect
```

### Insufficient Resources

```jsx
Grimnir has 50 Stamina and 35 Savagery, attempts Slam.
├── BLOCK: "Insufficient resources. Need 55 Stamina + 40 Savagery."
├── Have: 50 Stamina (need 55), 35 Savagery (need 40)
└── Suggest: "Build more Savagery with Savage Strike first."
```

### Target Stun-Immune

```jsx
Undying Automaton (boss) is immune to [Stunned].
├── Damage: Dealt normally (6d10/7d10)
├── [Stunned]: Resisted (immune)
├── Double Damage: Still granted
└── Result: Still powerful for damage, but loses CC value
```

### Miss

```jsx
Grimnir rolls MIGHT 3 vs. Defense 5 → 0 successes.
├── MISS: No damage, no Stun, no double damage buff
├── Resources: Still consumed (55 Stamina + 40 Savagery)
├── Cooldown: Still triggered (4 turns)
└── Result: Devastating waste of resources
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Heretical Augmentation:** Requires [Piston Hammer] augment ([Blunt] tag)
- **Impaling Spike:** Root → Swap → Slam combo (multi-turn setup)
- **Savage Strike:** Follow-up with double damage for massive burst
- **Monstrous Apotheosis:** +25% damage during [Apotheosis] amplifies already huge hit
- **Pain Fuels Savagery:** Sustain the 40 Savagery cost through taking damage

### External Synergies

- **Hólmgangr (Challenge):** Duelist setup for isolated target Slam
- **Atgeir-wielder:** Root into Slam combo
- **Strandhögg:** Momentum burst into Slam finisher
- **Tank Support:** Slam sets up massive burst window for entire party

### Counter-Interactions

- **Stun Immunity:** Boss enemies often resist—loses CC value at Rank 2
- **High Savagery Cost:** 40 Savagery requires significant buildup
- **Long Cooldown:** 4 turns (2 on kill) limits spam potential

### Augment Requirements

| Augment | Tag | Overcharged Piston Slam |
| --- | --- | --- |
| [Piston Hammer] | [Blunt] | ✅ Valid |
| [Serrated Claw] | [Piercing] | ❌ Invalid |
| [Injector Spike] | [Piercing] | ❌ Invalid |
| [Taser Gauntlet] | [Energy] | ❌ Invalid |

---

## Tactical Applications

1. **Burst Window Opener:** Stun + double damage sets up massive damage spike
2. **Elite Execution:** Save for dangerous elites—disable + huge damage
3. **Boss Phase Transition:** Time for boss vulnerability windows
4. **Chain Kill Potential:** Kill bonus (cooldown -2) enables rapid cleanup
5. **Augment Build Path:** [Blunt] augment enables burst damage playstyle

---

## v5.0 Compliance Notes

- **Tier 3 Structure:** R2→R3 progression (starts at Rank 2 when trained)
- **Rank 3 Trigger:** Capstone trained
- **Augment Requirement:** Demonstrates build diversity through equipment gating
- **High Investment:** 5 PP + 40 Savagery + 55 Stamina reflects power level
- **Augment System:** See [Weapon-Stump Augmentation System — Mechanic Specification v5.0](Weapon-Stump%20Augmentation%20System%20%E2%80%94%20Mechanic%20Specif%20c3328afcc82e442bb9680f41336f44d5.md) for gating details