---
id: SPEC-CMD-INVENTORY
title: "Inventory Commands"
version: 1.1
status: draft
last-updated: 2025-12-14
parent: parser.md
related-files:
  - path: "docs/08-ui/inventory-ui.md"
    status: Reference
  - path: "docs/04-systems/inventory-voice.md"
    status: Reference
  - path: "RuneAndRust.Engine/Commands/InventoryCommand.cs"
    status: Planned
  - path: "RuneAndRust.Engine/Commands/EquipCommand.cs"
    status: Planned
---

# Inventory Commands

---

## Voice Guidance

> **See:** [SPEC-SYSTEM-INVENTORY-VOICE](../../04-systems/inventory-voice.md) for mandatory terminology and flavor text standards.

---

## UI Integration

> **UI Integration:** Commands in this spec trigger the unified inventory UI defined in [SPEC-UI-INVENTORY](../inventory-ui.md). The UI displays equipment slots, burden tracking, item examination, and comparison views sourced from `IInventoryViewModel`.

---

## Overview

Inventory commands manage items, equipment, and consumables. The inventory is referred to as the **Pack** or **Burden**.

| Command | Aliases | Syntax | Context |
|---------|---------|--------|---------|
| `inventory` | `i`, `inv` | `inventory` | Any |
| `equip` | `eq`, `wear` | `equip <item>` | Exploration, Inventory |
| `unequip` | `uneq`, `remove` | `unequip <slot>` | Exploration, Inventory |
| `swap` | `switch` | `swap weapon` | Any |
| `examine` | `x`, `inspect` | `examine <item>` | Any |
| `take` | `get`, `pickup` | `take <item>` | Exploration |
| `drop` | — | `drop <item>` | Exploration, Inventory |
| `use` | `consume` | `use <consumable>` | Any (non-combat) |

---

## 1. Inventory

### 1.1 Syntax

```
inventory
i
inv
i quick
```

### 1.2 Full Display

```
> inventory

  ┌─────────────────────────────────────────────────────────────────────┐
  │  HP: 45/60 ████████░░  Stamina: 80/100 ████████░░                   │
  ├─────────────────────────────────────────────────────────────────────┤
  │  ╔══════════════════════════════════════════════════════════════╗   │
  │  ║  PACK                                          Scrip: 127    ║   │
  │  ║  ────────────────────────────────────────────────────────    ║   │
  │  ║  Burden: MEDIUM (42/60 lbs)  ████████████░░░░░░              ║   │
  │  ╟──────────────────────────────────────────────────────────────╢   │
  │  ║  WIELDED                                                     ║   │
  │  ║  ───────                                                     ║   │
  │  ║    Hand:  Iron Longsword (2d8+3)                             ║   │
  │  ║    Body:  Chainmail (+4 Soak)                                ║   │
  │  ║    Off:   —                                                  ║   │
  │  ║    Head:  —                                                  ║   │
  │  ╟──────────────────────────────────────────────────────────────╢   │
  │  ║  DRAUGHTS & RITUALS                                          ║   │
  │  ║  ───────────────────                                         ║   │
  │  ║    [1] Mending Draught x3                                    ║   │
  │  ║    [2] Rot-Cure x1                                           ║   │
  │  ║    [3] Vigor Tonic x2                                        ║   │
  │  ╟──────────────────────────────────────────────────────────────╢   │
  │  ║  SALVAGE & TOOLS                                             ║   │
  │  ║  ───────────────                                             ║   │
  │  ║    [4] Iron Key                                              ║   │
  │  ║    [5] Spirit Slate                                          ║   │
  │  ║    [6] Scrap Metal x12                                       ║   │
  │  ╚══════════════════════════════════════════════════════════════╝   │
  ├─────────────────────────────────────────────────────────────────────┤
  │  [1-6] Examine  [E] Equip  [D] Drop  [U] Use  [C] Close             │
  ├─────────────────────────────────────────────────────────────────────┤
  │  [Inv] > _                                                          │
  └─────────────────────────────────────────────────────────────────────┘
```

### 1.3 Quick Display

```
> i quick

  WIELDED: Iron Longsword | Chainmail | —
  DRAUGHTS: Mending x3, Rot-Cure x1, Vigor x2
  SCRIP: 127 | BURDEN: Medium (42/60)
```

### 1.4 Keyboard Navigation

| Key | TUI Action | GUI Equivalent |
|-----|------------|----------------|
| `1-9` | Select item by number | — |
| `↑` / `↓` | Navigate items | Arrow keys |
| `E` | Equip selected | Click slot |
| `U` | Unequip selected | Drag to inventory |
| `D` | Drop selected | Right-click → Drop |
| `X` | Examine selected | Hover / click |
| `S` | Sort inventory | Sort button |
| `C` / `Esc` | Close inventory | Close button |

