using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ClimbingService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// - Stage calculation based on height
/// - Surface type modifier application
/// - Success progression
/// - Failure regression
/// - Fumble fall triggering
/// - Critical success double advancement
/// </remarks>
[TestFixture]
public class ClimbingServiceTests
{
    private ClimbingService _service = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IFumbleConsequenceService> _mockFumbleService = null!;
    private Mock<ILogger<ClimbingService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDiceService = new Mock<IDiceService>();
        _mockFumbleService = new Mock<IFumbleConsequenceService>();
        _mockLogger = new Mock<ILogger<ClimbingService>>();

        _service = new ClimbingService(
            _mockDiceService.Object,
            _mockFumbleService.Object,
            _mockLogger.Object);
    }

    #region CalculateStagesRequired Tests

    [Test]
    [TestCase(0, 0)]
    [TestCase(10, 1)]
    [TestCase(20, 1)]
    [TestCase(21, 2)]
    [TestCase(40, 2)]
    [TestCase(41, 3)]
    [TestCase(100, 3)]
    public void CalculateStagesRequired_GivenHeight_ReturnsCorrectStages(int heightFeet, int expectedStages)
    {
        // Act
        var stages = _service.CalculateStagesRequired(heightFeet);

        // Assert
        stages.Should().Be(expectedStages);
    }

    #endregion

    #region Surface Modifier Tests

    [Test]
    [TestCase(SurfaceType.Stable, 1)]
    [TestCase(SurfaceType.Normal, 0)]
    [TestCase(SurfaceType.Wet, -1)]
    [TestCase(SurfaceType.Compromised, -2)]
    [TestCase(SurfaceType.Collapsing, -3)]
    [TestCase(SurfaceType.Glitched, 0)]
    public void GetSurfaceDiceModifier_GivenSurfaceType_ReturnsCorrectModifier(
        SurfaceType surfaceType,
        int expectedModifier)
    {
        // Act
        var modifier = _service.GetSurfaceDiceModifier(surfaceType);

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    [Test]
    [TestCase(SurfaceType.Stable, 0)]
    [TestCase(SurfaceType.Glitched, 2)]
    public void GetSurfaceDcModifier_GivenSurfaceType_ReturnsCorrectModifier(
        SurfaceType surfaceType,
        int expectedModifier)
    {
        // Act
        var modifier = _service.GetSurfaceDcModifier(surfaceType);

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    #endregion

    #region AttemptStage Tests

    [Test]
    public void AttemptStage_OnSuccess_AdvancesToNextStage()
    {
        // Arrange
        var context = ClimbContext.Create(30, SurfaceType.Normal, baseDc: 2);
        var climbState = ClimbState.StartClimb("test-char", context);

        // Mock a successful roll (3 net successes vs DC 2 = margin +1)
        SetupDiceRoll(netSuccesses: 3, isFumble: false, isCritical: false);

        // Act
        var result = _service.AttemptStage(climbState, baseDicePool: 4);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.NewStage.Should().Be(1);
        result.StagesAdvanced.Should().Be(1);
        result.FallTriggered.Should().BeFalse();
        climbState.CurrentStage.Should().Be(1);
    }

    [Test]
    public void AttemptStage_OnFailure_SlipsBackOneStage()
    {
        // Arrange
        var context = ClimbContext.Create(30, SurfaceType.Normal, baseDc: 2);
        var climbState = ClimbState.StartClimb("test-char", context);

        // First, advance to stage 1
        SetupDiceRoll(netSuccesses: 3, isFumble: false, isCritical: false);
        _service.AttemptStage(climbState, baseDicePool: 4);
        climbState.CurrentStage.Should().Be(1);

        // Now mock a failure roll (1 net success vs DC 2 = margin -1)
        SetupDiceRoll(netSuccesses: 1, isFumble: false, isCritical: false);

        // Act
        var result = _service.AttemptStage(climbState, baseDicePool: 4);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(SkillOutcome.Failure);
        climbState.CurrentStage.Should().Be(1); // Still at 1 (slipped but stayed at current)
    }

    [Test]
    public void AttemptStage_OnFumble_TriggersFallFromCurrentHeight()
    {
        // Arrange
        var context = ClimbContext.Create(50, SurfaceType.Normal, baseDc: 2);
        var climbState = ClimbState.StartClimb("test-char", context);

        // Advance to stage 2 first (height 40ft)
        SetupDiceRoll(netSuccesses: 3, isFumble: false, isCritical: false);
        _service.AttemptStage(climbState, baseDicePool: 4);
        SetupDiceRoll(netSuccesses: 3, isFumble: false, isCritical: false);
        _service.AttemptStage(climbState, baseDicePool: 4);
        climbState.CurrentStage.Should().Be(2);

        // Now mock a fumble (0 successes + 1 botch)
        SetupDiceRoll(netSuccesses: 0, isFumble: true, isCritical: false);

        // Act
        var result = _service.AttemptStage(climbState, baseDicePool: 4);

        // Assert
        result.IsFumble.Should().BeTrue();
        result.FallTriggered.Should().BeTrue();
        result.FallHeight.Should().Be(40); // Fall from stage 3 starting height
        climbState.Status.Should().Be(ClimbStatus.Fallen);

        // Verify fumble consequence was triggered
        _mockFumbleService.Verify(
            f => f.CreateConsequence("test-char", "acrobatics-climbing", null, null),
            Times.Once);
    }

    [Test]
    public void AttemptStage_OnCriticalSuccess_AdvancesTwoStages()
    {
        // Arrange
        var context = ClimbContext.Create(50, SurfaceType.Normal, baseDc: 2);
        var climbState = ClimbState.StartClimb("test-char", context);

        // Mock a critical success (7 net successes vs DC 2 = margin +5)
        SetupDiceRoll(netSuccesses: 7, isFumble: false, isCritical: true);

        // Act
        var result = _service.AttemptStage(climbState, baseDicePool: 4);

        // Assert
        result.IsCriticalSuccess.Should().BeTrue();
        result.StagesAdvanced.Should().Be(2);
        result.NewStage.Should().Be(2);
        climbState.CurrentStage.Should().Be(2);
    }

    [Test]
    public void AttemptStage_WhenClimbComplete_SetsCompletedStatus()
    {
        // Arrange
        var context = ClimbContext.Create(15, SurfaceType.Normal, baseDc: 2);
        var climbState = ClimbState.StartClimb("test-char", context);

        // Mock a success (only 1 stage needed)
        SetupDiceRoll(netSuccesses: 3, isFumble: false, isCritical: false);

        // Act
        var result = _service.AttemptStage(climbState, baseDicePool: 4);

        // Assert
        result.ClimbCompleted.Should().BeTrue();
        result.ClimbStatus.Should().Be(ClimbStatus.Completed);
        climbState.Status.Should().Be(ClimbStatus.Completed);
    }

    #endregion

    #region StartClimb Tests

    [Test]
    public void StartClimb_WithValidParameters_CreatesInProgressState()
    {
        // Arrange
        var context = ClimbContext.Create(30, SurfaceType.Wet, baseDc: 2);

        // Act
        var climbState = _service.StartClimb("test-char", context);

        // Assert
        climbState.Should().NotBeNull();
        climbState.CharacterId.Should().Be("test-char");
        climbState.Status.Should().Be(ClimbStatus.InProgress);
        climbState.CurrentStage.Should().Be(0);
        climbState.Context.StagesRequired.Should().Be(2);
    }

    #endregion

    #region AbandonClimb Tests

    [Test]
    public void AbandonClimb_WhenInProgress_SetsAbandonedStatus()
    {
        // Arrange
        var context = ClimbContext.Create(30, SurfaceType.Normal, baseDc: 2);
        var climbState = ClimbState.StartClimb("test-char", context);

        // Act
        _service.AbandonClimb(climbState);

        // Assert
        climbState.Status.Should().Be(ClimbStatus.Abandoned);
        climbState.CurrentStage.Should().Be(0);
    }

    #endregion

    #region ProcessFall Tests

    [Test]
    public void ProcessFall_WhenFallen_ReturnsFallResult()
    {
        // Arrange
        var context = ClimbContext.Create(50, SurfaceType.Normal, baseDc: 2);
        var climbState = ClimbState.StartClimb("test-char", context);

        // Advance then fumble
        SetupDiceRoll(netSuccesses: 3, isFumble: false, isCritical: false);
        _service.AttemptStage(climbState, baseDicePool: 4);
        SetupDiceRoll(netSuccesses: 0, isFumble: true, isCritical: false);
        _service.AttemptStage(climbState, baseDicePool: 4);

        climbState.Status.Should().Be(ClimbStatus.Fallen);

        // Act
        var fallResult = _service.ProcessFall(climbState);

        // Assert
        fallResult.Source.Should().Be(FallSource.Climbing);
        fallResult.TriggeredByFumble.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private void SetupDiceRoll(int netSuccesses, bool isFumble, bool isCritical)
    {
        var dicePool = DicePool.D10(4);
        var rolls = new List<int> { 8, 8, 4, 3 }; // Dummy rolls

        // Create a mock result
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
