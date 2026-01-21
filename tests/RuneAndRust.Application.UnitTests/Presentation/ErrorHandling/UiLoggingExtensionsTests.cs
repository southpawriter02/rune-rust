// ═══════════════════════════════════════════════════════════════════════════════
// UiLoggingExtensionsTests.cs
// Unit tests for UiLoggingExtensions logging patterns.
// Version: 0.13.5d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Extensions;

namespace RuneAndRust.Application.UnitTests.Presentation.ErrorHandling;

/// <summary>
/// Unit tests for <see cref="UiLoggingExtensions"/> logging patterns.
/// </summary>
[TestFixture]
public class UiLoggingExtensionsTests
{
    private Mock<ILogger> _mockLogger = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger>();
        
        // Enable logging for all levels
        _mockLogger
            .Setup(x => x.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOG RENDER START TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LogRenderStart returns a disposable scope.
    /// </summary>
    [Test]
    public void LogRenderStart_ValidLogger_ReturnsDisposable()
    {
        // Act
        using var scope = _mockLogger.Object.LogRenderStart("TestComponent");

        // Assert
        scope.Should().NotBeNull();
        scope.Should().BeAssignableTo<IDisposable>();
    }

    /// <summary>
    /// Verifies LogRenderStart logs at Trace level.
    /// </summary>
    [Test]
    public void LogRenderStart_ValidLogger_LogsAtTraceLevel()
    {
        // Act
        using var scope = _mockLogger.Object.LogRenderStart("HealthBar");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Render starting")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies LogRenderStart handles null logger gracefully.
    /// </summary>
    [Test]
    public void LogRenderStart_NullLogger_ReturnsNonNullDisposable()
    {
        // Arrange
        ILogger? nullLogger = null;

        // Act
        using var scope = nullLogger!.LogRenderStart("Component");

        // Assert
        scope.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies the returned scope can be disposed without error.
    /// </summary>
    [Test]
    public void LogRenderStart_DisposingScope_DoesNotThrow()
    {
        // Arrange
        var scope = _mockLogger.Object.LogRenderStart("TestComponent");

        // Act
        var act = () => scope.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOG RENDER COMPLETE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LogRenderComplete logs at Trace level with duration.
    /// </summary>
    [Test]
    public void LogRenderComplete_ValidInputs_LogsAtTraceLevelWithDuration()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(5.5);

        // Act
        _mockLogger.Object.LogRenderComplete("Component", duration);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Render complete")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies LogRenderComplete handles null logger gracefully.
    /// </summary>
    [Test]
    public void LogRenderComplete_NullLogger_DoesNotThrow()
    {
        // Arrange
        ILogger? nullLogger = null;

        // Act
        var act = () => nullLogger!.LogRenderComplete("Component", TimeSpan.Zero);

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOG RENDER ERROR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LogRenderError logs at Warning level with exception.
    /// </summary>
    [Test]
    public void LogRenderError_ValidInputs_LogsAtWarningLevelWithException()
    {
        // Arrange
        var exception = new FormatException("Test error");

        // Act
        _mockLogger.Object.LogRenderError("HealthBar", exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Render failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies LogRenderError handles null logger gracefully.
    /// </summary>
    [Test]
    public void LogRenderError_NullLogger_DoesNotThrow()
    {
        // Arrange
        ILogger? nullLogger = null;
        var exception = new Exception("Test");

        // Act
        var act = () => nullLogger!.LogRenderError("Component", exception);

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOG STATE CHANGE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LogStateChange logs at Debug level with old and new values.
    /// </summary>
    [Test]
    public void LogStateChange_ValidInputs_LogsAtDebugLevel()
    {
        // Act
        _mockLogger.Object.LogStateChange("HealthBar", "Health", 100, 75);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("State change")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies LogStateChange handles string values.
    /// </summary>
    [Test]
    public void LogStateChange_StringValues_LogsSuccessfully()
    {
        // Act
        var act = () => _mockLogger.Object.LogStateChange("Component", "Status", "Active", "Inactive");

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOG COMPONENT EVENT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LogComponentEvent logs at Debug level.
    /// </summary>
    [Test]
    public void LogComponentEvent_ValidInputs_LogsAtDebugLevel()
    {
        // Act
        _mockLogger.Object.LogComponentEvent("StatusBar", "Activated");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Component event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies LogComponentEvent with details logs successfully.
    /// </summary>
    [Test]
    public void LogComponentEvent_WithDetails_LogsSuccessfully()
    {
        // Act
        var act = () => _mockLogger.Object.LogComponentEvent(
            "GridCell", "Selected", new { X = 5, Y = 3 });

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOG FALLBACK TRIGGERED TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LogFallbackTriggered logs at Warning level.
    /// </summary>
    [Test]
    public void LogFallbackTriggered_ValidInputs_LogsAtWarningLevel()
    {
        // Act
        _mockLogger.Object.LogFallbackTriggered("HealthBar", FallbackType.Placeholder);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fallback triggered")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies LogFallbackTriggered handles null logger gracefully.
    /// </summary>
    [Test]
    public void LogFallbackTriggered_NullLogger_DoesNotThrow()
    {
        // Arrange
        ILogger? nullLogger = null;

        // Act
        var act = () => nullLogger!.LogFallbackTriggered("Component", FallbackType.Empty);

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOG ERROR CLASSIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LogErrorClassification logs Transient errors at Debug level.
    /// </summary>
    [Test]
    public void LogErrorClassification_TransientSeverity_LogsAtDebugLevel()
    {
        // Arrange
        var exception = new TimeoutException("Timeout");

        // Act
        _mockLogger.Object.LogErrorClassification("Component", exception, ErrorSeverity.Transient);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Transient error")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies LogErrorClassification logs Recoverable errors at Warning level.
    /// </summary>
    [Test]
    public void LogErrorClassification_RecoverableSeverity_LogsAtWarningLevel()
    {
        // Arrange
        var exception = new FormatException("Format error");

        // Act
        _mockLogger.Object.LogErrorClassification("Component", exception, ErrorSeverity.Recoverable);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Recoverable error")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies LogErrorClassification logs Critical errors at Error level.
    /// </summary>
    [Test]
    public void LogErrorClassification_CriticalSeverity_LogsAtErrorLevel()
    {
        // Arrange
        var exception = new OutOfMemoryException("OOM");

        // Act
        _mockLogger.Object.LogErrorClassification("Component", exception, ErrorSeverity.Critical);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Critical error")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
