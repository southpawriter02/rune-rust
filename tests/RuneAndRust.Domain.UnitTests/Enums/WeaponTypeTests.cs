using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

[TestFixture]
public class WeaponTypeTests
{
    [Test]
    public void WeaponType_ContainsAllExpectedValues()
    {
        // Assert
        Enum.GetValues<WeaponType>().Should().HaveCount(4);
        Enum.IsDefined(WeaponType.Sword).Should().BeTrue();
        Enum.IsDefined(WeaponType.Axe).Should().BeTrue();
        Enum.IsDefined(WeaponType.Dagger).Should().BeTrue();
        Enum.IsDefined(WeaponType.Staff).Should().BeTrue();
    }

    [Test]
    [TestCase("Sword", WeaponType.Sword)]
    [TestCase("sword", WeaponType.Sword)]
    [TestCase("SWORD", WeaponType.Sword)]
    [TestCase("Axe", WeaponType.Axe)]
    [TestCase("Dagger", WeaponType.Dagger)]
    [TestCase("Staff", WeaponType.Staff)]
    public void TryParse_ValidWeaponType_ReturnsTrue(string input, WeaponType expected)
    {
        // Act
        var success = Enum.TryParse<WeaponType>(input, ignoreCase: true, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [Test]
    [TestCase("InvalidWeapon")]
    [TestCase("Bow")]
    [TestCase("Spear")]
    [TestCase("")]
    public void TryParse_InvalidWeaponType_ReturnsFalse(string input)
    {
        // Act
        var success = Enum.TryParse<WeaponType>(input, ignoreCase: true, out _);

        // Assert
        success.Should().BeFalse();
    }
}
