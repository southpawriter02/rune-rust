---
id: SPEC-ATTACK-001
title: Attack Resolution System
version: 1.1.0
status: Implemented
last_updated: 2025-12-23
related_specs: [SPEC-DICE-001, SPEC-COMBAT-001, SPEC-STATUS-001, SPEC-TRAUMA-001]
---

# SPEC-ATTACK-001: Attack Resolution System

> **Version:** 1.1.0
> **Status:** Implemented (v0.2.2b)
> **Service:** `AttackResolutionService`
> **Location:** `RuneAndRust.Engine/Services/AttackResolutionService.cs`

---

## Overview

The **Attack Resolution System** determines hit/miss outcomes and calculates damage for all combat attacks using a dice pool success-counting mechanic. The system provides three attack types (Light, Standard, Heavy) with distinct stamina costs and damage modifiers, integrates stress-based defense penalties, and produces five outcome categories from Fumble to Critical Hit.

### Core Design Principles

1. **Success-Threshold Mechanic**: Rolled successes must exceed Defense Score / 5 threshold
2. **Tri-Weight Attack System**: Light (fast/weak), Standard (balanced), Heavy (slow/powerful)
3. **Stamina Resource Management**: Attack types cost 15/25/40 stamina respectively
4. **Outcome Scaling**: Net successes determine damage multiplier (×0 to ×2.0)
5. **Minimum Damage on Hit**: Soak cannot reduce damage below 1 on successful hits

### System Boundaries

**IN SCOPE:**
- Hit/miss determination via dice pool rolling
- Defense score calculation (base 10 + Finesse - Stress penalty)
- Damage calculation with attack type bonuses
- Soak reduction from armor and Fortified status
- Critical hit and fumble detection
- Stamina cost validation for attack affordability

**OUT OF SCOPE:**
- Turn order management (handled by CombatService)
- Status effect application from attacks (handled by AbilityService)
- Enemy death / victory conditions (handled by CombatService)
- Ranged vs melee distinction (future feature)
- Damage type resistance/weakness (future feature)

---

## Attack Types

### Attack Type Matrix

| Type | Stamina Cost | Hit Modifier | Damage Bonus | Use Case |
|------|-------------|--------------|--------------|----------|
| **Light** | 15 | +1 to pool | +0 damage | Stamina conservation, low-risk strikes |
| **Standard** | 25 | +0 to pool | +2 damage | Balanced default attack |
| **Heavy** | 40 | -1 to pool | +4 damage | High-damage finishing moves |

### Tactical Trade-offs

**Light Attack:**
- Lowest stamina cost (can perform ~13 attacks at 200 max stamina)
- +1 die to attack pool (higher hit chance)
- No damage bonus (relies on base might + weapon)
- **Best for**: Swarm enemies, stamina-starved characters, consistent chip damage

**Standard Attack:**
- Moderate stamina cost (can perform ~8 attacks at 200 max stamina)
- No pool modifier (baseline difficulty)
- +2 flat damage (meaningful but not overwhelming)
- **Best for**: General combat, balanced characters, reliable damage

**Heavy Attack:**
- High stamina cost (can perform ~5 attacks at 200 max stamina)
- -1 die to attack pool (lower hit chance)
- +4 flat damage (doubles Light attack potential)
- **Best for**: Bosses, high-might characters, critical moments

---

## Behaviors

### Primary Behaviors

#### 1. Attack Resolution (`ResolveMeleeAttack`)

**Signature:**
```csharp
AttackResult ResolveMeleeAttack(
    Combatant attacker,
    Combatant defender,
    AttackType attackType)
```

**Process:**

1. **Validate Stamina**:
   ```csharp
   var cost = GetStaminaCost(attackType);
   if (attacker.CurrentStamina < cost)
   {
       throw new InvalidOperationException("Insufficient stamina for attack");
   }
   ```

2. **Calculate Defense Score**:
   ```csharp
   var defenseScore = CalculateDefenseScore(defender);
   // Base 10 + Finesse - (Stress / 20, capped at 5)
   ```

3. **Determine Success Threshold**:
   ```csharp
   var successThreshold = defenseScore / 5;  // Integer division
   ```

4. **Roll Attack Pool**:
   ```csharp
   var poolSize = CalculateAttackPool(attacker, attackType);
   var rollResult = await _diceService.RollPoolAsync(poolSize, "attack roll");
   ```

