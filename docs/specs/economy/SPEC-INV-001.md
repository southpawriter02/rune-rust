---
id: SPEC-INV-001
title: Inventory & Equipment System
version: 1.0.1
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-CHAR-001, SPEC-REST-001, SPEC-CRAFT-001, SPEC-REPAIR-001]
---

# SPEC-INV-001: Inventory & Equipment System

> **Version:** 1.0.1
> **Status:** Implemented (v0.3.0)
> **Service:** `InventoryService`, `LootService`
> **Location:** `RuneAndRust.Engine/Services/`

---

## Overview

The Inventory & Equipment System manages item storage, equipment slots, burden calculation, and loot generation. It integrates with character stats through equipment bonuses and affects gameplay through burden states that limit mobility and combat effectiveness.

---

## Core Concepts

### Inventory
- Character's carried items (pack)
- Weight-based capacity (`MIGHT × 10,000g`)
- Stackable and non-stackable items

### Equipment
- Items equipped in slots
- Provide attribute bonuses and combat stats
- Have durability and requirements

### Burden
- Weight ratio affects character
- Three states: Light, Heavy, Overburdened

---

## Equipment Slots

| Slot | Value | Description | Typical Items |
|------|-------|-------------|---------------|
| MainHand | 0 | Primary weapon | Swords, axes, staves |
| OffHand | 1 | Secondary/shield | Shields, torches, secondary weapons |
| Head | 2 | Headgear | Helms, hoods, goggles |
| Body | 3 | Torso armor | Plate, leather, robes |
| Hands | 4 | Hand protection | Gauntlets, gloves, bracers |
| Feet | 5 | Footwear | Boots, sandals, sabatons |
| Accessory | 6 | Miscellaneous | Rings, amulets, belts |

---

## Behaviors

### Primary Behaviors

#### 1. Add Item (`AddItemAsync`)

```csharp
Task<InventoryResult> AddItemAsync(Character character, Item item, int quantity = 1)
```

**Sequence:**
1. Validate quantity > 0
2. Check for existing inventory entry
3. If exists and stackable → add to stack (check max stack)
4. If exists and not stackable → reject
5. If new → create inventory entry at next slot
6. Save changes

**Stack Handling:**
```csharp
if (item.IsStackable)
{
    var newQuantity = existingEntry.Quantity + quantity;
    if (newQuantity > item.MaxStackSize)
    {
        return new InventoryResult(false, $"Cannot stack more than {item.MaxStackSize}");
    }
    existingEntry.Quantity = newQuantity;
}
```

#### 2. Remove Item (`RemoveItemAsync`)

```csharp
Task<InventoryResult> RemoveItemAsync(Character character, string itemName, int quantity = 1)
```

**Sequence:**
1. Find item by name
2. Validate sufficient quantity
3. If removing all → delete entry
4. If removing partial → decrement quantity
5. Save changes

#### 3. Drop Item (`DropItemAsync`)

```csharp
Task<InventoryResult> DropItemAsync(Character character, string itemName)
```

**Restrictions:**
- Cannot drop equipped items (must unequip first)
- Cannot drop Key Items (`ItemType.KeyItem`)

#### 4. Equip Item (`EquipItemAsync`)

```csharp
Task<InventoryResult> EquipItemAsync(Character character, string itemName)
```

**Sequence:**
1. Find item in inventory
2. Validate item is Equipment type
3. Validate not already equipped
4. Check attribute requirements
5. If slot occupied → auto-unequip current
6. Set IsEquipped = true
7. Recalculate equipment bonuses

**Requirement Checking:**
```csharp
if (!equipment.MeetsRequirements(character))
{
    return new InventoryResult(false, "You don't meet the requirements");
}
```

#### 5. Unequip Item (`UnequipItemAsync`, `UnequipSlotAsync`)

```csharp
Task<InventoryResult> UnequipItemAsync(Character character, string itemName)
Task<InventoryResult> UnequipSlotAsync(Character character, EquipmentSlot slot)
```

**Sequence:**
1. Find equipped item (by name or slot)
2. Set IsEquipped = false
3. Recalculate equipment bonuses

#### 6. Calculate Burden (`CalculateBurdenAsync`)

```csharp
Task<BurdenState> CalculateBurdenAsync(Character character)
```

**Formula:**
```csharp
var ratio = (double)currentWeight / maxCapacity;

return ratio switch
{
    >= 0.9 => BurdenState.Overburdened,  // 90%+
    >= 0.7 => BurdenState.Heavy,          // 70-89%
    _ => BurdenState.Light                // 0-69%
};
```

#### 7. Recalculate Equipment Bonuses (`RecalculateEquipmentBonusesAsync`)

```csharp
Task RecalculateEquipmentBonusesAsync(Character character)
```

