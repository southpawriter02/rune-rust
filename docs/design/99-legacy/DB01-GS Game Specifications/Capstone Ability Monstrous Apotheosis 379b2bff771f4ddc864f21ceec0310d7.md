# Capstone Ability: Monstrous Apotheosis

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-MONSTROUSAPOTHEOSIS-v5.0
Mechanical Role: Damage Dealer
Parent item: Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0 (Skar-Horde%20Aspirant%20(Augmented%20Brawler)%20%E2%80%94%20Speciali%20dcff21d0a06040698381b59039deaf60.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Extreme
Voice Validated: No

## Overview

| Property | Value | Property | Value |
| --- | --- | --- | --- |
| **Ability Name** | Monstrous Apotheosis | **Ability Type** | Active (Ultimate) |
| **Tier** | Capstone | **PP Cost** | 6 PP |
| **Specialization** | Skar-Horde Aspirant | **Ranks** | R1 → R2 → R3 |
| **Resource Cost** | 20 Stamina + 75 Savagery | **Action Type** | Bonus Action |
| **Prerequisite** | Both Tier 3 abilities trained | **Trauma Risk** | **Extreme** (Psychic Stress on exit) |

---

## Rank Progression

| Rank | Trigger | Key Unlock |
| --- | --- | --- |
| **Rank 1** | Train ability (6 PP) | 3 turns, free Strikes, +15% damage, 30 Stress on exit |
| **Rank 2** | Tree progression | 3 turns, +20% damage, 25 Stress, Fear immune |
| **Rank 3** | Full tree completion | 4 turns, +25% damage, 20 Stress, Fear+Stun immune, early exit |

---

## Thematic Description

> *"You give in completely. The whispers become a roar. Your augment screams with power. You are no longer human. You are a weapon. You are inevitable."*
> 

Monstrous Apotheosis is the ultimate expression of the Skar-Horde philosophy—the moment when the Aspirant surrenders completely to their augmented fury. For several devastating turns, they transcend mortal limitations, becoming an unstoppable engine of destruction. Free attacks, automatic bleeding, immunity to crowd control, and amplified damage—all culminating in a choice: ride the wave to its end and pay the Stress cost, or exit early and preserve your sanity.

---

## Mechanical Implementation

### Rank 1 (Foundation — 6 PP)

- **Cost:** 20 Stamina + 75 Savagery | Bonus Action
- **Frequency:** Once per combat
- **Duration:** 3 turns

**[Apotheosis] State Effects:**

- **Savage Strike** costs **0 Stamina**
- **+15% all damage** dealt

**Exit:**

- **Full Duration:** At end of 3 turns, gain **30 Psychic Stress**

### Rank 2 (Advanced — Tree Progression)

- **Duration:** 3 turns
- **Damage Bonus:** +20% all damage (increased from 15%)
- **Immunity:** Immune to [Feared]
- **Stress Reduction:** Exit Stress reduced to **25** (from 30)

**[Apotheosis] Additional Effect:**

- **Grievous Wound** also applies **[Bleeding]** (1d6 Physical/turn, 2 turns)

### Rank 3 (Mastery — Full Tree Completion)

- **Duration:** 4 turns (increased from 3)
- **Damage Bonus:** +25% all damage (increased from 20%)
- **Immunity:** Immune to [Feared] **and [Stunned]**
- **Stress Reduction:** Exit Stress reduced to **20** (from 25)

**[Apotheosis] Additional Effects:**

- **Grievous Wound** also applies **[Bleeding]** (2d6 Physical/turn, 3 turns)
- **Early Exit:** Can end [Apotheosis] early as a **free action** to **avoid Stress penalty entirely**

---

## Resolution Pipeline

### Activation Resolution

```jsx
1. VALIDATE: Not already in [Apotheosis]
2. VALIDATE: Stamina >= 20 AND Savagery >= 75
3. VALIDATE: Once per combat not used
4. COST: Deduct 20 Stamina + 75 Savagery
5. SET: Apotheosis.Active = TRUE
6. SET: Apotheosis.TurnsRemaining = Duration (3/3/4 by rank)
7. SET: Apotheosis.UsedThisCombat = TRUE
8. APPLY: All [Apotheosis] state effects (by rank)
9. OUTPUT: "Monstrous Apotheosis activated! X turns remaining."
```

### Turn Resolution (During Apotheosis)

```jsx
1. START OF TURN:
   a. CHECK: Apotheosis.TurnsRemaining > 0?
   b. IF yes: Apply state effects
   c. OUTPUT: "[Apotheosis]: X turns remaining"

2. DURING TURN:
   a. Savage Strike: Cost = 0 Stamina
   b. Grievous Wound (Rank 2+): Also applies [Bleeding]
      - Rank 2: 1d6/turn, 2 turns
      - Rank 3: 2d6/turn, 3 turns
   c. All damage: ×1.15/1.20/1.25 (by rank)
   d. Rank 2+: Immune to [Feared]
   e. Rank 3: Immune to [Feared] and [Stunned]

3. END OF TURN:
   a. DECREMENT: Apotheosis.TurnsRemaining -= 1
   b. CHECK: TurnsRemaining = 0?
      - IF yes: Trigger Aftermath Resolution
```

### Early Exit Resolution (Rank 3 Only)

```jsx
1. TRIGGER: Player chooses "End Apotheosis" (free action)
2. VALIDATE: Currently in [Apotheosis]
3. VALIDATE: Rank 3 (early exit only available at Rank 3)
4. REMOVE: All [Apotheosis] state effects
5. SET: Apotheosis.Active = FALSE
6. SKIP: Stress penalty (avoided by early exit)
7. OUTPUT: "Apotheosis ended early. Stress penalty avoided."
```

### Aftermath Resolution (Full Duration)

```jsx
1. TRIGGER: Apotheosis.TurnsRemaining reaches 0
2. REMOVE: All [Apotheosis] state effects
3. SET: Apotheosis.Active = FALSE
4. APPLY: Psychic Stress (by rank, irreducible)
   - Rank 1: 30 Stress
   - Rank 2: 25 Stress
   - Rank 3: 20 Stress
5. CHECK: Stress >= Breaking Point?
   a. IF yes: Trigger Breaking Point resolution
6. OUTPUT: "Apotheosis ends. The backlash floods your mind. (+X Stress)"
```

---

## Worked Examples

### Example 1: Basic Apotheosis (Rank 1)

```jsx
Grimnir activates Monstrous Apotheosis (Rank 1).
├── Savagery: 80 → 5 (cost: 75)
├── Stamina: 60 → 40 (cost: 20)
├── [Apotheosis] Active: 3 turns
├── Damage Bonus: +15%

Turn 1 (Apotheosis):
├── Savage Strike: 0 Stamina (free!)
├── Damage: 2d10[8,6] + 5 = 19 × 1.15 = 21 Physical

Turn 2 (Apotheosis):
├── Savage Strike: 0 Stamina
├── Building Savagery back up

Turn 3 (Apotheosis):
├── Savage Strike: 0 Stamina
├── [Apotheosis] Ends
├── Stress: 15 → 45 (+30 Psychic Stress)
└── Result: 3-turn rampage, heavy Stress cost
```

### Example 2: Advanced Apotheosis with Bleeding (Rank 2)

```jsx
Grimnir activates Monstrous Apotheosis (Rank 2).
├── [Apotheosis] Active: 3 turns
├── Damage Bonus: +20%
├── Immune to [Feared]

Turn 2: Use Grievous Wound
├── [Grievous Wound] applied (4 turns, 1d12/turn)
├── [Bleeding] ALSO applied (1d6/turn, 2 turns) ← Rank 2 bonus!
├── Combined DoT: 1d12 + 1d6 per turn

[Apotheosis] Ends:
├── Stress: 15 → 40 (+25 Psychic Stress)
└── Result: Stacking DoTs, reduced Stress penalty
```

### Example 3: Full Mastery Apotheosis (Rank 3)

```jsx
Grimnir activates Monstrous Apotheosis (Rank 3).
├── [Apotheosis] Active: 4 turns (extended!)
├── Damage Bonus: +25%
├── Immune to [Feared] and [Stunned]

Turn 2: Use Grievous Wound
├── [Grievous Wound] applied
├── [Bleeding] applied (2d6/turn, 3 turns) ← Rank 3 bonus!

Turn 4: Combat ends, enemies dead
├── Decision: Ride out or early exit?

Grimnir uses Early Exit (free action):
├── [Apotheosis] ended
├── Stress penalty: AVOIDED (0 instead of 20)
└── Result: Smart play—preserved sanity when power wasn't needed
```

### Example 4: Breaking Point Risk Management

```jsx
Grimnir at 35 Stress, activates Apotheosis.
├── Rank 1: Would end at 35 + 30 = 65 Stress (Breaking Point!)
├── Rank 2: Would end at 35 + 25 = 60 Stress (Breaking Point!)
├── Rank 3: Would end at 35 + 20 = 55 Stress (Breaking Point!)
├── Rank 3 with Early Exit: 35 + 0 = 35 Stress (SAFE)

Options:
├── R1-R2: Must accept Breaking Point risk
├── R3: Can early exit to avoid all Stress
└── Result: Rank 3 provides crucial safety valve
```

### Example 5: Immune to CC During Apotheosis

```jsx
Enemy caster attempts [Feared] on Grimnir during Apotheosis (Rank 2+).
├── [Apotheosis] Active: YES
├── [Feared] Immunity: YES
├── RESULT: "Grimnir is immune to [Feared] during Apotheosis."
└── No effect—Aspirant is beyond mortal fear

Enemy tank attempts [Stunned] via Shield Bash (Rank 3 only).
├── [Stunned] Immunity: YES (Rank 3)
├── RESULT: "Grimnir is immune to [Stunned] during Apotheosis."
└── Cannot be locked down during transformation
```

---

## Failure Modes

### Insufficient Savagery

```jsx
Grimnir has 60 Savagery, attempts Apotheosis (cost 75).
├── BLOCK: "Insufficient Savagery. Need 75, have 60."
└── Suggest: "Build Savagery with Savage Strike first."
```

### Already Used This Combat

```jsx
Grimnir used Apotheosis earlier, attempts again.
├── BLOCK: "Monstrous Apotheosis already used this combat."
└── Suggest: "Once per combat limitation. Save for crucial moments."
```

### Early Exit Not Available (Rank 1-2)

```jsx
Grimnir (Rank 2) wants to exit early.
├── Early Exit: Only available at Rank 3
├── BLOCK: "Early exit requires Rank 3."
└── Result: Must accept Stress penalty at Rank 1-2
```

### Breaking Point Triggered

```jsx
Grimnir at 45 Stress, Apotheosis ends with +30 Stress (Rank 1).
├── Stress: 45 → 75
├── Breaking Point: 50
├── TRIGGER: Breaking Point exceeded by 25
├── CONSEQUENCE: Trauma Economy resolution (severe)
└── Result: Power achieved, sanity lost
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Savage Strike:** Free attacks (0 Stamina) during window = maximum Savagery regeneration
- **Grievous Wound:** Automatic [Bleeding] stacking creates devastating DoT (Rank 2+)
- **The Price of Power:** Build to 75 threshold faster with amplified Savagery
- **Horrific Form:** Immune to Fear yourself while inflicting it on others
- **Pain Fuels Savagery:** Taking hits during Apotheosis rebuilds Savagery for post-transformation

### External Synergies (CRITICAL)

- **Bone-Setter:** **ESSENTIAL** — Post-Apotheosis Stress recovery
- **Skjaldmær:** Tank protection during vulnerable aftermath (0 Savagery)
- **Skald:** Mental stability buffs reduce effective Stress impact
- **Coordinated Burst:** Party times burst damage during Apotheosis window

### Counter-Interactions

- **Long Fights:** Once per combat—wasted if used too early
- **Stress Threshold:** Exit Stress can trigger Breaking Point (less at higher ranks)
- **Post-Apotheosis Vulnerability:** 0 Savagery after, vulnerable to counterattack

---

## Tactical Applications

1. **Boss Burst:** Time for boss vulnerability windows or phase transitions
2. **Elite Deletion:** Extended turns of amplified damage + free attacks = rapid kills
3. **Emergency CC Immunity:** Activation grants Fear/Stun immunity when needed
4. **Stress Management:** Rank 3 early exit option enables risk-free partial usage
5. **Rank Investment Reward:** Each rank reduces Stress and adds capabilities

---

## v5.0 Compliance Notes

- **Capstone Structure:** R1→R2→R3 progression
- **Rank 1 Trigger:** Train ability (6 PP)
- **Rank 2-3 Trigger:** Tree progression / Full tree completion
- **Trauma Economy:** Scaling Stress penalty (30 → 25 → 20) rewards investment
- **Once Per Combat:** Prevents spam, encourages strategic timing
- **Strategic Choice:** Rank 3 early exit option adds meaningful player decision