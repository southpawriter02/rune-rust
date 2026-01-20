using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Item range properties (v0.5.1a).
/// </summary>
[TestFixture]
public class ItemRangeTests
{
    [Test]
    public void Item_DefaultRange_Is1()
    {
        // Arrange & Act
        var item = Item.CreateSword();

        // Assert
        item.Range.Should().Be(1);
        item.RangeType.Should().Be(RangeType.Melee);
    }

    [Test]
    public void SetRange_ValidValues_UpdatesProperties()
    {
        // Arrange
        var item = Item.CreateSword();

        // Act
        item.SetRange(5, RangeType.Ranged);

        // Assert
        item.Range.Should().Be(5);
        item.RangeType.Should().Be(RangeType.Ranged);
    }

    [Test]
    public void SetRange_ZeroRange_ThrowsException()
    {
        // Arrange
        var item = Item.CreateSword();

        // Act
        var act = () => item.SetRange(0, RangeType.Melee);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetEffectiveRange_MeleeType_Returns1()
    {
        // Arrange
        var item = Item.CreateSword();
        item.SetRange(5, RangeType.Melee); // Even with range 5

        // Act
        var effectiveRange = item.GetEffectiveRange();

        // Assert
        effectiveRange.Should().Be(1);
    }

    [Test]
    public void GetEffectiveRange_ReachType_Returns2()
    {
        // Arrange
        var item = Item.CreateSword();
        item.SetRange(5, RangeType.Reach); // Even with range 5

        // Act
        var effectiveRange = item.GetEffectiveRange();

        // Assert
        effectiveRange.Should().Be(2);
    }

    [Test]
    public void GetEffectiveRange_RangedType_ReturnsConfiguredRange()
    {
        // Arrange
        var item = Item.CreateSword();
        item.SetRange(12, RangeType.Ranged);

        // Act
        var effectiveRange = item.GetEffectiveRange();

        // Assert
        effectiveRange.Should().Be(12);
    }

    [TestCase(RangeType.Melee, 1, true)]
    [TestCase(RangeType.Melee, 2, false)]
    [TestCase(RangeType.Reach, 1, true)]
    [TestCase(RangeType.Reach, 2, true)]
    [TestCase(RangeType.Reach, 3, false)]
    public void IsInRange_VariousDistances_ReturnsExpected(RangeType rangeType, int distance, bool expected)
    {
        // Arrange
        var item = Item.CreateSword();
        item.SetRange(5, rangeType);

        // Act
        var isInRange = item.IsInRange(distance);

        // Assert
        isInRange.Should().Be(expected);
    }

    [TestCase(5, 5, true)]
    [TestCase(5, 6, false)]
    [TestCase(12, 10, true)]
    public void IsInRange_RangedType_UsesConfiguredRange(int weaponRange, int distance, bool expected)
    {
        // Arrange
        var item = Item.CreateSword();
        item.SetRange(weaponRange, RangeType.Ranged);

        // Act
        var isInRange = item.IsInRange(distance);

        // Assert
        isInRange.Should().Be(expected);
    }
}
