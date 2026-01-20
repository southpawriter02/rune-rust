using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for PrerequisiteValidator.
/// </summary>
/// <remarks>
/// <para>Tests cover all prerequisite validation scenarios:</para>
/// <list type="bullet">
///   <item><description>Node prerequisites (required talents with rank >= 1)</description></item>
///   <item><description>Stat prerequisites (minimum attribute values)</description></item>
///   <item><description>Combined prerequisite validation</description></item>
///   <item><description>Edge cases (no prerequisites, missing nodes)</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class PrerequisiteValidatorTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IAbilityTreeProvider> _mockTreeProvider = null!;
    private Mock<ILogger<PrerequisiteValidator>> _mockLogger = null!;
    private PrerequisiteValidator _validator = null!;

    // Test nodes for prerequisite testing
    private AbilityTreeNode _frenzyNode = null!;
    private AbilityTreeNode _rageNode = null!;
    private AbilityTreeNode _nodeWithStatPrereq = null!;
    private AbilityTreeNode _nodeWithMultiplePrereqs = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockTreeProvider = new Mock<IAbilityTreeProvider>();
        _mockLogger = new Mock<ILogger<PrerequisiteValidator>>();

        // Create test nodes with various prerequisite configurations
        _frenzyNode = new AbilityTreeNode
        {
            NodeId = "frenzy",
            AbilityId = "frenzy-ability",
            Name = "Frenzy",
            Description = "Enter a battle rage",
            Tier = 1,
            PointCost = 1,
            MaxRank = 3,
            PrerequisiteNodeIds = [], // No node prerequisites
            StatPrerequisites = [],   // No stat prerequisites
            Position = NodePosition.Origin
        };

        _rageNode = new AbilityTreeNode
        {
            NodeId = "rage",
            AbilityId = "rage-ability",
            Name = "Rage",
            Description = "Damage boost",
            Tier = 2,
            PointCost = 2,
            MaxRank = 1,
            PrerequisiteNodeIds = ["frenzy"], // Requires frenzy
            StatPrerequisites = [],
            Position = new NodePosition(0, 1)
        };

        _nodeWithStatPrereq = new AbilityTreeNode
        {
            NodeId = "mighty-blow",
            AbilityId = "mighty-blow-ability",
            Name = "Mighty Blow",
            Description = "A powerful strike",
            Tier = 2,
            PointCost = 1,
            MaxRank = 1,
            PrerequisiteNodeIds = [],
            StatPrerequisites = [new StatPrerequisite("might", 14)], // Requires might >= 14
            Position = new NodePosition(1, 0)
        };

        _nodeWithMultiplePrereqs = new AbilityTreeNode
        {
            NodeId = "berserker-mastery",
            AbilityId = "berserker-mastery-ability",
            Name = "Berserker Mastery",
            Description = "Ultimate warrior ability",
            Tier = 3,
            PointCost = 3,
            MaxRank = 1,
            PrerequisiteNodeIds = ["frenzy", "rage"], // Requires both frenzy and rage
            StatPrerequisites = [
                new StatPrerequisite("might", 16),
                new StatPrerequisite("fortitude", 12)
            ],
            Position = new NodePosition(0, 2)
        };

        // Setup tree provider to return node information for failure messages
        _mockTreeProvider.Setup(p => p.FindNode("frenzy")).Returns(_frenzyNode);
        _mockTreeProvider.Setup(p => p.FindNode("rage")).Returns(_rageNode);
        _mockTreeProvider.Setup(p => p.FindNode("mighty-blow")).Returns(_nodeWithStatPrereq);
        _mockTreeProvider.Setup(p => p.FindNode("berserker-mastery")).Returns(_nodeWithMultiplePrereqs);

        _validator = new PrerequisiteValidator(_mockTreeProvider.Object, _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // NO PREREQUISITES TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ValidatePrerequisites returns Valid when node has no prerequisites.
    /// </summary>
    [Test]
    public void ValidatePrerequisites_NoPrerequisites_ReturnsValid()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _validator.ValidatePrerequisites(player, _frenzyNode);

        // Assert
        result.IsValid.Should().BeTrue();
        result.FailureReasons.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // NODE PREREQUISITE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ValidatePrerequisites returns Valid when node prerequisite is met.
    /// </summary>
    [Test]
    public void ValidatePrerequisites_NodePrereqMet_ReturnsValid()
    {
        // Arrange
        var player = CreateTestPlayer();
        // Player has allocated to frenzy at rank 1
        var frenzyAllocation = TalentAllocation.Create("frenzy", 1);
        player.AddTalentAllocation(frenzyAllocation);

        // Act
        var result = _validator.ValidatePrerequisites(player, _rageNode);

        // Assert
        result.IsValid.Should().BeTrue();
        result.FailureReasons.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that ValidatePrerequisites returns Invalid when node prerequisite is not met.
    /// </summary>
    [Test]
    public void ValidatePrerequisites_NodePrereqNotMet_ReturnsInvalid()
    {
        // Arrange
        var player = CreateTestPlayer();
        // Player has NOT allocated to frenzy

        // Act
        var result = _validator.ValidatePrerequisites(player, _rageNode);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReasons.Should().HaveCount(1);
        result.FailureReasons[0].Should().Contain("Frenzy").And.Contain("unlocked");
    }

    /// <summary>
    /// Verifies that MeetsNodePrerequisites returns true when all node prereqs are met.
    /// </summary>
    [Test]
    public void MeetsNodePrerequisites_AllMet_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentAllocation(TalentAllocation.Create("frenzy", 1));

        // Act
        var result = _validator.MeetsNodePrerequisites(player, _rageNode);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that MeetsNodePrerequisites returns false when a node prereq is not met.
    /// </summary>
    [Test]
    public void MeetsNodePrerequisites_NotMet_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();
        // No allocations

        // Act
        var result = _validator.MeetsNodePrerequisites(player, _rageNode);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // STAT PREREQUISITE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ValidatePrerequisites returns Valid when stat prerequisite is met.
    /// </summary>
    [Test]
    public void ValidatePrerequisites_StatPrereqMet_ReturnsValid()
    {
        // Arrange - player with might >= 14
        var player = CreateTestPlayerWithAttributes(
            might: 15, fortitude: 10, will: 10, wits: 10, finesse: 10);

        // Act
        var result = _validator.ValidatePrerequisites(player, _nodeWithStatPrereq);

        // Assert
        result.IsValid.Should().BeTrue();
        result.FailureReasons.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that ValidatePrerequisites returns Invalid when stat prerequisite is not met.
    /// </summary>
    [Test]
    public void ValidatePrerequisites_StatPrereqNotMet_ReturnsInvalid()
    {
        // Arrange - player with might < 14
        var player = CreateTestPlayerWithAttributes(
            might: 10, fortitude: 10, will: 10, wits: 10, finesse: 10);

        // Act
        var result = _validator.ValidatePrerequisites(player, _nodeWithStatPrereq);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReasons.Should().HaveCount(1);
        result.FailureReasons[0].Should().Contain("Might").And.Contain("14").And.Contain("10");
    }

    /// <summary>
    /// Verifies that MeetsStatPrerequisites returns true when all stat prereqs are met.
    /// </summary>
    [Test]
    public void MeetsStatPrerequisites_AllMet_ReturnsTrue()
    {
        // Arrange - player with sufficient stats
        var player = CreateTestPlayerWithAttributes(
            might: 18, fortitude: 14, will: 10, wits: 10, finesse: 10);

        // Act
        var result = _validator.MeetsStatPrerequisites(player, _nodeWithMultiplePrereqs);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that MeetsStatPrerequisites returns false when any stat prereq is not met.
    /// </summary>
    [Test]
    public void MeetsStatPrerequisites_NotMet_ReturnsFalse()
    {
        // Arrange - player with insufficient fortitude
        var player = CreateTestPlayerWithAttributes(
            might: 18, fortitude: 10, will: 10, wits: 10, finesse: 10);

        // Act
        var result = _validator.MeetsStatPrerequisites(player, _nodeWithMultiplePrereqs);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // COMBINED PREREQUISITES TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ValidatePrerequisites returns Valid when all prerequisites are met.
    /// </summary>
    [Test]
    public void ValidatePrerequisites_AllPrerequisitesMet_ReturnsValid()
    {
        // Arrange - player with required allocations and stats
        var player = CreateTestPlayerWithAttributes(
            might: 18, fortitude: 14, will: 10, wits: 10, finesse: 10);
        player.AddTalentAllocation(TalentAllocation.Create("frenzy", 1));
        player.AddTalentAllocation(TalentAllocation.Create("rage", 1));

        // Act
        var result = _validator.ValidatePrerequisites(player, _nodeWithMultiplePrereqs);

        // Assert
        result.IsValid.Should().BeTrue();
        result.FailureReasons.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that ValidatePrerequisites collects all failure reasons.
    /// </summary>
    [Test]
    public void ValidatePrerequisites_MultipleFailures_ReturnsAllReasons()
    {
        // Arrange - player missing node prereqs and stat prereqs
        var player = CreateTestPlayerWithAttributes(
            might: 10, fortitude: 10, will: 10, wits: 10, finesse: 10);
        // Player has neither frenzy nor rage allocated
        // Player has neither might >= 16 nor fortitude >= 12

        // Act
        var result = _validator.ValidatePrerequisites(player, _nodeWithMultiplePrereqs);

        // Assert
        result.IsValid.Should().BeFalse();
        // Should have 4 failure reasons: 2 nodes + 2 stats
        result.FailureReasons.Should().HaveCount(4);
        result.FailureReasons.Should().Contain(r => r.Contains("Frenzy"));
        result.FailureReasons.Should().Contain(r => r.Contains("Rage"));
        result.FailureReasons.Should().Contain(r => r.Contains("Might"));
        result.FailureReasons.Should().Contain(r => r.Contains("Fortitude"));
    }

    // ═══════════════════════════════════════════════════════════════
    // EDGE CASES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ValidatePrerequisites handles missing prerequisite node gracefully.
    /// </summary>
    [Test]
    public void ValidatePrerequisites_MissingPrereqNodeDefinition_UsesNodeIdInMessage()
    {
        // Arrange
        var player = CreateTestPlayer();
        var nodeWithMissingPrereq = new AbilityTreeNode
        {
            NodeId = "test-node",
            AbilityId = "test-ability",
            Name = "Test Node",
            Description = "Test",
            Tier = 2,
            PointCost = 1,
            MaxRank = 1,
            PrerequisiteNodeIds = ["nonexistent-node"],
            StatPrerequisites = [],
            Position = NodePosition.Origin
        };

        // Setup provider to return null for unknown node
        _mockTreeProvider.Setup(p => p.FindNode("nonexistent-node")).Returns((AbilityTreeNode?)null);

        // Act
        var result = _validator.ValidatePrerequisites(player, nodeWithMissingPrereq);

        // Assert
        result.IsValid.Should().BeFalse();
        // Should use node ID as fallback when node name is not available
        result.FailureReasons[0].Should().Contain("nonexistent-node");
    }

    /// <summary>
    /// Verifies that MeetsNodePrerequisites returns true for nodes without node prerequisites.
    /// </summary>
    [Test]
    public void MeetsNodePrerequisites_NoNodePrerequisites_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act - frenzy has no node prerequisites
        var result = _validator.MeetsNodePrerequisites(player, _frenzyNode);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that MeetsStatPrerequisites returns true for nodes without stat prerequisites.
    /// </summary>
    [Test]
    public void MeetsStatPrerequisites_NoStatPrerequisites_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act - frenzy has no stat prerequisites
        var result = _validator.MeetsStatPrerequisites(player, _frenzyNode);

        // Assert
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a test player with default stats.
    /// </summary>
    private static Player CreateTestPlayer()
    {
        return new Player("TestPlayer", new Stats(100, 10, 5));
    }

    /// <summary>
    /// Creates a test player with specific attribute values.
    /// </summary>
    /// <param name="might">The player's might attribute.</param>
    /// <param name="fortitude">The player's fortitude attribute.</param>
    /// <param name="will">The player's will attribute.</param>
    /// <param name="wits">The player's wits attribute.</param>
    /// <param name="finesse">The player's finesse attribute.</param>
    /// <returns>A Player instance with the specified attributes.</returns>
    private static Player CreateTestPlayerWithAttributes(
        int might, int fortitude, int will, int wits, int finesse)
    {
        // Use the constructor overload that accepts attributes
        return new Player(
            "TestPlayer",
            "human",
            "soldier",
            new PlayerAttributes(might, fortitude, will, wits, finesse));
    }
}
