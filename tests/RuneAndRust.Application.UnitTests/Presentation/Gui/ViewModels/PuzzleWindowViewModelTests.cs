namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.ViewModels;

[TestFixture]
public class PuzzleWindowViewModelTests
{
    private Puzzle CreateLeverPuzzle() => new(
        Name: "Test Lever Puzzle",
        Instructions: "Toggle the levers.",
        Type: PuzzleType.Lever,
        Elements: [new PuzzleElement("l1", "A"), new PuzzleElement("l2", "B"), new PuzzleElement("l3", "C")],
        Solution: "101",
        MaxAttempts: 3,
        Hints: ["First hint", "Second hint"]);

    // ============================================================================
    // Load and Reset Tests
    // ============================================================================

    [Test]
    public void LoadPuzzle_SetsPropertiesCorrectly()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        var puzzle = CreateLeverPuzzle();

        // Act
        vm.LoadPuzzle(puzzle);

        // Assert
        vm.Title.Should().Be("Test Lever Puzzle");
        vm.Instructions.Should().Be("Toggle the levers.");
        vm.PuzzleType.Should().Be(PuzzleType.Lever);
        vm.Elements.Should().HaveCount(3);
        vm.AttemptsRemaining.Should().Be(3);
        vm.HintsRemaining.Should().Be(2);
    }

    [Test]
    public void Reset_ClearsAllElementStates()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());
        vm.ToggleElementCommand.Execute(vm.Elements[0]);

        // Act
        vm.ResetCommand.Execute(null);

        // Assert
        vm.Elements.All(e => !e.IsToggled).Should().BeTrue();
        vm.CurrentInput.Should().BeEmpty();
    }

    // ============================================================================
    // Toggle (Lever) Tests
    // ============================================================================

    [Test]
    public void ToggleElement_LeverPuzzle_TogglesState()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());
        var element = vm.Elements[0];

        // Act
        vm.ToggleElementCommand.Execute(element);

        // Assert
        element.IsToggled.Should().BeTrue();
    }

    [Test]
    public void ToggleElement_UpdatesCurrentInput()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());

        // Act
        vm.ToggleElementCommand.Execute(vm.Elements[0]);
        vm.ToggleElementCommand.Execute(vm.Elements[2]);

        // Assert
        vm.CurrentInput.Should().Contain("↓");
    }

    // ============================================================================
    // Solve Tests
    // ============================================================================

    [Test]
    public void Solve_CorrectSolution_SetsSolved()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());
        vm.ToggleElementCommand.Execute(vm.Elements[0]); // 1
        vm.ToggleElementCommand.Execute(vm.Elements[2]); // 1

        // Act
        vm.SolveCommand.Execute(null);

        // Assert
        vm.IsSolved.Should().BeTrue();
        vm.ResultMessage.Should().Contain("SOLVED");
    }

    [Test]
    public void Solve_IncorrectSolution_DecrementsAttempts()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());

        // Act
        vm.SolveCommand.Execute(null); // Wrong (all up)

        // Assert
        vm.AttemptsRemaining.Should().Be(2);
        vm.IsSolved.Should().BeFalse();
    }

    [Test]
    public void Solve_OutOfAttempts_SetsFailed()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());

        // Act
        vm.SolveCommand.Execute(null); // Attempt 1
        vm.SolveCommand.Execute(null); // Attempt 2
        vm.SolveCommand.Execute(null); // Attempt 3

        // Assert
        vm.HasFailed.Should().BeTrue();
        vm.ResultMessage.Should().Contain("FAILED");
    }

    // ============================================================================
    // Hint Tests
    // ============================================================================

    [Test]
    public void UseHint_RevealsHintAndDecrements()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());

        // Act
        vm.UseHintCommand.Execute(null);

        // Assert
        vm.CurrentHint.Should().Be("First hint");
        vm.HintsRemaining.Should().Be(1);
    }

    [Test]
    public void UseHint_NoHintsRemaining_DoesNothing()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());
        vm.UseHintCommand.Execute(null);
        vm.UseHintCommand.Execute(null);

        // Act
        vm.UseHintCommand.Execute(null);

        // Assert
        vm.HintsRemaining.Should().Be(0);
    }

    // ============================================================================
    // Give Up Tests
    // ============================================================================

    [Test]
    public void GiveUp_SetsFailed()
    {
        // Arrange
        var vm = new PuzzleWindowViewModel();
        vm.LoadPuzzle(CreateLeverPuzzle());

        // Act
        vm.GiveUpCommand.Execute(null);

        // Assert
        vm.HasFailed.Should().BeTrue();
        vm.ResultMessage.Should().Contain("gave up");
    }
}
