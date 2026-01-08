using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class WeaponBonusesTests
{
    [Test]
    public void None_HasNoBonuses()
    {
        // Act
        var bonuses = WeaponBonuses.None;

        // Assert
        bonuses.HasBonuses.Should().BeFalse();
        bonuses.Might.Should().Be(0);
        bonuses.Fortitude.Should().Be(0);
        bonuses.Will.Should().Be(0);
        bonuses.Wits.Should().Be(0);
        bonuses.Finesse.Should().Be(0);
        bonuses.AttackModifier.Should().Be(0);
    }

    [Test]
    public void ForFinesse_CreatesFinesseBonus()
    {
        // Act
        var bonuses = WeaponBonuses.ForFinesse(2);

        // Assert
        bonuses.HasBonuses.Should().BeTrue();
        bonuses.Finesse.Should().Be(2);
        bonuses.Might.Should().Be(0);
        bonuses.Will.Should().Be(0);
        bonuses.AttackModifier.Should().Be(0);
    }

    [Test]
    public void ForWill_CreatesWillBonus()
    {
        // Act
        var bonuses = WeaponBonuses.ForWill(3);

        // Assert
        bonuses.HasBonuses.Should().BeTrue();
        bonuses.Will.Should().Be(3);
        bonuses.Finesse.Should().Be(0);
        bonuses.Might.Should().Be(0);
    }

    [Test]
    public void ForAttack_CreatesAttackModifier()
    {
        // Act
        var bonuses = WeaponBonuses.ForAttack(-1);

        // Assert
        bonuses.HasBonuses.Should().BeTrue();
        bonuses.AttackModifier.Should().Be(-1);
        bonuses.Finesse.Should().Be(0);
        bonuses.Might.Should().Be(0);
    }

    [Test]
    public void HasBonuses_WithNonZeroValue_ReturnsTrue()
    {
        // Arrange
        var bonuses = new WeaponBonuses(Might: 1);

        // Assert
        bonuses.HasBonuses.Should().BeTrue();
    }

    [Test]
    public void HasBonuses_WithAllZeros_ReturnsFalse()
    {
        // Arrange
        var bonuses = new WeaponBonuses();

        // Assert
        bonuses.HasBonuses.Should().BeFalse();
    }

    [Test]
    public void ToString_WithBonuses_FormatsCorrectly()
    {
        // Arrange
        var bonuses = new WeaponBonuses(Might: 2, Finesse: -1, AttackModifier: 1);

        // Act
        var result = bonuses.ToString();

        // Assert
        result.Should().Contain("+2 Might");
        result.Should().Contain("-1 Finesse");
        result.Should().Contain("+1 Attack");
    }

    [Test]
    public void ToString_WithNoBonuses_ReturnsNone()
    {
        // Arrange
        var bonuses = WeaponBonuses.None;

        // Act
        var result = bonuses.ToString();

        // Assert
        result.Should().Be("None");
    }
}
