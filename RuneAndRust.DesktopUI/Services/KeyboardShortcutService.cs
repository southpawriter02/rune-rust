using Avalonia.Input;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Implementation of keyboard shortcut service.
/// </summary>
public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<(Key, KeyModifiers), (Action Action, string Description)> _shortcuts = new();

    public KeyboardShortcutService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void RegisterShortcut(Key key, KeyModifiers modifiers, Action action, string description)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or whitespace", nameof(description));

        _shortcuts[(key, modifiers)] = (action, description);
        _logger.Debug("Registered shortcut: {Key}+{Modifiers} - {Description}",
            key, modifiers, description);
    }

    /// <inheritdoc/>
    public void RegisterShortcut(Key key, Action action, string description)
    {
        RegisterShortcut(key, KeyModifiers.None, action, description);
    }

    /// <inheritdoc/>
    public bool HandleKeyPress(Key key, KeyModifiers modifiers)
    {
        if (_shortcuts.TryGetValue((key, modifiers), out var shortcut))
        {
            _logger.Debug("Executing shortcut: {Key}+{Modifiers} - {Description}",
                key, modifiers, shortcut.Description);

            try
            {
                shortcut.Action();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error executing shortcut: {Key}+{Modifiers}", key, modifiers);
                return false;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public IEnumerable<(Key Key, KeyModifiers Modifiers, string Description)> GetRegisteredShortcuts()
    {
        return _shortcuts
            .Select(kvp => (kvp.Key.Item1, kvp.Key.Item2, kvp.Value.Description))
            .OrderBy(x => x.Item1)
            .ThenBy(x => x.Item2);
    }
}
