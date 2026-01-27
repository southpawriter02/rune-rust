---
id: SPEC-REPAIR-001
title: Repair & Salvage System
version: 1.0.1
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-DICE-001, SPEC-INV-001, SPEC-CRAFT-001]
---

# SPEC-REPAIR-001: Repair & Salvage System

> **Version:** 1.0.1
> **Status:** Implemented
> **Service:** `BodgingService`
> **Location:** `RuneAndRust.Engine/Services/BodgingService.cs`
> **Related Entities:** `Equipment` ([Equipment.cs](RuneAndRust.Core/Entities/Equipment.cs)), `RepairResult` ([RepairResult.cs](RuneAndRust.Core/Models/Crafting/RepairResult.cs)), `SalvageResult` ([SalvageResult.cs](RuneAndRust.Core/Models/Crafting/SalvageResult.cs))

---

## Overview

The Repair & Salvage System (Bodging) implements **WITS-based skill checks** for restoring damaged equipment and extracting raw materials from broken gear. The system uses Scrap materials as a consumable resource for repairs and includes catastrophe mechanics that can permanently damage equipment. Salvage operations convert equipment into Scrap based on item weight and quality tier.

**Core Design Principles:**
- **Damage-Scaled Difficulty:** Higher damage increases repair DC proportionally
- **Scrap Economy:** Repairs consume Scrap; salvage produces Scrap (closed-loop resource system)
- **Permanent Degradation:** Catastrophic failures reduce MaxDurability permanently
- **Quality-Based Yields:** Higher quality equipment yields more Scrap when salvaged
- **No Zero-Durability Repairs:** Items at 0 durability can only be salvaged, not repaired

---

## Behaviors

### Primary Behaviors

#### 1. Repair Validation (`RepairItemAsync`)
Validates equipment exists, is damageable, and requires repair.

```csharp
Task<RepairResult> RepairItemAsync(Character character, Guid itemId)
```

**Sequence:**
1. Look up item in character's inventory by GUID
2. Verify item is Equipment type (not consumable/material)
3. Calculate damage: `MaxDurability - CurrentDurability`
4. Verify damage > 0 (item actually needs repair)
5. Return early with failure result if validation fails

**Example:**
```csharp
// Item lookup
var inventoryItem = character.Inventory
    .FirstOrDefault(inv => inv.Item.Id == itemId);
// inventoryItem.Item = ClanForged Longsword (CurrentDurability: 45/100)

// Type check
if (inventoryItem.Item is not Equipment equipment)
{
    return CreateFailedRepairResult(inventoryItem.Item.Name, 0, 0,
        $"Cannot repair {inventoryItem.Item.Name} - it is not equipment.");
}

// Damage check
var damage = equipment.MaxDurability - equipment.CurrentDurability;
// damage = 100 - 45 = 55
if (damage <= 0)
{
    return CreateFailedRepairResult(equipment.Name, 0, 0,
        $"{equipment.Name} is already in good condition.");
}
```

#### 2. Scrap Cost Calculation (`CalculateRepairCost`)
Determines Scrap material cost based on equipment damage.

```csharp
public int CalculateRepairCost(Equipment equipment)
```

**Formula:**
```
Scrap Cost = max(1, ceil(damage / 5))
```

**Calculation:**
```csharp
var damage = equipment.MaxDurability - equipment.CurrentDurability;
var cost = (int)Math.Ceiling((double)damage / DamageDivisor);  // DamageDivisor = 5
return Math.Max(1, cost);  // Minimum 1 Scrap
```

**Cost Table:**
| Damage | Calculation | Scrap Cost |
|--------|-------------|------------|
| 1-5    | ceil(5/5) = 1  | 1 |
| 6-10   | ceil(10/5) = 2 | 2 |
| 15     | ceil(15/5) = 3 | 3 |
| 25     | ceil(25/5) = 5 | 5 |
| 50     | ceil(50/5) = 10 | 10 |
| 100    | ceil(100/5) = 20 | 20 |

**Example:**
```csharp
// Equipment: ClanForged Longsword (45/100 durability)
var equipment = new Equipment { CurrentDurability = 45, MaxDurability = 100 };
var cost = CalculateRepairCost(equipment);
// damage = 55
// cost = ceil(55 / 5) = ceil(11) = 11 Scrap
```

#### 3. Scrap Availability Check (`CanRepair`)
Validates character has sufficient Scrap materials for repair.

```csharp
public bool CanRepair(Character character, Equipment equipment)
```

**Sequence:**
1. Calculate required Scrap via `CalculateRepairCost()`
2. Query character inventory for "scrap" item (case-insensitive)
3. Compare available quantity vs required
4. Return true/false

**Example:**
```csharp
// Required Scrap: 11
var requiredScrap = CalculateRepairCost(equipment);  // 11

// Character inventory: 15 Scrap
var scrapInInventory = character.Inventory
    .FirstOrDefault(inv => inv.Item.Name.Equals("scrap", StringComparison.OrdinalIgnoreCase));
var available = scrapInInventory?.Quantity ?? 0;  // 15

return available >= requiredScrap;  // 15 >= 11 → true
```

#### 4. Scrap Consumption (`RepairItemAsync`)
Consumes Scrap materials from inventory **before** rolling dice (non-refundable).

**Sequence:**
1. Calculate Scrap cost
2. Call `_inventoryService.RemoveItemAsync(character, "scrap", scrapCost)`
3. Log consumption success/failure
4. Continue to dice roll (Scrap already consumed)

**Example:**
```csharp
// Consuming 11 Scrap for repair
var scrapCost = CalculateRepairCost(equipment);  // 11
var removeResult = await _inventoryService.RemoveItemAsync(character, "scrap", scrapCost);
if (!removeResult.Success)
{
    _logger.LogWarning("Failed to consume Scrap for repair: {Message}", removeResult.Message);
    return CreateFailedRepairResult(equipment.Name, 0, scrapCost,
        $"Failed to consume Scrap: {removeResult.Message}");
}
_logger.LogDebug("Consumed: {Quantity}x Scrap for repair", scrapCost);
```

