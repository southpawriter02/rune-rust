using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Core.Factions;
using RuneAndRust.Persistence;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.33.4: Integration tests for complete faction system workflow
/// Tests end-to-end scenarios involving multiple services
/// </summary>
[TestFixture]
public class FactionIntegrationTests
{
    private FactionService _factionService;
    private ReputationService _reputationService;
    private FactionEncounterService _encounterService;
    private SaveRepository _saveRepository;
    private string _testDbDirectory;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        // Create unique test database
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"faction_integration_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbDirectory);

        // Initialize database schema
        _saveRepository = new SaveRepository(_testDbDirectory);
        _connectionString = $"Data Source={Path.Combine(_testDbDirectory, "runeandrust.db")}";

        // Initialize services
        _reputationService = new ReputationService(_connectionString);
        _factionService = new FactionService(_connectionString);
        _encounterService = new FactionEncounterService(_factionService, _reputationService);
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
    public void CompleteWorkflow_KillUndying_AffectsMultipleFactions()
    {
        // Arrange
        int characterId = 1;

        // Act - Kill Undying enemy (witnessed by all factions)
        _factionService.ProcessWitnessedAction(
            characterId,
            WitnessedActionTypes.KillUndying,
            null,
            "Muspelheim"
        );

        // Assert - Check multiple faction reputations
        var ironBanesRep = _factionService.GetCharacterReputation(characterId, 1); // Iron-Banes should gain
        var godSleeperRep = _factionService.GetCharacterReputation(characterId, 2); // God-Sleepers should lose

        Assert.That(ironBanesRep, Is.Not.Null);
        Assert.That(ironBanesRep!.ReputationValue, Is.GreaterThan(0), "Iron-Banes should gain reputation");

        Assert.That(godSleeperRep, Is.Not.Null);
        Assert.That(godSleeperRep!.ReputationValue, Is.LessThan(0), "God-Sleepers should lose reputation");
    }

    [Test]
    public void CompleteWorkflow_BuildReputation_UnlocksQuests()
    {
        // Arrange
        int characterId = 1;
        int ironBanesFactionId = 1;

        // Act - Build reputation through multiple actions
        for (int i = 0; i < 5; i++)
        {
            _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillUndying, null, "Trunk");
        }

        // Get available quests
        var quests = _factionService.GetAvailableFactionQuests(characterId, ironBanesFactionId);
        var reputation = _factionService.GetCharacterReputation(characterId, ironBanesFactionId);

