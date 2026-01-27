# Tier 2 Ability: Grievous Wound

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-GRIEVOUSWOUND-v5.0
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
| **Ability Name** | Grievous Wound | **Ability Type** | Active |
| **Tier** | 2 (Advanced) | **PP Cost** | 4 PP |
| **Specialization** | Skar-Horde Aspirant | **Resource Cost** | 45 Stamina + 30 Savagery |
| **Action Type** | Standard Action | **Cooldown** | 2 turns |
| **Prerequisite** | 8 PP in Skar-Horde tree | **Trauma Risk** | None |

---

## Thematic Description

> *"You carve a wound that armor cannot protect against. A wound that does not close. A wound that reminds them what mortality means."*
> 

Grievous Wound is the Skar-Horde Aspirant's signature attack—a savage strike that bypasses all conventional defenses. The wound isn't merely physical; it's a violation of the target's sense of invulnerability. No matter how much armor they wear, no matter how high their Soak, the [Grievous Wound] bleeds through everything. This is why the Aspirant is the ultimate tank-buster.

---

## Mechanical Implementation

### Rank 2 (Foundation — 4 PP)

- **Cost:** 45 Stamina + 30 Savagery | Standard Action
- **Cooldown:** 2 turns
- **Target:** Single enemy within melee range (1 hex)
- **Attack Roll:** MIGHT vs. target Defense
- **Initial Damage:** 3d8 Physical damage
- **Debuff:** Applies **[Grievous Wound]** for 3 turns
    - Deals **1d10 Physical damage per turn**
    - **Bypasses all Soak** (armor is irrelevant)
    - Cannot be cleansed by standard healing

### Rank 3 (Mastery — With Capstone)

- **Initial Damage:** 4d8 Physical damage (increased)
- **DoT Duration:** 4 turns (extended)
- **DoT Damage:** 1d12 Physical per turn (increased from 1d10)
- **Kill Bonus:** If target dies while affected, **refund 20 Savagery**
- **Spread:** On target death, [Grievous Wound] spreads to one adjacent enemy (3 turns)

---

## Resolution Pipeline

### Attack Resolution

```
1. VALIDATE: Target in melee range (1 hex)
2. VALIDATE: Stamina >= 45 AND Savagery >= 30
3. VALIDATE: Cooldown available (not on cooldown)
4. COST: Deduct 45 Stamina + 30 Savagery
5. SET: Cooldown = 2 turns
6. ROLL: MIGHT-based attack vs. target Defense
7. IF hit:
   a. DAMAGE: 3d8/4d8 Physical (by rank)
   b. APPLY: [Grievous Wound] debuff
      - Duration: 3/4 turns (by rank)
      - Tick Damage: 1d10/1d12 Physical (by rank)
      - Property: Bypasses_Soak = TRUE
   c. CHECK: The Price of Power active?
      - IF yes: No additional Stress (Savagery spent, not generated)
8. IF miss:
   a. No damage
   b. No debuff applied
   c. Resources still consumed
   d. Cooldown still triggered
```

### DoT Tick Resolution (Per Turn)

```
1. CHECK: Target has [Grievous Wound]?
2. IF yes:
   a. ROLL: 1d10/1d12 Physical damage (by rank)
   b. APPLY: Damage directly to HP (bypasses Soak)
   c. DECREMENT: Duration by 1
   d. CHECK: Target HP <= 0?
      - IF yes AND Rank 3: Refund 20 Savagery to Aspirant
      - IF yes AND Rank 3: Spread to adjacent enemy
3. IF duration = 0:
   a. REMOVE: [Grievous Wound] debuff
```

### Spread Resolution (Rank 3)

```
1. TRIGGER: Target with [Grievous Wound] dies
2. FIND: Adjacent enemies (1 hex from corpse)
3. IF adjacent enemies exist:
   a. SELECT: Lowest HP enemy (or random if tied)
   b. APPLY: [Grievous Wound] (3 turns, 1d12/turn)
   c. OUTPUT: "[Grievous Wound] spreads to [Target]!"
4. IF no adjacent enemies:
   a. OUTPUT: "No valid spread targets."
```

---

## Worked Examples

### Example 1: Basic Grievous Wound (Rank 2)

