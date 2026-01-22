# v0.29.5: Movement System Integration & Forced Movement

Type: Technical
Description: Integrates movement system with Muspelheim environmental hazards, implements forced movement mechanics for lava/hazard interactions.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.29.1-v0.29.4, v0.20 (Tactical Grid)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.29: Muspelheim Biome Implementation (v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.29.5-MOVEMENT

**Status:** Design Complete — Ready for Phase 2 (Implementation)

**Timeline:** 5-10 hours

**Prerequisites:** v0.29.1 (Database), v0.29.2 (Hazards), v0.29.4 (Grid generation)

**Parent Spec:** v0.29: Muspelheim Biome Implementation

---

## I. Executive Summary

This specification completes the **movement system integration** for Muspelheim hazards, enabling lava rivers and other blocking hazards to actually prevent movement. This is the final integration work for v0.29, making the biome fully functional.

### Scope

**Movement Integration:**

- Wire `RoomTile.IsPassable()` into `MovementService.TryMove()`
- Enforce blocking for `[Chasm/Lava River]` hazards
- Handle movement validation before Stamina/KE consumption
- Provide clear feedback for blocked movement attempts

**Forced Movement Integration:**

- Push/Pull abilities check target tile passability
- Environmental kills when pushed into `[Chasm]`
- Integration with existing forced movement system (v0.20.4)
- Controller archetype "lava push" tactical fantasy

**Testing:**

- Movement blocking validation tests
- Forced movement lethality tests
- Edge case coverage (corners, multi-tile push)
- Integration with existing movement tests

**Result:** Muspelheim hazards enforce movement restrictions, completing v0.29 tactical gameplay.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.29.5)

**Movement Blocking:**

- `MovementService.TryMove()` validation logic
- `RoomTile.IsPassable()` integration
- Blocking for `[Chasm/Lava River]` hazards
- Movement attempt feedback messages
- Pre-movement validation (before resource consumption)

**Forced Movement:**

- Push/Pull ability validation
- Environmental kill logic (pushed into `[Chasm]`)
- Death tracking (separate from heat deaths)
- Combat log messages for environmental kills

**Integration Points:**

- v0.20: Tactical Grid (existing `MovementService`)
- v0.20.4: Advanced Movement (forced movement system)
- v0.29.2: Environmental Hazards (hazard definitions)
- v0.15: Trauma Economy (death tracking)

**Testing:**

- Unit tests for movement validation
- Integration tests for forced movement
- Edge cases (null tiles, invalid positions)
- Combat scenario tests

**Quality:**

- v5.0 setting compliance
- Serilog structured logging
- 85%+ test coverage
- Clear error messages

### ❌ Explicitly Out of Scope

- Grid persistence to database (defer to v0.45 optimization)
- Advanced pathfinding around hazards (v0.34)
- Flying/levitation mechanics (v0.40+)
- Teleportation through hazards (handled separately)
- AI pathfinding integration (v0.36)
- Movement cost calculation changes (already complete)
- New movement abilities (not needed for v0.29)

---

## III. Movement Blocking Implementation

### Current State Analysis

**What Exists:**

```csharp
// v0.29.2: RoomTile already has IsPassable() logic
public class RoomTile
{
    public bool IsPassable()
    {
        // Check if tile has blocking hazard
        if (HasFeature("[Chasm/Lava River]")) return false;
        if (HasFeature("[Collapsed Structure]")) return false;
        
        // Occupied tiles are passable (can move through allies)
        return true;
    }
}

// v0.20: MovementService exists but doesn't check IsPassable()
public class MovementService
{
    public bool TryMove(Combatant combatant, GridPosition targetPosition)
    {
        // Current: Only checks if tile exists and has enough KE/Stamina
        // Missing: IsPassable() check
        var tile = _gridService.GetTile(targetPosition);
        if (tile == null) return false;
        
        var moveCost = CalculateMovementCost(combatant.Position, targetPosition);
        if (combatant.KineticEnergy < moveCost) return false;
        
        // Execute movement (PROBLEM: doesn't check blocking hazards)
        combatant.Position = targetPosition;
        combatant.KineticEnergy -= moveCost;
        return true;
    }
}
```

**What's Missing:**

- `TryMove()` doesn't call `IsPassable()`
- No feedback when movement blocked by hazard
- Resource consumption happens even if movement fails

### Integration Solution

**Updated MovementService.TryMove():**

