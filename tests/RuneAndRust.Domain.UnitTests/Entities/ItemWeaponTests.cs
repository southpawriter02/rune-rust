using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class ItemWeaponTests
{
    [Test]
    public void CreateSword_HasWeaponProperties()
    {
        // Act
        var sword = Item.CreateSword();

        // Assert
        sword.IsWeapon.Should().BeTrue();
        sword.WeaponType.Should().Be(WeaponType.Sword);
        sword.DamageDice.Should().Be("1d8");
        sword.EquipmentSlot.Should().Be(EquipmentSlot.Weapon);
    }

    [Test]
    public void CreateBattleAxe_HasAttackPenalty()
    {
        // Act
        var axe = Item.CreateBattleAxe();

        // Assert
        axe.IsWeapon.Should().BeTrue();
        axe.WeaponType.Should().Be(WeaponType.Axe);
        axe.DamageDice.Should().Be("1d10");
        axe.WeaponBonuses.AttackModifier.Should().Be(-1);
    }

    [Test]
    public void CreateSteelDagger_HasFinesseBonus()
    {
        // Act
        var dagger = Item.CreateSteelDagger();

        // Assert
        dagger.IsWeapon.Should().BeTrue();
        dagger.WeaponType.Should().Be(WeaponType.Dagger);
        dagger.DamageDice.Should().Be("1d4");
        dagger.WeaponBonuses.Finesse.Should().Be(2);
    }

    [Test]
    public void CreateOakStaff_HasWillBonus()
    {
        // Act
        var staff = Item.CreateOakStaff();

        // Assert
        staff.IsWeapon.Should().BeTrue();
        staff.WeaponType.Should().Be(WeaponType.Staff);
        staff.DamageDice.Should().Be("1d6");
        staff.WeaponBonuses.Will.Should().Be(2);
    }

    [Test]
    public void IsWeapon_WithWeaponItem_ReturnsTrue()
    {
        // Arrange
        var sword = Item.CreateSword();

        // Assert
        sword.IsWeapon.Should().BeTrue();
    }

    [Test]
    public void IsWeapon_WithNonWeaponItem_ReturnsFalse()
    {
        // Arrange
        var potion = Item.CreateHealthPotion();

        // Assert
        potion.IsWeapon.Should().BeFalse();
    }

    [Test]
    public void GetDamageDicePool_WithWeapon_ReturnsValidPool()
    {
        // Arrange
        var sword = Item.CreateSword();

        // Act
        var dicePool = sword.GetDamageDicePool();

        // Assert
        dicePool.Should().NotBeNull();
        dicePool!.Value.Count.Should().Be(1);
        dicePool.Value.Faces.Should().Be(8);
    }

    [Test]
    public void GetDamageDicePool_WithNonWeapon_ReturnsNull()
    {
        // Arrange
        var potion = Item.CreateHealthPotion();

        // Act
        var dicePool = potion.GetDamageDicePool();

        // Assert
        dicePool.Should().BeNull();
    }
}
