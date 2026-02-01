namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Unit tests for <see cref="CorruptionTracker"/> domain entity.
/// Tests cover factory creation, IEntity compliance, computed penalty properties,
/// threshold crossing logic, stage-based bonuses/penalties, faction lock,
/// Terminal Error detection, AddCorruption behavior, internal setters, and ToString.
/// </summary>
[TestFixture]
public class CorruptionTrackerTests
{
    // -------------------------------------------------------------------------
    // Constants & Helpers
    // -------------------------------------------------------------------------

    private static readonly Guid TestCharacterId = Guid.NewGuid();

    /// <summary>
    /// Creates a CorruptionTracker with the specified corruption level for testing.
    /// </summary>
    private static CorruptionTracker CreateTrackerAt(int corruption, Guid? characterId = null)
    {
        var tracker = CorruptionTracker.Create(characterId ?? TestCharacterId);
        tracker.SetCorruption(corruption);
        return tracker;
    }

    // -------------------------------------------------------------------------
    // Factory Methods — Create
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Create initializes all properties to their default values.
    /// </summary>
    [Test]
    public void Create_InitializesWithZeroCorruption()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var tracker = CorruptionTracker.Create(characterId);

        // Assert
        tracker.CharacterId.Should().Be(characterId);
        tracker.CurrentCorruption.Should().Be(0);
        tracker.Stage.Should().Be(CorruptionStage.Uncorrupted);
        tracker.Threshold25Triggered.Should().BeFalse();
        tracker.Threshold50Triggered.Should().BeFalse();
        tracker.Threshold75Triggered.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create generates a unique non-empty ID.
    /// </summary>
    [Test]
    public void Create_GeneratesUniqueId()
    {
        // Act
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Assert
        tracker.Id.Should().NotBeEmpty(
            because: "each tracker must have a unique identifier");
    }

