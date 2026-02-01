// ═══════════════════════════════════════════════════════════════════════════════
// StressServiceTests.cs
// Unit tests for StressService — the core implementation of IStressService
// managing stress application, WILL-based resistance, recovery formulas,
// and post-Trauma Check reset operations.
// Version: 0.18.0d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Exceptions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Builders;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class StressServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════

    private Mock<IPlayerRepository> _mockRepository = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<ILogger<StressService>> _mockLogger = null!;
    private StressService _service = null!;
    private Player _testPlayer = null!;
    private Guid _characterId;

    /// <summary>Default WILL attribute for test player.</summary>
    private const int DefaultWill = 4;

    /// <summary>Default starting stress for test player.</summary>
    private const int DefaultStress = 35;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IPlayerRepository>();
        _mockDiceService = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<StressService>>();

        // Create test player with known stress and WILL values
        _testPlayer = CreateTestPlayer(DefaultStress, DefaultWill);
        _characterId = _testPlayer.Id;

        // Configure repository to return test player
        _mockRepository
            .Setup(r => r.GetByIdAsync(_characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testPlayer);

        // Configure repository update to succeed
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SaveResult.Succeeded(_characterId));

        _service = new StressService(
            _mockRepository.Object,
            _mockDiceService.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StressService(null!, _mockDiceService.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("playerRepository");
    }

    [Test]
    public void Constructor_NullDiceService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StressService(_mockRepository.Object, null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("diceService");
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StressService(_mockRepository.Object, _mockDiceService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET STRESS STATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetStressState_ReturnsCorrectState()
    {
        // Act
        var state = _service.GetStressState(_characterId);

        // Assert
        state.CurrentStress.Should().Be(DefaultStress);
        state.Threshold.Should().Be(StressThreshold.Uneasy);
        state.DefensePenalty.Should().Be(1); // floor(35/20) = 1
    }

    [Test]
    public void GetStressState_CharacterNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Player?)null);

        // Act
        var act = () => _service.GetStressState(missingId);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    [Test]
    public void GetStressState_ZeroStress_ReturnsCalmState()
    {
        // Arrange
        var player = CreateTestPlayer(0, DefaultWill);
        SetupPlayer(player);

        // Act
        var state = _service.GetStressState(player.Id);

        // Assert
        state.CurrentStress.Should().Be(0);
        state.Threshold.Should().Be(StressThreshold.Calm);
        state.DefensePenalty.Should().Be(0);
        state.HasSkillDisadvantage.Should().BeFalse();
        state.RequiresTraumaCheck.Should().BeFalse();
    }

    [Test]
    public void GetStressState_MaxStress_ReturnsTraumaState()
    {
        // Arrange
        var player = CreateTestPlayer(100, DefaultWill);
        SetupPlayer(player);

        // Act
        var state = _service.GetStressState(player.Id);

        // Assert
        state.CurrentStress.Should().Be(100);
        state.Threshold.Should().Be(StressThreshold.Trauma);
        state.DefensePenalty.Should().Be(5);
        state.HasSkillDisadvantage.Should().BeTrue();
        state.RequiresTraumaCheck.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET DEFENSE PENALTY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0)]    // Calm: floor(0/20) = 0
    [TestCase(19, 0)]   // Calm boundary: floor(19/20) = 0
    [TestCase(20, 1)]   // Uneasy: floor(20/20) = 1
    [TestCase(39, 1)]   // Uneasy boundary: floor(39/20) = 1
    [TestCase(40, 2)]   // Anxious: floor(40/20) = 2
    [TestCase(59, 2)]   // Anxious boundary: floor(59/20) = 2
    [TestCase(60, 3)]   // Panicked: floor(60/20) = 3
    [TestCase(79, 3)]   // Panicked boundary: floor(79/20) = 3
    [TestCase(80, 4)]   // Breaking: floor(80/20) = 4
    [TestCase(99, 4)]   // Breaking boundary: floor(99/20) = 4
    [TestCase(100, 5)]  // Trauma: floor(100/20) = 5
    public void GetDefensePenalty_ReturnsCorrectPenalty(int stress, int expectedPenalty)
    {
        // Arrange
        var player = CreateTestPlayer(stress, DefaultWill);
        SetupPlayer(player);

        // Act
        var penalty = _service.GetDefensePenalty(player.Id);

        // Assert
        penalty.Should().Be(expectedPenalty);
    }

    // ═══════════════════════════════════════════════════════════════
    // HAS SKILL DISADVANTAGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, false)]    // Calm: no disadvantage
    [TestCase(39, false)]   // Uneasy: no disadvantage
    [TestCase(59, false)]   // Anxious: no disadvantage
    [TestCase(79, false)]   // Panicked: no disadvantage
    [TestCase(80, true)]    // Breaking: disadvantage starts
    [TestCase(99, true)]    // Breaking boundary: disadvantage
    [TestCase(100, true)]   // Trauma: disadvantage
    public void HasSkillDisadvantage_ReturnsCorrectFlag(int stress, bool expectedDisadvantage)
    {
        // Arrange
        var player = CreateTestPlayer(stress, DefaultWill);
        SetupPlayer(player);

        // Act
        var hasDisadvantage = _service.HasSkillDisadvantage(player.Id);

        // Assert
        hasDisadvantage.Should().Be(expectedDisadvantage);
    }

    // ═══════════════════════════════════════════════════════════════
    // REQUIRES TRAUMA CHECK TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, false)]
    [TestCase(50, false)]
    [TestCase(99, false)]
    [TestCase(100, true)]
    public void RequiresTraumaCheck_ReturnsCorrectFlag(int stress, bool expected)
    {
        // Arrange
        var player = CreateTestPlayer(stress, DefaultWill);
        SetupPlayer(player);

        // Act
        var requires = _service.RequiresTraumaCheck(player.Id);

        // Assert
        requires.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // APPLY STRESS TESTS — NO RESISTANCE
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyStress_WithNoResistance_AppliesFullStress()
    {
        // Arrange — default player at stress 35

        // Act
        var result = _service.ApplyStress(
            _characterId, 20, StressSource.Combat, resistDc: 0);

        // Assert
        result.PreviousStress.Should().Be(35);
        result.NewStress.Should().Be(55);
        result.StressGained.Should().Be(20);
        result.Source.Should().Be(StressSource.Combat);
        result.ResistanceResult.Should().BeNull();
    }

    [Test]
    public void ApplyStress_ClampsToMax100()
    {
        // Arrange — player at stress 85
        var player = CreateTestPlayer(85, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyStress(
            player.Id, 30, StressSource.Combat, resistDc: 0);

        // Assert — 85 + 30 = 115, clamped to 100
        result.NewStress.Should().Be(100);
        result.StressGained.Should().Be(15); // 100 - 85 = 15
    }

    [Test]
    public void ApplyStress_ZeroAmount_NoChange()
    {
        // Act
        var result = _service.ApplyStress(
            _characterId, 0, StressSource.Combat, resistDc: 0);

        // Assert
        result.PreviousStress.Should().Be(35);
        result.NewStress.Should().Be(35);
        result.StressGained.Should().Be(0);
    }

    [Test]
    public void ApplyStress_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.ApplyStress(
            _characterId, -5, StressSource.Combat);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ApplyStress_DetectsThresholdCrossing()
    {
        // Arrange — player at stress 35 (Uneasy), adding 10 → 45 (Anxious)
        var player = CreateTestPlayer(35, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyStress(
            player.Id, 10, StressSource.Exploration, resistDc: 0);

        // Assert
        result.PreviousThreshold.Should().Be(StressThreshold.Uneasy);
        result.NewThreshold.Should().Be(StressThreshold.Anxious);
        result.ThresholdCrossed.Should().BeTrue();
    }

    [Test]
    public void ApplyStress_NoThresholdCrossing_WhenStayingInSameRange()
    {
        // Arrange — player at stress 21 (Uneasy), adding 5 → 26 (still Uneasy)
        var player = CreateTestPlayer(21, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyStress(
            player.Id, 5, StressSource.Combat, resistDc: 0);

        // Assert
        result.ThresholdCrossed.Should().BeFalse();
    }

    [Test]
    public void ApplyStress_TriggersTraumaCheck_WhenReaching100()
    {
        // Arrange — player at stress 85
        var player = CreateTestPlayer(85, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyStress(
            player.Id, 20, StressSource.Combat, resistDc: 0);

        // Assert — 85 + 20 = 105, clamped to 100
        result.NewStress.Should().Be(100);
        result.TraumaCheckTriggered.Should().BeTrue();
        result.ThresholdCrossed.Should().BeTrue();
    }

    [Test]
    public void ApplyStress_DoesNotTriggerTraumaCheck_At99()
    {
        // Arrange — player at stress 85
        var player = CreateTestPlayer(85, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyStress(
            player.Id, 14, StressSource.Combat, resistDc: 0);

        // Assert — 85 + 14 = 99
        result.NewStress.Should().Be(99);
        result.TraumaCheckTriggered.Should().BeFalse();
    }

    [Test]
    public void ApplyStress_PersistsChange_ToRepository()
    {
        // Act
        _service.ApplyStress(_characterId, 20, StressSource.Combat, resistDc: 0);

        // Assert — verify repository update was called
        _mockRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ApplyStress_UpdatesPlayerEntity_StressValue()
    {
        // Act
        _service.ApplyStress(_characterId, 20, StressSource.Combat, resistDc: 0);

        // Assert — verify the player's stress was updated
        _testPlayer.PsychicStress.Should().Be(55);
    }

    [Test]
    [TestCase(StressSource.Combat)]
    [TestCase(StressSource.Exploration)]
    [TestCase(StressSource.Narrative)]
    [TestCase(StressSource.Heretical)]
    [TestCase(StressSource.Environmental)]
    [TestCase(StressSource.Corruption)]
    public void ApplyStress_RecordsCorrectSource(StressSource source)
    {
        // Act
        var result = _service.ApplyStress(
            _characterId, 10, source, resistDc: 0);

        // Assert
        result.Source.Should().Be(source);
    }

    // ═══════════════════════════════════════════════════════════════
    // APPLY STRESS TESTS — WITH RESISTANCE
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyStress_WithResistance_0Successes_AppliesFullStress()
    {
        // Arrange — 0 successes = 0% reduction
        SetupDiceRoll(netSuccesses: 0);

        // Act
        var result = _service.ApplyStress(
            _characterId, 20, StressSource.Combat, resistDc: 2);

        // Assert — 35 + 20 = 55
        result.StressGained.Should().Be(20);
        result.NewStress.Should().Be(55);
        result.ResistanceResult.Should().NotBeNull();
        result.ResistanceResult!.Value.ReductionPercent.Should().Be(0.00m);
    }

    [Test]
    public void ApplyStress_WithResistance_1Success_Reduces50Percent()
    {
        // Arrange — 1 success = 50% reduction
        SetupDiceRoll(netSuccesses: 1);

        // Act
        var result = _service.ApplyStress(
            _characterId, 20, StressSource.Combat, resistDc: 2);

        // Assert — 20 * 0.50 = 10, 35 + 10 = 45
        result.StressGained.Should().Be(10);
        result.NewStress.Should().Be(45);
        result.ResistanceResult!.Value.ReductionPercent.Should().Be(0.50m);
    }

    [Test]
    public void ApplyStress_WithResistance_2Successes_Reduces75Percent()
    {
        // Arrange — 2 successes = 75% reduction
        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.ApplyStress(
            _characterId, 20, StressSource.Combat, resistDc: 2);

        // Assert — 20 * 0.25 = 5, 35 + 5 = 40
        result.StressGained.Should().Be(5);
        result.NewStress.Should().Be(40);
        result.ResistanceResult!.Value.ReductionPercent.Should().Be(0.75m);
    }

    [Test]
    public void ApplyStress_WithResistance_3Successes_Reduces75Percent()
    {
        // Arrange — 3 successes = 75% reduction (same as 2)
        SetupDiceRoll(netSuccesses: 3);

        // Act
        var result = _service.ApplyStress(
            _characterId, 20, StressSource.Combat, resistDc: 2);

        // Assert — 20 * 0.25 = 5, 35 + 5 = 40
        result.StressGained.Should().Be(5);
        result.ResistanceResult!.Value.ReductionPercent.Should().Be(0.75m);
    }

    [Test]
    public void ApplyStress_WithResistance_4PlusSuccesses_Reduces100Percent()
    {
        // Arrange — 4+ successes = 100% reduction
        SetupDiceRoll(netSuccesses: 4);

        // Act
        var result = _service.ApplyStress(
            _characterId, 20, StressSource.Combat, resistDc: 2);

        // Assert — 20 * 0.00 = 0, 35 + 0 = 35
        result.StressGained.Should().Be(0);
        result.NewStress.Should().Be(35);
        result.ResistanceResult!.Value.ReductionPercent.Should().Be(1.00m);
        result.ResistanceResult!.Value.WasFullyResisted.Should().BeTrue();
    }

    [Test]
    public void ApplyStress_WithResistance_5Successes_AlsoReduces100Percent()
    {
        // Arrange — 5 successes = still 100% reduction
        SetupDiceRoll(netSuccesses: 5);

        // Act
        var result = _service.ApplyStress(
            _characterId, 20, StressSource.Combat, resistDc: 2);

        // Assert
        result.StressGained.Should().Be(0);
        result.ResistanceResult!.Value.ReductionPercent.Should().Be(1.00m);
    }

    [Test]
    public void ApplyStress_WithResistance_UsesWillForDicePool()
    {
        // Arrange — player with WILL=6
        var player = CreateTestPlayer(35, will: 6);
        SetupPlayer(player);
        SetupDiceRoll(netSuccesses: 1);

        // Act
        _service.ApplyStress(player.Id, 20, StressSource.Combat, resistDc: 2);

        // Assert — verify dice service was called with WILL dice pool (6d10)
        _mockDiceService.Verify(
            d => d.Roll(
                It.Is<DicePool>(p => p.Count == 6 && p.DiceType == DiceType.D10),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()),
            Times.Once);
    }

    [Test]
    public void ApplyStress_WithResistDc0_DoesNotRollDice()
    {
        // Act
        _service.ApplyStress(_characterId, 20, StressSource.Combat, resistDc: 0);

        // Assert — dice service should not be called
        _mockDiceService.Verify(
            d => d.Roll(
                It.IsAny<DicePool>(),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // RECOVER STRESS TESTS — REST TYPE
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RecoverStress_ShortRest_UsesWillTimes2()
    {
        // Arrange — player at stress 50, WILL=4 → recovery = 4 * 2 = 8
        var player = CreateTestPlayer(50, will: 4);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, RestType.Short);

        // Assert
        result.AmountRecovered.Should().Be(8);
        result.NewStress.Should().Be(42);
    }

    [Test]
    public void RecoverStress_LongRest_UsesWillTimes5()
    {
        // Arrange — player at stress 50, WILL=4 → recovery = 4 * 5 = 20
        var player = CreateTestPlayer(50, will: 4);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, RestType.Long);

        // Assert
        result.AmountRecovered.Should().Be(20);
        result.NewStress.Should().Be(30);
    }

    [Test]
    public void RecoverStress_Sanctuary_FullyResets()
    {
        // Arrange — player at stress 85
        var player = CreateTestPlayer(85, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, RestType.Sanctuary);

        // Assert
        result.NewStress.Should().Be(0);
        result.AmountRecovered.Should().Be(85);
        result.IsFullyRecovered.Should().BeTrue();
    }

    [Test]
    public void RecoverStress_Milestone_FixedRecoveryOf25()
    {
        // Arrange — player at stress 50
        var player = CreateTestPlayer(50, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, RestType.Milestone);

        // Assert
        result.AmountRecovered.Should().Be(25);
        result.NewStress.Should().Be(25);
    }

    [Test]
    public void RecoverStress_Milestone_IgnoresWillAttribute()
    {
        // Arrange — two players with different WILL but same stress
        var player1 = CreateTestPlayer(50, will: 2);
        var player2 = CreateTestPlayer(50, will: 10);
        SetupPlayer(player1);

        // Act
        var result1 = _service.RecoverStress(player1.Id, RestType.Milestone);
        SetupPlayer(player2);
        var result2 = _service.RecoverStress(player2.Id, RestType.Milestone);

        // Assert — both recover exactly 25 regardless of WILL
        result1.AmountRecovered.Should().Be(25);
        result2.AmountRecovered.Should().Be(25);
    }

    [Test]
    public void RecoverStress_ClampsToMinimum0()
    {
        // Arrange — player at stress 10, WILL=10 → WILL*5 = 50, but clamp to 0
        var player = CreateTestPlayer(10, will: 10);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, RestType.Long);

        // Assert — 10 - 50 would be -40, clamped to 0
        result.NewStress.Should().Be(0);
        result.AmountRecovered.Should().Be(10);
        result.IsFullyRecovered.Should().BeTrue();
    }

    [Test]
    public void RecoverStress_DetectsThresholdDrop()
    {
        // Arrange — player at stress 85 (Breaking), WILL=10 → Long = 50 → 35 (Uneasy)
        var player = CreateTestPlayer(85, will: 10);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, RestType.Long);

        // Assert
        result.PreviousThreshold.Should().Be(StressThreshold.Breaking);
        result.NewThreshold.Should().Be(StressThreshold.Uneasy);
        result.ThresholdDropped.Should().BeTrue();
    }

    [Test]
    public void RecoverStress_NoThresholdDrop_WhenStayingInSameRange()
    {
        // Arrange — player at stress 45 (Anxious), WILL=1 → Short = 2 → 43 (still Anxious)
        var player = CreateTestPlayer(45, will: 1);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, RestType.Short);

        // Assert
        result.ThresholdDropped.Should().BeFalse();
    }

    [Test]
    public void RecoverStress_PersistsChange_ToRepository()
    {
        // Act
        _service.RecoverStress(_characterId, RestType.Short);

        // Assert
        _mockRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // RECOVER STRESS TESTS — NAMED SOURCE
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RecoverStress_NamedSource_ReducesByAmount()
    {
        // Arrange — player at stress 50
        var player = CreateTestPlayer(50, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, 15, "Calming Draught");

        // Assert
        result.AmountRecovered.Should().Be(15);
        result.NewStress.Should().Be(35);
    }

    [Test]
    public void RecoverStress_NamedSource_ClampsToMinimum0()
    {
        // Arrange — player at stress 10
        var player = CreateTestPlayer(10, DefaultWill);
        SetupPlayer(player);

        // Act
        var result = _service.RecoverStress(player.Id, 50, "Sanctuary Ward");

        // Assert
        result.NewStress.Should().Be(0);
        result.AmountRecovered.Should().Be(10);
    }

    [Test]
    public void RecoverStress_NamedSource_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.RecoverStress(_characterId, -5, "Invalid Source");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RecoverStress_NamedSource_NullSource_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.RecoverStress(_characterId, 10, null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RecoverStress_NamedSource_EmptySource_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.RecoverStress(_characterId, 10, "");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RecoverStress_NamedSource_WhitespaceSource_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.RecoverStress(_characterId, 10, "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // RESET AFTER TRAUMA CHECK TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ResetAfterTraumaCheck_Passed_SetsStressTo75()
    {
        // Arrange — player at stress 100
        var player = CreateTestPlayer(100, DefaultWill);
        SetupPlayer(player);

        // Act
        _service.ResetAfterTraumaCheck(player.Id, passed: true);

        // Assert
        player.PsychicStress.Should().Be(75);
    }

    [Test]
    public void ResetAfterTraumaCheck_Failed_SetsStressTo50()
    {
        // Arrange — player at stress 100
        var player = CreateTestPlayer(100, DefaultWill);
        SetupPlayer(player);

        // Act
        _service.ResetAfterTraumaCheck(player.Id, passed: false);

        // Assert
        player.PsychicStress.Should().Be(50);
    }

    [Test]
    public void ResetAfterTraumaCheck_PersistsChange_ToRepository()
    {
        // Arrange — player at stress 100
        var player = CreateTestPlayer(100, DefaultWill);
        SetupPlayer(player);

        // Act
        _service.ResetAfterTraumaCheck(player.Id, passed: true);

        // Assert
        _mockRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ResetAfterTraumaCheck_CharacterNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Player?)null);

        // Act
        var act = () => _service.ResetAfterTraumaCheck(missingId, passed: true);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // PERFORM RESISTANCE CHECK TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0.00)]
    [TestCase(1, 0.50)]
    [TestCase(2, 0.75)]
    [TestCase(3, 0.75)]
    [TestCase(4, 1.00)]
    [TestCase(5, 1.00)]
    public void PerformResistanceCheck_CalculatesCorrectReduction(
        int successes, double expectedReduction)
    {
        // Arrange
        SetupDiceRoll(netSuccesses: successes);

        // Act
        var result = _service.PerformResistanceCheck(_characterId, 20, 2);

        // Assert
        result.ReductionPercent.Should().Be((decimal)expectedReduction);
    }

    [Test]
    public void PerformResistanceCheck_CalculatesCorrectFinalStress()
    {
        // Arrange — 1 success = 50% reduction on 20 base = 10 final
        SetupDiceRoll(netSuccesses: 1);

        // Act
        var result = _service.PerformResistanceCheck(_characterId, 20, 2);

        // Assert
        result.BaseStress.Should().Be(20);
        result.FinalStress.Should().Be(10);
        result.Successes.Should().Be(1);
        result.Succeeded.Should().BeTrue();
    }

    [Test]
    public void PerformResistanceCheck_NegativeBaseStress_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.PerformResistanceCheck(_characterId, -5, 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void PerformResistanceCheck_NegativeDc_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.PerformResistanceCheck(_characterId, 20, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void PerformResistanceCheck_ZeroBaseStress_ReturnsZeroFinal()
    {
        // Arrange
        SetupDiceRoll(netSuccesses: 0);

        // Act
        var result = _service.PerformResistanceCheck(_characterId, 0, 2);

        // Assert
        result.FinalStress.Should().Be(0);
        result.BaseStress.Should().Be(0);
    }

    [Test]
    public void PerformResistanceCheck_Truncation_15BaseWith50PercentReduction()
    {
        // Arrange — 1 success = 50%, 15 * 0.5 = 7.5, truncated to 7
        SetupDiceRoll(netSuccesses: 1);

        // Act
        var result = _service.PerformResistanceCheck(_characterId, 15, 2);

        // Assert
        result.FinalStress.Should().Be(7); // Truncated, not rounded
    }

    // ═══════════════════════════════════════════════════════════════
    // INTEGRATION-STYLE TESTS (multi-step scenarios)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Scenario_ApplyStress_ThenRecover_StressUpdatesCorrectly()
    {
        // Arrange — player at stress 35
        // Apply 25 stress → 60 (Panicked)
        var applyResult = _service.ApplyStress(
            _characterId, 25, StressSource.Combat, resistDc: 0);
        applyResult.NewStress.Should().Be(60);

        // Act — Short Rest recovery with WILL=4 → 4*2 = 8
        var recoverResult = _service.RecoverStress(_characterId, RestType.Short);

        // Assert — 60 - 8 = 52
        recoverResult.NewStress.Should().Be(52);
        recoverResult.AmountRecovered.Should().Be(8);
    }

    [Test]
    public void Scenario_StressReaches100_ThenTraumaReset()
    {
        // Arrange — player at stress 85
        var player = CreateTestPlayer(85, DefaultWill);
        SetupPlayer(player);

        // Apply 15 stress → 100 (Trauma)
        var applyResult = _service.ApplyStress(
            player.Id, 15, StressSource.Heretical, resistDc: 0);
        applyResult.TraumaCheckTriggered.Should().BeTrue();

        // Act — Failed trauma check → stress to 50
        _service.ResetAfterTraumaCheck(player.Id, passed: false);

        // Assert
        player.PsychicStress.Should().Be(50);
        var state = _service.GetStressState(player.Id);
        state.Threshold.Should().Be(StressThreshold.Anxious);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a test <see cref="Player"/> with specified stress and WILL values.
    /// </summary>
    /// <param name="stress">Initial psychic stress value (0-100).</param>
    /// <param name="will">WILL attribute value for resistance checks and recovery formulas.</param>
    /// <returns>A new <see cref="Player"/> configured for testing.</returns>
    private static Player CreateTestPlayer(int stress, int will)
    {
        var player = PlayerBuilder.Create()
            .WithName("TestHero")
            .WithAttributes(
                might: 8,
                fortitude: 8,
                will: will,
                wits: 8,
                finesse: 8)
            .Build();

        player.SetPsychicStress(stress);
        return player;
    }

    /// <summary>
    /// Configures the mock repository to return the specified player when queried by ID.
    /// </summary>
    /// <param name="player">The player to return from repository lookups.</param>
    private void SetupPlayer(Player player)
    {
        _mockRepository
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
    }

    /// <summary>
    /// Configures the mock dice service to return a roll result with the specified net successes.
    /// </summary>
    /// <param name="netSuccesses">The number of net successes to return from the roll.</param>
    private void SetupDiceRoll(int netSuccesses)
    {
        _mockDiceService
            .Setup(d => d.Roll(
                It.IsAny<DicePool>(),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .Returns(new DiceRollResult
            {
                Pool = DicePool.D10(DefaultWill),
                Rolls = [],
                ExplosionRolls = [],
                TotalSuccesses = netSuccesses,
                TotalBotches = 0,
                NetSuccesses = netSuccesses,
                IsCriticalSuccess = netSuccesses >= 5,
                IsFumble = false
            });
    }
}
