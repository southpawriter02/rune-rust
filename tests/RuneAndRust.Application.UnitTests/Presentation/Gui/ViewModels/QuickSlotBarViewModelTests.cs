using Avalonia.Input;
using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="QuickSlotBarViewModel"/>.
/// </summary>
[TestFixture]
public class QuickSlotBarViewModelTests
{
    private QuickSlotBarViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new QuickSlotBarViewModel();
    }

    /// <summary>
    /// Verifies bar initializes with 8 slots.
    /// </summary>
    [Test]
    public void Constructor_Initializes8Slots()
    {
        // Assert
        _viewModel.Slots.Should().HaveCount(8);
        _viewModel.Slots.Select(s => s.SlotNumber).Should().BeEquivalentTo([1, 2, 3, 4, 5, 6, 7, 8]);
    }

    /// <summary>
    /// Verifies GetSlot returns correct slot.
    /// </summary>
    [Test]
    public void GetSlot_ValidNumber_ReturnsSlot()
    {
        // Act
        var slot = _viewModel.GetSlot(5);

        // Assert
        slot.Should().NotBeNull();
        slot!.SlotNumber.Should().Be(5);
    }

    /// <summary>
    /// Verifies UseSlot on empty slot does not throw.
    /// </summary>
    [Test]
    public void UseSlot_OnEmptySlot_DoesNotThrow()
    {
        // Act
        var act = () => _viewModel.UseSlot(1);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies ability assignment to slot.
    /// </summary>
    [Test]
    public void AssignAbilityToSlot_ValidSlot_AssignsAbility()
    {
        // Arrange
        var ability = AbilityDefinition.Create(
            "shield-bash",
            "Shield Bash",
            "Smash with your shield",
            ["warrior"],
            AbilityCost.None,
            cooldown: 2,
            [],
            AbilityTargetType.SingleEnemy);

        // Act
        _viewModel.AssignAbilityToSlot(1, ability);

        // Assert
        var slot = _viewModel.GetSlot(1);
        slot!.AssignedAbility.Should().Be(ability);
        slot.IsEmpty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies item assignment to slot.
    /// </summary>
    [Test]
    public void AssignItemToSlot_ValidSlot_AssignsItem()
    {
        // Act
        _viewModel.AssignItemToSlot(4, "potion-hp", "Health Potion", "❤️");

        // Assert
        var slot = _viewModel.GetSlot(4);
        slot!.AssignedItemId.Should().Be("potion-hp");
        slot.AssignedItemName.Should().Be("Health Potion");
    }

    /// <summary>
    /// Verifies keyboard D1 activates slot 1.
    /// </summary>
    [Test]
    public void HandleKeyPress_D1_ActivatesSlot1()
    {
        // Arrange
        var abilityUsed = false;
        var ability = AbilityDefinition.Create(
            "test",
            "Test",
            "Test ability",
            ["warrior"],
            AbilityCost.None,
            cooldown: 0,
            [],
            AbilityTargetType.Self);
        _viewModel.AssignAbilityToSlot(1, ability);
        _viewModel.AbilityUsed += _ => abilityUsed = true;

        // Act
        _viewModel.HandleKeyPress(Key.D1);

        // Assert
        abilityUsed.Should().BeTrue();
    }

    /// <summary>
    /// Verifies NumPad keys work for slots.
    /// </summary>
    [Test]
    public void HandleKeyPress_NumPad5_ActivatesSlot5()
    {
        // Arrange
        var itemUsed = false;
        _viewModel.AssignItemToSlot(5, "item-test", "Test Item", null);
        _viewModel.ItemUsed += _ => itemUsed = true;

        // Act
        _viewModel.HandleKeyPress(Key.NumPad5);

        // Assert
        itemUsed.Should().BeTrue();
    }

    /// <summary>
    /// Verifies invalid slot number is handled gracefully.
    /// </summary>
    [Test]
    public void UseSlot_InvalidNumber_DoesNotThrow()
    {
        // Act
        var act = () => _viewModel.UseSlot(99);

        // Assert
        act.Should().NotThrow();
    }
}
