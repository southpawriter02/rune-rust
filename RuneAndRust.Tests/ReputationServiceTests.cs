using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Core.Factions;
using RuneAndRust.Persistence;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.33.2: Test suite for ReputationService
/// Validates reputation calculations, tier transitions, and price modifiers
/// </summary>
[TestFixture]
public class ReputationServiceTests
{
    private ReputationService _reputationService;
    private SaveRepository _saveRepository;
    private string _testDbDirectory;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        // Create unique test database
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"rep_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbDirectory);

        // Initialize database schema
        _saveRepository = new SaveRepository(_testDbDirectory);
        _connectionString = $"Data Source={Path.Combine(_testDbDirectory, "runeandrust.db")}";

        _reputationService = new ReputationService(_connectionString);
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
    public void GetReputationTier_WithExaltedValue_ReturnsExalted()
    {
        // Arrange
        var reputationValue = 80;

        // Act
        var tier = _reputationService.GetReputationTier(reputationValue);

        // Assert
        Assert.That(tier, Is.EqualTo(FactionReputationTier.Exalted));
    }

    [Test]
    public void GetReputationTier_WithAlliedValue_ReturnsAllied()
    {
        // Arrange
        var reputationValue = 60;

        // Act
        var tier = _reputationService.GetReputationTier(reputationValue);

        // Assert
        Assert.That(tier, Is.EqualTo(FactionReputationTier.Allied));
    }

    [Test]
    public void GetReputationTier_WithFriendlyValue_ReturnsFriendly()
    {
        // Arrange
        var reputationValue = 30;

        // Act
        var tier = _reputationService.GetReputationTier(reputationValue);

        // Assert
        Assert.That(tier, Is.EqualTo(FactionReputationTier.Friendly));
    }

    [Test]
    public void GetReputationTier_WithNeutralValue_ReturnsNeutral()
    {
        // Arrange
        var reputationValue = 0;

        // Act
        var tier = _reputationService.GetReputationTier(reputationValue);

        // Assert
        Assert.That(tier, Is.EqualTo(FactionReputationTier.Neutral));
    }

    [Test]
    public void GetReputationTier_WithHostileValue_ReturnsHostile()
    {
        // Arrange
        var reputationValue = -40;

        // Act
        var tier = _reputationService.GetReputationTier(reputationValue);

        // Assert
        Assert.That(tier, Is.EqualTo(FactionReputationTier.Hostile));
    }

    [Test]
    public void GetReputationTier_WithHatedValue_ReturnsHated()
    {
        // Arrange
        var reputationValue = -85;

        // Act
        var tier = _reputationService.GetReputationTier(reputationValue);

        // Assert
        Assert.That(tier, Is.EqualTo(FactionReputationTier.Hated));
    }

    [Test]
    [TestCase(75, FactionReputationTier.Exalted)]
    [TestCase(50, FactionReputationTier.Allied)]
    [TestCase(25, FactionReputationTier.Friendly)]
    [TestCase(-25, FactionReputationTier.Neutral)]
    [TestCase(-75, FactionReputationTier.Hostile)]
    [TestCase(-100, FactionReputationTier.Hated)]
    public void GetReputationTier_BoundaryValues_ReturnsCorrectTier(int value, FactionReputationTier expectedTier)
    {
        // Act
        var tier = _reputationService.GetReputationTier(value);

        // Assert
        Assert.That(tier, Is.EqualTo(expectedTier));
    }

    [Test]
    public void GetPriceModifier_Exalted_Returns30PercentDiscount()
    {
        // Act
        var modifier = _reputationService.GetPriceModifier(FactionReputationTier.Exalted);

        // Assert
        Assert.That(modifier, Is.EqualTo(0.70f));
    }

    [Test]
    public void GetPriceModifier_Allied_Returns20PercentDiscount()
    {
        // Act
        var modifier = _reputationService.GetPriceModifier(FactionReputationTier.Allied);

        // Assert
        Assert.That(modifier, Is.EqualTo(0.80f));
    }

