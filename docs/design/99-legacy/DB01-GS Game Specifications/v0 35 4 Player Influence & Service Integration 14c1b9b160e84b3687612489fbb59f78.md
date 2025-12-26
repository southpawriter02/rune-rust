# v0.35.4: Player Influence & Service Integration

Type: Technical
Description: TerritoryService orchestration layer, player territorial action tracking, influence calculation from player actions, complete service integration with v0.33/v0.34/v0.14/v0.10-v0.12. 8-9 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.35.1 (Territory Database), v0.35.2 (Territory Mechanics), v0.35.3 (World Events)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.35: Territory Control & Dynamic World (v0%2035%20Territory%20Control%20&%20Dynamic%20World%2029703a131b8c4c4699182fcc43d99a22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.35.4-PLAYER-INFLUENCE

**Parent Specification:** [v0.35: Territory Control & Dynamic World](v0%2035%20Territory%20Control%20&%20Dynamic%20World%2029703a131b8c4c4699182fcc43d99a22.md)

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-9 hours

**Prerequisites:** v0.35.1 (Territory Database), v0.35.2 (Territory Mechanics), v0.35.3 (World Events)

---

## I. Executive Summary

v0.35.4 provides the **final integration layer for Territory Control**:

- **TerritoryService** — Top-level orchestration service
- **Player influence tracking** — Log and calculate impact of player actions
- **Complete service integration** — Wire together all v0.35 components
- **Comprehensive testing** — 15+ unit tests for full system coverage
- **Performance optimization** — Caching and query optimization

This specification completes the Territory Control system by connecting player actions to world consequences.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.35.4)

- TerritoryService orchestration layer
- Player territorial action tracking and recording
- Influence calculation from player actions
- Integration with v0.33 (Faction System)
- Integration with v0.34 (Companion System)
- Integration with v0.14 (Quest System)
- Integration with v0.10-v0.12 (Procedural Generation)
- Complete unit test suite (15+ tests, 85%+ coverage)
- Performance optimization (caching, indexes)
- Serilog structured logging throughout
- Deployment instructions and verification

### ❌ Explicitly Out of Scope

- UI components (separate phase)
- Advanced analytics (territorial heatmaps) (defer to v2.0+)
- Player-founded factions (defer to v2.0+)
- Faction diplomacy (defer to v2.0+)
- Territory mini-map overlay (defer to UI phase)
- Historical territory tracking (defer to v2.0+)

---

## III. TerritoryService (Orchestration Layer)

**Purpose:** Top-level service that coordinates all territory control functionality.

