using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages input key bindings via JSON configuration (v0.3.9c).
/// Loads/saves bindings from data/input_bindings.json.
/// </summary>
/// <remarks>See: SPEC-INPUT-001, Section "InputConfigurationService" for keybinding API.</remarks>
public class InputConfigurationService : IInputConfigurationService
{
    private readonly ILogger<InputConfigurationService> _logger;
    private readonly string _configPath;
    private Dictionary<ConsoleKey, string> _keyMap;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InputConfigurationService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public InputConfigurationService(ILogger<InputConfigurationService> logger)
    {
        _logger = logger;
        _configPath = Path.Combine("data", "input_bindings.json");
        _keyMap = GetDefaults();
    }

    /// <inheritdoc/>
    public void LoadBindings()
    {
        _logger.LogDebug("[Input] Attempting to load bindings from {Path}", _configPath);

        if (!File.Exists(_configPath))
        {
            _logger.LogInformation("[Input] No config file found at {Path}, using defaults", _configPath);
            _keyMap = GetDefaults();
            return;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<InputBindingsConfig>(json, JsonOptions);

            if (config?.Bindings == null || config.Bindings.Count == 0)
            {
                _logger.LogWarning("[Input] Config file empty or invalid, using defaults");
                _keyMap = GetDefaults();
                return;
            }

            _keyMap = new Dictionary<ConsoleKey, string>();
            var loadedCount = 0;

            foreach (var kvp in config.Bindings)
            {
                if (Enum.TryParse<ConsoleKey>(kvp.Key, ignoreCase: true, out var key))
                {
                    _keyMap[key] = kvp.Value;
                    loadedCount++;
                }
                else
                {
                    _logger.LogWarning("[Input] Unknown key name '{KeyName}', skipping", kvp.Key);
                }
            }

            _logger.LogInformation("[Input] Loaded {Count} key bindings from {Source}",
                loadedCount, _configPath);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "[Input] Failed to parse config: {Error}", ex.Message);
            _keyMap = GetDefaults();
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "[Input] Failed to read config file: {Error}", ex.Message);
            _keyMap = GetDefaults();
        }
    }

    /// <inheritdoc/>
    public void SaveBindings()
    {
        _logger.LogDebug("[Input] Saving bindings to {Path}", _configPath);

        try
        {
            // Ensure data directory exists
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var config = new InputBindingsConfig
            {
                Bindings = _keyMap.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value)
            };

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(_configPath, json);

            _logger.LogInformation("[Input] Saved {Count} bindings to {Path}",
                _keyMap.Count, _configPath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "[Input] Failed to save config: {Error}", ex.Message);
        }
    }

    /// <inheritdoc/>
    public string? GetCommandForKey(ConsoleKey key)
    {
        if (_keyMap.TryGetValue(key, out var command))
        {
            _logger.LogTrace("[Input] Resolved {Key} to command '{Command}'", key, command);
            return command;
        }

        _logger.LogTrace("[Input] No binding found for {Key}", key);
        return null;
    }

    /// <inheritdoc/>
    public ConsoleKey? GetKeyForCommand(string command)
    {
        foreach (var kvp in _keyMap)
        {
            if (kvp.Value.Equals(command, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogTrace("[Input] Resolved command '{Command}' to {Key}", command, kvp.Key);
                return kvp.Key;
            }
        }

        _logger.LogTrace("[Input] No key bound to command '{Command}'", command);
        return null;
    }

    /// <inheritdoc/>
    public void SetBinding(ConsoleKey key, string command)
    {
        var previousCommand = _keyMap.TryGetValue(key, out var existing) ? existing : null;
        _keyMap[key] = command;

        if (previousCommand != null)
        {
            _logger.LogInformation("[Input] Rebound {Key} from '{OldCommand}' to '{NewCommand}'",
                key, previousCommand, command);
        }
        else
        {
            _logger.LogInformation("[Input] Bound {Key} to '{Command}'", key, command);
        }
    }

    /// <inheritdoc/>
    public bool RemoveBinding(ConsoleKey key)
    {
        if (_keyMap.Remove(key))
        {
            _logger.LogInformation("[Input] Removed binding for {Key}", key);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<ConsoleKey, string> GetAllBindings()
    {
        return _keyMap.AsReadOnly();
    }

    /// <inheritdoc/>
    public void ResetToDefaults()
    {
        _keyMap = GetDefaults();
        _logger.LogInformation("[Input] Reset to default bindings ({Count} keys)", _keyMap.Count);
    }

    /// <summary>
    /// Gets the default key bindings.
    /// </summary>
    private static Dictionary<ConsoleKey, string> GetDefaults()
    {
        return new Dictionary<ConsoleKey, string>
        {
            // Movement
            { ConsoleKey.N, "north" },
            { ConsoleKey.S, "south" },
            { ConsoleKey.E, "east" },
            { ConsoleKey.W, "west" },
            { ConsoleKey.U, "up" },
            { ConsoleKey.D, "down" },

            // Core Actions
            { ConsoleKey.Enter, "confirm" },
            { ConsoleKey.Escape, "cancel" },
            { ConsoleKey.M, "menu" },
            { ConsoleKey.H, "help" },

            // Screen Navigation
            { ConsoleKey.I, "inventory" },
            { ConsoleKey.C, "character" },
            { ConsoleKey.J, "journal" },
            { ConsoleKey.B, "bench" },

            // Gameplay
            { ConsoleKey.F, "interact" },
            { ConsoleKey.L, "look" },
            { ConsoleKey.X, "search" },
            { ConsoleKey.Spacebar, "wait" },

            // Combat
            { ConsoleKey.A, "attack" },
            { ConsoleKey.Q, "light" },
            { ConsoleKey.R, "heavy" }
        };
    }

    /// <summary>
    /// Internal configuration class for JSON serialization.
    /// </summary>
    private class InputBindingsConfig
    {
        public Dictionary<string, string> Bindings { get; set; } = new();
    }
}
