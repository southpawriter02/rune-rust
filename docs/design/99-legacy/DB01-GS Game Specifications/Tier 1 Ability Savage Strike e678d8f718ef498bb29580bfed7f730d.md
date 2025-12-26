# Tier 1 Ability: Savage Strike

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-SAVAGESTRIKE-v5.0
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
| **Ability Name** | Savage Strike | **Ability Type** | Active |
| **Tier** | 1 (Foundation) | **PP Cost** | 0 PP (free with spec) |
| **Specialization** | Skar-Horde Aspirant | **Ranks** | R1 → R2 → R3 |
| **Resource Cost** | 40 Stamina | **Action Type** | Standard Action |
| **Prerequisite** | Unlock Skar-Horde Aspirant (3 PP) | **Trauma Risk** | None |

---

## Rank Progression

| Rank | Trigger | Key Unlock |
| --- | --- | --- |
| **Rank 1** | Unlock specialization (3 PP) | 2d[Aug] damage, +15 Savagery |
| **Rank 2** | Train 2 Tier 2 abilities | +20 Savagery, crit bonus |
| **Rank 3** | Train Capstone | +25 Savagery, cleave on kill |

---

## Thematic Description

> *"You don't swing your augment—you unleash it. Each strike is a statement: I have paid the price. Now you pay yours."*
> 

Savage Strike is the Skar-Horde Aspirant's bread-and-butter attack—the fundamental expression of their augmented fury. Every hit generates Savagery, the unique resource that fuels the Aspirant's most devastating abilities. The strike itself is brutal and efficient, designed to build momentum for the carnage to come.

---

## Mechanical Implementation

### Rank 1 (Foundation — With Spec Unlock)

- **Cost:** 40 Stamina | Standard Action
- **Target:** Single enemy within melee range (1 hex)
- **Attack Roll:** MIGHT vs. target Defense
- **Damage:** 2d[Augment] + MIGHT modifier
- **Savagery Generation:** +15 Savagery on hit

### Rank 2 (Advanced — 2 Tier 2 Abilities Trained)

- **Savagery Generation:** +20 Savagery on hit (increased)
- **Crit Bonus:** On critical hit, generate **+10 bonus Savagery** (total 30)
- **Damage:** 2d[Augment] + MIGHT + 2 (flat bonus)

### Rank 3 (Mastery — Capstone Trained)

- **Savagery Generation:** +25 Savagery on hit (increased)
- **Crit Bonus:** On critical hit, generate **+15 bonus Savagery** (total 40)
- **Damage:** 2d[Augment] + MIGHT + 4 (increased flat bonus)
- **Cleave:** On kill, deal **50% damage** to one adjacent enemy

---

## Resolution Pipeline

### Attack Resolution

```jsx
1. VALIDATE: Target in melee range (1 hex)
2. VALIDATE: Stamina >= 40
3. COST: Deduct 40 Stamina
4. ROLL: MIGHT-based attack vs. target Defense
5. DETERMINE: Critical hit? (natural max on dice)
6. IF hit:
   a. ROLL: Damage = 2d[Augment] + MIGHT + FlatBonus
      - Rank 1: +0 flat bonus
      - Rank 2: +2 flat bonus
      - Rank 3: +4 flat bonus
   b. APPLY: Augment special effects (if any)
   c. GENERATE: Savagery
      - Rank 1: +15
      - Rank 2: +20 (+10 on crit = 30)
      - Rank 3: +25 (+15 on crit = 40)
   d. CHECK: The Price of Power active?
      - IF yes: Savagery × 2.0, then Stress = Savagery / 15
   e. CHECK: Target killed AND Rank 3?
      - IF yes: Trigger Cleave resolution
7. IF miss:
   a. No damage
   b. No Savagery generated
   c. Stamina still consumed
```

### Cleave Resolution (Rank 3)

```jsx
1. TRIGGER: Target killed by Savage Strike
2. FIND: Adjacent enemies (1 hex from corpse)
3. IF adjacent enemies exist:
   a. SELECT: Lowest HP enemy (or random if tied)
   b. CALCULATE: Cleave Damage = Original Damage × 0.5
   c. ROLL: Auto-hit (no attack roll for cleave)
   d. APPLY: Cleave damage to secondary target
   e. NO Savagery generated from cleave damage
   f. CHECK: Secondary target killed?
      - IF yes: No further cleave (once per Strike)
4. IF no adjacent enemies:
   a. OUTPUT: "No valid cleave targets."
```

---

## Worked Examples

### Example 1: Basic Savage Strike (Rank 1)