```csharp
public class TerritoryService
{
    private readonly IDbConnection _db;
    private readonly ILogger<TerritoryService> _logger;
    private readonly TerritoryControlService _controlService;
    private readonly FactionWarService _warService;
    private readonly WorldEventService _eventService;
    private readonly FactionService _factionService;
    private readonly QuestService _questService;
    
    // Caching for frequently accessed data
    private readonly MemoryCache _sectorControlCache;
    private const int CACHE_DURATION_SECONDS = 300; // 5 minutes
    
    public TerritoryService(
        IDbConnection db,
        ILogger<TerritoryService> logger,
        TerritoryControlService controlService,
        FactionWarService warService,
        WorldEventService eventService,
        FactionService factionService,
        QuestService questService)
    {
        _db = db;
        _logger = logger;
        _controlService = controlService;
        _warService = warService;
        _eventService = eventService;
        _factionService = factionService;
        _questService = questService;
        _sectorControlCache = new MemoryCache(new MemoryCacheOptions());
    }
    
    /// <summary>
    /// Record player action that affects territorial influence.
    /// </summary>
    public async Task RecordPlayerAction(
        int characterId,
        int sectorId,
        string actionType,
        string affectedFaction,
        string notes = null)
    {
        using var operation = _logger.BeginTimedOperation(
            "Record player territorial action: {ActionType} by character {CharacterId} in sector {SectorId}",
            actionType, characterId, sectorId);
        
        try
        {
            // Calculate influence delta based on action type and reputation
            double influenceDelta = await CalculateInfluenceDelta(
                characterId, 
                sectorId, 
                actionType, 
                affectedFaction);
            
            // Record action in database
            await _db.ExecuteAsync(@"
                INSERT INTO Player_Territorial_Actions (
                    character_id, world_id, sector_id, action_type, 
                    affected_faction, influence_delta, notes
                )
                VALUES (
                    @CharacterId, 1, @SectorId, @ActionType, 
                    @AffectedFaction, @InfluenceDelta, @Notes
                )",
                new 
                { 
                    CharacterId = characterId,
                    SectorId = sectorId,
                    ActionType = actionType,
                    AffectedFaction = affectedFaction,
                    InfluenceDelta = influenceDelta,
                    Notes = notes
                });
            
            // Apply influence shift
            await _controlService.ShiftInfluence(
                sectorId, 
                affectedFaction, 
                influenceDelta,
                $"Player action: {actionType}");
            
            // Check if action triggers war
            var controlState = await _controlService.CalculateSectorControlState(sectorId);
            if (controlState.State == "Contested")
            {
                await _warService.CheckWarTrigger(sectorId);
            }
            
            // Check if action affects active war
            var activeWar = await _warService.GetActiveWarForSector(sectorId);
            if (activeWar != null)
            {
                // Determine which side benefits
                double warBalanceShift = CalculateWarBalanceShift(
                    activeWar, 
                    affectedFaction, 
                    influenceDelta);
                
                await _warService.AdvanceWar(
                    activeWar.WarId, 
                    warBalanceShift, 
                    $"Player action: {actionType}");
            }
            
            // Invalidate cache
            _sectorControlCache.Remove($"control_state_{sectorId}");
            
            _logger.Information(
                "Player action recorded: {ActionType} by character {CharacterId} " +
                "affected {Faction} by {Delta} in sector {SectorId}",
                actionType, characterId, affectedFaction, influenceDelta, sectorId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, 
                "Failed to record player action {ActionType} for character {CharacterId}",
                actionType, characterId);
            throw;
        }
    }
    
    /// <summary>
    /// Calculate influence delta from player action.
    /// </summary>
    private async Task<double> CalculateInfluenceDelta(
        int characterId,
        int sectorId,
        string actionType,
        string factionName)
    {
        // Base influence values per action type
        double baseInfluence = actionType switch
        {
            "Kill_Enemy" => 0.5,
            "Complete_Quest" => 5.0,
            "Defend_Territory" => 5.0,
            "Sabotage" => -5.0,
            "Escort_Caravan" => 4.0,
            "Destroy_Hazard" => 2.0,
            "Activate_Artifact" => 6.0,
            _ => 1.0
        };
        
        // Get reputation multiplier
        var reputation = await _factionService.GetReputation(characterId, factionName);
        double reputationMultiplier = 1.0 + (reputation / 200.0); // -100 to +100 → 0.5x to 1.5x
        reputationMultiplier = Math.Clamp(reputationMultiplier, 0.5, 1.5);
        
        // Check if sector is player's "home territory" (highest reputation faction controls it)
        var dominantFaction = await _controlService.GetDominantFaction(sectorId);
        double territoryBonus = dominantFaction == factionName ? 1.2 : 1.0;
        
        double finalInfluence = baseInfluence * reputationMultiplier * territoryBonus;
        
        _logger.Debug(
            "Influence calculation: base={Base}, reputation_mult={RepMult}, " +
            "territory_bonus={TerBonus}, final={Final}",
            baseInfluence, reputationMultiplier, territoryBonus, finalInfluence);
        
        return Math.Clamp(finalInfluence, -10.0, 10.0);
    }
    
    /// <summary>
    /// Calculate war balance shift from player action.
    /// </summary>
    private double CalculateWarBalanceShift(
        FactionWar war,
        string affectedFaction,
        double influenceDelta)
    {
        // Determine which side the action benefits
        if (affectedFaction == war.FactionA)
        {
            return influenceDelta * 2.0; // War actions have double impact
        }
        else if (affectedFaction == war.FactionB)
        {
            return -influenceDelta * 2.0; // Negative for faction_b
        }
        
        return 0.0; // Action doesn't affect war participants
    }
    
    /// <summary>
    /// Get complete territory status for a sector (cached).
    /// </summary>
    public async Task<TerritoryStatus> GetSectorTerritoryStatus(int sectorId)
    {
        string cacheKey = $"control_state_{sectorId}";
        
        if (_sectorControlCache.TryGetValue(cacheKey, out TerritoryStatus cachedStatus))
        {
            _logger.Debug("Cache hit for sector {SectorId} territory status", sectorId);
            return cachedStatus;
        }
        
        using var operation = _logger.BeginTimedOperation(
            "Get territory status for sector {SectorId}",
            sectorId);
        
        try
        {
            var controlState = await _controlService.CalculateSectorControlState(sectorId);
            var influences = await _controlService.GetSectorInfluences(sectorId);
            var activeWar = await _warService.GetActiveWarForSector(sectorId);
            var activeEvents = await _eventService.GetActiveSectorEvents(sectorId);
            
            var status = new TerritoryStatus
            {
                SectorId = sectorId,
                ControlState = controlState.State,
                DominantFaction = controlState.DominantFaction,
                ContestedFactions = controlState.ContestedFactions,
                FactionInfluences = influences,
                ActiveWar = activeWar,
                ActiveEvents = activeEvents
            };
            
            // Cache for 5 minutes
            _sectorControlCache.Set(
                cacheKey, 
                status, 
                TimeSpan.FromSeconds(CACHE_DURATION_SECONDS));
            
            return status;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get territory status for sector {SectorId}", sectorId);
            throw;
        }
    }
    
    /// <summary>
    /// Process daily territory updates (wars, events, influence decay).
    /// </summary>
    public async Task ProcessDailyTerritoryUpdate()
    {
        using var operation = _logger.BeginTimedOperation("Process daily territory update");
        
        try
        {
            _logger.Information("Starting daily territory update");
            
            // Process all active wars
            var activeWars = await _warService.GetActiveWars();
            foreach (var war in activeWars)
            {
                // Check for time-based resolution
                var warDuration = ([DateTime.Now](http://DateTime.Now) - war.WarStartDate).Days;
                if (warDuration >= 15) // Max duration
                {
                    string victor = war.WarBalance > 0 ? war.FactionA : war.FactionB;
                    await _warService.ResolveWar(war.WarId, victor);
                }
            }
            
            // Process all active events
            await _eventService.ProcessDailyEvents();
            
            // Check for new war triggers in all contested sectors
            var contestedSectors = await GetContestedSectors();
            foreach (var sectorId in contestedSectors)
            {
                await _warService.CheckWarTrigger(sectorId);
            }
            
            // Generate random events
            await _eventService.GenerateRandomEvents();
            
            // Clear cache
            _sectorControlCache.Clear();
            
            _logger.Information(
                "Daily territory update complete: {WarCount} wars, {SectorCount} contested sectors",
                activeWars.Count, contestedSectors.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to process daily territory update");
            throw;
        }
    }
    
    /// <summary>
    /// Get player's total influence contribution per faction.
    /// </summary>
    public async Task<Dictionary<string, double>> GetPlayerTotalInfluence(int characterId)
    {
        var influences = await _db.QueryAsync<(string faction, double total)>(@"
            SELECT affected_faction, SUM(influence_delta) as total
            FROM Player_Territorial_Actions
            WHERE character_id = @CharacterId
            GROUP BY affected_faction
            ORDER BY total DESC",
            new { CharacterId = characterId });
        
        return influences.ToDictionary(i => i.faction, i => [i.total](http://i.total));
    }
    
    /// <summary>
    /// Get all contested sectors.
    /// </summary>
    private async Task<List<int>> GetContestedSectors()
    {
        return (await _db.QueryAsync<int>(@"
            SELECT DISTINCT sector_id
            FROM Faction_Territory_Control
            WHERE control_state = 'Contested'")).ToList();
    }
}
```

