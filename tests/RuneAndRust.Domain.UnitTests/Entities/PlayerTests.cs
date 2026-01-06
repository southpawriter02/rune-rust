using FluentAssertions;
using RuneAndRust.Domain.Entities;
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
}
