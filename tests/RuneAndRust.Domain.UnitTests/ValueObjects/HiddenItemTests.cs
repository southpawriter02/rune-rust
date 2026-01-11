using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the HiddenItem value object.
/// </summary>
[TestFixture]
public class HiddenItemTests
{
    [Test]
    public void Create_CreatesNewHiddenItemWithGeneratedId()
    {
        // Arrange
        var item = new Item("Ancient Key", "An ancient key covered in rust.", ItemType.Quest);

        // Act
        var hiddenItem = HiddenItem.Create(item, 15, "glint of metal");

        // Assert
        hiddenItem.Id.Should().NotBe(Guid.Empty);
        hiddenItem.Item.Should().Be(item);
        hiddenItem.DiscoveryDC.Should().Be(15);
        hiddenItem.Hint.Should().Be("glint of metal");
        hiddenItem.IsDiscovered.Should().BeFalse();
    }

    [Test]
    public void AsDiscovered_ReturnsNewInstanceWithIsDiscoveredTrue()
    {
        // Arrange
        var item = new Item("Rusty Key", "A key covered in rust.", ItemType.Quest);
        var hiddenItem = HiddenItem.Create(item, 12);

        // Act
        var discovered = hiddenItem.AsDiscovered();

        // Assert
        discovered.IsDiscovered.Should().BeTrue();
        hiddenItem.IsDiscovered.Should().BeFalse(); // Original unchanged
        discovered.Id.Should().Be(hiddenItem.Id);
        discovered.Item.Should().Be(item);
    }
}
