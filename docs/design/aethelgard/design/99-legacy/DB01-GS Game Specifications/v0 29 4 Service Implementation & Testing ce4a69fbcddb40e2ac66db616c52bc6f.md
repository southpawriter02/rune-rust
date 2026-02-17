# v0.29.4: Service Implementation & Testing

Type: Technical
Description: Complete MuspelheimService, BiomeGenerationService.GenerateMuspelheimSector(), IntenseHeatService, unit test suite (10+ tests, 85% coverage), and integration testing.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.29.1-v0.29.3 (All prior Muspelheim specs)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.29: Muspelheim Biome Implementation (v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.29.4-SERVICES

**Status:** Design Complete — Ready for Phase 2 (Implementation)

**Timeline:** 9-15 hours

**Prerequisites:** v0.29.1 (Database), v0.29.2 (Hazards), v0.29.3 (Enemies)

**Parent Spec:** v0.29: Muspelheim Biome Implementation[[1]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)

---

## I. Executive Summary

This specification defines **service orchestration, procedural generation, and comprehensive testing** for the Muspelheim biome. It integrates all components from v0.29.1-v0.29.3 into a cohesive, playable biome experience.

### Scope

**Service Implementation:**

- MuspelheimService (orchestration layer)
- BiomeGenerationService.GenerateMuspelheimSector() (procedural generation)
- SpawnService integration (enemy placement)
- Resource drop service (loot system)
- Biome statistics tracking

**Testing Framework:**

- Integration tests (3+ scenarios)
- End-to-end biome generation test
- Balance validation tests
- Performance benchmarks
- Edge case coverage

**Quality Deliverables:**

- 85%+ total test coverage for v0.29
- Complete Serilog structured logging
- v5.0 setting compliance verification
- Balance tuning recommendations
- Known issues documentation

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.29.4)

**Service Orchestration:**

- MuspelheimService complete implementation
- Integration with DynamicRoomEngine (v0.10-v0.12)
- Integration with Tactical Grid (v0.20)
- Integration with Environmental Combat (v0.22)
- Biome entry/exit hooks

**Procedural Generation:**

- Room template selection (WFC constraints)
- Hazard placement algorithms
- Enemy spawn placement
- Resource node placement
- Lava river generation

**Testing:**

- Unit tests for all services
- Integration tests (biome generation, combat, heat damage)
- Balance validation (damage rates, spawn frequencies)
- Performance tests (generation time < 2s)
- Edge cases (0% Fire Resistance, 100% immunity)

**Documentation:**

- Implementation roadmap
- Known limitations
- Future enhancements
- Balance recommendations

### ❌ Explicitly Out of Scope

- Surtur's Herald full boss encounter (v0.35)
- Advanced AI behaviors (v0.36)
- UI/visual polish (separate phase)
- Audio design (separate phase)
- Additional biomes (Niflheim v0.30, Alfheim v0.31)
- Legendary crafting recipes (v0.36)

---

## III. MuspelheimService (Orchestration Layer)

### Service Architecture

