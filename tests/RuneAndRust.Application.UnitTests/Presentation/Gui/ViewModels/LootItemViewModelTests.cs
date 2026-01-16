using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="LootItemViewModel"/>.
/// </summary>
[TestFixture]
public class LootItemViewModelTests
{
    /// <summary>
    /// Verifies single item display without quantity.
    /// </summary>
    [Test]
    public void DisplayText_SingleItem_DoesNotShowQuantity()
    {
        // Arrange
        var loot = new LootItemViewModel("sword-001", "Iron Sword");

        // Assert
        loot.DisplayText.Should().Be("• Iron Sword");
        loot.Quantity.Should().Be(1);
    }

    /// <summary>
    /// Verifies multiple items show quantity.
    /// </summary>
    [Test]
    public void DisplayText_MultipleItems_ShowsQuantity()
    {
        // Arrange
        var loot = new LootItemViewModel("bone-frag", "Bone Fragment", quantity: 3);

        // Assert
        loot.DisplayText.Should().Be("• Bone Fragment (x3)");
        loot.Quantity.Should().Be(3);
    }
}