        // Assert
        Assert.That(reputation!.ReputationValue, Is.GreaterThanOrEqualTo(25), "Should reach Friendly tier");
        Assert.That(quests.Count, Is.GreaterThan(0), "Should have unlocked quests");
    }

    [Test]
    public void CompleteWorkflow_BuildReputation_UnlocksRewards()
    {
        // Arrange
        int characterId = 1;
        int ironBanesFactionId = 1;

        // Act - Build to Allied tier (50+)
        _reputationService.ModifyReputation(characterId, ironBanesFactionId, 55, "Test");

        var rewards = _factionService.GetAvailableFactionRewards(characterId, ironBanesFactionId);

        // Assert
        Assert.That(rewards.Count, Is.GreaterThan(0));
        Assert.That(rewards.Any(r => r.RequiredReputation <= 50), Is.True);
    }

    [Test]
    public void CompleteWorkflow_HostileReputation_TriggersEncounters()
    {
        // Arrange
        int characterId = 1;
        int godSleeperFactionId = 2;

        // Act - Make God-Sleepers hostile
        _reputationService.ModifyReputation(characterId, godSleeperFactionId, -60, "Killed their gods");

        // Check if hostile
        var hostileFactions = _factionService.GetHostileFactions(characterId);

        // Check ambush chance
        var ambushChance = _encounterService.GetAmbushChance(characterId, godSleeperFactionId);

        // Assert
        Assert.That(hostileFactions.Any(f => f.FactionId == godSleeperFactionId), Is.True);
        Assert.That(ambushChance, Is.GreaterThan(0f), "Should have ambush chance when hostile");
    }

    [Test]
    public void CompleteWorkflow_FriendlyReputation_EnablesAssistance()
    {
        // Arrange
        int characterId = 1;
        int rustClansFactionId = 4;

        // Act - Build to Allied tier
        _reputationService.ModifyReputation(characterId, rustClansFactionId, 60, "Helped defend outpost");

        var willHelp = _encounterService.WillOfferAssistance(characterId, rustClansFactionId);

        // Assert
        Assert.That(willHelp, Is.True, "Allied factions should offer assistance");
    }

    [Test]
    public void CompleteWorkflow_MutualExclusivity_IronBanesVsGodSleepers()
    {
        // Arrange
        int characterId = 1;

        // Act - Kill Jötun-Forged (benefits Iron-Banes, angers God-Sleepers)
        _factionService.ProcessWitnessedAction(
            characterId,
            WitnessedActionTypes.KillJotunForged,
            null,
            "Jotunheim"
        );

        var ironBanesRep = _factionService.GetCharacterReputation(characterId, 1);
        var godSleeperRep = _factionService.GetCharacterReputation(characterId, 2);

        // Assert - Mutually exclusive gains/losses
        Assert.That(ironBanesRep!.ReputationValue, Is.EqualTo(30), "Iron-Banes should gain major reputation");
        Assert.That(godSleeperRep!.ReputationValue, Is.EqualTo(-60), "God-Sleepers should lose major reputation");
        Assert.That(ironBanesRep.ReputationValue + godSleeperRep.ReputationValue, Is.LessThan(0),
            "Cannot be friendly with both mutually exclusive factions");
    }

    [Test]
    public void CompleteWorkflow_PriceModifiers_VaryByReputation()
    {
        // Arrange
        int characterId = 1;
        int factionId = 3; // Jötun-Readers

        // Act & Assert - Neutral reputation
        var neutralModifier = _factionService.GetPriceModifier(characterId, factionId);
        Assert.That(neutralModifier, Is.EqualTo(1.0f), "Neutral should have no modifier");

        // Build to Friendly
        _reputationService.ModifyReputation(characterId, factionId, 30, "Test");
        var friendlyModifier = _factionService.GetPriceModifier(characterId, factionId);
        Assert.That(friendlyModifier, Is.LessThan(1.0f), "Friendly should get discount");

        // Build to Exalted
        _reputationService.ModifyReputation(characterId, factionId, 50, "Test");
        var exaltedModifier = _factionService.GetPriceModifier(characterId, factionId);
        Assert.That(exaltedModifier, Is.LessThan(friendlyModifier), "Exalted should get better discount");
    }

    [Test]
    public void CompleteWorkflow_EncounterGeneration_VariesByReputation()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;
        int characterLevel = 5;

        // Test Hostile encounters
        _reputationService.ModifyReputation(characterId, factionId, -70, "Made hostile");

        var hostileEncounterMod = _factionService.GetEncounterModifier(characterId, factionId);
        Assert.That(hostileEncounterMod, Is.GreaterThan(1.0f), "Hostile should increase encounter rate");

        // Test Friendly encounters
        _reputationService.ModifyReputation(characterId, factionId, 150, "Made friendly"); // Will clamp to 100

        var friendlyEncounterMod = _factionService.GetEncounterModifier(characterId, factionId);
        Assert.That(friendlyEncounterMod, Is.LessThan(1.0f), "Friendly should decrease hostile encounters");
    }

    [Test]
    public void CompleteWorkflow_IndependentsPath_MaintainsNeutrality()
    {
        // Arrange
        int characterId = 1;
        int independentsFactionId = 5;

        // Act - Gain reputation with Independents without joining other factions
        _reputationService.ModifyReputation(characterId, independentsFactionId, 40, "Solo survival");

        // Check other factions remain neutral
        var ironBanesRep = _factionService.GetCharacterReputation(characterId, 1);
        var godSleeperRep = _factionService.GetCharacterReputation(characterId, 2);
        var jotunReadersRep = _factionService.GetCharacterReputation(characterId, 3);

        // Assert - Independents can coexist with neutral standing in other factions
        Assert.That(ironBanesRep?.ReputationValue ?? 0, Is.EqualTo(0));
        Assert.That(godSleeperRep?.ReputationValue ?? 0, Is.EqualTo(0));
        Assert.That(jotunReadersRep?.ReputationValue ?? 0, Is.EqualTo(0));
    }

    [Test]
    public void CompleteWorkflow_AlliedFactions_BenefitTogether()
    {
        // Arrange
        int characterId = 1;

        // Act - Help Iron-Banes (allied with Rust-Clans)
        for (int i = 0; i < 3; i++)
        {
            _factionService.ProcessWitnessedAction(characterId, WitnessedActionTypes.KillUndying, null, "Trunk");
        }

        var ironBanesRep = _factionService.GetCharacterReputation(characterId, 1);
        var rustClansRep = _factionService.GetCharacterReputation(characterId, 4);

        // Assert - Both allied factions should benefit (though Iron-Banes more directly)
        Assert.That(ironBanesRep!.ReputationValue, Is.GreaterThan(0));

        // Check that they are allies
        var ironBanes = _factionService.GetFactionById(1);
        Assert.That(ironBanes!.IsAlly("RustClans"), Is.True);
    }

    [Test]
    public void CompleteWorkflow_EnemyFactions_ConflictExists()
    {
        // Arrange - Get Iron-Banes and God-Sleepers
        var ironBanes = _factionService.GetFactionById(1);
        var godSleepers = _factionService.GetFactionById(2);

        // Assert - Verify enemy relationship
        Assert.That(ironBanes!.IsEnemy("GodSleeperCultists"), Is.True);
        Assert.That(godSleepers!.IsEnemy("IronBanes"), Is.True);
    }

    [Test]
    public void CompleteWorkflow_ReputationClamping_EnforcesLimits()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1;

        // Act - Try to exceed maximum
        _reputationService.ModifyReputation(characterId, factionId, 200, "Excessive gain");

        var reputation = _factionService.GetCharacterReputation(characterId, factionId);

        // Assert
        Assert.That(reputation!.ReputationValue, Is.EqualTo(100), "Should clamp at maximum");
        Assert.That(reputation.ReputationTier, Is.EqualTo(FactionReputationTier.Exalted));

        // Act - Try to go below minimum
        _reputationService.ModifyReputation(characterId, factionId, -300, "Excessive loss");

        reputation = _factionService.GetCharacterReputation(characterId, factionId);

        // Assert
        Assert.That(reputation.ReputationValue, Is.EqualTo(-100), "Should clamp at minimum");
        Assert.That(reputation.ReputationTier, Is.EqualTo(FactionReputationTier.Hated));
    }

    [Test]
    public void CompleteWorkflow_DataRecovery_BenefitsJotunReaders()
    {
        // Arrange
        int characterId = 1;

        // Act - Recover data (Jötun-Readers love this)
        _factionService.ProcessWitnessedAction(
            characterId,
            WitnessedActionTypes.RecoverData,
            null,
            "Alfheim"
        );

        var jotunReadersRep = _factionService.GetCharacterReputation(characterId, 3);

        // Assert
        Assert.That(jotunReadersRep!.ReputationValue, Is.GreaterThan(0));
    }

    [Test]
    public void CompleteWorkflow_KnowledgeHoarding_AngersFactions()
    {
        // Arrange
        int characterId = 1;

        // Act - Hoard knowledge (Jötun-Readers hate this)
        _factionService.ProcessWitnessedAction(
            characterId,
            WitnessedActionTypes.HoardKnowledge,
            null,
            "Alfheim"
        );

        var jotunReadersRep = _factionService.GetCharacterReputation(characterId, 3);

        // Assert
        Assert.That(jotunReadersRep!.ReputationValue, Is.LessThan(0), "Hoarding knowledge should anger scholars");
    }

    [Test]
    public void CompleteWorkflow_TradeActions_BuildsRustClansReputation()
    {
        // Arrange
        int characterId = 1;

        // Act - Trade with merchants
        _factionService.ProcessWitnessedAction(
            characterId,
            WitnessedActionTypes.TradeWithMerchant,
            null,
            "Midgard"
        );

        var rustClansRep = _factionService.GetCharacterReputation(characterId, 4);

        // Assert
        Assert.That(rustClansRep!.ReputationValue, Is.GreaterThan(0));
    }
}
