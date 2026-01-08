using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class PlayerTests
{
    [Test]
    public void Constructor_WithValidName_CreatesPlayer()
    {
        // Arrange & Act
        var player = new Player("TestHero");

        // Assert
        player.Name.Should().Be("TestHero");
        player.Health.Should().Be(player.Stats.MaxHealth);
        player.IsAlive.Should().BeTrue();
        player.Position.Should().Be(Position.Origin);
    }

    [Test]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new Player(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TakeDamage_WhenDamageLessThanHealth_ReducesHealth()
    {
        // Arrange
        var player = new Player("TestHero", new Stats(100, 10, 5));
        var initialHealth = player.Health;

        // Act
        var actualDamage = player.TakeDamage(20);

        // Assert
        actualDamage.Should().Be(15); // 20 - 5 defense
        player.Health.Should().Be(initialHealth - 15);
        player.IsAlive.Should().BeTrue();
    }

    [Test]
    public void TakeDamage_WhenDamageExceedsHealth_SetsHealthToZero()
    {
        // Arrange
        var player = new Player("TestHero", new Stats(50, 10, 0));

        // Act
        player.TakeDamage(100);

        // Assert
        player.Health.Should().Be(0);
        player.IsAlive.Should().BeFalse();
        player.IsDead.Should().BeTrue();
    }

    [Test]
    public void TakeDamage_WithNegativeDamage_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var act = () => player.TakeDamage(-5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Heal_WhenBelowMaxHealth_IncreasesHealth()
    {
        // Arrange
        var player = new Player("TestHero", new Stats(100, 10, 0));
        player.TakeDamage(50);
        var healthAfterDamage = player.Health;

        // Act
        var amountHealed = player.Heal(25);

        // Assert
        amountHealed.Should().Be(25);
        player.Health.Should().Be(healthAfterDamage + 25);
    }

    [Test]
    public void Heal_WhenAtMaxHealth_DoesNotExceedMax()
    {
        // Arrange
        var player = new Player("TestHero", new Stats(100, 10, 0));
        player.TakeDamage(10);

        // Act
        var amountHealed = player.Heal(50);

        // Assert
        amountHealed.Should().Be(10);
        player.Health.Should().Be(100);
    }

    [Test]
    public void TryPickUpItem_WhenInventoryHasSpace_ReturnsTrue()
    {
        // Arrange
        var player = new Player("TestHero");
        var item = Item.CreateSword();

        // Act
        var result = player.TryPickUpItem(item);

        // Assert
        result.Should().BeTrue();
        player.Inventory.Contains(item).Should().BeTrue();
    }

    [Test]
    public void MoveTo_UpdatesPlayerPosition()
    {
        // Arrange
        var player = new Player("TestHero");
        var newPosition = new Position(5, 10);

        // Act
        player.MoveTo(newPosition);

        // Assert
        player.Position.Should().Be(newPosition);
    }

    // ===== Equipment Tests (v0.0.7a) =====

    [Test]
    public void Constructor_InitializesEmptyEquipment()
    {
        // Arrange & Act
        var player = new Player("TestHero");

        // Assert
        player.Equipment.Should().NotBeNull();
        player.EquippedItemCount.Should().Be(0);
    }

    [Test]
    public void TryEquip_WithEquippableItem_ReturnsTrue()
    {
        // Arrange
        var player = new Player("TestHero");
        var sword = Item.CreateSword();

        // Act
        var result = player.TryEquip(sword);

        // Assert
        result.Should().BeTrue();
        player.IsSlotOccupied(EquipmentSlot.Weapon).Should().BeTrue();
        player.GetEquippedItem(EquipmentSlot.Weapon).Should().Be(sword);
        player.EquippedItemCount.Should().Be(1);
    }

    [Test]
    public void TryEquip_WithOccupiedSlot_ReturnsFalse()
    {
        // Arrange
        var player = new Player("TestHero");
        var sword1 = Item.CreateSword();
        var sword2 = Item.CreateSword();
        player.TryEquip(sword1);

        // Act
        var result = player.TryEquip(sword2);

        // Assert
        result.Should().BeFalse();
        player.GetEquippedItem(EquipmentSlot.Weapon).Should().Be(sword1);
    }

    [Test]
    public void TryEquip_WithNonEquippableItem_ThrowsInvalidOperationException()
    {
        // Arrange
        var player = new Player("TestHero");
        var potion = Item.CreateHealthPotion();

        // Act
        var act = () => player.TryEquip(potion);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Unequip_WithEquippedItem_ReturnsItem()
    {
        // Arrange
        var player = new Player("TestHero");
        var sword = Item.CreateSword();
        player.TryEquip(sword);

        // Act
        var result = player.Unequip(EquipmentSlot.Weapon);

        // Assert
        result.Should().Be(sword);
        player.IsSlotOccupied(EquipmentSlot.Weapon).Should().BeFalse();
        player.EquippedItemCount.Should().Be(0);
    }

    [Test]
    public void Unequip_WithEmptySlot_ReturnsNull()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = player.Unequip(EquipmentSlot.Weapon);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetAllEquippedItems_ReturnsAllEquippedItems()
    {
        // Arrange
        var player = new Player("TestHero");
        var sword = Item.CreateSword();
        var armor = Item.CreateLeatherArmor();
        player.TryEquip(sword);
        player.TryEquip(armor);

        // Act
        var items = player.GetAllEquippedItems();

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain(sword);
        items.Should().Contain(armor);
    }
}
