namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for managing input key bindings (v0.3.9c).
/// Provides JSON-based configuration for customizable keyboard controls.
/// </summary>
public interface IInputConfigurationService
{
    /// <summary>
    /// Loads key bindings from the configuration file.
    /// Falls back to defaults if file is missing or invalid.
    /// </summary>
    void LoadBindings();

    /// <summary>
    /// Saves current key bindings to the configuration file.
    /// </summary>
    void SaveBindings();

    /// <summary>
    /// Gets the command string associated with a key.
    /// </summary>
    /// <param name="key">The console key to look up.</param>
    /// <returns>The command string if bound; null otherwise.</returns>
    string? GetCommandForKey(ConsoleKey key);

    /// <summary>
    /// Gets the key currently bound to a command (reverse lookup) (v0.3.10c).
    /// </summary>
    /// <param name="command">The command string to look up.</param>
    /// <returns>The console key if command is bound; null otherwise.</returns>
    ConsoleKey? GetKeyForCommand(string command);

    /// <summary>
    /// Sets or updates a key binding.
    /// </summary>
    /// <param name="key">The console key to bind.</param>
    /// <param name="command">The command string to associate with the key.</param>
    void SetBinding(ConsoleKey key, string command);

    /// <summary>
    /// Removes a key binding if it exists.
    /// </summary>
    /// <param name="key">The console key to unbind.</param>
    /// <returns>True if the binding was removed; false if it didn't exist.</returns>
    bool RemoveBinding(ConsoleKey key);

    /// <summary>
    /// Gets an immutable copy of all current key bindings.
    /// </summary>
    /// <returns>A read-only dictionary of key-to-command mappings.</returns>
    IReadOnlyDictionary<ConsoleKey, string> GetAllBindings();

    /// <summary>
    /// Resets all bindings to their default values.
    /// </summary>
    void ResetToDefaults();
}
