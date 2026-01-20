using FluentAssertions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Logging;
using RuneAndRust.TestUtilities.Logging;

namespace RuneAndRust.Application.UnitTests.Logging;

/// <summary>
/// Tests for the PerformanceScope timing utility.
/// </summary>
[TestFixture]
public class PerformanceScopeTests
{
    private TestLogger<PerformanceScopeTests> _logger = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new TestLogger<PerformanceScopeTests>();
    }

    [Test]
    public void PerformanceScope_LogsElapsedTime_OnDispose()
    {
        // Arrange & Act
        using (new PerformanceScope(_logger, "TestOperation"))
        {
            // Simulate some work
            Thread.Sleep(10);
        }

        // Assert
        _logger.ContainsMessage("TestOperation").Should().BeTrue();
        _logger.ContainsMessage("completed in").Should().BeTrue();
    }

    [Test]
    public void PerformanceScope_WarnsOnSlowOperation_WhenThresholdExceeded()
    {
        // Arrange & Act
        using (new PerformanceScope(_logger, "SlowOperation", warningThresholdMs: 5))
        {
            // Simulate slow operation that exceeds threshold
            Thread.Sleep(20);
        }

        // Assert
        _logger.GetCount(LogLevel.Warning).Should().BeGreaterThan(0);
        _logger.ContainsMessage(LogLevel.Warning, "SlowOperation").Should().BeTrue();
        _logger.ContainsMessage("threshold").Should().BeTrue();
    }

    [Test]
    public void PerformanceScope_LogsAtConfiguredLevel_WhenBelowThreshold()
    {
        // Arrange & Act
        using (new PerformanceScope(_logger, "FastOperation", LogLevel.Information, warningThresholdMs: 5000))
        {
            // Fast operation that doesn't exceed threshold
        }

        // Assert
        _logger.GetCount(LogLevel.Warning).Should().Be(0);
        _logger.ContainsMessage(LogLevel.Information, "FastOperation").Should().BeTrue();
    }

    [Test]
    public void PerformanceScope_LogsTrace_OnStart()
    {
        // Arrange & Act
        using (new PerformanceScope(_logger, "TracedOperation"))
        {
            // Just creating the scope should log trace
        }

        // Assert
        _logger.ContainsMessage(LogLevel.Trace, "Starting operation").Should().BeTrue();
        _logger.ContainsMessage(LogLevel.Trace, "TracedOperation").Should().BeTrue();
    }

    [Test]
    public void PerformanceScope_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act
        var act = () => new PerformanceScope(null!, "Operation");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Test]
    public void PerformanceScope_ThrowsArgumentNullException_WhenOperationNameIsNull()
    {
        // Arrange & Act
        var act = () => new PerformanceScope(_logger, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("operationName");
    }
}
