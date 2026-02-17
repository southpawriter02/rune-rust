using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="WeaveDisruptionResult"/>.
/// Covers dispel roll tracking, Resonance bonus, Corruption state,
/// AP cost/Cascade tracking, and display method output.
/// </summary>
[TestFixture]
public class WeaveDisruptionResultTests
{
    // ===== Factory / Init Tests =====

    [Test]
    public void Create_WithDefaults_HasZeroValues()
    {
        // Act
        var result = new WeaveDisruptionResult();

        // Assert
        result.DispelRoll.Should().Be(0);
        result.ResonanceBonus.Should().Be(0);
        result.TotalRoll.Should().Be(0);
        result.ResonanceBefore.Should().Be(0);
        result.ResonanceAfter.Should().Be(0);
        result.ResonanceGained.Should().Be(0);
        result.CorruptionCheckPerformed.Should().BeFalse();
        result.CorruptionTriggered.Should().BeFalse();
        result.ApCostPaid.Should().Be(0);
        result.CascadeApplied.Should().BeFalse();
    }

    [Test]
    public void Create_WithDispelRoll_TracksTotalCorrectly()
    {
        // Act
        var result = new WeaveDisruptionResult
        {
            DispelRoll = 14,
            ResonanceBonus = 6,
            TotalRoll = 20
        };

        // Assert
        result.DispelRoll.Should().Be(14);
        result.ResonanceBonus.Should().Be(6);
        result.TotalRoll.Should().Be(20);
    }

    [Test]
    public void Create_WithResonanceChange_TracksBeforeAndAfter()
    {
        // Act
        var result = new WeaveDisruptionResult
        {
            ResonanceBefore = 5,
            ResonanceAfter = 6,
            ResonanceGained = 1
        };

        // Assert
        result.ResonanceBefore.Should().Be(5);
        result.ResonanceAfter.Should().Be(6);
        result.ResonanceGained.Should().Be(1);
    }

    [Test]
    public void Create_WithCorruptionTriggered_TracksAllFields()
    {
        // Act
        var result = new WeaveDisruptionResult
        {
            CorruptionCheckPerformed = true,
            CorruptionTriggered = true,
            CorruptionReason = "Corruption at Resonance 7",
            CorruptionRoll = 4,
            CorruptionRiskPercent = 5
        };

        // Assert
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeTrue();
        result.CorruptionRoll.Should().Be(4);
        result.CorruptionRiskPercent.Should().Be(5);
    }

    [Test]
    public void Create_WithCascadeApplied_TracksReducedCost()
    {
        // Act
        var result = new WeaveDisruptionResult
        {
            ApCostPaid = 2,
            CascadeApplied = true
        };

        // Assert
        result.ApCostPaid.Should().Be(2);
        result.CascadeApplied.Should().BeTrue();
    }

    [Test]
    public void Create_WithoutCascade_TracksBaseCost()
    {
        // Act
        var result = new WeaveDisruptionResult
        {
            ApCostPaid = 3,
            CascadeApplied = false
        };

        // Assert
        result.ApCostPaid.Should().Be(3);
        result.CascadeApplied.Should().BeFalse();
    }

    // ===== Display Method Tests =====

    [Test]
    public void GetDispelBreakdown_ReturnsFormattedString()
    {
        // Arrange
        var result = new WeaveDisruptionResult
        {
            DispelRoll = 14,
            ResonanceBonus = 6,
            TotalRoll = 20
        };

        // Act
        var breakdown = result.GetDispelBreakdown();

        // Assert
        breakdown.Should().Be("Weave Disruption: d20 = 14 + Resonance 6 = 20 (total)");
    }

    [Test]
    public void GetDescription_WithCascadeAndCorruption_IncludesBothTags()
    {
        // Arrange
        var result = new WeaveDisruptionResult
        {
            DispelRoll = 14,
            ResonanceBonus = 6,
            TotalRoll = 20,
            ResonanceBefore = 6,
            ResonanceAfter = 7,
            ResonanceGained = 1,
            ApCostPaid = 2,
            CascadeApplied = true,
            CorruptionCheckPerformed = false,
            CorruptionTriggered = true,
            CorruptionRoll = 4,
            CorruptionRiskPercent = 5
        };

        // Act
        var desc = result.GetDescription();

        // Assert
        desc.Should().Contain("Cascade: -1 AP");
        desc.Should().Contain("CORRUPTION +1");
        desc.Should().Contain("2 AP");
        desc.Should().Contain("d20 = 14");
    }

    [Test]
    public void GetResonanceChange_ReturnsFormattedString()
    {
        // Arrange
        var result = new WeaveDisruptionResult
        {
            ResonanceBefore = 6,
            ResonanceAfter = 7,
            ResonanceGained = 1
        };

        // Act
        var change = result.GetResonanceChange();

        // Assert
        change.Should().Be("Resonance: 6 → 7 (+1)");
    }
}
