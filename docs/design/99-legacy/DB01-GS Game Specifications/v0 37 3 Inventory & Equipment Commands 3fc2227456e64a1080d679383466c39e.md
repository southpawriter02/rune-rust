# v0.37.3: Inventory & Equipment Commands

Type: Technical
Description: Inventory commands - inventory, equipment, take, drop, use with encumbrance system
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.37, v0.3
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.37: Command System & Parser (v0%2037%20Command%20System%20&%20Parser%20eee6b259408f440e89f79a78d59f04fd.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.37.3-INVENTORY-COMMANDS

**Status:** Implementation Complete — Ready for Integration

**Timeline:** 8-10 hours

**Prerequisites:** v0.37 (Command System), v0.3 (Equipment System)

**Parent Specification:** v0.37: Command System & Parser[[1]](v0%2037%20Command%20System%20&%20Parser%20eee6b259408f440e89f79a78d59f04fd.md)

**v2.0 Sources:** Feature Specification: The inventory Command[[2]](https://www.notion.so/Feature-Specification-The-inventory-Command-2a355eb312da8030bdabf342e142aa8a?pvs=21), Feature Specification: The equipment Command[[3]](https://www.notion.so/Feature-Specification-The-equipment-Command-2a355eb312da80539cd7e9912359b2f0?pvs=21), Feature Specification: The use Command[[4]](https://www.notion.so/Feature-Specification-The-use-Command-2a355eb312da8097ac70fc48ec9c594a?pvs=21)

---

## I. Executive Summary

This specification defines **inventory and equipment commands**: `inventory`, `equipment`, `take`, `drop`, and `use`. These commands manage items and loadouts.

**Commands Covered:**

- `inventory` / `inv` / `i` — View carried items
- `equipment` / `eq` — View equipped gear
- `take` / `get` / `pickup` — Acquire items
- `drop` — Drop items from inventory
- `use` — Consume items or interact with objects

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.37.3)

- `inventory` screen display
- `equipment` loadout display
- `take` command with container integration
- `drop` command with confirmation
- `use` command for consumables and interactive objects
- Unit tests (10+ tests, 80%+ coverage)

### ❌ Explicitly Out of Scope

- Crafting commands (defer to v0.36)
- Trading commands (defer to v0.9)
- Artifact attunement (defer to v0.36)
- Advanced inventory management (auto-sort, filters) (defer to v1.0+)

---

## III. Command Implementations

### A. The `inventory` Command

**Syntax:** `inventory`

**Aliases:** `inv`, `i`

**Purpose:** Display all carried items with capacity info.

**Example Output:**

```
╔════════════════════════════════════════╗
║ Your Inventory                         ║
║ Capacity: 18/24 (Normal)               ║
╠════════════════════════════════════════╣
║ WEAPONS:                               ║
║  - Iron Axe (2H, 2d6 damage)           ║
║  - Rusted Dagger (1H, 1d4 damage)      ║
║                                        ║
║ ARMOR:                                 ║
║  - Scavenged Plate (Soak 6)            ║
║  - [Clan-Forged] Helm (Soak 2)         ║
║                                        ║
║ CONSUMABLES:                           ║
║  - Healing Poultice (x5)               ║
║  - Aether Flask (x2)                   ║
║                                        ║
║ MISC:                                  ║
║  - Rusted Key                          ║
║  - Scrap (125)                         ║
╚════════════════════════════════════════╝

Commands: equip [item], drop [item], use [item]
```

**Unit Test Example:**

```csharp
[TestMethod]
public void Inventory_DisplaysAllItems()
{
    // Arrange
    var items = new List<ItemInstance>
    {
        CreateItem("Iron Axe", "Weapon", equipped: false),
        CreateItem("Healing Poultice", "Consumable", quantity: 5)
    };
    var mockInventory = CreateMockInventoryService(items);
    var command = new InventoryCommand(mockInventory, _logger);
    var state = CreateGameState();

    // Act
    var result = command.Execute(state, Array.Empty<string>());

    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsTrue(result.Message.Contains("Iron Axe"));
    Assert.IsTrue(result.Message.Contains("Healing Poultice"));
    Assert.IsTrue(result.Message.Contains("(x5)"));
}
```

---

### B. The `equipment` Command

**Syntax:** `equipment`

**Aliases:** `eq`

**Purpose:** Display currently equipped gear and stats.

**Example Output:**

```
╔══════════════════════════════════════╗
║ Your Loadout                         ║
╠══════════════════════════════════════╣
║ MainHand:  Iron Axe (2d6+MIGHT)      ║
║ OffHand:   (empty)                   ║
║ Head:      [Clan-Forged] Helm        ║
║ Chest:     Scavenged Plate           ║
║ Artifact:  [Wyrd Stone] (Attuned)    ║
║                                      ║
║ Attunement: 1/3 slots used           ║
║                                      ║
║ STATS:                               ║
║  HP:      110/110    Soak:     8     ║
║  Stamina:  80/80     Defense:  13    ║
╚══════════════════════════════════════╝

Commands: unequip [item], attune [artifact]
```

---

### C. The `take` Command

**Syntax:** `take [item]` or `take [item] from [container]`

**Aliases:** `get`, `pickup`

**Purpose:** Acquire items from room or containers.

**Examples:**

```
> take axe
You take the Iron Axe.

> take key from chest
You take the Rusted Key from the chest.

> take all from corpse
You take:
- Rusted Dagger
- 15 Scrap
- Note
```

**Encumbrance Check:**

```
> take plate armor
Your inventory is full. Drop something first or you'll be encumbered.
(Encumbered: -2 FINESSE, movement costs +2 Stamina)
```

**Unit Test Example:**

```csharp
[TestMethod]
public void Take_ItemInRoom_AddsToInventory()
{
    // Arrange
    var item = CreateItemInRoom("Iron Axe");
    var mockRoom = CreateMockRoomServiceWithItem(item);
    var command = new TakeCommand(mockRoom, _inventoryService, _logger);
    var state = CreateGameState();

    // Act
    var result = command.Execute(state, new[] { "axe" });

    // Assert
    Assert.IsTrue(result.Success);
    _inventoryService.Verify(i => i.AddItem(1, item.ItemId), Times.Once);
    mockRoom.Verify(r => r.RemoveItemFromRoom(item.RoomItemId), Times.Once);
}

[TestMethod]
public void Take_InventoryFull_ReturnsWarning()
{
    // Arrange
    var command = new TakeCommand(_roomService, _inventoryService, _logger);
    var state = CreateGameState();
    state.CurrentCharacter.CurrentCarryWeight = 24; // At capacity

    // Act
    var result = command.Execute(state, new[] { "plate" });

    // Assert
    Assert.IsTrue(result.Message.Contains("inventory is full"));
}
```

---

### D. The `drop` Command

**Syntax:** `drop [item]`

**Purpose:** Remove item from inventory and place in room.

**Example:**

```
> drop rusty_key
You drop the Rusted Key.
```

**Important Item Warning:**

```
> drop quest_item
Warning: This is a quest item. Are you sure? (yes/no)
> yes
You drop the quest item.
```

---

### E. The `use` Command

**Syntax:** `use [item]` or `use [item] on [target]`

**Purpose:** Consume items or interact with environmental objects.

**Consumable Usage:**

```
> use poultice
You apply the healing poultice. (+20 HP)
HP: 95/110

> use aether_flask
You drink the Aetheric concoction. (+15 Aether)
Aether: 25/30
```

**Environmental Interaction:**

```
> use key on door
You unlock the door with the Rusted Key.
The key crumbles to dust.

> use lever
You pull the lever.
[MIGHT Check: 14 vs DC 12] SUCCESS
The vault door grinds open!
```

**Unit Test Example:**

```csharp
[TestMethod]
public void Use_HealingPoultice_RestoresHP()
{
    // Arrange
    var poultice = CreateConsumable("Healing Poultice", healAmount: 20);
    var mockInventory = CreateMockInventoryServiceWithItem(poultice);
    var command = new UseCommand(_inventoryService, _consumableService, _logger);
    var state = CreateGameState();
    state.CurrentCharacter.CurrentHitPoints = 75;
    state.CurrentCharacter.MaxHitPoints = 110;

    // Act
    var result = command.Execute(state, new[] { "poultice" });

    // Assert
    Assert.IsTrue(result.Success);
    Assert.AreEqual(95, state.CurrentCharacter.CurrentHitPoints); // 75 + 20
    _inventoryService.Verify(i => i.RemoveItem(1, poultice.ItemId, 1), Times.Once);
}

[TestMethod]
public void Use_KeyOnDoor_UnlocksDoor()
{
    // Arrange
    var key = CreateItem("Rusted Key");
    var door = CreateLockedDoor("east", requiresKey: "Rusted Key");
    var command = new UseCommand(_inventoryService, _interactionService, _logger);
    var state = CreateGameState();

    // Act
    var result = command.Execute(state, new[] { "key", "on", "door" });

    // Assert
    Assert.IsTrue(result.Success);
    _interactionService.Verify(i => i.UnlockDoor(door.DoorId, key.ItemId), Times.Once);
}
```

---

## IV. Database Schema

```sql
-- Use existing Equipment & Inventory tables from v0.3
-- No additional tables required

-- Inventory capacity tracking
ALTER TABLE Characters ADD COLUMN current_carry_weight INTEGER DEFAULT 0;
ALTER TABLE Characters ADD COLUMN max_carry_capacity INTEGER DEFAULT 24;
```

---

## V. Serilog Logging Examples

```csharp
public class TakeCommand : ICommand
{
    private readonly ILogger<TakeCommand> _logger;

    public CommandResult Execute(GameState state, string[] args)
    {
        _logger.Information(
            "Take command: CharacterId={CharacterId}, Item={Item}, RoomId={RoomId}",
            state.CurrentCharacter.CharacterId,
            string.Join(" ", args),
            state.CurrentRoom.RoomId);

        var item = _roomService.GetItemInRoom(state.CurrentRoom.RoomId, itemName);

        if (item == null)
        {
            _logger.Warning(
                "Take failed: Item not found: Item={Item}, RoomId={RoomId}",
                itemName,
                state.CurrentRoom.RoomId);
            return CommandResult.Failure($"There is no '{itemName}' here.");
        }

        // Check encumbrance
        if (WillBeEncumbered(state.CurrentCharacter, item))
        {
            _logger.Warning(
                "Take warning: Encumbrance: CharacterId={CharacterId}, CurrentWeight={Current}, MaxCapacity={Max}",
                state.CurrentCharacter.CharacterId,
                state.CurrentCharacter.CurrentCarryWeight,
                state.CurrentCharacter.MaxCarryCapacity);
        }

        _inventoryService.AddItem(state.CurrentCharacter.CharacterId, item.ItemId);
        _roomService.RemoveItemFromRoom(item.RoomItemId);

        _logger.Information(
            "Take successful: Item={Item}, NewInventorySize={Size}",
            item.ItemName,
            _inventoryService.GetInventorySize(state.CurrentCharacter.CharacterId));

        return CommandResult.Success($"You take the {item.ItemName}.");
    }
}
```

---

## VI. Success Criteria

### Functional Requirements

- [ ]  `inventory` displays all items with capacity
- [ ]  `equipment` shows equipped gear and stats
- [ ]  `take` adds items to inventory
- [ ]  `take` checks encumbrance limits
- [ ]  `drop` removes items from inventory
- [ ]  `use` consumes items correctly
- [ ]  `use` interacts with environment objects
- [ ]  All commands have clear feedback

### Quality Gates

- [ ]  80%+ unit test coverage
- [ ]  Serilog logging on all operations
- [ ]  Integration with Equipment System (v0.3)
- [ ]  Encumbrance system functional

---

**Inventory commands complete. Total: ~400 lines of service code + 10 unit tests.**