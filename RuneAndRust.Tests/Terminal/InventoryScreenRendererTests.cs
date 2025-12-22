using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Services;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the InventoryScreenRenderer class (v0.3.7a).
/// Note: These tests focus on renderer construction and input handling.
/// Full visual rendering tests are done via manual integration testing.
/// </summary>
public class InventoryScreenRendererTests
{
    private readonly Mock<ILogger<InventoryScreenRenderer>> _mockLogger;
    private readonly InventoryScreenRenderer _sut;

    public InventoryScreenRendererTests()
    {
        _mockLogger = new Mock<ILogger<InventoryScreenRenderer>>();
        _sut = new InventoryScreenRenderer(_mockLogger.Object);
    }

    [Fact]
    public void Render_WithEmptyBackpack_DoesNotThrow()
    {
        // Arrange
        var vm = CreateEmptyViewModel();

        // Act
        var action = () => _sut.Render(vm);

        // Assert - Should not throw even with empty inventory
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithItems_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithItems();

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithBrokenEquipment_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithBrokenEquipment();

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithSelectedItem_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithItems() with { SelectedIndex = 1 };

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    #region Helper Methods

    private static InventoryViewModel CreateEmptyViewModel()
    {
        var equippedItems = new Dictionary<EquipmentSlot, EquippedItemView?>();
        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            equippedItems[slot] = null;
        }

        return new InventoryViewModel(
            CharacterName: "Test Hero",
            EquippedItems: equippedItems,
            BackpackItems: new List<BackpackItemView>(),
            CurrentWeight: 0,
            MaxCapacity: 50000,
            BurdenPercentage: 0,
            BurdenState: BurdenState.Light,
            SelectedIndex: 0
        );
    }

    private static InventoryViewModel CreateViewModelWithItems()
    {
        var equippedItems = new Dictionary<EquipmentSlot, EquippedItemView?>();
        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            equippedItems[slot] = null;
        }
        equippedItems[EquipmentSlot.MainHand] = new EquippedItemView(
            Name: "Iron Sword",
            Quality: QualityTier.ClanForged,
            DurabilityPercentage: 85,
            IsBroken: false
        );

        var backpackItems = new List<BackpackItemView>
        {
            new BackpackItemView(
                Index: 1,
                Name: "Health Potion",
                Quantity: 3,
                Quality: QualityTier.Scavenged,
                WeightGrams: 300,
                ItemType: ItemType.Consumable,
                IsEquipable: false
            ),
            new BackpackItemView(
                Index: 2,
                Name: "Iron Dagger",
                Quantity: 1,
                Quality: QualityTier.ClanForged,
                WeightGrams: 500,
                ItemType: ItemType.Weapon,
                IsEquipable: true
            )
        };

        return new InventoryViewModel(
            CharacterName: "Test Hero",
            EquippedItems: equippedItems,
            BackpackItems: backpackItems,
            CurrentWeight: 2300,
            MaxCapacity: 50000,
            BurdenPercentage: 4,
            BurdenState: BurdenState.Light,
            SelectedIndex: 0
        );
    }

    private static InventoryViewModel CreateViewModelWithBrokenEquipment()
    {
        var equippedItems = new Dictionary<EquipmentSlot, EquippedItemView?>();
        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            equippedItems[slot] = null;
        }
        equippedItems[EquipmentSlot.Body] = new EquippedItemView(
            Name: "Broken Leather Armor",
            Quality: QualityTier.JuryRigged,
            DurabilityPercentage: 0,
            IsBroken: true
        );

        return new InventoryViewModel(
            CharacterName: "Test Hero",
            EquippedItems: equippedItems,
            BackpackItems: new List<BackpackItemView>(),
            CurrentWeight: 3000,
            MaxCapacity: 50000,
            BurdenPercentage: 6,
            BurdenState: BurdenState.Light,
            SelectedIndex: 0
        );
    }

    #endregion
}
