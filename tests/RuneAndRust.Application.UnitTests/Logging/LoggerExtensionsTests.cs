using FluentAssertions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Logging;

namespace RuneAndRust.Application.UnitTests.Logging;

/// <summary>
/// Tests for the LoggerExtensions extension methods.
/// </summary>
[TestFixture]
public class LoggerExtensionsTests
{
    private TestLogger<LoggerExtensionsTests> _logger = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new TestLogger<LoggerExtensionsTests>();
    }

    [Test]
    public void LogPlayerAction_LogsWithCorrectTemplate()
    {
        // Arrange
        var player = CreateTestPlayer("TestPlayer");

        // Act
        _logger.LogPlayerAction(player, "attacked");

        // Assert
        _logger.ContainsMessage(LogLevel.Information, "Player TestPlayer attacked").Should().BeTrue();
    }

    [Test]
    public void LogPlayerAction_WithTarget_IncludesTarget()
    {
        // Arrange
        var player = CreateTestPlayer("Hero");

        // Act
        _logger.LogPlayerAction(player, "examined", "chest");

        // Assert
        _logger.ContainsMessage(LogLevel.Information, "Hero").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Information, "examined").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Information, "chest").Should().BeTrue();
    }

    [Test]
    public void LogCombat_LogsAllCombatFields()
    {
        // Arrange
        var player = CreateTestPlayer("Warrior");
        var monster = CreateTestMonster("Goblin");

        // Act
        _logger.LogCombat(player, monster, 10, 5, false, false);

        // Assert
        _logger.ContainsMessage("Warrior").Should().BeTrue();
        _logger.ContainsMessage("Goblin").Should().BeTrue();
        _logger.ContainsMessage("10").Should().BeTrue(); // Damage dealt
        _logger.ContainsMessage("5").Should().BeTrue();  // Damage received
    }

    [Test]
    public void LogStateChange_FormatsOldAndNewValues()
    {
        // Arrange & Act
        _logger.LogStateChange("Player", "player-1", "Health", 100, 85);

        // Assert
        _logger.ContainsMessage(LogLevel.Information, "Player").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Information, "Health").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Information, "100").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Information, "85").Should().BeTrue();
    }

    [Test]
    public void BeginPerformanceScope_ReturnsPerformanceScope()
    {
        // Act
        var scope = _logger.BeginPerformanceScope("TestOperation");

        // Assert
        scope.Should().NotBeNull();
        scope.Should().BeOfType<PerformanceScope>();
        scope.Dispose();
    }

    [Test]
    public void LogConfigurationLoaded_FormatsWithAllParameters()
    {
        // Act
        _logger.LogConfigurationLoaded(5, "monsters", "monsters.json");

        // Assert
        _logger.ContainsMessage(LogLevel.Information, "5").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Information, "monsters").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Information, "monsters.json").Should().BeTrue();
    }

    [Test]
    public void LogEntityNotFound_LogsAtDebugLevel()
    {
        // Act
        _logger.LogEntityNotFound("Monster", "goblin-1");

        // Assert
        _logger.GetCount(LogLevel.Debug).Should().Be(1);
        _logger.ContainsMessage(LogLevel.Debug, "Monster").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Debug, "goblin-1").Should().BeTrue();
    }

    private static Player CreateTestPlayer(string name)
    {
        return new Player(name);
    }

    private static Monster CreateTestMonster(string name)
    {
        return new Monster(name, "A test monster", 50, new Stats(50, 5, 2));
    }
}