5. **Calculate Net Successes**:
   ```csharp
   var netSuccesses = rollResult.Successes - successThreshold;
   ```

6. **Determine Outcome**:
   ```csharp
   var outcome = DetermineOutcome(rollResult, netSuccesses);
   ```

7. **Calculate Damage** (if hit):
   ```csharp
   var rawDamage = CalculateRawDamage(attacker, attackType, rollResult);
   var finalDamage = ApplyModifiers(rawDamage, defender, outcome);
   ```

8. **Return Result**:
   ```csharp
   return new AttackResult(
       Outcome: outcome,
       NetSuccesses: netSuccesses,
       RawDamage: rawDamage,
       FinalDamage: finalDamage,
       IsHit: outcome != AttackOutcome.Fumble && outcome != AttackOutcome.Miss
   );
   ```

**Outcomes:**
- **Fumble**: 0 successes + 1+ botches → 0 damage
- **Miss**: Net successes ≤ 0 → 0 damage
- **Glancing**: Net successes 1-2 → ×0.5 damage multiplier
- **Solid**: Net successes 3-4 → ×1.0 damage multiplier
- **Critical**: Net successes 5+ → ×2.0 damage multiplier

---

#### 2. Defense Score Calculation (`CalculateDefenseScore`)

**Formula:**
```
Defense Score = 10 + Finesse - Stress Penalty
```

**Stress Penalty Calculation:**
```csharp
var stressPenalty = Math.Min(defender.PsychicStress / 20, 5);
// Penalty = Stress / 20, capped at -5 maximum
```

**Example:**
- Base: 10
- Finesse: 8
- Stress: 60 (penalty = 60/20 = 3)
- **Defense Score: 10 + 8 - 3 = 15**
- **Success Threshold: 15 / 5 = 3 successes required**

---

#### 3. Attack Pool Calculation (`CalculateAttackPool`)

**Formula:**
```
Attack Pool = Might + Attack Type Modifier
```

**Attack Type Modifiers:**
- Light: +1 die
- Standard: +0 dice
- Heavy: -1 die

**Example:**
- Might: 7
- Attack Type: Heavy (-1)
- **Attack Pool: 7 - 1 = 6d10**

**Minimum Pool:**
- Always at least 1d10 (even with Might 0 + Heavy attack)

---

#### 4. Raw Damage Calculation (`CalculateRawDamage`)

**Formula:**
```
Raw Damage = Might + Weapon Damage Roll + Attack Type Bonus
```

**Weapon Damage Roll:**
```csharp
var weaponDamage = attacker.WeaponDamageDie > 0
    ? await _diceService.RollSingleAsync(attacker.WeaponDamageDie, "weapon damage")
    : 0;
```

**Attack Type Bonuses:**
- Light: +0
- Standard: +2
- Heavy: +4

**Example:**
- Might: 7
- Weapon: 1d8 → rolled 5
- Attack Type: Standard (+2)
- **Raw Damage: 7 + 5 + 2 = 14**

---

#### 5. Final Damage Application (`ApplyModifiers`)

**Process:**

1. **Apply Outcome Multiplier**:
   ```csharp
   var outcomeMultiplier = outcome switch
   {
       AttackOutcome.Fumble => 0f,
       AttackOutcome.Miss => 0f,
       AttackOutcome.Glancing => 0.5f,
       AttackOutcome.Solid => 1.0f,
       AttackOutcome.Critical => 2.0f,
       _ => 0f
   };
   var modifiedDamage = (int)(rawDamage * outcomeMultiplier);
   ```

2. **Apply Status Effect Multiplier** (Vulnerable):
   ```csharp
   var statusMultiplier = _statusEffectService.GetDamageMultiplier(defender);
   // Returns 1.5f if Vulnerable, 1.0f otherwise
   modifiedDamage = (int)(modifiedDamage * statusMultiplier);
   ```

3. **Calculate Total Soak**:
   ```csharp
   var armorSoak = defender.Soak;
   var fortifiedSoak = _statusEffectService.GetSoakModifier(defender);
   // Returns +2 per Fortified stack
   var totalSoak = armorSoak + fortifiedSoak;
   ```

4. **Apply Soak Reduction**:
   ```csharp
   var finalDamage = Math.Max(1, modifiedDamage - totalSoak);
   // Minimum 1 damage on any hit
   ```

