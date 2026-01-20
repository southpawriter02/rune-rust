using FluentAssertions;
using Microsoft.Extensions.Logging;
using RuneAndRust.TestUtilities.Logging;

namespace RuneAndRust.Application.UnitTests.Logging;

/// <summary>
/// Tests for the TestLogger test utility.
/// </summary>
[TestFixture]
public class LoggingInfrastructureTests
{
    private TestLogger<LoggingInfrastructureTests> _logger = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new TestLogger<LoggingInfrastructureTests>();
    }

    [Test]
    public void TestLogger_CapturesLogEntries_WhenLogIsCalled()
    {
        // Arrange & Act
        _logger.LogInformation("Test message 1");
        _logger.LogWarning("Test message 2");
        _logger.LogError("Test message 3");

        // Assert
        _logger.Entries.Should().HaveCount(3);
    }

    [Test]
    public void TestLogger_FiltersByLevel_ReturnsOnlyMatchingEntries()
    {
        // Arrange
        _logger.LogDebug("Debug message");
        _logger.LogInformation("Info message 1");
        _logger.LogInformation("Info message 2");
        _logger.LogWarning("Warning message");

        // Act
        var infoEntries = _logger.GetEntries(LogLevel.Information).ToList();

        // Assert
        infoEntries.Should().HaveCount(2);
        infoEntries.Should().AllSatisfy(e => e.Level.Should().Be(LogLevel.Information));
    }

    [Test]
    public void TestLogger_ContainsMessage_ReturnsTrueForExistingFragment()
    {
        // Arrange
        _logger.LogInformation("User logged in successfully");

        // Act & Assert
        _logger.ContainsMessage("logged in").Should().BeTrue();
        _logger.ContainsMessage("LOGGED IN").Should().BeTrue(); // Case insensitive
        _logger.ContainsMessage("logged out").Should().BeFalse();
    }

    [Test]
    public void TestLogger_ContainsMessage_WithLevel_FiltersCorrectly()
    {
        // Arrange
        _logger.LogInformation("Operation completed");
        _logger.LogWarning("Operation completed with warnings");

        // Act & Assert
        _logger.ContainsMessage(LogLevel.Information, "Operation completed").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Warning, "warnings").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Error, "Operation").Should().BeFalse();
    }

    [Test]
    public void TestLogger_Clear_RemovesAllEntries()
    {
        // Arrange
        _logger.LogInformation("Test 1");
        _logger.LogInformation("Test 2");
        _logger.Entries.Should().HaveCount(2);

        // Act
        _logger.Clear();

        // Assert
        _logger.Entries.Should().BeEmpty();
    }

    [Test]
    public void TestLogger_GetCount_ReturnsCorrectCount()
    {
        // Arrange
        _logger.LogDebug("Debug 1");
        _logger.LogDebug("Debug 2");
        _logger.LogInformation("Info 1");
        _logger.LogWarning("Warning 1");

        // Act & Assert
        _logger.GetCount(LogLevel.Debug).Should().Be(2);
        _logger.GetCount(LogLevel.Information).Should().Be(1);
        _logger.GetCount(LogLevel.Warning).Should().Be(1);
        _logger.GetCount(LogLevel.Error).Should().Be(0);
    }

    [Test]
    public void TestLoggerFactory_GetTestLogger_ReturnsSameInstanceForSameType()
    {
        // Arrange
        var factory = new TestLoggerFactory();

        // Act
        var logger1 = factory.GetTestLogger<LoggingInfrastructureTests>();
        var logger2 = factory.GetTestLogger<LoggingInfrastructureTests>();

        // Assert
        logger1.Should().BeSameAs(logger2);
    }
}
