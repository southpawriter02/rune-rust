using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for DebugConsoleService (v0.3.17a).
/// Validates log buffer, command history, and visibility state management.
/// </summary>
public class DebugConsoleServiceTests
{
    private readonly DebugConsoleService _sut;
    private readonly ILogger<DebugConsoleService> _mockLogger;

    public DebugConsoleServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<DebugConsoleService>>();
        _sut = new DebugConsoleService(_mockLogger);
    }

    #region Toggle Tests

    [Fact]
    public void Toggle_SwitchesVisibilityFalseToTrue()
    {
        // Arrange
        _sut.IsVisible.Should().BeFalse("initial state should be not visible");

        // Act
        _sut.Toggle();

        // Assert
        _sut.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void Toggle_SwitchesVisibilityTrueToFalse()
    {
        // Arrange
        _sut.Toggle(); // Set to true
        _sut.IsVisible.Should().BeTrue("precondition: should be visible");

        // Act
        _sut.Toggle();

        // Assert
        _sut.IsVisible.Should().BeFalse();
    }

    #endregion

    #region WriteLog Tests

    [Fact]
    public void WriteLog_AddsEntryToHistory()
    {
        // Arrange & Act
        _sut.WriteLog("Test message");

        // Assert
        _sut.LogHistory.Should().HaveCount(1);
        _sut.LogHistory[0].Should().Contain("Test message");
    }

    [Fact]
    public void WriteLog_FormatsEntryWithTimestampAndSource()
    {
        // Arrange & Act
        _sut.WriteLog("Test message", "TestSource");

        // Assert
        var entry = _sut.LogHistory[0];
        entry.Should().MatchRegex(@"\[\d{2}:\d{2}:\d{2}\]");
        entry.Should().Contain("[TestSource]");
        entry.Should().Contain("Test message");
    }

    [Fact]
    public void WriteLog_UsesDefaultSourceWhenNotSpecified()
    {
        // Arrange & Act
        _sut.WriteLog("Test message");

        // Assert
        _sut.LogHistory[0].Should().Contain("[System]");
    }

    [Fact]
    public void WriteLog_MaintainsBufferLimit_RemovesOldest()
    {
        // Arrange - Add 55 entries (limit is 50)
        for (var i = 1; i <= 55; i++)
        {
            _sut.WriteLog($"Message {i}");
        }

        // Assert
        _sut.LogHistory.Should().HaveCount(50, "buffer should be capped at 50 entries");
        _sut.LogHistory[0].Should().Contain("Message 6", "oldest entries should be removed");
        _sut.LogHistory[49].Should().Contain("Message 55", "newest entry should be last");
    }

    #endregion

    #region SubmitCommand Tests

    [Fact]
    public void SubmitCommand_AddsToCommandHistory()
    {
        // Arrange & Act
        _sut.SubmitCommand("test command");

        // Assert
        _sut.CommandHistory.Should().HaveCount(1);
        _sut.CommandHistory[0].Should().Be("test command");
    }

    [Fact]
    public void SubmitCommand_WritesToLogWithUserSource()
    {
        // Arrange & Act
        _sut.SubmitCommand("test command");

        // Assert
        _sut.LogHistory.Should().HaveCount(1);
        _sut.LogHistory[0].Should().Contain("[User]");
        _sut.LogHistory[0].Should().Contain("test command");
    }

    [Fact]
    public void SubmitCommand_IgnoresEmptyInput()
    {
        // Arrange & Act
        _sut.SubmitCommand("");
        _sut.SubmitCommand("   ");
        _sut.SubmitCommand(null!);

        // Assert
        _sut.CommandHistory.Should().BeEmpty("empty commands should be ignored");
        _sut.LogHistory.Should().BeEmpty("empty commands should not be logged");
    }

    [Fact]
    public void CommandHistory_MaintainsLimit_RemovesOldest()
    {
        // Arrange - Add 25 commands (limit is 20)
        for (var i = 1; i <= 25; i++)
        {
            _sut.SubmitCommand($"command{i}");
        }

        // Assert
        _sut.CommandHistory.Should().HaveCount(20, "history should be capped at 20 commands");
        _sut.CommandHistory[0].Should().Be("command6", "oldest commands should be removed");
        _sut.CommandHistory[19].Should().Be("command25", "newest command should be last");
    }

    #endregion

    #region ClearLog Tests

    [Fact]
    public void ClearLog_EmptiesLogBuffer()
    {
        // Arrange
        _sut.WriteLog("Message 1");
        _sut.WriteLog("Message 2");
        _sut.WriteLog("Message 3");
        _sut.LogHistory.Should().HaveCount(3, "precondition: should have 3 entries");

        // Act
        _sut.ClearLog();

        // Assert
        _sut.LogHistory.Should().BeEmpty("log should be cleared");
    }

    [Fact]
    public void ClearLog_DoesNotClearCommandHistory()
    {
        // Arrange
        _sut.SubmitCommand("command1");
        _sut.SubmitCommand("command2");

        // Act
        _sut.ClearLog();

        // Assert
        _sut.CommandHistory.Should().HaveCount(2, "command history should be preserved");
    }

    #endregion
}