```csharp
using Serilog;
using RuneAndRust.Core.Models;
using [RuneAndRust.Services](http://RuneAndRust.Services).Biomes;
using [RuneAndRust.Services](http://RuneAndRust.Services).Combat;
using System.Collections.Generic;

namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    /// <summary>
    /// Orchestration service for Muspelheim biome.
    /// Coordinates generation, hazards, enemies, and heat mechanics.
    /// </summary>
    public class MuspelheimService
    {
        private readonly ILogger _log;
        private readonly BiomeGenerationService _biomeGenService;
        private readonly IntenseHeatService _heatService;
        private readonly HazardPlacementService _hazardService;
        private readonly SpawnService _spawnService;
        private readonly ResourceDropService _resourceService;
        private readonly BiomeStatusService _biomeStatusService;
        private readonly BrittlenessService _brittlenessService;

        public MuspelheimService(
            ILogger log,
            BiomeGenerationService biomeGenService,
            IntenseHeatService heatService,
            HazardPlacementService hazardService,
            SpawnService spawnService,
            ResourceDropService resourceService,
            BiomeStatusService biomeStatusService,
            BrittlenessService brittlenessService)
        {
            _log = log;
            _biomeGenService = biomeGenService;
            _heatService = heatService;
            _hazardService = hazardService;
            _spawnService = spawnService;
            _resourceService = resourceService;
            _biomeStatusService = biomeStatusService;
            _brittlenessService = brittlenessService;
        }

        /// <summary>
        /// Initialize Muspelheim biome for a party.
        /// Called when party first enters biome.
        /// </summary>
        public MuspelheimBiomeInstance InitializeBiome(
            Party party,
            int sectorDepth,
            int? seed = null)
        {
            using (_log.BeginTimedOperation(
                "Initializing Muspelheim for party {PartyName} at depth {Depth}",
                [party.Name](http://party.Name),
                sectorDepth))
            {
                // Generate sector
                var sector = _biomeGenService.GenerateMuspelheimSector(
                    sectorDepth,
                    seed
                );

                _log.Information(
                    "Generated Muspelheim sector: {RoomCount} rooms, {HazardCount} hazards",
                    sector.Rooms.Count,
                    sector.TotalHazards
                );

                // Initialize biome status for each character
                foreach (var character in party.Characters)
                {
                    _biomeStatusService.InitializeCharacterBiomeStatus(
                        character.CharacterId,
                        biomeId: 4 // Muspelheim
                    );
                }

                // Check party Fire Resistance preparedness
                CheckPartyPreparedness(party);

                var biomeInstance = new MuspelheimBiomeInstance
                {
                    Sector = sector,
                    Party = party,
                    CurrentRoomId = sector.EntranceRoomId,
                    TurnsElapsed = 0,
                    IsActive = true
                };

                _log.Information(
                    "Muspelheim biome initialized for {PartyName}",
                    [party.Name](http://party.Name)
                );

                return biomeInstance;
            }
        }

        /// <summary>
        /// Process end-of-turn effects for Muspelheim.
        /// Applies [Intense Heat], hazard damage, etc.
        /// </summary>
        public void ProcessEndOfTurnEffects(
            MuspelheimBiomeInstance biomeInstance,
            Room currentRoom)
        {
            using (_log.BeginTimedOperation(
                "Processing Muspelheim end-of-turn effects for turn {Turn}",
                biomeInstance.TurnsElapsed))
            {
                var combatants = biomeInstance.GetAllCombatants();

                // 1. Apply [Intense Heat] ambient condition
                _heatService.ProcessEndOfTurnHeat(combatants, biomeId: 4);

                // 2. Apply environmental hazard damage
                foreach (var combatant in combatants)
                {
                    var tile = currentRoom.GetTileForCombatant(combatant);
                    if (tile != null)
                    {
                        ApplyTileHazardDamage(combatant, tile);
                    }
                }

                // 3. Trigger dynamic hazards (steam vents, collapsing catwalks)
                TriggerDynamicHazards(currentRoom);

                // 4. Check for gas pocket chain reactions
                CheckGasExplosions(currentRoom);

                // 5. Update biome statistics
                biomeInstance.TurnsElapsed++;
                UpdateBiomeStatistics(biomeInstance);

                _log.Information(
                    "End-of-turn processing complete for turn {Turn}",
                    biomeInstance.TurnsElapsed
                );
            }
        }

        /// <summary>
        /// Apply damage from hazards on a tile.
        /// </summary>
        private void ApplyTileHazardDamage(Combatant combatant, TacticalTile tile)
        {
            foreach (var hazard in tile.Features)
            {
                if (hazard.DamagePerTurn <= 0) continue;

                int damage = hazard.DamagePerTurn;

                // Apply resistance
                if (hazard.DamageType == [DamageType.Fire](http://DamageType.Fire))
                {
                    damage = _heatService.ApplyFireResistance(combatant, damage);
                }

                // Apply damage
                _damageService.ApplyDamage(
                    combatant,
                    damage,
                    hazard.DamageType,
                    hazard.FeatureName
                );

                _log.Information(
                    "{Combatant} takes {Damage} {Type} damage from {Hazard}",
                    [combatant.Name](http://combatant.Name),
                    damage,
                    hazard.DamageType,
                    hazard.FeatureName
                );
            }
        }

        /// <summary>
        /// Check party preparedness for Muspelheim.
        /// Logs warnings if Fire Resistance is insufficient.
        /// </summary>
        private void CheckPartyPreparedness(Party party)
        {
            int totalFireResistance = 0;
            int characterCount = party.Characters.Count;

            foreach (var character in party.Characters)
            {
                int fireRes = character.GetResistance([DamageType.Fire](http://DamageType.Fire));
                totalFireResistance += fireRes;

                if (fireRes < 50)
                {
                    _log.Warning(
                        "{Character} has low Fire Resistance: {Percent}% (recommended: 50%+)",
                        [character.Name](http://character.Name),
                        fireRes
                    );
                }
            }

            int averageFireResistance = totalFireResistance / characterCount;

            _log.Information(
                "Party average Fire Resistance: {Average}%",
                averageFireResistance
            );

            if (averageFireResistance < 40)
            {
                _log.Warning(
                    "Party is underprepared for Muspelheim (avg {Percent}% Fire Resistance)",
                    averageFireResistance
                );
            }
        }

        /// <summary>
        /// Handle party leaving Muspelheim.
        /// Finalizes statistics, removes ambient effects.
        /// </summary>
        public void OnBiomeExit(MuspelheimBiomeInstance biomeInstance)
        {
            using (_log.BeginTimedOperation(
                "Processing Muspelheim exit for party {PartyName}",
                [biomeInstance.Party.Name](http://biomeInstance.Party.Name)))
            {
                // Update biome statistics
                foreach (var character in [biomeInstance.Party](http://biomeInstance.Party).Characters)
                {
                    _biomeStatusService.FinalizeSession(
                        character.CharacterId,
                        biomeId: 4,
                        totalTurns: biomeInstance.TurnsElapsed
                    );
                }

                // Remove ambient condition effects (stop heat checks)
                biomeInstance.IsActive = false;

                _log.Information(
                    "Party {PartyName} exited Muspelheim after {Turns} turns",
                    [biomeInstance.Party.Name](http://biomeInstance.Party.Name),
                    biomeInstance.TurnsElapsed
                );
            }
        }

        // Additional methods: TriggerDynamicHazards, CheckGasExplosions, UpdateBiomeStatistics
        // ... (implementations follow similar patterns) ...
    }
}
```

