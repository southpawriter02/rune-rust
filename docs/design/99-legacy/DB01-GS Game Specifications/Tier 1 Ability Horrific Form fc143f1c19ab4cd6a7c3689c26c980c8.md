# Tier 1 Ability: Horrific Form

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-HORRIFICFORM-v5.0
Mechanical Role: Controller/Debuffer
Parent item: Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0 (Skar-Horde%20Aspirant%20(Augmented%20Brawler)%20%E2%80%94%20Speciali%20dcff21d0a06040698381b59039deaf60.md)
Proof-of-Concept Flag: No
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

## Overview

| Property | Value | Property | Value |
| --- | --- | --- | --- |
| **Ability Name** | Horrific Form | **Ability Type** | Passive |
| **Tier** | 1 (Foundation) | **PP Cost** | 0 PP (free with spec) |
| **Specialization** | Skar-Horde Aspirant | **Ranks** | R1 → R2 → R3 |
| **Prerequisite** | Unlock Skar-Horde Aspirant (3 PP) | **Trauma Risk** | None (inflicts Fear on others) |

---

## Rank Progression

| Rank | Trigger | Key Unlock |
| --- | --- | --- |
| **Rank 1** | Unlock specialization (3 PP) | 25% Fear chance on melee attackers |
| **Rank 2** | Train 2 Tier 2 abilities | 40% Fear chance, extended duration |
| **Rank 3** | Train Capstone | 50% Fear chance, +5 Savagery per Fear |

---

## Thematic Description

> *"They see the scars. They see the augment where a hand should be. They see the look in your eyes—the one that says you've already paid every price. And they hesitate."*
> 

Horrific Form reflects the psychological impact of the Aspirant's self-mutilation. The sight of their deliberate disfigurement—the crude augmentation socket, the ritual scarification, the calculated absence of humanity—triggers an instinctive revulsion in enemies. Those who strike the Aspirant in melee range risk being overwhelmed by the wrongness of what they see.

---

## Mechanical Implementation

### Rank 1 (Foundation — With Spec Unlock)

- **Trigger:** Enemy attacks you with a melee attack
- **Effect:** 25% chance to apply **[Feared]** for 1 turn
    - [Feared]: Cannot willingly move toward you, -2 to attack rolls, disadvantage on WILL saves
- **Limitation:** Once per enemy per combat (cannot re-fear same target)

### Rank 2 (Advanced — 2 Tier 2 Abilities Trained)

- **Fear Chance:** 40% (increased from 25%)
- **Fear Duration:** 2 turns (increased from 1)
- **Intimidation Aura:** Enemies within 2 hexes have -1 to WILL saves

### Rank 3 (Mastery — Capstone Trained)

- **Fear Chance:** 50% (increased from 40%)
- **Fear Duration:** 2 turns
- **Savagery Bonus:** Gain **+5 Savagery** when you successfully inflict [Feared]
- **Intimidation Aura:** Enemies within 2 hexes have -2 to WILL saves (increased)
- **Fear Spread:** When an enemy becomes [Feared], adjacent enemies must save vs. WILL or become [Shaken] (1 turn)

---

## Resolution Pipeline

### Fear Trigger Resolution

```jsx
1. TRIGGER: Enemy melee attack against Aspirant (hit or miss)
2. CHECK: Has this enemy already been Fear-checked this combat?
   - IF yes: SKIP (once per enemy per combat)
3. ROLL: d100 vs. Fear Threshold
   - Rank 1: 25% (roll 1-25 = Fear triggers)
   - Rank 2: 40% (roll 1-40 = Fear triggers)
   - Rank 3: 50% (roll 1-50 = Fear triggers)
4. IF Fear triggers:
   a. APPLY: [Feared] status to attacker
      - Duration: 1 turn (Rank 1) / 2 turns (Rank 2-3)
   b. IF Rank 3:
      i. GENERATE: +5 Savagery
      ii. TRIGGER: Fear Spread (adjacent enemies)
   c. MARK: Enemy as "Fear-checked" for this combat
5. IF Fear does not trigger:
   a. MARK: Enemy as "Fear-checked" for this combat
   b. No further Fear checks against this enemy
```

### Fear Spread Resolution (Rank 3)

```jsx
1. TRIGGER: [Feared] applied to enemy
2. FIND: Enemies adjacent to newly-[Feared] enemy (1 hex)
3. FOR each adjacent enemy:
   a. CHECK: Already [Feared] or [Shaken]?
      - IF yes: SKIP
   b. ROLL: Enemy WILL save vs. DC 12 (-2 from Intimidation Aura)
   c. IF fail:
      i. APPLY: [Shaken] for 1 turn
      ii. [Shaken]: -1 to attack rolls, -1 to damage
   d. IF success:
      i. No effect
```

### Intimidation Aura (Rank 2+)

```jsx
1. PASSIVE: Always active
2. RANGE: 2 hexes from Aspirant
3. EFFECT: Enemies in range suffer WILL save penalty
   - Rank 2: -1 to WILL saves
   - Rank 3: -2 to WILL saves
4. STACKING: Does not stack with other Aspirants
```

