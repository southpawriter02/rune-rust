// ═══════════════════════════════════════════════════════════════════════════════
// RenderErrorHandlerTests.cs
// Unit tests for RenderErrorHandler error classification and handling.
// Version: 0.13.5d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Utilities;

namespace RuneAndRust.Application.UnitTests.Presentation.ErrorHandling;

/// <summary>
/// Unit tests for <see cref="RenderErrorHandler"/> error classification utilities.
/// </summary>
[TestFixture]
public class RenderErrorHandlerTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ERROR SEVERITY CLASSIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies TimeoutException is classified as Transient severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_TimeoutException_ReturnsTransient()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Transient);
    }

    /// <summary>
    /// Verifies FormatException is classified as Recoverable severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_FormatException_ReturnsRecoverable()
    {
        // Arrange
        var exception = new FormatException("Invalid format");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Recoverable);
    }

    /// <summary>
    /// Verifies ArgumentOutOfRangeException is classified as Recoverable severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_ArgumentOutOfRangeException_ReturnsRecoverable()
    {
        // Arrange
        var exception = new ArgumentOutOfRangeException("value", "Value out of range");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Recoverable);
    }

    /// <summary>
    /// Verifies KeyNotFoundException is classified as Recoverable severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_KeyNotFoundException_ReturnsRecoverable()
    {
        // Arrange
        var exception = new KeyNotFoundException("Key not found");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Recoverable);
    }

    /// <summary>
    /// Verifies DivideByZeroException is classified as Recoverable severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_DivideByZeroException_ReturnsRecoverable()
    {
        // Arrange
        var exception = new DivideByZeroException("Division by zero");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Recoverable);
    }

    /// <summary>
    /// Verifies NullReferenceException is classified as Permanent severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_NullReferenceException_ReturnsPermanent()
    {
        // Arrange
        var exception = new NullReferenceException("Object reference was null");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Permanent);
    }

    /// <summary>
    /// Verifies ArgumentNullException is classified as Permanent severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_ArgumentNullException_ReturnsPermanent()
    {
        // Arrange
        var exception = new ArgumentNullException("parameter");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Permanent);
    }

    /// <summary>
    /// Verifies InvalidOperationException is classified as Permanent severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_InvalidOperationException_ReturnsPermanent()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Permanent);
    }

    /// <summary>
    /// Verifies OutOfMemoryException is classified as Critical severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_OutOfMemoryException_ReturnsCritical()
    {
        // Arrange
        var exception = new OutOfMemoryException("Out of memory");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Critical);
    }

    /// <summary>
    /// Verifies ObjectDisposedException is classified as Critical severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_ObjectDisposedException_ReturnsCritical()
    {
        // Arrange
        var exception = new ObjectDisposedException("resource");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Critical);
    }

    /// <summary>
    /// Verifies unknown exceptions default to Recoverable severity.
    /// </summary>
    [Test]
    public void GetErrorSeverity_UnknownException_DefaultsToRecoverable()
    {
        // Arrange
        var exception = new CustomTestException("Custom exception");

        // Act
        var severity = RenderErrorHandler.GetErrorSeverity(exception);

        // Assert
        severity.Should().Be(ErrorSeverity.Recoverable);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SHOULD RETRY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies TimeoutException qualifies for retry.
    /// </summary>
    [Test]
    public void ShouldRetry_TimeoutException_ReturnsTrue()
    {
        // Arrange
        var exception = new TimeoutException("Timed out");

        // Act
        var shouldRetry = RenderErrorHandler.ShouldRetry(exception);

        // Assert
        shouldRetry.Should().BeTrue();
    }

    /// <summary>
    /// Verifies ArgumentException does not qualify for retry.
    /// </summary>
    [Test]
    public void ShouldRetry_ArgumentException_ReturnsFalse()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        var shouldRetry = RenderErrorHandler.ShouldRetry(exception);

        // Assert
        shouldRetry.Should().BeFalse();
    }

    /// <summary>
    /// Verifies ObjectDisposedException does not qualify for retry.
    /// </summary>
    [Test]
    public void ShouldRetry_ObjectDisposedException_ReturnsFalse()
    {
        // Arrange
        var exception = new ObjectDisposedException("resource");

        // Act
        var shouldRetry = RenderErrorHandler.ShouldRetry(exception);

        // Assert
        shouldRetry.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FALLBACK CONTENT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies Empty fallback type returns empty string.
    /// </summary>
    [Test]
    public void GetFallbackContent_Empty_ReturnsEmptyString()
    {
        // Act
        var content = RenderErrorHandler.GetFallbackContent("TestComponent", FallbackType.Empty);

        // Assert
        content.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies Placeholder fallback type returns bracketed component name.
    /// </summary>
    [Test]
    public void GetFallbackContent_Placeholder_ReturnsBracketedName()
    {
        // Act
        var content = RenderErrorHandler.GetFallbackContent("HealthBar", FallbackType.Placeholder);

        // Assert
        content.Should().Be("[HealthBar]");
    }

    /// <summary>
    /// Verifies ErrorMessage fallback type returns exclamation-prefixed name.
    /// </summary>
    [Test]
    public void GetFallbackContent_ErrorMessage_ReturnsExclamationPrefixedName()
    {
        // Act
        var content = RenderErrorHandler.GetFallbackContent("GridCell", FallbackType.ErrorMessage);

        // Assert
        content.Should().Be("[!GridCell]");
    }

    /// <summary>
    /// Verifies LastKnown fallback type returns tilde-prefixed name.
    /// </summary>
    [Test]
    public void GetFallbackContent_LastKnown_ReturnsTildePrefixedName()
    {
        // Act
        var content = RenderErrorHandler.GetFallbackContent("StatusBar", FallbackType.LastKnown);

        // Assert
        content.Should().Be("[~StatusBar]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE ERROR CONTEXT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies CreateErrorContext creates context with correct properties.
    /// </summary>
    [Test]
    public void CreateErrorContext_ValidInputs_CreatesContextWithCorrectProperties()
    {
        // Arrange
        var exception = new FormatException("Invalid format");
        var componentName = "TestComponent";

        // Act
        var context = RenderErrorHandler.CreateErrorContext(exception, componentName);

        // Assert
        context.ComponentName.Should().Be(componentName);
        context.Exception.Should().BeSameAs(exception);
        context.Severity.Should().Be(ErrorSeverity.Recoverable);
        context.IsRecoverable.Should().BeTrue();
        context.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies CreateErrorContext marks critical errors as not recoverable.
    /// </summary>
    [Test]
    public void CreateErrorContext_CriticalException_MarksAsNotRecoverable()
    {
        // Arrange
        var exception = new OutOfMemoryException("Out of memory");
        var componentName = "CriticalComponent";

        // Act
        var context = RenderErrorHandler.CreateErrorContext(exception, componentName);

        // Assert
        context.Severity.Should().Be(ErrorSeverity.Critical);
        context.IsRecoverable.Should().BeFalse();
    }

    /// <summary>
    /// Verifies CreateErrorContext throws on null exception.
    /// </summary>
    [Test]
    public void CreateErrorContext_NullException_ThrowsArgumentNullException()
    {
        // Act
        var act = () => RenderErrorHandler.CreateErrorContext(null!, "Component");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("exception");
    }

    /// <summary>
    /// Verifies CreateErrorContext throws on null component name.
    /// </summary>
    [Test]
    public void CreateErrorContext_NullComponentName_ThrowsArgumentException()
    {
        // Arrange
        var exception = new Exception("Test");

        // Act
        var act = () => RenderErrorHandler.CreateErrorContext(exception, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("componentName");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER TYPES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Custom exception for testing unknown exception handling.
    /// </summary>
    private sealed class CustomTestException : Exception
    {
        public CustomTestException(string message) : base(message) { }
    }
}
