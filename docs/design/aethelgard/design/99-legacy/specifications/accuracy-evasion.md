# Accuracy & Evasion — Mechanic Specification v5.0

Type: Mechanic
Description: Hit determination mechanics including attack dice pools, defense dice pools, success thresholds, fumble detection, bonus dice sources (flanking, elevation, analyzed, equipment), and tactical modifiers affecting accuracy.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Combat Resolution System, Dice Pool System, Attribute System, Equipment System, Tactical Grid System
Implementation Difficulty: Hard
Balance Validated: No
Document ID: AAM-SPEC-MECH-ACCURACY-v5.0
Parent item: Combat Resolution System — Core System Specification v5.0 (Combat%20Resolution%20System%20%E2%80%94%20Core%20System%20Specificati%20ed573bf38f6e42cca9de406c493efed5.md)
Proof-of-Concept Flag: No
Related Projects: (PROJECT) Game Specification Consolidation & Standardization (https://www.notion.so/PROJECT-Game-Specification-Consolidation-Standardization-e1d0c8b2ea2042f9b9c08471c6077c92?pvs=21)
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 3 (Technical)
Voice Validated: No

## Core Philosophy

The Accuracy & Evasion system determines hit or miss through opposed dice pool rolls, creating tactical uncertainty where no attack is guaranteed and no defense is perfect. Investment in offensive attributes increases reliability; investment in STURDINESS provides survivability.

> "The blade seeks flesh as water seeks the lowest ground—but flesh may move, may twist, may refuse the blade's argument."
> 

---

## Opposed Roll System

### Core Mechanic

```
Attack Roll:   Base Attribute + Accuracy Bonuses → Roll Xd6
Defense Roll:  STURDINESS → Roll Yd6
Net Successes: Attack Successes - Defense Successes
Result:        Net > 0 = HIT | Net ≤ 0 = MISS
```

**Success Definition:** Each die showing **5 or 6** counts as one success (33% per die).

### Tie-Breaking Rule

**Defender wins ties.** When net successes = 0, the attack misses.

---

## Attack Dice Pool Calculation

### Base Formula

```
Attack Dice = Base Attribute + Equipment + Status + Ability Bonuses
Attack Dice = CLAMP(Total, 1, 20)  // Min 1, Max 20
```

### Accuracy Bonus Sources

| Source | Bonus | Duration | Notes |
| --- | --- | --- | --- |
| **Equipment** | +0 to +3 | Permanent | Weapon accuracy stat |
| **[Analyzed]** | +2 | 2-3 turns | Target marked by ability |
| **Battle Rage** | +2 | 3 turns | Self-buff active |
| **Saga of Courage** | +2 | While performing | Skald performance |
| **Ability Bonus** | Varies | Next attack | Consumed after use |

### Example Calculation

```
Rogue attacks with FINESSE 6, +2 accuracy dagger, target [Analyzed]:
  Base FINESSE: 6d6
  Equipment:    +2
  [Analyzed]:   +2
  Total:        10d6
```

---

## Defense Dice Pool

### Simple Formula

```
Defense Dice = Defender's STURDINESS
```

**No modifiers apply to defense dice.** STURDINESS directly determines defensive capability.

| STURDINESS | Defense Dice | Avg Successes |
| --- | --- | --- |
| 3 | 3d6 | 1.0 |
| 5 | 5d6 | 1.67 |
| 7 | 7d6 | 2.33 |
| 10 | 10d6 | 3.33 |

---

## Hit Probability Matrix

| Attack ↓ / Defense → | 3d6 | 5d6 | 7d6 | 10d6 |
| --- | --- | --- | --- | --- |
| **3d6** | 50% | 30% | 15% | 5% |
| **5d6** | 70% | 50% | 30% | 15% |
| **7d6** | 85% | 70% | 50% | 30% |
| **10d6** | 95% | 85% | 70% | 50% |
| **12d6** (+bonuses) | 98% | 92% | 80% | 65% |
| **15d6** (+bonuses) | 99% | 96% | 88% | 75% |

---

## Accuracy Bonus Impact

| Bonus | Total Dice | Hit Chance vs 5d6 | Δ vs Baseline |
| --- | --- | --- | --- |
| +0 (baseline 6d6) | 6d6 | 58% | — |
| +2 | 8d6 | 75% | +17% |
| +4 | 10d6 | 87% | +29% |
| +6 | 12d6 | 93% | +35% |

**Key Insight:** Each +2 dice adds ~15-20% hit chance.

---

## STURDINESS Investment Value

| STURDINESS | Enemy Hit Chance (vs 6d6) | Enemy Miss Rate |
| --- | --- | --- |
| 2 | 77% | 23% |
| 3 | 67% | 33% |
| 5 | 50% | 50% |
| 7 | 33% | 67% |
| 10 | 13% | 87% |

**Recommendation:** Maintain STURDINESS 3-4 minimum to avoid excessive vulnerability.

---

## Resolution Pipeline

```
1. ATTACK DECLARATION
   └── Player/AI selects attack action and target

2. BONUS AGGREGATION
   ├── Base Attribute (MIGHT/FINESSE/WILL)
   ├── Equipment Accuracy Bonus
   ├── Status Effect Bonuses
   └── Ability Bonus Dice

3. ATTACK ROLL
   ├── Roll Attack Dice Pool
   └── Count successes (5-6 on each die)

4. DEFENSE ROLL
   ├── Roll STURDINESS dice
   └── Count successes (5-6 on each die)

5. NET SUCCESS CALCULATION
   ├── Net = Attack Successes - Defense Successes
   ├── Net > 0 → HIT (proceed to damage)
   └── Net ≤ 0 → MISS (combat log message)

6. CLEANUP
   └── Consume temporary bonuses (ability dice)
```

---

## Combat Log Display

### Hit Example

```
You attack Skeleton Warrior!
Calculating attack bonuses:
  Base FINESSE: 6d6
  Equipment: +2
  [Analyzed]: +2
  Total: 10d6

Rolled 10d6: [6, 5, 4, 6, 2, 5, 3, 1, 6, 5] = 6 successes
Skeleton Warrior defends!
Rolled 5d6: [3, 5, 2, 6, 4] = 2 successes
Net successes: 6 - 2 = 4 (HIT!)
Your attack lands!
```

### Miss Example

```
Rolled 5d6: [6, 5, 4, 3, 2] = 2 successes
Enemy defends!
Rolled 5d6: [5, 6, 4, 1, 3] = 2 successes
Net successes: 2 - 2 = 0 (MISS - Defender wins ties)
The attack is deflected!
```

---

## Database Schema

```sql
-- Attack roll tracking
CREATE TABLE AttackRolls (
    roll_id INTEGER PRIMARY KEY AUTOINCREMENT,
    combat_instance_id INTEGER NOT NULL,
    turn_number INTEGER NOT NULL,
    attacker_id INTEGER NOT NULL,
    target_id INTEGER NOT NULL,
    base_attribute TEXT CHECK(base_attribute IN ('MIGHT', 'FINESSE', 'WILL')),
    base_dice INTEGER NOT NULL,
    equipment_bonus INTEGER DEFAULT 0,
    status_bonuses INTEGER DEFAULT 0,
    ability_bonus INTEGER DEFAULT 0,
    total_attack_dice INTEGER NOT NULL,
    attack_successes INTEGER NOT NULL,
    defense_dice INTEGER NOT NULL,
    defense_successes INTEGER NOT NULL,
    net_successes INTEGER NOT NULL,
    hit_result INTEGER NOT NULL,  -- 1 = hit, 0 = miss
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (attacker_id) REFERENCES Characters(character_id),
    FOREIGN KEY (target_id) REFERENCES Characters(character_id)
);

-- Index for combat analysis
CREATE INDEX idx_attack_combat ON AttackRolls(combat_instance_id, turn_number);
CREATE INDEX idx_attack_hit_rate ON AttackRolls(attacker_id, hit_result);
```

---

## Service Integration

### IAccuracyService

```csharp
public interface IAccuracyService
{
    /// <summary>
    /// Calculates total attack dice pool from all sources.
    /// </summary>
    int CalculateAttackDicePool(Character attacker, Character target, Weapon weapon);
    
    /// <summary>
    /// Calculates defense dice pool (STURDINESS).
    /// </summary>
    int CalculateDefenseDicePool(Character defender);
    
    /// <summary>
    /// Rolls attack dice and counts successes.
    /// </summary>
    DiceResult RollAttack(int dicePool);
    
    /// <summary>
    /// Rolls defense dice and counts successes.
    /// </summary>
    DiceResult RollDefense(int dicePool);
    
    /// <summary>
    /// Determines hit/miss from net successes.
    /// </summary>
    bool DetermineHit(int attackSuccesses, int defenseSuccesses);
    
    /// <summary>
    /// Aggregates all accuracy bonuses for combat log display.
    /// </summary>
    AccuracyBreakdown GetAccuracyBreakdown(Character attacker, Character target, Weapon weapon);
}
```

---

## Worked Examples

### Example 1: Balanced Matchup

**Setup:** FINESSE 6, +2 weapon vs STURDINESS 5

```
Attack Pool: 6 + 2 = 8d6
Roll: [6, 5, 4, 6, 2, 5, 3, 1] = 4 successes

Defense Pool: 5d6
Roll: [3, 5, 2, 6, 4] = 2 successes

Net: 4 - 2 = 2 → HIT
```

### Example 2: Tie (Defender Wins)

**Setup:** FINESSE 5 vs STURDINESS 5

```
Attack Pool: 5d6
Roll: [6, 5, 4, 3, 2] = 2 successes

Defense Pool: 5d6
Roll: [5, 6, 4, 1, 3] = 2 successes

Net: 2 - 2 = 0 → MISS (tie)
```

### Example 3: Stacked Bonuses

**Setup:** MIGHT 10, +3 weapon, [Analyzed], Battle Rage

```
Attack Pool: 10 + 3 + 2 + 2 = 17d6
Roll: [6,5,4,6,2,5,3,1,6,5,4,6,2,5,3,1,6] = 9 successes

Defense Pool: 10d6 (tank)
Roll: [3,5,2,6,4,1,5,6,4,3] = 4 successes

Net: 9 - 4 = 5 → HIT (overwhelming)
```

### Example 4: Glass Cannon Vulnerability

**Setup:** Enemy 6d6 attack vs STURDINESS 2

```
Enemy Attack: 6d6
Roll: [6, 4, 5, 2, 1, 3] = 2 successes

Player Defense: 2d6
Roll: [3, 1] = 0 successes

Net: 2 - 0 = 2 → HIT (player takes damage)
```

---

## Balance Targets

### Hit Rate Targets

| Build Archetype | Target Hit Rate |
| --- | --- |
| Glass Cannon | 75-90% |
| Balanced Build | 50-65% |
| Tank (offensive) | 30-50% |

### Miss Rate Target

**Overall:** 30-40% of attacks should miss (prevents "rocket tag" combat).

### Investment Value

- **+1 Offensive Attribute:** ~+10% hit chance
- **+1 STURDINESS:** ~+8% enemy miss rate

---

## Migration Notes

**Source Documents:**

- SPEC-COMBAT-004: Accuracy & Evasion System Specification
- v2.0 Hit/Miss Mechanics Documentation
- CombatEngine.cs implementation (lines 130-251, 420-480)

**Implementation Status:** Ready for Alpha

**Estimated Implementation:** 6-8 hours