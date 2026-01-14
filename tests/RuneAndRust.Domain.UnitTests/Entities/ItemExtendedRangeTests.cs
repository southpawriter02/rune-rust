using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Item extended range properties (v0.5.1b).
/// </summary>
[TestFixture]
public class ItemExtendedRangeTests
{
    private static Item CreateRangedWeapon(int range = 8, int minRange = 2, int? optimalRange = null)
    {
        var weapon = Item.CreateSword();
        weapon.SetRange(range, RangeType.Ranged);
        weapon.SetExtendedRangeProperties(
            minRange, optimalRange, rangePenalty: 1,
            thrown: false, twoHanded: true, meleeCapable: false, requiresAmmunition: true);
        return weapon;
    }

    // ===== Extended Properties Tests =====

    [Test]
    public void Item_DefaultMinRange_Is0()
    {
        // Arrange & Act
        var item = Item.CreateSword();

        // Assert
        item.MinRange.Should().Be(0);
        item.HasMinRange.Should().BeFalse();
    }

    [Test]
    public void SetExtendedRangeProperties_ValidValues_SetsProperties()
    {
        // Arrange
        var weapon = Item.CreateSword();

        // Act
        weapon.SetExtendedRangeProperties(
            minRange: 2, optimalRange: 6, rangePenalty: 1,
            thrown: true, twoHanded: false, meleeCapable: true, requiresAmmunition: false);

        // Assert
        weapon.MinRange.Should().Be(2);
        weapon.OptimalRange.Should().Be(6);
        weapon.RangePenalty.Should().Be(1);
        weapon.Thrown.Should().BeTrue();
        weapon.TwoHanded.Should().BeFalse();
        weapon.MeleeCapable.Should().BeTrue();
        weapon.RequiresAmmunition.Should().BeFalse();
    }

    [Test]
    public void SetExtendedRangeProperties_NegativeMinRange_ThrowsException()
    {
        // Arrange
        var weapon = Item.CreateSword();

        // Act
        var act = () => weapon.SetExtendedRangeProperties(
            minRange: -1, optimalRange: null, rangePenalty: 1,
            thrown: false, twoHanded: false, meleeCapable: false, requiresAmmunition: false);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void HasMinRange_WhenMinRangeSet_ReturnsTrue()
    {
        // Arrange
        var weapon = CreateRangedWeapon(minRange: 2);

        // Assert
        weapon.HasMinRange.Should().BeTrue();
    }

    // ===== GetOptimalRange Tests =====

    [Test]
    public void GetOptimalRange_WhenSet_ReturnsConfiguredValue()
    {
        // Arrange
        var weapon = CreateRangedWeapon(range: 12, optimalRange: 6);

        // Act
        var optimal = weapon.GetOptimalRange();

        // Assert
        optimal.Should().Be(6);
    }

    [Test]
    public void GetOptimalRange_WhenNull_ReturnsHalfOfRange()
    {
        // Arrange
        var weapon = CreateRangedWeapon(range: 12, optimalRange: null);

        // Act
        var optimal = weapon.GetOptimalRange();

        // Assert
        optimal.Should().Be(6); // 12 / 2
    }

    // ===== GetPenaltyAtDistance Tests =====

    [Test]
    public void GetPenaltyAtDistance_WithinOptimal_ReturnsZero()
    {
        // Arrange - range 12, optimal 6
        var weapon = CreateRangedWeapon(range: 12, optimalRange: 6);

        // Act
        var penalty = weapon.GetPenaltyAtDistance(5);

        // Assert
        penalty.Should().Be(0);
    }

    [Test]
    public void GetPenaltyAtDistance_BeyondOptimal_ReturnsPenalty()
    {
        // Arrange - range 12, optimal 6, penalty 1 per cell
        var weapon = CreateRangedWeapon(range: 12, optimalRange: 6);

        // Act - distance 8 = 2 cells beyond optimal
        var penalty = weapon.GetPenaltyAtDistance(8);

        // Assert
        penalty.Should().Be(2);
    }

    [Test]
    public void GetPenaltyAtDistance_TooClose_ReturnsMaxValue()
    {
        // Arrange - minRange 2
        var weapon = CreateRangedWeapon(minRange: 2);

        // Act
        var penalty = weapon.GetPenaltyAtDistance(1);

        // Assert
        penalty.Should().Be(int.MaxValue);
    }

    [Test]
    public void GetPenaltyAtDistance_OutOfRange_ReturnsMaxValue()
    {
        // Arrange - range 8
        var weapon = CreateRangedWeapon(range: 8);

        // Act
        var penalty = weapon.GetPenaltyAtDistance(10);

        // Assert
        penalty.Should().Be(int.MaxValue);
    }

    [Test]
    public void GetPenaltyAtDistance_MeleeWeapon_AlwaysZero()
    {
        // Arrange
        var weapon = Item.CreateSword(); // Melee

        // Act
        var penalty = weapon.GetPenaltyAtDistance(5);

        // Assert
        penalty.Should().Be(0);
    }
}
