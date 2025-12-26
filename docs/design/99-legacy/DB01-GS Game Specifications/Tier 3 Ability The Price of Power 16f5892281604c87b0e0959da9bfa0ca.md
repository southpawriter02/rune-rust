# Tier 3 Ability: The Price of Power

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-THEPRICEOFPOWER-v5.0
Mechanical Role: Damage Dealer
Parent item: Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0 (Skar-Horde%20Aspirant%20(Augmented%20Brawler)%20%E2%80%94%20Speciali%20dcff21d0a06040698381b59039deaf60.md)
Proof-of-Concept Flag: No
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Validated: No

## Overview

| Property | Value | Property | Value |
| --- | --- | --- | --- |
| **Ability Name** | The Price of Power | **Ability Type** | Passive |
| **Tier** | 3 (Mastery) | **PP Cost** | 5 PP |
| **Specialization** | Skar-Horde Aspirant | **Ranks** | R2 → R3 |
| **Prerequisite** | All Tier 2 abilities trained | **Trauma Risk** | **High** (Psychic Stress generation) |

---

## Rank Progression

| Rank | Trigger | Key Unlock |
| --- | --- | --- |
| **Rank 2** | Train ability (5 PP) | +75% Savagery, 1 Stress per 10 Savagery |
| **Rank 3** | Train Capstone | +100% Savagery, +2 damage at 75+, Fear/Confused resist |

---

## Thematic Description

> *"The rush of transhuman power is intoxicating. The whispers in your mind grow louder. You don't care. Power is worth any price."*
> 

The Price of Power represents the Skar-Horde Aspirant's full embrace of their heretical path. Every surge of Savagery now comes with a cost—Psychic Stress that accumulates with each moment of augmented fury. The whispers grow louder, the visions darker, but the power... the power is undeniable. This ability is the defining choice that separates casual Aspirants from true believers.

---

## Mechanical Implementation

### Rank 2 (Foundation — 5 PP)

- **Effect:** Gain **75% more Savagery** from all sources
- **Cost:** Whenever you generate Savagery, gain **1 Psychic Stress per 10 Savagery generated**
- **Threshold Bonus:** When above 50 Savagery, gain **+1 to all damage rolls**

### Rank 3 (Mastery — Capstone Trained)

- **Effect:** Gain **100% more Savagery** from all sources (doubled)
- **Cost:** Whenever you generate Savagery, gain **1 Psychic Stress per 15 Savagery generated** (improved ratio)
- **Threshold Bonus:** When above 75 Savagery, gain **+2 to all damage rolls** (increased)
- **Mental Fortitude:** Gain +1 to saves against [Feared] and [Confused]

---

## Resolution Pipeline

### Savagery Amplification

```jsx
1. TRIGGER: Any Savagery generation event
2. SOURCE: Savage Strike, Pain Fuels Savagery, Horrific Form, etc.
3. CALCULATE: Base Savagery from source
4. AMPLIFY: Final Savagery = Base × Multiplier
   - Rank 2: Base × 1.75 (75% bonus)
   - Rank 3: Base × 2.0 (100% bonus)
5. CAP: Ensure Final Savagery does not exceed per-source caps
6. GENERATE: Add Final Savagery to current total
7. CAP: Ensure total does not exceed 100 (max)
8. CALCULATE: Stress = Final Savagery / Stress Ratio (round down)
   - Rank 2: Final Savagery / 10
   - Rank 3: Final Savagery / 15
9. APPLY: Add Stress to current Psychic Stress total
10. CHECK: Stress threshold warnings if applicable
```

### Damage Bonus Check

```jsx
1. TRIGGER: Any damage roll
2. CHECK: Current Savagery vs. Threshold
   - Rank 2: Savagery >= 50?
   - Rank 3: Savagery >= 75?
3. IF threshold met:
   a. ADD: +1/+2 to damage roll (by rank)
   b. OUTPUT: "The Price of Power: +X damage"
4. IF threshold not met:
   a. No bonus
```

### Stress Accumulation Examples

```jsx
Rank 2 (1 Stress per 10 Savagery):
├── 10 Savagery → 1 Stress
├── 17 Savagery → 1 Stress (rounds down)
├── 25 Savagery → 2 Stress
├── 43 Savagery → 4 Stress

Rank 3 (1 Stress per 15 Savagery):
├── 15 Savagery → 1 Stress
├── 25 Savagery → 1 Stress (rounds down)
├── 30 Savagery → 2 Stress
├── 50 Savagery → 3 Stress
```

---

## Worked Examples

### Example 1: Savage Strike with Price of Power (Rank 2)

```jsx
Grimnir (with The Price of Power R2) uses Savage Strike.
├── Base Savagery (Rank 3 Strike): 25
├── Price of Power: 25 × 1.75 = 43.75 → 43 Savagery
├── Savagery: 20 → 63 (+43)
├── Stress Calculation: 43 / 10 = 4.3 → 4 Psychic Stress
├── Stress: 10 → 14 (+4)
├── Threshold Check: 63 >= 50 → +1 damage active
└── Result: Strong Savagery gain with moderate Stress
```

### Example 2: Savage Strike with Price of Power (Rank 3)

