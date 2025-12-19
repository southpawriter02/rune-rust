using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the GameService class.
/// Demonstrates the testing pattern for services with ILogger injection.
/// </summary>
public class GameServiceTests
{
    private readonly Mock<ILogger<GameService>> _mockLogger;
    private readonly GameService _sut; // System Under Test

    public GameServiceTests()
    {
        _mockLogger = new Mock<ILogger<GameService>>();
        _sut = new GameService(_mockLogger.Object);
    }

    [Fact]
    public void Start_ShouldLogInitializationMessage()
    {
        // Act
        _sut.Start();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rune & Rust Engine Initialized")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldNotThrow()
    {
        // Arrange & Act
        var action = () => new GameService(_mockLogger.Object);

        // Assert
        action.Should().NotThrow();
    }
}
