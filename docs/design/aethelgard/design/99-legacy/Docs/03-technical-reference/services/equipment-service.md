# EquipmentService

> **File:** `RuneAndRust.Engine/EquipmentService.cs`
> **Version:** v0.3+
> **Purpose:** Manages equipment, inventory operations, and stat calculations

## Overview

`EquipmentService` handles all equipment-related operations including:
- Equipping and unequipping weapons and armor
- Inventory management (add, remove, pickup, drop)
- Stat recalculation when equipment changes
- Equipment comparison for upgrade analysis
- Attribute bonus calculations from equipment

## Dependencies

None - `EquipmentService` is a standalone service with no external dependencies.

```csharp
public class EquipmentService
{
    // No constructor dependencies
}
```

## Equipment System

### Quality Tiers

Equipment quality follows the Aethelgard Saga System:

| Tier | Name | Description |
|------|------|-------------|
| 0 | Jury-Rigged | Scrap held together with hope and wire |
| 1 | Scavenged | Salvaged from ruins, functional but worn |
| 2 | Clan-Forged | Crafted by survivor communities, solid work |
| 3 | Optimized | Pre-Glitch tech, carefully maintained |
| 4 | Myth-Forged | Legendary artifacts, touched by the Blight |

### Equipment Types

```csharp
public enum EquipmentType
{
    Weapon,
    Armor,
    Accessory  // v0.16
}
```

### Weapon Categories

| Category | Attribute | Description |
|----------|-----------|-------------|
| `Axe` | MIGHT | Warrior weapon |
| `Greatsword` | MIGHT | Two-handed warrior weapon |
| `Spear` | FINESSE | Reach weapon |
| `Dagger` | FINESSE | Fast weapon |
| `Staff` | WILL | Mystic, Aether cost reduction |
| `Focus` | WILL | Mystic, ability enhancement |
| `Blade` | Varies | One-handed blade (v0.16) |
| `Blunt` | Varies | One-handed blunt (v0.16) |
| `EnergyMelee` | Varies | Shock-baton, plasma cutter (v0.16) |
| `Rifle` | Varies | Ranged firearm (v0.16) |
| `HeavyBlunt` | Varies | Two-handed blunt (v0.16) |

### Armor Categories

| Category | Trade-offs |
|----------|------------|
| `Light` | Low HP, low defense, +FINESSE, +Evasion |
| `Medium` | Balanced stats |
| `Heavy` | High HP, high defense, -FINESSE, +STURDINESS |

---

## Public Methods

### Equipment Management

#### EquipWeapon

```csharp
public bool EquipWeapon(PlayerCharacter player, Equipment weapon)
```

Equips a weapon from inventory.

**Behavior:**
1. Validates item is a weapon
2. Removes weapon from inventory
3. If current weapon equipped, moves to inventory (if space)
4. Sets new weapon as equipped
5. Recalculates player stats

**Returns:** `true` if successful, `false` if item is not a weapon

---

#### EquipArmor

```csharp
public bool EquipArmor(PlayerCharacter player, Equipment armor)
```

Equips armor from inventory.

**Behavior:**
1. Validates item is armor
2. Removes armor from inventory
3. If current armor equipped, moves to inventory (if space)
4. Sets new armor as equipped
5. Recalculates player stats

**Returns:** `true` if successful, `false` if item is not armor

---

#### UnequipWeapon

```csharp
public bool UnequipWeapon(PlayerCharacter player)
```

Unequips weapon to inventory.

**Returns:** `false` if no weapon equipped or inventory full

---

#### UnequipArmor

```csharp
public bool UnequipArmor(PlayerCharacter player)
```

Unequips armor to inventory.

**Returns:** `false` if no armor equipped or inventory full

---

### Inventory Management

#### AddToInventory

```csharp
public bool AddToInventory(PlayerCharacter player, Equipment item)
```

Adds item to player inventory.

**Returns:** `false` if inventory is full (`player.Inventory.Count >= player.MaxInventorySize`)

---

#### RemoveFromInventory

```csharp
public bool RemoveFromInventory(PlayerCharacter player, Equipment item)
```

Removes item from inventory.

**Returns:** `true` if item was found and removed

---

#### PickupItem

```csharp
public bool PickupItem(PlayerCharacter player, Room room, Equipment item)
```

