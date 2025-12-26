# v0.35.2: Territory Mechanics & Faction Wars

Type: Technical
Description: TerritoryControlService implementation, FactionWarService, control state calculation, war triggering/advancement/resolution, influence calculation engine. 8-11 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.35.1 (Territory Database), v0.33.4 (FactionService)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.35: Territory Control & Dynamic World (v0%2035%20Territory%20Control%20&%20Dynamic%20World%2029703a131b8c4c4699182fcc43d99a22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.35.2-TERRITORY-MECHANICS

**Parent Specification:** [v0.35: Territory Control & Dynamic World](v0%2035%20Territory%20Control%20&%20Dynamic%20World%2029703a131b8c4c4699182fcc43d99a22.md)

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-11 hours

**Prerequisites:** v0.35.1 (Territory Database), v0.33.4 (FactionService)

---

## I. Executive Summary

v0.35.2 implements the **core territory control and faction war mechanics**:

- **TerritoryControlService** — Calculate sector control states and manage influence
- **FactionWarService** — Trigger, advance, and resolve faction wars
- **Influence calculation engine** — Determine how player/NPC actions affect territory
- **War balance mechanics** — Track war progression and determine victors

This specification provides the gameplay systems that make territory control dynamic and responsive to player actions.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.35.2)

- TerritoryControlService implementation
- FactionWarService implementation
- Control state calculation (Stable/Contested/War/Independent/Ruined)
- War triggering conditions
- War advancement logic (daily progression)
- War resolution and victor determination
- Influence shifting from player actions
- Unit test suite (10+ tests, 85%+ coverage)
- Serilog structured logging
- Integration with v0.33 (FactionService)

### ❌ Explicitly Out of Scope

- Dynamic quest generation (defer to v0.35.3)
- World event system (defer to v0.35.3)
- Player influence tracking table writes (defer to v0.35.4)
- UI components (separate phase)
- Multiple simultaneous wars per sector (v0.35 limit: 1 war per sector)
- Faction diplomacy (defer to v2.0+)
- Advanced war tactics (supply lines, sieges) (defer to v2.0+)

---

## III. Service Architecture

### TerritoryControlService

**Purpose:** Manage faction influence and calculate sector control states.

**Core Methods:**