```csharp
using Serilog;
using RuneAndRust.Core.Models;
using [RuneAndRust.Services](http://RuneAndRust.Services);

namespace [RuneAndRust.Services](http://RuneAndRust.Services)
{
    public class MovementService
    {
        private readonly ILogger<MovementService> _logger;
        private readonly TacticalGridService _gridService;
        private readonly CombatLogService _combatLog;
        
        public MovementService(
            ILogger<MovementService> logger,
            TacticalGridService gridService,
            CombatLogService combatLog)
        {
            _logger = logger;
            _gridService = gridService;
            _combatLog = combatLog;
        }
        
        /// <summary>
        /// Attempts to move a combatant to a target position.
        /// Validates tile passability BEFORE consuming movement resources.
        /// </summary>
        public bool TryMove(Combatant combatant, GridPosition targetPosition)
        {
            _logger.Debug(
                "Movement attempt: {Combatant} from {From} to {To}",
                [combatant.Name](http://combatant.Name),
                combatant.Position,
                targetPosition);
            
            // Validation 1: Target tile exists
            var targetTile = _gridService.GetTile(targetPosition);
            if (targetTile == null)
            {
                _logger.Warning(
                    "Movement failed: Invalid target position {Position}",
                    targetPosition);
                _combatLog.Add($"{[combatant.Name](http://combatant.Name)} cannot move to invalid position.");
                return false;
            }
            
            // Validation 2: Target tile is passable (NEW)
            if (!targetTile.IsPassable())
            {
                var blockingFeature = targetTile.GetBlockingFeature();
                
                _logger.Information(
                    "Movement blocked: {Combatant} cannot enter {Position} due to {Feature}",
                    [combatant.Name](http://combatant.Name),
                    targetPosition,
                    blockingFeature);
                
                _combatLog.Add(
                    $"{[combatant.Name](http://combatant.Name)} cannot move into {blockingFeature}. Path blocked.",
                    CombatLogType.MovementBlocked);
                
                return false;
            }
            
            // Validation 3: Has sufficient movement resources
            var moveCost = CalculateMovementCost(combatant.Position, targetPosition);
            if (combatant.KineticEnergy < moveCost)
            {
                _logger.Debug(
                    "Movement failed: Insufficient KE ({Current} < {Required})",
                    combatant.KineticEnergy,
                    moveCost);
                _combatLog.Add($"{[combatant.Name](http://combatant.Name)} lacks kinetic energy to move.");
                return false;
            }
            
            // Execute movement
            var fromPosition = combatant.Position;
            combatant.Position = targetPosition;
            combatant.KineticEnergy -= moveCost;
            
            _logger.Information(
                "Movement success: {Combatant} moved from {From} to {To} (KE: {KESpent}/{KERemaining})",
                [combatant.Name](http://combatant.Name),
                fromPosition,
                targetPosition,
                moveCost,
                combatant.KineticEnergy);
            
            _combatLog.Add(
                $"{[combatant.Name](http://combatant.Name)} moves to {targetPosition.ToDisplayString()}.",
                CombatLogType.Movement);
            
            return true;
        }
        
        // ... rest of MovementService methods ...
    }
}
```

### RoomTile.GetBlockingFeature() Helper

**Add helper method to identify blocking hazard:**

```csharp
public class RoomTile
{
    public bool IsPassable()
    {
        if (HasFeature("[Chasm/Lava River]")) return false;
        if (HasFeature("[Collapsed Structure]")) return false;
        return true;
    }
    
    /// <summary>
    /// Returns the name of the feature blocking movement, or null if passable.
    /// </summary>
    public string GetBlockingFeature()
    {
        if (HasFeature("[Chasm/Lava River]")) return "[Chasm/Lava River]";
        if (HasFeature("[Collapsed Structure]")) return "[Collapsed Structure]";
        return null;
    }
}
```

---

## IV. Forced Movement Integration

### Controller's Playground: Push Into Lava

**Design Fantasy:**

- Warrior with Shield Bash can push enemies
- Mystic with Telekinetic Shove can push enemies
- Adept with tactical positioning can push enemies
- **Goal:** Push enemies into `[Chasm/Lava River]` for instant kill

### ForcedMovementService Extension

**Current State (v0.20.4):**

```csharp
// Forced movement exists but doesn't check lethality
public class ForcedMovementService
{
    public bool TryPush(Combatant source, Combatant target, int distance)
    {
        var direction = CalculateDirection(source.Position, target.Position);
        var finalPosition = target.Position + (direction * distance);
        
        // Current: Just moves target, doesn't check hazards
        target.Position = finalPosition;
        return true;
    }
}
```

**Updated Implementation:**

