using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="MessageLogViewModel"/>.
/// </summary>
[TestFixture]
public class MessageLogViewModelTests
{
    private MessageLogViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new MessageLogViewModel();
    }

    /// <summary>
    /// Verifies that constructor initializes with sample messages.
    /// </summary>
    [Test]
    public void Constructor_InitializesWithSampleMessages()
    {
        // Assert
        _viewModel.Messages.Should().NotBeEmpty();
        _viewModel.AutoScrollEnabled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that AddMessage adds to the collection.
    /// </summary>
    [Test]
    public void AddMessage_AddsToCollection()
    {
        // Arrange
        var initialCount = _viewModel.Messages.Count;
        var message = GameMessage.Info("New message");

        // Act
        _viewModel.AddMessage(message);

        // Assert
        _viewModel.Messages.Should().HaveCount(initialCount + 1);
        _viewModel.Messages.Last().Text.Should().Be("New message");
    }

    /// <summary>
    /// Verifies that ClearLogCommand clears all messages.
    /// </summary>
    [Test]
    public void ClearLogCommand_ClearsAllMessages()
    {
        // Act
        _viewModel.ClearLogCommand.Execute(null);

        // Assert
        _viewModel.Messages.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that ToggleAutoScrollCommand toggles the property.
    /// </summary>
    [Test]
    public void ToggleAutoScrollCommand_TogglesAutoScroll()
    {
        // Arrange
        _viewModel.AutoScrollEnabled.Should().BeTrue();

        // Act
        _viewModel.ToggleAutoScrollCommand.Execute(null);

        // Assert
        _viewModel.AutoScrollEnabled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that messages over MaxMessages are trimmed.
    /// </summary>
    [Test]
    public void AddMessage_OverLimit_TrimsOldMessages()
    {
        // Arrange
        _viewModel.ClearLogCommand.Execute(null);
        
        for (int i = 0; i < MessageLogViewModel.MaxMessages + 10; i++)
        {
            _viewModel.AddMessage(GameMessage.Info($"Message {i}"));
        }

        // Assert
        _viewModel.Messages.Should().HaveCount(MessageLogViewModel.MaxMessages);
        _viewModel.Messages.First().Text.Should().Be("Message 10");
    }

    /// <summary>
    /// Verifies that combat messages are filtered when disabled.
    /// </summary>
    [Test]
    public void AddMessage_CombatDisabled_FiltersMessage()
    {
        // Arrange
        _viewModel.ClearLogCommand.Execute(null);
        _viewModel.ShowCombatMessages = false;
        var combatMessage = GameMessage.Combat("Hit!", MessageType.CombatHit);

        // Act
        _viewModel.AddMessage(combatMessage);

        // Assert
        _viewModel.Messages.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that AddMessage with text and type works correctly.
    /// </summary>
    [Test]
    public void AddMessage_WithTextAndType_AddsCorrectMessage()
    {
        // Arrange
        _viewModel.ClearLogCommand.Execute(null);

        // Act
        _viewModel.AddMessage("Test message", MessageType.Warning);

        // Assert
        _viewModel.Messages.Should().HaveCount(1);
        _viewModel.Messages.First().Text.Should().Be("Test message");
        _viewModel.Messages.First().Type.Should().Be(MessageType.Warning);
    }
}