---

## IV. Integration with Existing Systems

### v0.33 Faction System Integration

```csharp
public class FactionTerritoryIntegration
{
    /// <summary>
    /// Reputation gains modified by territory control.
    /// </summary>
    public async Task<int> CalculateReputationGain(
        int characterId,
        string factionName,
        int baseReputation,
        int sectorId)
    {
        var dominantFaction = await _territoryService.GetDominantFaction(sectorId);
        
        // Bonus reputation if helping faction in their home territory
        double multiplier = dominantFaction == factionName ? 1.5 : 1.0;
        
        return (int)(baseReputation * multiplier);
    }
    
    /// <summary>
    /// Faction reputation affects territorial influence power.
    /// </summary>
    public double GetInfluencePowerMultiplier(int reputation)
    {
        // -100 to +100 reputation → 0.5x to 1.5x influence power
        return Math.Clamp(1.0 + (reputation / 200.0), 0.5, 1.5);
    }
}
```

### v0.34 NPC Companion Integration

```csharp
public class CompanionTerritoryReactions
{
    /// <summary>
    /// Companions react to entering faction-controlled territory.
    /// </summary>
    public async Task OnEnterSector(Companion companion, int sectorId)
    {
        var status = await _territoryService.GetSectorTerritoryStatus(sectorId);
        
        if (companion.FactionAffiliation == status.DominantFaction)
        {
            // Friendly territory - morale boost
            companion.ApplyBuff("Home_Territory_Morale", duration: 10, value: 10);
            companion.Dialogue = "Good to be among allies.";
        }
        else if (IsHostileFaction(companion.FactionAffiliation, status.DominantFaction))
        {
            // Hostile territory - stress
            await companion.ApplyPsychicStress(5, "Hostile Territory");
            companion.Dialogue = "We should be careful here. Not our friends.";
        }
    }
}
```