    /// <summary>
    /// Verifies that two consecutive Create calls produce different IDs.
    /// </summary>
    [Test]
    public void Create_TwoConsecutiveCalls_GenerateDifferentIds()
    {
        // Act
        var tracker1 = CorruptionTracker.Create(Guid.NewGuid());
        var tracker2 = CorruptionTracker.Create(Guid.NewGuid());

        // Assert
        tracker1.Id.Should().NotBe(tracker2.Id);
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException for empty Guid.
    /// </summary>
    [Test]
    public void Create_WithEmptyGuid_ThrowsArgumentException()
    {
        // Act
        var act = () => CorruptionTracker.Create(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("characterId");
    }

    // -------------------------------------------------------------------------
    // IEntity Compliance
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that CorruptionTracker implements the IEntity interface.
    /// </summary>
    [Test]
    public void CorruptionTracker_ImplementsIEntity()
    {
        // Arrange & Act
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Assert
        tracker.Should().BeAssignableTo<IEntity>(
            because: "CorruptionTracker must implement IEntity for repository compatibility");
    }

    /// <summary>
    /// Verifies that the Id property is accessible via IEntity cast.
    /// </summary>
    [Test]
    public void Id_IsAccessibleViaIEntity()
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Act
        IEntity entity = tracker;

        // Assert
        entity.Id.Should().Be(tracker.Id);
        entity.Id.Should().NotBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Penalty Calculation Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that MaxHpPenaltyPercent, MaxApPenaltyPercent, and ResolveDicePenalty
    /// compute correctly using floor(Corruption/10)*5 and floor(Corruption/20) formulas.
    /// </summary>
    [Test]
    [TestCase(0, 0, 0, 0)]       // 0-9 range: 0%, 0%, 0 dice
    [TestCase(9, 0, 0, 0)]       // Upper boundary of 0-9
    [TestCase(10, 5, 5, 0)]      // 10-19 range: 5%, 5%, 0 dice
    [TestCase(19, 5, 5, 0)]      // Upper boundary of 10-19
    [TestCase(20, 10, 10, 1)]    // 20-29 range: 10%, 10%, 1 die
    [TestCase(25, 10, 10, 1)]    // Threshold25
    [TestCase(39, 15, 15, 1)]    // Upper boundary of 30-39
    [TestCase(40, 20, 20, 2)]    // 40-49 range
    [TestCase(45, 20, 20, 2)]    // Mid-range example from design spec
    [TestCase(50, 25, 25, 2)]    // Threshold50 (faction lock)
    [TestCase(60, 30, 30, 3)]    // 60-69 range
    [TestCase(75, 35, 35, 3)]    // Threshold75
    [TestCase(80, 40, 40, 4)]    // 80-89 range
    [TestCase(85, 40, 40, 4)]    // Mid-range
    [TestCase(99, 45, 45, 4)]    // Upper boundary before Terminal Error
    [TestCase(100, 50, 50, 5)]   // Terminal Error — maximum penalties
    public void PenaltyProperties_CalculateCorrectly(
        int corruption,
        int expectedHpPenalty,
        int expectedApPenalty,
        int expectedResolvePenalty)
    {
        // Arrange
        var tracker = CreateTrackerAt(corruption);

        // Assert
        tracker.MaxHpPenaltyPercent.Should().Be(expectedHpPenalty,
            because: $"HP penalty at corruption {corruption} should be floor({corruption}/10)*5 = {expectedHpPenalty}");
        tracker.MaxApPenaltyPercent.Should().Be(expectedApPenalty,
            because: $"AP penalty at corruption {corruption} should be floor({corruption}/10)*5 = {expectedApPenalty}");
        tracker.ResolveDicePenalty.Should().Be(expectedResolvePenalty,
            because: $"Resolve penalty at corruption {corruption} should be floor({corruption}/20) = {expectedResolvePenalty}");
    }

    // -------------------------------------------------------------------------
    // Stage Computation Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Stage is correctly computed from CurrentCorruption
    /// at all stage boundaries.
    /// </summary>
    [Test]
    [TestCase(0, CorruptionStage.Uncorrupted)]
    [TestCase(19, CorruptionStage.Uncorrupted)]
    [TestCase(20, CorruptionStage.Tainted)]
    [TestCase(39, CorruptionStage.Tainted)]
    [TestCase(40, CorruptionStage.Infected)]
    [TestCase(59, CorruptionStage.Infected)]
    [TestCase(60, CorruptionStage.Blighted)]
    [TestCase(79, CorruptionStage.Blighted)]
    [TestCase(80, CorruptionStage.Corrupted)]
    [TestCase(99, CorruptionStage.Corrupted)]
    [TestCase(100, CorruptionStage.Consumed)]
    public void Stage_ComputedCorrectlyAtBoundaries(int corruption, CorruptionStage expectedStage)
    {
        // Arrange
        var tracker = CreateTrackerAt(corruption);

        // Assert
        tracker.Stage.Should().Be(expectedStage);
    }

    // -------------------------------------------------------------------------
    // TechBonus & SocialPenalty Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that TechBonus returns the correct stage-based bonus.
    /// </summary>
    [Test]
    [TestCase(0, 0)]     // Uncorrupted: +0
    [TestCase(19, 0)]    // Uncorrupted upper boundary
    [TestCase(20, 1)]    // Tainted: +1
    [TestCase(39, 1)]    // Tainted upper boundary
    [TestCase(40, 2)]    // Infected: +2
    [TestCase(60, 2)]    // Blighted: +2
    [TestCase(80, 2)]    // Corrupted: +2
    [TestCase(100, 0)]   // Consumed: +0 (irrelevant)
    public void TechBonus_ReturnsCorrectStageBasedValue(int corruption, int expectedBonus)
    {
        // Arrange
        var tracker = CreateTrackerAt(corruption);

        // Assert
        tracker.TechBonus.Should().Be(expectedBonus);
    }

    /// <summary>
    /// Verifies that SocialPenalty returns the correct stage-based penalty.
    /// </summary>
    [Test]
    [TestCase(0, 0)]     // Uncorrupted: -0
    [TestCase(19, 0)]    // Uncorrupted upper boundary
    [TestCase(20, -1)]   // Tainted: -1
    [TestCase(39, -1)]   // Tainted upper boundary
    [TestCase(40, -2)]   // Infected: -2
    [TestCase(60, -2)]   // Blighted: -2
    [TestCase(80, -2)]   // Corrupted: -2
    [TestCase(100, 0)]   // Consumed: -0 (irrelevant)
    public void SocialPenalty_ReturnsCorrectStageBasedValue(int corruption, int expectedPenalty)
    {
        // Arrange
        var tracker = CreateTrackerAt(corruption);

        // Assert
        tracker.SocialPenalty.Should().Be(expectedPenalty);
    }

    // -------------------------------------------------------------------------
    // IsFactionLocked Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that IsFactionLocked is correct at the 50 threshold boundary.
    /// </summary>
    [Test]
    [TestCase(0, false)]
    [TestCase(49, false)]
    [TestCase(50, true)]
    [TestCase(75, true)]
    [TestCase(100, true)]
    public void IsFactionLocked_CorrectAtThreshold(int corruption, bool expectedLocked)
    {
        // Arrange
        var tracker = CreateTrackerAt(corruption);

        // Assert
        tracker.IsFactionLocked.Should().Be(expectedLocked,
            because: $"faction should{(expectedLocked ? "" : " not")} be locked at corruption {corruption}");
    }

    // -------------------------------------------------------------------------
    // IsTerminalError Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that IsTerminalError is true only at 100 corruption.
    /// </summary>
    [Test]
    [TestCase(0, false)]
    [TestCase(50, false)]
    [TestCase(99, false)]
    [TestCase(100, true)]
    public void IsTerminalError_CorrectAtBoundary(int corruption, bool expectedTerminal)
    {
        // Arrange
        var tracker = CreateTrackerAt(corruption);

        // Assert
        tracker.IsTerminalError.Should().Be(expectedTerminal);
    }

    // -------------------------------------------------------------------------
    // Threshold Crossing Tests — AddCorruption
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that crossing threshold 25 triggers exactly once.
    /// </summary>
    [Test]
    public void AddCorruption_CrossingThreshold25_TriggersOnce()
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Act — First crossing
        var result1 = tracker.AddCorruption(30, CorruptionSource.Environmental);

        // Assert — First crossing detected
        result1.ThresholdCrossed.Should().Be(25);
        tracker.Threshold25Triggered.Should().BeTrue();

        // Act — Reset and try again (threshold already triggered)
        tracker.SetCorruption(20);
        tracker.SetThresholdTriggers(true, false, false);
        var result2 = tracker.AddCorruption(10, CorruptionSource.Environmental);

        // Assert — Threshold not triggered again
        result2.ThresholdCrossed.Should().BeNull(
            because: "threshold 25 should only trigger once per character lifetime");
    }

    /// <summary>
    /// Verifies that crossing threshold 50 triggers exactly once.
    /// </summary>
    [Test]
    public void AddCorruption_CrossingThreshold50_TriggersOnce()
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());
        tracker.SetCorruption(45);
        tracker.SetThresholdTriggers(true, false, false); // 25 already triggered

