# Corrosive Strike — Tier 2 Ability

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-CORROSIVESTRIKE-v5.0
Mechanical Role: Controller/Debuffer, Damage Dealer
Parent item: Iron-Bane (Zealous Purifier) — Specialization Specification v5.0 (Iron-Bane%20(Zealous%20Purifier)%20%E2%80%94%20Specialization%20Spec%20c2718eab17e04443af19f9da976f4ad3.md)
Proof-of-Concept Flag: No
Resource System: Stamina, Vengeance
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Core Identity

| Property | Value |
| --- | --- |
| **Ability Name** | Corrosive Strike |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | 2 (Advanced Extermination) |
| **Type** | Active (Melee Attack + Debuff) |
| **PP Cost** | 4 PP |
| **Stamina Cost** | 45 → 40 (by Rank) |
| **Vengeance Cost** | 25 → 20 (by Rank) |
| **Target** | Single Undying/Mechanical enemy (melee) |

---

## Description

The Iron-Bane's blade doesn't just cut—it **purifies**. When it strikes Undying metal, the sanctified edge catalyzes a reaction in the corrupted alloy. Rust blooms from the wound. Structural integrity fails. The machine's armor begins to *consume itself*.

This is not magic. This is **metallurgy weaponized by faith**. The Iron-Bane knows exactly where to strike, exactly how to angle the blade, to trigger the corrosive chain reaction that will reduce their enemy to ruin.

> *"I don't break armor. I teach it to destroy itself."*
> 

---

## Mechanics

### Rank Progression

| Rank | Unlock Condition | Stamina | Vengeance | Damage | [Corroded] Effect |
| --- | --- | --- | --- | --- | --- |
| **Rank 2** | Train ability (4 PP) | 45 | 25 | 4d10 + MIGHT | -3 Soak, 3 rounds |
| **Rank 3** | Train Capstone | 40 | 20 | 5d10 + MIGHT | -4 Soak, 4 rounds; applies on Graze |

### Formula

```
Damage = DamageDice[Rank] + MIGHT
CorrodedSoak = -SoakReduction[Rank]
CorrodedDuration = Duration[Rank]

Where:
  DamageDice[R2] = 4d10
  DamageDice[R3] = 5d10
  
  SoakReduction[R2] = 3
  SoakReduction[R3] = 4
  
  Duration[R2] = 3 rounds
  Duration[R3] = 4 rounds

Condition: Target.Faction ∈ {Undying, Mechanical}
Stacking: [Corroded] stacks up to 2× (max -8 Soak at R3)
```

### Resolution Pipeline

1. **Target Validation:** Confirm target is Undying/Mechanical (fails otherwise)
2. **Resource Deduction:** Spend Stamina (45/40) + Vengeance (25/20)
3. **Attack Roll:** FINESSE + Weapon Skill vs target Defense
4. **Hit Determination:** Graze / Solid / Critical
5. **Damage Application:** Apply 4d10/5d10 + MIGHT Physical
6. **Debuff Application:**
    - R2: Apply [Corroded] on Solid Hit or better
    - R3: Apply [Corroded] on ANY successful hit (including Graze)

---

## Worked Examples

### Example 1: Rank 2 — Standard Armor Strip

**Situation:** Grizelda (MIGHT 4, Vengeance 35) attacks Iron Husk (Soak 8)

```
Cost: 45 Stamina, 25 Vengeance → Vengeance now 10
Attack Roll: 5d10 vs Defense 13 → Solid Hit

Damage: 4d10 + 4 = [8, 6, 9, 5] + 4 = 32 Physical
Before Soak: 32 damage
After Soak (8): 24 damage dealt

[Corroded] Applied: -3 Soak for 3 rounds
Iron Husk effective Soak: 8 - 3 = 5

Next attack deals: 32 - 5 = 27 damage (+3 from Soak reduction)
```

### Example 2: Rank 3 — Graze Still Applies Debuff

**Situation:** Grizelda barely connects against an agile Sentinel

```
Cost: 40 Stamina, 20 Vengeance
Attack Roll: 5d10 vs Defense 16 → Graze (marginal hit)

Damage: 5d10 + 4 = [7, 4, 8, 6, 3] ÷ 2 + 4 = 18 Physical (half dice on Graze)
Result: 18 damage (reduced by Graze)

R3 Special: [Corroded] STILL applies on Graze
[Corroded]: -4 Soak for 4 rounds
Party can now burst the Sentinel with reduced defenses
```

### Example 3: Stacking Corroded for Maximum Effect

**Situation:** Two Corrosive Strikes against boss (Soak 12)

```
Strike 1 (R3): Solid Hit
  Damage: 5d10 + 4 = 33 Physical → 33 - 12 = 21 dealt
  [Corroded] Stack 1: -4 Soak
  Boss Soak: 12 - 4 = 8

Strike 2 (R3): Solid Hit  
  Damage: 5d10 + 4 = 29 Physical → 29 - 8 = 21 dealt
  [Corroded] Stack 2: -4 Soak (stacks to max 2)
  Boss Soak: 12 - 8 = 4

Total Soak Reduction: -8 (maximum stack)
Boss now takes nearly full damage from all party attacks
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Wrong Faction** | Ability cannot be used; command rejected |
| **Miss** | Resources spent, no damage, no [Corroded] |
| **Graze at R2** | Half damage dealt, [Corroded] does NOT apply |
| **Insufficient Resources** | Ability cannot activate; need both Stamina AND Vengeance |
| **Stack Cap** | Third+ application refreshes duration but doesn't increase Soak reduction beyond -8 |

---

## Tactical Applications

1. **Tank-Buster:** Strip Soak from heavily armored Undying elites and bosses
2. **Party Force Multiplier:** Reduced Soak benefits ALL physical damage dealers
3. **Setup for Annihilate:** [Corroded] target is one prerequisite closer to execution
4. **Stack Priority:** Two applications on boss = -8 Soak = devastating vulnerability
5. **R3 Reliability:** Graze application ensures debuff even on near-misses

---

## Integration Notes

### Synergies

- **Sanctified Steel:** Builds the Vengeance spent here
- **Chains of Decay:** AoE alternative for applying [Corroded] to groups
- **Annihilate Iron Heart:** [Corroded] target setup for execution
- **Purging Alacrity:** Applying [Corroded] triggers +3 Defense buff
- **Rust-Witch:** Combined corrosive effects devastate Undying groups

### Anti-Synergies

- **Non-Undying Enemies:** Cannot be used at all against organic targets
- **Vengeance Starvation:** Competes with other Vengeance spenders
- **Low-Soak Targets:** Overkill against lightly armored enemies

### The [Corroded] Status Effect

```
[Corroded]
- Soak reduced by 3-4 (rank dependent)
- Duration: 3-4 rounds
- Stacks up to 2 times
- DoT: 1d10 Arcane damage per round (represents structural decay)
- Cannot be cleansed by standard methods (requires specialized repair)
```

### Combat Log Example

```
> Grizelda's blade glows with an entropic light as she unleashes 
> a Corrosive Strike!
> The strike hits the Rusted Warden! Solid Hit!
> The Warden takes 28 Physical damage, and its armor begins to 
> flake away into rust! It is now [Corroded]!
> (Warden Soak reduced by 3 for 3 rounds)
```