```csharp
public class TerritoryControlService
{
    private readonly IDbConnection _db;
    private readonly ILogger<TerritoryControlService> _logger;
    private readonly FactionService _factionService;
    
    public TerritoryControlService(
        IDbConnection db,
        ILogger<TerritoryControlService> logger,
        FactionService factionService)
    {
        _db = db;
        _logger = logger;
        _factionService = factionService;
    }
    
    /// <summary>
    /// Calculate control state for a sector based on faction influence distribution.
    /// </summary>
    public async Task<SectorControlState> CalculateSectorControlState(int sectorId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Calculate sector control state for {SectorId}", sectorId);
        
        try
        {
            // Get all faction influences for sector
            var influences = await _db.QueryAsync<FactionInfluence>(@"
                SELECT faction_name, influence_value
                FROM Faction_Territory_Control
                WHERE sector_id = @SectorId
                ORDER BY influence_value DESC",
                new { SectorId = sectorId });
            
            if (!influences.Any())
            {
                _logger.Warning("No faction influence data for sector {SectorId}", sectorId);
                return new SectorControlState 
                { 
                    State = "Independent", 
                    DominantFaction = "Independents" 
                };
            }
            
            var topInfluence = influences.First();
            var secondInfluence = influences.Skip(1).FirstOrDefault();
            
            // Check for active war
            var activeWar = await _db.QuerySingleOrDefaultAsync<FactionWar>(@"
                SELECT * FROM Faction_Wars
                WHERE sector_id = @SectorId AND is_active = 1",
                new { SectorId = sectorId });
            
            if (activeWar != null)
            {
                _logger.Information(
                    "Sector {SectorId} in active war between {FactionA} and {FactionB}",
                    sectorId, activeWar.FactionA, activeWar.FactionB);
                
                return new SectorControlState
                {
                    State = "War",
                    DominantFaction = null,
                    ContestedFactions = new[] { activeWar.FactionA, activeWar.FactionB }
                };
            }
            
            // Stable: One faction has 60%+ influence
            if (topInfluence.InfluenceValue >= 60.0)
            {
                _logger.Debug(
                    "Sector {SectorId} stable under {Faction} control ({Influence}%)",
                    sectorId, topInfluence.FactionName, topInfluence.InfluenceValue);
                
                return new SectorControlState
                {
                    State = "Stable",
                    DominantFaction = topInfluence.FactionName
                };
            }
            
            // Contested: Two or more factions have 40-60% influence
            if (secondInfluence != null &&
                topInfluence.InfluenceValue >= 40.0 &&
                secondInfluence.InfluenceValue >= 40.0)
            {
                _logger.Information(
                    "Sector {SectorId} contested between {Faction1} ({Influence1}%) and {Faction2} ({Influence2}%)",
                    sectorId, topInfluence.FactionName, topInfluence.InfluenceValue,
                    secondInfluence.FactionName, secondInfluence.InfluenceValue);
                
                return new SectorControlState
                {
                    State = "Contested",
                    DominantFaction = null,
                    ContestedFactions = new[] 
                    { 
                        topInfluence.FactionName, 
                        secondInfluence.FactionName 
                    }
                };
            }
            
            // Independent: No faction exceeds 40% influence
            _logger.Debug(
                "Sector {SectorId} independent (top faction {Faction} at {Influence}%)",
                sectorId, topInfluence.FactionName, topInfluence.InfluenceValue);
            
            return new SectorControlState
            {
                State = "Independent",
                DominantFaction = "Independents"
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to calculate control state for sector {SectorId}", sectorId);
            throw;
        }
    }
    
    /// <summary>
    /// Get the dominant faction for a sector (faction with highest influence).
    /// </summary>
    public async Task<string> GetDominantFaction(int sectorId)
    {
        var influence = await _db.QuerySingleOrDefaultAsync<FactionInfluence>(@"
            SELECT faction_name, influence_value
            FROM Faction_Territory_Control
            WHERE sector_id = @SectorId
            ORDER BY influence_value DESC
            LIMIT 1",
            new { SectorId = sectorId });
        
        return influence?.FactionName ?? "Independents";
    }
    
    /// <summary>
    /// Shift faction influence in a sector by a delta amount.
    /// </summary>
    public async Task ShiftInfluence(
        int sectorId, 
        string factionName, 
        double influenceDelta,
        string reason)
    {
        using var operation = _logger.BeginTimedOperation(
            "Shift influence for {Faction} in sector {SectorId} by {Delta}",
            factionName, sectorId, influenceDelta);
        
        try
        {
            // Apply influence change
            await _db.ExecuteAsync(@"
                UPDATE Faction_Territory_Control
                SET influence_value = influence_value + @Delta,
                    last_updated = CURRENT_TIMESTAMP
                WHERE sector_id = @SectorId AND faction_name = @FactionName",
                new { SectorId = sectorId, FactionName = factionName, Delta = influenceDelta });
            
            // Normalize influences if sum exceeds 100
            await NormalizeInfluences(sectorId);
            
            // Recalculate control state
            var newState = await CalculateSectorControlState(sectorId);
            
            // Update control_state for all factions in sector
            await _db.ExecuteAsync(@"
                UPDATE Faction_Territory_Control
                SET control_state = @State
                WHERE sector_id = @SectorId",
                new { SectorId = sectorId, State = newState.State });
            
            _logger.Information(
                "Influence shifted: {Faction} in sector {SectorId} by {Delta} ({Reason}). New state: {State}",
                factionName, sectorId, influenceDelta, reason, newState.State);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, 
                "Failed to shift influence for {Faction} in sector {SectorId}",
                factionName, sectorId);
            throw;
        }
    }
    
    /// <summary>
    /// Normalize influences in a sector so sum doesn't exceed 100%.
    /// </summary>
    private async Task NormalizeInfluences(int sectorId)
    {
        var influences = await _db.QueryAsync<FactionInfluence>(@"
            SELECT faction_name, influence_value
            FROM Faction_Territory_Control
            WHERE sector_id = @SectorId",
            new { SectorId = sectorId });
        
        double totalInfluence = influences.Sum(f => f.InfluenceValue);
        
        if (totalInfluence > 100.0)
        {
            _logger.Debug(
                "Normalizing influences in sector {SectorId} (total: {Total}%)",
                sectorId, totalInfluence);
            
            // Scale all influences proportionally
            foreach (var faction in influences)
            {
                double normalized = (faction.InfluenceValue / totalInfluence) * 100.0;
                
                await _db.ExecuteAsync(@"
                    UPDATE Faction_Territory_Control
                    SET influence_value = @Normalized
                    WHERE sector_id = @SectorId AND faction_name = @FactionName",
                    new { SectorId = sectorId, FactionName = faction.FactionName, Normalized = normalized });
            }
        }
    }
    
    /// <summary>
    /// Get all faction influences for a sector.
    /// </summary>
    public async Task<List<FactionInfluence>> GetSectorInfluences(int sectorId)
    {
        return (await _db.QueryAsync<FactionInfluence>(@"
            SELECT faction_name, influence_value, control_state
            FROM Faction_Territory_Control
            WHERE sector_id = @SectorId
            ORDER BY influence_value DESC",
            new { SectorId = sectorId })).ToList();
    }
}
```

