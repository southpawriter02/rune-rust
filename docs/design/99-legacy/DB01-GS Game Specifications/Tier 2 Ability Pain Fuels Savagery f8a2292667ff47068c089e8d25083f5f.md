# Tier 2 Ability: Pain Fuels Savagery

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-SKARHORDEAPIRANT-PAINFUELSSAVAGERY-v5.0
Mechanical Role: Damage Dealer
Parent item: Skar-Horde Aspirant (Augmented Brawler) — Specialization Specification v5.0 (Skar-Horde%20Aspirant%20(Augmented%20Brawler)%20%E2%80%94%20Speciali%20dcff21d0a06040698381b59039deaf60.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Passive
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## Overview

| Property | Value | Property | Value |
| --- | --- | --- | --- |
| **Ability Name** | Pain Fuels Savagery | **Ability Type** | Passive |
| **Tier** | 2 (Advanced) | **PP Cost** | 4 PP |
| **Specialization** | Skar-Horde Aspirant | **Resource Cost** | None (Passive) |
| **Prerequisite** | 8 PP in Skar-Horde tree | **Trauma Risk** | None (indirect via The Price of Power) |

---

## Thematic Description

> *"Every wound is fuel. Every blow against you is a gift. Pain is just another resource."*
> 

The Skar-Horde Aspirant has transcended the normal relationship between pain and weakness. Where others flinch, they lean in. Where others cry out, they smile. Pain isn't suffering—it's power, raw and immediate. Every blow that lands on their scarred flesh converts directly into the Savagery that fuels their devastating counterattacks.

---

## Mechanical Implementation

### Rank 2 (Foundation — 4 PP)

- **Trigger:** You take damage from any source
- **Effect:** Generate Savagery equal to **10% of damage taken**
- **Cap:** Maximum **20 Savagery per hit**
- **Exclusion:** Does not trigger on self-inflicted damage
- **DoT Interaction:** Triggers once per DoT tick

### Rank 3 (Mastery — With Capstone)

- **Conversion Rate:** 20% of damage taken (doubled from 10%)
- **Cap:** Maximum **30 Savagery per hit** (increased)
- **Defensive Scaling:** Gain **+1 Soak per 25 Savagery** you currently have
- **Snowball Potential:** More Savagery = More Soak = Survive longer = More Savagery

---

## Resolution Pipeline

### Damage-to-Savagery Conversion

```
1. TRIGGER: Aspirant takes damage (any source)
2. VALIDATE: Damage source is NOT self-inflicted
3. CALCULATE: Raw Savagery = Damage × Conversion Rate
   - Rank 2: Damage × 0.10
   - Rank 3: Damage × 0.20
4. CAP: Apply maximum per-hit cap
   - Rank 2: min(Raw Savagery, 20)
   - Rank 3: min(Raw Savagery, 30)
5. GENERATE: Add capped Savagery to current total
6. CAP: Ensure Savagery does not exceed 100 (max)
7. CHECK: The Price of Power active?
   a. IF yes: Generate Stress = Savagery Generated / 10 (round down)
8. CHECK: Rank 3 Soak bonus?
   a. IF yes: Recalculate Soak bonus = floor(Current Savagery / 25)
```

### Soak Bonus Calculation (Rank 3)

```
1. TRIGGER: Savagery total changes
2. CALCULATE: Soak Bonus = floor(Current Savagery / 25)
   - 0-24 Savagery: +0 Soak
   - 25-49 Savagery: +1 Soak
   - 50-74 Savagery: +2 Soak
   - 75-99 Savagery: +3 Soak
   - 100 Savagery: +4 Soak
3. UPDATE: Character Soak = Base Soak + Soak Bonus
```

---

## Worked Examples

### Example 1: Basic Conversion (Rank 2)

```
Grimnir takes 18 damage from Blight-Touched Raider.
├── Damage: 18
├── Conversion: 18 × 10% = 1.8 → 1 Savagery (rounded down)
├── Cap Check: 1 ≤ 20 (not capped)
├── Savagery: 25 → 26 (+1)
└── Result: Minor Savagery gain from moderate hit
```

### Example 2: Large Hit, Capped (Rank 2)

```
Grimnir takes 65 damage from Undying Juggernaut critical hit.
├── Damage: 65
├── Conversion: 65 × 10% = 6.5 → 6 Savagery
├── Cap Check: 6 ≤ 20 (not capped)
├── Savagery: 30 → 36 (+6)
└── Result: Heavy hit generates significant Savagery

Now consider a 250 damage hit:
├── Conversion: 250 × 10% = 25 Savagery
├── Cap Check: 25 > 20 → CAPPED to 20
└── Result: Massive hit still capped at 20 Savagery
```

### Example 3: Rank 3 with Soak Scaling

```
Grimnir (Rank 3) has 60 Savagery, takes 25 damage.
├── Current Soak Bonus: floor(60/25) = +2 Soak
├── Damage after Soak: 25 - 2 = 23 actual damage
├── Conversion: 23 × 20% = 4.6 → 4 Savagery
├── Savagery: 60 → 64 (+4)
├── New Soak Bonus: floor(64/25) = +2 Soak (unchanged)
└── Result: Higher Savagery provides damage reduction

Later, Savagery reaches 75:
├── New Soak Bonus: floor(75/25) = +3 Soak
└── Result: Snowball effect—more durable as fight continues
```

### Example 4: Combined with Horrific Form (Rank 3)

```
Enemy hits Grimnir for 30 damage. Horrific Form triggers Fear.
├── Pain Fuels Savagery: 30 × 20% = 6 Savagery
├── Horrific Form (Rank 3): Fear triggers = +5 Savagery
├── Total from single hit: +11 Savagery
├── Savagery: 50 → 61
└── Result: Getting hit generates massive Savagery from both passives
```

### Example 5: DoT Tick Interaction

```
Grimnir is affected by [Bleeding] (1d6/turn, 3 turns).
Turn 1: 1d6[4] = 4 damage
├── Conversion: 4 × 20% = 0.8 → 0 Savagery (rounded down)
└── Result: Small DoT ticks may generate 0 Savagery

Turn 2: 1d6[6] = 6 damage
├── Conversion: 6 × 20% = 1.2 → 1 Savagery
└── Result: Each tick triggers separately
```

---

## Failure Modes

### Self-Inflicted Damage (No Trigger)

```
Grimnir uses ability that costs HP or triggers self-damage.
├── Self-inflicted: YES
├── Pain Fuels Savagery: Does NOT trigger
└── Result: Cannot game the system with self-damage
```

### One-Shot Death

```
Grimnir takes 150 damage, has 80 HP.
├── Damage: 150
├── HP: 80 → -70 (DEAD)
├── Savagery Generated: Technically yes, but...
└── Result: Death prevents using generated Savagery
```

### Savagery Cap (100)

```
Grimnir has 95 Savagery, takes 50 damage.
├── Conversion: 50 × 20% = 10 Savagery
├── Savagery: 95 + 10 = 105 → CAPPED to 100
└── Result: 5 Savagery wasted (at cap)
```

---

## Synergies & Interactions

### Internal Synergies (Skar-Horde Tree)

- **Horrific Form:** Getting hit generates Savagery AND potentially Fears attacker
- **The Price of Power:** Up to 2x Savagery multiplier (but with Stress cost)
- **Savage Strike:** Alternate Savagery source when enemies avoid you
- **Grievous Wound/Overcharged Piston Slam:** More Savagery = more ability uses

### External Synergies

- **Berserkr:** Both reward taking damage in combat
- **Skjaldmær:** Can deliberately position to take hits while protected
- **Bone-Setter:** Controlled damage intake with healing support
- **Tank Builds:** Rank 3 Soak bonus enhances durability

### Counter-Interactions

- **One-Shots:** Dying before converting pain to power
- **Avoidance Builds:** Enemies that don't hit you generate no Savagery
- **Kiting Enemies:** Ranged enemies deny melee Savagery generation

### The Price of Power Interaction

```
With both Pain Fuels Savagery AND The Price of Power:
├── Base: 10%/20% conversion
├── Price of Power: +50%/75%/100% Savagery from all sources
├── Combined (Rank 3 both): 20% × 2.0 = 40% effective conversion
├── Example: 50 damage = 20 Savagery (capped at 30)
├── BUT: Also generates Stress per 10-15 Savagery
└── Result: Massive Savagery generation with Stress cost
```

---

## Tactical Applications

1. **Resource Engine:** Secondary Savagery generation alongside Savage Strike
2. **Aggressive Positioning:** Incentivizes trading hits rather than avoiding damage
3. **Healer Coordination:** "Keep me between 40-50% HP" maximizes Savagery without death risk
4. **Comeback Mechanic:** More damage taken = faster power accumulation
5. **Rank 3 Tank Hybrid:** Soak scaling enables off-tank capabilities at high Savagery

---

## v5.0 Compliance Notes

- **Risk/Reward Loop:** Core to Skar-Horde identity—pain is power
- **Scaling Pattern:** Foundation (conversion rate) → Mastery (doubled rate + defensive synergy)
- **Healer Tension:** Creates interesting party dynamics around HP management
- **Tier 2 Structure:** R2→R3 progression (unlocks at 8 PP, Rank 3 with Capstone)
- **Trauma Economy:** Indirect Stress via The Price of Power interaction