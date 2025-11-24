using Avalonia.Input;
using System;
using System.Collections.Generic;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service for managing keyboard shortcuts throughout the application.
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Registers a keyboard shortcut with modifiers.
    /// </summary>
    /// <param name="key">The key to register.</param>
    /// <param name="modifiers">The key modifiers (Ctrl, Alt, Shift, etc.).</param>
    /// <param name="action">The action to execute when the shortcut is triggered.</param>
    /// <param name="description">Human-readable description of the shortcut.</param>
    void RegisterShortcut(Key key, KeyModifiers modifiers, Action action, string description);

    /// <summary>
    /// Registers a keyboard shortcut without modifiers.
    /// </summary>
    /// <param name="key">The key to register.</param>
    /// <param name="action">The action to execute when the shortcut is triggered.</param>
    /// <param name="description">Human-readable description of the shortcut.</param>
    void RegisterShortcut(Key key, Action action, string description);

    /// <summary>
    /// Handles a key press event and executes the associated action if registered.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <param name="modifiers">The active key modifiers.</param>
    /// <returns>True if the shortcut was handled, false otherwise.</returns>
    bool HandleKeyPress(Key key, KeyModifiers modifiers);

    /// <summary>
    /// Gets all registered keyboard shortcuts.
    /// </summary>
    IEnumerable<(Key Key, KeyModifiers Modifiers, string Description)> GetRegisteredShortcuts();
}
