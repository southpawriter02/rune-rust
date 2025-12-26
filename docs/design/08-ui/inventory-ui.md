---
id: SPEC-UI-INVENTORY
title: "Inventory UI — TUI & GUI Specification"
version: 1.2
status: draft
last-updated: 2025-12-15
related-files:
  - path: "docs/08-ui/commands/inventory.md"
    status: Reference
  - path: "docs/08-ui/tui-layout.md"
    status: Reference
---

# Inventory UI — TUI & GUI Specification

> *"A scavenger's pack is their lifeline. Know what you carry — it may be all that stands between you and the long dark."*

---

## 1. Overview

This specification defines the terminal (TUI) and graphical (GUI) interfaces for inventory management, including equipment, consumables, salvage, and burden tracking.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-UI-INVENTORY` |
| Category | UI System |
| Priority | High |
| Status | Draft |

### 1.2 Terminology

| Term | Meaning |
|------|---------|
| **Pack** | The player's inventory |
| **Burden** | Encumbrance level (Light/Medium/Heavy/Overburdened) |
| **Scrip** | Currency |
| **Draughts** | Consumable potions |
| **Salvage** | Crafting materials and key items |
| **Wielded** | Currently equipped items |

### 1.3 Design Pillars

- **Quick Access** — View pack contents rapidly
- **Clear Slots** — Equipment slots immediately visible
- **Burden Awareness** — Weight/encumbrance always shown
- **Item Details** — Examine any item for full stats
- **Immersive Flavor** — Items described, not just listed

---

## 2. TUI Inventory Layout

### 2.1 Full Inventory Screen

```
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

### 2.2 Component Breakdown

| Component | Description | Lines |
|-----------|-------------|-------|
| **Header Bar** | Player HP/Stamina | 1 |
| **Pack Header** | Title, Scrip amount | 2 |
| **Burden Bar** | Weight with visual bar | 1 |
| **Wielded Section** | Equipment slots | 5 |
| **Draughts Section** | Consumables | Variable |
| **Salvage Section** | Materials and key items | Variable |
| **Command Bar** | Available actions | 1 |
| **Input Prompt** | `[Inv] >` | 1 |

---

## 3. Equipment Slots

### 3.1 Complete Slot List

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

### 3.2 TUI Slot Display

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

### 3.3 Weapon Swapping

> [!NOTE]
> **Design Decision:** Players can swap between one-handed (dual-wield) and two-handed weapon configurations if their **specialization allows** such proficiency.

**Swap Command:**
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

**Swap Requirements:**
| Condition | Can Swap? |
|-----------|-----------|
| Has 2H proficiency (specialization) | ✓ |
| Has dual-wield proficiency | ✓ |
| No relevant proficiency | ✗ (must equip one style only) |
| In combat | ✓ (costs action) |
| Out of combat | ✓ (free) |

### 3.4 Empty vs Equipped

```
Left:    Iron Longsword (2d8+3)     ← Equipped with stats
Right:   —                           ← Empty slot
Two-H:   — [T to swap]               ← Available if proficient
```


---

## 4. Item Categories

### 4.1 Category Display

| Category | Header | Contains |
|----------|--------|----------|
| **Draughts & Rituals** | Consumables | Potions, scrolls, food |
| **Salvage & Tools** | Utility | Keys, materials, tools |
| **Weapons** | Arms | Unequipped weapons |
| **Armor** | Protection | Unequipped armor |

### 4.2 Item Numbering

Items are numbered sequentially for quick access:

```
[1] Mending Draught x3
[2] Rot-Cure x1
[3] Vigor Tonic x2
[4] Iron Key
[5] Spirit Slate
```

---

## 5. Item Stacking

### 5.1 Stacking Rules

Items stack based on their category. Stackable items display as `Item Name x#` in both TUI and GUI.

| Category | Stackable? | Max Stack | Example Display |
|----------|------------|-----------|-----------------|
| **Weapons** | No | 1 | `Iron Longsword` |
| **Armor** | No | 1 | `Chainmail` |
| **Accessories** | No | 1 | `Runed Bracer` |
| **Draughts** | Yes | 10 | `Mending Draught x10` |
| **Common Salvage** | Yes | 100 | `Scrap Metal x100` |
| **Rare Salvage** | Yes | 20 | `Iron Heart Fragment x20` |
| **Alchemy Ingredients** | Yes | 50 | `Rot-Moss x50` |
| **Keys/Quest Items** | No | 1 | `Iron Key` |
| **Currency (Scrip)** | N/A | Unlimited | Tracked separately |

### 5.2 Stack Display