**Sequence:**
1. Clear existing `EquipmentBonuses` dictionary
2. Iterate all equipped items
3. Sum attribute bonuses from each equipment
4. Apply Heavy burden penalty (-2 FINESSE) if applicable

**Bonus Aggregation:**
```csharp
foreach (var entry in equippedItems)
{
    if (entry.Item is Equipment equipment)
    {
        foreach (var bonus in equipment.AttributeBonuses)
        {
            if (character.EquipmentBonuses.ContainsKey(bonus.Key))
                character.EquipmentBonuses[bonus.Key] += bonus.Value;
            else
                character.EquipmentBonuses[bonus.Key] = bonus.Value;
        }
    }
}
```

#### 8. Find Item by Tag (`FindItemByTagAsync`)

```csharp
Task<InventoryItem?> FindItemByTagAsync(Character character, string tag)
```

**Usage:** Used by RestService to find Ration and Water items.

---

## Burden States

| State | Weight Ratio | Effects |
|-------|--------------|---------|
| **Light** | 0-69% | No penalties |
| **Heavy** | 70-89% | -2 FINESSE (applied via equipment bonuses) |
| **Overburdened** | 90-100% | Cannot move (room transition blocked) |

---

## Restrictions

### Item Management
1. **No negative quantities** - Prevented at add/remove
2. **Stack limits enforced** - Cannot exceed `MaxStackSize`
3. **Equipped items immovable** - Must unequip before drop

### Equipment
1. **Type requirement** - Only Equipment subtype can equip
2. **Slot exclusivity** - One item per slot
3. **Attribute requirements** - Must meet minimums

### Burden
1. **Movement blocked at Overburdened** - `CanMoveAsync()` returns false
2. **Finesse penalty at Heavy** - Automatic equipment recalc

---

## Limitations

### Numerical Bounds
| Constraint | Value | Notes |
|------------|-------|-------|
| Capacity formula | `MIGHT × 10,000g` | In grams |
| Heavy threshold | 70% | Of capacity |
| Overburdened threshold | 90% | Of capacity |
| Heavy FINESSE penalty | -2 | Applied to bonuses |
| Default max stack | 99 | Per item type |

### System Gaps
- No sort/organize inventory
- No item comparison UI
- No equipment preview before equip
- No set bonuses

---

## Use Cases

### UC-1: Loot Pickup
```csharp
var item = lootService.GenerateItem(context);
var result = await inventoryService.AddItemAsync(character, item);

if (!result.Success)
{
    // "Your pack is full" or "Cannot stack more"
}
```

### UC-2: Equipment Swap
```csharp
// Player wants to equip new sword
var result = await inventoryService.EquipItemAsync(character, "Iron Greatsword");

// Automatically unequips current MainHand weapon
// result.Message = "You equip Iron Greatsword, replacing Rusty Blade."
```

### UC-3: Supply Consumption (Rest)
```csharp
var ration = await inventoryService.FindItemByTagAsync(character, "Ration");
if (ration != null)
{
    await inventoryService.RemoveItemAsync(character, ration.Item.Name, 1);
}
```

### UC-4: Burden Check Before Move
```csharp
if (!await inventoryService.CanMoveAsync(character))
{
    return "You are carrying too much to move. Drop some items.";
}
// Proceed with room transition
```

---

## Cross-Links

### Dependencies (Consumes)
| Service | Specification | Usage |
|---------|---------------|-------|
| `IInventoryRepository` | Infrastructure | Persistence |
| `ILogger` | Infrastructure | Operation tracing |

### Dependents (Provides To)
| Service | Specification | Usage |
|---------|---------------|-------|
| `RestService` | [SPEC-REST-001](SPEC-REST-001.md) | Supply consumption |
| `StatCalculationService` | [SPEC-CHAR-001](SPEC-CHAR-001.md) | Equipment bonus integration |
| `NavigationService` | [SPEC-NAV-001](SPEC-NAV-001.md) | Burden movement check |
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Weapon/armor stats |

---

## Related Services

### Primary Implementation
| File | Purpose |
|------|---------|
| `InventoryService.cs` | Item management |
| `LootService.cs` | Loot generation |
| `LootTables.cs` | Quality-tiered drop tables |

### Supporting Types
| File | Purpose |
|------|---------|
| `InventoryItem.cs` | Join entity |
| `Item.cs` | Base item entity |
| `Equipment.cs` | Equipable item subtype |
| `InventoryResult.cs` | Operation result |
| `BurdenState.cs` | Burden enum |

---

## Data Models

### InventoryItem (Join Entity)
```csharp
public class InventoryItem
{
    public Guid CharacterId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public int SlotPosition { get; set; }
    public bool IsEquipped { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime LastModified { get; set; }

    // Navigation
    public Character Character { get; set; }
    public Item Item { get; set; }
}
```

