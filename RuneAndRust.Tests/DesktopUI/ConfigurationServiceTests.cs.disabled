using RuneAndRust.DesktopUI.Services;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for ConfigurationService.
/// </summary>
public class ConfigurationServiceTests
{
    [Fact]
    public void ConfigurationService_LoadConfiguration_ReturnsDefaultWhenFileNotExists()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var config = service.LoadConfiguration();

        // Assert
        Assert.NotNull(config);
        Assert.Equal(1.0f, config.MasterVolume);
        Assert.Equal(1.0f, config.SFXVolume);
        Assert.Equal(1.0f, config.MusicVolume);
        Assert.Equal(WindowMode.Windowed, config.WindowMode);
    }

    [Fact]
    public void ConfigurationService_SaveAndLoad_Persists()
    {
        // Arrange
        var service = new ConfigurationService();
        var config = new UIConfiguration
        {
            MasterVolume = 0.7f,
            WindowWidth = 1920,
            WindowHeight = 1080
        };

        // Act
        service.SaveConfiguration(config);
        var loaded = service.LoadConfiguration();

        // Assert
        Assert.Equal(0.7f, loaded.MasterVolume);
        Assert.Equal(1920, loaded.WindowWidth);
        Assert.Equal(1080, loaded.WindowHeight);
    }

    [Fact]
    public async Task ConfigurationService_ValidateConfiguration_ReturnsTrue()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var result = await service.ValidateConfigurationAsync();

        // Assert
        Assert.True(result);
    }
}
