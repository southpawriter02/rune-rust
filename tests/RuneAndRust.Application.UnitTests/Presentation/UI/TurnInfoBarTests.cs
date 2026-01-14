using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="TurnInfoBar"/>.
/// </summary>
[TestFixture]
public class TurnInfoBarTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private TurnInfoBar _turnInfoBar = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _turnInfoBar = new TurnInfoBar(_mockTerminal.Object);
    }

    #region Render Tests

    [Test]
    public void Render_IncludesRoundNumber()
    {
        // Act
        var result = _turnInfoBar.Render(3, "Hero", true, true, 4, 4);

        // Assert
        result.Should().Contain("Round 3");
    }

    [Test]
    public void Render_PlayerTurn_ShowsYourTurn()
    {
        // Act
        var result = _turnInfoBar.Render(1, "Hero", true, true, 4, 4);

        // Assert
        result.Should().Contain("YOUR TURN");
    }

    [Test]
    public void Render_EnemyTurn_ShowsEnemyName()
    {
        // Act
        var result = _turnInfoBar.Render(1, "Goblin", false, true, 3, 3);

        // Assert
        result.Should().Contain("GOBLIN'S TURN");
    }

    [Test]
    public void Render_ActionAvailable_ShowsAvailable()
    {
        // Act
        var result = _turnInfoBar.Render(1, "Hero", true, true, 4, 4);

        // Assert
        result.Should().Contain("Action:");
        result.Should().Contain("Available");
    }

    [Test]
    public void Render_ActionUsed_ShowsUsed()
    {
        // Act
        var result = _turnInfoBar.Render(1, "Hero", true, false, 4, 4);

        // Assert
        result.Should().Contain("Used");
    }

    [Test]
    public void Render_IncludesMovementInfo()
    {
        // Act
        var result = _turnInfoBar.Render(1, "Hero", true, true, 2, 4);

        // Assert
        result.Should().Contain("Move: 2/4");
    }

    [Test]
    public void Render_WithBonusAction_ShowsBonusInfo()
    {
        // Act
        var result = _turnInfoBar.Render(1, "Hero", true, true, 4, 4, true, true);

        // Assert
        result.Should().Contain("Bonus:");
    }

    #endregion
}
