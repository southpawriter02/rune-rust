namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Manages command history for input recall via arrow keys.
/// </summary>
public interface ICommandHistoryService
{
    /// <summary>
    /// Adds a command to history.
    /// </summary>
    /// <remarks>
    /// Empty commands are ignored. Duplicates are moved to front.
    /// </remarks>
    /// <param name="command">The command text to add.</param>
    void Add(string command);
    
    /// <summary>
    /// Gets the previous command in history (up arrow).
    /// </summary>
    /// <returns>Previous command, or null if at oldest.</returns>
    string? GetPrevious();
    
    /// <summary>
    /// Gets the next command in history (down arrow).
    /// </summary>
    /// <returns>Next command, or null/empty if at newest.</returns>
    string? GetNext();
    
    /// <summary>
    /// Resets navigation to the end (new command position).
    /// </summary>
    /// <remarks>
    /// Called after a command is executed or when starting fresh.
    /// </remarks>
    void ResetNavigation();
    
    /// <summary>
    /// Saves the current input before navigating history.
    /// </summary>
    /// <param name="currentInput">The partially typed input to preserve.</param>
    void SaveCurrentInput(string currentInput);
    
    /// <summary>
    /// Gets the current history count.
    /// </summary>
    int Count { get; }
    
    /// <summary>
    /// Gets the maximum history size.
    /// </summary>
    int MaxSize { get; }
    
    /// <summary>
    /// Gets whether currently navigating history.
    /// </summary>
    bool IsNavigating { get; }
    
    /// <summary>
    /// Clears all history.
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Gets all commands in history (newest first).
    /// </summary>
    IReadOnlyList<string> GetAll();
}