```
╟──────────────────────────────────────────────────────────────╢
║  DRAUGHTS & RITUALS                                          ║
║  ───────────────────                                         ║
║    [1] Mending Draught x3                                    ║
║    [2] Rot-Cure x1                                           ║
║    [3] Vigor Tonic x10  [FULL STACK]                         ║
╟──────────────────────────────────────────────────────────────╢
║  SALVAGE & TOOLS                                             ║
║  ───────────────                                             ║
║    [4] Iron Key                                              ║
║    [5] Scrap Metal x47                                       ║
║    [6] Scrap Metal x53                                       ║
║    [7] Rot-Moss x12                                          ║
╚══════════════════════════════════════════════════════════════╝
```

> [!NOTE]
> When a stack reaches maximum capacity, additional items of the same type occupy a new inventory slot. In the example above, the player has 100 Scrap Metal split across two stacks (47 + 53).

### 5.3 Weight Calculation for Stacks

Stack weight is calculated as: `base_weight × quantity`

| Item | Base Weight | Quantity | Total Weight |
|------|-------------|----------|--------------|
| Mending Draught | 0.5 lbs | x3 | 1.5 lbs |
| Scrap Metal | 0.1 lbs | x47 | 4.7 lbs |
| Rot-Moss | 0.05 lbs | x12 | 0.6 lbs |

### 5.4 Splitting Stacks

Players can split stacks for trading or dropping partial quantities:

```
> split scrap metal 20

  You separate 20 Scrap Metal from the pile.
  [Scrap Metal x47 → x27]
  [New stack: Scrap Metal x20]
```

### 5.5 Auto-Stacking

When picking up stackable items:
1. System checks for existing partial stacks of the same item
2. Items fill partial stacks first (up to max)
3. Overflow creates new stacks
4. If inventory is full and no partial stacks exist, item cannot be picked up

```
> take scrap metal

  You gather the salvage.
  [Scrap Metal: 47 → 52]  (filled existing stack)

> take scrap metal pile

  You gather the salvage.
  [Scrap Metal: 52 → 100]  (stack full)
  [New stack: Scrap Metal x15]  (overflow)
```

---

## 6. Item Charges

### 6.1 Charge System Overview

Some items have multiple uses before being depleted. This applies primarily to **applied effects** (coatings, oils) rather than **ingested consumables** (draughts, tonics).

| Consumable Type | Uses Charges? | Depletion Trigger |
|-----------------|---------------|-------------------|
| **Ingested** (draughts, tonics) | No | Single-use, decrements stack |
| **Applied** (blade coatings, oils) | Yes | Per hit or per action |
| **Duration** (elixirs, buffs) | No | Time-based expiration |
| **Thrown** (bombs, flasks) | No | Single-use, decrements stack |

### 6.2 Charge Display

Items with charges show remaining uses:

```
╟──────────────────────────────────────────────────────────────╢
║  COATINGS & OILS                                             ║
║  ───────────────                                             ║
║    [1] Blade Venom x2           [3/3] [3/3]                  ║
║    [2] Blade Venom              [1/3]  ← partially used      ║
║    [3] Rust-Proof Oil x3        [5/5] [5/5] [5/5]            ║
║    [4] Baron's Black            [2/3]                        ║
╟──────────────────────────────────────────────────────────────╢
║  DRAUGHTS & RITUALS                                          ║
║  ───────────────────                                         ║
║    [5] Mending Draught x3       (single-use, no charges)     ║
║    [6] Glitch-Sight Draught x1                               ║
╚══════════════════════════════════════════════════════════════╝
```

### 6.3 Charge Properties

| Property | Description |
|----------|-------------|
| `MaxCharges` | Total uses when full (e.g., 3 for Blade Venom) |
| `CurrentCharges` | Remaining uses |
| `ChargeType` | What depletes a charge (Hit, Action, Application) |

### 6.4 Charge Depletion

```
> attack rust-horror

  You strike with your poisoned blade!
  [Hit] 2d8+3 = 14 slashing + 1d10 = 7 poison

  Blade Venom: 3/3 → 2/3 charges remaining
```

```
> apply rust-proof oil to armor

  You work the oil into the chainmail joints.
  [Rust-Proof Oil applied to Body Armor — 8 hours]

  Rust-Proof Oil: 5/5 → 4/5 charges remaining
```

### 6.5 Stacking Rules for Charged Items

| Scenario | Behavior |
|----------|----------|
| **Full-charge items** | Stack normally (`Blade Venom x3`) |
| **Partial-charge items** | Do NOT stack with full items |
| **Multiple partial items** | Each occupies separate slot |

> [!NOTE]
> Partial-charge items cannot be combined. A `Blade Venom [1/3]` and `Blade Venom [2/3]` remain separate inventory entries. This prevents exploits and keeps tracking simple.

### 6.6 Charge Examination

