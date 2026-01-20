namespace RuneAndRust.Presentation.Gui.Services;

using Avalonia.Input;

/// <summary>
/// Manages keyboard shortcuts and keybindings with context-aware processing.
/// </summary>
/// <remarks>
/// Global shortcuts are always checked first. Context-specific shortcuts
/// are checked next based on the current game state.
/// </remarks>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Gets or sets the current shortcut context.
    /// </summary>
    ShortcutContext CurrentContext { get; set; }

    /// <summary>
    /// Processes a key press and executes the bound action if one exists.
    /// </summary>
    /// <param name="key">The key pressed.</param>
    /// <param name="modifiers">The key modifiers.</param>
    /// <returns>True if a shortcut was executed; otherwise, false.</returns>
    bool ProcessKeyDown(Key key, KeyModifiers modifiers);

    /// <summary>
    /// Registers an action to be invoked when its bound shortcut is pressed.
    /// </summary>
    /// <param name="actionId">The unique action identifier.</param>
    /// <param name="action">The action to execute.</param>
    void RegisterAction(string actionId, Action action);

    /// <summary>
    /// Unregisters a previously registered action.
    /// </summary>
    /// <param name="actionId">The action identifier to unregister.</param>
    void UnregisterAction(string actionId);

    /// <summary>
    /// Gets the current keybinding for an action.
    /// </summary>
    /// <param name="actionId">The action identifier.</param>
    /// <returns>The bound key gesture, or null if not bound.</returns>
    KeyGesture? GetBinding(string actionId);

    /// <summary>
    /// Sets a custom keybinding for an action.
    /// </summary>
    /// <param name="actionId">The action identifier.</param>
    /// <param name="gesture">The new key gesture to bind.</param>
    void SetBinding(string actionId, KeyGesture gesture);

    /// <summary>
    /// Resets all keybindings to their default values.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Gets all bindings for a specific context.
    /// </summary>
    /// <param name="context">The shortcut context.</param>
    /// <returns>A dictionary mapping action IDs to their key gestures.</returns>
    IReadOnlyDictionary<string, KeyGesture> GetBindingsForContext(ShortcutContext context);

    /// <summary>
    /// Checks if setting a binding would create a conflict.
    /// </summary>
    /// <param name="actionId">The action to rebind.</param>
    /// <param name="gesture">The proposed new gesture.</param>
    /// <returns>True if another action already uses this gesture in the same context.</returns>
    bool HasConflict(string actionId, KeyGesture gesture);

    /// <summary>
    /// Gets all registered shortcuts with their display names and current bindings.
    /// </summary>
    /// <returns>Collection of shortcut display information.</returns>
    IReadOnlyList<ShortcutDisplayInfo> GetAllShortcuts();
}