### v0.14 Quest System Integration

```csharp
public class TerritorialQuestGeneration
{
    /// <summary>
    /// Generate faction-appropriate quests based on territory control.
    /// </summary>
    public async Task<List<Quest>> GenerateTerritorialQuests(int sectorId)
    {
        var status = await _territoryService.GetSectorTerritoryStatus(sectorId);
        var quests = new List<Quest>();
        
        if (status.ControlState == "Contested")
        {
            // Generate "Support Faction" quests for top 2 factions
            foreach (var faction in status.ContestedFactions)
            {
                quests.Add(await _questService.CreateQuest(new QuestDefinition
                {
                    QuestName = $"Support {faction} Expansion",
                    Description = $"Help {faction} gain control of this contested sector.",
                    FactionAffiliation = faction,
                    RewardReputation = 15,
                    ObjectiveType = "TerritorialControl"
                }));
            }
        }
        else if (status.ControlState == "War")
        {
            // Generate war effort quests
            if (status.ActiveWar != null)
            {
                quests.Add(await _questService.CreateQuest(new QuestDefinition
                {
                    QuestName = $"The {status.ActiveWar.FactionA} War Effort",
                    Description = "Support the war effort by eliminating enemy combatants.",
                    FactionAffiliation = status.ActiveWar.FactionA,
                    ObjectiveType = "KillEnemies",
                    ObjectiveCount = 10,
                    RewardReputation = 25
                }));
            }
        }
        
        return quests;
    }
}
```

### v0.10-v0.12 Procedural Generation Integration