```csharp
using Serilog;
using RuneAndRust.Core.Models;
using [RuneAndRust.Services](http://RuneAndRust.Services);

namespace [RuneAndRust.Services](http://RuneAndRust.Services)
{
    public class ForcedMovementService
    {
        private readonly ILogger<ForcedMovementService> _logger;
        private readonly TacticalGridService _gridService;
        private readonly DamageService _damageService;
        private readonly CombatLogService _combatLog;
        private readonly BiomeStatusService _biomeStatusService;
        
        /// <summary>
        /// Attempts to push a target in a direction for a given distance.
        /// Checks each tile along the path for lethality.
        /// </summary>
        public ForcedMovementResult TryPush(
            Combatant source,
            Combatant target,
            Vector2Int direction,
            int distance)
        {
            _logger.Information(
                "Forced movement: {Source} pushes {Target} {Distance} tiles {Direction}",
                [source.Name](http://source.Name),
                [target.Name](http://target.Name),
                distance,
                direction);
            
            var currentPosition = target.Position;
            var tilesTraversed = 0;
            
            for (int i = 1; i <= distance; i++)
            {
                var nextPosition = currentPosition + direction;
                var nextTile = _gridService.GetTile(nextPosition);
                
                // Check 1: Tile exists
                if (nextTile == null)
                {
                    _logger.Debug(
                        "Forced movement stopped: Edge of battlefield at {Position}",
                        nextPosition);
                    break; // Stop at edge
                }
                
                // Check 2: Tile is passable
                if (!nextTile.IsPassable())
                {
                    var blockingFeature = nextTile.GetBlockingFeature();
                    
                    // Check if blocking feature is lethal
                    if (IsLethalHazard(blockingFeature))
                    {
                        return HandleEnvironmentalKill(
                            source,
                            target,
                            nextTile,
                            blockingFeature,
                            tilesTraversed);
                    }
                    else
                    {
                        // Non-lethal blocking (e.g., wall)
                        _logger.Information(
                            "Forced movement stopped: {Target} impacts {Feature}",
                            [target.Name](http://target.Name),
                            blockingFeature);
                        
                        _combatLog.Add(
                            $"{[target.Name](http://target.Name)} is pushed into {blockingFeature} and stops.",
                            CombatLogType.ForcedMovement);
                        
                        break;
                    }
                }
                
                // Move to next tile
                currentPosition = nextPosition;
                tilesTraversed++;
            }
            
            // Update final position
            target.Position = currentPosition;
            
            _logger.Information(
                "Forced movement complete: {Target} pushed {Tiles} tiles to {Position}",
                [target.Name](http://target.Name),
                tilesTraversed,
                currentPosition);
            
            return new ForcedMovementResult
            {
                Success = true,
                FinalPosition = currentPosition,
                TilesTraversed = tilesTraversed,
                WasLethal = false
            };
        }
        
        /// <summary>
        /// Checks if a hazard causes instant death on forced entry.
        /// </summary>
        private bool IsLethalHazard(string featureName)
        {
            return featureName switch
            {
                "[Chasm/Lava River]" => true,
                "[Collapsed Structure]" => false, // Just blocks, not lethal
                _ => false
            };
        }
        
        /// <summary>
        /// Handles instant death from forced movement into lethal hazard.
        /// </summary>
        private ForcedMovementResult HandleEnvironmentalKill(
            Combatant source,
            Combatant target,
            RoomTile lethalTile,
            string hazardName,
            int tilesTraversed)
        {
            _logger.Warning(
                "Environmental kill: {Target} pushed into {Hazard} by {Source}",
                [target.Name](http://target.Name),
                hazardName,
                [source.Name](http://source.Name));
            
            // Apply instant death
            target.CurrentHP = 0;
            target.IsAlive = false;
            
            // Track environmental death
            if (_biomeStatusService != null && target is PlayerCharacter player)
            {
                _biomeStatusService.IncrementEnvironmentalDeaths(
                    player.CharacterId,
                    biomeId: 4, // Muspelheim
                    hazardName);
            }
            
            // Dramatic combat log message
            _combatLog.Add(
                $"{[target.Name](http://target.Name)} is pushed into {hazardName}! Environmental kill.",
                CombatLogType.EnvironmentalKill);
            
            _combatLog.Add(
                $"{[target.Name](http://target.Name)} has been defeated.",
                CombatLogType.Death);
            
            return new ForcedMovementResult
            {
                Success = true,
                FinalPosition = lethalTile.Position,
                TilesTraversed = tilesTraversed,
                WasLethal = true,
                KillingHazard = hazardName
            };
        }
    }
    
    /// <summary>
    /// Result of a forced movement attempt.
    /// </summary>
    public class ForcedMovementResult
    {
        public bool Success { get; set; }
        public GridPosition FinalPosition { get; set; }
        public int TilesTraversed { get; set; }
        public bool WasLethal { get; set; }
        public string KillingHazard { get; set; }
    }
}
```

### BiomeStatusService Extension

**Add environmental death tracking:**

```csharp
public class BiomeStatusService
{
    /// <summary>
    /// Tracks deaths from environmental hazards (forced movement into lava, etc.).
    /// Separate from heat deaths (ambient condition).
    /// </summary>
    public void IncrementEnvironmentalDeaths(
        int characterId,
        int biomeId,
        string hazardName)
    {
        _logger.Information(
            "Environmental death tracked: Character {CharacterId} killed by {Hazard} in Biome {BiomeId}",
            characterId,
            hazardName,
            biomeId);
        
        var sql = @"
            UPDATE Characters_BiomeStatus
            SET times_died_to_environment = times_died_to_environment + 1,
                last_updated = CURRENT_TIMESTAMP
            WHERE character_id = @CharacterId AND biome_id = @BiomeId
        ";
        
        _database.Execute(sql, new { CharacterId = characterId, BiomeId = biomeId });
    }
}
```

**Database Schema Addition:**

```sql
-- Add column to Characters_BiomeStatus
ALTER TABLE Characters_BiomeStatus
ADD COLUMN times_died_to_environment INTEGER DEFAULT 0;

-- times_died_to_heat: Deaths from [Intense Heat] ambient condition
-- times_died_to_environment: Deaths from pushed into hazards, falling, etc.
```

---

## V. Integration Points

### v0.20 Tactical Grid Integration

**Existing System:**

- `MovementService` handles voluntary movement
- `GridPosition` defines tile coordinates
- `KineticEnergy` resource for movement

**Integration:**

- ✅ `TryMove()` now calls `RoomTile.IsPassable()`
- ✅ Movement blocked by hazards before KE consumption
- ✅ Clear feedback messages

### v0.20.4 Advanced Movement Integration

**Existing System:**

- `ForcedMovementService` handles push/pull
- Abilities like Shield Bash, Telekinetic Shove

**Integration:**

- ✅ `TryPush()` checks tile passability
- ✅ Lethal hazards cause instant death
- ✅ Non-lethal blocking stops movement
- ✅ Returns detailed result (tiles traversed, lethality)

### v0.29.2 Environmental Hazards Integration

**Existing System:**

- `RoomTile.HasFeature()` checks for hazards
- `[Chasm/Lava River]` defined as blocking + lethal

**Integration:**

