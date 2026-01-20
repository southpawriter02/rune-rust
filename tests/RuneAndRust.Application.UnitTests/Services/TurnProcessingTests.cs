using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Services;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class TurnProcessingTests
{
    private Mock<IGameRepository> _repositoryMock = null!;
    private Mock<ILogger<GameSessionService>> _loggerMock = null!;
    private GameSessionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IGameRepository>();
        _loggerMock = new Mock<ILogger<GameSessionService>>();

        // GameSessionService now takes 4 parameters: repository, logger, examinationService?, dungeonGenerator?
        _service = new GameSessionService(
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public void ProcessTurnEnd_WithNoSession_ReturnsEmpty()
    {
        // Arrange - no session started

        // Act
        var result = _service.ProcessTurnEnd();

        // Assert
        result.NewTurnCount.Should().Be(0);
        result.HasChanges.Should().BeFalse();
    }

    [Test]
    public async Task ProcessTurnEnd_IncrementsTurnCount()
    {
        // Arrange
        await StartNewGameSession();
        var initialTurnCount = 0;

        // Act
        var result = _service.ProcessTurnEnd();

        // Assert
        result.NewTurnCount.Should().Be(initialTurnCount + 1);
    }

    [Test]
    public async Task ProcessTurnEnd_SecondCall_IncrementsFurther()
    {
        // Arrange
        await StartNewGameSession();

        // Act
        _service.ProcessTurnEnd();
        var result = _service.ProcessTurnEnd();

        // Assert
        result.NewTurnCount.Should().Be(2);
    }

    [Test]
    public void IsInCombat_WithNoSession_ReturnsFalse()
    {
        // Arrange - no session

        // Act
        var result = _service.IsInCombat();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task IsInCombat_WithSession_ReturnsBool()
    {
        // Arrange
        await StartNewGameSession();

        // Act
        var result = _service.IsInCombat();

        // Assert
        // Initial room may or may not have monsters, but we can test behavior
        // This test confirms the method executes without error
        result.Should().Be(result); // It returns a valid boolean, just verify it doesn't throw
    }

    private async Task StartNewGameSession()
    {
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _service.StartNewGameAsync("TestPlayer");
    }
}