```csharp
public class TerritoryInfluencedGeneration
{
    /// <summary>
    /// Modify sector generation based on controlling faction.
    /// </summary>
    public async Task<SectorGenerationParams> GetTerritorialGenerationParams(int sectorId)
    {
        var status = await _territoryService.GetSectorTerritoryStatus(sectorId);
        
        var baseParams = new SectorGenerationParams();
        
        // Faction-specific modifications
        switch (status.DominantFaction)
        {
            case "Iron-Banes":
                baseParams.EnemyFactionFilter = "Undying";
                baseParams.EnemyDensityMultiplier = 1.5; // More Undying patrols
                baseParams.LootTableModifier = "Anti-Undying_Gear";
                break;
                
            case "Jötun-Readers":
                baseParams.ArtifactSpawnRate = 1.3; // +30% artifacts
                baseParams.ScholarNPCChance = 0.15; // 15% scholar NPCs
                baseParams.EnvironmentalStorytelling = "Pre-Glitch_History";
                break;
                
            case "God-Sleeper Cultists":
                baseParams.EnemyFactionFilter = "Jötun-Forged";
                baseParams.EnemyDensityMultiplier = 1.4; // +40% constructs
                baseParams.HazardDensityMultiplier = 1.25; // Reality distortions
                break;
                
            case "Rust-Clans":
                baseParams.SalvageMaterialRate = 1.5; // +50% salvage
                baseParams.MerchantPriceModifier = 0.85; // -15% prices
                baseParams.ScavengerNPCChance = 0.20; // 20% scavenger NPCs
                break;
                
            case "Independents":
                baseParams.EnemyVarietyMultiplier = 1.5; // More diverse encounters
                baseParams.NeutralNPCChance = 0.30; // 30% neutral NPCs
                break;
        }
        
        // War zone modifications
        if (status.ControlState == "War" && status.ActiveWar != null)
        {
            baseParams.HazardDensityMultiplier *= (1.0 + status.ActiveWar.CollateralDamage / 100.0);
            baseParams.AmbientDescription = "War-torn sector with signs of recent conflict.";
        }
        
        return baseParams;
    }
}
```

---

## V. Data Models

```csharp
public class TerritoryStatus
{
    public int SectorId { get; set; }
    public string ControlState { get; set; } // "Stable", "Contested", "War", "Independent", "Ruined"
    public string DominantFaction { get; set; }
    public string[] ContestedFactions { get; set; }
    public List<FactionInfluence> FactionInfluences { get; set; }
    public FactionWar ActiveWar { get; set; }
    public List<WorldEvent> ActiveEvents { get; set; }
}

public class PlayerTerritorialAction
{
    public int ActionId { get; set; }
    public int CharacterId { get; set; }
    public int SectorId { get; set; }
    public string ActionType { get; set; }
    public string AffectedFaction { get; set; }
    public double InfluenceDelta { get; set; }
    public DateTime ActionTimestamp { get; set; }
    public string Notes { get; set; }
}

public class SectorGenerationParams
{
    public string EnemyFactionFilter { get; set; }
    public double EnemyDensityMultiplier { get; set; } = 1.0;
    public double EnemyVarietyMultiplier { get; set; } = 1.0;
    public double HazardDensityMultiplier { get; set; } = 1.0;
    public double ArtifactSpawnRate { get; set; } = 1.0;
    public double SalvageMaterialRate { get; set; } = 1.0;
    public string LootTableModifier { get; set; }
    public double ScholarNPCChance { get; set; } = 0.0;
    public double ScavengerNPCChance { get; set; } = 0.0;
    public double NeutralNPCChance { get; set; } = 0.0;
    public double MerchantPriceModifier { get; set; } = 1.0;
    public string EnvironmentalStorytelling { get; set; }
    public string AmbientDescription { get; set; }
}
```

---

## VI. Unit Tests