- ✅ `IsPassable()` enforces blocking
- ✅ `GetBlockingFeature()` identifies hazard
- ✅ `IsLethalHazard()` determines environmental kill eligibility

### v0.15 Trauma Economy Integration

**Existing System:**

- Death tracking for statistics
- Stress from witnessing deaths

**Integration:**

- ✅ Environmental deaths tracked separately from heat deaths
- ✅ Statistics distinguish death types
- ✅ Potential: Party Stress when ally pushed into lava (future enhancement)

---

## VI. Edge Cases & Error Handling

### Edge Case 1: Push Off Battlefield Edge

**Scenario:** Enemy pushed toward edge of grid

**Handling:**

```csharp
// Stop at edge, don't crash
if (nextTile == null)
{
    _logger.Debug("Forced movement stopped at battlefield edge");
    break; // Stop at last valid tile
}
```

**Result:** Target stops at edge tile, doesn't go off-grid

### Edge Case 2: Multi-Tile Push Into Lava

**Scenario:** Enemy pushed 3 tiles, lava is tile 2

**Handling:**

```csharp
// Check each tile in path
for (int i = 1; i <= distance; i++)
{
    var nextTile = currentPosition + direction;
    if (!nextTile.IsPassable() && IsLethalHazard(...))
    {
        // Instant death, stop traversal
        return HandleEnvironmentalKill(...);
    }
}
```

**Result:** Death occurs on first lethal tile, remaining distance irrelevant

### Edge Case 3: Push Ally Into Lava

**Scenario:** Player accidentally pushes ally (should this be possible?)

**Handling:**

```csharp
// Design decision: Allow friendly fire for tactical depth
// No special handling - ally dies if pushed into lava
// Combat log makes it clear who caused the death
_combatLog.Add(
    $"{[source.Name](http://source.Name)} pushes {[target.Name](http://target.Name)} into [Lava River]! Friendly fire.");
```

**Result:** Friendly fire allowed, player must be careful with push abilities

### Edge Case 4: Null Tile in Grid

**Scenario:** Generated grid has null tile (generation bug)

**Handling:**

```csharp
if (targetTile == null)
{
    _logger.Error(
        "CRITICAL: Null tile at valid grid position {Position}. Grid generation bug.",
        targetPosition);
    return false; // Fail gracefully
}
```

**Result:** Movement fails safely, logs error for debugging

### Edge Case 5: Occupied Tile with Hazard

**Scenario:** Ally standing on `[Burning Ground]` (passable hazard)

**Handling:**

```csharp
public bool IsPassable()
{
    // Occupied tiles are passable (can move through allies)
    // Only check blocking features
    if (HasFeature("[Chasm/Lava River]")) return false;
    
    // [Burning Ground] doesn't block, even if occupied
    return true;
}
```

**Result:** Can move through allies, even on damaging (but passable) hazards

---

## VII. Unit Tests

### Test Suite 1: Movement Blocking

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using [RuneAndRust.Services](http://RuneAndRust.Services);
using RuneAndRust.Core.Models;

[TestClass]
public class MovementBlockingTests
{
    private MovementService _movementService;
    private Mock<TacticalGridService> _mockGridService;
    private Mock<CombatLogService> _mockCombatLog;
    private Mock<ILogger<MovementService>> _mockLogger;
    
    [TestInitialize]
    public void Setup()
    {
        _mockGridService = new Mock<TacticalGridService>();
        _mockCombatLog = new Mock<CombatLogService>();
        _mockLogger = new Mock<ILogger<MovementService>>();
        
        _movementService = new MovementService(
            _mockLogger.Object,
            _mockGridService.Object,
            _mockCombatLog.Object);
    }
    
    [TestMethod]
    public void TryMove_IntoLavaRiver_BlocksMovement()
    {
        // Arrange
        var combatant = CreateTestCombatant(
            position: new GridPosition(Zone.Player, Row.Front, 0),
            kineticEnergy: 30);
        
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);
        var lavaTile = CreateTileWithFeature("[Chasm/Lava River]");
        
        _mockGridService.Setup(g => g.GetTile(targetPosition))
            .Returns(lavaTile);
        
        // Act
        var result = _movementService.TryMove(combatant, targetPosition);
        
        // Assert
        Assert.IsFalse(result, "Movement should be blocked by lava");
        Assert.AreEqual(30, combatant.KineticEnergy, "KE should not be consumed");
        Assert.AreEqual(
            new GridPosition(Zone.Player, Row.Front, 0),
            combatant.Position,
            "Position should not change");
        
        _mockCombatLog.Verify(
            c => c.Add(
                [It.Is](http://It.Is)<string>(s => s.Contains("cannot move")),
                It.IsAny<CombatLogType>()),
            Times.Once);
    }
    
    [TestMethod]
    public void TryMove_IntoPassableTile_AllowsMovement()
    {
        // Arrange
        var combatant = CreateTestCombatant(
            position: new GridPosition(Zone.Player, Row.Front, 0),
            kineticEnergy: 30);
        
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);
        var passableTile = CreatePassableTile();
        
        _mockGridService.Setup(g => g.GetTile(targetPosition))
            .Returns(passableTile);
        
        // Act
        var result = _movementService.TryMove(combatant, targetPosition);
        
        // Assert
        Assert.IsTrue(result, "Movement should succeed");
        Assert.IsTrue(combatant.KineticEnergy < 30, "KE should be consumed");
        Assert.AreEqual(targetPosition, combatant.Position, "Position should update");
    }
    
    [TestMethod]
    public void TryMove_IntoBurningGround_AllowsMovement()
    {
        // Arrange: [Burning Ground] is passable, just deals damage
        var combatant = CreateTestCombatant(
            position: new GridPosition(Zone.Player, Row.Front, 0),
            kineticEnergy: 30);
        
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);
        var burningTile = CreateTileWithFeature("[Burning Ground]");
        burningTile.IsPassable = () => true; // Passable but damaging
        
        _mockGridService.Setup(g => g.GetTile(targetPosition))
            .Returns(burningTile);
        
        // Act
        var result = _movementService.TryMove(combatant, targetPosition);
        
        // Assert
        Assert.IsTrue(result, "Movement into [Burning Ground] should succeed");
        Assert.AreEqual(targetPosition, combatant.Position);
    }
    
    [TestMethod]
    public void TryMove_InsufficientKE_FailsBeforePassabilityCheck()
    {
        // Arrange: KE insufficient, tile is lava (both should fail)
        var combatant = CreateTestCombatant(
            position: new GridPosition(Zone.Player, Row.Front, 0),
            kineticEnergy: 0); // No KE
        
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);
        var lavaTile = CreateTileWithFeature("[Chasm/Lava River]");
        
        _mockGridService.Setup(g => g.GetTile(targetPosition))
            .Returns(lavaTile);
        
        // Act
        var result = _movementService.TryMove(combatant, targetPosition);
        
        // Assert
        Assert.IsFalse(result);
        // NOTE: Order matters - passability checked BEFORE KE consumption
        // Combat log should mention blocking, not KE
    }
}
```

### Test Suite 2: Forced Movement Lethality

```csharp
[TestClass]
public class ForcedMovementLethalityTests
{
    private ForcedMovementService _forcedMovementService;
    private Mock<TacticalGridService> _mockGridService;
    private Mock<DamageService> _mockDamageService;
    private Mock<CombatLogService> _mockCombatLog;
    private Mock<BiomeStatusService> _mockBiomeStatus;
    
