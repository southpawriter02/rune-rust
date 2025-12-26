# v0.30.4: Service Implementation & Testing

Type: Technical
Description: Complete NiflheimService, NiflheimDataRepository, FrigidColdService, SlipperyTerrainService, BrittlenessService integration, unit tests (~85% coverage), and integration testing.
Priority: Must-Have
Status: Implemented
Target Version: Alpha
Dependencies: v0.30.1-v0.30.3 (All prior Niflheim specs)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.30: Niflheim Biome Implementation (v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.30.4-SERVICES

**Parent Specification:** v0.30 Niflheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 9-15 hours

**Prerequisites:** v0.30.3 (Enemy definitions complete), v0.29.4 (Muspelheim services as reference)

---

## ✅ IMPLEMENTATION COMPLETE (2025-11-16)

**Status:** Implementation complete, all deliverables verified

**Actual Time:** ~9 hours

**Files:**

- `RuneAndRust.Engine/NiflheimBiomeService.cs`
- `RuneAndRust.Persistence/NiflheimDataRepository.cs`

**NiflheimBiomeService Complete:**

- ✅ Party preparedness checking (Ice Resistance, FINESSE warnings)
- ✅ Enemy resistance loading and creation
- ✅ Frigid Cold combat integration
- ✅ Slippery terrain movement processing
- ✅ Brittleness mechanic (Ice → Brittle → Physical combo)
- ✅ Critical hit slow processing
- ✅ Combat end stress application

**NiflheimDataRepository Complete:**

- ✅ Room template queries with verticality filtering
- ✅ Enemy spawn queries with level/tier filtering
- ✅ JSON resistance/tag parsing
- ✅ Environmental hazard queries
- ✅ Resource drop queries

**Integration Points Complete:**

- ✅ CombatService: Frigid Cold initialization, attack processing
- ✅ MovementService: Slippery terrain checks
- ✅ ConditionService: [Frigid Cold], [Brittle], [Slowed] effects
- ✅ DiceService: FINESSE checks, damage rolls
- ✅ EnvironmentalObjectService: Hazard placement

**Code Statistics:**

- ~2,500+ lines of code across 6 files
- 3 SQL schema files (Data/)
- 2 C# service files (Engine/, Persistence/)

**Ready For:** Testing, BiomeGenerationService WFC integration, gameplay validation

---

## I. Overview

This specification defines the complete service implementation and testing framework for the Niflheim biome, including biome generation, environmental systems, combat integration, and comprehensive test suites.

### Core Deliverables

- **NiflheimService** complete implementation
- **BiomeGenerationService** extensions for Niflheim
- **FrigidColdService** integration
- **SlipperyTerrainService** integration
- **BrittlenessService** integration
- **Unit test suite** (10+ tests, ~85% coverage)
- **Integration test suite** (5+ scenarios)
- **End-to-end testing** framework

---

## II. Service Architecture

### A. NiflheimService.cs

**Primary service orchestrating all Niflheim-specific mechanics.**

```csharp
using Microsoft.Extensions.Logging;
using [RuneAndRust.Services](http://RuneAndRust.Services);
using RuneAndRust.Models;

namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    /// <summary>
    /// Service managing Niflheim biome mechanics:
    /// - Frigid Cold ambient condition
    /// - Slippery Terrain movement
    /// - Brittleness mechanic
    /// - Biome-specific combat interactions
    /// </summary>
    public class NiflheimService
    {
        private readonly ILogger<NiflheimService> _logger;
        private readonly FrigidColdService _frigidColdService;
        private readonly SlipperyTerrainService _slipperyTerrainService;
        private readonly BrittlenessService _brittlenessService;
        private readonly ConditionService _conditionService;
        
        public NiflheimService(
            ILogger<NiflheimService> logger,
            FrigidColdService frigidColdService,
            SlipperyTerrainService slipperyTerrainService,
            BrittlenessService brittlenessService,
            ConditionService conditionService)
        {
            _logger = logger;
            _frigidColdService = frigidColdService;
            _slipperyTerrainService = slipperyTerrainService;
            _brittlenessService = brittlenessService;
            _conditionService = conditionService;
        }
        
        /// <summary>
        /// Initialize Niflheim biome for combat.
        /// Applies Frigid Cold ambient condition to all combatants.
        /// </summary>
        public void InitializeCombat(CombatInstance combat)
        {
            if (combat.Biome?.BiomeId != 5) return;
            
            _logger.Information(
                "Initializing Niflheim combat {CombatId} at {Verticality}",
                combat.CombatId, combat.VerticalityTier);
            
            // Apply Frigid Cold
            _frigidColdService.ApplyFrigidCold(combat);
            
            _logger.Information(
                "Niflheim combat initialized: [Frigid Cold] applied to {CombatantCount} combatants",
                combat.AllCombatants.Count);
        }
        
        /// <summary>
        /// Process end-of-turn effects for Niflheim.
        /// Applies Psychic Stress accumulation.
        /// </summary>
        public void ProcessEndOfTurn(CombatInstance combat, int currentTurn)
        {
            if (combat.Biome?.BiomeId != 5) return;
            
            _logger.Debug(
                "Processing Niflheim end-of-turn effects for turn {Turn}",
                currentTurn);
            
            // No per-turn damage like Muspelheim
            // Frigid Cold stress applied at combat end
        }
        
        /// <summary>
        /// Process combat end for Niflheim.
        /// Applies environmental Psychic Stress.
        /// </summary>
        public void ProcessCombatEnd(CombatInstance combat)
        {
            if (combat.Biome?.BiomeId != 5) return;
            
            _logger.Information(
                "Processing Niflheim combat end for {CombatId}",
                combat.CombatId);
            
            foreach (var character in combat.PlayerParty)
            {
                _frigidColdService.ApplyEnvironmentalStress(character, combat);
            }
        }
        
        /// <summary>
        /// Process movement through Niflheim terrain.
        /// Handles slippery terrain checks.
        /// </summary>
        public bool ProcessMovement(
            Combatant combatant,
            RoomTile fromTile,
            RoomTile toTile)
        {
            if (toTile.Biome?.BiomeId != 5) return true;
            
            _logger.Debug(
                "{Combatant} attempting movement to Niflheim tile ({X}, {Y})",
                [combatant.Name](http://combatant.Name), toTile.X, toTile.Y);
            
            // Check slippery terrain
            if (toTile.HasFeature("Slippery Terrain"))
            {
                return _slipperyTerrainService.ProcessSlipperyMovement(
                    combatant, fromTile, toTile);
            }
            
            return true;
        }
        
        /// <summary>
        /// Process attack in Niflheim.
        /// Handles brittleness mechanic and critical hit slow.
        /// </summary>
        public void ProcessAttack(
            Combatant attacker,
            Combatant target,
            AttackResult result)
        {
            if (target.CurrentBiome?.BiomeId != 5) return;
            
            // Process brittleness (Ice damage on Ice-resistant targets)
            if (result.DamageType == "Ice")
            {
                _brittlenessService.ProcessBrittlenessCheck(target, result);
            }
            
            // Process critical hit slow (Frigid Cold)
            if (result.IsCriticalHit)
            {
                _frigidColdService.ProcessCriticalHitSlow(target, result);
            }
        }
    }
}
```

---

### B. BiomeGenerationService Extensions

**Niflheim sector generation using WFC algorithm.**

```csharp
namespace [RuneAndRust.Services](http://RuneAndRust.Services)
{
    public partial class BiomeGenerationService
    {
        /// <summary>
        /// Generate a Niflheim sector using Wave Function Collapse.
        /// Supports both [Roots] and [Canopy] verticality tiers.
        /// </summary>
        public Sector GenerateNiflheimSector(
            int targetRoomCount = 6,
            int partyLevel = 7,
            string verticalityTier = null)
        {
            _logger.Information(
                "Generating Niflheim sector: {RoomCount} rooms, party level {Level}, tier {Tier}",
                targetRoomCount, partyLevel, verticalityTier ?? "Random");
            
            // Select verticality tier if not specified
            if (verticalityTier == null)
            {
                verticalityTier = SelectNiflheimVerticalityTier();
            }
            
            // Get room templates for selected tier
            var templates = _templateRepository.GetByBiomeAndTier(5, verticalityTier);
            
            _logger.Debug(
                "Retrieved {TemplateCount} Niflheim room templates for {Tier}",
                templates.Count, verticalityTier);
            
            // Generate sector using WFC
            var sector = _wfcEngine.GenerateSector(
                templates,
                targetRoomCount,
                biomeId: 5,
                verticalityTier);
            
            // Populate with enemies and hazards
            PopulateNiflheimSector(sector, partyLevel, verticalityTier);
            
            _logger.Information(
                "Niflheim sector generated: {ActualRooms} rooms, {Enemies} enemies, {Hazards} hazards",
                sector.Rooms.Count,
                sector.Enemies.Count,
                sector.Hazards.Count);
            
            return sector;
        }
        
        /// <summary>
        /// Select verticality tier for Niflheim.
        /// 60% Roots, 40% Canopy.
        /// </summary>
        private string SelectNiflheimVerticalityTier()
        {
            var roll = _random.NextDouble();
            
            if (roll < 0.6)
            {
                _logger.Debug("Selected Niflheim verticality: Roots");
                return "Roots";
            }
            else
            {
                _logger.Debug("Selected Niflheim verticality: Canopy");
                return "Canopy";
            }
        }
        
        /// <summary>
        /// Populate Niflheim sector with enemies, hazards, and resources.
        /// </summary>
        private void PopulateNiflheimSector(
            Sector sector,
            int partyLevel,
            string verticalityTier)
        {
            _logger.Debug(
                "Populating Niflheim sector {SectorId}",
                sector.SectorId);
            
            foreach (var room in sector.Rooms)
            {
                // Generate tactical grid
                room.Grid = _gridService.GenerateGrid(room.Size);
                
                // Place slippery terrain (60-80% of tiles)
                PlaceSlipperyTerrain(room);
                
                // Place environmental hazards
                PlaceNiflheimHazards(room);
                
                // Spawn enemies
                SpawnNiflheimEnemies(room, partyLevel, verticalityTier);
                
                // Place resources
                PlaceNiflheimResources(room);
            }
        }
        
        /// <summary>
        /// Place slippery terrain on 60-80% of tiles.
        /// </summary>
        private void PlaceSlipperyTerrain(Room room)
        {
            var tileCount = room.Grid.Tiles.Count;
            var slipperyCount = (int)(tileCount * _[random.Next](http://random.Next)(60, 81) / 100.0);
            
            _logger.Debug(
                "Placing slippery terrain: {SlipperyCount}/{TotalCount} tiles",
                slipperyCount, tileCount);
            
            var availableTiles = room.Grid.Tiles
                .Where(t => !t.HasFeature("Cover") && !t.HasFeature("Obstacle"))
                .OrderBy(_ => _[random.Next](http://random.Next)())
                .Take(slipperyCount)
                .ToList();
            
            foreach (var tile in availableTiles)
            {
                tile.AddFeature("Slippery Terrain");
            }
        }
        
        /// <summary>
        /// Place Niflheim-specific hazards.
        /// </summary>
        private void PlaceNiflheimHazards(Room room)
        {
            var hazardTemplates = _hazardRepository.GetByBiome(5);
            var hazardCount = CalculateHazardCount(room.Size);
            
            _logger.Debug(
                "Placing {HazardCount} Niflheim hazards in room {RoomId}",
                hazardCount, room.RoomId);
            
            for (int i = 0; i < hazardCount; i++)
            {
                var hazard = SelectWeightedHazard(hazardTemplates);
                var tile = SelectHazardTile(room.Grid, hazard);
                
                if (tile != null)
                {
                    tile.AddHazard(hazard);
                    
                    _logger.Debug(
                        "Placed {HazardName} at ({X}, {Y})",
                        [hazard.Name](http://hazard.Name), tile.X, tile.Y);
                }
            }
        }
        
        /// <summary>
        /// Spawn Niflheim enemies based on party level and verticality.
        /// </summary>
        private void SpawnNiflheimEnemies(
            Room room,
            int partyLevel,
            string verticalityTier)
        {
            var spawnTable = _spawnRepository.GetByBiomeAndTier(5, verticalityTier);
            var enemyCount = CalculateEnemyCount(room.Size, partyLevel);
            
            _logger.Debug(
                "Spawning {EnemyCount} Niflheim enemies (level {Level}, tier {Tier})",
                enemyCount, partyLevel, verticalityTier);
            
            for (int i = 0; i < enemyCount; i++)
            {
                var enemyId = SelectWeightedEnemy(spawnTable, partyLevel);
                var enemy = _enemyRepository.GetById(enemyId);
                var tile = SelectSpawnTile(room.Grid);
                
                if (tile != null)
                {
                    tile.SpawnEnemy(enemy);
                    
                    _logger.Debug(
                        "Spawned {EnemyName} (Level {Level}) at ({X}, {Y})",
                        [enemy.Name](http://enemy.Name), enemy.Level, tile.X, tile.Y);
                }
            }
        }
        
        /// <summary>
        /// Place Niflheim resource drops.
        /// </summary>
        private void PlaceNiflheimResources(Room room)
        {
            var resources = _resourceRepository.GetByBiome(5);
            var resourceCount = CalculateResourceCount(room.Size);
            
            _logger.Debug(
                "Placing {ResourceCount} Niflheim resources",
                resourceCount);
            
            for (int i = 0; i < resourceCount; i++)
            {
                var resource = SelectWeightedResource(resources);
                var tile = SelectResourceTile(room.Grid);
                
                if (tile != null)
                {
                    tile.PlaceResource(resource);
                    
                    _logger.Debug(
                        "Placed {ResourceName} (Tier {Tier}) at ({X}, {Y})",
                        [resource.Name](http://resource.Name), resource.QualityTier, tile.X, tile.Y);
                }
            }
        }
    }
}
```

---

## III. Integration Points

### A. CombatService Integration

```csharp
public partial class CombatService
{
    private readonly NiflheimService _niflheimService;
    
    public void InitializeCombat(CombatInstance combat)
    {
        // Existing initialization...
        
        // Biome-specific initialization
        switch (combat.Biome?.BiomeId)
        {
            case 4: // Muspelheim
                _muspelheimService.InitializeCombat(combat);
                break;
            case 5: // Niflheim
                _niflheimService.InitializeCombat(combat);
                break;
        }
    }
    
    public AttackResult ProcessAttack(
        Combatant attacker,
        Combatant target,
        Ability ability)
    {
        var result = CalculateAttack(attacker, target, ability);
        
        // Apply damage...
        
        // Biome-specific attack processing
        switch (target.CurrentBiome?.BiomeId)
        {
            case 4: // Muspelheim
                _muspelheimService.ProcessAttack(attacker, target, result);
                break;
            case 5: // Niflheim
                _niflheimService.ProcessAttack(attacker, target, result);
                break;
        }
        
        return result;
    }
}
```

### B. MovementService Integration

```csharp
public partial class MovementService
{
    private readonly NiflheimService _niflheimService;
    
    public bool TryMove(Combatant combatant, RoomTile targetTile)
    {
        // Existing passability checks...
        
        // Biome-specific movement processing
        if (targetTile.Biome?.BiomeId == 5) // Niflheim
        {
            bool success = _niflheimService.ProcessMovement(
                combatant, combatant.CurrentTile, targetTile);
            
            if (!success)
            {
                _logger.Information(
                    "{Combatant} movement failed: Slippery terrain knockdown",
                    [combatant.Name](http://combatant.Name));
                return false;
            }
        }
        
        // Complete movement...
        return true;
    }
}
```

---

## IV. Unit Test Suite

### NiflheimServiceTests.cs

```csharp
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using [RuneAndRust.Services](http://RuneAndRust.Services).Biomes;
using RuneAndRust.Models;

namespace [RuneAndRust.Tests.Services](http://RuneAndRust.Tests.Services).Biomes
{
    public class NiflheimServiceTests
    {
        private readonly Mock<ILogger<NiflheimService>> _loggerMock;
        private readonly Mock<FrigidColdService> _frigidColdMock;
        private readonly Mock<SlipperyTerrainService> _slipperyTerrainMock;
        private readonly Mock<BrittlenessService> _brittlenessMock;
        private readonly NiflheimService _service;
        
        public NiflheimServiceTests()
        {
            _loggerMock = new Mock<ILogger<NiflheimService>>();
            _frigidColdMock = new Mock<FrigidColdService>();
            _slipperyTerrainMock = new Mock<SlipperyTerrainService>();
            _brittlenessMock = new Mock<BrittlenessService>();
            
            _service = new NiflheimService(
                _loggerMock.Object,
                _frigidColdMock.Object,
                _slipperyTerrainMock.Object,
                _brittlenessMock.Object,
                Mock.Of<ConditionService>());
        }
        
        [Fact]
        public void InitializeCombat_NiflheimBiome_AppliesFrigidCold()
        {
            // Arrange
            var combat = CreateNiflheimCombat();
            
            // Act
            _service.InitializeCombat(combat);
            
            // Assert
            _frigidColdMock.Verify(
                x => x.ApplyFrigidCold(combat),
                Times.Once);
        }
        
        [Fact]
        public void InitializeCombat_NonNiflheimBiome_DoesNothing()
        {
            // Arrange
            var combat = CreateMuspelheimCombat();
            
            // Act
            _service.InitializeCombat(combat);
            
            // Assert
            _frigidColdMock.Verify(
                x => x.ApplyFrigidCold(It.IsAny<CombatInstance>()),
                Times.Never);
        }
        
        [Fact]
        public void ProcessMovement_SlipperyTerrain_CallsSlipperyTerrainService()
        {
            // Arrange
            var combatant = CreateTestCombatant();
            var fromTile = CreateNormalTile();
            var toTile = CreateSlipperyTile();
            
            _slipperyTerrainMock
                .Setup(x => x.ProcessSlipperyMovement(combatant, fromTile, toTile))
                .Returns(true);
            
            // Act
            bool result = _service.ProcessMovement(combatant, fromTile, toTile);
            
            // Assert
            Assert.True(result);
            _slipperyTerrainMock.Verify(
                x => x.ProcessSlipperyMovement(combatant, fromTile, toTile),
                Times.Once);
        }
        
        [Fact]
        public void ProcessAttack_IceDamage_CallsBrittlenessService()
        {
            // Arrange
            var attacker = CreateTestCombatant();
            var target = CreateIceResistantEnemy();
            var result = new AttackResult { DamageType = "Ice", DamageDone = 20 };
            
            // Act
            _service.ProcessAttack(attacker, target, result);
            
            // Assert
            _brittlenessMock.Verify(
                x => x.ProcessBrittlenessCheck(target, result),
                Times.Once);
        }
        
        [Fact]
        public void ProcessAttack_CriticalHit_CallsFrigidColdService()
        {
            // Arrange
            var attacker = CreateTestCombatant();
            var target = CreateTestCombatant();
            var result = new AttackResult { IsCriticalHit = true, DamageDone = 30 };
            
            // Act
            _service.ProcessAttack(attacker, target, result);
            
            // Assert
            _frigidColdMock.Verify(
                x => x.ProcessCriticalHitSlow(target, result),
                Times.Once);
        }
        
        [Fact]
        public void ProcessCombatEnd_NiflheimBiome_AppliesStressToAllPartyMembers()
        {
            // Arrange
            var combat = CreateNiflheimCombat();
            var partySize = combat.PlayerParty.Count;
            
            // Act
            _service.ProcessCombatEnd(combat);
            
            // Assert
            _frigidColdMock.Verify(
                x => x.ApplyEnvironmentalStress(
                    It.IsAny<Character>(),
                    combat),
                Times.Exactly(partySize));
        }
        
        // Helper methods
        private CombatInstance CreateNiflheimCombat()
        {
            return new CombatInstance
            {
                CombatId = 1,
                Biome = new Biome { BiomeId = 5, BiomeName = "Niflheim" },
                PlayerParty = new List<Character> { new Character(), new Character() },
                Enemies = new List<Enemy>(),
                AllCombatants = new List<Combatant>()
            };
        }
        
        private CombatInstance CreateMuspelheimCombat()
        {
            return new CombatInstance
            {
                Biome = new Biome { BiomeId = 4, BiomeName = "Muspelheim" }
            };
        }
        
        private Combatant CreateTestCombatant()
        {
            return new Combatant
            {
                Name = "Test Character",
                CurrentBiome = new Biome { BiomeId = 5 }
            };
        }
        
        private Combatant CreateIceResistantEnemy()
        {
            var enemy = new Combatant
            {
                Name = "Frost-Rimed Undying",
                CurrentBiome = new Biome { BiomeId = 5 }
            };
            enemy.Resistances["Ice"] = 75;
            return enemy;
        }
        
        private RoomTile CreateNormalTile()
        {
            return new RoomTile { X = 0, Y = 0 };
        }
        
        private RoomTile CreateSlipperyTile()
        {
            var tile = new RoomTile
            {
                X = 1,
                Y = 1,
                Biome = new Biome { BiomeId = 5 }
            };
            tile.AddFeature("Slippery Terrain");
            return tile;
        }
    }
}
```

---

## V. Integration Test Suite

### NiflheimIntegrationTests.cs

```csharp
using Xunit;
using [RuneAndRust.Services](http://RuneAndRust.Services);
using RuneAndRust.Models;

namespace RuneAndRust.Tests.Integration
{
    public class NiflheimIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public void GenerateNiflheimSector_RootsTier_GeneratesCorrectly()
        {
            // Arrange
            var biomeService = GetService<BiomeGenerationService>();
            
            // Act
            var sector = biomeService.GenerateNiflheimSector(
                targetRoomCount: 6,
                partyLevel: 8,
                verticalityTier: "Roots");
            
            // Assert
            Assert.NotNull(sector);
            Assert.Equal(5, sector.Biome.BiomeId); // Niflheim
            Assert.Equal("Roots", sector.VerticalityTier);
            Assert.InRange(sector.Rooms.Count, 4, 8); // Target ±2
            Assert.True(sector.Enemies.Any());
            Assert.True(sector.Hazards.Any());
            
            // Verify slippery terrain coverage
            var totalTiles = sector.Rooms.Sum(r => r.Grid.Tiles.Count);
            var slipperyTiles = sector.Rooms
                .SelectMany(r => r.Grid.Tiles)
                .Count(t => t.HasFeature("Slippery Terrain"));
            var coverage = (double)slipperyTiles / totalTiles;
            
            Assert.InRange(coverage, 0.55, 0.85); // 60-80% ±5% variance
        }
        
        [Fact]
        public void GenerateNiflheimSector_CanopyTier_GeneratesCorrectly()
        {
            // Arrange
            var biomeService = GetService<BiomeGenerationService>();
            
            // Act
            var sector = biomeService.GenerateNiflheimSector(
                targetRoomCount: 6,
                partyLevel: 8,
                verticalityTier: "Canopy");
            
            // Assert
            Assert.Equal("Canopy", sector.VerticalityTier);
            Assert.True(sector.Rooms.All(r => r.Template.VerticalityTier == "Canopy"));
        }
        
        [Fact]
        public void NiflheimCombat_FullScenario_AllSystemsWork()
        {
            // Arrange
            var combatService = GetService<CombatService>();
            var combat = CreateTestNiflheimCombat();
            var player = combat.PlayerParty[0];
            var enemy = SpawnEnemy(501, combat); // Frost-Rimed Undying
            
            // Act & Assert: Initialize combat
            combatService.InitializeCombat(combat);
            Assert.Equal(50, player.DamageVulnerabilities["Ice"]);
            
            // Act & Assert: Ice attack applies Brittle
            var iceAbility = player.Abilities.First(a => a.DamageType == "Ice");
            var iceAttack = combatService.ProcessAttack(player, enemy, iceAbility);
            Assert.True(enemy.HasCondition("Brittle"));
            
            // Act & Assert: Physical attack exploits Brittle
            var physAbility = player.Abilities.First(a => a.DamageType == "Physical");
            var physAttack = combatService.ProcessAttack(player, enemy, physAbility);
            Assert.True(physAttack.DamageDone > physAbility.BaseDamage);
            
            // Act & Assert: Critical hit applies Slowed
            var critAttack = combatService.ProcessAttack(enemy, player, enemy.Abilities[0]);
            critAttack.IsCriticalHit = true;
            combatService.ProcessCriticalHitEffects(critAttack);
            Assert.True(player.HasCondition("Slowed"));
            
            // Act & Assert: Combat end applies stress
            var initialStress = player.PsychicStress;
            combatService.EndCombat(combat);
            Assert.True(player.PsychicStress > initialStress);
        }
        
        [Fact]
        public void SlipperyTerrain_FullScenario_KnockdownAndDamage()
        {
            // Arrange
            var movementService = GetService<MovementService>();
            var combatant = CreateTestCombatant(finesse: 8); // Low FINESSE
            var slipperyTile = CreateSlipperyTile();
            
            SetNextDiceRoll(1); // Ensure failure
            
            // Act
            bool success = movementService.TryMove(combatant, slipperyTile);
            
            // Assert
            Assert.False(success);
            Assert.True(combatant.HasCondition("Knocked Down"));
            Assert.True(combatant.CurrentHP < combatant.MaxHP);
        }
        
        [Fact]
        public void IceWalkerPassive_IgnoresSlipperyTerrain()
        {
            // Arrange
            var movementService = GetService<MovementService>();
            var beast = SpawnEnemy(503); // Ice-Adapted Beast with Ice-Walker
            var slipperyTile = CreateSlipperyTile();
            
            // Act
            bool success = movementService.TryMove(beast, slipperyTile);
            
            // Assert
            Assert.True(success);
            Assert.False(beast.HasCondition("Knocked Down"));
        }
    }
}
```

---

## VI. Success Criteria

### Functional Requirements

- [ ]  NiflheimService orchestrates all biome mechanics
- [ ]  BiomeGenerationService generates Niflheim sectors
- [ ]  Dual verticality (Roots/Canopy) works correctly
- [ ]  Slippery terrain covers 60-80% of tiles
- [ ]  FrigidColdService applies correctly
- [ ]  SlipperyTerrainService processes movement
- [ ]  BrittlenessService applies on Ice damage
- [ ]  All enemy types spawn with correct weights
- [ ]  Resources spawn with correct distribution

### Quality Gates

- [ ]  85%+ unit test coverage achieved
- [ ]  All services use Serilog structured logging
- [ ]  Integration tests pass
- [ ]  Performance acceptable (< 500ms sector generation)
- [ ]  Memory usage reasonable (< 50MB per sector)

### Balance Validation

- [ ]  Frigid Cold feels consistently dangerous
- [ ]  Slippery terrain creates tactical challenge
- [ ]  Brittleness combo is rewarding
- [ ]  Enemy distribution feels balanced
- [ ]  Resource drops feel appropriate
- [ ]  Sector generation variety is good

---

## VII. Deployment Instructions

### Step 1: Compile All Services

```bash
dotnet build Services/Biomes/NiflheimService.cs
dotnet build Services/BiomeGenerationService.Niflheim.cs
dotnet build Services/FrigidColdService.cs
dotnet build Services/SlipperyTerrainService.cs
dotnet build Services/BrittlenessService.cs
```

### Step 2: Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~NiflheimServiceTests"
dotnet test --filter "FullyQualifiedName~FrigidColdServiceTests"
dotnet test --filter "FullyQualifiedName~SlipperyTerrainServiceTests"
dotnet test --filter "FullyQualifiedName~BrittlenessServiceTests"
```

### Step 3: Run Integration Tests

```bash
dotnet test --filter "FullyQualifiedName~NiflheimIntegrationTests"
```

### Step 4: Generate Test Coverage Report

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```

### Step 5: Manual End-to-End Testing

**Test Scenario 1: Sector Generation**

```bash
dotnet run -- generate-sector --biome Niflheim --tier Roots
```

Verify:

- 4-8 rooms generated
- Slippery terrain on most tiles
- Enemies present
- Hazards spawned

**Test Scenario 2: Combat Encounter**

```bash
dotnet run -- test-combat --biome Niflheim --enemy "Frost-Rimed Undying"
```

Verify:

- [Frigid Cold] applies at combat start
- Ice attacks apply [Brittle]
- Physical attacks exploit [Brittle]
- Critical hits apply [Slowed]
- Stress accumulates at combat end

**Test Scenario 3: Movement**

```bash
dotnet run -- test-movement --biome Niflheim
```

Verify:

- FINESSE checks trigger on slippery terrain
- Failures cause knockdown + damage
- Knockdown immunity bypasses checks
- Forced movement amplified on ice

---

## VIII. Performance Benchmarks

### Expected Performance Targets

**Sector Generation:**

- Small sector (4 rooms): < 200ms
- Medium sector (6 rooms): < 350ms
- Large sector (8 rooms): < 500ms

**Combat Initialization:**

- Apply Frigid Cold: < 10ms
- Spawn 4-6 enemies: < 50ms

**Per-Turn Operations:**

- Slippery terrain check: < 5ms
- Brittleness check: < 3ms
- Critical slow check: < 3ms

**Memory Usage:**

- Sector with 6 rooms: < 50MB
- Combat with 6 combatants: < 20MB

---

## IX. Next Steps

Once v0.30.4 is complete:

**v0.30 Niflheim is COMPLETE (35-50 hours total)**

- ✅ Database schema and room templates
- ✅ Environmental hazards and conditions
- ✅ Enemy definitions and spawn system
- ✅ Service implementation and testing

**Next Version:**

- v0.31: Alfheim Biome (Energy/Aetheric theme)
- v0.32: Jotunheim Biome (Giant/Industrial theme)

---

## X. Related Documents

**Parent Specification:**

- v0.30: Niflheim Biome Implementation[[1]](v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)

**Child Specifications:**

- v0.30.1: Database Schema[[2]](v0%2030%201%20Database%20Schema%20&%20Room%20Templates%208f251d9f2b39447299157b78b963d1ed.md)
- v0.30.2: Environmental Hazards[[3]](v0%2030%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%2025f6a1f3b0a843ce9a72b86503b1de60.md)
- v0.30.3: Enemy Definitions[[4]](v0%2030%203%20Enemy%20Definitions%20&%20Spawn%20System%2098ff5d3b26b44f7db6b8bdf0843c77fe.md)

**Reference Implementation:**

- v0.29.4: Muspelheim Services[[5]](v0%2029%204%20Service%20Implementation%20&%20Testing%20ce4a69fbcddb40e2ac66db616c52bc6f.md)

**Prerequisites:**

- Master Roadmap[[6]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)

---

**v0.30 Niflheim specification suite COMPLETE. Ready for implementation.**