Picks up item from room ground to inventory.

**Behavior:**
1. Validates item is on ground
2. Validates inventory has space
3. Removes from `room.ItemsOnGround`
4. Adds to `player.Inventory`

---

#### DropItem

```csharp
public bool DropItem(PlayerCharacter player, Room room, Equipment item)
```

Drops item from inventory to room ground.

**Behavior:**
1. Validates item is in inventory
2. Removes from `player.Inventory`
3. Adds to `room.ItemsOnGround`

---

### Item Search

#### FindInInventory

```csharp
public Equipment? FindInInventory(PlayerCharacter player, string itemName)
```

Finds item in inventory by name (case-insensitive, partial match).

```csharp
var sword = equipmentService.FindInInventory(player, "sword");
// Matches "Iron Sword", "Rusty Sword", etc.
```

---

#### FindOnGround

```csharp
public Equipment? FindOnGround(Room room, string itemName)
```

Finds item on room ground by name (case-insensitive, partial match).

---

### Stat Calculations

#### RecalculatePlayerStats

```csharp
public void RecalculatePlayerStats(PlayerCharacter player)
```

Recalculates all player stats based on equipped items.

**Calculations:**

1. **MaxHP:**
   ```
   BaseHP = ClassBase + (Milestone × 10)

   Class bases:
   - Warrior: 50
   - Skirmisher: 40
   - Adept: 35
   - Mystic: 30

   Final MaxHP = BaseHP × WarriorsVigor(1.10) + ArmorHPBonus
   ```

2. **HP Preservation:**
   - Maintains HP percentage after stat change
   - `newHP = min(MaxHP × oldHPRatio, newMaxHP)`

3. **MaxAP (Mystic only, v0.19.8):**
   ```
   BaseMaxAP = (WILL × 10) + 50
   Final MaxAP = BaseMaxAP × AethericAttunement(1.10)
   ```

---

#### GetEffectiveAttributeValue

```csharp
public int GetEffectiveAttributeValue(PlayerCharacter player, string attributeName)
```

Gets attribute value including equipment bonuses.

**Calculation:**
```
EffectiveValue = BaseAttribute + WeaponBonuses + ArmorBonuses
```

**Example:**
```csharp
// Player has base MIGHT 4, weapon gives +1 MIGHT
var effectiveMight = equipmentService.GetEffectiveAttributeValue(player, "MIGHT");
// Returns 5
```

---

#### GetTotalDefenseBonus

```csharp
public int GetTotalDefenseBonus(PlayerCharacter player)
```

Gets total defense bonus from equipped armor.

---

#### GetTotalAccuracyBonus

```csharp
public int GetTotalAccuracyBonus(PlayerCharacter player)
```

Gets total accuracy bonus from equipped weapon.

---

#### GetWeaponDamage

```csharp
public (int damageDice, int damageBonus, int staminaCost) GetWeaponDamage(PlayerCharacter player)
```

Gets weapon damage info for combat calculations.

**Returns:** Tuple of (damageDice, damageBonus, staminaCost)

**Unarmed Fallback:** `(1, -2, 5)` - equivalent to 1d4

---

### Equipment Comparison

#### CompareEquipment

```csharp
public EquipmentComparison CompareEquipment(Equipment? current, Equipment proposed)
```

Compares two equipment items for upgrade/downgrade analysis.

**Comparison Factors:**

For **weapons:**
- Average damage (dice × 3 + bonus)
- Stamina cost (lower is better)
- Accuracy bonus
- Attribute bonuses
- Special effects

For **armor:**
- HP bonus
- Defense bonus
- Attribute bonuses
- Special effects

**Returns:** `EquipmentComparison` with:
- `IsUpgrade` - true if proposed is generally better
- `Differences` - list of human-readable differences
- Numeric diffs for damage, HP, defense, etc.

---

## Data Models

### Equipment