    [TestInitialize]
    public void Setup()
    {
        _mockGridService = new Mock<TacticalGridService>();
        _mockDamageService = new Mock<DamageService>();
        _mockCombatLog = new Mock<CombatLogService>();
        _mockBiomeStatus = new Mock<BiomeStatusService>();
        
        _forcedMovementService = new ForcedMovementService(
            Mock.Of<ILogger<ForcedMovementService>>(),
            _mockGridService.Object,
            _mockDamageService.Object,
            _mockCombatLog.Object,
            _mockBiomeStatus.Object);
    }
    
    [TestMethod]
    public void TryPush_IntoLavaRiver_CausesInstantDeath()
    {
        // Arrange
        var source = CreateTestCombatant("Warrior");
        var target = CreateTestCombatant(
            "Enemy",
            position: new GridPosition(Zone.Enemy, Row.Front, 0),
            currentHP: 50);
        
        var lavaPosition = new GridPosition(Zone.Enemy, Row.Front, 1);
        var lavaTile = CreateTileWithFeature("[Chasm/Lava River]");
        lavaTile.Position = lavaPosition;
        
        _mockGridService.Setup(g => g.GetTile(lavaPosition))
            .Returns(lavaTile);
        
        var direction = new Vector2Int(0, 1); // Push right
        
        // Act
        var result = _forcedMovementService.TryPush(source, target, direction, distance: 1);
        
        // Assert
        Assert.IsTrue(result.Success, "Push should execute");
        Assert.IsTrue(result.WasLethal, "Push into lava should be lethal");
        Assert.AreEqual("[Chasm/Lava River]", result.KillingHazard);
        Assert.AreEqual(0, target.CurrentHP, "Target should be dead (0 HP)");
        Assert.IsFalse(target.IsAlive, "Target.IsAlive should be false");
        
        _mockCombatLog.Verify(
            c => c.Add(
                [It.Is](http://It.Is)<string>(s => s.Contains("Environmental kill")),
                CombatLogType.EnvironmentalKill),
            Times.Once);
    }
    
    [TestMethod]
    public void TryPush_ThreeTilesWithLavaAtTwo_StopsAtLavaWithDeath()
    {
        // Arrange: Push 3 tiles, but lava is at tile 2
        var source = CreateTestCombatant("Warrior");
        var target = CreateTestCombatant(
            "Enemy",
            position: new GridPosition(Zone.Enemy, Row.Front, 0));
        
        var tile1 = CreatePassableTile();
        var tile2Lava = CreateTileWithFeature("[Chasm/Lava River]");
        var tile3 = CreatePassableTile(); // Never reached
        
        _mockGridService.Setup(g => g.GetTile(new GridPosition(Zone.Enemy, Row.Front, 1)))
            .Returns(tile1);
        _mockGridService.Setup(g => g.GetTile(new GridPosition(Zone.Enemy, Row.Front, 2)))
            .Returns(tile2Lava);
        _mockGridService.Setup(g => g.GetTile(new GridPosition(Zone.Enemy, Row.Front, 3)))
            .Returns(tile3);
        
        var direction = new Vector2Int(0, 1);
        
        // Act
        var result = _forcedMovementService.TryPush(source, target, direction, distance: 3);
        
        // Assert
        Assert.IsTrue(result.WasLethal);
        Assert.AreEqual(1, result.TilesTraversed, "Should traverse 1 tile before hitting lava");
        Assert.AreEqual(0, target.CurrentHP);
    }
    
    [TestMethod]
    public void TryPush_IntoWall_StopsWithoutDeath()
    {
        // Arrange: Non-lethal blocking (e.g., collapsed structure)
        var source = CreateTestCombatant("Warrior");
        var target = CreateTestCombatant(
            "Enemy",
            position: new GridPosition(Zone.Enemy, Row.Front, 0),
            currentHP: 50);
        
        var wallPosition = new GridPosition(Zone.Enemy, Row.Front, 1);
        var wallTile = CreateTileWithFeature("[Collapsed Structure]");
        wallTile.Position = wallPosition;
        
        _mockGridService.Setup(g => g.GetTile(wallPosition))
            .Returns(wallTile);
        
        var direction = new Vector2Int(0, 1);
        
        // Act
        var result = _forcedMovementService.TryPush(source, target, direction, distance: 1);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.WasLethal, "Wall should not be lethal");
        Assert.AreEqual(50, target.CurrentHP, "Target should be alive");
        Assert.AreEqual(0, result.TilesTraversed, "Should not traverse into wall");
    }
    
