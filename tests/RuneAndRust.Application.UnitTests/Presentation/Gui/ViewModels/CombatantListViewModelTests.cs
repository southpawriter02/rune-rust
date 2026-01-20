namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for CombatantListViewModel.
/// </summary>
[TestFixture]
public class CombatantListViewModelTests
{
    [Test]
    public void Constructor_AddsSampleCombatants_ForDesignTime()
    {
        // Arrange & Act
        var vm = new CombatantListViewModel();

        // Assert
        vm.Combatants.Should().HaveCountGreaterThan(0);
        vm.Combatants.Should().Contain(c => c.Name == "Hero");
    }

    [Test]
    public void CombatantEntry_DisplaysCorrectTurnIndicator_WhenCurrentTurn()
    {
        // Arrange
        var vm = new CombatantListViewModel();
        var heroEntry = vm.Combatants.First(c => c.Name == "Hero");

        // Assert - Hero is current turn in sample data
        heroEntry.IsCurrentTurn.Should().BeTrue();
        heroEntry.TurnIndicator.Should().Be("►");
    }

    [Test]
    public void CombatantEntry_DisplaysEmptyTurnIndicator_WhenNotCurrentTurn()
    {
        // Arrange
        var vm = new CombatantListViewModel();
        var skeletonEntry = vm.Combatants.First(c => c.Name == "Skeleton");

        // Assert
        skeletonEntry.IsCurrentTurn.Should().BeFalse();
        skeletonEntry.TurnIndicator.Should().Be(" ");
    }

    [Test]
    public void Clear_RemovesAllCombatants()
    {
        // Arrange
        var vm = new CombatantListViewModel();
        vm.Combatants.Should().HaveCountGreaterThan(0);

        // Act
        vm.Clear();

        // Assert
        vm.Combatants.Should().BeEmpty();
        vm.CurrentTurnEntityId.Should().Be(Guid.Empty);
    }
}
