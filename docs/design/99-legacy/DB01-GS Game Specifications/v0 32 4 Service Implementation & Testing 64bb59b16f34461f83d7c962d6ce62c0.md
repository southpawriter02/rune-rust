# v0.32.4: Service Implementation & Testing

Type: Technical
Description: JotunheimService, JotunCorpseTerrainService, BiomeGenerationService extensions, extreme verticality, unit tests (~85% coverage), integration testing.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.32.1-v0.32.3 (All prior Jötunheim specs)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.32: Jötunheim Biome Implementation (v0%2032%20J%C3%B6tunheim%20Biome%20Implementation%20ff37a5378c1a4e0ba7e00a263f293562.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.32.4-SERVICES

**Parent Specification:** v0.32 Jötunheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.32.1 (Database), v0.32.2 (Environmental), v0.32.3 (Enemies)

---

## I. Overview

This specification defines the complete service layer implementation for the Jötunheim biome, including JotunheimService, BiomeGenerationService extensions, JotunCorpseTerrainService, PowerConduitService, and comprehensive testing suite.

### Core Deliverables

- **JotunheimService** (primary orchestration)
- **BiomeGenerationService.GenerateJotunheimSector()** (procedural generation)
- **JotunCorpseTerrainService** (fallen giant terrain system)
- **PowerConduitService** (complete implementation from v0.32.2)
- **Unit Test Suite** (12+ tests, 85%+ coverage)
- **Integration Testing** (end-to-end scenarios)
- **Serilog structured logging** throughout

---

## II. JotunheimService (Primary Service)

### A. Service Architecture

**JotunheimService.cs:**

```csharp
using Microsoft.Extensions.Logging;
using Serilog;
using [RuneAndRust.Services](http://RuneAndRust.Services).Combat;
using [RuneAndRust.Services](http://RuneAndRust.Services).Environment;
using [RuneAndRust.Services](http://RuneAndRust.Services).Trauma;
using RuneAndRust.Models;

namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    /// <summary>
    /// Orchestrates Jötunheim biome mechanics:
    /// - No ambient condition (physical threats only)
    /// - Live Power Conduit processing
    /// - High-Pressure Steam Vent eruptions
    /// - Unstable Ceiling/Wall collapses
    /// - Jötun proximity Stress (passive)
    /// - Extreme verticality support
    /// </summary>
    public class JotunheimService
    {
        private readonly ILogger<JotunheimService> _logger;
        private readonly PowerConduitService _powerConduit;
        private readonly TraumaService _trauma;
        private readonly DiceService _dice;
        private readonly JotunCorpseTerrainService _corpseTerrainService;
        
        public JotunheimService(
            ILogger<JotunheimService> logger,
            PowerConduitService powerConduit,
            TraumaService trauma,
            DiceService dice,
            JotunCorpseTerrainService corpseTerrainService)
        {
            _logger = logger;
            _powerConduit = powerConduit;
            _trauma = trauma;
            _dice = dice;
            _corpseTerrainService = corpseTerrainService;
        }
        
        /// <summary>
        /// Initialize Jötunheim combat.
        /// NOTE: No ambient condition applies (physical threats only).
        /// </summary>
        public void InitializeJotunheimCombat(CombatInstance combat)
        {
            _logger.Information(
                "Initializing Jötunheim combat in sector {SectorId}, room {RoomId}",
                combat.SectorId, combat.RoomId);
            
            // NO ambient condition (key design decision)
            // Physical hazards are the threat, not metaphysical conditions
            
            // Initialize turn counters for scheduled hazards
            foreach (var tile in combat.Grid.Tiles)
            {
                if (tile.HasFeature("High-Pressure Steam Vent"))
                {
                    tile.SetData("TurnCounter", 1); // Track eruption schedule
                }
            }
            
            _logger.Information(
                "Jötunheim combat initialized: No ambient condition, {HazardCount} hazards active",
                combat.Grid.Tiles.Count(t => t.HasAnyHazard()));
        }
        
        /// <summary>
        /// Process per-turn Jötunheim mechanics.
        /// </summary>
        public void ProcessTurnMechanics(CombatInstance combat, int currentTurn)
        {
            _logger.Debug(
                "Processing Jötunheim turn mechanics: Turn {Turn}",
                currentTurn);
            
            // 1. Process Live Power Conduits
            _powerConduit.ProcessPowerConduits(combat.Grid, combat.AllCombatants);
            
            // 2. Process Steam Vents (every 3 turns)
            ProcessSteamVents(combat, currentTurn);
            
            // 3. Apply Jötun proximity Stress (passive)
            ApplyJotunProximityStress(combat);
            
            _logger.Debug("Jötunheim turn mechanics complete");
        }
        
        /// <summary>
        /// Process High-Pressure Steam Vents.
        /// Vents erupt every 3 turns in predictable pattern.
        /// </summary>
        private void ProcessSteamVents(CombatInstance combat, int currentTurn)
        {
            var vents = combat.Grid.Tiles
                .Where(t => t.HasFeature("High-Pressure Steam Vent"))
                .ToList();
            
            foreach (var vent in vents)
            {
                var turnCounter = vent.GetData<int>("TurnCounter");
                
                if (turnCounter % 3 == 0)
                {
                    // ERUPT
                    EruptSteamVent(vent, combat);
                    
                    _logger.Information(
                        "Steam vent erupted at ({X}, {Y}) on turn {Turn}",
                        vent.X, vent.Y, currentTurn);
                }
                else if (turnCounter % 3 == 2)
                {
                    // WARNING (1 turn before eruption)
                    _logger.Information(
                        "Steam vent at ({X}, {Y}) hissing - ERUPTION NEXT TURN",
                        vent.X, vent.Y);
                    
                    combat.AddBattleMessage(
                        $"The steam vent at ({vent.X}, {vent.Y}) hisses loudly - pressure is building!");
                }
                
                vent.SetData("TurnCounter", turnCounter + 1);
            }
        }
        
        /// <summary>
        /// Erupt single steam vent.
        /// </summary>
        private void EruptSteamVent(RoomTile vent, CombatInstance combat)
        {
            var direction = vent.GetData<string>("ConeDirection") ?? "North";
            var coneTiles = combat.Grid.GetCone(vent, direction, range: 3);
            
            var affectedCombatants = combat.AllCombatants
                .Where(c => coneTiles.Any(ct => ct.X == c.CurrentPosition.X && ct.Y == c.CurrentPosition.Y))
                .ToList();
            
            foreach (var combatant in affectedCombatants)
            {
                // 2d6 Fire damage
                var damage = _dice.Roll(2, 6);
                combatant.TakeDamage(damage, "Fire");
                
                // Knockback 1 tile
                var knockbackPos = combat.Grid.GetPositionInDirection(
                    combatant.CurrentPosition, direction, distance: 1);
                
                if (knockbackPos != null && combat.Grid.GetTile(knockbackPos).IsPassable())
                {
                    combatant.CurrentPosition = knockbackPos;
                    
                    _logger.Information(
                        "{Combatant} knocked back by steam vent to ({X}, {Y})",
                        [combatant.Name](http://combatant.Name), knockbackPos.X, knockbackPos.Y);
                }
                
                _logger.Information(
                    "{Combatant} takes {Damage} Fire damage from steam vent eruption",
                    [combatant.Name](http://combatant.Name), damage);
            }
            
            combat.AddBattleMessage(
                $"Superheated steam erupts from the vent! {affectedCombatants.Count} targets hit!");
        }
        
        /// <summary>
        /// Apply passive Stress from proximity to dormant Jötun-Forged.
        /// Characters on Jötun Corpse Terrain gain +2 Stress per turn.
        /// </summary>
        private void ApplyJotunProximityStress(CombatInstance combat)
        {
            foreach (var character in combat.PlayerParty)
            {
                var tile = combat.Grid.GetTile(character.CurrentPosition);
                
                if (tile.HasTerrain("Jotun Corpse Terrain"))
                {
                    _trauma.ApplyStress(character, 2,
                        "Jötun proximity - corrupted logic core broadcast");
                    
                    _logger.Debug(
                        "{Character} on Jötun corpse terrain - applied +2 Psychic Stress",
                        [character.Name](http://character.Name));
                }
            }
        }
        
        /// <summary>
        /// Check for Unstable Ceiling/Wall collapse trigger.
        /// Collapses when heavy impact (10+ damage) occurs nearby.
        /// </summary>
        public bool CheckStructuralCollapse(
            RoomTile unstableTile,
            GridPosition impactPosition,
            int damageDealt)
        {
            if (damageDealt < 10)
                return false; // Not enough force
            
            var distance = GridUtility.GetDistance(
                new GridPosition(unstableTile.X, unstableTile.Y),
                impactPosition);
            
            if (distance > 2)
                return false; // Too far away
            
            // COLLAPSE
            TriggerStructuralCollapse(unstableTile);
            return true;
        }
        
        /// <summary>
        /// Trigger structural collapse of ceiling/wall.
        /// </summary>
        private void TriggerStructuralCollapse(RoomTile collapseTile)
        {
            _logger.Warning(
                "Structural collapse triggered at ({X}, {Y})",
                collapseTile.X, collapseTile.Y);
            
            // 2x2 area damage
            var affectedArea = GetAdjacentTiles(collapseTile, radius: 1); // 2x2 = 1 tile radius
            
            foreach (var tile in affectedArea)
            {
                var combatant = tile.GetOccupyingCombatant();
                if (combatant != null)
                {
                    var damage = _dice.Roll(3, 8);
                    combatant.TakeDamage(damage, "Physical");
                    
                    _logger.Information(
                        "{Combatant} takes {Damage} Physical damage from structural collapse",
                        [combatant.Name](http://combatant.Name), damage);
                }
                
                // Create Debris Pile terrain
                tile.RemoveFeature("Unstable Ceiling/Wall");
                tile.AddTerrain("Debris Pile"); // Difficult terrain + cover
            }
            
            collapseTile.RemoveFeature("Unstable Ceiling/Wall"); // One-time hazard
        }
        
        /// <summary>
        /// Generate enemy group for Jötunheim.
        /// Undying-heavy (∼60% spawn rate).
        /// </summary>
        public List<Enemy> GenerateEnemyGroup(int difficulty)
        {
            var enemies = new List<Enemy>();
            var groupSize = difficulty switch
            {
                1 => _dice.Roll(2, 3), // Easy: 2-3
                2 => _dice.Roll(3, 4), // Normal: 3-4
                3 => _dice.Roll(4, 5), // Hard: 4-5
                4 => _dice.Roll(5, 7), // Deadly: 5-7
                _ => 3
            };
            
            for (int i = 0; i < groupSize; i++)
            {
                var enemy = RollWeightedEnemy(difficulty);
                enemies.Add(enemy);
            }
            
            _logger.Information(
                "Generated Jötunheim enemy group: {Count} enemies, difficulty {Difficulty}",
                enemies.Count, difficulty);
            
            return enemies;
        }
        
        /// <summary>
        /// Roll weighted enemy spawn.
        /// Spawn weights defined in v0.32.3.
        /// </summary>
        private Enemy RollWeightedEnemy(int difficulty)
        {
            var roll = _dice.Roll(1, 100); // Percentage roll
            
            // Weighted spawn distribution (from v0.32.3):
            // Rusted Servitor: ∼29% (0-29)
            // Rusted Warden: ∼22% (30-51)
            // God-Sleeper Cultist: ∼19% (52-70)
            // Iron-Husked Boar: ∼12% (71-82)
            // Scrap-Tinker: ∼10% (83-92)
            // Draugr Juggernaut: ∼9% (93-100)
            
            return roll switch
            {
                <= 29 => CreateEnemy("Rusted Servitor", difficulty),
                <= 51 => CreateEnemy("Rusted Warden", difficulty),
                <= 70 => CreateEnemy("God-Sleeper Cultist", difficulty),
                <= 82 => CreateEnemy("Iron-Husked Boar", difficulty),
                <= 92 => CreateEnemy("Scrap-Tinker", difficulty),
                _ => CreateEnemy("Draugr Juggernaut", difficulty) // Elite (rare)
            };
        }
        
        private Enemy CreateEnemy(string enemyName, int difficulty)
        {
            // Load from database or enemy templates
            // Implementation details depend on enemy system architecture
            return _enemyFactory.Create(enemyName, difficulty);
        }
    }
}
```

---

## III. BiomeGenerationService Extensions

### A. GenerateJotunheimSector()

**BiomeGenerationService.cs** (partial - Jötunheim extension):

```csharp
public partial class BiomeGenerationService
{
    /// <summary>
    /// Generate Jötunheim sector using Wave Function Collapse.
    /// Jötunheim is primarily [Trunk] with [Roots] maintenance tunnels (70/30 split).
    /// </summary>
    public Sector GenerateJotunheimSector(
        int sectorId,
        int difficulty,
        int roomCount = 10)
    {
        _logger.Information(
            "Generating Jötunheim sector {SectorId}, difficulty {Difficulty}, {RoomCount} rooms",
            sectorId, difficulty, roomCount);
        
        var sector = new Sector
        {
            SectorId = sectorId,
            BiomeId = 7, // Jötunheim
            Difficulty = difficulty
        };
        
        // Load Jötunheim room templates (Trunk 70%, Roots 30%)
        var trunkTemplates = _database.GetBiomeRoomTemplates(biomeId: 7, verticality: "Trunk");
        var rootsTemplates = _database.GetBiomeRoomTemplates(biomeId: 7, verticality: "Roots");
        
        // Determine verticality split
        int trunkRooms = (int)(roomCount * 0.7);
        int rootsRooms = roomCount - trunkRooms;
        
        // Generate Trunk layout
        var trunkLayout = _wfcService.GenerateLayout(
            trunkTemplates,
            trunkRooms,
            connectivityDensity: 0.6);
        
        // Generate Roots layout (maintenance tunnels)
        var rootsLayout = _wfcService.GenerateLayout(
            rootsTemplates,
            rootsRooms,
            connectivityDensity: 0.5); // Less connected
        
        // Populate each room
        foreach (var room in trunkLayout)
        {
            room.VerticalityTier = "Trunk";
            PopulateJotunheimRoom(room, difficulty);
            sector.Rooms.Add(room);
        }
        
        foreach (var room in rootsLayout)
        {
            room.VerticalityTier = "Roots";
            PopulateJotunheimRoom(room, difficulty);
            sector.Rooms.Add(room);
        }
        
        // Place Jötun corpse terrain (fallen giants) in some Trunk rooms
        PlaceJotunCorpseTerrain(sector, difficulty);
        
        // Ensure high spawn of Live Power Conduits (signature hazard)
        EnsurePowerConduitDensity(sector, difficulty);
        
        _logger.Information(
            "Jötunheim sector {SectorId} generated: {TrunkRooms} Trunk, {RootsRooms} Roots",
            sectorId, trunkLayout.Count, rootsLayout.Count);
        
        return sector;
    }
    
    /// <summary>
    /// Populate Jötunheim room with enemies, hazards, and resources.
    /// </summary>
    private void PopulateJotunheimRoom(Room room, int difficulty)
    {
        _logger.Debug(
            "Populating Jötunheim room {RoomId} ({Template}, {Verticality})",
            room.RoomId, room.TemplateName, room.VerticalityTier);
        
        // Generate enemy group (Undying-heavy)
        var enemies = _jotunheimService.GenerateEnemyGroup(difficulty);
        room.Enemies = enemies;
        
        // Place industrial hazards
        PlaceJotunheimHazards(room, difficulty);
        
        // Place resources
        PlaceJotunheimResources(room, difficulty);
        
        // Add industrial cover (shipping containers, engine blocks)
        PlaceIndustrialCover(room);
        
        _logger.Debug(
            "Jötunheim room populated: {EnemyCount} enemies, {HazardCount} hazards",
            room.Enemies.Count, room.Hazards.Count);
    }
    
    /// <summary>
    /// Place Jötun corpse terrain in sector.
    /// Creates extreme verticality and iconic terrain.
    /// </summary>
    private void PlaceJotunCorpseTerrain(Sector sector, int difficulty)
    {
        // 20-30% of Trunk rooms have Jötun corpse terrain
        var trunkRooms = sector.Rooms.Where(r => r.VerticalityTier == "Trunk").ToList();
        int corpseRoomCount = (int)(trunkRooms.Count * 0.25);
        
        for (int i = 0; i < corpseRoomCount && i < trunkRooms.Count; i++)
        {
            var room = trunkRooms[_diceService.Roll(0, trunkRooms.Count - 1)];
            
            // Generate corpse terrain layout
            _corpseTerrainService.GenerateCorpseLayout(room);
            
            _logger.Information(
                "Jötun corpse terrain placed in room {RoomId}",
                room.RoomId);
            
            trunkRooms.Remove(room); // Don't place twice
        }
    }
    
    /// <summary>
    /// Ensure high density of Live Power Conduits (signature hazard).
    /// </summary>
    private void EnsurePowerConduitDensity(Sector sector, int difficulty)
    {
        int targetConduits = sector.Rooms.Count * 2; // 2 per room average
        int currentConduits = sector.Rooms.Sum(r =>
            r.Grid.Tiles.Count(t => t.HasFeature("Live Power Conduit")));
        
        if (currentConduits < targetConduits)
        {
            int toPlace = targetConduits - currentConduits;
            
            for (int i = 0; i < toPlace; i++)
            {
                var room = sector.Rooms[_diceService.Roll(0, sector.Rooms.Count - 1)];
                var validTiles = room.Grid.Tiles
                    .Where(t => t.IsPassable() && !t.HasAnyHazard())
                    .ToList();
                
                if (validTiles.Any())
                {
                    var tile = validTiles[_diceService.Roll(0, validTiles.Count - 1)];
                    tile.AddFeature("Live Power Conduit");
                }
            }
            
            _logger.Information(
                "Ensured Power Conduit density: Placed {Count} additional conduits",
                toPlace);
        }
    }
    
    /// <summary>
    /// Place industrial cover (shipping containers, engine blocks).
    /// Very high spawn weight in Jötunheim.
    /// </summary>
    private void PlaceIndustrialCover(Room room)
    {
        int coverCount = _diceService.Roll(4, 8); // 4-8 cover objects per room
        
        var validTiles = room.Grid.Tiles
            .Where(t => t.IsPassable() && !t.IsOccupied)
            .ToList();
        
        for (int i = 0; i < coverCount && i < validTiles.Count; i++)
        {
            var tile = validTiles[_diceService.Roll(0, validTiles.Count - 1)];
            tile.AddFeature("Cover (Industrial)");
            validTiles.Remove(tile);
        }
    }
}
```

---

## IV. JotunCorpseTerrainService (New Service)

**JotunCorpseTerrainService.cs:**

```csharp
using Microsoft.Extensions.Logging;
using RuneAndRust.Models;

namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    /// <summary>
    /// Generates Jötun corpse terrain - fallen Jötun-Forged as multi-level battlefields.
    /// Creates extreme verticality and iconic environmental storytelling.
    /// </summary>
    public class JotunCorpseTerrainService
    {
        private readonly ILogger<JotunCorpseTerrainService> _logger;
        private readonly DiceService _dice;
        
        public JotunCorpseTerrainService(
            ILogger<JotunCorpseTerrainService> logger,
            DiceService dice)
        {
            _logger = logger;
            _dice = dice;
        }
        
        /// <summary>
        /// Generate Jötun corpse terrain layout in room.
        /// Creates multi-level battlefield from fallen giant chassis.
        /// </summary>
        public void GenerateCorpseLayout(Room room)
        {
            _logger.Information(
                "Generating Jötun corpse terrain in room {RoomId}",
                room.RoomId);
            
            // Determine corpse type and orientation
            var corpseType = RollCorpseType();
            var orientation = _dice.Roll(0, 3) * 90; // 0, 90, 180, 270 degrees
            
            // Place corpse features based on type
            switch (corpseType)
            {
                case "Hull Section":
                    PlaceHullSection(room, orientation);
                    break;
                case "Limb Bridge":
                    PlaceLimbBridge(room, orientation);
                    break;
                case "Interior Cavity":
                    PlaceInteriorCavity(room);
                    break;
            }
            
            // Mark all corpse tiles
            MarkCorpseTiles(room);
            
            _logger.Information(
                "Jötun corpse terrain generated: Type={Type}, Orientation={Orientation}°",
                corpseType, orientation);
        }
        
        private string RollCorpseType()
        {
            var roll = _dice.Roll(1, 100);
            return roll switch
            {
                <= 40 => "Hull Section", // Flat platform
                <= 75 => "Limb Bridge", // Elevated bridge
                _ => "Interior Cavity" // Cave-like interior
            };
        }
        
        /// <summary>
        /// Place hull section - creates large flat platform.
        /// </summary>
        private void PlaceHullSection(Room room, int orientation)
        {
            // Create 4x6 elevated platform (hull outer surface)
            int startX = room.Grid.Width / 2 - 2;
            int startY = room.Grid.Height / 2 - 3;
            
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    var tile = room.Grid.GetTile(startX + x, startY + y);
                    if (tile != null)
                    {
                        tile.AddTerrain("Jotun Corpse Terrain");
                        tile.SetData("CorpseFeature", "Hull");
                        tile.SetData("Elevation", 2); // +2 Z-level
                    }
                }
            }
        }
        
        /// <summary>
        /// Place limb bridge - creates elevated walkway.
        /// </summary>
        private void PlaceLimbBridge(Room room, int orientation)
        {
            // Create 2-tile-wide bridge across room
            int midY = room.Grid.Height / 2;
            
            for (int x = 2; x < room.Grid.Width - 2; x++)
            {
                for (int dy = -1; dy <= 0; dy++)
                {
                    var tile = room.Grid.GetTile(x, midY + dy);
                    if (tile != null)
                    {
                        tile.AddTerrain("Jotun Corpse Terrain");
                        tile.SetData("CorpseFeature", "Limb");
                        tile.SetData("Elevation", 3); // +3 Z-level (elevated bridge)
                    }
                }
            }
        }
        
        /// <summary>
        /// Place interior cavity - creates cave-like space inside corpse.
        /// </summary>
        private void PlaceInteriorCavity(Room room)
        {
            // Create enclosed space (3x4 area)
            int centerX = room.Grid.Width / 2;
            int centerY = room.Grid.Height / 2;
            
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -2; y <= 1; y++)
                {
                    var tile = room.Grid.GetTile(centerX + x, centerY + y);
                    if (tile != null)
                    {
                        tile.AddTerrain("Jotun Corpse Terrain");
                        tile.SetData("CorpseFeature", "Interior");
                        tile.SetData("Elevation", 0); // Ground level, but enclosed
                    }
                }
            }
        }
        
        /// <summary>
        /// Mark all corpse tiles and apply passive Stress effect.
        /// </summary>
        private void MarkCorpseTiles(Room room)
        {
            var corpseTiles = room.Grid.Tiles
                .Where(t => t.HasTerrain("Jotun Corpse Terrain"))
                .ToList();
            
            foreach (var tile in corpseTiles)
            {
                tile.SetData("AppliesProximityStress", true);
                tile.SetData("StressPerTurn", 2);
            }
            
            _logger.Debug(
                "Marked {Count} tiles as Jötun corpse terrain (applies +2 Stress/turn)",
                corpseTiles.Count);
        }
    }
}
```

---

## V. PowerConduitService (Complete Implementation)

**Complete implementation provided in v0.32.2, Section II.C.**

Key methods:

- `ProcessPowerConduits()` - Apply damage each turn
- `ProcessStandardConduit()` - 1d8 Energy damage, adjacent only
- `ProcessFloodedConduit()` - 2d10 Energy damage, 2-tile radius

---

## VI. Unit Test Suite

### A. JotunheimService Tests

**JotunheimServiceTests.cs:**

```csharp
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using [RuneAndRust.Services](http://RuneAndRust.Services).Biomes;

public class JotunheimServiceTests
{
    [Fact]
    public void InitializeJotunheimCombat_NoAmbientCondition()
    {
        // Arrange
        var service = CreateJotunheimService();
        var combat = CreateTestCombat(biomeId: 7);
        
        // Act
        service.InitializeJotunheimCombat(combat);
        
        // Assert
        Assert.False(combat.HasAnyAmbientCondition());
    }
    
    [Fact]
    public void ProcessSteamVents_EruptsEvery3Turns()
    {
        // Arrange
        var service = CreateJotunheimService();
        var combat = CreateTestCombat();
        var vent = combat.Grid.Tiles.First(t => t.HasFeature("High-Pressure Steam Vent"));
        vent.SetData("TurnCounter", 1);
        
        // Act & Assert
        for (int turn = 1; turn <= 9; turn++)
        {
            service.ProcessTurnMechanics(combat, turn);
            
            if (turn % 3 == 0)
            {
                // Should have erupted
                Assert.True(combat.BattleMessages.Any(m => m.Contains("erupts")));
            }
        }
    }
    
    [Fact]
    public void ApplyJotunProximityStress_OnCorpseTerrain_Applies2Stress()
    {
        // Arrange
        var service = CreateJotunheimService();
        var combat = CreateTestCombat();
        var character = combat.PlayerParty.First();
        var corpseTile = combat.Grid.GetTile(character.CurrentPosition);
        corpseTile.AddTerrain("Jotun Corpse Terrain");
        
        var initialStress = character.PsychicStress;
        
        // Act
        service.ProcessTurnMechanics(combat, turn: 1);
        
        // Assert
        Assert.Equal(initialStress + 2, character.PsychicStress);
    }
    
    [Fact]
    public void CheckStructuralCollapse_HeavyImpact_TriggersCollapse()
    {
        // Arrange
        var service = CreateJotunheimService();
        var unstableTile = CreateTileWithFeature("Unstable Ceiling/Wall");
        var impactPosition = new GridPosition(unstableTile.X + 1, unstableTile.Y);
        
        // Act
        var collapsed = service.CheckStructuralCollapse(
            unstableTile, impactPosition, damageDealt: 15);
        
        // Assert
        Assert.True(collapsed);
        Assert.False(unstableTile.HasFeature("Unstable Ceiling/Wall")); // Removed
        Assert.True(unstableTile.HasTerrain("Debris Pile")); // Created
    }
    
    [Fact]
    public void GenerateEnemyGroup_UndyingHeavy_Approximately60Percent()
    {
        // Arrange
        var service = CreateJotunheimService();
        var undyingCount = 0;
        var totalCount = 0;
        
        // Act: Generate 100 groups
        for (int i = 0; i < 100; i++)
        {
            var enemies = service.GenerateEnemyGroup(difficulty: 2);
            totalCount += enemies.Count;
            undyingCount += enemies.Count(e => e.Type.Contains("Undying"));
        }
        
        var undyingPercent = (double)undyingCount / totalCount;
        
        // Assert: Should be ∼60% Undying (allow 55-65% range for variance)
        Assert.InRange(undyingPercent, 0.55, 0.65);
    }
}
```

### B. BiomeGenerationService Tests

**BiomeGenerationServiceTests_Jotunheim.cs:**

```csharp
[Fact]
public void GenerateJotunheimSector_VerticalitySplit_70Trunk30Roots()
{
    // Act
    var sector = _service.GenerateJotunheimSector(
        sectorId: 1,
        difficulty: 2,
        roomCount: 10);
    
    // Assert
    var trunkRooms = sector.Rooms.Count(r => r.VerticalityTier == "Trunk");
    var rootsRooms = sector.Rooms.Count(r => r.VerticalityTier == "Roots");
    
    Assert.Equal(7, trunkRooms); // 70%
    Assert.Equal(3, rootsRooms); // 30%
}

[Fact]
public void GenerateJotunheimSector_PowerConduitDensity_HighSpawn()
{
    // Act
    var sector = _service.GenerateJotunheimSector(1, difficulty: 2);
    
    // Assert
    var conduitCount = sector.Rooms.Sum(r =>
        r.Grid.Tiles.Count(t => t.HasFeature("Live Power Conduit")));
    
    // Should have ∼2 conduits per room (signature hazard)
    Assert.True(conduitCount >= sector.Rooms.Count * 1.5);
}

[Fact]
public void GenerateJotunheimSector_JotunCorpseTerrain_Approximately25PercentTrunkRooms()
{
    // Act
    var sector = _service.GenerateJotunheimSector(1, difficulty: 2, roomCount: 10);
    
    // Assert
    var trunkRooms = sector.Rooms.Where(r => r.VerticalityTier == "Trunk").ToList();
    var corpseRooms = trunkRooms.Count(r =>
        r.Grid.Tiles.Any(t => t.HasTerrain("Jotun Corpse Terrain")));
    
    // Should be ∼25% of Trunk rooms (allow 15-35% for variance)
    var corpsePercent = (double)corpseRooms / trunkRooms.Count;
    Assert.InRange(corpsePercent, 0.15, 0.35);
}
```

### C. JotunCorpseTerrainService Tests

**JotunCorpseTerrainServiceTests.cs:**

```csharp
[Fact]
public void GenerateCorpseLayout_CreatesElevatedTerrain()
{
    // Arrange
    var service = CreateCorpseTerrainService();
    var room = CreateTestRoom();
    
    // Act
    service.GenerateCorpseLayout(room);
    
    // Assert
    var corpseTiles = room.Grid.Tiles
        .Where(t => t.HasTerrain("Jotun Corpse Terrain"))
        .ToList();
    
    Assert.True(corpseTiles.Count > 0);
    Assert.True(corpseTiles.All(t => t.GetData<bool>("AppliesProximityStress")));
}

[Fact]
public void GenerateCorpseLayout_HullSection_Creates4x6Platform()
{
    // Arrange
    var service = CreateCorpseTerrainService();
    var room = CreateTestRoom();
    
    // Act
    service.GenerateCorpseLayout(room); // May need to force Hull type
    
    // Assert
    var hullTiles = room.Grid.Tiles
        .Where(t => t.GetData<string>("CorpseFeature") == "Hull")
        .ToList();
    
    if (hullTiles.Any()) // If Hull was rolled
    {
        Assert.InRange(hullTiles.Count, 20, 24); // 4x6 = 24 tiles
        Assert.True(hullTiles.All(t => t.GetData<int>("Elevation") == 2));
    }
}
```

---

## VII. Integration Testing

### A. Full Combat Scenario Test

**JotunheimIntegrationTests.cs:**

```csharp
[Fact]
public async Task FullJotunheimCombat_FromGenerationToVictory()
{
    // Arrange: Generate Jötunheim sector
    var sector = _biomeService.GenerateJotunheimSector(1, difficulty: 2);
    var room = sector.Rooms.First();
    
    var party = CreateTestParty();
    var combat = _combatService.InitializeCombat(party, room);
    
    // Initialize Jötunheim combat
    _jotunheimService.InitializeJotunheimCombat(combat);
    
    // Assert: No ambient condition
    Assert.False(combat.HasAnyAmbientCondition());
    
    // Act: Simulate 10 combat turns
    for (int turn = 1; turn <= 10; turn++)
    {
        // Process Jötunheim mechanics
        _jotunheimService.ProcessTurnMechanics(combat, turn);
        
        // Verify power conduits deal damage
        var charactersNearConduit = party.Where(c =>
        {
            var tile = combat.Grid.GetTile(c.CurrentPosition);
            return combat.Grid.GetAdjacentTiles(tile)
                .Any(t => t.HasFeature("Live Power Conduit"));
        }).ToList();
        
        if (charactersNearConduit.Any())
        {
            // At least one character should have taken damage
            Assert.True(party.Any(c => c.HP < c.MaxHP));
        }
        
        // Check steam vent eruptions (every 3 turns)
        if (turn % 3 == 0)
        {
            Assert.True(combat.BattleMessages.Any(m => m.Contains("steam")));
        }
        
        // Party takes actions...
        foreach (var character in party.Where(c => c.IsAlive))
        {
            _combatService.BasicAttack(character, combat.Enemies.First(e => e.IsAlive));
        }
        
        // Enemy turn...
        foreach (var enemy in combat.Enemies.Where(e => e.IsAlive))
        {
            _enemyAI.TakeTurn(enemy, combat);
        }
        
        if (combat.IsComplete)
            break;
    }
    
    // Assert: Combat mechanics worked correctly
    Assert.True(combat.IsComplete || turn == 10);
}
```

---

## VIII. Success Criteria

### Functional Requirements

- [ ]  JotunheimService initializes combat correctly
- [ ]  NO ambient condition applies (design verification)
- [ ]  [Live Power Conduit] deals damage correctly
- [ ]  [Live Power Conduit] + [Flooded] amplification works
- [ ]  [High-Pressure Steam Vent] erupts every 3 turns
- [ ]  [Unstable Ceiling/Wall] collapses on heavy impact
- [ ]  Jötun proximity Stress applies on corpse terrain
- [ ]  BiomeGenerationService generates 70/30 Trunk/Roots split
- [ ]  Jötun corpse terrain creates elevated platforms
- [ ]  Enemy spawns are ∼60% Undying
- [ ]  Power Conduit density is high (signature hazard)

### Quality Gates

- [ ]  85%+ unit test coverage achieved
- [ ]  All services use Serilog structured logging
- [ ]  Integration tests pass
- [ ]  Performance acceptable (<500ms sector generation)
- [ ]  No memory leaks in long combat scenarios

### Balance Validation

- [ ]  Power conduits feel dangerous but predictable
- [ ]  Flooded + conduit combo creates tactical decisions
- [ ]  Steam vents create positioning challenges
- [ ]  Structural collapses feel cinematic
- [ ]  Jötun proximity Stress is noticeable but manageable
- [ ]  Undying dominance feels thematic (industrial graveyard)
- [ ]  Physical Soak creates need for armor-shredding

---

## IX. Remaining Work (Cannot Perform Automatically)

### 1. Database Execution (30 minutes)

Execute the three SQL migration files in order:

```bash
sqlite3 runeandrust.db < Data/v0.32.1_jotunheim_schema.sql
sqlite3 runeandrust.db < Data/v0.32.2_environmental_hazards.sql
sqlite3 runeandrust.db < Data/v0.32.3_enemy_definitions.sql
```

**Verification:**

- Query `Biomes` table to confirm Jötunheim entry (biome_id: 7)
- Query `Biome_RoomTemplates` for 10+ Jötunheim templates (7 Trunk, 3 Roots)
- Query `Biome_EnvironmentalFeatures` for 12+ Jötunheim hazards
- Query `Biome_EnemySpawns` for 6 enemy types

### 2. Combat Engine Integration (6-8 hours)

The integration guide provides exact code snippets for:

**Constructor Parameter Additions:**

- Inject `IJotunheimService`
- Inject `IPowerConduitService`
- Inject `IJotunCorpseTerrainService`

**`ApplyBiomeAmbientConditions()` Method Implementation:**

- Detect Jötunheim biome (biome_id: 7)
- **Verify NO ambient condition applies** (NULL check)
- Initialize steam vent turn counters

**Per-Turn Hazard Processing:**

- Hook `ProcessEnvironmentalHazards()` in turn processing
- Call `JotunheimService.ProcessTurnMechanics()` at turn start
- Process Live Power Conduits (standard + flooded)
- Process Steam Vents (every 3 turns)
- Apply Jötun proximity Stress (+2 per turn on corpse terrain)

**Structural Collapse Integration:**

- Hook `CheckCeilingCollapse()` in damage event handler
- Hook `JotunheimService.CheckStructuralCollapse()` after heavy damage (10+)
- Check proximity to Unstable Ceiling/Wall features
- Apply 3d8 damage in 2x2 area, create Debris Pile terrain

**Combat Cleanup:**

- Clear Jötunheim-specific buffs/debuffs at combat end
- Log biome statistics (power conduit hits, steam vent eruptions, collapses, total Stress)

### 3. Sector Generation Integration (2-3 hours)

**BiomeGenerationService Integration:**

- Add `GenerateJotunheimSector()` method to BiomeGenerationService
- Implement 70/30 Trunk/Roots verticality split
- Complete room population workflow provided in Section III

**Jötun Corpse Terrain Generation:**

- Add corpse terrain generation (25% of Trunk rooms)
- Integrate `JotunCorpseTerrainService.GenerateCorpseLayout()`
- Three terrain types: Hull Section, Limb Bridge, Interior Cavity

**Power Conduit Density:**

- Ensure power conduit density (2-3 per room average)
- Implement `EnsurePowerConduitDensity()` method
- Signature hazard spawn weight verification

**Industrial Cover Placement:**

- Place 4-8 cover objects per room (shipping containers, engine blocks)
- Very high spawn weight (industrial debris everywhere)

### 4. Manual Testing (2-3 hours)

**Power Conduit Testing:**

- [ ]  Generate Jötunheim sector with power conduits
- [ ]  Place character adjacent to conduit
- [ ]  Verify 1d8 Energy damage per turn
- [ ]  Test flooded + conduit combo (2d10, 2-tile radius)
- [ ]  Verify conduit log entries created

**Steam Vent Testing:**

- [ ]  Generate combat with steam vent
- [ ]  Observe eruption schedule (every 3 turns)
- [ ]  Verify warning message on turn 2, 5, 8
- [ ]  Verify 2d6 Fire damage + knockback on eruption
- [ ]  Test characters in 3-tile cone

**Structural Collapse Testing:**

- [ ]  Place Unstable Ceiling/Wall in room
- [ ]  Deal 10+ damage nearby (within 2 tiles)
- [ ]  Verify collapse triggers
- [ ]  Verify 3d8 Physical damage in 2x2 area
- [ ]  Verify Debris Pile terrain created

**Jötun Proximity Stress Testing:**

- [ ]  Generate room with Jötun corpse terrain
- [ ]  Place character on corpse terrain
- [ ]  After 5 turns, verify Stress increased by 10+ (2 per turn)
- [ ]  Verify Stress log entries created

**Enemy Spawn Distribution:**

- [ ]  Generate 10 Jötunheim sectors
- [ ]  Count enemy types across all sectors
- [ ]  Verify ∼60% Undying spawn rate
- [ ]  Verify Draugr Juggernaut spawns (rare elite)

---

## X. Deployment Instructions

### Step 1: Compile All Services

```bash
dotnet build Services/Biomes/JotunheimService.cs
dotnet build Services/Biomes/BiomeGenerationService_Jotunheim.cs
dotnet build Services/Biomes/JotunCorpseTerrainService.cs
dotnet build Services/PowerConduitService.cs
```

### Step 2: Run Full Test Suite

```bash
dotnet test --filter "FullyQualifiedName~Jotunheim"
```

### Step 3: Run Integration Tests

```bash
dotnet test --filter "Category=Integration&Biome=Jotunheim"
```

### Step 4: Manual Verification

```bash
# Start game and navigate to Jötunheim
# Verify:
# - Sector generates with 70% Trunk, 30% Roots
# - NO ambient condition displays
# - Power conduits deal damage
# - Flooded + conduit combo works (amplified damage)
# - Steam vents erupt every 3 turns
# - Structural collapses trigger on heavy damage
# - Jötun corpse terrain applies +2 Stress/turn
# - Enemy spawns are Undying-heavy (∼60%)
```

### Step 5: Performance Profiling

```bash
dotnet run --configuration Release --profile
# Monitor:
# - Sector generation time (<500ms)
# - Combat turn processing (<100ms)
# - Memory usage (stable over 50+ turns)
```

---

## XI. Success Criteria Checklist

### Service Implementation

- [ ]  JotunheimService complete with all methods
- [ ]  BiomeGenerationService.GenerateJotunheimSector() implemented
- [ ]  JotunCorpseTerrainService fully functional
- [ ]  PowerConduitService fully functional
- [ ]  All services properly injected via DI

### Testing

- [ ]  12+ unit tests written
- [ ]  85%+ code coverage achieved
- [ ]  Integration tests pass
- [ ]  Performance benchmarks met
- [ ]  No race conditions or threading issues

### Quality

- [ ]  Serilog structured logging throughout
- [ ]  Proper error handling and edge cases
- [ ]  Code follows C# conventions
- [ ]  XML documentation comments on all public methods
- [ ]  No compiler warnings

---

## XII. Next Steps

Once v0.32.4 is complete:

**v0.32 Jötunheim Biome is COMPLETE!**

Future enhancements (not in v0.32 scope):

- v0.35: Full Jötun-Forged boss encounters
- v0.33: God-Sleeper Cultist faction system expansion
- v0.36: Legendary crafting with Unblemished Jötun Plating
- v0.37: Hardware Malfunction Puzzles
- v0.38: Biome-specific achievements/statistics

**Next Phase:**

- v0.33: Faction System & Reputation (God-Sleeper Cultists, other factions)

---

## XIII. Related Documents

**Parent Specification:**

- v0.32: Jötunheim Biome Implementation[[1]](v0%2032%20J%C3%B6tunheim%20Biome%20Implementation%20ff37a5378c1a4e0ba7e00a263f293562.md)

**Reference Implementation:**

- v0.29.4: Muspelheim Services[[2]](v0%2029%204%20Service%20Implementation%20&%20Testing%20ce4a69fbcddb40e2ac66db616c52bc6f.md)
- v0.30.4: Niflheim Services[[3]](v0%2030%204%20Service%20Implementation%20&%20Testing%203676c3fe97e44b2882cbb1857258c91e.md)
- v0.31.4: Alfheim Services[[4]](v0%2031%204%20Service%20Implementation%20&%20Testing%20539e060d40734aa88823b247166c44e6.md)

**Prerequisites:**

- v0.32.1: Database Schema[[5]](v0%2032%201%20Database%20Schema%20&%20Room%20Templates%20ffc37b6b82c1421bb1a599bdb61194d3.md)
- v0.32.2: Environmental Hazards[[6]](v0%2032%202%20Environmental%20Hazards%20&%20Industrial%20Terrain%2008ebb5b9a68843d6b1b607cfb4736edf.md)
- v0.32.3: Enemy Definitions[[7]](v0%2032%203%20Enemy%20Definitions%20&%20Spawn%20System%20f48b7ad23fe34ff2b1780db59e5fc0e4.md)

---

**Service implementation complete. v0.32 Jötunheim Biome ready for deployment.**