    [TestMethod]
    public void TryPush_PlayerIntoLava_TracksEnvironmentalDeath()
    {
        // Arrange: Player (not enemy) pushed into lava
        var source = CreateTestEnemy("Rival Berserker");
        var target = CreateTestPlayerCharacter(
            "TestPlayer",
            characterId: 123,
            position: new GridPosition(Zone.Player, Row.Front, 0));
        
        var lavaPosition = new GridPosition(Zone.Player, Row.Front, 1);
        var lavaTile = CreateTileWithFeature("[Chasm/Lava River]");
        lavaTile.Position = lavaPosition;
        
        _mockGridService.Setup(g => g.GetTile(lavaPosition))
            .Returns(lavaTile);
        
        var direction = new Vector2Int(0, 1);
        
        // Act
        var result = _forcedMovementService.TryPush(source, target, direction, distance: 1);
        
        // Assert
        Assert.IsTrue(result.WasLethal);
        
        // Verify environmental death tracked for player
        _mockBiomeStatus.Verify(
            b => b.IncrementEnvironmentalDeaths(
                123, // character_id
                4,   // biome_id (Muspelheim)
                "[Chasm/Lava River]"),
            Times.Once);
    }
}
```

### Test Suite 3: Edge Cases

```csharp
[TestClass]
public class MovementEdgeCaseTests
{
    [TestMethod]
    public void TryPush_OffBattlefieldEdge_StopsAtEdge()
    {
        // Arrange: Push toward edge, tile 2 doesn't exist
        var source = CreateTestCombatant("Warrior");
        var target = CreateTestCombatant(
            "Enemy",
            position: new GridPosition(Zone.Enemy, Row.Front, 0));
        
        var tile1 = CreatePassableTile();
        
        _mockGridService.Setup(g => g.GetTile(new GridPosition(Zone.Enemy, Row.Front, 1)))
            .Returns(tile1);
        _mockGridService.Setup(g => g.GetTile(new GridPosition(Zone.Enemy, Row.Front, 2)))
            .Returns((RoomTile)null); // Edge of battlefield
        
        var direction = new Vector2Int(0, 1);
        
        // Act
        var result = _forcedMovementService.TryPush(source, target, direction, distance: 3);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.WasLethal);
        Assert.AreEqual(1, result.TilesTraversed, "Should stop at tile 1 (last valid)");
        Assert.AreEqual(
            new GridPosition(Zone.Enemy, Row.Front, 1),
            target.Position);
    }
    
    [TestMethod]
    public void IsPassable_OccupiedTileWithBurningGround_ReturnsTrue()
    {
        // Arrange: Ally standing on [Burning Ground]
        var tile = CreateTileWithFeature("[Burning Ground]");
        tile.Occupant = CreateTestCombatant("Ally");
        
        // Act
        var passable = tile.IsPassable();
        
        // Assert
        Assert.IsTrue(passable, "Should be able to move through ally on passable hazard");
    }
    
    [TestMethod]
    public void GetBlockingFeature_NoBlockingFeatures_ReturnsNull()
    {
        // Arrange
        var tile = CreatePassableTile();
        
        // Act
        var feature = tile.GetBlockingFeature();
        
        // Assert
        Assert.IsNull(feature);
    }
}
```

---

## VIII. Combat Scenario Tests

### Integration Test 1: Complete Push-Into-Lava Scenario

```csharp
[TestMethod]
public void CombatScenario_WarriorPushesEnemyIntoLava_Success()
{
    // Arrange: Full combat scenario
    var encounter = CreateTestEncounter();
    var warrior = CreatePlayerWarrior("TestWarrior");
    var enemy = CreateTestEnemy("Forge-Hardened Undying");
    
    // Setup battlefield with lava
    var grid = CreateMuspelheimGrid();
    var lavaPosition = new GridPosition(Zone.Enemy, Row.Front, 2);
    var lavaTile = grid.GetTile(lavaPosition);
    lavaTile.AddFeature("[Chasm/Lava River]");
    
    // Position combatants
    warrior.Position = new GridPosition(Zone.Player, Row.Front, 0);
    enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 1);
    
    // Warrior has Shield Bash ability (pushes 1 tile)
    var shieldBash = new Ability
    {
        Name = "Shield Bash",
        ForcedMovementDistance = 1,
        ForcedMovementType = ForcedMovementType.Push
    };
    
    // Act: Warrior uses Shield Bash
    var result = _abilityService.UseAbility(warrior, shieldBash, enemy);
    
    // Assert: Enemy pushed into lava and dies
    Assert.IsTrue(result.Success);
    Assert.IsFalse(enemy.IsAlive, "Enemy should be dead");
    Assert.AreEqual(0, enemy.CurrentHP);
    
    // Verify combat log shows environmental kill
    var combatLog = _combatLogService.GetRecentEntries(5);
    Assert.IsTrue(
        combatLog.Any(e => e.Contains("Environmental kill")),
        "Combat log should mention environmental kill");
}
```

### Integration Test 2: Movement Blocked Feedback

```csharp
[TestMethod]
public void CombatScenario_PlayerTriesToWalkIntoLava_BlockedWithFeedback()
{
    // Arrange
    var encounter = CreateTestEncounter();
    var player = CreatePlayerCharacter("TestPlayer");
    player.Position = new GridPosition(Zone.Player, Row.Front, 0);
    player.KineticEnergy = 30;
    
    var lavaPosition = new GridPosition(Zone.Player, Row.Front, 1);
    var lavaTile = _gridService.GetTile(lavaPosition);
    lavaTile.AddFeature("[Chasm/Lava River]");
    
    // Act: Player attempts to move into lava
    var result = _movementService.TryMove(player, lavaPosition);
    
    // Assert: Movement blocked
    Assert.IsFalse(result, "Movement should be blocked");
    Assert.AreEqual(30, player.KineticEnergy, "KE should not be consumed");
    Assert.AreEqual(
        new GridPosition(Zone.Player, Row.Front, 0),
        player.Position,
        "Position should not change");
    
    // Verify feedback message
    var combatLog = _combatLogService.GetRecentEntries(1);
    Assert.IsTrue(
        combatLog[0].Contains("cannot move"),
        "Should provide clear feedback about blocking");
}
```

---

## IX. Success Criteria Checklist

### Functional Requirements

- [ ]  `MovementService.TryMove()` calls `RoomTile.IsPassable()`
- [ ]  Movement blocked by `[Chasm/Lava River]` hazards
- [ ]  Movement validation occurs BEFORE KE/Stamina consumption
- [ ]  Clear combat log messages for blocked movement
- [ ]  `ForcedMovementService.TryPush()` checks tile passability
- [ ]  Push into `[Chasm/Lava River]` causes instant death
- [ ]  Push into non-lethal blocking (wall) stops without death
- [ ]  Environmental deaths tracked in `Characters_BiomeStatus`
- [ ]  `ForcedMovementResult` returns detailed information
- [ ]  Edge cases handled gracefully (null tiles, battlefield edge)

### Quality Gates

- [ ]  85%+ unit test coverage for movement integration
- [ ]  All integration tests pass
- [ ]  Serilog structured logging implemented
- [ ]  Combat log messages clear and informative
- [ ]  No crashes on edge cases
- [ ]  v5.0 setting compliance verified

### Integration Verification

- [ ]  v0.20 Tactical Grid integration functional
- [ ]  v0.20.4 Forced Movement integration functional
- [ ]  v0.29.2 Hazard definitions enforced
- [ ]  v0.15 Death tracking updated
- [ ]  No regressions in existing movement tests

### Player Experience

- [ ]  Movement blocking feels responsive
- [ ]  Environmental kills feel impactful (combat log messages)
- [ ]  "Controller's playground" fantasy realized
- [ ]  Tactical positioning rewarded
- [ ]  Friendly fire clearly communicated

---

## X. Implementation Roadmap

### Phase 1: Movement Blocking (2-3 hours)

- [ ]  Update `MovementService.TryMove()` with passability check
- [ ]  Add `RoomTile.GetBlockingFeature()` helper method
- [ ]  Implement combat log messages for blocked movement
- [ ]  Write unit tests for movement blocking (5 tests)
- [ ]  Verify KE not consumed on blocked movement

### Phase 2: Forced Movement Lethality (2-3 hours)

- [ ]  Update `ForcedMovementService.TryPush()` with lethality checks
- [ ]  Implement `IsLethalHazard()` helper method
- [ ]  Implement `HandleEnvironmentalKill()` method
- [ ]  Create `ForcedMovementResult` class
- [ ]  Write unit tests for forced movement (6 tests)

### Phase 3: Database & Tracking (1 hour)

- [ ]  Add `times_died_to_environment` column to `Characters_BiomeStatus`
- [ ]  Implement `BiomeStatusService.IncrementEnvironmentalDeaths()`
- [ ]  Test environmental death tracking
- [ ]  Verify statistics separate from heat deaths

### Phase 4: Integration Testing (1-2 hours)

- [ ]  Write combat scenario tests (2 tests)
- [ ]  End-to-end test: Warrior pushes enemy into lava
- [ ]  End-to-end test: Player tries to walk into lava
- [ ]  Verify all existing movement tests still pass
- [ ]  Performance check (movement should not be slower)

### Phase 5: Edge Cases & Polish (1 hour)

- [ ]  Test battlefield edge push behavior
- [ ]  Test null tile handling
- [ ]  Test occupied tile with hazard
- [ ]  Review combat log message clarity
- [ ]  Code review and cleanup

**Total: 7-10 hours**

---

## XIV. Deployment Instructions

### Database Migration

**Apply schema changes:**

```bash
sqlite3 your_database.db < Data/v0.29.5_movement_integration.sql
```

This adds the `times_died_to_environment` column to `Characters_BiomeStatus` table.

### Run Tests

**Execute test suites:**

```bash
# Test Suite 1: Movement Blocking (5 tests)
dotnet test --filter "FullyQualifiedName~MovementBlockingTests"

