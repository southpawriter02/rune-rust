# Damage Calculation — Mechanic Specification v5.0

Type: Mechanic
Description: Comprehensive damage calculation mechanics including base damage formulas, attribute scaling, weapon damage dice, critical hits, armor soak, damage types, resistances/vulnerabilities, and damage reduction from defensive actions.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Combat Resolution System, Dice Pool System, Attribute System, Equipment System
Implementation Difficulty: Hard
Balance Validated: No
Document ID: AAM-SPEC-MECH-DAMAGE-v5.0
Parent item: Combat Resolution System — Core System Specification v5.0 (Combat%20Resolution%20System%20%E2%80%94%20Core%20System%20Specificati%20ed573bf38f6e42cca9de406c493efed5.md)
Proof-of-Concept Flag: No
Related Projects: (PROJECT) Game Specification Consolidation & Standardization (https://www.notion.so/PROJECT-Game-Specification-Consolidation-Standardization-e1d0c8b2ea2042f9b9c08471c6077c92?pvs=21)
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Layer: Layer 3 (Technical)
Voice Validated: No

## Core Philosophy

Damage calculation transforms successful attacks into meaningful HP reduction through a layered system of modifiers, multipliers, and mitigation. Every damage number tells a story of tactical choices made by both attacker and defender.

> "The blade does not lie. Its edge speaks only truth—measured in blood and bone, counted by the observers who survive."
> 

---

## Damage Pipeline Overview

```
1. CHECK HIT (from Accuracy System)
   └── Net Successes > 0? → Continue
   └── Net Successes ≤ 0? → DEFLECTED (0 damage)

2. ROLL BASE DAMAGE
   ├── Weapon Dice (e.g., 3d6)
   ├── [Inspired] Active? → +3 Dice
   ├── Critical Hit? → Double Dice
   └── Add Flat Bonus (e.g., +3)

3. APPLY STANCE BONUS
   └── Aggressive Stance: +4 flat damage

4. APPLY STATUS MODIFIERS
   ├── [Vulnerable] on target: ×1.25
   └── [Defensive Stance] on attacker: ×0.75

5. APPLY MITIGATION
   ├── Defense Bonus: ×(1 - DefenseBonus/100)
   ├── Soak: -Flat Reduction
   └── IgnoresArmor? → Skip Both

6. MINIMUM DAMAGE RULE
   └── Final = Max(1, Calculated)

7. APPLY TO HP
   ├── target.HP -= damage
   └── HP ≤ 0? → Death
```

---

## Base Damage Calculation

### Weapon Damage Formula

```
Base Damage = Roll(DamageDice) + DamageBonus
```

### Standard Weapons

| Weapon | Dice | Bonus | Avg | Range | Attribute |
| --- | --- | --- | --- | --- | --- |
| Unarmed | 1d6 | -2 | 1.5 | 1*–4 | MIGHT |
| Dagger | 2d6 | +2 | 9 | 4–14 | FINESSE |
| Longsword | 3d6 | +3 | 13.5 | 6–21 | MIGHT |
| Greatsword | 4d6 | +5 | 19 | 9–29 | MIGHT |
| Atgeir | 3d6 | +4 | 14.5 | 7–22 | FINESSE |
| Staff (Mystic) | 2d6 | +0 | 7 | 2–12 | WILL |
| Thunder Hammer | 4d6 | +6 | 20 | 10–30 | MIGHT |

*\*Unarmed minimum is 1 due to minimum damage rule*

### Quality Tier Modifiers

| Quality | Dice Mod | Bonus Mod | Longsword Example |
| --- | --- | --- | --- |
| Jury-Rigged | -1d6 | -1 | 2d6+2 (avg 9) |
| Scavenged | — | — | 3d6+3 (avg 13.5) |
| Clan-Forged | +1d6 | +1 | 4d6+4 (avg 18) |
| Optimized | +1d6 | +2 | 4d6+5 (avg 19) |
| Myth-Forged | +2d6 | +3 | 5d6+6 (avg 23.5) |

---

## Status Effect Modifiers

### Offensive Modifiers

| Status | Effect | Application | Typical Value |
| --- | --- | --- | --- |
| **[Vulnerable]** | ×1.25 (+25%) | On target | +3-5 damage per hit |
| **[Inspired]** | +3 damage dice | On attacker | +10.5 avg damage |
| **[Defensive Stance]** | ×0.75 (-25%) | On attacker | -3-5 damage dealt |
| **Aggressive Stance** | +4 flat damage | On attacker | +4 per attack |

### Modifier Stacking Order

1. **[Inspired]** — Add dice before rolling
2. **Critical Hit** — Double dice before rolling
3. **Roll Damage** — Sum all dice + flat bonus
4. **Stance Bonus** — Add flat bonus
5. **[Vulnerable]** — Multiply by 1.25, floor result
6. **[Defensive Stance]** — Multiply by 0.75, floor result

---

## Damage Mitigation

### Defense Bonus (Active Defense)

| Defend Successes | Defense Bonus | Damage Reduction |
| --- | --- | --- |
| 1 success | 25% | ×0.75 |
| 2 successes | 50% | ×0.50 |
| 3+ successes | 75% (cap) | ×0.25 |

**Formula:**

```
Reduced = Damage × (1 - DefenseBonus / 100)
```

### Soak (Armor Absorption)

| Armor Tier | Soak Value | Enemy Examples |
| --- | --- | --- |
| None | 0 | Blighted Wanderers |
| Light | 1-3 | Rust Stalkers, Raiders |
| Medium | 4-7 | Haugbui, Iron-Bane Sentinels |
| Heavy | 8-12 | Draugr Lords, Ancient Constructs |

**Formula:**

```
Final = ReducedDamage - Soak
```

### Ignores Armor (Heretical Abilities)

Abilities with `IgnoresArmor = true` bypass **both** Defense Bonus and Soak.

| Ability | Dice | Avg Damage | Notes |
| --- | --- | --- | --- |
| Void Strike | 3d8 | 13.5 | Ignores all mitigation |
| Psychic Lash | 2d6 | 7 | Ignores all mitigation |
| Desperate Gambit | 4d10 | 22 | AOE, ignores armor |

---

## Critical Hit Mechanics

### Critical Hit Determination

```
Crit Chance = 5% (base) + Flanking Bonus
If Random() < CritChance → CRITICAL HIT
```

### Critical Hit Effect

- **Double damage dice** (not flat bonuses)
- Longsword 3d6+3 → Critical 6d6+3

| Weapon | Normal | Critical | Damage Increase |
| --- | --- | --- | --- |
| Dagger (2d6+2) | avg 9 | avg 16 | +77% |
| Longsword (3d6+3) | avg 13.5 | avg 24 | +78% |
| Greatsword (4d6+5) | avg 19 | avg 33 | +74% |

### Flanking Bonus to Crit

| Flanking Condition | Crit Bonus | Total Chance |
| --- | --- | --- |
| No flanking | +0% | 5% |
| 2 allies adjacent | +10% | 15% |
| 3+ allies adjacent | +15% | 20% |

---

## Minimum Damage Rule

**Rule:** Successful attacks (net successes > 0) always deal at least **1 damage**.

```
IF NetSuccesses > 0:
    FinalDamage = Max(1, CalculatedDamage)
ELSE:
    FinalDamage = 0  // Deflected
```

**Purpose:** Prevents attacks from feeling completely nullified against high-armor enemies.

---

## Trauma Economy Integration

### Stress from Damage Events

| Event | Stress | Notes |
| --- | --- | --- |
| Take damage > 25% HP | +5 | Significant wound |
| Take damage > 50% HP | +10 | Critical wound |
| Deal killing blow | -3 | Combat validation |
| Overkill damage | -5 | Overwhelming force |

---

## Database Schema

```sql
-- Damage event tracking
CREATE TABLE DamageEvents (
    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
    combat_instance_id INTEGER NOT NULL,
    turn_number INTEGER NOT NULL,
    attacker_id INTEGER NOT NULL,
    target_id INTEGER NOT NULL,
    attack_type TEXT CHECK(attack_type IN ('Weapon', 'Ability', 'Heretical', 'Environmental')),
    base_damage INTEGER NOT NULL,
    status_modifier REAL DEFAULT 1.0,
    defense_bonus_reduction INTEGER DEFAULT 0,
    soak_reduction INTEGER DEFAULT 0,
    final_damage INTEGER NOT NULL,
    was_critical INTEGER DEFAULT 0,
    ignored_armor INTEGER DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (attacker_id) REFERENCES Characters(character_id),
    FOREIGN KEY (target_id) REFERENCES Characters(character_id)
);

-- Index for combat analysis
CREATE INDEX idx_damage_combat ON DamageEvents(combat_instance_id, turn_number);
CREATE INDEX idx_damage_attacker ON DamageEvents(attacker_id);
```

---

## Service Integration

### IDamageCalculationService

```csharp
public interface IDamageCalculationService
{
    /// <summary>
    /// Calculates base weapon damage from dice roll and bonus.
    /// </summary>
    int CalculateBaseDamage(Weapon weapon, bool isCritical, bool hasInspired);
    
    /// <summary>
    /// Applies status effect modifiers to base damage.
    /// </summary>
    int ApplyStatusModifiers(int baseDamage, Character attacker, Character target);
    
    /// <summary>
    /// Applies mitigation (Defense Bonus + Soak) unless ignores armor.
    /// </summary>
    int ApplyMitigation(int damage, Character target, bool ignoresArmor);
    
    /// <summary>
    /// Enforces minimum damage rule for successful hits.
    /// </summary>
    int ApplyMinimumDamageRule(int damage, int netSuccesses);
    
    /// <summary>
    /// Full damage pipeline from base roll to final HP reduction.
    /// </summary>
    DamageResult CalculateDamage(AttackContext context);
}
```

---

## Worked Examples

### Example 1: Standard Attack

**Setup:** Longsword (3d6+3), no modifiers, vs enemy with Soak 3

```
Base Damage: Roll 3d6 = [4, 6, 2] = 12 + 3 = 15
Status Mods: None (×1.0)
Mitigation: 15 - 3 (Soak) = 12
Minimum: Max(1, 12) = 12
→ Enemy takes 12 damage
```

### Example 2: [Vulnerable] + Defense Bonus

**Setup:** Dagger (2d6+2), target [Vulnerable], target defending (50%)

```
Base Damage: Roll 2d6 = [5, 6] = 11 + 2 = 13
Status Mods: 13 × 1.25 = 16.25 → 16
Defense Bonus: 16 × 0.50 = 8
→ Enemy takes 8 damage
```

### Example 3: Critical Hit + [Inspired]

**Setup:** Longsword (3d6+3), [Inspired], critical hit

```
Dice: 3d6 base + 3 [Inspired] = 6d6
Critical: 6d6 × 2 = 12d6
Roll: 12d6 = 42 + 3 = 45
→ Enemy takes 45 damage (devastating)
```

### Example 4: Heretical Ability vs Heavy Armor

**Setup:** Void Strike (3d8), vs Draugr Lord (Defense 75%, Soak 10)

```
Base Damage: Roll 3d8 = [7, 5, 6] = 18
IgnoresArmor: TRUE
Skip Defense Bonus: ✓
Skip Soak: ✓
→ Enemy takes 18 damage (full, unmitigated)
```

---

## Balance Targets

### Time-to-Kill (TTK)

| Enemy HP | Dagger | Longsword | Greatsword |
| --- | --- | --- | --- |
| 30 HP | 4 turns | 3 turns | 2 turns |
| 50 HP | 6 turns | 4 turns | 3 turns |
| 100 HP | 12 turns | 8 turns | 6 turns |

**Target:** 3-6 turns for 50 HP standard enemy

### Weapon Progression Feel

- Each tier should feel **30-40% more powerful**
- Dagger → Longsword: +50% damage ✓
- Longsword → Greatsword: +41% damage ✓

---

## Migration Notes

**Source Documents:**

- SPEC-COMBAT-002: Damage Calculation System Specification
- v2.0 Damage Mechanics Documentation
- CombatEngine.cs implementation (lines 215-450)

**Implementation Status:** Ready for Alpha

**Estimated Implementation:** 8-12 hours