---

### FactionWarService

**Purpose:** Manage faction wars including triggering, advancement, and resolution.

**Core Methods:**

```csharp
public class FactionWarService
{
    private readonly IDbConnection _db;
    private readonly ILogger<FactionWarService> _logger;
    private readonly TerritoryControlService _territoryService;
    
    private const double WAR_TRIGGER_THRESHOLD = 45.0; // Both factions need 45%+
    private const int WAR_TRIGGER_DURATION_DAYS = 10; // Contested for 10+ days
    private const double WAR_VICTORY_THRESHOLD = 50.0; // ±50 war_balance
    private const int WAR_MAX_DURATION_DAYS = 15;
    private const double WAR_VICTOR_INFLUENCE_GAIN = 20.0;
    private const double WAR_LOSER_INFLUENCE_LOSS = 20.0;
    private const int WAR_COLLATERAL_DAMAGE_PERCENT = 25; // Hazard density increase
    
    public FactionWarService(
        IDbConnection db,
        ILogger<FactionWarService> logger,
        TerritoryControlService territoryService)
    {
        _db = db;
        _logger = logger;
        _territoryService = territoryService;
    }
    
    /// <summary>
    /// Check if contested sector should escalate to war.
    /// </summary>
    public async Task<bool> CheckWarTrigger(int sectorId)
    {
        using var operation = _logger.BeginTimedOperation(
            "Check war trigger for sector {SectorId}", sectorId);
        
        try
        {
            // Check if already at war
            var existingWar = await _db.QuerySingleOrDefaultAsync<FactionWar>(@"
                SELECT * FROM Faction_Wars
                WHERE sector_id = @SectorId AND is_active = 1",
                new { SectorId = sectorId });
            
            if (existingWar != null)
            {
                _logger.Debug("Sector {SectorId} already at war", sectorId);
                return false;
            }
            
            // Get top two factions
            var influences = await _territoryService.GetSectorInfluences(sectorId);
            
            if (influences.Count < 2)
                return false;
            
            var faction1 = influences[0];
            var faction2 = influences[1];
            
            // Check trigger threshold
            if (faction1.InfluenceValue >= WAR_TRIGGER_THRESHOLD &&
                faction2.InfluenceValue >= WAR_TRIGGER_THRESHOLD)
            {
                _logger.Warning(
                    "War trigger threshold met in sector {SectorId}: {Faction1} ({Influence1}%) vs {Faction2} ({Influence2}%)",
                    sectorId, faction1.FactionName, faction1.InfluenceValue,
                    faction2.FactionName, faction2.InfluenceValue);
                
                // Trigger war
                await InitiateWar(sectorId, faction1.FactionName, faction2.FactionName);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to check war trigger for sector {SectorId}", sectorId);
            throw;
        }
    }
    
    /// <summary>
    /// Initiate a new faction war.
    /// </summary>
    private async Task InitiateWar(int sectorId, string factionA, string factionB)
    {
        using var operation = _logger.BeginTimedOperation(
            "Initiate war in sector {SectorId} between {FactionA} and {FactionB}",
            sectorId, factionA, factionB);
        
        try
        {
            // Create war record
            var warId = await _db.QuerySingleAsync<int>(@"
                INSERT INTO Faction_Wars (
                    world_id, sector_id, faction_a, faction_b, 
                    war_balance, is_active
                )
                VALUES (1, @SectorId, @FactionA, @FactionB, 0.0, 1)
                RETURNING war_id",
                new { SectorId = sectorId, FactionA = factionA, FactionB = factionB });
            
            // Update control state to 'War'
            await _db.ExecuteAsync(@"
                UPDATE Faction_Territory_Control
                SET control_state = 'War'
                WHERE sector_id = @SectorId",
                new { SectorId = sectorId });
            
            _logger.Warning(
                "[WAR INITIATED] War {WarId} started in sector {SectorId}: {FactionA} vs {FactionB}",
                warId, sectorId, factionA, factionB);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, 
                "Failed to initiate war in sector {SectorId}",
                sectorId);
            throw;
        }
    }
    
    /// <summary>
    /// Advance an active war by processing daily progression.
    /// </summary>
    public async Task AdvanceWar(int warId, double balanceShift, string reason)
    {
        using var operation = _logger.BeginTimedOperation(
            "Advance war {WarId} by {Shift} ({Reason})",
            warId, balanceShift, reason);
        
        try
        {
            var war = await _db.QuerySingleOrDefaultAsync<FactionWar>(@"
                SELECT * FROM Faction_Wars WHERE war_id = @WarId",
                new { WarId = warId });
            
            if (war == null || !war.IsActive)
            {
                _logger.Warning("War {WarId} not found or not active", warId);
                return;
            }
            
            // Update war balance
            double newBalance = Math.Clamp(
                war.WarBalance + balanceShift, 
                -100.0, 
                100.0);
            
            await _db.ExecuteAsync(@"
                UPDATE Faction_Wars
                SET war_balance = @Balance
                WHERE war_id = @WarId",
                new { WarId = warId, Balance = newBalance });
            
            _logger.Information(
                "War {WarId} balance shifted from {OldBalance} to {NewBalance} ({Reason})",
                warId, war.WarBalance, newBalance, reason);
            
            // Check for victory condition
            if (Math.Abs(newBalance) >= WAR_VICTORY_THRESHOLD)
            {
                string victor = newBalance > 0 ? war.FactionA : war.FactionB;
                await ResolveWar(warId, victor);
            }
            else
            {
                // Check for time limit
                var warDuration = ([DateTime.Now](http://DateTime.Now) - war.WarStartDate).Days;
                if (warDuration >= WAR_MAX_DURATION_DAYS)
                {
                    _logger.Warning(
                        "War {WarId} reached max duration ({Days} days), forcing resolution",
                        warId, WAR_MAX_DURATION_DAYS);
                    
                    string victor = newBalance > 0 ? war.FactionA : war.FactionB;
                    await ResolveWar(warId, victor);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to advance war {WarId}", warId);
            throw;
        }
    }
    
    /// <summary>
    /// Resolve a war and apply consequences.
    /// </summary>
    public async Task ResolveWar(int warId, string victor)
    {
        using var operation = _logger.BeginTimedOperation(
            "Resolve war {WarId} with victor {Victor}",
            warId, victor);
        
        try
        {
            var war = await _db.QuerySingleOrDefaultAsync<FactionWar>(@"
                SELECT * FROM Faction_Wars WHERE war_id = @WarId",
                new { WarId = warId });
            
            if (war == null)
            {
                _logger.Error("War {WarId} not found", warId);
                return;
            }
            
            string loser = victor == war.FactionA ? war.FactionB : war.FactionA;
            
            // Update war record
            await _db.ExecuteAsync(@"
                UPDATE Faction_Wars
                SET is_active = 0,
                    war_end_date = CURRENT_TIMESTAMP,
                    victor = @Victor,
                    collateral_damage = @Damage
                WHERE war_id = @WarId",
                new { WarId = warId, Victor = victor, Damage = WAR_COLLATERAL_DAMAGE_PERCENT });
            
            // Apply influence changes
            await _territoryService.ShiftInfluence(
                war.SectorId, 
                victor, 
                WAR_VICTOR_INFLUENCE_GAIN,
                $"Victory in war {warId}");
            
            await _territoryService.ShiftInfluence(
                war.SectorId,
                loser,
                -WAR_LOSER_INFLUENCE_LOSS,
                $"Defeat in war {warId}");
            
            // Update control state (war -> new state)
            var newState = await _territoryService.CalculateSectorControlState(war.SectorId);
            await _db.ExecuteAsync(@"
                UPDATE Faction_Territory_Control
                SET control_state = @State
                WHERE sector_id = @SectorId",
                new { SectorId = war.SectorId, State = newState.State });
            
            _logger.Warning(
                "[WAR RESOLVED] War {WarId} in sector {SectorId} won by {Victor}. " +
                "Loser: {Loser}. New state: {State}",
                warId, war.SectorId, victor, loser, newState.State);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to resolve war {WarId}", warId);
            throw;
        }
    }
    
    /// <summary>
    /// Get all active wars.
    /// </summary>
    public async Task<List<FactionWar>> GetActiveWars()
    {
        return (await _db.QueryAsync<FactionWar>(@"
            SELECT * FROM Faction_Wars
            WHERE is_active = 1
            ORDER BY war_start_date DESC")).ToList();
    }
    
    /// <summary>
    /// Get active war for a specific sector.
    /// </summary>
    public async Task<FactionWar> GetActiveWarForSector(int sectorId)
    {
        return await _db.QuerySingleOrDefaultAsync<FactionWar>(@"
            SELECT * FROM Faction_Wars
            WHERE sector_id = @SectorId AND is_active = 1",
            new { SectorId = sectorId });
    }
}
```

