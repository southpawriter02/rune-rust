using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Services;

[TestFixture]
public class CombatServiceWeaponTests
{
    private CombatService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new CombatService(NullLogger<CombatService>.Instance);
    }

    private static Player CreateTestPlayer()
    {
        return new Player("TestHero");
    }

    [Test]
    public void GetPlayerDamageDice_WithWeapon_ReturnsWeaponDice()
    {
        // Arrange
        var player = CreateTestPlayer();
        var sword = Item.CreateSword();
        player.TryEquip(sword);

        // Act
        var dice = _service.GetPlayerDamageDice(player);

        // Assert
        dice.Faces.Should().Be(8); // 1d8 for sword
        dice.Count.Should().Be(1);
    }

    [Test]
    public void GetPlayerDamageDice_Unarmed_ReturnsD4()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var dice = _service.GetPlayerDamageDice(player);

        // Assert
        dice.Should().Be(CombatService.UnarmedDamageDice);
        dice.Faces.Should().Be(4);
    }

    [Test]
    public void GetPlayerDamageDice_WithBattleAxe_Returns1d10()
    {
        // Arrange
        var player = CreateTestPlayer();
        var axe = Item.CreateBattleAxe();
        player.TryEquip(axe);

        // Act
        var dice = _service.GetPlayerDamageDice(player);

        // Assert
        dice.Faces.Should().Be(10); // 1d10 for battle axe
    }

    [Test]
    public void GetPlayerWeaponName_WithWeapon_ReturnsName()
    {
        // Arrange
        var player = CreateTestPlayer();
        var sword = Item.CreateSword();
        player.TryEquip(sword);

        // Act
        var name = _service.GetPlayerWeaponName(player);

        // Assert
        name.Should().Be("Rusty Sword");
    }

    [Test]
    public void GetPlayerWeaponName_Unarmed_ReturnsFists()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var name = _service.GetPlayerWeaponName(player);

        // Assert
        name.Should().Be("fists");
    }

    [Test]
    public void GetWeaponAttackModifier_WithModifier_ReturnsValue()
    {
        // Arrange
        var player = CreateTestPlayer();
        var axe = Item.CreateBattleAxe();
        player.TryEquip(axe);

        // Act
        var modifier = _service.GetWeaponAttackModifier(player);

        // Assert
        modifier.Should().Be(-1);
    }

    [Test]
    public void GetWeaponAttackModifier_NoWeapon_ReturnsZero()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var modifier = _service.GetWeaponAttackModifier(player);

        // Assert
        modifier.Should().Be(0);
    }

    [Test]
    public void GetWeaponBonuses_WithDagger_ReturnsFinesseBonus()
    {
        // Arrange
        var player = CreateTestPlayer();
        var dagger = Item.CreateSteelDagger();
        player.TryEquip(dagger);

        // Act
        var bonuses = _service.GetWeaponBonuses(player);

        // Assert
        bonuses.HasBonuses.Should().BeTrue();
        bonuses.Finesse.Should().Be(2);
    }

    [Test]
    public void GetWeaponBonuses_NoWeapon_ReturnsNone()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var bonuses = _service.GetWeaponBonuses(player);

        // Assert
        bonuses.Should().Be(WeaponBonuses.None);
        bonuses.HasBonuses.Should().BeFalse();
    }

    [Test]
    public void UnarmedDamageDice_IsD4()
    {
        // Assert
        CombatService.UnarmedDamageDice.Faces.Should().Be(4);
        CombatService.UnarmedDamageDice.Count.Should().Be(1);
    }

    [Test]
    public void DefaultWeaponDice_IsD6()
    {
        // Assert
        CombatService.DefaultWeaponDice.Faces.Should().Be(6);
        CombatService.DefaultWeaponDice.Count.Should().Be(1);
    }
}
