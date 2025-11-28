using RuneAndRust.DesktopUI.Services;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for ConfigurationService.
/// v0.43.21: Updated tests for current ConfigurationService with GameConfiguration model.
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
        Assert.NotNull(config.Audio);
        Assert.NotNull(config.Graphics);
        Assert.NotNull(config.Gameplay);
        Assert.NotNull(config.Accessibility);
        Assert.NotNull(config.Controls);
    }

    [Fact]
    public void ConfigurationService_GetDefaultConfiguration_ReturnsValidDefaults()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var config = service.GetDefaultConfiguration();

        // Assert - Audio defaults
        Assert.Equal(1.0f, config.Audio.MasterVolume);
        Assert.Equal(0.8f, config.Audio.MusicVolume);
        Assert.Equal(1.0f, config.Audio.SFXVolume);
        Assert.False(config.Audio.IsMuted);

        // Assert - Graphics defaults
        Assert.Equal(WindowMode.Windowed, config.Graphics.WindowMode);
        Assert.Equal(1280, config.Graphics.WindowWidth);
        Assert.Equal(720, config.Graphics.WindowHeight);
        Assert.True(config.Graphics.VSync);
        Assert.Equal(60, config.Graphics.TargetFPS);

        // Assert - Gameplay defaults
        Assert.True(config.Gameplay.AutoSave);
        Assert.Equal(5, config.Gameplay.AutoSaveInterval);
        Assert.True(config.Gameplay.ShowDamageNumbers);
        Assert.Equal(1.0f, config.Gameplay.CombatSpeed);

        // Assert - Accessibility defaults
        Assert.Equal(1.0f, config.Accessibility.UIScale);
        Assert.False(config.Accessibility.ColorblindMode);
        Assert.False(config.Accessibility.ReducedMotion);

        // Assert - Controls defaults
        Assert.True(config.Controls.ShowKeyboardHints);
        Assert.Equal(1.0f, config.Controls.MouseSensitivity);
    }

    [Fact]
    public void ConfigurationService_SaveAndLoad_Persists()
    {
        // Arrange
        var service = new ConfigurationService();
        var config = service.GetDefaultConfiguration();
        config.Audio.MasterVolume = 0.7f;
        config.Graphics.WindowWidth = 1920;
        config.Graphics.WindowHeight = 1080;
        config.Gameplay.AutoSave = false;

        // Act
        service.SaveConfiguration(config);
        var loaded = service.LoadConfiguration();

        // Assert
        Assert.Equal(0.7f, loaded.Audio.MasterVolume);
        Assert.Equal(1920, loaded.Graphics.WindowWidth);
        Assert.Equal(1080, loaded.Graphics.WindowHeight);
        Assert.False(loaded.Gameplay.AutoSave);
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

    [Fact]
    public void ConfigurationService_ResetToDefaults_RestoresDefaultValues()
    {
        // Arrange
        var service = new ConfigurationService();
        var config = service.GetDefaultConfiguration();
        config.Audio.MasterVolume = 0.3f;
        config.Graphics.Fullscreen = true;
        service.SaveConfiguration(config);

        // Act
        service.ResetToDefaults();
        var loaded = service.LoadConfiguration();

        // Assert
        Assert.Equal(1.0f, loaded.Audio.MasterVolume);
        Assert.False(loaded.Graphics.Fullscreen);
    }

    [Fact]
    public void ConfigurationService_SaveConfiguration_RaisesConfigurationChangedEvent()
    {
        // Arrange
        var service = new ConfigurationService();
        GameConfiguration? receivedConfig = null;
        service.ConfigurationChanged += (s, c) => receivedConfig = c;

        var config = service.GetDefaultConfiguration();
        config.Audio.MasterVolume = 0.5f;

        // Act
        service.SaveConfiguration(config);

        // Assert
        Assert.NotNull(receivedConfig);
        Assert.Equal(0.5f, receivedConfig.Audio.MasterVolume);
    }

    [Fact]
    public void ConfigurationService_SaveConfiguration_ValidatesAndFixesOutOfRangeValues()
    {
        // Arrange
        var service = new ConfigurationService();
        var config = service.GetDefaultConfiguration();
        config.Audio.MasterVolume = 1.5f;  // Out of range - should be clamped
        config.Audio.SFXVolume = -0.5f;    // Out of range - should be clamped
        config.Graphics.TargetFPS = 300;   // Out of range - should be clamped

        // Act
        service.SaveConfiguration(config);
        var loaded = service.LoadConfiguration();

        // Assert - Values should be clamped to valid ranges
        Assert.Equal(1.0f, loaded.Audio.MasterVolume);
        Assert.Equal(0.0f, loaded.Audio.SFXVolume);
        Assert.Equal(240, loaded.Graphics.TargetFPS);  // Max is 240
    }

    [Fact]
    public void ConfigurationService_LoadConfiguration_InitializesMissingSubConfigs()
    {
        // This tests that even if sub-configs are null, they get initialized
        // Arrange
        var service = new ConfigurationService();

        // Act
        var config = service.LoadConfiguration();

        // Assert - All sub-configs should be non-null
        Assert.NotNull(config.Graphics);
        Assert.NotNull(config.Audio);
        Assert.NotNull(config.Gameplay);
        Assert.NotNull(config.Accessibility);
        Assert.NotNull(config.Controls);
    }

    [Fact]
    public void ConfigurationService_GraphicsConfig_HasCorrectDefaults()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var config = service.GetDefaultConfiguration();

        // Assert
        Assert.False(config.Graphics.Fullscreen);
        Assert.True(config.Graphics.VSync);
        Assert.Equal(60, config.Graphics.TargetFPS);
        Assert.Equal(WindowMode.Windowed, config.Graphics.WindowMode);
        Assert.Equal("1280x720", config.Graphics.Resolution);
        Assert.Equal(2, config.Graphics.QualityLevel);  // High quality default
        Assert.True(config.Graphics.ParticleEffects);
        Assert.True(config.Graphics.ScreenShake);
    }

    [Fact]
    public void ConfigurationService_AudioConfig_HasCorrectDefaults()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var config = service.GetDefaultConfiguration();

        // Assert
        Assert.Equal(1.0f, config.Audio.MasterVolume);
        Assert.Equal(0.8f, config.Audio.MusicVolume);
        Assert.Equal(1.0f, config.Audio.SFXVolume);
        Assert.Equal(1.0f, config.Audio.UIVolume);
        Assert.Equal(0.7f, config.Audio.AmbientVolume);
        Assert.False(config.Audio.IsMuted);
    }

    [Fact]
    public void ConfigurationService_GameplayConfig_HasCorrectDefaults()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var config = service.GetDefaultConfiguration();

        // Assert
        Assert.True(config.Gameplay.AutoSave);
        Assert.Equal(5, config.Gameplay.AutoSaveInterval);
        Assert.True(config.Gameplay.ShowDamageNumbers);
        Assert.True(config.Gameplay.ShowHitConfirmation);
        Assert.True(config.Gameplay.PauseOnFocusLost);
        Assert.False(config.Gameplay.ShowGridCoordinates);
        Assert.Equal(1.0f, config.Gameplay.CombatSpeed);
        Assert.True(config.Gameplay.AllowAnimationSkip);
        Assert.True(config.Gameplay.ShowTutorialHints);
        Assert.False(config.Gameplay.ConfirmEndTurn);
    }

    [Fact]
    public void ConfigurationService_AccessibilityConfig_HasCorrectDefaults()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var config = service.GetDefaultConfiguration();

        // Assert
        Assert.Equal(1.0f, config.Accessibility.UIScale);
        Assert.False(config.Accessibility.ColorblindMode);
        Assert.Equal(ColorblindType.None, config.Accessibility.ColorblindType);
        Assert.False(config.Accessibility.ReducedMotion);
        Assert.False(config.Accessibility.HighContrast);
        Assert.Equal(1.0f, config.Accessibility.FontScale);
        Assert.False(config.Accessibility.ShowSubtitles);
        Assert.False(config.Accessibility.ScreenReaderSupport);
        Assert.False(config.Accessibility.VisualAudioCues);
    }

    [Fact]
    public void ConfigurationService_ControlsConfig_HasCorrectDefaults()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var config = service.GetDefaultConfiguration();

        // Assert
        Assert.True(config.Controls.ShowKeyboardHints);
        Assert.Equal(1.0f, config.Controls.MouseSensitivity);
        Assert.False(config.Controls.InvertScroll);
        Assert.True(config.Controls.EdgeScrolling);
        Assert.False(config.Controls.DoubleClickConfirm);
    }

    [Fact]
    public void ConfigurationService_AutoSaveInterval_ClampedToValidRange()
    {
        // Arrange
        var service = new ConfigurationService();
        var config = service.GetDefaultConfiguration();

        // Test lower bound
        config.Gameplay.AutoSaveInterval = 0;
        service.SaveConfiguration(config);
        var loaded = service.LoadConfiguration();
        Assert.Equal(1, loaded.Gameplay.AutoSaveInterval);

        // Test upper bound
        config.Gameplay.AutoSaveInterval = 100;
        service.SaveConfiguration(config);
        loaded = service.LoadConfiguration();
        Assert.Equal(30, loaded.Gameplay.AutoSaveInterval);
    }

    [Fact]
    public void ConfigurationService_CombatSpeed_ClampedToValidRange()
    {
        // Arrange
        var service = new ConfigurationService();
        var config = service.GetDefaultConfiguration();

        // Test lower bound
        config.Gameplay.CombatSpeed = 0.1f;
        service.SaveConfiguration(config);
        var loaded = service.LoadConfiguration();
        Assert.Equal(0.5f, loaded.Gameplay.CombatSpeed);

        // Test upper bound
        config.Gameplay.CombatSpeed = 5.0f;
        service.SaveConfiguration(config);
        loaded = service.LoadConfiguration();
        Assert.Equal(2.0f, loaded.Gameplay.CombatSpeed);
    }

    [Fact]
    public void ConfigurationService_UIScale_ClampedToValidRange()
    {
        // Arrange
        var service = new ConfigurationService();
        var config = service.GetDefaultConfiguration();

        // Test lower bound
        config.Accessibility.UIScale = 0.5f;
        service.SaveConfiguration(config);
        var loaded = service.LoadConfiguration();
        Assert.Equal(0.8f, loaded.Accessibility.UIScale);

        // Test upper bound
        config.Accessibility.UIScale = 2.0f;
        service.SaveConfiguration(config);
        loaded = service.LoadConfiguration();
        Assert.Equal(1.5f, loaded.Accessibility.UIScale);
    }

    [Fact]
    public void ConfigurationService_MouseSensitivity_ClampedToValidRange()
    {
        // Arrange
        var service = new ConfigurationService();
        var config = service.GetDefaultConfiguration();

        // Test lower bound
        config.Controls.MouseSensitivity = 0.01f;
        service.SaveConfiguration(config);
        var loaded = service.LoadConfiguration();
        Assert.Equal(0.1f, loaded.Controls.MouseSensitivity);

        // Test upper bound
        config.Controls.MouseSensitivity = 10.0f;
        service.SaveConfiguration(config);
        loaded = service.LoadConfiguration();
        Assert.Equal(3.0f, loaded.Controls.MouseSensitivity);
    }

    [Fact]
    public void WindowMode_Enum_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)WindowMode.Windowed);
        Assert.Equal(1, (int)WindowMode.Borderless);
        Assert.Equal(2, (int)WindowMode.Fullscreen);
    }

    [Fact]
    public void ColorblindType_Enum_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)ColorblindType.None);
        Assert.Equal(1, (int)ColorblindType.Protanopia);
        Assert.Equal(2, (int)ColorblindType.Deuteranopia);
        Assert.Equal(3, (int)ColorblindType.Tritanopia);
    }
}
