using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Integration;
using RuneAndRust.Core;
using RuneAndRust.Persistence;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.35.4: Integration tests for Territory Control with other systems
/// Tests Faction, Companion, and Quest integration
/// </summary>
[TestFixture]
public class TerritoryIntegrationTests
{
    private TerritoryService _territoryService;
    private TerritoryControlService _controlService;
    private FactionWarService _warService;
    private WorldEventService _eventService;
    private ReputationService _reputationService;
    private FactionTerritoryIntegration _factionIntegration;
    private CompanionTerritoryReactions _companionReactions;
    private SaveRepository _saveRepository;
    private string _testDbDirectory;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        // Create unique test database
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"territory_integration_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbDirectory);

        // Initialize database schema
        _saveRepository = new SaveRepository(_testDbDirectory);
        _connectionString = $"Data Source={Path.Combine(_testDbDirectory, \"runeandrust.db\")}";

        // Initialize services
        _controlService = new TerritoryControlService(_connectionString);
        _warService = new FactionWarService(_connectionString, _controlService);
        _eventService = new WorldEventService(_connectionString, _controlService);
        _reputationService = new ReputationService(_connectionString);

        _territoryService = new TerritoryService(
            _connectionString,
            _controlService,
            _warService,
            _eventService,
            _reputationService);

