using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the GameService class.
/// Demonstrates the testing pattern for services with dependency injection.
/// </summary>
public class GameServiceTests
{
    private readonly Mock<ILogger<GameService>> _mockGameLogger;
    private readonly Mock<ILogger<CommandParser>> _mockParserLogger;
    private readonly Mock<IInputHandler> _mockInputHandler;
    private readonly Mock<ICombatService> _mockCombatService;
    private readonly GameState _state;
    private readonly CommandParser _parser;

    public GameServiceTests()
    {
        _mockGameLogger = new Mock<ILogger<GameService>>();
        _mockParserLogger = new Mock<ILogger<CommandParser>>();
        _mockInputHandler = new Mock<IInputHandler>();
        _mockCombatService = new Mock<ICombatService>();
        _state = new GameState();
        _parser = new CommandParser(_mockParserLogger.Object, _mockInputHandler.Object, _state);
    }

    [Fact]
    public void Start_ShouldLogGameLoopInitializedMessage()
    {
        // Arrange - Input "quit" immediately to exit the loop
        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");
        var sut = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, null);

        // Act
        sut.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockGameLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Game Loop Initialized")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Start_ShouldLogGameLoopEndedMessage()
    {
        // Arrange - Input "quit" immediately to exit the loop
        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");
        var sut = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, null);

        // Act
        sut.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockGameLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Game Loop Ended")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange & Act
        var action = () => new GameService(
            _mockGameLogger.Object,
            _mockInputHandler.Object,
            _parser,
            _state,
            _mockCombatService.Object,
            null);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Start_ShouldDisplayWelcomeMessage()
    {
        // Arrange
        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");
        var sut = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, null);

        // Act
        sut.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(
            x => x.DisplayMessage(It.Is<string>(s => s.Contains("Welcome to Rune & Rust"))),
            Times.Once);
    }

    [Fact]
    public void Start_ShouldDisplayFarewellMessage()
    {
        // Arrange
        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");
        var sut = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, null);

        // Act
        sut.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(
            x => x.DisplayMessage(It.Is<string>(s => s.Contains("Farewell"))),
            Times.Once);
    }
}
