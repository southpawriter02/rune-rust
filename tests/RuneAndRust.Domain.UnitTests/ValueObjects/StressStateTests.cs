namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="StressState"/> value object.
/// </summary>
[TestFixture]
public class StressStateTests
{
    // -------------------------------------------------------------------------
    // Factory Methods — Create
    // -------------------------------------------------------------------------

    [Test]
    public void Create_StoresValidValue()
    {
        // Act
        var state = StressState.Create(45);

        // Assert
        state.CurrentStress.Should().Be(45);
    }

    [Test]
    public void Create_ClampsNegativeToZero()
    {
        // Act
        var state = StressState.Create(-10);

        // Assert
        state.CurrentStress.Should().Be(0);
    }

    [Test]
    public void Create_ClampsAbove100()
    {
        // Act
        var state = StressState.Create(150);

        // Assert
        state.CurrentStress.Should().Be(100);
    }

    // -------------------------------------------------------------------------
    // Factory Methods — Calm
    // -------------------------------------------------------------------------

    [Test]
    public void Calm_ReturnsZeroStress()
    {
        // Act
        var state = StressState.Calm;

        // Assert
        state.CurrentStress.Should().Be(0);
        state.Threshold.Should().Be(StressThreshold.Calm);
        state.DefensePenalty.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — DefensePenalty
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, 0)]
    [TestCase(19, 0)]
    [TestCase(20, 1)]
    [TestCase(40, 2)]
    [TestCase(60, 3)]
    [TestCase(80, 4)]
    [TestCase(100, 5)]
    public void DefensePenalty_CalculatedCorrectly(int stress, int expectedPenalty)
    {
        // Act
        var state = StressState.Create(stress);

        // Assert
        state.DefensePenalty.Should().Be(expectedPenalty);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — HasSkillDisadvantage
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(79, false)]
    [TestCase(80, true)]
    [TestCase(99, true)]
    [TestCase(100, true)]
    public void HasSkillDisadvantage_SetCorrectly(int stress, bool expected)
    {
        // Act
        var state = StressState.Create(stress);

        // Assert
        state.HasSkillDisadvantage.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — RequiresTraumaCheck
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(99, false)]
    [TestCase(100, true)]
    [TestCase(150, true)]  // Clamped to 100
    public void RequiresTraumaCheck_TrueOnlyAt100OrAbove(int stress, bool expected)
    {
        // Act
        var state = StressState.Create(stress);

        // Assert
        state.RequiresTraumaCheck.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — IsCalm, IsBreaking
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, true)]
    [TestCase(19, true)]
    [TestCase(20, false)]
    [TestCase(50, false)]
    public void IsCalm_TrueOnlyForCalmThreshold(int stress, bool expected)
    {
        // Act
        var state = StressState.Create(stress);

        // Assert
        state.IsCalm.Should().Be(expected);
    }

    [Test]
    [TestCase(79, false)]
    [TestCase(80, true)]
    [TestCase(99, true)]
    [TestCase(100, true)]
    public void IsBreaking_TrueForBreakingAndTrauma(int stress, bool expected)
    {
        // Act
        var state = StressState.Create(stress);

        // Assert
        state.IsBreaking.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — StressPercentage
    // -------------------------------------------------------------------------

    [Test]
    public void StressPercentage_CalculatesCorrectly()
    {
        // Act
        var state = StressState.Create(50);

        // Assert
        state.StressPercentage.Should().BeApproximately(0.5, 0.001);
    }

    // -------------------------------------------------------------------------
    // Mutation Methods — WithStress
    // -------------------------------------------------------------------------

    [Test]
    public void WithStress_CreatesNewInstance()
    {
        // Arrange
        var original = StressState.Create(30);

        // Act
        var updated = original.WithStress(75);

        // Assert
        updated.CurrentStress.Should().Be(75);
        original.CurrentStress.Should().Be(30, "original should be unchanged (immutable)");
    }

    // -------------------------------------------------------------------------
    // Mutation Methods — WithStressAdded
    // -------------------------------------------------------------------------

    [Test]
    public void WithStressAdded_IncreasesStress()
    {
        // Arrange
        var state = StressState.Create(30);

        // Act
        var result = state.WithStressAdded(15);

        // Assert
        result.CurrentStress.Should().Be(45);
    }

    [Test]
    public void WithStressAdded_ClampsToMax()
    {
        // Arrange
        var state = StressState.Create(90);

        // Act
        var result = state.WithStressAdded(50);

        // Assert
        result.CurrentStress.Should().Be(100);
    }

    [Test]
    public void WithStressAdded_ThrowsForNegativeAmount()
    {
        // Arrange
        var state = StressState.Create(50);

        // Act
        var act = () => state.WithStressAdded(-5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // -------------------------------------------------------------------------
    // Mutation Methods — WithStressReduced
    // -------------------------------------------------------------------------

    [Test]
    public void WithStressReduced_DecreasesStress()
    {
        // Arrange
        var state = StressState.Create(60);

        // Act
        var result = state.WithStressReduced(20);

        // Assert
        result.CurrentStress.Should().Be(40);
    }

    [Test]
    public void WithStressReduced_ClampsToMin()
    {
        // Arrange
        var state = StressState.Create(10);

        // Act
        var result = state.WithStressReduced(50);

        // Assert
        result.CurrentStress.Should().Be(0);
    }

    [Test]
    public void WithStressReduced_ThrowsForNegativeAmount()
    {
        // Arrange
        var state = StressState.Create(50);

        // Act
        var act = () => state.WithStressReduced(-5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var state = StressState.Create(45);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be("Stress: 45/100 [Anxious] (Def: -2)");
    }

    [Test]
    public void ToString_ZeroStress_FormatsCorrectly()
    {
        // Arrange
        var state = StressState.Calm;

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be("Stress: 0/100 [Calm] (Def: -0)");
    }
}