---

## Worked Examples

### Example 1: Basic Fear Trigger (Rank 1)

```jsx
Blight-Touched Raider attacks Grimnir in melee.
├── Fear Check: First melee attack from this enemy
├── Fear Roll: d100 = 18 (≤25, FEAR TRIGGERS)
├── [Feared] Applied: 1 turn
│   ├── Raider cannot move toward Grimnir
│   ├── Raider suffers -2 to attack rolls
│   └── Raider has disadvantage on WILL saves
├── Raider marked "Fear-checked" (no more Fear checks this combat)
└── Result: Enemy neutered for 1 turn
```

### Example 2: Fear Fails, No Retry (Rank 1)

```jsx
Undying Sentinel attacks Grimnir in melee.
├── Fear Check: First melee attack from this enemy
├── Fear Roll: d100 = 67 (>25, NO FEAR)
├── Sentinel marked "Fear-checked"

Later: Sentinel attacks Grimnir again.
├── Fear Check: Already "Fear-checked"
├── SKIP: No new Fear roll
└── Result: Each enemy only gets one Fear check per combat
```

### Example 3: Fear + Savagery Generation (Rank 3)

```jsx
Corrupted Hound attacks Grimnir in melee.
├── Fear Roll: d100 = 32 (≤50, FEAR TRIGGERS)
├── [Feared] Applied: 2 turns
├── Savagery Bonus: +5 Savagery
├── Savagery: 40 → 45
├── Fear Spread Check: Adjacent Blight-Touched Raider
│   ├── WILL save: DC 12 - 2 (Aura) = DC 10
│   ├── Raider rolls: 8 (FAIL)
│   └── [Shaken] Applied: 1 turn
└── Result: Multi-target CC from single trigger + Savagery gain
```

### Example 4: Combined with Pain Fuels Savagery (Rank 3)

```jsx
Enemy hits Grimnir for 25 damage, Fear triggers.
├── Pain Fuels Savagery: 25 × 20% = 5 Savagery
├── Horrific Form: +5 Savagery (Fear triggered)
├── Total Savagery: +10 from single enemy attack
├── With Price of Power: 10 × 2.0 = +20 Savagery
└── Result: Getting hit generates massive Savagery from multiple sources
```

---

## Failure Modes

### Ranged Attack (No Trigger)

```jsx
Archer shoots Grimnir from 5 hexes away.
├── Attack Type: Ranged (not melee)
├── Horrific Form: Does NOT trigger
└── Result: Only melee attackers risk Fear
```

### Already Fear-Checked Enemy

```jsx
Raider attacks Grimnir (already Fear-checked earlier).
├── Fear Check: SKIP (already checked this combat)
└── Result: No additional Fear opportunities
```

### Fear-Immune Enemy

```jsx
Undying Automaton (boss) attacks Grimnir.
├── Fear Roll: d100 = 15 (would trigger)
├── Automaton: Immune to [Feared]
├── [Feared]: Resisted
└── Result: Some enemies cannot be Feared
```

### Multiple Aspirants (No Stacking)

```jsx
Two Aspirants in party, both have Intimidation Aura.
├── Aura 1: -2 WILL saves (Rank 3)
├── Aura 2: -2 WILL saves (Rank 3)
├── Combined: -2 WILL saves (does not stack)
└── Result: Only strongest aura applies
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Savage Strike:** Fear → target has -2 attack → safer Strike buildup
- **Pain Fuels Savagery:** Taking hit + Fear = double Savagery generation
- **The Price of Power:** +5 Savagery from Fear doubled to +10
- **Monstrous Apotheosis:** Immune to Fear yourself while inflicting it

### External Synergies

- **Skald (Saga of Dread):** Stacking Fear effects
- **Seiðkona (Terror):** Mental attack synergy
- **Tank Positioning:** Stand in front to maximize melee Fear triggers

### [Feared] Status Effects

| Effect | Description |
| --- | --- |
| Movement | Cannot willingly move toward Fear source |
| Attack Penalty | -2 to attack rolls |
| WILL Penalty | Disadvantage on WILL saves |
| Duration | 1 turn (R1) / 2 turns (R2-3) |

---

## Tactical Applications

1. **Melee Deterrent:** Enemies risk Fear when engaging you in melee
2. **Action Denial:** [Feared] enemies waste turns fleeing or attacking poorly
3. **Savagery Battery:** Rank 3 turns Fear triggers into resource generation
4. **Zone Control:** Intimidation Aura weakens all nearby enemies
5. **Crowd Control Chain:** Fear Spread can disable multiple enemies from one trigger

---

## v5.0 Compliance Notes

- **Tier 1 Structure:** R1→R2→R3 progression (free with spec, ranks via tree investment)
- **Rank 2 Trigger:** 2 Tier 2 abilities trained
- **Rank 3 Trigger:** Capstone trained
- **Once-Per-Enemy:** Prevents Fear-lock cheese; each enemy gets one check
- **No PP Cost:** Tier 1 abilities are granted free when specialization is unlocked