using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Core.Territory;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.35.3: Test suite for TerritorialQuestGenerator
/// Validates quest template generation from events and faction states
/// </summary>
[TestFixture]
public class TerritorialQuestGeneratorTests
{
    private TerritorialQuestGenerator _generator;

    [SetUp]
    public void Setup()
    {
        _generator = new TerritorialQuestGenerator();
    }

    [Test]
    public void GenerateEventQuest_AwakeningRitual_CreatesDisruptQuest()
    {
        // Arrange
        int sectorId = 5;
        string eventType = "Awakening_Ritual";
        string faction = "GodSleeperCultists";

        // Act
        var template = _generator.GenerateEventQuest(sectorId, eventType, faction);

        // Assert
        Assert.That(template, Is.Not.Null);
        Assert.That(template!.QuestName, Is.EqualTo("Disrupt the Awakening Ritual"));
        Assert.That(template.SectorId, Is.EqualTo(sectorId));
        Assert.That(template.ObjectiveType, Is.EqualTo("KillEnemies"));
        Assert.That(template.ObjectiveCount, Is.EqualTo(5));
        Assert.That(template.TargetFaction, Is.EqualTo("GodSleeperCultists"));
        Assert.That(template.TimeLimit, Is.EqualTo(7));
    }

    [Test]
    public void GenerateEventQuest_ExcavationDiscovery_CreatesClaimQuest()
    {
        // Arrange
        int sectorId = 4;
        string eventType = "Excavation_Discovery";
        string faction = "JotunReaders";

        // Act
        var template = _generator.GenerateEventQuest(sectorId, eventType, faction);

        // Assert
        Assert.That(template, Is.Not.Null);
        Assert.That(template!.QuestName, Is.EqualTo("Claim the Pre-Glitch Cache"));
        Assert.That(template.ObjectiveType, Is.EqualTo("ReachLocation"));
        Assert.That(template.RewardItems, Has.Count.EqualTo(2));
        Assert.That(template.RewardItems, Does.Contain("Ancient Data Core"));
        Assert.That(template.TimeLimit, Is.EqualTo(5));
    }

    [Test]
    public void GenerateEventQuest_PurgeCampaign_CreatesJoinQuest()
    {
        // Arrange
        int sectorId = 2;
        string eventType = "Purge_Campaign";
        string faction = "IronBanes";

        // Act
        var template = _generator.GenerateEventQuest(sectorId, eventType, faction);

        // Assert
        Assert.That(template, Is.Not.Null);
        Assert.That(template!.QuestName, Is.EqualTo("Join the Purge"));
        Assert.That(template.ObjectiveCount, Is.EqualTo(15));
        Assert.That(template.TargetFaction, Is.EqualTo("Undying"));
        Assert.That(template.RewardFaction, Is.EqualTo("IronBanes"));
        Assert.That(template.TimeLimit, Is.EqualTo(10));
    }

    [Test]
    public void GenerateEventQuest_Incursion_CreatesDefendQuest()
    {
        // Arrange
        int sectorId = 7;
        string eventType = "Incursion";
        string faction = "JotunReaders";

        // Act
        var template = _generator.GenerateEventQuest(sectorId, eventType, faction);

        // Assert
        Assert.That(template, Is.Not.Null);
        Assert.That(template!.QuestName, Does.Contain("Incursion"));
        Assert.That(template.FactionPenalty, Is.EqualTo(faction));
        Assert.That(template.PenaltyAmount, Is.LessThan(0));
    }

    [Test]
    public void GenerateEventQuest_SupplyRaid_CreatesRecoverQuest()
    {
        // Arrange
        int sectorId = 6;
        string eventType = "Supply_Raid";

        // Act
        var template = _generator.GenerateEventQuest(sectorId, eventType, null);

        // Assert
        Assert.That(template, Is.Not.Null);
        Assert.That(template!.QuestName, Is.EqualTo("Recover Raided Supplies"));
        Assert.That(template.ObjectiveType, Is.EqualTo("KillEnemies"));
        Assert.That(template.TimeLimit, Is.EqualTo(1));
    }

