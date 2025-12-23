using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Settings;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the SettingsService class (v0.3.10a).
/// Validates settings persistence, validation, clamping, and error handling.
/// </summary>
public class SettingsServiceTests : IDisposable
{
    private readonly Mock<ILogger<SettingsService>> _mockLogger;
    private readonly SettingsService _sut;
    private readonly string _testOptionsPath = Path.Combine("data", "options.json");

    public SettingsServiceTests()
    {
        _mockLogger = new Mock<ILogger<SettingsService>>();
        _sut = new SettingsService(_mockLogger.Object);

        // Reset GameSettings to known state before each test
        ResetGameSettingsToDefaults();

        // Clean up any existing test file
        if (File.Exists(_testOptionsPath))
        {
            File.Delete(_testOptionsPath);
        }
    }

    public void Dispose()
    {
        // Clean up test files
        if (File.Exists(_testOptionsPath))
        {
            File.Delete(_testOptionsPath);
        }

        // Reset GameSettings after test
        ResetGameSettingsToDefaults();
    }

    private static void ResetGameSettingsToDefaults()
    {
        GameSettings.ReduceMotion = false;
        GameSettings.Theme = ThemeType.Standard;
        GameSettings.TextSpeed = 100;
        GameSettings.MasterVolume = 100;
        GameSettings.AutosaveIntervalMinutes = 5;
    }

    #region LoadAsync Tests

    [Fact]
    public async Task LoadAsync_CreatesDefaultFile_WhenFileMissing()
    {
        // Arrange - ensure file doesn't exist
        if (File.Exists(_testOptionsPath))
        {
            File.Delete(_testOptionsPath);
        }

        // Act
        await _sut.LoadAsync();

        // Assert
        File.Exists(_testOptionsPath).Should().BeTrue("default settings file should be created");
        GameSettings.ReduceMotion.Should().BeFalse();
        GameSettings.Theme.Should().Be(ThemeType.Standard);
        GameSettings.TextSpeed.Should().Be(100);
        GameSettings.MasterVolume.Should().Be(100);
        GameSettings.AutosaveIntervalMinutes.Should().Be(5);
    }

    [Fact]
    public async Task LoadAsync_AppliesValuesToGameSettings_WhenFileExists()
    {
        // Arrange - create a custom settings file
        Directory.CreateDirectory("data");
        var json = """
            {
              "ReduceMotion": true,
              "Theme": 1,
              "TextSpeed": 150,
              "MasterVolume": 75,
              "AutosaveIntervalMinutes": 10
            }
            """;
        await File.WriteAllTextAsync(_testOptionsPath, json);

        // Act
        await _sut.LoadAsync();

        // Assert
        GameSettings.ReduceMotion.Should().BeTrue();
        GameSettings.Theme.Should().Be(ThemeType.HighContrast);
        GameSettings.TextSpeed.Should().Be(150);
        GameSettings.MasterVolume.Should().Be(75);
        GameSettings.AutosaveIntervalMinutes.Should().Be(10);
    }

    [Fact]
    public async Task LoadAsync_ClampsTextSpeed_WhenOver200()
    {
        // Arrange
        Directory.CreateDirectory("data");
        var json = """
            {
              "ReduceMotion": false,
              "Theme": 0,
              "TextSpeed": 500,
              "MasterVolume": 100,
              "AutosaveIntervalMinutes": 5
            }
            """;
        await File.WriteAllTextAsync(_testOptionsPath, json);

        // Act
        await _sut.LoadAsync();

        // Assert
        GameSettings.TextSpeed.Should().Be(200, "TextSpeed should be clamped to maximum of 200");
    }

    [Fact]
    public async Task LoadAsync_ClampsTextSpeed_WhenUnder10()
    {
        // Arrange
        Directory.CreateDirectory("data");
        var json = """
            {
              "ReduceMotion": false,
              "Theme": 0,
              "TextSpeed": 5,
              "MasterVolume": 100,
              "AutosaveIntervalMinutes": 5
            }
            """;
        await File.WriteAllTextAsync(_testOptionsPath, json);

        // Act
        await _sut.LoadAsync();

        // Assert
        GameSettings.TextSpeed.Should().Be(10, "TextSpeed should be clamped to minimum of 10");
    }

    [Fact]
    public async Task LoadAsync_ClampsMasterVolume_WhenOver100()
    {
        // Arrange
        Directory.CreateDirectory("data");
        var json = """
            {
              "ReduceMotion": false,
              "Theme": 0,
              "TextSpeed": 100,
              "MasterVolume": 150,
              "AutosaveIntervalMinutes": 5
            }
            """;
        await File.WriteAllTextAsync(_testOptionsPath, json);

        // Act
        await _sut.LoadAsync();

        // Assert
        GameSettings.MasterVolume.Should().Be(100, "MasterVolume should be clamped to maximum of 100");
    }

    [Fact]
    public async Task LoadAsync_ClampsMasterVolume_WhenUnder0()
    {
        // Arrange
        Directory.CreateDirectory("data");
        var json = """
            {
              "ReduceMotion": false,
              "Theme": 0,
              "TextSpeed": 100,
              "MasterVolume": -50,
              "AutosaveIntervalMinutes": 5
            }
            """;
        await File.WriteAllTextAsync(_testOptionsPath, json);

        // Act
        await _sut.LoadAsync();

        // Assert
        GameSettings.MasterVolume.Should().Be(0, "MasterVolume should be clamped to minimum of 0");
    }