**Example (Critical Hit with Vulnerable + Fortified Target):**
- Raw Damage: 14
- Outcome: Critical (×2.0) → 28
- Vulnerable: ×1.5 → 42
- Armor Soak: 5
- Fortified ×2: +4
- Total Soak: 9
- **Final Damage: 42 - 9 = 33**

---

#### 6. Outcome Determination (`DetermineOutcome`)

**Decision Logic:**
```csharp
// Fumble: 0 successes AND 1+ botches
if (rollResult.Successes == 0 && rollResult.Botches > 0)
    return AttackOutcome.Fumble;

// Miss: Net successes ≤ 0
if (netSuccesses <= 0)
    return AttackOutcome.Miss;

// Outcome tiers based on net successes
return netSuccesses switch
{
    >= 5 => AttackOutcome.Critical,   // 5+ net successes
    >= 3 => AttackOutcome.Solid,      // 3-4 net successes
    _ => AttackOutcome.Glancing       // 1-2 net successes
};
```

**Net Success Thresholds:**
- ≤ 0: Miss
- 1-2: Glancing (×0.5 damage)
- 3-4: Solid (×1.0 damage)
- 5+: Critical (×2.0 damage)

**Special Case - Fumble:**
- Requires 0 successes (all dice ≥ 7) AND at least 1 botch (rolled 1)
- Probability: Very low (< 1% with typical pools)
- Effect: Narrative failure, 0 damage, potential enemy morale boost

---

#### 7. Stamina Affordability Check (`CanAffordAttack`)

**Purpose:** Allow AI and UI to validate attack selection before execution

**Signature:**
```csharp
bool CanAffordAttack(Combatant attacker, AttackType attackType)
```

**Implementation:**
```csharp
var cost = GetStaminaCost(attackType);
return attacker.CurrentStamina >= cost;
```

**Usage:**
```csharp
// Enemy AI checking attack affordability
if (!_attackResolution.CanAffordAttack(enemy, AttackType.Heavy))
{
    // Fall back to Standard or Light attack
}
```

---

## Restrictions

### MUST Requirements

1. **MUST validate stamina before attack execution**
   - **Reason:** Prevent negative stamina states
   - **Implementation:** AttackResolutionService.cs:78

2. **MUST apply minimum 1 damage on successful hits**
   - **Reason:** Prevent infinite combat loops with high-soak enemies
   - **Implementation:** AttackResolutionService.cs:198

3. **MUST cap stress penalty at -5 defense**
   - **Reason:** Prevent defense score becoming negative or zero
   - **Implementation:** AttackResolutionService.cs:132

4. **MUST use integer division for success threshold**
   - **Reason:** Consistent rounding (15 defense = 3 threshold, not 3.33)
   - **Implementation:** AttackResolutionService.cs:147

5. **MUST apply outcome multiplier before soak reduction**
   - **Reason:** Soak reduces final damage, not raw roll
   - **Implementation:** AttackResolutionService.cs:182-198

6. **MUST roll weapon damage die separately from attack pool**
   - **Reason:** Attack pool determines hit/miss, weapon die determines damage
   - **Implementation:** AttackResolutionService.cs:165-171

7. **MUST check Vulnerable status before soak reduction**
   - **Reason:** Status multipliers apply to modified damage
   - **Implementation:** AttackResolutionService.cs:185-188

8. **MUST ensure attack pool has minimum 1 die**
   - **Reason:** Prevent zero-die pools (undefined behavior)
   - **Implementation:** AttackResolutionService.cs:152

---

### MUST NOT Requirements

1. **MUST NOT allow negative defense scores**
   - **Violation Impact:** Negative threshold (success guaranteed)
   - **Enforcement:** Stress penalty capped at -5

2. **MUST NOT reduce damage below 0**
   - **Violation Impact:** Healing via attacks
   - **Enforcement:** Math.Max(0, damage) on all calculations

3. **MUST NOT apply soak to miss/fumble outcomes**
   - **Violation Impact:** Negative damage display
   - **Enforcement:** Early return on miss outcomes

4. **MUST NOT allow fractional damage values**
   - **Violation Impact:** Display/tracking issues
   - **Enforcement:** (int) casts on all multiplier applications

5. **MUST NOT skip outcome determination**
   - **Violation Impact:** Undefined damage multiplier
   - **Enforcement:** All paths through ResolveMeleeAttack() call DetermineOutcome()

