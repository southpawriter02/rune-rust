using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the TrackingService implementing Extended Tracking System mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the core tracking mechanics as defined in v0.15.5a:
/// <list type="bullet">
///   <item><description>Trail age setting correct base DCs</description></item>
///   <item><description>Tracking modifier accumulation</description></item>
///   <item><description>Phase transitions in the tracking state machine</description></item>
///   <item><description>Recovery procedures with correct DC modifiers</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class TrackingServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    private TrackingService _service = null!;
    private Mock<ILogger<TrackingService>> _mockLogger = null!;
    private Mock<ILogger<SkillCheckService>> _mockSkillCheckLogger = null!;
    private Mock<ILogger<DiceService>> _mockDiceLogger = null!;
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private Mock<ITrackingStateRepository> _mockRepository = null!;
    private SkillCheckService _skillCheckService = null!;
    private DiceService _diceService = null!;

    /// <summary>
    /// Seeded random for deterministic test results.
    /// </summary>
    private const int TestSeed = 42;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<TrackingService>>();
        _mockSkillCheckLogger = new Mock<ILogger<SkillCheckService>>();
        _mockDiceLogger = new Mock<ILogger<DiceService>>();
        _mockConfig = new Mock<IGameConfigurationProvider>();
        _mockRepository = new Mock<ITrackingStateRepository>();

        // Use seeded random for deterministic dice rolls
        var seededRandom = new Random(TestSeed);
        _diceService = new DiceService(_mockDiceLogger.Object, seededRandom);

        SetupDefaultMocks();

        _skillCheckService = new SkillCheckService(
            _diceService,
            _mockConfig.Object,
            _mockSkillCheckLogger.Object);

        _service = new TrackingService(
            _skillCheckService,
            _mockRepository.Object,
            _mockConfig.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// Sets up default mock configurations for skill definitions.
    /// </summary>
    private void SetupDefaultMocks()
    {
        // Set up Wasteland Survival skill
        var wastelandSurvival = SkillDefinition.Create(
            "wasteland-survival",
            "Wasteland Survival",
            "Survival and tracking in the wasteland.",
            "wits",
            "fortitude",
            "1d10");

        _mockConfig.Setup(c => c.GetSkillById("wasteland-survival"))
            .Returns(wastelandSurvival);
        _mockConfig.Setup(c => c.GetSkills())
            .Returns(new List<SkillDefinition> { wastelandSurvival });

        // Set up default difficulty classes
        var dcDefinitions = new List<DifficultyClassDefinition>
        {
            DifficultyClassDefinition.Create("easy", "Easy", "Simple tasks", 8),
            DifficultyClassDefinition.Create("moderate", "Moderate", "Standard difficulty", 12),
            DifficultyClassDefinition.Create("hard", "Hard", "Challenging tasks", 16),
            DifficultyClassDefinition.Create("very-hard", "Very Hard", "Difficult tasks", 20),
            DifficultyClassDefinition.Create("extreme", "Extreme", "Near impossible", 24),
            DifficultyClassDefinition.Create("legendary", "Legendary", "Master level", 28)
        };

        foreach (var dc in dcDefinitions)
        {
            _mockConfig.Setup(c => c.GetDifficultyClassById(dc.Id)).Returns(dc);
        }
        _mockConfig.Setup(c => c.GetDifficultyClasses()).Returns(dcDefinitions);

        // Default repository setup - no active tracking
        _mockRepository.Setup(r => r.HasActiveTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TrackingState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Creates a test player with specified attributes.
    /// </summary>
    /// <param name="wits">Wits attribute value (primary for tracking).</param>
    /// <param name="fortitude">Fortitude attribute value (secondary for tracking).</param>
    /// <param name="might">Might attribute value.</param>
    /// <param name="finesse">Finesse attribute value.</param>
    /// <returns>A new Player instance for testing.</returns>
    private static Player CreateTestPlayer(
        int wits = 10,
        int fortitude = 8,
        int might = 8,
        int finesse = 8)
    {
        var attributes = new PlayerAttributes(might, fortitude, 8, wits, finesse);
        return new Player("TestTracker", "human", "scout", attributes, "TestPlayer");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRAIL AGE DC TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that each trail age classification sets the correct base DC.
    /// </summary>
    /// <param name="trailAge">The trail age classification to test.</param>
    /// <param name="expectedDc">The expected base difficulty class.</param>
    /// <remarks>
    /// <para>
    /// Trail age DCs as defined in the Extended Tracking System:
    /// <list type="bullet">
    ///   <item><description>Obvious: DC 8 (caravan, large group)</description></item>
    ///   <item><description>Fresh: DC 12 (less than 1 hour)</description></item>
    ///   <item><description>Standard: DC 16 (2-8 hours)</description></item>
    ///   <item><description>Old: DC 20 (12-24 hours)</description></item>
    ///   <item><description>Obscured: DC 24 (deliberately hidden)</description></item>
    ///   <item><description>Cold: DC 28 (24-48 hours, Master rank required)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Test]
    [TestCase(TrailAge.Obvious, 8)]
    [TestCase(TrailAge.Fresh, 12)]
    [TestCase(TrailAge.Standard, 16)]
    [TestCase(TrailAge.Old, 20)]
    [TestCase(TrailAge.Obscured, 24)]
    [TestCase(TrailAge.Cold, 28)]
    public void TrailAge_SetsCorrectBaseDc(TrailAge trailAge, int expectedDc)
    {
        // Arrange
        var state = TrackingState.Create("test-player-id", "test target", trailAge);

        // Act
        var baseDc = state.BaseDc;

        // Assert
        baseDc.Should().Be(expectedDc,
            because: $"trail age {trailAge} should have base DC of {expectedDc}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRACKING MODIFIER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that tracking modifiers accumulate correctly to produce the correct total DC modifier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tests the following condition modifiers:
    /// <list type="bullet">
    ///   <item><description>Blood trail: -4 DC</description></item>
    ///   <item><description>Fresh injury: -2 DC</description></item>
    ///   <item><description>Multiple targets (2+): -2 DC</description></item>
    ///   <item><description>Recent rain: +4 DC</description></item>
    ///   <item><description>Target hiding: +2 DC</description></item>
    ///   <item><description>Hours elapsed: +1 DC per hour</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Test]
    public void TrackingModifiers_AccumulateCorrectly()
    {
        // Arrange & Act - Test blood trail alone
        var bloodTrailOnly = new TrackingModifiers(hasBloodTrail: true);
        bloodTrailOnly.TotalDcModifier.Should().Be(-4,
            because: "blood trail should reduce DC by 4");

        // Test fresh injury alone
        var injuryOnly = new TrackingModifiers(targetIsFreshlyInjured: true);
        injuryOnly.TotalDcModifier.Should().Be(-2,
            because: "fresh injury should reduce DC by 2");

        // Test multiple targets alone
        var multipleTargets = new TrackingModifiers(targetCount: 3);
        multipleTargets.TotalDcModifier.Should().Be(-2,
            because: "multiple targets should reduce DC by 2");

        // Test single target (no modifier)
        var singleTarget = new TrackingModifiers(targetCount: 1);
        singleTarget.TotalDcModifier.Should().Be(0,
            because: "single target should not affect DC");

        // Test rain alone
        var rainOnly = new TrackingModifiers(hasRecentRain: true);
        rainOnly.TotalDcModifier.Should().Be(4,
            because: "recent rain should increase DC by 4");

        // Test target hiding alone
        var hidingOnly = new TrackingModifiers(targetIsHiding: true);
        hidingOnly.TotalDcModifier.Should().Be(2,
            because: "target hiding should increase DC by 2");

        // Test hours elapsed (3 hours)
        var hoursElapsed = new TrackingModifiers(hoursElapsed: 3);
        hoursElapsed.TotalDcModifier.Should().Be(3,
            because: "each hour should increase DC by 1");

        // Test combined modifiers
        var combinedModifiers = new TrackingModifiers(
            hasBloodTrail: true,       // -4
            hasRecentRain: true,       // +4
            targetCount: 2,            // -2
            targetIsHiding: true,      // +2
            targetIsFreshlyInjured: true, // -2
            hoursElapsed: 2);          // +2
        // Expected: -4 + 4 - 2 + 2 - 2 + 2 = 0
        combinedModifiers.TotalDcModifier.Should().Be(0,
            because: "combined modifiers should sum correctly (-4+4-2+2-2+2 = 0)");

        // Test equipment and familiar territory dice modifiers
        var diceModifiers = new TrackingModifiers(
            equipmentBonus: 2,
            isFamiliarTerritory: true);
        diceModifiers.TotalDiceModifier.Should().Be(4,
            because: "equipment (2) + familiar territory (2) should equal 4");
    }

    /// <summary>
    /// Verifies that terrain type correctly determines check interval distance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Check intervals by terrain:
    /// <list type="bullet">
    ///   <item><description>OpenWasteland: 2.0 miles</description></item>
    ///   <item><description>ModerateRuins: 1.0 mile</description></item>
    ///   <item><description>DenseRuins: 0.5 miles</description></item>
    ///   <item><description>Labyrinthine: 0.1 miles</description></item>
    ///   <item><description>GlitchedLabyrinth: 0.1 miles</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Test]
    [TestCase(NavigationTerrainType.OpenWasteland, 2.0f)]
    [TestCase(NavigationTerrainType.ModerateRuins, 1.0f)]
    [TestCase(NavigationTerrainType.DenseRuins, 0.5f)]
    [TestCase(NavigationTerrainType.Labyrinthine, 0.1f)]
    [TestCase(NavigationTerrainType.GlitchedLabyrinth, 0.1f)]
    public void TrackingModifiers_GetCheckIntervalMiles_ReturnsCorrectInterval(
        NavigationTerrainType terrain,
        float expectedInterval)
    {
        // Arrange
        var modifiers = new TrackingModifiers(terrain: terrain);

        // Act
        var interval = modifiers.GetCheckIntervalMiles();

        // Assert
        interval.Should().Be(expectedInterval,
            because: $"terrain {terrain} should have check interval of {expectedInterval} miles");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PHASE TRANSITION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that tracking state phase transitions follow the correct state machine rules.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Valid phase transitions:
    /// <list type="bullet">
    ///   <item><description>Acquisition → Pursuit: On successful acquisition</description></item>
    ///   <item><description>Pursuit → ClosingIn: When within 500ft</description></item>
    ///   <item><description>Pursuit → Lost: On failed pursuit check</description></item>
    ///   <item><description>ClosingIn → Lost: On failed closing check</description></item>
    ///   <item><description>Lost → Pursuit: On successful recovery</description></item>
    ///   <item><description>Lost → Cold: After 3 failed recovery attempts</description></item>
    ///   <item><description>Acquisition → Cold: After 3 failed acquisition attempts</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Test]
    public void PhaseTransitions_WorkProperly()
    {
        // Arrange - Create initial tracking state
        var state = TrackingState.Create("test-player-id", "test target", TrailAge.Standard);

        // Assert initial state
        state.CurrentPhase.Should().Be(TrackingPhase.Acquisition,
            because: "new tracking should start in Acquisition phase");
        state.Status.Should().Be(TrackingStatus.Active,
            because: "new tracking should have Active status");

        // Act & Assert - Transition to Pursuit
        state.TransitionToPursuit();
        state.CurrentPhase.Should().Be(TrackingPhase.Pursuit,
            because: "successful acquisition should transition to Pursuit");
        state.FailedAttemptsInPhase.Should().Be(0,
            because: "failed attempts should reset on phase transition");

        // Act & Assert - Transition to ClosingIn
        state.TransitionToClosingIn(300);
        state.CurrentPhase.Should().Be(TrackingPhase.ClosingIn,
            because: "within 500ft should transition to ClosingIn");
        state.EstimatedDistanceToTarget.Should().Be(300,
            because: "estimated distance should be set on transition");

        // Act & Assert - Transition to Lost (from ClosingIn)
        state.TransitionToLost();
        state.CurrentPhase.Should().Be(TrackingPhase.Lost,
            because: "failed check should transition to Lost");

        // Act & Assert - Recover to Pursuit
        state.RecoverToPursuit();
        state.CurrentPhase.Should().Be(TrackingPhase.Pursuit,
            because: "successful recovery should return to Pursuit");

        // Act & Assert - Test Cold transition
        state.TransitionToLost();
        state.TransitionToCold();
        state.CurrentPhase.Should().Be(TrackingPhase.Cold,
            because: "3 failures should transition to Cold");
        state.Status.Should().Be(TrackingStatus.TrailCold,
            because: "Cold phase should set TrailCold status");
        state.IsComplete.Should().BeTrue(
            because: "Cold is a terminal state");
    }

    /// <summary>
    /// Verifies that invalid phase transitions throw appropriate exceptions.
    /// </summary>
    [Test]
    public void PhaseTransitions_ThrowOnInvalidTransition()
    {
        // Arrange
        var state = TrackingState.Create("test-player-id", "test target", TrailAge.Standard);

        // Act & Assert - Cannot transition to ClosingIn from Acquisition
        var actClosingIn = () => state.TransitionToClosingIn(300);
        actClosingIn.Should().Throw<InvalidOperationException>(
            because: "cannot transition to ClosingIn from Acquisition");

        // Act & Assert - Cannot recover when not Lost
        var actRecover = () => state.RecoverToPursuit();
        actRecover.Should().Throw<InvalidOperationException>(
            because: "cannot recover when not in Lost phase");

        // Transition to Cold
        state.TransitionToCold();

        // Act & Assert - Cannot transition from Cold
        var actFromCold = () => state.TransitionToLost();
        actFromCold.Should().Throw<InvalidOperationException>(
            because: "cannot transition from Cold phase - it's terminal");
    }

    /// <summary>
    /// Verifies that failed attempts counter increments and resets correctly.
    /// </summary>
    [Test]
    public void FailedAttempts_IncrementAndReset()
    {
        // Arrange
        var state = TrackingState.Create("test-player-id", "test target", TrailAge.Standard);
        var mockCheck = CreateMockCheck(isSuccess: false);

        // Act - Record first failure
        state.RecordFailure(mockCheck);
        state.FailedAttemptsInPhase.Should().Be(1);

        // Act - Record second failure
        state.RecordFailure(mockCheck);
        state.FailedAttemptsInPhase.Should().Be(2);

        // Act - Transition to Pursuit (resets counter)
        state.TransitionToPursuit();
        state.FailedAttemptsInPhase.Should().Be(0,
            because: "counter should reset on phase transition");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECOVERY PROCEDURE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that recovery procedures apply correct DC modifiers.
    /// </summary>
    /// <param name="recoveryType">The type of recovery procedure.</param>
    /// <param name="expectedDcModifier">The expected DC modifier for this procedure.</param>
    /// <remarks>
    /// <para>
    /// Recovery procedure modifiers:
    /// <list type="bullet">
    ///   <item><description>Backtrack: +0 DC, 10 minutes</description></item>
    ///   <item><description>SpiralSearch: +4 DC, 30 minutes</description></item>
    ///   <item><description>ReturnToLastKnown: +8 DC, 1 hour</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Test]
    [TestCase(RecoveryType.Backtrack, 0)]
    [TestCase(RecoveryType.SpiralSearch, 4)]
    [TestCase(RecoveryType.ReturnToLastKnown, 8)]
    public async Task RecoveryProcedures_ApplyCorrectDcModifier(
        RecoveryType recoveryType,
        int expectedDcModifier)
    {
        // Arrange
        var player = CreateTestPlayer(wits: 14, fortitude: 10);
        var state = TrackingState.Create(player.Id.ToString(), "test target", TrailAge.Standard);

        // Transition to Pursuit first, then to Lost
        state.TransitionToPursuit();
        state.TransitionToLost();

        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var modifiers = TrackingModifiers.Default;

        // Act
        var result = await _service.AttemptRecoveryAsync(
            player,
            state.TrackingId,
            recoveryType,
            modifiers);

        // Assert - The check was performed with the correct DC modifier
        // Base DC for Standard trail is 16
        // Expected effective DC = 16 + expectedDcModifier
        var expectedEffectiveDc = 16 + expectedDcModifier;

        result.Check.BaseDc.Should().Be(16,
            because: "base DC should be from Standard trail age");

        // Note: EffectiveDc includes base + cumulative + condition + recovery modifiers
        // With no cumulative or condition modifiers, it should be base + recovery modifier
        result.Check.EffectiveDc.Should().Be(expectedEffectiveDc,
            because: $"{recoveryType} should add +{expectedDcModifier} to DC");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INITIATE TRACKING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that initiating tracking creates proper state and performs acquisition check.
    /// </summary>
    [Test]
    public async Task InitiateTracking_CreatesStateAndPerformsCheck()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 14, fortitude: 10);
        var modifiers = TrackingModifiers.Default;

        // Act
        var result = await _service.InitiateTrackingAsync(
            player,
            "fleeing raiders",
            TrailAge.Fresh,
            modifiers);

        // Assert
        result.Should().NotBeNull();
        result.WasAcquisition.Should().BeTrue(
            because: "this was an acquisition attempt");
        result.Check.Phase.Should().Be(TrackingPhase.Acquisition);
        result.Check.BaseDc.Should().Be(12,
            because: "Fresh trail age has DC 12");

        // Verify repository was called to save
        _mockRepository.Verify(
            r => r.SaveAsync(It.IsAny<TrackingState>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that initiating tracking fails when player already has active tracking.
    /// </summary>
    [Test]
    public async Task InitiateTracking_ThrowsWhenAlreadyTracking()
    {
        // Arrange
        var player = CreateTestPlayer();
        _mockRepository.Setup(r => r.HasActiveTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var modifiers = TrackingModifiers.Default;

        // Act & Assert
        var act = () => _service.InitiateTrackingAsync(
            player,
            "test target",
            TrailAge.Standard,
            modifiers);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already has an active pursuit*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTINUE PURSUIT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that continuing pursuit requires correct phase.
    /// </summary>
    [Test]
    public async Task ContinuePursuit_ThrowsWhenNotInPursuitPhase()
    {
        // Arrange
        var player = CreateTestPlayer();
        var state = TrackingState.Create(player.Id.ToString(), "test target", TrailAge.Standard);
        // State is in Acquisition phase

        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var modifiers = TrackingModifiers.Default;

        // Act & Assert
        var act = () => _service.ContinuePursuitAsync(
            player,
            state.TrackingId,
            modifiers,
            1.0f);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Must be in Pursuit phase*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CLOSE IN TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that closing in within 50 feet results in automatic success.
    /// </summary>
    [Test]
    public async Task CloseIn_AutoSuccessWithin50Feet()
    {
        // Arrange
        var player = CreateTestPlayer();
        var state = TrackingState.Create(player.Id.ToString(), "test target", TrailAge.Standard);
        state.TransitionToPursuit();

        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var modifiers = TrackingModifiers.Default;

        // Act
        var result = await _service.CloseInAsync(
            player,
            state.TrackingId,
            modifiers,
            currentDistanceFeet: 40); // Within 50ft threshold

        // Assert
        result.IsSuccess.Should().BeTrue(
            because: "within 50ft should be automatic success");
        result.TargetFound.Should().BeTrue(
            because: "auto-success should find the target");
        result.TrackingState.Status.Should().Be(TrackingStatus.TargetFound);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ABANDON TRACKING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that abandoning tracking sets the correct status.
    /// </summary>
    [Test]
    public async Task AbandonTracking_SetsAbandonedStatus()
    {
        // Arrange
        var player = CreateTestPlayer();
        var state = TrackingState.Create(player.Id.ToString(), "test target", TrailAge.Standard);

        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        // Act
        await _service.AbandonTrackingAsync(state.TrackingId);

        // Assert
        state.Status.Should().Be(TrackingStatus.Abandoned);
        state.IsComplete.Should().BeTrue();

        _mockRepository.Verify(
            r => r.SaveAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that abandoning already completed tracking throws exception.
    /// </summary>
    [Test]
    public async Task AbandonTracking_ThrowsWhenAlreadyComplete()
    {
        // Arrange
        var state = TrackingState.Create("test-player-id", "test target", TrailAge.Standard);
        state.TransitionToCold();

        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        // Act & Assert
        var act = () => _service.AbandonTrackingAsync(state.TrackingId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot abandon tracking*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRACKING STATE QUERY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetTrackingStateAsync returns state from repository.
    /// </summary>
    [Test]
    public async Task GetTrackingStateAsync_ReturnsStateFromRepository()
    {
        // Arrange
        var state = TrackingState.Create("test-player-id", "test target", TrailAge.Standard);

        _mockRepository.Setup(r => r.GetByIdAsync(state.TrackingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        // Act
        var result = await _service.GetTrackingStateAsync(state.TrackingId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(state);
    }

    /// <summary>
    /// Verifies that GetActiveTrackingAsync returns active tracking from repository.
    /// </summary>
    [Test]
    public async Task GetActiveTrackingAsync_ReturnsActiveTracking()
    {
        // Arrange
        var playerId = "test-player-id";
        var state = TrackingState.Create(playerId, "test target", TrailAge.Standard);

        _mockRepository.Setup(r => r.GetActiveByTrackerAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        // Act
        var result = await _service.GetActiveTrackingAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(state);
    }

    /// <summary>
    /// Verifies that HasActiveTrackingAsync delegates to repository.
    /// </summary>
    [Test]
    public async Task HasActiveTrackingAsync_DelegatesToRepository()
    {
        // Arrange
        var playerId = "test-player-id";
        _mockRepository.Setup(r => r.HasActiveTrackingAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.HasActiveTrackingAsync(playerId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(
            r => r.HasActiveTrackingAsync(playerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRACKING RESULT FACTORY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that TrackingResult factory methods produce correct results.
    /// </summary>
    [Test]
    public void TrackingResult_FactoryMethods_ProduceCorrectResults()
    {
        // Arrange
        var state = TrackingState.Create("test-player-id", "test target", TrailAge.Standard);
        var check = CreateMockCheck(isSuccess: true);
        var discovery = TrackingDiscovery.DirectionOnly("Northeast");

        // Act & Assert - Success factory
        var successResult = TrackingResult.Success(
            TrackingPhase.Pursuit,
            TrackingPhase.Acquisition,
            check,
            state,
            "Found the trail!",
            new[] { "Continue pursuit" },
            discovery);

        successResult.IsSuccess.Should().BeTrue();
        successResult.PhaseChanged.Should().BeTrue();
        successResult.HasDiscovery.Should().BeTrue();

        // Act & Assert - Failure factory
        var failureResult = TrackingResult.Failure(
            TrackingPhase.Lost,
            TrackingPhase.Pursuit,
            check,
            state,
            "Lost the trail.",
            new[] { "Backtrack", "Spiral search" });

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.TrailLost.Should().BeTrue();

        // Act & Assert - TargetLocated factory
        state.MarkTargetFound();
        var locatedResult = TrackingResult.TargetLocated(
            TrackingPhase.ClosingIn,
            check,
            state,
            "Target found!");

        locatedResult.TargetFound.Should().BeTrue();
        locatedResult.IsComplete.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that TrackingDiscovery factory methods work correctly.
    /// </summary>
    [Test]
    public void TrackingDiscovery_FactoryMethods_ProduceCorrectResults()
    {
        // Act & Assert - DirectionOnly
        var directionOnly = TrackingDiscovery.DirectionOnly("Northwest");
        directionOnly.Direction.Should().Be("Northwest");
        directionOnly.TargetCount.Should().BeNull();
        directionOnly.TrailAge.Should().BeNull();

        // Act & Assert - WithTargetCount
        var withCount = TrackingDiscovery.WithTargetCount(5, "East");
        withCount.TargetCount.Should().Be(5);
        withCount.Direction.Should().Be("East");

        // Act & Assert - WithTrailAge
        var withAge = TrackingDiscovery.WithTrailAge(TrailAge.Fresh, "South");
        withAge.TrailAge.Should().Be(TrailAge.Fresh);
        withAge.Direction.Should().Be("South");

        // Act & Assert - Full
        var full = TrackingDiscovery.Full(3, TrailAge.Standard, "Northeast", "One is limping");
        full.TargetCount.Should().Be(3);
        full.TrailAge.Should().Be(TrailAge.Standard);
        full.Direction.Should().Be("Northeast");
        full.AdditionalInfo.Should().Be("One is limping");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a mock TrackingCheck for testing purposes.
    /// </summary>
    /// <param name="isSuccess">Whether the check should be marked as successful.</param>
    /// <returns>A TrackingCheck instance for testing.</returns>
    private static TrackingCheck CreateMockCheck(bool isSuccess)
    {
        var outcome = isSuccess ? SkillOutcome.FullSuccess : SkillOutcome.Failure;
        var netSuccesses = isSuccess ? 3 : -1;

        return TrackingCheck.Create(
            checkId: Guid.NewGuid().ToString(),
            phase: TrackingPhase.Acquisition,
            baseDc: 12,
            effectiveDc: 12,
            netSuccesses: netSuccesses,
            outcome: outcome,
            terrain: NavigationTerrainType.OpenWasteland,
            distance: 0f,
            timestamp: DateTime.UtcNow,
            modifiers: "None");
    }
}
