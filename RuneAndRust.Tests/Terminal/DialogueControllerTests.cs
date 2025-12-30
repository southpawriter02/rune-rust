using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Terminal.Controllers;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for the DialogueController (v0.4.2d - The Parley).
/// Tests input handling, navigation, and selection logic.
/// </summary>
public class DialogueControllerTests
{
    private readonly Mock<IDialogueService> _mockDialogueService;
    private readonly Mock<IDialogueScreenRenderer> _mockRenderer;
    private readonly Mock<ILogger<DialogueController>> _mockLogger;
    private readonly GameState _gameState;
    private readonly Character _testCharacter;
    private readonly DialogueController _sut;

    public DialogueControllerTests()
    {
        _mockDialogueService = new Mock<IDialogueService>();
        _mockRenderer = new Mock<IDialogueScreenRenderer>();
        _mockLogger = new Mock<ILogger<DialogueController>>();
        _gameState = new GameState();
        _testCharacter = new Character
        {
            Id = Guid.NewGuid(),
            Name = "TestChar",
            Level = 1
        };

        _sut = new DialogueController(
            _mockDialogueService.Object,
            _mockRenderer.Object,
            _mockLogger.Object);
    }

    private DialogueViewModel CreateTestViewModel(int optionCount = 3, bool allAvailable = true)
    {
        var options = new List<DialogueOptionViewModel>();
        for (int i = 0; i < optionCount; i++)
        {
            options.Add(new DialogueOptionViewModel
            {
                OptionId = $"option_{i}",
                Text = $"Option {i}",
                IsAvailable = allAvailable || i == 0,
                IsVisible = true,
                DisplayOrder = i,
                LockedReason = allAvailable ? null : (i == 0 ? null : "Locked")
            });
        }

        return new DialogueViewModel
        {
            SessionId = Guid.NewGuid(),
            NpcName = "Test NPC",
            SpeakerName = "Test NPC",
            DialogueText = "Test dialogue text",
            CurrentNodeId = "node_1",
            Options = options,
            IsTerminalNode = false
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Navigation Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task HandleInputAsync_UpArrow_DecrementsSelectedIndex()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Set initial index to 1
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);

        // Act
        await _sut.HandleInputAsync(ConsoleKey.UpArrow, _testCharacter, _gameState);

        // Assert
        _sut.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public async Task HandleInputAsync_DownArrow_IncrementsSelectedIndex()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Act
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);