    [Test]
    public void GenerateEventQuest_Catastrophe_CreatesStabilizeQuest()
    {
        // Arrange
        int sectorId = 8;
        string eventType = "Catastrophe";

        // Act
        var template = _generator.GenerateEventQuest(sectorId, eventType, null);

        // Assert
        Assert.That(template, Is.Not.Null);
        Assert.That(template!.QuestName, Is.EqualTo("Stabilize Reality Corruption"));
        Assert.That(template.ObjectiveType, Is.EqualTo("DestroyHazards"));
        Assert.That(template.ObjectiveCount, Is.EqualTo(12));
    }

    [Test]
    public void GenerateEventQuest_ScavengerCaravan_CreatesEscortQuest()
    {
        // Arrange
        int sectorId = 6;
        string eventType = "Scavenger_Caravan";
        string faction = "RustClans";

        // Act
        var template = _generator.GenerateEventQuest(sectorId, eventType, faction);

        // Assert
        Assert.That(template, Is.Not.Null);
        Assert.That(template!.QuestName, Is.EqualTo("Escort the Scavenger Caravan"));
        Assert.That(template.ObjectiveType, Is.EqualTo("Defend"));
        Assert.That(template.RewardFaction, Is.EqualTo("RustClans"));
    }

    [Test]
    public void GenerateEventQuest_UnknownEventType_ReturnsNull()
    {
        // Arrange
        int sectorId = 1;
        string eventType = "Unknown_Event";

        // Act
        var template = _generator.GenerateEventQuest(sectorId, eventType, null);

        // Assert
        Assert.That(template, Is.Null);
    }

    [Test]
    public void GetFactionQuestTemplates_IronBanes_ReturnsPatrolQuests()
    {
        // Arrange
        string faction = "IronBanes";
        int sectorId = 2;

        // Act
        var templates = _generator.GetFactionQuestTemplates(faction, sectorId);

        // Assert
        Assert.That(templates, Is.Not.Null);
        Assert.That(templates, Has.Count.EqualTo(2));
        Assert.That(templates.Any(t => t.QuestName == "Patrol Duty"), Is.True);
    }

    [Test]
    public void GetFactionQuestTemplates_JotunReaders_ReturnsResearchQuests()
    {
        // Arrange
        string faction = "JotunReaders";
        int sectorId = 4;

        // Act
        var templates = _generator.GetFactionQuestTemplates(faction, sectorId);

        // Assert
        Assert.That(templates, Is.Not.Null);
        Assert.That(templates, Has.Count.EqualTo(2));
        Assert.That(templates.Any(t => t.QuestName.Contains("Research")), Is.True);
    }

    [Test]
    public void GetFactionQuestTemplates_RustClans_ReturnsSalvageQuests()
    {
        // Arrange
        string faction = "RustClans";
        int sectorId = 6;

        // Act
        var templates = _generator.GetFactionQuestTemplates(faction, sectorId);

        // Assert
        Assert.That(templates, Is.Not.Null);
        Assert.That(templates, Has.Count.EqualTo(2));
        Assert.That(templates.Any(t => t.QuestName.Contains("Salvage") || t.QuestName.Contains("Trade")), Is.True);
    }

    [Test]
    public void GetFactionQuestTemplates_GodSleeperCultists_ReturnsRitualQuests()
    {
        // Arrange
        string faction = "GodSleeperCultists";
        int sectorId = 5;

        // Act
        var templates = _generator.GetFactionQuestTemplates(faction, sectorId);

        // Assert
        Assert.That(templates, Is.Not.Null);
        Assert.That(templates, Has.Count.EqualTo(1));
        Assert.That(templates[0].QuestName, Does.Contain("Ritual"));
    }

    [Test]
    public void GetFactionQuestTemplates_UnknownFaction_ReturnsEmptyList()
    {
        // Arrange
        string faction = "UnknownFaction";
        int sectorId = 1;

        // Act
        var templates = _generator.GetFactionQuestTemplates(faction, sectorId);

        // Assert
        Assert.That(templates, Is.Not.Null);
        Assert.That(templates, Is.Empty);
    }
}