    [Test]
    public void GetPriceModifier_Friendly_Returns10PercentDiscount()
    {
        // Act
        var modifier = _reputationService.GetPriceModifier(FactionReputationTier.Friendly);

        // Assert
        Assert.That(modifier, Is.EqualTo(0.90f));
    }

    [Test]
    public void GetPriceModifier_Neutral_ReturnsNormalPrice()
    {
        // Act
        var modifier = _reputationService.GetPriceModifier(FactionReputationTier.Neutral);

        // Assert
        Assert.That(modifier, Is.EqualTo(1.0f));
    }

    [Test]
    public void GetPriceModifier_Hostile_Returns25PercentMarkup()
    {
        // Act
        var modifier = _reputationService.GetPriceModifier(FactionReputationTier.Hostile);

        // Assert
        Assert.That(modifier, Is.EqualTo(1.25f));
    }

    [Test]
    public void GetPriceModifier_Hated_Returns50PercentMarkup()
    {
        // Act
        var modifier = _reputationService.GetPriceModifier(FactionReputationTier.Hated);

        // Assert
        Assert.That(modifier, Is.EqualTo(1.50f));
    }

    [Test]
    public void GetEncounterFrequencyModifier_Exalted_ReturnsZero()
    {
        // Act
        var modifier = _reputationService.GetEncounterFrequencyModifier(FactionReputationTier.Exalted);

        // Assert
        Assert.That(modifier, Is.EqualTo(0.0f));
    }

    [Test]
    public void GetEncounterFrequencyModifier_Hated_ReturnsTripleRate()
    {
        // Act
        var modifier = _reputationService.GetEncounterFrequencyModifier(FactionReputationTier.Hated);

        // Assert
        Assert.That(modifier, Is.EqualTo(3.0f));
    }

    [Test]
    public void ModifyReputation_NewCharacter_CreatesReputationEntry()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1; // Iron-Banes

        // Act
        _reputationService.ModifyReputation(characterId, factionId, 25, "Test action");