---

## 2. Equipment Slots

### 2.1 Complete Slot List

| Category | Slot | Display Name | Item Types |
|----------|------|--------------|------------|
| **Weapons** | `left_hand` | Left Hand | One-handed weapon |
| | `right_hand` | Right Hand | One-handed weapon |
| | `two_hand` | Two-Handed | Two-handed weapon |
| | `ranged` | Ranged | Bows, crossbows, thrown |
| **Armor** | `helmet` | Helmet | Head protection |
| | `body` | Body Armor | Chest armor |
| | `gloves` | Gloves | Hand armor |
| | `boots` | Boots | Foot armor |
| | `belt` | Belt | Utility belt |
| | `cloak` | Cloak | Capes, cloaks |
| **Accessories** | `left_bracer` | Left Bracer | Arm guards, rune bracers |
| | `right_bracer` | Right Bracer | Arm guards, rune bracers |
| | `left_ring` | Left Ring | Ring |
| | `right_ring` | Right Ring | Ring |
| | `jewel` | Jewel | Amulet, necklace, talisman |

### 2.2 Full Slot Display

```
╟──────────────────────────────────────────────────────────────╢
║  WIELDED                                                     ║
║  ───────                                                     ║
║  WEAPONS                                                     ║
║    Left:    Iron Longsword (2d8+3)                           ║
║    Right:   Parrying Dagger (+1 Def)                         ║
║    Ranged:  Hunting Bow (1d8)                                ║
║    Two-H:   — [Press T to swap]                              ║
║  ARMOR                                                       ║
║    Helmet:  —                                                ║
║    Body:    Chainmail (+4 Soak)                              ║
║    Gloves:  Leather Grips (+1 Grip)                          ║
║    Boots:   Iron-Shod Boots (+1 Soak)                        ║
║    Belt:    Scavenger's Belt (4 slots)                       ║
║    Cloak:   —                                                ║
║  ACCESSORIES                                                 ║
║    L-Brace: Runed Bracer (+2 Aether)                         ║
║    R-Brace: —                                                ║
║    L-Ring:  Band of Vigor (+5 HP)                            ║
║    R-Ring:  —                                                ║
║    Jewel:   Bone Talisman (-5 Stress)                        ║
╟──────────────────────────────────────────────────────────────╢
```

---

## 3. Equip

### 3.1 Syntax

```
equip <item>
eq <item>
wear <item>
```

### 3.2 Slot Assignment

Items auto-assign to the appropriate slot based on type:

| Item Type | Assigned Slot | Notes |
|-----------|---------------|-------|
| One-handed weapon | Left Hand | Right Hand if Left occupied |
| Two-handed weapon | Two-Handed | Clears both hands |
| Shield | Right Hand | Off-hand default |
| Armor | Body | — |
| Helmet | Helmet | — |
| Gloves | Gloves | — |
| Boots | Boots | — |
| Belt | Belt | — |
| Cloak | Cloak | — |
| Bracer | Left Bracer | Right if Left occupied |
| Ring | Left Ring | Right if Left occupied |
| Amulet/Necklace | Jewel | — |

### 3.3 Examples

```
> equip longsword

  You grip the Iron Longsword. It feels cold and eager.
  [Left Hand: Iron Longsword (2d8+3)]

> equip chainmail

  You don the Chainmail. It smells of old blood.
  [Body: Chainmail (+4 Soak)]

> equip ring of vigor

  You slide the Band of Vigor onto your finger.
  [Left Ring: Band of Vigor (+5 HP)]
```

### 3.4 Validation

| Condition | Error Message |
|-----------|---------------|
| Item not in Pack | "You don't have 'X' in your Pack." |
| Slot occupied | Auto-unequips current item |
| Cannot equip during combat | "Cannot change equipment during combat." |
| Missing proficiency | "You lack proficiency with 'X'." |

### 3.5 Events Raised

| Event | When | Payload |
|-------|------|---------|
| `ItemEquippedEvent` | Item equipped | `ItemId`, `Slot`, `PreviousItemId` |

---

## 4. Unequip

### 4.1 Syntax

```
unequip <slot>
unequip <item>
uneq <slot>
remove <item>
```

### 4.2 Examples

```
> unequip left hand

  You sheathe the Iron Longsword.
  [Left Hand: Empty]

> unequip chainmail

  You shed the Chainmail.
  [Body: Empty]
```