---

## IV. Procedural Generation Service

### BiomeGenerationService Extension

```csharp
namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    public partial class BiomeGenerationService
    {
        /// <summary>
        /// Generate a Muspelheim sector using WFC constraints.
        /// </summary>
        public MuspelheimSector GenerateMuspelheimSector(
            int sectorDepth,
            int? seed = null)
        {
            using (_log.BeginTimedOperation(
                "Generating Muspelheim sector at depth {Depth}",
                sectorDepth))
            {
                var random = seed.HasValue ? new Random(seed.Value) : new Random();

                // Load room templates from database
                var templates = _database.GetRoomTemplates(biomeId: 4);
                _log.Information("Loaded {Count} room templates", templates.Count);

                // Generate room graph using WFC
                var roomGraph = GenerateRoomGraph(templates, sectorDepth, random);
                _log.Information("Generated room graph: {RoomCount} rooms", roomGraph.Rooms.Count);

                // Create rooms from templates
                var rooms = new List<Room>();
                foreach (var node in roomGraph.Rooms)
                {
                    var room = CreateRoomFromTemplate(node.Template, node.Position, random);
                    rooms.Add(room);
                }

                // Place hazards in each room
                foreach (var room in rooms)
                {
                    PlaceHazardsInRoom(room, random);
                }

                // Place enemies in each room
                foreach (var room in rooms)
                {
                    PlaceEnemiesInRoom(room, sectorDepth, random);
                }

                // Place resource nodes
                foreach (var room in rooms)
                {
                    PlaceResourceNodes(room, random);
                }

                // Connect rooms with doors/passages
                ConnectRooms(rooms, roomGraph);

                var sector = new MuspelheimSector
                {
                    Rooms = rooms,
                    Depth = sectorDepth,
                    EntranceRoomId = rooms.First(r => r.IsEntrance).RoomId,
                    ExitRoomId = rooms.First(r => r.IsExit).RoomId,
                    GeneratedAt = DateTime.UtcNow
                };

                _log.Information(
                    "Muspelheim sector generation complete: {RoomCount} rooms, {HazardCount} hazards, {EnemyCount} enemies",
                    sector.Rooms.Count,
                    sector.TotalHazards,
                    sector.TotalEnemies
                );

                return sector;
            }
        }

        /// <summary>
        /// Generate room connectivity graph using WFC constraints.
        /// </summary>
        private RoomGraph GenerateRoomGraph(
            List<RoomTemplate> templates,
            int sectorDepth,
            Random random)
        {
            // Determine room count based on depth
            int baseRoomCount = 8;
            int variability = [random.Next](http://random.Next)(-2, 3);
            int totalRooms = baseRoomCount + variability;

            _log.Information(
                "Generating room graph with {Count} rooms",
                totalRooms
            );

            var graph = new RoomGraph();

            // 1. Select entrance room (can_be_entrance = 1)
            var entranceTemplates = templates.Where(t => t.CanBeEntrance).ToList();
            var entranceTemplate = entranceTemplates[[random.Next](http://random.Next)(entranceTemplates.Count)];
            var entranceNode = new RoomNode
            {
                Template = entranceTemplate,
                Position = new Vector2Int(0, 0),
                IsEntrance = true
            };
            graph.Rooms.Add(entranceNode);

            // 2. Use WFC to place remaining rooms
            var queue = new Queue<RoomNode>();
            queue.Enqueue(entranceNode);

            while (queue.Count > 0 && graph.Rooms.Count < totalRooms)
            {
                var currentNode = queue.Dequeue();
                int connectionsNeeded = [random.Next](http://random.Next)(
                    currentNode.Template.MinConnections,
                    currentNode.Template.MaxConnections + 1
                );

                for (int i = 0; i < connectionsNeeded && graph.Rooms.Count < totalRooms; i++)
                {
                    // Get eligible templates based on WFC adjacency rules
                    var eligibleTemplates = GetEligibleAdjacentTemplates(
                        currentNode.Template,
                        templates
                    );

                    if (!eligibleTemplates.Any()) continue;

                    var nextTemplate = SelectWeightedTemplate(eligibleTemplates, random);
                    var nextPosition = GetAdjacentPosition(currentNode.Position, graph);

                    var nextNode = new RoomNode
                    {
                        Template = nextTemplate,
                        Position = nextPosition
                    };

                    graph.Rooms.Add(nextNode);
                    graph.AddConnection(currentNode, nextNode);
                    queue.Enqueue(nextNode);
                }
            }

            // 3. Mark exit room (can_be_exit = 1)
            var exitCandidates = graph.Rooms
                .Where(r => r.Template.CanBeExit && !r.IsEntrance)
                .ToList();
            if (exitCandidates.Any())
            {
                var exitNode = exitCandidates[[random.Next](http://random.Next)(exitCandidates.Count)];
                exitNode.IsExit = true;
            }

            _log.Information(
                "Room graph generated: {RoomCount} rooms, {ConnectionCount} connections",
                graph.Rooms.Count,
                graph.Connections.Count
            );

            return graph;
        }

        /// <summary>
        /// Get templates that can be adjacent based on WFC rules.
        /// </summary>
        private List<RoomTemplate> GetEligibleAdjacentTemplates(
            RoomTemplate currentTemplate,
            List<RoomTemplate> allTemplates)
        {
            var adjacencyRules = ParseAdjacencyRules(currentTemplate.WfcAdjacencyRules);

            var eligible = allTemplates.Where(t =>
            {
                // Check if allowed
                if (adjacencyRules.Allow.Any() && !adjacencyRules.Allow.Contains(t.TemplateName))
                {
                    return false;
                }

                // Check if forbidden
                if (adjacencyRules.Forbid.Contains(t.TemplateName))
                {
                    return false;
                }

                return true;
            }).ToList();

            return eligible;
        }

        /// <summary>
        /// Place hazards in a room based on template hazard_density.
        /// </summary>
        private void PlaceHazardsInRoom(Room room, Random random)
        {
            var hazards = _database.GetEnvironmentalFeatures(biomeId: 4);
            var template = room.Template;

            // Filter hazards by density eligibility
            var eligibleHazards = hazards
                .Where(h => IsHazardEligible(h, template.HazardDensity))
                .ToList();

            _log.Information(
                "Placing hazards in {RoomName}: {EligibleCount} eligible hazards",
                [room.Name](http://room.Name),
                eligibleHazards.Count
            );

            foreach (var hazard in eligibleHazards)
            {
                _hazardService.PlaceHazardType(room, hazard, random);
            }
        }

        /// <summary>
        /// Place enemies in a room based on spawn weights.
        /// </summary>
        private void PlaceEnemiesInRoom(
            Room room,
            int sectorDepth,
            Random random)
        {
            // Boss room check
            if (room.Template.TemplateName == "Containment Breach Zone")
            {
                PlaceBossEnemy(room, sectorDepth);
                return;
            }

            // Determine enemy count based on room size
            int enemyCount = CalculateEnemyCount(room, random);

            _log.Information(
                "Placing {Count} enemies in {RoomName}",
                enemyCount,
                [room.Name](http://room.Name)
            );

            var eligibleEnemies = _database.GetEnemySpawns(
                biomeId: 4,
                minLevel: sectorDepth,
                maxLevel: sectorDepth + 2
            );

            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = SelectWeightedEnemy(eligibleEnemies, random);
                var spawnPosition = FindValidSpawnPosition(room, random);

                if (spawnPosition != null)
                {
                    room.SpawnEnemy(enemy, spawnPosition);
                }
            }
        }

        // Additional helper methods...
    }
}
```

