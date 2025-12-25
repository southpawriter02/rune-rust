using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for CrashService (v0.3.16a).
/// Validates crash report generation, file writing, and formatting.
/// </summary>
public class CrashServiceTests : IDisposable
{
    private readonly ILogger<CrashService> _mockLogger;
    private readonly CrashService _sut;
    private readonly string _crashDir = "logs/crashes";

    public CrashServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<CrashService>>();
        _sut = new CrashService(_mockLogger);

        // Clean up crash directory before tests
        CleanupCrashFiles();
    }

    public void Dispose()
    {
        // Clean up test-generated crash files
        CleanupCrashFiles();
    }

    private void CleanupCrashFiles()
    {
        if (Directory.Exists(_crashDir))
        {
            foreach (var file in Directory.GetFiles(_crashDir, "crash_*.txt"))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignore cleanup failures
                }
            }
        }
    }

    #region GenerateReportPath Tests

    [Fact]
    public void GenerateReportPath_CreatesValidFilename()
    {
        // Arrange
        var timestamp = new DateTime(2025, 12, 25, 14, 30, 45);

        // Act
        var path = _sut.GenerateReportPath(timestamp);

        // Assert
        path.Should().Be(Path.Combine("logs", "crashes", "crash_20251225_143045.txt"));
    }

    [Fact]
    public void GenerateReportPath_PadsZeros_ForSingleDigitValues()
    {
        // Arrange
        var timestamp = new DateTime(2025, 1, 5, 9, 3, 7);

        // Act
        var path = _sut.GenerateReportPath(timestamp);

        // Assert
        path.Should().Contain("crash_20250105_090307.txt");
    }

    #endregion

    #region LogCrash Tests

    [Fact]
    public void LogCrash_CreatesDirectory_IfMissing()
    {
        // Arrange
        if (Directory.Exists(_crashDir))
            Directory.Delete(_crashDir, true);

        var ex = new InvalidOperationException("Test exception");

        // Act
        _sut.LogCrash(ex);

        // Assert
        Directory.Exists(_crashDir).Should().BeTrue();
    }

    [Fact]
    public void LogCrash_WritesFile_WithExceptionDetails()
    {
        // Arrange
        var ex = new ArgumentNullException("testParam", "Test null reference");

        // Act
        var path = _sut.LogCrash(ex);

        // Assert
        File.Exists(path).Should().BeTrue();
        var content = File.ReadAllText(path);
        content.Should().Contain("ArgumentNullException");
        content.Should().Contain("Test null reference");
        content.Should().Contain("testParam");
    }

    [Fact]
    public void LogCrash_IncludesSystemInfo()
    {
        // Arrange
        var ex = new Exception("System info test");

        // Act
        var path = _sut.LogCrash(ex);

        // Assert
        var content = File.ReadAllText(path);
        content.Should().Contain("v0.3.16a");
        content.Should().Contain(Environment.OSVersion.ToString());
        content.Should().Contain(Environment.Version.ToString());
    }

    [Fact]
    public void LogCrash_HandlesNestedExceptions()
    {
        // Arrange
        var inner = new InvalidOperationException("Inner error");
        var outer = new ApplicationException("Outer error", inner);

        // Act
        var path = _sut.LogCrash(outer);

        // Assert
        var content = File.ReadAllText(path);
        content.Should().Contain("Outer error");
        content.Should().Contain("INNER EXCEPTION");
        content.Should().Contain("Inner error");
    }

    [Fact]
    public void LogCrash_ReturnsValidPath()
    {
        // Arrange
        var ex = new Exception("Path test");

        // Act
        var path = _sut.LogCrash(ex);

        // Assert
        path.Should().StartWith(_crashDir);
        path.Should().EndWith(".txt");
        path.Should().Contain("crash_");
    }

    [Fact]
    public void LogCrash_IncludesStackTrace()
    {
        // Arrange
        Exception? capturedException = null;
        try
        {
            ThrowNestedException();
        }
        catch (Exception ex)
        {
            capturedException = ex;
        }

        // Act
        var path = _sut.LogCrash(capturedException!);

        // Assert
        var content = File.ReadAllText(path);
        content.Should().Contain("STACK TRACE:");
        content.Should().Contain("ThrowNestedException");
    }

    [Fact]
    public void LogCrash_IncludesReportingUrl()
    {
        // Arrange
        var ex = new Exception("URL test");

        // Act
        var path = _sut.LogCrash(ex);

        // Assert
        var content = File.ReadAllText(path);
        content.Should().Contain("https://github.com/southpawriter02/rune-rust/issues");
    }

    [Fact]
    public void LogCrash_HandlesNullStackTrace()
    {
        // Arrange - Create exception without throwing (no stack trace)
        var ex = new Exception("No stack trace");

        // Act
        var path = _sut.LogCrash(ex);

        // Assert
        var content = File.ReadAllText(path);
        content.Should().Contain("No stack trace available.");
    }

    #endregion

    #region Helper Methods

    private static void ThrowNestedException()
    {
        throw new InvalidOperationException("Deliberately thrown for stack trace test");
    }

    #endregion
}