### Item Entity
```csharp
public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ItemType ItemType { get; set; }
    public string Description { get; set; }
    public string? DetailedDescription { get; set; }
    public int Weight { get; set; }           // In grams
    public int Value { get; set; }            // In Scrip
    public QualityTier Quality { get; set; }
    public bool IsStackable { get; set; }
    public int MaxStackSize { get; set; } = 99;
    public List<string> Tags { get; set; } = new();
}
```

### Equipment Entity
```csharp
public class Equipment : Item
{
    public EquipmentSlot Slot { get; set; }
    public Dictionary<Attribute, int> AttributeBonuses { get; set; } = new();
    public int MaxDurability { get; set; } = 100;
    public int CurrentDurability { get; set; } = 100;
    public int? SoakBonus { get; set; }
    public int? DamageDie { get; set; }
    public Dictionary<Attribute, int> Requirements { get; set; } = new();

    public bool MeetsRequirements(Character character)
    {
        foreach (var req in Requirements)
        {
            if (character.GetEffectiveAttribute(req.Key) < req.Value)
                return false;
        }
        return true;
    }
}
```

### InventoryResult
```csharp
public record InventoryResult(bool Success, string Message);
```

### BurdenState Enum
```csharp
public enum BurdenState
{
    Light,       // 0-69% capacity
    Heavy,       // 70-89% capacity
    Overburdened // 90-100% capacity
}
```

---

## Quality Tiers

| Tier | Value | Description | Typical Stats |
|------|-------|-------------|---------------|
| JuryRigged | 0 | Improvised, fragile | Low stats, poor durability |
| Scavenged | 1 | Standard salvaged goods | Moderate stats |
| ClanForged | 2 | Dvergr-crafted quality | Standard stats |
| Optimized | 3 | Pre-Glitch or masterwork | Good stats, bonuses |
| MythForged | 4 | Legendary, enhanced | High stats, special effects |

---

## Configuration

### Burden Thresholds
```csharp
private const double HeavyBurdenThreshold = 0.7;
private const double OverburdenedThreshold = 0.9;
```

### Capacity Calculation
```csharp
private const int GramsPerMight = 10000;

public int GetMaxCapacity(Character character)
{
    var effectiveMight = character.GetEffectiveAttribute(Attribute.Might);
    return effectiveMight * GramsPerMight;
}
```

### Heavy Burden Penalty
```csharp
// Applied during RecalculateEquipmentBonusesAsync
if (burden == BurdenState.Heavy)
{
    character.EquipmentBonuses[Attribute.Finesse] =
        character.EquipmentBonuses.GetValueOrDefault(Attribute.Finesse, 0) - 2;
}
```

---

## Testing

### Test Files
- `InventoryServiceTests.cs`
- `LootServiceTests.cs`

### Critical Test Scenarios
1. Add stackable item (new and existing)
2. Add non-stackable duplicate (rejection)
3. Stack limit enforcement
4. Remove partial stack
5. Remove entire stack
6. Equip/unequip cycle
7. Auto-unequip on slot conflict
8. Requirement checking
9. Burden calculation at all thresholds
10. Heavy burden penalty application
11. Overburdened movement block
12. Tag-based item lookup

---

## Design Rationale

### Why Weight-Based Capacity?
- More realistic than slot-based
- MIGHT attribute relevance for non-combat
- Encourages meaningful item choices

### Why Automatic Unequip on Slot Conflict?
- Reduces tedious unequip commands
- Common RPG convention
- Smoother UX flow

### Why Tag-Based Supply Lookup?
- Flexibility for item variants
- Cleaner than hardcoded names
- Extensible without code changes

### Why FINESSE Penalty at Heavy?
- Agility reduced by weight (realistic)
- Affects Defense Score (gameplay impact)
- Encourages weight management

### Why Movement Block at Overburdened?
- Clear consequence for overloading
- Forces decision making
- Prevents trivial inventory abuse

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- **CRITICAL:** Fixed Equipment Slots table:
  - Removed "Legs" slot (never implemented)
  - Added "Hands" slot (position 4) for gauntlets, gloves, bracers
  - Added "Accessory" slot (position 6) for rings, amulets, belts
- **CRITICAL:** Fixed QualityTier names to match implementation:
  - Salvaged → Scavenged
  - Functional → ClanForged
  - Refined → Optimized
- Added code traceability remarks to 6 implementation files:
  - IInventoryService.cs, InventoryService.cs
  - ILootService.cs, LootService.cs
  - Equipment.cs, InventoryItem.cs

### v1.0.0 (2025-12-22)
**Initial Release:**
- Inventory & Equipment system documentation
- Burden calculation (Light, Heavy, Overburdened)
- Weight-based capacity (MIGHT × 10,000g)
- Equipment slots and attribute bonuses
- Loot generation system

---

**END OF SPECIFICATION**
