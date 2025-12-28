using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Terminal.Services;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for the TerminalService (v0.3.23c).
/// Tests mouse mode enable/disable and terminal capability detection.
/// </summary>
/// <remarks>
/// Note: These tests focus on state management. Actual VT escape code emission
/// is not verified as Console.Write cannot be easily intercepted in tests.
/// </remarks>
public class TerminalServiceTests
{
    private readonly ILogger<TerminalService> _logger;

    public TerminalServiceTests()
    {
        _logger = Substitute.For<ILogger<TerminalService>>();
    }

    #region EnableMouseMode Tests

    [Fact]
    public void EnableMouseMode_SetsIsMouseEnabled_WhenSupported()
    {
        // Arrange - Force mouse support by setting WT_SESSION
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", "test-session");
            var service = new TerminalService(_logger);

            // Act
            service.EnableMouseMode();

            // Assert
            service.IsMouseEnabled.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
        }
    }

    [Fact]
    public void EnableMouseMode_IsIdempotent()
    {
        // Arrange
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", "test-session");
            var service = new TerminalService(_logger);

            // Act - Enable twice
            service.EnableMouseMode();
            service.EnableMouseMode();

            // Assert - Should still be enabled
            service.IsMouseEnabled.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
        }
    }

    #endregion

    #region DisableMouseMode Tests

    [Fact]
    public void DisableMouseMode_ClearsIsMouseEnabled()
    {
        // Arrange
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", "test-session");
            var service = new TerminalService(_logger);
            service.EnableMouseMode();
            service.IsMouseEnabled.Should().BeTrue();

            // Act
            service.DisableMouseMode();

            // Assert
            service.IsMouseEnabled.Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
        }
    }

    [Fact]
    public void DisableMouseMode_IsIdempotent()
    {
        // Arrange
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", "test-session");
            var service = new TerminalService(_logger);
            service.EnableMouseMode();

            // Act - Disable twice
            service.DisableMouseMode();
            service.DisableMouseMode();

            // Assert - Should still be disabled
            service.IsMouseEnabled.Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
        }
    }

    [Fact]
    public void DisableMouseMode_SafeWhenNeverEnabled()
    {
        // Arrange
        var service = new TerminalService(_logger);

        // Act - Should not throw
        service.DisableMouseMode();

        // Assert
        service.IsMouseEnabled.Should().BeFalse();
    }

    #endregion

    #region IsMouseSupported Tests

    [Fact]
    public void IsMouseSupported_ReturnsTrue_ForWindowsTerminal()
    {
        // Arrange
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", "test-session");
            var service = new TerminalService(_logger);

            // Act
            var result = service.IsMouseSupported();

            // Assert
            result.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
        }
    }

    [Fact]
    public void IsMouseSupported_ReturnsTrue_ForXterm()
    {
        // Arrange
        var originalTerm = Environment.GetEnvironmentVariable("TERM");
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", null);
            Environment.SetEnvironmentVariable("TERM", "xterm-256color");
            var service = new TerminalService(_logger);

            // Act
            var result = service.IsMouseSupported();

            // Assert
            result.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", originalTerm);
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
        }
    }

    [Fact]
    public void IsMouseSupported_ReturnsTrue_ForVSCode()
    {
        // Arrange
        var originalTermProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        var originalTerm = Environment.GetEnvironmentVariable("TERM");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", null);
            Environment.SetEnvironmentVariable("TERM", "dumb");
            Environment.SetEnvironmentVariable("TERM_PROGRAM", "vscode");
            var service = new TerminalService(_logger);

            // Act
            var result = service.IsMouseSupported();

            // Assert
            result.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM_PROGRAM", originalTermProgram);
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
            Environment.SetEnvironmentVariable("TERM", originalTerm);
        }
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_DisablesMouseMode()
    {
        // Arrange
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", "test-session");
            var service = new TerminalService(_logger);
            service.EnableMouseMode();
            service.IsMouseEnabled.Should().BeTrue();

            // Act
            service.Dispose();

            // Assert
            service.IsMouseEnabled.Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
        }
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        // Arrange
        var originalWtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("WT_SESSION", "test-session");
            var service = new TerminalService(_logger);
            service.EnableMouseMode();

            // Act - Dispose twice
            service.Dispose();
            service.Dispose();

            // Assert - Should not throw
            service.IsMouseEnabled.Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable("WT_SESSION", originalWtSession);
        }
    }

    #endregion

    #region Initial State Tests

    [Fact]
    public void Constructor_StartsWithMouseDisabled()
    {
        // Arrange & Act
        var service = new TerminalService(_logger);

        // Assert
        service.IsMouseEnabled.Should().BeFalse();
    }

    #endregion
}