```csharp
public class Equipment
{
    // Identity
    public string Name { get; set; }
    public string Description { get; set; }
    public QualityTier Quality { get; set; }
    public EquipmentType Type { get; set; }

    // Weapon-specific
    public WeaponCategory? WeaponCategory { get; set; }
    public string WeaponAttribute { get; set; }  // "MIGHT", "FINESSE", "WILL"
    public int DamageDice { get; set; }          // Number of d6s
    public int DamageBonus { get; set; }         // Flat damage modifier
    public int StaminaCost { get; set; }         // Stamina per attack
    public int AccuracyBonus { get; set; }       // Attack roll modifier

    // Armor-specific
    public ArmorCategory? ArmorCategory { get; set; }
    public int HPBonus { get; set; }             // Added to MaxHP
    public int DefenseBonus { get; set; }        // Enemy attack penalty

    // Bonuses (both types)
    public List<EquipmentBonus> Bonuses { get; set; }

    // Special properties (Myth-Forged)
    public bool IgnoresArmor { get; set; }
    public string SpecialEffect { get; set; }
}
```

### EquipmentBonus

```csharp
public class EquipmentBonus
{
    public string AttributeName { get; set; }  // "MIGHT", "FINESSE", etc.
    public int BonusValue { get; set; }
    public string Description { get; set; }    // Human-readable
}
```

### EquipmentComparison

```csharp
public class EquipmentComparison
{
    public Equipment? Current { get; set; }
    public Equipment Proposed { get; set; }
    public bool IsUpgrade { get; set; }
    public List<string> Differences { get; set; }

    // Weapon comparison
    public int DamageDiceDiff { get; set; }
    public int DamageBonusDiff { get; set; }

    // Armor comparison
    public int HPBonusDiff { get; set; }
    public int DefenseBonusDiff { get; set; }
}
```

---

## Usage Examples

### Equipping a Weapon

```csharp
var equipmentService = new EquipmentService();

// Find weapon in inventory
var weapon = equipmentService.FindInInventory(player, "iron sword");
if (weapon != null)
{
    if (equipmentService.EquipWeapon(player, weapon))
    {
        Console.WriteLine($"Equipped {weapon.GetDisplayName()}");
        Console.WriteLine($"Damage: {weapon.GetDamageDescription()}");
    }
}
```

### Picking Up and Comparing Equipment

```csharp
// Find item on ground
var foundItem = equipmentService.FindOnGround(room, "axe");
if (foundItem != null)
{
    // Compare to currently equipped
    var comparison = equipmentService.CompareEquipment(
        player.EquippedWeapon,
        foundItem
    );

    Console.WriteLine(comparison.IsUpgrade ? "UPGRADE" : "DOWNGRADE");
    foreach (var diff in comparison.Differences)
    {
        Console.WriteLine($"  {diff}");
    }

    // Pick up if upgrade
    if (comparison.IsUpgrade && equipmentService.PickupItem(player, room, foundItem))
    {
        equipmentService.EquipWeapon(player, foundItem);
    }
}
```

### Getting Combat Stats

```csharp
// For combat calculations
var (damageDice, damageBonus, staminaCost) = equipmentService.GetWeaponDamage(player);
var accuracyBonus = equipmentService.GetTotalAccuracyBonus(player);
var effectiveMight = equipmentService.GetEffectiveAttributeValue(player, "MIGHT");

Console.WriteLine($"Attack: {damageDice}d6{damageBonus:+#;-#;+0}");
Console.WriteLine($"Accuracy: +{accuracyBonus}");
Console.WriteLine($"Effective MIGHT: {effectiveMight}");
```

---

## Integration Points

### With CombatEngine

```csharp
// CombatEngine uses EquipmentService for damage calculations
var (dice, bonus, _) = _equipmentService.GetWeaponDamage(player);
var accuracy = _equipmentService.GetTotalAccuracyBonus(player);
var might = _equipmentService.GetEffectiveAttributeValue(player, "MIGHT");
```

### With CharacterFactory

```csharp
// CharacterFactory sets initial equipment
var startingWeapon = CreateStartingWeapon(characterClass);
equipmentService.EquipWeapon(player, startingWeapon);
```

### With LootService

```csharp
// LootService generates equipment drops
var loot = GenerateLoot(enemy.Level, biome);
foreach (var item in loot)
{
    room.ItemsOnGround.Add(item);
}
```

---

## Related Documentation

- [CombatEngine](combat-engine.md) - Uses equipment stats for combat
- [LootService](loot-service.md) - Generates equipment drops
- [CraftingService](crafting-service.md) - Equipment crafting
- [MerchantService](merchant-service.md) - Equipment trading