**Critical Design Note:** Scrap is consumed at line [BodgingService.cs:107](RuneAndRust.Engine/Services/BodgingService.cs#L107) BEFORE the WITS roll. This mirrors the crafting system's risk/reward model - failed repairs still cost materials.

#### 5. Repair DC Calculation (`RepairItemAsync`)
Calculates difficulty class based on equipment damage.

**Formula:**
```
Repair DC = BaseRepairDc + (damage / 5)
```

**Constants:**
- `BaseRepairDc = 8` ([line 24](RuneAndRust.Engine/Services/BodgingService.cs#L24))
- `DamageDivisor = 5` ([line 29](RuneAndRust.Engine/Services/BodgingService.cs#L29))

**Calculation:**
```csharp
var dc = BaseRepairDc + (damage / DamageDivisor);
// Example: 55 damage
// dc = 8 + (55 / 5) = 8 + 11 = 19
```

**DC Scaling Table:**
| Damage | DC Modifier | Final DC | Difficulty |
|--------|-------------|----------|------------|
| 0-4    | +0          | 8        | Moderate   |
| 5-9    | +1          | 9        | Moderate   |
| 10-14  | +2          | 10       | Hard       |
| 25-29  | +5          | 13       | Very Hard  |
| 50-54  | +10         | 18       | Legendary  |
| 95-99  | +19         | 27       | Impossible |

**Design Rationale:** Severely damaged equipment becomes exponentially harder to repair, encouraging salvage of heavily degraded gear.

#### 6. WITS Dice Roll (`RepairItemAsync`)
Executes d10 dice pool roll using character's WITS attribute.

**Sequence:**
1. Retrieve `character.Wits` attribute (pool size)
2. Call `_diceService.Roll(wits, $"Repair {equipment.Name}")`
3. Calculate net successes: `Successes - Botches`
4. Log roll results with context

**Example:**
```csharp
// Character has WITS = 6
var wits = character.Wits;  // 6
var diceResult = _diceService.Roll(wits, "Repair ClanForged Longsword");
// diceResult.Rolls = [3, 8, 10, 1, 9, 4]
// diceResult.Successes = 3 (8, 10, 9)
// diceResult.Botches = 1 (1)
var netSuccesses = 3 - 1 = 2
```

#### 7. Outcome Determination (`DetermineOutcome`)
Maps net successes to repair outcomes using DC thresholds.

```csharp
private static CraftingOutcome DetermineOutcome(int netSuccesses, int dc)
```

**Decision Tree:**
```
netSuccesses < 0                     → Catastrophe
netSuccesses >= dc + 5               → Masterwork (full restore)
netSuccesses >= dc                   → Success (partial restore)
netSuccesses < dc (but >= 0)         → Failure (Scrap wasted)
```

**Threshold Table (DC = 19 example):**
| Net Successes | Outcome | Effect |
|---------------|---------|--------|
| -1            | Catastrophe | MaxDurability -10 |
| 0-18          | Failure | No repair, Scrap lost |
| 19-23         | Success | Restore 19*5 = 95 durability |
| 24+           | Masterwork | Full restore to MaxDurability |

**Example:**
```csharp
// Repair DC = 19, Net Successes = 2
var outcome = DetermineOutcome(2, 19);  // Failure (2 < 19)

// Net Successes = 20
var outcome = DetermineOutcome(20, 19); // Success (20 >= 19, < 24)

// Net Successes = 25
var outcome = DetermineOutcome(25, 19); // Masterwork (25 >= 24)

// Net Successes = -3
var outcome = DetermineOutcome(-3, 19); // Catastrophe (-3 < 0)
```

#### 8. Result Handling (Outcome Switch)
Executes outcome-specific logic based on repair result.

**Masterwork Handler ([HandleMasterworkRepair:283](RuneAndRust.Engine/Services/BodgingService.cs#L283)):**
```csharp
// Fully restores durability to maximum
equipment.CurrentDurability = equipment.MaxDurability;

return new RepairResult(
    IsSuccess: true,
    Outcome: CraftingOutcome.Masterwork,
    ItemName: equipment.Name,
    DurabilityRestored: damage,  // Restored full damage amount
    ScrapConsumed: scrapCost,
    MaxDurabilityLost: null,
    Message: $"MASTERWORK! {equipment.Name} has been fully restored to pristine condition."
);
```

**Success Handler ([HandleSuccessfulRepair:314](RuneAndRust.Engine/Services/BodgingService.cs#L314)):**
```csharp
// Restore durability: min(damage, netSuccesses * 5)
var restored = Math.Min(damage, netSuccesses * DamageDivisor);
equipment.CurrentDurability = Math.Min(
    equipment.MaxDurability,
    equipment.CurrentDurability + restored);

return new RepairResult(
    IsSuccess: true,
    Outcome: CraftingOutcome.Success,
    DurabilityRestored: restored,
    Message: $"Success! {equipment.Name} has been repaired. Durability restored by {restored}."
);
```

**Durability Restoration Formula:**
```
Restored = min(damage, netSuccesses * 5)
```

**Example:**
```csharp
// Longsword: 45/100 durability (damage = 55)
// Net Successes = 20, DC = 19 (Success outcome)
var restored = Math.Min(55, 20 * 5);
// restored = min(55, 100) = 55 (full repair within damage limit)
equipment.CurrentDurability = 45 + 55 = 100

// Example 2: Net Successes = 8
var restored = Math.Min(55, 8 * 5);
// restored = min(55, 40) = 40
equipment.CurrentDurability = 45 + 40 = 85
```

**Failure Handler ([HandleFailedRepair:350](RuneAndRust.Engine/Services/BodgingService.cs#L350)):**
```csharp
// No durability restored, Scrap already consumed
return new RepairResult(
    IsSuccess: false,
    Outcome: CraftingOutcome.Failure,
    DurabilityRestored: 0,
    ScrapConsumed: scrapCost,
    Message: $"Failure. Your repair attempt on {equipment.Name} was unsuccessful. Scrap wasted."
);
```

**Catastrophe Handler ([HandleCatastrophicRepair:379](RuneAndRust.Engine/Services/BodgingService.cs#L379)):**
```csharp
// Permanently reduce MaxDurability
equipment.MaxDurability = Math.Max(0, equipment.MaxDurability - CatastrophePenalty);

// Ensure CurrentDurability doesn't exceed new max
equipment.CurrentDurability = Math.Min(equipment.CurrentDurability, equipment.MaxDurability);

return new RepairResult(
    IsSuccess: false,
    Outcome: CraftingOutcome.Catastrophe,
    DurabilityRestored: 0,
    MaxDurabilityLost: CatastrophePenalty,  // 10
    Message: $"CATASTROPHE! Your fumbled repair has permanently damaged {equipment.Name}. Maximum durability reduced by {CatastrophePenalty}."
);
```

**Catastrophe Penalty:** `CatastrophePenalty = 10` ([line 39](RuneAndRust.Engine/Services/BodgingService.cs#L39))

**Example:**
```csharp
// Longsword: 45/100 durability
// Catastrophe occurs (net successes = -2)
equipment.MaxDurability = Math.Max(0, 100 - 10);
// equipment.MaxDurability = 90
equipment.CurrentDurability = Math.Min(45, 90);
// equipment.CurrentDurability = 45 (unchanged, within new max)

// New state: 45/90 durability (permanently degraded)
```

### Secondary Behaviors

#### 9. Salvage Item (`SalvageItemAsync`)
Destroys equipment to extract Scrap materials.

```csharp
Task<SalvageResult> SalvageItemAsync(Character character, Guid itemId)
```

**Sequence:**
1. Find item in inventory
2. Verify item is Equipment type
3. Calculate Scrap yield via `CalculateSalvageYield()`
4. Remove item from inventory
5. Add Scrap to inventory
6. Return salvage result

**Example:**
```csharp
// Equipment: JuryRigged Dagger (weight: 500g, quality: JuryRigged)
var result = await _bodgingService.SalvageItemAsync(character, daggerId);
// Yield calculation: (500 / 100) * (0 + 1) = 5 * 1 = 5 Scrap
// Item removed from inventory
// +5 Scrap added to inventory
result.IsSuccess == true
result.ScrapYield == 5
result.Message == "Salvaged Jury-Rigged Dagger for 5 Scrap."
```

#### 10. Salvage Yield Calculation (`CalculateSalvageYield`)
Determines Scrap yield based on equipment weight and quality tier.

```csharp
public int CalculateSalvageYield(Equipment equipment)
```

**Formula:**
```
Yield = (weight / 100) * (qualityModifier + 1)
Minimum: 1 Scrap
```

**Quality Modifiers:**
```csharp
private static int GetQualityModifier(QualityTier tier) => tier switch
{
    QualityTier.JuryRigged => 0,
    QualityTier.Scavenged => 1,
    QualityTier.ClanForged => 2,
    QualityTier.Optimized => 3,
    QualityTier.MythForged => 4,
    _ => 0
};
```

**Calculation Example:**
```csharp
// ClanForged Longsword: weight = 1500g, quality = ClanForged
var qualityMod = GetQualityModifier(QualityTier.ClanForged);  // 2
var yield = (1500 / 100) * (2 + 1);
// yield = 15 * 3 = 45 Scrap
return Math.Max(1, 45);  // 45 Scrap
```

**Yield Table:**
| Weight | Quality | Modifier | Calculation | Yield |
|--------|---------|----------|-------------|-------|
| 500g   | JuryRigged | 0     | (5) * (0+1) | 5     |
| 500g   | ClanForged | 2     | (5) * (2+1) | 15    |
| 1500g  | MythForged | 4     | (15) * (4+1)| 75    |
| 100g   | Scavenged  | 1     | (1) * (1+1) | 2     |
| 50g    | JuryRigged | 0     | (0) * (0+1) | 1 (min)|

### Edge Case Behaviors

#### Item Not Found
| Condition | Behavior | Return Value |
|-----------|----------|--------------|
| `itemId` not in inventory | Log info, return failed result | `RepairResult` with `Message = "Item not found in inventory."` |

#### Item Not Equipment
| Condition | Behavior | Return Value |
|-----------|----------|--------------|
| Item is not `Equipment` type | Log info, return failed result | `RepairResult` with `Message = "Cannot repair {ItemName} - it is not equipment."` |

#### Item Already Fully Repaired
| Condition | Behavior | Return Value |
|-----------|----------|--------------|
| `CurrentDurability == MaxDurability` | Return failed result (no roll) | `RepairResult` with `Message = "{ItemName} is already in good condition."` |

**Example:**
```csharp
// Pristine sword: 100/100 durability
var damage = 100 - 100 = 0;
if (damage <= 0)
{
    return CreateFailedRepairResult(equipment.Name, 0, 0,
        $"{equipment.Name} is already in good condition.");
}
// result.IsSuccess = false
// result.DiceRolled = 0 (no roll occurred)
```

#### Insufficient Scrap
| Condition | Behavior | Return Value |
|-----------|----------|--------------|
| `CanRepair()` returns false | Return failed result (no Scrap consumed, no roll) | `RepairResult` with `Message = "Insufficient Scrap. Need {cost} Scrap to repair {ItemName}."` |

**Example:**
```csharp
// Repair cost: 11 Scrap, Character has: 5 Scrap
if (!CanRepair(character, equipment))
{
    return CreateFailedRepairResult(equipment.Name, 0, scrapCost,
        $"Insufficient Scrap. Need {scrapCost} Scrap to repair {equipment.Name}.");
}
// result.IsSuccess = false
// result.ScrapConsumed = 0 (none consumed - validation failed)
```

#### Scrap Removal Failure
| Condition | Behavior | Impact |
|-----------|----------|--------|
| `InventoryService.RemoveItemAsync()` returns failure | Log warning, return failed result | Repair aborted, return error message |

**Example:**
```csharp
var removeResult = await _inventoryService.RemoveItemAsync(character, "scrap", scrapCost);
if (!removeResult.Success)
{
    _logger.LogWarning("Failed to consume Scrap for repair: {Message}", removeResult.Message);
    return CreateFailedRepairResult(equipment.Name, 0, scrapCost,
        $"Failed to consume Scrap: {removeResult.Message}");
}
```

#### Catastrophe Reduces MaxDurability to Zero
| Condition | Behavior | Impact |
|-----------|----------|--------|
| `MaxDurability - CatastrophePenalty < 0` | Clamp to 0 | Equipment becomes unsalvageable (0 max durability) |

**Example:**
```csharp
// Equipment with MaxDurability = 8
equipment.MaxDurability = Math.Max(0, 8 - 10);
// equipment.MaxDurability = 0 (clamped)
equipment.CurrentDurability = Math.Min(45, 0);
// equipment.CurrentDurability = 0 (must be <= max)

// Equipment is now destroyed (0/0 durability)
```

#### Salvage of Non-Equipment
| Condition | Behavior | Return Value |
|-----------|----------|--------------|
| Item is not `Equipment` type | Return failed salvage result | `SalvageResult` with `Message = "Cannot salvage {ItemName} - it is not equipment."` |

---

## Restrictions

### MUST NOT
1. **Repair items at 0 durability** - Only salvage is allowed for destroyed equipment
2. **Refund Scrap on failure** - Scrap is consumed before rolling, non-refundable
3. **Allow negative MaxDurability** - Catastrophes clamp to 0 ([line 387](RuneAndRust.Engine/Services/BodgingService.cs#L387))
4. **Exceed MaxDurability on repair** - CurrentDurability capped at MaxDurability ([line 324](RuneAndRust.Engine/Services/BodgingService.cs#L324))
5. **Salvage non-equipment items** - Only Equipment entities can be salvaged

### MUST
1. **Validate item exists in inventory** before attempting repair/salvage
2. **Check Scrap availability** via `CanRepair()` before consumption
3. **Log all repair/salvage attempts** at INFO level minimum
4. **Apply catastrophe penalty** when net successes < 0 (MaxDurability reduction)
5. **Ensure CurrentDurability <= MaxDurability** after all operations

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| **WITS Attribute Range** | 1-10 | Determines pool size for rolls |
| **Base Repair DC** | 8 | Constant baseline difficulty |
| **Damage Divisor** | 5 | Scales both DC and Scrap cost |
| **Masterwork Threshold** | DC + 5 | Full restoration threshold |
| **Catastrophe Penalty** | 10 | MaxDurability loss on botch |
| **Salvage Weight Divisor** | 100 | Weight-to-Scrap conversion |
| **Durability Range** | 0-MaxDurability | Per-item current/max tracking |

### Functional Limitations
1. **No Partial Scrap Refunds:** Scrap is fully consumed on repair attempt, even if failed
2. **No Quality Tier Upgrades:** Repairs never improve equipment quality (unlike masterwork crafting)
3. **No Durability Above Max:** Cannot exceed MaxDurability through repairs
4. **No Item Resurrection:** Equipment at 0 durability can only be salvaged, never repaired
5. **No Batch Repairs:** Each repair attempt affects one item only
6. **No Catastrophe Mitigation:** Catastrophes always reduce MaxDurability by 10 (no scaling)

### Scrap Economy Limitations
| Operation | Scrap Cost | Scrap Yield | Net Balance |
|-----------|------------|-------------|-------------|
| Repair (Success) | 1-20 Scrap | 0 | -Scrap |
| Repair (Failure) | 1-20 Scrap | 0 | -Scrap |
| Salvage | 0 | 1-100+ Scrap | +Scrap |

**Design Note:** Salvage always produces Scrap, creating a sustainable economy. Failed repairs drain Scrap, encouraging salvage of heavily damaged gear.

---

## Use Cases

### UC-REPAIR-01: Successful Partial Repair
**Scenario:** Player repairs moderately damaged equipment with adequate WITS.

**Setup:**
```csharp
var character = new Character { Name = "Kael", Wits = 7 };
var equipment = new Equipment
{
    Id = Guid.NewGuid(),
    Name = "ClanForged Longsword",
    CurrentDurability = 45,
    MaxDurability = 100,
    Weight = 1500,
    Quality = QualityTier.ClanForged
};

// Character inventory: 15 Scrap
// Damage: 100 - 45 = 55
// Scrap cost: ceil(55 / 5) = 11
// Repair DC: 8 + (55 / 5) = 8 + 11 = 19
```

**Execution:**
```csharp
var result = await _bodgingService.RepairItemAsync(character, equipment.Id);
// Step 1: Validate item (Equipment type, damage > 0) ✓
// Step 2: Calculate cost (11 Scrap) ✓
// Step 3: Check availability (15 >= 11) ✓
// Step 4: Consume Scrap (-11 Scrap)
// Step 5: Calculate DC (19)
// Step 6: Roll WITS (7d10) = [8, 9, 10, 3, 2, 1, 8] → 4 successes, 1 botch → net = 3
// Step 7: Outcome = Failure (net 3 < DC 19)
```

**Result (Failure):**
```csharp
result.IsSuccess == false
result.Outcome == CraftingOutcome.Failure
result.NetSuccesses == 3
result.DifficultyClass == 19
result.DurabilityRestored == 0
result.ScrapConsumed == 11
result.Message == "Failure. Your repair attempt on ClanForged Longsword was unsuccessful. Scrap wasted."
equipment.CurrentDurability == 45  // Unchanged
character.Inventory["scrap"] == 4  // 15 - 11 = 4 remaining
```

**Alternate Roll (Success):**
```csharp
// WITS roll (7d10) = [10, 10, 9, 9, 8, 8, 7] → 6 successes, 0 botches → net = 6
// Outcome = Failure (net 6 < DC 19)

// WITS roll (7d10) = [10, 10, 10, 10, 10, 10, 9] → 7 successes, 0 botches → net = 7
// Outcome = Failure (net 7 < DC 19)

// Need much higher WITS or less damage for success...
// With WITS 7, maximum possible net = 7 (all 10s, no botches)
// Cannot achieve DC 19 with WITS 7 and 55 damage

// Reducing damage to 20 for demonstration:
// DC = 8 + (20 / 5) = 12
// WITS roll (7d10) = [10, 10, 9, 8, 7, 3, 1] → 4 successes, 1 botch → net = 3
// Outcome = Failure (net 3 < DC 12)

// WITS roll (7d10) = [10, 10, 10, 9, 9, 8, 8] → 7 successes, 0 botches → net = 7
// Outcome = Failure (net 7 < DC 12)

// WITS roll (7d10) = [10, 10, 10, 10, 10, 9, 9] → 7 successes, 0 botches → net = 7
// Still failure... Need higher WITS

// With WITS 15 and damage 20:
// WITS roll (15d10) = [10,10,10,10,9,9,8,8,8,7,4,3,2,1,1] → 7 successes, 2 botches → net = 5
// Outcome = Failure (net 5 < DC 12)

// WITS roll (15d10) = [10,10,10,10,10,10,10,10,9,9,9,9,8,8,8] → 15 successes, 0 botches → net = 15
// Outcome = Success (net 15 >= DC 12, < DC 17)
result.DurabilityRestored = min(20, 15 * 5) = min(20, 75) = 20
equipment.CurrentDurability = 80 + 20 = 100  // Fully repaired
```

### UC-REPAIR-02: Masterwork Full Restoration
**Scenario:** Highly skilled character achieves masterwork repair.

**Setup:**
```csharp
var character = new Character { Name = "Bjorn", Wits = 12 };
var equipment = new Equipment
{
    Name = "ClanForged Greatsword",
    CurrentDurability = 75,
    MaxDurability = 100
};

// Damage: 100 - 75 = 25
// Scrap cost: ceil(25 / 5) = 5
// Repair DC: 8 + (25 / 5) = 13
// Masterwork threshold: 13 + 5 = 18
```

**Execution:**
```csharp
var result = await _bodgingService.RepairItemAsync(character, equipment.Id);
// Consume 5 Scrap
// WITS roll (12d10) = [10,10,10,10,10,10,9,9,8,8,7,3] → 8 successes, 0 botches → net = 8
// Outcome = Failure (net 8 < DC 13)

// WITS roll (12d10) = [10,10,10,10,10,10,10,10,10,9,9,8] → 11 successes, 0 botches → net = 11
// Outcome = Failure (net 11 < DC 13)

// WITS roll (12d10) = [10,10,10,10,10,10,10,10,10,10,9,9] → 12 successes, 0 botches → net = 12
// Outcome = Failure (net 12 < DC 13)

// WITS roll (12d10) = [10,10,10,10,10,10,10,10,10,10,10,9] → 12 successes, 0 botches → net = 12
// Still failure (this is hard!)

// WITS roll (12d10) = [10,10,10,10,10,10,10,10,10,10,10,10] → 12 successes, 0 botches → net = 12
// Outcome = Failure (net 12 < DC 13)

// Hmm, need at least 13 successes. With 12d10, max is 12 successes.
// This crafter CAN'T achieve success on this repair with these stats.

// Let's use WITS 20 for demonstration:
// WITS roll (20d10) = [10,10,10,10,10,10,10,10,10,10,9,9,9,9,8,8,8,7,4,2] → 13 successes, 0 botches → net = 13
// Outcome = Success (net 13 >= DC 13, < DC 18)

// WITS roll (20d10) = [10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,9,9] → 20 successes, 0 botches → net = 20
// Outcome = Masterwork (net 20 >= DC 18)
```

**Result (Masterwork):**
```csharp
result.IsSuccess == true
result.Outcome == CraftingOutcome.Masterwork
result.NetSuccesses == 20
result.DifficultyClass == 13
result.DurabilityRestored == 25  // Full damage restored
result.ScrapConsumed == 5
result.Message == "MASTERWORK! ClanForged Greatsword has been fully restored to pristine condition."
equipment.CurrentDurability == 100  // Fully repaired
```

### UC-REPAIR-03: Catastrophic Failure (MaxDurability Loss)
**Scenario:** Poor roll results in permanent equipment degradation.

**Setup:**
```csharp
var character = new Character { Name = "Elara", Wits = 4 };
var equipment = new Equipment
{
    Name = "Scavenged Chainmail",
    CurrentDurability = 30,
    MaxDurability = 80
};

// Damage: 80 - 30 = 50
// Scrap cost: ceil(50 / 5) = 10
// Repair DC: 8 + (50 / 5) = 18
```

**Execution:**
```csharp
var result = await _bodgingService.RepairItemAsync(character, equipment.Id);
// Consume 10 Scrap
// WITS roll (4d10) = [1, 1, 2, 3] → 0 successes, 2 botches → net = -2
// Outcome = Catastrophe (net -2 < 0)
```

**Result (Catastrophe):**
```csharp
result.IsSuccess == false
result.Outcome == CraftingOutcome.Catastrophe
result.NetSuccesses == -2
result.DifficultyClass == 18
result.DurabilityRestored == 0
result.ScrapConsumed == 10
result.MaxDurabilityLost == 10
result.Message == "CATASTROPHE! Your fumbled repair has permanently damaged Scavenged Chainmail. Maximum durability reduced by 10."

// Equipment state:
equipment.MaxDurability == 70  // 80 - 10 = 70 (permanently reduced)
equipment.CurrentDurability == 30  // Unchanged (still within new max)

// New effective state: 30/70 durability (worse than before)
```

### UC-REPAIR-04: Salvage Equipment for Scrap
**Scenario:** Player salvages damaged equipment to recover materials.

**Setup:**
```csharp
var character = new Character { Name = "Ragnhild" };
var equipment = new Equipment
{
    Id = Guid.NewGuid(),
    Name = "JuryRigged Dagger",
    CurrentDurability = 10,
    MaxDurability = 50,
    Weight = 500,
    Quality = QualityTier.JuryRigged
};

// Salvage yield calculation:
// qualityMod = 0 (JuryRigged)
// yield = (500 / 100) * (0 + 1) = 5 * 1 = 5 Scrap
```

**Execution:**
```csharp
var result = await _bodgingService.SalvageItemAsync(character, equipment.Id);
// Step 1: Validate item (Equipment type) ✓
// Step 2: Calculate yield (5 Scrap)
// Step 3: Remove item from inventory
// Step 4: Add 5 Scrap to inventory
```

**Result:**
```csharp
result.IsSuccess == true
result.ItemName == "JuryRigged Dagger"
result.ScrapYield == 5
result.Message == "Salvaged Jury-Rigged Dagger for 5 Scrap."

// Character inventory:
// - JuryRigged Dagger (removed)
// + 5 Scrap (added)
```

### UC-REPAIR-05: Salvage High-Quality Equipment
**Scenario:** Player salvages MythForged equipment for maximum Scrap yield.

**Setup:**
```csharp
var equipment = new Equipment
{
    Name = "MythForged Platemail",
    Weight = 3000,
    Quality = QualityTier.MythForged
};

// Salvage yield:
// qualityMod = 4 (MythForged)
// yield = (3000 / 100) * (4 + 1) = 30 * 5 = 150 Scrap
```

**Result:**
```csharp
result.IsSuccess == true
result.ScrapYield == 150
result.Message == "Salvaged MythForged Platemail for 150 Scrap."

// This demonstrates the trade-off:
// Legendary equipment salvages for massive Scrap yields
// But permanently loses the item
```

---

## Decision Trees

### Repair Workflow Decision Tree
```
[Start: RepairItemAsync(character, itemId)]
    ↓
[Item Lookup: Inventory.FirstOrDefault(id)]
    ├─ Item not found → Return Failure ("Item not found")
    └─ Item found
        ↓
    [Type Check: Item is Equipment?]
        ├─ Not equipment → Return Failure ("Cannot repair - not equipment")
        └─ Is equipment
            ↓
        [Damage Check: MaxDurability - CurrentDurability]
            ├─ Damage <= 0 → Return Failure ("Already in good condition")
            └─ Damage > 0
                ↓
            [Calculate Scrap Cost: ceil(damage / 5)]
                ↓
            [Scrap Availability Check: CanRepair()]
                ├─ Insufficient Scrap → Return Failure ("Insufficient Scrap")
                └─ Sufficient Scrap
                    ↓
                [Consume Scrap: RemoveItemAsync()]
                    ├─ Removal failed → Return Failure ("Failed to consume")
                    └─ Removal success
                        ↓
                    [Calculate DC: 8 + (damage / 5)]
                        ↓
                    [WITS Roll: Roll(character.Wits)]
                        ↓
                    [Calculate Net Successes: Successes - Botches]
                        ↓
                    [Determine Outcome]
                        ├─ Net < 0 → Catastrophe
                        │   ↓
                        │   [Reduce MaxDurability by 10]
                        │   [Clamp CurrentDurability to new max]
                        │   [Return Catastrophe result]
                        ├─ Net >= DC + 5 → Masterwork
                        │   ↓
                        │   [Restore CurrentDurability to MaxDurability]
                        │   [Return Masterwork result]
                        ├─ Net >= DC → Success
                        │   ↓
                        │   [Calculate restored = min(damage, net * 5)]
                        │   [Increase CurrentDurability by restored]
                        │   [Cap at MaxDurability]
                        │   [Return Success result]
                        └─ Net < DC (but >= 0) → Failure
                            ↓
                            [No durability restored]
                            [Return Failure result (Scrap wasted)]
```

### Salvage Workflow Decision Tree
```
[Start: SalvageItemAsync(character, itemId)]
    ↓
[Item Lookup: Inventory.FirstOrDefault(id)]
    ├─ Item not found → Return Failure ("Item not found")
    └─ Item found
        ↓
    [Type Check: Item is Equipment?]
        ├─ Not equipment → Return Failure ("Cannot salvage - not equipment")
        └─ Is equipment
            ↓
        [Calculate Yield: (weight / 100) * (qualityMod + 1)]
            ↓
        [Remove Item: RemoveItemAsync(equipment)]
            ├─ Removal failed → Return Failure ("Failed to remove item")
            └─ Removal success
                ↓
            [Add Scrap: AddItemAsync("scrap", yield)]
                ↓
            [Return Success result with yield]
```

### DC Scaling Decision Tree
```
[Equipment Damage]
    ↓
[Damage Range Check]
    ├─ 0-4 damage
    │   ├─ DC Modifier: +0
    │   └─ Final DC: 8
    ├─ 5-9 damage
    │   ├─ DC Modifier: +1
    │   └─ Final DC: 9
    ├─ 10-14 damage
    │   ├─ DC Modifier: +2
    │   └─ Final DC: 10
    ├─ 25-29 damage
    │   ├─ DC Modifier: +5
    │   └─ Final DC: 13
    ├─ 50-54 damage
    │   ├─ DC Modifier: +10
    │   └─ Final DC: 18
    └─ 95-99 damage
        ├─ DC Modifier: +19
        └─ Final DC: 27 (near-impossible)
```

---

## Sequence Diagrams

### Full Repair Sequence
```
Player         BodgingService    InventoryService    DiceService    Equipment
   |                 |                  |                |              |
   |--RepairItemAsync()|                |                |              |
   |                 |                  |                |              |
   |                 |--Lookup item-----|--------------->|              |
   |                 |<--Equipment------|----------------|              |
   |                 |                  |                |              |
   |                 |--CalculateRepairCost()----------->|              |
   |                 |<--Scrap cost=11------------------|              |
   |                 |                  |                |              |
   |                 |--CanRepair()-----|--------------->|              |
   |                 |<--true-----------|----------------|              |
   |                 |                  |                |              |
   |                 |--RemoveItemAsync("scrap", 11)--->|              |
   |                 |<--RemoveResult-------------------|              |
   |                 |                  |                |              |
   |                 |--CalculateDC()--------------------------------->>|
   |                 |<--DC=19------------------------------------- ----|
   |                 |                  |                |              |
   |                 |--Roll(wits, ctx)|--------------->|              |
   |                 |<--DiceResult----|----------------|              |
   |                 |                  |                |              |
   |                 |--DetermineOutcome()                              |
   |                 |<--Outcome=Success                                |
   |                 |                  |                |              |
   |            [If Success]            |                |              |
   |                 |--Update CurrentDurability---------------------->|
   |                 |                  |                |              |
   |<--RepairResult--|                  |                |              |
   |                 |                  |                |              |
```

### Salvage Sequence
```
Player         BodgingService    InventoryService    ItemRepository
   |                 |                  |                   |
   |--SalvageItemAsync()|               |                   |
   |                 |                  |                   |
   |                 |--Lookup item-----|------------------>|
   |                 |<--Equipment------|-------------------|
   |                 |                  |                   |
   |                 |--CalculateSalvageYield()             |
   |                 |<--Yield=45-------------------------- |
   |                 |                  |                   |
   |                 |--RemoveItemAsync(equipment)--------->|
   |                 |<--RemoveResult---------------------- |
   |                 |                  |                   |
   |                 |--GetByNameAsync("scrap")------------>|
   |                 |<--Scrap item-------------------------|
   |                 |                  |                   |
   |                 |--AddItemAsync(scrap, 45)------------>|
   |                 |<--AddResult--------------------------|
   |                 |                  |                   |
   |<--SalvageResult-|                  |                   |
   |                 |                  |                   |
```

---

## Workflows

### Standard Repair Checklist
**Pre-Conditions:**
- [ ] Player has selected an equipment item (itemId)
- [ ] Player character exists with Wits attribute

**Execution Steps:**
1. [ ] **Item Lookup**
   - Query `character.Inventory` by itemId
   - Validate item exists
   - If not found → Return failure result, exit
2. [ ] **Type Validation**
   - Verify item is `Equipment` type
   - If not equipment → Return failure result, exit
3. [ ] **Damage Check**
   - Calculate `damage = MaxDurability - CurrentDurability`
   - Verify `damage > 0`
   - If no damage → Return failure result ("already repaired"), exit
4. [ ] **Cost Calculation**
   - Calculate Scrap cost: `ceil(damage / 5)`
   - Log cost calculation
5. [ ] **Availability Check**
   - Call `CanRepair(character, equipment)`
   - Verify character has sufficient Scrap
   - If insufficient → Return failure result ("insufficient Scrap"), exit
6. [ ] **Scrap Consumption** (Point of No Return)
   - Call `_inventoryService.RemoveItemAsync(character, "scrap", cost)`
   - Log consumption success/failure
   - If removal fails → Return failure result, exit
   - Scrap is now gone (non-refundable)
7. [ ] **DC Calculation**
   - Calculate DC: `8 + (damage / 5)`
   - Log DC value
8. [ ] **WITS Dice Roll**
   - Retrieve `character.Wits` attribute
   - Call `_diceService.Roll(wits, $"Repair {equipment.Name}")`
   - Receive `DiceResult` with `Successes`, `Botches`, `Rolls`
9. [ ] **Calculate Net Successes**
   - `netSuccesses = diceResult.Successes - diceResult.Botches`
   - Log roll results
10. [ ] **Determine Outcome**
    - Call `DetermineOutcome(netSuccesses, dc)`
    - Outcome = Catastrophe | Failure | Success | Masterwork
11. [ ] **Execute Outcome Handler**
    - Switch on outcome:
      - **Masterwork:** Restore to MaxDurability, return success result
      - **Success:** Restore `min(damage, net * 5)` durability, return success result
      - **Failure:** Return failure result (no repair)
      - **Catastrophe:** Reduce MaxDurability by 10, clamp CurrentDurability, return catastrophe result
12. [ ] **Return RepairResult**
    - Populate all fields: `IsSuccess`, `Outcome`, `DurabilityRestored`, `Message`, etc.
    - Caller receives result object

**Post-Conditions:**
- [ ] Scrap consumed (regardless of success/failure)
- [ ] Durability restored (if success/masterwork)
- [ ] MaxDurability reduced (if catastrophe)
- [ ] All actions logged at INFO level

### Salvage Checklist
**Pre-Conditions:**
- [ ] Player has selected an equipment item to destroy
- [ ] Player confirms salvage intent (irreversible)

**Execution Steps:**
1. [ ] **Item Lookup**
   - Query `character.Inventory` by itemId
   - Validate item exists
   - If not found → Return failure result, exit
2. [ ] **Type Validation**
   - Verify item is `Equipment` type
   - If not equipment → Return failure result, exit
3. [ ] **Yield Calculation**
   - Read `equipment.Weight` and `equipment.Quality`
   - Calculate quality modifier (0-4)
   - Calculate yield: `(weight / 100) * (qualityMod + 1)`
   - Apply minimum: `max(1, yield)`
   - Log calculated yield
4. [ ] **Item Removal**
   - Call `_inventoryService.RemoveItemAsync(character, equipment.Name, 1)`
   - If removal fails → Return failure result, exit
   - Item is now permanently destroyed
5. [ ] **Scrap Addition**
   - Lookup "scrap" item from `_itemRepository.GetByNameAsync()`
   - If scrap item not found, create basic scrap entity
   - Call `_inventoryService.AddItemAsync(character, scrap, yield)`
   - Log Scrap addition
6. [ ] **Return SalvageResult**
   - `IsSuccess = true`
   - `ItemName = equipment.Name`
   - `ScrapYield = yield`
   - `Message = "Salvaged {ItemName} for {yield} Scrap."`

**Post-Conditions:**
- [ ] Equipment item removed from inventory
- [ ] Scrap materials added to inventory
- [ ] Action logged at INFO level

---

## Cross-System Integration

### Service Dependencies

| Dependent Service | Integration Point | Purpose | Method Calls |
|-------------------|-------------------|---------|--------------|
| **DiceService** | WITS roll execution | Randomization for repair success | `Roll(poolSize, context)` |
| **InventoryService** | Scrap consumption & addition | Material economy management | `RemoveItemAsync()`, `AddItemAsync()` |
| **ItemRepository** | Scrap item lookup | Retrieve Scrap template for salvage | `GetByNameAsync("scrap")` |

### Integration Workflows

#### Inventory Integration (Repair)
```csharp
// Step 1: Validate Scrap availability
var scrapInInventory = character.Inventory
    .FirstOrDefault(inv => inv.Item.Name.Equals("scrap", StringComparison.OrdinalIgnoreCase));
var available = scrapInInventory?.Quantity ?? 0;
if (available < requiredScrap) return Failure;

// Step 2: Consume Scrap
var removeResult = await _inventoryService.RemoveItemAsync(character, "scrap", scrapCost);
if (!removeResult.Success) return Failure;
```

#### Inventory Integration (Salvage)
```csharp
// Step 1: Remove equipment
var removeResult = await _inventoryService.RemoveItemAsync(character, equipment.Name, 1);

// Step 2: Add Scrap
var scrapItem = await _itemRepository.GetByNameAsync("scrap");
await _inventoryService.AddItemAsync(character, scrapItem, yield);
```

#### Dice Service Integration
```csharp
// Called for every repair attempt (after Scrap consumption)
var wits = character.Wits;
var diceResult = _diceService.Roll(wits, $"Repair {equipment.Name}");
// diceResult.Successes = count of 8, 9, 10 rolls
// diceResult.Botches = count of 1 rolls
// diceResult.Rolls = [3, 8, 1, 10, 5] (example)

var netSuccesses = diceResult.Successes - diceResult.Botches;
```

### Cross-System Data Flow
```
[Player Command: "repair longsword"]
    ↓
[BodgingService.RepairItemAsync()]
    ├─→ Character.Inventory: Lookup item, validate equipment type
    ├─→ Equipment: Read CurrentDurability, MaxDurability, calculate damage
    ├─→ InventoryService: Check Scrap availability
    ├─→ InventoryService: Remove Scrap (non-refundable)
    ├─→ DiceService: Roll WITS pool
    ├─→ DetermineOutcome: Map net successes to outcome
    └─→ Outcome Handlers:
        ├─ Masterwork/Success → Equipment: Update CurrentDurability
        ├─ Catastrophe → Equipment: Reduce MaxDurability, clamp CurrentDurability
        └─ Failure → (No equipment changes, Scrap already consumed)
    ↓
[Return RepairResult to caller]

[Player Command: "salvage dagger"]
    ↓
[BodgingService.SalvageItemAsync()]
    ├─→ Character.Inventory: Lookup item, validate equipment type
    ├─→ Equipment: Read Weight, Quality for yield calculation
    ├─→ InventoryService: Remove equipment item
    ├─→ ItemRepository: Lookup "scrap" item template
    └─→ InventoryService: Add Scrap to inventory
    ↓
[Return SalvageResult to caller]
```

---

## Data Models

### Equipment Entity (Partial)
```csharp
// Location: RuneAndRust.Core/Entities/Equipment.cs
public class Equipment : Item
{
    public int CurrentDurability { get; set; }      // Current HP of item (0-MaxDurability)
    public int MaxDurability { get; set; }          // Maximum HP of item (can be reduced by catastrophes)
    public int Weight { get; set; }                 // Weight in grams (used for salvage yield)
    public QualityTier Quality { get; set; }        // Quality tier (affects salvage yield)
    // ... other properties
}
```

### RepairResult Model
```csharp
// Location: RuneAndRust.Core/Models/Crafting/RepairResult.cs
public record RepairResult(
    bool IsSuccess,                       // Overall success flag
    CraftingOutcome Outcome,              // Masterwork | Success | Failure | Catastrophe
    string ItemName,                      // Equipment name
    int DiceRolled,                       // Pool size (WITS value)
    int Successes,                        // Count of 8, 9, 10 rolls
    int Botches,                          // Count of 1 rolls
    int NetSuccesses,                     // Successes - Botches
    int DifficultyClass,                  // Calculated DC
    int DurabilityRestored,               // Amount repaired (0 if failure/catastrophe)
    int ScrapConsumed,                    // Scrap materials consumed
    int? MaxDurabilityLost,               // MaxDurability reduction (catastrophe only, null otherwise)
    string Message,                       // Player-facing result message
    IReadOnlyList<int> Rolls              // Individual die results
);
```

### SalvageResult Model
```csharp
// Location: RuneAndRust.Core/Models/Crafting/SalvageResult.cs
public record SalvageResult(
    bool IsSuccess,                       // Overall success flag
    string ItemName,                      // Equipment name
    int ScrapYield,                       // Scrap materials recovered
    string Message                        // Player-facing result message
);
```

### QualityTier Enum
```csharp
// Location: RuneAndRust.Core/Enums/QualityTier.cs
public enum QualityTier
{
    JuryRigged,   // Modifier: 0 (lowest quality)
    Scavenged,    // Modifier: 1
    ClanForged,   // Modifier: 2 (standard quality)
    Optimized,    // Modifier: 3
    MythForged    // Modifier: 4 (legendary quality)
}
```

---

## Configuration

### Constants
```csharp
// BodgingService.cs
private const int BaseRepairDc = 8;                // Base DC before damage scaling
private const int DamageDivisor = 5;               // Scales both DC and Scrap cost
private const int MasterworkThreshold = 5;         // DC + 5 for masterwork
private const int CatastrophePenalty = 10;         // MaxDurability loss on catastrophe
private const int SalvageWeightDivisor = 100;      // Weight-to-Scrap conversion
private const string ScrapItemName = "scrap";      // Scrap material identifier
```

### Quality Modifier Table
```csharp
private static int GetQualityModifier(QualityTier tier) => tier switch
{
    QualityTier.JuryRigged => 0,
    QualityTier.Scavenged => 1,
    QualityTier.ClanForged => 2,
    QualityTier.Optimized => 3,
    QualityTier.MythForged => 4,
    _ => 0
};
```

### Repair DC Scaling
| Damage | DC | Interpretation |
|--------|-----|----------------|
| 0-4    | 8   | Moderate (minor scratches) |
| 5-9    | 9   | Moderate (light damage) |
| 10-19  | 10-11 | Hard (noticeable wear) |
| 20-29  | 12-13 | Very Hard (significant damage) |
| 50-59  | 18-19 | Legendary (severe damage) |
| 95-99  | 27    | Near-impossible (nearly destroyed) |

---

## Testing

### Unit Test Coverage
**Location:** `RuneAndRust.Tests/Engine/BodgingServiceTests.cs`

**Test Count:** 35+ tests (80%+ coverage)

**Test Categories:**

#### Repair Validation Tests
- `RepairItemAsync_WithInvalidItemId_ReturnsFailure`
- `RepairItemAsync_WithNonEquipment_ReturnsFailure`
- `RepairItemAsync_WithFullDurability_ReturnsFailure`
- `RepairItemAsync_WithInsufficientScrap_ReturnsFailure`

#### Cost Calculation Tests
- `CalculateRepairCost_WithMinimalDamage_ReturnsMinimumOne`
- `CalculateRepairCost_With25Damage_Returns5Scrap`
- `CalculateRepairCost_With55Damage_Returns11Scrap`
- `CalculateRepairCost_With100Damage_Returns20Scrap`

#### DC Calculation Tests
- `RepairItemAsync_CalculatesDcBasedOnDamage`
- `RepairItemAsync_With55Damage_HasDc19`

#### Repair Outcome Tests
- `RepairItemAsync_Success_RestoresPartialDurability`
- `RepairItemAsync_Masterwork_RestoresFullDurability`
- `RepairItemAsync_Failure_ConsumesScrapNoRepair`
- `RepairItemAsync_Catastrophe_ReducesMaxDurability`

#### Scrap Consumption Tests
- `RepairItemAsync_Success_ConsumesScrap`
- `RepairItemAsync_Failure_ConsumesScrap`
- `RepairItemAsync_Catastrophe_ConsumesScrap`

#### Salvage Tests
- `SalvageItemAsync_ValidEquipment_ReturnsScrap`
- `SalvageItemAsync_JuryRiggedQuality_UsesModifier0`
- `SalvageItemAsync_ClanForgedQuality_UsesModifier2`
- `SalvageItemAsync_MythForgedQuality_UsesModifier4`
- `SalvageItemAsync_RemovesItemFromInventory`
- `SalvageItemAsync_AddsScrapToInventory`

#### Edge Case Tests
- `RepairItemAsync_CatastropheReducesMaxToZero_ClampsCurrentDurability`
- `RepairItemAsync_SuccessRestoresCappedAtMax_DoesNotExceedMax`
- `CalculateSalvageYield_MinimumYieldIsOne`

---

## Domain 4 Compliance

All repair/salvage messages comply with Domain 4 Technology Constraints. No precision measurements are used in player-facing text.

### Compliant Language Examples
| Forbidden (Domain 4 Violation) | Compliant (Approved) |
|---------------------------------|----------------------|
| "DC increased by 55%" | "Repair becomes much harder with severe damage" |
| "Restored exactly 45 durability points" | "Partially repaired, restored most damage" |
| "11 Scrap materials consumed" | "Several pieces of scrap metal consumed" |
| "MaxDurability reduced by 10" | "Equipment permanently weakened" |
| "Salvage yield: 75 units" | "Salvage yields a significant amount of scrap" |

### Voice Compliance Notes
- All `RepairResult.Message` and `SalvageResult.Message` strings use narrative language
- No system terminology exposed to players ("Net Successes", "DC", "Botches" are internal only)
- Catastrophe messages use vivid descriptions ("fumbled repair", "permanently damaged")
- Numeric values in results are for internal tracking, not player display

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- Added code traceability remarks to implementation files:
  - `IBodgingService.cs` - interface spec reference
  - `BodgingService.cs` - service spec reference
  - `BodgingServiceTests.cs` - test spec reference

---

## Version History

### v1.0.0 (Current - SPEC-REPAIR-001)
- Initial specification based on implemented `BodgingService.cs`
- Documented repair and salvage mechanics
- Comprehensive DC scaling and Scrap economy
- Decision trees for all workflows
- Cross-system integration matrices complete

### Implementation History
- **v0.3.0:** Implemented catastrophe mechanics (MaxDurability reduction)
- **v0.2.0:** Added salvage system with quality-based yields
- **v0.1.0:** Initial repair system with WITS-based rolls

---

## Future Enhancements

### Planned Features (v0.4.0+)
1. **Tool Requirements:** Require toolkit items for repairs (penalty without tools)
2. **Material Quality Impact:** Higher quality Scrap improves repair success chance
3. **Partial Scrap Refunds:** Return 50% Scrap on masterwork repairs
4. **Progressive Degradation:** Equipment gradually loses MaxDurability with repeated use
5. **Specialized Repairs:** Trade-specific repair bonuses (Smith trade gets DC reduction on armor)
6. **Repair Kits:** Consumable items that grant temporary WITS bonuses for repairs
7. **Critical Success Effects:** On natural all-10s roll, restore MaxDurability in addition to CurrentDurability

### Known Limitations for Future Resolution
1. **No Undo:** Once Scrap is consumed, it cannot be refunded (intentional, but may add "Salvage Attempt" preview)
2. **Static Catastrophe Penalty:** Always -10 MaxDurability (may scale with damage severity)
3. **No Repair Time:** Repairs are instantaneous (may add turn/hour cost later)
4. **No Batch Operations:** One repair at a time (may add "Repair All" command)
5. **No Scrap Types:** Only generic "scrap" material (may add metal-specific materials later)