---

## V. Integration Tests

### Test 1: Complete Biome Generation

```csharp
[TestClass]
public class MuspelheimIntegrationTests
{
    [TestMethod]
    public void GenerateMuspelheimSector_Success()
    {
        // Arrange
        var service = CreateMuspelheimService();
        var party = CreateTestParty();

        // Act
        var biomeInstance = service.InitializeBiome(party, sectorDepth: 8, seed: 12345);

        // Assert
        Assert.IsNotNull(biomeInstance);
        Assert.IsNotNull(biomeInstance.Sector);
        Assert.IsTrue(biomeInstance.Sector.Rooms.Count >= 6);
        Assert.IsTrue(biomeInstance.Sector.Rooms.Count <= 12);

        // Verify entrance and exit exist
        Assert.IsTrue(biomeInstance.Sector.Rooms.Any(r => r.IsEntrance));
        Assert.IsTrue(biomeInstance.Sector.Rooms.Any(r => r.IsExit));

        // Verify hazards placed
        int totalHazards = biomeInstance.Sector.Rooms.Sum(r => r.Tiles.Count(t => t.HasHazard));
        Assert.IsTrue(totalHazards > 0, "No hazards placed");

        // Verify enemies spawned
        int totalEnemies = biomeInstance.Sector.Rooms.Sum(r => r.Enemies.Count);
        Assert.IsTrue(totalEnemies > 0, "No enemies spawned");
    }
}
```

