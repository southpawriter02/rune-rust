namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="StressHistoryEntry"/> domain entity.
/// Tests cover both factory methods (FromApplicationResult and Create),
/// IEntity compliance, property mapping correctness, threshold crossing recording,
/// resistance check recording, all stress source values, edge cases, and ToString formatting.
/// </summary>
[TestFixture]
public class StressHistoryEntryTests
{
    // -------------------------------------------------------------------------
    // Constants & Helpers
    // -------------------------------------------------------------------------

    private static readonly Guid TestCharacterId = Guid.NewGuid();

    /// <summary>
    /// Creates a StressApplicationResult with no resistance for testing.
    /// </summary>
    private static StressApplicationResult CreateBasicResult(
        int previousStress = 35,
        int newStress = 55,
        StressSource source = StressSource.Combat) =>
        StressApplicationResult.Create(previousStress, newStress, source);

    /// <summary>
    /// Creates a StressApplicationResult with a resistance check for testing.
    /// </summary>
    private static StressApplicationResult CreateResultWithResistance(
        int previousStress = 35,
        int newStress = 45,
        StressSource source = StressSource.Combat,
        int successes = 1,
        int baseStress = 20) =>
        StressApplicationResult.Create(
            previousStress,
            newStress,
            source,
            StressCheckResult.Create(successes, baseStress));

    // -------------------------------------------------------------------------
    // Factory Methods — FromApplicationResult (No Resistance)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FromApplicationResult correctly maps all properties when no
    /// resistance check was performed.
    /// </summary>
    [Test]
    public void FromApplicationResult_WithNoResistance_SetsCorrectValues()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var result = CreateBasicResult(previousStress: 35, newStress: 55, source: StressSource.Combat);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(characterId, result);

