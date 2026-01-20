using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class AbilityCostTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsCost()
    {
        // Arrange & Act
        var cost = AbilityCost.Create("mana", 25);

        // Assert
        cost.ResourceTypeId.Should().Be("mana");
        cost.Amount.Should().Be(25);
        cost.HasCost.Should().BeTrue();
    }

    [Test]
    public void None_HasCostReturnsFalse()
    {
        // Arrange & Act
        var cost = AbilityCost.None;

        // Assert
        cost.HasCost.Should().BeFalse();
        cost.Amount.Should().Be(0);
        cost.ResourceTypeId.Should().BeEmpty();
    }

    [Test]
    public void HasCost_WithPositiveAmount_ReturnsTrue()
    {
        // Arrange
        var cost = AbilityCost.Create("rage", 15);

        // Assert
        cost.HasCost.Should().BeTrue();
    }

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var costWithResource = AbilityCost.Create("mana", 30);
        var freeCost = AbilityCost.None;

        // Assert
        costWithResource.ToString().Should().Be("30 mana");
        freeCost.ToString().Should().Be("Free");
    }
}
