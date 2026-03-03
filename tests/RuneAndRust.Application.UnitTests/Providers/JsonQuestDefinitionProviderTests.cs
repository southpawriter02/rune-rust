using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Providers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RuneAndRust.Application.UnitTests.Providers
{
    [TestFixture]
    public class JsonQuestDefinitionProviderTests
    {
        private IQuestDefinitionProvider _provider;
        private Mock<ILogger<JsonQuestDefinitionProvider>> _mockLogger;
        private string _configPath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            _configPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "quests.json");

            if (!File.Exists(_configPath))
            {
                Assert.Ignore($"Configuration file not found at {_configPath}");
            }
        }

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<JsonQuestDefinitionProvider>>();
            _provider = new JsonQuestDefinitionProvider(_configPath, _mockLogger.Object);
        }

        [Test]
        public void Constructor_LoadsQuestsFromConfiguration()
        {
            // Arrange & Act
            var provider = new JsonQuestDefinitionProvider(_configPath, _mockLogger.Object);

            // Assert
            var allQuests = provider.GetAllDefinitions();
            allQuests.Should().NotBeNull();
            allQuests.Count.Should().BeGreaterThanOrEqualTo(10);
        }

        [Test]
        public void GetDefinition_ExistingQuest_ReturnsDefinition()
        {
            // Arrange
            var questId = "quest_clear_nest";

            // Act
            var definition = _provider.GetDefinition(questId);

            // Assert
            definition.Should().NotBeNull();
            definition!.QuestId.Should().Be(questId);
            definition.Name.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetDefinition_NonExistent_ReturnsNull()
        {
            // Arrange
            var questId = "quest_does_not_exist_12345";

            // Act
            var definition = _provider.GetDefinition(questId);

            // Assert
            definition.Should().BeNull();
        }

        [Test]
        public void GetQuestsByFaction_ExistingFaction_ReturnsFilteredList()
        {
            // Arrange
            var faction = "rust-clans";

            // Act
            var quests = _provider.GetQuestsByFaction(faction);

            // Assert
            quests.Should().NotBeNull();
            quests.Should().NotBeEmpty();
            quests.All(q => q.Faction == faction).Should().BeTrue();
        }

        [Test]
        public void GetQuestsByGiver_ExistingNpc_ReturnsQuests()
        {
            // Arrange
            var npcId = "thorvald_guard";

            // Act
            var quests = _provider.GetQuestsByGiver(npcId);

            // Assert
            quests.Should().NotBeNull();
            quests.Should().NotBeEmpty();
            quests.All(q => q.GiverNpcId == npcId).Should().BeTrue();
        }

        [Test]
        public void GetAvailableQuests_FiltersCorrectly()
        {
            // Arrange
            var playerLevel = 1;
            var completedQuestIds = new HashSet<string> { "quest_clear_nest" };

            // Act
            var availableQuests = _provider.GetAvailableQuests(playerLevel, completedQuestIds);

            // Assert
            availableQuests.Should().NotBeNull();
            availableQuests.Should().NotBeEmpty();

            // Verify that completed quests are filtered out
            availableQuests.Any(q => completedQuestIds.Contains(q.QuestId)).Should().BeFalse();

            // Verify that all returned quests meet the level requirement
            availableQuests.All(q => q.MinimumLegend <= playerLevel).Should().BeTrue();
        }
    }
}
