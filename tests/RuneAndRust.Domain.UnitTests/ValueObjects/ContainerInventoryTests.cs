using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the ContainerInventory value object.
/// </summary>
[TestFixture]
public class ContainerInventoryTests
{
    #region Create Tests

    [Test]
    public void Create_WithValidCapacity_ReturnsContainer()
    {
        // Act
        var container = ContainerInventory.Create(10);

        // Assert
        container.Should().NotBeNull();
        container.Capacity.Should().Be(10);
        container.Count.Should().Be(0);
        container.IsEmpty.Should().BeTrue();
        container.IsFull.Should().BeFalse();
    }

    [Test]
    public void Create_WithZeroCapacity_ThrowsException()
    {
        // Act
        var act = () => ContainerInventory.Create(0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Capacity must be positive*");
    }

    [Test]
    public void Create_WithNegativeCapacity_ThrowsException()
    {
        // Act
        var act = () => ContainerInventory.Create(-5);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Empty_ReturnsEmptyContainerWithDefaultCapacity()
    {
        // Act
        var container = ContainerInventory.Empty();

        // Assert
        container.Capacity.Should().Be(10);
        container.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region TryAddItem Tests

    [Test]
    public void TryAddItem_WhenNotFull_ReturnsTrue()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        var item = Item.CreateHealthPotion();

        // Act
        var result = container.TryAddItem(item);

        // Assert
        result.Should().BeTrue();
        container.Count.Should().Be(1);
        container.Items.Should().Contain(item);
    }

    [Test]
    public void TryAddItem_WhenFull_ReturnsFalse()
    {
        // Arrange
        var container = ContainerInventory.Create(1);
        container.TryAddItem(Item.CreateHealthPotion());

        // Act
        var result = container.TryAddItem(Item.CreateScroll());

        // Assert
        result.Should().BeFalse();
        container.Count.Should().Be(1);
    }

    [Test]
    public void TryAddItem_WithNullItem_ReturnsFalse()
    {
        // Arrange
        var container = ContainerInventory.Create(5);

        // Act
        var result = container.TryAddItem(null!);

        // Assert
        result.Should().BeFalse();
        container.Count.Should().Be(0);
    }

    #endregion

    #region RemoveItem Tests

    [Test]
    public void RemoveItem_ExistingItem_ReturnsTrue()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        var item = Item.CreateHealthPotion();
        container.TryAddItem(item);

        // Act
        var result = container.RemoveItem(item);

        // Assert
        result.Should().BeTrue();
        container.Count.Should().Be(0);
        container.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void RemoveItem_NonExistingItem_ReturnsFalse()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        container.TryAddItem(Item.CreateHealthPotion());
        var otherItem = Item.CreateScroll();

        // Act
        var result = container.RemoveItem(otherItem);

        // Assert
        result.Should().BeFalse();
        container.Count.Should().Be(1);
    }

    #endregion

    #region GetItemByName Tests

    [Test]
    public void GetItemByName_CaseInsensitive_ReturnsItem()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        var potion = Item.CreateHealthPotion();
        container.TryAddItem(potion);

        // Act
        var result = container.GetItemByName("HEALTH POTION");

        // Assert
        result.Should().Be(potion);
    }

    [Test]
    public void GetItemByName_NotFound_ReturnsNull()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        container.TryAddItem(Item.CreateHealthPotion());

        // Act
        var result = container.GetItemByName("Sword");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetItemByPartialName Tests

    [Test]
    public void GetItemByPartialName_MatchesPartial_ReturnsItem()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        var potion = Item.CreateHealthPotion();
        container.TryAddItem(potion);

        // Act
        var result = container.GetItemByPartialName("Health");

        // Assert
        result.Should().Be(potion);
    }

    #endregion

    #region TakeAll Tests

    [Test]
    public void TakeAll_ReturnsAllItemsAndClears()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        var item1 = Item.CreateHealthPotion();
        var item2 = Item.CreateScroll();
        container.TryAddItem(item1);
        container.TryAddItem(item2);

        // Act
        var result = container.TakeAll().ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(item1);
        result.Should().Contain(item2);
        container.IsEmpty.Should().BeTrue();
        container.Count.Should().Be(0);
    }

    #endregion

    #region State Property Tests

    [Test]
    public void IsEmpty_WhenEmpty_ReturnsTrue()
    {
        // Arrange
        var container = ContainerInventory.Create(5);

        // Assert
        container.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void IsFull_WhenAtCapacity_ReturnsTrue()
    {
        // Arrange
        var container = ContainerInventory.Create(2);
        container.TryAddItem(Item.CreateHealthPotion());
        container.TryAddItem(Item.CreateScroll());

        // Assert
        container.IsFull.Should().BeTrue();
        container.RemainingCapacity.Should().Be(0);
    }

    [Test]
    public void RemainingCapacity_ReturnsCorrectValue()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        container.TryAddItem(Item.CreateHealthPotion());
        container.TryAddItem(Item.CreateScroll());

        // Assert
        container.RemainingCapacity.Should().Be(3);
    }

    #endregion

    #region GetContentsDescription Tests

    [Test]
    public void GetContentsDescription_WhenEmpty_ReturnsEmptyMessage()
    {
        // Arrange
        var container = ContainerInventory.Create(5);

        // Act
        var result = container.GetContentsDescription();

        // Assert
        result.Should().Be("The container is empty.");
    }

    [Test]
    public void GetContentsDescription_WithItems_ReturnsFormattedList()
    {
        // Arrange
        var container = ContainerInventory.Create(5);
        container.TryAddItem(Item.CreateHealthPotion());

        // Act
        var result = container.GetContentsDescription();

        // Assert
        result.Should().Contain("Inside you find:");
        result.Should().Contain("Health Potion");
    }

    #endregion
}
