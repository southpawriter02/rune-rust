namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for CombatActionBarViewModel.
/// </summary>
[TestFixture]
public class CombatActionBarViewModelTests
{
    [Test]
    public void Constructor_InitializesWithPlayerTurnState()
    {
        // Arrange & Act
        var vm = new CombatActionBarViewModel();

        // Assert
        vm.IsPlayerTurn.Should().BeTrue();
        vm.HasAction.Should().BeTrue();
        vm.HasBonus.Should().BeTrue();
        vm.TurnText.Should().Be("YOUR TURN");
    }

    [Test]
    public void DefendCommand_ConsumesAction()
    {
        // Arrange
        var vm = new CombatActionBarViewModel();
        vm.HasAction.Should().BeTrue();

        // Act
        vm.DefendCommand.Execute(null);

        // Assert
        vm.HasAction.Should().BeFalse();
        vm.ActionIndicator.Should().Be("✗");
    }

    [Test]
    public void TurnText_ReturnsEnemyTurn_WhenNotPlayerTurn()
    {
        // Arrange
        var vm = new CombatActionBarViewModel();

        // Act
        vm.SetTurnState(isPlayerTurn: false, movement: 3, maxMovement: 4);

        // Assert
        vm.TurnText.Should().Be("Enemy Turn");
        vm.IsPlayerTurn.Should().BeFalse();
    }

    [Test]
    public void SetTurnState_UpdatesAllProperties()
    {
        // Arrange
        var vm = new CombatActionBarViewModel();
        vm.DefendCommand.Execute(null); // Consume action

        // Act
        vm.SetTurnState(isPlayerTurn: true, movement: 6, maxMovement: 6);

        // Assert
        vm.HasAction.Should().BeTrue();
        vm.HasBonus.Should().BeTrue();
        vm.RemainingMovement.Should().Be(6);
        vm.MaxMovement.Should().Be(6);
        vm.CanAttack.Should().BeTrue();
        vm.CanMove.Should().BeTrue();
    }

    [Test]
    public void SetRound_UpdatesCurrentRound()
    {
        // Arrange
        var vm = new CombatActionBarViewModel();

        // Act
        vm.SetRound(5);

        // Assert
        vm.CurrentRound.Should().Be(5);
    }

    [Test]
    public void CanMove_ReturnsFalse_WhenNoRemainingMovement()
    {
        // Arrange
        var vm = new CombatActionBarViewModel();

        // Act
        vm.SetTurnState(isPlayerTurn: true, movement: 0, maxMovement: 4);

        // Assert
        vm.CanMove.Should().BeFalse();
    }

    [Test]
    public void CanAttack_ReturnsFalse_WhenNotPlayerTurn()
    {
        // Arrange
        var vm = new CombatActionBarViewModel();

        // Act
        vm.SetTurnState(isPlayerTurn: false, movement: 4, maxMovement: 4);

        // Assert
        vm.CanAttack.Should().BeFalse();
    }
}
