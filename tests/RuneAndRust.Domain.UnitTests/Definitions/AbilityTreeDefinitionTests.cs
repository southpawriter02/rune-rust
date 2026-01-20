using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for AbilityTreeDefinition, AbilityTreeBranch, and AbilityTreeNode.
/// </summary>
/// <remarks>
/// <para>Tests cover all tree structure operations:</para>
/// <list type="bullet">
///   <item><description>Tree creation and validation</description></item>
///   <item><description>Node retrieval and search operations</description></item>
///   <item><description>Branch operations and tier calculations</description></item>
///   <item><description>Point requirement calculations</description></item>
///   <item><description>Prerequisite checking</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class AbilityTreeDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════
    // TREE CREATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AbilityTreeDefinition.Create creates a valid tree with correct properties.
    /// </summary>
    [Test]
    public void Create_ValidParameters_ReturnsTreeWithCorrectProperties()
    {
        // Act
        var tree = AbilityTreeDefinition.Create(
            treeId: "Warrior-Tree",
            classId: "Warrior",
            name: "Warrior Talents",
            description: "Master of arms",
            pointsPerLevel: 2,
            iconPath: "icons/warrior.png");

        // Assert
        tree.Id.Should().NotBeEmpty();
        tree.TreeId.Should().Be("warrior-tree"); // Lowercased
        tree.ClassId.Should().Be("warrior"); // Lowercased
        tree.Name.Should().Be("Warrior Talents");
        tree.Description.Should().Be("Master of arms");
        tree.PointsPerLevel.Should().Be(2);
        tree.IconPath.Should().Be("icons/warrior.png");
        tree.Branches.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that AbilityTreeDefinition.Create throws when treeId is null or empty.
    /// </summary>
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_InvalidTreeId_ThrowsArgumentException(string? treeId)
    {
        // Act
        var act = () => AbilityTreeDefinition.Create(
            treeId: treeId!,
            classId: "warrior",
            name: "Warrior",
            description: "Test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that PointsPerLevel defaults to 1 when invalid value provided.
    /// </summary>
    [Test]
    public void Create_InvalidPointsPerLevel_DefaultsToOne()
    {
        // Act
        var tree = AbilityTreeDefinition.Create(
            treeId: "test-tree",
            classId: "warrior",
            name: "Test",
            description: "Test",
            pointsPerLevel: -5);

        // Assert
        tree.PointsPerLevel.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ALL NODES TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllNodes returns all nodes across all branches.
    /// </summary>
    [Test]
    public void GetAllNodes_ReturnsAllNodesAcrossAllBranches()
    {
        // Arrange
        var tree = CreateWarriorTree();

        // Act
        var allNodes = tree.GetAllNodes().ToList();

        // Assert
        allNodes.Should().HaveCount(10); // 3 + 4 + 3 nodes
        allNodes.Should().Contain(n => n.NodeId == "frenzy");
        allNodes.Should().Contain(n => n.NodeId == "shield-wall");
        allNodes.Should().Contain(n => n.NodeId == "inspire");
    }

    /// <summary>
    /// Verifies that GetAllNodes returns empty when tree has no branches.
    /// </summary>
    [Test]
    public void GetAllNodes_EmptyBranches_ReturnsEmpty()
    {
        // Arrange
        var tree = AbilityTreeDefinition.Create(
            "test-tree", "warrior", "Test", "Description");

        // Act
        var nodes = tree.GetAllNodes();

        // Assert
        nodes.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // FIND NODE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that FindNode returns the correct node when found.
    /// </summary>
    [Test]
    public void FindNode_ExistingNode_ReturnsCorrectNode()
    {
        // Arrange
        var tree = CreateWarriorTree();

        // Act
        var node = tree.FindNode("rage");

        // Assert
        node.Should().NotBeNull();
        node!.NodeId.Should().Be("rage");
        node.Name.Should().Be("Rage");
        node.Tier.Should().Be(2);
    }

    /// <summary>
    /// Verifies that FindNode returns null for unknown node ID.
    /// </summary>
    [Test]
    public void FindNode_UnknownNode_ReturnsNull()
    {
        // Arrange
        var tree = CreateWarriorTree();

        // Act
        var node = tree.FindNode("nonexistent");

        // Assert
        node.Should().BeNull();
    }

    /// <summary>
    /// Verifies that FindNode is case-insensitive.
    /// </summary>
    [Test]
    public void FindNode_CaseInsensitive_FindsNode()
    {
        // Arrange
        var tree = CreateWarriorTree();

        // Act
        var node = tree.FindNode("FRENZY");

        // Assert
        node.Should().NotBeNull();
        node!.NodeId.Should().Be("frenzy");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TOTAL POINTS REQUIRED TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetTotalPointsRequired calculates correctly including multi-rank nodes.
    /// </summary>
    [Test]
    public void GetTotalPointsRequired_CalculatesCorrectly()
    {
        // Arrange
        var tree = CreateWarriorTree();

        // Act
        var totalPoints = tree.GetTotalPointsRequired();

        // Assert
        // Berserker: frenzy(1*3) + bloodlust(1*1) + rage(2*1) = 6
        // Guardian: shield-wall(1*3) + taunt(1*1) + iron-skin(2*1) + aura(2*1) = 8
        // Champion: rallying-cry(1*3) + battle-master(1*1) + inspire(2*1) = 6
        // Total: 6 + 8 + 6 = 20
        totalPoints.Should().Be(20);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET NODES AT TIER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetNodesAtTier returns only nodes at the specified tier.
    /// </summary>
    [Test]
    public void GetNodesAtTier_ReturnsCorrectNodes()
    {
        // Arrange
        var tree = CreateWarriorTree();

        // Act
        var tier1Nodes = tree.GetNodesAtTier(1).ToList();
        var tier2Nodes = tree.GetNodesAtTier(2).ToList();

        // Assert
        tier1Nodes.Should().HaveCount(6); // 2 per branch
        tier1Nodes.Should().OnlyContain(n => n.Tier == 1);

        tier2Nodes.Should().HaveCount(4); // 1 + 2 + 1
        tier2Nodes.Should().OnlyContain(n => n.Tier == 2);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET MAX TIER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetMaxTier returns the highest tier present.
    /// </summary>
    [Test]
    public void GetMaxTier_ReturnsHighestTier()
    {
        // Arrange
        var tree = CreateWarriorTree();

        // Act
        var maxTier = tree.GetMaxTier();

        // Assert
        maxTier.Should().Be(2);
    }

    /// <summary>
    /// Verifies that GetMaxTier returns 0 for empty tree.
    /// </summary>
    [Test]
    public void GetMaxTier_EmptyTree_ReturnsZero()
    {
        // Arrange
        var tree = AbilityTreeDefinition.Create(
            "test-tree", "warrior", "Test", "Description");

        // Act
        var maxTier = tree.GetMaxTier();

        // Assert
        maxTier.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // BRANCH TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AbilityTreeBranch.GetNodesAtTier works correctly.
    /// </summary>
    [Test]
    public void Branch_GetNodesAtTier_ReturnsCorrectNodes()
    {
        // Arrange
        var tree = CreateWarriorTree();
        var berserkerBranch = tree.Branches.First(b => b.BranchId == "berserker");

        // Act
        var tier1Nodes = berserkerBranch.GetNodesAtTier(1).ToList();
        var tier2Nodes = berserkerBranch.GetNodesAtTier(2).ToList();

        // Assert
        tier1Nodes.Should().HaveCount(2);
        tier2Nodes.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that AbilityTreeBranch.GetMaxTier returns correct value.
    /// </summary>
    [Test]
    public void Branch_GetMaxTier_ReturnsCorrectValue()
    {
        // Arrange
        var tree = CreateWarriorTree();
        var berserkerBranch = tree.Branches.First(b => b.BranchId == "berserker");

        // Act
        var maxTier = berserkerBranch.GetMaxTier();

        // Assert
        maxTier.Should().Be(2);
    }

    /// <summary>
    /// Verifies that AbilityTreeBranch.GetTotalPointsRequired calculates correctly.
    /// </summary>
    [Test]
    public void Branch_GetTotalPointsRequired_CalculatesCorrectly()
    {
        // Arrange
        var tree = CreateWarriorTree();
        var berserkerBranch = tree.Branches.First(b => b.BranchId == "berserker");

        // Act
        var totalPoints = berserkerBranch.GetTotalPointsRequired();

        // Assert
        // frenzy(1*3) + bloodlust(1*1) + rage(2*1) = 6
        totalPoints.Should().Be(6);
    }

    // ═══════════════════════════════════════════════════════════════
    // NODE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AbilityTreeNode.HasPrerequisites returns true when prerequisites exist.
    /// </summary>
    [Test]
    public void Node_HasPrerequisites_ReturnsTrue_WhenPrerequisitesExist()
    {
        // Arrange
        var node = CreateNodeWithPrerequisites();

        // Assert
        node.HasPrerequisites.Should().BeTrue();
        node.HasNodePrerequisites.Should().BeTrue();
        node.HasStatPrerequisites.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that AbilityTreeNode.HasPrerequisites returns false when no prerequisites.
    /// </summary>
    [Test]
    public void Node_HasPrerequisites_ReturnsFalse_WhenNoPrerequisites()
    {
        // Arrange
        var node = new AbilityTreeNode
        {
            NodeId = "test",
            AbilityId = "test",
            Name = "Test",
            Description = "Test",
            Tier = 1,
            PrerequisiteNodeIds = [],
            StatPrerequisites = []
        };

        // Assert
        node.HasPrerequisites.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that AbilityTreeNode.IsMultiRank detects MaxRank greater than 1.
    /// </summary>
    [Test]
    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    public void Node_IsMultiRank_DetectsCorrectly(int maxRank, bool expectedIsMultiRank)
    {
        // Arrange
        var node = new AbilityTreeNode
        {
            NodeId = "test",
            AbilityId = "test",
            Name = "Test",
            Description = "Test",
            Tier = 1,
            MaxRank = maxRank
        };

        // Assert
        node.IsMultiRank.Should().Be(expectedIsMultiRank);
    }

    /// <summary>
    /// Verifies that AbilityTreeNode.GetTotalPointsToMax calculates correctly.
    /// </summary>
    [Test]
    public void Node_GetTotalPointsToMax_CalculatesCorrectly()
    {
        // Arrange
        var node = new AbilityTreeNode
        {
            NodeId = "frenzy",
            AbilityId = "frenzy",
            Name = "Frenzy",
            Description = "Battle rage",
            Tier = 1,
            PointCost = 1,
            MaxRank = 3
        };

        // Act
        var totalPoints = node.GetTotalPointsToMax();

        // Assert
        totalPoints.Should().Be(3);
    }

    /// <summary>
    /// Verifies that AbilityTreeNode.RequiresNode checks prerequisites correctly.
    /// </summary>
    [Test]
    public void Node_RequiresNode_ChecksCorrectly()
    {
        // Arrange
        var node = CreateNodeWithPrerequisites();

        // Assert
        node.RequiresNode("frenzy").Should().BeTrue();
        node.RequiresNode("FRENZY").Should().BeTrue(); // Case insensitive
        node.RequiresNode("shield-wall").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that AbilityTreeNode.MeetsStatRequirement checks correctly.
    /// </summary>
    [Test]
    public void Node_MeetsStatRequirement_ChecksCorrectly()
    {
        // Arrange
        var node = CreateNodeWithPrerequisites();

        // Assert
        node.MeetsStatRequirement("strength", 14).Should().BeTrue();
        node.MeetsStatRequirement("strength", 15).Should().BeTrue();
        node.MeetsStatRequirement("strength", 13).Should().BeFalse();
        node.MeetsStatRequirement("dexterity", 10).Should().BeTrue(); // No requirement
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a test warrior tree with 3 branches and 10 nodes.
    /// </summary>
    private static AbilityTreeDefinition CreateWarriorTree()
    {
        var tree = AbilityTreeDefinition.Create(
            treeId: "warrior-tree",
            classId: "warrior",
            name: "Warrior Talents",
            description: "Master of arms",
            pointsPerLevel: 1);

        var berserkerBranch = new AbilityTreeBranch
        {
            BranchId = "berserker",
            Name = "Berserker",
            Description = "Fury and destruction",
            Nodes =
            [
                new AbilityTreeNode
                {
                    NodeId = "frenzy",
                    AbilityId = "frenzy",
                    Name = "Frenzy",
                    Description = "Battle rage",
                    Tier = 1,
                    PointCost = 1,
                    MaxRank = 3,
                    Position = new NodePosition(0, 1)
                },
                new AbilityTreeNode
                {
                    NodeId = "bloodlust",
                    AbilityId = "bloodlust",
                    Name = "Bloodlust",
                    Description = "Heal on kill",
                    Tier = 1,
                    PointCost = 1,
                    MaxRank = 1,
                    Position = new NodePosition(1, 1)
                },
                new AbilityTreeNode
                {
                    NodeId = "rage",
                    AbilityId = "rage",
                    Name = "Rage",
                    Description = "Damage boost",
                    Tier = 2,
                    PointCost = 2,
                    MaxRank = 1,
                    PrerequisiteNodeIds = ["frenzy"],
                    StatPrerequisites = [new StatPrerequisite("strength", 14)],
                    Position = new NodePosition(0, 2)
                }
            ]
        };

        var guardianBranch = new AbilityTreeBranch
        {
            BranchId = "guardian",
            Name = "Guardian",
            Description = "Protection",
            Nodes =
            [
                new AbilityTreeNode
                {
                    NodeId = "shield-wall",
                    AbilityId = "shield-wall",
                    Name = "Shield Wall",
                    Description = "Block chance",
                    Tier = 1,
                    PointCost = 1,
                    MaxRank = 3,
                    Position = new NodePosition(0, 1)
                },
                new AbilityTreeNode
                {
                    NodeId = "taunt",
                    AbilityId = "taunt",
                    Name = "Taunt",
                    Description = "Force attack",
                    Tier = 1,
                    PointCost = 1,
                    MaxRank = 1,
                    Position = new NodePosition(1, 1)
                },
                new AbilityTreeNode
                {
                    NodeId = "iron-skin",
                    AbilityId = "iron-skin",
                    Name = "Iron Skin",
                    Description = "Damage reduction",
                    Tier = 2,
                    PointCost = 2,
                    MaxRank = 1,
                    PrerequisiteNodeIds = ["shield-wall"],
                    Position = new NodePosition(0, 2)
                },
                new AbilityTreeNode
                {
                    NodeId = "aura-of-defense",
                    AbilityId = "aura-of-defense",
                    Name = "Aura of Defense",
                    Description = "Ally protection",
                    Tier = 2,
                    PointCost = 2,
                    MaxRank = 1,
                    PrerequisiteNodeIds = ["taunt"],
                    Position = new NodePosition(1, 2)
                }
            ]
        };

        var championBranch = new AbilityTreeBranch
        {
            BranchId = "champion",
            Name = "Champion",
            Description = "Leadership",
            Nodes =
            [
                new AbilityTreeNode
                {
                    NodeId = "rallying-cry",
                    AbilityId = "rallying-cry",
                    Name = "Rallying Cry",
                    Description = "Ally boost",
                    Tier = 1,
                    PointCost = 1,
                    MaxRank = 3,
                    Position = new NodePosition(0, 1)
                },
                new AbilityTreeNode
                {
                    NodeId = "battle-master",
                    AbilityId = "battle-master",
                    Name = "Battle Master",
                    Description = "Maneuvers",
                    Tier = 1,
                    PointCost = 1,
                    MaxRank = 1,
                    Position = new NodePosition(1, 1)
                },
                new AbilityTreeNode
                {
                    NodeId = "inspire",
                    AbilityId = "inspire",
                    Name = "Inspire",
                    Description = "Extra action",
                    Tier = 2,
                    PointCost = 2,
                    MaxRank = 1,
                    PrerequisiteNodeIds = ["rallying-cry"],
                    StatPrerequisites = [new StatPrerequisite("charisma", 12)],
                    Position = new NodePosition(0, 2)
                }
            ]
        };

        tree.SetBranches([berserkerBranch, guardianBranch, championBranch]);
        return tree;
    }

    /// <summary>
    /// Creates a node with node and stat prerequisites for testing.
    /// </summary>
    private static AbilityTreeNode CreateNodeWithPrerequisites()
    {
        return new AbilityTreeNode
        {
            NodeId = "rage",
            AbilityId = "rage",
            Name = "Rage",
            Description = "Massive damage boost",
            Tier = 2,
            PointCost = 2,
            MaxRank = 1,
            PrerequisiteNodeIds = ["frenzy"],
            StatPrerequisites = [new StatPrerequisite("strength", 14)]
        };
    }
}
