// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionServiceTests.cs
// Unit tests for CorruptionService — the core implementation of ICorruptionService
// managing corruption accumulation, Blot-Priest transfer, rare removal, stage-based
// skill modifiers, resource penalty queries, and WILL-based Terminal Error checks.
// Version: 0.18.1d
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
public class CorruptionServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════

    private Mock<ICorruptionRepository> _mockCorruptionRepository = null!;
    private Mock<IPlayerRepository> _mockPlayerRepository = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<ILogger<CorruptionService>> _mockLogger = null!;
    private CorruptionService _service = null!;
    private Player _testPlayer = null!;
    private CorruptionTracker _testTracker = null!;
    private Guid _characterId;

    /// <summary>Default WILL attribute for test player.</summary>
    private const int DefaultWill = 4;

    /// <summary>Default starting corruption for test tracker.</summary>
    private const int DefaultCorruption = 30;

    [SetUp]
    public void SetUp()
    {
        _mockCorruptionRepository = new Mock<ICorruptionRepository>();
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockDiceService = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<CorruptionService>>();

        // Create test player with known WILL value
        _testPlayer = CreateTestPlayer(DefaultWill);
        _characterId = _testPlayer.Id;

        // Create test tracker with known corruption
        _testTracker = CreateTestTracker(_characterId, DefaultCorruption);

        // Configure corruption repository to return test tracker
        _mockCorruptionRepository
            .Setup(r => r.GetByCharacterIdAsync(_characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testTracker);

        // Configure player repository to return test player
        _mockPlayerRepository
            .Setup(r => r.GetByIdAsync(_characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testPlayer);

        _service = new CorruptionService(
            _mockCorruptionRepository.Object,
            _mockPlayerRepository.Object,
            _mockDiceService.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullCorruptionRepository_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CorruptionService(
            null!, _mockPlayerRepository.Object, _mockDiceService.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("corruptionRepository");
    }

    [Test]
    public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CorruptionService(
            _mockCorruptionRepository.Object, null!, _mockDiceService.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("playerRepository");
    }

    [Test]
    public void Constructor_NullDiceService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CorruptionService(
            _mockCorruptionRepository.Object, _mockPlayerRepository.Object, null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("diceService");
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CorruptionService(
            _mockCorruptionRepository.Object, _mockPlayerRepository.Object, _mockDiceService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET CORRUPTION STATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetCorruptionState_ReturnsCorrectState()
    {
        // Act
        var state = _service.GetCorruptionState(_characterId);

        // Assert
        state.CurrentCorruption.Should().Be(DefaultCorruption);
        state.Stage.Should().Be(CorruptionStage.Tainted);
    }

    [Test]
    public void GetCorruptionState_ZeroCorruption_ReturnsUncorruptedState()
    {
        // Arrange
        var tracker = CreateTestTracker(_characterId, 0);
        SetupTracker(tracker);

        // Act
        var state = _service.GetCorruptionState(_characterId);

        // Assert
        state.CurrentCorruption.Should().Be(0);
        state.Stage.Should().Be(CorruptionStage.Uncorrupted);
        state.IsUncorrupted.Should().BeTrue();
    }

    [Test]
    public void GetCorruptionState_MaxCorruption_ReturnsConsumedState()
    {
        // Arrange
        var tracker = CreateTestTracker(_characterId, 100);
        SetupTracker(tracker);

        // Act
        var state = _service.GetCorruptionState(_characterId);

        // Assert
        state.CurrentCorruption.Should().Be(100);
        state.Stage.Should().Be(CorruptionStage.Consumed);
        state.IsConsumed.Should().BeTrue();
    }

    [Test]
    public void GetCorruptionState_CharacterNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        SetupNoTracker(missingId);
        SetupNoPlayer(missingId);

        // Act
        var act = () => _service.GetCorruptionState(missingId);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET MAX HP PENALTY PERCENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0)]      // floor(0/10) * 5 = 0%
    [TestCase(9, 0)]      // floor(9/10) * 5 = 0%
    [TestCase(10, 5)]     // floor(10/10) * 5 = 5%
    [TestCase(19, 5)]     // floor(19/10) * 5 = 5%
    [TestCase(20, 10)]    // floor(20/10) * 5 = 10%
    [TestCase(45, 20)]    // floor(45/10) * 5 = 20%
    [TestCase(50, 25)]    // floor(50/10) * 5 = 25%
    [TestCase(99, 45)]    // floor(99/10) * 5 = 45%
    [TestCase(100, 50)]   // floor(100/10) * 5 = 50%
    public void GetMaxHpPenaltyPercent_ReturnsCorrectPenalty(int corruption, int expectedPenalty)
    {
        // Arrange
        var tracker = CreateTestTracker(_characterId, corruption);
        SetupTracker(tracker);

        // Act
        var penalty = _service.GetMaxHpPenaltyPercent(_characterId);

        // Assert
        penalty.Should().Be(expectedPenalty);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET MAX AP PENALTY PERCENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0)]      // floor(0/10) * 5 = 0%
    [TestCase(9, 0)]      // floor(9/10) * 5 = 0%
    [TestCase(10, 5)]     // floor(10/10) * 5 = 5%
    [TestCase(19, 5)]     // floor(19/10) * 5 = 5%
    [TestCase(20, 10)]    // floor(20/10) * 5 = 10%
    [TestCase(45, 20)]    // floor(45/10) * 5 = 20%
    [TestCase(50, 25)]    // floor(50/10) * 5 = 25%
    [TestCase(99, 45)]    // floor(99/10) * 5 = 45%
    [TestCase(100, 50)]   // floor(100/10) * 5 = 50%
    public void GetMaxApPenaltyPercent_ReturnsCorrectPenalty(int corruption, int expectedPenalty)
    {
        // Arrange
        var tracker = CreateTestTracker(_characterId, corruption);
        SetupTracker(tracker);

        // Act
        var penalty = _service.GetMaxApPenaltyPercent(_characterId);

        // Assert
        penalty.Should().Be(expectedPenalty);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET RESOLVE DICE PENALTY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0)]      // floor(0/20) = 0
    [TestCase(19, 0)]     // floor(19/20) = 0
    [TestCase(20, 1)]     // floor(20/20) = 1
    [TestCase(39, 1)]     // floor(39/20) = 1
    [TestCase(40, 2)]     // floor(40/20) = 2
    [TestCase(59, 2)]     // floor(59/20) = 2
    [TestCase(60, 3)]     // floor(60/20) = 3
    [TestCase(79, 3)]     // floor(79/20) = 3
    [TestCase(80, 4)]     // floor(80/20) = 4
    [TestCase(99, 4)]     // floor(99/20) = 4
    [TestCase(100, 5)]    // floor(100/20) = 5
    public void GetResolveDicePenalty_ReturnsCorrectPenalty(int corruption, int expectedPenalty)
    {
        // Arrange
        var tracker = CreateTestTracker(_characterId, corruption);
        SetupTracker(tracker);

        // Act
        var penalty = _service.GetResolveDicePenalty(_characterId);

        // Assert
        penalty.Should().Be(expectedPenalty);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET SKILL MODIFIERS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetSkillModifiers_Uncorrupted_ReturnsNoModifiers()
    {
        // Arrange — corruption 0 (Uncorrupted stage)
        var tracker = CreateTestTracker(_characterId, 0);
        SetupTracker(tracker);

        // Act
        var modifiers = _service.GetSkillModifiers(_characterId);

        // Assert
        modifiers.TechBonus.Should().Be(0);
        modifiers.SocialPenalty.Should().Be(0);
        modifiers.FactionLocked.Should().BeFalse();
        modifiers.HasModifiers.Should().BeFalse();
    }

    [Test]
    public void GetSkillModifiers_Tainted_ReturnsTech1Social1()
    {
        // Arrange — corruption 25 (Tainted stage)
        var tracker = CreateTestTracker(_characterId, 25);
        SetupTracker(tracker);

        // Act
        var modifiers = _service.GetSkillModifiers(_characterId);

        // Assert
        modifiers.TechBonus.Should().Be(1);
        modifiers.SocialPenalty.Should().Be(-1);
        modifiers.FactionLocked.Should().BeFalse();
    }

    [Test]
    public void GetSkillModifiers_Infected_ReturnsTech2Social2FactionLocked()
    {
        // Arrange — corruption 50 (Infected stage, faction locked at >= 50)
        var tracker = CreateTestTracker(_characterId, 50);
        SetupTracker(tracker);

        // Act
        var modifiers = _service.GetSkillModifiers(_characterId);

        // Assert
        modifiers.TechBonus.Should().Be(2);
        modifiers.SocialPenalty.Should().Be(-2);
        modifiers.FactionLocked.Should().BeTrue();
    }

    [Test]
    public void GetSkillModifiers_Blighted_ReturnsTech2Social2FactionLocked()
    {
        // Arrange — corruption 65 (Blighted stage)
        var tracker = CreateTestTracker(_characterId, 65);
        SetupTracker(tracker);

        // Act
        var modifiers = _service.GetSkillModifiers(_characterId);

        // Assert
        modifiers.TechBonus.Should().Be(2);
        modifiers.SocialPenalty.Should().Be(-2);
        modifiers.FactionLocked.Should().BeTrue();
    }

    [Test]
    public void GetSkillModifiers_Corrupted_ReturnsTech2Social2FactionLocked()
    {
        // Arrange — corruption 85 (Corrupted stage)
        var tracker = CreateTestTracker(_characterId, 85);
        SetupTracker(tracker);

        // Act
        var modifiers = _service.GetSkillModifiers(_characterId);

        // Assert
        modifiers.TechBonus.Should().Be(2);
        modifiers.SocialPenalty.Should().Be(-2);
        modifiers.FactionLocked.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // ADD CORRUPTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AddCorruption_ValidAmount_ReturnsCorrectResult()
    {
        // Arrange — default tracker at corruption 30

        // Act
        var result = _service.AddCorruption(
            _characterId, 20, CorruptionSource.HereticalAbility);

        // Assert
        result.PreviousCorruption.Should().Be(30);
        result.NewCorruption.Should().Be(50);
        result.AmountGained.Should().Be(20);
        result.Source.Should().Be(CorruptionSource.HereticalAbility);
    }

    [Test]
    public void AddCorruption_ClampsTo100()
    {
        // Arrange — tracker at corruption 90
        var tracker = CreateTestTracker(_characterId, 90);
        SetupTracker(tracker);

        // Act
        var result = _service.AddCorruption(
            _characterId, 30, CorruptionSource.ForlornContact);

        // Assert — 90 + 30 = 120, clamped to 100
        result.NewCorruption.Should().Be(100);
        result.AmountGained.Should().Be(10); // 100 - 90 = 10
        result.IsTerminalError.Should().BeTrue();
    }

    [Test]
    public void AddCorruption_ZeroAmount_NoChange()
    {
        // Act
        var result = _service.AddCorruption(
            _characterId, 0, CorruptionSource.Environmental);

        // Assert
        result.PreviousCorruption.Should().Be(30);
        result.NewCorruption.Should().Be(30);
        result.AmountGained.Should().Be(0);
    }

    [Test]
    public void AddCorruption_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.AddCorruption(
            _characterId, -5, CorruptionSource.MysticMagic);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AddCorruption_DetectsThresholdCrossing()
    {
        // Arrange — tracker at corruption 20, adding 10 → 30 (crosses 25 threshold)
        var tracker = CreateTestTracker(_characterId, 20);
        SetupTracker(tracker);

        // Act
        var result = _service.AddCorruption(
            _characterId, 10, CorruptionSource.Artifact);

        // Assert
        result.ThresholdCrossed.Should().Be(25);
    }

    [Test]
    public void AddCorruption_NoThresholdCrossing_WhenStayingInSameRange()
    {
        // Arrange — tracker at corruption 26, adding 5 → 31 (no threshold between 25 and 50)
        var tracker = CreateTestTracker(_characterId, 26);
        tracker.SetThresholdTriggers(true, false, false); // 25 already triggered
        SetupTracker(tracker);

        // Act
        var result = _service.AddCorruption(
            _characterId, 5, CorruptionSource.Consumable);

        // Assert
        result.ThresholdCrossed.Should().BeNull();
    }

    [Test]
    public void AddCorruption_DetectsStageCrossing()
    {
        // Arrange — tracker at corruption 18 (Uncorrupted), adding 5 → 23 (Tainted)
        var tracker = CreateTestTracker(_characterId, 18);
        SetupTracker(tracker);

        // Act
        var result = _service.AddCorruption(
            _characterId, 5, CorruptionSource.Environmental);

        // Assert
        result.StageCrossed.Should().BeTrue();
        result.PreviousStage.Should().Be(CorruptionStage.Uncorrupted);
        result.NewStage.Should().Be(CorruptionStage.Tainted);
    }

    [Test]
    public void AddCorruption_DetectsTerminalError_AtExactly100()
    {
        // Arrange — tracker at corruption 90, adding 10 → exactly 100
        var tracker = CreateTestTracker(_characterId, 90);
        SetupTracker(tracker);

        // Act
        var result = _service.AddCorruption(
            _characterId, 10, CorruptionSource.Ritual);

        // Assert
        result.IsTerminalError.Should().BeTrue();
        result.NewCorruption.Should().Be(100);
    }

    [Test]
    [TestCase(CorruptionSource.MysticMagic)]
    [TestCase(CorruptionSource.HereticalAbility)]
    [TestCase(CorruptionSource.Artifact)]
    [TestCase(CorruptionSource.Environmental)]
    [TestCase(CorruptionSource.Consumable)]
    [TestCase(CorruptionSource.Ritual)]
    [TestCase(CorruptionSource.ForlornContact)]
    [TestCase(CorruptionSource.BlightTransfer)]
    public void AddCorruption_RecordsCorrectSource(CorruptionSource source)
    {
        // Act
        var result = _service.AddCorruption(_characterId, 5, source);

        // Assert
        result.Source.Should().Be(source);
    }

    [Test]
    public void AddCorruption_PersistsChange_ToRepository()
    {
        // Act
        _service.AddCorruption(_characterId, 10, CorruptionSource.HereticalAbility);

        // Assert — verify repository update was called
        _mockCorruptionRepository.Verify(
            r => r.UpdateAsync(It.IsAny<CorruptionTracker>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void AddCorruption_CreatesTrackerIfNotExists()
    {
        // Arrange — no existing tracker
        var newCharacterId = _testPlayer.Id;
        SetupNoTracker(newCharacterId);

        // Player exists (to allow tracker creation)
        _mockPlayerRepository
            .Setup(r => r.GetByIdAsync(newCharacterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testPlayer);

        // Act
        var result = _service.AddCorruption(
            newCharacterId, 15, CorruptionSource.HereticalAbility);

        // Assert — tracker was created and add was called
        _mockCorruptionRepository.Verify(
            r => r.AddAsync(It.IsAny<CorruptionTracker>(), It.IsAny<CancellationToken>()),
            Times.Once);
        result.PreviousCorruption.Should().Be(0); // New tracker starts at 0
        result.NewCorruption.Should().Be(15);
    }

    [Test]
    public void AddCorruption_CharacterNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        SetupNoTracker(missingId);
        SetupNoPlayer(missingId);

        // Act
        var act = () => _service.AddCorruption(
            missingId, 10, CorruptionSource.Environmental);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    [Test]
    public void AddCorruption_DetectsFactionLock_WhenCrossing50()
    {
        // Arrange — tracker at corruption 45, adding 10 → 55 (crosses 50)
        var tracker = CreateTestTracker(_characterId, 45);
        tracker.SetThresholdTriggers(true, false, false); // 25 already triggered
        SetupTracker(tracker);

        // Act
        var result = _service.AddCorruption(
            _characterId, 10, CorruptionSource.HereticalAbility);

        // Assert
        result.NowFactionLocked.Should().BeTrue();
        result.ThresholdCrossed.Should().Be(50);
    }

    // ═══════════════════════════════════════════════════════════════
    // TRANSFER CORRUPTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void TransferCorruption_ValidTransfer_MovesCorruptionCorrectly()
    {
        // Arrange
        var sourceId = _characterId; // corruption 30
        var targetId = Guid.NewGuid();
        var targetPlayer = CreateTestPlayer(DefaultWill);

        var targetTracker = CreateTestTracker(targetId, 10);
        SetupTracker(targetTracker);
        SetupPlayer(targetPlayer, targetId);

        // Act
        var result = _service.TransferCorruption(sourceId, targetId, 20);

        // Assert
        result.Success.Should().BeTrue();
        result.AmountTransferred.Should().Be(20);
        result.SourceNewCorruption.Should().Be(10); // 30 - 20 = 10
        result.TargetNewCorruption.Should().Be(30); // 10 + 20 = 30
        result.TargetTerminalError.Should().BeFalse();
    }

    [Test]
    public void TransferCorruption_InsufficientCorruption_ReturnsFailed()
    {
        // Arrange — source has 30, requesting 50
        var targetId = Guid.NewGuid();
        var targetTracker = CreateTestTracker(targetId, 10);
        SetupTracker(targetTracker);
        SetupPlayer(CreateTestPlayer(DefaultWill), targetId);

        // Act
        var result = _service.TransferCorruption(_characterId, targetId, 50);

        // Assert
        result.Success.Should().BeFalse();
        result.AmountTransferred.Should().Be(0);
        result.SourceNewCorruption.Should().Be(30); // unchanged
        result.TargetNewCorruption.Should().Be(10); // unchanged
    }

    [Test]
    public void TransferCorruption_SelfTransfer_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.TransferCorruption(_characterId, _characterId, 10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("toCharacterId");
    }

    [Test]
    public void TransferCorruption_ZeroAmount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.TransferCorruption(
            _characterId, Guid.NewGuid(), 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void TransferCorruption_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.TransferCorruption(
            _characterId, Guid.NewGuid(), -5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void TransferCorruption_TargetReaches100_FlagsTerminalError()
    {
        // Arrange — source has 30, target has 85, transfer 20 → target = 105, clamped to 100
        var targetId = Guid.NewGuid();
        var targetTracker = CreateTestTracker(targetId, 85);
        SetupTracker(targetTracker);
        SetupPlayer(CreateTestPlayer(DefaultWill), targetId);

        // Act
        var result = _service.TransferCorruption(_characterId, targetId, 20);

        // Assert
        result.Success.Should().BeTrue();
        result.TargetTerminalError.Should().BeTrue();
        result.TargetNewCorruption.Should().Be(100);
        result.SourceNewCorruption.Should().Be(10); // 30 - 20
    }

    [Test]
    public void TransferCorruption_PersistsBothTrackers()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var targetTracker = CreateTestTracker(targetId, 10);
        SetupTracker(targetTracker);
        SetupPlayer(CreateTestPlayer(DefaultWill), targetId);

        // Act
        _service.TransferCorruption(_characterId, targetId, 10);

        // Assert — update called twice (once per tracker)
        _mockCorruptionRepository.Verify(
            r => r.UpdateAsync(It.IsAny<CorruptionTracker>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Test]
    public void TransferCorruption_UsesBlightTransferSource()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var targetTracker = CreateTestTracker(targetId, 10);
        SetupTracker(targetTracker);
        SetupPlayer(CreateTestPlayer(DefaultWill), targetId);

        // Act
        _service.TransferCorruption(_characterId, targetId, 5);

        // Assert — target's corruption was added via BlightTransfer source
        // The tracker received AddCorruption call with BlightTransfer source
        targetTracker.CurrentCorruption.Should().Be(15);
    }

    [Test]
    public void TransferCorruption_CharacterNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange — target does not exist
        var missingTargetId = Guid.NewGuid();
        SetupNoTracker(missingTargetId);
        SetupNoPlayer(missingTargetId);

        // Act
        var act = () => _service.TransferCorruption(
            _characterId, missingTargetId, 10);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // REMOVE CORRUPTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RemoveCorruption_ValidAmount_ReturnsTrue()
    {
        // Arrange — tracker at corruption 30

        // Act
        var result = _service.RemoveCorruption(
            _characterId, 10, "Divine Purification Ritual");

        // Assert
        result.Should().BeTrue();
        _testTracker.CurrentCorruption.Should().Be(20);
    }

    [Test]
    public void RemoveCorruption_ExceedsCurrentCorruption_ReturnsFalse()
    {
        // Arrange — tracker at corruption 30, requesting 50

        // Act
        var result = _service.RemoveCorruption(
            _characterId, 50, "Purification Attempt");

        // Assert
        result.Should().BeFalse();
        _testTracker.CurrentCorruption.Should().Be(30); // unchanged
    }

    [Test]
    public void RemoveCorruption_ZeroCorruption_ReturnsFalse()
    {
        // Arrange — tracker at corruption 0
        var tracker = CreateTestTracker(_characterId, 0);
        SetupTracker(tracker);

        // Act
        var result = _service.RemoveCorruption(
            _characterId, 5, "Purification Attempt");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void RemoveCorruption_ZeroAmount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.RemoveCorruption(
            _characterId, 0, "Some Reason");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RemoveCorruption_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.RemoveCorruption(
            _characterId, -5, "Some Reason");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RemoveCorruption_NullReason_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.RemoveCorruption(
            _characterId, 10, null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RemoveCorruption_EmptyReason_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.RemoveCorruption(
            _characterId, 10, "");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RemoveCorruption_WhitespaceReason_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.RemoveCorruption(
            _characterId, 10, "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RemoveCorruption_PersistsChange_ToRepository()
    {
        // Act
        _service.RemoveCorruption(_characterId, 5, "Pure Essence Artifact");

        // Assert
        _mockCorruptionRepository.Verify(
            r => r.UpdateAsync(It.IsAny<CorruptionTracker>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void RemoveCorruption_ExactAmountRemoved_ReturnsTrue()
    {
        // Arrange — tracker at corruption 30, removing exactly 30

        // Act
        var result = _service.RemoveCorruption(
            _characterId, 30, "Complete Divine Purification");

        // Assert
        result.Should().BeTrue();
        _testTracker.CurrentCorruption.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // PERFORM TERMINAL ERROR CHECK TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void PerformTerminalErrorCheck_PassCheck_SetsCorruptionTo99()
    {
        // Arrange — tracker at corruption 100
        var tracker = CreateTestTracker(_characterId, 100);
        SetupTracker(tracker);
        SetupDiceRoll(netSuccesses: 3); // Exactly meets DC 3

        // Act
        var result = _service.PerformTerminalErrorCheck(_characterId);

        // Assert
        result.Survived.Should().BeTrue();
        result.BecameForlorn.Should().BeFalse();
        result.FinalCorruption.Should().Be(99);
        tracker.CurrentCorruption.Should().Be(99);
    }

    [Test]
    public void PerformTerminalErrorCheck_FailCheck_CorruptionStaysAt100()
    {
        // Arrange — tracker at corruption 100
        var tracker = CreateTestTracker(_characterId, 100);
        SetupTracker(tracker);
        SetupDiceRoll(netSuccesses: 2); // Below DC 3

        // Act
        var result = _service.PerformTerminalErrorCheck(_characterId);

        // Assert
        result.Survived.Should().BeFalse();
        result.BecameForlorn.Should().BeTrue();
        result.FinalCorruption.Should().Be(100);
        tracker.CurrentCorruption.Should().Be(100); // Not changed on failure
    }

    [Test]
    public void PerformTerminalErrorCheck_NotAtTerminalError_ThrowsInvalidOperationException()
    {
        // Arrange — tracker at corruption 90 (not at Terminal Error)
        var tracker = CreateTestTracker(_characterId, 90);
        SetupTracker(tracker);

        // Act
        var act = () => _service.PerformTerminalErrorCheck(_characterId);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void PerformTerminalErrorCheck_UsesEffectiveWill_WithResolvePenalty()
    {
        // Arrange — WILL 4, corruption 100 → Resolve penalty = floor(100/20) = 5
        // effectiveWill = max(1, 4 - 5) = 1
        var tracker = CreateTestTracker(_characterId, 100);
        SetupTracker(tracker);
        SetupDiceRoll(netSuccesses: 0);

        // Act
        _service.PerformTerminalErrorCheck(_characterId);

        // Assert — verify dice service was called with 1d10 (min die)
        _mockDiceService.Verify(
            d => d.Roll(
                It.Is<DicePool>(p => p.Count == 1 && p.DiceType == DiceType.D10),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()),
            Times.Once);
    }

    [Test]
    public void PerformTerminalErrorCheck_MinimumOneDie_WhenResolvePenaltyExceedsWill()
    {
        // Arrange — WILL 3, corruption 100 → Resolve penalty = 5
        // effectiveWill = max(1, 3 - 5) = max(1, -2) = 1
        var lowWillPlayer = CreateTestPlayer(will: 3);
        var tracker = CreateTestTracker(lowWillPlayer.Id, 100);

        _mockCorruptionRepository
            .Setup(r => r.GetByCharacterIdAsync(lowWillPlayer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracker);
        _mockPlayerRepository
            .Setup(r => r.GetByIdAsync(lowWillPlayer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lowWillPlayer);

        SetupDiceRoll(netSuccesses: 0);

        // Act — create a new service instance with the new player
        _service.PerformTerminalErrorCheck(lowWillPlayer.Id);

        // Assert — verify at least 1 die was rolled
        _mockDiceService.Verify(
            d => d.Roll(
                It.Is<DicePool>(p => p.Count == 1),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()),
            Times.Once);
    }

    [Test]
    public void PerformTerminalErrorCheck_HighWillPlayer_RollsWithReducedPool()
    {
        // Arrange — WILL 8, corruption 100 → Resolve penalty = 5
        // effectiveWill = max(1, 8 - 5) = 3
        var highWillPlayer = CreateTestPlayer(will: 8);
        var tracker = CreateTestTracker(highWillPlayer.Id, 100);

        _mockCorruptionRepository
            .Setup(r => r.GetByCharacterIdAsync(highWillPlayer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracker);
        _mockPlayerRepository
            .Setup(r => r.GetByIdAsync(highWillPlayer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(highWillPlayer);

        SetupDiceRoll(netSuccesses: 3);

        // Act
        var result = _service.PerformTerminalErrorCheck(highWillPlayer.Id);

        // Assert
        result.Survived.Should().BeTrue();
        _mockDiceService.Verify(
            d => d.Roll(
                It.Is<DicePool>(p => p.Count == 3 && p.DiceType == DiceType.D10),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()),
            Times.Once);
    }

    [Test]
    public void PerformTerminalErrorCheck_CriticalSuccess_WhenSuccessesDoubleOrMoreDc()
    {
        // Arrange — 6 successes vs DC 3 → critical success (6 >= 3 * 2 = 6)
        var highWillPlayer = CreateTestPlayer(will: 12);
        var tracker = CreateTestTracker(highWillPlayer.Id, 100);

        _mockCorruptionRepository
            .Setup(r => r.GetByCharacterIdAsync(highWillPlayer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracker);
        _mockPlayerRepository
            .Setup(r => r.GetByIdAsync(highWillPlayer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(highWillPlayer);

        SetupDiceRoll(netSuccesses: 6);

        // Act
        var result = _service.PerformTerminalErrorCheck(highWillPlayer.Id);

        // Assert
        result.Survived.Should().BeTrue();
        result.WasCriticalSuccess.Should().BeTrue();
        result.SuccessesRolled.Should().Be(6);
    }

    [Test]
    public void PerformTerminalErrorCheck_ExactDcSuccesses_Survives()
    {
        // Arrange — exactly 3 successes vs DC 3
        var tracker = CreateTestTracker(_characterId, 100);
        SetupTracker(tracker);
        SetupDiceRoll(netSuccesses: 3);

        // Act
        var result = _service.PerformTerminalErrorCheck(_characterId);

        // Assert
        result.Survived.Should().BeTrue();
        result.SuccessesRolled.Should().Be(3);
        result.RequiredDc.Should().Be(3);
    }

    [Test]
    public void PerformTerminalErrorCheck_OneLessThanDc_Fails()
    {
        // Arrange — 2 successes vs DC 3
        var tracker = CreateTestTracker(_characterId, 100);
        SetupTracker(tracker);
        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.PerformTerminalErrorCheck(_characterId);

        // Assert
        result.Survived.Should().BeFalse();
        result.BecameForlorn.Should().BeTrue();
        result.SuccessesRolled.Should().Be(2);
    }

    [Test]
    public void PerformTerminalErrorCheck_Survived_PersistsTracker()
    {
        // Arrange
        var tracker = CreateTestTracker(_characterId, 100);
        SetupTracker(tracker);
        SetupDiceRoll(netSuccesses: 4);

        // Act
        _service.PerformTerminalErrorCheck(_characterId);

        // Assert — verify update called to persist corruption = 99
        _mockCorruptionRepository.Verify(
            r => r.UpdateAsync(It.IsAny<CorruptionTracker>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void PerformTerminalErrorCheck_Failed_DoesNotPersistTracker()
    {
        // Arrange
        var tracker = CreateTestTracker(_characterId, 100);
        SetupTracker(tracker);
        SetupDiceRoll(netSuccesses: 1);

        // Act
        _service.PerformTerminalErrorCheck(_characterId);

        // Assert — tracker NOT updated on failure (corruption stays at 100)
        _mockCorruptionRepository.Verify(
            r => r.UpdateAsync(It.IsAny<CorruptionTracker>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET OR CREATE TRACKER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetOrCreateTracker_ExistingTracker_ReturnsFromRepository()
    {
        // Arrange — default setup has existing tracker

        // Act — any query method triggers GetOrCreateTracker
        var state = _service.GetCorruptionState(_characterId);

        // Assert — no Add was called (tracker already exists)
        _mockCorruptionRepository.Verify(
            r => r.AddAsync(It.IsAny<CorruptionTracker>(), It.IsAny<CancellationToken>()),
            Times.Never);
        state.CurrentCorruption.Should().Be(DefaultCorruption);
    }

    [Test]
    public void GetOrCreateTracker_NoTracker_CreatesAndPersistsNew()
    {
        // Arrange — no existing tracker for this character
        var newId = _testPlayer.Id;
        SetupNoTracker(newId);

        // Act
        var state = _service.GetCorruptionState(newId);

        // Assert — a new tracker was created (Add called) and starts at 0
        _mockCorruptionRepository.Verify(
            r => r.AddAsync(
                It.Is<CorruptionTracker>(t => t.CharacterId == newId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        state.CurrentCorruption.Should().Be(0);
    }

    [Test]
    public void GetOrCreateTracker_NoCharacter_ThrowsCharacterNotFoundException()
    {
        // Arrange — neither tracker nor player exists
        var missingId = Guid.NewGuid();
        SetupNoTracker(missingId);
        SetupNoPlayer(missingId);

        // Act
        var act = () => _service.GetCorruptionState(missingId);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // INTEGRATION-STYLE TESTS (multi-step scenarios)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Scenario_AddCorruption_ThenTransfer_CorruptionUpdatesCorrectly()
    {
        // Arrange — source at 30, target at 10
        var targetId = Guid.NewGuid();
        var targetTracker = CreateTestTracker(targetId, 10);
        SetupTracker(targetTracker);
        SetupPlayer(CreateTestPlayer(DefaultWill), targetId);

        // Step 1: Add 20 corruption to source → 50
        var addResult = _service.AddCorruption(
            _characterId, 20, CorruptionSource.HereticalAbility);
        addResult.NewCorruption.Should().Be(50);

        // Step 2: Transfer 15 from source → target
        var transferResult = _service.TransferCorruption(
            _characterId, targetId, 15);

        // Assert
        transferResult.Success.Should().BeTrue();
        transferResult.SourceNewCorruption.Should().Be(35); // 50 - 15
        transferResult.TargetNewCorruption.Should().Be(25); // 10 + 15
    }

    [Test]
    public void Scenario_CorruptionReaches100_ThenTerminalErrorCheck_Survived()
    {
        // Arrange — tracker at 85
        var tracker = CreateTestTracker(_characterId, 85);
        SetupTracker(tracker);
        SetupDiceRoll(netSuccesses: 4);

        // Step 1: Add 15 → 100 (Terminal Error)
        var addResult = _service.AddCorruption(
            _characterId, 15, CorruptionSource.ForlornContact);
        addResult.IsTerminalError.Should().BeTrue();

        // Step 2: Perform Terminal Error check
        var terminalResult = _service.PerformTerminalErrorCheck(_characterId);

        // Assert
        terminalResult.Survived.Should().BeTrue();
        terminalResult.FinalCorruption.Should().Be(99);
        tracker.CurrentCorruption.Should().Be(99);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a test <see cref="Player"/> with specified WILL attribute.
    /// </summary>
    /// <param name="will">WILL attribute value for Terminal Error survival checks.</param>
    /// <returns>A new <see cref="Player"/> configured for testing.</returns>
    private static Player CreateTestPlayer(int will)
    {
        return PlayerBuilder.Create()
            .WithName("TestHero")
            .WithAttributes(
                might: 8,
                fortitude: 8,
                will: will,
                wits: 8,
                finesse: 8)
            .Build();
    }

    /// <summary>
    /// Creates a test <see cref="CorruptionTracker"/> with specified corruption value.
    /// </summary>
    /// <param name="characterId">The character ID for the tracker.</param>
    /// <param name="corruption">The initial corruption value (0-100).</param>
    /// <returns>A new <see cref="CorruptionTracker"/> configured for testing.</returns>
    private static CorruptionTracker CreateTestTracker(Guid characterId, int corruption)
    {
        var tracker = CorruptionTracker.Create(characterId);
        tracker.SetCorruption(corruption);
        return tracker;
    }

    /// <summary>
    /// Configures the mock corruption repository to return the specified tracker when queried.
    /// </summary>
    /// <param name="tracker">The tracker to return from repository lookups.</param>
    private void SetupTracker(CorruptionTracker tracker)
    {
        _mockCorruptionRepository
            .Setup(r => r.GetByCharacterIdAsync(tracker.CharacterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracker);
    }

    /// <summary>
    /// Configures the mock corruption repository to return null (no tracker found).
    /// </summary>
    /// <param name="characterId">The character ID to return null for.</param>
    private void SetupNoTracker(Guid characterId)
    {
        _mockCorruptionRepository
            .Setup(r => r.GetByCharacterIdAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CorruptionTracker?)null);
    }

    /// <summary>
    /// Configures the mock player repository to return the specified player for a given ID.
    /// </summary>
    /// <param name="player">The player to return.</param>
    /// <param name="characterId">The character ID to map to this player.</param>
    private void SetupPlayer(Player player, Guid characterId)
    {
        _mockPlayerRepository
            .Setup(r => r.GetByIdAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
    }

    /// <summary>
    /// Configures the mock player repository to return null (no player found).
    /// </summary>
    /// <param name="characterId">The character ID to return null for.</param>
    private void SetupNoPlayer(Guid characterId)
    {
        _mockPlayerRepository
            .Setup(r => r.GetByIdAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Player?)null);
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
