using Avalonia.Input;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="CommandInputViewModel"/>.
/// </summary>
[TestFixture]
public class CommandInputViewModelTests
{
    private Mock<ICommandHistoryService> _historyMock = null!;
    private Mock<ITabCompletionService> _completionMock = null!;
    private CommandInputViewModel _viewModel = null!;

    [SetUp]
    public void Setup()
    {
        _historyMock = new Mock<ICommandHistoryService>();
        _completionMock = new Mock<ITabCompletionService>();

        _viewModel = new CommandInputViewModel(
            _historyMock.Object,
            _completionMock.Object);
    }

    /// <summary>
    /// Verifies that pressing Enter raises the CommandSubmitted event.
    /// </summary>
    [Test]
    public void HandleKeyDown_Enter_RaisesCommandSubmitted()
    {
        // Arrange
        _viewModel.InputText = "go north";
        string? submittedCommand = null;
        _viewModel.CommandSubmitted += cmd => submittedCommand = cmd;

        // Act
        _viewModel.HandleKeyDown(Key.Enter);

        // Assert
        submittedCommand.Should().Be("go north");
        _viewModel.InputText.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that submitting adds to history.
    /// </summary>
    [Test]
    public void HandleKeyDown_Enter_AddsToHistory()
    {
        // Arrange
        _viewModel.InputText = "attack";

        // Act
        _viewModel.HandleKeyDown(Key.Enter);

        // Assert
        _historyMock.Verify(x => x.Add("attack"), Times.Once);
    }

    /// <summary>
    /// Verifies that empty input does not submit.
    /// </summary>
    [Test]
    public void HandleKeyDown_Enter_WithEmptyInput_DoesNotSubmit()
    {
        // Arrange
        _viewModel.InputText = "";
        string? submittedCommand = null;
        _viewModel.CommandSubmitted += cmd => submittedCommand = cmd;

        // Act
        _viewModel.HandleKeyDown(Key.Enter);

        // Assert
        submittedCommand.Should().BeNull();
        _historyMock.Verify(x => x.Add(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that pressing Up navigates history back.
    /// </summary>
    [Test]
    public void HandleKeyDown_Up_NavigatesHistoryBack()
    {
        // Arrange
        _historyMock.Setup(x => x.GetPrevious()).Returns("previous command");

        // Act
        _viewModel.HandleKeyDown(Key.Up);

        // Assert
        _viewModel.InputText.Should().Be("previous command");
        _viewModel.IsNavigatingHistory.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Tab triggers completion.
    /// </summary>
    [Test]
    public void HandleKeyDown_Tab_SingleMatch_AppliesInline()
    {
        // Arrange
        _viewModel.InputText = "att";
        _completionMock.Setup(x => x.GetCompletions(It.IsAny<string>(), It.IsAny<CompletionContext>()))
            .Returns(new List<string> { "attack" });

        // Act
        _viewModel.HandleKeyDown(Key.Tab);

        // Assert
        _viewModel.InputText.Should().Be("attack ");
        _viewModel.IsCompletionVisible.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Tab shows popup for multiple matches.
    /// </summary>
    [Test]
    public void HandleKeyDown_Tab_MultipleMatches_ShowsPopup()
    {
        // Arrange
        _viewModel.InputText = "a";
        _completionMock.Setup(x => x.GetCompletions(It.IsAny<string>(), It.IsAny<CompletionContext>()))
            .Returns(new List<string> { "attack", "armor", "axe" });

        // Act
        _viewModel.HandleKeyDown(Key.Tab);

        // Assert
        _viewModel.IsCompletionVisible.Should().BeTrue();
        _viewModel.Suggestions.Should().HaveCount(3);
        _viewModel.SelectedSuggestionIndex.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Escape hides the popup.
    /// </summary>
    [Test]
    public void HandleKeyDown_Escape_WhenPopupVisible_HidesPopup()
    {
        // Arrange
        _viewModel.InputText = "a";
        _completionMock.Setup(x => x.GetCompletions(It.IsAny<string>(), It.IsAny<CompletionContext>()))
            .Returns(new List<string> { "attack", "armor" });
        _viewModel.HandleKeyDown(Key.Tab); // Open popup

        // Act
        var handled = _viewModel.HandleKeyDown(Key.Escape);

        // Assert
        handled.Should().BeTrue();
        _viewModel.IsCompletionVisible.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Up/Down navigates suggestions when popup is visible.
    /// </summary>
    [Test]
    public void HandleKeyDown_UpDown_WhenPopupVisible_NavigatesSuggestions()
    {
        // Arrange
        _viewModel.InputText = "a";
        _completionMock.Setup(x => x.GetCompletions(It.IsAny<string>(), It.IsAny<CompletionContext>()))
            .Returns(new List<string> { "attack", "armor", "axe" });
        _viewModel.HandleKeyDown(Key.Tab);

        // Act - Move down
        _viewModel.HandleKeyDown(Key.Down);

        // Assert
        _viewModel.SelectedSuggestionIndex.Should().Be(1);

        // Act - Move up
        _viewModel.HandleKeyDown(Key.Up);

        // Assert
        _viewModel.SelectedSuggestionIndex.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Enter accepts selected suggestion.
    /// </summary>
    [Test]
    public void HandleKeyDown_Enter_WhenPopupVisible_AcceptsSuggestion()
    {
        // Arrange
        _viewModel.InputText = "a";
        _completionMock.Setup(x => x.GetCompletions(It.IsAny<string>(), It.IsAny<CompletionContext>()))
            .Returns(new List<string> { "attack", "armor" });
        _viewModel.HandleKeyDown(Key.Tab);

        // Act
        _viewModel.HandleKeyDown(Key.Enter);

        // Assert
        _viewModel.InputText.Should().Be("attack ");
        _viewModel.IsCompletionVisible.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ApplyCompletion updates input correctly.
    /// </summary>
    [Test]
    public void ApplyCompletion_ReplacesCurrentWord()
    {
        // Arrange
        _viewModel.InputText = "use hea";

        // Act
        _viewModel.ApplyCompletion("healing-potion");

        // Assert
        _viewModel.InputText.Should().Be("use healing-potion ");
    }

    /// <summary>
    /// Verifies that history navigation saves current input.
    /// </summary>
    [Test]
    public void NavigateHistory_SavesCurrentInput()
    {
        // Arrange
        _viewModel.InputText = "partial";
        _historyMock.Setup(x => x.GetPrevious()).Returns("previous");
        _historyMock.Setup(x => x.GetNext()).Returns((string?)null);

        // Act - Navigate back
        _viewModel.HandleKeyDown(Key.Up);

        // Assert
        _historyMock.Verify(x => x.SaveCurrentInput("partial"), Times.Once);
    }
}
