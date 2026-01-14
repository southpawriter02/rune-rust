namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Abstracts terminal operations for cross-platform compatibility.
/// </summary>
/// <remarks>
/// Provides a layer of abstraction over System.Console to enable:
/// <list type="bullet">
/// <item><description>Testability (mock terminal in unit tests)</description></item>
/// <item><description>Cross-platform compatibility</description></item>
/// <item><description>Capability detection (color, unicode)</description></item>
/// </list>
/// </remarks>
public interface ITerminalService
{
    /// <summary>
    /// Gets the current terminal size.
    /// </summary>
    /// <returns>Tuple of (Width, Height) in character cells.</returns>
    (int Width, int Height) GetSize();
    
    /// <summary>
    /// Sets the cursor position.
    /// </summary>
    /// <param name="x">Column (0-indexed from left).</param>
    /// <param name="y">Row (0-indexed from top).</param>
    void SetCursorPosition(int x, int y);
    
    /// <summary>
    /// Writes text at the current cursor position.
    /// </summary>
    /// <param name="text">Text to write.</param>
    void Write(string text);
    
    /// <summary>
    /// Writes a line at the current cursor position.
    /// </summary>
    /// <param name="text">Text to write.</param>
    void WriteLine(string text);
    
    /// <summary>
    /// Clears the entire terminal.
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Clears a specific rectangular region.
    /// </summary>
    /// <param name="x">Starting column.</param>
    /// <param name="y">Starting row.</param>
    /// <param name="width">Width in characters.</param>
    /// <param name="height">Height in rows.</param>
    void ClearRegion(int x, int y, int width, int height);
    
    /// <summary>
    /// Event raised when the terminal is resized.
    /// </summary>
    /// <remarks>
    /// On platforms without native resize events, this may be triggered by polling.
    /// </remarks>
    event Action<(int Width, int Height)>? OnResize;
    
    /// <summary>
    /// Gets whether color output is supported.
    /// </summary>
    bool SupportsColor { get; }
    
    /// <summary>
    /// Gets whether Unicode output is supported.
    /// </summary>
    bool SupportsUnicode { get; }
}
