# Inventory & Equipment System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Active
> **Specification ID**: SPEC-SYSTEM-002

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-27 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [x] **Review**: Ready for stakeholder review
- [x] **Approved**: Approved for implementation
- [x] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: Game Designer
- **Design**: Equipment progression, inventory management
- **Balance**: Stat scaling, quality tier distribution
- **Implementation**: EquipmentService.cs, Equipment.cs
- **QA/Testing**: Equipment balance, inventory edge cases

---

## Executive Summary

### Purpose Statement
The Inventory & Equipment System manages player item storage, equipment slots, stat bonuses, and equipment-based character progression through quality tiers and attribute modifiers.

### Scope
**In Scope**:
- Inventory capacity management and item storage
- Equipment slots (weapon, armor, accessory)
- Quality tiers (Jury-Rigged through Myth-Forged)
- Stat bonuses from equipped items
- Equip/unequip operations
- Pickup/drop item from rooms

**Out of Scope**:
- Equipment generation/drops → `SPEC-ECONOMY-001` (Loot System)
- Crafting and modification → `SPEC-SYSTEM-004` (Crafting System)
- Shop/merchant transactions → `SPEC-ECONOMY-002` (Economy System)
- Equipment durability → Future enhancement

### Success Criteria
- **Player Experience**: Clear, responsive item management with meaningful gear progression
- **Technical**: Inventory operations complete in <50ms
- **Design**: Quality tiers create clear power progression
- **Balance**: Equipment bonuses are impactful but not overwhelming

---

## Related Documentation

### Dependencies
**Depends On**:
- Character System: PlayerCharacter model, attributes → `SPEC-PROGRESSION-001`
- Combat System: Damage calculation uses weapon stats → `SPEC-COMBAT-002`
- Save System: Equipment persistence → `SPEC-SYSTEM-001`

**Depended Upon By**:
- Combat System: Weapon damage, armor defense → `SPEC-COMBAT-002`
- Loot System: Equipment drops → `SPEC-ECONOMY-001`
- Crafting System: Equipment modification → `SPEC-SYSTEM-004`
- Character Sheet UI: Equipment display

### Related Specifications
- `SPEC-SYSTEM-001`: Save/Load System
- `SPEC-ECONOMY-001`: Loot & Equipment Generation
- `SPEC-COMBAT-002`: Damage Calculation

### Code References
- **Primary Service**: `RuneAndRust.Engine/EquipmentService.cs`
- **Core Model**: `RuneAndRust.Core/Equipment.cs`
- **Database**: `RuneAndRust.Engine/EquipmentDatabase.cs`
- **Tests**: `RuneAndRust.Tests/EquipmentServiceTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Meaningful Progression**
   - **Rationale**: Each quality tier should feel like a significant upgrade
   - **Examples**: Myth-Forged items have unique effects; Jury-Rigged items are stopgaps

2. **Simple Slot System**
   - **Rationale**: Players should instantly understand equipment management
   - **Examples**: One weapon, one armor, one accessory - no complex slot puzzles

3. **Attribute Synergy**
   - **Rationale**: Equipment should complement character build choices
   - **Examples**: MIGHT weapons for Warriors, WILL staves for Mystics

### Player Experience Goals
**Target Experience**: Excitement at finding upgrades, satisfaction in meaningful gear choices

**Moment-to-Moment Gameplay**:
- Player finds loot drop in room
- Compares stats with current equipment
- Makes meaningful equip/sell decision
- Feels power increase from upgrades

**Learning Curve**:
- **Novice** (0-2 hours): Understands equip/unequip, sees bigger numbers = better
- **Intermediate** (2-10 hours): Appreciates attribute synergy, quality tier differences
- **Expert** (10+ hours): Optimizes loadout for specific encounters, values special effects

### Design Constraints
- **Technical**: Equipment must serialize to JSON for persistence
- **Gameplay**: Inventory limited to 20 items (prevents hoarding)
- **Narrative**: Equipment names fit post-apocalyptic Norse setting
- **Scope**: No equipment durability or repair mechanics

---

## Functional Requirements

### FR-001: Equip Weapon
**Priority**: Critical
**Status**: Implemented

**Description**:
System must equip a weapon from inventory to the weapon slot, moving any currently equipped weapon to inventory if space permits.

**Rationale**:
Players need to upgrade weapons as they progress. Smooth swapping reduces friction.

**Acceptance Criteria**:
- [x] Weapon validated as EquipmentType.Weapon
- [x] Weapon removed from inventory if present
- [x] Previous weapon moved to inventory (if space available)
- [x] Player stats recalculated after equip
- [x] Returns false if item is not a weapon

**Example Scenarios**:
1. **Scenario**: Player equips Iron Axe over Rusty Hatchet
   - **Input**: EquipWeapon(player, ironAxe)
   - **Expected Output**: player.EquippedWeapon = ironAxe, rustyHatchet in inventory
   - **Success Condition**: MaxHP and damage calculations reflect new weapon

2. **Edge Case**: Equip weapon when inventory is full
   - **Input**: Player has 20 items, equips new weapon
   - **Expected Behavior**: Old weapon dropped (not in inventory), new weapon equipped

**Dependencies**:
- Requires: FR-007 (stat recalculation)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/EquipmentService.cs:EquipWeapon()`
- **Data Requirements**: Valid Equipment with Type=Weapon

