using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for TalentPointService.
/// </summary>
/// <remarks>
/// <para>Tests cover all talent point operations:</para>
/// <list type="bullet">
///   <item><description>Point queries (unspent, earned, spent)</description></item>
///   <item><description>Point awarding and spending</description></item>
///   <item><description>Allocation tracking and queries</description></item>
///   <item><description>Eligibility checks (CanSpendOn)</description></item>
///   <item><description>Event logging</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class TalentPointServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IAbilityTreeProvider> _mockTreeProvider = null!;
    private Mock<IPrerequisiteValidator> _mockPrerequisiteValidator = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<TalentPointService>> _mockLogger = null!;
    private TalentPointService _service = null!;

    // Test tree and nodes
    private AbilityTreeDefinition _warriorTree = null!;
    private AbilityTreeNode _frenzyNode = null!;
    private AbilityTreeNode _rageNode = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockTreeProvider = new Mock<IAbilityTreeProvider>();
        _mockPrerequisiteValidator = new Mock<IPrerequisiteValidator>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<TalentPointService>>();

        // Create test nodes
        _frenzyNode = new AbilityTreeNode
        {
            NodeId = "frenzy",
            AbilityId = "frenzy-ability",
            Name = "Frenzy",
            Description = "Enter a battle rage",
            Tier = 1,
            PointCost = 1,
            MaxRank = 3,
            PrerequisiteNodeIds = [],
            StatPrerequisites = [],
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
            PrerequisiteNodeIds = ["frenzy"],
            StatPrerequisites = [new StatPrerequisite("strength", 14)],
            Position = new NodePosition(0, 1)
        };

        // Create test tree
        var berserkerBranch = new AbilityTreeBranch
        {
            BranchId = "berserker",
            Name = "Berserker",
            Description = "Fury and destruction",
            Nodes = [_frenzyNode, _rageNode]
        };

        _warriorTree = AbilityTreeDefinition.Create(
            treeId: "warrior-tree",
            classId: "warrior",
            name: "Warrior Talents",
            description: "Master of arms",
            pointsPerLevel: 1);
        _warriorTree.SetBranches([berserkerBranch]);

        // Setup default provider behavior
        SetupDefaultTreeProvider();

        // Setup default prerequisite validator (always valid)
        _mockPrerequisiteValidator
            .Setup(v => v.ValidatePrerequisites(It.IsAny<Player>(), It.IsAny<AbilityTreeNode>()))
            .Returns(PrerequisiteResult.Valid());

        _service = new TalentPointService(
            _mockTreeProvider.Object,
            _mockPrerequisiteValidator.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // POINT QUERY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetUnspentPoints returns the correct value.
    /// </summary>
    [Test]
    public void GetUnspentPoints_ReturnsCorrectValue()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);

        // Act
        var result = _service.GetUnspentPoints(player);

        // Assert
        result.Should().Be(5);
    }

    /// <summary>
    /// Verifies that GetTotalPointsEarned returns the correct value.
    /// </summary>
    [Test]
    public void GetTotalPointsEarned_ReturnsCorrectValue()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SpendTalentPoints(2);

        // Act
        var result = _service.GetTotalPointsEarned(player);

        // Assert
        result.Should().Be(5);
    }

    /// <summary>
    /// Verifies that GetTotalPointsSpent calculates correctly across allocations.
    /// </summary>
    [Test]
    public void GetTotalPointsSpent_CalculatesCorrectlyAcrossAllocations()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(10);

        // Create some allocations
        var allocation1 = TalentAllocation.Create("frenzy", 1);
        allocation1.IncrementRank(); // rank 2
        player.AddTalentAllocation(allocation1);

        var allocation2 = TalentAllocation.Create("rage", 2);
        player.AddTalentAllocation(allocation2);

        // Act
        var result = _service.GetTotalPointsSpent(player);

        // Assert - frenzy: 2 ranks * 1 cost = 2, rage: 1 rank * 2 cost = 2, total = 4
        result.Should().Be(4);
    }

    // ═══════════════════════════════════════════════════════════════
    // AWARD POINTS TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AwardPoints increases both unspent and total earned.
    /// </summary>
    [Test]
    public void AwardPoints_IncreasesUnspentAndTotalEarned()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        _service.AwardPoints(player, 3);

        // Assert
        player.UnspentTalentPoints.Should().Be(3);
        player.TotalTalentPointsEarned.Should().Be(3);
    }

    /// <summary>
    /// Verifies that AwardPoints logs the event.
    /// </summary>
    [Test]
    public void AwardPoints_LogsCharacterEvent()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        _service.AwardPoints(player, 2);

        // Assert
        _mockEventLogger.Verify(
            l => l.LogCharacter(
                "TalentPointEarned",
                It.IsAny<string>(),
                player.Id,
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // SPEND POINT TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SpendPoint creates a new allocation for unallocated nodes.
    /// </summary>
    [Test]
    public void SpendPoint_CreatesNewAllocationForUnallocatedNode()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        // Act
        var result = _service.SpendPoint(player, "frenzy");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ResultType.Should().Be(AllocationResultType.Success);
        result.NodeId.Should().Be("frenzy");
        result.NewRank.Should().Be(1);

        var allocation = player.GetAllocation("frenzy");
        allocation.Should().NotBeNull();
        allocation!.CurrentRank.Should().Be(1);
    }

    /// <summary>
    /// Verifies that SpendPoint increments existing allocation rank.
    /// </summary>
    [Test]
    public void SpendPoint_IncrementsExistingAllocationRank()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        // Allocate first rank
        _service.SpendPoint(player, "frenzy");

        // Act - allocate second rank
        var result = _service.SpendPoint(player, "frenzy");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.NewRank.Should().Be(2);

        var allocation = player.GetAllocation("frenzy");
        allocation!.CurrentRank.Should().Be(2);
    }

    /// <summary>
    /// Verifies that SpendPoint deducts points from player.
    /// </summary>
    [Test]
    public void SpendPoint_DeductsPointsFromPlayer()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        // Act
        _service.SpendPoint(player, "frenzy");

        // Assert
        player.UnspentTalentPoints.Should().Be(4);
    }

    /// <summary>
    /// Verifies that SpendPoint fails with insufficient points.
    /// </summary>
    [Test]
    public void SpendPoint_FailsWithInsufficientPoints()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(1);
        player.SetClass("warrior", "warrior");

        // Act - rage costs 2 points, player has 1
        var result = _service.SpendPoint(player, "rage");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(AllocationResultType.InsufficientPoints);
        result.FailureReason.Should().Contain("Need 2 points").And.Contain("have 1");
    }

    /// <summary>
    /// Verifies that SpendPoint fails at max rank.
    /// </summary>
    [Test]
    public void SpendPoint_FailsAtMaxRank()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(10);
        player.SetClass("warrior", "warrior");

        // Allocate all 3 ranks of frenzy
        _service.SpendPoint(player, "frenzy");
        _service.SpendPoint(player, "frenzy");
        _service.SpendPoint(player, "frenzy");

        // Act - try to allocate 4th rank
        var result = _service.SpendPoint(player, "frenzy");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(AllocationResultType.AtMaxRank);
        result.FailureReason.Should().Contain("max rank 3");
    }

    /// <summary>
    /// Verifies that SpendPoint fails when node is not found.
    /// </summary>
    [Test]
    public void SpendPoint_FailsWhenNodeNotFound()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);

        // Setup provider to return null for unknown node
        _mockTreeProvider.Setup(p => p.FindNode("nonexistent")).Returns((AbilityTreeNode?)null);

        // Act
        var result = _service.SpendPoint(player, "nonexistent");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(AllocationResultType.Failed);
        result.FailureReason.Should().Contain("not found");
    }

    /// <summary>
    /// Verifies that SpendPoint fails when prerequisites are not met.
    /// </summary>
    [Test]
    public void SpendPoint_FailsWithoutPrerequisites()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        // Setup prerequisite validator to fail
        _mockPrerequisiteValidator
            .Setup(v => v.ValidatePrerequisites(player, _rageNode))
            .Returns(PrerequisiteResult.Invalid(["Requires Frenzy rank 1"]));

        // Act
        var result = _service.SpendPoint(player, "rage");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(AllocationResultType.PrerequisitesNotMet);
        result.FailureReason.Should().Contain("Frenzy");
    }

    /// <summary>
    /// Verifies that SpendPoint logs TalentPointSpentEvent.
    /// </summary>
    [Test]
    public void SpendPoint_LogsTalentPointSpentEvent()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        // Act
        _service.SpendPoint(player, "frenzy");

        // Assert
        _mockEventLogger.Verify(
            l => l.LogCharacter(
                "TalentPointSpent",
                It.IsAny<string>(),
                player.Id,
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that SpendPoint logs AbilityUnlockedEvent on first rank.
    /// </summary>
    [Test]
    public void SpendPoint_LogsAbilityUnlockedEventOnFirstRank()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        // Act
        _service.SpendPoint(player, "frenzy");

        // Assert
        _mockEventLogger.Verify(
            l => l.LogAbility(
                "AbilityUnlocked",
                It.Is<string>(s => s.Contains("frenzy")),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that SpendPoint does not log AbilityUnlockedEvent on subsequent ranks.
    /// </summary>
    [Test]
    public void SpendPoint_DoesNotLogAbilityUnlockedOnSubsequentRanks()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        // Allocate first rank
        _service.SpendPoint(player, "frenzy");

        // Reset mock to clear first call
        _mockEventLogger.Invocations.Clear();

        // Act - allocate second rank
        _service.SpendPoint(player, "frenzy");

        // Assert - AbilityUnlocked should not be called
        _mockEventLogger.Verify(
            l => l.LogAbility(
                "AbilityUnlocked",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // ALLOCATION QUERY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetNodeRank returns 0 for unallocated nodes.
    /// </summary>
    [Test]
    public void GetNodeRank_ReturnsZeroForUnallocatedNode()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.GetNodeRank(player, "frenzy");

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetNodeRank returns the correct rank for allocated nodes.
    /// </summary>
    [Test]
    public void GetNodeRank_ReturnsCorrectRankForAllocatedNode()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        _service.SpendPoint(player, "frenzy");
        _service.SpendPoint(player, "frenzy");

        // Act
        var result = _service.GetNodeRank(player, "frenzy");

        // Assert
        result.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // ELIGIBILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CanSpendOn returns true when all conditions are met.
    /// </summary>
    [Test]
    public void CanSpendOn_ReturnsTrueWhenAllConditionsMet()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        // Act
        var result = _service.CanSpendOn(player, "frenzy");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CanSpendOn returns false at max rank.
    /// </summary>
    [Test]
    public void CanSpendOn_ReturnsFalseAtMaxRank()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(10);
        player.SetClass("warrior", "warrior");

        // Max out frenzy
        _service.SpendPoint(player, "frenzy");
        _service.SpendPoint(player, "frenzy");
        _service.SpendPoint(player, "frenzy");

        // Act
        var result = _service.CanSpendOn(player, "frenzy");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetAvailableNodes returns only nodes the player can spend on.
    /// </summary>
    [Test]
    public void GetAvailableNodes_ReturnsOnlySpendableNodes()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(1); // Only 1 point, can't afford rage (costs 2)
        player.SetClass("warrior", "warrior");

        // Act
        var result = _service.GetAvailableNodes(player);

        // Assert
        result.Should().HaveCount(1);
        result[0].NodeId.Should().Be("frenzy");
    }

    // ═══════════════════════════════════════════════════════════════
    // POINT SUMMARY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetPointSummary returns correct values.
    /// </summary>
    [Test]
    public void GetPointSummary_ReturnsCorrectValues()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.AddTalentPoints(5);
        player.SetClass("warrior", "warrior");

        _service.SpendPoint(player, "frenzy");
        _service.SpendPoint(player, "frenzy");

        // Act
        var summary = _service.GetPointSummary(player);

        // Assert
        summary.Unspent.Should().Be(3); // 5 - 2
        summary.TotalEarned.Should().Be(5);
        summary.TotalSpent.Should().Be(2); // 2 ranks at 1 point each
        summary.AllocatedNodes.Should().Be(1);
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
    /// Sets up the default tree provider mocks.
    /// </summary>
    private void SetupDefaultTreeProvider()
    {
        _mockTreeProvider.Setup(p => p.TreeCount).Returns(1);
        _mockTreeProvider.Setup(p => p.FindNode("frenzy")).Returns(_frenzyNode);
        _mockTreeProvider.Setup(p => p.FindNode("rage")).Returns(_rageNode);
        _mockTreeProvider.Setup(p => p.GetTree("warrior-tree")).Returns(_warriorTree);
        _mockTreeProvider.Setup(p => p.GetTreeForClass("warrior")).Returns(_warriorTree);
        _mockTreeProvider.Setup(p => p.NodeExists("frenzy")).Returns(true);
        _mockTreeProvider.Setup(p => p.NodeExists("rage")).Returns(true);
    }
}