        // Assert
        entry.CharacterId.Should().Be(characterId);
        entry.Amount.Should().Be(20, because: "with no resistance, Amount equals StressGained");
        entry.Source.Should().Be(StressSource.Combat);
        entry.ResistDc.Should().BeNull(because: "no resistance check was performed");
        entry.Resisted.Should().BeFalse(because: "no resistance result means no resistance");
        entry.FinalAmount.Should().Be(20, because: "FinalAmount is the actual stress gained");
        entry.PreviousStress.Should().Be(35);
        entry.NewStress.Should().Be(55);
    }

    /// <summary>
    /// Verifies that FromApplicationResult generates a unique non-empty ID.
    /// </summary>
    [Test]
    public void FromApplicationResult_GeneratesUniqueId()
    {
        // Arrange
        var result = CreateBasicResult();

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry.Id.Should().NotBeEmpty(because: "each history entry must have a unique identifier");
    }

    /// <summary>
    /// Verifies that two consecutive FromApplicationResult calls produce different IDs.
    /// </summary>
    [Test]
    public void FromApplicationResult_TwoConsecutiveCalls_GenerateDifferentIds()
    {
        // Arrange
        var result = CreateBasicResult();

        // Act
        var entry1 = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);
        var entry2 = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry1.Id.Should().NotBe(entry2.Id, because: "each history entry should have a unique ID");
    }

    /// <summary>
    /// Verifies that FromApplicationResult sets CreatedAt to approximately DateTime.UtcNow.
    /// </summary>
    [Test]
    public void FromApplicationResult_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var result = CreateBasicResult();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        entry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    // -------------------------------------------------------------------------
    // Factory Methods — FromApplicationResult (With Resistance)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FromApplicationResult correctly maps resistance check data
    /// when a successful resistance check was performed (reduction > 0%).
    /// </summary>
    [Test]
    public void FromApplicationResult_WithSuccessfulResistance_SetsResistedTrue()
    {
        // Arrange — 1 success = 50% reduction
        var characterId = Guid.NewGuid();
        var result = CreateResultWithResistance(
            previousStress: 35,
            newStress: 45,
            successes: 1,
            baseStress: 20);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(characterId, result);

        // Assert
        entry.Resisted.Should().BeTrue(because: "1 success gives 50% reduction, which is > 0");
        entry.ResistDc.Should().Be(1, because: "ResistDc stores the number of successes rolled");
        entry.Amount.Should().Be(20, because: "Amount is the base stress before resistance");
    }

    /// <summary>
    /// Verifies that FromApplicationResult correctly handles a failed resistance check
    /// (0 successes = 0% reduction).
    /// </summary>
    [Test]
    public void FromApplicationResult_WithFailedResistance_SetsResistedFalse()
    {
        // Arrange — 0 successes = 0% reduction
        var characterId = Guid.NewGuid();
        var result = CreateResultWithResistance(
            previousStress: 35,
            newStress: 55,
            successes: 0,
            baseStress: 20);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(characterId, result);

        // Assert
        entry.Resisted.Should().BeFalse(because: "0 successes means 0% reduction, not resisted");
        entry.ResistDc.Should().Be(0, because: "ResistDc stores the number of successes (0)");
        entry.Amount.Should().Be(20, because: "Amount is the base stress before resistance");
    }

    /// <summary>
    /// Verifies that FromApplicationResult correctly handles full resistance
    /// (4+ successes = 100% reduction).
    /// </summary>
    [Test]
    public void FromApplicationResult_WithFullResistance_SetsResistedTrue()
    {
        // Arrange — 4 successes = 100% reduction, finalStress = 0
        var checkResult = StressCheckResult.Create(successes: 4, baseStress: 20);
        var result = StressApplicationResult.Create(
            previousStress: 35,
            newStress: 35,
            source: StressSource.Combat,
            resistanceResult: checkResult);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry.Resisted.Should().BeTrue(because: "4 successes gives 100% reduction");
        entry.FinalAmount.Should().Be(0, because: "100% resistance means no stress gained");
    }

    // -------------------------------------------------------------------------
    // Factory Methods — FromApplicationResult (Threshold Crossing)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FromApplicationResult records the new threshold when a
    /// threshold boundary was crossed.
    /// </summary>
    [Test]
    public void FromApplicationResult_WithThresholdCrossed_RecordsNewThreshold()
    {
        // Arrange — crossing from Uneasy (20-39) to Panicked (60-79)
        var result = StressApplicationResult.Create(
            previousStress: 35,
            newStress: 65,
            source: StressSource.Combat);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry.ThresholdCrossed.Should().Be(StressThreshold.Panicked,
            because: "the character crossed into the Panicked threshold");
    }

    /// <summary>
    /// Verifies that FromApplicationResult sets ThresholdCrossed to null when
    /// no threshold boundary was crossed.
    /// </summary>
    [Test]
    public void FromApplicationResult_WithNoThresholdCrossed_SetsThresholdNull()
    {
        // Arrange — staying within Uneasy (20-39)
        var result = StressApplicationResult.Create(
            previousStress: 25,
            newStress: 35,
            source: StressSource.Combat);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry.ThresholdCrossed.Should().BeNull(
            because: "the character remained in the same threshold tier");
    }

    // -------------------------------------------------------------------------
    // Factory Methods — FromApplicationResult (All StressSource Values)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FromApplicationResult correctly stores each StressSource enum value.
    /// </summary>
    [Test]
    [TestCase(StressSource.Combat)]
    [TestCase(StressSource.Exploration)]
    [TestCase(StressSource.Narrative)]
    [TestCase(StressSource.Heretical)]
    [TestCase(StressSource.Environmental)]
    [TestCase(StressSource.Corruption)]
    public void FromApplicationResult_AllStressSources_StoredCorrectly(StressSource source)
    {
        // Arrange
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 50,
            source: source);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry.Source.Should().Be(source);
    }

    // -------------------------------------------------------------------------
    // Factory Methods — Create (Manual Construction)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Create sets all properties correctly from explicit parameters.
    /// </summary>
    [Test]
    public void Create_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var entry = StressHistoryEntry.Create(
            characterId,
            amount: 25,
            source: StressSource.Heretical,
            resistDc: 3,
            resisted: true,
            finalAmount: 6,
            previousStress: 75,
            newStress: 81);

        // Assert
        entry.CharacterId.Should().Be(characterId);
        entry.Amount.Should().Be(25);
        entry.Source.Should().Be(StressSource.Heretical);
        entry.ResistDc.Should().Be(3);
        entry.Resisted.Should().BeTrue();
        entry.FinalAmount.Should().Be(6);
        entry.PreviousStress.Should().Be(75);
        entry.NewStress.Should().Be(81);
    }

    /// <summary>
    /// Verifies that Create records the threshold when specified.
    /// </summary>
    [Test]
    public void Create_WithThresholdCrossed_RecordsThreshold()
    {
        // Arrange & Act
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 25,
            source: StressSource.Heretical,
            resistDc: 3,
            resisted: true,
            finalAmount: 6,
            previousStress: 75,
            newStress: 81,
            thresholdCrossed: StressThreshold.Breaking);

        // Assert
        entry.ThresholdCrossed.Should().Be(StressThreshold.Breaking);
    }

    /// <summary>
    /// Verifies that Create defaults ThresholdCrossed to null when not specified.
    /// </summary>
    [Test]
    public void Create_WithoutThresholdCrossed_DefaultsToNull()
    {
        // Arrange & Act
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 10,
            source: StressSource.Environmental,
            resistDc: null,
            resisted: false,
            finalAmount: 10,
            previousStress: 30,
            newStress: 40);

        // Assert
        entry.ThresholdCrossed.Should().BeNull(
            because: "the default value for thresholdCrossed parameter is null");
    }

    /// <summary>
    /// Verifies that Create generates a unique non-empty ID.
    /// </summary>
    [Test]
    public void Create_GeneratesUniqueId()
    {
        // Arrange & Act
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 10,
            source: StressSource.Combat,
            resistDc: null,
            resisted: false,
            finalAmount: 10,
            previousStress: 0,
            newStress: 10);

        // Assert
        entry.Id.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that Create sets CreatedAt to approximately DateTime.UtcNow.
    /// </summary>
    [Test]
    public void Create_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 10,
            source: StressSource.Combat,
            resistDc: null,
            resisted: false,
            finalAmount: 10,
            previousStress: 0,
            newStress: 10);

        // Assert
        entry.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        entry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that two consecutive Create calls produce different IDs.
    /// </summary>
    [Test]
    public void Create_TwoConsecutiveCalls_GenerateDifferentIds()
    {
        // Act
        var entry1 = StressHistoryEntry.Create(
            TestCharacterId, 10, StressSource.Combat, null, false, 10, 0, 10);
        var entry2 = StressHistoryEntry.Create(
            TestCharacterId, 10, StressSource.Combat, null, false, 10, 0, 10);

        // Assert
        entry1.Id.Should().NotBe(entry2.Id);
    }

    // -------------------------------------------------------------------------
    // IEntity Compliance
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that StressHistoryEntry implements the IEntity interface.
    /// </summary>
    [Test]
    public void StressHistoryEntry_ImplementsIEntity()
    {
        // Arrange & Act
        var entry = StressHistoryEntry.Create(
            TestCharacterId, 10, StressSource.Combat, null, false, 10, 0, 10);

        // Assert
        entry.Should().BeAssignableTo<IEntity>(
            because: "StressHistoryEntry must implement IEntity for repository compatibility");
    }

    /// <summary>
    /// Verifies that the Id property is accessible when cast to IEntity.
    /// </summary>
    [Test]
    public void Id_IsAccessibleViaIEntity()
    {
        // Arrange
        var entry = StressHistoryEntry.Create(
            TestCharacterId, 10, StressSource.Combat, null, false, 10, 0, 10);

        // Act
        IEntity entity = entry;

        // Assert
        entity.Id.Should().Be(entry.Id);
        entity.Id.Should().NotBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Edge Cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FromApplicationResult handles zero stress gained (stress unchanged).
    /// </summary>
    [Test]
    public void FromApplicationResult_ZeroStressGained_SetsAmountCorrectly()
    {
        // Arrange — stress 50 → 50 (no change, e.g., fully resisted)
        var result = StressApplicationResult.Create(
            previousStress: 50,
            newStress: 50,
            source: StressSource.Combat);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry.Amount.Should().Be(0, because: "StressGained is 0 when stress doesn't change");
        entry.FinalAmount.Should().Be(0);
        entry.PreviousStress.Should().Be(50);
        entry.NewStress.Should().Be(50);
    }

    /// <summary>
    /// Verifies that FromApplicationResult correctly handles stress reaching maximum (100),
    /// which triggers a Trauma Check and crosses into the Trauma threshold.
    /// </summary>
    [Test]
    public void FromApplicationResult_MaxStress_TraumaThreshold()
    {
        // Arrange — stress reaches 100 (Trauma threshold)
        var result = StressApplicationResult.Create(
            previousStress: 80,
            newStress: 100,
            source: StressSource.Heretical);

        // Act
        var entry = StressHistoryEntry.FromApplicationResult(TestCharacterId, result);

        // Assert
        entry.NewStress.Should().Be(100);
        entry.ThresholdCrossed.Should().Be(StressThreshold.Trauma,
            because: "crossing from Breaking (80-99) to Trauma (100) should be recorded");
    }

    /// <summary>
    /// Verifies that Create handles zero amounts without throwing exceptions.
    /// </summary>
    [Test]
    public void Create_WithZeroAmounts_NoException()
    {
        // Act
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 0,
            source: StressSource.Combat,
            resistDc: null,
            resisted: false,
            finalAmount: 0,
            previousStress: 0,
            newStress: 0);

        // Assert
        entry.Amount.Should().Be(0);
        entry.FinalAmount.Should().Be(0);
        entry.PreviousStress.Should().Be(0);
        entry.NewStress.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Create handles null ResistDc with no resistance.
    /// </summary>
    [Test]
    public void Create_WithNullResistDc_SetsNull()
    {
        // Act
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 15,
            source: StressSource.Exploration,
            resistDc: null,
            resisted: false,
            finalAmount: 15,
            previousStress: 20,
            newStress: 35);

        // Assert
        entry.ResistDc.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that ToString produces a readable format with source and stress transition.
    /// </summary>
    [Test]
    public void ToString_WithNoThreshold_FormatsCorrectly()
    {
        // Arrange
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 20,
            source: StressSource.Combat,
            resistDc: null,
            resisted: false,
            finalAmount: 20,
            previousStress: 25,
            newStress: 35);

        // Act
        var display = entry.ToString();

        // Assert
        display.Should().Contain("Combat");
        display.Should().Contain("20→20");
        display.Should().Contain("25→35");
        display.Should().NotContain("[Threshold:");
        display.Should().NotContain("[Resisted]");
    }

    /// <summary>
    /// Verifies that ToString includes threshold information when a threshold was crossed.
    /// </summary>
    [Test]
    public void ToString_WithThreshold_IncludesThresholdInfo()
    {
        // Arrange
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 25,
            source: StressSource.Heretical,
            resistDc: null,
            resisted: false,
            finalAmount: 25,
            previousStress: 35,
            newStress: 60,
            thresholdCrossed: StressThreshold.Panicked);

        // Act
        var display = entry.ToString();

        // Assert
        display.Should().Contain("[Threshold: Panicked]");
    }

    /// <summary>
    /// Verifies that ToString includes resisted indicator when stress was resisted.
    /// </summary>
    [Test]
    public void ToString_WithResistance_IncludesResistedIndicator()
    {
        // Arrange
        var entry = StressHistoryEntry.Create(
            TestCharacterId,
            amount: 20,
            source: StressSource.Combat,
            resistDc: 2,
            resisted: true,
            finalAmount: 5,
            previousStress: 30,
            newStress: 35);

        // Act
        var display = entry.ToString();

        // Assert
        display.Should().Contain("[Resisted]");
        display.Should().Contain("20→5");
    }
}
