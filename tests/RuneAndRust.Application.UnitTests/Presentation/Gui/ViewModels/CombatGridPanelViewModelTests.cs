namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Tests for <see cref="CombatGridPanelViewModel"/>.
/// </summary>
[TestFixture]
public class CombatGridPanelViewModelTests
{
    [Test]
    public void Constructor_InitializesSampleGrid()
    {
        // Arrange & Act
        var vm = new CombatGridPanelViewModel();

        // Assert
        vm.GridWidth.Should().Be(8);
        vm.GridHeight.Should().Be(8);
        vm.FlatCells.Should().HaveCount(64); // 8x8
        vm.IsInCombat.Should().BeTrue();
    }

    [Test]
    public void Constructor_PopulatesCoordinateLabels()
    {
        // Arrange & Act
        var vm = new CombatGridPanelViewModel();

        // Assert
        vm.RowLabels.Should().HaveCount(8);
        vm.RowLabels[0].Should().Be("A");
        vm.RowLabels[7].Should().Be("H");
        vm.ColumnLabels.Should().HaveCount(8);
        vm.ColumnLabels[0].Should().Be(1);
        vm.ColumnLabels[7].Should().Be(8);
    }

    [Test]
    public void Constructor_PopulatesLegendItems()
    {
        // Arrange & Act
        var vm = new CombatGridPanelViewModel();

        // Assert
        vm.LegendItems.Should().HaveCountGreaterThan(0);
        vm.LegendItems.Should().Contain(item => item.Symbol == "@" && item.Label == "You");
    }

    [Test]
    public void Constructor_CreatesSampleTokens()
    {
        // Arrange & Act
        var vm = new CombatGridPanelViewModel();

        // Assert
        vm.Tokens.Should().HaveCount(3);
        vm.Tokens.Should().Contain(t => t.Symbol == "@"); // Hero token
    }

    [Test]
    public void SetCombatState_WhenFalse_ClearsGrid()
    {
        // Arrange
        var vm = new CombatGridPanelViewModel();
        vm.FlatCells.Should().HaveCountGreaterThan(0);
        vm.Tokens.Should().HaveCountGreaterThan(0);

        // Act
        vm.SetCombatState(false);

        // Assert
        vm.IsInCombat.Should().BeFalse();
        vm.FlatCells.Should().BeEmpty();
        vm.Tokens.Should().BeEmpty();
    }

    [Test]
    public void SetCombatState_WhenTrue_KeepsExistingData()
    {
        // Arrange
        var vm = new CombatGridPanelViewModel();
        var initialCellCount = vm.FlatCells.Count;

        // Act
        vm.SetCombatState(true);

        // Assert
        vm.IsInCombat.Should().BeTrue();
        vm.FlatCells.Should().HaveCount(initialCellCount);
    }
}
