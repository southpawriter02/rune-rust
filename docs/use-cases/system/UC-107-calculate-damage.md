# UC-107: Calculate Damage

**Actor:** System
**Priority:** High
**Version:** v0.0.9
**Status:** Implemented

## Description

System calculates final damage dealt in combat, applying damage types, resistances, defense, and modifiers.

## Trigger

- Player attacks monster
- Monster attacks player
- Ability deals damage
- Status effect deals damage

## Preconditions

- Attacker and target exist
- Base damage value available
- Damage type specified (or default physical)

## Basic Flow

1. System receives attack parameters:
   - Base damage
   - Damage type
   - Attacker
   - Target
2. System retrieves target's resistances
3. System calculates type-modified damage:
   - Apply resistance percentage
   - Apply immunity (if any)
4. System applies defense reduction:
   - Final = TypeDamage - Defense
   - Minimum 1 damage (unless immune)
5. System applies any final modifiers:
   - Critical hit multiplier
   - Vulnerability
6. System returns damage result

## Alternative Flows

### AF-1: Physical Damage

1. Damage type is Physical (default)
2. Apply physical resistance percentage
3. Apply target defense stat
4. Standard calculation path

### AF-2: Elemental Damage

1. Damage type is Fire/Ice/Lightning/etc.
2. Apply elemental resistance for that type
3. Defense may not apply (configurable)
4. Elemental effects may trigger

### AF-3: Immunity

1. Target immune to damage type
2. Damage reduced to 0
3. Display "Immune!" message
4. No damage applied

### AF-4: Vulnerability

1. Target vulnerable to damage type
2. Damage increased (typically 150%)
3. Display "Vulnerable!" message
4. Enhanced damage applied

### AF-5: Critical Hit

1. Attack flagged as critical
2. Multiply final damage (typically 150%)
3. Display "Critical!" message

### AF-6: Minimum Damage

1. Defense would reduce to 0 or below
2. Apply minimum damage (1)
3. Unless target is immune

### AF-7: Percentage Resistance

1. Target has 25% Fire resistance
2. Fire damage reduced by 25%
3. 100 Fire → 75 Fire damage
4. Then defense applied

## Exception Flows

### EF-1: Missing Resistance Data

1. Damage type not in target's resistances
2. Assume 0% resistance (full damage)
3. Log if unexpected

### EF-2: Invalid Damage Value

1. Base damage is negative or invalid
2. Use 0 damage
3. Log warning

### EF-3: Null Target

1. No valid target provided
2. Return 0 damage result
3. Log error

## Postconditions

- Damage value calculated
- Modifiers applied
- Result available for application
- Type effectiveness communicated

## Business Rules

- All damage types have a base multiplier of 1.0
- Resistance reduces damage by percentage
- Immunity reduces damage to 0
- Vulnerability increases damage
- Defense reduces physical damage
- Minimum damage is 1 (unless immune)
- Critical hits apply last (multiplicative)

## Damage Types

| Type | Common Sources | Notes |
|------|----------------|-------|
| Physical | Weapons, basic attacks | Reduced by Defense |
| Fire | Fire spells, fire monsters | Burns |
| Ice | Ice spells | May slow |
| Lightning | Storm spells | Chain potential |
| Poison | Poison abilities | DoT |
| Holy | Paladin abilities | Extra vs undead |
| Dark | Necromancy | Drains life |

## Damage Calculation Formula

```
Raw Damage
    │
    ▼
┌─────────────────────────┐
│ Check Immunity          │
│ If immune → return 0    │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Apply Resistance        │
│ Damage × (1 - Resist%)  │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Apply Defense           │
│ (Physical only)         │
│ Damage - Defense        │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Apply Vulnerability     │
│ Damage × 1.5 if vuln    │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Apply Critical          │
│ Damage × 1.5 if crit    │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Apply Minimum (max 1)   │
└───────────┬─────────────┘
            │
            ▼
       Final Damage
```

## Example Calculations

### Standard Physical Attack

```
Player attacks Goblin:
- Base Attack: 20
- Goblin Defense: 5
- No resistance

Damage = 20 - 5 = 15
```

### Fire Spell vs Resistant Target

```
Fireball vs Dragon:
- Base Damage: 30
- Dragon Fire Resistance: 75%
- Defense doesn't apply to magic

Damage = 30 × (1 - 0.75) = 7.5 → 7
```

### Critical Physical Attack

```
Critical hit on Skeleton:
- Base Attack: 20
- Skeleton Defense: 3
- Critical multiplier: 1.5

Damage = (20 - 3) × 1.5 = 25.5 → 25
```

## UI Display

Normal damage:

```
You attack the Goblin for 15 damage!
```

Type effective:

```
You cast Fireball!
The Ice Golem is vulnerable to fire!
Critical damage: 45 fire damage!
```

Resisted:

```
You cast Fireball!
The Dragon resists! (75% fire resistance)
The Dragon takes 7 fire damage.
```

Immune:

```
You cast Fireball!
The Fire Elemental is immune to fire!
No damage dealt.
```

## Related Use Cases

- [UC-005: Engage in Combat](../player/UC-005-engage-in-combat.md) - Uses damage calculation
- [UC-006: Use Ability](../player/UC-006-use-ability.md) - Ability damage
- [UC-104: Monster Turn](UC-104-monster-turn.md) - Monster damage

## Implementation Notes

- Calculation via `DamageCalculationService.Calculate(params)`
- Resistances from `Monster.Resistances` or `Player.Resistances`
- Returns `DamageResult` with final damage, type, modifiers applied
- Defense from `Entity.Defense` stat
