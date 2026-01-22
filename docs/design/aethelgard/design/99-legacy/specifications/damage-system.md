# Damage System — Mechanic Specification v5.0

Status: Proposed
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## I. Core Philosophy: The Process of Data-Loss

In the broken reality of Aethelgard, dealing damage is the act of causing **catastrophic data-loss** to a target's "system integrity" (Health Pool).

**The Conceptual Model:**

Damage in Aethelgard is not simply "hitting someone with a sword"—it is:

- **Corrupting data:** Destabilizing the target's coherent system structure
- **Forcing integrity loss:** Reducing the target's ability to maintain functional existence
- **Bypassing error correction:** Overwhelming the target's defensive protocols (Soak)

**Design Philosophy:**

The Damage System is a **multi-stage pipeline** that:

1. **Calculates raw potential:** What is the maximum damage this attack could deal?
2. **Applies mitigation:** How much does the target's defenses reduce that potential?
3. **Resolves final harm:** How much HP is actually lost?

**Why This Design?**

**Thematic Justification:**

- Reflects the "system crash" nature of combat in Aethelgard
- Armor doesn't make you harder to hit—it reduces damage (Soak, not Defense)
- Different damage types exploit different vulnerabilities (elemental weaknesses)

**Gameplay Benefits:**

- **Transparent:** Every step is visible to players (combat log shows roll → soak → final damage)
- **Tactical depth:** Players can optimize damage by exploiting resistances/vulnerabilities
- **Attribute identity:** MIGHT/FINESSE/WILL govern damage output, STURDINESS governs damage mitigation
- **Specialist identity:** Different builds interact with different pipeline stages

---

## II. Parent System

**Parent:** [Combat System — Core System Specification v5.0](Combat%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%204fdd9ec9ec974a75b45746d33e32a7d1.md)

**Relationship:**

The Damage System is a **child mechanic** of the Combat System. It is invoked AFTER the Accuracy Check System:

**Combat System Damage Pipeline:**

1. Accuracy Check System: Does attack hit? (Miss/Grazing/Solid/Critical)
2. **Damage System:** How much damage? (Roll → Mitigation → Final Damage)
3. Status Effect System: Apply additional effects (if applicable)

**Integration Point:**

The Combat System's `CombatEngine.ResolveAttack()` method calls:

1. `AccuracyService.CheckAccuracy()` → Returns hit quality
2. **`DamageService.CalculateDamage()`** → Returns final damage (this system)
3. `DamageService.ApplyDamage()` → Reduces target HP

---

*Migration in progress from v2.0 Damage System specification. Remaining sections to be added incrementally.*

1. `DamageService.ApplyDamage()` → Reduces target HP

---

## III. Trigger Conditions

**When is the Damage System invoked?**

The Damage System is triggered when an attack **successfully hits** a target (determined by the Accuracy Check System).

**Specific Trigger Criteria:**

**✅ Triggers Damage System:**

- Accuracy Check returned: GRAZING_HIT, SOLID_HIT, CRITICAL_HIT, or SYSTEM_EXPLOIT
- Attack deals HP damage (not purely status effect application)
- Attack is not flagged as `[No Damage]`

**❌ Bypasses Damage System:**

- Accuracy Check returned: MISS (attack failed to connect)
- Attack is flagged as `[No Damage]` (e.g., pure debuff abilities)
- Attack deals only Psychic Stress (not HP damage) — uses Trauma Economy instead

**Exception: Psychic Damage**

Psychic damage has a **unique resolution path**:

- Bypasses Accuracy Check entirely (no physical evasion possible)
- Uses **WILL-based Resolve Check** instead of Soak for mitigation
- May deal both HP damage AND Psychic Stress simultaneously

---

*Migration in progress. Next: Section IV - The Damage Pipeline.*

- May deal both HP damage AND Psychic Stress simultaneously

---

## IV. The Damage Pipeline: Step-by-Step Resolution

Every damaging attack follows a **four-step sequential pipeline**.

### Step 1: Build Damage Pool

**Formula (Physical Attacks):**

```
Damage Pool = Weapon Base Dice + Governing Attribute
```

**Governing Attribute:**

- **Melee (heavy):** MIGHT
- **Melee (finesse):** FINESSE
- **Ranged:** FINESSE

**Formula (Mystic Attacks):**

```
Damage Pool = Spell Base Dice + WILL
```

**Critical Hit Modifier:**

- If CRITICAL_HIT: Double the total dice pool

**Example:**

```
Kael (MIGHT 8) with Longsword (4d6)
Normal: 4d6 + 8d6 = 12d6
Critical: 12d6 × 2 = 24d6
```

---

### Step 2: Roll Damage Pool

**Process:**

1. Roll each die (d6)
2. Count successes (5 or 6 = success)
3. Total successes = **Incoming Damage Value**

**Example:**

```
Pool: 12d6
Results: [1, 5, 3, 6, 2, 5, 4, 6, 1, 5, 3, 6]
Successes: 6
Incoming Damage = 6
```

---

### Step 3: Apply Mitigation

**3a. Resistances/Vulnerabilities:**

- **Resistance:** Damage × 0.5
- **Vulnerability:** Damage × 2.0
- **Neutral:** Damage × 1.0

**3b. Subtract Soak:**

```
Soak = (STURDINESS ÷ 2) + Armor Soak + Bonuses
Mitigated Damage = Modified Damage - Soak
```

**3c. Hit Quality Modifier:**

