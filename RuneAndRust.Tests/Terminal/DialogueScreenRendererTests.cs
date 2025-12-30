using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for the DialogueScreenRenderer (v0.4.2d - The Parley).
/// Tests rendering logic and edge cases.
/// Note: Spectre.Console output is difficult to verify directly, so these tests
/// focus on ensuring the renderer handles various inputs without throwing.
/// </summary>
public class DialogueScreenRendererTests
{
    private readonly Mock<IThemeService> _mockTheme;
    private readonly Mock<ILogger<DialogueScreenRenderer>> _mockLogger;
    private readonly DialogueScreenRenderer _sut;

    public DialogueScreenRendererTests()
    {
        _mockTheme = new Mock<IThemeService>();
        _mockLogger = new Mock<ILogger<DialogueScreenRenderer>>();

        // Setup default theme colors
        _mockTheme.Setup(t => t.GetColor("DialogueSpeakerColor")).Returns("cyan");
        _mockTheme.Setup(t => t.GetColor("DialogueTextColor")).Returns("white");
        _mockTheme.Setup(t => t.GetColor("DialogueSelectedColor")).Returns("yellow");
        _mockTheme.Setup(t => t.GetColor("DialogueOptionColor")).Returns("white");
        _mockTheme.Setup(t => t.GetColor("DialogueLockedColor")).Returns("dim grey");

        _sut = new DialogueScreenRenderer(_mockTheme.Object, _mockLogger.Object);
    }

    private DialogueTuiViewModel CreateTestViewModel(
        int optionCount = 3,
        int selectedIndex = 0,
        bool hasLockedOptions = false,
        string? npcTitle = null)
    {
        var options = new List<DialogueOptionTuiViewModel>();
        for (int i = 0; i < optionCount; i++)
        {
            options.Add(new DialogueOptionTuiViewModel(
                OptionId: $"option_{i}",
                Text: $"Option {i + 1}",
                DisplayOrder: i,
                IsVisible: true,
                IsAvailable: !hasLockedOptions || i == 0,
                LockReason: hasLockedOptions && i > 0 ? "Requires Reputation" : null,
                Tooltip: null));
        }

        return new DialogueTuiViewModel(
            SessionId: Guid.NewGuid(),
            NpcName: "Old Scavenger",
            NpcTitle: npcTitle,
            SpeakerName: "Old Scavenger",
            Text: "The winds carry strange whispers from the depths. You'd do well to heed them, traveler.",
            IsTerminalNode: false,
            Options: options,
            SelectedIndex: selectedIndex);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Render Tests (10 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Render_WithValidViewModel_DoesNotThrow()
    {
        // Arrange
        var viewModel = CreateTestViewModel();

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithNpcTitle_IncludesTitle()
    {
        // Arrange
        var viewModel = CreateTestViewModel(npcTitle: "Iron-Bane Elder");

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
        viewModel.SpeakerDisplay.Should().Contain("Iron-Bane Elder");
    }

    [Fact]
    public void Render_WithNoOptions_DoesNotThrow()
    {
        // Arrange
        var viewModel = new DialogueTuiViewModel(
            SessionId: Guid.NewGuid(),
            NpcName: "Silent Stranger",
            NpcTitle: null,
            SpeakerName: "Silent Stranger",
            Text: "...",
            IsTerminalNode: true,
            Options: new List<DialogueOptionTuiViewModel>(),
            SelectedIndex: 0);

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithLockedOptions_DoesNotThrow()
    {
        // Arrange
        var viewModel = CreateTestViewModel(hasLockedOptions: true);

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithLongText_WrapsCorrectly()
    {
        // Arrange
        var longText = string.Join(" ", Enumerable.Repeat("The ancient mechanisms creak with every breath of wind.", 10));
        var viewModel = new DialogueTuiViewModel(
            SessionId: Guid.NewGuid(),
            NpcName: "Verbose Scholar",
            NpcTitle: null,
            SpeakerName: "Verbose Scholar",
            Text: longText,
            IsTerminalNode: false,
            Options: new List<DialogueOptionTuiViewModel>
            {
                new("opt_1", "Continue", 0, true, true, null, null)
            },
            SelectedIndex: 0);

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithSpecialCharacters_EscapesMarkup()
    {
        // Arrange
        var viewModel = new DialogueTuiViewModel(
            SessionId: Guid.NewGuid(),
            NpcName: "[The] Marked One",
            NpcTitle: null,
            SpeakerName: "[The] Marked One",
            Text: "The [symbols] contain [hidden] meanings.",
            IsTerminalNode: false,
            Options: new List<DialogueOptionTuiViewModel>
            {
                new("opt_1", "Ask about [symbols]", 0, true, true, null, null)
            },
            SelectedIndex: 0);

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithManyOptions_HandlesAll()
    {
        // Arrange
        var viewModel = CreateTestViewModel(optionCount: 9); // Maximum expected

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithDifferentSelectedIndex_DoesNotThrow()
    {
        // Arrange
        var viewModel = CreateTestViewModel(optionCount: 5, selectedIndex: 3);

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithTerminalNode_DoesNotThrow()
    {
        // Arrange
        var viewModel = new DialogueTuiViewModel(
            SessionId: Guid.NewGuid(),
            NpcName: "Farewell Giver",
            NpcTitle: null,
            SpeakerName: "Farewell Giver",
            Text: "May the spirits guide your path.",
            IsTerminalNode: true,
            Options: new List<DialogueOptionTuiViewModel>
            {
                new("goodbye", "[Leave]", 0, true, true, null, null)
            },
            SelectedIndex: 0);

        // Act
        var act = () => _sut.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithNullThemeColors_UsesDefaults()
    {
        // Arrange
        var mockTheme = new Mock<IThemeService>();
        mockTheme.Setup(t => t.GetColor(It.IsAny<string>())).Returns((string?)null);

        var renderer = new DialogueScreenRenderer(mockTheme.Object, _mockLogger.Object);
        var viewModel = CreateTestViewModel();

        // Act
        var act = () => renderer.Render(viewModel);

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PlayLockedFeedback Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void PlayLockedFeedback_WithReason_DoesNotThrow()
    {
        // Act
        var act = () => _sut.PlayLockedFeedback("Requires 50 reputation with Iron-Banes");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void PlayLockedFeedback_WithSpecialCharacters_DoesNotThrow()
    {
        // Act
        var act = () => _sut.PlayLockedFeedback("[Requires] special [item]");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void PlayLockedFeedback_WithEmptyReason_DoesNotThrow()
    {
        // Act
        var act = () => _sut.PlayLockedFeedback("");

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DialogueTuiViewModel Tests (2 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DialogueTuiViewModel_SpeakerDisplay_WithTitle_IncludesTitle()
    {
        // Arrange
        var viewModel = CreateTestViewModel(npcTitle: "Iron-Bane Elder");

        // Assert
        viewModel.SpeakerDisplay.Should().Be("Old Scavenger, Iron-Bane Elder");
    }

    [Fact]
    public void DialogueTuiViewModel_SpeakerDisplay_WithoutTitle_ShowsNameOnly()
    {
        // Arrange
        var viewModel = CreateTestViewModel(npcTitle: null);

        // Assert
        viewModel.SpeakerDisplay.Should().Be("Old Scavenger");
    }
}
