using FluentAssertions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class GameSessionTurnTests
{
    [Test]
    public void TurnCount_StartsAtZero()
    {
        // Arrange & Act
        var session = GameSession.CreateNew("TestPlayer");

        // Assert
        session.TurnCount.Should().Be(0);
    }

    [Test]
    public void AdvanceTurn_IncrementsTurnCount()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");
        var initialTurnCount = session.TurnCount;

        // Act
        session.AdvanceTurn();

        // Assert
        session.TurnCount.Should().Be(initialTurnCount + 1);
    }

    [Test]
    public void AdvanceTurn_ReturnsNewTurnCount()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");

        // Act
        var result = session.AdvanceTurn();

        // Assert
        result.Should().Be(1);
        result.Should().Be(session.TurnCount);
    }

    [Test]
    public void AdvanceTurn_UpdatesLastPlayed()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");
        var initialLastPlayed = session.LastPlayedAt;

        // Wait a tiny bit to ensure time difference
        Thread.Sleep(10);

        // Act
        session.AdvanceTurn();

        // Assert
        session.LastPlayedAt.Should().BeAfter(initialLastPlayed);
    }

    [Test]
    public void AdvanceTurn_MultipleTimes_IncrementsCorrectly()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");

        // Act
        session.AdvanceTurn();
        session.AdvanceTurn();
        session.AdvanceTurn();

        // Assert
        session.TurnCount.Should().Be(3);
    }
}
