# v0.39.3: Content Density & Population Budget

Type: Technical
Description: Global sector budgets, room density classification, threat heatmap generation
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.10-v0.12, v0.39.1, v0.39.2
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.39: Advanced Dynamic Room Engine (v0%2039%20Advanced%20Dynamic%20Room%20Engine%20ea7030c7db18486d90330325a4e97005.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Design Phase

**Prerequisites:** v0.10-v0.12 (Dynamic Room Engine), v0.39.1 (3D Vertical System), v0.39.2 (Biome Transitions)

**Timeline:** 18-25 hours (3-4 weeks part-time)

**Goal:** Implement intelligent global population budgets preventing content over-saturation

**Philosophy:** Not every room needs combat—pacing through global density awareness

---

## I. Executive Summary

v0.39.3 implements **global content density management** that prevents the exhausting over-population problem in current v0.11 population system. Instead of per-room budgets that lead to 40+ enemies per sector, this system uses global budgets distributed intelligently across rooms.

**What v0.39.3 Delivers:**

- Global sector budgets (12-15 enemies, not 25-40)
- Room density classification (Empty/Light/Medium/Heavy/Boss)
- Budget distribution algorithm prioritizing variety
- Threat heatmap generation
- Integration with v0.11 population pipeline

**Why This Matters:**

Current v0.11 system has **naive per-room budgets**:

```jsx
Current Problem:
Room 1: 5 enemies + 2 hazards = 7 threats
Room 2: 3 enemies + 3 hazards = 6 threats
Room 3: 7 enemies + 1 hazard = 8 threats
Room 4: 4 enemies + 2 hazards = 6 threats
Room 5: 6 enemies + 2 hazards = 8 threats

Total: 25 enemies + 10 hazards = 35 threats in 5 rooms

Result: EXHAUSTING. Every room is combat. No pacing.
```

**v0.39.3 Solution:**

```jsx
Global Budget System:
Total Sector Budget: 12 enemies + 8 hazards = 20 threats

Distribution:
Room 1 (Empty): 0 threats - Breather room
Room 2 (Light): 2 enemies - Minor encounter
Room 3 (Medium): 4 enemies + 2 hazards - Standard fight
Room 4 (Light): 1 enemy + 2 hazards - Environmental challenge
Room 5 (Boss): 5 enemies + 4 hazards - Climactic battle

Result: Varied pacing. Breather rooms between fights.
```

---

## II. Global Budget Calculation

### Budget Formula

```csharp
public class ContentDensityService
{
    public GlobalBudget CalculateGlobalBudget(
        int roomCount,
        DifficultyTier difficulty,
        string biomeId)
    {
        // Base: 2.0-2.5 enemies per room (not 4-5)
        var baseEnemiesPerRoom = 2.2f;
        var baseHazardsPerRoom = 1.5f;
        var baseLootPerRoom = 0.8f;
        
        var baseEnemies = (int)(roomCount * baseEnemiesPerRoom);
        var baseHazards = (int)(roomCount * baseHazardsPerRoom);
        var baseLoot = (int)(roomCount * baseLootPerRoom);
        
        // Difficulty multiplier
        var difficultyMultiplier = difficulty switch
        {
            DifficultyTier.Easy => 0.8f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.3f,
            DifficultyTier.Lethal => 1.6f,
            _ => 1.0f
        };
        
        // Biome modifier (some biomes are more dangerous)
        var biomeMultiplier = GetBiomeMultiplier(biomeId);
        
        return new GlobalBudget
        {
            TotalEnemyBudget = (int)(baseEnemies * difficultyMultiplier * biomeMultiplier),
            TotalHazardBudget = (int)(baseHazards * difficultyMultiplier * biomeMultiplier),
            TotalLootBudget = baseLoot,  // Loot NOT scaled by difficulty
            
            EnemiesSpawned = 0,
            HazardsSpawned = 0,
            LootSpawned = 0
        };
    }
    
    private float GetBiomeMultiplier(string biomeId)
    {
        return biomeId switch
        {
            "the_roots" => 1.0f,
            "muspelheim" => 1.2f,      // More dangerous
            "niflheim" => 1.2f,        // More dangerous
            "alfheim" => 0.9f,         // Slightly less dense
            "jotunheim" => 1.3f,       // Most dangerous
            _ => 1.0f
        };
    }
}

public class GlobalBudget
{
    public int TotalEnemyBudget { get; set; }
    public int TotalHazardBudget { get; set; }
    public int TotalLootBudget { get; set; }
    
    public int EnemiesSpawned { get; set; }
    public int HazardsSpawned { get; set; }
    public int LootSpawned { get; set; }
    
    public int RemainingEnemyBudget => TotalEnemyBudget - EnemiesSpawned;
    public int RemainingHazardBudget => TotalHazardBudget - HazardsSpawned;
    public int RemainingLootBudget => TotalLootBudget - LootSpawned;
    
    public bool IsEnemyBudgetExhausted => RemainingEnemyBudget <= 0;
    public bool IsHazardBudgetExhausted => RemainingHazardBudget <= 0;
    public bool IsLootBudgetExhausted => RemainingLootBudget <= 0;
}
```

### Example Budget Calculations

**Scenario 1: 7-room Normal difficulty sector in The Roots**

```jsx
Inputs:
- Room Count: 7
- Difficulty: Normal (1.0×)
- Biome: The Roots (1.0×)

Calculation:
Base Enemies: 7 × 2.2 = 15.4 → 15 enemies
Base Hazards: 7 × 1.5 = 10.5 → 10 hazards
Base Loot: 7 × 0.8 = 5.6 → 5 loot nodes

Total Budget:
- 15 enemies
- 10 hazards
- 5 loot nodes

Average per room: 2.1 enemies, 1.4 hazards (vs old system: 4-5 enemies)
```

**Scenario 2: 5-room Hard difficulty sector in Muspelheim**

```jsx
Inputs:
- Room Count: 5
- Difficulty: Hard (1.3×)
- Biome: Muspelheim (1.2×)

Calculation:
Base Enemies: 5 × 2.2 = 11 enemies
Apply multipliers: 11 × 1.3 × 1.2 = 17.16 → 17 enemies

Base Hazards: 5 × 1.5 = 7.5 → 7 hazards
Apply multipliers: 7.5 × 1.3 × 1.2 = 11.7 → 11 hazards

Total Budget:
- 17 enemies (challenging)
- 11 hazards (fire-themed)
- 4 loot nodes (not scaled)
```

---

## III. Room Density Classification

### Density Types

```csharp
public enum RoomDensity
{
    Empty,      // 0 threats - Breather/exploration room
    Light,      // 1-2 threats - Minor encounter
    Medium,     // 3-4 threats - Standard fight
    Heavy,      // 5-7 threats - Challenging battle
    Boss        // 8+ threats - Climactic encounter
}

public class DensityClassificationService
{
    public Dictionary<Room, RoomDensity> ClassifyRooms(List<Room> rooms, Random rng)
    {
        var classifications = new Dictionary<Room, RoomDensity>();
        
        // Fixed classifications
        foreach (var room in rooms)
        {
            if (room.Archetype == RoomArchetype.BossArena)
            {
                classifications[room] = RoomDensity.Boss;
            }
            else if (room.Archetype == RoomArchetype.EntryHall)
            {
                classifications[room] = RoomDensity.Light;  // Safe start
            }
            else if (room.Archetype == RoomArchetype.SecretRoom)
            {
                classifications[room] = RoomDensity.Empty;  // Reward exploration
            }
        }
        
        // Classify remaining rooms
        var unclassifiedRooms = rooms
            .Where(r => !classifications.ContainsKey(r))
            .ToList();
        
        // Target distribution:
        // 10-15% Empty
        // 40-50% Light
        // 25-35% Medium
        // 10-15% Heavy
        
        var emptyCount = (int)(unclassifiedRooms.Count * 0.12f);
        var lightCount = (int)(unclassifiedRooms.Count * 0.45f);
        var mediumCount = (int)(unclassifiedRooms.Count * 0.30f);
        var heavyCount = unclassifiedRooms.Count - emptyCount - lightCount - mediumCount;
        
        // Shuffle and assign
        var shuffled = unclassifiedRooms.OrderBy(r => [rng.Next](http://rng.Next)()).ToList();
        
        for (int i = 0; i < shuffled.Count; i++)
        {
            var density = i switch
            {
                < var e when e < emptyCount => RoomDensity.Empty,
                < var l when l < emptyCount + lightCount => RoomDensity.Light,
                < var m when m < emptyCount + lightCount + mediumCount => RoomDensity.Medium,
                _ => RoomDensity.Heavy
            };
            
            classifications[shuffled[i]] = density;
        }
        
        return classifications;
    }
}
```

### Density Target Ranges

| Density | Threats | % of Rooms | Purpose |
| --- | --- | --- | --- |
| Empty | 0 | 10-15% | Breather/exploration/loot |
| Light | 1-2 | 40-50% | Minor encounters, pacing |
| Medium | 3-4 | 25-35% | Standard combat, challenge |
| Heavy | 5-7 | 10-15% | Difficult battles, preparation required |
| Boss | 8+ | 5% | Climactic encounters (1 per sector) |

**Example 7-Room Sector:**

```jsx
Room 1 (Entry Hall): Light - 1 enemy
Room 2 (Corridor): Empty - 0 threats (exploration)
Room 3 (Chamber): Medium - 3 enemies + 1 hazard
Room 4 (Junction): Light - 2 enemies
Room 5 (Chamber): Heavy - 5 enemies + 2 hazards
Room 6 (Corridor): Light - 1 enemy + 1 hazard
Room 7 (Boss Arena): Boss - 3 enemies + 5 hazards

Total: 15 enemies + 9 hazards = 24 threats across 7 rooms
Average: 3.4 threats per room (but distributed intelligently)
```

---

## IV. Budget Distribution Algorithm

### Distribution Strategy

```csharp
public class BudgetDistributionService
{
    private readonly ILogger<BudgetDistributionService> _logger;
    
    public SectorPopulationPlan DistributeBudget(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        Random rng)
    {
        var plan = new SectorPopulationPlan();
        
        // Step 1: Allocate to Boss rooms first (20-30% of budget)
        AllocateToBossRooms(budget, densityMap, plan);
        
        // Step 2: Allocate to Heavy rooms (15-20% of budget)
        AllocateToHeavyRooms(budget, densityMap, plan);
        
        // Step 3: Allocate to Medium rooms (30-40% of budget)
        AllocateToMediumRooms(budget, densityMap, plan);
        
        // Step 4: Allocate to Light rooms (remaining budget)
        AllocateToLightRooms(budget, densityMap, plan);
        
        // Step 5: Empty rooms get loot, not threats
        AllocateLootToEmptyRooms(budget, densityMap, plan);
        
        _logger.Information(
            "Budget distribution complete: Enemies={Enemies}/{Total}, Hazards={Hazards}/{Total}",
            plan.TotalEnemiesAllocated, budget.TotalEnemyBudget,
            plan.TotalHazardsAllocated, budget.TotalHazardBudget);
        
        return plan;
    }
    
    private void AllocateToBossRooms(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        SectorPopulationPlan plan)
    {
        var bossRooms = densityMap
            .Where(kvp => kvp.Value == RoomDensity.Boss)
            .Select(kvp => kvp.Key)
            .ToList();
        
        if (!bossRooms.Any()) return;
        
        // Boss rooms get 20-30% of total budget
        var bossEnemyBudget = (int)(budget.TotalEnemyBudget * 0.25f);
        var bossHazardBudget = (int)(budget.TotalHazardBudget * 0.25f);
        
        foreach (var room in bossRooms)
        {
            plan.RoomAllocations[[room.Id](http://room.Id)] = new RoomAllocation
            {
                RoomId = [room.Id](http://room.Id),
                AllocatedEnemies = bossEnemyBudget / bossRooms.Count,
                AllocatedHazards = bossHazardBudget / bossRooms.Count,
                AllocatedLoot = 2  // Boss rooms always have good loot
            };
        }
        
        budget.EnemiesSpawned += bossEnemyBudget;
        budget.HazardsSpawned += bossHazardBudget;
    }
    
    private void AllocateToHeavyRooms(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        SectorPopulationPlan plan)
    {
        var heavyRooms = densityMap
            .Where(kvp => kvp.Value == RoomDensity.Heavy)
            .Select(kvp => kvp.Key)
            .ToList();
        
        if (!heavyRooms.Any()) return;
        
        // Heavy rooms: 5-7 threats each
        foreach (var room in heavyRooms)
        {
            var enemyAllocation = Math.Min(5, budget.RemainingEnemyBudget);
            var hazardAllocation = Math.Min(2, budget.RemainingHazardBudget);
            
            plan.RoomAllocations[[room.Id](http://room.Id)] = new RoomAllocation
            {
                RoomId = [room.Id](http://room.Id),
                AllocatedEnemies = enemyAllocation,
                AllocatedHazards = hazardAllocation,
                AllocatedLoot = 1
            };
            
            budget.EnemiesSpawned += enemyAllocation;
            budget.HazardsSpawned += hazardAllocation;
        }
    }
    
    private void AllocateToMediumRooms(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        SectorPopulationPlan plan)
    {
        var mediumRooms = densityMap
            .Where(kvp => kvp.Value == RoomDensity.Medium)
            .Select(kvp => kvp.Key)
            .ToList();
        
        if (!mediumRooms.Any()) return;
        
        // Medium rooms: 3-4 threats each
        foreach (var room in mediumRooms)
        {
            var enemyAllocation = Math.Min(3, budget.RemainingEnemyBudget);
            var hazardAllocation = Math.Min(1, budget.RemainingHazardBudget);
            
            plan.RoomAllocations[[room.Id](http://room.Id)] = new RoomAllocation
            {
                RoomId = [room.Id](http://room.Id),
                AllocatedEnemies = enemyAllocation,
                AllocatedHazards = hazardAllocation,
                AllocatedLoot = 1
            };
            
            budget.EnemiesSpawned += enemyAllocation;
            budget.HazardsSpawned += hazardAllocation;
        }
    }
}

public class SectorPopulationPlan
{
    public Dictionary<string, RoomAllocation> RoomAllocations { get; set; } = new();
    
    public int TotalEnemiesAllocated => RoomAllocations.Values.Sum(a => a.AllocatedEnemies);
    public int TotalHazardsAllocated => RoomAllocations.Values.Sum(a => a.AllocatedHazards);
    public int TotalLootAllocated => RoomAllocations.Values.Sum(a => a.AllocatedLoot);
}

public class RoomAllocation
{
    public string RoomId { get; set; }
    public int AllocatedEnemies { get; set; }
    public int AllocatedHazards { get; set; }
    public int AllocatedLoot { get; set; }
    public RoomDensity Density { get; set; }
}
```

---

## V. Threat Heatmap Generation

### Heatmap Purpose

A **threat heatmap** visualizes density distribution across the sector, helping with:

- Balance validation
- Debugging over-dense areas
- Identifying pacing issues

```csharp
public class ThreatHeatmapService
{
    public ThreatHeatmap GenerateHeatmap(Sector sector, SectorPopulationPlan plan)
    {
        var heatmap = new ThreatHeatmap();
        
        foreach (var room in sector.Rooms)
        {
            if (!plan.RoomAllocations.TryGetValue([room.Id](http://room.Id), out var allocation))
                continue;
            
            var threatLevel = allocation.AllocatedEnemies + allocation.AllocatedHazards;
            
            heatmap.RoomThreatLevels[[room.Id](http://room.Id)] = new ThreatLevel
            {
                RoomId = [room.Id](http://room.Id),
                Position = room.Position,
                TotalThreats = threatLevel,
                Enemies = allocation.AllocatedEnemies,
                Hazards = allocation.AllocatedHazards,
                Intensity = CalculateIntensity(threatLevel)
            };
        }
        
        heatmap.AverageThreatLevel = heatmap.RoomThreatLevels.Values
            .Average(t => t.TotalThreats);
        
        heatmap.MaxThreatLevel = heatmap.RoomThreatLevels.Values
            .Max(t => t.TotalThreats);
        
        return heatmap;
    }
    
    private ThreatIntensity CalculateIntensity(int threatCount)
    {
        return threatCount switch
        {
            0 => ThreatIntensity.None,
            1 or 2 => ThreatIntensity.Low,
            3 or 4 => ThreatIntensity.Medium,
            5 or 6 or 7 => ThreatIntensity.High,
            _ => ThreatIntensity.Extreme
        };
    }
}

public class ThreatHeatmap
{
    public Dictionary<string, ThreatLevel> RoomThreatLevels { get; set; } = new();
    public float AverageThreatLevel { get; set; }
    public int MaxThreatLevel { get; set; }
}

public class ThreatLevel
{
    public string RoomId { get; set; }
    public RoomPosition Position { get; set; }
    public int TotalThreats { get; set; }
    public int Enemies { get; set; }
    public int Hazards { get; set; }
    public ThreatIntensity Intensity { get; set; }
}

public enum ThreatIntensity
{
    None,      // 0 threats
    Low,       // 1-2 threats
    Medium,    // 3-4 threats
    High,      // 5-7 threats
    Extreme    // 8+ threats
}
```

### Heatmap Visualization (Debug)

```
Threat Heatmap: "Sector_12345"

Z=0 (Ground Level):
  [Entry Hall]    Intensity: Low      (1 enemy)
  [Corridor]      Intensity: None     (0 threats)
  [Chamber]       Intensity: Medium   (3 enemies + 1 hazard)
  
Z=-1 (Lower Level):
  [Junction]      Intensity: Low      (2 enemies)
  [Chamber]       Intensity: High     (5 enemies + 2 hazards)
  [Boss Arena]    Intensity: Extreme  (3 enemies + 5 hazards)

Statistics:
- Average Threat Level: 2.8 threats/room
- Max Threat Level: 8 threats (Boss Arena)
- Empty Rooms: 1 (16.7%)
- Light Rooms: 2 (33.3%)
- Medium Rooms: 1 (16.7%)
- Heavy Rooms: 1 (16.7%)
- Boss Rooms: 1 (16.7%)
```

---

## VI. Integration with v0.11 Population

### Modified Population Pipeline

```csharp
public class DungeonGenerator
{
    private readonly IContentDensityService _densityService;
    private readonly IDensityClassificationService _classificationService;
    private readonly IBudgetDistributionService _distributionService;
    
    public Sector Generate(int seed, SectorBlueprint blueprint)
    {
        var rng = new Random(seed);
        
        // v0.10: Layout generation (UNCHANGED)
        var graph = GenerateGraph(blueprint, rng);
        
        // v0.39.1: 3D spatial layout (UNCHANGED)
        var sector = _spatialLayoutService.ConvertGraphTo3DLayout(graph, seed);
        
        // v0.39.2: Biome transitions (UNCHANGED)
        ApplyBiomeTransitions(sector, blueprint, rng);
        
        // === NEW: v0.39.3 Content Density Management ===
        
        // Step 1: Calculate global budget
        var globalBudget = _densityService.CalculateGlobalBudget(
            sector.Rooms.Count,
            blueprint.DifficultyTier,
            blueprint.BiomeId);
        
        _logger.Information(
            "Global budget calculated: Enemies={Enemies}, Hazards={Hazards}",
            globalBudget.TotalEnemyBudget,
            globalBudget.TotalHazardBudget);
        
        // Step 2: Classify room densities
        var densityMap = _classificationService.ClassifyRooms(sector.Rooms, rng);
        
        // Step 3: Distribute budget across rooms
        var populationPlan = _distributionService.DistributeBudget(
            globalBudget,
            densityMap,
            rng);
        
        // Step 4: Populate rooms using allocated budgets (MODIFIED v0.11)
        PopulateRoomsWithBudgets(sector, populationPlan, rng);
        
        // Step 5: Generate threat heatmap (for debugging)
        var heatmap = _heatmapService.GenerateHeatmap(sector, populationPlan);
        LogHeatmap(heatmap);
        
        return sector;
    }
    
    private void PopulateRoomsWithBudgets(
        Sector sector,
        SectorPopulationPlan plan,
        Random rng)
    {
        foreach (var room in sector.Rooms)
        {
            if (!plan.RoomAllocations.TryGetValue([room.Id](http://room.Id), out var allocation))
                continue;
            
            // OLD: Calculate per-room budget
            // var budget = CalculateSpawnBudget(room);  // REMOVED
            
            // NEW: Use allocated budget from plan
            var enemyBudget = allocation.AllocatedEnemies;
            var hazardBudget = allocation.AllocatedHazards;
            var lootBudget = allocation.AllocatedLoot;
            
            // Spawn enemies up to budget
            SpawnEnemies(room, enemyBudget, rng);
            
            // Spawn hazards up to budget
            SpawnHazards(room, hazardBudget, rng);
            
            // Spawn loot up to budget
            SpawnLoot(room, lootBudget, rng);
            
            _logger.Debug(
                "Room populated: {RoomId}, Enemies={Enemies}, Hazards={Hazards}, Loot={Loot}",
                [room.Id](http://room.Id), enemyBudget, hazardBudget, lootBudget);
        }
    }
}
```

---

## VII. Database Schema

```sql
-- =====================================================
-- SECTOR POPULATION BUDGET
-- =====================================================

CREATE TABLE Sector_Population_Budget (
    budget_id INTEGER PRIMARY KEY AUTOINCREMENT,
    sector_id INTEGER NOT NULL,
    
    -- Global budgets
    total_enemy_budget INTEGER NOT NULL,
    total_hazard_budget INTEGER NOT NULL,
    total_loot_budget INTEGER NOT NULL,
    
    -- Actual spawned counts
    enemies_spawned INTEGER DEFAULT 0,
    hazards_spawned INTEGER DEFAULT 0,
    loot_spawned INTEGER DEFAULT 0,
    
    -- Metadata
    difficulty_tier TEXT,
    biome_id TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    CHECK (enemies_spawned <= total_enemy_budget),
    CHECK (hazards_spawned <= total_hazard_budget),
    CHECK (loot_spawned <= total_loot_budget)
);

CREATE INDEX idx_budget_sector ON Sector_Population_Budget(sector_id);

-- =====================================================
-- ROOM DENSITY CLASSIFICATION
-- =====================================================

ALTER TABLE Rooms ADD COLUMN density_classification TEXT; -- Empty, Light, Medium, Heavy, Boss
ALTER TABLE Rooms ADD COLUMN allocated_enemy_budget INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN allocated_hazard_budget INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN allocated_loot_budget INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN actual_enemies_spawned INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN actual_hazards_spawned INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN actual_loot_spawned INTEGER DEFAULT 0;

CREATE INDEX idx_rooms_density ON Rooms(density_classification);

-- =====================================================
-- THREAT HEATMAP (for debugging/analytics)
-- =====================================================

CREATE TABLE Threat_Heatmap (
    heatmap_id INTEGER PRIMARY KEY AUTOINCREMENT,
    sector_id INTEGER NOT NULL,
    room_id INTEGER NOT NULL,
    
    -- Threat counts
    total_threats INTEGER DEFAULT 0,
    enemy_count INTEGER DEFAULT 0,
    hazard_count INTEGER DEFAULT 0,
    
    -- Intensity classification
    threat_intensity TEXT, -- None, Low, Medium, High, Extreme
    
    -- Position (for spatial heatmap visualization)
    coord_x INTEGER,
    coord_y INTEGER,
    coord_z INTEGER,
    
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (room_id) REFERENCES Rooms(room_id) ON DELETE CASCADE,
    CHECK (threat_intensity IN ('None', 'Low', 'Medium', 'High', 'Extreme'))
);

CREATE INDEX idx_heatmap_sector ON Threat_Heatmap(sector_id);
CREATE INDEX idx_heatmap_intensity ON Threat_Heatmap(threat_intensity);
```

---

## VIII. Testing Requirements

### Unit Tests (Target: 85%+ Coverage)

```csharp
[TestClass]
public class ContentDensityServiceTests
{
    [TestMethod]
    public void CalculateGlobalBudget_7RoomsNormal_Returns15Enemies()
    {
        // Arrange
        var service = new ContentDensityService(Mock.Of<ILogger>());
        
        // Act
        var budget = service.CalculateGlobalBudget(
            roomCount: 7,
            DifficultyTier.Normal,
            "the_roots");
        
        // Assert
        Assert.AreEqual(15, budget.TotalEnemyBudget);  // 7 × 2.2 = 15.4 → 15
        Assert.AreEqual(10, budget.TotalHazardBudget); // 7 × 1.5 = 10.5 → 10
    }
    
    [TestMethod]
    public void CalculateGlobalBudget_HardDifficulty_Increases30Percent()
    {
        // Arrange
        var service = new ContentDensityService(Mock.Of<ILogger>());
        
        // Act
        var normal = service.CalculateGlobalBudget(7, DifficultyTier.Normal, "the_roots");
        var hard = service.CalculateGlobalBudget(7, DifficultyTier.Hard, "the_roots");
        
        // Assert
        Assert.AreEqual((int)(normal.TotalEnemyBudget * 1.3f), hard.TotalEnemyBudget);
    }
}

[TestClass]
public class DensityClassificationServiceTests
{
    [TestMethod]
    public void ClassifyRooms_7Rooms_FollowsTargetDistribution()
    {
        // Arrange
        var service = new DensityClassificationService();
        var rooms = CreateTestRooms(7);
        
        // Act
        var classifications = service.ClassifyRooms(rooms, new Random(12345));
        
        // Assert - Should have variety
        var densityCounts = classifications.Values
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());
        
        Assert.IsTrue(densityCounts.ContainsKey(RoomDensity.Empty));
        Assert.IsTrue(densityCounts.ContainsKey(RoomDensity.Light));
        Assert.IsTrue(densityCounts.ContainsKey(RoomDensity.Medium));
    }
    
    [TestMethod]
    public void ClassifyRooms_BossRoom_AlwaysBossDensity()
    {
        // Arrange
        var service = new DensityClassificationService();
        var rooms = new List<Room>
        {
            new Room { Archetype = RoomArchetype.BossArena }
        };
        
        // Act
        var classifications = service.ClassifyRooms(rooms, new Random());
        
        // Assert
        Assert.AreEqual(RoomDensity.Boss, classifications[rooms[0]]);
    }
}

[TestClass]
public class BudgetDistributionServiceTests
{
    [TestMethod]
    public void DistributeBudget_BossRoomGets25PercentOfBudget()
    {
        // Arrange
        var service = new BudgetDistributionService(Mock.Of<ILogger>());
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 20,
            TotalHazardBudget = 10
        };
        var densityMap = new Dictionary<Room, RoomDensity>
        {
            { new Room { Id = "boss" }, RoomDensity.Boss },
            { new Room { Id = "room1" }, RoomDensity.Light }
        };
        
        // Act
        var plan = service.DistributeBudget(budget, densityMap, new Random());
        
        // Assert
        var bossAllocation = plan.RoomAllocations["boss"];
        Assert.IsTrue(bossAllocation.AllocatedEnemies >= 4);  // ~25% of 20
        Assert.IsTrue(bossAllocation.AllocatedEnemies <= 6);
    }
}
```

---

## IX. Success Criteria

**v0.39.3 is DONE when:**

- [ ]  **Global Budget Calculation:**
    - [ ]  Budget formula implemented
    - [ ]  Difficulty multipliers correct
    - [ ]  Biome multipliers applied
    - [ ]  Budgets reasonable (2.0-2.5 enemies/room avg)
- [ ]  **Density Classification:**
    - [ ]  5 density types implemented
    - [ ]  Target distribution followed (10-15% Empty, 40-50% Light, etc.)
    - [ ]  Boss rooms always Boss density
    - [ ]  Entry halls always Light density
    - [ ]  Secret rooms always Empty density
- [ ]  **Budget Distribution:**
    - [ ]  Boss rooms get 20-30% of budget
    - [ ]  Heavy rooms get 15-20% of budget
    - [ ]  Distribution algorithm respects density types
    - [ ]  Budget not exceeded
- [ ]  **Threat Heatmap:**
    - [ ]  Heatmap generation functional
    - [ ]  Intensity classification correct
    - [ ]  Statistics calculated
    - [ ]  Debug visualization available
- [ ]  **Integration with v0.11:**
    - [ ]  Population pipeline modified
    - [ ]  Per-room budgets replaced with allocated budgets
    - [ ]  Spawning respects allocated limits
    - [ ]  No over-population
- [ ]  **Database Schema:**
    - [ ]  Sector_Population_Budget table created
    - [ ]  Room density columns added
    - [ ]  Threat_Heatmap table created
    - [ ]  Indexes created
- [ ]  **Testing:**
    - [ ]  85%+ unit test coverage
    - [ ]  15+ unit tests passing
    - [ ]  Budget calculations validated
    - [ ]  Distribution algorithm tested
- [ ]  **Validation:**
    - [ ]  Generated sectors have variety
    - [ ]  10-15% of rooms are Empty
    - [ ]  Boss rooms have most threats
    - [ ]  Average threats/room is 2-3 (not 5-7)
- [ ]  **Playtesting:**
    - [ ]  20+ sectors generated
    - [ ]  Pacing feels good
    - [ ]  Breather rooms appreciated
    - [ ]  No exhaustion from over-dense sectors

---

## X. Timeline

**Week 1: Global Budget System (6-8 hours)**

- GlobalBudget model
- Budget calculation algorithm
- Difficulty/biome multipliers
- ContentDensityService

**Week 2: Density Classification (5-7 hours)**

- RoomDensity enum
- Classification algorithm
- Target distribution logic
- DensityClassificationService

**Week 3: Budget Distribution (5-7 hours)**

- Distribution algorithm
- Priority-based allocation
- Boss/Heavy/Medium/Light allocation
- BudgetDistributionService

**Week 4: Integration & Testing (6-8 hours)**

- v0.11 population integration
- Threat heatmap generation
- Unit tests (15+)
- Validation and playtesting

**Total: 18-25 hours (3-4 weeks part-time)**

---

**Ready to solve the over-population problem with intelligent density management.**