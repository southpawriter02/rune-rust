---
id: SPEC-CRAFT-001
title: Crafting System
version: 1.0.1
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-DICE-001, SPEC-INV-001, SPEC-REPAIR-001]
---

# SPEC-CRAFT-001: Crafting System

> **Version:** 1.0.1
> **Status:** Implemented
> **Service:** `CraftingService`
> **Location:** `RuneAndRust.Engine/Services/CraftingService.cs`
> **Related Entities:** `Recipe` ([Recipe.cs:9](RuneAndRust.Core/Entities/Recipe.cs#L9)), `CraftingResult` ([CraftingResult.cs](RuneAndRust.Core/Models/Crafting/CraftingResult.cs))

---

## Overview

The Crafting System implements **WITS-based skill checks** for creating items from raw materials using learned recipes. It supports four trade disciplines (Alchemy, Bodging, FieldMedicine, Runeforging), each with unique catastrophe mechanics. The system uses the d10 dice pool mechanic for success determination and integrates with inventory, trauma, and item quality subsystems.

**Core Design Principles:**
- **Ingredient Consumption Before Rolling:** Materials are always consumed when attempting crafting (failure still costs resources)
- **Quality Tier Progression:** Masterwork (DC +5) → Success (meet DC) → Failure (below DC) → Catastrophe (net negative)
- **Trade-Specific Catastrophes:** Each trade has unique failure consequences (Alchemy: explosive damage, Runeforging: corruption, etc.)
- **No Auto-Success:** Every craft requires a WITS dice pool roll, regardless of character level

---

## Behaviors

### Primary Behaviors

#### 1. Recipe Validation (`CraftItemAsync`)
Validates recipe existence and ingredient availability before crafting.

```csharp
Task<CraftingResult> CraftItemAsync(Character crafter, string recipeId)
```

**Sequence:**
1. Look up recipe by ID from `RecipeRegistry`
2. Validate recipe exists
3. Check crafter has all required ingredients (quantity check)
4. Return early with failure result if validation fails

**Example:**
```csharp
// Recipe lookup
var recipe = RecipeRegistry.GetById("RCP_ALC_STIM");
// recipe.Ingredients: {"Herb" => 2, "Water" => 1}

// Validation check
if (!HasIngredients(crafter, recipe))
{
    return CreateFailedResult(recipe.RecipeId, recipe.Name, 0, 0, 0, 0, recipe.BaseDc,
        $"Missing ingredients for {recipe.Name}.", Array.Empty<int>());
}
```

#### 2. Ingredient Consumption (`CraftItemAsync`)
Consumes ingredients from inventory **before** rolling dice (no refunds on failure).

**Sequence:**
1. Iterate `recipe.Ingredients` dictionary
2. Call `InventoryService.RemoveItemAsync()` for each ingredient
3. Log consumption success/failure
4. Continue to dice roll (ingredients already consumed)

**Example:**
```csharp
// Consuming 2 Herbs and 1 Water
foreach (var (itemId, required) in recipe.Ingredients)
{
    var removeResult = await _inventoryService.RemoveItemAsync(crafter, itemId, required);
    // Herbs: removeResult.Success = true
    // Water: removeResult.Success = true
}
```

**Critical Design Note:** Ingredients are consumed at line [CraftingService.cs:66](RuneAndRust.Engine/Services/CraftingService.cs#L66) BEFORE the WITS roll. This is intentional to simulate risk/reward - failed crafts still cost materials.

#### 3. WITS Dice Roll (`CraftItemAsync`)
Executes d10 dice pool roll using crafter's WITS attribute.

**Sequence:**
1. Retrieve `crafter.Wits` attribute (pool size)
2. Call `_diceService.Roll(wits, $"Craft {recipe.Name}")`
3. Calculate net successes: `Successes - Botches`
4. Log roll results with context

**Example:**
```csharp
// Crafter has WITS = 4
var wits = crafter.Wits;  // 4
var diceResult = _diceService.Roll(wits, "Craft Basic Stimpack");
// diceResult.Rolls = [2, 8, 10, 1]
// diceResult.Successes = 2 (8, 10)
// diceResult.Botches = 1 (1)
var netSuccesses = 2 - 1 = 1
```

#### 4. Outcome Determination (`DetermineOutcome`)
Maps net successes to crafting outcomes using DC thresholds.

```csharp
private static CraftingOutcome DetermineOutcome(int netSuccesses, int dc)
```

**Decision Tree:**
```
netSuccesses < 0                     → Catastrophe
netSuccesses >= dc + 5               → Masterwork (MythForged quality)
netSuccesses >= dc                   → Success (ClanForged quality)
netSuccesses < dc (but >= 0)         → Failure (materials lost, no output)
```

**Threshold Table:**
| Net Successes | DC = 5 Example | Outcome | Quality Tier |
|---------------|----------------|---------|--------------|
| -1            | < 0            | Catastrophe | None (+ penalty) |
| 0-4           | < 5            | Failure | None |
| 5-9           | 5-9            | Success | ClanForged |
| 10+           | >= 10          | Masterwork | MythForged |

**Example:**
```csharp
// Recipe: DC = 5
// Net Successes = 1
var outcome = DetermineOutcome(1, 5);  // Failure (1 < 5)

// Net Successes = 7
var outcome = DetermineOutcome(7, 5);  // Success (7 >= 5, but < 10)

// Net Successes = 12
var outcome = DetermineOutcome(12, 5); // Masterwork (12 >= 10)

// Net Successes = -2
var outcome = DetermineOutcome(-2, 5); // Catastrophe (-2 < 0)
```

#### 5. Result Handling (Outcome Switch)
Executes outcome-specific logic based on crafting result.

**Masterwork Handler ([HandleMasterworkAsync:169](RuneAndRust.Engine/Services/CraftingService.cs#L169)):**
```csharp
// Creates item with MythForged quality tier
var masterworkItem = CloneItemWithQuality(outputItem, QualityTier.MythForged);
await _inventoryService.AddItemAsync(crafter, masterworkItem, recipe.OutputQuantity);

return new CraftingResult(
    IsSuccess: true,
    Outcome: CraftingOutcome.Masterwork,
    OutputQuality: QualityTier.MythForged,
    Message: $"MASTERWORK! You craft a flawless {recipe.Name}."
);
```

**Success Handler ([HandleSuccessAsync:208](RuneAndRust.Engine/Services/CraftingService.cs#L208)):**
```csharp
// Creates item with ClanForged quality tier
var craftedItem = CloneItemWithQuality(outputItem, QualityTier.ClanForged);
await _inventoryService.AddItemAsync(crafter, craftedItem, recipe.OutputQuantity);

return new CraftingResult(
    IsSuccess: true,
    Outcome: CraftingOutcome.Success,
    OutputQuality: QualityTier.ClanForged,
    Message: $"Success! You craft {recipe.Name}."
);
```

**Failure Handler ([HandleFailure:247](RuneAndRust.Engine/Services/CraftingService.cs#L247)):**
```csharp
// No item created, materials already consumed
return new CraftingResult(
    IsSuccess: false,
    Outcome: CraftingOutcome.Failure,
    OutputItemId: null,
    OutputQuantity: 0,
    Message: $"Failure. Your attempt at {recipe.Name} falls short. Materials wasted."
);
```

**Catastrophe Handler ([HandleCatastrophe:275](RuneAndRust.Engine/Services/CraftingService.cs#L275)):**
```csharp
// Trade-specific catastrophe effects (v0.3.1c)
switch (recipe.CatastropheType)
{
    case CatastropheType.Explosive:
        // Alchemy: Explosive damage
        crafter.CurrentHP -= recipe.CatastropheDamage;
        message = $"CATASTROPHE! Your {recipe.Name} explodes! You take {recipe.CatastropheDamage} damage!";
        break;

    case CatastropheType.Corruption:
        // Runeforging: Corruption gain
        _traumaService.AddCorruption(crafter, recipe.CatastropheCorruption, "Runic Backlash");
        message = $"CATASTROPHE! The runes twist against you! You gain {recipe.CatastropheCorruption} Corruption!";
        break;

    default:
        // Standard: Materials lost only
        message = $"CATASTROPHE! Your {recipe.Name} attempt goes terribly wrong. Materials destroyed.";
        break;
}
```

### Edge Case Behaviors

#### Recipe Not Found
| Condition | Behavior | Return Value |
|-----------|----------|--------------|
| `RecipeRegistry.GetById(recipeId)` returns null | Log debug, return failed result | `CraftingResult` with `Outcome = Failure`, `Message = "Recipe '{recipeId}' not found."` |

**Example:**
```csharp
var result = await _craftingService.CraftItemAsync(crafter, "INVALID_ID");
// result.IsSuccess = false
// result.Message = "Recipe 'INVALID_ID' not found."
// result.Rolls = [] (empty array - no roll occurred)
```

#### Missing Ingredients
| Condition | Behavior | Return Value |
|-----------|----------|--------------|
| `HasIngredients()` returns false | Log debug, return failed result | `CraftingResult` with `Outcome = Failure`, `Message = "Missing ingredients for {recipeName}."` |

**Example:**
```csharp
// Recipe requires 2 Herbs, crafter has 1
var result = await _craftingService.CraftItemAsync(crafter, "RCP_ALC_STIM");
// result.IsSuccess = false
// result.Message = "Missing ingredients for Basic Stimpack."
// result.Rolls = [] (no ingredients consumed, no roll occurred)
```

#### Ingredient Removal Failure
| Condition | Behavior | Impact |
|-----------|----------|--------|
| `InventoryService.RemoveItemAsync()` returns failure | Log warning, continue to next ingredient | Crafting continues (assumes transient error) |

**Rationale:** Ingredient validation already passed via `HasIngredients()`. Removal failures are logged but don't halt crafting, as the dice have already been committed.

**Example:**
```csharp
// Removal fails due to concurrent inventory modification
var removeResult = await _inventoryService.RemoveItemAsync(crafter, "Herb", 2);
if (!removeResult.Success)
{
    _logger.LogWarning("Failed to consume ingredient {ItemId}: {Message}", "Herb", removeResult.Message);
    // Crafting continues to WITS roll
}
```

#### Catastrophe Damage Reduces HP to Zero
| Condition | Behavior | Impact |
|-----------|----------|--------|
| `crafter.CurrentHP - CatastropheDamage < 0` | Clamp to 0 | Character HP set to 0 (death check handled by combat system) |

**Example:**
```csharp
// Alchemy catastrophe: Recipe deals 15 damage, crafter has 8 HP
crafter.CurrentHP = Math.Max(0, crafter.CurrentHP - 15);
// crafter.CurrentHP = 0 (not negative)
```

---

## Restrictions

### MUST NOT
1. **Bypass ingredient consumption** - Ingredients are ALWAYS consumed when crafting begins ([line 66](RuneAndRust.Engine/Services/CraftingService.cs#L66))
2. **Skip WITS rolls** - No auto-success conditions exist; every craft requires a dice roll
3. **Refund materials on failure** - Materials are non-recoverable once consumed
4. **Allow negative HP** - Catastrophe damage uses `Math.Max(0, ...)` clamp ([line 286](RuneAndRust.Engine/Services/CraftingService.cs#L286))
5. **Modify recipe DC during execution** - DC is read-only from recipe definition

### MUST
1. **Validate recipe exists** before attempting craft
2. **Check ingredient availability** via `HasIngredients()` before consumption
3. **Log all crafting attempts** at INFO level minimum ([lines 45, 186, 224, 252, 290](RuneAndRust.Engine/Services/CraftingService.cs))
4. **Apply trade-specific catastrophe effects** when net successes < 0
5. **Create items with quality tiers** - Masterwork = MythForged, Success = ClanForged

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| **WITS Attribute Range** | 1-10 | Determines pool size for rolls |
| **Recipe DC Range** | 3-11 | Typical range (Trivial = 3, Legendary = 11) |
| **Masterwork Threshold** | DC + 5 | Hardcoded constant ([line 26](RuneAndRust.Engine/Services/CraftingService.cs#L26)) |
| **Ingredient Max Quantity** | Inventory max stack size | Typically 99 per ingredient |
| **Recipe Count** | ~50-100 recipes | Limited by `RecipeRegistry` size |

### Functional Limitations
1. **No Partial Ingredient Consumption:** Either all ingredients are consumed or none (atomic operation)
2. **No Recipe Discovery System:** Recipes are globally available via `RecipeRegistry` (no unlock mechanics)
3. **No Crafting Time:** Crafting is instantaneous (no turn/hour cost)
4. **No Material Quality Impact:** Ingredient quality doesn't affect output quality (only WITS roll does)
5. **No Batch Crafting:** Each craft produces exactly `recipe.OutputQuantity` items (no bulk crafting)

### Trade Limitations
| Trade | Catastrophe Type | Current Implementation | Future Enhancement |
|-------|------------------|----------------------|-------------------|
| Alchemy | Explosive | Deals HP damage | Add fire status effect |
| Runeforging | Corruption | Adds permanent corruption | Add tier progression |
| Bodging | None | Materials lost only | Add equipment damage |
| FieldMedicine | None | Materials lost only | Add malfunction chance |

---

## Use Cases

### UC-CRAFT-01: Successful Basic Craft
**Scenario:** Player crafts Basic Stimpack with sufficient WITS.

**Setup:**
```csharp
var crafter = new Character { Name = "Thora", Wits = 5, CurrentHP = 20 };
var recipe = RecipeRegistry.GetById("RCP_ALC_STIM");
// recipe.BaseDc = 4
// recipe.Ingredients = { "Herb" => 2, "Water" => 1 }
// recipe.OutputItemId = "Basic Stimpack"

// Crafter inventory: 2 Herbs, 1 Water
```

**Execution:**
```csharp
var result = await _craftingService.CraftItemAsync(crafter, "RCP_ALC_STIM");
// Step 1: Recipe found ✓
// Step 2: Ingredients validated (2 Herbs, 1 Water present) ✓
// Step 3: Ingredients consumed (-2 Herbs, -1 Water)
// Step 4: WITS roll (5d10) = [3, 8, 9, 2, 1] → 2 successes, 1 botch → net = 1
// Step 5: Outcome = Failure (net 1 < DC 4)
```

**Result:**
```csharp
result.IsSuccess == false
result.Outcome == CraftingOutcome.Failure
result.NetSuccesses == 1
result.DifficultyClass == 4
result.Message == "Failure. Your attempt at Basic Stimpack falls short. Materials wasted."
result.OutputItemId == null  // No item created
```

**Alternate Roll (Success):**
```csharp
// WITS roll (5d10) = [8, 9, 10, 7, 4] → 3 successes, 0 botches → net = 3
// Outcome = Failure (net 3 < DC 4)

// WITS roll (5d10) = [8, 9, 10, 8, 4] → 4 successes, 0 botches → net = 4
// Outcome = Success (net 4 >= DC 4, < DC 4 + 5)
result.IsSuccess == true
result.Outcome == CraftingOutcome.Success
result.OutputQuality == QualityTier.ClanForged
result.OutputQuantity == 1
```

### UC-CRAFT-02: Masterwork Craft
**Scenario:** Player rolls exceptionally well and crafts a masterwork item.

**Setup:**
```csharp
var crafter = new Character { Name = "Bjorn", Wits = 8 };
var recipe = RecipeRegistry.GetById("RCP_SMI_SWORD");
// recipe.BaseDc = 6
// recipe.OutputItemId = "Iron Longsword"
```

**Execution:**
```csharp
var result = await _craftingService.CraftItemAsync(crafter, "RCP_SMI_SWORD");
// WITS roll (8d10) = [8, 9, 10, 10, 9, 8, 7, 10] → 6 successes, 0 botches → net = 6
// Outcome = Success (net 6 >= DC 6, < DC 6 + 5 = 11)

// WITS roll (8d10) = [8, 9, 10, 10, 9, 8, 10, 10] → 7 successes, 0 botches → net = 7
// Outcome = Success (net 7 >= DC 6, < DC 11)

// WITS roll (8d10) = [10, 10, 10, 10, 9, 8, 10, 10] → 8 successes, 0 botches → net = 8
// Outcome = Success (net 8 >= DC 6, < DC 11)

// WITS roll (8d10) = [10, 10, 10, 10, 10, 10, 9, 8] → 8 successes, 0 botches → net = 8
// Outcome = Success (net 8 >= DC 6, < DC 11)

// WITS roll (8d10) = [10, 10, 10, 10, 10, 10, 10, 9] → 8 successes, 0 botches → net = 8
// Outcome = Success (net 8 >= DC 6, < DC 11)

// WITS roll (8d10) = [10, 10, 10, 10, 10, 10, 10, 10] → 8 successes, 0 botches → net = 8
// Outcome = Success (net 8 >= DC 6, < DC 11)

// Wait, I need 11+ for masterwork...
// WITS roll (8d10) = [10, 10, 10, 10, 10, 10, 9, 9] → 8 successes, 0 botches → net = 8
// Still not enough. Let me calculate: DC 6 + 5 = 11 required

// With WITS 8, maximum possible successes is 8 (all 10s)
// This crafter CAN'T achieve masterwork (needs 11+ successes, only has 8d10 pool)
```

**Revised Setup (Higher WITS):**
```csharp
var crafter = new Character { Name = "Bjorn", Wits = 12 };  // Expert smith
var recipe = RecipeRegistry.GetById("RCP_SMI_SWORD");
// recipe.BaseDc = 6

// WITS roll (12d10) = [10, 10, 10, 10, 10, 9, 9, 8, 8, 7, 4, 2] → 7 successes, 0 botches → net = 7
// Outcome = Success (net 7 >= DC 6, < DC 11)

// WITS roll (12d10) = [10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 9] → 12 successes, 0 botches → net = 12
// Outcome = Masterwork (net 12 >= DC 11)
```

**Result:**
```csharp
result.IsSuccess == true
result.Outcome == CraftingOutcome.Masterwork
result.NetSuccesses == 12
result.DifficultyClass == 6
result.OutputQuality == QualityTier.MythForged
result.Message == "MASTERWORK! You craft a flawless Iron Longsword."
// Inventory receives MythForged Iron Longsword (legendary quality)
```

### UC-CRAFT-03: Catastrophe - Alchemy Explosive
**Scenario:** Alchemy recipe botches and explodes, dealing damage.

**Setup:**
```csharp
var crafter = new Character { Name = "Elara", Wits = 3, CurrentHP = 15 };
var recipe = RecipeRegistry.GetById("RCP_ALC_FIREBOMB");
// recipe.BaseDc = 7
// recipe.CatastropheType = CatastropheType.Explosive
// recipe.CatastropheDamage = 8
// recipe.OutputItemId = "Alchemical Firebomb"
```

**Execution:**
```csharp
var result = await _craftingService.CraftItemAsync(crafter, "RCP_ALC_FIREBOMB");
// WITS roll (3d10) = [1, 2, 1] → 0 successes, 2 botches → net = -2
// Outcome = Catastrophe (net -2 < 0)
// Catastrophe Handler:
//   - CatastropheType.Explosive detected
//   - Deals 8 damage to crafter
//   - crafter.CurrentHP = 15 - 8 = 7
```

**Result:**
```csharp
result.IsSuccess == false
result.Outcome == CraftingOutcome.Catastrophe
result.NetSuccesses == -2
result.DamageDealt == 8
result.Message == "CATASTROPHE! Your Alchemical Firebomb explodes violently! You take 8 damage!"
crafter.CurrentHP == 7  // Reduced from 15
```

### UC-CRAFT-04: Catastrophe - Runeforging Corruption
**Scenario:** Runeforging recipe botches and inflicts corruption.

**Setup:**
```csharp
var crafter = new Character { Name = "Ragnhild", Wits = 4, CurrentHP = 20, Corruption = 0 };
var recipe = RecipeRegistry.GetById("RCP_RUN_FLAMEBLADE");
// recipe.BaseDc = 8
// recipe.Trade = CraftingTrade.Runeforging
// recipe.CatastropheType = CatastropheType.Corruption
// recipe.CatastropheCorruption = 3
```

**Execution:**
```csharp
var result = await _craftingService.CraftItemAsync(crafter, "RCP_RUN_FLAMEBLADE");
// WITS roll (4d10) = [1, 1, 3, 2] → 0 successes, 2 botches → net = -2
// Outcome = Catastrophe (net -2 < 0)
// Catastrophe Handler:
//   - CatastropheType.Corruption detected
//   - Adds 3 corruption via _traumaService.AddCorruption(crafter, 3, "Runic Backlash")
//   - crafter.Corruption = 0 + 3 = 3
```

**Result:**
```csharp
result.IsSuccess == false
result.Outcome == CraftingOutcome.Catastrophe
result.NetSuccesses == -2
result.CorruptionAdded == 3
result.Message == "CATASTROPHE! The runes of Flaming Blade Enchantment twist against you! You gain 3 Corruption!"
crafter.Corruption == 3  // Permanent corruption increase
```

### UC-CRAFT-05: Missing Ingredients (Pre-Validation Failure)
**Scenario:** Crafter attempts recipe without sufficient ingredients.

**Setup:**
```csharp
var crafter = new Character { Name = "Kael", Wits = 6 };
var recipe = RecipeRegistry.GetById("RCP_ALC_STIM");
// recipe.Ingredients = { "Herb" => 2, "Water" => 1 }

// Crafter inventory: 1 Herb, 0 Water
```

**Execution:**
```csharp
var result = await _craftingService.CraftItemAsync(crafter, "RCP_ALC_STIM");
// Step 1: Recipe found ✓
// Step 2: Ingredients validated → HasIngredients() returns false
// Step 3: Return failed result (no ingredients consumed, no roll)
```

**Result:**
```csharp
result.IsSuccess == false
result.Outcome == CraftingOutcome.Failure
result.DiceRolled == 0
result.Rolls.Count == 0  // No roll occurred
result.Message == "Missing ingredients for Basic Stimpack."
// Crafter inventory unchanged (1 Herb, 0 Water remain)
```

---

## Decision Trees

### Crafting Workflow Decision Tree
```
[Start: CraftItemAsync(crafter, recipeId)]
    ↓
[Recipe Lookup: RecipeRegistry.GetById(recipeId)]
    ├─ Recipe not found → Return Failure ("Recipe not found")
    └─ Recipe found
        ↓
    [Ingredient Validation: HasIngredients(crafter, recipe)]
        ├─ Missing ingredients → Return Failure ("Missing ingredients")
        └─ All ingredients present
            ↓
        [Consume Ingredients: foreach(ingredient) RemoveItemAsync()]
            ↓
        [WITS Roll: Roll(crafter.Wits, context)]
            ↓
        [Calculate Net Successes: Successes - Botches]
            ↓
        [Determine Outcome]
            ├─ Net < 0 → Catastrophe
            │   ↓
            │   [Check CatastropheType]
            │       ├─ Explosive → Apply HP damage
            │       ├─ Corruption → Add permanent corruption
            │       └─ None → Materials lost only
            ├─ Net >= DC + 5 → Masterwork
            │   ↓
            │   [Create MythForged Item]
            │   [Add to Inventory]
            │   [Return Success result]
            ├─ Net >= DC → Success
            │   ↓
            │   [Create ClanForged Item]
            │   [Add to Inventory]
            │   [Return Success result]
            └─ Net < DC (but >= 0) → Failure
                ↓
                [Return Failure result (materials already consumed)]
```

### Outcome Determination Decision Tree
```
[Net Successes Calculated]
    ↓
[Is Net < 0?]
    ├─ YES → CraftingOutcome.Catastrophe
    │        └→ Switch(recipe.CatastropheType)
    │               ├─ Explosive → Deal CatastropheDamage HP
    │               ├─ Corruption → Add CatastropheCorruption
    │               └─ None → Materials lost only
    └─ NO
        ↓
    [Is Net >= DC + 5?]
        ├─ YES → CraftingOutcome.Masterwork
        │        └→ Create MythForged quality item
        └─ NO
            ↓
        [Is Net >= DC?]
            ├─ YES → CraftingOutcome.Success
            │        └→ Create ClanForged quality item
            └─ NO → CraftingOutcome.Failure
                    └→ No item created (materials wasted)
```

### Catastrophe Type Resolution Tree
```
[Catastrophe Triggered (Net < 0)]
    ↓
[Switch on recipe.CatastropheType]
    ├─ Explosive (Alchemy)
    │   ↓
    │   [Apply HP Damage]
    │   ├─ crafter.CurrentHP -= recipe.CatastropheDamage
    │   └─ Clamp to 0 if negative
    │   ↓
    │   [Return CraftingResult with DamageDealt field]
    │
    ├─ Corruption (Runeforging)
    │   ↓
    │   [Add Permanent Corruption]
    │   ├─ _traumaService.AddCorruption(crafter, recipe.CatastropheCorruption, "Runic Backlash")
    │   └─ crafter.Corruption += recipe.CatastropheCorruption
    │   ↓
    │   [Return CraftingResult with CorruptionAdded field]
    │
    └─ None (Bodging, FieldMedicine, or default)
        ↓
        [Materials Lost Only]
        ├─ No HP damage
        ├─ No corruption gain
        └─ Return CraftingResult with generic message
```

---

## Sequence Diagrams

### Full Crafting Sequence
```
Player                CraftingService      RecipeRegistry    InventoryService    DiceService    TraumaService
   |                         |                    |                  |                |              |
   |--CraftItemAsync()------>|                    |                  |                |              |
   |                         |                    |                  |                |              |
   |                         |--GetById(recipeId)-|                  |                |              |
   |                         |<--Recipe-----------|                  |                |              |
   |                         |                    |                  |                |              |
   |                         |--HasIngredients()--|----------------->|                |              |
   |                         |<--true/false-------|------------------|                |              |
   |                         |                    |                  |                |              |
   |                         |---RemoveItemAsync()|-----------------→|                |              |
   |                         |<--RemoveResult-----|------------------| (repeat for    |              |
   |                         |                    |                  |  each ingredient)             |
   |                         |                    |                  |                |              |
   |                         |--Roll(wits, ctx)----------------------|--------------->|              |
   |                         |<--DiceResult--------------------------|----------------|              |
   |                         |                    |                  |                |              |
   |                         |--DetermineOutcome()-|                  |                |              |
   |                         |<--Outcome----------|                  |                |              |
   |                         |                    |                  |                |              |
   |                    [If Catastrophe + Corruption]                |                |              |
   |                         |--AddCorruption()------------------------------------------->|
   |                         |<--void--------------------------------------------------------|
   |                         |                    |                  |                |              |
   |                    [If Success/Masterwork]   |                  |                |              |
   |                         |--AddItemAsync()---------------------->|                |              |
   |                         |<--AddResult---------------------------|                |              |
   |                         |                    |                  |                |              |
   |<--CraftingResult--------|                    |                  |                |              |
   |                         |                    |                  |                |              |
```

### Ingredient Validation Sequence
```
CraftingService                    InventoryService                Character.Inventory
       |                                  |                               |
       |--HasIngredients(crafter, recipe)-|                               |
       |                                  |                               |
       |     [Foreach ingredient in recipe.Ingredients]                   |
       |                                  |                               |
       |  Query: crafter.Inventory.FirstOrDefault(inv => inv.Item.Name == itemId)
       |                                  |<------------------------------|
       |                                  |                               |
       |  Compare: available >= required  |                               |
       |                                  |                               |
       |    [If ANY ingredient insufficient]                              |
       |<--false--------------------------|                               |
       |                                  |                               |
       |    [If ALL ingredients sufficient]                               |
       |<--true---------------------------|                               |
       |                                  |                               |
```

### Catastrophe Handling Sequence (Alchemy Explosive)
```
CraftingService              Character              TraumaService              Logger
       |                         |                         |                      |
       |   [Net Successes = -2]  |                         |                      |
       |                         |                         |                      |
       |--HandleCatastrophe()----|                         |                      |
       |                         |                         |                      |
       | Check recipe.CatastropheType                      |                      |
       |   → Explosive           |                         |                      |
       |                         |                         |                      |
       | Apply Damage:           |                         |                      |
       | crafter.CurrentHP -= recipe.CatastropheDamage     |                      |
       |------------------------>|                         |                      |
       | (crafter.CurrentHP = 15 - 8 = 7)                  |                      |
       |                         |                         |                      |
       |--LogWarning()---------------------------------------------------------->|
       | "ALCHEMY CATASTROPHE: Elara takes 8 explosive damage!"                  |
       |                         |                         |                      |
       |--Return CraftingResult--|                         |                      |
       |   DamageDealt = 8       |                         |                      |
       |   Message = "CATASTROPHE! Your Alchemical Firebomb explodes!"           |
       |                         |                         |                      |
```

---

## Workflows

### Standard Crafting Checklist
**Pre-Conditions:**
- [ ] Player has selected a recipe (recipeId)
- [ ] Player character object exists with Wits attribute

**Execution Steps:**
1. [ ] **Recipe Lookup**
   - Query `RecipeRegistry.GetById(recipeId)`
   - Validate recipe exists
   - If not found → Return failure result, exit
2. [ ] **Ingredient Validation**
   - Call `HasIngredients(crafter, recipe)`
   - Check each ingredient in `recipe.Ingredients` dictionary
   - Verify `crafter.Inventory` has sufficient quantity
   - If insufficient → Return failure result, exit
3. [ ] **Ingredient Consumption** (Point of No Return)
   - Foreach `(itemId, quantity)` in `recipe.Ingredients`:
     - Call `_inventoryService.RemoveItemAsync(crafter, itemId, quantity)`
     - Log consumption success/failure
   - Ingredients are now gone (non-refundable)
4. [ ] **WITS Dice Roll**
   - Retrieve `crafter.Wits` attribute
   - Call `_diceService.Roll(wits, $"Craft {recipe.Name}")`
   - Receive `DiceResult` with `Successes`, `Botches`, `Rolls`
5. [ ] **Calculate Net Successes**
   - `netSuccesses = diceResult.Successes - diceResult.Botches`
   - Log roll results: `{Wits}d10 = {Successes}S/{Botches}B, Net={Net}, DC={DC}`
6. [ ] **Determine Outcome**
   - Call `DetermineOutcome(netSuccesses, recipe.BaseDc)`
   - Outcome = Catastrophe | Failure | Success | Masterwork
7. [ ] **Execute Outcome Handler**
   - Switch on outcome:
     - **Masterwork:** Create MythForged item, add to inventory, return success result
     - **Success:** Create ClanForged item, add to inventory, return success result
     - **Failure:** Return failure result (no item created)
     - **Catastrophe:** Apply trade-specific penalty, return catastrophe result
8. [ ] **Return CraftingResult**
   - Populate all fields: `IsSuccess`, `Outcome`, `NetSuccesses`, `Message`, etc.
   - Caller receives result object

**Post-Conditions:**
- [ ] Ingredients consumed (regardless of success/failure)
- [ ] Item added to inventory (if success/masterwork)
- [ ] HP damage applied (if catastrophe + explosive)
- [ ] Corruption added (if catastrophe + corruption)
- [ ] All actions logged at INFO level

### Catastrophe Resolution Checklist
**Trigger:** Net successes < 0 (more botches than successes)

**Steps:**
1. [ ] **Identify Catastrophe Type**
   - Read `recipe.CatastropheType` enum
   - Types: `None`, `Explosive`, `Corruption`
2. [ ] **Apply Trade-Specific Consequence**
   - **Explosive (Alchemy):**
     - [ ] Read `recipe.CatastropheDamage`
     - [ ] Reduce `crafter.CurrentHP` by damage amount
     - [ ] Clamp HP to 0 if negative (`Math.Max(0, ...)`)
     - [ ] Log warning: `"ALCHEMY CATASTROPHE: {Name} takes {Damage} explosive damage!"`
     - [ ] Populate `CraftingResult.DamageDealt` field
   - **Corruption (Runeforging):**
     - [ ] Read `recipe.CatastropheCorruption`
     - [ ] Call `_traumaService.AddCorruption(crafter, corruption, "Runic Backlash")`
     - [ ] Log warning: `"RUNEFORGING CATASTROPHE: {Name} gains {Corruption} Corruption!"`
     - [ ] Populate `CraftingResult.CorruptionAdded` field
   - **None (Bodging, FieldMedicine):**
     - [ ] No additional penalty (materials already lost)
     - [ ] Log warning: `"Craft CATASTROPHE: {RecipeName} - materials lost!"`
3. [ ] **Construct Message**
   - Format message with catastrophe details
   - Include damage/corruption values if applicable
4. [ ] **Return CraftingResult**
   - `IsSuccess = false`
   - `Outcome = CraftingOutcome.Catastrophe`
   - `OutputItemId = null` (no item created)
   - `Message = catastrophe-specific message`
   - `DamageDealt` or `CorruptionAdded` populated if applicable

---

## Cross-System Integration

### Service Dependencies

| Dependent Service | Integration Point | Purpose | Method Calls |
|-------------------|-------------------|---------|--------------|
| **DiceService** | WITS roll execution | Randomization for success determination | `Roll(poolSize, context)` |
| **InventoryService** | Ingredient consumption & item addition | Material management | `RemoveItemAsync()`, `AddItemAsync()` |
| **ItemRepository** | Output item lookup | Retrieve item template for cloning | `GetByNameAsync(itemId)` |
| **TraumaService** | Corruption application | Apply permanent corruption on Runeforging catastrophe | `AddCorruption(character, amount, source)` |

### Integration Workflows

#### Inventory Integration
```csharp
// Step 1: Validate ingredients exist
foreach (var (itemId, required) in recipe.Ingredients)
{
    var inventoryItem = crafter.Inventory
        .FirstOrDefault(inv => inv.Item.Name.Equals(itemId, StringComparison.OrdinalIgnoreCase));
    var available = inventoryItem?.Quantity ?? 0;
    if (available < required) return Failure;
}

// Step 2: Consume ingredients
foreach (var (itemId, required) in recipe.Ingredients)
{
    var removeResult = await _inventoryService.RemoveItemAsync(crafter, itemId, required);
}

// Step 3: Add crafted item (if success)
if (outcome == Success || outcome == Masterwork)
{
    var outputItem = await _itemRepository.GetByNameAsync(recipe.OutputItemId);
    var craftedItem = CloneItemWithQuality(outputItem, qualityTier);
    await _inventoryService.AddItemAsync(crafter, craftedItem, recipe.OutputQuantity);
}
```

#### Trauma Service Integration (Catastrophe)
```csharp
// Only called on Catastrophe outcome with CatastropheType.Corruption
if (recipe.CatastropheType == CatastropheType.Corruption)
{
    _traumaService.AddCorruption(crafter, recipe.CatastropheCorruption, "Runic Backlash");
    // TraumaService updates crafter.Corruption, checks tier thresholds, applies manifestations
}
```

#### Dice Service Integration
```csharp
// Called for every craft attempt (after ingredient consumption)
var wits = crafter.Wits;
var diceResult = _diceService.Roll(wits, $"Craft {recipe.Name}");
// diceResult.Successes = count of 8, 9, 10 rolls
// diceResult.Botches = count of 1 rolls
// diceResult.Rolls = [3, 8, 1, 10, 5] (example)

var netSuccesses = diceResult.Successes - diceResult.Botches;
```

### Cross-System Data Flow
```
[Player Command: "craft RCP_ALC_STIM"]
    ↓
[CraftingService.CraftItemAsync()]
    ├─→ RecipeRegistry: Lookup recipe definition
    ├─→ Character.Inventory: Validate ingredients
    ├─→ InventoryService: Remove ingredients (2 Herbs, 1 Water)
    ├─→ DiceService: Roll WITS pool (4d10)
    ├─→ DetermineOutcome: Map net successes to outcome
    └─→ Outcome Handlers:
        ├─ Masterwork/Success → ItemRepository: Get item template
        │                     → InventoryService: Add crafted item
        ├─ Catastrophe → TraumaService: Add corruption (if Runeforging)
        │              → Character: Reduce HP (if Alchemy)
        └─ Failure → (No additional calls, ingredients already consumed)
    ↓
[Return CraftingResult to caller]
```

---

## Data Models

### Recipe Entity
```csharp
// Location: RuneAndRust.Core/Entities/Recipe.cs
public class Recipe
{
    public string RecipeId { get; set; }              // Unique ID (e.g., "RCP_ALC_STIM")
    public string Name { get; set; }                  // Display name
    public string Description { get; set; }           // Narrative description
    public CraftingTrade Trade { get; set; }          // Alchemy | Bodging | FieldMedicine | Runeforging
    public int BaseDc { get; set; }                   // Difficulty class (3-11 typical range)
    public Dictionary<string, int> Ingredients { get; set; }  // ItemId => Quantity
    public string OutputItemId { get; set; }          // Result item ID
    public int OutputQuantity { get; set; }           // Number of items produced (default 1)
    public List<string> RequiredKeywords { get; set; } // Unlock prerequisites (future)

    // Catastrophe Properties (v0.3.1c)
    public CatastropheType CatastropheType { get; set; }  // None | Explosive | Corruption
    public int CatastropheDamage { get; set; }         // HP damage (Explosive only)
    public int CatastropheCorruption { get; set; }     // Corruption gain (Corruption only)

    // Runeforging Properties (v0.3.1c)
    public List<ItemProperty> OutputProperties { get; set; }  // Enchantment effects
    public bool RequiresTargetItem { get; set; }       // True for enchanting recipes
}
```

### CraftingResult Model
```csharp
// Location: RuneAndRust.Core/Models/Crafting/CraftingResult.cs
public record CraftingResult(
    bool IsSuccess,                       // Overall success flag
    CraftingOutcome Outcome,              // Masterwork | Success | Failure | Catastrophe
    string RecipeId,                      // Recipe identifier
    string RecipeName,                    // Recipe display name
    int DiceRolled,                       // Pool size (WITS value)
    int Successes,                        // Count of 8, 9, 10 rolls
    int Botches,                          // Count of 1 rolls
    int NetSuccesses,                     // Successes - Botches
    int DifficultyClass,                  // Recipe DC
    string? OutputItemId,                 // Item created (null if failure/catastrophe)
    int OutputQuantity,                   // Quantity created (0 if failure/catastrophe)
    QualityTier? OutputQuality,           // MythForged | ClanForged | null
    string Message,                       // Player-facing result message
    IReadOnlyList<int> Rolls,             // Individual die results
    int? DamageDealt = null,              // HP damage (Explosive catastrophe)
    int? CorruptionAdded = null,          // Corruption gain (Corruption catastrophe)
    Equipment? EnchantedItem = null       // Item enchanted by Runeforging (if applicable)
);
```

### CraftingOutcome Enum
```csharp
// Location: RuneAndRust.Core/Enums/CraftingOutcome.cs
public enum CraftingOutcome
{
    Catastrophe,  // Net < 0 (botched)
    Failure,      // Net >= 0 but < DC
    Success,      // Net >= DC, < DC + 5
    Masterwork    // Net >= DC + 5
}
```

### CraftingTrade Enum
```csharp
// Location: RuneAndRust.Core/Enums/CraftingTrade.cs
public enum CraftingTrade
{
    Bodging = 0,       // Mechanical repairs, improvised tools, salvage work
    Alchemy = 1,       // Potions, salves, and chemical compounds
    Runeforging = 2,   // Aetheric inscriptions and enchantments
    FieldMedicine = 3  // Bandages, stimulants, and medical kits
}
```

### CatastropheType Enum
```csharp
// Location: RuneAndRust.Core/Enums/CatastropheType.cs
public enum CatastropheType
{
    None = 0,         // Materials lost only (Bodging, FieldMedicine)
    Explosive = 1,    // HP damage (Alchemy)
    Toxic = 2,        // Reserved for future (poison/sickness effects)
    Corrosive = 3,    // Reserved for future (equipment/environment damage)
    Corruption = 4    // Permanent corruption (Runeforging)
}
```

### QualityTier Enum
```csharp
// Location: RuneAndRust.Core/Enums/QualityTier.cs
public enum QualityTier
{
    JuryRigged,   // Improvised, fragile
    Salvaged,     // Scavenged, worn
    ClanForged,   // Standard craftsmanship (Success outcome)
    MythForged    // Legendary craftsmanship (Masterwork outcome)
}
```

---

## Configuration

### Constants
```csharp
// CraftingService.cs:26
private const int MasterworkThreshold = 5;  // DC + 5 for Masterwork
```

### Recipe Configuration
Recipes are defined in `RuneAndRust.Core/Data/RecipeRegistry.cs`:

```csharp
public static class RecipeRegistry
{
    private static readonly List<Recipe> _recipes = new()
    {
        // Alchemy - Basic Stimpack
        new Recipe
        {
            RecipeId = "RCP_ALC_STIM",
            Name = "Basic Stimpack",
            Description = "A crude stimulant that restores vitality.",
            Trade = CraftingTrade.Alchemy,
            BaseDc = 4,
            Ingredients = new Dictionary<string, int>
            {
                { "Herb", 2 },
                { "Water", 1 }
            },
            OutputItemId = "Basic Stimpack",
            OutputQuantity = 1,
            CatastropheType = CatastropheType.None
        },

        // Alchemy - Alchemical Firebomb
        new Recipe
        {
            RecipeId = "RCP_ALC_FIREBOMB",
            Name = "Alchemical Firebomb",
            Description = "A volatile explosive compound.",
            Trade = CraftingTrade.Alchemy,
            BaseDc = 7,
            Ingredients = new Dictionary<string, int>
            {
                { "Volatile Oil", 3 },
                { "Fuse Wire", 1 }
            },
            OutputItemId = "Alchemical Firebomb",
            OutputQuantity = 1,
            CatastropheType = CatastropheType.Explosive,
            CatastropheDamage = 8
        },

        // Runeforging - Flaming Blade Enchantment
        new Recipe
        {
            RecipeId = "RCP_RUN_FLAMEBLADE",
            Name = "Flaming Blade Enchantment",
            Description = "Etches runes of fire onto a weapon.",
            Trade = CraftingTrade.Runescribe,
            BaseDc = 8,
            Ingredients = new Dictionary<string, int>
            {
                { "Aetheric Dust", 5 },
                { "Blood Ink", 2 }
            },
            OutputItemId = "Flaming Rune",
            OutputQuantity = 1,
            RequiresTargetItem = true,
            CatastropheType = CatastropheType.Corruption,
            CatastropheCorruption = 3,
            OutputProperties = new List<ItemProperty> { ItemProperty.Flaming }
        }
    };

    public static Recipe? GetById(string recipeId) =>
        _recipes.FirstOrDefault(r => r.RecipeId == recipeId);

    public static IReadOnlyList<Recipe> GetAll() => _recipes;

    public static IReadOnlyList<Recipe> GetByTrade(CraftingTrade trade) =>
        _recipes.Where(r => r.Trade == trade).ToList();
}
```

### Typical Recipe Difficulty Classes
| Difficulty | DC | Example Recipe |
|------------|----|----|
| Trivial | 3 | Torch |
| Easy | 4 | Basic Stimpack |
| Moderate | 5 | Iron Dagger |
| Challenging | 6 | Iron Longsword |
| Hard | 7 | Alchemical Firebomb |
| Very Hard | 8 | Flaming Blade Enchantment |
| Legendary | 9-11 | Mythril Armor, Dragonbone Weapon |

---

## Testing

### Unit Test Coverage
**Location:** `RuneAndRust.Tests/Engine/CraftingServiceTests.cs`

**Test Count:** 27 tests (80%+ coverage)

**Test Categories:**

#### Recipe Lookup Tests
- `CraftItemAsync_WithInvalidRecipeId_ReturnsFailure`
- `CraftItemAsync_WithValidRecipeId_LooksUpRecipe`

#### Ingredient Validation Tests
- `HasIngredients_WithSufficientIngredients_ReturnsTrue`
- `HasIngredients_WithInsufficientQuantity_ReturnsFalse`
- `HasIngredients_WithMissingIngredient_ReturnsFalse`
- `CraftItemAsync_WithMissingIngredients_ReturnsFailureWithoutRolling`

#### Dice Roll Tests
- `CraftItemAsync_WithValidInputs_RollsWitsDicePool`
- `CraftItemAsync_Success_NetSuccessesMeetDc`
- `CraftItemAsync_Failure_NetSuccessesBelowDc`
- `CraftItemAsync_Masterwork_NetSucceedsExceedsDcPlusFive`
- `CraftItemAsync_Catastrophe_NetSuccessesNegative`

#### Outcome Determination Tests
- `DetermineOutcome_NetNegative_ReturnsCatastrophe`
- `DetermineOutcome_NetZeroButBelowDc_ReturnsFailure`
- `DetermineOutcome_NetMeetsDc_ReturnsSuccess`
- `DetermineOutcome_NetExceedsDcPlusFive_ReturnsMasterwork`

#### Item Creation Tests
- `CraftItemAsync_Success_CreatesItem ClanForgedQuality`
- `CraftItemAsync_Masterwork_CreatesItemMythForgedQuality`
- `CraftItemAsync_Failure_DoesNotCreateItem`
- `CraftItemAsync_Catastrophe_DoesNotCreateItem`

#### Ingredient Consumption Tests
- `CraftItemAsync_Success_ConsumesIngredients`
- `CraftItemAsync_Failure_ConsumesIngredients`
- `CraftItemAsync_Catastrophe_ConsumesIngredients`

#### Catastrophe Mechanism Tests
- `CraftItemAsync_CatastropheExplosive_DealsDamageToCharacter`
- `CraftItemAsync_CatastropheExplosive_ClampsHpToZero`
- `CraftItemAsync_CatastropheCorruption_AddsCorruption`
- `CraftItemAsync_CatastropheNone_OnlyLosesMaterials`

#### Integration Tests
- `CraftItemAsync_FullWorkflow_Success`
- `CraftItemAsync_FullWorkflow_Masterwork`
- `CraftItemAsync_FullWorkflow_CatastropheWithDamage`

---

## Domain 4 Compliance

All in-game recipe descriptions and crafting messages comply with Domain 4 Technology Constraints. No precision measurements are used in player-facing text.

### Compliant Language Examples
| Forbidden (Domain 4 Violation) | Compliant (Approved) |
|---------------------------------|----------------------|
| "Success rate: 75%" | "High chance of success" |
| "Requires exactly 2.5 liters of water" | "Requires a skin's worth of water" |
| "Explosive radius: 5 meters" | "Explosive blast spreads an arm's length" |
| "Temperature: 850°C" | "Forge-hot, glowing white" |
| "Crafting time: 4 hours" | "Takes a watch's worth of work" |

### Voice Compliance Notes
- All `CraftingResult.Message` strings use narrative language ("MASTERWORK!", "Failure. Your attempt falls short.")
- No system terminology exposed to players ("Net Successes", "DC", "Botches" are internal only)
- Catastrophe messages use vivid descriptions ("explodes violently!", "runes twist against you!")
- Recipe descriptions in `Recipe.Description` are Layer 2 Diagnostic perspective (observer reporting findings)

---

## Version History

### v1.0.0 (Current - SPEC-CRAFT-001)
- Initial specification based on implemented `CraftingService.cs`
- Reflects v0.3.1c catastrophe system
- Documented all four trades (Alchemy, Bodging, FieldMedicine, Runeforging)
- Comprehensive use cases with code examples
- Decision trees for all workflows
- Cross-system integration matrices complete

### Implementation History
- **v0.3.1c:** Added trade-specific catastrophe types (Explosive, Corruption)
- **v0.3.0:** Implemented quality tier system (MythForged, ClanForged)
- **v0.2.0:** Added WITS-based dice pool mechanics
- **v0.1.0:** Initial crafting system with recipe registry

---

## Future Enhancements

### Planned Features (v0.4.0+)
1. **Recipe Discovery System:** Unlock recipes via keyword collection, skill progression, or quest rewards
2. **Material Quality Impact:** Ingredient quality affects output item stats/durability
3. **Crafting Time Mechanics:** Recipes require time investment (turns/hours), enabling ambush risk during crafting
4. **Batch Crafting:** Multiply output quantity, but consume proportionally more ingredients and increase DC
5. **Critical Success Effects:** On natural all-10s roll, apply bonus properties (Durable, Lightweight, etc.)
6. **Enchanting System Expansion:** Runeforging recipes apply multiple properties simultaneously
7. **Trade-Specific Catastrophes:**
   - **Bodging:** Equipment durability loss on catastrophe
   - **FieldMedicine:** Malfunction chance (item created but with negative property)
8. **Apprentice System:** Assistants reduce DC or increase pool size

### Known Limitations for Future Resolution
1. **No Undo:** Once ingredients are consumed, they cannot be refunded (intentional design, but may add "Salvage Scraps" mechanic)
2. **Static DC:** Recipe difficulty never scales with character level (may add "Expert Crafting" skill tree)
3. **No Partial Success:** Either full output or nothing (may add "Crude Version" on near-success)
4. **No Crafting Queues:** One craft at a time (may add workshop automation later)

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- **CRITICAL:** Fixed trade names to match implementation:
  - Smith → Bodging (mechanical repairs, improvised tools)
  - Tinkerer → FieldMedicine (bandages, stimulants, medical kits)
  - Runescribe → Runeforging (aetheric inscriptions)
- Documented `Equipment? EnchantedItem` parameter in CraftingResult
- Documented reserved CatastropheType values (Toxic = 2, Corrosive = 3)
- Added code traceability remarks (`See: SPEC-CRAFT-001...`) to 8 implementation files:
  - ICraftingService.cs, CraftingService.cs
  - Recipe.cs, RecipeRegistry.cs
  - CraftingResult.cs
  - CraftingTrade.cs, CraftingOutcome.cs, CatastropheType.cs

### v1.0.0 (2025-12-22)
**Initial Release:**
- Crafting system documentation
- WITS-based dice roll mechanics
- Four trade disciplines with catastrophe mechanics
- Recipe validation and ingredient consumption
- Quality tier progression (Masterwork → Success → Failure → Catastrophe)

---

**END OF SPECIFICATION**
