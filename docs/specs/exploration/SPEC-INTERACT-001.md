---
id: SPEC-INTERACT-001
title: Interaction System
version: 1.0.1
status: Implemented
created: 2025-12-22
updated: 2025-12-22
last_updated: 2025-12-25
tags: [exploration, wits, examine, loot, containers, discovery]
related_specs: [SPEC-DICE-001, SPEC-INV-001, SPEC-CODEX-001]
---

# Interaction System

## Overview

The **Interaction System** implements WITS-based player-object interaction mechanics for exploration gameplay. Players examine objects to reveal tiered descriptions (base, detailed, expert), search containers for procedurally generated loot, and manipulate containers (open/close). The system integrates with `LootService` for item generation and `DataCaptureService` for lore discovery.

### Core Design Principles

1. **WITS-Based Discovery**: Examination success is determined by WITS attribute dice rolls, rewarding perceptive characters.
2. **Tiered Information Reveal**: Objects have 3 description tiers (base, detailed, expert) unlocked by net successes (0, 1+, 3+).
3. **Persistent State Tracking**: Highest examination tier is saved to prevent redundant rolling; containers track searched/looted state.
4. **Procedural Loot Generation**: Container contents generated on first search based on biome, danger level, loot tier, and WITS bonus.
5. **Stateful Container Management**: Containers must be opened before searching; loot persists until explicitly taken.

### Implementation Location

- **Service**: [RuneAndRust.Engine/Services/InteractionService.cs](../../RuneAndRust.Engine/Services/InteractionService.cs) (583 lines)
- **Interface**: [RuneAndRust.Core/Interfaces/IInteractionService.cs](../../RuneAndRust.Core/Interfaces/IInteractionService.cs) (75 lines)
- **Tests**: [RuneAndRust.Tests/Engine/InteractionServiceTests.cs](../../RuneAndRust.Tests/Engine/InteractionServiceTests.cs) (601 lines)

---

## Behaviors

### Primary Behaviors

#### 1. WITS-Based Examination

**Purpose**: Roll WITS dice pool to determine how much information is revealed about an object.