```csharp
[TestClass]
public class TerritoryServiceTests
{
    private Mock<IDbConnection> _dbMock;
    private Mock<ILogger<TerritoryService>> _loggerMock;
    private Mock<TerritoryControlService> _controlServiceMock;
    private Mock<FactionWarService> _warServiceMock;
    private Mock<WorldEventService> _eventServiceMock;
    private Mock<FactionService> _factionServiceMock;
    private Mock<QuestService> _questServiceMock;
    private TerritoryService _service;
    
    [TestInitialize]
    public void Setup()
    {
        _dbMock = new Mock<IDbConnection>();
        _loggerMock = new Mock<ILogger<TerritoryService>>();
        _controlServiceMock = new Mock<TerritoryControlService>();
        _warServiceMock = new Mock<FactionWarService>();
        _eventServiceMock = new Mock<WorldEventService>();
        _factionServiceMock = new Mock<FactionService>();
        _questServiceMock = new Mock<QuestService>();
        
        _service = new TerritoryService(
            _dbMock.Object,
            _loggerMock.Object,
            _controlServiceMock.Object,
            _warServiceMock.Object,
            _eventServiceMock.Object,
            _factionServiceMock.Object,
            _questServiceMock.Object);
    }
    
    [TestMethod]
    public async Task RecordPlayerAction_CompleteQuest_AppliesInfluence()
    {
        // Arrange
        int characterId = 1;
        int sectorId = 3;
        string actionType = "Complete_Quest";
        string faction = "Jötun-Readers";
        
        _factionServiceMock.Setup(f => f.GetReputation(characterId, faction))
            .ReturnsAsync(50); // +50 reputation → 1.25x multiplier
        
        _controlServiceMock.Setup(c => c.GetDominantFaction(sectorId))
            .ReturnsAsync(faction); // Home territory → 1.2x bonus
        
        // Expected: 5.0 * 1.25 * 1.2 = 7.5
        
        // Act
        await _service.RecordPlayerAction(characterId, sectorId, actionType, faction);
        
        // Assert
        _dbMock.Verify(
            db => db.ExecuteAsync(
                [It.Is](http://It.Is)<string>(sql => sql.Contains("INSERT INTO Player_Territorial_Actions")),
                [It.Is](http://It.Is)<object>(p => 
                    Math.Abs((double)p.GetType().GetProperty("InfluenceDelta").GetValue(p) - 7.5) < 0.1)),
            Times.Once);
        
        _controlServiceMock.Verify(
            c => c.ShiftInfluence(sectorId, faction, It.IsInRange(7.0, 8.0, Range.Inclusive), It.IsAny<string>()),
            Times.Once);
    }
    
    [TestMethod]
    public async Task RecordPlayerAction_TriggersWarCheck_WhenContested()
    {
        // Arrange
        int characterId = 1;
        int sectorId = 3;
        
        _controlServiceMock.Setup(c => c.CalculateSectorControlState(sectorId))
            .ReturnsAsync(new SectorControlState { State = "Contested" });
        
        // Act
        await _service.RecordPlayerAction(characterId, sectorId, "Kill_Enemy", "Iron-Banes");
        
        // Assert
        _warServiceMock.Verify(
            w => w.CheckWarTrigger(sectorId),
            Times.Once,
            "Should check for war trigger when sector is contested");
    }
    
    [TestMethod]
    public async Task RecordPlayerAction_ShiftsWarBalance_WhenWarActive()
    {
        // Arrange
        int characterId = 1;
        int sectorId = 3;
        var activeWar = new FactionWar
        {
            WarId = 1,
            SectorId = sectorId,
            FactionA = "Jötun-Readers",
            FactionB = "Rust-Clans",
            WarBalance = 10.0,
            IsActive = true
        };
        
        _warServiceMock.Setup(w => w.GetActiveWarForSector(sectorId))
            .ReturnsAsync(activeWar);
        
        _factionServiceMock.Setup(f => f.GetReputation(characterId, "Jötun-Readers"))
            .ReturnsAsync(0);
        
        // Act
        await _service.RecordPlayerAction(characterId, sectorId, "Complete_Quest", "Jötun-Readers");
        
        // Assert
        _warServiceMock.Verify(
            w => w.AdvanceWar(
                activeWar.WarId, 
                [It.Is](http://It.Is)<double>(shift => shift > 0), // Positive shift for faction_a
                It.IsAny<string>()),
            Times.Once);
    }
    
    [TestMethod]
    public async Task GetSectorTerritoryStatus_UsesCache_OnSecondCall()
    {
        // Arrange
        int sectorId = 3;
        
        _controlServiceMock.Setup(c => c.CalculateSectorControlState(sectorId))
            .ReturnsAsync(new SectorControlState { State = "Stable" });
        
        // Act
        var status1 = await _service.GetSectorTerritoryStatus(sectorId);
        var status2 = await _service.GetSectorTerritoryStatus(sectorId);
        
        // Assert
        _controlServiceMock.Verify(
            c => c.CalculateSectorControlState(sectorId),
            Times.Once,
            "Should only calculate once, second call uses cache");
    }
    
    [TestMethod]
    public async Task ProcessDailyTerritoryUpdate_ResolvesExpiredWars()
    {
        // Arrange
        var expiredWar = new FactionWar
        {
            WarId = 1,
            SectorId = 3,
            FactionA = "Iron-Banes",
            FactionB = "Rust-Clans",
            WarStartDate = [DateTime.Now](http://DateTime.Now).AddDays(-16), // 16 days old
            WarBalance = 20.0,
            IsActive = true
        };
        
        _warServiceMock.Setup(w => w.GetActiveWars())
            .ReturnsAsync(new List<FactionWar> { expiredWar });
        
        // Act
        await _service.ProcessDailyTerritoryUpdate();
        
        // Assert
        _warServiceMock.Verify(
            w => w.ResolveWar(expiredWar.WarId, "Iron-Banes"),
            Times.Once,
            "Should resolve war that exceeded 15-day limit");
    }
    
    [TestMethod]
    public async Task GetPlayerTotalInfluence_AggregatesCorrectly()
    {
        // Arrange
        int characterId = 1;
        var mockInfluences = new List<(string faction, double total)>
        {
            ("Iron-Banes", 45.5),
            ("Jötun-Readers", 32.0),
            ("Rust-Clans", -10.0) // Sabotage
        };
        
        _dbMock.Setup(db => db.QueryAsync<(string, double)>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(mockInfluences);
        
        // Act
        var result = await _service.GetPlayerTotalInfluence(characterId);
        
        // Assert
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(45.5, result["Iron-Banes"]);
        Assert.AreEqual(32.0, result["Jötun-Readers"]);
        Assert.AreEqual(-10.0, result["Rust-Clans"]);
    }
}

[TestClass]
public class TerritoryIntegrationTests
{
    [TestMethod]
    public async Task FactionReputationBonus_AppliesInHomeTerritory()
    {
        // Test reputation gain bonus when helping faction in their territory
    }
    
    [TestMethod]
    public async Task CompanionReaction_PositiveInHomeTerritory()
    {
        // Test companion morale boost in friendly territory
    }
    
    [TestMethod]
    public async Task TerritorialQuests_GenerateForContestedSectors()
    {
        // Test quest generation in contested sectors
    }
    
    [TestMethod]
    public async Task ProceduralGeneration_ModifiesByControllingFaction()
    {
        // Test generation parameter changes based on faction control
    }
}
```

