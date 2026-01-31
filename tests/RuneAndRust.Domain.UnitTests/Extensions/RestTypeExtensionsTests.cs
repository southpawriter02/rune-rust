namespace RuneAndRust.Domain.UnitTests.Extensions;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Unit tests for <see cref="RestTypeExtensions"/> extension methods.
/// </summary>
[TestFixture]
public class RestTypeExtensionsTests
{
    // -------------------------------------------------------------------------
    // CalculateRecovery — Per Rest Type
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, 0)]
    [TestCase(4, 8)]
    [TestCase(10, 20)]
    public void CalculateRecovery_ShortRest_ReturnsWillTimesTwo(int will, int expected)
    {
        // Act
        var result = RestType.Short.CalculateRecovery(will);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(4, 20)]
    [TestCase(10, 50)]
    public void CalculateRecovery_LongRest_ReturnsWillTimesFive(int will, int expected)
    {
        // Act
        var result = RestType.Long.CalculateRecovery(will);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void CalculateRecovery_Sanctuary_ReturnsIntMaxValue()
    {
        // Act
        var result = RestType.Sanctuary.CalculateRecovery(4);

        // Assert
        result.Should().Be(int.MaxValue);
    }

    [Test]
    public void CalculateRecovery_Milestone_ReturnsFixed25()
    {
        // Act
        var result = RestType.Milestone.CalculateRecovery(0);

        // Assert
        result.Should().Be(25);
    }

    [Test]
    public void CalculateRecovery_Milestone_IgnoresWillAttribute()
    {
        // Act — WILL should not affect milestone recovery
        var withZeroWill = RestType.Milestone.CalculateRecovery(0);
        var withHighWill = RestType.Milestone.CalculateRecovery(10);

        // Assert
        withZeroWill.Should().Be(25);
        withHighWill.Should().Be(25);
    }

    [Test]
    public void CalculateRecovery_ThrowsForNegativeWill()
    {
        // Act
        var act = () => RestType.Short.CalculateRecovery(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // -------------------------------------------------------------------------
    // GetFormulaDescription
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(RestType.Short, "Short Rest (WILL × 2)")]
    [TestCase(RestType.Long, "Long Rest (WILL × 5)")]
    [TestCase(RestType.Sanctuary, "Sanctuary (Full Reset)")]
    [TestCase(RestType.Milestone, "Milestone (+25)")]
    public void GetFormulaDescription_ReturnsExpected(RestType restType, string expected)
    {
        // Act
        var result = restType.GetFormulaDescription();

        // Assert
        result.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // IsFullRecovery
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(RestType.Short, false)]
    [TestCase(RestType.Long, false)]
    [TestCase(RestType.Sanctuary, true)]
    [TestCase(RestType.Milestone, false)]
    public void IsFullRecovery_TrueOnlyForSanctuary(RestType restType, bool expected)
    {
        // Act
        var result = restType.IsFullRecovery();

        // Assert
        result.Should().Be(expected);
    }
}
