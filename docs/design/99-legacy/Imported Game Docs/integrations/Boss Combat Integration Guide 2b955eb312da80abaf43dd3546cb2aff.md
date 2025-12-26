# Boss Combat Integration Guide

## v0.23 Boss Encounter System

This guide explains how to integrate the v0.23 Boss Encounter System into your existing combat loop using the `BossCombatIntegration` service.

---

## Table of Contents

1. [Overview](Boss%20Combat%20Integration%20Guide%202b955eb312da80abaf43dd3546cb2aff.md)
2. [Setup & Initialization](Boss%20Combat%20Integration%20Guide%202b955eb312da80abaf43dd3546cb2aff.md)
3. [Combat Loop Integration Points](Boss%20Combat%20Integration%20Guide%202b955eb312da80abaf43dd3546cb2aff.md)
4. [Code Examples](Boss%20Combat%20Integration%20Guide%202b955eb312da80abaf43dd3546cb2aff.md)
5. [UI Integration](Boss%20Combat%20Integration%20Guide%202b955eb312da80abaf43dd3546cb2aff.md)
6. [Testing](Boss%20Combat%20Integration%20Guide%202b955eb312da80abaf43dd3546cb2aff.md)

---

## Overview

The `BossCombatIntegration` service provides a clean interface between your combat engine and the boss encounter system. It handles:

- Boss encounter initialization
- Phase transitions (with add spawning)
- Telegraphed ability mechanics
- Interrupt system
- Enrage triggers
- Vulnerability windows
- Boss-specific loot generation

---

## Setup & Initialization

### 1. Add Boss Combat Integration to CombatEngine

In your `CombatEngine.cs` constructor, add the boss integration service:

```csharp
public class CombatEngine
{
    // Existing services...
    private readonly BossCombatIntegration? _bossIntegration;

    public CombatEngine(
        DiceService diceService,
        // ... other dependencies ...
        BossEncounterRepository? bossRepository = null)
    {
        _diceService = diceService;
        // ... existing initialization ...

        // Initialize boss integration if repository provided
        if (bossRepository != null)
        {
            _bossIntegration = new BossCombatIntegration(bossRepository, diceService);
        }
    }
}

```

### 2. Seed Boss Data on Application Startup

```csharp
// In your application initialization (e.g., Program.cs or Main)
var repository = new BossEncounterRepository(dbPath);
var masterSeeder = new BossMasterSeeder(repository);

// Seed all boss data (encounters, abilities, loot)
masterSeeder.SeedAllBossData();

// Validate seeding
var validation = masterSeeder.ValidateSeedData();
if (!validation.IsValid)
{
    Console.WriteLine("Boss data validation failed!");
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"  ERROR: {error}");
    }
}

```

---

## Combat Loop Integration Points

### Integration Point 1: Combat Initialization

**When:** At the start of combat
**Where:** In `InitializeCombat()` method

```csharp
public CombatState InitializeCombat(PlayerCharacter player, List<Enemy> enemies, Room? currentRoom = null, bool canFlee = true)
{
    var combatState = new CombatState
    {
        Player = player,
        Enemies = new List<Enemy>(enemies),
        IsActive = true,
        CanFlee = canFlee,
        CurrentRoom = currentRoom
    };

    // [v0.20] Initialize tactical grid
    combatState.Grid = _gridService.InitializeGrid(player, enemies);

    // Roll initiative
    RollInitiative(combatState);

    // === BOSS INTEGRATION POINT 1 ===
    // Initialize boss encounters if present
    _bossIntegration?.InitializeBossEncounters(combatState);

    return combatState;
}

```

### Integration Point 2: After Player Attack

**When:** After player deals damage to an enemy
**Where:** In `PlayerAttack()` method, after damage is dealt

```csharp
public void PlayerAttack(CombatState combatState, Enemy target)
{
    // ... existing attack code ...

    // Deal damage
    target.HP -= damageDealt;

    // === BOSS INTEGRATION POINT 2 ===
    // Check for telegraph interrupts
    if (target.IsBoss && damageDealt > 0)
    {
        _bossIntegration?.CheckTelegraphInterrupt(combatState, target, damageDealt);
    }

    // === BOSS INTEGRATION POINT 3 ===
    // Check for phase transitions and enrage
    if (target.IsBoss && target.HP > 0)
    {
        _bossIntegration?.ProcessBossAction(combatState, target);
    }

    // ... rest of attack code ...
}

```

### Integration Point 3: Boss AI Turn

**When:** During enemy turn processing
**Where:** In enemy turn logic (create new method or modify existing)

