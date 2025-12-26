# Sanctified Steel — Tier 1 Ability

Type: Ability
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-SANCTIFIEDSTEEL-v5.0
Mechanical Role: Damage Dealer
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
| **Ability Name** | Sanctified Steel |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | 1 (Foundational Vows) |
| **Type** | Active (Melee Attack) |
| **PP Cost** | 3 PP |
| **Stamina Cost** | 40 → 35 → 30 (by Rank) |
| **Target** | Single enemy (melee range) |

---

## Description

The Iron-Bane's blade is not merely a weapon—it is a **sacrament**. Forged from pure, newly-mined iron (never salvaged alloy), blessed by chapter priests, inscribed with prayers of purification. When it strikes corrupted metal, the impact is more than physical.

The blade *burns* with righteous purpose. Sparks fly not from friction, but from the collision of pure faith with corrupted code. Each strike is a prayer made manifest, and each prayer feeds the Iron-Bane's holy Vengeance.

> *"Every blow against the Undying is an act of worship."*
> 

---

## Mechanics

### Rank Progression

| Rank | Unlock Condition | Stamina | Damage | Vengeance Generation |
| --- | --- | --- | --- | --- |
| **Rank 1** | Train ability (3 PP) | 40 | Weapon + MIGHT | +15 vs Undying/Mech, +0 vs other |
| **Rank 2** | Train 2 Tier 2 abilities | 35 | Weapon + MIGHT + 1d10 | +18 vs Undying/Mech, +5 vs other |
| **Rank 3** | Train Capstone | 30 | Weapon + MIGHT + 2d10 (Holy) | +20 vs Undying/Mech, +8 vs other |

### Formula

```
Damage = WeaponBaseDice + MIGHT + BonusDice[Rank]
VengeanceGain = BaseVengeance[Rank] × FactionMultiplier

Where:
  BonusDice[R1] = 0
  BonusDice[R2] = 1d10
  BonusDice[R3] = 2d10 (Holy damage type)

  BaseVengeance[R1] = 15 (Undying) / 0 (Other)
  BaseVengeance[R2] = 18 (Undying) / 5 (Other)
  BaseVengeance[R3] = 20 (Undying) / 8 (Other)

  FactionMultiplier = 1.0 if Target.Faction ∈ {Undying, Mechanical}
                    = 0.0 (R1) or reduced (R2-R3) otherwise
```

### Resolution Pipeline

1. **Cost Payment:** Deduct Stamina (40/35/30 by rank)
2. **Attack Roll:** FINESSE + Weapon Skill vs target Defense
3. **Hit Determination:** Graze (partial) / Solid Hit / Critical Hit
4. **Damage Application:** Apply Weapon + MIGHT + bonus dice
5. **Faction Check:** Determine if target is Undying/Mechanical
6. **Vengeance Generation:** Add appropriate Vengeance based on faction and rank

---

## Worked Examples

### Example 1: Rank 1 vs Undying (Primary Use Case)

**Situation:** Grizelda (MIGHT 4, Vengeance 20) attacks Rusted Warden with longsword (2d10)

```
Stamina Cost: 40 → Current Stamina reduced
Attack Roll: FINESSE 3 + Weapon Skill 2 = 5d10 vs Defense 12
Roll: [9, 7, 5, 8, 6] → Solid Hit

Damage: 2d10 (weapon) + 4 (MIGHT) = [7, 5] + 4 = 16 Physical
Faction: Undying ✓
Vengeance: +15 → Current Vengeance: 20 + 15 = 35
```

### Example 2: Rank 2 vs Non-Undying

**Situation:** Same Grizelda attacks a Blighted Beast

```
Stamina Cost: 35 (reduced at R2)
Attack Roll: 5d10 vs Defense 10 → Solid Hit

Damage: 2d10 + 1d10 (R2 bonus) + 4 = [6, 8, 4] + 4 = 22 Physical
Faction: Beast (not Undying)
Vengeance: +5 (reduced rate) → Still generates some resource
```

### Example 3: Rank 3 Critical Hit vs Undying Elite

**Situation:** Mastery-rank Grizelda crits an Iron Hulk

```
Stamina Cost: 30 (minimum at R3)
Attack Roll: 6d10 vs Defense 14 → Critical Hit (double damage dice)

Damage: (2d10 + 2d10) × 2 + 4 = [8, 6, 9, 7] × 2 + 4 = 64 Physical/Holy
Faction: Undying ✓
Vengeance: +20 → Enables Corrosive Strike follow-up
Holy Damage: Bypasses some Undying resistances
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Miss** | Stamina spent, no damage, no Vengeance generated |
| **Graze** | Half damage, full Vengeance (if Undying) |
| **Insufficient Stamina** | Ability cannot be activated |
| **Out of Range** | Must be in melee; cannot target back row without reach weapon |
| **Non-Undying at R1** | Attack works but generates zero Vengeance |

---

## Tactical Applications

1. **Vengeance Engine:** Primary method of building Vengeance for spender abilities
2. **Opener:** Use in early combat rounds to accumulate resource before Corrosive Strike
3. **Efficient Spam:** Low Stamina cost at R3 (30) enables sustained pressure
4. **Faction Flexibility:** R2+ generates *some* Vengeance even vs non-Undying
5. **Holy Damage (R3):** Bypasses resistances on certain Undying variants

---

## Integration Notes

### Synergies

- **Corrosive Strike:** Vengeance generated here enables the debuff attack
- **Chains of Decay:** Accumulated Vengeance spent on AoE
- **Annihilate Iron Heart:** High Vengeance pool (75) required for execution
- **Purging Alacrity:** Follow-up attacks benefit from Defense bonus after debuff

### Anti-Synergies

- **Non-Undying Campaigns:** R1 generates zero Vengeance vs organics — severely limits power
- **Ranged Builds:** Requires melee range; no ranged option
- **Stamina-Poor Builds:** Even reduced cost (30) adds up over extended fights

### Combat Log Examples

```
> Grizelda's Sanctified Steel strikes the Rusted Warden! Solid Hit!
> The Warden takes 16 Physical damage.
> Her righteous blow fuels her crusade! Grizelda gains 15 Vengeance!
```

```
> Grizelda's Sanctified Steel strikes the Ash-Vargr! Solid Hit!
> The Ash-Vargr takes 14 Physical damage.
> (No Vengeance generated — target is not Undying)
```