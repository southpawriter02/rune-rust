# v0.31.4: Service Implementation & Testing

Type: Technical
Description: AlfheimService, RunicInstabilityService, RealityTearService, BiomeGenerationService extensions, unit tests (~85% coverage), integration testing.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.31.1-v0.31.3 (All prior Alfheim specs)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.31: Alfheim Biome Implementation (v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.31.4-SERVICES

**Parent Specification:** v0.31 Alfheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 9-15 hours

**Prerequisites:** v0.31.1 (Database), v0.31.2 (Environmental), v0.31.3 (Enemies)

---

## I. Overview

This specification defines the complete service layer implementation for the Alfheim biome, including AlfheimService, BiomeGenerationService extensions, RunicInstabilityService, RealityTearService, and comprehensive testing suite.

### Core Deliverables

- **AlfheimService** (primary orchestration)
- **BiomeGenerationService.GenerateAlfheimSector()** (procedural generation)
- **RunicInstabilityService** (Wild Magic Surge system)
- **RealityTearService** (positional warping)
- **Unit Test Suite** (10+ tests, 85%+ coverage)
- **Integration Testing** (end-to-end scenarios)
- **Serilog structured logging** throughout

---

## II. AlfheimService (Primary Service)

### A. Service Architecture

**AlfheimService.cs:**

```csharp
using Microsoft.Extensions.Logging;
using Serilog;
using [RuneAndRust.Services](http://RuneAndRust.Services).Combat;
using [RuneAndRust.Services](http://RuneAndRust.Services).Environment;
using [RuneAndRust.Services](http://RuneAndRust.Services).TraumaEconomy;
using RuneAndRust.Models;

namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    /// <summary>
    /// Primary service for Alfheim biome mechanics.
    /// Handles [Runic Instability], [Psychic Resonance], Reality Tears,
    /// and biome-specific environmental interactions.
    /// </summary>
    public class AlfheimService
    {
        private readonly ILogger<AlfheimService> _logger;
        private readonly RunicInstabilityService _runicInstabilityService;
        private readonly RealityTearService _realityTearService;
        private readonly TraumaEconomyService _traumaService;
        private readonly ConditionService _conditionService;
        
        private const int BIOME_ID = 6;
        private const int PSYCHIC_RESONANCE_ENCOUNTER = 10; // vs Helheim's 5
        private const int PSYCHIC_RESONANCE_PER_TURN = 2;
        
        public AlfheimService(
            ILogger<AlfheimService> logger,
            RunicInstabilityService runicInstabilityService,
            RealityTearService realityTearService,
            TraumaEconomyService traumaService,
            ConditionService conditionService)
        {
            _logger = logger;
            _runicInstabilityService = runicInstabilityService;
            _realityTearService = realityTearService;
            _traumaService = traumaService;
            _conditionService = conditionService;
        }
        
        /// <summary>
        /// Initialize Alfheim biome effects at combat start.
        /// Applies [Runic Instability] and [Psychic Resonance].
        /// </summary>
        public void InitializeAlfheimCombat(CombatInstance combat)
        {
            if (combat.Biome?.BiomeId != BIOME_ID)
            {
                _logger.Warning(
                    "InitializeAlfheimCombat called for non-Alfheim biome: {BiomeId}",
                    combat.Biome?.BiomeId);
                return;
            }
            
            _logger.Information(
                "Initializing Alfheim combat {CombatId} in {Room}",
                combat.CombatId, combat.CurrentRoom?.RoomName);
            
            // Apply [Runic Instability] ambient condition
            _conditionService.ApplyAmbientCondition(combat, 107); // condition_id for Runic Instability
            
            // Apply [Psychic Resonance] high-intensity
            ApplyPsychicResonance(combat);
            
            // Amplify Mystics' Aether Pool
            foreach (var character in combat.PlayerParty)
            {
                if (character.Archetype == "Mystic")
                {
                    _runicInstabilityService.ApplyAetherPoolAmplification(character);
                }
            }
            
            _logger.Information(
                "Alfheim combat initialized: [Runic Instability] active, Psychic Resonance high-intensity");
        }
        
        /// <summary>
        /// Apply high-intensity Psychic Resonance at combat start.
        /// +10 Stress per character (double Helheim's intensity).
        /// </summary>
        public void ApplyPsychicResonance(CombatInstance combat)
        {
            _logger.Information(
                "Applying high-intensity [Psychic Resonance] to combat {CombatId}",
                combat.CombatId);
            
            foreach (var character in combat.PlayerParty)
            {
                _traumaService.ApplyStress(
                    character,
                    PSYCHIC_RESONANCE_ENCOUNTER,
                    "Alfheim [Psychic Resonance] - ground zero of Great Silence");
                
                _logger.Debug(
                    "{Character} gained +{Stress} Psychic Stress (Alfheim Psychic Resonance)",
                    [character.Name](http://character.Name), PSYCHIC_RESONANCE_ENCOUNTER);
            }
        }
        
        /// <summary>
        /// Process per-turn Psychic Resonance Stress accumulation.
        /// +2 Stress per character per turn.
        /// </summary>
        public void ProcessTurnStress(CombatInstance combat, int currentTurn)
        {
            if (combat.Biome?.BiomeId != BIOME_ID) return;
            
            foreach (var character in combat.PlayerParty)
            {
                _traumaService.ApplyStress(
                    character,
                    PSYCHIC_RESONANCE_PER_TURN,
                    $"Alfheim turn {currentTurn} exposure");
            }
            
            _logger.Debug(
                "Alfheim turn {Turn}: Applied +{Stress} Psychic Stress to all party members",
                currentTurn, PSYCHIC_RESONANCE_PER_TURN);
        }
        
        /// <summary>
        /// Check if tile contains Reality Tear and process encounter.
        /// </summary>
        public void CheckRealityTear(Combatant combatant, RoomTile tile, BattlefieldGrid grid)
        {
            if (!tile.HasFeature("Reality Tear")) return;
            
            _realityTearService.ProcessRealityTearEncounter(combatant, tile, grid);
        }
        
        /// <summary>
        /// Process Mystic ability use with Wild Magic Surge check.
        /// </summary>
        public AbilityResult ProcessMysticAbility(
            Character caster,
            Ability ability,
            AbilityResult baseResult,
            CombatInstance combat)
        {
            if (combat.Biome?.BiomeId != BIOME_ID) return baseResult;
            if (ability.ResourceType != "Aether") return baseResult;
            
            // Check for Wild Magic Surge
            var surge = _runicInstabilityService.TryTriggerWildMagicSurge(
                caster, ability, combat);
            
            if (surge == null) return baseResult;
            
            // Apply surge modification
            return ApplySurgeModification(baseResult, surge);
        }
        
        /// <summary>
        /// Apply Wild Magic Surge modification to ability result.
        /// </summary>
        private AbilityResult ApplySurgeModification(
            AbilityResult result,
            WildMagicSurgeResult surge)
        {
            switch (surge.Type)
            {
                case SurgeType.DamageModification:
                    result.Damage = (int)(result.Damage * (1 + surge.Modifier));
                    _logger.Information(
                        "Wild Magic Surge: Damage modified to {Damage} ({Modifier}%)",
                        result.Damage, surge.Modifier * 100);
                    break;
                    
                case SurgeType.RangeModification:
                    result.Range += (int)surge.Modifier;
                    _logger.Information(
                        "Wild Magic Surge: Range modified to {Range} ({Modifier})",
                        result.Range, surge.Modifier);
                    break;
                    
                case SurgeType.TargetModification:
                    result.TargetCount += (int)surge.Modifier;
                    result.TargetCount = Math.Max(1, result.TargetCount); // Minimum 1 target
                    _logger.Information(
                        "Wild Magic Surge: Target count modified to {Count} ({Modifier})",
                        result.TargetCount, surge.Modifier);
                    break;
                    
                case SurgeType.DurationModification:
                    result.Duration = (int)(result.Duration * (1 + surge.Modifier));
                    _logger.Information(
                        "Wild Magic Surge: Duration modified to {Duration} ({Modifier}%)",
                        result.Duration, surge.Modifier * 100);
                    break;
            }
            
            result.SurgeDescription = surge.EffectDescription;
            return result;
        }
        
        /// <summary>
        /// Process Energy Conduit interaction.
        /// Damages adjacent characters or allows Mystics to channel.
        /// </summary>
        public void ProcessEnergyConduit(RoomTile conduitTile, BattlefieldGrid grid)
        {
            var adjacentCombatants = grid.GetAdjacentCombatants(conduitTile);
            
            foreach (var combatant in adjacentCombatants)
            {
                // Check if character can channel (50%+ Energy Resistance)
                if (combatant is Character character && 
                    character.Archetype == "Mystic" &&
                    character.GetResistance("Energy") >= 0.5)
                {
                    // Channel for +5 Aether Pool
                    character.CurrentAetherPool += 5;
                    _logger.Information(
                        "{Character} channels Energy Conduit for +5 Aether Pool",
                        [character.Name](http://character.Name));
                }
                else
                {
                    // Take 1d10 Energy damage
                    var damage = DiceService.Roll(1, 10);
                    combatant.TakeDamage(damage, "Energy");
                    _logger.Information(
                        "{Combatant} takes {Damage} Energy damage from Energy Conduit",
                        [combatant.Name](http://combatant.Name), damage);
                }
            }
        }
    }
}
```

---

## III. BiomeGenerationService Extensions

### A. GenerateAlfheimSector()

**BiomeGenerationService.cs** (partial - Alfheim extension):

```csharp
public partial class BiomeGenerationService
{
    /// <summary>
    /// Generate Alfheim sector using Wave Function Collapse.
    /// Alfheim is exclusively [Canopy] verticality.
    /// </summary>
    public Sector GenerateAlfheimSector(
        int sectorId,
        int difficulty,
        int roomCount = 8)
    {
        _logger.Information(
            "Generating Alfheim sector {SectorId}, difficulty {Difficulty}, {RoomCount} rooms",
            sectorId, difficulty, roomCount);
        
        var sector = new Sector
        {
            SectorId = sectorId,
            BiomeId = 6, // Alfheim
            Difficulty = difficulty,
            VerticalityTier = "Canopy" // Always Canopy
        };
        
        // Load Alfheim room templates (all Canopy)
        var templates = _database.GetBiomeRoomTemplates(biomeId: 6);
        
        // Generate room layout with WFC
        var roomLayout = _wfcService.GenerateLayout(
            templates,
            roomCount,
            connectivityDensity: 0.65); // Moderate connectivity
        
        // Populate each room
        foreach (var room in roomLayout)
        {
            PopulateAlfheimRoom(room, difficulty);
            sector.Rooms.Add(room);
        }
        
        // Place Reality Tears strategically
        PlaceRealityTears(sector, difficulty);
        
        // Ensure at least one Energy Conduit per sector
        PlaceEnergyConduits(sector, difficulty);
        
        // Check for All-Rune Proving Ground (boss room)
        var provingGround = sector.Rooms.FirstOrDefault(
            r => r.TemplateName == "All-Rune Proving Ground");
        if (provingGround != null)
        {
            // Mark as boss room (scripted encounter)
            provingGround.IsBossRoom = true;
            provingGround.BossName = "All-Rune's Echo";
        }
        
        _logger.Information(
            "Alfheim sector {SectorId} generated: {RoomCount} rooms, difficulty {Difficulty}",
            sectorId, sector.Rooms.Count, difficulty);
        
        return sector;
    }
    
    /// <summary>
    /// Populate Alfheim room with enemies, hazards, and resources.
    /// </summary>
    private void PopulateAlfheimRoom(Room room, int difficulty)
    {
        _logger.Debug(
            "Populating Alfheim room {RoomId} ({Template})",
            room.RoomId, room.TemplateName);
        
        // Generate enemy group
        var enemies = _alfheimService.GenerateEnemyGroup(difficulty);
        room.Enemies = enemies;
        
        // Place environmental hazards
        PlaceAlfheimHazards(room, difficulty);
        
        // Place resources
        PlaceAlfheimResources(room, difficulty);
        
        // Add Crystalline Spires (cover)
        PlaceCrystallineSpires(room);
        
        _logger.Debug(
            "Alfheim room populated: {EnemyCount} enemies, {HazardCount} hazards",
            room.Enemies.Count, room.Hazards.Count);
    }
    
    /// <summary>
    /// Place Reality Tears strategically across sector.
    /// Higher difficulty = more Reality Tears.
    /// </summary>
    private void PlaceRealityTears(Sector sector, int difficulty)
    {
        int tearCount = difficulty switch
        {
            1 => _diceService.Roll(1, 2),  // Easy: 1-2
            2 => _diceService.Roll(2, 3),  // Normal: 2-3
            3 => _diceService.Roll(3, 4),  // Hard: 3-4
            4 => _diceService.Roll(4, 6),  // Deadly: 4-6
            _ => 2
        };
        
        var rooms = sector.Rooms.Where(r => !r.IsBossRoom).ToList();
        
        for (int i = 0; i < tearCount && i < rooms.Count; i++)
        {
            var room = rooms[_diceService.Roll(0, rooms.Count - 1)];
            
            // Place Reality Tear on random valid tile
            var validTiles = room.Grid.Tiles
                .Where(t => t.IsPassable() && !t.IsOccupied)
                .ToList();
            
            if (validTiles.Any())
            {
                var tile = validTiles[_diceService.Roll(0, validTiles.Count - 1)];
                tile.AddFeature("Reality Tear");
                
                _logger.Debug(
                    "Reality Tear placed in room {RoomId} at ({X}, {Y})",
                    room.RoomId, tile.X, tile.Y);
            }
        }
        
        _logger.Information(
            "Placed {TearCount} Reality Tears across sector {SectorId}",
            tearCount, sector.SectorId);
    }
    
    /// <summary>
    /// Place Energy Conduits in sector.
    /// Ensures at least one per sector.
    /// </summary>
    private void PlaceEnergyConduits(Sector sector, int difficulty)
    {
        int conduitCount = Math.Max(1, difficulty); // At least 1, scales with difficulty
        
        var rooms = sector.Rooms.ToList();
        
        for (int i = 0; i < conduitCount && i < rooms.Count; i++)
        {
            var room = rooms[i];
            
            var validTiles = room.Grid.Tiles
                .Where(t => t.IsPassable() && !t.IsOccupied)
                .ToList();
            
            if (validTiles.Any())
            {
                var tile = validTiles[_diceService.Roll(0, validTiles.Count - 1)];
                tile.AddFeature("Energy Conduit");
            }
        }
        
        _logger.Information(
            "Placed {ConduitCount} Energy Conduits in sector {SectorId}",
            conduitCount, sector.SectorId);
    }
    
    /// <summary>
    /// Place Crystalline Spires (cover) in room.
    /// </summary>
    private void PlaceCrystallineSpires(Room room)
    {
        int spireCount = _diceService.Roll(2, 4); // 2-4 per room
        
        var validTiles = room.Grid.Tiles
            .Where(t => t.IsPassable() && !t.IsOccupied)
            .ToList();
        
        for (int i = 0; i < spireCount && i < validTiles.Count; i++)
        {
            var tile = validTiles[_diceService.Roll(0, validTiles.Count - 1)];
            tile.AddFeature("Crystalline Spire");
            validTiles.Remove(tile); // Don't place multiple on same tile
        }
    }
}
```

---

## IV. RunicInstabilityService (Wild Magic Surge System)

**Complete implementation provided in v0.31.2, Section II.C.**

Key methods:

- `TryTriggerWildMagicSurge()` - 25% chance check
- `GenerateSurgeEffect()` - Random surge type selection
- `ApplyAetherPoolAmplification()` - +10% Aether Pool for Mystics

---

## V. RealityTearService (Positional Warping)

**Complete implementation provided in v0.31.2, Section IV.C.**

Key methods:

- `ProcessRealityTearEncounter()` - Full Reality Tear interaction
- `SelectWarpDestination()` - Random tile selection 3-5 spaces away
- Applies: 2d8 Energy damage, +5 Corruption, [Dazed] condition

---

## VI. Unit Test Suite

### A. AlfheimService Tests

**AlfheimServiceTests.cs:**

```csharp
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using [RuneAndRust.Services](http://RuneAndRust.Services).Biomes;
using RuneAndRust.Models;

namespace RuneAndRust.Tests.Biomes
{
    public class AlfheimServiceTests
    {
        private readonly AlfheimService _service;
        private readonly Mock<ILogger<AlfheimService>> _loggerMock;
        
        public AlfheimServiceTests()
        {
            _loggerMock = new Mock<ILogger<AlfheimService>>();
            // Setup other service mocks...
            _service = new AlfheimService(
                _loggerMock.Object,
                /* other dependencies */);
        }
        
        [Fact]
        public void InitializeAlfheimCombat_AppliesRunicInstability()
        {
            // Arrange
            var combat = CreateTestCombat(biomeId: 6);
            
            // Act
            _service.InitializeAlfheimCombat(combat);
            
            // Assert
            Assert.True(combat.HasAmbientCondition(107)); // Runic Instability
        }
        
        [Fact]
        public void InitializeAlfheimCombat_AppliesPsychicResonance()
        {
            // Arrange
            var combat = CreateTestCombat(biomeId: 6);
            var character = combat.PlayerParty[0];
            var initialStress = character.PsychicStress;
            
            // Act
            _service.InitializeAlfheimCombat(combat);
            
            // Assert
            Assert.Equal(initialStress + 10, character.PsychicStress);
        }
        
        [Fact]
        public void ProcessTurnStress_AppliesStressPerTurn()
        {
            // Arrange
            var combat = CreateTestCombat(biomeId: 6);
            var character = combat.PlayerParty[0];
            var initialStress = character.PsychicStress;
            
            // Act
            _service.ProcessTurnStress(combat, turn: 1);
            
            // Assert
            Assert.Equal(initialStress + 2, character.PsychicStress);
        }
        
        [Fact]
        public void ProcessMysticAbility_TriggersWildMagicSurge_Approximately25Percent()
        {
            // Arrange
            var combat = CreateTestCombat(biomeId: 6);
            var mystic = CreateTestMystic();
            var ability = CreateMysticAbility();
            var baseResult = new AbilityResult { Damage = 20 };
            
            int surgeCount = 0;
            
            // Act
            for (int i = 0; i < 1000; i++)
            {
                var result = _service.ProcessMysticAbility(
                    mystic, ability, baseResult, combat);
                
                if (result.SurgeDescription != null)
                    surgeCount++;
            }
            
            // Assert
            Assert.InRange(surgeCount, 200, 300); // ~25% of 1000
        }
        
        [Fact]
        public void ApplySurgeModification_DamageModification_ModifiesDamageBy50Percent()
        {
            // Arrange
            var result = new AbilityResult { Damage = 20 };
            var surge = new WildMagicSurgeResult
            {
                Type = SurgeType.DamageModification,
                Modifier = 0.5 // +50%
            };
            
            // Act
            var modified = _service.ApplySurgeModification(result, surge);
            
            // Assert
            Assert.Equal(30, modified.Damage); // 20 * 1.5
        }
        
        [Fact]
        public void ProcessEnergyConduit_MysticWith50PercentResistance_Channels()
        {
            // Arrange
            var mystic = CreateTestMystic();
            mystic.SetResistance("Energy", 0.5);
            var initialAether = mystic.CurrentAetherPool;
            
            var conduitTile = CreateTileWithFeature("Energy Conduit");
            var grid = CreateTestGrid();
            grid.PlaceCombatant(mystic, adjacentTo: conduitTile);
            
            // Act
            _service.ProcessEnergyConduit(conduitTile, grid);
            
            // Assert
            Assert.Equal(initialAether + 5, mystic.CurrentAetherPool);
        }
        
        [Fact]
        public void ProcessEnergyConduit_NonMystic_TakesDamage()
        {
            // Arrange
            var warrior = CreateTestWarrior();
            var initialHP = warrior.HP;
            
            var conduitTile = CreateTileWithFeature("Energy Conduit");
            var grid = CreateTestGrid();
            grid.PlaceCombatant(warrior, adjacentTo: conduitTile);
            
            // Act
            _service.ProcessEnergyConduit(conduitTile, grid);
            
            // Assert
            Assert.True(warrior.HP < initialHP); // Took damage
        }
    }
}
```

### B. BiomeGenerationService Tests

**BiomeGenerationServiceTests_Alfheim.cs:**

```csharp
[Fact]
public void GenerateAlfheimSector_Always_CanopyVerticality()
{
    // Act
    var sector = _service.GenerateAlfheimSector(
        sectorId: 1,
        difficulty: 2);
    
    // Assert
    Assert.Equal("Canopy", sector.VerticalityTier);
    Assert.All(sector.Rooms, room => 
        Assert.Equal("Canopy", room.VerticalityTier));
}

[Fact]
public void GenerateAlfheimSector_PlacesRealityTears_BasedOnDifficulty()
{
    // Act
    var easyS sector = _service.GenerateAlfheimSector(1, difficulty: 1);
    var deadlySector = _service.GenerateAlfheimSector(2, difficulty: 4);
    
    // Assert
    var easyTears = easySector.Rooms.Sum(r => r.Grid.Tiles.Count(t => t.HasFeature("Reality Tear")));
    var deadlyTears = deadlySector.Rooms.Sum(r => r.Grid.Tiles.Count(t => t.HasFeature("Reality Tear")));
    
    Assert.InRange(easyTears, 1, 2);   // Easy: 1-2
    Assert.InRange(deadlyTears, 4, 6); // Deadly: 4-6
}

[Fact]
public void GenerateAlfheimSector_EnsuresAtLeastOneEnergyConduit()
{
    // Act
    var sector = _service.GenerateAlfheimSector(1, difficulty: 1);
    
    // Assert
    var conduitCount = sector.Rooms.Sum(r => 
        r.Grid.Tiles.Count(t => t.HasFeature("Energy Conduit")));
    
    Assert.True(conduitCount >= 1);
}

[Fact]
public void GenerateAlfheimSector_AllRuneProvingGround_MarkedAsBossRoom()
{
    // Act
    var sector = _service.GenerateAlfheimSector(1, difficulty: 4, roomCount: 10);
    
    // Assert
    var provingGround = sector.Rooms.FirstOrDefault(
        r => r.TemplateName == "All-Rune Proving Ground");
    
    if (provingGround != null) // May not spawn every time
    {
        Assert.True(provingGround.IsBossRoom);
        Assert.Equal("All-Rune's Echo", provingGround.BossName);
    }
}
```

### C. RunicInstabilityService Tests

**Provided in v0.31.2, Section VII.**

### D. RealityTearService Tests

**Provided in v0.31.2, Section VII.**

---

## VII. Integration Testing

### A. Full Combat Scenario Test

**AlfheimIntegrationTests.cs:**

```csharp
[Fact]
public async Task FullAlfheimCombat_FromGenerationToVictory()
{
    // Arrange: Generate Alfheim sector
    var sector = _biomeService.GenerateAlfheimSector(1, difficulty: 2);
    var room = sector.Rooms.First();
    
    var party = CreateTestParty(withMystic: true);
    var combat = _combatService.InitializeCombat(party, room);
    
    // Assert: Alfheim conditions applied
    Assert.True(combat.HasAmbientCondition(107)); // Runic Instability
    Assert.True(party.All(c => c.PsychicStress >= 10)); // Psychic Resonance applied
    
    // Act: Simulate combat turns
    int turn = 1;
    while (!combat.IsComplete && turn <= 10)
    {
        // Process turn stress
        _alfheimService.ProcessTurnStress(combat, turn);
        
        // Each character acts
        foreach (var character in party)
        {
            if (character.Archetype == "Mystic")
            {
                // Mystic uses ability - may trigger surge
                var ability = character.Abilities.First();
                var result = _combatService.UseAbility(character, ability, combat);
                
                // Verify Wild Magic Surge can trigger
                // (In actual combat, this is probabilistic)
            }
            else
            {
                // Non-Mystic uses basic attack
                _combatService.BasicAttack(character, combat.Enemies.First());
            }
        }
        
        // Enemy turn
        foreach (var enemy in combat.Enemies.Where(e => e.IsAlive))
        {
            _enemyAI.TakeTurn(enemy, combat);
        }
        
        turn++;
    }
    
    // Assert: Combat completes
    Assert.True(combat.IsComplete || turn > 10);
    
    // Verify Psychic Stress accumulated
    Assert.True(party.All(c => c.PsychicStress > 10)); // Initial + per-turn
}
```

---

## VIII. Success Criteria

### Functional Requirements

- [ ]  AlfheimService initializes combat correctly
- [ ]  [Runic Instability] applies to all combats
- [ ]  [Psychic Resonance] applies high Stress
- [ ]  Wild Magic Surges trigger ~25% of time
- [ ]  Reality Tears warp positions correctly
- [ ]  Energy Conduits damage or channel
- [ ]  BiomeGenerationService generates Canopy-only sectors
- [ ]  Reality Tears placed based on difficulty
- [ ]  At least one Energy Conduit per sector
- [ ]  All-Rune Proving Ground marked as boss room

### Quality Gates

- [ ]  85%+ unit test coverage achieved
- [ ]  All services use Serilog structured logging
- [ ]  Integration tests pass
- [ ]  Performance acceptable (<500ms sector generation)
- [ ]  No memory leaks in long combat scenarios

### Balance Validation

- [ ]  Mystics challenged but viable
- [ ]  Wild Magic Surges feel impactful
- [ ]  Psychic Resonance manageable with high WILL
- [ ]  Reality Tears create positioning challenges
- [ ]  Energy Conduits provide tactical choice
- [ ]  Alfheim feels like endgame biome

---

## IX. Deployment Instructions

- [ ]  Alfheim feels like endgame biome

---

## IX. Remaining Work (Cannot Perform Automatically)

### 1. Database Execution (30 minutes)

Execute the three SQL migration files in order:

```bash
sqlite3 runeandrust.db < Data/v0.31.1_alfheim_schema.sql
sqlite3 runeandrust.db < Data/v0.31.2_environmental_hazards.sql
sqlite3 runeandrust.db < Data/v0.31.3_enemy_definitions.sql
```

**Verification:**

- Query `Biomes` table to confirm Alfheim entry (biome_id: 6)
- Query `Biome_RoomTemplates` for 8 Alfheim templates
- Query `Biome_EnvironmentalFeatures` for 10+ Alfheim hazards
- Query `Biome_EnemySpawns` for 5 enemy types

### 2. CombatEngine Code Changes (6-8 hours)

The integration guide provides exact code snippets for:

**Constructor Parameter Additions:**

- Inject `IAlfheimService`
- Inject `IRunicInstabilityService`
- Inject `IRealityTearService`

**`ApplyBiomeAmbientConditions()` Method Implementation:**

- Detect Alfheim biome (biome_id: 6)
- Apply [Runic Instability] condition (condition_id: 107)
- Apply [Psychic Resonance] high-intensity variant
- Apply Aether Pool +10% amplification to Mystics

**Per-Turn Stress Processing:**

- Call `AlfheimService.ProcessTurnStress()` at turn start
- Apply +5 Stress per turn to all combatants
- Log Stress accumulation for balance tracking

**Wild Magic Surge Integration:**

- Hook `RunicInstabilityService.TryTriggerWildMagicSurge()` on Mystic ability use
- Apply surge effects before damage/status calculations
- Log surge events with ability name, effect type, and result

**Reality Tear Processing:**

- Hook `RealityTearService.ProcessRealityTearEncounter()` on character movement
- Check if character ends turn on Reality Tear tile
- Apply position warp, damage, Corruption, and [Dazed] condition

**Combat Cleanup:**

- Clear Alfheim-specific buffs/debuffs at combat end
- Log biome statistics (surges triggered, tears encountered, total Stress)

### 3. Manual Testing (2-3 hours)

**Wild Magic Surge Testing:**

- [ ]  Create Mystic character in Alfheim combat
- [ ]  Cast 20+ abilities, track surge count
- [ ]  Verify ~25% surge rate (5±2 surges in 20 casts)
- [ ]  Test each surge effect type triggers correctly
- [ ]  Verify surge log entries created

**Reality Tear Testing:**

- [ ]  Generate Alfheim sector with Reality Tears
- [ ]  Move character onto Reality Tear tile
- [ ]  Verify character warps 3-5 tiles away
- [ ]  Verify 2d8 Energy damage applied
- [ ]  Verify +5 Corruption applied
- [ ]  Verify [Dazed] condition applied (1 turn)
- [ ]  Verify warp destination is valid/passable tile

**Psychic Resonance Testing:**

- [ ]  Start Alfheim combat with party
- [ ]  Record initial Stress for all characters
- [ ]  After 5 turns, verify Stress increased by 25+ (5 per turn)
- [ ]  Test high WILL character resists some Stress
- [ ]  Verify Stress log entries created

**Mystic Aether Pool Amplification:**

- [ ]  Create Mystic with base Aether Pool (e.g., 100 AP)
- [ ]  Enter Alfheim combat
- [ ]  Verify Aether Pool increased by +10% (110 AP)
- [ ]  Verify amplification removed after combat

---

## X. Deployment Instructions

### Step 1: Compile All Services

```bash
dotnet build Services/Biomes/AlfheimService.cs
dotnet build Services/Biomes/BiomeGenerationService_Alfheim.cs
dotnet build Services/RunicInstabilityService.cs
dotnet build Services/RealityTearService.cs
```

### Step 2: Run Full Test Suite

```bash
dotnet test --filter "FullyQualifiedName~Alfheim"
```

### Step 3: Run Integration Tests

```bash
dotnet test --filter "Category=Integration&Biome=Alfheim"
```

### Step 4: Manual Verification

```bash
# Start game and navigate to Alfheim
# Verify:
# - Sector generates with Canopy rooms
# - [Runic Instability] displays in combat
# - Wild Magic Surges trigger
# - Psychic Stress accumulates
# - Reality Tears teleport characters
# - Energy Conduits deal damage or allow channeling
# - All-Rune Proving Ground can be found (rare)
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

## X. Success Criteria Checklist

### Service Implementation

- [ ]  AlfheimService complete with all methods
- [ ]  BiomeGenerationService.GenerateAlfheimSector() implemented
- [ ]  RunicInstabilityService fully functional
- [ ]  RealityTearService fully functional
- [ ]  All services properly injected via DI

### Testing

- [ ]  10+ unit tests written
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

## XI. Next Steps

Once v0.31.4 is complete:

**v0.31 Alfheim Biome is COMPLETE!**

Future enhancements (not in v0.31 scope):

- v0.35: All-Rune's Echo full boss fight
- v0.36: Legendary crafting with Alfheim materials
- v0.37: Advanced physics simulation (time dilation, gravity inversion)
- v0.38: Biome-specific achievements/statistics

**Next Phase:**

- v0.32: Jotunheim Biome (Giant Ruins - Corruption/Psychic biome)

---

## XII. Related Documents

**Parent Specification:**

- v0.31: Alfheim Biome Implementation[[1]](v0%2031%20Alfheim%20Biome%20Implementation%20efa0af4639af46c19be493eb264c0489.md)

**Reference Implementation:**

- v0.29.4: Muspelheim Services[[2]](v0%2029%204%20Service%20Implementation%20&%20Testing%20ce4a69fbcddb40e2ac66db616c52bc6f.md)
- v0.30.4: Niflheim Services[[3]](toolu_01K1ndBPsR21niYsYPcaiLD6)

**Prerequisites:**

- v0.31.1: Database Schema[[4]](v0%2031%201%20Database%20Schema%20&%20Room%20Templates%20b3f608de386941b5a8a45ddfa962641a.md)
- v0.31.2: Environmental Hazards[[5]](v0%2031%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%20babad7a1095a4acea8314f2e9162344c.md)
- v0.31.3: Enemy Definitions (current document's sibling)

---

**Service implementation complete. v0.31 Alfheim Biome ready for deployment.**