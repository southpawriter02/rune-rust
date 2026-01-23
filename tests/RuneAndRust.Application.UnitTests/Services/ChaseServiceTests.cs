using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ChaseService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Chase initialization at specified distance</description></item>
///   <item><description>Distance changes on success/failure</description></item>
///   <item><description>Caught status when distance reaches 0</description></item>
///   <item><description>Escaped status when distance reaches 6</description></item>
///   <item><description>Obstacle generation and variety</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ChaseServiceTests
{
    private IDiceService _diceService = null!;
    private ILogger<ChaseService> _logger = null!;
    private ChaseService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _diceService = Substitute.For<IDiceService>();
        _logger = Substitute.For<ILogger<ChaseService>>();
        _sut = new ChaseService(_diceService, _logger);
    }

    #region Chase Initialization Tests

    [Test]
    public void StartChase_WithDefaultDistance_StartsAtDistance3()
    {
        // Act
        var chase = _sut.StartChase("fleeing-1", "pursuer-1");

        // Assert
        chase.Should().NotBeNull();
        chase.Distance.Should().Be(ChaseState.DefaultStartDistance);
        chase.FleeingId.Should().Be("fleeing-1");
        chase.PursuerId.Should().Be("pursuer-1");
        chase.Status.Should().Be(ChaseStatus.InProgress);
        chase.RoundNumber.Should().Be(0);
    }

    [Test]
    public void StartChase_WithCustomDistance_StartsAtSpecifiedDistance()
    {
        // Act
        var chase = _sut.StartChase("fleeing-1", "pursuer-1", startDistance: 5);

        // Assert
        chase.Distance.Should().Be(5);
        chase.GetCurrentZone().Should().Be("Far");
    }

    [Test]
    public void StartChase_WithMaxRounds_SetsMaxRoundsLimit()
    {
        // Act
        var chase = _sut.StartChase("fleeing-1", "pursuer-1", maxRounds: 10);

        // Assert
        chase.MaxRounds.Should().Be(10);
    }

    [Test]
    public void StartChase_GeneratesUniqueChaseIds()
    {
        // Act
        var chase1 = _sut.StartChase("fleeing-1", "pursuer-1");
        var chase2 = _sut.StartChase("fleeing-2", "pursuer-2");

        // Assert
        chase1.ChaseId.Should().NotBe(chase2.ChaseId);
    }

    [Test]
    public void StartChase_SameFleeingAndPursuer_ThrowsException()
    {
        // Act & Assert
        var act = () => _sut.StartChase("player-1", "player-1");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*same character*");
    }

    #endregion

    #region Distance Movement Tests

    [Test]
    public void ProcessRound_FleeingSuccess_IncreasesDistance()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1", startDistance: 3);
        _sut.GenerateObstacle(chase.ChaseId);
        
        // Fleeing rolls high (success), pursuer also rolls (using same mock)
        // With 5 successes and DC 2-4, both succeed, so check individual changes
        SetupDiceRoll(successes: 5, botches: 0);

        // Act
        var result = _sut.ProcessRound(chase.ChaseId, fleeingDicePool: 6, pursuerDicePool: 4);

        // Assert - fleeing with 5 successes vs DC 2-4 should succeed and get +1 distance
        result.FleeingDistanceChange.Should().BeGreaterThan(0);
        result.FleeingSucceeded.Should().BeTrue();
    }

    [Test]
    public void ProcessRound_BothSucceed_DistanceDependsOnMargins()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1", startDistance: 3);
        _sut.GenerateObstacle(chase.ChaseId);
        
        // Both succeed with similar margins
        SetupDiceRoll(successes: 4, botches: 0);

        // Act
        var result = _sut.ProcessRound(chase.ChaseId, fleeingDicePool: 5, pursuerDicePool: 5);

        // Assert - both succeed, fleeing +1 pursuer -1 (which becomes +1 for pursuer closing)
        // Net result depends on implementation
        result.ChaseStatus.Should().Be(ChaseStatus.InProgress);
    }

    [Test]
    public void CalculateDistanceChange_FleeingSuccess_ReturnsPositive()
    {
        // Act
        var change = _sut.CalculateDistanceChange(SkillOutcome.FullSuccess, isFleeingCharacter: true);

        // Assert
        change.Should().BeGreaterThan(0);
    }

    [Test]
    public void CalculateDistanceChange_PursuerSuccess_ReturnsNegative()
    {
        // Act
        var change = _sut.CalculateDistanceChange(SkillOutcome.FullSuccess, isFleeingCharacter: false);

        // Assert
        change.Should().BeLessThan(0);
    }

    [Test]
    public void CalculateDistanceChange_CriticalSuccess_DoubleMovement()
    {
        // Act
        var normalChange = _sut.CalculateDistanceChange(SkillOutcome.FullSuccess, isFleeingCharacter: true);
        var criticalChange = _sut.CalculateDistanceChange(SkillOutcome.CriticalSuccess, isFleeingCharacter: true);

        // Assert
        criticalChange.Should().Be(normalChange * 2);
    }

    #endregion

    #region Chase End Condition Tests

    [Test]
    public void ProcessRound_DistanceReachesZero_StatusBecomesCaught()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1", startDistance: 1);
        _sut.GenerateObstacle(chase.ChaseId);
        
        // Fleeing fails badly, pursuer succeeds well - should drop distance to 0
        SetupDiceRollSequence(
            (successes: 0, botches: 1), // Fleeing fumble
            (successes: 6, botches: 0)  // Pursuer success
        );

        // Act
        var result = _sut.ProcessRound(chase.ChaseId, fleeingDicePool: 3, pursuerDicePool: 6);

        // Assert
        result.ChaseStatus.Should().Be(ChaseStatus.Caught);
        result.FleeingCaught.Should().BeTrue();
        chase.Status.Should().Be(ChaseStatus.Caught);
    }

    [Test]
    public void ProcessRound_DistanceReachesSix_StatusBecomesEscaped()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1", startDistance: 5);
        _sut.GenerateObstacle(chase.ChaseId);
        
        // Fleeing succeeds well, pursuer fails - should push distance to 6+
        SetupDiceRollSequence(
            (successes: 6, botches: 0), // Fleeing success
            (successes: 1, botches: 0)  // Pursuer failure
        );

        // Act
        var result = _sut.ProcessRound(chase.ChaseId, fleeingDicePool: 6, pursuerDicePool: 3);

        // Assert
        result.ChaseStatus.Should().Be(ChaseStatus.Escaped);
        result.FleeingEscaped.Should().BeTrue();
        chase.Status.Should().Be(ChaseStatus.Escaped);
    }

    [Test]
    public void AbandonChase_ByFleeing_SetsAbandonedStatus()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1");

        // Act
        _sut.AbandonChase(chase.ChaseId, "fleeing-1");

        // Assert
        chase.Status.Should().Be(ChaseStatus.Abandoned);
        chase.AbandonedById.Should().Be("fleeing-1");
    }

    [Test]
    public void AbandonChase_ByPursuer_SetsAbandonedStatus()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1");

        // Act
        _sut.AbandonChase(chase.ChaseId, "pursuer-1");

        // Assert
        chase.Status.Should().Be(ChaseStatus.Abandoned);
        chase.AbandonedById.Should().Be("pursuer-1");
    }

    [Test]
    public void ProcessRound_MaxRoundsReached_StatusBecomesTimedOut()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1", startDistance: 3, maxRounds: 1);
        _sut.GenerateObstacle(chase.ChaseId);
        
        // Both succeed, distance stays similar
        SetupDiceRoll(successes: 3, botches: 0);

        // Act
        var result = _sut.ProcessRound(chase.ChaseId, fleeingDicePool: 5, pursuerDicePool: 5);

        // Assert
        result.ChaseStatus.Should().Be(ChaseStatus.TimedOut);
        chase.Status.Should().Be(ChaseStatus.TimedOut);
    }

    #endregion

    #region Obstacle Generation Tests

    [Test]
    public void GenerateObstacle_ReturnsValidObstacle()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1");

        // Act
        var obstacle = _sut.GenerateObstacle(chase.ChaseId);

        // Assert
        obstacle.Dc.Should().BeGreaterThanOrEqualTo(2);
        obstacle.Dc.Should().BeLessThanOrEqualTo(4);
        obstacle.SkillRequired.Should().Be("Acrobatics");
        obstacle.SuccessDescription.Should().NotBeEmpty();
        obstacle.FailureDescription.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateObstacle_AddsToChaseHistory()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1");

        // Act
        var obstacle = _sut.GenerateObstacle(chase.ChaseId);

        // Assert
        chase.ObstacleHistory.Should().ContainSingle();
        chase.ObstacleHistory[0].Should().Be(obstacle);
    }

    [Test]
    public void GenerateObstacle_AvoidsRepetition()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1");

        // Act - Generate multiple obstacles
        var obstacles = new List<ChaseObstacle>();
        for (var i = 0; i < 10; i++)
        {
            var obstacle = _sut.GenerateObstacle(chase.ChaseId);
            obstacles.Add(obstacle);
            
            // Clear current obstacle for next round
            SetupDiceRoll(successes: 3, botches: 0);
            _sut.ProcessRound(chase.ChaseId, fleeingDicePool: 5, pursuerDicePool: 5);
            
            if (chase.Status != ChaseStatus.InProgress)
                break;
        }

        // Assert - Should have variety (not all same type)
        var distinctTypes = obstacles.Select(o => o.ObstacleType).Distinct().Count();
        distinctTypes.Should().BeGreaterThan(1);
    }

    #endregion

    #region Chase Query Tests

    [Test]
    public void GetChase_ExistingChase_ReturnsChase()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1");

        // Act
        var retrieved = _sut.GetChase(chase.ChaseId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Should().Be(chase);
    }

    [Test]
    public void GetChase_NonExistentChase_ReturnsNull()
    {
        // Act
        var retrieved = _sut.GetChase("nonexistent-chase");

        // Assert
        retrieved.Should().BeNull();
    }

    [Test]
    public void GetActiveChases_ReturnsOnlyInProgressChases()
    {
        // Arrange
        var chase1 = _sut.StartChase("fleeing-1", "pursuer-1");
        var chase2 = _sut.StartChase("fleeing-2", "pursuer-2");
        _sut.AbandonChase(chase1.ChaseId, "fleeing-1");

        // Act
        var activeChases = _sut.GetActiveChases();

        // Assert
        activeChases.Should().HaveCount(1);
        activeChases[0].ChaseId.Should().Be(chase2.ChaseId);
    }

    [Test]
    public void GetCurrentZone_ReturnsCorrectZone()
    {
        // Arrange
        var chase = _sut.StartChase("fleeing-1", "pursuer-1", startDistance: 3);

        // Act
        var zone = _sut.GetCurrentZone(chase.ChaseId);

        // Assert
        zone.Should().Be("Near");
    }

    #endregion

    #region ChaseState Entity Tests

    [Test]
    public void ChaseState_Create_ValidatesStartDistance()
    {
        // Act & Assert
        var act = () => ChaseState.Create("chase-1", "fleeing-1", "pursuer-1", startDistance: 0);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("startDistance");
    }

    [Test]
    public void ChaseState_Create_ValidatesMaxRounds()
    {
        // Act & Assert
        var act = () => ChaseState.Create("chase-1", "fleeing-1", "pursuer-1", maxRounds: 0);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxRounds");
    }

    [Test]
    public void ChaseState_GetCurrentZone_ReturnsCorrectZones()
    {
        // Arrange & Act & Assert
        var chaseClose = ChaseState.Create("chase-1", "fleeing", "pursuer", startDistance: 1);
        chaseClose.GetCurrentZone().Should().Be("Close");

        var chaseNear = ChaseState.Create("chase-2", "fleeing", "pursuer", startDistance: 3);
        chaseNear.GetCurrentZone().Should().Be("Near");

        var chaseFar = ChaseState.Create("chase-3", "fleeing", "pursuer", startDistance: 5);
        chaseFar.GetCurrentZone().Should().Be("Far");
    }

    #endregion

    #region ChaseObstacle Value Object Tests

    [Test]
    public void ChaseObstacle_CreateGap_ReturnsValidObstacle()
    {
        // Act
        var obstacle = ChaseObstacle.CreateGap(dc: 3, gapWidth: "15 feet");

        // Assert
        obstacle.ObstacleType.Should().Be(ObstacleType.Gap);
        obstacle.Dc.Should().Be(3);
        obstacle.SkillRequired.Should().Be("Acrobatics");
        obstacle.TypeName.Should().Be("Gap");
        obstacle.SuccessDescription.Should().Contain("15 feet");
    }

    [Test]
    public void ChaseObstacle_CreateClimb_ReturnsValidObstacle()
    {
        // Act
        var obstacle = ChaseObstacle.CreateClimb(dc: 2, surface: "chain-link fence");

        // Assert
        obstacle.ObstacleType.Should().Be(ObstacleType.Climb);
        obstacle.Dc.Should().Be(2);
        obstacle.SuccessDescription.Should().Contain("chain-link fence");
    }

    [Test]
    public void ChaseObstacle_RequiresSpecializedService_TrueForGapAndClimb()
    {
        // Arrange
        var gap = ChaseObstacle.CreateGap(dc: 2);
        var climb = ChaseObstacle.CreateClimb(dc: 2);
        var debris = ChaseObstacle.CreateDebris(dc: 2);

        // Assert
        gap.RequiresSpecializedService.Should().BeTrue();
        climb.RequiresSpecializedService.Should().BeTrue();
        debris.RequiresSpecializedService.Should().BeFalse();
    }

    [Test]
    public void ChaseObstacle_ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var obstacle = ChaseObstacle.CreateGap(dc: 3);

        // Act
        var display = obstacle.ToDisplayString();

        // Assert
        display.Should().Be("Gap (DC 3) - Acrobatics");
    }

    #endregion

    #region ChaseRoundResult Value Object Tests

    [Test]
    public void ChaseRoundResult_NetDistanceChange_CalculatesCorrectly()
    {
        // Arrange
        var obstacle = ChaseObstacle.CreateGap(dc: 3);
        var result = new ChaseRoundResult(
            RoundNumber: 1,
            Obstacle: obstacle,
            FleeingNetSuccesses: 4,
            FleeingOutcome: SkillOutcome.FullSuccess,
            FleeingDistanceChange: 1,
            PursuerNetSuccesses: 2,
            PursuerOutcome: SkillOutcome.Failure,
            PursuerDistanceChange: 1, // Pursuer failure = +1 for fleeing
            PreviousDistance: 3,
            NewDistance: 5,
            ChaseStatus: ChaseStatus.InProgress);

        // Assert
        result.NetDistanceChange.Should().Be(2);
        result.FleeingWonRound.Should().BeTrue();
        result.PursuerWonRound.Should().BeFalse();
    }

    [Test]
    public void ChaseRoundResult_GetNarrative_GeneratesText()
    {
        // Arrange
        var obstacle = ChaseObstacle.CreateGap(dc: 3);
        var result = new ChaseRoundResult(
            RoundNumber: 1,
            Obstacle: obstacle,
            FleeingNetSuccesses: 4,
            FleeingOutcome: SkillOutcome.FullSuccess,
            FleeingDistanceChange: 1,
            PursuerNetSuccesses: 2,
            PursuerOutcome: SkillOutcome.Failure,
            PursuerDistanceChange: 1,
            PreviousDistance: 3,
            NewDistance: 5,
            ChaseStatus: ChaseStatus.InProgress);

        // Act
        var narrative = result.GetNarrative("Kira", "Guard");

        // Assert
        narrative.Should().NotBeEmpty();
        narrative.Should().Contain("Kira pulls ahead");
    }

    #endregion

    #region Helper Methods

    private void SetupDiceRoll(int successes, int botches)
    {
        var values = new List<int>();
        for (var i = 0; i < successes; i++)
            values.Add(8); // success value
        for (var i = 0; i < botches; i++)
            values.Add(1); // botch value
        while (values.Count < 5)
            values.Add(5);

        var result = new DiceRollResult(DicePool.D10(values.Count), values.ToArray());

        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>())
            .Returns(result);
    }

    private void SetupDiceRollSequence(params (int successes, int botches)[] rolls)
    {
        var callIndex = 0;
        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>())
            .Returns(callInfo =>
            {
                var (successCount, botchCount) = rolls[callIndex % rolls.Length];
                callIndex++;

                var values = new List<int>();
                for (var i = 0; i < successCount; i++)
                    values.Add(8);
                for (var i = 0; i < botchCount; i++)
                    values.Add(1);
                while (values.Count < 5)
                    values.Add(5);

                return new DiceRollResult(DicePool.D10(values.Count), values.ToArray());
            });
    }

    #endregion
}