        // Initialize integration helpers
        _factionIntegration = new FactionTerritoryIntegration(_territoryService, _reputationService);
        _companionReactions = new CompanionTerritoryReactions(_territoryService);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDbDirectory))
        {
            Directory.Delete(_testDbDirectory, true);
        }
    }

    #region Faction Integration Tests

    [Test]
    public void FactionReputationBonus_AppliesInHomeTerritory()
    {
        // Arrange: Sector 2 (Muspelheim) controlled by Iron-Banes
        int characterId = 1;
        int factionId = 1; // Iron-Banes
        int sectorId = 2;
        int baseReputation = 10;

        // Act
        int modifiedRep = _factionIntegration.CalculateReputationGain(
            characterId, factionId, baseReputation, sectorId);

        // Assert: Should get 1.5x bonus in home territory
        Assert.That(modifiedRep, Is.EqualTo(15)); // 10 * 1.5
    }

    [Test]
    public void FactionReputationBonus_NoModifierInOtherTerritory()
    {
        // Arrange: Sector 4 (Alfheim) controlled by Jötun-Readers
        int characterId = 1;
        int factionId = 1; // Iron-Banes (not controlling faction)
        int sectorId = 4;
        int baseReputation = 10;

        // Act
        int modifiedRep = _factionIntegration.CalculateReputationGain(
            characterId, factionId, baseReputation, sectorId);

        // Assert: No modifier
        Assert.That(modifiedRep, Is.EqualTo(10));
    }

    [Test]
    public void InfluencePowerMultiplier_HighReputation_HigherMultiplier()
    {
        // Arrange
        int reputation = 100; // Max reputation

        // Act
        double multiplier = _factionIntegration.GetInfluencePowerMultiplier(reputation);

        // Assert: 100 reputation → 1.5x multiplier
        Assert.That(multiplier, Is.EqualTo(1.5).Within(0.01));
    }

    [Test]
    public void InfluencePowerMultiplier_LowReputation_LowerMultiplier()
    {
        // Arrange
        int reputation = -100; // Minimum reputation

        // Act
        double multiplier = _factionIntegration.GetInfluencePowerMultiplier(reputation);

        // Assert: -100 reputation → 0.5x multiplier
        Assert.That(multiplier, Is.EqualTo(0.5).Within(0.01));
    }

    [Test]
    public void InfluencePowerMultiplier_NeutralReputation_NormalMultiplier()
    {
        // Arrange
        int reputation = 0;

        // Act
        double multiplier = _factionIntegration.GetInfluencePowerMultiplier(reputation);

        // Assert: 0 reputation → 1.0x multiplier
        Assert.That(multiplier, Is.EqualTo(1.0).Within(0.01));
    }

    [Test]
    public void IsPlayerInFriendlyTerritory_SameFaction_ReturnsTrue()
    {
        // Arrange: Sector 2 controlled by Iron-Banes
        int characterId = 1;
        int factionId = 1; // Iron-Banes
        int sectorId = 2;

        // Act
        bool isFriendly = _factionIntegration.IsPlayerInFriendlyTerritory(characterId, factionId, sectorId);

        // Assert
        Assert.That(isFriendly, Is.True);
    }

    [Test]
    public void IsPlayerInFriendlyTerritory_DifferentFaction_ReturnsFalse()
    {
        // Arrange: Sector 4 controlled by Jötun-Readers
        int characterId = 1;
        int factionId = 1; // Iron-Banes
        int sectorId = 4;

        // Act
        bool isFriendly = _factionIntegration.IsPlayerInFriendlyTerritory(characterId, factionId, sectorId);

        // Assert
        Assert.That(isFriendly, Is.False);
    }

    #endregion

    #region Companion Integration Tests

    [Test]
    public void CompanionReaction_HomeTerritory_PositiveMorale()
    {
        // Arrange: Iron-Bane companion entering Iron-Bane territory
        var companion = new Companion
        {
            CompanionID = 1,
            CompanionName = "Sigrid",
            FactionAffiliation = "IronBanes"
        };
        int sectorId = 2; // Iron-Bane controlled

        // Act
        var (dialogue, buffName, buffDuration, buffValue) = _companionReactions.GetCompanionReaction(companion, sectorId);

        // Assert
        Assert.That(dialogue, Does.Contain("allies").IgnoreCase);
        Assert.That(buffName, Is.EqualTo("Home_Territory_Morale"));
        Assert.That(buffValue, Is.GreaterThan(0)); // Positive buff
    }

    [Test]
    public void CompanionReaction_HostileTerritory_Stress()
    {
        // Arrange: Iron-Bane companion entering God-Sleeper territory
        var companion = new Companion
        {
            CompanionID = 2,
            CompanionName = "Thorvald",
            FactionAffiliation = "IronBanes"
        };
        int sectorId = 5; // Jotunheim - God-Sleeper controlled

        // Act
        var (dialogue, buffName, buffDuration, buffValue) = _companionReactions.GetCompanionReaction(companion, sectorId);

        // Assert
        Assert.That(dialogue, Does.Contain("careful").IgnoreCase.Or.Contains("not our friends"));
        Assert.That(buffName, Is.EqualTo("Hostile_Territory_Stress"));
        Assert.That(buffValue, Is.GreaterThan(0)); // Stress value
    }

    [Test]
    public void CompanionReaction_WarZone_CombatReadiness()
    {
        // Arrange: Companion entering war zone (sector 3)
        var companion = new Companion
        {
            CompanionID = 3,
            CompanionName = "Erik",
            FactionAffiliation = "Independents"
        };
        int sectorId = 3; // War zone

        var status = _territoryService.GetSectorTerritoryStatus(sectorId);
        if (status.ControlState != "War")
        {
            Assert.Inconclusive("Sector 3 not in war state - test requires active war");
            return;
        }

        // Act
        var (dialogue, buffName, buffDuration, buffValue) = _companionReactions.GetCompanionReaction(companion, sectorId);

        // Assert
        Assert.That(dialogue, Does.Contain("war").IgnoreCase.Or.Contains("sharp"));
        Assert.That(buffName, Is.EqualTo("Combat_Readiness"));
    }

    [Test]
    public void CompanionReaction_NeutralTerritory_NoSpecialReaction()
    {
        // Arrange: Companion entering neutral/independent territory
        var companion = new Companion
        {
            CompanionID = 4,
            CompanionName = "Ulf",
            FactionAffiliation = "RustClans"
        };
        int sectorId = 1; // Midgard - Independent

        // Act
        var (dialogue, buffName, buffDuration, buffValue) = _companionReactions.GetCompanionReaction(companion, sectorId);

        // Assert
        Assert.That(buffName, Is.Null); // No buff in neutral territory
        Assert.That(dialogue, Does.Contain("neutral").IgnoreCase.Or.Contains("independent"));
    }

    [Test]
    public void CompanionComment_WarZone_DescribesWar()
    {
        // Arrange
        var companion = new Companion
        {
            CompanionID = 1,
            CompanionName = "Sigrid",
            FactionAffiliation = "IronBanes"
        };
        int sectorId = 3; // War zone

        var status = _territoryService.GetSectorTerritoryStatus(sectorId);
        if (status.ControlState != "War")
        {
            Assert.Inconclusive("Sector 3 not in war state");
            return;
        }

        // Act
        string comment = _companionReactions.GetCompanionComment(companion, sectorId);

        // Assert
        Assert.That(comment, Does.Contain("war").IgnoreCase);
        Assert.That(comment, Does.Contain(status.ActiveWar!.FactionA).Or.Contains(status.ActiveWar.FactionB));
    }

    [Test]
    public void CompanionComment_ContestedSector_DescribesContest()
    {
        // Arrange
        var companion = new Companion
        {
            CompanionID = 1,
            CompanionName = "Sigrid"
        };
        int sectorId = 7; // Vanaheim - typically contested

        var status = _territoryService.GetSectorTerritoryStatus(sectorId);
        if (status.ControlState != "Contested")
        {
            Assert.Inconclusive("Sector 7 not contested");
            return;
        }

        // Act
        string comment = _companionReactions.GetCompanionComment(companion, sectorId);

        // Assert
        Assert.That(comment, Does.Contain("contested").IgnoreCase);
    }

    [Test]
    public void CompanionComment_StableSector_DescribesControl()
    {
        // Arrange
        var companion = new Companion
        {
            CompanionID = 1,
            CompanionName = "Sigrid"
        };
        int sectorId = 2; // Muspelheim - stable Iron-Bane control

        // Act
        string comment = _companionReactions.GetCompanionComment(companion, sectorId);

        // Assert
        Assert.That(comment, Does.Contain("controls").IgnoreCase);
        Assert.That(comment, Does.Contain("IronBanes"));
    }

    #endregion

    #region Full System Integration Tests

    [Test]
    public void FullIntegration_PlayerQuestCompletion_AffectsAllSystems()
    {
        // Arrange
        int characterId = 1;
        int sectorId = 2;
        string faction = "IronBanes";

        // Get initial state
        var initialStatus = _territoryService.GetSectorTerritoryStatus(sectorId);
        var initialInfluence = initialStatus.FactionInfluences
            .First(i => i.FactionName == faction)
            .InfluenceValue;

        // Act: Player completes quest for Iron-Banes
        _territoryService.RecordPlayerAction(characterId, sectorId, "Complete_Quest", faction);

        // Assert: Multiple systems affected
        // 1. Influence increased
        var newStatus = _territoryService.GetSectorTerritoryStatus(sectorId);
        var newInfluence = newStatus.FactionInfluences
            .First(i => i.FactionName == faction)
            .InfluenceValue;
        Assert.That(newInfluence, Is.GreaterThan(initialInfluence));

        // 2. Player action recorded
        var totalInfluence = _territoryService.GetPlayerTotalInfluence(characterId);
        Assert.That(totalInfluence, Contains.Key(faction));

        // 3. Generation params affected
        var genParams = _territoryService.GetSectorGenerationParams(sectorId);
        Assert.That(genParams.EnemyFactionFilter, Is.Not.Null);
    }

    [Test]
    public void FullIntegration_MultiplePlayersCompeting_IndependentTracking()
    {
        // Arrange
        int player1 = 1;
        int player2 = 2;
        int sectorId = 3;

        // Act: Both players take actions
        _territoryService.RecordPlayerAction(player1, sectorId, "Complete_Quest", "JotunReaders");
        _territoryService.RecordPlayerAction(player2, sectorId, "Complete_Quest", "RustClans");

        // Assert: Separate tracking
        var player1Influence = _territoryService.GetPlayerTotalInfluence(player1);
        var player2Influence = _territoryService.GetPlayerTotalInfluence(player2);

        Assert.That(player1Influence, Contains.Key("JotunReaders"));
        Assert.That(player2Influence, Contains.Key("RustClans"));
        Assert.That(player1Influence.ContainsKey("RustClans"), Is.False);
        Assert.That(player2Influence.ContainsKey("JotunReaders"), Is.False);
    }

    #endregion
}
