using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

[TestFixture]
public class ArmorTypeTests
{
    [Test]
    public void ArmorType_ContainsAllExpectedValues()
    {
        // Assert
        Enum.GetValues<ArmorType>().Should().HaveCount(3);
        Enum.IsDefined(ArmorType.Light).Should().BeTrue();
        Enum.IsDefined(ArmorType.Medium).Should().BeTrue();
        Enum.IsDefined(ArmorType.Heavy).Should().BeTrue();
    }

    [Test]
    [TestCase("Light", ArmorType.Light)]
    [TestCase("light", ArmorType.Light)]
    [TestCase("LIGHT", ArmorType.Light)]
    [TestCase("Medium", ArmorType.Medium)]
    [TestCase("Heavy", ArmorType.Heavy)]
    public void TryParse_ValidArmorType_ReturnsTrue(string input, ArmorType expected)
    {
        // Act
        var success = Enum.TryParse<ArmorType>(input, ignoreCase: true, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }
}