### 4.3 Validation

| Condition | Error Message |
|-----------|---------------|
| Slot already empty | "Nothing equipped in that slot." |
| Pack full | "Pack is full. Drop something first." |

### 4.4 Events Raised

| Event | When | Payload |
|-------|------|---------|
| `ItemUnequippedEvent` | Item unequipped | `ItemId`, `Slot` |

---

## 5. Swap (Weapon Configuration)

### 5.1 Syntax

```
swap weapon
swap
switch weapon
```

### 5.2 Behavior

Swaps between one-handed (dual-wield) and two-handed weapon configurations:

```
> swap weapon

  You sheathe your Longsword and Dagger, gripping the Dvergr Greataxe.
  [Two-Handed: Dvergr Greataxe (2d10+5)]
  [Left Hand: —]
  [Right Hand: —]
```

**Swap Back:**

```
> swap weapon

  You sling the Greataxe and draw your blades.
  [Left Hand: Iron Longsword (2d8+3)]
  [Right Hand: Parrying Dagger (+1 Def)]
  [Two-Handed: —]
```

### 5.3 Requirements

| Condition | Can Swap? |
|-----------|-----------|
| Has 2H proficiency (specialization) | ✓ |
| Has dual-wield proficiency | ✓ |
| No relevant proficiency | ✗ (must equip one style only) |
| In combat | ✓ (costs action) |
| Out of combat | ✓ (free) |

### 5.4 Events Raised

| Event | When | Payload |
|-------|------|---------|
| `WeaponSwappedEvent` | Configuration changed | `PreviousConfig`, `NewConfig` |

---

## 6. Examine

### 6.1 Syntax

```
examine <item>
x <item>
inspect <item>
```

### 6.2 Display

```
> examine longsword

  ┌────────────────────────────────────────┐
  │ IRON LONGSWORD                         │
  │ [Uncommon] Weapon — Sword              │
  ├────────────────────────────────────────┤
  │ Damage: 2d8+3 Slashing                 │
  │ Weight: 4 lbs                          │
  │ Value:  45 Scrip                       │
  ├────────────────────────────────────────┤
  │ A well-forged blade, still sharp       │
  │ despite the rust on its pommel. The    │
  │ maker's mark has worn away.            │
  └────────────────────────────────────────┘
```

### 6.3 Rarity Colors

| Rarity | Color | Example |
|--------|-------|---------|
| Common | White | `[Common] Rusty Dagger` |
| Uncommon | Green | `[Uncommon] Iron Longsword` |
| Rare | Blue | `[Rare] Dvergr-Forged Axe` |
| Epic | Purple | `[Epic] Runed Greatsword` |
| Legendary | Gold | `[Legendary] Gungnir Shard` |

### 6.4 Comparison Display

When examining equipment while similar type is equipped:

```
> examine chainmail

  ┌────────────────────────────────────────┐
  │ CHAINMAIL                              │
  │ [Uncommon] Armor — Medium              │
  ├────────────────────────────────────────┤
  │ Soak:   +4                             │
  │ Weight: 20 lbs                         │
  ├────────────────────────────────────────┤
  │ COMPARED TO EQUIPPED (Leather Vest):   │
  │   Soak:   +4  vs  +2  (+2 better)      │
  │   Weight: 20  vs  5   (+15 heavier)    │
  └────────────────────────────────────────┘
```

---

## 7. Take

### 7.1 Syntax

```
take <item>
get <item>
pickup <item>
take all
```

### 7.2 Examples

```
> take sword

  You take the Rusty Sword. It is heavy.
  Burden: 40/60 → 44/60 lbs

> take all

  You gather:
  - Rusty Sword
  - Mending Draught x2
  - 15 Scrip
```

### 7.3 Validation

| Condition | Error Message |
|-----------|---------------|
| Item not in room | "There is no 'X' here." |
| Pack full | "Pack is full. Drop something first." |
| Item too heavy | "The 'X' is too heavy. Burden would exceed capacity." |

### 7.4 Events Raised

| Event | When | Payload |
|-------|------|---------|
| `ItemTakenEvent` | Item picked up | `ItemId`, `Quantity`, `RoomId` |
| `ScripGainedEvent` | Currency collected | `Amount`, `Source` |

---

## 8. Drop

### 8.1 Syntax

```
drop <item>
drop <item> x<quantity>
```

### 8.2 Examples

```
> drop rusty dagger

  You cast aside the Rusty Dagger. It clatters on the stone.
  Burden: 42/60 → 40/60 lbs

> drop mending draught x2

  You leave the Mending Draught x2.
```

### 8.3 Validation

