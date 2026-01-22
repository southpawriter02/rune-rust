# v0.37.1: Core Navigation Commands

Type: Technical
Description: Navigation commands - look, go, investigate, search with RoomService integration
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.37, v0.46
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.37: Command System & Parser (v0%2037%20Command%20System%20&%20Parser%20eee6b259408f440e89f79a78d59f04fd.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.37.1-NAVIGATION-COMMANDS

**Status:** Implementation Complete — Ready for Integration

**Timeline:** 10-12 hours

**Prerequisites:** v0.37 (Command System), v0.46 (Dynamic Room Engine)

**Parent Specification:** v0.37: Command System & Parser[[1]](v0%2037%20Command%20System%20&%20Parser%20eee6b259408f440e89f79a78d59f04fd.md)

**v2.0 Sources:** Feature Specification: The look Command[[2]](https://www.notion.so/Feature-Specification-The-look-Command-2a355eb312da80cd9e0bf7c606ba31e8?pvs=21), Feature Specification: The go Command[[3]](https://www.notion.so/Feature-Specification-The-go-Command-2a355eb312da8056a7bcc42967a3247c?pvs=21), Feature Specification: The investigate Command[[4]](https://www.notion.so/Feature-Specification-The-investigate-Command-2a355eb312da8092a17ff2e6948f0cda?pvs=21)

---

## I. Executive Summary

This specification defines **core navigation commands**: `look`, `go`, `investigate`, and `search`. These are the player's primary interface for exploring and perceiving the game world.

**Commands Covered:**

- `look` / `l` / `examine` / `x` — Primary perception
- `go` / `g` / `move` / `n`/`s`/`e`/`w` — Room navigation
- `investigate` / `inv` — Active perception check
- `search` — Container/area search

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.37.1)

- Full `look` command implementation with room descriptions
- `go` command with exit validation
- `investigate` command with WITS checks
- `search` command for containers
- Integration with RoomService and SkillCheckService
- Unit tests (10+ tests, 80%+ coverage)

### ❌ Explicitly Out of Scope

- Combat-specific perception (defer to v0.37.2)
- 3D vertical navigation (defer to v0.45)
- Procedural room generation (covered in v0.46)
- Advanced perception mechanics (defer to v0.38+)

---

## III. Command Implementations

### A. The `look` Command

**Syntax:** `look` or `look at [target]`

**Aliases:** `l`, `examine`, `x`

**Purpose:** Display room description and examine objects.

**Example Output:**

```
> look

=== The Rusted Forge ===
A collapsed workshop choked with twisted metal. Scorch marks spiral across corroded walls.

Exits: north (Shattered Hall), east (Supply Cache)
Objects: [lever], [workbench], [corpse]
Enemies: Rust Warden (wounded)
```

**Service Interface:**

```csharp
public interface IRoomService
{
    Room GetCurrentRoom(int characterId);
    List<Exit> GetAvailableExits(int roomId);
    List<InteractiveObject> GetInteractiveObjects(int roomId);
    InteractiveObject GetObjectByName(int roomId, string name);
}
```

**Unit Test Example:**

```csharp
[TestMethod]
public void Look_DisplaysCompleteRoomDescription()
{
    // Arrange
    var room = new Room
    {
        RoomId = 1,
        RoomName = "Test Room",
        Description = "A test room."
    };
    var mockRoomService = CreateMockRoomService(room);
    var command = new LookCommand(mockRoomService, _logger);
    var state = CreateGameState();

    // Act
    var result = command.Execute(state, Array.Empty<string>());

    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsTrue(result.Message.Contains("Test Room"));
    Assert.IsTrue(result.Message.Contains("A test room."));
}
```

---

### B. The `go` Command

**Syntax:** `go [direction]` or `go [room_name]`

**Aliases:** `g`, `move`, `n`, `s`, `e`, `w`, `u`, `d`

**Purpose:** Move between rooms.

**Validation Rules:**

1. Cannot move during combat (must `flee`)
2. Exit must exist
3. Exit must not be locked
4. Character must have movement capability

**Error Messages:**

```
> go west
There is no exit to the west. Valid exits: north, east

> go north (during combat)
You cannot leave during combat. Use 'flee' to escape.

> go east (locked door)
The door to the east is locked. You need an Iron Key.
```

**Unit Test Example:**

```csharp
[TestMethod]
public void Go_ValidExit_MovesCharacter()
{
    // Arrange
    var exit = new Exit { Direction = "north", DestinationRoomId = 2 };
    var mockRoomService = CreateMockRoomServiceWithExits(new[] { exit });
    var command = new GoCommand(mockRoomService, _movementService, _logger);
    var state = CreateGameState();

    // Act
    var result = command.Execute(state, new[] { "north" });

    // Assert
    Assert.IsTrue(result.Success);
    _movementService.Verify(m => m.MoveCharacter(1, 2), Times.Once);
}

[TestMethod]
public void Go_DuringCombat_ReturnsError()
{
    // Arrange
    var command = new GoCommand(_roomService, _movementService, _logger);
    var state = CreateGameState();
    state.IsInCombat = true;

    // Act
    var result = command.Execute(state, new[] { "north" });

    // Assert
    Assert.IsFalse(result.Success);
    Assert.IsTrue(result.Message.Contains("cannot leave during combat"));
}
```

---

### C. The `investigate` Command

**Syntax:** `investigate [target]`

**Aliases:** `inv`

**Purpose:** Perform WITS-based skill check to discover hidden content.

**Check Resolution:**

```
1. Roll WITS + Perception skill vs DC
2. Success: Grant rewards (items, info, secret exits)
3. Failure: Generic failure message
4. Mark target as investigated
```

**Example Output:**

```
> investigate corpse

[WITS Check: 16 vs DC 12] SUCCESS

You find hidden in the corpse:
- Rusted Key
- 15 Scrap
- Note: "The lever opens the vault."
```

**Unit Test Example:**

```csharp
[TestMethod]
public void Investigate_Success_GrantsRewards()
{
    // Arrange
    var investigatable = new InvestigatableObject
    {
        Name = "corpse",
        InvestigationDC = 12,
        RewardItems = new[] { new ItemReward { ItemId = 1, Quantity = 1 } }
    };
    var mockSkillCheck = CreateMockSkillCheckService(16); // Success
    var command = new InvestigateCommand(_roomService, mockSkillCheck, _itemService, _logger);
    var state = CreateGameState();

    // Act
    var result = command.Execute(state, new[] { "corpse" });

    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsTrue(result.Message.Contains("SUCCESS"));
    _itemService.Verify(i => i.AddItemToInventory(1, 1, 1), Times.Once);
}
```

---

### D. The `search` Command

**Syntax:** `search [container]`

**Purpose:** Search containers for loot (no skill check).

**Behavior:**

- Simpler than `investigate` (no DC)
- Finds obvious/visible contents
- Cannot be searched twice

**Example:**

```
> search chest

You find:
- Iron Sword
- 25 Scrap
- Healing Poultice (x3)
```

---

## IV. Database Schema

```sql
CREATE TABLE InteractiveObjects (
    object_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,
    object_name TEXT NOT NULL,
    description TEXT,
    detailed_description TEXT,
    is_investigatable BOOLEAN DEFAULT 0,
    investigation_dc INTEGER,
    success_description TEXT,
    failure_description TEXT,
    already_investigated BOOLEAN DEFAULT 0,
    FOREIGN KEY (room_id) REFERENCES Rooms(room_id)
);

CREATE TABLE SearchableContainers (
    container_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,
    container_name TEXT NOT NULL,
    already_searched BOOLEAN DEFAULT 0,
    FOREIGN KEY (room_id) REFERENCES Rooms(room_id)
);

CREATE TABLE ContainerContents (
    content_id INTEGER PRIMARY KEY AUTOINCREMENT,
    container_id INTEGER NOT NULL,
    item_id INTEGER NOT NULL,
    quantity INTEGER DEFAULT 1,
    FOREIGN KEY (container_id) REFERENCES SearchableContainers(container_id),
    FOREIGN KEY (item_id) REFERENCES Items(item_id)
);
```

---

## V. Serilog Logging Examples

```csharp
public class LookCommand : ICommand
{
    private readonly ILogger<LookCommand> _logger;

    public CommandResult Execute(GameState state, string[] args)
    {
        _logger.Information(
            "Look command executed: CharacterId={CharacterId}, Target={Target}, RoomId={RoomId}",
            state.CurrentCharacter.CharacterId,
            args.Length > 0 ? string.Join(" ", args) : "(none)",
            state.CurrentRoom.RoomId);

        try
        {
            var result = args.Length == 0 
                ? DescribeRoom(state) 
                : ExamineTarget(state, args);

            _logger.Debug(
                "Look command completed: Success={Success}, OutputLength={Length}",
                result.Success,
                result.Message.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Look command failed: CharacterId={CharacterId}, Error={ErrorType}",
                state.CurrentCharacter.CharacterId,
                ex.GetType().Name);
            throw;
        }
    }
}
```

---

## VI. Success Criteria

### Functional Requirements

- [ ]  `look` displays complete room state
- [ ]  `look at` examines specific targets
- [ ]  `go` validates exits and moves character
- [ ]  `go` prevents movement during combat
- [ ]  `investigate` performs WITS checks
- [ ]  `investigate` grants rewards on success
- [ ]  `search` finds container contents
- [ ]  All commands have clear error messages

### Quality Gates

- [ ]  80%+ unit test coverage
- [ ]  Serilog logging on all operations
- [ ]  Integration with RoomService functional
- [ ]  All v2.0 behaviors preserved

---

**Navigation commands complete. Total: ~350 lines of service code + 10 unit tests.**