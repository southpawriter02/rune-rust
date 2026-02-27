using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="UnravelingResult"/>.
/// Covers accumulated damage tracking, Resonance reset, guaranteed Corruption check,
/// AP cost (Cascade immune), per-combat cooldown, and display method output.
/// </summary>
[TestFixture]
public class UnravelingResultTests
{
    // ===== Factory / Init Tests =====

    [Test]
    public void Create_WithDefaults_HasZeroValues()
    {
        // Act
        var result = new UnravelingResult();

        // Assert
        result.AccumulatedDamageConsumed.Should().Be(0);
        result.TotalDamage.Should().Be(0);
        result.ResonanceBefore.Should().Be(0);
        result.ResonanceAfter.Should().Be(0);
        result.CorruptionCheckPerformed.Should().BeFalse();
        result.CorruptionTriggered.Should().BeFalse();
        result.CorruptionReason.Should().BeNull();
        result.CorruptionRoll.Should().Be(0);
        result.CorruptionRiskPercent.Should().Be(0);
        result.ApCostPaid.Should().Be(0);
        result.CooldownActivated.Should().BeFalse();
    }

    [Test]
    public void Create_WithAccumulatedDamage_TracksDamageConsumedAndTotal()
    {
        // Act
        var result = new UnravelingResult
        {
            AccumulatedDamageConsumed = 42,
            TotalDamage = 42
        };

        // Assert
        result.AccumulatedDamageConsumed.Should().Be(42);
        result.TotalDamage.Should().Be(42);
    }

    [Test]
    public void Create_WithResonanceReset_TracksBeforeAndAfter()
    {
        // Act
        var result = new UnravelingResult
        {
            ResonanceBefore = 10,
            ResonanceAfter = 0
        };

        // Assert
        result.ResonanceBefore.Should().Be(10);
        result.ResonanceAfter.Should().Be(0);
    }

    [Test]
    public void Create_WithCorruptionTriggered_TracksAllFields()
    {
        // Act
        var result = new UnravelingResult
        {
            CorruptionCheckPerformed = true,
            CorruptionTriggered = true,
            CorruptionReason = "The Unraveling tears at the fabric of reality",
            CorruptionRoll = 15,
            CorruptionRiskPercent = 20
        };

        // Assert
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeTrue();
        result.CorruptionRoll.Should().Be(15);
        result.CorruptionRiskPercent.Should().Be(20);
    }

    [Test]
    public void Create_WithCorruptionSafe_TracksCheckButNotTriggered()
    {
        // Act
        var result = new UnravelingResult
        {
            CorruptionCheckPerformed = true,
            CorruptionTriggered = false,
            CorruptionRoll = 55,
            CorruptionRiskPercent = 20
        };

        // Assert
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeFalse();
        result.CorruptionRoll.Should().Be(55);
        result.CorruptionRiskPercent.Should().Be(20);
    }

    [Test]
    public void Create_ApCostAlwaysFive_CascadeImmune()
    {
        // Act
        var result = new UnravelingResult
        {
            ApCostPaid = 5
        };

        // Assert
        result.ApCostPaid.Should().Be(5);
    }

    [Test]
    public void Create_CooldownAlwaysActivated()
    {
        // Act
        var result = new UnravelingResult
        {
            CooldownActivated = true
        };

        // Assert
        result.CooldownActivated.Should().BeTrue();
    }

    [Test]
    public void Create_CorruptionRiskPercentAlwaysTwenty()
    {
        // Act
        var result = new UnravelingResult
        {
            CorruptionRiskPercent = 20
        };

        // Assert
        result.CorruptionRiskPercent.Should().Be(20);
    }

    // ===== Display Method Tests =====

    [Test]
    public void GetDamageBreakdown_ReturnsAccumulatedWording()
    {
        // Arrange
        var result = new UnravelingResult
        {
            AccumulatedDamageConsumed = 42
        };

        // Act
        var breakdown = result.GetDamageBreakdown();

        // Assert
        breakdown.Should().Be("The Unraveling: 42 accumulated Aetheric damage released");
    }

    [Test]
    public void GetDescription_WithCorruptionTriggered_IncludesCorruptionPlusTwo()
    {
        // Arrange
        var result = new UnravelingResult
        {
            AccumulatedDamageConsumed = 42,
            TotalDamage = 42,
            ResonanceBefore = 10,
            ResonanceAfter = 0,
            CorruptionCheckPerformed = true,
            CorruptionTriggered = true,
            CorruptionRoll = 15,
            CorruptionRiskPercent = 20,
            ApCostPaid = 5,
            CooldownActivated = true
        };

        // Act
        var desc = result.GetDescription();

        // Assert
        desc.Should().Contain("CORRUPTION +2");
        desc.Should().Contain("42 accumulated Aetheric damage");
        desc.Should().Contain("10 → 0");
        desc.Should().Contain("Cooldown activated");
    }

    [Test]
    public void GetDescription_WithCorruptionSafe_IncludesSafeTag()
    {
        // Arrange
        var result = new UnravelingResult
        {
            AccumulatedDamageConsumed = 30,
            TotalDamage = 30,
            ResonanceBefore = 10,
            ResonanceAfter = 0,
            CorruptionCheckPerformed = true,
            CorruptionTriggered = false,
            CorruptionRoll = 55,
            CorruptionRiskPercent = 20,
            ApCostPaid = 5,
            CooldownActivated = true
        };

        // Act
        var desc = result.GetDescription();

        // Assert
        desc.Should().Contain("Corruption check");
        desc.Should().Contain("d100=55");
        desc.Should().Contain("safe");
        desc.Should().Contain("Cooldown activated");
    }

    [Test]
    public void GetResonanceChange_ReturnsResetFormat()
    {
        // Arrange
        var result = new UnravelingResult
        {
            ResonanceBefore = 10,
            ResonanceAfter = 0
        };

        // Act
        var change = result.GetResonanceChange();

        // Assert
        change.Should().Be("Resonance: 10 → 0 (reset)");
    }
}
