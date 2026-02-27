using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="VolvasVisionResult"/>.
/// Covers reveal radius, Resonance tracking, Corruption state, AP cost/Cascade tracking,
/// and display method output.
/// </summary>
[TestFixture]
public class VolvasVisionResultTests
{
    // ===== Factory / Init Tests =====

    [Test]
    public void Create_WithDefaults_HasZeroValuesAndDefaultRadius()
    {
        // Act
        var result = new VolvasVisionResult();

        // Assert
        result.RevealRadius.Should().Be(15);
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
        var result = new VolvasVisionResult
        {
            ResonanceBefore = 4,
            ResonanceAfter = 6,
            ResonanceGained = 2
        };

        // Assert
        result.ResonanceBefore.Should().Be(4);
        result.ResonanceAfter.Should().Be(6);
        result.ResonanceGained.Should().Be(2);
    }

    [Test]
    public void Create_WithCorruptionTriggered_TracksAllFields()
    {
        // Act
        var result = new VolvasVisionResult
        {
            ResonanceBefore = 6,
            ResonanceAfter = 8,
            ResonanceGained = 2,
            CorruptionCheckPerformed = true,
            CorruptionTriggered = true,
            CorruptionReason = "Corruption triggered at Resonance 6",
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
    public void Create_WithCorruptionSafeWithRoll_TracksCheckButNotTriggered()
    {
        // Act
        var result = new VolvasVisionResult
        {
            CorruptionCheckPerformed = true,
            CorruptionTriggered = false,
            CorruptionRoll = 65,
            CorruptionRiskPercent = 5
        };

        // Assert
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeFalse();
        result.CorruptionRoll.Should().Be(65);
    }

    [Test]
    public void Create_WithCascadeApplied_TracksReducedCost()
    {
        // Act
        var result = new VolvasVisionResult
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
        var result = new VolvasVisionResult
        {
            ApCostPaid = 3,
            CascadeApplied = false
        };

        // Assert
        result.ApCostPaid.Should().Be(3);
        result.CascadeApplied.Should().BeFalse();
    }

    [Test]
    public void RevealRadius_DefaultsToFifteen()
    {
        // Act
        var result = new VolvasVisionResult();

        // Assert
        result.RevealRadius.Should().Be(15);
    }

    // ===== Display Method Tests =====

    [Test]
    public void GetDescription_WithCascadeAndCorruption_IncludesBothTags()
    {
        // Arrange
        var result = new VolvasVisionResult
        {
            RevealRadius = 15,
            ResonanceBefore = 6,
            ResonanceAfter = 8,
            ResonanceGained = 2,
            ApCostPaid = 2,
            CascadeApplied = true,
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
        desc.Should().Contain("15-space radius");
    }

    [Test]
    public void GetDescription_SafeWithRoll_IncludesCheckTag()
    {
        // Arrange
        var result = new VolvasVisionResult
        {
            RevealRadius = 15,
            ResonanceBefore = 5,
            ResonanceAfter = 7,
            ResonanceGained = 2,
            ApCostPaid = 3,
            CascadeApplied = false,
            CorruptionCheckPerformed = true,
            CorruptionTriggered = false,
            CorruptionRoll = 65,
            CorruptionRiskPercent = 5
        };

        // Act
        var desc = result.GetDescription();

        // Assert
        desc.Should().Contain("Corruption check");
        desc.Should().Contain("d100=65");
        desc.Should().Contain("safe");
        desc.Should().NotContain("Cascade");
    }

    [Test]
    public void GetResonanceChange_ReturnsFormattedString()
    {
        // Arrange
        var result = new VolvasVisionResult
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