---

## Limitations

### Numerical Bounds

| Constraint | Value | Notes |
|------------|-------|-------|
| Min attack pool | 1d10 | Even with Might 0 + Heavy |
| Max stress penalty | -5 defense | Caps at 100 stress |
| Min damage on hit | 1 | After all reductions |
| Max outcome multiplier | ×2.0 | Critical hits |
| Defense score base | 10 | Before Finesse/modifiers |
| Success threshold divisor | 5 | Defense / 5 (integer) |

### Functional Limitations

1. **No Ranged Attack Support**
   - Current: All attacks assume melee
   - Future: Separate ranged attack types with different pool modifiers

2. **No Damage Type Resistance**
   - Current: Physical damage only, no resistances
   - Future: Fire/Ice/Lightning with resistance/weakness modifiers

3. **No Multi-Target Attacks**
   - Current: Single target only
   - Future: Area attacks with reduced damage per target

4. **No Attack Abilities**
   - Current: Basic attacks only
   - Future: Special abilities integrated via AbilityService

5. **No Weapon Quality Modifiers**
   - Current: Weapon damage die is fixed
   - Future: Quality bonuses (+1 to die size for Optimized, etc.)

6. **No Critical Failure Effects**
   - Current: Fumble = 0 damage, no penalties
   - Future: Fumble penalties (self-damage, weapon drop, etc.)

7. **No Defending Stance Bonus**
   - Current: `IsDefending` property exists on Combatant but is not used in defense calculation
   - Future: +2 defense bonus when in defending stance (requires CombatService integration)

---

## Use Cases

### USE CASE 1: Standard Player Attack (Hit)

**Setup:**
```csharp
var player = new Combatant
{
    Might = 7,
    CurrentStamina = 100,
    WeaponDamageDie = 8,  // Longsword (1d8)
    WeaponName = "Scavenged Longsword"
};

var enemy = new Combatant
{
    Finesse = 5,
    PsychicStress = 40,  // 2 penalty
    Soak = 3
};
```

**Execution:**
```csharp
var result = _attackResolution.ResolveMeleeAttack(
    player, enemy, AttackType.Standard);
```

**Internal Flow:**

1. **Stamina Check**: 100 ≥ 25 (Standard cost) ✓
2. **Defense Score**: 10 + 5 - 2 = 13
3. **Success Threshold**: 13 / 5 = 2 successes required
4. **Attack Pool**: 7 + 0 (Standard) = 7d10
5. **Roll**: {3, 5, 8, 2, 9, 4, 6} → 3 successes (5, 8, 9 ≥ 7)
6. **Net Successes**: 3 - 2 = 1
7. **Outcome**: Glancing (net 1-2)
8. **Raw Damage**: 7 (Might) + 5 (1d8 roll) + 2 (Standard bonus) = 14
9. **Outcome Multiplier**: ×0.5 (Glancing) → 7 damage
10. **Status Multiplier**: ×1.0 (no Vulnerable) → 7 damage
11. **Soak Reduction**: 7 - 3 = 4
12. **Final Damage**: 4

**Assertions:**
- `result.Outcome == AttackOutcome.Glancing`
- `result.NetSuccesses == 1`
- `result.RawDamage == 14`
- `result.FinalDamage == 4`
- `result.IsHit == true`

---

### USE CASE 2: Heavy Attack Critical Hit

**Setup:**
```csharp
var player = new Combatant
{
    Might = 9,
    CurrentStamina = 50,
    WeaponDamageDie = 10,  // Greatsword (1d10)
};

var enemy = new Combatant
{
    Finesse = 3,
    PsychicStress = 0,
    Soak = 5,
    StatusEffects = new List<ActiveStatusEffect>()  // No Fortified
};
```

**Execution:**
```csharp
var result = _attackResolution.ResolveMeleeAttack(
    player, enemy, AttackType.Heavy);
```

**Internal Flow:**

1. **Defense Score**: 10 + 3 = 13
2. **Success Threshold**: 13 / 5 = 2
3. **Attack Pool**: 9 - 1 (Heavy) = 8d10
4. **Roll**: {8, 9, 7, 10, 8, 6, 7, 9} → 7 successes
5. **Net Successes**: 7 - 2 = 5
6. **Outcome**: Critical (net 5+)
7. **Raw Damage**: 9 + 8 (1d10 roll) + 4 (Heavy) = 21
8. **Outcome Multiplier**: ×2.0 (Critical) → 42
9. **Soak Reduction**: 42 - 5 = 37
10. **Final Damage**: 37

