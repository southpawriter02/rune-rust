using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class PlayerEffectiveStatsTests
{
    [Test]
    public void GetEffectiveStats_WithArmor_IncludesDefenseBonus()
    {
        // Arrange
        var player = new Player("TestHero");
        var armor = Item.CreateLeatherArmor(); // DefenseBonus = 2
        player.TryEquip(armor);
        var baseDefense = player.Stats.Defense;

        // Act
        var effectiveStats = player.GetEffectiveStats();

        // Assert
        effectiveStats.Defense.Should().Be(baseDefense + 2);
    }

    [Test]
    public void GetEffectiveStats_WithMultipleArmor_StacksDefenseBonuses()
    {
        // Arrange
        var player = new Player("TestHero");
        var armor = Item.CreateLeatherArmor(); // DefenseBonus = 2
        var helmet = Item.CreateIronHelmet(); // DefenseBonus = 2
        var shield = Item.CreateWoodenShield(); // DefenseBonus = 1
        player.TryEquip(armor);
        player.TryEquip(helmet);
        player.TryEquip(shield);
        var baseDefense = player.Stats.Defense;

        // Act
        var effectiveStats = player.GetEffectiveStats();

        // Assert
        effectiveStats.Defense.Should().Be(baseDefense + 5); // 2 + 2 + 1
    }

    [Test]
    public void GetEffectiveStats_WithStatModifiers_IncludesMaxHealthBonus()
    {
        // Arrange
        var player = new Player("TestHero");
        var amulet = Item.CreateAmuletOfVitality(); // MaxHealth = 10
        player.TryEquip(amulet);
        var baseMaxHealth = player.Stats.MaxHealth;

        // Act
        var effectiveStats = player.GetEffectiveStats();

        // Assert
        effectiveStats.MaxHealth.Should().Be(baseMaxHealth + 10);
    }

    [Test]
    public void GetEffectiveAttributes_WithStatModifiers_IncludesBonuses()
    {
        // Arrange
        var player = new Player("TestHero");
        var ring = Item.CreateRingOfStrength(); // Might = 2
        player.TryEquip(ring);
        var baseMight = player.Attributes.Might;

        // Act
        var effectiveAttributes = player.GetEffectiveAttributes();

        // Assert
        effectiveAttributes.Might.Should().Be(baseMight + 2);
    }

    [Test]
    public void GetEffectiveAttributes_WithWeaponBonuses_IncludesBonuses()
    {
        // Arrange
        var player = new Player("TestHero");
        var dagger = Item.CreateSteelDagger(); // Finesse = 2
        player.TryEquip(dagger);
        var baseFinesse = player.Attributes.Finesse;

        // Act
        var effectiveAttributes = player.GetEffectiveAttributes();

        // Assert
        effectiveAttributes.Finesse.Should().Be(baseFinesse + 2);
    }

    [Test]
    public void GetTotalInitiativePenalty_SumsAllPenalties()
    {
        // Arrange
        var player = new Player(
            "TestHero",
            "human",
            "soldier",
            new PlayerAttributes(14, 14, 10, 10, 10) // High stats to meet requirements
        );
        var chainMail = Item.CreateChainMail(); // InitiativePenalty = -1
        player.TryEquip(chainMail);

        // Act
        var penalty = player.GetTotalInitiativePenalty();

        // Assert
        penalty.Should().Be(-1);
    }

    [Test]
    public void GetTotalDefenseBonus_SumsDefenseFromAllSources()
    {
        // Arrange
        var player = new Player("TestHero");
        var armor = Item.CreateLeatherArmor(); // DefenseBonus = 2
        player.TryEquip(armor);

        // Act
        var bonus = player.GetTotalDefenseBonus();

        // Assert
        bonus.Should().Be(2);
    }

    [Test]
    public void GetEffectiveStats_NoEquipment_ReturnsBaseStats()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var effectiveStats = player.GetEffectiveStats();

        // Assert
        effectiveStats.Defense.Should().Be(player.Stats.Defense);
        effectiveStats.MaxHealth.Should().Be(player.Stats.MaxHealth);
        effectiveStats.Attack.Should().Be(player.Stats.Attack);
    }
}
