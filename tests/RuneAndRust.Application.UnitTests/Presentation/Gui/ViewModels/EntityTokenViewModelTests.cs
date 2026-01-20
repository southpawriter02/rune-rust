namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Tests for <see cref="EntityTokenViewModel"/>.
/// </summary>
[TestFixture]
public class EntityTokenViewModelTests
{
    [Test]
    public void Constructor_WithDesignTimeParameters_SetsProperties()
    {
        // Arrange & Act
        var vm = new EntityTokenViewModel("Skeleton", "S", "#FF4500", 2, 3, 40);

        // Assert
        vm.Symbol.Should().Be("S");
        vm.Color.Should().Be("#FF4500");
        vm.CanvasX.Should().Be(80); // 2 * 40
        vm.CanvasY.Should().Be(120); // 3 * 40
    }

    [Test]
    public void Symbol_WhenCombatantNull_UsesDesignSymbol()
    {
        // Arrange
        var vm = new EntityTokenViewModel("Hero", "@", "#32CD32", 0, 0, 40);

        // Act & Assert
        vm.Symbol.Should().Be("@");
        vm.Combatant.Should().BeNull();
    }

    [Test]
    public void IsCurrentTurn_CanBeSet()
    {
        // Arrange
        var vm = new EntityTokenViewModel("Test", "T", "#FFFFFF", 0, 0, 40);

        // Act
        vm.IsCurrentTurn = true;

        // Assert
        vm.IsCurrentTurn.Should().BeTrue();
    }

    [Test]
    public void IsSelected_CanBeSet()
    {
        // Arrange
        var vm = new EntityTokenViewModel("Test", "T", "#FFFFFF", 0, 0, 40);

        // Act
        vm.IsSelected = true;

        // Assert
        vm.IsSelected.Should().BeTrue();
    }

    [Test]
    public void DefaultConstructor_SetsReasonableDefaults()
    {
        // Arrange & Act
        var vm = new EntityTokenViewModel();

        // Assert
        vm.CanvasX.Should().Be(0);
        vm.CanvasY.Should().Be(0);
        vm.Symbol.Should().Be("@");
        vm.Color.Should().Be("#32CD32");
    }
}
