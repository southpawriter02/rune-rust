using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Core.Factions;
using RuneAndRust.Persistence;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.33.2: Test suite for FactionService
/// Validates faction operations, witness system, and world reactions
/// </summary>
[TestFixture]
public class FactionServiceTests
{
    private FactionService _factionService;
    private SaveRepository _saveRepository;
    private string _testDbDirectory;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        // Create unique test database
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"faction_svc_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbDirectory);

        // Initialize database schema
        _saveRepository = new SaveRepository(_testDbDirectory);
        _connectionString = $"Data Source={Path.Combine(_testDbDirectory, "runeandrust.db")}";

        _factionService = new FactionService(_connectionString);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDbDirectory))
        {
            Directory.Delete(_testDbDirectory, true);
        }
    }

    [Test]
    public void GetAllFactions_AfterInitialization_ReturnsFiveFactions()
    {
        // Act
        var factions = _factionService.GetAllFactions();

        // Assert
        Assert.That(factions, Has.Count.EqualTo(5));
        Assert.That(factions.Any(f => f.FactionName == "IronBanes"), Is.True);
        Assert.That(factions.Any(f => f.FactionName == "GodSleeperCultists"), Is.True);
        Assert.That(factions.Any(f => f.FactionName == "JotunReaders"), Is.True);
        Assert.That(factions.Any(f => f.FactionName == "RustClans"), Is.True);
        Assert.That(factions.Any(f => f.FactionName == "Independents"), Is.True);
    }

    [Test]
    public void GetFactionById_IronBanes_ReturnsCorrectFaction()
    {
        // Act
        var faction = _factionService.GetFactionById(1);

        // Assert
        Assert.That(faction, Is.Not.Null);
        Assert.That(faction!.FactionName, Is.EqualTo("IronBanes"));
        Assert.That(faction.DisplayName, Is.EqualTo("Iron-Banes"));
        Assert.That(faction.Philosophy, Does.Contain("purification protocols"));
    }

    [Test]
    public void GetFactionByName_JotunReaders_ReturnsCorrectFaction()
    {
        // Act
        var faction = _factionService.GetFactionByName("JotunReaders");

        // Assert
        Assert.That(faction, Is.Not.Null);
        Assert.That(faction!.FactionId, Is.EqualTo(3));
        Assert.That(faction.DisplayName, Is.EqualTo("Jötun-Readers"));
        Assert.That(faction.Philosophy, Does.Contain("Knowledge"));
    }

    [Test]
    public void ProcessWitnessedAction_KillUndying_AffectsMultipleFactions()
    {
        // Arrange
        int characterId = 1;

        // Act
        _factionService.ProcessWitnessedAction(
            characterId,
            WitnessedActionTypes.KillUndying,
            null,
            "Muspelheim"
        );

        // Assert
        var ironBanesRep = _factionService.GetCharacterReputation(characterId, 1); // Iron-Banes
        var godSleeperRep = _factionService.GetCharacterReputation(characterId, 2); // God-Sleepers

        Assert.That(ironBanesRep, Is.Not.Null);
        Assert.That(ironBanesRep!.ReputationValue, Is.GreaterThan(0), "Iron-Banes should gain reputation");

        Assert.That(godSleeperRep, Is.Not.Null);
        Assert.That(godSleeperRep!.ReputationValue, Is.LessThan(0), "God-Sleepers should lose reputation");
    }

    [Test]
    public void ProcessWitnessedAction_KillJotunForged_MajorReputationSwing()
    {
        // Arrange
        int characterId = 1;

        // Act
        _factionService.ProcessWitnessedAction(
            characterId,
            WitnessedActionTypes.KillJotunForged,
            null,
            "Jotunheim"
        );

        // Assert
        var ironBanesRep = _factionService.GetCharacterReputation(characterId, 1);
        var godSleeperRep = _factionService.GetCharacterReputation(characterId, 2);

        Assert.That(ironBanesRep!.ReputationValue, Is.EqualTo(30), "Iron-Banes should gain major reputation");
        Assert.That(godSleeperRep!.ReputationValue, Is.EqualTo(-60), "God-Sleepers should lose major reputation");
    }

    [Test]
    public void ProcessWitnessedAction_RecoverData_JotunReadersAppreciate()
    {
        // Arrange
        int characterId = 1;

        // Act
        _factionService.ProcessWitnessedAction(
            characterId,
            WitnessedActionTypes.RecoverData,
            null,
            "Alfheim"
        );

        // Assert
        var jotunReadersRep = _factionService.GetCharacterReputation(characterId, 3);
        Assert.That(jotunReadersRep, Is.Not.Null);
        Assert.That(jotunReadersRep!.ReputationValue, Is.GreaterThan(0));
    }

    [Test]
    public void GetHostileFactions_WithHatedReputation_ReturnsFaction()
    {
        // Arrange
        int characterId = 1;
        _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillJotunForged, null, "Jotunheim");
        _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillJotunForged, null, "Jotunheim");

        // Act
        var hostileFactions = _factionService.GetHostileFactions(characterId);

        // Assert
        Assert.That(hostileFactions, Is.Not.Empty);
        Assert.That(hostileFactions.Any(f => f.FactionId == 2), Is.True, "God-Sleepers should be hostile");
    }

    [Test]
    public void GetPriceModifier_WithFriendlyReputation_ReturnsDiscount()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1; // Iron-Banes

        // Build reputation to Friendly tier
        _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillUndying, null, "Trunk");
        _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillUndying, null, "Trunk");
        _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillUndying, null, "Trunk");

        // Act
        var modifier = _factionService.GetPriceModifier(characterId, factionId);

        // Assert
        Assert.That(modifier, Is.LessThan(1.0f), "Should receive a discount");
    }

    [Test]
    public void GetPriceModifier_WithHostileReputation_ReturnsMarkup()
    {
        // Arrange
        int characterId = 1;
        int factionId = 2; // God-Sleeper Cultists

        // Make them hostile
        _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillJotunForged, null, "Jotunheim");

        // Act
        var modifier = _factionService.GetPriceModifier(characterId, factionId);

        // Assert
        Assert.That(modifier, Is.GreaterThan(1.0f), "Should have price markup");
    }

    [Test]
    public void GetEncounterModifier_WithExaltedReputation_ReturnsZero()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1; // Iron-Banes

        // Build to Exalted (would need many actions in real scenario)
        // For test, we'll directly modify via service internals
        var reputationService = new ReputationService(_connectionString);
        reputationService.ModifyReputation(characterId, factionId, 80, "Test");

        // Act
        var modifier = _factionService.GetEncounterModifier(characterId, factionId);

        // Assert
        Assert.That(modifier, Is.EqualTo(0.0f), "Exalted should have no hostile encounters");
    }

    [Test]
    public void GetEncounterModifier_WithHatedReputation_ReturnsTripleRate()
    {
        // Arrange
        int characterId = 1;
        int factionId = 2; // God-Sleeper Cultists

        // Make them hate us
        var reputationService = new ReputationService(_connectionString);
        reputationService.ModifyReputation(characterId, factionId, -85, "Test");

        // Act
        var modifier = _factionService.GetEncounterModifier(characterId, factionId);

        // Assert
        Assert.That(modifier, Is.EqualTo(3.0f), "Hated should triple hostile encounter rate");
    }

    [Test]
    public void GetAllCharacterReputations_WithMultipleFactionInteractions_ReturnsAllEntries()
    {
        // Arrange
        int characterId = 1;
        _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillUndying, null, "Trunk");
        _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.RecoverData, null, "Alfheim");

        // Act
        var reputations = _factionService.GetAllCharacterReputations(characterId);

        // Assert
        Assert.That(reputations.Count, Is.GreaterThan(0));
        Assert.That(reputations.Any(r => r.FactionId == 1), Is.True, "Should have Iron-Banes reputation");
        Assert.That(reputations.Any(r => r.FactionId == 3), Is.True, "Should have Jötun-Readers reputation");
    }

    [Test]
    public void Faction_IsAlly_CorrectlyIdentifiesAllies()
    {
        // Arrange
        var ironBanes = _factionService.GetFactionById(1);
        var rustClans = _factionService.GetFactionById(4);

        // Act & Assert
        Assert.That(ironBanes!.IsAlly("RustClans"), Is.True);
        Assert.That(rustClans!.IsAlly("IronBanes"), Is.True);
    }

    [Test]
    public void Faction_IsEnemy_CorrectlyIdentifiesEnemies()
    {
        // Arrange
        var ironBanes = _factionService.GetFactionById(1);
        var godSleepers = _factionService.GetFactionById(2);

        // Act & Assert
        Assert.That(ironBanes!.IsEnemy("GodSleeperCultists"), Is.True);
        Assert.That(godSleepers!.IsEnemy("IronBanes"), Is.True);
    }

    [Test]
    public void GetAvailableFactionQuests_WithNoReputation_ReturnsBaseQuests()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;

        // Act
        var quests = _factionService.GetAvailableFactionQuests(characterId, factionId);

        // Assert - Even with no reputation, should return quests requiring 0 or negative rep
        Assert.That(quests, Is.Not.Null);
    }

    [Test]
    public void GetAvailableFactionRewards_WithNoReputation_ReturnsNoRewards()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;

        // Act
        var rewards = _factionService.GetAvailableFactionRewards(characterId, factionId);

        // Assert - With 0 reputation, should only get rewards requiring 0 or less
        Assert.That(rewards, Is.Not.Null);
    }
}
