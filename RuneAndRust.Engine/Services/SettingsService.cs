using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Settings;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages user settings persistence to/from options.json (v0.3.10a).
/// Loads settings at startup, validates values, and saves changes to disk.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly string _optionsPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        _optionsPath = Path.Combine("data", "options.json");
    }

    /// <inheritdoc/>
    public async Task LoadAsync()
    {
        try
        {
            if (!File.Exists(_optionsPath))
            {
                _logger.LogInformation("[Settings] No settings file found at {Path}, creating defaults", _optionsPath);
                await ResetToDefaultsAsync();
                return;
            }

            var json = await File.ReadAllTextAsync(_optionsPath);
            var dto = JsonSerializer.Deserialize<SettingsDto>(json, JsonOptions);

            if (dto == null)
            {
                _logger.LogWarning("[Settings] Settings file was empty or invalid, resetting to defaults");
                await ResetToDefaultsAsync();
                return;
            }

            ApplyDtoToSettings(dto);
            _logger.LogInformation("[Settings] Loaded settings. Theme: {Theme}, ReduceMotion: {Motion}",
                GameSettings.Theme, GameSettings.ReduceMotion);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[Settings] Failed to parse settings file: {Error}. Resetting to defaults.", ex.Message);
            await ResetToDefaultsAsync();
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "[Settings] Failed to read settings file: {Error}. Resetting to defaults.", ex.Message);
            await ResetToDefaultsAsync();
        }
    }

    /// <inheritdoc/>
    public async Task SaveAsync()
    {
        try
        {
            var dto = new SettingsDto
            {
                ReduceMotion = GameSettings.ReduceMotion,
                Theme = (int)GameSettings.Theme,
                TextSpeed = GameSettings.TextSpeed,
                MasterVolume = GameSettings.MasterVolume,
                AutosaveIntervalMinutes = GameSettings.AutosaveIntervalMinutes,
                Language = GameSettings.Language
            };

            var directory = Path.GetDirectoryName(_optionsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("[Settings] Created directory {Directory}", directory);
            }

            var json = JsonSerializer.Serialize(dto, JsonOptions);
            await File.WriteAllTextAsync(_optionsPath, json);

            _logger.LogDebug("[Settings] Saved settings to {Path}", _optionsPath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "[Settings] Failed to save settings: {Error}", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task ResetToDefaultsAsync()
    {
        // Set all properties to defaults
        GameSettings.ReduceMotion = false;
        GameSettings.Theme = ThemeType.Standard;
        GameSettings.TextSpeed = 100;
        GameSettings.MasterVolume = 100;
        GameSettings.AutosaveIntervalMinutes = 5;
        GameSettings.Language = "en-US";

        await SaveAsync();
        _logger.LogInformation("[Settings] Reset to default settings");
    }

    /// <summary>
    /// Applies DTO values to GameSettings with validation and clamping.
    /// </summary>
    private void ApplyDtoToSettings(SettingsDto dto)
    {
        // Boolean - no validation needed
        GameSettings.ReduceMotion = dto.ReduceMotion;

        // Theme - validate enum
        if (Enum.IsDefined(typeof(ThemeType), dto.Theme))
        {
            GameSettings.Theme = (ThemeType)dto.Theme;
        }
        else
        {
            _logger.LogWarning("[Settings] Invalid theme value {Value}, defaulting to Standard", dto.Theme);
            GameSettings.Theme = ThemeType.Standard;
        }

        // TextSpeed - clamp to 10-200
        if (dto.TextSpeed < 10 || dto.TextSpeed > 200)
        {
            var clamped = Math.Clamp(dto.TextSpeed, 10, 200);
            _logger.LogWarning("[Settings] TextSpeed value {Value} out of range, clamped to {Clamped}",
                dto.TextSpeed, clamped);
            GameSettings.TextSpeed = clamped;
        }
        else
        {
            GameSettings.TextSpeed = dto.TextSpeed;
        }

        // MasterVolume - clamp to 0-100
        if (dto.MasterVolume < 0 || dto.MasterVolume > 100)
        {
            var clamped = Math.Clamp(dto.MasterVolume, 0, 100);
            _logger.LogWarning("[Settings] MasterVolume value {Value} out of range, clamped to {Clamped}",
                dto.MasterVolume, clamped);
            GameSettings.MasterVolume = clamped;
        }
        else
        {
            GameSettings.MasterVolume = dto.MasterVolume;
        }

        // AutosaveIntervalMinutes - clamp to 1-60
        if (dto.AutosaveIntervalMinutes < 1 || dto.AutosaveIntervalMinutes > 60)
        {
            var clamped = Math.Clamp(dto.AutosaveIntervalMinutes, 1, 60);
            _logger.LogWarning("[Settings] AutosaveIntervalMinutes value {Value} out of range, clamped to {Clamped}",
                dto.AutosaveIntervalMinutes, clamped);
            GameSettings.AutosaveIntervalMinutes = clamped;
        }
        else
        {
            GameSettings.AutosaveIntervalMinutes = dto.AutosaveIntervalMinutes;
        }

        // Language - validate non-empty, default to en-US (v0.3.15b)
        if (string.IsNullOrWhiteSpace(dto.Language))
        {
            _logger.LogWarning("[Settings] Invalid Language value, defaulting to en-US");
            GameSettings.Language = "en-US";
        }
        else
        {
            GameSettings.Language = dto.Language;
        }
    }
}
