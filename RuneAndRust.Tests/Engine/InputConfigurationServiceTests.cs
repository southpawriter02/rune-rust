using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the InputConfigurationService class (v0.3.9c).
/// Validates JSON loading/saving, key binding operations, and logging behavior.
/// </summary>
public class InputConfigurationServiceTests : IDisposable
{
    private readonly Mock<ILogger<InputConfigurationService>> _mockLogger;
    private readonly InputConfigurationService _sut;
    private readonly string _testConfigPath;

    public InputConfigurationServiceTests()
    {
        _mockLogger = new Mock<ILogger<InputConfigurationService>>();
        _sut = new InputConfigurationService(_mockLogger.Object);
        _testConfigPath = Path.Combine("data", "input_bindings.json");
    }

    public void Dispose()
    {
        // Clean up test config file if created
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    #region LoadBindings Tests

    [Fact]
    public void LoadBindings_ReturnsDefaults_WhenFileNotFound()
    {
        // Arrange - ensure file doesn't exist
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }

        // Act
        _sut.LoadBindings();

        // Assert
        var bindings = _sut.GetAllBindings();
        bindings.Should().NotBeEmpty("default bindings should be loaded when file is missing");
        bindings.Should().ContainKey(ConsoleKey.I, "default bindings should include 'I' for inventory");
    }

    [Fact]
    public void LoadBindings_ParsesValidJson_Successfully()
    {
        // Arrange - create valid config file
        var testJson = """
        {
          "bindings": {
            "Z": "test_command",
            "Y": "another_command"
          }
        }
        """;
        Directory.CreateDirectory("data");
        File.WriteAllText(_testConfigPath, testJson);

        // Act
        _sut.LoadBindings();

        // Assert
        var bindings = _sut.GetAllBindings();
        bindings.Should().ContainKey(ConsoleKey.Z);
        bindings[ConsoleKey.Z].Should().Be("test_command");
        bindings.Should().ContainKey(ConsoleKey.Y);
        bindings[ConsoleKey.Y].Should().Be("another_command");
    }

    [Fact]
    public void LoadBindings_HandlesInvalidJson_Gracefully()
    {
        // Arrange - create invalid JSON file
        Directory.CreateDirectory("data");
        File.WriteAllText(_testConfigPath, "{ this is not valid json }");

        // Act
        _sut.LoadBindings();

        // Assert - should fall back to defaults
        var bindings = _sut.GetAllBindings();
        bindings.Should().NotBeEmpty("should fall back to defaults on invalid JSON");
        VerifyLogLevel(LogLevel.Warning);
    }

    [Fact]
    public void LoadBindings_LogsInfoOnSuccess()
    {
        // Arrange - create valid config file
        var testJson = """
        {
          "bindings": {
            "Z": "test_command"
          }
        }
        """;
        Directory.CreateDirectory("data");
        File.WriteAllText(_testConfigPath, testJson);

        // Act
        _sut.LoadBindings();

        // Assert
        VerifyLogLevel(LogLevel.Information);
        VerifyLogMessageContains("Loaded");
    }

    #endregion

    #region GetCommandForKey Tests

    [Fact]
    public void GetCommandForKey_ReturnsCommand_WhenBound()
    {
        // Arrange
        _sut.SetBinding(ConsoleKey.T, "test_action");

        // Act
        var result = _sut.GetCommandForKey(ConsoleKey.T);

        // Assert
        result.Should().Be("test_action");
    }

    [Fact]
    public void GetCommandForKey_ReturnsNull_WhenNotBound()
    {
        // Act
        var result = _sut.GetCommandForKey(ConsoleKey.F24);  // Unlikely to be bound

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SetBinding Tests

    [Fact]
    public void SetBinding_AddsNewBinding_Successfully()
    {
        // Act
        _sut.SetBinding(ConsoleKey.P, "new_action");

        // Assert
        var bindings = _sut.GetAllBindings();
        bindings.Should().ContainKey(ConsoleKey.P);
        bindings[ConsoleKey.P].Should().Be("new_action");
    }

    [Fact]
    public void SetBinding_OverwritesExisting_Successfully()
    {
        // Arrange
        _sut.SetBinding(ConsoleKey.P, "original_action");

        // Act
        _sut.SetBinding(ConsoleKey.P, "updated_action");

        // Assert
        var bindings = _sut.GetAllBindings();
        bindings[ConsoleKey.P].Should().Be("updated_action");
    }

    [Fact]
    public void SetBinding_LogsBindingChange()
    {
        // Act
        _sut.SetBinding(ConsoleKey.O, "logged_action");

        // Assert
        VerifyLogLevel(LogLevel.Information);
        VerifyLogMessageContains("Bound");
    }

    #endregion

    #region SaveBindings Tests

    [Fact]
    public void SaveBindings_WritesValidJson_ToFile()
    {
        // Arrange
        _sut.SetBinding(ConsoleKey.X, "save_test");

        // Act
        _sut.SaveBindings();

        // Assert
        File.Exists(_testConfigPath).Should().BeTrue("config file should be created");
        var json = File.ReadAllText(_testConfigPath);
        json.Should().Contain("save_test");
        json.Should().Contain("\"X\"");
    }

    #endregion

    #region GetAllBindings Tests

    [Fact]
    public void GetAllBindings_ReturnsReadOnlyDictionary()
    {
        // Arrange
        _sut.SetBinding(ConsoleKey.F1, "readonly_test");

        // Act
        var bindings = _sut.GetAllBindings();

        // Assert - returned dictionary should be read-only (IReadOnlyDictionary)
        bindings.Should().ContainKey(ConsoleKey.F1);
        bindings[ConsoleKey.F1].Should().Be("readonly_test");

        // Verify it's truly read-only by checking the type doesn't support modification
        bindings.Should().BeAssignableTo<IReadOnlyDictionary<ConsoleKey, string>>();
    }

    #endregion

    #region DefaultBindings Tests

    [Fact]
    public void DefaultBindings_ContainsExpectedKeys()
    {
        // Arrange - reset to defaults
        _sut.ResetToDefaults();

        // Act
        var bindings = _sut.GetAllBindings();

        // Assert - verify expected default bindings
        bindings.Should().ContainKey(ConsoleKey.I, "default should have 'I' for inventory");
        bindings.Should().ContainKey(ConsoleKey.N, "default should have 'N' for north");
        bindings.Should().ContainKey(ConsoleKey.S, "default should have 'S' for south");
        bindings.Should().ContainKey(ConsoleKey.E, "default should have 'E' for east");
        bindings.Should().ContainKey(ConsoleKey.W, "default should have 'W' for west");
        bindings.Should().ContainKey(ConsoleKey.L, "default should have 'L' for look");
        bindings.Should().ContainKey(ConsoleKey.Spacebar, "default should have 'Spacebar' for wait");

        // Verify expected command mappings
        bindings[ConsoleKey.I].Should().Be("inventory");
        bindings[ConsoleKey.N].Should().Be("north");
        bindings[ConsoleKey.L].Should().Be("look");
    }

    #endregion

    #region Helper Methods

    private void VerifyLogLevel(LogLevel level)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogMessageContains(string substring)
    {
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(substring)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
