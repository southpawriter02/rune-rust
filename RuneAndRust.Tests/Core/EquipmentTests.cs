using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Equipment entity.
/// Validates equipment-specific properties and inheritance from Item.
/// </summary>
public class EquipmentTests
{
    #region Inheritance Tests

    [Fact]
    public void Equipment_InheritsFromItem()
    {
        // Arrange & Act
        var equipment = new Equipment();

        // Assert
        equipment.Should().BeAssignableTo<Item>();
    }

    [Fact]
    public void Equipment_HasItemProperties()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Iron Helm",
            Description = "A sturdy iron helmet.",
            Weight = 2000,
            Value = 150,
            Quality = QualityTier.ClanForged,
            ItemType = ItemType.Armor
        };

        // Assert
        equipment.Name.Should().Be("Iron Helm");
        equipment.Description.Should().Be("A sturdy iron helmet.");
        equipment.Weight.Should().Be(2000);
        equipment.Value.Should().Be(150);
        equipment.Quality.Should().Be(QualityTier.ClanForged);
        equipment.ItemType.Should().Be(ItemType.Armor);
    }

    #endregion

    #region Slot Tests

    [Fact]
    public void Equipment_Slot_DefaultsToMainHand()
    {
        // Arrange & Act
        var equipment = new Equipment();

        // Assert
        equipment.Slot.Should().Be(EquipmentSlot.MainHand);
    }

    [Theory]
    [InlineData(EquipmentSlot.MainHand)]
    [InlineData(EquipmentSlot.OffHand)]
    [InlineData(EquipmentSlot.Head)]
    [InlineData(EquipmentSlot.Body)]
    [InlineData(EquipmentSlot.Hands)]
    [InlineData(EquipmentSlot.Feet)]
    [InlineData(EquipmentSlot.Accessory)]
    public void Equipment_Slot_CanBeSetToAllSlots(EquipmentSlot slot)
    {
        // Arrange
        var equipment = new Equipment();

        // Act
        equipment.Slot = slot;

        // Assert
        equipment.Slot.Should().Be(slot);
    }

    #endregion

    #region Attribute Bonuses Tests

    [Fact]
    public void Equipment_AttributeBonuses_DefaultsToEmptyDictionary()
    {
        // Arrange & Act
        var equipment = new Equipment();

        // Assert
        equipment.AttributeBonuses.Should().NotBeNull();
        equipment.AttributeBonuses.Should().BeEmpty();
    }

    [Fact]
    public void Equipment_AttributeBonuses_CanAddBonus()
    {
        // Arrange
        var equipment = new Equipment();

        // Act
        equipment.AttributeBonuses[CharacterAttribute.Might] = 2;

        // Assert
        equipment.AttributeBonuses.Should().ContainKey(CharacterAttribute.Might);
        equipment.AttributeBonuses[CharacterAttribute.Might].Should().Be(2);
    }

    [Fact]
    public void Equipment_GetAttributeBonus_ReturnsZeroForMissingAttribute()
    {
        // Arrange
        var equipment = new Equipment();

        // Act
        var bonus = equipment.GetAttributeBonus(CharacterAttribute.Sturdiness);

        // Assert
        bonus.Should().Be(0);
    }

    [Fact]
    public void Equipment_GetAttributeBonus_ReturnsCorrectValue()
    {
        // Arrange
        var equipment = new Equipment();
        equipment.AttributeBonuses[CharacterAttribute.Finesse] = 3;

        // Act
        var bonus = equipment.GetAttributeBonus(CharacterAttribute.Finesse);

        // Assert
        bonus.Should().Be(3);
    }

    [Fact]
    public void Equipment_CanHaveMultipleAttributeBonuses()
    {
        // Arrange
        var equipment = new Equipment();
        equipment.AttributeBonuses[CharacterAttribute.Might] = 1;
        equipment.AttributeBonuses[CharacterAttribute.Sturdiness] = 2;
        equipment.AttributeBonuses[CharacterAttribute.Finesse] = -1;

        // Assert
        equipment.GetAttributeBonus(CharacterAttribute.Might).Should().Be(1);
        equipment.GetAttributeBonus(CharacterAttribute.Sturdiness).Should().Be(2);
        equipment.GetAttributeBonus(CharacterAttribute.Finesse).Should().Be(-1);
    }

    #endregion

    #region Combat Properties Tests

    [Fact]
    public void Equipment_SoakBonus_DefaultsToZero()
    {
        // Arrange & Act
        var equipment = new Equipment();

        // Assert
        equipment.SoakBonus.Should().Be(0);
    }

    [Fact]
    public void Equipment_SoakBonus_CanBeSet()
    {
        // Arrange
        var equipment = new Equipment();

        // Act
        equipment.SoakBonus = 5;

        // Assert
        equipment.SoakBonus.Should().Be(5);
    }

    [Fact]
    public void Equipment_DamageDie_DefaultsToZero()
    {
        // Arrange & Act
        var equipment = new Equipment();

        // Assert
        equipment.DamageDie.Should().Be(0);
    }

    [Fact]
    public void Equipment_DamageDie_CanBeSetForWeapons()
    {
        // Arrange
        var weapon = new Equipment
        {
            ItemType = ItemType.Weapon,
            DamageDie = 8 // d8
        };

        // Assert
        weapon.DamageDie.Should().Be(8);
    }

    #endregion

    #region Requirements Tests

    [Fact]
    public void Equipment_Requirements_DefaultsToEmptyDictionary()
    {
        // Arrange & Act
        var equipment = new Equipment();

        // Assert
        equipment.Requirements.Should().NotBeNull();
        equipment.Requirements.Should().BeEmpty();
    }

    [Fact]
    public void Equipment_MeetsRequirements_ReturnsTrueWhenNoRequirements()
    {
        // Arrange
        var equipment = new Equipment();
        var character = new Character { Might = 5 };

        // Act
        var result = equipment.MeetsRequirements(character);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equipment_MeetsRequirements_ReturnsTrueWhenCharacterMeetsAll()
    {
        // Arrange
        var equipment = new Equipment();
        equipment.Requirements[CharacterAttribute.Might] = 6;
        equipment.Requirements[CharacterAttribute.Sturdiness] = 4;

        var character = new Character { Might = 7, Sturdiness = 5 };

        // Act
        var result = equipment.MeetsRequirements(character);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equipment_MeetsRequirements_ReturnsFalseWhenCharacterFailsOne()
    {
        // Arrange
        var equipment = new Equipment();
        equipment.Requirements[CharacterAttribute.Might] = 8;

        var character = new Character { Might = 5 };

        // Act
        var result = equipment.MeetsRequirements(character);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equipment_MeetsRequirements_ReturnsTrueWhenExactlyMeetsRequirement()
    {
        // Arrange
        var equipment = new Equipment();
        equipment.Requirements[CharacterAttribute.Finesse] = 7;

        var character = new Character { Finesse = 7 };

        // Act
        var result = equipment.MeetsRequirements(character);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Full Equipment Creation Tests

    [Fact]
    public void Equipment_CanCreateFullyConfiguredWeapon()
    {
        // Arrange & Act
        var sword = new Equipment
        {
            Name = "Frost-Etched Blade",
            Description = "A blade marked with frost runes.",
            DetailedDescription = "Ancient runes pulse with cold light.",
            ItemType = ItemType.Weapon,
            Slot = EquipmentSlot.MainHand,
            Quality = QualityTier.ClanForged,
            Weight = 1800,
            Value = 500,
            DamageDie = 8,
            AttributeBonuses = { [CharacterAttribute.Might] = 1 },
            Requirements = { [CharacterAttribute.Might] = 4 }
        };

        // Assert
        sword.Name.Should().Be("Frost-Etched Blade");
        sword.ItemType.Should().Be(ItemType.Weapon);
        sword.Slot.Should().Be(EquipmentSlot.MainHand);
        sword.DamageDie.Should().Be(8);
        sword.GetAttributeBonus(CharacterAttribute.Might).Should().Be(1);
    }

    [Fact]
    public void Equipment_CanCreateFullyConfiguredArmor()
    {
        // Arrange & Act
        var armor = new Equipment
        {
            Name = "Scavenged Plate",
            Description = "Dented but serviceable plate armor.",
            ItemType = ItemType.Armor,
            Slot = EquipmentSlot.Body,
            Quality = QualityTier.Scavenged,
            Weight = 12000,
            Value = 300,
            SoakBonus = 3,
            AttributeBonuses = { [CharacterAttribute.Finesse] = -1 },
            Requirements = { [CharacterAttribute.Sturdiness] = 5, [CharacterAttribute.Might] = 4 }
        };

        // Assert
        armor.Name.Should().Be("Scavenged Plate");
        armor.ItemType.Should().Be(ItemType.Armor);
        armor.Slot.Should().Be(EquipmentSlot.Body);
        armor.SoakBonus.Should().Be(3);
        armor.GetAttributeBonus(CharacterAttribute.Finesse).Should().Be(-1);
        armor.Requirements.Should().HaveCount(2);
    }

    #endregion
}