```
> examine blade venom

  ┌────────────────────────────────────────┐
  │ BLADE VENOM                            │
  │ [Uncommon] Coating — Poison            │
  ├────────────────────────────────────────┤
  │ Charges: 2/3 (per hit)                 │
  │ Effect:  +1d10 poison damage           │
  │ Weight:  0.2 lbs                       │
  │ Value:   18 Scrip                      │
  ├────────────────────────────────────────┤
  │ A glass vial of milky-green liquid     │
  │ that clings to steel. Apply to blade   │
  │ before combat. Corpse Flower base.     │
  └────────────────────────────────────────┘
```

---

## 7. Burden System Display

### 7.1 Burden Tiers

| Tier | Weight Range | Effect | Color |
|------|--------------|--------|-------|
| **Light** | 0-40% capacity | No penalty | Green |
| **Medium** | 41-70% capacity | -1 movement | Yellow |
| **Heavy** | 71-90% capacity | -2 movement, -1 FINESSE | Orange |
| **Overburdened** | 91-100% | Cannot run, -2 FINESSE | Red |

### 7.2 Burden Bar

```
Burden: MEDIUM (42/60 lbs)  ████████████░░░░░░
```

### 7.3 Overweight Warning

```
Burden: OVERBURDENED (58/60 lbs)  ██████████████████ [!]
  Warning: You cannot run. Drop items to move freely.
```

---

## 8. Item Examination

### 8.1 Examine Command

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

### 8.2 Rarity Colors

| Rarity | Color | Example |
|--------|-------|---------|
| Common | White | `[Common] Rusty Dagger` |
| Uncommon | Green | `[Uncommon] Iron Longsword` |
| Rare | Blue | `[Rare] Dvergr-Forged Axe` |
| Epic | Purple | `[Epic] Runed Greatsword` |
| Legendary | Gold | `[Legendary] Gungnir Shard` |

### 8.3 Comparison Display

When examining a weapon/armor while one is equipped:

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

## 9. Inventory Actions

### 9.1 Equip

```
> equip longsword

  You grip the Iron Longsword. It feels cold and eager.
  [Hand: Iron Longsword (2d8+3)]
```

### 9.2 Unequip

```
> unequip hand

  You sheathe the Iron Longsword.
  [Hand: Empty]
```

### 9.3 Use Consumable

```
> use mending draught

  You swallow the bitter red sludge. Fire knits your flesh.
  [+14 HP] (45/60 → 59/60)
  
  Mending Draught: 3 → 2 remaining
```

### 9.4 Drop

```
> drop rusty dagger

  You cast aside the Rusty Dagger. It clatters on the stone.
  Burden: 42/60 → 40/60 lbs
```

### 9.5 Close Inventory

```
> close

  [Returning to exploration...]
```

---

## 10. Quick Inventory (Exploration Mode)

A condensed inventory view for exploration without entering full inventory mode:

```
> i quick

  WIELDED: Iron Longsword | Chainmail | —
  DRAUGHTS: Mending x3, Rot-Cure x1, Vigor x2
  SCRIP: 127 | BURDEN: Medium (42/60)
```

---

## 11. GUI Inventory Panel

### 11.1 Layout

```
┌───────────────────────────────────────────────────────────────────────┐
│  PACK                                                    Scrip: 127  │
│  Burden: ████████████░░░░░░ MEDIUM (42/60 lbs)                       │
├─────────────────────────────────┬─────────────────────────────────────┤
│  WIELDED                        │  ITEM DETAILS                       │
│  ┌─────┐ ┌─────┐ ┌─────┐        │  ┌─────────────────────────────────┐│
│  │ ⚔️  │ │ 🛡️  │ │ 👕 │        │  │ IRON LONGSWORD               [U]││
│  │Hand │ │Off  │ │Body│        │  │ [Uncommon] Weapon — Sword      ││
│  └─────┘ └─────┘ └─────┘        │  │                                 ││
│  ┌─────┐ ┌─────┐ ┌─────┐        │  │ Damage: 2d8+3 Slashing         ││
│  │ 🎩 │ │ 📿  │ │ 💍 │        │  │ Weight: 4 lbs                   ││
│  │Head │ │Neck │ │Ring│        │  │ Value:  45 Scrip                ││
│  └─────┘ └─────┘ └─────┘        │  │                                 ││
├─────────────────────────────────┤  │ A well-forged blade, still     ││
│  INVENTORY                      │  │ sharp despite the rust...       ││
│  ┌─────────────────────────────┐│  └─────────────────────────────────┘│
│  │ 🧪 Mending Draught x3      ││  ┌─────────────────────────────────┐│
│  │ 🧪 Rot-Cure x1             ││  │ [E] Equip  [D] Drop  [U] Use    ││
│  │ 🧪 Vigor Tonic x2          ││  │ [S] Sort   [C] Close            ││
│  │ 🔑 Iron Key                ││  └─────────────────────────────────┘│
│  │ 📜 Spirit Slate            ││                                     │
│  │ ⚙️ Scrap Metal x12         ││                                     │
│  └─────────────────────────────┘│                                     │
└─────────────────────────────────┴─────────────────────────────────────┘
```

