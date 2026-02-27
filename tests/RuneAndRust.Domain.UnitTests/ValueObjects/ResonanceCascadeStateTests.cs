using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ResonanceCascadeState"/>.
/// Covers activation at various Resonance levels, cost reduction calculations,
/// unlock status, and display methods.
/// </summary>
[TestFixture]
public class ResonanceCascadeStateTests
{
    // ===== Evaluation Tests =====

    [Test]
    public void Evaluate_NotUnlocked_ReturnsInactive()
    {
        // Act
        var state = ResonanceCascadeState.Evaluate(7, cascadeUnlocked: false);

        // Assert
        state.IsActive.Should().BeFalse();
        state.IsUnlocked.Should().BeFalse();
        state.CostReduction.Should().Be(0);
        state.CurrentResonance.Should().Be(7);
    }

    [Test]
    public void Evaluate_UnlockedAtResonance0_ReturnsInactive()
    {
        // Act
        var state = ResonanceCascadeState.Evaluate(0, cascadeUnlocked: true);

        // Assert
        state.IsActive.Should().BeFalse();
        state.IsUnlocked.Should().BeTrue();
        state.CostReduction.Should().Be(0);
    }

    [Test]
    public void Evaluate_UnlockedAtResonance4_ReturnsInactive()
    {
        // Act
        var state = ResonanceCascadeState.Evaluate(4, cascadeUnlocked: true);

        // Assert
        state.IsActive.Should().BeFalse();
        state.CostReduction.Should().Be(0);
    }

    [Test]
    public void Evaluate_UnlockedAtResonance5_ReturnsActive()
    {
        // Act
        var state = ResonanceCascadeState.Evaluate(5, cascadeUnlocked: true);

        // Assert
        state.IsActive.Should().BeTrue();
        state.IsUnlocked.Should().BeTrue();
        state.CostReduction.Should().Be(1);
        state.MinimumApCost.Should().Be(1);
    }

    [Test]
    public void Evaluate_UnlockedAtResonance7_ReturnsActive()
    {
        // Act
        var state = ResonanceCascadeState.Evaluate(7, cascadeUnlocked: true);

        // Assert
        state.IsActive.Should().BeTrue();
        state.CostReduction.Should().Be(1);
    }

    [Test]
    public void Evaluate_UnlockedAtResonance10_ReturnsActive()
    {
        // Act
        var state = ResonanceCascadeState.Evaluate(10, cascadeUnlocked: true);

        // Assert
        state.IsActive.Should().BeTrue();
        state.CurrentResonance.Should().Be(10);
    }

    // ===== Cost Reduction Tests =====

    [Test]
    public void GetReducedCost_WhenInactive_ReturnsBaseCost()
    {
        // Arrange
        var state = ResonanceCascadeState.Evaluate(3, cascadeUnlocked: true);

        // Act & Assert
        state.GetReducedCost(2).Should().Be(2);
        state.GetReducedCost(3).Should().Be(3);
    }

    [TestCase(1, 1, Description = "Seiðr Bolt: 1→1 (already at minimum)")]
    [TestCase(2, 1, Description = "Fate's Thread / Wyrd Sight: 2→1")]
    [TestCase(3, 2, Description = "Weave Disruption: 3→2")]
    [TestCase(5, 4, Description = "Future Aether Storm: 5→4")]
    public void GetReducedCost_WhenActive_ReducesByOne_MinimumOne(int baseCost, int expected)
    {
        // Arrange
        var state = ResonanceCascadeState.Evaluate(5, cascadeUnlocked: true);

        // Act
        var reduced = state.GetReducedCost(baseCost);

        // Assert
        reduced.Should().Be(expected);
    }

    [Test]
    public void GetReducedCost_NotUnlocked_ReturnsBaseCost()
    {
        // Arrange
        var state = ResonanceCascadeState.Evaluate(8, cascadeUnlocked: false);

        // Act
        state.GetReducedCost(3).Should().Be(3);
    }

    // ===== Display Method Tests =====

    [Test]
    public void GetDescription_NotUnlocked_DescribesLocked()
    {
        // Arrange
        var state = ResonanceCascadeState.Evaluate(5, cascadeUnlocked: false);

        // Act
        var desc = state.GetDescription();

        // Assert
        desc.Should().Contain("not unlocked");
    }

    [Test]
    public void GetDescription_UnlockedButInactive_DescribesRequirement()
    {
        // Arrange
        var state = ResonanceCascadeState.Evaluate(3, cascadeUnlocked: true);

        // Act
        var desc = state.GetDescription();

        // Assert
        desc.Should().Contain("inactive");
        desc.Should().Contain("3");
    }

    [Test]
    public void GetDescription_Active_DescribesReduction()
    {
        // Arrange
        var state = ResonanceCascadeState.Evaluate(7, cascadeUnlocked: true);

        // Act
        var desc = state.GetDescription();

        // Assert
        desc.Should().Contain("ACTIVE");
        desc.Should().Contain("1 less AP");
    }

    [Test]
    public void GetStatusString_Active_ShowsCompactStatus()
    {
        // Arrange
        var state = ResonanceCascadeState.Evaluate(6, cascadeUnlocked: true);

        // Act
        var status = state.GetStatusString();

        // Assert
        status.Should().Contain("ACTIVE");
        status.Should().Contain("-1 AP");
        status.Should().Contain("Resonance 6");
    }

    [Test]
    public void GetStatusString_Locked_ShowsLocked()
    {
        // Arrange
        var state = ResonanceCascadeState.Evaluate(8, cascadeUnlocked: false);

        // Act
        var status = state.GetStatusString();

        // Assert
        status.Should().Contain("locked");
    }
}