### Test 2: [Intense Heat] Damage Loop

```csharp
[TestMethod]
public void IntenseHeat_DamageLoop_10Turns()
{
    // Arrange
    var service = CreateMuspelheimService();
    var party = CreateTestParty();
    var biomeInstance = service.InitializeBiome(party, sectorDepth: 8);
    var character = party.Characters.First();
    int initialHP = character.CurrentHP;

    // Act: Simulate 10 turns of combat
    for (int turn = 0; turn < 10; turn++)
    {
        service.ProcessEndOfTurnEffects(biomeInstance, biomeInstance.CurrentRoom);
    }

    // Assert: Character should have taken heat damage
    Assert.IsTrue(character.CurrentHP < initialHP, "No heat damage taken");

    // Verify biome status tracking
    var biomeStatus = _database.GetBiomeStatus(character.CharacterId, biomeId: 4);
    Assert.IsTrue(biomeStatus.HeatDamageTaken > 0);
}
```

### Test 3: Brittleness Combo

```csharp
[TestMethod]
public void Brittleness_IceThenPhysical_ComboWorks()
{
    // Arrange
    var service = CreateMuspelheimService();
    var party = CreateTestParty();
    var biomeInstance = service.InitializeBiome(party, sectorDepth: 8);
    
    // Spawn Forge-Hardened Undying (Fire Resistant)
    var enemy = SpawnTestEnemy("Forge-Hardened Undying");
    biomeInstance.CurrentRoom.AddEnemy(enemy);

    // Act: Ice Mystic attacks
    var iceDamage = 15;
    _damageService.ApplyDamage(enemy, iceDamage, [DamageType.Ice](http://DamageType.Ice), "Frost Bolt");

    // Assert: [Brittle] applied
    Assert.IsTrue(enemy.HasStatusEffect("[Brittle]"));

    // Act: Physical Warrior attacks
    int physicalDamageBase = 20;
    int enemyHPBefore = enemy.CurrentHP;
    _damageService.ApplyDamage(enemy, physicalDamageBase, DamageType.Physical, "Greatsword");
    int damageTaken = enemyHPBefore - enemy.CurrentHP;

    // Assert: Physical damage boosted by ~50%
    Assert.IsTrue(damageTaken >= 28, $"Expected ~30 damage, got {damageTaken}");
}
```

