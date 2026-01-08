using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class ItemArmorTests
{
    [Test]
    public void CreateLeatherArmor_HasArmorProperties()
    {
        // Act
        var armor = Item.CreateLeatherArmor();

        // Assert
        armor.IsArmor.Should().BeTrue();
        armor.ArmorType.Should().Be(ArmorType.Light);
        armor.DefenseBonus.Should().Be(2);
        armor.EquipmentSlot.Should().Be(EquipmentSlot.Armor);
    }

    [Test]
    public void CreateChainMail_HasMediumArmorProperties()
    {
        // Act
        var armor = Item.CreateChainMail();

        // Assert
        armor.IsArmor.Should().BeTrue();
        armor.ArmorType.Should().Be(ArmorType.Medium);
        armor.DefenseBonus.Should().Be(4);
        armor.InitiativePenalty.Should().Be(-1);
        armor.HasRequirements.Should().BeTrue();
    }

    [Test]
    public void CreatePlateArmor_HasHeavyArmorProperties()
    {
        // Act
        var armor = Item.CreatePlateArmor();

        // Assert
        armor.IsArmor.Should().BeTrue();
        armor.ArmorType.Should().Be(ArmorType.Heavy);
        armor.DefenseBonus.Should().Be(6);
        armor.InitiativePenalty.Should().Be(-3);
        armor.HasRequirements.Should().BeTrue();
        armor.Requirements.MinFortitude.Should().Be(14);
        armor.Requirements.MinMight.Should().Be(12);
    }

    [Test]
    public void CreateRingOfStrength_HasStatModifiers()
    {
        // Act
        var ring = Item.CreateRingOfStrength();

        // Assert
        ring.IsArmor.Should().BeFalse();
        ring.StatModifiers.HasModifiers.Should().BeTrue();
        ring.StatModifiers.Might.Should().Be(2);
        ring.EquipmentSlot.Should().Be(EquipmentSlot.Ring);
    }

    [Test]
    public void CreateAmuletOfVitality_HasStatModifiers()
    {
        // Act
        var amulet = Item.CreateAmuletOfVitality();

        // Assert
        amulet.StatModifiers.HasModifiers.Should().BeTrue();
        amulet.StatModifiers.Fortitude.Should().Be(2);
        amulet.StatModifiers.MaxHealth.Should().Be(10);
        amulet.EquipmentSlot.Should().Be(EquipmentSlot.Amulet);
    }

    [Test]
    public void IsArmor_WithArmorItem_ReturnsTrue()
    {
        // Arrange
        var armor = Item.CreateLeatherArmor();

        // Assert
        armor.IsArmor.Should().BeTrue();
    }

    [Test]
    public void IsArmor_WithNonArmorItem_ReturnsFalse()
    {
        // Arrange
        var potion = Item.CreateHealthPotion();

        // Assert
        potion.IsArmor.Should().BeFalse();
    }

    [Test]
    public void HasRequirements_WithRequirements_ReturnsTrue()
    {
        // Arrange
        var chainMail = Item.CreateChainMail();

        // Assert
        chainMail.HasRequirements.Should().BeTrue();
    }

    [Test]
    public void HasRequirements_WithNoRequirements_ReturnsFalse()
    {
        // Arrange
        var leatherArmor = Item.CreateLeatherArmor();

        // Assert
        leatherArmor.HasRequirements.Should().BeFalse();
    }
}
