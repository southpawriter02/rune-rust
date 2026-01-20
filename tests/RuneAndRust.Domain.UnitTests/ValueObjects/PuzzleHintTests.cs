using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the PuzzleHint value object.
/// </summary>
[TestFixture]
public class PuzzleHintTests
{
    [Test]
    public void Free_CreatesHintWithZeroDC()
    {
        // Act
        var hint = PuzzleHint.Free("This is a hint", 1);

        // Assert
        hint.Text.Should().Be("This is a hint");
        hint.Order.Should().Be(1);
        hint.RevealDC.Should().Be(0);
        hint.IsFree.Should().BeTrue();
        hint.HasCost.Should().BeFalse();
    }

    [Test]
    public void WithCheck_CreatesHintWithDC()
    {
        // Act
        var hint = PuzzleHint.WithCheck("Think carefully", 2, 12, "Intelligence");

        // Assert
        hint.Text.Should().Be("Think carefully");
        hint.Order.Should().Be(2);
        hint.RevealDC.Should().Be(12);
        hint.RevealAttribute.Should().Be("Intelligence");
        hint.IsFree.Should().BeFalse();
        hint.HasCost.Should().BeTrue();
    }

    [Test]
    public void Free_WithNullText_ThrowsException()
    {
        // Act
        var act = () => PuzzleHint.Free(null!, 1);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void WithCheck_WithZeroOrder_ThrowsException()
    {
        // Act
        var act = () => PuzzleHint.WithCheck("Hint", 0, 10, "Wisdom");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
