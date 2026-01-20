namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.ViewModels.Wizard;

[TestFixture]
public class WizardViewModelTests
{
    // ============================================================================
    // Name Validation Tests
    // ============================================================================

    [Test]
    public void NameEntry_Validate_ReturnsFalse_WhenNameTooShort()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new NameEntryStepViewModel(data);
        vm.CharacterName = "A";

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeFalse();
        vm.ValidationMessage.Should().Contain("2 characters");
    }

    [Test]
    public void NameEntry_Validate_ReturnsFalse_WhenNameTooLong()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new NameEntryStepViewModel(data);
        vm.CharacterName = "ThisNameIsWayTooLongForTheValidator";

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeFalse();
        vm.ValidationMessage.Should().Contain("20 characters");
    }

    [Test]
    public void NameEntry_Validate_ReturnsTrue_WhenNameValid()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new NameEntryStepViewModel(data);
        vm.CharacterName = "Thorin";

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeTrue();
        data.Name.Should().Be("Thorin");
    }

    // ============================================================================
    // Race Selection Tests
    // ============================================================================

    [Test]
    public void RaceSelection_Validate_ReturnsFalse_WhenNoRaceSelected()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new RaceSelectionStepViewModel(data);
        vm.SelectedRace = null;

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeFalse();
        vm.ValidationMessage.Should().Contain("select a race");
    }

    [Test]
    public void RaceSelection_Validate_ReturnsTrue_WhenRaceSelected()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new RaceSelectionStepViewModel(data);
        vm.SelectedRace = vm.Races[0];

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeTrue();
        data.RaceId.Should().NotBeNullOrEmpty();
    }

    // ============================================================================
    // Class Selection Tests
    // ============================================================================

    [Test]
    public void ClassSelection_Validate_ReturnsFalse_WhenNoClassSelected()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new ClassSelectionStepViewModel(data);
        vm.SelectedClass = null;

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeFalse();
        vm.ValidationMessage.Should().Contain("select a class");
    }

    [Test]
    public void ClassSelection_Validate_ReturnsTrue_WhenClassSelected()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new ClassSelectionStepViewModel(data);
        vm.SelectedClass = vm.Classes[0];

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeTrue();
        data.ClassId.Should().NotBeNullOrEmpty();
    }

    // ============================================================================
    // Stat Allocation Tests
    // ============================================================================

    [Test]
    public void StatAllocation_RecalculatePoints_UpdatesPointsRemaining()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new StatAllocationStepViewModel(data);

        // Act - all stats at 10, point cost is 2 each, 6 stats = 12 points spent
        vm.RecalculatePoints();

        // Assert
        vm.PointsRemaining.Should().Be(27 - 12); // 15 remaining
    }

    [Test]
    public void StatAllocation_IncrementStat_ReducesPoints()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new StatAllocationStepViewModel(data);
        var initialPoints = vm.PointsRemaining;
        var stat = vm.Stats[0];

        // Act
        stat.IncrementCommand.Execute(null);

        // Assert
        vm.PointsRemaining.Should().BeLessThan(initialPoints);
    }

    [Test]
    public void StatAllocation_Validate_ReturnsTrue_WhenPointsNotOverspent()
    {
        // Arrange
        var data = new CharacterCreationData();
        var vm = new StatAllocationStepViewModel(data);

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeTrue();
    }

    // ============================================================================
    // Navigation Tests
    // ============================================================================

    [Test]
    public void WizardViewModel_GoNext_AdvancesToNextStep()
    {
        // Arrange
        var vm = new WizardViewModel();
        var nameStep = (NameEntryStepViewModel)vm.CurrentStepViewModel;
        nameStep.CharacterName = "TestHero";

        // Act
        vm.GoNextCommand.Execute(null);

        // Assert
        vm.CurrentStep.Should().Be(2);
        vm.CurrentStepViewModel.Should().BeOfType<RaceSelectionStepViewModel>();
    }

    [Test]
    public void WizardViewModel_GoBack_ReturnsToPreviousStep()
    {
        // Arrange
        var vm = new WizardViewModel();
        var nameStep = (NameEntryStepViewModel)vm.CurrentStepViewModel;
        nameStep.CharacterName = "TestHero";
        vm.GoNextCommand.Execute(null);

        // Act
        vm.GoBackCommand.Execute(null);

        // Assert
        vm.CurrentStep.Should().Be(1);
        vm.CurrentStepViewModel.Should().BeOfType<NameEntryStepViewModel>();
    }
}