---

## VII. Performance Optimization

### Caching Strategy

```csharp
// Territory status cached for 5 minutes
private readonly MemoryCache _sectorControlCache;
private const int CACHE_DURATION_SECONDS = 300;

// Cache key format: "control_state_{sectorId}"
// Invalidated on:
// - Player action affecting sector
// - War state change
// - Daily update
```

### Database Indexes (Already in v0.35.1)

- `idx_territory_control_sector` — Fast sector lookups
- `idx_territory_control_faction` — Fast faction queries
- `idx_wars_active` — Fast active war queries
- `idx_player_actions_character` — Fast player history

### Query Optimization

```sql
-- Optimized query: Get dominant faction (uses index)
SELECT faction_name, influence_value
FROM Faction_Territory_Control
WHERE sector_id = ?
ORDER BY influence_value DESC
LIMIT 1;

-- Optimized query: Get contested sectors (uses index)
SELECT DISTINCT sector_id
FROM Faction_Territory_Control
WHERE control_state = 'Contested';
```

---

## VIII. Deployment Instructions

### Step 1: Ensure Prerequisites Deployed

```bash
# Verify v0.35.1 database schema exists
sqlite3 your_database.db "SELECT name FROM sqlite_master WHERE type='table' AND name='Faction_Territory_Control';"

# Verify v0.35.2 services compiled
# Check for TerritoryControlService.cs, FactionWarService.cs

# Verify v0.35.3 services compiled
# Check for WorldEventService.cs
```

### Step 2: Deploy TerritoryService

```bash
# Add TerritoryService.cs to Services/
# Register in dependency injection container

services.AddScoped<TerritoryService>();
```

### Step 3: Wire Up Integration Points