---

### FR-002: Equip Armor
**Priority**: Critical
**Status**: Implemented

**Description**:
System must equip armor from inventory to the armor slot, moving any currently equipped armor to inventory if space permits.

**Rationale**:
Armor provides survivability. Players need to upgrade as challenges increase.

**Acceptance Criteria**:
- [x] Armor validated as EquipmentType.Armor
- [x] Armor removed from inventory if present
- [x] Previous armor moved to inventory (if space available)
- [x] Player MaxHP recalculated after equip
- [x] Returns false if item is not armor

**Example Scenarios**:
1. **Scenario**: Player equips Clan-Forged Chainmail
   - **Input**: EquipArmor(player, chainmail) with HPBonus=15
   - **Expected Output**: player.MaxHP increases by 15, old armor to inventory
   - **Success Condition**: HP ratio maintained after recalculation

**Dependencies**:
- Requires: FR-007 (stat recalculation)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/EquipmentService.cs:EquipArmor()`

---

### FR-003: Unequip to Inventory
**Priority**: High
**Status**: Implemented

**Description**:
System must move equipped item to inventory if space available, leaving slot empty.

**Rationale**:
Players may want to fight unarmed or compare stats without equipment.

**Acceptance Criteria**:
- [x] Returns false if slot already empty
- [x] Returns false if inventory full (count >= MaxInventorySize)
- [x] Item added to inventory, slot set to null
- [x] Stats recalculated after unequip

**Example Scenarios**:
1. **Scenario**: Player unequips weapon
   - **Input**: UnequipWeapon(player)
   - **Expected Output**: Weapon in inventory, EquippedWeapon = null
   - **Success Condition**: Inventory count increased by 1

**Dependencies**:
- Requires: FR-007 (stat recalculation)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/EquipmentService.cs:UnequipWeapon()`, `UnequipArmor()`

---

### FR-004: Add to Inventory
**Priority**: High
**Status**: Implemented

**Description**:
System must add item to player inventory if capacity allows.

**Rationale**:
Inventory is primary storage for loot. Capacity limit creates meaningful choices.

**Acceptance Criteria**:
- [x] Returns false if inventory at capacity (count >= MaxInventorySize)
- [x] Item added to inventory list
- [x] Returns true on success

**Example Scenarios**:
1. **Scenario**: Player picks up potion
   - **Input**: AddToInventory(player, potion), inventory has 15/20 items
   - **Expected Output**: true, inventory now 16/20
   - **Success Condition**: Item accessible in player.Inventory

2. **Edge Case**: Inventory full
   - **Input**: AddToInventory(player, item), inventory at 20/20
   - **Expected Behavior**: false, item not added

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/EquipmentService.cs:AddToInventory()`

---

### FR-005: Remove from Inventory
**Priority**: High
**Status**: Implemented

**Description**:
System must remove specified item from inventory.

**Rationale**:
Items may be sold, dropped, or consumed. Clean removal is essential.

**Acceptance Criteria**:
- [x] Item removed from inventory list if present
- [x] Returns true if item was found and removed
- [x] Returns false if item not in inventory

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/EquipmentService.cs:RemoveFromInventory()`

---

### FR-006: Pickup/Drop Items
**Priority**: High
**Status**: Implemented

**Description**:
System must support picking up items from room ground and dropping items to room ground.

**Rationale**:
Items exist in world before being collected. Players may need to drop items to make space.

**Acceptance Criteria**:
- [x] PickupItem validates item exists in room.ItemsOnGround
- [x] PickupItem validates inventory has space
- [x] Item moved from room to inventory on pickup
- [x] DropItem validates item exists in inventory
- [x] Item moved from inventory to room.ItemsOnGround on drop

