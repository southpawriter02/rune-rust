using FluentAssertions;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Domain.UnitTests.Definitions;

[TestFixture]
public class DifficultyClassDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_CreatesDifficultyClass()
    {
        // Act
        var dc = DifficultyClassDefinition.Create(
            id: "moderate",
            name: "Moderate",
            description: "Requires effort.",
            targetNumber: 12,
            color: "#FFFF44");

        // Assert
        dc.Id.Should().Be("moderate");
        dc.Name.Should().Be("Moderate");
        dc.TargetNumber.Should().Be(12);
        dc.Color.Should().Be("#FFFF44");
    }

    [Test]
    public void Create_WithZeroTargetNumber_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => DifficultyClassDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            targetNumber: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetNumber");
    }

    [Test]
    public void IsMet_WhenResultMeetsOrExceeds_ReturnsTrue()
    {
        // Arrange
        var dc = DifficultyClassDefinition.Create(
            id: "moderate",
            name: "Moderate",
            description: "Test",
            targetNumber: 12);

        // Act & Assert
        dc.IsMet(12).Should().BeTrue();
        dc.IsMet(15).Should().BeTrue();
        dc.IsMet(11).Should().BeFalse();
    }

    [Test]
    public void GetMargin_CalculatesCorrectly()
    {
        // Arrange
        var dc = DifficultyClassDefinition.Create(
            id: "moderate",
            name: "Moderate",
            description: "Test",
            targetNumber: 12);

        // Act & Assert
        dc.GetMargin(15).Should().Be(3);
        dc.GetMargin(12).Should().Be(0);
        dc.GetMargin(8).Should().Be(-4);
    }
}
