namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Unit tests for <see cref="StressThreshold"/> enum and
/// <see cref="StressThresholdExtensions"/> extension methods.
/// </summary>
[TestFixture]
public class StressThresholdTests
{
    // -------------------------------------------------------------------------
    // FromStressValue — Boundary Tests
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, StressThreshold.Calm)]
    [TestCase(19, StressThreshold.Calm)]
    [TestCase(20, StressThreshold.Uneasy)]
    [TestCase(39, StressThreshold.Uneasy)]
    [TestCase(40, StressThreshold.Anxious)]
    [TestCase(59, StressThreshold.Anxious)]
    [TestCase(60, StressThreshold.Panicked)]
    [TestCase(79, StressThreshold.Panicked)]
    [TestCase(80, StressThreshold.Breaking)]
    [TestCase(99, StressThreshold.Breaking)]
    [TestCase(100, StressThreshold.Trauma)]
    public void FromStressValue_ReturnsCorrectThreshold(int stress, StressThreshold expected)
    {
        // Act
        var result = StressThresholdExtensions.FromStressValue(stress);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void FromStressValue_ThrowsForNegativeStress()
    {
        // Act
        var act = () => StressThresholdExtensions.FromStressValue(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void FromStressValue_ThrowsForStressAbove100()
    {
        // Act
        var act = () => StressThresholdExtensions.FromStressValue(101);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // -------------------------------------------------------------------------
    // GetMinStress — Range Lower Bounds
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(StressThreshold.Calm, 0)]
    [TestCase(StressThreshold.Uneasy, 20)]
    [TestCase(StressThreshold.Anxious, 40)]
    [TestCase(StressThreshold.Panicked, 60)]
    [TestCase(StressThreshold.Breaking, 80)]
    [TestCase(StressThreshold.Trauma, 100)]
    public void GetMinStress_ReturnsCorrectValues(StressThreshold threshold, int expectedMin)
    {
        // Act
        var result = threshold.GetMinStress();

        // Assert
        result.Should().Be(expectedMin);
    }

    // -------------------------------------------------------------------------
    // GetMaxStress — Range Upper Bounds
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(StressThreshold.Calm, 19)]
    [TestCase(StressThreshold.Uneasy, 39)]
    [TestCase(StressThreshold.Anxious, 59)]
    [TestCase(StressThreshold.Panicked, 79)]
    [TestCase(StressThreshold.Breaking, 99)]
    [TestCase(StressThreshold.Trauma, 100)]
    public void GetMaxStress_ReturnsCorrectValues(StressThreshold threshold, int expectedMax)
    {
        // Act
        var result = threshold.GetMaxStress();

        // Assert
        result.Should().Be(expectedMax);
    }

    // -------------------------------------------------------------------------
    // GetDefensePenalty — Penalty Matches Ordinal
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(StressThreshold.Calm, 0)]
    [TestCase(StressThreshold.Uneasy, 1)]
    [TestCase(StressThreshold.Anxious, 2)]
    [TestCase(StressThreshold.Panicked, 3)]
    [TestCase(StressThreshold.Breaking, 4)]
    [TestCase(StressThreshold.Trauma, 5)]
    public void GetDefensePenalty_MatchesThresholdOrdinal(StressThreshold threshold, int expectedPenalty)
    {
        // Act
        var result = threshold.GetDefensePenalty();

        // Assert
        result.Should().Be(expectedPenalty);
    }

    // -------------------------------------------------------------------------
    // ToDisplayString — Formatting
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(StressThreshold.Calm, "Calm (0-19)")]
    [TestCase(StressThreshold.Anxious, "Anxious (40-59)")]
    [TestCase(StressThreshold.Trauma, "Trauma (100-100)")]
    public void ToDisplayString_FormatsCorrectly(StressThreshold threshold, string expected)
    {
        // Act
        var result = threshold.ToDisplayString();

        // Assert
        result.Should().Be(expected);
    }
}
