# v0.13 Persistent World State System - Integration Guide

## Overview

This guide explains how to integrate the v0.13 Persistent World State System into the main game loop. The system enables player actions to permanently alter generated Sectors through delta-based state recording and reconstruction.

## Architecture Summary

**Core Components:**

- `WorldStateChange` - Model for recording state changes
- `WorldStateRepository` - Database persistence layer
- `DestructionService` - Handles element destruction and state application
- Command handlers for `destroy` and `history` commands

**Flow:**

1. Player performs action (destroy terrain, defeat enemy)
2. Action recorded to `world_state_changes` table
3. On room load, changes applied to base generated state
4. Result: Persistent, player-modified world

---

## Step 1: Initialize Services in Program.cs

Add these service instances to your `Program.cs`:

```csharp
// v0.13: World state persistence services
private static WorldStateRepository _worldStateRepository = new();
private static DestructionService _destructionService = new(_worldStateRepository, _diceService);

```

---

## Step 2: Integrate with Room Generation

### In GameWorld Room Loading

After a room is generated (via `DungeonGenerator` and `RoomInstantiator`), apply world state changes:

```csharp
// After room generation in GameWorld or DungeonGenerator
public Room LoadRoomWithState(string roomId, int saveId)
{
    // Step 1: Generate base room (existing v0.10-v0.12 logic)
    var room = _roomInstantiator.InstantiateRoom(node, roomId, dungeonId);

    // Step 2: Apply population (v0.11)
    _populationPipeline.PopulateRoom(room, biome, dungeonSeed);

    // Step 3: Apply world state changes (v0.13 NEW)
    var sectorSeed = ExtractSectorSeed(roomId);
    var changes = _worldStateRepository.GetChangesForRoom(saveId, sectorSeed, roomId);

    if (changes.Count > 0)
    {
        _destructionService.ApplyWorldStateChanges(room, changes, saveId);
        _log.Information("Applied {Count} world state changes to room: {RoomId}",
            changes.Count, roomId);
    }

    // Step 4: Initialize destructible element HP tracking
    _destructionService.InitializeRoomElements(room);

    return room;
}

private string ExtractSectorSeed(string roomId)
{
    // room_d1_n5 -> "d1"
    if (roomId.StartsWith("room_d"))
    {
        var parts = roomId.Split('_');
        return parts.Length >= 2 ? parts[1] : roomId;
    }
    return roomId;
}

```

---

## Step 3: Add Command Handlers

### Destroy Command Handler

Add this handler to your main command processing loop in `Program.cs`:

```csharp
case CommandType.Destroy:
    HandleDestroy(parsed.Target);
    break;

```

Implement the handler:

```csharp
private static void HandleDestroy(string target)
{
    if (string.IsNullOrWhiteSpace(target))
    {
        Console.WriteLine("What do you want to destroy? (e.g., 'destroy pillar', 'smash grating')");
        return;
    }

    var currentRoom = _gameState.World.GetCurrentRoom(_player);
    if (currentRoom == null)
    {
        Console.WriteLine("You are not in a valid location.");
        return;
    }

    // Get save ID for recording changes
    var saveId = _worldStateRepository.GetSaveIdForCharacter(_player.Name);
    if (saveId == null)
    {
        _log.Warning("No save ID found for character: {CharacterName}", _player.Name);
        Console.WriteLine("Unable to save world changes. Please save your game first.");
        return;
    }

    var turnNumber = _gameState.TurnNumber; // Track for ordering

    // Try to find matching static terrain
    var terrain = FindTerrainByName(currentRoom, target);
    if (terrain != null)
    {
        var result = _destructionService.AttemptDestroyTerrain(
            currentRoom,
            terrain,
            _player,
            saveId.Value,
            turnNumber);

        Console.WriteLine(result.Message);

        if (result.WasDestroyed)
        {
            // Remove from room
            currentRoom.StaticTerrain.Remove(terrain);

            // Add rubble if applicable
            if (result.SpawnRubble)
            {
                currentRoom.StaticTerrain.Add(new StaticTerrain
                {
                    TerrainId = $"rubble_from_{terrain.TerrainId}",
                    Name = "Rubble Pile",
                    Description = $"Debris from the destroyed {terrain.Name.ToLower()}.",
                    Type = StaticTerrainType.RubblePile,
                    CoverProvided = CoverType.Partial,
                    AccuracyModifier = -2,
                    IsDifficultTerrain = true,
                    MovementCostModifier = 2,
                    IsDestructible = true,
                    HP = 15
                });

                Console.WriteLine("A pile of rubble now occupies the space.");
            }
        }
        return;
    }

    // Try to find matching dynamic hazard
    var hazard = FindHazardByName(currentRoom, target);
    if (hazard != null)
    {
        var result = _destructionService.AttemptDestroyHazard(
            currentRoom,
            hazard,
            _player,
            saveId.Value,
            turnNumber);

        Console.WriteLine(result.Message);

        if (result.WasDestroyed)
        {
            // Remove from room
            currentRoom.DynamicHazards.Remove(hazard);

            // Handle secondary effects
            if (result.CausedSecondaryEffect)
            {
                Console.WriteLine("The destruction causes a violent secondary reaction!");

                // Apply damage if it's an explosive hazard
                if (hazard.Type == DynamicHazardType.PressurizedPipe)
                {
                    var damage = _diceService.RollDice(2, 6);
                    _player.HP -= damage;
                    Console.WriteLine($"You take {damage} damage from the explosion!");
                }
            }
        }
        return;
    }

    // No matching element found
    Console.WriteLine($"You cannot find '{target}' to destroy.");
}

private static StaticTerrain? FindTerrainByName(Room room, string target)
{
    var normalized = target.ToLower().Trim();

    return room.StaticTerrain.FirstOrDefault(t =>
        t.Name.ToLower().Contains(normalized) ||
        t.Type.ToString().ToLower().Contains(normalized) ||
        normalized.Contains(t.Type.ToString().ToLower()));
}

private static DynamicHazard? FindHazardByName(Room room, string target)
{
    var normalized = target.ToLower().Trim();

    return room.DynamicHazards.FirstOrDefault(h =>
        h.Name.ToLower().Contains(normalized) ||
        h.Type.ToString().ToLower().Contains(normalized) ||
        normalized.Contains(h.Type.ToString().ToLower()));
}

```