```jsx
Grimnir (with The Price of Power R3) uses Savage Strike.
├── Base Savagery (Rank 3 Strike): 25
├── Price of Power: 25 × 2.0 = 50 Savagery
├── Savagery: 20 → 70 (+50)
├── Stress Calculation: 50 / 15 = 3.33 → 3 Psychic Stress
├── Stress: 10 → 13 (+3)
├── Threshold Check: 70 < 75 → No damage bonus yet
└── Result: Massive Savagery gain with improved Stress ratio
```

### Example 3: Pain Fuels Savagery + Price of Power Combo (Rank 3)

```jsx
Grimnir takes 40 damage.
├── Pain Fuels Savagery (Rank 3): 40 × 20% = 8 Savagery
├── Price of Power: 8 × 2.0 = 16 Savagery
├── Savagery: 50 → 66 (+16)
├── Stress Calculation: 16 / 15 = 1.07 → 1 Psychic Stress
├── Stress: 15 → 16 (+1)
└── Result: Taking damage generates significant Savagery with modest Stress
```

### Example 4: Damage Bonus at High Savagery (Rank 3)

```jsx
Grimnir has 80 Savagery, uses Savage Strike.
├── Savagery >= 75: YES → +2 damage bonus active
├── Base Damage: 2d10[7,6] + MIGHT[5] = 18 Physical
├── Price of Power Bonus: +2 damage
├── Total Damage: 18 + 2 = 20 Physical
└── Result: High Savagery rewards aggressive play
```

### Example 5: Full Combat Stress Accumulation (Rank 3)

```jsx
Combat starts: 0 Savagery, 5 Stress

Turn 1: Savage Strike hits
├── Savagery: 0 → 50 (+25 × 2.0)
├── Stress: 5 → 8 (+3)

Turn 2: Take 30 damage, Savage Strike hits
├── Pain Fuels Savagery: +12 (6 × 2.0)
├── Savage Strike: +50 (25 × 2.0)
├── Savagery: 50 → 100 (capped, would be 112)
├── Stress from PFS: +0 (12/15 rounds to 0)
├── Stress from Strike: +3 (50/15 = 3)
├── Stress: 8 → 11 (+3)

Turn 3: Use Grievous Wound (spends 30 Savagery, no generation)
├── Savagery: 100 → 70
├── Stress: No change (spending, not generating)

End of combat: 70 Savagery, 11 Stress
└── Result: 6 Stress accumulated from one fight
```

---

## Failure Modes

### Stress Overload (Breaking Point)

```jsx
Grimnir at 48 Stress, Savage Strike generates 3 more Stress.
├── Stress: 48 → 51
├── Breaking Point: 50
├── TRIGGER: Breaking Point reached
├── CONSEQUENCE: Trauma Economy resolution (varies by build)
└── Result: Power came at ultimate price—mental breakdown
```

### No Stress Management

```jsx
Party has no Bone-Setter or Stress mitigation.
├── Combat 1: +6 Stress
├── Combat 2: +8 Stress
├── Combat 3: +7 Stress
├── Total: +21 Stress in one dungeon
└── Result: Without healing, Breaking Point inevitable
```

### Savagery Cap Waste

```jsx
Grimnir at 95 Savagery, Savage Strike hits.
├── Would generate: 50 Savagery
├── Savagery: 95 → 100 (capped, 45 wasted)
├── Stress: Still calculated on 50 (full generation)
└── Result: Pay full Stress cost but lose Savagery overflow
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Savage Strike:** 25 → 43/50 Savagery per hit (by rank)
- **Pain Fuels Savagery:** 20% → 35%/40% effective conversion (amplified)
- **Horrific Form:** +5 → +8/+10 Savagery from Fear triggers
- **Monstrous Apotheosis:** Faster buildup to 75 Savagery threshold

### External Synergies (CRITICAL)

- **Bone-Setter:** **ESSENTIAL** — Cognitive Realignment manages Stress accumulation
- **Skald:** Sagas provide mental stability and Stress reduction
- **Seiðkona:** Can transfer or redirect Stress
- **Rest Mechanics:** Sanctuary Rest critical for long-term Stress management

### Counter-Interactions

- **Stress Threshold:** Mental breakdown if not managed
- **No Healer:** Extremely dangerous without Stress mitigation
- **Long Dungeons:** Stress accumulates faster than natural recovery

### Stress Generation Summary

| Source | Base | Rank 2 (×1.75) | R2 Stress | Rank 3 (×2.0) | R3 Stress |
| --- | --- | --- | --- | --- | --- |
| Savage Strike (R3) | 25 | 43 | 4 | 50 | 3 |
| Pain Fuels (30 dmg, R3) | 6 | 10 | 1 | 12 | 0 |
| Horrific Form (R3) | 5 | 8 | 0 | 10 | 0 |

---

## Tactical Applications

1. **Power Spike:** Dramatically accelerates Savagery economy
2. **Risk Management:** Requires party coordination (Bone-Setter essential)
3. **Build Defining:** Separates "safe" Aspirants from true power seekers
4. **Rank 3 Efficiency:** Better Stress ratio rewards full tree investment
5. **Damage Threshold:** Bonus damage at high Savagery rewards maintaining resource

---

## v5.0 Compliance Notes

- **Tier 3 Structure:** R2→R3 progression (starts at Rank 2 when trained)
- **Rank 3 Trigger:** Capstone trained
- **Trauma Economy Integration:** Core example of power-for-Stress trade
- **High Trauma Risk:** Explicit Psychic Stress generation—heretical design
- **Party Composition Requirement:** Demands healer/support for long-term viability