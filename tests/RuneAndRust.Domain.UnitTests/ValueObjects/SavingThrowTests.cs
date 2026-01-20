using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the SavingThrow value object.
/// </summary>
[TestFixture]
public class SavingThrowTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsSavingThrow()
    {
        // Arrange & Act
        var save = SavingThrow.Create("Fortitude", 15, negates: true);

        // Assert
        save.Attribute.Should().Be("Fortitude");
        save.DC.Should().Be(15);
        save.Negates.Should().BeTrue();
    }

    [Test]
    public void Create_WithNullAttribute_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => SavingThrow.Create(null!, 12);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Fortitude_CreatesFortitudeSave()
    {
        // Arrange & Act
        var save = SavingThrow.Fortitude(14);

        // Assert
        save.Attribute.Should().Be("Fortitude");
        save.DC.Should().Be(14);
        save.Negates.Should().BeFalse();
    }

    [Test]
    public void Agility_CreatesAgilitySave()
    {
        // Arrange & Act
        var save = SavingThrow.Agility(12, negates: true);

        // Assert
        save.Attribute.Should().Be("Agility");
        save.DC.Should().Be(12);
        save.Negates.Should().BeTrue();
    }

    [Test]
    public void Will_CreatesWillSave()
    {
        // Arrange & Act
        var save = SavingThrow.Will(10);

        // Assert
        save.Attribute.Should().Be("Will");
        save.DC.Should().Be(10);
        save.Negates.Should().BeFalse();
    }

    [Test]
    public void Negates_DefaultsToFalse()
    {
        // Arrange & Act
        var save = SavingThrow.Create("Fortitude", 12);

        // Assert
        save.Negates.Should().BeFalse();
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var saveHalves = SavingThrow.Fortitude(12);
        var saveNegates = SavingThrow.Agility(14, negates: true);

        // Act
        var halvesStr = saveHalves.ToString();
        var negatesStr = saveNegates.ToString();

        // Assert
        halvesStr.Should().Be("Fortitude DC 12 (halves)");
        negatesStr.Should().Be("Agility DC 14 (negates)");
    }
}
