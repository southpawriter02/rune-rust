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
    public class JsonDialogueProviderTests
    {
        private IDialogueProvider _provider;
        private Mock<ILogger<JsonDialogueProvider>> _mockLogger;
        private string _configPath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            _configPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "dialogues.json");

            if (!File.Exists(_configPath))
            {
                Assert.Ignore($"Configuration file not found at {_configPath}");
            }
        }

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<JsonDialogueProvider>>();
            _provider = new JsonDialogueProvider(_configPath, _mockLogger.Object);
        }

        [Test]
        public void Constructor_LoadsDialoguesFromConfiguration()
        {
            // Arrange & Act
            var provider = new JsonDialogueProvider(_configPath, _mockLogger.Object);

            // Assert
            var allDialogueIds = provider.GetAllDialogueIds();
            allDialogueIds.Should().NotBeNull();
            allDialogueIds.Count().Should().BeGreaterThanOrEqualTo(4);
        }

        [Test]
        public void GetDialogueTree_ExistingTree_ReturnsNodes()
        {
            // Arrange
            var dialogueId = "thorvald_greeting";

            // Act
            var dialogueTree = _provider.GetDialogueTree(dialogueId);

            // Assert
            dialogueTree.Should().NotBeNull();
            dialogueTree.Should().NotBeEmpty();
            dialogueTree!.All(n => n != null).Should().BeTrue();
        }

        [Test]
        public void GetDialogueTree_NonExistent_ReturnsNull()
        {
            // Arrange
            var dialogueId = "dialogue_does_not_exist_12345";

            // Act
            var dialogueTree = _provider.GetDialogueTree(dialogueId);

            // Assert
            dialogueTree.Should().BeNull();
        }

        [Test]
        public void GetNode_ExistingNode_ReturnsCorrectNode()
        {
            // Arrange
            var dialogueId = "thorvald_greeting";
            var dialogueTree = _provider.GetDialogueTree(dialogueId);
            var firstNode = dialogueTree!.FirstOrDefault();
            var nodeId = firstNode?.Id;

            // Act
            var node = _provider.GetNode(dialogueId, nodeId!);

            // Assert
            node.Should().NotBeNull();
            node!.Id.Should().Be(nodeId);
            node.Text.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetNode_NonExistentNode_ReturnsNull()
        {
            // Arrange
            var dialogueId = "thorvald_greeting";
            var nodeId = "node_does_not_exist_12345";

            // Act
            var node = _provider.GetNode(dialogueId, nodeId);

            // Assert
            node.Should().BeNull();
        }

        [Test]
        public void HasDialogue_ForLoadedTree_ReturnsTrue()
        {
            // Arrange
            var dialogueId = "thorvald_greeting";

            // Act
            var hasDialogue = _provider.HasDialogue(dialogueId);

            // Assert
            hasDialogue.Should().BeTrue();
        }

        [Test]
        public void HasDialogue_ForNonExistentTree_ReturnsFalse()
        {
            // Arrange
            var dialogueId = "dialogue_does_not_exist_12345";

            // Act
            var hasDialogue = _provider.HasDialogue(dialogueId);

            // Assert
            hasDialogue.Should().BeFalse();
        }

        [Test]
        public void GetAllDialogueIds_ReturnsAllLoadedIds()
        {
            // Arrange & Act
            var allDialogueIds = _provider.GetAllDialogueIds();

            // Assert
            allDialogueIds.Should().NotBeNull();
            allDialogueIds.Should().NotBeEmpty();
            allDialogueIds.Count().Should().BeGreaterThanOrEqualTo(4);
            allDialogueIds.Should().AllSatisfy(id => id.Should().NotBeNullOrEmpty());
        }
    }
}