        // Act
        var result = tracker.AddCorruption(10, CorruptionSource.MysticMagic);

        // Assert
        result.ThresholdCrossed.Should().Be(50);
        tracker.Threshold50Triggered.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that crossing threshold 75 triggers exactly once.
    /// </summary>
    [Test]
    public void AddCorruption_CrossingThreshold75_TriggersOnce()
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());
        tracker.SetCorruption(70);
        tracker.SetThresholdTriggers(true, true, false); // 25 and 50 already triggered

        // Act
        var result = tracker.AddCorruption(10, CorruptionSource.HereticalAbility);

        // Assert
        result.ThresholdCrossed.Should().Be(75);
        tracker.Threshold75Triggered.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that adding corruption that doesn't cross any threshold returns null.
    /// </summary>
    [Test]
    public void AddCorruption_NoCrossing_ThresholdCrossedIsNull()
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Act — add 10 corruption (stays below 25)
        var result = tracker.AddCorruption(10, CorruptionSource.MysticMagic);

        // Assert
        result.ThresholdCrossed.Should().BeNull();
    }

    /// <summary>
    /// Verifies that a large corruption addition that crosses multiple thresholds
    /// only reports the first one crossed (25 before 50 before 75).
    /// </summary>
    [Test]
    public void AddCorruption_CrossingMultipleThresholds_ReportsFirstOnly()
    {
        // Arrange — start at 0, add 80 (crosses 25, 50, 75 simultaneously)
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Act
        var result = tracker.AddCorruption(80, CorruptionSource.ForlornContact);

        // Assert — only the first uncrossed threshold is reported
        result.ThresholdCrossed.Should().Be(25,
            because: "when multiple thresholds are crossed, only the first is reported");
        tracker.Threshold25Triggered.Should().BeTrue();
        // 50 and 75 are NOT triggered in this single call due to else-if logic
        tracker.Threshold50Triggered.Should().BeFalse();
        tracker.Threshold75Triggered.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // AddCorruption — Stage Crossing Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that AddCorruption correctly detects stage crossings.
    /// </summary>
    [Test]
    public void AddCorruption_CrossingStage_SetsStageCrossedTrue()
    {
        // Arrange — at 15 (Uncorrupted), add 10 to reach 25 (Tainted)
        var tracker = CreateTrackerAt(15);

        // Act
        var result = tracker.AddCorruption(10, CorruptionSource.MysticMagic);

        // Assert
        result.StageCrossed.Should().BeTrue();
        result.PreviousStage.Should().Be(CorruptionStage.Uncorrupted);
        result.NewStage.Should().Be(CorruptionStage.Tainted);
    }

    /// <summary>
    /// Verifies that AddCorruption within the same stage sets StageCrossed to false.
    /// </summary>
    [Test]
    public void AddCorruption_WithinSameStage_SetsStageCrossedFalse()
    {
        // Arrange — at 25 (Tainted), add 5 to reach 30 (still Tainted)
        var tracker = CreateTrackerAt(25);
        tracker.SetThresholdTriggers(true, false, false);

        // Act
        var result = tracker.AddCorruption(5, CorruptionSource.MysticMagic);

        // Assert
        result.StageCrossed.Should().BeFalse();
        result.PreviousStage.Should().Be(CorruptionStage.Tainted);
        result.NewStage.Should().Be(CorruptionStage.Tainted);
    }

    // -------------------------------------------------------------------------
    // AddCorruption — Terminal Error Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that AddCorruption reaching 100 sets IsTerminalError.
    /// </summary>
    [Test]
    public void AddCorruption_Reaching100_SetsIsTerminalError()
    {
        // Arrange
        var tracker = CreateTrackerAt(95);
        tracker.SetThresholdTriggers(true, true, true);

        // Act
        var result = tracker.AddCorruption(5, CorruptionSource.HereticalAbility);

        // Assert
        result.IsTerminalError.Should().BeTrue();
        tracker.IsTerminalError.Should().BeTrue();
        tracker.CurrentCorruption.Should().Be(100);
    }

    /// <summary>
    /// Verifies that AddCorruption clamps at MaxCorruption (100).
    /// </summary>
    [Test]
    public void AddCorruption_ExceedingMax_ClampsTo100()
    {
        // Arrange
        var tracker = CreateTrackerAt(95);
        tracker.SetThresholdTriggers(true, true, true);

        // Act
        var result = tracker.AddCorruption(20, CorruptionSource.ForlornContact);

        // Assert
        tracker.CurrentCorruption.Should().Be(100);
        result.NewCorruption.Should().Be(100);
        result.AmountGained.Should().Be(5,
            because: "only 5 of the 20 added corruption was effective due to clamping");
    }

    // -------------------------------------------------------------------------
    // AddCorruption — Negative Amount (Rare Reduction)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that negative amounts reduce corruption, clamped at 0.
    /// </summary>
    [Test]
    public void AddCorruption_NegativeAmount_ReducesCorruption()
    {
        // Arrange
        var tracker = CreateTrackerAt(30);
        tracker.SetThresholdTriggers(true, false, false);

        // Act
        var result = tracker.AddCorruption(-10, CorruptionSource.Ritual);

        // Assert
        tracker.CurrentCorruption.Should().Be(20);
        result.NewCorruption.Should().Be(20);
        result.AmountGained.Should().Be(-10);
    }

    /// <summary>
    /// Verifies that negative amounts clamp at MinCorruption (0).
    /// </summary>
    [Test]
    public void AddCorruption_NegativeExceedingMin_ClampsToZero()
    {
        // Arrange
        var tracker = CreateTrackerAt(5);

        // Act
        var result = tracker.AddCorruption(-20, CorruptionSource.Ritual);

        // Assert
        tracker.CurrentCorruption.Should().Be(0);
        result.NewCorruption.Should().Be(0);
        result.AmountGained.Should().Be(-5);
    }

    // -------------------------------------------------------------------------
    // AddCorruption — Source Recording
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that all CorruptionSource values are correctly stored in the result.
    /// </summary>
    [Test]
    [TestCase(CorruptionSource.MysticMagic)]
    [TestCase(CorruptionSource.HereticalAbility)]
    [TestCase(CorruptionSource.Artifact)]
    [TestCase(CorruptionSource.Environmental)]
    [TestCase(CorruptionSource.Consumable)]
    [TestCase(CorruptionSource.Ritual)]
    [TestCase(CorruptionSource.ForlornContact)]
    [TestCase(CorruptionSource.BlightTransfer)]
    public void AddCorruption_AllSources_StoredCorrectly(CorruptionSource source)
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Act
        var result = tracker.AddCorruption(5, source);

        // Assert
        result.Source.Should().Be(source);
    }

    // -------------------------------------------------------------------------
    // SetCorruption — Internal Method
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that SetCorruption clamps values to valid range.
    /// </summary>
    [Test]
    [TestCase(-10, 0)]
    [TestCase(0, 0)]
    [TestCase(50, 50)]
    [TestCase(100, 100)]
    [TestCase(150, 100)]
    public void SetCorruption_ClampsToValidRange(int inputValue, int expectedValue)
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Act
        tracker.SetCorruption(inputValue);

        // Assert
        tracker.CurrentCorruption.Should().Be(expectedValue);
    }

    // -------------------------------------------------------------------------
    // SetThresholdTriggers — Internal Method
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that SetThresholdTriggers sets all three flags correctly.
    /// </summary>
    [Test]
    public void SetThresholdTriggers_SetsAllFlags()
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Act
        tracker.SetThresholdTriggers(true, true, false);

        // Assert
        tracker.Threshold25Triggered.Should().BeTrue();
        tracker.Threshold50Triggered.Should().BeTrue();
        tracker.Threshold75Triggered.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that ToString includes basic corruption info and penalties.
    /// </summary>
    [Test]
    public void ToString_AtMidCorruption_IncludesPenaltiesAndFactionLock()
    {
        // Arrange
        var tracker = CreateTrackerAt(55);

        // Act
        var display = tracker.ToString();

        // Assert
        display.Should().Contain("55/100");
        display.Should().Contain("[Infected]");
        display.Should().Contain("HP:-25%");
        display.Should().Contain("AP:-25%");
        display.Should().Contain("Resolve:-2");
        display.Should().Contain("[FACTION LOCKED]");
        display.Should().NotContain("[TERMINAL ERROR]");
    }

    /// <summary>
    /// Verifies that ToString includes Terminal Error indicator at corruption 100.
    /// </summary>
    [Test]
    public void ToString_AtTerminalError_IncludesTerminalErrorFlag()
    {
        // Arrange
        var tracker = CreateTrackerAt(100);

        // Act
        var display = tracker.ToString();

        // Assert
        display.Should().Contain("[TERMINAL ERROR]");
        display.Should().Contain("[FACTION LOCKED]");
        display.Should().Contain("[Consumed]");
    }

    /// <summary>
    /// Verifies that ToString at zero corruption shows no special flags.
    /// </summary>
    [Test]
    public void ToString_AtZeroCorruption_NoSpecialFlags()
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());

        // Act
        var display = tracker.ToString();

        // Assert
        display.Should().Contain("0/100");
        display.Should().Contain("[Uncorrupted]");
        display.Should().Contain("HP:-0%");
        display.Should().NotContain("[TERMINAL ERROR]");
        display.Should().NotContain("[FACTION LOCKED]");
    }
}