---

## IV. Data Models

```csharp
public class SectorControlState
{
    public string State { get; set; } // "Stable", "Contested", "War", "Independent", "Ruined"
    public string DominantFaction { get; set; } // Faction with highest influence (null if contested/war)
    public string[] ContestedFactions { get; set; } // For contested/war states
}

public class FactionInfluence
{
    public string FactionName { get; set; }
    public double InfluenceValue { get; set; }
    public string ControlState { get; set; }
}

public class FactionWar
{
    public int WarId { get; set; }
    public int WorldId { get; set; }
    public int SectorId { get; set; }
    public string FactionA { get; set; }
    public string FactionB { get; set; }
    public DateTime WarStartDate { get; set; }
    public DateTime? WarEndDate { get; set; }
    public double WarBalance { get; set; } // -100 to +100
    public bool IsActive { get; set; }
    public string Victor { get; set; }
    public int CollateralDamage { get; set; }
}
```

---

## V. Unit Tests

### TerritoryControlService Tests

```csharp
[TestClass]
public class TerritoryControlServiceTests
{
    private Mock<IDbConnection> _dbMock;
    private Mock<ILogger<TerritoryControlService>> _loggerMock;
    private Mock<FactionService> _factionServiceMock;
    private TerritoryControlService _service;
    
    [TestInitialize]
    public void Setup()
    {
        _dbMock = new Mock<IDbConnection>();
        _loggerMock = new Mock<ILogger<TerritoryControlService>>();
        _factionServiceMock = new Mock<FactionService>();
        
        _service = new TerritoryControlService(
            _dbMock.Object,
            _loggerMock.Object,
            _factionServiceMock.Object);
    }
    
    [TestMethod]
    public async Task CalculateSectorControlState_OneFactionOver60Percent_ReturnsStable()
    {
        // Arrange
        var influences = new List<FactionInfluence>
        {
            new FactionInfluence { FactionName = "Iron-Banes", InfluenceValue = 65.0 },
            new FactionInfluence { FactionName = "Rust-Clans", InfluenceValue = 20.0 }
        };
        
        // Mock database query
        _dbMock.Setup(db => db.QueryAsync<FactionInfluence>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(influences);
        
        _dbMock.Setup(db => db.QuerySingleOrDefaultAsync<FactionWar>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync((FactionWar)null);
        
        // Act
        var result = await _service.CalculateSectorControlState(1);
        
        // Assert
        Assert.AreEqual("Stable", result.State);
        Assert.AreEqual("Iron-Banes", result.DominantFaction);
    }
    
    [TestMethod]
    public async Task CalculateSectorControlState_TwoFactionsOver40Percent_ReturnsContested()
    {
        // Arrange
        var influences = new List<FactionInfluence>
        {
            new FactionInfluence { FactionName = "Jötun-Readers", InfluenceValue = 48.0 },
            new FactionInfluence { FactionName = "Rust-Clans", InfluenceValue = 45.0 }
        };
        
        _dbMock.Setup(db => db.QueryAsync<FactionInfluence>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(influences);
        
        _dbMock.Setup(db => db.QuerySingleOrDefaultAsync<FactionWar>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync((FactionWar)null);
        
        // Act
        var result = await _service.CalculateSectorControlState(3);
        
        // Assert
        Assert.AreEqual("Contested", result.State);
        Assert.IsNull(result.DominantFaction);
        CollectionAssert.AreEquivalent(
            new[] { "Jötun-Readers", "Rust-Clans" },
            result.ContestedFactions);
    }
    
    [TestMethod]
    public async Task CalculateSectorControlState_ActiveWar_ReturnsWar()
    {
        // Arrange
        var activeWar = new FactionWar
        {
            WarId = 1,
            SectorId = 3,
            FactionA = "Jötun-Readers",
            FactionB = "Rust-Clans",
            IsActive = true
        };
        
        _dbMock.Setup(db => db.QuerySingleOrDefaultAsync<FactionWar>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(activeWar);
        
        // Act
        var result = await _service.CalculateSectorControlState(3);
        
        // Assert
        Assert.AreEqual("War", result.State);
        CollectionAssert.AreEquivalent(
            new[] { "Jötun-Readers", "Rust-Clans" },
            result.ContestedFactions);
    }
    
    [TestMethod]
    public async Task ShiftInfluence_PositiveDelta_IncreasesInfluence()
    {
        // Arrange
        int sectorId = 1;
        string factionName = "Iron-Banes";
        double delta = 5.0;
        
        var influences = new List<FactionInfluence>
        {
            new FactionInfluence { FactionName = "Iron-Banes", InfluenceValue = 35.0 },
            new FactionInfluence { FactionName = "Rust-Clans", InfluenceValue = 30.0 }
        };
        
        _dbMock.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(1);
        
        _dbMock.Setup(db => db.QueryAsync<FactionInfluence>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(influences);
        
        // Act
        await _service.ShiftInfluence(sectorId, factionName, delta, "Test shift");
        
        // Assert
        _dbMock.Verify(
            db => db.ExecuteAsync(
                [It.Is](http://It.Is)<string>(sql => sql.Contains("UPDATE Faction_Territory_Control")),
                [It.Is](http://It.Is)<object>(p => p.GetType().GetProperty("Delta").GetValue(p).Equals(delta))),
            Times.Once);
    }
}
```