```jsx
Grimnir uses Savage Strike against Blight-Touched Raider.
├── Stamina: 80 → 40 (cost: 40)
├── Attack Roll: MIGHT 4 vs. Defense 2 → 2 successes (HIT)
├── Augment: [Serrated Claw] (2d6)
├── Damage: 2d6[4,5] + MIGHT[4] = 13 Physical
├── Savagery: 0 → 15 (+15)
└── Result: Solid damage, Savagery building
```

### Example 2: Critical Strike (Rank 2)

```jsx
Grimnir uses Savage Strike, rolls critical hit.
├── Attack Roll: Natural 6 on d6 (CRITICAL)
├── Damage: 2d8[6,5] + MIGHT[4] + 2 = 17 Physical (×2 for crit = 34)
├── Savagery (base): +20
├── Savagery (crit bonus): +10
├── Total Savagery: 0 → 30
└── Result: Devastating hit, massive Savagery spike
```

### Example 3: Kill + Cleave (Rank 3)

```jsx
Grimnir uses Savage Strike against wounded Corrupted Hound (8 HP).
Adjacent: Blight-Touched Raider (25 HP)
├── Damage: 2d8[7,6] + MIGHT[5] + 4 = 22 Physical
├── Hound HP: 8 → -14 (DEAD)
├── Savagery: 50 → 75 (+25)
├── CLEAVE TRIGGERS:
│   ├── Adjacent target: Blight-Touched Raider
│   ├── Cleave damage: 22 × 50% = 11 Physical
│   ├── Raider HP: 25 → 14
│   └── No Savagery from cleave
└── Result: Multi-target damage from single attack
```

### Example 4: With The Price of Power (Rank 3)

```jsx
Grimnir (with The Price of Power) uses Savage Strike.
├── Base Savagery: +25
├── Price of Power: 25 × 2.0 = 50 Savagery
├── Savagery: 25 → 75 (+50)
├── Stress: 50 / 15 = 3.33 → 3 Psychic Stress
├── Stress: 12 → 15 (+3)
└── Result: Massive Savagery gain with Stress cost
```

---

## Failure Modes

### Insufficient Stamina

```jsx
Grimnir has 30 Stamina, attempts Savage Strike (cost 40).
├── BLOCK: "Insufficient Stamina. Need 40, have 30."
└── Suggest: "Wait for Stamina regeneration or use basic attack."
```

### Miss

```jsx
Grimnir rolls MIGHT 2 vs. Defense 4 → 0 successes.
├── MISS: No damage dealt
├── Savagery: Not generated (hit required)
├── Stamina: Still consumed (40)
└── Result: Resource waste on miss
```

### Savagery Cap

```jsx
Grimnir has 90 Savagery, Savage Strike generates +25.
├── Savagery: 90 + 25 = 115 → CAPPED at 100
├── 15 Savagery wasted (overflow)
└── Result: Spend Savagery before hitting cap
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Heretical Augmentation:** Damage dice determined by equipped augment
- **Pain Fuels Savagery:** Two Savagery sources for rapid buildup
- **The Price of Power:** Doubles Savagery generation (but adds Stress)
- **Grievous Wound / Impaling Spike:** Build Savagery → Spend on powerful abilities
- **Monstrous Apotheosis:** Savage Strike costs 0 Stamina during transformation

### External Synergies

- **Berserkr:** Both use momentum-building combat styles
- **Atgeir-wielder:** Root target → Free Savage Strike hits
- **Tank Support:** Protected while building Savagery

### Augment Damage Dice

| Augment Tier | Damage Dice |
| --- | --- |
| [Basic] | 2d6 |
| [Optimized] | 2d8 |
| [Masterwork] | 2d10 |

---

## Tactical Applications

1. **Savagery Engine:** Primary method of building resource for big abilities
2. **Sustained Damage:** Reliable DPS when big abilities are on cooldown
3. **Crit Fishing:** Rank 2-3 reward critical hits with bonus Savagery
4. **Cleave Cleanup:** Rank 3 enables efficient trash mob clearing
5. **Apotheosis Fuel:** During transformation, free Strikes maintain Savagery

---

## v5.0 Compliance Notes

- **Tier 1 Structure:** R1→R2→R3 progression (free with spec, ranks via tree investment)
- **Rank 2 Trigger:** 2 Tier 2 abilities trained
- **Rank 3 Trigger:** Capstone trained
- **Savagery Economy:** Core builder ability—defines resource loop
- **No PP Cost:** Tier 1 abilities are granted free when specialization is unlocked