| Condition | Error Message |
|-----------|---------------|
| Item not in Pack | "You don't have 'X' in your Pack." |
| Quest item | "This item is needed for a quest." |

### 8.4 Events Raised

| Event | When | Payload |
|-------|------|---------|
| `ItemDroppedEvent` | Item dropped | `ItemId`, `Quantity`, `RoomId` |

---

## 9. Use (Consumable)

### 9.1 Syntax

```
use <consumable>
consume <item>
```

### 9.2 Consumable Effects

| Item | Effect |
|------|--------|
| Mending Draught | +2d6 HP |
| Vigor Tonic | +25 Stamina |
| Rot-Cure | Remove Poison |
| Aether Phial | +15 Aether |

### 9.3 Examples

```
> use mending draught

  You swallow the bitter red sludge. Fire knits your flesh.
  [+14 HP] (45/60 → 59/60)

  Mending Draught: 3 → 2 remaining
```

### 9.4 Validation

| Condition | Error Message |
|-----------|---------------|
| Not a consumable | "'X' cannot be consumed." |
| In combat | "Cannot use items during combat (use abilities)." |
| Condition not met | "You are not poisoned." (for Rot-Cure) |

### 9.5 Events Raised

| Event | When | Payload |
|-------|------|---------|
| `ConsumableUsedEvent` | Item consumed | `ItemId`, `Effect`, `RemainingQuantity` |

---

## 10. Burden System

### 10.1 Burden Tiers

| Tier | Weight Range | Effect | Color |
|------|--------------|--------|-------|
| **Light** | 0-40% capacity | No penalty | Green |
| **Medium** | 41-70% capacity | -1 movement | Yellow |
| **Heavy** | 71-90% capacity | -2 movement, -1 FINESSE | Orange |
| **Overburdened** | 91-100% | Cannot run, -2 FINESSE | Red |

### 10.2 Display

```
Burden: MEDIUM (42/60 lbs)  ████████████░░░░░░
```

### 10.3 Overweight Warning

```
Burden: OVERBURDENED (58/60 lbs)  ██████████████████ [!]
  Warning: You cannot run. Drop items to move freely.
```

---

## 11. Shared Types

```csharp
public record ItemViewModel(
    Guid Id,
    string Name,
    string Description,
    ItemCategory Category,
    Rarity Rarity,
    float Weight,
    int Value,
    int Quantity,
    string? StatsSummary,
    bool IsEquippable,
    bool IsConsumable
);

public enum BurdenTier { Light, Medium, Heavy, Overburdened }

public enum EquipmentSlot
{
    // Weapons
    LeftHand, RightHand, TwoHanded, Ranged,
    // Armor
    Helmet, Body, Gloves, Boots, Belt, Cloak,
    // Accessories
    LeftBracer, RightBracer, LeftRing, RightRing, Jewel
}

public record ItemComparisonViewModel(
    ItemViewModel Item,
    ItemViewModel EquippedItem,
    IReadOnlyDictionary<string, (int Current, int Compared, int Difference)> StatComparisons
);
```

---

## 12. Implementation Status

| Command | File Path | Status |
|---------|-----------|--------|
| `inventory` | `RuneAndRust.Engine/Commands/InventoryCommand.cs` | ❌ Planned |
| `equip` | `RuneAndRust.Engine/Commands/EquipCommand.cs` | ❌ Planned |
| `unequip` | `RuneAndRust.Engine/Commands/UnequipCommand.cs` | ❌ Planned |
| `swap` | `RuneAndRust.Engine/Commands/SwapCommand.cs` | ❌ Planned |
| `examine` | `RuneAndRust.Engine/Commands/ExamineCommand.cs` | ❌ Planned |
| `take` | `RuneAndRust.Engine/Commands/TakeCommand.cs` | ❌ Planned |
| `drop` | `RuneAndRust.Engine/Commands/DropCommand.cs` | ❌ Planned |
| `use` | `RuneAndRust.Engine/Commands/UseItemCommand.cs` | ❌ Planned |

---

## 13. Related Specifications

| Document | Relationship |
|----------|--------------|
| [inventory-ui.md](../inventory-ui.md) | TUI/GUI presentation |
| [parser.md](parser.md) | Grammar rules for inventory commands |
| [inventory-voice.md](../../04-systems/inventory-voice.md) | Terminology standards |
| [crafting.md](crafting.md) | Crafting material usage |
| [trade.md](trade.md) | Buying/selling items |

---

## 14. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Aligned with inventory-ui.md: expanded 15-slot system, added keyboard navigation, swap command, examine command, burden system, shared types, comparison display |
