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
    public class JsonNpcDefinitionProviderTests
    {
        private INpcDefinitionProvider _provider;
        private Mock<ILogger<JsonNpcDefinitionProvider>> _mockLogger;
        private string _configPath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            _configPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "npcs.json");

            if (!File.Exists(_configPath))
            {
                Assert.Ignore($"Configuration file not found at {_configPath}");
            }
        }

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<JsonNpcDefinitionProvider>>();
            _provider = new JsonNpcDefinitionProvider(_configPath, _mockLogger.Object);
        }

        [Test]
        public void Constructor_LoadsNpcsFromConfiguration()
        {
            // Arrange & Act
            var provider = new JsonNpcDefinitionProvider(_configPath, _mockLogger.Object);

            // Assert
            var allNpcs = provider.GetAllDefinitions();
            allNpcs.Should().NotBeNull();
            allNpcs.Count.Should().BeGreaterThanOrEqualTo(6);
        }

        [Test]
        public void GetDefinition_ExistingNpc_ReturnsDefinition()
        {
            // Arrange
            var npcId = "thorvald_guard";

            // Act
            var definition = _provider.GetDefinition(npcId);

            // Assert
            definition.Should().NotBeNull();
            definition!.NpcId.Should().Be(npcId);
            definition.Name.Should().NotBeNullOrEmpty();
            definition.Description.Should().NotBeNullOrEmpty();
            definition.InitialGreeting.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetDefinition_NonExistent_ReturnsNull()
        {
            // Arrange
            var npcId = "npc_does_not_exist_12345";

            // Act
            var definition = _provider.GetDefinition(npcId);

            // Assert
            definition.Should().BeNull();
        }

        [Test]
        public void GetNpcsByFaction_MidgardCombine_ReturnsMultiple()
        {
            // Arrange
            var faction = "MidgardCombine";

            // Act
            var npcs = _provider.GetNpcsByFaction(faction);

            // Assert
            npcs.Should().NotBeNull();
            npcs.Should().NotBeEmpty();
            npcs.Count().Should().BeGreaterThan(1);
            npcs.All(n => n.Faction == faction).Should().BeTrue();
        }

        [Test]
        public void GetNpcsByTag_HoldNpc_ReturnsTaggedNpcs()
        {
            // Arrange
            var tag = "hold_npc";

            // Act
            var npcs = _provider.GetNpcsByTag(tag);

            // Assert
            npcs.Should().NotBeNull();
            npcs.Should().NotBeEmpty();
            npcs.All(n => n.Tags != null && n.Tags.Contains(tag)).Should().BeTrue();
        }

        [Test]
        public void NpcExists_ForLoadedNpc_ReturnsTrue()
        {
            // Arrange
            var npcId = "thorvald_guard";

            // Act
            var exists = _provider.NpcExists(npcId);

            // Assert
            exists.Should().BeTrue();
        }

        [Test]
        public void NpcExists_ForNonExistentNpc_ReturnsFalse()
        {
            // Arrange
            var npcId = "npc_does_not_exist_12345";

            // Act
            var exists = _provider.NpcExists(npcId);

            // Assert
            exists.Should().BeFalse();
        }
    }
}
