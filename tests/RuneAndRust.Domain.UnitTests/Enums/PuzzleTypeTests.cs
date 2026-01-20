using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for the PuzzleType enum.
/// </summary>
[TestFixture]
public class PuzzleTypeTests
{
    [Test]
    public void PuzzleType_HasAllExpectedValues()
    {
        // Act
        var values = Enum.GetValues<PuzzleType>();

        // Assert
        values.Should().HaveCount(5);
        values.Should().Contain(PuzzleType.Sequence);
        values.Should().Contain(PuzzleType.Combination);
        values.Should().Contain(PuzzleType.Pattern);
        values.Should().Contain(PuzzleType.Riddle);
        values.Should().Contain(PuzzleType.Logic);
    }

    [Test]
    public void PuzzleType_HasCorrectIntegerValues()
    {
        // Assert
        ((int)PuzzleType.Sequence).Should().Be(0);
        ((int)PuzzleType.Combination).Should().Be(1);
        ((int)PuzzleType.Pattern).Should().Be(2);
        ((int)PuzzleType.Riddle).Should().Be(3);
        ((int)PuzzleType.Logic).Should().Be(4);
    }
}