```csharp
private void ProcessBossTurn(CombatState combatState, Enemy boss, int currentTurn)
{
    if (!boss.IsBoss || boss.HP <= 0)
    {
        return;
    }

    // === BOSS INTEGRATION POINT 4 ===
    // Check if boss should start telegraphing
    var abilityToTelegraph = _bossIntegration?.ShouldBossTelegraph(boss, currentTurn);

    if (abilityToTelegraph != null)
    {
        // Boss starts telegraphing instead of normal attack
        _bossIntegration?.BeginBossTelegraph(combatState, boss, abilityToTelegraph, currentTurn);
    }
    else
    {
        // Boss uses normal attack
        ProcessEnemyAttack(combatState, boss);
    }
}

```

### Integration Point 4: End of Turn Processing

**When:** At the end of each combat turn
**Where:** After all combatants have acted

```csharp
public void ProcessEndOfTurn(CombatState combatState, int currentTurn)
{
    // ... existing end-of-turn logic ...

    // === BOSS INTEGRATION POINT 5 ===
    // Process telegraphed abilities and vulnerability windows
    _bossIntegration?.ProcessEndOfTurn(combatState, currentTurn);

    // ... status effects, hazards, etc. ...
}

```

### Integration Point 5: Loot Generation

**When:** After combat victory
**Where:** In `GenerateLoot()` method

```csharp
public void GenerateLoot(CombatState combatState, Room room)
{
    combatState.AddLogEntry("");
    combatState.AddLogEntry("=== LOOT ===");

    bool anyLoot = false;
    int totalCurrency = 0;

    foreach (var enemy in combatState.Enemies.Where(e => !e.IsAlive))
    {
        // === BOSS INTEGRATION POINT 6 ===
        // Use boss loot system for bosses
        if (enemy.IsBoss && _bossIntegration != null)
        {
            _bossIntegration.GenerateBossLoot(combatState, enemy, combatState.Player.Id);
            anyLoot = true;
            continue; // Skip normal loot for bosses
        }

        // Regular enemy loot
        var loot = _lootService.GenerateLoot(enemy, combatState.Player);
        // ... existing loot code ...
    }

    // ... rest of loot display ...
}

```

### Integration Point 6: Combat End Cleanup

**When:** When combat ends (victory or defeat)
**Where:** At combat conclusion

```csharp
public void EndCombat(CombatState combatState)
{
    // ... existing cleanup ...

    // === BOSS INTEGRATION POINT 7 ===
    // Clear boss combat state (telegraphs, etc.)
    _bossIntegration?.ClearBossCombatState(combatState);
}

```

---

## Code Examples

### Complete Example: Modified Combat Turn Loop

```csharp
public void ProcessCombatTurn(CombatState combatState)
{
    int currentTurn = combatState.TurnNumber;

    // Display active telegraphs at start of turn
    if (_bossIntegration != null && _bossIntegration.HasBossEncounter(combatState))
    {
        var telegraphs = _bossIntegration.GetActiveTelegraphsDisplay(combatState);
        if (telegraphs.Any())
        {
            combatState.AddLogEntry("");
            combatState.AddLogEntry("=== ACTIVE TELEGRAPHS ===");
            foreach (var telegraph in telegraphs)
            {
                combatState.AddLogEntry(telegraph);
            }
            combatState.AddLogEntry("");
        }

        var bossStatus = _bossIntegration.GetBossStatusDisplay(combatState);
        if (bossStatus.Any())
        {
            combatState.AddLogEntry("=== BOSS STATUS ===");
            foreach (var status in bossStatus)
            {
                combatState.AddLogEntry(status);
            }
            combatState.AddLogEntry("");
        }
    }

    // Process initiative order
    foreach (var participant in combatState.InitiativeOrder)
    {
        if (participant.IsPlayer)
        {
            // Handle player turn
            ProcessPlayerTurn(combatState);
        }
        else
        {
            var enemy = (Enemy)participant.Character!;
            if (enemy.HP > 0)
            {
                if (enemy.IsBoss)
                {
                    // Process boss turn with telegraph mechanics
                    ProcessBossTurn(combatState, enemy, currentTurn);
                }
                else
                {
                    // Regular enemy turn
                    ProcessEnemyAttack(combatState, enemy);
                }
            }
        }
    }

    // End of turn processing
    _bossIntegration?.ProcessEndOfTurn(combatState, currentTurn);

    // Increment turn counter
    combatState.TurnNumber++;
}

```

### Example: Boss-Specific Enemy Attack