**Implementation** ([InteractionService.cs:73-162](../../RuneAndRust.Engine/Services/InteractionService.cs#L73-L162)):
```csharp
public async Task<ExaminationResult> ExamineAsync(string targetName)
{
    // Validate game state
    if (_gameState.CurrentRoomId == null || _gameState.CurrentCharacter == null)
    {
        return ExaminationResult.NotFound(targetName);
    }

    // Find the object in the current room
    var interactableObject = await _objectRepository.GetByNameInRoomAsync(
        _gameState.CurrentRoomId.Value,
        targetName);

    if (interactableObject == null)
    {
        return ExaminationResult.NotFound(targetName);
    }

    // Roll WITS dice pool for examination
    var wits = _gameState.CurrentCharacter.GetAttribute(CharacterAttribute.Wits);
    var diceResult = _diceService.Roll(wits, $"Examine {targetName}");
    var netSuccesses = diceResult.Successes - diceResult.Botches;

    // Determine tier revealed
    int tierRevealed = CalculateTierRevealed(netSuccesses);

    // Check if new information was revealed
    bool newInfoRevealed = tierRevealed > interactableObject.HighestExaminationTier;

    // Update object state if new tier reached
    if (newInfoRevealed)
    {
        interactableObject.HighestExaminationTier = tierRevealed;
        interactableObject.HasBeenExamined = true;
        await _objectRepository.UpdateAsync(interactableObject);
        await _objectRepository.SaveChangesAsync();
    }

    // Build the combined description
    string description = BuildDescription(interactableObject, tierRevealed);

    // Attempt capture generation on expert tier examination
    if (tierRevealed >= ExpertTierThreshold && _captureService != null)
    {
        var witsBonus = _gameState.CurrentCharacter.GetAttribute(CharacterAttribute.Wits) / 2;
        var captureResult = await _captureService.TryGenerateFromExaminationAsync(
            _gameState.CurrentCharacter.Id,
            interactableObject,
            tierRevealed,
            witsBonus);

        if (captureResult.Success)
        {
            description += $"\n\n[Data Captured: {captureResult.Capture!.Type}]";
        }
    }

    return new ExaminationResult(
        Success: tierRevealed > 0,
        NetSuccesses: netSuccesses,
        TierRevealed: tierRevealed,
        Description: description,
        NewInfoRevealed: newInfoRevealed,
        Rolls: diceResult.Rolls
    );
}
```

**Behavior Details**:
- **WITS Roll**: Dice pool size = character's WITS attribute (typically 1-5).
- **Net Successes**: `diceResult.Successes - diceResult.Botches`.
- **Tier Calculation**:
  - 0 net → Base tier (0)
  - 1-2 net → Detailed tier (1)
  - 3+ net → Expert tier (2)
- **State Persistence**: Object's `HighestExaminationTier` updated only if current roll exceeds previous best.
- **Data Capture**: Expert tier examinations (3+ successes) have a chance to generate lore fragments.

---

#### 2. Tier Calculation

**Purpose**: Determine description tier based on net successes.

**Implementation** ([InteractionService.cs:534-541](../../RuneAndRust.Engine/Services/InteractionService.cs#L534-L541)):
```csharp
private static int CalculateTierRevealed(int netSuccesses)
{
    if (netSuccesses >= ExpertTierThreshold)  // 3+
        return 2; // Expert tier
    if (netSuccesses >= DetailedTierThreshold)  // 1+
        return 1; // Detailed tier
    return 0; // Base only
}
```

**Constants**:
- `DetailedTierThreshold = 1` (1+ net successes)
- `ExpertTierThreshold = 3` (3+ net successes)

**Tier Mapping**:
| Net Successes | Tier | Description Fields |
|---------------|------|-------------------|
| < 0 (fumble) | 0 | Base only |
| 0 | 0 | Base only |
| 1-2 | 1 | Base + Detailed |
| 3+ | 2 | Base + Detailed + Expert |

---

#### 3. Description Building

**Purpose**: Concatenate description fields based on revealed tier.

**Implementation** ([InteractionService.cs:546-561](../../RuneAndRust.Engine/Services/InteractionService.cs#L546-L561)):
```csharp
private static string BuildDescription(InteractableObject obj, int tierRevealed)
{
    var parts = new List<string> { obj.Description };  // Always include base

    if (tierRevealed >= 1 && !string.IsNullOrWhiteSpace(obj.DetailedDescription))
    {
        parts.Add(obj.DetailedDescription);
    }

    if (tierRevealed >= 2 && !string.IsNullOrWhiteSpace(obj.ExpertDescription))
    {
        parts.Add(obj.ExpertDescription);
    }

    return string.Join(" ", parts);
}
```

**Example**:
```
Base (Tier 0):
"A rusted metal crate."

Detailed (Tier 1):
"A rusted metal crate. The hinges are corroded but functional."

Expert (Tier 2):
"A rusted metal crate. The hinges are corroded but functional. Faint symbols are etched on the lid—Pre-Glitch markings."
```

---

#### 4. Container Opening

**Purpose**: Validate and open a container, enabling search/loot operations.

**Implementation** ([InteractionService.cs:165-209](../../RuneAndRust.Engine/Services/InteractionService.cs#L165-L209)):
```csharp
public async Task<string> OpenAsync(string targetName)
{
    var validationResult = ValidateGameState("Open");
    if (validationResult != null) return validationResult;

    var interactableObject = await _objectRepository.GetByNameInRoomAsync(
        _gameState.CurrentRoomId!.Value,
        targetName);

    if (interactableObject == null)
    {
        return $"You don't see anything called '{targetName}' here.";
    }

    if (!interactableObject.IsContainer)
    {
        return $"The {interactableObject.Name} cannot be opened.";
    }

    if (interactableObject.IsOpen)
    {
        return $"The {interactableObject.Name} is already open.";
    }

    if (interactableObject.IsLocked)
    {
        return $"The {interactableObject.Name} is locked. You need to unlock it first.";
    }

    // Open the container
    interactableObject.IsOpen = true;
    interactableObject.LastModified = DateTime.UtcNow;
    await _objectRepository.UpdateAsync(interactableObject);
    await _objectRepository.SaveChangesAsync();

    return $"You open the {interactableObject.Name}. It creaks on rusted hinges.";
}
```

**Validation Chain**:
1. Game state valid (room + character set)
2. Object exists in current room
3. Object is a container (`IsContainer == true`)
4. Container is not already open (`IsOpen == false`)
5. Container is not locked (`IsLocked == false`)

**State Mutation**:
- Sets `IsOpen = true`
- Updates `LastModified` timestamp
- Persists to database

---

#### 5. Container Closing

**Purpose**: Close an open container (inverse of open operation).

**Implementation** ([InteractionService.cs:212-250](../../RuneAndRust.Engine/Services/InteractionService.cs#L212-L250)):
```csharp
public async Task<string> CloseAsync(string targetName)
{
    var validationResult = ValidateGameState("Close");
    if (validationResult != null) return validationResult;

    var interactableObject = await _objectRepository.GetByNameInRoomAsync(
        _gameState.CurrentRoomId!.Value,
        targetName);

    if (interactableObject == null)
    {
        return $"You don't see anything called '{targetName}' here.";
    }

    if (!interactableObject.IsContainer)
    {
        return $"The {interactableObject.Name} cannot be closed.";
    }

    if (!interactableObject.IsOpen)
    {
        return $"The {interactableObject.Name} is already closed.";
    }

    // Close the container
    interactableObject.IsOpen = false;
    interactableObject.LastModified = DateTime.UtcNow;
    await _objectRepository.UpdateAsync(interactableObject);
    await _objectRepository.SaveChangesAsync();

    return $"You close the {interactableObject.Name}.";
}
```

**Validation Chain**:
1. Game state valid
2. Object exists in current room
3. Object is a container
4. Container is currently open (`IsOpen == true`)

**Behavior Note**: Closing a container does NOT remove generated loot from `_containerLoot` dictionary; loot persists.

---

#### 6. Room Search

**Purpose**: Roll WITS to discover objects in the current room with success-based detail.

**Implementation** ([InteractionService.cs:253-320](../../RuneAndRust.Engine/Services/InteractionService.cs#L253-L320)):
```csharp
public async Task<string> SearchAsync()
{
    var validationResult = ValidateGameState("Search");
    if (validationResult != null) return validationResult;

    // Roll WITS dice pool for search
    var wits = _gameState.CurrentCharacter!.GetAttribute(CharacterAttribute.Wits);
    var diceResult = _diceService.Roll(wits, "Search room");
    var netSuccesses = diceResult.Successes - diceResult.Botches;

    // Get all objects in room
    var objects = await _objectRepository.GetByRoomIdAsync(_gameState.CurrentRoomId!.Value);
    var objectList = objects.ToList();

    if (!objectList.Any())
    {
        return "Your search reveals nothing of interest. The room appears empty of notable objects.";
    }

    // Reveal objects based on search success
    var foundObjects = new List<string>();
    foreach (var obj in objectList)
    {
        foundObjects.Add(obj.Name);  // Always find visible objects
    }

    if (netSuccesses < 0)  // Fumble
    {
        return "Your hasty search disturbs the dust, revealing little of value. " +
               $"You notice: {string.Join(", ", foundObjects.Take(1))}.";
    }

    if (netSuccesses == 0)
    {
        return $"You search the area and find: {string.Join(", ", foundObjects)}.";
    }

    // Successful search provides more detail
    var containerCount = objectList.Count(o => o.IsContainer);
    var lockedCount = objectList.Count(o => o.IsLocked);

    var result = $"Your careful search reveals: {string.Join(", ", foundObjects)}.";

    if (containerCount > 0 && netSuccesses >= DetailedTierThreshold)  // 1+
    {
        var containerNote = containerCount == 1
            ? "One appears to be a container."
            : $"{containerCount} appear to be containers.";
        result += $" {containerNote}";
    }

    if (lockedCount > 0 && netSuccesses >= ExpertTierThreshold)  // 3+
    {
        var lockedNote = lockedCount == 1
            ? "You notice one is secured with a lock."
            : $"You notice {lockedCount} are secured with locks.";
        result += $" {lockedNote}";
    }

    return result;
}
```

**Success Tiers**:
- **< 0 (Fumble)**: Only reveals 1 object name
- **0**: Reveals all object names
- **1-2**: Reveals names + container count
- **3+**: Reveals names + container count + locked count

---

#### 7. Container Loot Generation

**Purpose**: Generate procedural loot from a container on first search.

**Implementation** ([InteractionService.cs:366-452](../../RuneAndRust.Engine/Services/InteractionService.cs#L366-L452)):
```csharp
public async Task<LootResult> SearchContainerAsync(string targetName)
{
    var validationResult = ValidateGameState("SearchContainer");
    if (validationResult != null)
    {
        return LootResult.Failure(validationResult);
    }

    // Find the container
    var container = await _objectRepository.GetByNameInRoomAsync(
        _gameState.CurrentRoomId!.Value,
        targetName);

    if (container == null)
    {
        return LootResult.Failure($"You don't see anything called '{targetName}' here.");
    }

    if (!container.IsContainer)
    {
        return LootResult.Failure($"The {container.Name} cannot be searched for loot.");
    }

    if (!container.IsOpen)
    {
        return LootResult.Failure($"The {container.Name} is closed. Open it first.");
    }

    if (container.HasBeenSearched)
    {
        // Return any remaining items in the container
        if (_containerLoot.TryGetValue(container.Id, out var remainingItems) && remainingItems.Count > 0)
        {
            var itemList = string.Join(", ", remainingItems.Select(i => i.Name));
            return LootResult.Found(
                $"The {container.Name} still contains: {itemList}.",
                remainingItems.AsReadOnly());
        }

        return LootResult.Empty($"You've already searched the {container.Name}. Nothing else remains.");
    }

    // Get room for context
    var room = await _roomRepository.GetByIdAsync(_gameState.CurrentRoomId!.Value);
    var biome = room?.BiomeType ?? BiomeType.Ruin;
    var danger = room?.DangerLevel ?? DangerLevel.Safe;

    // Get WITS bonus for quality rolls
    var witsBonus = _gameState.CurrentCharacter!.GetAttribute(CharacterAttribute.Wits) / 2;

    var context = new LootGenerationContext(biome, danger, container.LootTier, witsBonus);
    var lootResult = await _lootService.SearchContainerAsync(container, context);

    // Store generated items for taking
    if (lootResult.Success && lootResult.Items.Count > 0)
    {
        _containerLoot[container.Id] = lootResult.Items.ToList();
    }

    // Persist the container's searched state
    await _objectRepository.UpdateAsync(container);
    await _objectRepository.SaveChangesAsync();

    // Attempt capture generation during container search
    if (_captureService != null && _gameState.CurrentCharacter != null)
    {
        var captureResult = await _captureService.TryGenerateFromSearchAsync(
            _gameState.CurrentCharacter.Id,
            container,
            witsBonus);
    }

    return lootResult;
}
```

**Behavior Details**:
- **First Search**: Calls `LootService.SearchContainerAsync()` with generation context.
- **Loot Storage**: Generated items stored in `_containerLoot` dictionary (key = container GUID).
- **Subsequent Searches**: Returns previously generated loot if any remains.
- **WITS Bonus**: `character.Wits / 2` affects loot quality rolls in `LootService`.
- **Context Parameters**:
  - `BiomeType`: From current room (affects item type tables)
  - `DangerLevel`: From current room (affects quality tiers)
  - `LootTier`: From container entity (optional quality override)
  - `WitsBonus`: Character WITS / 2

---

#### 8. Item Taking

**Purpose**: Transfer an item from a container to player inventory.

**Implementation** ([InteractionService.cs:455-503](../../RuneAndRust.Engine/Services/InteractionService.cs#L455-L503)):
```csharp
public async Task<string> TakeItemAsync(string itemName)
{
    var validationResult = ValidateGameState("Take");
    if (validationResult != null) return validationResult;

    // Search through all open containers in the room for the item
    var containers = await _objectRepository.GetByRoomIdAsync(_gameState.CurrentRoomId!.Value);
    var openContainers = containers.Where(c => c.IsContainer && c.IsOpen).ToList();

    foreach (var container in openContainers)
    {
        if (_containerLoot.TryGetValue(container.Id, out var items))
        {
            var item = items.FirstOrDefault(i =>
                i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                // Remove from container loot
                items.Remove(item);

                // Add to player inventory if service available
                if (_inventoryService != null && _gameState.CurrentCharacter != null)
                {
                    var addResult = await _inventoryService.AddItemAsync(
                        _gameState.CurrentCharacter, item);

                    if (!addResult.Success)
                    {
                        // Put the item back if we couldn't add it
                        items.Add(item);
                        return addResult.Message;
                    }
                }

                return $"You take the {item.Name} from the {container.Name}.";
            }
        }
    }

    return $"You don't see any '{itemName}' to take.";
}
```

**Behavior Details**:
- **Search Scope**: All open containers in current room.
- **Case-Insensitive Match**: Item name comparison ignores case.
- **Transactional**: If `InventoryService.AddItemAsync()` fails (e.g., encumbered), item is restored to container.
- **Inventory Integration**: Delegates to `IInventoryService` for weight/capacity validation.

---

### Secondary Behaviors

#### 9. Game State Validation

**Purpose**: Validate that player is in a room with an active character before interactions.

**Implementation** ([InteractionService.cs:566-581](../../RuneAndRust.Engine/Services/InteractionService.cs#L566-L581)):
```csharp
private string? ValidateGameState(string action)
{
    if (_gameState.CurrentRoomId == null)
    {
        _logger.LogWarning("{Action} attempted with no current room set", action);
        return "You must be in a room to do that.";
    }

    if (_gameState.CurrentCharacter == null)
    {
        _logger.LogWarning("{Action} attempted with no current character", action);
        return "You must have an active character to do that.";
    }

    return null;  // Validation passed
}
```

**Used By**: `OpenAsync()`, `CloseAsync()`, `SearchAsync()`, `SearchContainerAsync()`, `TakeItemAsync()`

---

#### 10. Object Listing

**Purpose**: Format a list of visible objects for display.

**Implementation** ([InteractionService.cs:341-363](../../RuneAndRust.Engine/Services/InteractionService.cs#L341-L363)):
```csharp
public async Task<string> ListObjectsAsync()
{
    var objects = await GetVisibleObjectsAsync();
    var objectList = objects.ToList();

    if (!objectList.Any())
    {
        return "You see nothing of particular interest in this area.";
    }

    var names = objectList.Select(o => o.Name).ToList();

    if (names.Count == 1)
    {
        return $"You notice: {names[0]}.";
    }

    var lastItem = names.Last();
    var otherItems = string.Join(", ", names.Take(names.Count - 1));
    return $"You notice: {otherItems}, and {lastItem}.";
}
```

**Grammar**:
- Single object: `"You notice: Rusted Crate."`
- Multiple objects: `"You notice: Rusted Crate, Metal Locker, and Terminal."`

---

#### 11. Available Items Retrieval

**Purpose**: Get all items currently available in open containers for taking.

**Implementation** ([InteractionService.cs:506-529](../../RuneAndRust.Engine/Services/InteractionService.cs#L506-L529)):
```csharp
public async Task<IEnumerable<Item>> GetAvailableItemsAsync()
{
    if (_gameState.CurrentRoomId == null)
    {
        return Enumerable.Empty<Item>();
    }

    var allItems = new List<Item>();

    // Get items from all open containers
    var containers = await _objectRepository.GetByRoomIdAsync(_gameState.CurrentRoomId.Value);
    foreach (var container in containers.Where(c => c.IsContainer && c.IsOpen))
    {
        if (_containerLoot.TryGetValue(container.Id, out var items))
        {
            allItems.AddRange(items);
        }
    }

    return allItems;
}
```

**Behavior Details**:
- **No Dice Roll**: Unlike examine/search, this is a passive query (no WITS check).
- **Open Containers Only**: Only returns items from containers where `IsOpen == true`.
- **In-Memory Loot**: Items come from `_containerLoot` dictionary, not database.
- **Empty State**: Returns empty enumerable if no room set or no items available.

---

### Edge Cases

| Scenario | Handling | Location |
|----------|----------|----------|
| **No Current Room** | Return validation error message | [InteractionService.cs:78-82](../../RuneAndRust.Engine/Services/InteractionService.cs#L78-L82) |
| **No Current Character** | Return validation error message | [InteractionService.cs:84-88](../../RuneAndRust.Engine/Services/InteractionService.cs#L84-L88) |
| **Object Not Found** | Return `ExaminationResult.NotFound(targetName)` | [InteractionService.cs:95-100](../../RuneAndRust.Engine/Services/InteractionService.cs#L95-L100) |
| **Examine Lower Tier** | Description still shown, but `NewInfoRevealed = false` | [InteractionService.cs:114](../../RuneAndRust.Engine/Services/InteractionService.cs#L114) |
| **Open Already Open** | Return "already open" message | [InteractionService.cs:188-192](../../RuneAndRust.Engine/Services/InteractionService.cs#L188-L192) |
| **Open Locked Container** | Return "is locked" message | [InteractionService.cs:194-198](../../RuneAndRust.Engine/Services/InteractionService.cs#L194-L198) |
| **Close Closed Container** | Return "already closed" message | [InteractionService.cs:235-239](../../RuneAndRust.Engine/Services/InteractionService.cs#L235-L239) |
| **Search Empty Room** | Return "room appears empty" message | [InteractionService.cs:272-275](../../RuneAndRust.Engine/Services/InteractionService.cs#L272-L275) |
| **Search Fumble (< 0)** | Only reveal first object name | [InteractionService.cs:285-290](../../RuneAndRust.Engine/Services/InteractionService.cs#L285-L290) |
| **Search Closed Container** | Return "Open it first" error | [InteractionService.cs:393-397](../../RuneAndRust.Engine/Services/InteractionService.cs#L393-L397) |
| **Search Already Searched** | Return remaining loot or "nothing else remains" | [InteractionService.cs:399-411](../../RuneAndRust.Engine/Services/InteractionService.cs#L399-L411) |
| **Take from Closed Container** | Item not found (only searches open containers) | [InteractionService.cs:464](../../RuneAndRust.Engine/Services/InteractionService.cs#L464) |
| **Take Encumbered** | Item restored to container, inventory error returned | [InteractionService.cs:484-490](../../RuneAndRust.Engine/Services/InteractionService.cs#L484-L490) |

---

## Restrictions

### MUST Requirements

1. **MUST roll WITS for examine/search**: No auto-success; dice rolls are mandatory.
2. **MUST validate game state**: All operations require valid room and character.
3. **MUST persist examination tier**: `HighestExaminationTier` must be saved to prevent repeated rolls.
4. **MUST persist container state**: `IsOpen`, `HasBeenSearched` must be saved to database.
5. **MUST require open container for search**: Closed containers cannot be searched for loot.
6. **MUST generate loot only once**: First search generates loot; subsequent searches return remaining items.
7. **MUST validate item removal**: If `InventoryService.AddItemAsync()` fails, item must remain in container.

### MUST NOT Requirements

1. **MUST NOT allow auto-discovery**: All information reveals require WITS checks.
2. **MUST NOT regenerate loot**: Loot is generated once per container and persists until taken.
3. **MUST NOT bypass locked containers**: Open command must fail on locked containers.
4. **MUST NOT allow taking from closed containers**: `TakeItemAsync()` only searches open containers.
5. **MUST NOT show expert details without sufficient successes**: Tier thresholds are mandatory.
6. **MUST NOT duplicate items**: Items are removed from `_containerLoot` when taken.

---

## Limitations

### Numerical Constraints

- **WITS Range**: Typically 1-5 (base attribute value).
- **Tier Thresholds**: Detailed (1+ successes), Expert (3+ successes).
- **Container Loot Storage**: In-memory dictionary; cleared on application restart.
- **Description Field Lengths**: No hard limit, but typically 50-200 characters per tier.

### Functional Limitations

- **No Lockpicking**: Locked containers cannot be opened in current implementation.
- **No Container Capacity**: Containers have unlimited loot storage.
- **No Hidden Objects**: All objects in room are visible; search only adds detail.
- **No Skill Checks**: Only WITS-based checks; no Finesse/Might alternatives.
- **No Re-Examination Rolls**: Once expert tier reached, cannot roll again for better result.
- **No Trap Mechanics**: Containers do not have traps (deferred to `HazardService` integration).

### Loot System Limitations

- **No Item Stacking in Containers**: Loot generation creates individual item instances.
- **No Weight Limits**: Containers have no weight capacity.
- **No Quality Guarantees**: Loot quality is probabilistic based on danger level.
- **No Biome-Specific Guarantees**: Biome affects probability tables, not guaranteed item types.

---

## Use Cases

### UC-INTERACT-01: Base Tier Examination (0 Net Successes)

**Scenario**: Player with low WITS (2) examines object, gets 0 net successes.

**Setup**:
```csharp
var character = new Character { Name = "Novice", Wits = 2 };
_gameState.CurrentCharacter = character;
_gameState.CurrentRoomId = roomId;

var crate = new InteractableObject
{
    Name = "Rusted Crate",
    Description = "A rusted metal crate.",
    DetailedDescription = "The hinges are corroded but functional.",
    ExpertDescription = "Faint Pre-Glitch symbols are etched on the lid.",
    HighestExaminationTier = 0
};

_mockDice.Setup(d => d.Roll(2, It.IsAny<string>()))
    .Returns(new DiceResult { Successes = 0, Botches = 0, Rolls = new List<int> { 5, 6 } });
```

**Execution**:
```csharp
var result = await _sut.ExamineAsync("Rusted Crate");
```

**Expected Result**:
```csharp
result.Success == false  // 0 net = base only
result.NetSuccesses == 0
result.TierRevealed == 0
result.Description == "A rusted metal crate."
result.NewInfoRevealed == false  // Already had tier 0
```

---

### UC-INTERACT-02: Detailed Tier Examination (1-2 Net Successes)

**Scenario**: Player with moderate WITS (3) examines object, gets 2 net successes.

**Setup**:
```csharp
var character = new Character { Name = "Scout", Wits = 3 };
_gameState.CurrentCharacter = character;

var crate = new InteractableObject
{
    Name = "Rusted Crate",
    Description = "A rusted metal crate.",
    DetailedDescription = "The hinges are corroded but functional.",
    ExpertDescription = "Faint Pre-Glitch symbols are etched on the lid.",
    HighestExaminationTier = 0
};

_mockDice.Setup(d => d.Roll(3, It.IsAny<string>()))
    .Returns(new DiceResult { Successes = 2, Botches = 0, Rolls = new List<int> { 8, 9, 4 } });
```

**Execution**:
```csharp
var result = await _sut.ExamineAsync("Rusted Crate");
```

**Expected Result**:
```csharp
result.Success == true
result.NetSuccesses == 2
result.TierRevealed == 1  // Detailed tier
result.Description == "A rusted metal crate. The hinges are corroded but functional."
result.NewInfoRevealed == true  // Upgraded from tier 0 to tier 1
crate.HighestExaminationTier == 1  // Persisted
```

---

### UC-INTERACT-03: Expert Tier Examination (3+ Net Successes)

**Scenario**: Player with high WITS (5) examines object, gets 4 net successes.

**Setup**:
```csharp
var character = new Character { Name = "Sage", Wits = 5 };
_gameState.CurrentCharacter = character;

var crate = new InteractableObject
{
    Name = "Rusted Crate",
    Description = "A rusted metal crate.",
    DetailedDescription = "The hinges are corroded but functional.",
    ExpertDescription = "Faint Pre-Glitch symbols are etched on the lid.",
    HighestExaminationTier = 0
};

_mockDice.Setup(d => d.Roll(5, It.IsAny<string>()))
    .Returns(new DiceResult { Successes = 4, Botches = 0, Rolls = new List<int> { 8, 8, 9, 10, 5 } });
```

**Execution**:
```csharp
var result = await _sut.ExamineAsync("Rusted Crate");
```

**Expected Result**:
```csharp
result.Success == true
result.NetSuccesses == 4
result.TierRevealed == 2  // Expert tier
result.Description == "A rusted metal crate. The hinges are corroded but functional. Faint Pre-Glitch symbols are etched on the lid."
result.NewInfoRevealed == true
result.RevealedExpert == true
crate.HighestExaminationTier == 2
```

**Data Capture Trigger**:
- `DataCaptureService.TryGenerateFromExaminationAsync()` called
- Chance to generate lore fragment based on WITS bonus

---

### UC-INTERACT-04: Re-Examination (No New Info)

**Scenario**: Player re-examines object but rolls lower than previous best.

**Setup**:
```csharp
var character = new Character { Name = "Scout", Wits = 3 };
_gameState.CurrentCharacter = character;

var crate = new InteractableObject
{
    Name = "Rusted Crate",
    Description = "A rusted metal crate.",
    DetailedDescription = "The hinges are corroded but functional.",
    ExpertDescription = "Faint Pre-Glitch symbols are etched on the lid.",
    HighestExaminationTier = 2  // Previously examined with expert tier
};

_mockDice.Setup(d => d.Roll(3, It.IsAny<string>()))
    .Returns(new DiceResult { Successes = 1, Botches = 0, Rolls = new List<int> { 8, 4, 3 } });
```

**Execution**:
```csharp
var result = await _sut.ExamineAsync("Rusted Crate");
```

**Expected Result**:
```csharp
result.Success == true
result.NetSuccesses == 1
result.TierRevealed == 1  // Only detailed tier this time
result.Description == "A rusted metal crate. The hinges are corroded but functional."  // No expert text
result.NewInfoRevealed == false  // Did not exceed previous best (tier 2)
crate.HighestExaminationTier == 2  // Unchanged
```

---

### UC-INTERACT-05: Container Opening (Successful)

**Scenario**: Player opens a closed, unlocked container.

**Setup**:
```csharp
var container = new InteractableObject
{
    Name = "Metal Locker",
    IsContainer = true,
    IsOpen = false,
    IsLocked = false
};
```

**Execution**:
```csharp
var message = await _sut.OpenAsync("Metal Locker");
```

**Expected Result**:
```csharp
message == "You open the Metal Locker. It creaks on rusted hinges."
container.IsOpen == true
container.LastModified == DateTime.UtcNow  // Updated
```

---

### UC-INTERACT-06: Container Opening (Locked)

**Scenario**: Player attempts to open a locked container.

**Setup**:
```csharp
var container = new InteractableObject
{
    Name = "Secured Chest",
    IsContainer = true,
    IsOpen = false,
    IsLocked = true
};
```

**Execution**:
```csharp
var message = await _sut.OpenAsync("Secured Chest");
```

**Expected Result**:
```csharp
message == "The Secured Chest is locked. You need to unlock it first."
container.IsOpen == false  // Unchanged
```

---

### UC-INTERACT-07: Room Search (Successful)

**Scenario**: Player searches room with 2 net successes (detailed tier).

**Setup**:
```csharp
var character = new Character { Name = "Scout", Wits = 3 };
_gameState.CurrentCharacter = character;

var objects = new List<InteractableObject>
{
    new() { Name = "Rusted Crate", IsContainer = true, IsLocked = false },
    new() { Name = "Broken Terminal", IsContainer = false, IsLocked = false },
    new() { Name = "Secured Locker", IsContainer = true, IsLocked = true }
};

_mockDice.Setup(d => d.Roll(3, It.IsAny<string>()))
    .Returns(new DiceResult { Successes = 2, Botches = 0, Rolls = new List<int> { 8, 8, 4 } });
```

**Execution**:
```csharp
var message = await _sut.SearchAsync();
```

**Expected Result**:
```csharp
message == "Your careful search reveals: Rusted Crate, Broken Terminal, Secured Locker. 2 appear to be containers."
// Locked count not revealed (requires 3+ successes)
```

---

### UC-INTERACT-08: Room Search (Expert Tier)

**Scenario**: Player searches room with 4 net successes (expert tier).

**Setup**:
```csharp
var character = new Character { Name = "Sage", Wits = 5 };
_gameState.CurrentCharacter = character;

var objects = new List<InteractableObject>
{
    new() { Name = "Rusted Crate", IsContainer = true, IsLocked = false },
    new() { Name = "Secured Locker", IsContainer = true, IsLocked = true }
};

_mockDice.Setup(d => d.Roll(5, It.IsAny<string>()))
    .Returns(new DiceResult { Successes = 4, Botches = 0, Rolls = new List<int> { 8, 8, 9, 10, 5 } });
```

**Execution**:
```csharp
var message = await _sut.SearchAsync();
```

**Expected Result**:
```csharp
message == "Your careful search reveals: Rusted Crate, Secured Locker. 2 appear to be containers. You notice one is secured with a lock."
```

---

### UC-INTERACT-09: Container Loot Generation (First Search)

**Scenario**: Player searches open container for first time, loot is generated.

**Setup**:
```csharp
var character = new Character { Name = "Scout", Wits = 3 };
_gameState.CurrentCharacter = character;

var room = new Room
{
    BiomeType = BiomeType.Industrial,
    DangerLevel = DangerLevel.Moderate
};

var container = new InteractableObject
{
    Name = "Supply Crate",
    IsContainer = true,
    IsOpen = true,
    HasBeenSearched = false,
    LootTier = QualityTier.Scavenged
};

var generatedLoot = new List<Item>
{
    new() { Name = "Rusty Bolts", Value = 5, Weight = 100 },
    new() { Name = "Scrap Metal", Value = 10, Weight = 500 }
};

_mockLootService.Setup(l => l.SearchContainerAsync(container, It.IsAny<LootGenerationContext>()))
    .ReturnsAsync(LootResult.Found("You find some items.", generatedLoot.AsReadOnly()));
```

**Execution**:
```csharp
var result = await _sut.SearchContainerAsync("Supply Crate");
```

**Expected Result**:
```csharp
result.Success == true
result.Items.Count == 2
result.TotalValue == 15
result.TotalWeight == 600
_containerLoot[container.Id].Count == 2  // Stored for later taking
container.HasBeenSearched == true  // Persisted
```

**Loot Context**:
```csharp
context.BiomeType == BiomeType.Industrial
context.DangerLevel == DangerLevel.Moderate
context.LootTier == QualityTier.Scavenged
context.WitsBonus == 1  // Wits 3 / 2 = 1
```

---

### UC-INTERACT-10: Item Taking (Successful)

**Scenario**: Player takes item from open container.

**Setup**:
```csharp
var character = new Character { Name = "Scout", Wits = 3 };
_gameState.CurrentCharacter = character;

var container = new InteractableObject
{
    Id = Guid.NewGuid(),
    Name = "Supply Crate",
    IsContainer = true,
    IsOpen = true
};

var item = new Item { Name = "Rusty Bolts", Value = 5, Weight = 100 };

_containerLoot[container.Id] = new List<Item> { item };

_mockInventoryService.Setup(i => i.AddItemAsync(character, item))
    .ReturnsAsync(OperationResult.Success("Item added"));
```

**Execution**:
```csharp
var message = await _sut.TakeItemAsync("Rusty Bolts");
```

**Expected Result**:
```csharp
message == "You take the Rusty Bolts from the Supply Crate."
_containerLoot[container.Id].Count == 0  // Item removed
// Item added to character inventory via InventoryService
```

---

## Decision Trees

### 1. Examination Tier Determination

```
[ExamineAsync Entry]
    |
    ├─ Game state valid? ──No──> Return NotFound
    |     |
    |    Yes
    |     |
    ├─ Object exists in room? ──No──> Return NotFound
    |     |
    |    Yes
    |     |
    ├─ Roll WITS dice pool
    |     |
    ├─ Calculate net successes (S - B)
    |     |
    └─ Determine tier revealed:
         ├─ Net >= 3 ──> Tier 2 (Expert)
         ├─ Net >= 1 ──> Tier 1 (Detailed)
         └─ Net < 1  ──> Tier 0 (Base)
              |
         ├─ Tier > HighestExaminationTier? ──Yes──> Update object, NewInfo = true
         |     |
         |    No ──> NewInfo = false
         |     |
         ├─ Build description (base + detailed? + expert?)
         |     |
         ├─ Tier >= 3? ──Yes──> Attempt data capture
         |     |
         └─ Return ExaminationResult
```

---

### 2. Container Open Validation

```
[OpenAsync Entry]
    |
    ├─ Game state valid? ──No──> Return error message
    |     |
    |    Yes
    |     |
    ├─ Object exists? ──No──> "You don't see anything called..."
    |     |
    |    Yes
    |     |
    ├─ Is container? ──No──> "Cannot be opened"
    |     |
    |    Yes
    |     |
    ├─ Already open? ──Yes──> "Already open"
    |     |
    |    No
    |     |
    ├─ Is locked? ──Yes──> "Is locked. Unlock it first."
    |     |
    |    No
    |     |
    └─ Set IsOpen = true, persist, return success message
```

---

### 3. Container Search Flow

```
[SearchContainerAsync Entry]
    |
    ├─ Game state valid? ──No──> Return Failure
    |     |
    |    Yes
    |     |
    ├─ Container exists? ──No──> Return Failure("You don't see...")
    |     |
    |    Yes
    |     |
    ├─ Is container? ──No──> Return Failure("Cannot be searched")
    |     |
    |    Yes
    |     |
    ├─ Is open? ──No──> Return Failure("Open it first")
    |     |
    |    Yes
    |     |
    ├─ HasBeenSearched? ──Yes──> Return remaining loot or Empty
    |     |
    |    No (First search)
    |     |
    ├─ Get room context (BiomeType, DangerLevel)
    |     |
    ├─ Calculate WITS bonus (Wits / 2)
    |     |
    ├─ Create LootGenerationContext
    |     |
    ├─ Call LootService.SearchContainerAsync()
    |     |
    ├─ Store generated loot in _containerLoot[containerId]
    |     |
    ├─ Set HasBeenSearched = true, persist
    |     |
    ├─ Attempt data capture
    |     |
    └─ Return LootResult
```

---

### 4. Room Search Detail Tiers

```
[SearchAsync Entry]
    |
    ├─ Game state valid? ──No──> Return error message
    |     |
    |    Yes
    |     |
    ├─ Roll WITS dice pool
    |     |
    ├─ Get all objects in room
    |     |
    ├─ No objects? ──Yes──> "Room appears empty"
    |     |
    |    No
    |     |
    ├─ Calculate net successes
    |     |
    └─ Build result message:
         ├─ Net < 0 (Fumble) ──> "Hasty search" + 1st object only
         ├─ Net == 0         ──> "You search" + all object names
         ├─ Net >= 1         ──> Names + container count
         └─ Net >= 3         ──> Names + container count + locked count
```

---

### 5. Item Taking Workflow

```
[TakeItemAsync Entry]
    |
    ├─ Game state valid? ──No──> Return error message
    |     |
    |    Yes
    |     |
    ├─ Get all open containers in room
    |     |
    └─ For each open container:
         ├─ Loot exists in _containerLoot? ──No──> Continue loop
         |     |
         |    Yes
         |     |
         ├─ Item name matches (case-insensitive)? ──No──> Continue loop
         |     |
         |    Yes
         |     |
         ├─ Remove item from container loot
         |     |
         ├─ Call InventoryService.AddItemAsync()
         |     |
         ├─ Success? ──No──> Restore item to loot, return error
         |     |
         |    Yes
         |     |
         └─ Return success message
              |
         ├─ No match in any container? ──> "You don't see any..."
```

---

## Sequence Diagrams

### 1. Full Examination Flow (Expert Tier)

```
Player       InteractionService       DiceService       ObjectRepository       DataCaptureService
  |                  |                      |                    |                        |
  | ExamineAsync()   |                      |                    |                        |
  |----------------->|                      |                    |                        |
  |                  |                      |                    |                        |
  |                  | ValidateGameState()  |                    |                        |
  |                  | (room + character)   |                    |                        |
  |                  |                      |                    |                        |
  |                  | GetByNameInRoomAsync()|                   |                        |
  |                  |--------------------------------------------->|                        |
  |                  |<---------------------------------------------|                        |
  |                  |          InteractableObject                |                        |
  |                  |                      |                    |                        |
  |                  | Roll(WITS=5)         |                    |                        |
  |                  |--------------------->|                    |                        |
  |                  |<---------------------|                    |                        |
  |                  | DiceResult(4 successes)|                  |                        |
  |                  |                      |                    |                        |
  |                  | CalculateTierRevealed(4) → 2 (Expert)     |                        |
  |                  |                      |                    |                        |
  |                  | UpdateAsync(obj)     |                    |                        |
  |                  |--------------------------------------------->|                        |
  |                  | obj.HighestExaminationTier = 2            |                        |
  |                  |                      |                    |                        |
  |                  | SaveChangesAsync()   |                    |                        |
  |                  |--------------------------------------------->|                        |
  |                  |                      |                    |                        |
  |                  | TryGenerateFromExaminationAsync()         |                        |
  |                  |------------------------------------------------------------------>|
  |                  |<------------------------------------------------------------------|
  |                  |          CaptureResult (lore fragment)    |                        |
  |                  |                      |                    |                        |
  | ExaminationResult|                      |                    |                        |
  |<-----------------|                      |                    |                        |
```

---

### 2. Container Search and Loot Generation

```
Player       InteractionService       ObjectRepository       RoomRepository       LootService       DataCaptureService
  |                  |                      |                      |                    |                      |
  | SearchContainerAsync("Crate")          |                      |                    |                      |
  |----------------->|                      |                      |                    |                      |
  |                  |                      |                      |                    |                      |
  |                  | GetByNameInRoomAsync()|                     |                    |                      |
  |                  |--------------------->|                      |                    |                      |
  |                  |<---------------------|                      |                    |                      |
  |                  |    Container (IsOpen=true, HasBeenSearched=false)               |                      |
  |                  |                      |                      |                    |                      |
  |                  | GetByIdAsync(roomId) |                      |                    |                      |
  |                  |-------------------------------------->|     |                    |                      |
  |                  |<--------------------------------------|     |                    |                      |
  |                  |    Room (BiomeType, DangerLevel)      |     |                    |                      |
  |                  |                      |                      |                    |                      |
  |                  | Calculate context:   |                      |                    |                      |
  |                  | - BiomeType = Industrial                    |                    |                      |
  |                  | - DangerLevel = Moderate                    |                    |                      |
  |                  | - LootTier = Scavenged                      |                    |                      |
  |                  | - WitsBonus = 1 (Wits 3 / 2)                |                    |                      |
  |                  |                      |                      |                    |                      |
  |                  | SearchContainerAsync(container, context)   |                    |                      |
  |                  |-------------------------------------------------------->|         |                      |
  |                  |<--------------------------------------------------------|         |                      |
  |                  |    LootResult (2 items generated)          |                    |                      |
  |                  |                      |                      |                    |                      |
  |                  | Store in _containerLoot[containerId]       |                    |                      |
  |                  |                      |                      |                    |                      |
  |                  | UpdateAsync(container)|                     |                    |                      |
  |                  | HasBeenSearched = true|                     |                    |                      |
  |                  |--------------------->|                      |                    |                      |
  |                  |                      |                      |                    |                      |
  |                  | TryGenerateFromSearchAsync()               |                    |                      |
  |                  |-------------------------------------------------------------------------------->|
  |                  |<--------------------------------------------------------------------------------|
  |                  |                      |                      |                    |                      |
  | LootResult       |                      |                      |                    |                      |
  |<-----------------|                      |                      |                    |                      |
```

---

### 3. Item Taking with Inventory Integration

```
Player       InteractionService       ObjectRepository       InventoryService       Character
  |                  |                      |                      |                      |
  | TakeItemAsync("Rusty Bolts")           |                      |                      |
  |----------------->|                      |                      |                      |
  |                  |                      |                      |                      |
  |                  | GetByRoomIdAsync()   |                      |                      |
  |                  |--------------------->|                      |                      |
  |                  |<---------------------|                      |                      |
  |                  |  [Open containers]   |                      |                      |
  |                  |                      |                      |                      |
  |                  | Search _containerLoot|                      |                      |
  |                  | for item match       |                      |                      |
  |                  |                      |                      |                      |
  |                  | Remove item from loot|                      |                      |
  |                  |                      |                      |                      |
  |                  | AddItemAsync(character, item)              |                      |
  |                  |------------------------------------------>|                      |
  |                  |                      |                      |                      |
  |                  |                      |                      | Add to inventory     |
  |                  |                      |                      |--------------------->|
  |                  |                      |                      |                      |
  |                  |<------------------------------------------|                      |
  |                  |     OperationResult.Success               |                      |
  |                  |                      |                      |                      |
  | Success message  |                      |                      |                      |
  |<-----------------|                      |                      |                      |
```

---

## Workflows

### Workflow 1: Object Examination Checklist

**Purpose**: Step-by-step process for examining an object with WITS check.

**Preconditions**:
- Player in valid room with active character
- Object exists in current room

**Steps**:
1. ☐ **Validate Game State**: Check `CurrentRoomId` and `CurrentCharacter` are set ([InteractionService.cs:78-88](../../RuneAndRust.Engine/Services/InteractionService.cs#L78-L88))
2. ☐ **Find Object**: Query `IInteractableObjectRepository.GetByNameInRoomAsync()` ([InteractionService.cs:91-93](../../RuneAndRust.Engine/Services/InteractionService.cs#L91-L93))
3. ☐ **Roll WITS**: Get character WITS attribute, call `DiceService.Roll()` ([InteractionService.cs:103-104](../../RuneAndRust.Engine/Services/InteractionService.cs#L103-L104))
4. ☐ **Calculate Net Successes**: `Successes - Botches` ([InteractionService.cs:106](../../RuneAndRust.Engine/Services/InteractionService.cs#L106))
5. ☐ **Determine Tier**: Use `CalculateTierRevealed()` to map net successes to tier (0/1/2) ([InteractionService.cs:111](../../RuneAndRust.Engine/Services/InteractionService.cs#L111))
6. ☐ **Check New Info**: Compare `tierRevealed` to `object.HighestExaminationTier` ([InteractionService.cs:114](../../RuneAndRust.Engine/Services/InteractionService.cs#L114))
7. ☐ **Update State**: If new tier reached, update object and persist to database ([InteractionService.cs:117-123](../../RuneAndRust.Engine/Services/InteractionService.cs#L117-L123))
8. ☐ **Build Description**: Concatenate base/detailed/expert fields based on tier ([InteractionService.cs:130](../../RuneAndRust.Engine/Services/InteractionService.cs#L130))
9. ☐ **Attempt Data Capture**: If expert tier (3+ successes), call `DataCaptureService` ([InteractionService.cs:133-147](../../RuneAndRust.Engine/Services/InteractionService.cs#L133-L147))
10. ☐ **Return Result**: Create `ExaminationResult` record with all metadata ([InteractionService.cs:149-156](../../RuneAndRust.Engine/Services/InteractionService.cs#L149-L156))

**Postconditions**:
- Object state persisted if new tier revealed
- Player receives description text appropriate to tier
- Lore fragment possibly generated on expert tier

---

### Workflow 2: Container Loot Workflow

**Purpose**: Complete workflow from opening to taking items.

**Steps**:
1. ☐ **Open Container**: Call `OpenAsync(targetName)` ([InteractionService.cs:165-209](../../RuneAndRust.Engine/Services/InteractionService.cs#L165-L209))
   - Validates container exists, is unlocked, not already open
   - Sets `IsOpen = true`, persists
2. ☐ **Search Container**: Call `SearchContainerAsync(targetName)` ([InteractionService.cs:366-452](../../RuneAndRust.Engine/Services/InteractionService.cs#L366-452))
   - Validates container is open, not already searched
   - Gets room context (biome, danger level)
   - Calculates WITS bonus (`Wits / 2`)
   - Calls `LootService.SearchContainerAsync()` with generation context
   - Stores loot in `_containerLoot` dictionary
   - Sets `HasBeenSearched = true`, persists
3. ☐ **Review Loot**: Check `LootResult.Items` list
4. ☐ **Take Items**: Call `TakeItemAsync(itemName)` for each desired item ([InteractionService.cs:455-503](../../RuneAndRust.Engine/Services/InteractionService.cs#L455-L503))
   - Searches `_containerLoot` for item name match
   - Removes item from container loot
   - Calls `InventoryService.AddItemAsync()` to add to player inventory
   - Restores item to container if inventory add fails
5. ☐ **Close Container** (Optional): Call `CloseAsync(targetName)` ([InteractionService.cs:212-250](../../RuneAndRust.Engine/Services/InteractionService.cs#L212-L250))
   - Sets `IsOpen = false`, persists
   - Loot remains in `_containerLoot` (not cleared)

**Postconditions**:
- Items transferred from container to player inventory
- Loot generated once, persists until taken

---

## Cross-System Integration

### Integration Matrix

| System | Interface | Purpose | Direction |
|--------|-----------|---------|-----------|
| **Dice System** | `IDiceService` | Executes WITS-based rolls for examine/search | Interaction → Dice |
| **Loot System** | `ILootService` | Generates procedural loot for containers | Interaction → Loot |
| **Inventory System** | `IInventoryService` | Adds taken items to player inventory | Interaction → Inventory |
| **Data Capture** | `IDataCaptureService` | Generates lore fragments from expert examinations | Interaction → Capture |
| **Object Repository** | `IInteractableObjectRepository` | Persists object state (examination tier, open/searched) | Interaction → Repository |
| **Room Repository** | `IRoomRepository` | Provides biome/danger context for loot generation | Interaction → Repository |

---

### Integration Details

#### 1. Dice System Integration

**Direction**: InteractionService → DiceService

**Mechanism**:
```csharp
var wits = _gameState.CurrentCharacter.GetAttribute(CharacterAttribute.Wits);
var diceResult = _diceService.Roll(wits, $"Examine {targetName}");
var netSuccesses = diceResult.Successes - diceResult.Botches;
```

**Usage**:
- Examine: Rolls WITS to determine description tier
- Search: Rolls WITS to determine room detail level

---

#### 2. Loot System Integration

**Direction**: InteractionService → LootService

**Mechanism**:
```csharp
var context = new LootGenerationContext(biome, danger, container.LootTier, witsBonus);
var lootResult = await _lootService.SearchContainerAsync(container, context);
```

**Context Parameters**:
- `BiomeType`: From `Room.BiomeType` (affects item type tables)
- `DangerLevel`: From `Room.DangerLevel` (affects quality distribution)
- `LootTier`: From `InteractableObject.LootTier` (optional quality override)
- `WitsBonus`: `character.Wits / 2` (affects quality rolls)

**Data Flow**:
- `LootService` generates `List<Item>` based on context
- `InteractionService` stores items in `_containerLoot` dictionary
- Items persist until explicitly taken via `TakeItemAsync()`

---

#### 3. Inventory System Integration

**Direction**: InteractionService → InventoryService

**Mechanism**:
```csharp
var addResult = await _inventoryService.AddItemAsync(_gameState.CurrentCharacter, item);
if (!addResult.Success)
{
    // Restore item to container if add failed
    items.Add(item);
    return addResult.Message;  // Return error (e.g., "Encumbered")
}
```

**Transactional Behavior**:
- Item removed from `_containerLoot` before inventory add
- If add fails (encumbrance, invalid item), item restored to container
- Error message passed through to player

---

#### 4. Data Capture Integration

**Direction**: InteractionService → DataCaptureService

**Mechanism**:
```csharp
if (tierRevealed >= ExpertTierThreshold && _captureService != null)
{
    var witsBonus = _gameState.CurrentCharacter.GetAttribute(CharacterAttribute.Wits) / 2;
    var captureResult = await _captureService.TryGenerateFromExaminationAsync(
        _gameState.CurrentCharacter.Id,
        interactableObject,
        tierRevealed,
        witsBonus);

    if (captureResult.Success)
    {
        description += $"\n\n[Data Captured: {captureResult.Capture!.Type}]";
    }
}
```

**Trigger Conditions**:
- Expert tier examination (3+ net successes)
- `DataCaptureService` not null (optional dependency)

**Lore Generation**:
- Chance to generate lore fragments based on object metadata and WITS bonus
- Fragment appended to description text
- See SPEC-CODEX-001 for lore fragment details

---

## Data Models

### 1. ExaminationResult (Record)

**Purpose**: Contains WITS check results and revealed description.

**Definition** ([ExaminationResult.cs:13-50](../../RuneAndRust.Core/Models/ExaminationResult.cs#L13-L50)):
```csharp
public record ExaminationResult(
    bool Success,               // Tier > 0
    int NetSuccesses,           // Successes - Botches
    int TierRevealed,           // 0 = base, 1 = detailed, 2 = expert
    string Description,         // Combined description text
    bool NewInfoRevealed,       // Tier > HighestExaminationTier
    IReadOnlyList<int> Rolls    // Raw dice results
);
```

**Convenience Properties**:
```csharp
public bool IsFumble => NetSuccesses < 0;
public bool RevealedExpert => TierRevealed >= 2;
public bool RevealedDetailed => TierRevealed >= 1;
```

---

### 2. LootResult (Record)

**Purpose**: Contains procedurally generated loot items and metadata.

**Definition** ([LootResult.cs:15-54](../../RuneAndRust.Core/Models/LootResult.cs#L15-L54)):
```csharp
public record LootResult(
    bool Success,
    string Message,
    IReadOnlyList<Item> Items,
    int TotalValue,   // Sum of item.Value
    int TotalWeight   // Sum of item.Weight
);
```

**Factory Methods**:
```csharp
public static LootResult Found(string message, IReadOnlyList<Item> items);
public static LootResult Empty(string message);
public static LootResult Failure(string message);
```

---

### 3. LootGenerationContext (Record)

**Purpose**: Parameters for procedural loot generation.

**Definition** ([LootResult.cs:63-68](../../RuneAndRust.Core/Models/LootResult.cs#L63-L68)):
```csharp
public record LootGenerationContext(
    BiomeType BiomeType,
    DangerLevel DangerLevel,
    QualityTier? LootTier,
    int WitsBonus = 0
);
```

---

### 4. InteractableObject (Entity)

**Purpose**: Persistent storage for objects in rooms.

**Relevant Properties**:
```csharp
public class InteractableObject
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }            // Base tier
    public string? DetailedDescription { get; set; }   // Tier 1
    public string? ExpertDescription { get; set; }     // Tier 2

    public bool HasBeenExamined { get; set; }
    public int HighestExaminationTier { get; set; }    // 0/1/2

    public bool IsContainer { get; set; }
    public bool IsOpen { get; set; }
    public bool IsLocked { get; set; }
    public bool HasBeenSearched { get; set; }
    public QualityTier? LootTier { get; set; }

    public DateTime LastModified { get; set; }
}
```

---

## Configuration

### Tier Threshold Constants

**Location**: [InteractionService.cs:34-39](../../RuneAndRust.Engine/Services/InteractionService.cs#L34-L39)

```csharp
/// <summary>
/// Net successes required for detailed tier (1+).
/// </summary>
private const int DetailedTierThreshold = 1;

/// <summary>
/// Net successes required for expert tier (3+).
/// </summary>
private const int ExpertTierThreshold = 3;
```

**Tier Mapping Table**:
| Net Successes | Tier | Threshold Constant | Description Fields |
|---------------|------|-------------------|-------------------|
| < 1 | 0 | N/A | Base only |
| 1-2 | 1 | `DetailedTierThreshold` | Base + Detailed |
| 3+ | 2 | `ExpertTierThreshold` | Base + Detailed + Expert |

---

### WITS Bonus Calculation

```csharp
var witsBonus = character.GetAttribute(CharacterAttribute.Wits) / 2;
```

**Examples**:
- Wits 1 → Bonus 0
- Wits 2 → Bonus 1
- Wits 3 → Bonus 1
- Wits 4 → Bonus 2
- Wits 5 → Bonus 2

**Usage**: Affects loot quality rolls in `LootService` and data capture probability in `DataCaptureService`.

---

## Testing

### Test Suite Summary

**File**: [InteractionServiceTests.cs](../../RuneAndRust.Tests/Engine/InteractionServiceTests.cs) (601 lines)

**Coverage**:
- Examination tests (tiers, state persistence, data capture)
- Container tests (open/close validation chains)
- Search tests (room search, fumble handling, detail tiers)
- Loot tests (generation, re-search, taking)
- Edge cases (validation failures, empty rooms, locked containers)

---

## Domain 4 Compliance

All player-facing text is validated for Domain 4 compliance:

**Compliant Examples**:
- ✅ `"A rusted metal crate."` (observable description)
- ✅ `"The hinges are corroded but functional."` (physical state assessment)
- ✅ `"Faint Pre-Glitch symbols are etched on the lid."` (visual observation)
- ✅ `"It creaks on rusted hinges."` (audible observation)

**Avoided Patterns**:
- ❌ `"Container integrity: 78%"` (precision measurement)
- ❌ `"Scan reveals 3 items inside"` (technological omniscience)
- ❌ `"Lock mechanism rated at Level 5 security"` (technical specification)

---

## Future Extensions

### Planned v0.3.0 - Lockpicking Mechanics

**Current Limitation**: Locked containers cannot be opened.

**Planned Feature**:
```csharp
public async Task<string> PickLockAsync(string targetName)
{
    // Finesse-based dice check vs lock DC
    var finesse = _gameState.CurrentCharacter.GetAttribute(CharacterAttribute.Finesse);
    var diceResult = _diceService.Roll(finesse, "Pick lock");

    if (diceResult.NetSuccesses >= container.LockDC)
    {
        container.IsLocked = false;
        return "You successfully pick the lock.";
    }

    return "The lock resists your efforts.";
}
```

---

## Changelog

### v1.0.1 (2025-12-25)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- **ADD:** Documented `GetAvailableItemsAsync()` method (Secondary Behavior #11)
- Added code traceability remarks to implementation files:
  - `IInteractionService.cs` - interface spec reference
  - `InteractionService.cs` - service spec reference

### v1.0.0 (2025-12-22)
- ✅ Initial specification created
- ✅ Documented all interaction mechanics (examine, search, loot, take)
- ✅ 10 use cases documented with code walkthroughs
- ✅ 5 decision trees created
- ✅ 3 sequence diagrams created
- ✅ 2 workflows documented
- ✅ Cross-system integration matrix completed
- ✅ Domain 4 compliance verification

---

## Notes

- **Loot Persistence**: `_containerLoot` dictionary is in-memory; cleared on application restart. Future version will persist to database.
- **WITS Scaling**: Higher WITS characters reveal more information and get better loot quality, rewarding the attribute investment.
- **State Tracking**: Examination tier and container search state are persisted to prevent redundant rolling.
- **Data Capture Integration**: Expert tier examinations (3+ successes) integrate with Scavenger's Journal lore system (SPEC-CODEX-001).

---

**Specification Status**: ✅ Complete and verified against actual implementation (InteractionService.cs v0.2.2c)