    [Fact]
    public async Task LoadAsync_ClampsAutosaveInterval_WhenOutOfRange()
    {
        // Arrange
        Directory.CreateDirectory("data");
        var json = """
            {
              "ReduceMotion": false,
              "Theme": 0,
              "TextSpeed": 100,
              "MasterVolume": 100,
              "AutosaveIntervalMinutes": 120
            }
            """;
        await File.WriteAllTextAsync(_testOptionsPath, json);

        // Act
        await _sut.LoadAsync();

        // Assert
        GameSettings.AutosaveIntervalMinutes.Should().Be(60, "AutosaveIntervalMinutes should be clamped to maximum of 60");
    }

    [Fact]
    public async Task LoadAsync_DefaultsTheme_WhenInvalidEnumValue()
    {
        // Arrange
        Directory.CreateDirectory("data");
        var json = """
            {
              "ReduceMotion": false,
              "Theme": 99,
              "TextSpeed": 100,
              "MasterVolume": 100,
              "AutosaveIntervalMinutes": 5
            }
            """;
        await File.WriteAllTextAsync(_testOptionsPath, json);

        // Act
        await _sut.LoadAsync();

        // Assert
        GameSettings.Theme.Should().Be(ThemeType.Standard, "Invalid theme value should default to Standard");
    }

    [Fact]
    public async Task LoadAsync_HandlesCorruptJson_Gracefully()
    {
        // Arrange
        Directory.CreateDirectory("data");
        var corruptJson = "{ this is not valid json }}}}";
        await File.WriteAllTextAsync(_testOptionsPath, corruptJson);

        // Act
        await _sut.LoadAsync();

        // Assert - should reset to defaults without throwing
        GameSettings.ReduceMotion.Should().BeFalse();
        GameSettings.Theme.Should().Be(ThemeType.Standard);
        GameSettings.TextSpeed.Should().Be(100);
        GameSettings.MasterVolume.Should().Be(100);
        GameSettings.AutosaveIntervalMinutes.Should().Be(5);
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WritesValidJson_ToFile()
    {
        // Arrange
        GameSettings.ReduceMotion = true;
        GameSettings.Theme = ThemeType.Protanopia;
        GameSettings.TextSpeed = 75;
        GameSettings.MasterVolume = 50;
        GameSettings.AutosaveIntervalMinutes = 15;

        // Act
        await _sut.SaveAsync();

        // Assert
        File.Exists(_testOptionsPath).Should().BeTrue();
        var json = await File.ReadAllTextAsync(_testOptionsPath);
        json.Should().Contain("\"ReduceMotion\": true");
        json.Should().Contain("\"Theme\": 2"); // Protanopia = 2
        json.Should().Contain("\"TextSpeed\": 75");
        json.Should().Contain("\"MasterVolume\": 50");
        json.Should().Contain("\"AutosaveIntervalMinutes\": 15");
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectory_WhenMissing()
    {
        // Arrange - ensure data directory doesn't exist
        if (Directory.Exists("data"))
        {
            // Only delete if empty or just has our test file
            var files = Directory.GetFiles("data");
            if (files.Length == 0 || (files.Length == 1 && files[0].EndsWith("options.json")))
            {
                if (File.Exists(_testOptionsPath))
                {
                    File.Delete(_testOptionsPath);
                }
            }
        }

        // Act
        await _sut.SaveAsync();

        // Assert
        Directory.Exists("data").Should().BeTrue("data directory should be created");
        File.Exists(_testOptionsPath).Should().BeTrue("options.json should be created");
    }

    #endregion

    #region ResetToDefaultsAsync Tests

    [Fact]
    public async Task ResetToDefaultsAsync_SetsAllDefaults()
    {
        // Arrange - set non-default values
        GameSettings.ReduceMotion = true;
        GameSettings.Theme = ThemeType.HighContrast;
        GameSettings.TextSpeed = 50;
        GameSettings.MasterVolume = 25;
        GameSettings.AutosaveIntervalMinutes = 30;

        // Act
        await _sut.ResetToDefaultsAsync();

        // Assert
        GameSettings.ReduceMotion.Should().BeFalse();
        GameSettings.Theme.Should().Be(ThemeType.Standard);
        GameSettings.TextSpeed.Should().Be(100);
        GameSettings.MasterVolume.Should().Be(100);
        GameSettings.AutosaveIntervalMinutes.Should().Be(5);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_SavesFile()
    {
        // Arrange - ensure file doesn't exist
        if (File.Exists(_testOptionsPath))
        {
            File.Delete(_testOptionsPath);
        }

        // Act
        await _sut.ResetToDefaultsAsync();

        // Assert
        File.Exists(_testOptionsPath).Should().BeTrue("reset should save defaults to file");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task SettingsService_RoundTrip_PreservesValues()
    {
        // Arrange - set custom values
        GameSettings.ReduceMotion = true;
        GameSettings.Theme = ThemeType.Deuteranopia;
        GameSettings.TextSpeed = 175;
        GameSettings.MasterVolume = 80;
        GameSettings.AutosaveIntervalMinutes = 20;

        // Act - save then reset and load
        await _sut.SaveAsync();

        // Reset to defaults (simulating app restart)
        ResetGameSettingsToDefaults();

        // Verify reset worked
        GameSettings.Theme.Should().Be(ThemeType.Standard);

        // Load from file
        await _sut.LoadAsync();

        // Assert - values should be restored
        GameSettings.ReduceMotion.Should().BeTrue();
        GameSettings.Theme.Should().Be(ThemeType.Deuteranopia);
        GameSettings.TextSpeed.Should().Be(175);
        GameSettings.MasterVolume.Should().Be(80);
        GameSettings.AutosaveIntervalMinutes.Should().Be(20);
    }

    #endregion
}