**Example Scenarios**:
1. **Scenario**: Player picks up sword from ground
   - **Input**: PickupItem(player, room, sword)
   - **Expected Output**: sword in inventory, removed from room.ItemsOnGround

2. **Scenario**: Player drops item to make space
   - **Input**: DropItem(player, room, oldSword)
   - **Expected Output**: oldSword in room.ItemsOnGround, removed from inventory

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/EquipmentService.cs:PickupItem()`, `DropItem()`

---

### FR-007: Stat Recalculation
**Priority**: Critical
**Status**: Implemented

**Description**:
System must recalculate all equipment-affected stats when equipment changes, including MaxHP, attribute bonuses, and resource pools.

**Rationale**:
Equipment bonuses must reflect immediately. HP ratio preservation prevents death on equip change.

**Acceptance Criteria**:
- [x] MaxHP calculated from base + armor HPBonus + passives
- [x] HP ratio preserved (current HP / MaxHP maintained)
- [x] Attribute bonuses computed from both weapon and armor
- [x] Defense bonus computed from armor
- [x] Accuracy bonus computed from weapon
- [x] Mystic MaxAP recalculated if applicable

**Formula/Logic**:
```
MaxHP = BaseClassHP + (Milestone × 10) + Armor.HPBonus + Passives
CurrentHP = min(MaxHP × PreviousHPRatio, MaxHP)

EffectiveAttribute = BaseAttribute + Σ(EquipmentBonuses for that attribute)

For Mystic:
MaxAP = (WILL × 10) + 50 + Passives
CurrentAP = min(MaxAP × PreviousAPRatio, MaxAP)
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| BaseHP_Warrior | int | 40-60 | 50 | Warrior starting HP | Yes |
| BaseHP_Mystic | int | 25-40 | 30 | Mystic starting HP | Yes |
| BaseHP_Adept | int | 30-45 | 35 | Adept starting HP | Yes |
| BaseHP_Skirmisher | int | 35-50 | 40 | Skirmisher starting HP | Yes |
| HPPerMilestone | int | 5-15 | 10 | HP gained per milestone | Yes |

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/EquipmentService.cs:RecalculatePlayerStats()`

---

### FR-008: Find Items by Name
**Priority**: Medium
**Status**: Implemented

**Description**:
System must support finding items in inventory or room by partial name match.

**Rationale**:
Console interface requires text-based item selection. Partial matching improves UX.

**Acceptance Criteria**:
- [x] Case-insensitive matching
- [x] Partial string matching (contains)
- [x] Returns first match or null if none

**Example Scenarios**:
1. **Scenario**: Player types "iron" to find "Iron Axe"
   - **Input**: FindInInventory(player, "iron")
   - **Expected Output**: Equipment{Name="Iron Axe"}

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/EquipmentService.cs:FindInInventory()`, `FindOnGround()`

---

## System Mechanics

### Mechanic 1: Quality Tiers

**Overview**:
Equipment exists in five quality tiers, each representing a distinct power level and narrative origin.

**How It Works**:
1. Quality tier determines base stat ranges
2. Higher tiers have more bonuses and special effects
3. Tier affects drop rates and vendor prices
4. Myth-Forged items have unique abilities

**Quality Tier Breakdown**:
| Tier | Name | Stat Range | Bonuses | Drop Rate | Special |
|------|------|------------|---------|-----------|---------|
| 0 | Jury-Rigged | Very Low | 0 | Common | Breaks easily |
| 1 | Scavenged | Low | 0-1 | Common | Standard loot |
| 2 | Clan-Forged | Medium | 1-2 | Uncommon | Crafted quality |
| 3 | Optimized | High | 2-3 | Rare | Pre-Glitch tech |
| 4 | Myth-Forged | Very High | 3+ | Very Rare | Unique effects |

**Example Equipment by Tier**:
```
Jury-Rigged: "Scrap Shiv" - 1d6 damage, no bonuses
Scavenged: "Rust-Pitted Axe" - 1d6+1 damage, +1 MIGHT
Clan-Forged: "Ironhollow Cleaver" - 2d6 damage, +2 MIGHT, +1 accuracy
Optimized: "Sentinel's Rifle" - 2d6+2 damage, +2 FINESSE, +2 accuracy, ignores cover
Myth-Forged: "Járngreipr" - 3d6+3 damage, +3 MIGHT, +2 STURDINESS, ignores armor
```

**Edge Cases**:
1. **Quality downgrade on damage**: Future enhancement
   - **Condition**: Equipment takes excessive damage
   - **Behavior**: Quality tier reduces
   - **Example**: Optimized → Clan-Forged after heavy use