# Test Suite 2: Forced Movement Lethality (6 tests)
dotnet test --filter "FullyQualifiedName~ForcedMovementLethalityTests"

# Test Suite 3: Edge Cases (3 tests)
dotnet test --filter "FullyQualifiedName~MovementEdgeCaseTests"
```

**Expected Results:**

- All 14+ tests should pass
- 85%+ code coverage for movement integration
- No regressions in existing movement tests

### Verify Integration

**Manual testing checklist:**

- [ ]  Test movement blocking in Muspelheim with lava rivers
- [ ]  Test forced movement abilities (Shield Bash) near hazards
- [ ]  Check environmental death tracking in database
- [ ]  Verify combat log messages are clear and informative
- [ ]  Confirm KE not consumed on blocked movement
- [ ]  Test friendly fire scenario (push ally into lava)

**Database verification:**

```sql
-- Check environmental death tracking
SELECT character_id, times_died_to_heat, times_died_to_environment
FROM Characters_BiomeStatus
WHERE biome_id = 4;
```

---

### Implementation Documentation

**Full implementation details available in:**

- `Data/v0.29.5_IMPLEMENTATION_[SUMMARY.md](http://SUMMARY.md)` - 400+ line comprehensive guide
- Includes: edge cases, integration points, known limitations, future enhancements

**Quick Reference:**

- Movement blocking: `MovementService.TryMove()` now validates `RoomTile.IsPassable()`
- Forced movement: `ForcedMovementService.TryPush()` handles environmental kills
- Death tracking: `BiomeStatusService.IncrementEnvironmentalDeaths()` separates heat deaths from environmental deaths

---

## XI. v5.0 Setting Compliance

### Technology, Not Magic

✅ **All hazards are physical/technological:**

- Lava rivers = "molten slag from geothermal failure"
- Chasms = "structural collapses exposing magma below"
- Environmental kills = "thermal immolation," not curse/magic

✅ **Layer 2 Voice:**

- Combat logs: "TestWarrior pushes Enemy into [Lava River]! Environmental kill."
- Not: "TestWarrior banishes Enemy to fiery doom!"
- Technical, matter-of-fact descriptions

### Logging Standards

✅ **Structured logging with context:**

```csharp
_logger.Warning(
    "Environmental kill: {Target} pushed into {Hazard} by {Source}",
    [target.Name](http://target.Name),
    hazardName,
    [source.Name](http://source.Name));
```

### Testing Standards

✅ **85%+ coverage target:**

- 11+ unit tests planned
- 2 integration tests planned
- 3 edge case tests planned
- Total: 16+ tests for 5-10 hours of implementation

---

## XII. Known Limitations

### Current Scope Constraints

1. **No AI Pathfinding Integration:**
    - AI doesn't avoid lava rivers yet (v0.36)
    - Enemies may try to walk into hazards
    - Manual targeting required for push abilities
2. **No Advanced Pathfinding:**
    - Players must manually navigate around lava
    - No auto-pathfinding around hazards (v0.34)
    - No "safest path" calculation
3. **No Flying/Teleportation Exceptions:**
    - All movement uses same passability rules
    - Future abilities (Blink, Levitate) will bypass (v0.40+)
    - Teleportation currently undefined
4. **No Damage on Impact:**
    - Pushing into wall = stop, no damage
    - Could add impact damage in v0.34
    - Current focus: lethality only
5. **No Party Stress from Witnessing:**
    - Seeing ally pushed into lava = no Stress penalty yet
    - Trauma Economy integration in v0.34
    - Current: Only tracks deaths, not reactions

### Future Enhancements (Post-v0.29)

- **v0.34:** Party Stress when witnessing environmental kills
- **v0.34:** Impact damage when pushed into walls
- **v0.34:** Advanced pathfinding around hazards
- **v0.36:** AI avoids lava rivers
- **v0.40+:** Flying/teleportation bypass hazard blocking

---

## XIII. Related Documents

**Parent Specification:**

- v0.29: Muspelheim Biome Implementation

**Dependencies:**

- v0.29.1: Database Schema & Room Templates
- v0.29.2: Environmental Hazards & Ambient Conditions
- v0.29.4: Service Implementation & Testing

**Integration Points:**

- v0.20: Tactical Combat Grid System
- v0.20.4: Advanced Movement Abilities
- v0.15: Trauma Economy (death tracking)

**Project Context:**

- Master Roadmap
- AI Session Handoff
- MANDATORY Requirements

---

**Phase 1 (Specification Design): COMPLETE ✓**

**Phase 2 (Implementation): Ready to begin**

**Estimated Timeline: 5-10 hours**

---

## Post-v0.29.5: v0.29 Complete

Once v0.29.5 is implemented, **v0.29 Muspelheim Biome is 100% complete:**

✅ Database foundation (v0.29.1)

✅ Environmental hazards (v0.29.2)

✅ Enemy definitions (v0.29.3)

✅ Service implementation (v0.29.4)

✅ Movement integration (v0.29.5)

**Ready for v0.30:** Niflheim Biome can use v0.29 as template for 4-phase implementation.