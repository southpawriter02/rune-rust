---
id: SPEC-RESOURCE-001
title: Resource Management System
version: 1.1.0
status: Implemented
last_updated: 2025-12-23
related_specs: [SPEC-COMBAT-001, SPEC-CHAR-001, SPEC-STATUS-001]
---

# SPEC-RESOURCE-001: Resource Management System

> **Version:** 1.1.0
> **Status:** Implemented
> **Service:** `ResourceService`
> **Location:** `RuneAndRust.Engine/Services/ResourceService.cs`

---

## Table of Contents

1. [Overview](#overview)
2. [Core Behaviors](#core-behaviors)
3. [Restrictions](#restrictions)
4. [Limitations](#limitations)
5. [Use Cases](#use-cases)
6. [Decision Trees](#decision-trees)
7. [Sequence Diagrams](#sequence-diagrams)
8. [Workflows](#workflows)
9. [Cross-System Integration](#cross-system-integration)
10. [Data Models](#data-models)
11. [Configuration](#configuration)
12. [Testing](#testing)
13. [Domain 4 Compliance](#domain-4-compliance)
14. [Future Extensions](#future-extensions)
15. [Error Handling](#error-handling)
16. [Changelog](#changelog)

---

## Overview

The **Resource Management System** (`ResourceService`) is the core combat resource tracking system for *Rune & Rust*. It manages three resource types—**Health (HP)**, **Stamina**, and **Aether (AP)**—with a unique **Mystic Overcast** mechanic that allows Mystic archetypes to convert HP to AP at a 2:1 ratio when AP reserves are insufficient.

### Purpose

- **Resource Validation**: Determine if combatants can afford ability costs before execution
- **Resource Deduction**: Subtract costs from appropriate resource pools
- **Stamina Regeneration**: Restore stamina at the end of each combat turn based on Finesse attribute
- **Mystic Overcast Mechanic**: Enable Mystics to cast spells by sacrificing HP when AP is depleted
- **Source Synchronization**: Keep combat state synchronized with character/enemy source entities

### Key Features

1. **Three Resource Types**:
   - **Health (HP)**: Universal life total for all combatants
   - **Stamina**: Used for physical abilities (melee attacks, defensive maneuvers)
   - **Aether (AP)**: Used for mystic abilities (spells, rituals) - **Mystic archetypes only**

2. **Mystic Overcast Mechanic**:
   - **Conversion Ratio**: 2 HP per 1 missing AP
   - **Survival Requirement**: Combatant must have `CurrentHp > hpCost` to Overcast (cannot cast at cost of death)
   - **Logging**: All Overcast events are logged as warnings for visibility

3. **Stamina Regeneration Formula**:
   ```
   StaminaRegen = BaseStaminaRegen + (Finesse / 2)
   Default: 5 + (Finesse / 2) per turn
   ```

4. **Archetype-Based Restrictions**:
   - Non-Mystics **cannot** use Aether abilities (CanAfford returns false for ResourceType.Aether)
   - Mystics can use both Stamina and Aether abilities

5. **Status Effect Integration**:
   - **Stunned** status blocks stamina regeneration
   - Status effects checked via `StatusEffectService`

### System Context

**ResourceService** sits between `CombatService` (which orchestrates turns and actions) and individual `Combatant` entities (which store resource state). It acts as a **resource gatekeeper**, validating all ability costs before execution and handling edge cases like Overcast mechanics.

**Dependencies**:
- `ILogger<ResourceService>` - Traceability for resource operations
- `IStatusEffectService` - Status effect checking (Stunned blocks regen)

**Dependents**:
- `CombatService` - Calls `CanAfford()` before executing abilities
- `AbilityService` - Defines ability costs (HP/Stamina/Aether)
- UI Layer - Displays current/max resource values

---

## Core Behaviors

### 1. Resource Availability Checking (`CanAfford`)

**Signature**: `bool CanAfford(Combatant combatant, ResourceType resourceType, int cost)`

**Purpose**: Determines if a combatant has sufficient resources to execute an ability.

**Behavior**:

```csharp
// ResourceService.cs:46-71
public bool CanAfford(Combatant combatant, ResourceType resourceType, int cost)
{
    if (cost <= 0)
    {
        return true; // Free abilities always affordable
    }

    switch (resourceType)
    {
        case ResourceType.Health:
            return CanAffordHealth(combatant, cost);
        case ResourceType.Stamina:
            return CanAffordStamina(combatant, cost);
        case ResourceType.Aether:
            return CanAffordAether(combatant, cost);
        default:
            _logger.LogError("[Resource] Unknown ResourceType: {Type}", resourceType);
            return false;
    }
}
```

**Resource-Specific Logic**:

1. **Health** (`CanAffordHealth`):
   - Check: `combatant.CurrentHp >= cost` (must have at least the cost amount)
   - Rationale: Allows spending HP if you have exactly enough; death occurs at 0 HP

2. **Stamina** (`CanAffordStamina`):
   - Check: `combatant.CurrentStamina >= cost`
   - Rationale: Standard resource pool check

3. **Aether** (`CanAffordAether`):
   - **Archetype Check**: If non-Mystic, return `false` immediately
   - **Direct Afford**: If `CurrentAp >= cost`, return `true`
   - **Overcast Calculation**:
     ```csharp
     var apShortfall = cost - combatant.CurrentAp;
     var hpCost = apShortfall * OvercastHpRatio; // 2 HP per 1 AP
     return combatant.CurrentHp > hpCost;
     ```
   - **Survival Requirement**: Overcast requires `CurrentHp > hpCost` (cannot kill self)

**Edge Cases**:
- **Zero/Negative Cost**: Returns `true` (free abilities always affordable)
- **Non-Mystic Aether Request**: Returns `false` (archetype restriction)
- **Overcast at 1 HP**: Cannot Overcast if shortfall requires 2+ HP
- **Exact HP Match**: Overcast requires *strictly greater than* HP cost (e.g., 30 HP available, 30 HP cost = cannot afford)

**Logging**:
```csharp
_logger.LogDebug(
    "[Resource] {Name} CanAfford {Cost} {Type}: {Result}",
    combatant.Name, cost, resourceType, result);
```

---

### 2. Resource Deduction (`Deduct`)

**Signature**: `bool Deduct(Combatant combatant, ResourceType resourceType, int cost)`

**Purpose**: Subtracts resource costs from combatant pools and synchronizes changes to source entities.

**Behavior**:

```csharp
// ResourceService.cs:73-106
public bool Deduct(Combatant combatant, ResourceType resourceType, int cost)
{
    if (cost <= 0)
    {
        return true; // Nothing to deduct
    }

    if (!CanAfford(combatant, resourceType, cost))
    {
        _logger.LogWarning(
            "[Resource] {Name} CANNOT AFFORD {Cost} {Type}",
            combatant.Name, cost, resourceType);
        return false;
    }

    switch (resourceType)
    {
        case ResourceType.Health:
            return DeductHealth(combatant, cost);
        case ResourceType.Stamina:
            return DeductStamina(combatant, cost);
        case ResourceType.Aether:
            return DeductAether(combatant, cost);
        default:
            return false;
    }
}
```

**Resource-Specific Logic**:

1. **Health** (`DeductHealth`):
   ```csharp
   combatant.CurrentHp -= cost;
   SyncToSource(combatant, ResourceType.Health);
   ```

2. **Stamina** (`DeductStamina`):
   ```csharp
   combatant.CurrentStamina -= cost;
   SyncToSource(combatant, ResourceType.Stamina);
   ```

3. **Aether** (`DeductAether`) - **COMPLEX OVERCAST LOGIC**:
   ```csharp
   // ResourceService.cs:172-197
   private bool DeductAether(Combatant combatant, int cost)
   {
       // Direct deduction if sufficient AP
       if (combatant.CurrentAp >= cost)
       {
           combatant.CurrentAp -= cost;
           SyncToSource(combatant, ResourceType.Aether);
           return true;
       }

       // OVERCAST: Spend remaining AP + convert HP
       var apSpent = combatant.CurrentAp;
       var apShortfall = cost - apSpent;
       var hpCost = apShortfall * OvercastHpRatio; // 2 HP per 1 AP

       combatant.CurrentAp = 0; // Drain all AP
       combatant.CurrentHp -= hpCost; // Pay HP cost

       _logger.LogWarning(
           "[Resource] {Name} OVERCASTS! Spent {ApSpent} AP + {HpCost} HP for {TotalCost} AP ability.",
           combatant.Name, apSpent, hpCost, cost);

       SyncToSource(combatant, ResourceType.Health);
       return true;
   }
   ```

**Synchronization**:

The `SyncToSource` method updates the underlying `CharacterSource` or `Enemy` entity:

```csharp
// ResourceService.cs:199-223
private void SyncToSource(Combatant combatant, ResourceType resourceType)
{
    switch (combatant.CharacterSource)
    {
        case Character character:
            character.CurrentHp = combatant.CurrentHp;
            character.CurrentStamina = combatant.CurrentStamina;
            character.CurrentAp = combatant.CurrentAp;
            break;

        case Enemy enemy:
            enemy.CurrentHp = combatant.CurrentHp;
            enemy.CurrentStamina = combatant.CurrentStamina;
            enemy.CurrentAp = combatant.CurrentAp;
            break;

        default:
            _logger.LogWarning(
                "[Resource] Cannot sync {Name} - CharacterSource is null or unknown type",
                combatant.Name);
            break;
    }
}
```

**Edge Cases**:
- **Deduction Below 0**: No explicit clamping (assumes `CanAfford` validation prevents this)
- **Overcast with 0 AP**: Entire cost converted to HP (e.g., 0 AP, 10 AP cost = 20 HP cost)
- **Null CharacterSource**: Logs warning but does not throw exception

**Logging**:
- **Standard Deduction**: Debug-level log
- **Overcast Event**: **Warning-level log** for visibility (includes AP spent + HP cost breakdown)
- **Cannot Afford**: Warning-level log with cost details

---

### 3. Stamina Regeneration (`RegenerateStamina`)

**Signature**: `int RegenerateStamina(Combatant combatant)`

**Purpose**: Restores stamina at the end of each combat turn based on the Finesse attribute.

**Behavior**:

```csharp
// ResourceService.cs:108-141
public int RegenerateStamina(Combatant combatant)
{
    // Status Effect Check: Stunned blocks regeneration
    if (HasStatusEffect(combatant, StatusEffectType.Stunned))
    {
        _logger.LogDebug(
            "[Resource] {Name} is STUNNED, no stamina regen this turn",
            combatant.Name);
        return 0;
    }

    // Calculate regeneration: Base 5 + (Finesse / 2)
    var finesse = combatant.GetAttribute(Core.Enums.Attribute.Finesse);
    var regenAmount = BaseStaminaRegen + (finesse / 2);

    // Clamp to max stamina (cannot exceed maximum)
    var actualRegen = Math.Min(regenAmount, combatant.MaxStamina - combatant.CurrentStamina);

    if (actualRegen <= 0)
    {
        _logger.LogDebug(
            "[Resource] {Name} stamina already at max ({Current}/{Max})",
            combatant.Name, combatant.CurrentStamina, combatant.MaxStamina);
        return 0;
    }

    combatant.CurrentStamina += actualRegen;
    SyncToSource(combatant, ResourceType.Stamina);

    _logger.LogInformation(
        "[Resource] {Name} regenerates {Regen} stamina ({Current}/{Max})",
        combatant.Name, actualRegen, combatant.CurrentStamina, combatant.MaxStamina);

    return actualRegen;
}
```

**Formula Breakdown**:

```
regenAmount = BaseStaminaRegen + (Finesse / 2)
            = 5 + (Finesse / 2)

Examples:
- Finesse 4: 5 + (4/2) = 7 stamina
- Finesse 6: 5 + (6/2) = 8 stamina
- Finesse 10: 5 + (10/2) = 10 stamina
```

**Status Effect Integration**:

```csharp
// ResourceService.cs:225-235
private bool HasStatusEffect(Combatant combatant, StatusEffectType effectType)
{
    if (_statusEffectService == null)
    {
        return false;
    }

    var activeEffects = _statusEffectService.GetActiveEffects(combatant);
    return activeEffects.Any(e => e.Type == effectType);
}
```

**Edge Cases**:
- **Stunned Status**: Returns `0` regeneration (status effect blocks regen)
- **Already at Max**: Returns `0` (no overheal)
- **Null StatusEffectService**: Assumes no status effects (failsafe for testing)
- **Negative Finesse**: Integer division handles gracefully (Finesse -4 → -2, total = 3 regen)

**Logging**:
- **Successful Regen**: Information-level log with amount and new total
- **Stunned**: Debug-level log
- **Already at Max**: Debug-level log

---

### 4. Resource Getters

**GetCurrent**:
```csharp
// ResourceService.cs:143-153
public int GetCurrent(Combatant combatant, ResourceType resourceType)
{
    return resourceType switch
    {
        ResourceType.Health => combatant.CurrentHp,
        ResourceType.Stamina => combatant.CurrentStamina,
        ResourceType.Aether => combatant.CurrentAp,
        _ => 0
    };
}
```

**GetMax**:
```csharp
// ResourceService.cs:155-165
public int GetMax(Combatant combatant, ResourceType resourceType)
{
    return resourceType switch
    {
        ResourceType.Health => combatant.MaxHp,
        ResourceType.Stamina => combatant.MaxStamina,
        ResourceType.Aether => combatant.MaxAp,
        _ => 0
    };
}
```

**Purpose**: Provide read-only access to resource values for UI rendering and combat logic.

---

### 5. Archetype Validation (`IsMystic`)

**Signature**: `bool IsMystic(Combatant combatant)`

**Purpose**: Determine if a combatant is a Mystic archetype (eligible for Aether abilities).

**Behavior**:

```csharp
// ResourceService.cs:167-170
private bool IsMystic(Combatant combatant)
{
    return combatant.Archetype == ArchetypeType.Mystic;
}
```

**Integration**: Called by `CanAffordAether` to enforce archetype restrictions.

**Edge Cases**:
- **Null Archetype**: Comparison returns `false` (safe default)
- **Future Archetypes**: If new Mystic-like archetypes are added, this logic must expand

---

## Restrictions

### Hard Constraints (MUST NOT Violate)

1. **MUST NOT allow Overcast that kills the caster**:
   - `CanAffordAether` enforces `CurrentHp > hpCost` (strictly greater than)
   - Example: 30 HP, 15 AP shortfall = 30 HP cost → cannot afford (would reach 0 HP)

2. **MUST NOT allow non-Mystics to use Aether**:
   - `CanAffordAether` returns `false` immediately for non-Mystic archetypes
   - No exceptions (even if non-Mystic has AP somehow)

3. **MUST synchronize all resource changes to CharacterSource**:
   - `SyncToSource` called after every deduction/regeneration
   - Ensures `Character` or `Enemy` entities reflect current resource state

4. **MUST validate affordability before deduction**:
   - `Deduct` calls `CanAfford` internally before subtracting resources
   - Returns `false` if validation fails (no partial deductions)

5. **MUST enforce Overcast HP-to-AP ratio of 2:1**:
   - Hardcoded `OvercastHpRatio = 2` constant
   - Formula: `hpCost = apShortfall × 2`

6. **MUST respect status effects in regeneration**:
   - Stunned combatants regenerate 0 stamina
   - Other debuffs may be added in future

### Soft Constraints (SHOULD Follow)

1. **SHOULD log all Overcast events at Warning level**:
   - Provides visibility for balance tuning
   - Example: `"Anya OVERCASTS! Spent 5 AP + 10 HP for 10 AP ability."`

2. **SHOULD clamp regeneration to MaxStamina**:
   - Prevents stamina overflow bugs
   - Formula: `actualRegen = Min(regenAmount, MaxStamina - CurrentStamina)`

3. **SHOULD handle null CharacterSource gracefully**:
   - Log warning but do not throw exception
   - Allows testing with incomplete combatant setups

---

## Limitations

### Design Limitations

1. **No HP or Aether Regeneration**:
   - Only Stamina regenerates automatically
   - HP recovery requires rest, healing abilities, or consumables
   - Aether recovery requires rest or special abilities (not implemented)

2. **Fixed Overcast Ratio**:
   - 2:1 HP-to-AP ratio is hardcoded
   - No talents/items to modify ratio (future enhancement opportunity)

3. **Single Archetype Check**:
   - Only `Mystic` archetype can use Aether
   - Future archetypes (e.g., "Rune-Scribe") would require logic expansion

4. **No Resource Overflow Protection**:
   - Deductions can theoretically drive resources negative (assumes validation prevents this)
   - No explicit clamping to 0 in deduct methods

5. **Stamina Regen Formula Simplicity**:
   - Linear formula: `5 + (Finesse / 2)`
   - No diminishing returns or scaling caps

6. **No Resource Pooling**:
   - Each combatant has independent resource pools
   - No party-shared resources or "link HP" mechanics

### Performance Limitations

1. **StatusEffectService Dependency**:
   - Regeneration calls `GetActiveEffects()` every turn
   - Potential bottleneck in large combats (10+ combatants)

2. **Synchronization Overhead**:
   - Every deduction/regen triggers `SyncToSource`
   - May cause database update pressure in future (if CharacterSource is EF-tracked)

---

## Use Cases

### UC-RESOURCE-01: Warrior Uses Melee Ability (Stamina Cost)

**Scenario**: A Warrior player attempts to use "Power Strike" (5 Stamina cost) with 20/50 Stamina.

**Pre-Conditions**:
- Combatant: `{ Name = "Thorvald", Archetype = Warrior, CurrentStamina = 20, MaxStamina = 50 }`
- Ability: `Power Strike` (5 Stamina cost)

**Execution**:
```csharp
// 1. Validation
bool canAfford = _resourceService.CanAfford(thorvald, ResourceType.Stamina, 5);
// Returns: true (20 >= 5)

// 2. Deduction
bool success = _resourceService.Deduct(thorvald, ResourceType.Stamina, 5);
// Returns: true
// Updates: thorvald.CurrentStamina = 15
// Syncs: thorvald.CharacterSource.CurrentStamina = 15
```

**Post-Conditions**:
- `CurrentStamina`: 15/50
- Logs: `"[Resource] Thorvald deducted 5 Stamina (15/50 remaining)"`

**Code References**:
- `CanAffordStamina`: [ResourceService.cs:252-255](ResourceService.cs:252-255)
- `DeductStamina`: [ResourceService.cs:257-263](ResourceService.cs:257-263)

---

### UC-RESOURCE-02: Mystic Casts Spell with Sufficient AP (No Overcast)

**Scenario**: A Mystic player casts "Flame Bolt" (8 AP cost) with 15/30 AP.

**Pre-Conditions**:
- Combatant: `{ Name = "Anya", Archetype = Mystic, CurrentAp = 15, MaxAp = 30, CurrentHp = 50 }`
- Ability: `Flame Bolt` (8 AP cost)

**Execution**:
```csharp
// 1. Validation
bool canAfford = _resourceService.CanAfford(anya, ResourceType.Aether, 8);
// Returns: true (15 >= 8, no Overcast needed)

// 2. Deduction
bool success = _resourceService.Deduct(anya, ResourceType.Aether, 8);
// Returns: true
// Updates: anya.CurrentAp = 7
// Syncs: anya.CharacterSource.CurrentAp = 7
```

**Post-Conditions**:
- `CurrentAp`: 7/30
- `CurrentHp`: 50 (unchanged)
- Logs: `"[Resource] Anya deducted 8 Aether (7/30 remaining)"`

**Code References**:
- `CanAffordAether`: [ResourceService.cs:237-250](ResourceService.cs:237-250)
- `DeductAether` (direct path): [ResourceService.cs:172-178](ResourceService.cs:172-178)

---

### UC-RESOURCE-03: Mystic Overcasts to Cast High-Cost Spell

**Scenario**: A Mystic player casts "Chain Lightning" (20 AP cost) with 10/30 AP and 50/60 HP.

**Pre-Conditions**:
- Combatant: `{ Name = "Anya", Archetype = Mystic, CurrentAp = 10, MaxAp = 30, CurrentHp = 50, MaxHp = 60 }`
- Ability: `Chain Lightning` (20 AP cost)

**Execution**:
```csharp
// 1. Validation with Overcast Calculation
var apShortfall = 20 - 10; // 10 AP missing
var hpCost = apShortfall * 2; // 20 HP cost
bool canAfford = _resourceService.CanAfford(anya, ResourceType.Aether, 20);
// Checks: 50 > 20? Yes → Returns: true

// 2. Deduction with Overcast
bool success = _resourceService.Deduct(anya, ResourceType.Aether, 20);
// Returns: true
// Updates:
//   anya.CurrentAp = 0 (drained all AP)
//   anya.CurrentHp = 30 (50 - 20)
// Syncs: CharacterSource updated
```

**Post-Conditions**:
- `CurrentAp`: 0/30 (all AP spent)
- `CurrentHp`: 30/60 (paid 20 HP for 10 missing AP)
- Logs: `"[Resource] Anya OVERCASTS! Spent 10 AP + 20 HP for 20 AP ability."`

**Code References**:
- `CanAffordAether` (Overcast path): [ResourceService.cs:245-248](ResourceService.cs:245-248)
- `DeductAether` (Overcast path): [ResourceService.cs:180-197](ResourceService.cs:180-197)

---

### UC-RESOURCE-04: Mystic Cannot Overcast (Would Die)

**Scenario**: A Mystic attempts to cast "Meteor Storm" (30 AP cost) with 0 AP and 20 HP.

**Pre-Conditions**:
- Combatant: `{ Name = "Anya", Archetype = Mystic, CurrentAp = 0, MaxAp = 30, CurrentHp = 20, MaxHp = 60 }`
- Ability: `Meteor Storm` (30 AP cost)

**Execution**:
```csharp
// 1. Validation with Overcast Calculation
var apShortfall = 30 - 0; // 30 AP missing
var hpCost = apShortfall * 2; // 60 HP cost
bool canAfford = _resourceService.CanAfford(anya, ResourceType.Aether, 30);
// Checks: 20 > 60? No → Returns: false

// 2. Deduction Attempt
bool success = _resourceService.Deduct(anya, ResourceType.Aether, 30);
// CanAfford fails → Returns: false
// No resource changes
```

**Post-Conditions**:
- `CurrentAp`: 0/30 (unchanged)
- `CurrentHp`: 20/60 (unchanged)
- Logs: `"[Resource] Anya CANNOT AFFORD 30 Aether (Overcast would require 60 HP, only has 20)"`

**Rationale**: Overcast requires `CurrentHp > hpCost` (strictly greater than). Even if Anya had exactly 60 HP, she still could not afford the cast (60 > 60 is false).

**Code References**:
- `CanAffordAether`: [ResourceService.cs:248](ResourceService.cs:248)

---

### UC-RESOURCE-05: Non-Mystic Attempts Aether Ability (Denied)

**Scenario**: A Warrior player attempts to use "Flame Bolt" (8 AP cost, Aether ability).

**Pre-Conditions**:
- Combatant: `{ Name = "Thorvald", Archetype = Warrior, CurrentAp = 10, CurrentHp = 100 }`
- Ability: `Flame Bolt` (8 AP cost, Aether)

**Execution**:
```csharp
// 1. Archetype Check
bool isMystic = _resourceService.IsMystic(thorvald);
// Returns: false (Archetype = Warrior)

// 2. Validation
bool canAfford = _resourceService.CanAfford(thorvald, ResourceType.Aether, 8);
// CanAffordAether → IsMystic check fails → Returns: false

// 3. Deduction Attempt
bool success = _resourceService.Deduct(thorvald, ResourceType.Aether, 8);
// Returns: false
```

**Post-Conditions**:
- `CurrentAp`: 10 (unchanged)
- Logs: `"[Resource] Thorvald CANNOT AFFORD 8 Aether (non-Mystic archetype)"`

**Rationale**: Only Mystic archetypes can use Aether abilities. Even if Thorvald somehow had AP, the archetype check blocks the ability.

**Code References**:
- `IsMystic`: [ResourceService.cs:167-170](ResourceService.cs:167-170)
- `CanAffordAether` (archetype gate): [ResourceService.cs:237-240](ResourceService.cs:237-240)

---

### UC-RESOURCE-06: Stamina Regeneration (Normal)

**Scenario**: At the end of a combat turn, a Ranger with Finesse 6 regenerates stamina.

**Pre-Conditions**:
- Combatant: `{ Name = "Einar", Finesse = 6, CurrentStamina = 20, MaxStamina = 50 }`
- No status effects

**Execution**:
```csharp
// 1. Status Effect Check
bool isStunned = _resourceService.HasStatusEffect(einar, StatusEffectType.Stunned);
// Returns: false

// 2. Calculate Regeneration
var finesse = einar.GetAttribute(Attribute.Finesse); // 6
var regenAmount = BaseStaminaRegen + (finesse / 2); // 5 + 3 = 8

// 3. Clamp to Max
var actualRegen = Math.Min(8, 50 - 20); // Min(8, 30) = 8

// 4. Apply Regeneration
int regen = _resourceService.RegenerateStamina(einar);
// Returns: 8
// Updates: einar.CurrentStamina = 28
// Syncs: einar.CharacterSource.CurrentStamina = 28
```

**Post-Conditions**:
- `CurrentStamina`: 28/50 (+8)
- Logs: `"[Resource] Einar regenerates 8 stamina (28/50)"`

**Code References**:
- `RegenerateStamina`: [ResourceService.cs:117-120](ResourceService.cs:117-120)

---

### UC-RESOURCE-07: Stamina Regeneration Blocked by Stunned Status

**Scenario**: A stunned Warrior attempts to regenerate stamina at turn end.

**Pre-Conditions**:
- Combatant: `{ Name = "Thorvald", Finesse = 4, CurrentStamina = 10, MaxStamina = 50 }`
- Active Status Effects: `[Stunned (2 turns remaining)]`

**Execution**:
```csharp
// 1. Status Effect Check
bool isStunned = _resourceService.HasStatusEffect(thorvald, StatusEffectType.Stunned);
// Returns: true

// 2. Regeneration Blocked
int regen = _resourceService.RegenerateStamina(thorvald);
// Returns: 0 (Stunned blocks regen)
// No resource changes
```

**Post-Conditions**:
- `CurrentStamina`: 10/50 (unchanged)
- Logs: `"[Resource] Thorvald is STUNNED, no stamina regen this turn"`

**Code References**:
- `RegenerateStamina` (Stunned check): [ResourceService.cs:110-116](ResourceService.cs:110-116)
- `HasStatusEffect`: [ResourceService.cs:225-235](ResourceService.cs:225-235)

---

### UC-RESOURCE-08: Stamina Regeneration with Already-Full Stamina

**Scenario**: A combatant with full stamina regenerates at turn end.

**Pre-Conditions**:
- Combatant: `{ Name = "Einar", Finesse = 6, CurrentStamina = 50, MaxStamina = 50 }`

**Execution**:
```csharp
// 1. Calculate Regeneration
var regenAmount = 5 + (6 / 2); // 8

// 2. Clamp to Max
var actualRegen = Math.Min(8, 50 - 50); // Min(8, 0) = 0

// 3. Early Return
int regen = _resourceService.RegenerateStamina(einar);
// Returns: 0
// No resource changes
```

**Post-Conditions**:
- `CurrentStamina`: 50/50 (unchanged)
- Logs: `"[Resource] Einar stamina already at max (50/50)"`

**Code References**:
- `RegenerateStamina` (max clamp): [ResourceService.cs:122-129](ResourceService.cs:122-129)

---

### UC-RESOURCE-09: Edge Case - Overcast with Exactly Matching HP

**Scenario**: A Mystic with exactly enough HP to match the Overcast cost attempts to cast.

**Pre-Conditions**:
- Combatant: `{ Name = "Anya", Archetype = Mystic, CurrentAp = 0, CurrentHp = 20, MaxHp = 60 }`
- Ability: `Flame Bolt` (10 AP cost → 20 HP Overcast cost)

**Execution**:
```csharp
// 1. Validation
var apShortfall = 10 - 0; // 10 AP missing
var hpCost = 10 * 2; // 20 HP cost
bool canAfford = _resourceService.CanAfford(anya, ResourceType.Aether, 10);
// Checks: 20 > 20? No → Returns: false
```

**Post-Conditions**:
- `CurrentAp`: 0 (unchanged)
- `CurrentHp`: 20 (unchanged)
- Ability **cannot be cast**

**Rationale**: The survival check uses `>` (strictly greater than), not `>=`. This prevents Mystics from casting at the cost of death. To cast a 10 AP ability with 0 AP, the Mystic must have **at least 21 HP** (20 HP cost + 1 HP to survive).

**Code References**:
- `CanAffordAether`: [ResourceService.cs:248](ResourceService.cs:248)

---

### UC-RESOURCE-10: Resource Getter Usage (UI Display)

**Scenario**: Combat UI needs to display current HP, Stamina, and AP for a Mystic combatant.

**Pre-Conditions**:
- Combatant: `{ Name = "Anya", CurrentHp = 45, MaxHp = 60, CurrentStamina = 28, MaxStamina = 50, CurrentAp = 12, MaxAp = 30 }`

**Execution**:
```csharp
// 1. Get Current Resources
int currentHp = _resourceService.GetCurrent(anya, ResourceType.Health); // 45
int currentStamina = _resourceService.GetCurrent(anya, ResourceType.Stamina); // 28
int currentAp = _resourceService.GetCurrent(anya, ResourceType.Aether); // 12

// 2. Get Max Resources
int maxHp = _resourceService.GetMax(anya, ResourceType.Health); // 60
int maxStamina = _resourceService.GetMax(anya, ResourceType.Stamina); // 50
int maxAp = _resourceService.GetMax(anya, ResourceType.Aether); // 30

// 3. UI Rendering
Console.WriteLine($"HP: {currentHp}/{maxHp}"); // "HP: 45/60"
Console.WriteLine($"Stamina: {currentStamina}/{maxStamina}"); // "Stamina: 28/50"
Console.WriteLine($"AP: {currentAp}/{maxAp}"); // "AP: 12/30"
```

**Post-Conditions**:
- No resource changes (read-only operation)

**Code References**:
- `GetCurrent`: [ResourceService.cs:143-153](ResourceService.cs:143-153)
- `GetMax`: [ResourceService.cs:155-165](ResourceService.cs:155-165)

---

## Decision Trees

### Decision Tree 1: CanAfford Resource Type Routing

```
Input: CanAfford(combatant, resourceType, cost)
│
├─ cost <= 0?
│  └─ YES → Return TRUE (free ability)
│
└─ NO → Switch on resourceType
   │
   ├─ ResourceType.Health
   │  └─ CanAffordHealth(combatant, cost)
   │     └─ combatant.CurrentHp >= cost?
   │        ├─ YES → Return TRUE
   │        └─ NO → Return FALSE
   │
   ├─ ResourceType.Stamina
   │  └─ CanAffordStamina(combatant, cost)
   │     └─ combatant.CurrentStamina >= cost?
   │        ├─ YES → Return TRUE
   │        └─ NO → Return FALSE
   │
   ├─ ResourceType.Aether
   │  └─ CanAffordAether(combatant, cost)
   │     │
   │     ├─ IsMystic(combatant)?
   │     │  └─ NO → Return FALSE (archetype restriction)
   │     │
   │     └─ YES → combatant.CurrentAp >= cost?
   │        │
   │        ├─ YES → Return TRUE (direct afford)
   │        │
   │        └─ NO → Calculate Overcast
   │           │
   │           ├─ apShortfall = cost - CurrentAp
   │           ├─ hpCost = apShortfall × 2
   │           │
   │           └─ combatant.CurrentHp > hpCost?
   │              ├─ YES → Return TRUE (can Overcast)
   │              └─ NO → Return FALSE (would die)
   │
   └─ Unknown ResourceType
      └─ Log Error → Return FALSE
```

**Key Nodes**:
- **Archetype Gate**: Non-Mystics cannot use Aether abilities (hard constraint)
- **Overcast Calculation**: `hpCost = (cost - CurrentAp) × 2`
- **Survival Check**: `CurrentHp > hpCost` (strictly greater than)

---

### Decision Tree 2: Deduct Resource Type Routing

```
Input: Deduct(combatant, resourceType, cost)
│
├─ cost <= 0?
│  └─ YES → Return TRUE (nothing to deduct)
│
├─ CanAfford(combatant, resourceType, cost)?
│  └─ NO → Log Warning → Return FALSE
│
└─ YES → Switch on resourceType
   │
   ├─ ResourceType.Health
   │  └─ DeductHealth(combatant, cost)
   │     ├─ combatant.CurrentHp -= cost
   │     ├─ SyncToSource(combatant, Health)
   │     └─ Return TRUE
   │
   ├─ ResourceType.Stamina
   │  └─ DeductStamina(combatant, cost)
   │     ├─ combatant.CurrentStamina -= cost
   │     ├─ SyncToSource(combatant, Stamina)
   │     └─ Return TRUE
   │
   ├─ ResourceType.Aether
   │  └─ DeductAether(combatant, cost)
   │     │
   │     ├─ combatant.CurrentAp >= cost?
   │     │  │
   │     │  ├─ YES → Direct Deduction
   │     │  │  ├─ combatant.CurrentAp -= cost
   │     │  │  ├─ SyncToSource(combatant, Aether)
   │     │  │  └─ Return TRUE
   │     │  │
   │     │  └─ NO → Overcast Deduction
   │     │     ├─ apSpent = combatant.CurrentAp
   │     │     ├─ apShortfall = cost - apSpent
   │     │     ├─ hpCost = apShortfall × 2
   │     │     ├─ combatant.CurrentAp = 0
   │     │     ├─ combatant.CurrentHp -= hpCost
   │     │     ├─ Log Warning (OVERCAST event)
   │     │     ├─ SyncToSource(combatant, Health)
   │     │     └─ Return TRUE
   │
   └─ Unknown ResourceType
      └─ Return FALSE
```

**Key Nodes**:
- **Pre-Validation**: Always check `CanAfford` before deducting
- **Overcast Path**: Drains all AP + converts HP at 2:1 ratio
- **Synchronization**: Every deduction triggers `SyncToSource`

---

### Decision Tree 3: Stamina Regeneration

```
Input: RegenerateStamina(combatant)
│
├─ HasStatusEffect(combatant, Stunned)?
│  └─ YES → Log Debug (Stunned blocks regen) → Return 0
│
└─ NO → Calculate Regeneration
   │
   ├─ finesse = combatant.GetAttribute(Attribute.Finesse)
   ├─ regenAmount = BaseStaminaRegen + (finesse / 2)
   │              = 5 + (finesse / 2)
   │
   ├─ actualRegen = Min(regenAmount, MaxStamina - CurrentStamina)
   │
   ├─ actualRegen <= 0?
   │  └─ YES → Log Debug (already at max) → Return 0
   │
   └─ NO → Apply Regeneration
      ├─ combatant.CurrentStamina += actualRegen
      ├─ SyncToSource(combatant, Stamina)
      ├─ Log Information (regen amount + new total)
      └─ Return actualRegen
```

**Key Nodes**:
- **Status Effect Check**: Stunned blocks all regeneration
- **Finesse Scaling**: Integer division by 2 (e.g., Finesse 7 → +3 bonus)
- **Max Clamping**: Prevents stamina overflow

---

## Sequence Diagrams

### Sequence Diagram 1: Mystic Overcast Casting

```
Actor: CombatService
Service: ResourceService
Entity: Combatant (Anya)

CombatService -> ResourceService: CanAfford(anya, Aether, 20)
ResourceService -> ResourceService: IsMystic(anya)?
ResourceService -> ResourceService: CurrentAp (10) >= 20? NO
ResourceService -> ResourceService: Calculate Overcast:
                                     apShortfall = 20 - 10 = 10
                                     hpCost = 10 × 2 = 20
ResourceService -> ResourceService: CurrentHp (50) > 20? YES
ResourceService -> CombatService: Return TRUE

CombatService -> ResourceService: Deduct(anya, Aether, 20)
ResourceService -> ResourceService: CanAfford? YES (already validated)
ResourceService -> ResourceService: DeductAether(anya, 20):
                                     apSpent = 10
                                     apShortfall = 10
                                     hpCost = 20
ResourceService -> Combatant: CurrentAp = 0
ResourceService -> Combatant: CurrentHp = 30 (50 - 20)
ResourceService -> ResourceService: SyncToSource(anya, Health)
ResourceService -> Logger: LogWarning("Anya OVERCASTS! Spent 10 AP + 20 HP for 20 AP ability.")
ResourceService -> CombatService: Return TRUE

CombatService -> AbilityService: ExecuteAbility(anya, "Chain Lightning")
```

**Key Interactions**:
1. Validation calculates Overcast cost before deduction
2. Deduction drains all AP (10 → 0) and converts HP (50 → 30)
3. Synchronization updates `CharacterSource` entity
4. Warning-level log provides visibility

---

### Sequence Diagram 2: Stamina Regeneration at Turn End

```
Actor: CombatService
Service: ResourceService
Service: StatusEffectService
Entity: Combatant (Einar)

CombatService -> ResourceService: RegenerateStamina(einar)
ResourceService -> StatusEffectService: GetActiveEffects(einar)
StatusEffectService -> ResourceService: Return [None]
ResourceService -> ResourceService: HasStatusEffect(Stunned)? NO
ResourceService -> Combatant: GetAttribute(Finesse) → 6
ResourceService -> ResourceService: regenAmount = 5 + (6 / 2) = 8
ResourceService -> ResourceService: actualRegen = Min(8, 50 - 20) = 8
ResourceService -> Combatant: CurrentStamina = 28 (20 + 8)
ResourceService -> ResourceService: SyncToSource(einar, Stamina)
ResourceService -> Logger: LogInformation("Einar regenerates 8 stamina (28/50)")
ResourceService -> CombatService: Return 8
```

**Key Interactions**:
1. Status effect check queries `StatusEffectService`
2. Finesse attribute determines regen amount
3. Clamping prevents overflow
4. Synchronization updates source entity
5. Information-level log for visibility

---

### Sequence Diagram 3: Non-Mystic Aether Denial

```
Actor: CombatService
Service: ResourceService
Entity: Combatant (Thorvald - Warrior)

CombatService -> ResourceService: CanAfford(thorvald, Aether, 8)
ResourceService -> ResourceService: IsMystic(thorvald)?
ResourceService -> Combatant: thorvald.Archetype → Warrior
ResourceService -> ResourceService: Archetype == Mystic? NO
ResourceService -> Logger: LogDebug("Thorvald cannot use Aether (non-Mystic)")
ResourceService -> CombatService: Return FALSE

CombatService -> UI: DisplayMessage("You cannot use mystic abilities!")
```

**Key Interactions**:
1. Archetype check occurs immediately in `CanAffordAether`
2. No Overcast calculation needed (early return)
3. Debug-level log (not warning, since this is expected behavior)
4. UI layer displays user-friendly error message

---

## Workflows

### Workflow 1: Resource Validation Checklist

**Purpose**: Validate if a combatant can afford an ability cost before execution.

**Steps**:
1. ✅ **Identify Resource Type**: Determine if ability costs Health, Stamina, or Aether
2. ✅ **Get Cost Amount**: Retrieve ability cost from `Ability` entity
3. ✅ **Call CanAfford**: `ResourceService.CanAfford(combatant, resourceType, cost)`
4. ✅ **Handle Result**:
   - **TRUE**: Proceed to deduction
   - **FALSE**: Display error message, cancel ability
5. ✅ **Log Outcome**: Debug-level log for validation result

**Example**:
```csharp
var ability = GetAbility("Power Strike");
var cost = ability.StaminaCost; // 5
var canAfford = _resourceService.CanAfford(combatant, ResourceType.Stamina, cost);

if (!canAfford)
{
    _ui.DisplayMessage($"Not enough stamina! Need {cost}, have {combatant.CurrentStamina}");
    return;
}

// Proceed with ability execution...
```

---

### Workflow 2: Resource Deduction Checklist

**Purpose**: Deduct ability costs from combatant resource pools.

**Steps**:
1. ✅ **Pre-Validate**: Ensure `CanAfford` returned TRUE (Deduct internally validates again)
2. ✅ **Call Deduct**: `ResourceService.Deduct(combatant, resourceType, cost)`
3. ✅ **Check Return Value**:
   - **TRUE**: Deduction successful, resources updated
   - **FALSE**: Deduction failed (should never happen if pre-validated)
4. ✅ **Verify Synchronization**: Confirm `CharacterSource` entity updated
5. ✅ **Log Outcome**: Info-level log for successful deduction, Warning-level for Overcast

**Example**:
```csharp
bool success = _resourceService.Deduct(combatant, ResourceType.Aether, 15);

if (!success)
{
    _logger.LogError("Deduction failed despite CanAfford validation!");
    return;
}

// Execute ability effect...
_abilityService.ExecuteEffect(combatant, ability);
```

---

### Workflow 3: Stamina Regeneration Checklist (Turn End)

**Purpose**: Regenerate stamina for all combatants at the end of each combat turn.

**Steps**:
1. ✅ **Iterate All Combatants**: Loop through player party + enemy combatants
2. ✅ **Call RegenerateStamina**: `ResourceService.RegenerateStamina(combatant)`
3. ✅ **Check Status Effects**: Service internally checks for Stunned (blocks regen)
4. ✅ **Calculate Regen Amount**: `5 + (Finesse / 2)`
5. ✅ **Clamp to Max**: Prevent stamina overflow
6. ✅ **Apply Regeneration**: Update `CurrentStamina` and sync to source
7. ✅ **Log Outcome**: Info-level log for successful regen, Debug-level if blocked/at max
8. ✅ **Display to UI**: Show "Combatant regenerated X stamina" message

**Example**:
```csharp
// At end of combat turn
foreach (var combatant in _combatService.GetAllCombatants())
{
    var regenAmount = _resourceService.RegenerateStamina(combatant);

    if (regenAmount > 0)
    {
        _ui.DisplayMessage($"{combatant.Name} regenerated {regenAmount} stamina.");
    }
}
```

---

### Workflow 4: Overcast Decision Flow (Internal)

**Purpose**: Determine if a Mystic can Overcast and calculate HP cost.

**Steps**:
1. ✅ **Archetype Check**: Verify `combatant.Archetype == Mystic` (non-Mystics cannot Overcast)
2. ✅ **Direct Afford Check**: If `CurrentAp >= cost`, no Overcast needed (return TRUE)
3. ✅ **Calculate Shortfall**: `apShortfall = cost - CurrentAp`
4. ✅ **Calculate HP Cost**: `hpCost = apShortfall × OvercastHpRatio` (2:1 ratio)
5. ✅ **Survival Check**: `CurrentHp > hpCost`? (strictly greater than)
   - **YES**: Can Overcast (return TRUE)
   - **NO**: Would die (return FALSE)
6. ✅ **Log Decision**: Debug-level log with shortfall and HP cost breakdown

**Example** (Internal to ResourceService):
```csharp
// CanAffordAether logic
if (!IsMystic(combatant))
{
    return false; // Step 1: Archetype gate
}

if (combatant.CurrentAp >= cost)
{
    return true; // Step 2: Direct afford
}

// Step 3-5: Overcast calculation
var apShortfall = cost - combatant.CurrentAp;
var hpCost = apShortfall * OvercastHpRatio;

return combatant.CurrentHp > hpCost; // Step 5: Survival check
```

---

### Workflow 5: Source Synchronization Checklist

**Purpose**: Ensure `CharacterSource` or `Enemy` entities reflect current resource state.

**Steps**:
1. ✅ **Identify Source Type**: Check if `CharacterSource` is `Character` or `Enemy`
2. ✅ **Copy Current Resources**: Update `CurrentHp`, `CurrentStamina`, `CurrentAp` from `Combatant`
3. ✅ **Handle Null Source**: Log warning if `CharacterSource` is null (graceful degradation)
4. ✅ **Trigger Persistence** (if applicable): If EF-tracked, changes will persist on `SaveChanges()`

**Example** (Internal to ResourceService):
```csharp
// SyncToSource logic
switch (combatant.CharacterSource)
{
    case Character character:
        character.CurrentHp = combatant.CurrentHp;
        character.CurrentStamina = combatant.CurrentStamina;
        character.CurrentAp = combatant.CurrentAp;
        break;

    case Enemy enemy:
        enemy.CurrentHp = combatant.CurrentHp;
        enemy.CurrentStamina = combatant.CurrentStamina;
        enemy.CurrentAp = combatant.CurrentAp;
        break;

    default:
        _logger.LogWarning("Cannot sync {Name} - CharacterSource is null", combatant.Name);
        break;
}
```

---

## Cross-System Integration

### Integration 1: CombatService (Ability Execution)

**Relationship**: `CombatService` → `ResourceService`

**Integration Points**:
1. **Ability Cost Validation**:
   ```csharp
   // CombatService.cs (hypothetical)
   public bool CanExecuteAbility(Combatant actor, Ability ability)
   {
       return _resourceService.CanAfford(actor, ability.ResourceType, ability.Cost);
   }
   ```

2. **Ability Cost Deduction**:
   ```csharp
   public void ExecuteAbility(Combatant actor, Ability ability)
   {
       bool success = _resourceService.Deduct(actor, ability.ResourceType, ability.Cost);

       if (!success)
       {
           throw new InvalidOperationException("Deduction failed despite validation!");
       }

       _abilityService.ExecuteEffect(actor, ability);
   }
   ```

3. **Turn End Regeneration**:
   ```csharp
   public void EndTurn()
   {
       foreach (var combatant in GetAllCombatants())
       {
           _resourceService.RegenerateStamina(combatant);
       }
   }
   ```

**Data Flow**:
- `CombatService` provides `Combatant` entities to `ResourceService`
- `ResourceService` updates resource values and syncs to source entities
- `CombatService` displays UI messages based on resource changes

---

### Integration 2: AbilityService (Cost Definitions)

**Relationship**: `AbilityService` ↔ `ResourceService`

**Integration Points**:
1. **Ability Cost Metadata**:
   ```csharp
   // Ability.cs (entity)
   public class Ability
   {
       public ResourceType CostType { get; set; } // Health, Stamina, Aether
       public int Cost { get; set; } // 5, 10, 15, etc.
   }
   ```

2. **Cost Validation Flow**:
   ```csharp
   // AbilityService calls ResourceService for validation
   var ability = GetAbility("Power Strike");
   bool canAfford = _resourceService.CanAfford(combatant, ability.CostType, ability.Cost);
   ```

**Data Flow**:
- `AbilityService` stores cost metadata in `Ability` entities
- `ResourceService` consumes cost metadata for validation/deduction
- No circular dependency (one-way data flow)

---

### Integration 3: StatusEffectService (Regeneration Blocking)

**Relationship**: `ResourceService` → `StatusEffectService`

**Integration Points**:
1. **Status Effect Query**:
   ```csharp
   // ResourceService.cs:225-235
   private bool HasStatusEffect(Combatant combatant, StatusEffectType effectType)
   {
       if (_statusEffectService == null)
       {
           return false; // Graceful degradation for testing
       }

       var activeEffects = _statusEffectService.GetActiveEffects(combatant);
       return activeEffects.Any(e => e.Type == effectType);
   }
   ```

2. **Stunned Check in Regeneration**:
   ```csharp
   // ResourceService.cs:110-116
   public int RegenerateStamina(Combatant combatant)
   {
       if (HasStatusEffect(combatant, StatusEffectType.Stunned))
       {
           _logger.LogDebug("{Name} is STUNNED, no stamina regen", combatant.Name);
           return 0;
       }

       // ... regeneration logic
   }
   ```

**Data Flow**:
- `StatusEffectService` tracks active status effects per combatant
- `ResourceService` queries status effects to block regeneration
- Future: Other status effects (e.g., "Energized" for bonus regen)

---

### Integration 4: UI Layer (Resource Display)

**Relationship**: UI ViewModel → `ResourceService`

**Integration Points**:
1. **Current/Max Resource Getters**:
   ```csharp
   // CombatViewModel.cs (hypothetical)
   public class CombatantViewModel
   {
       public string HpBar => $"{_resourceService.GetCurrent(Combatant, ResourceType.Health)}/{_resourceService.GetMax(Combatant, ResourceType.Health)}";
       public string StaminaBar => $"{_resourceService.GetCurrent(Combatant, ResourceType.Stamina)}/{_resourceService.GetMax(Combatant, ResourceType.Stamina)}";
       public string ApBar => $"{_resourceService.GetCurrent(Combatant, ResourceType.Aether)}/{_resourceService.GetMax(Combatant, ResourceType.Aether)}";
   }
   ```

2. **Overcast Warning**:
   ```csharp
   // UI checks if Mystic is about to Overcast
   if (combatant.CurrentAp < ability.Cost && combatant.Archetype == Mystic)
   {
       var apShortfall = ability.Cost - combatant.CurrentAp;
       var hpCost = apShortfall * 2;

       DisplayWarning($"Casting will cost {hpCost} HP! Continue?");
   }
   ```

**Data Flow**:
- UI queries resource values for display
- UI warns players about Overcast costs before execution
- UI displays regen messages at turn end

---

### Integration 5: Character/Enemy Entities (Source Synchronization)

**Relationship**: `ResourceService` ↔ `Character`/`Enemy` entities

**Integration Points**:
1. **Combatant Creation**:
   ```csharp
   // CombatService.cs (hypothetical)
   public Combatant CreateCombatant(Character character)
   {
       return new Combatant
       {
           CharacterSource = character, // Link to source entity
           CurrentHp = character.CurrentHp,
           MaxHp = character.MaxHp,
           CurrentStamina = character.CurrentStamina,
           MaxStamina = character.MaxStamina,
           CurrentAp = character.CurrentAp,
           MaxAp = character.MaxAp
       };
   }
   ```

2. **Synchronization After Resource Change**:
   ```csharp
   // ResourceService.cs:199-223
   private void SyncToSource(Combatant combatant, ResourceType resourceType)
   {
       switch (combatant.CharacterSource)
       {
           case Character character:
               character.CurrentHp = combatant.CurrentHp;
               character.CurrentStamina = combatant.CurrentStamina;
               character.CurrentAp = combatant.CurrentAp;
               break;

           case Enemy enemy:
               enemy.CurrentHp = combatant.CurrentHp;
               enemy.CurrentStamina = combatant.CurrentStamina;
               enemy.CurrentAp = combatant.CurrentAp;
               break;
       }
   }
   ```

**Data Flow**:
- `Combatant` is a combat-specific DTO (temporary state)
- `Character`/`Enemy` entities are persistent (database-backed)
- Every resource change syncs from `Combatant` → source entity
- Post-combat, source entities persist changes

---

## Data Models

### Core Entity: Combatant

**Purpose**: Combat-specific wrapper around `Character` or `Enemy` entities with resource tracking.

**Properties**:
```csharp
public class Combatant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ArchetypeType Archetype { get; set; }

    // Resource Properties
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentAp { get; set; } // Aether Points (Mystics only)
    public int MaxAp { get; set; }

    // Source Linkage
    public object CharacterSource { get; set; } // Character or Enemy entity

    // Attribute Access
    public int GetAttribute(Attribute attribute) { /* ... */ }
}
```

**Usage Context**:
- Created by `CombatService` at combat start
- Modified by `ResourceService` during combat
- Synced to `CharacterSource` after every resource change
- Disposed at combat end (source entities persist)

---

### Enum: ResourceType

**Purpose**: Identify which resource pool an ability costs.

**Definition**:
```csharp
public enum ResourceType
{
    Health,   // HP - universal resource
    Stamina,  // Physical abilities (melee, defense)
    Aether    // Mystic abilities (spells, rituals)
}
```

**Usage**:
- `Ability.CostType` - defines which resource is consumed
- `ResourceService.CanAfford/Deduct` - switch on resource type
- `ResourceService.GetCurrent/GetMax` - read resource values

---

### Enum: ArchetypeType

**Purpose**: Combatant class/role (determines Aether access).

**Definition** (relevant subset):
```csharp
public enum ArchetypeType
{
    Warrior,   // Cannot use Aether
    Ranger,    // Cannot use Aether
    Mystic,    // CAN use Aether + Overcast
    // ... other archetypes
}
```

**Usage**:
- `Combatant.Archetype` - stored on combatant
- `ResourceService.IsMystic()` - determines Aether eligibility
- `CanAffordAether` - archetype gate check

---

### Enum: StatusEffectType

**Purpose**: Active status effects that modify resource behavior.

**Definition** (relevant subset):
```csharp
public enum StatusEffectType
{
    Stunned,   // Blocks stamina regeneration
    // ... other effects
}
```

**Usage**:
- `StatusEffectService.GetActiveEffects()` - returns active effects
- `ResourceService.HasStatusEffect()` - checks for specific effect
- `RegenerateStamina` - Stunned check blocks regen

---

### Entity: Character (Source Entity)

**Purpose**: Persistent player character with resource state.

**Properties** (resource-relevant subset):
```csharp
public class Character
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ArchetypeType Archetype { get; set; }

    // Persistent Resources
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentAp { get; set; }
    public int MaxAp { get; set; }

    // Attributes
    public Dictionary<Attribute, int> Attributes { get; set; }
}
```

**Synchronization**:
- Updated by `ResourceService.SyncToSource()` after resource changes
- Persisted to database post-combat

---

### Entity: Enemy (Source Entity)

**Purpose**: Persistent enemy with resource state.

**Properties** (resource-relevant subset):
```csharp
public class Enemy
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ArchetypeType Archetype { get; set; }

    // Persistent Resources
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentAp { get; set; }
    public int MaxAp { get; set; }

    // Attributes
    public Dictionary<Attribute, int> Attributes { get; set; }
}
```

**Synchronization**:
- Updated by `ResourceService.SyncToSource()` after resource changes
- Enemies typically destroyed post-combat (no persistence)

---

## Configuration

### Constants (ResourceService.cs:18-25)

**Purpose**: Hardcoded configuration values for resource mechanics.

**Definitions**:
```csharp
/// <summary>
/// Base stamina regeneration per turn (before Finesse bonus).
/// </summary>
private const int BaseStaminaRegen = 5;

/// <summary>
/// HP cost per 1 missing AP when Mystics Overcast.
/// Formula: hpCost = apShortfall × OvercastHpRatio
/// </summary>
private const int OvercastHpRatio = 2;
```

**Usage**:
- **BaseStaminaRegen**: Applied in `RegenerateStamina` formula (`5 + Finesse/2`)
- **OvercastHpRatio**: Applied in `CanAffordAether` and `DeductAether` Overcast calculations

**Future Configuration**:
- These constants could be externalized to `appsettings.json` for balance tuning
- Example: `"Combat": { "BaseStaminaRegen": 5, "OvercastHpRatio": 2 }`

---

## Testing

### Test File: ResourceServiceTests.cs

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Tests/Engine/ResourceServiceTests.cs`
**Lines**: 619 lines
**Test Count**: 30 tests

---

### Test Category 1: CanAfford Tests (10 tests, lines 29-186)

**Purpose**: Validate resource affordability checks across all resource types and edge cases.

**Tests**:
1. **CanAfford_Health_WithSufficientHp_ReturnsTrue** (lines 29-41)
   - Scenario: Combatant has 100 HP, ability costs 20 HP
   - Expected: `CanAfford` returns `true`

2. **CanAfford_Health_WithInsufficientHp_ReturnsFalse** (lines 43-55)
   - Scenario: Combatant has 10 HP, ability costs 20 HP
   - Expected: `CanAfford` returns `false`

3. **CanAfford_Health_ExactMatch_ReturnsTrue** (behavior from code)
   - Scenario: Combatant has 20 HP, ability costs 20 HP (exact match)
   - Expected: `CanAfford` returns `true` (requires `CurrentHp >= cost`)

4. **CanAfford_Stamina_WithSufficientStamina_ReturnsTrue** (lines 71-83)
   - Scenario: Combatant has 50 Stamina, ability costs 10 Stamina
   - Expected: `CanAfford` returns `true`

5. **CanAfford_Stamina_WithInsufficientStamina_ReturnsFalse** (lines 85-97)
   - Scenario: Combatant has 5 Stamina, ability costs 10 Stamina
   - Expected: `CanAfford` returns `false`

6. **CanAfford_Aether_NonMystic_ReturnsFalse** (lines 99-111)
   - Scenario: Warrior combatant attempts Aether ability
   - Expected: `CanAfford` returns `false` (archetype restriction)

7. **CanAfford_Aether_Mystic_SufficientAp_ReturnsTrue** (lines 113-125)
   - Scenario: Mystic has 30 AP, ability costs 10 AP
   - Expected: `CanAfford` returns `true` (direct afford, no Overcast)

8. **CanAfford_Aether_Mystic_InsufficientAp_CanOvercast_ReturnsTrue** (lines 127-141)
   - Scenario: Mystic has 10 AP, 100 HP, ability costs 20 AP (shortfall = 10, HP cost = 20)
   - Expected: `CanAfford` returns `true` (Overcast affordable)

9. **CanAfford_Aether_Mystic_InsufficientAp_CannotOvercast_ReturnsFalse** (lines 143-157)
   - Scenario: Mystic has 0 AP, 10 HP, ability costs 20 AP (shortfall = 20, HP cost = 40)
   - Expected: `CanAfford` returns `false` (Overcast would kill)

10. **CanAfford_ZeroCost_AlwaysReturnsTrue** (lines 159-186)
    - Scenarios: Health, Stamina, Aether with cost = 0
    - Expected: All return `true` (free abilities always affordable)

**Code References**:
- `CanAffordHealth`: [ResourceService.cs:237-240](ResourceService.cs:237-240)
- `CanAffordStamina`: [ResourceService.cs:252-255](ResourceService.cs:252-255)
- `CanAffordAether`: [ResourceService.cs:237-250](ResourceService.cs:237-250)

---

### Test Category 2: Deduct Tests (9 tests, lines 189-339)

**Purpose**: Validate resource deductions, including Overcast mechanics.

**Tests**:
1. **Deduct_Health_Success** (lines 189-206)
   - Scenario: Deduct 20 HP from combatant with 100 HP
   - Expected: `CurrentHp` = 80, synced to source

2. **Deduct_Stamina_Success** (lines 208-225)
   - Scenario: Deduct 10 Stamina from combatant with 50 Stamina
   - Expected: `CurrentStamina` = 40, synced to source

3. **Deduct_Aether_DirectDeduction_Success** (lines 227-244)
   - Scenario: Mystic deducts 10 AP from 30 AP (no Overcast)
   - Expected: `CurrentAp` = 20, synced to source

4. **Deduct_Aether_Overcast_Success** (lines 246-268)
   - Scenario: Mystic has 10 AP, 100 HP, costs 20 AP (Overcast shortfall = 10, HP cost = 20)
   - Expected: `CurrentAp` = 0, `CurrentHp` = 80, Overcast log generated

5. **Deduct_Aether_FullOvercast_Success** (lines 270-292)
   - Scenario: Mystic has 0 AP, 100 HP, costs 15 AP (full Overcast = 30 HP)
   - Expected: `CurrentAp` = 0, `CurrentHp` = 70

6. **Deduct_InsufficientResources_ReturnsFalse** (lines 294-307)
   - Scenario: Combatant has 5 HP, attempts to deduct 20 HP
   - Expected: `Deduct` returns `false`, no resource changes

7. **Deduct_NonMystic_Aether_ReturnsFalse** (lines 309-322)
   - Scenario: Warrior attempts Aether deduction
   - Expected: `Deduct` returns `false`, no resource changes

8. **Deduct_ZeroCost_NoChanges** (lines 324-339)
   - Scenario: Deduct 0 cost from all resource types
   - Expected: All return `true`, no resource changes

9. **Deduct_SyncsToCharacterSource** (lines 246-268, implicit validation)
   - Scenario: All successful deductions
   - Expected: `CharacterSource` HP/Stamina/AP updated

**Code References**:
- `DeductHealth`: [ResourceService.cs:265-271](ResourceService.cs:265-271)
- `DeductStamina`: [ResourceService.cs:257-263](ResourceService.cs:257-263)
- `DeductAether`: [ResourceService.cs:172-197](ResourceService.cs:172-197)

---

### Test Category 3: RegenerateStamina Tests (6 tests, lines 342-453)

**Purpose**: Validate stamina regeneration formula and status effect blocking.

**Tests**:
1. **RegenerateStamina_Success** (lines 342-361)
   - Scenario: Combatant with Finesse 6, CurrentStamina 20/50
   - Expected: Regen = 8 (5 + 6/2), CurrentStamina = 28

2. **RegenerateStamina_ZeroFinesse_BaseRegenOnly** (lines 363-382)
   - Scenario: Combatant with Finesse 0
   - Expected: Regen = 5 (base only), CurrentStamina increases by 5

3. **RegenerateStamina_HighFinesse_BonusRegen** (lines 384-403)
   - Scenario: Combatant with Finesse 10
   - Expected: Regen = 10 (5 + 10/2), CurrentStamina increases by 10

4. **RegenerateStamina_AlreadyAtMax_ReturnsZero** (lines 405-421)
   - Scenario: Combatant with CurrentStamina = MaxStamina
   - Expected: Regen = 0, no change

5. **RegenerateStamina_StunnedStatus_ReturnsZero** (lines 423-443)
   - Scenario: Combatant with Stunned status effect
   - Expected: Regen = 0, no change, log message "STUNNED"

6. **RegenerateStamina_ClampsToMax** (lines 445-453)
   - Scenario: Combatant at 48/50 Stamina, regen would be 8
   - Expected: Regen = 2 (clamped to max), CurrentStamina = 50

**Code References**:
- `RegenerateStamina`: [ResourceService.cs:108-141](ResourceService.cs:108-141)
- `HasStatusEffect`: [ResourceService.cs:225-235](ResourceService.cs:225-235)

---

### Test Category 4: GetCurrent/GetMax Tests (2 tests, lines 456-487)

**Purpose**: Validate resource getters for UI display.

**Tests**:
1. **GetCurrent_ReturnsCorrectValues** (lines 456-471)
   - Scenario: Query current HP, Stamina, AP
   - Expected: Returns exact current values

2. **GetMax_ReturnsCorrectValues** (lines 473-487)
   - Scenario: Query max HP, Stamina, AP
   - Expected: Returns exact max values

**Code References**:
- `GetCurrent`: [ResourceService.cs:143-153](ResourceService.cs:143-153)
- `GetMax`: [ResourceService.cs:155-165](ResourceService.cs:155-165)

---

### Test Category 5: IsMystic Tests (3 tests, lines 490-538)

**Purpose**: Validate archetype eligibility for Aether abilities.

**Tests**:
1. **IsMystic_MysticArchetype_ReturnsTrue** (lines 490-503)
   - Scenario: Combatant with Archetype = Mystic
   - Expected: `IsMystic` returns `true`

2. **IsMystic_WarriorArchetype_ReturnsFalse** (lines 505-518)
   - Scenario: Combatant with Archetype = Warrior
   - Expected: `IsMystic` returns `false`

3. **IsMystic_RangerArchetype_ReturnsFalse** (lines 520-538)
   - Scenario: Combatant with Archetype = Ranger
   - Expected: `IsMystic` returns `false`

**Code References**:
- `IsMystic`: [ResourceService.cs:167-170](ResourceService.cs:167-170)

---

### Test Category 6: Edge Cases (3 tests, lines 541-596)

**Purpose**: Validate boundary conditions and error handling.

**Tests**:
1. **Deduct_ExactHpCost_WouldDie_ReturnsFalse** (lines 541-554)
   - Scenario: Combatant has 20 HP, ability costs 20 HP
   - Expected: `CanAfford` returns `false`, `Deduct` returns `false`

2. **Deduct_Overcast_ExactHpMatch_ReturnsFalse** (lines 556-571)
   - Scenario: Mystic has 0 AP, 20 HP, costs 10 AP (Overcast = 20 HP exact match)
   - Expected: `CanAfford` returns `false` (survival check fails)

3. **SyncToSource_NullSource_LogsWarning** (lines 573-596)
   - Scenario: Combatant with `CharacterSource = null`
   - Expected: Deduction succeeds, log warning about null source

**Code References**:
- `CanAffordHealth`: [ResourceService.cs:237-240](ResourceService.cs:237-240)
- `CanAffordAether` (survival check): [ResourceService.cs:248](ResourceService.cs:248)
- `SyncToSource` (null handling): [ResourceService.cs:215-221](ResourceService.cs:215-221)

---

### Test Helpers

**Helper Methods** (lines 598-619):
```csharp
private Combatant CreatePlayerCombatant(ArchetypeType archetype)
{
    var character = new Character
    {
        Id = Guid.NewGuid(),
        Name = "Test Character",
        Archetype = archetype,
        CurrentHp = 100,
        MaxHp = 100,
        CurrentStamina = 50,
        MaxStamina = 50,
        CurrentAp = 30,
        MaxAp = 30,
        Attributes = new Dictionary<Attribute, int>
        {
            { Attribute.Finesse, 6 }
        }
    };

    return new Combatant
    {
        CharacterSource = character,
        CurrentHp = character.CurrentHp,
        MaxHp = character.MaxHp,
        CurrentStamina = character.CurrentStamina,
        MaxStamina = character.MaxStamina,
        CurrentAp = character.CurrentAp,
        MaxAp = character.MaxAp,
        Archetype = archetype
    };
}
```

**Purpose**: Standardized combatant creation for test consistency.

---

## Domain 4 Compliance

### Assessment: N/A (Mechanical System)

**ResourceService** is a purely mechanical combat system with no narrative content. Domain 4 constraints apply to **descriptive text** (item descriptions, bestiary entries, dialogue), not game mechanics.

### Justification

1. **No Player-Facing Text**: ResourceService does not generate any descriptive text for the player.
2. **Logging Only**: All output is structured log messages for developer traceability (e.g., `"Anya OVERCASTS! Spent 10 AP + 20 HP"`).
3. **Numeric Data**: All values are precise game state (HP, Stamina, AP) with no narrative interpretation.

### Related Compliant Systems

If **UI messages** display resource changes to the player, those messages MUST comply with Domain 4:

❌ **Forbidden**:
- "You regenerated exactly 8 stamina."
- "Overcasting costs precisely 20 HP."
- "Your stamina is at 45.2% capacity."

✅ **Compliant**:
- "You feel your vigor returning." (regen message)
- "The spell drains your life force!" (Overcast message)
- "Your stamina wanes." (low stamina warning)

**Recommendation**: Create a `ResourceMessageService` to translate mechanical events into Domain 4-compliant narrative messages.

---

## Future Extensions

### Enhancement 1: Variable Overcast Ratio (Talents/Items)

**Current Limitation**: Overcast ratio is hardcoded to 2:1 (2 HP per 1 AP).

**Proposed Enhancement**:
- Add `OvercastRatio` property to `Combatant` or `Character`
- Default: 2.0 (current behavior)
- Talent: "Efficient Overcast" → Ratio = 1.5 (25% cheaper)
- Item: "Aetheric Conduit" → Ratio = 1.0 (1:1 conversion)

**Implementation**:
```csharp
// Replace hardcoded OvercastHpRatio with dynamic property
private int CalculateOvercastCost(Combatant combatant, int apShortfall)
{
    var ratio = combatant.OvercastRatio ?? OvercastHpRatio; // Default to 2.0
    return (int)(apShortfall * ratio);
}
```

**Impact**:
- Introduces build diversity (Mystic specialization)
- Requires database schema update (`Character.OvercastRatio`)

---

### Enhancement 2: Aether Regeneration

**Current Limitation**: Aether does not regenerate during combat (only via rest).

**Proposed Enhancement**:
- Add `RegenerateAether(Combatant combatant)` method
- Formula: `regenAmount = BasAetherRegen + (Will / 2)` (parallels Stamina formula)
- Triggered at turn end for Mystics only

**Implementation**:
```csharp
public int RegenerateAether(Combatant combatant)
{
    if (!IsMystic(combatant))
    {
        return 0; // Only Mystics regenerate AP
    }

    var will = combatant.GetAttribute(Attribute.Will);
    var regenAmount = BaseAetherRegen + (will / 2); // e.g., 3 + (Will / 2)

    var actualRegen = Math.Min(regenAmount, combatant.MaxAp - combatant.CurrentAp);

    if (actualRegen <= 0)
    {
        return 0;
    }

    combatant.CurrentAp += actualRegen;
    SyncToSource(combatant, ResourceType.Aether);

    return actualRegen;
}
```

**Balance Considerations**:
- Low regen rate (e.g., 3 base) prevents infinite spellcasting
- Encourages strategic AP spending vs. spamming Overcast

---

### Enhancement 3: HP Regeneration (Lifesteal/Vampiric Traits)

**Current Limitation**: HP does not regenerate during combat (healing abilities/items only).

**Proposed Enhancement**:
- Add `RegenerateHealth(Combatant combatant, int amount)` method
- Triggered by traits (e.g., "Vampiric Strike" heals for 20% damage dealt)
- Item effects (e.g., "Regeneration Potion" grants 5 HP/turn for 3 turns)

**Implementation**:
```csharp
public int RegenerateHealth(Combatant combatant, int amount)
{
    var actualRegen = Math.Min(amount, combatant.MaxHp - combatant.CurrentHp);

    if (actualRegen <= 0)
    {
        return 0;
    }

    combatant.CurrentHp += actualRegen;
    SyncToSource(combatant, ResourceType.Health);

    _logger.LogInformation("{Name} regenerates {Amount} HP ({Current}/{Max})",
        combatant.Name, actualRegen, combatant.CurrentHp, combatant.MaxHp);

    return actualRegen;
}
```

**Use Cases**:
- Lifesteal abilities (heal for damage dealt)
- Regeneration buffs (5 HP/turn for 3 turns)
- Boss mechanics (heals 10% HP when minion dies)

---

### Enhancement 4: Resource Caps and Overflow

**Current Limitation**: No hard caps on resource max values (could theoretically reach int.MaxValue).

**Proposed Enhancement**:
- Add global resource caps (e.g., `MaxHpCap = 9999`)
- Prevent buff stacking from exceeding caps
- Overflow protection (prevent negative resources)

**Implementation**:
```csharp
private const int MaxHpCap = 9999;
private const int MaxStaminaCap = 500;
private const int MaxApCap = 500;

private int ClampToResourceCap(ResourceType resourceType, int value)
{
    return resourceType switch
    {
        ResourceType.Health => Math.Min(value, MaxHpCap),
        ResourceType.Stamina => Math.Min(value, MaxStaminaCap),
        ResourceType.Aether => Math.Min(value, MaxApCap),
        _ => value
    };
}
```

---

### Enhancement 5: Status Effect Modifiers for Regeneration

**Current Implementation**: Only Stunned blocks stamina regen.

**Proposed Enhancement**:
- **Energized**: +50% stamina regen
- **Exhausted**: -50% stamina regen
- **Arcane Focus**: +100% Aether regen (if Aether regen is implemented)

**Implementation**:
```csharp
public int RegenerateStamina(Combatant combatant)
{
    // Stunned check (existing)
    if (HasStatusEffect(combatant, StatusEffectType.Stunned))
    {
        return 0;
    }

    var finesse = combatant.GetAttribute(Attribute.Finesse);
    var regenAmount = BaseStaminaRegen + (finesse / 2);

    // NEW: Status effect modifiers
    if (HasStatusEffect(combatant, StatusEffectType.Energized))
    {
        regenAmount = (int)(regenAmount * 1.5); // +50%
    }

    if (HasStatusEffect(combatant, StatusEffectType.Exhausted))
    {
        regenAmount = (int)(regenAmount * 0.5); // -50%
    }

    var actualRegen = Math.Min(regenAmount, combatant.MaxStamina - combatant.CurrentStamina);

    // ... rest of method
}
```

---

### Enhancement 6: Resource Pools (Party-Shared Resources)

**Current Limitation**: Each combatant has independent resource pools.

**Proposed Enhancement**:
- "Linked HP" mechanic (damage distributed across party)
- "Shared Stamina Pool" (party draws from common pool)
- Boss mechanic: "Feeds on HP" (boss gains HP when player takes damage)

**Implementation** (example: Linked HP):
```csharp
public bool DeductLinkedHealth(List<Combatant> linkedGroup, int totalDamage)
{
    var totalHp = linkedGroup.Sum(c => c.CurrentHp);

    if (totalHp <= totalDamage)
    {
        return false; // Entire group would die
    }

    // Distribute damage evenly
    var damagePerMember = totalDamage / linkedGroup.Count;

    foreach (var combatant in linkedGroup)
    {
        DeductHealth(combatant, damagePerMember);
    }

    return true;
}
```

**Use Cases**:
- "Blood Pact" ability (link HP pools for damage sharing)
- Boss mechanic (party must coordinate resource usage)

---

## Error Handling

### Error Pattern 1: Unknown Resource Type

**Scenario**: Invalid `ResourceType` enum value passed to methods.

**Handling**:
```csharp
// ResourceService.cs:68-71
default:
    _logger.LogError("[Resource] Unknown ResourceType: {Type}", resourceType);
    return false;
```

**Recovery**: Returns `false` (safe failure), logs error for debugging.

---

### Error Pattern 2: Null CharacterSource

**Scenario**: Combatant's `CharacterSource` is null (should never happen in production).

**Handling**:
```csharp
// ResourceService.cs:215-221
default:
    _logger.LogWarning(
        "[Resource] Cannot sync {Name} - CharacterSource is null or unknown type",
        combatant.Name);
    break;
```

**Recovery**: Log warning, skip synchronization (graceful degradation for testing).

---

### Error Pattern 3: Validation Failure in Deduct

**Scenario**: `Deduct` called without prior `CanAfford` check (developer error).

**Handling**:
```csharp
// ResourceService.cs:82-88
if (!CanAfford(combatant, resourceType, cost))
{
    _logger.LogWarning(
        "[Resource] {Name} CANNOT AFFORD {Cost} {Type}",
        combatant.Name, cost, resourceType);
    return false;
}
```

**Recovery**: Returns `false`, logs warning. No resource changes applied.

---

### Error Pattern 4: Null StatusEffectService

**Scenario**: `IStatusEffectService` is null (dependency injection failure or testing).

**Handling**:
```csharp
// ResourceService.cs:227-230
if (_statusEffectService == null)
{
    return false; // Assume no status effects
}
```

**Recovery**: Assumes no status effects present (graceful degradation for testing).

---

## Changelog

### v1.1.0 (2025-12-23)

- Added YAML frontmatter with id, title, version, status, last_updated, related_specs
- Fixed CanAffordHealth documentation: changed `>` to `>=` to match code behavior
- Fixed decision tree diagram to show `CurrentHp >= cost` for Health affordability
- Fixed test scenario description for exact HP match case
- Added code traceability references to ResourceService.cs, IResourceService.cs, ResourceType.cs

### Version 1.0.0 (2025-12-22) - Initial Implementation

**Implemented Features**:
- ✅ `CanAfford` resource validation for Health, Stamina, Aether
- ✅ `Deduct` resource deduction with source synchronization
- ✅ `RegenerateStamina` with Finesse-based scaling
- ✅ Mystic Overcast mechanic (2 HP per 1 missing AP)
- ✅ Archetype-based Aether restriction (Mystics only)
- ✅ Status effect integration (Stunned blocks regen)
- ✅ Source synchronization (`Character`/`Enemy` entity updates)
- ✅ Comprehensive logging (Debug/Info/Warning levels)

**Test Coverage**:
- ✅ 30 tests across 6 categories
- ✅ 619 lines of test code
- ✅ 100% method coverage

**Known Limitations**:
- No HP or Aether regeneration
- Fixed Overcast ratio (2:1)
- No resource caps enforcement

**Future Work**:
- Variable Overcast ratio (talents/items)
- Aether regeneration mechanic
- HP regeneration (lifesteal/vampiric)
- Status effect modifiers for regen
- Resource pools (party-shared mechanics)

---

**End of SPEC-RESOURCE-001**
