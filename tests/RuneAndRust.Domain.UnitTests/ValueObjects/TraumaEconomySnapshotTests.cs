namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TraumaEconomySnapshot"/> value object.
/// Verifies factory methods, validation, and snapshot capture behavior.
/// </summary>
[TestFixture]
public class TraumaEconomySnapshotTests
{
    // -------------------------------------------------------------------------
    // Factory Method — Create
    // -------------------------------------------------------------------------

    [Test]
    public void Create_FromState_CapturesAllValues()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var trauma = CharacterTrauma.Create(
            characterId,
            "survivors-guilt",
            "AllyDeath",
            DateTime.UtcNow);
        var traumas = new List<CharacterTrauma> { trauma }.AsReadOnly();

        var state = TraumaEconomyState.Create(
            characterId,
            StressState.Create(65),
            CorruptionState.Create(40),
            CpsState.Create(65),
            traumas);

        // Act
        var snapshot = TraumaEconomySnapshot.Create(state);

        // Assert
        snapshot.CharacterId.Should().Be(characterId);
        snapshot.Stress.Should().Be(65);
        snapshot.StressThreshold.Should().Be(StressThreshold.Panicked);
        snapshot.Corruption.Should().Be(40);
        snapshot.CorruptionStage.Should().Be(CorruptionStage.Infected);
        snapshot.CpsStage.Should().Be(CpsStage.RuinMadness);
        snapshot.TraumaCount.Should().Be(1);
        snapshot.TraumaIds.Should().Contain("survivors-guilt");
        snapshot.WarningLevel.Should().Be(WarningLevel.Critical); // CPS requires panic check
    }

    [Test]
    public void Create_WithExplicitTimestamp_UsesProvidedTime()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var state = TraumaEconomyState.Create(
            characterId,
            StressState.Calm,
            CorruptionState.Uncorrupted,
            CpsState.Create(0),
            traumas: null);

        var customTime = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var snapshot = TraumaEconomySnapshot.Create(state, customTime);

        // Assert
        snapshot.CapturedAt.Should().Be(customTime);
    }

    [Test]
    public void Create_WithNullState_ThrowsArgumentNullException()
    {
        // Act
        var act = () => TraumaEconomySnapshot.Create(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // Empty Snapshot
    // -------------------------------------------------------------------------

    [Test]
    public void Empty_HasDefaultValues()
    {
        // Act
        var empty = TraumaEconomySnapshot.Empty;

        // Assert
        empty.CharacterId.Should().Be(Guid.Empty);
        empty.CapturedAt.Should().Be(DateTime.MinValue);
        empty.Stress.Should().Be(0);
        empty.StressThreshold.Should().Be(StressThreshold.Calm);
        empty.Corruption.Should().Be(0);
        empty.CorruptionStage.Should().Be(CorruptionStage.Uncorrupted);
        empty.CpsStage.Should().Be(CpsStage.None);
        empty.TraumaCount.Should().Be(0);
        empty.TraumaIds.Should().BeEmpty();
        empty.WarningLevel.Should().Be(WarningLevel.None);
        empty.WasCritical.Should().BeFalse();
        empty.WasTerminal.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

    [Test]
    public void IsValid_WithValidSnapshot_ReturnsTrue()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(50),
            CorruptionState.Create(30),
            CpsState.Create(50),
            traumas: null);

        var snapshot = TraumaEconomySnapshot.Create(state);

        // Assert
        snapshot.IsValid().Should().BeTrue();
    }

    [Test]
    public void IsValid_WithEmptySnapshot_ReturnsTrue()
    {
        // Act
        var empty = TraumaEconomySnapshot.Empty;

        // Assert
        empty.IsValid().Should().BeTrue();
    }

    [Test]
    public void IsValid_WithMismatchedTraumaCount_ReturnsFalse()
    {
        // Arrange - Create a snapshot and manually modify it via record with
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Calm,
            CorruptionState.Uncorrupted,
            CpsState.Create(0),
            traumas: null);

        var validSnapshot = TraumaEconomySnapshot.Create(state);

        // Create an invalid snapshot with mismatched count via record with
        var invalidSnapshot = validSnapshot with { TraumaCount = 5 };

        // Assert
        invalidSnapshot.IsValid().Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Derived Properties
    // -------------------------------------------------------------------------

    [Test]
    public void WasCritical_WhenStateIsCritical_CapturesCorrectly()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(85), // Critical
            CorruptionState.Uncorrupted,
            CpsState.Create(85),
            traumas: null);

        // Act
        var snapshot = TraumaEconomySnapshot.Create(state);

        // Assert
        snapshot.WasCritical.Should().BeTrue();
        snapshot.WasTerminal.Should().BeFalse();
    }

    [Test]
    public void WasTerminal_WhenStateIsTerminal_CapturesCorrectly()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(100), // Terminal
            CorruptionState.Uncorrupted,
            CpsState.Create(100),
            traumas: null);

        // Act
        var snapshot = TraumaEconomySnapshot.Create(state);

        // Assert
        snapshot.WasTerminal.Should().BeTrue();
        snapshot.WasCritical.Should().BeTrue(); // Terminal is also critical
    }

    // -------------------------------------------------------------------------
    // Record Equality
    // -------------------------------------------------------------------------

    [Test]
    public void Snapshots_WithSameValues_AreEqual()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var capturedAt = DateTime.UtcNow;

        var state = TraumaEconomyState.Create(
            characterId,
            StressState.Create(50),
            CorruptionState.Create(30),
            CpsState.Create(50),
            traumas: null);

        var snapshot1 = TraumaEconomySnapshot.Create(state, capturedAt);
        var snapshot2 = TraumaEconomySnapshot.Create(state, capturedAt);

        // Assert - Use BeEquivalentTo for deep value comparison
        snapshot1.Should().BeEquivalentTo(snapshot2);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(45),
            CorruptionState.Create(30),
            CpsState.Create(45),
            traumas: null);

        var snapshot = TraumaEconomySnapshot.Create(state);

        // Act
        var result = snapshot.ToString();

        // Assert
        result.Should().Contain("Stress=45");
        result.Should().Contain("Corruption=30");
        result.Should().Contain("CPS=GlimmerMadness");
        result.Should().Contain("Traumas=0");
    }
}