- **GRAZING_HIT:** Damage × 0.5
- **SOLID_HIT+:** No modifier

---

### Step 4: Apply to HP

```
New HP = Current HP - Final Damage
```

**Minimum Damage Rule:** Successful hits always deal at least 1 damage (even if Soak > Incoming Damage).

---

*Migration in progress. Next: Section V - Damage Types.*

**Minimum Damage Rule:** Successful hits always deal at least 1 damage (even if Soak > Incoming Damage).

---

## V. Damage Types & Tactical Implications

Aethelgard features **eight damage types**, each with distinct tactical properties.

| Damage Type | Source Examples | Mitigated By | Special Properties |
| --- | --- | --- | --- |
| **Physical** | Weapons, claws, rocks | Soak | Most common; kinetic force |
| **Fire** | Spells, bombs, hazards | Soak | Thermal energy; may ignite |
| **Ice** | Spells, hazards | Soak | Thermal loss; may slow |
| **Lightning** | Spells, hazards | Soak | Electrical surge; may stun |
| **Poison** | Toxins, curses | Soak | Often damage-over-time |
| **Arcane** | Spells, curses, runes | Soak | Tainted Aether assault |
| **Psychic** | Mental assaults | WILL Resolve Check | **Bypasses Soak** entirely |
| **Sonic** | Screams, horns | Soak (partial) | **Armor Piercing** property |

### Key Tactical Notes

**Physical:** Standard damage type for all weapon attacks. Most reliable.

**Elemental (Fire/Ice/Lightning):** Exploit enemy vulnerabilities. Often apply status effects.

**Poison:** Excellent for sustained damage via DoT effects. Bypasses high-Soak over time.

**Arcane:** Rare damage type. Few enemies resist it.

**Psychic:** Ultimate tank-buster. Bypasses Soak entirely, targets WILL instead. Deals HP damage AND/OR Psychic Stress.

**Sonic:** Natural Armor Piercing (ignores 50% of Soak). Excellent vs. heavy armor.

---

*Migration in progress. Next: Section VI - Integration.*

**Sonic:** Natural Armor Piercing (ignores 50% of Soak). Excellent vs. heavy armor.

---

## VI. Integration & Systemic Identity

### System Dependencies

**Foundation Systems:**

- [AI Session Handoff — DB10 Specifications Migration](https://www.notion.so/AI-Session-Handoff-DB10-Specifications-Migration-065ef630b24048129cc560a393bd2547?pvs=21): Provides dice rolling and success counting
- [AI Session Handoff — DB10 Specifications Migration](https://www.notion.so/AI-Session-Handoff-DB10-Specifications-Migration-065ef630b24048129cc560a393bd2547?pvs=21): Provides MIGHT, FINESSE, WILL, STURDINESS

**Parent System:**

- [Combat System — Core System Specification v5.0](Combat%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%204fdd9ec9ec974a75b45746d33e32a7d1.md): Invokes Damage System after Accuracy Check

**Sibling Systems:**

- [Accuracy Check System — Mechanic Specification v5.0](Accuracy%20Check%20System%20%E2%80%94%20Mechanic%20Specification%20v5%20%20e0b01bf86d4141c7bc2e7131b3b650bc.md): Called before Damage System
- **Status Effect System:** Called after Damage System (if applicable)

**Referenced Systems:**

- **Equipment System:** Provides weapon damage dice, armor Soak values
- **Resistance System:** Modifies damage based on resistances/vulnerabilities
- [**Feature Specification: The Fury Resource System**](https://www.notion.so/Feature-Specification-The-Fury-Resource-System-2a355eb312da80768e66db52bdd7cf19?pvs=21): Psychic damage triggers Stress
- **HP System:** Damage reduces HP, triggers Bloodied/Defeated states

---

### Service Architecture

**Primary Service: `DamageService.cs`**

**Key Methods:**

```
CalculateDamagePool(attackerID, weaponData, hitQuality) → int
RollDamage(damagePool) → int
ApplyMitigation(incomingDamage, targetID, damageType, hitQuality) → int
ApplyDamage(targetID, finalDamage, damageType) → DamageResult
```

---

### Attribute Expression

The Damage Pipeline perfectly expresses combat attributes:

**FINESSE:** Determines if you hit (Accuracy Check)

**MIGHT/WILL:** Determines how hard you hit (Damage Pool)

**STURDINESS:** Determines how much you endure (Soak)

---

### Specialist Identity

Specializations interact with different pipeline stages:

**Rust-Witch:** Applies `[Corroded]` debuff, attacks Soak (Step 3)

**Hólmgangr:** Uses Parry to negate at Step 1 (before damage)

**Berserker:** Maximizes Damage Roll (Step 2)

**Echo-Caller:** Bypasses pipeline with Psychic damage

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 Damage System Feature Specification

**Target:** DB10 Damage System — Mechanic Specification v5.0

**Status:** ✅ Draft Complete

**All sections migrated:**

- ✅ I. Core Philosophy
- ✅ II. Parent System
- ✅ III. Trigger Conditions
- ✅ IV. The Damage Pipeline (4 steps)
- ✅ V. Damage Types & Tactical Implications
- ✅ VI. Integration & Systemic Identity

**TIER 2 Progress: 3/4 complete**

- ✅ Combat System (orchestrator)
- ✅ Accuracy Check System
- ✅ Damage System
- ⏳ Status Effect System (remaining)

**Next:** Begin Status Effect System migration to complete TIER 2.