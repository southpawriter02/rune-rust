using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="AetherStormResult"/>.
/// Covers damage tracking, Resonance tracking, Corruption state, AP cost/Cascade tracking,
/// and display method output.
/// </summary>
[TestFixture]
public class AetherStormResultTests
{
    // ===== Factory / Init Tests =====

    [Test]
    public void Create_WithDefaults_HasZeroValues()
    {
        // Act
        var result = new AetherStormResult();

        // Assert
        result.DamageRoll.Should().Be(0);
        result.TotalDamage.Should().Be(0);
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
    public void Create_WithDamage_TracksDamageRollAndTotal()
    {
        // Act
        var result = new AetherStormResult
        {
            DamageRoll = 16,
            TotalDamage = 16
        };

        // Assert
        result.DamageRoll.Should().Be(16);
        result.TotalDamage.Should().Be(16);
    }

    [Test]
    public void Create_WithResonanceChange_TracksBeforeAndAfter()
    {
        // Act
        var result = new AetherStormResult
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
        var result = new AetherStormResult
        {
            ResonanceBefore = 8,
            ResonanceAfter = 10,
            ResonanceGained = 2,
            CorruptionCheckPerformed = true,
            CorruptionTriggered = true,
            CorruptionReason = "Corruption triggered at Resonance 8",
            CorruptionRoll = 10,
            CorruptionRiskPercent = 15
        };

        // Assert
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeTrue();
        result.CorruptionRoll.Should().Be(10);
        result.CorruptionRiskPercent.Should().Be(15);
    }

    [Test]
    public void Create_WithCorruptionSafeWithRoll_TracksCheckButNotTriggered()
    {
        // Act
        var result = new AetherStormResult
        {
            CorruptionCheckPerformed = true,
            CorruptionTriggered = false,
            CorruptionRoll = 85,
            CorruptionRiskPercent = 15
        };

        // Assert
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeFalse();
        result.CorruptionRoll.Should().Be(85);
    }

    [Test]
    public void Create_WithCascadeApplied_TracksReducedCost()
    {
        // Act
        var result = new AetherStormResult
        {
            ApCostPaid = 4,
            CascadeApplied = true
        };

        // Assert
        result.ApCostPaid.Should().Be(4);
        result.CascadeApplied.Should().BeTrue();
    }

    [Test]
    public void Create_WithoutCascade_TracksBaseCost()
    {
        // Act
        var result = new AetherStormResult
        {
            ApCostPaid = 5,
            CascadeApplied = false
        };

        // Assert
        result.ApCostPaid.Should().Be(5);
        result.CascadeApplied.Should().BeFalse();
    }

    // ===== Display Method Tests =====

    [Test]
    public void GetDamageBreakdown_ReturnsFormattedString()
    {
        // Arrange
        var result = new AetherStormResult
        {
            DamageRoll = 16,
            TotalDamage = 16
        };

        // Act
        var breakdown = result.GetDamageBreakdown();

        // Assert
        breakdown.Should().Be("Aether Storm: 4d6 = 16 Aetheric damage (Total: 16)");
    }

    [Test]
    public void GetDescription_WithCascadeAndCorruption_IncludesBothTags()
    {
        // Arrange
        var result = new AetherStormResult
        {
            DamageRoll = 16,
            TotalDamage = 16,
            ResonanceBefore = 8,
            ResonanceAfter = 10,
            ResonanceGained = 2,
            ApCostPaid = 4,
            CascadeApplied = true,
            CorruptionTriggered = true,
            CorruptionRoll = 10,
            CorruptionRiskPercent = 15
        };

        // Act
        var desc = result.GetDescription();

        // Assert
        desc.Should().Contain("Cascade: -1 AP");
        desc.Should().Contain("CORRUPTION +1");
        desc.Should().Contain("4 AP");
        desc.Should().Contain("16 Aetheric damage");
    }

    [Test]
    public void GetDescription_SafeWithRoll_IncludesCheckTag()
    {
        // Arrange
        var result = new AetherStormResult
        {
            DamageRoll = 14,
            TotalDamage = 14,
            ResonanceBefore = 5,
            ResonanceAfter = 7,
            ResonanceGained = 2,
            ApCostPaid = 5,
            CascadeApplied = false,
            CorruptionCheckPerformed = true,
            CorruptionTriggered = false,
            CorruptionRoll = 85,
            CorruptionRiskPercent = 5
        };

        // Act
        var desc = result.GetDescription();

        // Assert
        desc.Should().Contain("Corruption check");
        desc.Should().Contain("d100=85");
        desc.Should().Contain("safe");
        desc.Should().NotContain("Cascade");
    }

    [Test]
    public void GetResonanceChange_ReturnsFormattedString()
    {
        // Arrange
        var result = new AetherStormResult
        {
            ResonanceBefore = 6,
            ResonanceAfter = 8,
            ResonanceGained = 2
        };

        // Act
        var change = result.GetResonanceChange();

        // Assert
        change.Should().Contain("Resonance: 6 → 8");
        change.Should().Contain("+2");
    }
}
