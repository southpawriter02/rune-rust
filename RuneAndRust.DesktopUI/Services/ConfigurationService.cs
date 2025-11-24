using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Implementation of configuration service.
/// Stub implementation for v0.43.1, full implementation in v0.43.18.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private const string ConfigFileName = "ui_config.json";
    private readonly string _configPath;

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "RuneAndRust");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, ConfigFileName);
    }

    /// <inheritdoc/>
    public UIConfiguration LoadConfiguration()
    {
        if (!File.Exists(_configPath))
        {
            return new UIConfiguration();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<UIConfiguration>(json) ?? new UIConfiguration();
        }
        catch
        {
            // If loading fails, return default configuration
            return new UIConfiguration();
        }
    }

    /// <inheritdoc/>
    public void SaveConfiguration(UIConfiguration config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_configPath, json);
    }

    /// <inheritdoc/>
    public Task<bool> ValidateConfigurationAsync()
    {
        // Stub implementation
        return Task.FromResult(true);
    }
}
