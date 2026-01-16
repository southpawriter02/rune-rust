using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="QuickSlotViewModel"/>.
/// </summary>
[TestFixture]
public class QuickSlotViewModelTests
{
    /// <summary>
    /// Verifies new slot is empty by default.
    /// </summary>
    [Test]
    public void Constructor_NewSlot_IsEmpty()
    {
        // Arrange & Act
        var slot = new QuickSlotViewModel(1);

        // Assert
        slot.IsEmpty.Should().BeTrue();
        slot.CanUse.Should().BeFalse();
        slot.SlotNumber.Should().Be(1);
    }

    /// <summary>
    /// Verifies ability assignment sets properties correctly.
    /// </summary>
    [Test]
    public void AssignAbility_ValidAbility_SetsProperties()
    {
        // Arrange
        var slot = new QuickSlotViewModel(3);
        var ability = AbilityDefinition.Create(
            "flame-bolt",
            "Flame Bolt",
            "Hurls a bolt of fire",
            ["mage"],
            AbilityCost.None,
            cooldown: 0,
            [],
            AbilityTargetType.SingleEnemy);

        // Act
        slot.AssignAbility(ability);

        // Assert
        slot.IsEmpty.Should().BeFalse();
        slot.CanUse.Should().BeTrue();
        slot.Name.Should().Be("Flame Bolt");
        slot.AssignedAbility.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies item assignment clears ability.
    /// </summary>
    [Test]
    public void AssignItem_AfterAbility_ClearsAbility()
    {
        // Arrange
        var slot = new QuickSlotViewModel(2);
        var ability = AbilityDefinition.Create(
            "heal",
            "Heal",
            "Restores health",
            ["cleric"],
            AbilityCost.None,
            cooldown: 0,
            [],
            AbilityTargetType.Self);
        slot.AssignAbility(ability);

        // Act
        slot.AssignItem("potion-001", "Health Potion", "🧪");

        // Assert
        slot.AssignedAbility.Should().BeNull();
        slot.AssignedItemId.Should().Be("potion-001");
        slot.Name.Should().Be("Health Potion");
    }

    /// <summary>
    /// Verifies clear removes all assignments.
    /// </summary>
    [Test]
    public void Clear_AfterAssignment_IsEmpty()
    {
        // Arrange
        var slot = new QuickSlotViewModel(5);
        slot.AssignItem("scroll-001", "Scroll of Fire", "📜");

        // Act
        slot.Clear();

        // Assert
        slot.IsEmpty.Should().BeTrue();
        slot.CanUse.Should().BeFalse();
    }

    /// <summary>
    /// Verifies cooldown prevents use.
    /// </summary>
    [Test]
    public void CanUse_WhenOnCooldown_ReturnsFalse()
    {
        // Arrange
        var slot = new QuickSlotViewModel(1);
        slot.AssignItem("item-001", "Item", null);
        slot.IsOnCooldown = true;

        // Assert
        slot.CanUse.Should().BeFalse();
    }
}
