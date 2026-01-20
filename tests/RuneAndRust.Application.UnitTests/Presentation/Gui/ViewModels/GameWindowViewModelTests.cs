using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="GameWindowViewModel"/>.
/// </summary>
[TestFixture]
public class GameWindowViewModelTests
{
    private GameWindowViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        // Use parameter-less constructor for testing
        _viewModel = new GameWindowViewModel();
    }

    /// <summary>
    /// Verifies that WindowTitle has the expected default value.
    /// </summary>
    [Test]
    public void WindowTitle_DefaultValue_IsRuneAndRust()
    {
        // Assert
        _viewModel.WindowTitle.Should().Be("Rune and Rust");
    }

    /// <summary>
    /// Verifies that SetCombatMode sets the IsInCombat property.
    /// </summary>
    [Test]
    public void SetCombatMode_SetsIsInCombat()
    {
        // Act
        _viewModel.SetCombatMode(true);

        // Assert
        _viewModel.IsInCombat.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SetCombatMode can disable combat mode.
    /// </summary>
    [Test]
    public void SetCombatMode_ToFalse_ClearsIsInCombat()
    {
        // Arrange
        _viewModel.SetCombatMode(true);

        // Act
        _viewModel.SetCombatMode(false);

        // Assert
        _viewModel.IsInCombat.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsInCombat is false by default.
    /// </summary>
    [Test]
    public void IsInCombat_DefaultValue_IsFalse()
    {
        // Assert
        _viewModel.IsInCombat.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that NewGameCommand can execute.
    /// </summary>
    [Test]
    public void NewGameCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.NewGameCommand.CanExecute(null).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SaveGameCommand can execute.
    /// </summary>
    [Test]
    public void SaveGameCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.SaveGameCommand.CanExecute(null).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that LoadGameCommand can execute.
    /// </summary>
    [Test]
    public void LoadGameCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.LoadGameCommand.CanExecute(null).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ReturnToMainMenuCommand can execute.
    /// </summary>
    [Test]
    public void ReturnToMainMenuCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.ReturnToMainMenuCommand.CanExecute(null).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that QuitCommand can execute.
    /// </summary>
    [Test]
    public void QuitCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.QuitCommand.CanExecute(null).Should().BeTrue();
    }
}
