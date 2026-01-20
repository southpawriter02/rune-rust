using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Manages command history with navigation support.
/// </summary>
/// <remarks>
/// Features:
/// - In-memory history storage (newest first)
/// - Configurable max size (default 100)
/// - Duplicate handling (move to front)
/// - Empty command exclusion
/// - Navigation index tracking
/// - Current input preservation
/// </remarks>
public class CommandHistoryService : ICommandHistoryService
{
    private readonly List<string> _history = new();
    private readonly ILogger<CommandHistoryService>? _logger;
    private readonly CommandHistorySettings _settings;
    
    private int _currentIndex = -1;
    private string? _tempInput;
    
    /// <inheritdoc/>
    public int Count => _history.Count;
    
    /// <inheritdoc/>
    public int MaxSize => _settings.MaxEntries;
    
    /// <inheritdoc/>
    public bool IsNavigating => _currentIndex >= 0;
    
    /// <summary>
    /// Initializes a new instance with settings.
    /// </summary>
    public CommandHistoryService(
        IOptions<CommandHistorySettings> settings,
        ILogger<CommandHistoryService>? logger = null)
    {
        _settings = settings.Value;
        _logger = logger;
    }
    
    /// <summary>
    /// Initializes a new instance with default settings.
    /// </summary>
    public CommandHistoryService(ILogger<CommandHistoryService>? logger = null)
    {
        _settings = new CommandHistorySettings();
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public void Add(string command)
    {
        if (!_settings.Enabled) return;
        
        if (string.IsNullOrWhiteSpace(command))
        {
            if (_settings.ExcludeEmpty)
            {
                _logger?.LogDebug("Empty command excluded from history");
                return;
            }
        }
        
        var trimmedCommand = command.Trim();
        
        // Handle duplicates
        if (_settings.MoveDuplicatesToFront)
        {
            var existingIndex = _history.IndexOf(trimmedCommand);
            if (existingIndex >= 0)
            {
                _history.RemoveAt(existingIndex);
                _logger?.LogDebug("Duplicate command '{Command}' moved to front", 
                    trimmedCommand);
            }
        }
        else if (_history.Contains(trimmedCommand))
        {
            _logger?.LogDebug("Duplicate command '{Command}' not added", 
                trimmedCommand);
            ResetNavigation();
            return;
        }
        
        // Add to front (most recent)
        _history.Insert(0, trimmedCommand);
        
        // Trim to max size
        while (_history.Count > MaxSize)
        {
            var removed = _history[^1];
            _history.RemoveAt(_history.Count - 1);
            _logger?.LogDebug("History exceeded max size, removed oldest: '{Command}'", 
                removed);
        }
        
        _logger?.LogDebug("Added command to history: '{Command}'. Total: {Count}", 
            trimmedCommand, _history.Count);
        
        ResetNavigation();
    }
    
    /// <inheritdoc/>
    public string? GetPrevious()
    {
        if (_history.Count == 0)
        {
            return null;
        }
        
        if (_currentIndex < _history.Count - 1)
        {
            _currentIndex++;
            _logger?.LogDebug("History navigated to index {Index}: '{Command}'", 
                _currentIndex, _history[_currentIndex]);
            return _history[_currentIndex];
        }
        
        // Already at oldest, stay there
        _logger?.LogDebug("At oldest history entry, staying at index {Index}", 
            _currentIndex);
        return _history[_currentIndex];
    }
    
    /// <inheritdoc/>
    public string? GetNext()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            _logger?.LogDebug("History navigated to index {Index}: '{Command}'", 
                _currentIndex, _history[_currentIndex]);
            return _history[_currentIndex];
        }
        
        if (_currentIndex == 0)
        {
            // Move back to new input
            _currentIndex = -1;
            var restored = _tempInput ?? "";
            _tempInput = null;
            
            _logger?.LogDebug("Returned to new input, restored: '{Input}'", restored);
            return restored;
        }
        
        // Already at new input position
        return null;
    }
    
    /// <inheritdoc/>
    public void ResetNavigation()
    {
        _currentIndex = -1;
        _tempInput = null;
        _logger?.LogDebug("History navigation reset");
    }
    
    /// <inheritdoc/>
    public void SaveCurrentInput(string currentInput)
    {
        if (_currentIndex == -1)
        {
            _tempInput = currentInput;
            _logger?.LogDebug("Saved current input for restoration: '{Input}'", 
                currentInput);
        }
    }
    
    /// <inheritdoc/>
    public void Clear()
    {
        _history.Clear();
        ResetNavigation();
        _logger?.LogInformation("Command history cleared");
    }
    
    /// <inheritdoc/>
    public IReadOnlyList<string> GetAll()
    {
        return _history.AsReadOnly();
    }
}