### History Command Handler

Add this handler:

```csharp
case CommandType.History:
    HandleHistory();
    break;

```

Implement the handler:

```csharp
private static void HandleHistory()
{
    var currentRoom = _gameState.World.GetCurrentRoom(_player);
    if (currentRoom == null)
    {
        Console.WriteLine("You are not in a valid location.");
        return;
    }

    var saveId = _worldStateRepository.GetSaveIdForCharacter(_player.Name);
    if (saveId == null)
    {
        Console.WriteLine("No save history available.");
        return;
    }

    var sectorSeed = ExtractSectorSeed(currentRoom.RoomId);
    var changes = _worldStateRepository.GetChangesForRoom(saveId.Value, sectorSeed, currentRoom.RoomId);

    if (changes.Count == 0)
    {
        Console.WriteLine("This room is unchanged from its original state.");
        return;
    }

    Console.WriteLine($"\\n**Room History: {currentRoom.Name}**");
    Console.WriteLine($"Total modifications: {changes.Count}\\n");

    foreach (var change in changes.OrderBy(c => c.Timestamp))
    {
        var timeAgo = FormatTimeAgo(change.Timestamp);
        var description = GetChangeDescription(change);

        Console.WriteLine($"- {description} ({timeAgo})");
    }
    Console.WriteLine();
}

private static string GetChangeDescription(WorldStateChange change)
{
    return change.ChangeType switch
    {
        WorldStateChangeType.TerrainDestroyed =>
            $"Destroyed {GetElementName(change.TargetId)}",
        WorldStateChangeType.HazardDestroyed =>
            $"Disabled {GetElementName(change.TargetId)}",
        WorldStateChangeType.EnemyDefeated =>
            $"Defeated {change.TargetId}",
        WorldStateChangeType.LootCollected =>
            $"Collected loot from {GetElementName(change.TargetId)}",
        _ => $"Modified {change.TargetId}"
    };
}

private static string GetElementName(string targetId)
{
    // Convert "pillar_1" to "Pillar"
    var parts = targetId.Split('_');
    if (parts.Length > 0)
    {
        var name = parts[0];
        return char.ToUpper(name[0]) + name.Substring(1);
    }
    return targetId;
}

private static string FormatTimeAgo(DateTime timestamp)
{
    var elapsed = DateTime.UtcNow - timestamp;

    if (elapsed.TotalMinutes < 1)
        return "just now";
    if (elapsed.TotalMinutes < 60)
        return $"{(int)elapsed.TotalMinutes} min ago";
    if (elapsed.TotalHours < 24)
        return $"{(int)elapsed.TotalHours} hr ago";
    return $"{(int)elapsed.TotalDays} days ago";
}

```

---

## Step 4: Integrate with Combat System

### Record Enemy Defeats

In your combat resolution logic (likely in `CombatEngine.cs` or `Program.cs`), add world state recording:

```csharp
// After enemy is defeated in combat
public void OnEnemyDefeated(Enemy enemy, Room currentRoom, int saveId, int turnNumber)
{
    // Existing logic: remove from combat, drop loot, etc.

    // v0.13: Record defeat to world state
    _destructionService.RecordEnemyDefeat(
        currentRoom,
        enemy,
        saveId,
        turnNumber,
        droppedLoot: enemy.DroppedLoot != null,
        lootDropId: enemy.DroppedLoot?.Id);

    _log.Information("Enemy defeat recorded to world state: {EnemyName}", enemy.Name);
}

```

### Combat-Driven Terrain Destruction

For area-of-effect abilities that damage terrain:

```csharp
// In ability execution logic
public void ExecuteExplosiveAbility(Ability ability, Room room, int saveId, int turnNumber)
{
    // Apply combat damage to combatants (existing logic)

    // v0.13: Apply damage to destructible terrain in area
    foreach (var terrain in room.StaticTerrain.Where(t => t.IsDestructible))
    {
        if (IsInAffectedArea(terrain, ability.AoERadius))
        {
            var result = _destructionService.AttemptDestroyTerrain(
                room,
                terrain,
                _player,
                saveId,
                turnNumber);

            if (result.WasDestroyed)
            {
                Console.WriteLine($"The {terrain.Name} is destroyed by the explosion!");
                room.StaticTerrain.Remove(terrain);

                if (result.SpawnRubble)
                {
                    // Add rubble (see HandleDestroy for implementation)
                }
            }
        }
    }
}

```

---

## Step 5: Update Save/Load Flow

### On Save

No changes needed - world state changes are automatically persisted to the database as they occur.

### On Load

Ensure the save ID is accessible after loading:

```csharp
public void LoadGame(string characterName)
{
    // Existing load logic
    var (player, worldState, roomItemsJson, npcStatesJson, dungeonSeed, biomeId) =
        _saveRepository.LoadGame(characterName);

    // Store save ID for world state queries
    var saveId = _worldStateRepository.GetSaveIdForCharacter(characterName);
    _currentSaveId = saveId; // Store in game state

    // Rest of load logic...
}

```

---

## Step 6: Update Room Description

Modify room description generation to show modifications:

```csharp
public static void DisplayRoomDescription(Room room, int saveId)
{
    Console.WriteLine($"\\n{room.Name}");
    Console.WriteLine(room.Description);

    // v0.13: Show modification indicator
    var changeCount = _worldStateRepository.GetChangeCountForRoom(saveId, room.RoomId);
    if (changeCount > 0)
    {
        Console.WriteLine($"\\n**[Modified by player actions: {changeCount} changes]**");
    }

    // Show terrain
    if (room.StaticTerrain.Count > 0)
    {
        Console.WriteLine("\\n**Terrain:**");
        foreach (var terrain in room.StaticTerrain)
        {
            var modifier = terrain.CoverProvided != CoverType.None
                ? $" (Cover: {terrain.CoverProvided})"
                : "";

            // Show if generated from destruction
            var origin = terrain.TerrainId.Contains("rubble_from_")
                ? " [Debris]"
                : "";

            Console.WriteLine($"- {terrain.Name}{modifier}{origin}");
        }
    }

    // ... rest of description (enemies, hazards, etc.)
}

```

---

## Step 7: Testing Checklist

After integration, test these scenarios:

### Basic Destruction

- [ ]  Destroy a pillar - verify it's removed and rubble spawns
- [ ]  Destroy rubble pile - verify it's removed
- [ ]  Destroy a hazard - verify it's disabled
- [ ]  Try to destroy non-destructible element - verify failure message

### Persistence

- [ ]  Destroy terrain, save, quit, reload - verify still destroyed
- [ ]  Defeat enemy, save, quit, reload - verify enemy doesn't respawn
- [ ]  Apply multiple changes to same room - verify all persist

### History Command

- [ ]  Run `history` in modified room - verify changes listed
- [ ]  Run `history` in unmodified room - verify "unchanged" message

### Combat Integration

- [ ]  Defeat enemy in combat - verify recorded to world state
- [ ]  Use AoE ability near terrain - verify terrain takes damage

### Performance

- [ ]  Load room with 0 changes - verify fast (<10ms)
- [ ]  Load room with 10 changes - verify acceptable (<50ms)
- [ ]  Load room with 50 changes - verify acceptable (<100ms)

---

## Troubleshooting

### Changes not persisting

- Verify `WorldStateRepository` is initialized
- Check that save ID is retrieved correctly
- Confirm database table created: `world_state_changes`

### Room state not applying on load

- Verify `ApplyWorldStateChanges` is called after room generation
- Check that sector seed extraction is correct
- Ensure changes queried for correct save ID

### Performance issues

- Check change count per room (should be < 50)
- Consider implementing `WorldStateCache` (Phase 6)
- Run `WorldStateCompactor` periodically

---

## Next Steps

After basic integration is complete:

1. **Phase 6: Performance Optimization**
    - Implement `WorldStateCache` for frequently loaded rooms
    - Implement `WorldStateCompactor` to reduce redundant changes
2. **Phase 8: Testing**
    - Write comprehensive unit tests
    - Add integration tests for save/load cycle
3. **Future Enhancements**
    - Time-based decay (terrain regenerates after weeks?)
    - Settlement building (create new terrain)
    - Environmental puzzles (require destruction to solve)

---

## File Reference

**Core Models:**

- `RuneAndRust.Core/WorldStateChange.cs`
- `RuneAndRust.Core/DestructibleElement.cs`

**Services:**

- `RuneAndRust.Persistence/WorldStateRepository.cs`
- `RuneAndRust.Engine/DestructionService.cs`

**Command System:**

- `RuneAndRust.Engine/CommandParser.cs` (updated with Destroy/History)

**Integration Points:**

- `Program.cs` - Command handlers
- `GameWorld.cs` or room loading logic - State application
- `CombatEngine.cs` - Enemy defeat recording

---

## Version

**Implementation:** v0.13
**Date:** 2025
**Author:** Claude Code (Anthropic)
**Status:** Ready for Integration