**Assertions:**
- `result.Outcome == AttackOutcome.Critical`
- `result.NetSuccesses == 5`
- `result.FinalDamage == 37`

---

### USE CASE 3: Light Attack Against Fortified Defender

**Setup:**
```csharp
var player = new Combatant
{
    Might = 5,
    CurrentStamina = 20,  // Low stamina, must use Light
    WeaponDamageDie = 6
};

var enemy = new Combatant
{
    Finesse = 6,
    Soak = 2,
    StatusEffects = new List<ActiveStatusEffect>
    {
        new ActiveStatusEffect
        {
            Type = StatusEffectType.Fortified,
            Stacks = 3,  // +6 soak total
            DurationRemaining = 2
        }
    }
};
```

**Execution:**
```csharp
var result = _attackResolution.ResolveMeleeAttack(
    player, enemy, AttackType.Light);
```

**Internal Flow:**

1. **Defense Score**: 10 + 6 = 16
2. **Success Threshold**: 16 / 5 = 3
3. **Attack Pool**: 5 + 1 (Light) = 6d10
4. **Roll**: {7, 8, 9, 4, 6, 10} → 4 successes
5. **Net Successes**: 4 - 3 = 1
6. **Outcome**: Glancing (net 1-2)
7. **Raw Damage**: 5 + 4 (1d6 roll) + 0 (Light) = 9
8. **Outcome Multiplier**: ×0.5 → 4 (rounded down)
9. **Total Soak**: 2 (armor) + 6 (Fortified ×3) = 8
10. **Soak Reduction**: 4 - 8 = -4 → **Minimum 1 damage enforced**
11. **Final Damage**: 1

**Assertions:**
- `result.FinalDamage == 1` (minimum damage on hit)
- High soak negated most damage, but hit still dealt 1 HP

---

### USE CASE 4: Fumble (Critical Failure)

**Setup:**
```csharp
var player = new Combatant { Might = 4, CurrentStamina = 30 };
var enemy = new Combatant { Finesse = 8, Soak = 5 };
```

**Execution:**
```csharp
// Simulated dice roll: {1, 1, 4, 6} → 0 successes, 2 botches
var result = _attackResolution.ResolveMeleeAttack(
    player, enemy, AttackType.Standard);
```

**Internal Flow:**

1. **Roll Result**: Successes = 0, Botches = 2
2. **Fumble Check**: 0 successes AND botches > 0 → Fumble
3. **Outcome**: Fumble
4. **Damage**: 0 (fumbles deal no damage)

**Assertions:**
- `result.Outcome == AttackOutcome.Fumble`
- `result.IsHit == false`
- `result.FinalDamage == 0`

---

### USE CASE 5: Miss (Insufficient Net Successes)

**Setup:**
```csharp
var player = new Combatant { Might = 3, CurrentStamina = 40 };
var enemy = new Combatant
{
    Finesse = 10
};
```

**Execution:**
```csharp
var result = _attackResolution.ResolveMeleeAttack(
    player, enemy, AttackType.Heavy);
```

**Internal Flow:**

1. **Defense Score**: 10 + 10 = 20
2. **Success Threshold**: 20 / 5 = 4
3. **Attack Pool**: 3 - 1 (Heavy) = 2d10 (very small pool)
4. **Roll**: {8, 7} → 2 successes
5. **Net Successes**: 2 - 4 = -2
6. **Outcome**: Miss (net ≤ 0)
7. **Damage**: 0

**Assertions:**
- `result.Outcome == AttackOutcome.Miss`
- `result.NetSuccesses == -2`
- `result.IsHit == false`
- `result.FinalDamage == 0`

---

### USE CASE 6: Stamina Insufficiency (Error)

**Setup:**
```csharp
var player = new Combatant
{
    Might = 7,
    CurrentStamina = 10  // Only 10 stamina remaining
};
var enemy = new Combatant { Finesse = 5 };
```

**Execution:**
```csharp
// Attempt Heavy attack (40 stamina cost)
_attackResolution.ResolveMeleeAttack(
    player, enemy, AttackType.Heavy);
```

**Expected Behavior:**
```
InvalidOperationException: "Insufficient stamina for Heavy attack (required: 40, available: 10)"
```

