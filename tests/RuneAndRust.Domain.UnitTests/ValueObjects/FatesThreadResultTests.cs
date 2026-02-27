using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="FatesThreadResult"/>.
/// Covers Resonance tracking, Corruption state, AP cost/Cascade tracking,
/// and display method output.
/// </summary>
[TestFixture]
public class FatesThreadResultTests
{
    // ===== Factory / Init Tests =====

    [Test]
    public void Create_WithDefaults_HasZeroValues()
    {
        // Act
        var result = new FatesThreadResult();

        // Assert
        result.ResonanceBefore.Should().Be(0);
        result.ResonanceAfter.Should().Be(0);
        result.ResonanceGained.Should().Be(0);
        result.CorruptionCheckPerformed.Should().BeFalse();
        result.CorruptionTriggered.Should().BeFalse();
        result.CorruptionReason.Should().BeNull();
        result.CorruptionRoll.Should().Be(0);
        result.CorruptionRiskPercent.Should().Be(0);
        result.ApCostPaid.Should().Be(0);
        result.CascadeApplied.Should().BeFalse();
    }

    [Test]
    public void Create_WithResonanceChange_TracksBeforeAndAfter()
    {
        // Act
        var result = new FatesThreadResult
        {
            ResonanceBefore = 3,
            ResonanceAfter = 5,
            ResonanceGained = 2
        };

        // Assert
        result.ResonanceBefore.Should().Be(3);
        result.ResonanceAfter.Should().Be(5);
        result.ResonanceGained.Should().Be(2);
    }

    [Test]
    public void Create_WithCorruptionTriggered_TracksAllFields()
    {
        // Act
        var result = new FatesThreadResult
        {
            ResonanceBefore = 6,
            ResonanceAfter = 8,
            ResonanceGained = 2,
            CorruptionCheckPerformed = true,
            CorruptionTriggered = true,
            CorruptionReason = "Corruption triggered at Resonance 6",
            CorruptionRoll = 3,
            CorruptionRiskPercent = 5
        };

        // Assert
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeTrue();
        result.CorruptionRoll.Should().Be(3);
        result.CorruptionRiskPercent.Should().Be(5);
    }

    [Test]
    public void Create_WithCorruptionSafeWithRoll_TracksCheckButNotTriggered()
    {
        // Act
        var result = new FatesThreadResult
        {
            CorruptionCheckPerformed = true,
            CorruptionTriggered = false,
            CorruptionRoll = 78,
            CorruptionRiskPercent = 5
        };

        // Assert
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeFalse();
        result.CorruptionRoll.Should().Be(78);
    }

    [Test]
    public void Create_WithCascadeApplied_TracksReducedCost()
    {
        // Act
        var result = new FatesThreadResult
        {
            ApCostPaid = 1,
            CascadeApplied = true
        };

        // Assert
        result.ApCostPaid.Should().Be(1);
        result.CascadeApplied.Should().BeTrue();
    }

    [Test]
    public void Create_WithoutCascade_TracksBaseCost()
    {
        // Act
        var result = new FatesThreadResult
        {
            ApCostPaid = 2,
            CascadeApplied = false
        };

        // Assert
        result.ApCostPaid.Should().Be(2);
        result.CascadeApplied.Should().BeFalse();
    }

    // ===== Display Method Tests =====

    [Test]
    public void GetDescription_WithCascadeAndCorruption_IncludesBothTags()
    {
        // Arrange
        var result = new FatesThreadResult
        {
            ResonanceBefore = 6,
            ResonanceAfter = 8,
            ResonanceGained = 2,
            ApCostPaid = 1,
            CascadeApplied = true,
            CorruptionCheckPerformed = false,
            CorruptionTriggered = true,
            CorruptionRoll = 3,
            CorruptionRiskPercent = 5
        };

        // Act
        var desc = result.GetDescription();

        // Assert
        desc.Should().Contain("Cascade: -1 AP");
        desc.Should().Contain("CORRUPTION +1");
        desc.Should().Contain("1 AP");
    }

    [Test]
    public void GetDescription_SafeWithRoll_IncludesCheckTag()
    {
        // Arrange
        var result = new FatesThreadResult
        {
            ResonanceBefore = 5,
            ResonanceAfter = 7,
            ResonanceGained = 2,
            ApCostPaid = 2,
            CascadeApplied = false,
            CorruptionCheckPerformed = true,
            CorruptionTriggered = false,
            CorruptionRoll = 78,
            CorruptionRiskPercent = 5
        };

        // Act
        var desc = result.GetDescription();

        // Assert
        desc.Should().Contain("Corruption check");
        desc.Should().Contain("d100=78");
        desc.Should().Contain("safe");
        desc.Should().NotContain("Cascade");
    }

    [Test]
    public void GetResonanceChange_ReturnsFormattedString()
    {
        // Arrange
        var result = new FatesThreadResult
        {
            ResonanceBefore = 4,
            ResonanceAfter = 6,
            ResonanceGained = 2
        };

        // Act
        var change = result.GetResonanceChange();

        // Assert
        change.Should().Be("Resonance: 4 → 6 (+2)");
    }
}
