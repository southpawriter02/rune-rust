using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.18: Implementation of configuration service.
/// Manages persistent storage of game configuration settings.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private const string ConfigFileName = "game_config.json";
    private readonly string _configPath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Event raised when configuration changes.
    /// </summary>
    public event EventHandler<GameConfiguration>? ConfigurationChanged;

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "RuneAndRust");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, ConfigFileName);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        Console.WriteLine($"[CONFIG] Configuration path: {_configPath}");
    }

    /// <inheritdoc/>
    public GameConfiguration LoadConfiguration()
    {
        if (!File.Exists(_configPath))
        {
            Console.WriteLine("[CONFIG] No configuration file found, using defaults");
            return GetDefaultConfiguration();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<GameConfiguration>(json, _jsonOptions);

            if (config == null)
            {
                Console.WriteLine("[CONFIG] Failed to deserialize configuration, using defaults");
                return GetDefaultConfiguration();
            }

            Console.WriteLine("[CONFIG] Configuration loaded successfully");
            return ValidateAndFixConfiguration(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONFIG] Error loading configuration: {ex.Message}");
            return GetDefaultConfiguration();
        }
    }

    /// <inheritdoc/>
    public void SaveConfiguration(GameConfiguration config)
    {
        try
        {
            var validatedConfig = ValidateAndFixConfiguration(config);
            var json = JsonSerializer.Serialize(validatedConfig, _jsonOptions);
            File.WriteAllText(_configPath, json);

            Console.WriteLine("[CONFIG] Configuration saved successfully");
            ConfigurationChanged?.Invoke(this, validatedConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONFIG] Error saving configuration: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public void ResetToDefaults()
    {
        var defaults = GetDefaultConfiguration();
        SaveConfiguration(defaults);
        Console.WriteLine("[CONFIG] Configuration reset to defaults");
    }

    /// <inheritdoc/>
    public GameConfiguration GetDefaultConfiguration()
    {
        return new GameConfiguration
        {
            Graphics = new GraphicsConfig
            {
                Fullscreen = false,
                VSync = true,
                TargetFPS = 60,
                WindowMode = WindowMode.Windowed,
                WindowWidth = 1280,
                WindowHeight = 720,
                Resolution = "1280x720",
                QualityLevel = 2,
                ParticleEffects = true,
                ScreenShake = true
            },
            Audio = new AudioConfig
            {
                MasterVolume = 1.0f,
                MusicVolume = 0.8f,
                SFXVolume = 1.0f,
                UIVolume = 1.0f,
                AmbientVolume = 0.7f,
                IsMuted = false
            },
            Gameplay = new GameplayConfig
            {
                AutoSave = true,
                AutoSaveInterval = 5,
                ShowDamageNumbers = true,
                ShowHitConfirmation = true,
                PauseOnFocusLost = true,
                ShowGridCoordinates = false,
                CombatSpeed = 1.0f,
                AllowAnimationSkip = true,
                ShowTutorialHints = true,
                ConfirmEndTurn = false
            },
            Accessibility = new AccessibilityConfig
            {
                UIScale = 1.0f,
                ColorblindMode = false,
                ColorblindType = ColorblindType.None,
                ReducedMotion = false,
                HighContrast = false,
                FontScale = 1.0f,
                ShowSubtitles = false,
                ScreenReaderSupport = false,
                VisualAudioCues = false
            },
            Controls = new ControlsConfig
            {
                ShowKeyboardHints = true,
                MouseSensitivity = 1.0f,
                InvertScroll = false,
                EdgeScrolling = true,
                DoubleClickConfirm = false
            }
        };
    }

    /// <inheritdoc/>
    public Task<bool> ValidateConfigurationAsync()
    {
        try
        {
            var config = LoadConfiguration();
            var isValid = ValidateConfiguration(config);
            return Task.FromResult(isValid);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Validates a configuration and returns whether it's valid.
    /// </summary>
    private bool ValidateConfiguration(GameConfiguration config)
    {
        if (config == null) return false;
        if (config.Graphics == null) return false;
        if (config.Audio == null) return false;
        if (config.Gameplay == null) return false;
        if (config.Accessibility == null) return false;
        if (config.Controls == null) return false;

        // Validate ranges
        if (config.Audio.MasterVolume < 0 || config.Audio.MasterVolume > 1) return false;
        if (config.Audio.MusicVolume < 0 || config.Audio.MusicVolume > 1) return false;
        if (config.Audio.SFXVolume < 0 || config.Audio.SFXVolume > 1) return false;
        if (config.Graphics.TargetFPS < 30 || config.Graphics.TargetFPS > 240) return false;
        if (config.Accessibility.UIScale < 0.8f || config.Accessibility.UIScale > 1.5f) return false;

        return true;
    }

    /// <summary>
    /// Validates and fixes configuration values that are out of range.
    /// </summary>
    private GameConfiguration ValidateAndFixConfiguration(GameConfiguration config)
    {
        // Ensure all sub-configurations exist
        config.Graphics ??= new GraphicsConfig();
        config.Audio ??= new AudioConfig();
        config.Gameplay ??= new GameplayConfig();
        config.Accessibility ??= new AccessibilityConfig();
        config.Controls ??= new ControlsConfig();

        // Fix audio values
        config.Audio.MasterVolume = Math.Clamp(config.Audio.MasterVolume, 0f, 1f);
        config.Audio.MusicVolume = Math.Clamp(config.Audio.MusicVolume, 0f, 1f);
        config.Audio.SFXVolume = Math.Clamp(config.Audio.SFXVolume, 0f, 1f);
        config.Audio.UIVolume = Math.Clamp(config.Audio.UIVolume, 0f, 1f);
        config.Audio.AmbientVolume = Math.Clamp(config.Audio.AmbientVolume, 0f, 1f);

        // Fix graphics values
        config.Graphics.TargetFPS = Math.Clamp(config.Graphics.TargetFPS, 30, 240);
        config.Graphics.QualityLevel = Math.Clamp(config.Graphics.QualityLevel, 0, 2);

        // Fix gameplay values
        config.Gameplay.AutoSaveInterval = Math.Clamp(config.Gameplay.AutoSaveInterval, 1, 30);
        config.Gameplay.CombatSpeed = Math.Clamp(config.Gameplay.CombatSpeed, 0.5f, 2.0f);

        // Fix accessibility values
        config.Accessibility.UIScale = Math.Clamp(config.Accessibility.UIScale, 0.8f, 1.5f);
        config.Accessibility.FontScale = Math.Clamp(config.Accessibility.FontScale, 0.8f, 1.5f);

        // Fix controls values
        config.Controls.MouseSensitivity = Math.Clamp(config.Controls.MouseSensitivity, 0.1f, 3.0f);

        return config;
    }
}