**Assertions:**
- Exception thrown before dice rolling
- Player stamina unchanged
- No attack result generated

---

## Decision Trees

### Decision Tree 1: Attack Resolution Flow

```
ResolveMeleeAttack(attacker, defender, attackType)
│
├─ VALIDATE STAMINA
│  ├─ CurrentStamina < Cost?
│  │  └─ YES → throw InvalidOperationException
│  └─ NO → CONTINUE
│
├─ CALCULATE DEFENSE SCORE
│  ├─ Base = 10
│  ├─ Add Finesse
│  ├─ Subtract Stress Penalty (min(Stress/20, 5))
│  └─ Success Threshold = Defense / 5 (integer)
│
├─ ROLL ATTACK POOL
│  ├─ Pool Size = Might + Attack Type Modifier
│  ├─ Min Pool = 1d10
│  └─ Roll → Successes, Botches
│
├─ DETERMINE OUTCOME
│  ├─ Successes == 0 AND Botches > 0?
│  │  └─ YES → Fumble (0 damage, RETURN)
│  ├─ Net Successes = Successes - Threshold
│  ├─ Net ≤ 0?
│  │  └─ YES → Miss (0 damage, RETURN)
│  ├─ Net 1-2?
│  │  └─ YES → Glancing (×0.5 multiplier)
│  ├─ Net 3-4?
│  │  └─ YES → Solid (×1.0 multiplier)
│  └─ Net 5+?
│     └─ YES → Critical (×2.0 multiplier)
│
├─ CALCULATE RAW DAMAGE (if hit)
│  ├─ Roll Weapon Damage Die
│  ├─ Raw = Might + Weapon Roll + Attack Type Bonus
│  └─ CONTINUE
│
├─ APPLY MODIFIERS
│  ├─ Modified = Raw × Outcome Multiplier
│  ├─ Modified = Modified × Status Multiplier (Vulnerable)
│  ├─ Total Soak = Armor + Fortified
│  ├─ Final = Modified - Total Soak
│  └─ Final = max(1, Final)  // Minimum damage on hit
│
└─ RETURN AttackResult
   ├─ Outcome
   ├─ Net Successes
   ├─ Raw Damage
   ├─ Final Damage
   └─ IsHit
```

---

### Decision Tree 2: Outcome Determination Logic

```
DetermineOutcome(rollResult, netSuccesses)
│
├─ CHECK FUMBLE CONDITION
│  ├─ rollResult.Successes == 0?
│  │  └─ YES → rollResult.Botches > 0?
│  │           └─ YES → RETURN Fumble
│  └─ NO → CONTINUE
│
├─ CHECK MISS CONDITION
│  ├─ netSuccesses <= 0?
│  │  └─ YES → RETURN Miss
│  └─ NO → CONTINUE (hit confirmed)
│
├─ DETERMINE HIT QUALITY
│  ├─ netSuccesses >= 5?
│  │  └─ YES → RETURN Critical
│  ├─ netSuccesses >= 3?
│  │  └─ YES → RETURN Solid
│  └─ Else (netSuccesses 1-2)
│     └─ RETURN Glancing
```