```csharp
// In CombatService.cs
public async Task OnEnemyKilled(int characterId, Enemy enemy, int sectorId)
{
    // Existing kill logic...
    
    // NEW: Record territorial action
    string enemyFaction = enemy.FactionAffiliation;
    string playerFaction = await _factionService.GetHighestReputationFaction(characterId);
    
    if (playerFaction != null)
    {
        await _territoryService.RecordPlayerAction(
            characterId, 
            sectorId, 
            "Kill_Enemy", 
            playerFaction,
            notes: $"Killed {[enemy.Name](http://enemy.Name)} ({enemyFaction})");
    }
}

// In QuestService.cs
public async Task OnQuestCompleted(int characterId, Quest quest)
{
    // Existing quest completion logic...
    
    // NEW: Record territorial action if quest has faction affiliation
    if (!string.IsNullOrEmpty(quest.FactionAffiliation) && quest.SectorId.HasValue)
    {
        await _territoryService.RecordPlayerAction(
            characterId,
            quest.SectorId.Value,
            "Complete_Quest",
            quest.FactionAffiliation,
            notes: $"Completed: {quest.QuestName}");
    }
}
```

### Step 4: Schedule Daily Updates

```csharp
// Add to game loop or scheduled task
public class GameDayScheduler
{
    private readonly TerritoryService _territoryService;
    
    public async Task OnNewGameDay()
    {
        _logger.Information("Processing new game day");
        
        // Process territory updates
        await _territoryService.ProcessDailyTerritoryUpdate();
        
        // Other daily tasks...
    }
}
```

### Step 5: Verification

```csharp
// Test player action recording
var characterId = 1;
var sectorId = 3;
await _territoryService.RecordPlayerAction(
    characterId, sectorId, "Complete_Quest", "Iron-Banes");

// Verify in database
var actions = await _db.QueryAsync(@"
    SELECT * FROM Player_Territorial_Actions
    WHERE character_id = @CharacterId",
    new { CharacterId = characterId });

Console.WriteLine($"Recorded {actions.Count()} actions");

// Test territory status retrieval
var status = await _territoryService.GetSectorTerritoryStatus(sectorId);
Console.WriteLine($"Sector {sectorId} control: {status.ControlState}");
Console.WriteLine($"Dominant faction: {status.DominantFaction}");
```

---

## IX. Success Criteria

**Functional Requirements:**

- ✅ Player actions recorded in Player_Territorial_Actions table
- ✅ Influence calculated with reputation and territory bonuses
- ✅ Player actions shift faction influence
- ✅ Player actions advance wars when active
- ✅ Daily updates process wars and events
- ✅ Territory status cached for performance
- ✅ Complete integration with v0.33, v0.34, v0.14, v0.10-v0.12

**Quality Gates:**

- ✅ 15+ unit tests, 85%+ coverage
- ✅ Serilog structured logging throughout
- ✅ v5.0 compliance (Layer 2 voice, no fantasy language)
- ✅ ASCII-only entity names
- ✅ Performance targets: <50ms territory lookups, <100ms influence calculations

**Integration Validation:**

- ✅ Faction reputation affects influence power
- ✅ Companions react to territory control
- ✅ Quests generated from territory state
- ✅ Procedural generation modified by faction

---

## X. v5.0 Compliance Notes

### Layer 2 Voice (Diagnostic/Clinical)

**✅ Correct Usage:**

- "Territory control orchestration"
- "Player action influence calculation"
- "Faction signal strength modifiers"
- "Integration service architecture"

**❌ Incorrect Usage:**

- ~~"Divine territory blessing"~~
- ~~"Magical faction power"~~
- ~~"Sacred land ownership"~~

---

**Status:** Implementation-ready orchestration and integration layer complete.

**v0.35 Territory Control & Dynamic World system fully specified across 4 child specifications:**

- v0.35.1: Database Schema ✅
- v0.35.2: Territory Mechanics & Wars ✅
- v0.35.3: World Events & Consequences ✅
- v0.35.4: Player Influence & Integration ✅

**Total Implementation Time:** 30-40 hours