        // Assert
        _sut.SelectedIndex.Should().Be(1);
    }

    [Fact]
    public async Task HandleInputAsync_UpArrowAtTop_StaysAtZero()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Act
        await _sut.HandleInputAsync(ConsoleKey.UpArrow, _testCharacter, _gameState);

        // Assert
        _sut.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public async Task HandleInputAsync_DownArrowAtBottom_StaysAtMax()
    {
        // Arrange
        var viewModel = CreateTestViewModel(3);
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Move to bottom
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);

        // Act - try to go past bottom
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);

        // Assert
        _sut.SelectedIndex.Should().Be(2); // Should stay at 2 (max for 3 options)
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Selection Tests (5 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task HandleInputAsync_EnterOnAvailableOption_SelectsOption()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);
        _mockDialogueService
            .Setup(s => s.SelectOptionAsync(_testCharacter, "option_0", _gameState))
            .ReturnsAsync(DialogueStepResult.Continue(viewModel));

        // Act
        var result = await _sut.HandleInputAsync(ConsoleKey.Enter, _testCharacter, _gameState);

        // Assert
        result.Should().Be(GamePhase.Dialogue);
        _mockDialogueService.Verify(s => s.SelectOptionAsync(_testCharacter, "option_0", _gameState), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_EnterOnLockedOption_PlaysLockedFeedback()
    {
        // Arrange
        var viewModel = CreateTestViewModel(3, allAvailable: false);
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Navigate to locked option
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);

        // Act
        var result = await _sut.HandleInputAsync(ConsoleKey.Enter, _testCharacter, _gameState);

        // Assert
        result.Should().Be(GamePhase.Dialogue);
        _mockRenderer.Verify(r => r.PlayLockedFeedback("Locked"), Times.Once);
        _mockDialogueService.Verify(s => s.SelectOptionAsync(It.IsAny<Character>(), It.IsAny<string>(), It.IsAny<GameState>()), Times.Never);
    }

    [Fact]
    public async Task HandleInputAsync_SelectTerminalOption_ReturnsExploration()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);
        _mockDialogueService
            .Setup(s => s.SelectOptionAsync(_testCharacter, "option_0", _gameState))
            .ReturnsAsync(DialogueStepResult.End(DialogueEndReason.NpcExit));

        // Act
        var result = await _sut.HandleInputAsync(ConsoleKey.Enter, _testCharacter, _gameState);

        // Assert
        result.Should().Be(GamePhase.Exploration);
    }

    [Fact]
    public async Task HandleInputAsync_NumberKey_SelectsCorrespondingOption()
    {
        // Arrange
        var viewModel = CreateTestViewModel(3);
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);
        _mockDialogueService
            .Setup(s => s.SelectOptionAsync(_testCharacter, "option_1", _gameState))
            .ReturnsAsync(DialogueStepResult.Continue(viewModel));

        // Act
        var result = await _sut.HandleInputAsync(ConsoleKey.D2, _testCharacter, _gameState);

        // Assert
        // Note: SelectedIndex is reset after successful selection, but we verify the right option was selected
        _mockDialogueService.Verify(s => s.SelectOptionAsync(_testCharacter, "option_1", _gameState), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_InvalidNumberKey_DoesNothing()
    {
        // Arrange
        var viewModel = CreateTestViewModel(2); // Only 2 options
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Act
        var result = await _sut.HandleInputAsync(ConsoleKey.D9, _testCharacter, _gameState); // Option 9 doesn't exist

        // Assert
        result.Should().Be(GamePhase.Dialogue);
        _mockDialogueService.Verify(s => s.SelectOptionAsync(It.IsAny<Character>(), It.IsAny<string>(), It.IsAny<GameState>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Exit Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task HandleInputAsync_Escape_EndsDialogue()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);
        _mockDialogueService
            .Setup(s => s.EndDialogueAsync(DialogueEndReason.PlayerCancel, _gameState))
            .ReturnsAsync(new DialogueEndResult { Success = true, Duration = TimeSpan.FromSeconds(5) });

        // Act
        var result = await _sut.HandleInputAsync(ConsoleKey.Escape, _testCharacter, _gameState);

        // Assert
        result.Should().Be(GamePhase.Exploration);
        _mockDialogueService.Verify(s => s.EndDialogueAsync(DialogueEndReason.PlayerCancel, _gameState), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Q_EndsDialogue()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);
        _mockDialogueService
            .Setup(s => s.EndDialogueAsync(DialogueEndReason.PlayerCancel, _gameState))
            .ReturnsAsync(new DialogueEndResult { Success = true, Duration = TimeSpan.FromSeconds(5) });

        // Act
        var result = await _sut.HandleInputAsync(ConsoleKey.Q, _testCharacter, _gameState);

        // Assert
        result.Should().Be(GamePhase.Exploration);
    }

    [Fact]
    public async Task HandleInputAsync_NoActiveDialogue_ReturnsExploration()
    {
        // Arrange
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync((DialogueViewModel?)null);

        // Act
        var result = await _sut.HandleInputAsync(ConsoleKey.Enter, _testCharacter, _gameState);

        // Assert
        result.Should().Be(GamePhase.Exploration);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // BuildTuiViewModelAsync Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task BuildTuiViewModelAsync_WithActiveDialogue_ReturnsViewModel()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Act
        var result = await _sut.BuildTuiViewModelAsync(_testCharacter, _gameState);

        // Assert
        result.Should().NotBeNull();
        result!.NpcName.Should().Be("Test NPC");
        result.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public async Task BuildTuiViewModelAsync_NoActiveDialogue_ReturnsNull()
    {
        // Arrange
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync((DialogueViewModel?)null);

        // Act
        var result = await _sut.BuildTuiViewModelAsync(_testCharacter, _gameState);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task BuildTuiViewModelAsync_ClampsSelectionToValidRange()
    {
        // Arrange
        var viewModel = CreateTestViewModel(2);
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Manually set selection beyond range (simulates node transition)
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);

        // Act - Build with only 2 options
        var result = await _sut.BuildTuiViewModelAsync(_testCharacter, _gameState);

        // Assert
        result!.SelectedIndex.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task BuildTuiViewModelAsync_FiltersHiddenOptions()
    {
        // Arrange
        var viewModel = CreateTestViewModel(3);
        viewModel.Options[1].IsVisible = false; // Hide middle option

        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        // Act
        var result = await _sut.BuildTuiViewModelAsync(_testCharacter, _gameState);

        // Assert
        result!.Options.Count.Should().Be(2); // Only visible options
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ResetSelection Tests (2 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ResetSelection_ResetsIndexToZero()
    {
        // Arrange
        var viewModel = CreateTestViewModel();
        _mockDialogueService
            .Setup(s => s.GetCurrentDialogueAsync(_testCharacter, _gameState))
            .ReturnsAsync(viewModel);

        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);
        await _sut.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, _gameState);

        // Act
        _sut.ResetSelection();

        // Assert
        _sut.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public void ResetSelection_ClearsLastSelectedOptionId()
    {
        // Arrange - set a value
        // (In practice, this would be set during selection)

        // Act
        _sut.ResetSelection();

        // Assert
        _sut.LastSelectedOptionId.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // IsInDialogue Tests (2 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void IsInDialogue_WithActiveSession_ReturnsTrue()
    {
        // Arrange
        _mockDialogueService
            .Setup(s => s.IsInDialogue(_testCharacter.Id, _gameState))
            .Returns(true);

        // Act
        var result = _sut.IsInDialogue(_testCharacter.Id, _gameState);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInDialogue_WithNoSession_ReturnsFalse()
    {
        // Arrange
        _mockDialogueService
            .Setup(s => s.IsInDialogue(_testCharacter.Id, _gameState))
            .Returns(false);

        // Act
        var result = _sut.IsInDialogue(_testCharacter.Id, _gameState);

        // Assert
        result.Should().BeFalse();
    }
}