**Outcome Probabilities (example with 7d10 pool, threshold 3):**
- Fumble: ~1% (requires 0 successes + botch)
- Miss: ~15% (net ≤ 0)
- Glancing: ~30% (net 1-2)
- Solid: ~40% (net 3-4)
- Critical: ~14% (net 5+)

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IDiceService` | [SPEC-DICE-001](SPEC-DICE-001.md) | D10 pool rolling for attack rolls, single die rolls for weapon damage |
| `IStatusEffectService` | [SPEC-STATUS-001](SPEC-STATUS-001.md) | Damage multiplier (Vulnerable ×1.5), soak modifier (Fortified +2/stack) |
| `ITraumaService` | [SPEC-TRAUMA-001](SPEC-TRAUMA-001.md) | Stress-based defense penalty calculation |
| `ILogger` | Infrastructure | Attack event tracing |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Attack execution via ExecutePlayerAttack() and ExecuteEnemyAttack() |
| `EnemyAIService` | [SPEC-AI-001](SPEC-AI-001.md) | Stamina affordability checks via CanAffordAttack() |

---

## Data Models

### AttackResult

**Source:** `RuneAndRust.Core/Models/Combat/AttackResult.cs`

```csharp
public record AttackResult(
    AttackOutcome Outcome,
    int NetSuccesses,
    int RawDamage,
    int FinalDamage,
    bool IsHit,
    DamageType DamageType = DamageType.Physical
);
```

**Field Descriptions:**
- `Outcome`: AttackOutcome enum (Fumble, Miss, Glancing, Solid, Critical)
- `NetSuccesses`: Rolled successes minus success threshold (can be negative)
- `RawDamage`: Pre-modifier damage (Might + weapon + bonus)
- `FinalDamage`: After outcome multiplier, status effects, and soak
- `IsHit`: True if outcome is Glancing/Solid/Critical
- `DamageType`: Damage type (Physical default, future: Fire/Ice/etc.)

---

### AttackOutcome Enum

**Source:** `RuneAndRust.Core/Enums/AttackOutcome.cs`

```csharp
public enum AttackOutcome
{
    Fumble,     // 0 successes + 1+ botches → ×0 damage
    Miss,       // Net successes ≤ 0 → ×0 damage
    Glancing,   // Net successes 1-2 → ×0.5 damage
    Solid,      // Net successes 3-4 → ×1.0 damage
    Critical    // Net successes 5+ → ×2.0 damage
}
```

---

### AttackType Enum

**Source:** `RuneAndRust.Core/Enums/AttackType.cs`

```csharp
public enum AttackType
{
    Light,      // 15 stamina, +1 pool, +0 damage
    Standard,   // 25 stamina, +0 pool, +2 damage
    Heavy       // 40 stamina, -1 pool, +4 damage
}
```

---

## Configuration

### Attack Type Constants

```csharp
private const int LightStaminaCost = 15;
private const int StandardStaminaCost = 25;
private const int HeavyStaminaCost = 40;

private const int LightPoolModifier = 1;
private const int StandardPoolModifier = 0;
private const int HeavyPoolModifier = -1;

