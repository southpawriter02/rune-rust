# Counter-Attack & Parry — Mechanic Specification v5.0

Type: Mechanic
Description: Reactive defense system including universal parry mechanics, parry pool calculation (FINESSE + Weapon Skill + Bonus Dice), four-tier outcome system (Failed/Standard/Superior/Critical), riposte triggers, Hólmgangr Reactive Parry specialization, Atgeir-wielder bonuses, and Trauma Economy stress integration.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Combat Resolution System, Dice Pool System, Attribute System, Equipment System, Trauma Economy System, Specialization System
Implementation Difficulty: Hard
Balance Validated: No
Document ID: AAM-SPEC-MECH-COUNTERATTACK-v5.0
Parent item: Combat Resolution System — Core System Specification v5.0 (Combat%20Resolution%20System%20%E2%80%94%20Core%20System%20Specificati%20ed573bf38f6e42cca9de406c493efed5.md)
Proof-of-Concept Flag: No
Related Projects: (PROJECT) Game Specification Consolidation & Standardization (https://www.notion.so/PROJECT-Game-Specification-Consolidation-Standardization-e1d0c8b2ea2042f9b9c08471c6077c92?pvs=21)
Resource System: Stamina
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Layer: Layer 3 (Technical)
Voice Validated: No

## Core Philosophy

Counter-attacks transform defensive play from passive damage mitigation into **aggressive punishing** where pattern recognition and timing create offense from defense.

> "A parry is not a simple block; it is an act of logical causality. By interfering with an attack at the exact moment of its execution, a parry rewrites the causal outcome."
> 

---

## Parry System

### Parry Pool Calculation

```
Parry Pool = FINESSE + Weapon Skill + Modifiers
```

**Modifier Sources:**

- Specialization bonuses (Hólmgangr, Atgeir-wielder)
- Equipment bonuses (parrying daggers, shield)
- Stance bonuses (Defensive Stance: +1d10)
- Status effects (Analyzed: -1d10 to attacker)

### Parry Outcomes

| Outcome | Condition | Effect |
| --- | --- | --- |
| **Failed** | Parry < Accuracy | Attack hits normally |
| **Standard** | Parry = Accuracy | Attack negated, no Riposte |
| **Superior** | Parry > Accuracy | Attack negated, Hólmgangr Ripostes |
| **Critical** | Parry > Accuracy by 5+ | Attack negated, ALL characters Riposte |

### Action Economy

- **Parry:** Reaction (once per round; Hólmgangr Rank 3: twice per round)
- **Riposte:** Free basic melee attack, cannot be parried
- **Timing:** Declared after attack roll, before damage resolution

---

## Specialization Bonuses

| Specialization | Parry Bonus | Special Ability |
| --- | --- | --- |
| **Hólmgangr (Rank 1)** | +1d10 | Superior Parry triggers Riposte |
| **Hólmgangr (Rank 2)** | +2d10 | Riposte deals +1d6 bonus damage |
| **Hólmgangr (Rank 3)** | +2d10 | 2 parries per round; Riposte inflicts Staggered |
| **Atgeir-wielder** | +1d10 | Reach advantage on Riposte |
| **Skjaldmær** | Shield Block (separate) | Can parry for adjacent allies |
| **All Others** | None | Critical Parry only triggers Riposte |

---

## Riposte Mechanics

### Basic Riposte

- **Type:** Free basic melee attack
- **Timing:** Immediately after parry resolution
- **Damage:** Standard weapon damage + MIGHT
- **Cannot Be Parried:** Enemy cannot counter a Riposte
- **No Reaction Cost:** Does not consume your reaction

### Enhanced Riposte (Hólmgangr Rank 2+)

- **Bonus Damage:** +1d6 per rank above 1
- **Status Infliction (Rank 3):** Target becomes Staggered (1 round)

### Riposte Restrictions

- Must be in melee range of attacker
- Cannot Riposte ranged attacks (unless special ability)
- One Riposte per triggering parry

---

## Trauma Economy Integration

### Stress Vectors

| Event | Stress Gain | Notes |
| --- | --- | --- |
| Failed Parry | +5-8 | Vulnerability exposed |
| Riposte Miss | +3 | Wasted opportunity |
| 3+ consecutive failures | +10 | Skill doubt cascade |
| Parried then hit anyway | +6 | Defensive futility |

### Stress Relief

| Event | Stress Relief | Notes |
| --- | --- | --- |
| Superior Parry | -3 | Tactical satisfaction |
| Critical Parry | -5 | Mastery demonstration |
| Riposte Kill | -8 | Mastery validation |
| Pattern mastery (5+ parries vs same enemy) | -5 | Predictive confidence |

---

## Resolution Pipeline

```
1. ATTACK DECLARED
   └── Attacker rolls Accuracy Pool
   
2. PARRY OPPORTUNITY
   ├── Defender has Reaction available? → Continue
   └── No Reaction? → Skip to Step 5
   
3. PARRY DECLARATION
   └── Defender rolls Parry Pool (FINESSE + Weapon Skill + Modifiers)
   
4. PARRY RESOLUTION
   ├── Parry < Accuracy → FAILED (attack proceeds)
   ├── Parry = Accuracy → STANDARD (attack negated)
   ├── Parry > Accuracy → SUPERIOR (attack negated, check Riposte)
   └── Parry > Accuracy by 5+ → CRITICAL (all can Riposte)
   
5. RIPOSTE CHECK
   ├── Critical Parry? → All adjacent allies may Riposte
   ├── Superior Parry + Hólmgangr? → Parrier Ripostes
   └── Standard/Failed? → No Riposte
   
6. RIPOSTE EXECUTION (if triggered)
   ├── Roll attack vs target's Defense
   ├── Apply weapon damage + MIGHT + bonuses
   └── Apply status effects (Hólmgangr Rank 3: Staggered)
   
7. STRESS RESOLUTION
   └── Apply Trauma Economy effects based on outcome
```

---

## Database Schema

```sql
-- Parry statistics tracking
CREATE TABLE ParryStatistics (
    character_id INTEGER PRIMARY KEY,
    total_parry_attempts INTEGER DEFAULT 0,
    successful_parries INTEGER DEFAULT 0,
    superior_parries INTEGER DEFAULT 0,
    critical_parries INTEGER DEFAULT 0,
    ripostes_attempted INTEGER DEFAULT 0,
    ripostes_landed INTEGER DEFAULT 0,
    riposte_kills INTEGER DEFAULT 0,
    stress_from_parries INTEGER DEFAULT 0,
    stress_relieved_from_parries INTEGER DEFAULT 0,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id)
);

-- Parry event log for pattern analysis
CREATE TABLE ParryEventLog (
    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
    combat_instance_id INTEGER NOT NULL,
    turn_number INTEGER NOT NULL,
    parrier_id INTEGER NOT NULL,
    attacker_id INTEGER NOT NULL,
    parry_pool_total INTEGER NOT NULL,
    accuracy_pool_total INTEGER NOT NULL,
    outcome TEXT CHECK(outcome IN ('Failed', 'Standard', 'Superior', 'Critical')),
    riposte_triggered INTEGER DEFAULT 0,
    riposte_hit INTEGER DEFAULT 0,
    riposte_damage INTEGER DEFAULT 0,
    stress_change INTEGER DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (parrier_id) REFERENCES Characters(character_id),
    FOREIGN KEY (attacker_id) REFERENCES Characters(character_id)
);

-- Index for pattern analysis queries
CREATE INDEX idx_parry_pattern ON ParryEventLog(parrier_id, attacker_id, outcome);
```

---

## Service Integration

### CounterAttackService Methods

```csharp
public interface ICounterAttackService
{
    /// <summary>
    /// Calculates parry pool for a character against an incoming attack.
    /// </summary>
    ParryPool CalculateParryPool(Character parrier, Attack incomingAttack);
    
    /// <summary>
    /// Resolves a parry attempt and returns the outcome.
    /// </summary>
    ParryResult ResolveParry(ParryPool parryPool, int accuracyTotal);
    
    /// <summary>
    /// Determines if a riposte is triggered and who can perform it.
    /// </summary>
    RiposteOpportunity CheckRiposteEligibility(ParryResult result, Character parrier);
    
    /// <summary>
    /// Executes a riposte attack against the original attacker.
    /// </summary>
    RiposteResult ExecuteRiposte(Character riposting, Character target);
    
    /// <summary>
    /// Applies stress changes based on parry/riposte outcomes.
    /// </summary>
    void ApplyTraumaEconomyEffects(Character character, ParryResult result, RiposteResult riposte);
}
```

---

## Worked Examples

### Example 1: Hólmgangr Superior Parry

**Setup:** Hólmgangr (Rank 2) with FINESSE 4, Weapon Skill 3, parrying a Draugr attack.

```
Parry Pool: 4 (FINESSE) + 3 (Weapon Skill) + 2d10 (Hólmgangr Rank 2) = 7 base + 2d10
Roll: 7 + 8 + 6 = 21

Draugr Accuracy: 15

Result: 21 > 15 → SUPERIOR PARRY
- Attack negated
- Riposte triggered (Hólmgangr ability)
- Riposte damage: Weapon (2d6) + MIGHT (3) + 1d6 (Rank 2 bonus) = 3d6 + 3
- Stress: -3 (Superior Parry satisfaction)
```

### Example 2: Critical Parry (Party-Wide Riposte)

**Setup:** Skjaldmær parries a Vault Custodian's sweeping attack.

```
Parry Pool: 3 (FINESSE) + 4 (Shield Skill) + 2 (Defensive Stance) = 9 base
Roll: 9 + 7 = 16

Vault Custodian Accuracy: 10

Result: 16 > 10 by 6 → CRITICAL PARRY
- Attack negated
- ALL adjacent party members may Riposte
- Party-wide stress relief: -5 each (mastery demonstration)
```

---

## Balance Considerations

### Design Intent

- Parry should be a **meaningful choice**, not automatic
- Superior/Critical parries reward investment in parry-focused builds
- Non-specialists can still benefit from Critical Parries (universal floor)

### Tuning Levers

- **Parry Pool modifiers:** Adjust specialization bonuses
- **Critical threshold:** Currently 5+ margin; adjustable
- **Riposte damage:** Bonus damage per rank
- **Stress values:** Trauma Economy impact

---

## Migration Notes

**Source Documents:**

- v2.0 Parry System Specification
- Reactive Parry (Hólmgangr Tier 2 Ability)
- SPEC-COMBAT-006: Counter-Attack & Parry System

**Implementation Status:** Ready for Alpha

**Estimated Implementation:** 8-12 hours