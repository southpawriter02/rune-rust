using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Infrastructure.Providers;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for JsonAbilityTreeProvider.
/// </summary>
/// <remarks>
/// <para>Tests cover all provider operations:</para>
/// <list type="bullet">
///   <item><description>Loading trees from configuration</description></item>
///   <item><description>Tree retrieval by ID and class</description></item>
///   <item><description>Node lookup across trees</description></item>
///   <item><description>Branch lookup by node</description></item>
///   <item><description>Error handling for invalid configuration</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class JsonAbilityTreeProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<JsonAbilityTreeProvider>> _mockLogger = null!;
    private string _tempConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JsonAbilityTreeProvider>>();
        _tempConfigPath = Path.Combine(Path.GetTempPath(), $"ability-trees-{Guid.NewGuid()}.json");
    }

    /// <summary>
    /// Cleans up temporary files after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_tempConfigPath))
        {
            File.Delete(_tempConfigPath);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // LOADING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the provider loads trees from configuration successfully.
    /// </summary>
    [Test]
    public void Constructor_ValidConfig_LoadsTreesSuccessfully()
    {
        // Arrange
        WriteTestConfig();

        // Act
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Assert
        provider.TreeCount.Should().Be(1);
        provider.TotalNodeCount.Should().Be(10);
    }

    /// <summary>
    /// Verifies that the provider throws when config file is not found.
    /// </summary>
    [Test]
    public void Constructor_MissingConfigFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.json");

        // Act
        var act = () => new JsonAbilityTreeProvider(nonExistentPath, _mockLogger.Object);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TREE FOR CLASS TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetTreeForClass returns the correct tree for a class.
    /// </summary>
    [Test]
    public void GetTreeForClass_ExistingClass_ReturnsCorrectTree()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var tree = provider.GetTreeForClass("warrior");

        // Assert
        tree.Should().NotBeNull();
        tree!.ClassId.Should().Be("warrior");
        tree.Name.Should().Be("Warrior Talents");
    }

    /// <summary>
    /// Verifies that GetTreeForClass returns null for unknown class.
    /// </summary>
    [Test]
    public void GetTreeForClass_UnknownClass_ReturnsNull()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var tree = provider.GetTreeForClass("mage");

        // Assert
        tree.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetTreeForClass is case-insensitive.
    /// </summary>
    [Test]
    public void GetTreeForClass_CaseInsensitive_FindsTree()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var tree = provider.GetTreeForClass("WARRIOR");

        // Assert
        tree.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TREE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetTree returns the correct tree by ID.
    /// </summary>
    [Test]
    public void GetTree_ExistingTreeId_ReturnsCorrectTree()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var tree = provider.GetTree("warrior-tree");

        // Assert
        tree.Should().NotBeNull();
        tree!.TreeId.Should().Be("warrior-tree");
    }

    // ═══════════════════════════════════════════════════════════════
    // FIND NODE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that FindNode finds a node across trees.
    /// </summary>
    [Test]
    public void FindNode_ExistingNode_ReturnsCorrectNode()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var node = provider.FindNode("frenzy");

        // Assert
        node.Should().NotBeNull();
        node!.NodeId.Should().Be("frenzy");
        node.Name.Should().Be("Frenzy");
        node.Tier.Should().Be(1);
        node.MaxRank.Should().Be(3);
    }

    /// <summary>
    /// Verifies that FindNode returns null for unknown node.
    /// </summary>
    [Test]
    public void FindNode_UnknownNode_ReturnsNull()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var node = provider.FindNode("nonexistent");

        // Assert
        node.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TREE CONTAINING NODE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetTreeContainingNode returns the correct tree.
    /// </summary>
    [Test]
    public void GetTreeContainingNode_ExistingNode_ReturnsCorrectTree()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var tree = provider.GetTreeContainingNode("frenzy");

        // Assert
        tree.Should().NotBeNull();
        tree!.TreeId.Should().Be("warrior-tree");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BRANCH CONTAINING NODE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetBranchContainingNode returns the correct branch.
    /// </summary>
    [Test]
    public void GetBranchContainingNode_ExistingNode_ReturnsCorrectBranch()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var branch = provider.GetBranchContainingNode("frenzy");

        // Assert
        branch.Should().NotBeNull();
        branch!.BranchId.Should().Be("berserker");
        branch.Name.Should().Be("Berserker");
    }

    /// <summary>
    /// Verifies that GetBranchContainingNode returns null for unknown node.
    /// </summary>
    [Test]
    public void GetBranchContainingNode_UnknownNode_ReturnsNull()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var branch = provider.GetBranchContainingNode("nonexistent");

        // Assert
        branch.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET CLASSES WITH TREES TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetClassesWithTrees returns all classes that have trees.
    /// </summary>
    [Test]
    public void GetClassesWithTrees_ReturnsAllClasses()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Act
        var classes = provider.GetClassesWithTrees();

        // Assert
        classes.Should().HaveCount(1);
        classes.Should().Contain("warrior");
    }

    // ═══════════════════════════════════════════════════════════════
    // HAS TREE FOR CLASS / NODE EXISTS TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasTreeForClass returns true for classes with trees.
    /// </summary>
    [Test]
    public void HasTreeForClass_ExistingClass_ReturnsTrue()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Assert
        provider.HasTreeForClass("warrior").Should().BeTrue();
        provider.HasTreeForClass("mage").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that NodeExists returns true for existing nodes.
    /// </summary>
    [Test]
    public void NodeExists_ExistingNode_ReturnsTrue()
    {
        // Arrange
        WriteTestConfig();
        var provider = new JsonAbilityTreeProvider(_tempConfigPath, _mockLogger.Object);

        // Assert
        provider.NodeExists("frenzy").Should().BeTrue();
        provider.NodeExists("nonexistent").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Writes a test configuration file with a warrior tree.
    /// </summary>
    private void WriteTestConfig()
    {
        var json = """
        {
          "abilityTrees": [
            {
              "treeId": "warrior-tree",
              "classId": "warrior",
              "name": "Warrior Talents",
              "description": "Master of arms and battlefield tactics",
              "pointsPerLevel": 1,
              "icon": "icons/trees/warrior.png",
              "branches": [
                {
                  "branchId": "berserker",
                  "name": "Berserker",
                  "description": "Fury and raw destructive power",
                  "icon": "icons/branches/berserker.png",
                  "nodes": [
                    {
                      "nodeId": "frenzy",
                      "abilityId": "frenzy",
                      "name": "Frenzy",
                      "description": "Enter a battle rage",
                      "tier": 1,
                      "pointCost": 1,
                      "maxRank": 3,
                      "prerequisites": [],
                      "statPrerequisites": [],
                      "position": { "x": 0, "y": 1 }
                    },
                    {
                      "nodeId": "bloodlust",
                      "abilityId": "bloodlust",
                      "name": "Bloodlust",
                      "description": "Heal on kill",
                      "tier": 1,
                      "pointCost": 1,
                      "maxRank": 1
                    },
                    {
                      "nodeId": "rage",
                      "abilityId": "rage",
                      "name": "Rage",
                      "description": "Damage boost",
                      "tier": 2,
                      "pointCost": 2,
                      "maxRank": 1,
                      "prerequisites": ["frenzy"],
                      "statPrerequisites": [{ "stat": "strength", "minValue": 14 }]
                    }
                  ]
                },
                {
                  "branchId": "guardian",
                  "name": "Guardian",
                  "description": "Protection and resilience",
                  "nodes": [
                    {
                      "nodeId": "shield-wall",
                      "abilityId": "shield-wall",
                      "name": "Shield Wall",
                      "description": "Block chance",
                      "tier": 1,
                      "pointCost": 1,
                      "maxRank": 3
                    },
                    {
                      "nodeId": "taunt",
                      "abilityId": "taunt",
                      "name": "Taunt",
                      "description": "Force attack",
                      "tier": 1,
                      "pointCost": 1,
                      "maxRank": 1
                    },
                    {
                      "nodeId": "iron-skin",
                      "abilityId": "iron-skin",
                      "name": "Iron Skin",
                      "description": "Damage reduction",
                      "tier": 2,
                      "pointCost": 2,
                      "maxRank": 1,
                      "prerequisites": ["shield-wall"]
                    },
                    {
                      "nodeId": "aura-of-defense",
                      "abilityId": "aura-of-defense",
                      "name": "Aura of Defense",
                      "description": "Ally protection",
                      "tier": 2,
                      "pointCost": 2,
                      "maxRank": 1,
                      "prerequisites": ["taunt"]
                    }
                  ]
                },
                {
                  "branchId": "champion",
                  "name": "Champion",
                  "description": "Leadership",
                  "nodes": [
                    {
                      "nodeId": "rallying-cry",
                      "abilityId": "rallying-cry",
                      "name": "Rallying Cry",
                      "description": "Ally boost",
                      "tier": 1,
                      "pointCost": 1,
                      "maxRank": 3
                    },
                    {
                      "nodeId": "battle-master",
                      "abilityId": "battle-master",
                      "name": "Battle Master",
                      "description": "Maneuvers",
                      "tier": 1,
                      "pointCost": 1,
                      "maxRank": 1
                    },
                    {
                      "nodeId": "inspire",
                      "abilityId": "inspire",
                      "name": "Inspire",
                      "description": "Extra action",
                      "tier": 2,
                      "pointCost": 2,
                      "maxRank": 1,
                      "prerequisites": ["rallying-cry"],
                      "statPrerequisites": [{ "stat": "charisma", "minValue": 12 }]
                    }
                  ]
                }
              ]
            }
          ]
        }
        """;

        File.WriteAllText(_tempConfigPath, json);
    }
}