---

## VI. Balance Validation

### Heat Damage Analysis

```csharp
[TestMethod]
public void BalanceTest_HeatDamage_SurvivalTime()
{
    // Test: Party with varying Fire Resistance levels
    var scenarios = new[]
    {
        new { FireRes = 0, ExpectedSurvival = 5 },
        new { FireRes = 25, ExpectedSurvival = 8 },
        new { FireRes = 50, ExpectedSurvival = 15 },
        new { FireRes = 75, ExpectedSurvival = 30 },
        new { FireRes = 100, ExpectedSurvival = int.MaxValue }
    };

    foreach (var scenario in scenarios)
    {
        var character = CreateTestCharacter(fireResistance: scenario.FireRes);
        int turnsSurvived = SimulateHeatDamageUntilDeath(character);

        _log.Information(
            "{FireRes}% Fire Resistance: survived {Turns} turns (expected {Expected})",
            scenario.FireRes,
            turnsSurvived,
            scenario.ExpectedSurvival
        );

        // Assert within 20% margin
        Assert.IsTrue(
            turnsSurvived >= scenario.ExpectedSurvival * 0.8,
            $"Survival time too low: {turnsSurvived} < {scenario.ExpectedSurvival * 0.8}"
        );
    }
}
```

### Enemy Spawn Distribution

```csharp
[TestMethod]
public void BalanceTest_EnemySpawnDistribution()
{
    // Generate 100 sectors, analyze enemy distribution
    var enemyCounts = new Dictionary<string, int>();

    for (int i = 0; i < 100; i++)
    {
        var sector = _biomeGenService.GenerateMuspelheimSector(
            sectorDepth: 8,
            seed: i
        );

        foreach (var room in sector.Rooms)
        {
            foreach (var enemy in room.Enemies)
            {
                if (!enemyCounts.ContainsKey([enemy.Name](http://enemy.Name)))
                {
                    enemyCounts[[enemy.Name](http://enemy.Name)] = 0;
                }
                enemyCounts[[enemy.Name](http://enemy.Name)]++;
            }
        }
    }

    // Expected distribution based on spawn weights
    // Forge-Hardened: 150, Magma Elemental: 80, Rival Berserker: 60, etc.
    _log.Information("Enemy spawn distribution over 100 sectors:");
    foreach (var kvp in enemyCounts.OrderByDescending(x => x.Value))
    {
        _log.Information("  {Enemy}: {Count}", kvp.Key, kvp.Value);
    }

    // Assert: Most common should be Forge-Hardened Undying
    var mostCommon = enemyCounts.OrderByDescending(x => x.Value).First();
    Assert.AreEqual("Forge-Hardened Undying", mostCommon.Key);
}
```

