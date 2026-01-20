using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the CoverType enum.
/// </summary>
[TestFixture]
public class CoverTypeTests
{
    [Test]
    public void CoverType_HasExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<CoverType>();

        // Assert
        values.Should().HaveCount(3);
        values.Should().Contain(CoverType.None);
        values.Should().Contain(CoverType.Partial);
        values.Should().Contain(CoverType.Full);
    }

    [Test]
    public void CoverType_ParsesFromString()
    {
        // Arrange & Act
        var none = Enum.Parse<CoverType>("None", ignoreCase: true);
        var partial = Enum.Parse<CoverType>("Partial", ignoreCase: true);
        var full = Enum.Parse<CoverType>("Full", ignoreCase: true);

        // Assert
        none.Should().Be(CoverType.None);
        partial.Should().Be(CoverType.Partial);
        full.Should().Be(CoverType.Full);
    }

    [Test]
    public void CoverType_HasCorrectIntegerValues()
    {
        // Assert
        ((int)CoverType.None).Should().Be(0);
        ((int)CoverType.Partial).Should().Be(1);
        ((int)CoverType.Full).Should().Be(2);
    }
}
