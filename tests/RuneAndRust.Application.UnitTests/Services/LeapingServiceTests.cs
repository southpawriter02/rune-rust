using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="LeapingService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// - Distance-to-DC mapping
/// - DC modifier calculations (running start, precision, glitched)
/// - Success/failure outcomes
/// - Fumble triggering [The Long Fall]
/// - Stamina cost calculations
/// </remarks>
[TestFixture]
public class LeapingServiceTests
{
    private LeapingService _service = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IFumbleConsequenceService> _mockFumbleService = null!;
    private Mock<ILogger<LeapingService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDiceService = new Mock<IDiceService>();
        _mockFumbleService = new Mock<IFumbleConsequenceService>();
        _mockLogger = new Mock<ILogger<LeapingService>>();

        _service = new LeapingService(
            _mockDiceService.Object,
            _mockFumbleService.Object,
            _mockLogger.Object);
    }

    #region Distance-to-DC Mapping Tests

    [Test]
    [TestCase(5, LeapDistance.Short, 1)]
    [TestCase(10, LeapDistance.Medium, 2)]
    [TestCase(15, LeapDistance.Long, 3)]
    [TestCase(20, LeapDistance.Extreme, 4)]
    [TestCase(25, LeapDistance.Heroic, 5)]
    public void GetDistanceCategory_GivenFeet_ReturnsCorrectCategoryAndDc(
        int distanceFeet,
        LeapDistance expectedCategory,
        int expectedDc)
    {
        // Act
        var category = _service.GetDistanceCategory(distanceFeet);
        var dc = _service.GetBaseDc(category);

        // Assert
        category.Should().Be(expectedCategory);
        dc.Should().Be(expectedDc);
    }

    [Test]
    [TestCase(1, LeapDistance.Short)]
    [TestCase(6, LeapDistance.Medium)]
    [TestCase(11, LeapDistance.Long)]
    [TestCase(16, LeapDistance.Extreme)]
    [TestCase(21, LeapDistance.Heroic)]
    public void GetDistanceCategory_AtMinimumFeet_ReturnsCorrectCategory(
        int distanceFeet,
        LeapDistance expectedCategory)
    {
        // Act
        var category = _service.GetDistanceCategory(distanceFeet);

        // Assert
        category.Should().Be(expectedCategory);
    }

    #endregion

    #region DC Modifier Tests

    [Test]
    public void CalculateDc_WithRunningStart_ReducesDcByOne()
    {
        // Arrange - 15ft leap normally DC 3
        var dcWithoutRunning = _service.CalculateDc(15, hasRunningStart: false);
        var dcWithRunning = _service.CalculateDc(15, hasRunningStart: true);

        // Assert
        dcWithoutRunning.Should().Be(3);
        dcWithRunning.Should().Be(2);
    }

    [Test]
    public void CalculateDc_WithPrecisionLanding_IncreasesDcByOne()
    {
        // Arrange - 10ft leap normally DC 2
        var dcNormal = _service.CalculateDc(10, landingType: LandingType.Normal);
        var dcPrecision = _service.CalculateDc(10, landingType: LandingType.Precision);

        // Assert
        dcNormal.Should().Be(2);
        dcPrecision.Should().Be(3);
    }

    [Test]
    public void CalculateDc_WithGlitchedLanding_IncreasesDcByTwo()
    {
        // Arrange - 10ft leap normally DC 2
        var dcNormal = _service.CalculateDc(10, landingType: LandingType.Normal);
        var dcGlitched = _service.CalculateDc(10, landingType: LandingType.Glitched);

        // Assert
        dcNormal.Should().Be(2);
        dcGlitched.Should().Be(4);
    }

    [Test]
    public void CalculateDc_WithDownwardLeap_ReducesDcByOne()
    {
        // Arrange - 10ft leap normally DC 2
        var dcNormal = _service.CalculateDc(10, landingType: LandingType.Normal);
        var dcDownward = _service.CalculateDc(10, landingType: LandingType.Downward);

        // Assert
        dcNormal.Should().Be(2);
        dcDownward.Should().Be(1);
    }

    [Test]
    public void CalculateDc_MinimumDcIsOne()
    {
        // Arrange - 5ft (DC 1) with running start (-1) and downward (-1) should still be 1
        var dc = _service.CalculateDc(5, hasRunningStart: true, landingType: LandingType.Downward);

        // Assert
        dc.Should().Be(1);
    }

    [Test]
    public void CalculateDc_WithEncumbered_IncreasesDcByOne()
    {
        // Arrange - 10ft leap normally DC 2
        var dcNormal = _service.CalculateDc(10, isEncumbered: false);
        var dcEncumbered = _service.CalculateDc(10, isEncumbered: true);

        // Assert
        dcNormal.Should().Be(2);
        dcEncumbered.Should().Be(3);
    }

    [Test]
    public void CalculateDc_WithLowGravity_ReducesDcByOne()
    {
        // Arrange - 15ft leap normally DC 3
        var dcNormal = _service.CalculateDc(15, hasLowGravity: false);
        var dcLowGravity = _service.CalculateDc(15, hasLowGravity: true);

        // Assert
        dcNormal.Should().Be(3);
        dcLowGravity.Should().Be(2);
    }

    #endregion

    #region AttemptLeap Tests

    [Test]
    public void AttemptLeap_OnSuccess_ReturnsLandedResult()
    {
        // Arrange
        var context = LeapContext.Simple(10, fallDepth: 20);
        SetupDiceRoll(netSuccesses: 3, isFumble: false, isCritical: false);

        // Act
        var result = _service.AttemptLeap("player-1", context, baseDicePool: 4);

        // Assert
        result.Landed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.FallTriggered.Should().BeFalse();
    }

    [Test]
    public void AttemptLeap_OnFailure_TriggersFall()
    {
        // Arrange
        var context = LeapContext.Simple(15, fallDepth: 30);
        SetupDiceRoll(netSuccesses: 1, isFumble: false, isCritical: false);

        // Act
        var result = _service.AttemptLeap("player-1", context, baseDicePool: 4);

        // Assert
        result.Landed.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.FallTriggered.Should().BeTrue();
        result.FallResult.Should().NotBeNull();
        result.FallResult!.Value.FallHeight.Should().Be(30);
    }

    [Test]
    public void AttemptLeap_OnFumble_TriggersTheLongFall()
    {
        // Arrange
        var context = LeapContext.Simple(15, fallDepth: 30);
        SetupDiceRoll(netSuccesses: 0, isFumble: true, isCritical: false);

        // Act
        var result = _service.AttemptLeap("player-1", context, baseDicePool: 4);

        // Assert
        result.Landed.Should().BeFalse();
        result.IsFumble.Should().BeTrue();
        result.FallTriggered.Should().BeTrue();
        result.BonusDamage.Should().Be(1);
        result.StressGained.Should().Be(2);
        result.StatusApplied.Should().Be("Disoriented");
        result.StatusDuration.Should().Be(2);

        // Verify fumble consequence was triggered
        _mockFumbleService.Verify(
            f => f.CreateConsequence("player-1", "acrobatics-leaping", null, null),
            Times.Once);
    }

    [Test]
    public void AttemptLeap_OnCriticalSuccess_HalvesStaminaCost()
    {
        // Arrange - Long leap (2 stamina base), critical should halve to 1
        var context = LeapContext.Simple(15, fallDepth: 20);
        SetupDiceRoll(netSuccesses: 8, isFumble: false, isCritical: true);

        // Act
        var result = _service.AttemptLeap("player-1", context, baseDicePool: 6);

        // Assert
        result.IsCriticalSuccess.Should().BeTrue();
        result.StaminaCost.Should().Be(1); // 2 / 2 = 1
    }

    [Test]
    public void AttemptLeap_OnMarginalSuccess_AddsOneStamina()
    {
        // Arrange - Medium leap (1 stamina base), marginal should add 1 = 2
        var context = LeapContext.Simple(10, fallDepth: 20);
        // Net successes = DC (2) exactly = margin 0 = marginal success
        SetupDiceRoll(netSuccesses: 2, isFumble: false, isCritical: false);

        // Act
        var result = _service.AttemptLeap("player-1", context, baseDicePool: 4);

        // Assert
        result.IsMarginalSuccess.Should().BeTrue();
        result.StaminaCost.Should().Be(2); // 1 + 1 = 2
    }

    #endregion

    #region Stamina Cost Tests

    [Test]
    [TestCase(LeapDistance.Short, SkillOutcome.FullSuccess, 1)]
    [TestCase(LeapDistance.Medium, SkillOutcome.FullSuccess, 1)]
    [TestCase(LeapDistance.Long, SkillOutcome.FullSuccess, 2)]
    [TestCase(LeapDistance.Extreme, SkillOutcome.FullSuccess, 2)]
    [TestCase(LeapDistance.Heroic, SkillOutcome.FullSuccess, 3)]
    public void GetStaminaCost_ForNormalSuccess_ReturnsBaseCost(
        LeapDistance distance,
        SkillOutcome outcome,
        int expectedCost)
    {
        // Act
        var cost = _service.GetStaminaCost(distance, outcome);

        // Assert
        cost.Should().Be(expectedCost);
    }

    [Test]
    public void GetStaminaCost_ForCriticalSuccess_HalvesCost()
    {
        // Arrange - Long leap base cost 2
        var cost = _service.GetStaminaCost(LeapDistance.Long, SkillOutcome.CriticalSuccess);

        // Assert
        cost.Should().Be(1); // 2 / 2 = 1
    }

    [Test]
    public void GetStaminaCost_ForFailure_ReturnsZero()
    {
        // Act
        var cost = _service.GetStaminaCost(LeapDistance.Long, SkillOutcome.Failure);

        // Assert
        cost.Should().Be(0); // Fall damage instead
    }

    #endregion

    #region RequiresMasterRank Tests

    [Test]
    [TestCase(20, false)]
    [TestCase(21, true)]
    [TestCase(25, true)]
    public void RequiresMasterRank_ReturnsCorrectValue(int distanceFeet, bool expected)
    {
        // Act
        var result = _service.RequiresMasterRank(distanceFeet);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Helper Methods

    private void SetupDiceRoll(int netSuccesses, bool isFumble, bool isCritical)
    {
        var dicePool = DicePool.D10(4);
        var rolls = new List<int> { 8, 8, 4, 3 }; // Dummy rolls

        var mockResult = new DiceRollResult(
            pool: dicePool,
            rolls: rolls)
        {
            NetSuccesses = netSuccesses,
            IsFumble = isFumble,
            IsCriticalSuccess = isCritical
        };

        _mockDiceService
            .Setup(d => d.Roll(
                It.IsAny<DicePool>(),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .Returns(mockResult);
    }

    #endregion
}
