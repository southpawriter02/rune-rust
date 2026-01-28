using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for DroppedItem tier-related functionality (v0.16.0d).
/// </summary>
[TestFixture]
public class DroppedItemTierTests
{
    // ═══════════════════════════════════════════════════════════════
    // CreateWeapon TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CreateWeapon_WithTierAndBonus_CreatesCorrectItem()
    {
        // Arrange & Act
        var weapon = DroppedItem.CreateWeapon(
            "iron-sword",
            "Iron Sword",
            QualityTier.ClanForged,
            attributeBonus: 2,
            bonusAttribute: "Might");

        // Assert
        weapon.ItemId.Should().Be("iron-sword");
        weapon.Name.Should().Be("Iron Sword");
        weapon.QualityTier.Should().Be(QualityTier.ClanForged);
        weapon.AttributeBonus.Should().Be(2);
        weapon.BonusAttribute.Should().Be("Might");
        weapon.Quantity.Should().Be(1);
    }

    [Test]
    public void CreateWeapon_WithoutBonus_HasNullBonusProperties()
    {
        // Arrange & Act
        var weapon = DroppedItem.CreateWeapon(
            "rusty-dagger",
            "Rusty Dagger",
            QualityTier.JuryRigged);

        // Assert
        weapon.QualityTier.Should().Be(QualityTier.JuryRigged);
        weapon.AttributeBonus.Should().BeNull();
        weapon.BonusAttribute.Should().BeNull();
        weapon.HasAttributeBonus.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // CreateArmor TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CreateArmor_WithTierAndBonus_CreatesCorrectItem()
    {
        // Arrange & Act
        var armor = DroppedItem.CreateArmor(
            "steel-plate",
            "Steel Plate",
            QualityTier.Optimized,
            attributeBonus: 3,
            bonusAttribute: "Vigor");

        // Assert
        armor.ItemId.Should().Be("steel-plate");
        armor.Name.Should().Be("Steel Plate");
        armor.QualityTier.Should().Be(QualityTier.Optimized);
        armor.AttributeBonus.Should().Be(3);
        armor.BonusAttribute.Should().Be("Vigor");
    }

    // ═══════════════════════════════════════════════════════════════
    // FormattedName TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormattedName_WithBonus_IncludesTierAndBonusSuffix()
    {
        // Arrange
        var weapon = DroppedItem.CreateWeapon(
            "iron-sword",
            "Iron Sword",
            QualityTier.ClanForged,
            attributeBonus: 2,
            bonusAttribute: "Might");

        // Act
        var formattedName = weapon.FormattedName;

        // Assert
        formattedName.Should().Be("[Clan-Forged] Iron Sword +2 MIG");
    }

    [Test]
    public void FormattedName_WithoutBonus_HasTierPrefixOnly()
    {
        // Arrange
        var weapon = DroppedItem.CreateWeapon(
            "rusty-dagger",
            "Rusty Dagger",
            QualityTier.JuryRigged);

        // Act
        var formattedName = weapon.FormattedName;

        // Assert
        formattedName.Should().Be("[Jury-Rigged] Rusty Dagger");
    }

    [Test]
    public void FormattedName_MythForged_ShowsCorrectTierName()
    {
        // Arrange
        var legendary = DroppedItem.CreateWeapon(
            "ragnarok-blade",
            "Ragnarok Blade",
            QualityTier.MythForged,
            attributeBonus: 5,
            bonusAttribute: "Finesse");

        // Act
        var formattedName = legendary.FormattedName;

        // Assert
        formattedName.Should().Be("[Myth-Forged] Ragnarok Blade +5 FIN");
    }

    // ═══════════════════════════════════════════════════════════════
    // HasAttributeBonus TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void HasAttributeBonus_WithValidBonus_ReturnsTrue()
    {
        // Arrange
        var weapon = DroppedItem.CreateWeapon(
            "sword",
            "Sword",
            QualityTier.ClanForged,
            attributeBonus: 1,
            bonusAttribute: "Might");

        // Act & Assert
        weapon.HasAttributeBonus.Should().BeTrue();
    }

    [Test]
    public void HasAttributeBonus_WithZeroBonus_ReturnsFalse()
    {
        // Arrange
        var weapon = DroppedItem.CreateWeapon(
            "sword",
            "Sword",
            QualityTier.ClanForged,
            attributeBonus: 0,
            bonusAttribute: "Might");

        // Act & Assert
        weapon.HasAttributeBonus.Should().BeFalse();
    }

    [Test]
    public void HasAttributeBonus_WithNullAttribute_ReturnsFalse()
    {
        // Arrange
        var weapon = DroppedItem.CreateWeapon(
            "sword",
            "Sword",
            QualityTier.ClanForged,
            attributeBonus: 2,
            bonusAttribute: null);

        // Act & Assert
        weapon.HasAttributeBonus.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // IsLegendary TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsLegendary_MythForgedTier_ReturnsTrue()
    {
        // Arrange
        var legendary = DroppedItem.CreateWeapon(
            "epic-sword",
            "Epic Sword",
            QualityTier.MythForged);

        // Act & Assert
        legendary.IsLegendary.Should().BeTrue();
    }

    [Test]
    public void IsLegendary_NonMythForgedTier_ReturnsFalse()
    {
        // Arrange
        var common = DroppedItem.CreateWeapon(
            "common-sword",
            "Common Sword",
            QualityTier.Scavenged);

        // Act & Assert
        common.IsLegendary.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // TierValue TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(QualityTier.JuryRigged, 0)]
    [TestCase(QualityTier.Scavenged, 1)]
    [TestCase(QualityTier.ClanForged, 2)]
    [TestCase(QualityTier.Optimized, 3)]
    [TestCase(QualityTier.MythForged, 4)]
    public void TierValue_ForEachTier_ReturnsCorrectNumericValue(QualityTier tier, int expected)
    {
        // Arrange
        var item = DroppedItem.CreateWeapon("test", "Test", tier);

        // Act & Assert
        item.TierValue.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ToString_SingleQuantity_OmitsQuantityPrefix()
    {
        // Arrange
        var item = DroppedItem.CreateWeapon("sword", "Iron Sword", QualityTier.Scavenged);

        // Act
        var result = item.ToString();

        // Assert
        result.Should().Be("[Scavenged] Iron Sword");
    }

    [Test]
    public void ToString_MultipleQuantity_IncludesQuantityPrefix()
    {
        // Arrange
        var item = DroppedItem.Create("potion", "Health Potion", quantity: 3);

        // Act
        var result = item.ToString();

        // Assert
        result.Should().Be("3x [Scavenged] Health Potion");
    }
}
