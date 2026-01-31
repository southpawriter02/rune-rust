namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="StressCheckResult"/> value object.
/// Tests cover the resistance reduction table, factory methods, computed properties,
/// arrow-expression properties, FinalStress truncation behavior, and ToString formatting.
/// </summary>
[TestFixture]
public class StressCheckResultTests
{
    // -------------------------------------------------------------------------
    // Factory Methods — Create (Reduction Table)
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, 20, 0.00, 20)]
    [TestCase(1, 20, 0.50, 10)]
    [TestCase(2, 20, 0.75, 5)]
    [TestCase(3, 20, 0.75, 5)]
    [TestCase(4, 20, 1.00, 0)]
    [TestCase(5, 20, 1.00, 0)]
    public void Create_CalculatesReductionCorrectly(
        int successes, int baseStress, decimal expectedPercent, int expectedFinal)
    {
        // Arrange & Act
        var result = StressCheckResult.Create(successes, baseStress);

        // Assert
        result.ReductionPercent.Should().Be(expectedPercent);
        result.FinalStress.Should().Be(expectedFinal);
        result.Succeeded.Should().Be(successes > 0);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    public void Create_ZeroBaseStress_AlwaysZeroFinalStress(int successes)
    {
        // Arrange & Act
        var result = StressCheckResult.Create(successes, baseStress: 0);

        // Assert
        result.FinalStress.Should().Be(0);
        result.BaseStress.Should().Be(0);
    }

    [Test]
    public void Create_LargeSuccessCount_TreatedAsFourPlus()
    {
        // Arrange & Act
        var result = StressCheckResult.Create(successes: 10, baseStress: 30);

        // Assert
        result.ReductionPercent.Should().Be(1.00m);
        result.FinalStress.Should().Be(0);
        result.WasFullyResisted.Should().Be(true);
    }

    [Test]
    public void Create_StoresSuccessesAndBaseStress()
    {
        // Arrange & Act
        var result = StressCheckResult.Create(successes: 3, baseStress: 40);

        // Assert
        result.Successes.Should().Be(3);
        result.BaseStress.Should().Be(40);
    }

    // -------------------------------------------------------------------------
    // Factory Methods — Create (Validation)
    // -------------------------------------------------------------------------

    [Test]
    public void Create_ThrowsForNegativeSuccesses()
    {
        // Act
        var act = () => StressCheckResult.Create(successes: -1, baseStress: 20);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Create_ThrowsForNegativeBaseStress()
    {
        // Act
        var act = () => StressCheckResult.Create(successes: 0, baseStress: -5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // -------------------------------------------------------------------------
    // Factory Methods — NoResistance
    // -------------------------------------------------------------------------

    [Test]
    public void NoResistance_ZeroSuccesses_FullStress()
    {
        // Arrange & Act
        var result = StressCheckResult.NoResistance(baseStress: 20);

        // Assert
        result.Successes.Should().Be(0);
        result.FinalStress.Should().Be(20);
        result.Succeeded.Should().Be(false);
    }

    [Test]
    public void NoResistance_ReductionPercentIsZero()
    {
        // Arrange & Act
        var result = StressCheckResult.NoResistance(baseStress: 15);

        // Assert
        result.ReductionPercent.Should().Be(0.00m);
    }

    [Test]
    public void NoResistance_ThrowsForNegativeBaseStress()
    {
        // Act
        var act = () => StressCheckResult.NoResistance(baseStress: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // -------------------------------------------------------------------------
    // Computed Properties — Succeeded
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    [TestCase(4, true)]
    public void Succeeded_ReflectsSuccessCount(int successes, bool expected)
    {
        // Arrange & Act
        var result = StressCheckResult.Create(successes, baseStress: 20);

        // Assert
        result.Succeeded.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — WasFullyResisted
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(4, 20, true)]   // 100% reduction → FinalStress = 0
    [TestCase(0, 20, false)]  // 0% reduction → FinalStress = 20
    [TestCase(1, 20, false)]  // 50% reduction → FinalStress = 10
    [TestCase(2, 20, false)]  // 75% reduction → FinalStress = 5
    [TestCase(4, 0, true)]    // 100% reduction of 0 → FinalStress = 0
    [TestCase(0, 0, true)]    // 0% reduction of 0 → FinalStress = 0
    public void WasFullyResisted_TrueOnlyWhenFinalStressIsZero(
        int successes, int baseStress, bool expected)
    {
        // Arrange & Act
        var result = StressCheckResult.Create(successes, baseStress);

        // Assert
        result.WasFullyResisted.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — WasPartiallyResisted
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(1, 20, true)]   // Succeeded + FinalStress > 0 → partial
    [TestCase(2, 20, true)]   // Succeeded + FinalStress > 0 → partial
    [TestCase(4, 20, false)]  // Succeeded but FinalStress == 0 → fully resisted, not partial
    [TestCase(0, 20, false)]  // Did not succeed → not partial
    [TestCase(1, 0, false)]   // Succeeded but FinalStress == 0 (base was 0) → not partial
    public void WasPartiallyResisted_TrueWhenSucceededButStressRemains(
        int successes, int baseStress, bool expected)
    {
        // Arrange & Act
        var result = StressCheckResult.Create(successes, baseStress);

        // Assert
        result.WasPartiallyResisted.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // FinalStress Calculation — Truncation Behavior
    // -------------------------------------------------------------------------

    [Test]
    public void FinalStress_TruncatesNotRounds_OneSuccess()
    {
        // Arrange & Act — 15 × 0.5 = 7.5 → truncated to 7
        var result = StressCheckResult.Create(successes: 1, baseStress: 15);

        // Assert
        result.FinalStress.Should().Be(7, "decimal truncation should round toward zero");
    }

    [Test]
    public void FinalStress_TruncatesNotRounds_TwoSuccesses()
    {
        // Arrange & Act — 10 × 0.25 = 2.5 → truncated to 2
        var result = StressCheckResult.Create(successes: 2, baseStress: 10);

        // Assert
        result.FinalStress.Should().Be(2, "decimal truncation should round toward zero");
    }

    [Test]
    public void FinalStress_TruncatesNotRounds_ThreeSuccesses()
    {
        // Arrange & Act — 7 × 0.25 = 1.75 → truncated to 1
        var result = StressCheckResult.Create(successes: 3, baseStress: 7);

        // Assert
        result.FinalStress.Should().Be(1, "decimal truncation should round toward zero");
    }

    [Test]
    [TestCase(0, 100, 100)]  // Full stress at max
    [TestCase(1, 100, 50)]   // 50% of 100
    [TestCase(2, 100, 25)]   // 25% of 100
    [TestCase(4, 100, 0)]    // Fully resisted
    [TestCase(1, 1, 0)]      // 1 × 0.5 = 0.5 → truncated to 0
    public void FinalStress_BoundaryValues(int successes, int baseStress, int expectedFinal)
    {
        // Arrange & Act
        var result = StressCheckResult.Create(successes, baseStress);

        // Assert
        result.FinalStress.Should().Be(expectedFinal);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_FormatsCorrectly_WithSuccesses()
    {
        // Arrange
        var result = StressCheckResult.Create(successes: 2, baseStress: 20);

        // Act
        var display = result.ToString();

        // Assert — P0 formats as percentage without decimals
        display.Should().Contain("2 successes");
        display.Should().Contain("75%");
        display.Should().Contain("20");
        display.Should().Contain("5");
    }

    [Test]
    public void ToString_FormatsCorrectly_ZeroSuccesses()
    {
        // Arrange
        var result = StressCheckResult.NoResistance(baseStress: 15);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("0 successes");
        display.Should().Contain("0%");
        display.Should().Contain("15");
    }

    [Test]
    public void ToString_FormatsCorrectly_FullResistance()
    {
        // Arrange
        var result = StressCheckResult.Create(successes: 4, baseStress: 30);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("4 successes");
        display.Should().Contain("100%");
        display.Should().Contain("→ 0");
    }
}