### FactionWarService Tests

```csharp
[TestClass]
public class FactionWarServiceTests
{
    [TestMethod]
    public async Task CheckWarTrigger_BothFactionsOver45Percent_TriggersWar()
    {
        // Arrange: Jötun-Readers 48%, Rust-Clans 46%
        // Act: CheckWarTrigger(3)
        // Assert: War initiated, returns true
    }
    
    [TestMethod]
    public async Task AdvanceWar_BalanceShiftPositive_FavorsFactionA()
    {
        // Arrange: War balance = 0, shift +15
        // Act: AdvanceWar(warId, +15)
        // Assert: War balance = +15, no victor yet
    }
    
    [TestMethod]
    public async Task AdvanceWar_BalanceExceedsThreshold_ResolvesWar()
    {
        // Arrange: War balance = +45, shift +10 (total +55)
        // Act: AdvanceWar(warId, +10)
        // Assert: War resolved, faction_a victorious
    }
    
    [TestMethod]
    public async Task ResolveWar_VictorDeclared_AppliesInfluenceChanges()
    {
        // Arrange: War in sector 3, victor = Jötun-Readers
        // Act: ResolveWar(warId, "Jötun-Readers")
        // Assert: Jötun-Readers +20%, Rust-Clans -20%
    }
}
```

---

## VI. Integration Points

### v0.33 FactionService Integration

```csharp
// Reputation affects influence power
public async Task<double> CalculateInfluencePower(int characterId, string factionName)
{
    var reputation = await _factionService.GetReputation(characterId, factionName);
    
    // Reputation multiplier: -100 to +100 → 0.5x to 1.5x
    double multiplier = 1.0 + (reputation / 200.0);
    
    return Math.Clamp(multiplier, 0.5, 1.5);
}
```

---

## VII. Success Criteria

**Functional Requirements:**

- ✅ TerritoryControlService calculates control states correctly
- ✅ Wars trigger when two factions exceed 45% influence
- ✅ War balance shifts from player actions
- ✅ Wars resolve with victory threshold (±50) or time limit (15 days)
- ✅ Victor gains +20% influence, loser loses -20%
- ✅ Control states update dynamically

**Quality Gates:**

- ✅ 10+ unit tests, 85%+ coverage
- ✅ Serilog structured logging
- ✅ v5.0 compliance
- ✅ ASCII-only entity names

---

**Status:** Implementation-ready territory and war mechanics complete.

**Next:** v0.35.3 (Dynamic World Events & Consequences)