**Related Requirements**: FR-001, FR-002, FR-007

---

### Mechanic 2: Attribute Bonuses

**Overview**:
Equipment can provide bonuses to character attributes, affecting combat performance.

**How It Works**:
1. Equipment defines list of EquipmentBonus objects
2. Each bonus specifies attribute name and value
3. GetEffectiveAttributeValue() sums base + all equipment bonuses
4. Combat calculations use effective values

**Formula**:
```
EffectiveAttribute(name) = BaseAttribute(name) +
  Σ(weapon.Bonuses.Where(b => b.AttributeName == name).BonusValue) +
  Σ(armor.Bonuses.Where(b => b.AttributeName == name).BonusValue)

Example:
  Player has MIGHT 4
  Equipped Axe has +2 MIGHT bonus
  Equipped Chainmail has +1 MIGHT bonus

  EffectiveMIGHT = 4 + 2 + 1 = 7
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| MaxAttributeBonus | int | 1-5 | 5 | Maximum bonus from single item | Yes |
| MaxTotalBonus | int | 5-10 | 10 | Maximum total bonus to any attribute | Yes |

**Data Flow**:
```
Input Sources:
  → PlayerCharacter.Attributes (base values)
  → EquippedWeapon.Bonuses[]
  → EquippedArmor.Bonuses[]

Processing:
  → GetEffectiveAttributeValue() iterates bonuses
  → Sums matching attribute bonuses
  → Returns total

Output Destinations:
  → Combat dice pools (MIGHT/FINESSE)
  → Ability calculations (WITS/WILL)
  → Defense calculations (STURDINESS)
```

**Related Requirements**: FR-007

---

### Mechanic 3: Inventory Management

**Overview**:
Players have limited inventory space, creating meaningful decisions about what to keep.

**How It Works**:
1. MaxInventorySize constant (default 20)
2. Operations check count before adding
3. Full inventory blocks pickup
4. Player must drop/sell to make space

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| MaxInventorySize | int | 10-50 | 20 | Maximum inventory items | Yes |

**Edge Cases**:
1. **Inventory full during equip swap**: Old item dropped to ground
   - **Condition**: Inventory at max, player equips new item
   - **Behavior**: Old equipment dropped, not lost
   - **Example**: Full inventory + equip new armor = old armor on ground

2. **Multiple identical items**: Each item separate entry
   - **Condition**: Player has 3 Health Potions
   - **Behavior**: 3 inventory slots used (no stacking for equipment)
   - **Example**: Consumables may stack differently (separate system)

**Related Requirements**: FR-004, FR-005, FR-006

---

## State Management

### System State

**State Variables**:
| Variable | Type | Persistence | Default | Description |
|----------|------|-------------|---------|-------------|
| player.Inventory | List<Equipment> | Permanent | [] | Items in inventory |
| player.EquippedWeapon | Equipment? | Permanent | null | Currently equipped weapon |
| player.EquippedArmor | Equipment? | Permanent | null | Currently equipped armor |
| player.MaxInventorySize | int | Permanent | 20 | Inventory capacity |
| room.ItemsOnGround | List<Equipment> | Permanent | [] | Items in current room |

**State Transitions**:
```
[In Inventory] ---EquipWeapon/Armor---> [Equipped]
[Equipped] ---Unequip---> [In Inventory]
[On Ground] ---Pickup---> [In Inventory]
[In Inventory] ---Drop---> [On Ground]
[Equipped] ---Equip New (full inv)---> [On Ground]
```

### Persistence Requirements

**Must Persist**:
- EquippedWeapon: Core character state
- EquippedArmor: Core character state
- Inventory[]: All collected items
- Room.ItemsOnGround: Per-room item persistence

**Save Format**:
- Equipment serialized to JSON strings
- inventory_json: JSON array of Equipment objects
- equipped_weapon_json: Single Equipment or null
- equipped_armor_json: Single Equipment or null
- room_items_json: Dictionary {roomId: Equipment[]}

---

## Integration Points

### Systems This System Consumes

#### Integration with Character System
**What We Use**: PlayerCharacter model, base attributes
**How We Use It**: Read/write inventory, equipment slots
**Dependency Type**: Hard

#### Integration with Room System
**What We Use**: Room.ItemsOnGround collection
**How We Use It**: Pickup/drop operations modify room state
**Dependency Type**: Hard for pickup/drop operations

### Systems That Consume This System

#### Consumed By Combat System
**What They Use**: Weapon damage dice, armor defense, attribute bonuses
**How They Use It**: GetEffectiveAttributeValue(), weapon.DamageDice, armor.DefenseBonus
**Stability Contract**: Weapon/armor always have valid stat values

#### Consumed By Save System
**What They Use**: JSON-serializable Equipment objects
**How They Use It**: Equipment serialized/deserialized during save/load
**Stability Contract**: Equipment model JSON-compatible

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
|-----------|----------|---------------|-----|-----|--------|------------------|
| Tier0_DamageDice | EquipmentDatabase | 1 | 1 | 1 | Baseline damage | Low |
| Tier4_DamageDice | EquipmentDatabase | 3 | 2 | 4 | Endgame damage | Medium |
| ArmorHPBonus_Heavy | EquipmentDatabase | 25 | 15 | 35 | Survivability | Medium |
| MaxAttributeBonus | Balance constant | 5 | 3 | 7 | Stat scaling | Medium |

### Balance Targets

**Target 1: Tier Progression**
- **Metric**: Damage increase per tier
- **Current**: ~25% increase per tier
- **Target**: 20-30% increase per tier
- **Levers**: DamageDice, DamageBonus per tier

**Target 2: Equipment vs Base Stats**
- **Metric**: Equipment contribution to effective stats
- **Current**: ~30% of total at endgame
- **Target**: 25-40% of total
- **Levers**: MaxAttributeBonus, bonus frequency

---

## Implementation Guidance

### Implementation Status

**Current State**: Complete

**Completed**:
- [x] Equipment model with all properties
- [x] EquipmentService with all operations
- [x] Quality tier system
- [x] Attribute bonus system
- [x] Stat recalculation
- [x] Pickup/drop operations
- [x] JSON serialization support

### Code Architecture

**Recommended Structure**:
```
RuneAndRust.Core/
  └─ Equipment.cs           // Equipment model, enums, EquipmentBonus

