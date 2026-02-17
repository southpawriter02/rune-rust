# Annihilate Iron Heart — Capstone Ability

Type: Ability
Priority: Nice-to-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-IRONBANE-ANNIHILATEIRONHEART-v5.0
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
| **Ability Name** | Annihilate Iron Heart |
| **Specialization** | Iron-Bane (Zealous Purifier) |
| **Tier** | Capstone (Ultimate Purification) |
| **Type** | Active (Execution) |
| **PP Cost** | 6 PP |
| **Stamina Cost** | 60 |
| **Vengeance Cost** | 75 |
| **Target** | Single [Bloodied] Undying with [Iron_Heart_Exposed] |
| **Cooldown** | Once per combat |
| **Rank** | None (Capstone — no progression) |

---

## Description

The Iron-Bane has done the work. They have studied the enemy. They have stripped its armor with corrosive faith. They have bled it, weakened it, prepared it for the final sacrament.

Now comes the **Annihilation**.

The Iron-Bane speaks the True Name of Ending—the holiest word in their scripture, never spoken except in this moment. Their blade becomes a vessel of pure destruction. They strike not at the machine's body, but at its **core**—the iron heart that animates the corruption.

The heart does not break. It **ceases to exist**.

> *"In the name of the Pure Iron, I unmake you."*
> 

---

## Mechanics

### Prerequisites

| Requirement | Description | Source |
| --- | --- | --- |
| **Target Faction** | Must be [Undying] (not generic Mechanical) | Faction check |
| **Target Health** | Must be [Bloodied] (≤50% HP) | HP threshold |
| **Critical Insight** | Must have [Iron_Heart_Exposed] flag | Undying Insight R3 Critical Success |

### Effect Summary (No Ranks)

| Condition | Effect |
| --- | --- |
| **All Prerequisites Met + Hit** | **INSTANT DEATH** (execution, bypasses remaining HP) |
| **Prerequisites Met + Miss** | Resources spent, no effect |
| **Prerequisites NOT Met** | 10d10 + MIGHT×2 Physical (fallback damage) |
| **On Execution** | +30 Vengeance refunded |

### Formula

```
Prerequisites:
  TargetFaction == Undying
  TargetHP <= 50% (Bloodied)
  Target.HasFlag(Iron_Heart_Exposed) == true

If AllPrerequisitesMet AND AttackHit:
  Result = INSTANT_DEATH
  Refund = +30 Vengeance

If NOT AllPrerequisitesMet AND AttackHit:
  Damage = 10d10 + (MIGHT × 2) Physical
  DamageType = Irresistible (bypasses Soak)
  
Cost: 60 Stamina + 75 Vengeance
Cooldown: 1/combat (resets on Sanctuary Rest)
```

### Resolution Pipeline

1. **Prerequisite Validation:**
    - Target is Undying faction? (not generic Mechanical)
    - Target is [Bloodied] (≤50% HP)?
    - Target has [Iron_Heart_Exposed] flag? (from Undying Insight R3 crit)
2. **Resource Deduction:** Spend 60 Stamina + 75 Vengeance
3. **Attack Roll:** FINESSE + Weapon Skill vs target Defense
4. **Execution Branch (all prerequisites + hit):**
    - Target instantly destroyed (bypasses remaining HP)
    - Iron-Bane gains +30 Vengeance
5. **Fallback Branch (missing prerequisites + hit):**
    - Deal 10d10 + MIGHT×2 Irresistible Physical damage
    - No Vengeance refund
6. **Miss:** Resources spent, no effect

---

## Worked Examples

### Example 1: Perfect Execution

**Situation:** Grizelda executes prepared Iron Hulk boss

```
Setup (previous rounds):
  - Undying Insight R3: Critical Success → [Iron_Heart_Exposed] ✓
  - Combat damage: Iron Hulk at 45% HP → [Bloodied] ✓
  - Faction: Undying ✓

Execution Attempt:
  Cost: 60 Stamina, 75 Vengeance
  Attack Roll: 6d10 vs Defense 14 → Solid Hit

Result: ALL PREREQUISITES MET + HIT
  → INSTANT DEATH
  → Iron Hulk is destroyed regardless of remaining HP
  → Grizelda gains +30 Vengeance (refund)

Combat Log: "Grizelda speaks the True Name of Ending!
            Her blade pierces the Iron Hulk's core!
            The Iron Heart ceases to exist!
            The Iron Hulk is ANNIHILATED!"
```