---

## VII. Performance Benchmarks

### Generation Time Test

```csharp
[TestMethod]
public void Performance_SectorGeneration_Under2Seconds()
{
    // Arrange
    var service = CreateMuspelheimService();
    var stopwatch = Stopwatch.StartNew();

    // Act: Generate sector
    var sector = _biomeGenService.GenerateMuspelheimSector(
        sectorDepth: 8,
        seed: 12345
    );

    stopwatch.Stop();

    // Assert: Generation time < 2 seconds
    _log.Information(
        "Sector generation time: {Milliseconds}ms",
        stopwatch.ElapsedMilliseconds
    );

    Assert.IsTrue(
        stopwatch.ElapsedMilliseconds < 2000,
        $"Generation too slow: {stopwatch.ElapsedMilliseconds}ms"
    );
}
```

---

## VIII. Known Limitations

### v0.29 Constraints

1. **Boss Encounter Framework Only:**
    - Surtur's Herald exists as spawnable enemy
    - Full multi-phase encounter deferred to v0.35
    - Current implementation: Basic stat block + spawn weight
2. **Simplified AI:**
    - Enemies use basic AI from existing systems
    - Advanced behaviors (Magma Elemental trail, Berserker Fury) deferred to v0.36
    - Current: Standard melee/ranged attack patterns
3. **Static Hazard Placement:**
    - Hazards placed at generation time
    - Dynamic hazards (steam vents, catwalks) have basic trigger logic
    - Advanced dynamic systems (destructible pipes) deferred to v0.34
4. **No Legendary Crafting:**
    - Resources drop (Star-Metal Ore, Surtur Engine Core)
    - Crafting recipes for legendaries deferred to v0.36
    - Current: Resources exist but no crafting implementation
5. **Basic Resource Node Placement:**
    - Nodes placed randomly based on room template resource_spawn_chance
    - Hidden legendary nodes (Eternal Ember) not yet discoverable
    - Advanced treasure placement deferred to v0.33

### Future Enhancements (Post-v0.29)

**v0.30-v0.32:** Additional biomes (Niflheim, Alfheim, Jotunheim)

**v0.33:** Advanced treasure placement, legendary node hints

**v0.34:** Environmental Combat expansion, destructible structures

**v0.35:** Surtur's Herald full boss encounter

**v0.36:** Legendary crafting recipes, advanced enemy AI

**v0.38:** Biome statistics dashboard UI

**v0.40:** Player-designed room workshop

---

## IX. Implementation Roadmap

### Phase 1: Service Implementation (4-6 hours)

- [ ]  MuspelheimService orchestration layer
- [ ]  BiomeGenerationService.GenerateMuspelheimSector()
- [ ]  WFC room graph generation
- [ ]  Hazard placement integration
- [ ]  Enemy spawn placement
- [ ]  Resource node placement

### Phase 2: Integration Testing (3-5 hours)

- [ ]  Complete biome generation test
- [ ]  [Intense Heat] damage loop test
- [ ]  Brittleness combo test
- [ ]  End-to-end playthrough test
- [ ]  Party preparedness test

### Phase 3: Balance Validation (2-3 hours)

- [ ]  Heat damage survival time analysis
- [ ]  Enemy spawn distribution analysis
- [ ]  Resource drop rate validation
- [ ]  Hazard density validation
- [ ]  Balance tuning recommendations

### Phase 4: Performance & Polish (2-3 hours)