### 11.2 GUI Components

| Component | Description |
|-----------|-------------|
| **Equipment Grid** | 6 slots with drag-drop support |
| **Inventory List** | Scrollable item list with icons |
| **Item Details Panel** | Shows selected item stats |
| **Action Buttons** | Equip, Drop, Use, Sort |
| **Burden Bar** | Visual weight display |

### 11.3 Interactive Elements

| Element | Click | Drag | Right-Click |
|---------|-------|------|-------------|
| Equipment slot | Select | Unequip to inventory | Context menu |
| Inventory item | Select | Equip (to slot) | Context menu |
| Item in details | — | — | — |

---

## 12. InventoryViewModel

### 12.1 Interface

```csharp
public interface IInventoryViewModel
{
    // State
    int Scrip { get; }
    float CurrentWeight { get; }
    float MaxWeight { get; }
    BurdenTier Burden { get; }
    
    // Equipment
    IReadOnlyDictionary<EquipmentSlot, ItemViewModel?> EquippedItems { get; }
    
    // Inventory
    IReadOnlyList<ItemViewModel> InventoryItems { get; }
    ItemViewModel? SelectedItem { get; set; }
    
    // Comparison
    ItemComparisonViewModel? Comparison { get; }
    
    // Commands
    ICommand EquipCommand { get; }
    ICommand UnequipCommand { get; }
    ICommand UseCommand { get; }
    ICommand DropCommand { get; }
    ICommand SortCommand { get; }
    ICommand CloseCommand { get; }
}

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
```

---

## 13. Configuration

| Setting | Default | Options |
|---------|---------|---------|
| `ShowItemWeight` | true | true/false |
| `ShowItemValue` | true | true/false |
| `AutoSort` | false | true/false |
| `SortOrder` | Category | Category/Name/Weight/Value/Rarity |
| `CompactView` | false | true/false |

---

## 14. Implementation Status

| Component | TUI Status | GUI Status |
|-----------|------------|------------|
| Inventory screen layout | ❌ Planned | ❌ Planned |
| Equipment slots display | ❌ Planned | ❌ Planned |
| Burden bar | ❌ Planned | ❌ Planned |
| Item categories | ❌ Planned | ❌ Planned |
| Item examination | ❌ Planned | ❌ Planned |
| Comparison display | ❌ Planned | ❌ Planned |
| Equip/Unequip | ❌ Planned | ❌ Planned |
| Use consumable | ❌ Planned | ❌ Planned |
| Drop item | ❌ Planned | ❌ Planned |
| Quick inventory | ❌ Planned | ❌ Planned |
| InventoryViewModel | ❌ Planned | ❌ Planned |

---

## 15. Phased Implementation Guide

### Phase 1: Core Display
- [ ] Inventory screen layout
- [ ] Equipment slots
- [ ] Item list with categories
- [ ] `[Inv] >` prompt

### Phase 2: Item Details
- [ ] Examine command
- [ ] Rarity colors
- [ ] Comparison display

### Phase 3: Actions
- [ ] Equip/Unequip
- [ ] Use consumable
- [ ] Drop item

### Phase 4: Burden System
- [ ] Weight calculation
- [ ] Burden bar
- [ ] Overweight warnings

### Phase 5: GUI
- [ ] InventoryViewModel
- [ ] Equipment grid with drag-drop
- [ ] Item details panel

---

## 16. Testing Requirements

### 16.1 TUI Tests
- [ ] Items display in correct categories
- [ ] Equipment slots show correct items
- [ ] Burden bar updates on equip/drop
- [ ] Rarity colors render correctly

### 16.2 GUI Tests
- [ ] Drag-drop equips item
- [ ] Item details update on selection
- [ ] Comparison shows when examining similar type

### 16.3 Integration Tests
- [ ] Equip → Stats update
- [ ] Use consumable → HP restored
- [ ] Drop → Item appears in room

---

## 17. Related Specifications

| Spec | Relationship |
|------|--------------|
| [commands/inventory.md](commands/inventory.md) | Command syntax |
| [tui-layout.md](tui-layout.md) | Screen composition |
| [terminal-adapter.md](terminal-adapter.md) | Terminal rendering |

---

## 18. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.2 | 2025-12-15 | Added Item Charges section (Section 6) |
| 1.1 | 2025-12-15 | Added Item Stacking section (Section 5) |
| 1.0 | 2025-12-14 | Initial specification |