private const int LightDamageBonus = 0;
private const int StandardDamageBonus = 2;
private const int HeavyDamageBonus = 4;
```

---

### Defense Calculation Constants

```csharp
private const int BaseDefenseScore = 10;
private const int StressPenaltyDivisor = 20;
private const int MaxStressPenalty = 5;
private const int SuccessThresholdDivisor = 5;
```

---

### Outcome Thresholds

```csharp
private const int GlancingThreshold = 1;   // Net 1-2
private const int SolidThreshold = 3;      // Net 3-4
private const int CriticalThreshold = 5;   // Net 5+
```

---

### Damage Multipliers

```csharp
private const float FumbleMultiplier = 0f;
private const float MissMultiplier = 0f;
private const float GlancingMultiplier = 0.5f;
private const float SolidMultiplier = 1.0f;
private const float CriticalMultiplier = 2.0f;
```

---

## Testing

### Test Summary

**Source:** `RuneAndRust.Tests/Engine/AttackResolutionServiceTests.cs` (651 lines)

**Test Count:** 24 tests

**Coverage Breakdown:**
- Stamina validation: 3 tests
- Defense score calculation: 4 tests
- Attack pool calculation: 3 tests
- Outcome determination: 5 tests
- Damage calculation: 6 tests
- Status effect integration: 3 tests

**Coverage Percentage:** ~85%

---

### Critical Test Scenarios

1. **Stamina Cost Validation** (lines 48-84)
   - Light attack costs 15 stamina
   - Standard attack costs 25 stamina
   - Heavy attack costs 40 stamina

2. **Defense Score with Stress** (lines 92-120)
   - Base 10 + Finesse - (Stress/20)
   - Stress penalty capped at -5

3. **Success Threshold Conversion** (lines 128-145)
   - Defense 15 → Threshold 3
   - Defense 20 → Threshold 4
   - Integer division (no rounding)

4. **Outcome Determination** (lines 153-220)
   - Fumble: 0 successes + botches
   - Miss: Net ≤ 0
   - Glancing: Net 1-2
   - Solid: Net 3-4
   - Critical: Net 5+

5. **Attack Type Modifiers** (lines 228-280)
   - Light: +1 pool, +0 damage
   - Standard: +0 pool, +2 damage
   - Heavy: -1 pool, +4 damage

6. **Minimum Damage on Hit** (lines 288-310)
   - Soak cannot reduce below 1
   - Only applies to hits (not misses)

7. **Status Effect Integration** (lines 318-365)
   - Vulnerable: ×1.5 damage multiplier
   - Fortified: +2 soak per stack
   - Applied before final soak reduction

8. **Edge Cases** (lines 373-430)
   - Zero Might attacker (minimum 1d10 pool)
   - Massive armor defender (minimum 1 damage)
   - Insufficient stamina exception

---

## Design Rationale

### Why Three Attack Types?

- **Tactical Depth**: Players must balance risk/reward (accuracy vs damage)
- **Stamina Management**: Forces resource decisions (conserve vs burst)
- **Enemy AI Variety**: Different archetypes prefer different types
- **Outcome Variance**: Different pool sizes create different probability curves

### Why Defense Score / 5 Threshold?

- **Scaling**: Higher defense requires proportionally more successes
- **Granularity**: 1-point Finesse increases matter (14 → 15 defense raises threshold)
- **Simplicity**: Integer division avoids fractional thresholds

### Why Minimum 1 Damage on Hit?

- **Prevents Stalemate**: High-soak enemies cannot become unkillable
- **Rewards Consistency**: Even weak attacks chip away at tough enemies
- **Psychological**: Players feel progress on every hit

### Why Separate Weapon Damage Die?

- **Roll Variety**: Two die rolls per attack (pool + weapon) creates variance
- **Weapon Identity**: Different weapons feel distinct (1d6 vs 1d10)
- **Upgrade Path**: Better weapons increase damage without changing hit chance

### Why ×2.0 Critical Multiplier (Not Higher)?

- **Balance**: ×3.0 or ×4.0 creates swing too wide (one-shot bosses)
- **Consistency**: ×2.0 feels impactful but not random
- **Soak Interaction**: Doubling pre-soak damage means soak still matters

---

## Changelog

### v1.1.0 - Documentation Accuracy Update (2025-12-23)

**Documentation Corrections:**
- **CHANGED**: Method signature `ResolveAttackAsync()` → `ResolveMeleeAttack()` (sync, not async)
- **REMOVED**: Defending Bonus from defense formula (not implemented in code)
- **ADDED**: "No Defending Stance Bonus" to Limitations section as future feature
- **UPDATED**: All USE CASE examples to use synchronous method signature

**No code changes - documentation-only update to match actual implementation.**

---

### v0.2.2b - Initial Attack Resolution Implementation (2025-11-28)
- **ADDED**: `ResolveMeleeAttack()` core attack resolution method
- **ADDED**: Three attack types (Light, Standard, Heavy)
- **ADDED**: Defense score calculation with stress penalties
- **ADDED**: Success threshold conversion (Defense / 5)
- **ADDED**: Outcome determination (Fumble, Miss, Glancing, Solid, Critical)
- **ADDED**: Raw damage calculation (Might + weapon + bonus)
- **ADDED**: Outcome multipliers (×0, ×0.5, ×1.0, ×2.0)
- **ADDED**: Status effect integration (Vulnerable, Fortified)
- **ADDED**: Minimum 1 damage on hit enforcement
- **ADDED**: Stamina affordability check (`CanAffordAttack()`)

---

## Related Specifications

- [SPEC-DICE-001](SPEC-DICE-001.md) - Dice Pool System (attack roll mechanics)
- [SPEC-COMBAT-001](SPEC-COMBAT-001.md) - Combat Service (attack orchestration)
- [SPEC-STATUS-001](SPEC-STATUS-001.md) - Status Effect System (damage/soak modifiers)
- [SPEC-TRAUMA-001](SPEC-TRAUMA-001.md) - Trauma System (stress defense penalties)
- [SPEC-AI-001](SPEC-AI-001.md) - Enemy AI (attack type selection)

---

## Code References

**Primary Implementation:**
- `RuneAndRust.Engine/Services/AttackResolutionService.cs` (241 lines)

**Interface:**
- `RuneAndRust.Core/Interfaces/IAttackResolutionService.cs` (28 lines)

**Tests:**
- `RuneAndRust.Tests/Engine/AttackResolutionServiceTests.cs` (651 lines, 24 tests)

**Data Models:**
- `RuneAndRust.Core/Models/Combat/AttackResult.cs`
- `RuneAndRust.Core/Enums/AttackOutcome.cs`
- `RuneAndRust.Core/Enums/AttackType.cs`
- `RuneAndRust.Core/Models/Combat/Combatant.cs`

---

**END OF SPECIFICATION**