        // Assert
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        Assert.That(reputation, Is.Not.Null);
        Assert.That(reputation!.ReputationValue, Is.EqualTo(25));
        Assert.That(reputation.ReputationTier, Is.EqualTo(FactionReputationTier.Friendly));
    }

    [Test]
    public void ModifyReputation_ExistingReputation_UpdatesValue()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;
        _reputationService.ModifyReputation(characterId, factionId, 20, "Initial gain");

        // Act
        _reputationService.ModifyReputation(characterId, factionId, 15, "Additional gain");

        // Assert
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        Assert.That(reputation!.ReputationValue, Is.EqualTo(35));
        Assert.That(reputation.ReputationTier, Is.EqualTo(FactionReputationTier.Friendly));
    }

    [Test]
    public void ModifyReputation_ClampAtMaximum_StopsAt100()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;

        // Act
        _reputationService.ModifyReputation(characterId, factionId, 150, "Huge gain");

        // Assert
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        Assert.That(reputation!.ReputationValue, Is.EqualTo(100));
        Assert.That(reputation.ReputationTier, Is.EqualTo(FactionReputationTier.Exalted));
    }

    [Test]
    public void ModifyReputation_ClampAtMinimum_StopsAtNegative100()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;

        // Act
        _reputationService.ModifyReputation(characterId, factionId, -150, "Huge loss");

        // Assert
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        Assert.That(reputation!.ReputationValue, Is.EqualTo(-100));
        Assert.That(reputation.ReputationTier, Is.EqualTo(FactionReputationTier.Hated));
    }

    [Test]
    public void ModifyReputation_TierTransition_UpdatesTierCorrectly()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;
        _reputationService.ModifyReputation(characterId, factionId, 20, "Initial");

        // Act - Push into Friendly tier
        _reputationService.ModifyReputation(characterId, factionId, 10, "Cross threshold");

        // Assert
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        Assert.That(reputation!.ReputationValue, Is.EqualTo(30));
        Assert.That(reputation.ReputationTier, Is.EqualTo(FactionReputationTier.Friendly));
    }

    [Test]
    public void IsFactionHostile_WithHatedReputation_ReturnsTrue()
    {
        // Arrange
        int characterId = 1;
        int factionId = 2; // God-Sleeper Cultists
        _reputationService.ModifyReputation(characterId, factionId, -85, "Kill their gods");

        // Act
        var isHostile = _reputationService.IsFactionHostile(characterId, factionId);

        // Assert
        Assert.That(isHostile, Is.True);
    }

    [Test]
    public void IsFactionHostile_WithHostileReputation_ReturnsTrue()
    {
        // Arrange
        int characterId = 1;
        int factionId = 2;
        _reputationService.ModifyReputation(characterId, factionId, -50, "Provoke faction");

        // Act
        var isHostile = _reputationService.IsFactionHostile(characterId, factionId);

        // Assert
        Assert.That(isHostile, Is.True);
    }

    [Test]
    public void IsFactionHostile_WithNeutralReputation_ReturnsFalse()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;

        // Act
        var isHostile = _reputationService.IsFactionHostile(characterId, factionId);

        // Assert
        Assert.That(isHostile, Is.False);
    }

    [Test]
    public void GetAllReputations_WithMultipleFactions_ReturnsAllEntries()
    {
        // Arrange
        int characterId = 1;
        _reputationService.ModifyReputation(characterId, 1, 30, "Iron-Banes");
        _reputationService.ModifyReputation(characterId, 2, -40, "God-Sleepers");
        _reputationService.ModifyReputation(characterId, 3, 20, "Jötun-Readers");

        // Act
        var reputations = _reputationService.GetAllReputations(characterId);

        // Assert
        Assert.That(reputations, Has.Count.EqualTo(3));
        Assert.That(reputations.Any(r => r.FactionId == 1 && r.ReputationValue == 30), Is.True);
        Assert.That(reputations.Any(r => r.FactionId == 2 && r.ReputationValue == -40), Is.True);
        Assert.That(reputations.Any(r => r.FactionId == 3 && r.ReputationValue == 20), Is.True);
    }

    [Test]
    public void CalculateReputationChange_KillUndying_IronBanesGainRep()
    {
        // Act
        var change = _reputationService.CalculateReputationChange(
            WitnessedActionTypes.KillUndying,
            1, // Iron-Banes
            null
        );

        // Assert
        Assert.That(change, Is.EqualTo(10));
    }

    [Test]
    public void CalculateReputationChange_KillUndying_GodSleepersLoseRep()
    {
        // Act
        var change = _reputationService.CalculateReputationChange(
            WitnessedActionTypes.KillUndying,
            2, // God-Sleeper Cultists
            null
        );

        // Assert
        Assert.That(change, Is.EqualTo(-20));
    }

    [Test]
    public void CalculateReputationChange_KillJotunForged_IronBanesGainMajorRep()
    {
        // Act
        var change = _reputationService.CalculateReputationChange(
            WitnessedActionTypes.KillJotunForged,
            1, // Iron-Banes
            null
        );

        // Assert
        Assert.That(change, Is.EqualTo(30));
    }

    [Test]
    public void CalculateReputationChange_KillJotunForged_GodSleepersLoseMajorRep()
    {
        // Act
        var change = _reputationService.CalculateReputationChange(
            WitnessedActionTypes.KillJotunForged,
            2, // God-Sleeper Cultists
            null
        );

        // Assert
        Assert.That(change, Is.EqualTo(-60));
    }

    [Test]
    public void CalculateReputationChange_RecoverData_JotunReadersGainRep()
    {
        // Act
        var change = _reputationService.CalculateReputationChange(
            WitnessedActionTypes.RecoverData,
            3, // Jötun-Readers
            null
        );

        // Assert
        Assert.That(change, Is.EqualTo(10));
    }

    [Test]
    public void CalculateReputationChange_DestroyData_JotunReadersLoseMajorRep()
    {
        // Act
        var change = _reputationService.CalculateReputationChange(
            WitnessedActionTypes.DestroyData,
            3, // Jötun-Readers
            null
        );

        // Assert
        Assert.That(change, Is.EqualTo(-20));
    }
}