```csharp
private void ProcessEnemyAttack(CombatState combatState, Enemy enemy)
{
    // ... existing attack code ...

    // After attack completes, check boss mechanics
    if (enemy.IsBoss && enemy.HP > 0)
    {
        _bossIntegration?.ProcessBossAction(combatState, enemy);
    }
}

```

---

## UI Integration

### Display Active Telegraphs

```csharp
// At start of player turn or in UI status panel
if (_bossIntegration.HasBossEncounter(combatState))
{
    var telegraphs = _bossIntegration.GetActiveTelegraphsDisplay(combatState);
    foreach (var telegraph in telegraphs)
    {
        DisplayTelegraphWarning(telegraph); // Your UI method
    }
}

```

### Display Boss Status

```csharp
// In enemy status display
var bossStatus = _bossIntegration.GetBossStatusDisplay(combatState);
foreach (var status in bossStatus)
{
    DisplayBossStatus(status); // Your UI method
}

```

### Example UI Output

```
TPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPW
Q �  BOSS ENCOUNTER: RUIN-WARDEN
ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]
Ruin-Warden emerges from the shadows!
Boss Type: Sector Boss
Phases: 3

=== ACTIVE TELEGRAPHS ===
�  Ruin-Warden: Graviton Pulse (2 turns)
    =�  15 damage needed to interrupt

=== BOSS STATUS ===
Ruin-Warden: Phase 2 | ENRAGED

```

---

## Testing

### Integration Test Example

```csharp
[Test]
public void BossCombat_FullWorkflow()
{
    // Arrange
    var repository = new BossEncounterRepository(testDbPath);
    var masterSeeder = new BossMasterSeeder(repository);
    masterSeeder.SeedAllBossData();

    var diceService = new DiceService(seed: 42);
    var bossIntegration = new BossCombatIntegration(repository, diceService);

    var combatEngine = new CombatEngine(
        diceService,
        sagaService,
        lootService,
        equipmentService,
        hazardService,
        currencyService,
        bossRepository: repository);

    var player = CreateTestPlayer();
    var boss = new Enemy
    {
        Id = "boss_test",
        Name = "Ruin-Warden",
        Type = EnemyType.RuinWarden,
        IsBoss = true,
        HP = 100,
        MaxHP = 100
    };

    // Act
    var combatState = combatEngine.InitializeCombat(player, new List<Enemy> { boss });

    // Assert
    Assert.IsTrue(bossIntegration.HasBossEncounter(combatState));
    Assert.AreEqual(1, boss.CurrentPhase);
    Assert.IsNotNull(boss.BossEncounterId);
}

```

---

## Summary

### Minimal Integration Checklist

- [ ]  Add `BossCombatIntegration` to `CombatEngine` constructor
- [ ]  Call `InitializeBossEncounters()` in `InitializeCombat()`
- [ ]  Call `CheckTelegraphInterrupt()` after player damage
- [ ]  Call `ProcessBossAction()` after boss takes damage
- [ ]  Call `ShouldBossTelegraph()` during boss AI turn
- [ ]  Call `ProcessEndOfTurn()` at end of each turn
- [ ]  Call `GenerateBossLoot()` instead of normal loot for bosses
- [ ]  Call `ClearBossCombatState()` when combat ends
- [ ]  Display telegraphs and boss status in UI
- [ ]  Seed boss data on application startup

### Optional Enhancements

- Add boss music/sound effects during encounters
- Highlight boss enemies in UI with special indicators
- Show phase transition animations
- Display countdown timers for telegraphed abilities
- Add achievement tracking for boss defeats
- Save boss kill history per character

---

## Troubleshooting

**Q: Boss doesn't initialize properly**
A: Ensure `EnemyType` matches one of the seeded boss types (RuinWarden, AethericAberration, ForlornArchivist, OmegaSentinel)

**Q: Telegraphs never trigger**
A: Check AI pattern telegraph frequency and ensure `ShouldBossTelegraph()` is called during boss turn

**Q: Loot doesn't generate**
A: Verify `GenerateBossLoot()` is called with correct `boss.BossEncounterId` and `player.Id`

**Q: Phase transitions don't work**
A: Make sure `ProcessBossAction()` is called after boss takes damage

---

## Reference

- `BossCombatIntegration.cs` - Main integration service
- `BossEncounterService.cs` - Phase and enrage mechanics
- `TelegraphedAbilityService.cs` - Telegraph and vulnerability mechanics
- `BossLootService.cs` - Boss loot generation
- `BossMasterSeeder.cs` - Database seeding
- `BossSystemIntegrationTests.cs` - Integration test examples

---

**Last Updated:** v0.23 Boss System
**Specification:** v0.23.1 (Framework) + v0.23.2 (Mechanics) + v0.23.3 (Loot)