- [ ]  Generation time benchmarks
- [ ]  Memory usage profiling
- [ ]  Logging optimization
- [ ]  Edge case coverage
- [ ]  Documentation finalization

**Total: 11-17 hours** (target 9-15 hours)

---

## X. Success Criteria

### Functional Requirements

- [ ]  Muspelheim sector generates successfully (6-12 rooms)
- [ ]  All 8 room templates can appear
- [ ]  All 8 hazard types place correctly
- [ ]  All 5 enemy types spawn correctly
- [ ]  [Intense Heat] applies every turn
- [ ]  [Brittle] mechanic works (Ice → Physical vulnerability)
- [ ]  Lava rivers block movement
- [ ]  Resource drops occur with correct probabilities
- [ ]  Biome statistics track correctly

### Quality Gates

- [ ]  85%+ unit test coverage for v0.29 overall
- [ ]  All services use Serilog structured logging
- [ ]  v5.0 setting compliance verified
- [ ]  v2.0 mechanical values preserved
- [ ]  ASCII-only entity names confirmed
- [ ]  Performance benchmarks met (<2s generation)

### Integration Verification

- [ ]  Tactical Grid integration works (lava as [Chasm])
- [ ]  Environmental Combat steam vents destructible
- [ ]  Trauma Economy heat Stress accumulates
- [ ]  Status Effects [Brittle] applies vulnerabilities
- [ ]  Party can navigate full sector entrance→exit

---

## XI. v5.0 Setting Compliance Verification

✅ **All v0.29 Components:**

- v0.29.1: Database (room templates, resources)
- v0.29.2: Hazards ([Intense Heat], 8 hazard types)
- v0.29.3: Enemies (5 types, Brittleness)
- v0.29.4: Services (orchestration, generation)

✅ **Technology, Not Magic:**

- All heat = thermal regulation failure
- Magma Elementals = corrupted monitoring constructs
- Surtur's Herald = Jötun-Forged warmachine
- No supernatural elements

✅ **Layer 2 Voice:**

- "Containment breach," "thermal load," "corrupted chassis"
- Industrial disaster terminology throughout

✅ **ASCII-Only Entity Names:**

- All entities verified ASCII-compliant

✅ **v2.0 Canonical Accuracy:**

- [Intense Heat] DC 12, 2d6 Fire damage preserved
- Fire Resistance 75% for Forge-Hardened preserved
- Brittleness mechanic (Ice → Physical vuln) preserved

---

## XII. Related Documents

**Parent:** v0.29: Muspelheim Biome Implementation[[1]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)

**Dependencies:**

- v0.29.1: Database Schema & Room Templates[[2]](v0%2029%201%20Database%20Schema%20&%20Room%20Templates%204459437f44df4cb2aa9f3c4a71efe23d.md)
- v0.29.2: Environmental Hazards & Ambient Conditions[[3]](v0%2029%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%200d398758edcb4560a4b0b4a9875c4a04.md)
- v0.29.3: Enemy Definitions & Spawn System[[4]](v0%2029%203%20Enemy%20Definitions%20&%20Spawn%20System%208f34dab3be0e4869bd99f792215abd8a.md)

**Canonical:** v2.0 Muspelheim Biome[[5]](https://www.notion.so/Feature-Specification-The-Muspelheim-Biome-2a355eb312da80cdab65de771b57e414?pvs=21)

**Requirements:** MANDATORY[[6]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)

**Project Context:**

- Master Roadmap[[7]](https://www.notion.so/Master-Roadmap-v0-1-v1-0-4b4f512f0dd7444486e2c59e676378ad?pvs=21)
- AI Session Handoff[[8]](https://www.notion.so/AI-Session-Handoff-Rune-Rust-Development-Status-e19fe6060d6d4e44ae7402d88e3cc6a3?pvs=21)

---

**Phase 1 (Service Design): COMPLETE ✓**

**Phase 2 (Implementation): Ready to begin**

**v0.29 Muspelheim Biome: ALL SPECIFICATIONS COMPLETE ✓**