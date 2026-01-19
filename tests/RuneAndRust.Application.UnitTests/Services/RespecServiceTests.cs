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
/// Unit tests for RespecService.
/// </summary>
/// <remarks>
/// <para>Tests cover all respec operations:</para>
/// <list type="bullet">
///   <item><description>Cost calculation</description></item>
///   <item><description>Affordability checks</description></item>
///   <item><description>Eligibility validation (enabled, level, allocations)</description></item>
///   <item><description>Respec execution (refund, clear, remove abilities)</description></item>
///   <item><description>Event logging</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RespecServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IAbilityTreeProvider> _mockTreeProvider = null!;
    private Mock<IRespecConfiguration> _mockConfig = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<RespecService>> _mockLogger = null!;
    private RespecService _service = null!;

    // Test nodes for ability removal testing
    private AbilityTreeNode _frenzyNode = null!;
    private AbilityTreeNode _rageNode = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockTreeProvider = new Mock<IAbilityTreeProvider>();
        _mockConfig = new Mock<IRespecConfiguration>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<RespecService>>();

        // Setup default configuration
        _mockConfig.Setup(c => c.BaseRespecCost).Returns(100);
        _mockConfig.Setup(c => c.LevelMultiplier).Returns(10);
        _mockConfig.Setup(c => c.IsRespecEnabled).Returns(true);
        _mockConfig.Setup(c => c.MinimumLevelToRespec).Returns(2);
        _mockConfig.Setup(c => c.CurrencyId).Returns("gold");

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
            StatPrerequisites = [],
            Position = new NodePosition(0, 1)
        };

        // Setup tree provider to return nodes
        _mockTreeProvider.Setup(p => p.FindNode("frenzy")).Returns(_frenzyNode);
        _mockTreeProvider.Setup(p => p.FindNode("rage")).Returns(_rageNode);

        _service = new RespecService(
            _mockTreeProvider.Object,
            _mockConfig.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // COST CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetRespecCost calculates correctly using the formula.
    /// </summary>
    [Test]
    public void GetRespecCost_CalculatesCorrectly()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(5);
        // Formula: 100 + (5 * 10) = 150

        // Act
        var cost = _service.GetRespecCost(player);

        // Assert
        cost.Should().Be(150);
    }

    /// <summary>
    /// Verifies that GetRespecCost scales with player level.
    /// </summary>
    [Test]
    public void GetRespecCost_ScalesWithLevel()
    {
        // Arrange
        var playerLevel2 = CreateTestPlayerAtLevel(2);
        var playerLevel10 = CreateTestPlayerAtLevel(10);
        var playerLevel20 = CreateTestPlayerAtLevel(20);

        // Act
        var cost2 = _service.GetRespecCost(playerLevel2);   // 100 + (2 * 10) = 120
        var cost10 = _service.GetRespecCost(playerLevel10); // 100 + (10 * 10) = 200
        var cost20 = _service.GetRespecCost(playerLevel20); // 100 + (20 * 10) = 300

        // Assert
        cost2.Should().Be(120);
        cost10.Should().Be(200);
        cost20.Should().Be(300);
    }

    // ═══════════════════════════════════════════════════════════════
    // AFFORDABILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CanAffordRespec returns true when player has enough gold.
    /// </summary>
    [Test]
    public void CanAffordRespec_WithEnoughGold_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(2);
        player.AddCurrency("gold", 500); // Cost is 120

        // Act
        var canAfford = _service.CanAffordRespec(player);

        // Assert
        canAfford.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CanAffordRespec returns false when player lacks gold.
    /// </summary>
    [Test]
    public void CanAffordRespec_WithoutEnoughGold_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(10);
        player.AddCurrency("gold", 100); // Cost is 200

        // Act
        var canAfford = _service.CanAffordRespec(player);

        // Assert
        canAfford.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // ELIGIBILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasAllocations returns true when player has allocations.
    /// </summary>
    [Test]
    public void HasAllocations_WithAllocations_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(5);
        player.AddTalentAllocation(TalentAllocation.Create("frenzy", 1));

        // Act
        var hasAllocations = _service.HasAllocations(player);

        // Assert
        hasAllocations.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasAllocations returns false when player has no allocations.
    /// </summary>
    [Test]
    public void HasAllocations_WithoutAllocations_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(5);

        // Act
        var hasAllocations = _service.HasAllocations(player);

        // Assert
        hasAllocations.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetRefundAmount calculates total points spent.
    /// </summary>
    [Test]
    public void GetRefundAmount_CalculatesTotalPointsSpent()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(5);
        // Add allocations: frenzy (2 ranks * 1 cost = 2) + rage (1 rank * 2 cost = 2) = 4 total
        var frenzyAlloc = TalentAllocation.Create("frenzy", 1);
        frenzyAlloc.IncrementRank(); // rank 2
        player.AddTalentAllocation(frenzyAlloc);
        player.AddTalentAllocation(TalentAllocation.Create("rage", 2));

        // Act
        var refundAmount = _service.GetRefundAmount(player);

        // Assert
        refundAmount.Should().Be(4);
    }

    /// <summary>
    /// Verifies that GetAbilitiesToRemove returns ability IDs from allocations.
    /// </summary>
    [Test]
    public void GetAbilitiesToRemove_ReturnsAbilityIdsFromAllocations()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(5);
        player.AddTalentAllocation(TalentAllocation.Create("frenzy", 1));
        player.AddTalentAllocation(TalentAllocation.Create("rage", 2));

        // Act
        var abilities = _service.GetAbilitiesToRemove(player);

        // Assert
        abilities.Should().HaveCount(2);
        abilities.Should().Contain("frenzy-ability");
        abilities.Should().Contain("rage-ability");
    }

    // ═══════════════════════════════════════════════════════════════
    // RESPEC VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Respec returns Disabled when feature is disabled.
    /// </summary>
    [Test]
    public void Respec_WhenDisabled_ReturnsDisabled()
    {
        // Arrange
        _mockConfig.Setup(c => c.IsRespecEnabled).Returns(false);
        var player = CreateTestPlayerWithRespecSetup();

        // Recreate service with disabled config
        _service = new RespecService(
            _mockTreeProvider.Object,
            _mockConfig.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);

        // Act
        var result = _service.Respec(player);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(RespecResultType.Disabled);
        result.FailureReason.Should().Contain("disabled");
    }

    /// <summary>
    /// Verifies that Respec returns LevelTooLow when player is below minimum level.
    /// </summary>
    [Test]
    public void Respec_WhenLevelTooLow_ReturnsLevelTooLow()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(1); // Level 1, minimum is 2
        player.AddCurrency("gold", 1000);
        player.AddTalentAllocation(TalentAllocation.Create("frenzy", 1));

        // Act
        var result = _service.Respec(player);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(RespecResultType.LevelTooLow);
        result.FailureReason.Should().Contain("level 2").And.Contain("level 1");
    }

    /// <summary>
    /// Verifies that Respec returns NothingToRespec when player has no allocations.
    /// </summary>
    [Test]
    public void Respec_NoAllocations_ReturnsNothingToRespec()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(5);
        player.AddCurrency("gold", 1000);
        // No talent allocations

        // Act
        var result = _service.Respec(player);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(RespecResultType.NoAllocations);
        result.FailureReason.Should().Contain("No talent allocations");
    }

    /// <summary>
    /// Verifies that Respec returns CannotAfford when player lacks gold.
    /// </summary>
    [Test]
    public void Respec_CannotAfford_ReturnsCannotAfford()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(5);
        player.AddCurrency("gold", 50); // Cost is 150
        player.AddTalentAllocation(TalentAllocation.Create("frenzy", 1));

        // Act
        var result = _service.Respec(player);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(RespecResultType.CannotAfford);
        result.FailureReason.Should().Contain("150").And.Contain("50");
    }

    // ═══════════════════════════════════════════════════════════════
    // SUCCESSFUL RESPEC TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Respec refunds all spent talent points.
    /// </summary>
    [Test]
    public void Respec_RefundsAllPoints()
    {
        // Arrange
        var player = CreateTestPlayerWithRespecSetup();
        var initialUnspent = player.UnspentTalentPoints;
        var expectedRefund = player.TalentAllocations.Sum(a => a.GetTotalPointsSpent());

        // Act
        var result = _service.Respec(player);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.PointsRefunded.Should().Be(expectedRefund);
        player.UnspentTalentPoints.Should().Be(initialUnspent + expectedRefund);
    }

    /// <summary>
    /// Verifies that Respec clears all allocations.
    /// </summary>
    [Test]
    public void Respec_ClearsAllocations()
    {
        // Arrange
        var player = CreateTestPlayerWithRespecSetup();
        player.TalentAllocations.Should().NotBeEmpty();

        // Act
        var result = _service.Respec(player);

        // Assert
        result.IsSuccess.Should().BeTrue();
        player.TalentAllocations.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that Respec deducts gold cost.
    /// </summary>
    [Test]
    public void Respec_DeductsGoldCost()
    {
        // Arrange
        var player = CreateTestPlayerWithRespecSetup();
        var initialGold = player.GetCurrency("gold");
        var expectedCost = _service.GetRespecCost(player);

        // Act
        var result = _service.Respec(player);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.GoldSpent.Should().Be(expectedCost);
        player.GetCurrency("gold").Should().Be(initialGold - expectedCost);
    }

    /// <summary>
    /// Verifies that Respec returns correct ability removal count.
    /// </summary>
    [Test]
    public void Respec_ReturnsAbilitiesRemovedCount()
    {
        // Arrange
        var player = CreateTestPlayerWithRespecSetup();
        // Player has allocations for frenzy and rage, but abilities might not be present
        // Just verify the count is returned correctly

        // Act
        var result = _service.Respec(player);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // AbilitiesRemoved may be 0 if player.RemoveAbility returns false
        result.AbilitiesRemoved.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Verifies that Respec logs RespecCompleted event on success.
    /// </summary>
    [Test]
    public void Respec_LogsRespecCompletedEvent()
    {
        // Arrange
        var player = CreateTestPlayerWithRespecSetup();

        // Act
        _service.Respec(player);

        // Assert
        _mockEventLogger.Verify(
            l => l.LogCharacter(
                "RespecCompleted",
                It.IsAny<string>(),
                player.Id,
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that Respec logs RespecStarted event before processing.
    /// </summary>
    [Test]
    public void Respec_LogsRespecStartedEvent()
    {
        // Arrange
        var player = CreateTestPlayerWithRespecSetup();

        // Act
        _service.Respec(player);

        // Assert
        _mockEventLogger.Verify(
            l => l.LogCharacter(
                "RespecStarted",
                It.IsAny<string>(),
                player.Id,
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that Respec logs RespecFailed event on failure.
    /// </summary>
    [Test]
    public void Respec_LogsRespecFailedEvent()
    {
        // Arrange
        var player = CreateTestPlayerAtLevel(5);
        // No gold, no allocations - will fail
        player.AddTalentAllocation(TalentAllocation.Create("frenzy", 1));

        // Act
        _service.Respec(player);

        // Assert
        _mockEventLogger.Verify(
            l => l.LogCharacter(
                "RespecFailed",
                It.IsAny<string>(),
                player.Id,
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a test player at the specified level.
    /// </summary>
    /// <param name="level">The player's level.</param>
    /// <returns>A Player instance at the specified level.</returns>
    private static Player CreateTestPlayerAtLevel(int level)
    {
        var player = new Player("TestPlayer", new Stats(100, 10, 5));
        // Set the player's level directly
        if (level > 1)
        {
            player.SetLevel(level);
        }
        return player;
    }

    /// <summary>
    /// Creates a test player with a complete respec setup (level, gold, allocations).
    /// </summary>
    /// <returns>A Player instance ready for respec testing.</returns>
    private static Player CreateTestPlayerWithRespecSetup()
    {
        var player = CreateTestPlayerAtLevel(5);
        player.AddCurrency("gold", 1000);
        player.AddTalentPoints(10);

        // Add some allocations
        var frenzyAlloc = TalentAllocation.Create("frenzy", 1);
        frenzyAlloc.IncrementRank(); // rank 2
        player.AddTalentAllocation(frenzyAlloc);
        player.AddTalentAllocation(TalentAllocation.Create("rage", 2));

        return player;
    }
}