```
Grimnir uses Grievous Wound against Undying Juggernaut (Soak 8).
├── Stamina: 80 → 35 (cost: 45)
├── Savagery: 45 → 15 (cost: 30)
├── Attack Roll: MIGHT 4 vs. Defense 3 → 1 success (HIT)
├── Initial Damage: 3d8[6,4,5] = 15 Physical
│   └── After Soak 8: 7 damage dealt
├── [Grievous Wound] Applied: 3 turns
├── Turn 2: DoT ticks 1d10[8] = 8 Physical (bypasses Soak 8!)
├── Turn 3: DoT ticks 1d10[6] = 6 Physical (bypasses Soak!)
├── Turn 4: DoT ticks 1d10[9] = 9 Physical (bypasses Soak!)
├── Total DoT Damage: 23 (all bypassing armor)
└── Result: 7 + 23 = 30 total damage vs. heavily armored target
   (Normal attacks would have dealt ~7 damage per hit)
```

### Example 2: Kill + Savagery Refund (Rank 3)

```
Grimnir uses Grievous Wound against wounded Blight-Touched Raider (12 HP).
├── Initial Damage: 4d8[5,7,3,6] = 21 Physical → Target at -9 HP (DEAD)
├── Target dies with [Grievous Wound] active
├── Rank 3 Kill Bonus: +20 Savagery refunded
├── Savagery: 15 → 35 (after refund)
└── Result: Efficient kill, Savagery economy maintained
```

### Example 3: Spread on Death (Rank 3)

```
Undying Sentinel (30 HP) has [Grievous Wound], adjacent to Corrupted Hound.
├── Turn 3: DoT ticks 1d12[11] = 11 Physical
├── Sentinel HP: 8 → -3 (DEAD)
├── Rank 3 Spread triggers
├── Adjacent target: Corrupted Hound
├── [Grievous Wound] spreads to Hound (3 turns, 1d12/turn)
└── Result: DoT chain continues without additional Savagery cost
```

---

## Failure Modes

### Insufficient Savagery

```
Grimnir has 20 Savagery, attempts Grievous Wound (cost 30).
├── BLOCK: "Insufficient Savagery. Need 30, have 20."
└── Suggest: "Use Savage Strike to build Savagery first."
```

### On Cooldown

```
Grimnir used Grievous Wound last turn, attempts again.
├── BLOCK: "Grievous Wound on cooldown. 1 turn remaining."
└── Suggest: "Use Savage Strike or other abilities while waiting."
```

### Miss

```
Grimnir rolls MIGHT 3 vs. Defense 5 → 0 successes.
├── MISS: No damage dealt
├── No [Grievous Wound] applied
├── Stamina + Savagery: Still consumed
├── Cooldown: Still triggered (2 turns)
└── Result: Significant resource waste on miss
```

### Target Immune to DoT

```
Certain bosses may have DoT immunity.
├── Initial damage: Applied normally
├── [Grievous Wound]: Resisted (immune)
└── Result: Reduced effectiveness against DoT-immune enemies
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Savage Strike:** Build 30+ Savagery with 2 Strikes → Spend on Grievous Wound
- **Pain Fuels Savagery:** Taking damage helps sustain Savagery economy
- **Monstrous Apotheosis:** During [Apotheosis], Grievous Wound also applies [Bleeding] (2d6/turn)
- **The Price of Power:** Faster Savagery regeneration for more frequent use

### External Synergies

- **Skjaldmær:** Tank holds target in place for guaranteed DoT ticks
- **Atgeir-wielder:** Root effects prevent target escape
- **Veiðimaðr (Blight Corruption):** Stacking DoTs for massive sustained damage
- **Impaling Spike:** Root target → Apply Grievous Wound safely

### Counter-Interactions

- **Bone-Setter:** Can heal through the DoT (but cannot remove [Grievous Wound])
- **High HP Pools:** Tanks survive long enough to outlast duration
- **DoT Immunity:** Some bosses resist persistent damage

---

## Tactical Applications

1. **Tank-Buster:** Primary use against heavily armored targets—Soak is irrelevant
2. **Elite Killer:** DoT ticks guarantee damage regardless of defenses
3. **Resource Investment:** High Savagery cost (30) for high payoff—choose targets wisely
4. **Kill Confirm:** Rank 3 refund rewards finishing wounded targets
5. **AoE Potential:** Rank 3 spread can chain through clustered enemies

---

## v5.0 Compliance Notes

- **Armor Bypass:** Core design identity—Soak is completely irrelevant to DoT
- **DoT Duration:** 3-4 turns provides strategic depth and counterplay window
- **Savagery Economy:** Expensive (30) but devastating—defines builder-spender loop
- **Scaling Pattern:** Foundation (base effect) → Mastery (enhanced + kill bonus + spread)
- **Tier 2 Structure:** R2→R3 progression (unlocks at 8 PP, Rank 3 with Capstone)