### Example 2: Fallback Damage (Missing Flag)

**Situation:** Target is Bloodied but wasn't investigated

```
Prerequisites:
  - [Iron_Heart_Exposed]: MISSING (no critical investigation)
  - [Bloodied]: Yes (40% HP)
  - Faction: Undying

Attack: 6d10 vs Defense 12 → Solid Hit
Fallback Damage: 10d10 + (4 × 2) = [8,7,6,9,5,4,8,7,6,3] + 8 = 71 Physical
Damage Type: Irresistible (bypasses all Soak)

Result: 71 damage dealt (significant but not execution)
No Vengeance refund

Combat Log: "The strike lands with devastating force, but the 
            machine's heart remains hidden! 71 irresistible damage!"
```

### Example 3: Execution Parade (Multiple Targets)

**Situation:** Extended combat, multiple Undying prepared

```
Round 5: Annihilate Iron Hulk #1
  Prerequisites: All met ✓
  Result: EXECUTION → +30 Vengeance refund
  Grizelda Vengeance: 75 - 75 + 30 = 30

Round 6-7: Rebuild Vengeance
  Sanctified Steel × 2: +40 Vengeance
  Indomitable Will resist: +20 Vengeance
  Grizelda Vengeance: 30 + 40 + 20 = 90

Round 8: Annihilate Iron Hulk #2 (if cooldown allows next combat)
  Note: Once per combat — must wait for Sanctuary Rest
```

---

## Failure Modes

| Failure Type | Result |
| --- | --- |
| **Missing [Iron_Heart_Exposed]** | Fallback damage only; no execution |
| **Target Not [Bloodied]** | Fallback damage only; weaken first |
| **Wrong Faction** | Cannot target; command rejected (Undying only) |
| **Attack Misses** | Resources spent, no effect (devastating waste) |
| **Cooldown Active** | Cannot use again until Sanctuary Rest |
| **Insufficient Resources** | Cannot activate; need 60 Stamina AND 75 Vengeance |

---

## Tactical Applications

1. **Boss Slayer:** End high-HP Undying bosses instantly with proper setup
2. **Resource Refund:** +30 Vengeance on execution partially recoups cost
3. **Combo Finisher:** Investigate → Corrosive Strike → Damage to Bloodied → Annihilate
4. **Party Coordination:** Allies help apply damage to reach [Bloodied] threshold
5. **Action Economy:** One action to bypass potentially hundreds of remaining HP

---

## Integration Notes

### The Complete Iron-Bane Loop

```
1. INVESTIGATE (Undying Insight R3)
   → Critical Success → [Iron_Heart_Exposed] flag applied

2. BUILD VENGEANCE (Sanctified Steel)
   → +15-20 per hit vs Undying
   → Target: 75+ Vengeance

3. STRIP DEFENSES (Corrosive Strike)
   → [Corroded] reduces Soak
   → Party deals more damage

4. DAMAGE TO BLOODIED (Party coordination)
   → Reduce target to ≤50% HP
   → [Bloodied] threshold met

5. EXECUTE (Annihilate Iron Heart)
   → All prerequisites met
   → INSTANT DEATH
```

### Synergies

- **Undying Insight R3:** REQUIRED — provides [Iron_Heart_Exposed] flag
- **Corrosive Strike:** Helps party damage target to [Bloodied]
- **Chains of Decay:** AoE softens multiple targets for execution parade
- **Party Damage:** Coordinate to push target below 50% HP

### Anti-Synergies

- **Fast Party Kills:** If party kills before setup, execution wasted
- **Low WITS:** Need Critical Success on investigation — harder without WITS
- **Vengeance Starvation:** 75 Vengeance is expensive; competes with Chains (40)
- **Miss Risk:** High-stakes attack roll; miss = 135 resources wasted

### Boss Modifier (Optional Rule)

Some elite Undying bosses may have **Iron Heart Immunity**:

- First successful Annihilate attempt removes immunity (deals fallback damage)
- Second successful attempt can execute normally
- Represents bosses with backup power cores or redundant systems

### Combat Log Example

```
> Grizelda's eyes glow with cold fury. The Warden's Iron Heart 
> is exposed! She channels all of her Vengeance into a single, 
> perfect strike!
> She unleashes Annihilate Iron Heart!
> The blow lands with the sound of shattering glass and a dying star!
> The attack bypasses all defenses!
> The Rusted Warden is ANNIHILATED!
> Grizelda's righteous victory restores 30 Vengeance!
```