RuneAndRust.Engine/
  ├─ EquipmentService.cs    // All equipment operations
  └─ EquipmentDatabase.cs   // Static equipment definitions

RuneAndRust.Persistence/
  └─ (Equipment persisted via SaveRepository JSON)

RuneAndRust.Tests/
  └─ EquipmentServiceTests.cs
```

---

## Testing & Verification

### Test Scenarios

#### Test Case 1: Equip/Unequip Roundtrip
**Type**: Unit

**Objective**: Verify equip and unequip preserve item correctly

**Test Steps**:
1. Create weapon, add to inventory
2. Call EquipWeapon
3. Verify weapon in slot, not in inventory
4. Call UnequipWeapon
5. Verify weapon in inventory, slot empty

**Expected Results**:
- Weapon correctly moves between inventory and slot
- Stats recalculated each time

#### Test Case 2: HP Ratio Preservation
**Type**: Unit

**Objective**: Verify HP ratio maintained during armor changes

**Test Steps**:
1. Set player HP to 50% of MaxHP
2. Equip armor with +20 HPBonus
3. Verify new HP = 50% of new MaxHP

**Expected Results**:
- HP ratio unchanged after equipment change
- No sudden death from equip operation

---

## Appendix

### Appendix A: Terminology

| Term | Definition |
|------|------------|
| **Quality Tier** | Equipment power level (0-4) |
| **Jury-Rigged** | Lowest tier, barely functional |
| **Myth-Forged** | Highest tier, legendary artifacts |
| **EquipmentBonus** | Attribute modifier from equipment |
| **HPBonus** | Direct MaxHP increase from armor |
| **DefenseBonus** | Enemy attack roll penalty |
| **AccuracyBonus** | Player attack roll bonus |

### Appendix B: Weapon Categories

| Category | Primary Attribute | Typical Class |
|----------|------------------|---------------|
| Axe | MIGHT | Warrior |
| Greatsword | MIGHT | Warrior |
| Spear | FINESSE | Skirmisher |
| Dagger | FINESSE | Skirmisher |
| Staff | WILL | Mystic |
| Focus | WILL | Mystic |
| Blade | MIGHT/FINESSE | Any |
| Rifle | FINESSE | Any |

### Appendix C: Armor Categories

| Category | HP Bonus | Defense | Attribute Effect |
|----------|----------|---------|------------------|
| Light | Low (5-10) | Low (0-1) | +FINESSE, +Evasion |
| Medium | Medium (10-20) | Medium (1-2) | Balanced |
| Heavy | High (15-30) | High (2-4) | -FINESSE, +STURDINESS |

---

**End of Specification**
